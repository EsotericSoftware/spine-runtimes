#!/bin/bash
set -e
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

trap "cleanup" ERR

setup() {
	cp $dir/.clang-format $dir/..
	cp $dir/build.gradle $dir/..
	cp $dir/settings.gradle $dir/..	
}

cleanup() {
	rm $dir/../.clang-format
	rm $dir/../build.gradle
	rm $dir/../settings.gradle
}

# copy Gradle and clang-format config to root
setup

# Execute spotless
pushd $dir/..
./formatters/gradlew spotlessApply
popd

# Delete Gradle and clang-format config files in root
cleanup