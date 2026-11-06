#!/bin/sh
set -eu

source_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
install_dir="$HOME/Applications/Morfa.app"
bin_dir="$HOME/.local/bin"
if [ -e "$install_dir" ] || [ -L "$install_dir" ]; then
    printf '%s\n' "An installation already exists at $install_dir. Run uninstall.sh first."
    exit 1
fi
# A morfa link left by an earlier Morfa whose app is gone (moved to the Trash instead of uninstall.sh) is replaced.
# Anything else at that path isn't ours and stays.
stale_link=false
if [ -L "$bin_dir/morfa" ] && [ ! -e "$bin_dir/morfa" ]; then
    case $(readlink "$bin_dir/morfa") in
        */Morfa.app/Contents/MacOS/morfa-cli) stale_link=true ;;
    esac
fi
if [ "$stale_link" = false ] && { [ -e "$bin_dir/morfa" ] || [ -L "$bin_dir/morfa" ]; }; then
    printf '%s\n' "$bin_dir/morfa already exists. Remove or rename it before installing Morfa."
    exit 1
fi
test -x "$source_dir/Morfa.app/Contents/MacOS/Morfa"
mkdir -p "$HOME/Applications" "$bin_dir"
/usr/bin/ditto "$source_dir/Morfa.app" "$install_dir"
# ditto keeps the download's quarantine flag: macOS would then start Morfa from a translocated copy and could stop
# the morfa command without a word (#316).
/usr/bin/xattr -dr com.apple.quarantine "$install_dir" 2>/dev/null || true
if [ "$stale_link" = true ]; then
    rm -f -- "$bin_dir/morfa"
    printf '%s\n' "Replaced the morfa link left by an earlier installation."
fi
ln -s "$install_dir/Contents/MacOS/morfa-cli" "$bin_dir/morfa"
printf '%s\n' "Installed $install_dir" "CLI: $bin_dir/morfa (add $bin_dir to PATH if needed)."
printf '%s\n' 'Open Morfa from Applications. Grant Accessibility and Finder Automation access when requested.'
printf '%s\n' 'If Morfa is already listed and switched on under Accessibility, remove it with the - button and add it again with +.'
