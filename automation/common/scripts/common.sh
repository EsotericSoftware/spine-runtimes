#!/bin/bash

on_error() {
  local parent_lineno="$1"
  local message="$2"
  local code="${3:-1}"
  if [[ -n "$message" ]] ; then
    echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    echo "!!! Error on or near line ${parent_lineno}: ${message}; exiting with status ${code}"
    echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  else
    echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    echo "!!! Error on or near line ${parent_lineno}; exiting with status ${code}"
    echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  fi
  exit "${code}"
}
trap 'on_error ${LINENO}' ERR

# prepare src dir via sparse checkout of spine-runtimes repo.
prepare_sparse_checkout_dir() {

    trap 'on_error ${LINENO}' ERR

    local out_commit_hash=$1
    local git_root_url="$2"
    local git_branch="$3"
    local git_subdir="$4"
    local commit_hash="$5"
    local dst_dir="$6"
    local gitattributes_file="$7"
    local prev_cwd=$(pwd)

    echo Preparing sparse checkout directory $dst_dir.
    mkdir -p "$dst_dir"
    cp "$gitattributes_file" "${dst_dir}/.gitattributes"
    
    cd "$dst_dir"

    if [ ! -d ".git" ]; then
        git init .
        git config core.sparseCheckout true
        # note: we want crlf in this repo since we got the most recent unitypackage commits
        # with crlf and want to minimize changes in submitted unitypackages.
        git config core.autocrlf true
        git remote add -f origin "${git_root_url}"
        echo "${git_subdir}/" > .git/info/sparse-checkout
        git checkout -f "${git_branch}"
    fi

    git pull
    git reset --hard "${commit_hash}"
    git clean -f -d
    out_hash=$(git rev-parse HEAD)
    eval "$out_commit_hash='${out_hash}'"

    cd "${prev_cwd}"
}

convert_file_line_ending() {

    local file="$1"
    # note: lf_handling = "conservative", "unify-to-crlf" or "unify-to-lf"
    local lf_handling="$2"

    # note: dos2unix will skip binary files, nevertheless we want to be conservative and check above
    file -bL --mime "${file}" | grep -q '^text'
    local grep_text_retval=$?
    if [ "${grep_text_retval}" == 0 ]; then
   
        local extension="${file##*.}"
        #local first_5_chars=$(head -c 5 "${file}")
        if ( [ "$extension" == "cs" ] || [ "$extension" == "json" ] || [ "$extension" == "txt" ] \
            || [ "$extension" == "cginc" ] || [ "$extension" == "asmdef" ] \
            || [ "$extension" == "shader" ] || [ "$extension" == "md" ] ); then

            local newline_count=`wc -l < "${file}"`
            local windows_cr_count=`grep -ac $'\r' "${file}"`
            if [ "$lf_handling" == "conservative" ]; then
                # convert only mixed ending files, convert to lf
                if [ ${windows_cr_count} -gt "0" ] && [ ${newline_count} != ${windows_cr_count} ]; then
                    # mixed line endings
                    # echo ${file} has cr count ${windows_cr_count}
                    dos2unix "${file}"
                    echo Converted mixed line-ending file ${file} to lf.
                fi
            elif [ "$lf_handling" == "unify-to-crlf" ]; then
                if [ ${newline_count} != ${windows_cr_count} ]; then
                    unix2dos "${file}"
                    echo Converted file ${file} to crlf.
                fi
            elif [ "$lf_handling" == "unify-to-lf" ]; then
                if [ ${windows_cr_count} -gt "0" ]; then
                    dos2unix "${file}"
                    echo Converted file ${file} to lf.
                fi
            fi
        elif  ( [ "$extension" == "meta" ] || [ "$extension" == "mat" ] || [ "$extension" == "unity" ] \
            || [ "$extension" == "physicsMaterial2D" ] || [ "$extension" == "physicsMaterial" ] \
            || [ "$extension" == "asset" ] || [ "$extension" == "controller" ] ); then

            # unity serialized files are always lf
            local windows_cr_count=`grep -ac $'\r' "${file}"`
            if [ ${windows_cr_count} -gt "0" ]; then
                dos2unix "${file}"
                echo Converted file ${file} to lf.
            fi
        fi
    fi
}

convert_all_files_line_endings_of_dir() {
    local path="$1"
    local lf_handling="$2"

    find "$path" -type f |
    while read file ; do 
        convert_file_line_ending "${file}" "${lf_handling}"
    done
}

