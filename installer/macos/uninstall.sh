#!/bin/sh
set -eu

install_dir="$HOME/Applications/Morfa.app"
if [ -L "$install_dir" ] || [ ! -d "$install_dir" ]; then
    printf '%s\n' "No ordinary Morfa app bundle was found at $install_dir."
    exit 1
fi
resolved_parent=$(CDPATH= cd -- "$HOME/Applications" && pwd -P)
resolved_app=$(CDPATH= cd -- "$install_dir" && pwd -P)
if [ "$resolved_app" != "$resolved_parent/Morfa.app" ]; then
    printf '%s\n' 'Refusing to remove an app outside the expected Applications directory.'
    exit 1
fi
bundle_id=$(/usr/libexec/PlistBuddy -c 'Print :CFBundleIdentifier' "$install_dir/Contents/Info.plist")
if [ "$bundle_id" != 'com.morfa.app' ]; then
    printf '%s\n' 'The bundle is not Morfa.'
    exit 1
fi
"$install_dir/Contents/MacOS/Morfa" --uninstall-cleanup
if [ -L "$HOME/.local/bin/morfa" ] && [ "$(readlink "$HOME/.local/bin/morfa")" = "$install_dir/Contents/MacOS/morfa-cli" ]; then
    rm -- "$HOME/.local/bin/morfa"
fi
rm -rf -- "$install_dir"
printf '%s\n' 'Morfa was removed. Your settings and conversion results were kept.'
