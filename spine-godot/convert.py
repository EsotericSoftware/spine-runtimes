# 
# Python script to convert a Godot project using Spine .json skeleton files to use the new
# extension .spine-json instead.
#
# Usage: python convert.py path/to/godot/project
#
# Note: ensure you have created a backup of your Godot project before running this script.
#
# The script will traverse all .json, .tscn, and .tres files in the directory recursively.
#
# For each .json file, it will rename the file to .spine-json and remove the .json.import file.
# Upon reloading the the project in Godot, the file will be re-imported and the a .spine-json.import
# file will be created in place of the .json.import file.
#
# For each .tscn or .tres file, it will replace the .json suffix in external resources of type
# SpineSkeletonFileResource with the new suffix .spine-json.
#
import sys
import os
import pathlib
import codecs

def convert_json(filename):
    file = codecs.open(filename, "r", "utf-8")
    content = file.read()
    file.close()
    
    if "skeleton" in content and "hash" in content and "spine" in content:
        path = pathlib.Path(filename)
        new_path = path.with_suffix('.spine-json')
        print("Renaming " + str(path) + " to " + str(new_path))
        path.rename(new_path)
        if os.path.exists(filename + ".import"):
            print("Removing " + str(filename) + ".import")
            os.remove(filename + ".import")

def convert_tscn_or_tres(filename):    
    file = codecs.open(filename, "r", "utf-8")
    content = file.read()
    file.close()

    new_content = ""
    is_converted = False
    for line in content.splitlines(True):
        if line.startswith("[ext_resource") and 'type="SpineSkeletonFileResource"' in line and '.json"' in line:
            if not is_converted:
                print("Converting TSCN file " + str(filename))
                is_converted = True
            print("Replacing .json with .spine-json in \n" + line)
            line = line.replace('.json"', '.spine-json"')
        new_content += line

    file = codecs.open(filename, "w", "utf-8")
    file.write(new_content)
    file.close()

def convert_tres(filename):
    print("Converting TRES file " + str(filename))
    with open(filename) as file:
        content = file.read()

def convert(path):
    for dirpath, dirs, files in os.walk(path):	
        for filename in files:
            file = os.path.join(dirpath,filename)
            if file.endswith(".json"):
                convert_json(file)
            elif file.endswith(".tscn") or file.endswith(".tres"):
                convert_tscn_or_tres(file)            

if __name__ == "__main__":
    if len(sys.argv) == 1:
        print("Please provide the path to your Godot project, e.g. python convert.py path/to/my/project.")
        sys.exit(-1)
    path = os.path.abspath(sys.argv[1])
    if not os.path.exists(path):
        print("Directory " + str(path) + " does not exist.")
        sys.exit(-1)
    if not os.path.isdir(path):
        print(str(path) + " is not a directory.")
        sys.exit(-1)
    print("Converting " + str(path))
    convert(path)    