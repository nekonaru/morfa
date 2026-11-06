#!/bin/sh
set -eu

data_dir=${XDG_DATA_HOME:-"$HOME/.local/share"}
install_dir="$data_dir/morfa/application"
desktop_file="$data_dir/applications/com.morfa.app.desktop"
gnome_extension_id="morfa-donut@nekonaru.github.io"
gnome_extension_dir="$data_dir/gnome-shell/extensions/$gnome_extension_id"
python3 - "$data_dir" "$install_dir" <<'PY'
import pathlib
import sys

root, target = map(pathlib.Path, sys.argv[1:])
expected = root.resolve() / 'morfa' / 'application'
if target.is_symlink() or target.resolve() != expected:
    raise SystemExit('Refusing to remove an installation outside its expected directory.')
if (target / '.morfa-package').read_text(encoding='utf-8').strip() != 'com.morfa.app':
    raise SystemExit('The directory is not an installation created by Morfa.')
PY
"$install_dir/Morfa" --uninstall-cleanup
if [ -f "$gnome_extension_dir/.morfa-package" ] && [ "$(cat "$gnome_extension_dir/.morfa-package")" = 'com.morfa.app' ]; then
    if command -v gnome-extensions >/dev/null 2>&1; then
        gnome-extensions disable "$gnome_extension_id" >/dev/null 2>&1 || true
    fi
    rm -rf -- "$gnome_extension_dir"
fi
if [ -L "$HOME/.local/bin/morfa" ] && [ "$(readlink "$HOME/.local/bin/morfa")" = "$install_dir/morfa-cli" ]; then
    rm -- "$HOME/.local/bin/morfa"
fi
if [ -f "$desktop_file" ] && grep -q '^X-Morfa-Managed=true$' "$desktop_file"; then
    rm -- "$desktop_file"
fi
rm -rf -- "$install_dir"
printf '%s\n' 'Morfa was removed. Your settings and conversion results were kept.'
