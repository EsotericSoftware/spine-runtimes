The Spine runtime for Unity comes with an example project which has "spineboy" walking. When clicked, he jumps and the transition to/from walking/jumping is blended smoothly.

# Requirements

1. Unity 4.2+

# Instructions

1. Copy `spine-csharp/src` to `spine-unity/Assets/Spine/spine-csharp`.
1. Open the `Assets/examples/spineboy/spineboy.unity` scene.

# Setup Tutorial Video

[![Setup tutorial video](http://i.imgur.com/2AyZq01.png)](http://www.youtube.com/watch?v=x1umSQulghA)

# Notes

- Atlas images should use premultiplied alpha.
- Unity scales large images down by default if they exceed 1024x1024, which causes the altas coordinates to be incorrect. To fix this, override the import settings in the Inspector for any large atlas image you have so Unity does not scale it down.
