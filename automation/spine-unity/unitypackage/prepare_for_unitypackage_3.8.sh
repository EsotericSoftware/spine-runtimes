#!/bin/bash
git_branch="3.8-beta"
spine_version="3.8"

src_commit_hash="HEAD"
date_string=`date +%Y-%m-%d`
package_filename="spine-unity-3_8-${date_string}-beta.unitypackage"
unityproject_base="./spine-3.8-unity2017-unitypackage"

input_args=()
while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    -h|--hash)
    src_commit_hash="$2"
    shift # past argument
    shift # past value
    ;;
    -f|--filename)
    package_filename="$2"
    shift # past argument
    shift # past value
    ;;
    *)    # unknown option
    input_args+=("$1") # save it in an array for later
    shift # past argument
    ;;
esac
done
set -- "${input_args[@]}" # restore positional parameters
if [[ -n $1 ]]; then
    echo "Last line of file specified as non-opt/last argument:"
    tail -1 "$1"
fi

./prepare_for_unitypackage_impl.sh --dir "${unityproject_base}" --version "${spine_version}" --branch "${git_branch}" --hash "${src_commit_hash}"  --filename "${package_filename}"
