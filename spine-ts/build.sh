#!/bin/sh
set -e -x
tsc -p tsconfig.json
tsc -p tsconfig.core.json
tsc -p tsconfig.webgl.json
tsc -p tsconfig.canvas.json
tsc -p tsconfig.threejs.json
tsc -p tsconfig.widget.json