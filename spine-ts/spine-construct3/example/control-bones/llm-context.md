# This is a Construct project
This folder contains a [Construct](https://www.construct.net) project. Construct 3 is the world's best browser-based game development tool combining ease of use with powerful features, and is an ideal choice for beginner game developers. Construct Animate is also an easy-to-use and capable browser-based animation tool. Both products share the same project format. A brief explanation of the project format is included here to assist large language models (LLMs) in understanding it. A more detailed overview is available in the official guide [Construct's project format](https://www.construct.net/en/tutorials/constructs-project-format-3275), so refer to that guide if possible. Further information can be found on the official Construct website, which includes an online manual with a detailed reference.

## The main project file
The file named `project.c3proj` is the main project file. It is in JSON format and acts as an index of all the content in the project. It also stores the basic project metadata, such as the name, viewport size, and so on.

## Subfolders
Here is a brief summary of what each subfolder in a Construct project is used for. Note some subfolders may be missing if they are not used.

- `objectTypes`: the project's object types, such as *PlayerSprite*, in JSON format.
- `images`: images for object types that use images or animations. Construct objects like Tiled Background and 9-patch use a single image, and so have a single file in this folder using the lowercased object name, e.g. *tiledbackground.png*. Animated objects (Sprite) have multiple image files using a filename in the format `objectname-animationname-framenumber` with the frame number zero-padded to three digits, e.g. *player-default-000.png*.
- `families`: the project's families, which are essentially groups of object types, in JSON format.
- `layouts`: the project's layouts, which consist of layers and arrangements of objects on those layers for purposes like menu and level design, in JSON format.
- `eventSheets`: the project's event sheets, which consist of Construct's visual blocks that it uses as an alternative to traditional programming languages, in JSON format
- `scripts`: JavaScript or TypeScript script files used by the project
- `sounds` and `music`: audio files in WebM Opus format
- `videos`: video files, typically in MPEG-4 H.264 format
- `fonts`: web fonts in WOFF format
- `icons`: icons and splash screens in PNG format
- `files`: general additional files of any kind that the project may make use of
- `timelines`: the project's timelines, which consist of a sequence of changes over time, such as to control a title screen animation, in JSON format
- `flowcharts`: the project's flowcharts, which consist of a series of connected nodes for purposes like conversation trees and finite state machines, in JSON format
- `3dmodels`: the project's 3D models, in JSON format. Construct supports importing GLTF (.gltf and .glb) models; these are processed during the import process and the saved JSON files are similar to but not necessarily exactly the same as GLTF files.

## UI state files
Construct projects contain a variety of files with the extension `.uistate.json`. These store information about the state of the user interface in the Construct editor. They do not affect the operation of the project in any way, and are probably best ignored by tools or LLMs.