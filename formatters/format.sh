#!/bin/bash
set -e
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

trap "cleanup" ERR

setup() {
	cp $dir/.clang-format $dir/..
	cp $dir/build.gradle $dir/..
	cp $dir/settings.gradle $dir/..	
	cp $dir/.editorconfig $dir/../spine-csharp	
	cp $dir/.editorconfig $dir/../spine-monogame
	cp $dir/.editorconfig $dir/../spine-unity
}

cleanup() {
	rm $dir/../.clang-format
	rm $dir/../build.gradle
	rm $dir/../settings.gradle
	rm $dir/../spine-csharp/.editorconfig		
	rm $dir/../spine-monogame/.editorconfig
	rm $dir/../spine-unity/.editorconfig
}

# copy Gradle, dotnet-format, and clang-format config to root
setup

# Execute spotless and dotnet-format
pushd $dir/..
./formatters/gradlew spotlessApply
if [ "$1" != "skipdotnet" ] ; then
	dotnet-format spine-csharp/spine-csharp.sln
	dotnet-format -f spine-monogame
	dotnet-format -f spine-unity
fi
popd

# Delete Gradle, dotnet-format, and clang-format config files in root
cleanup