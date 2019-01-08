#!/bin/bash

#include common script functionality
source "../../common/scripts/common.sh"

# argument parsing
src_commit_hash="HEAD"
date_string=`date +%Y-%m-%d`
package_filename="spine-unity-3_X-${date_string}.unitypackage"
git_branch="3.6"
spine_version="3.6"
unityproject_base="./spine-3.6-unity5.6-unitypackage"
git_track_package_changes=false
git_commit_package_changes=false
input_args=()
while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    -b|--branch)
    git_branch="$2"
    shift # past argument
    shift # past value
    ;;
    -h|--hash)
    src_commit_hash="$2"
    shift # past argument
    shift # past value
    ;;
    -v|--version)
    spine_version="$2"
    shift # past argument
    shift # past value
    ;;
    -t|--trackchanges)
    git_track_package_changes=$2
    shift # past argument
    shift # past value
    ;;
    -c|--commitchanges)
    git_commit_package_changes=$2
    shift # past argument
    shift # past value
    ;;
    -d|--dir)
    unityproject_base="$2"
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

echo src_commit_hash = "${src_commit_hash}"
echo git_branch = "${git_branch}"
echo spine_version = "${spine_version}"
echo unityproject_base = "${unityproject_base}"
echo package_filename = "${package_filename}"
if [[ -n $1 ]]; then
    echo "Last line of file specified as non-opt/last argument:"
    tail -1 "$1"
fi


# configuration
git_root="https://github.com/EsotericSoftware/spine-runtimes.git"
git_subdir_spine_csharp="spine-csharp/src"
git_subdir_spine_unity="spine-unity/Assets"
spine_csharp_metafiles_dir="spine-csharp-metafiles"

if [ $spine_version == "3.6" ]; then
    src_spine_unity_subdir="spine-unity/Assets/spine-unity"
    dst_spine_csharp_subdir="Assets/Spine/spine-csharp"
    dst_spine_unity_subdir="Assets/Spine/spine-unity"
    lf_handling="conservative"
else
    
    src_spine_unity_subdir="spine-unity/Assets/Spine"
    dst_spine_csharp_subdir="Assets/Spine/Runtime/spine-csharp"
    dst_spine_unity_subdir="Assets/Spine"
    lf_handling="unify-to-crlf"
fi
checkout_dir_base="${unityproject_base}/sparse_checkout"
checkout_dir_spine_csharp="${checkout_dir_base}/sparse_spine-csharp"
checkout_dir_spine_unity="${checkout_dir_base}/sparse_spine-unity"

src_spine_csharp="${checkout_dir_spine_csharp}/spine-csharp/src"
src_spine_unity="${checkout_dir_spine_unity}/${src_spine_unity_subdir}"
src_spine_examples="${checkout_dir_spine_unity}/spine-unity/Assets/Spine Examples"
dst_spine_csharp="${unityproject_base}/${dst_spine_csharp_subdir}"
dst_spine_unity="${unityproject_base}/${dst_spine_unity_subdir}"
dst_spine_examples="${unityproject_base}/Assets/Spine Examples"
dst_assets_base_dir="${unityproject_base}/Assets"

# path initialization
prev_workingdir="$(pwd)"
script_path=${0%/*}
cd ${script_path}

trap 'on_error ${LINENO}' ERR




main() {

    trap 'on_error ${LINENO}' ERR

    echo ----------------------------------------------------------------------
    echo Preparing sparse checkout reference dirs
    echo ----------------------------------------------------------------------
    mkdir -p "$checkout_dir_base"

    out_commit_hash1="invalid1"
    out_commit_hash2="invalid2"
    for ((n=0; n<3; n++));
    do
        # if commit hashes have just changed between calls, repeat preparation (pull) in both sparse checkout dirs.
        prepare_sparse_checkout_dir out_commit_hash1 "${git_root}" "${git_branch}" "${git_subdir_spine_csharp}" "${src_commit_hash}" "${checkout_dir_spine_csharp}" .gitattributes_for_sparse_co
        prepare_sparse_checkout_dir out_commit_hash2 "${git_root}" "${git_branch}" "${git_subdir_spine_unity}" "${src_commit_hash}" "${checkout_dir_spine_unity}" .gitattributes_for_sparse_co

        echo Commit hash spine-sharp = $out_commit_hash1
        echo Commit hash spine-unity = $out_commit_hash2

        [ "$out_commit_hash1" == "$out_commit_hash2" ] && break
        
    done
    if [ "$out_commit_hash1" != "$out_commit_hash2" ]; then
        on_error ${LINENO} "Failed to get equal commit hashes on both sparse checkouts 3 times - very unlikely, seems as if something has gone wrong." 1
    fi

    echo ----------------------------------------------------------------------
    echo Copying changes to target directory
    echo ----------------------------------------------------------------------
    # prepare target dir to track changes or not (via .gitignore ignore directory)
    if [ $git_track_package_changes == true ]; then
        rm "${dst_assets_base_dir}/.gitignore"
    else
        echo '*' > "${dst_assets_base_dir}/.gitignore"
    fi

    # copy latest changes from repository over to our Assets dir and delete old content beforehand.
    if [ $spine_version == "3.6" ]; then
        # in spine 3.6 we want to be more conservative and keep existing meta files
        # (they come from a previously extracted reference 3.6 unitypackage).
        # so in spine-csharp and dst_spine_unity dirs, we don't want to delete meta files.
        find "$dst_spine_csharp/" -type f -not -name '*.meta' -delete
        find "$dst_spine_csharp/" -type d -empty -delete
        find "$dst_spine_unity/" -type f -not -name '*.meta' -delete
        find "$dst_spine_unity/" -type d -empty -delete
    else
        # in spine 3.7 we delete all existing files and overwrite with pre-defined meta
        # files from $spine_csharp_metafiles_dir.
        rm -rf "$dst_spine_csharp/"*
        rm -rf "$dst_spine_unity/"*
    fi
    rm -rf "$dst_spine_examples/"*
    
    echo copying from "$src_spine_unity" to "$dst_spine_unity"
    cp -r "$src_spine_unity/." "$dst_spine_unity"
    echo copying "${src_spine_unity}.meta" to "${dst_spine_unity}.meta"
    cp "${src_spine_unity}.meta" "${dst_spine_unity}.meta"
    echo copying from "$src_spine_csharp" to "$dst_spine_csharp"
    cp -r "$src_spine_csharp/." "$dst_spine_csharp"
    echo copying from "$src_spine_examples" to "$dst_spine_examples"
    cp -r "$src_spine_examples/." "$dst_spine_examples"
    echo copying "${src_spine_examples}.meta" to "${dst_spine_examples}.meta"
    cp "${src_spine_examples}.meta" "${dst_spine_examples}.meta"
    if [ $spine_version != "3.6" ]; then
        echo copying meta files from "$spine_csharp_metafiles_dir/" to "$dst_spine_csharp/"
        cp -r "$spine_csharp_metafiles_dir/"* "$dst_spine_csharp/"
    fi

    if [ $spine_version != "3.6" ]; then
        rm -f "$dst_spine_csharp/add spine-csharp here.txt"
        rm -f "$dst_spine_csharp/add spine-csharp here.txt.meta"
    fi
    
    echo ----------------------------------------------------------------------
    echo Evaluating changes
    echo ----------------------------------------------------------------------
    if [ "$lf_handling" == "conservative" ]; then
        # remove whitespace-only changes, leaving mixed line-ending files where
        # changes with other endings are applied
        
        # add all diffs with whitespace-changes removed.
        local diff_without_whitespace=$(git diff -U0 -w --no-color --binary ${unityproject_base})
        if [ -z "$diff_without_whitespace" ]; then
            echo No non-whitespace changes to commit or package - did you call the script twice?
            exit 1
        fi
        git diff -U0 -w --no-color --binary ${unityproject_base} | git apply --cached --ignore-whitespace --unidiff-zero - && git checkout -- ${unityproject_base}  && git reset
    fi

    if [ "$lf_handling" == "conservative" ] && [ $git_track_package_changes == true]; then
        git add "$dst_spine_csharp/"
        git add "$dst_spine_unity/"
        git add "$dst_spine_examples/"

        git diff --name-only --cached ${unityproject_base} |
        while read file; do
            convert_file_line_ending "${file}" "${lf_handling}"
        done
    else
        convert_all_files_line_endings_of_dir "$dst_spine_csharp/" "${lf_handling}"
        convert_all_files_line_endings_of_dir "$dst_spine_unity/" "${lf_handling}"
        convert_all_files_line_endings_of_dir "$dst_spine_examples/" "${lf_handling}"
    fi

    if [ $git_track_package_changes == true ]; then
        
        echo ----------------------------------------------------------------------
        echo Staging changes for git repository
        echo ----------------------------------------------------------------------
        git add "$dst_spine_csharp/"
        git add "$dst_spine_unity/"
        git add "$dst_spine_examples/"
    
        if [ $git_commit_package_changes == true ] ; then
            echo ----------------------------------------------------------------------
            echo Committing changes to git repository
            echo ----------------------------------------------------------------------
            local optional_excluding_whitespace_message=""
            if [ "$lf_handling" == "conservative" ]; then
                optional_excluding_whitespace_message=" (excluding whitespace changes)"
            fi
            
            git commit -m "[unity][auto] ${package_filename}: integrated changes of main repository up to commit ${out_commit_hash1}${optional_excluding_whitespace_message}."
        fi
    fi
    
    echo ----------------------------------------------------------------------
    echo Finished successfully
    echo 
    echo Please open the Unity project and resolve potential remaining issues.
    echo 
    echo To export unitypackage:
    echo Select directories \"Spine\" and \"Spine Examples\" in Project window
    echo then right-click to open context menu, select \"Export Package..\".
    echo Untick \"Include dependencies\" and hit \"Export..\".
    echo Please name the package ${package_filename}.
    echo ----------------------------------------------------------------------
    
    # reset cwd
    cd "${prev_workingdir}"
}

main
