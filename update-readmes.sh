#!/bin/sh
# Needs gnu-sed on macOS for inplace replacement
# brew install --with-default-names gnu-sed
set -e

if [ -z "$1" ] 
	then
		echo "Usage: ./update-readmes.sh <from-branch> <to-branch>"
		echo "Example: ./update-readmes.sh 3.6 3.7"
		exit
fi

if [ -z "$2" ] 
	then
		echo "Usage: ./update-readmes.sh <from-branch> <to-branch>"
		echo "Example: ./update-readmes.sh 3.6 3.7"
		exit
fi

find . -type f -name 'README.md' | while read line; do
	echo "Updating $line"
	sed -i "s,https://github.com/EsotericSoftware/spine-runtimes/archive/$1.zip,https://github.com/EsotericSoftware/spine-runtimes/archive/$2.zip,g" $line
done 