#!/bin/bash

# Keep Git Bash / MSYS windows open after the script finishes so errors can be read.
# Set SPINE_GODOT_NO_PAUSE=1 to skip (CI, agents, nested scripts).

pause_on_exit() {
	local exit_code=$?
	if [[ "$OSTYPE" == "msys"* || "$OSTYPE" == "win32"* || "$OSTYPE" == "cygwin"* ]]; then
		if [ -z "${SPINE_GODOT_NO_PAUSE:-}" ]; then
			echo ""
			if [ $exit_code -eq 0 ]; then
				echo "Build finished successfully."
			else
				echo "Build failed with exit code $exit_code."
			fi
			echo "Press Enter to close this window..."
			read -r _
		fi
	fi
	return $exit_code
}

install_pause_on_exit() {
	trap pause_on_exit EXIT
}
