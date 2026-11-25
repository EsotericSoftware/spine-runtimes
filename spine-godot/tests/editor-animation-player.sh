#!/bin/bash
set -eu
if [ "$#" -lt 1 ] || [ "$#" -gt 2 ]; then
	echo "Usage: $0 /absolute/path/to/godot [extension-example/bin]" >&2
	exit 1
fi
godot="$1"
tests="$(cd "$(dirname "$0")" && pwd)"
project="$(mktemp -d "${TMPDIR:-/tmp}/spine-editor-animation-player.XXXXXX")"
trap 'rm -rf "$project"' EXIT
mkdir -p "$project/addons/test"
cp "$tests/editor-animation-player.gd" "$tests/animation-player-fixture.gd" "$project/addons/test/"
printf '%s\n' 'config_version=5' \
	'[application]' 'config/name="Spine AnimationPlayer editor regression"' \
	'[rendering]' 'renderer/rendering_method="gl_compatibility"' \
	'[editor_plugins]' 'enabled=PackedStringArray("res://addons/test/plugin.cfg")' >"$project/project.godot"
printf '%s\n' '[gd_scene format=3]' '[node name="AnimationPlayerTest" type="Node3D"]' >"$project/test.tscn"
printf '%s\n' '[plugin]' 'name="Spine AnimationPlayer regression"' \
	'description="Temporary editor regression"' 'author="Spine"' 'version="1.0"' \
	'script="editor-animation-player.gd"' >"$project/addons/test/plugin.cfg"
if [ "$#" = 2 ]; then cp -R "$2" "$project/bin"; fi
"$godot" --editor --path "$project" res://test.tscn
