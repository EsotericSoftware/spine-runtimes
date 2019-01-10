#!/bin/bash

date_string=`date +%Y-%m-%d`
package_filename="spine-unity-3_7-${date_string}.unitypackage"
export_dir="./packages"
unity_binary="/C/Program Files/Unity5.6/Editor/Unity"

input_args=()
while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    -u|--unity)
    unity_binary="$2"
    shift # past argument
    shift # past value
    ;;
    -f|--filename)
    package_filename="$2"
    shift # past argument
    shift # past value
    ;;
    -d|--export_dir)
    export_dir="$2"
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

logfile_dir="$export_dir/logfiles" 

dir_spine="Assets/Spine"
dir_examples="Assets/Spine Examples"


# path initialization
prev_workingdir="$(pwd)"
script_path=${0%/*}
cd ${script_path}
absolute_script_path="$(pwd)"

unityproject_base="${absolute_script_path}/spine-3.7-unity5.6-unitypackage"

# prepare output dirs and pacakge
mkdir -p "$export_dir"
mkdir -p "$logfile_dir"

echo Writing unitypackage to "$export_dir/$package_filename", unity="$unity_binary"
"$unity_binary" -batchmode -nographics -logFile "$logfile_dir/${package_filename}.log" -projectPath "$unityproject_base" -exportPackage "$dir_spine" "$dir_examples" "../$export_dir/$package_filename" -quit

cd ${prev_workingdir}
