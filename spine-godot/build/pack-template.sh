#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir/../godot/bin" > /dev/null

echo "$1" > version.txt
zip spine-godot-templates.zip ios.zip macos.zip windows_debug_x86_64.exe windows_release_x86_64.exe linux_x11_64_debug linux_x11_64_release web_debug.zip web_release.zip android_release.apk android_debug.apk android_source.zip version.txt

popd