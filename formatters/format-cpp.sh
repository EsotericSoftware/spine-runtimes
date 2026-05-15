#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

# Source logging utilities
source "$dir/logging/logging.sh"

log_title "C/C++ Formatting"

# Store original directory
pushd "$dir" > /dev/null

log_action "Checking for formatters/.clang-format"
if [ -f ".clang-format" ]; then
    log_ok
else
    log_fail
    popd > /dev/null
    exit 1
fi

# Define C/C++ source directories - be specific to avoid engine sources
cpp_dirs=(
    # spine-cpp
    "../spine-cpp/include/spine"
    "../spine-cpp/src/spine"
    "../spine-cpp/spine-cpp-lite"
    "../spine-cpp/tests"

    # spine-c
    "../spine-c/include"
    "../spine-c/src"
    "../spine-c/src/generated"
    "../spine-c/tests"

    # spine-godot
    "../spine-godot/spine_godot"

    # spine-ue
    "../spine-ue/Source/SpineUE"
    "../spine-ue/Plugins/SpinePlugin/Source/SpinePlugin/Public"
    "../spine-ue/Plugins/SpinePlugin/Source/SpinePlugin/Private"
    "../spine-ue/Plugins/SpinePlugin/Source/SpineEditorPlugin/Public"
    "../spine-ue/Plugins/SpinePlugin/Source/SpineEditorPlugin/Private"

    # spine-glfw
    "../spine-glfw/src"
    "../spine-glfw/example"

    # spine-sdl
    "../spine-sdl/src"
    "../spine-sdl/example"

    # spine-sfml
    "../spine-sfml/c/src/spine"
    "../spine-sfml/c/example"
    "../spine-sfml/cpp/src/spine"
    "../spine-sfml/cpp/example"

    # spine-ios
    "../spine-ios/Sources/SpineCppLite"
    "../spine-ios/Sources/SpineCppLite/include"
    "../spine-ios/Sources/SpineShadersStructs"
    "../spine-ios/Example/Spine iOS Example"

    # spine-flutter
    "../spine-flutter/ios/Classes"
    "../spine-flutter/macos/Classes"
    "../spine-flutter/src"
)

# Collect all C/C++ files from specified directories
files=()
for cpp_dir in "${cpp_dirs[@]}"; do
    if [ -d "$cpp_dir" ]; then
        while IFS= read -r -d '' file; do
            files+=("$file")
        done < <(find "$cpp_dir" \( -name "*.c" -o -name "*.cpp" -o -name "*.h" \) \
                 -not -path "*/.*" \
                 -not -path "*/build/*" \
                 -not -path "*/cmake-build-*/*" \
                 -not -path "*/third_party/*" \
                 -not -path "*/external/*" \
                 -not -type l \
                 -print0)
    fi
done


log_action "Formatting ${#files[@]} C/C++ files"
if FORMAT_OUTPUT=$(clang-format -i -style=file:".clang-format" "${files[@]}" 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$FORMAT_OUTPUT"
    popd > /dev/null
    exit 1
fi

# Return to original directory
popd > /dev/null