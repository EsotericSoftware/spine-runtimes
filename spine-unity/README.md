The Spine runtime for Unity comes with an example project which has "spineboy" walking. When clicked, he jumps and the transition to/from walking/jumping is blended smoothly. Use the instructions below for your version of Unity.

# Unity 4

1. Delete the "Assets/examples/Unity 3.5" directory.
1. Open the "Assets/examples/Unity 4/spineboy.unity" scene.

# Unity 3.5.7

1. Delete the "Assets/examples/Unity 4" directory.
1. Open the "Assets/examples/Unity 3.5/spineboy.unity" scene.

Unity 3.5.7 stores metadata in the project files, which are not included. References in the project need to be fixed up:

1. In the Project, select "examples/Unity 3.5/spineboy/spineboy Atlas".
1. In the Inspector, set the material to "spineboy".
1. In the Project, select "examplesUnity 3.5/spineboy/spineboy Skeleton Data".
1. In the Inspector, set the atlas to "spineboy Atlas".
1. In the Hierarchy, select "spineboy".
1. In the Inspector, set the skeleton component's skeleton data to "spineboy Skeleton Data".
1. In the Inspector, set the missing script to "SpineboyComponent".
