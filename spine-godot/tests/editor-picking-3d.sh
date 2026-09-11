#!/bin/bash
set -eu

if [ "$#" -lt 1 ] || [ "$#" -gt 2 ]; then
	echo "Usage: $0 /absolute/path/to/godot [extension-example/bin]" >&2
	exit 1
fi

godot="$1"
tests="$(cd "$(dirname "$0")" && pwd)"
project="$(mktemp -d "${TMPDIR:-/tmp}/spine-editor-picking.XXXXXX")"
trap 'rm -rf "$project"' EXIT
mkdir -p "$project/addons/picking"
cp "$tests/editor-picking-3d.gd" "$project/addons/picking/"
printf '%s\n' \
	'config_version=5' \
	'[application]' 'config/name="Spine editor picking regression"' \
	'[rendering]' 'renderer/rendering_method="gl_compatibility"' \
	'[editor_plugins]' 'enabled=PackedStringArray("res://addons/picking/plugin.cfg")' >"$project/project.godot"
printf '%s\n' '[gd_scene format=3]' '[node name="PickingTest" type="Node3D"]' >"$project/picking.tscn"
printf '%s\n' '[plugin]' 'name="Spine editor picking regression"' \
	'description="Temporary editor selection regression"' 'author="Spine"' \
	'version="1.0"' 'script="editor-picking-3d.gd"' >"$project/addons/picking/plugin.cfg"
if [ "$#" = 2 ]; then
	cp -R "$2" "$project/bin"
fi
"$godot" --editor --path "$project" res://picking.tscn
