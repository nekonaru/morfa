<#
.SYNOPSIS
  Builds the GitHub Release body for a version from CHANGELOG.md and CHANGELOG.id.md.

.DESCRIPTION
  Bilingual body, English first: a one-sentence summary, then New
  features / Improvements / Bug fixes with a bold lead-in per bullet, sections with no
  items are omitted, then Install / Update with the minimum OS and which file to download.
  Throws when the version block or its summary is missing, so a malformed changelog never
  produces a partial release note.

.EXAMPLE
  build/tools/make-release-notes.ps1 -Version 0.1.1 -Output notes.md
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Version,
    [string]$ChangelogEn = (Join-Path $PSScriptRoot '../../CHANGELOG.md'),
    [string]$ChangelogId = (Join-Path $PSScriptRoot '../../CHANGELOG.id.md'),
    [string]$Output = ''
)

$ErrorActionPreference = 'Stop'
$Version = $Version.Trim().TrimStart('v')

# Reads one changelog version block ("## vX.Y.Z", ended by the next version heading or EOF):
# the one-sentence summary and the guide sections with their bullets, in guide order.
function Get-ChangelogVersion {
    param([string]$Path, [string[]]$Sections)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Changelog not found: $Path" }
    $lines = Get-Content -LiteralPath $Path -Encoding utf8
    $heading = "^## v?$([regex]::Escape($Version))$"
    $start = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim() -match $heading) { $start = $i; break }
    }
    if ($start -lt 0) { throw "$Path has no version section for $Version." }
    $end = $lines.Count
    for ($i = $start + 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^## v?\d') { $end = $i; break }
    }
    $block = $lines[($start + 1)..($end - 1)]
    $summary = ''
    $found = @{}
    $current = $null
    foreach ($line in $block) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^## (\S.*)$') {
            $current = $Matches[1].Trim()
            continue
        }
        if ($current) {
            if ($trimmed.StartsWith('- ')) {
                if (-not $found.ContainsKey($current)) { $found[$current] = @() }
                $found[$current] += $trimmed
            }
            elseif ($trimmed -ne '' -and $found.ContainsKey($current) -and $found[$current].Count -gt 0) {
                $found[$current][-1] = "$($found[$current][-1]) $trimmed"
            }
            continue
        }
        if ($trimmed -ne '' -and -not $trimmed.StartsWith('- ')) {
            if (-not $summary) { $summary = $trimmed }
            else { $summary = "$summary $trimmed" }
        }
    }
    if (-not $summary) { throw "${Path}: version $Version has no summary sentence." }
    $ordered = [ordered]@{}
    foreach ($name in $Sections) {
        if ($found.ContainsKey($name)) { $ordered[$name] = $found[$name] }
    }
    if ($ordered.Count -eq 0) { throw "${Path}: version $Version has no New features, Improvements or Bug fixes items." }
    return [pscustomobject]@{ Summary = $summary; Sections = $ordered }
}

function Format-LanguageBlock {
    param(
        [pscustomobject]$Data,
        [string]$Title,
        [string]$InstallHeading,
        [string]$InstallText
    )
    $part = @()
    $part += "# $Title"
    $part += ''
    $part += $Data.Summary
    foreach ($name in $Data.Sections.Keys) {
        $part += ''
        $part += "## $name"
        $part += ''
        foreach ($bullet in $Data.Sections[$name]) { $part += $bullet }
    }
    $part += ''
    $part += "## $InstallHeading"
    $part += ''
    $part += $InstallText
    return ($part -join "`n")
}

$english = Get-ChangelogVersion -Path $ChangelogEn -Sections @('New features', 'Improvements', 'Bug fixes')
$indonesian = Get-ChangelogVersion -Path $ChangelogId -Sections @('Fitur baru', 'Peningkatan', 'Perbaikan bug')

$v = $Version
$installEn = @"
Download the file for your platform from Assets below, open it and follow the prompts. Requires Windows 10 (64-bit) or later, macOS 13 or later, or Ubuntu 22.04 or later.

Use ``Morfa-$v-win-Setup.exe`` for the Windows app, ``Morfa-$v-linux-x64.tar.gz`` (or ``-arm64.tar.gz``) on Linux, and ``Morfa-$v-osx-arm64.dmg`` (Apple Silicon) or ``Morfa-$v-osx-x64.dmg`` (Intel) on macOS.

On Windows, SmartScreen shows warnings because the build is not signed yet. Click More info, then Run anyway.

On Linux, drag gestures need X11. On Wayland, use the drop zone and the file manager menu.

On macOS, allow the app in System Settings > Privacy & Security > Open Anyway.
"@

$installId = @"
Download file untuk platformmu dari Assets di bawah, buka, lalu ikuti petunjuknya. Membutuhkan Windows 10 (64-bit) atau lebih baru, macOS 13 atau lebih baru, atau Ubuntu 22.04 atau lebih baru.

Gunakan ``Morfa-$v-win-Setup.exe`` untuk Windows, ``Morfa-$v-linux-x64.tar.gz`` (atau ``-arm64.tar.gz``) di Linux, dan ``Morfa-$v-osx-arm64.dmg`` (Apple Silicon) atau ``Morfa-$v-osx-x64.dmg`` (Intel) di macOS.

Di Windows, SmartScreen menampilkan peringatan karena build belum ditandatangani. Klik More info, lalu Run anyway.

Di Linux, gestur drag butuh X11. Di Wayland, pakai drop zone dan menu file manager.

Di macOS, izinkan aplikasi lewat System Settings > Privacy & Security > Open Anyway.
"@

$body = (Format-LanguageBlock -Data $english -Title 'English' -InstallHeading 'Install / Update' -InstallText $installEn.Trim()) +
        "`n`n<br>`n`n---`n`n<br>`n`n" +
        (Format-LanguageBlock -Data $indonesian -Title 'Bahasa Indonesia' -InstallHeading 'Instalasi / Update' -InstallText $installId.Trim()) + "`n"

if ($Output) {
    $dir = [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($Output))
    if ($dir) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    Set-Content -LiteralPath $Output -Value $body -Encoding utf8 -NoNewline:$false
    Write-Host "Wrote $Output ($($body.Length) chars)"
} else {
    Write-Output $body
}
