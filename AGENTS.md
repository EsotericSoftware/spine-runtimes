# Spine Runtimes Agent Rules

## Minimal operating rules
- Keep changes scoped to the requested task.
- Do not commit unless the user explicitly asks.
- Before editing, read files in full, especially if the read tool truncates them.
- Follow existing code style in touched files (naming, type usage, control flow, and error handling patterns).

## Git commit subject prefix (required)
Every commit subject must start with a runtime prefix.

Format:
- Single runtime: `[unity] Fix clipping regression`
- Multiple runtimes: `[c][cpp] Sync physics constraint handling`

Use lowercase prefixes exactly as listed below.

### Runtime prefixes
- `[android]` -> `spine-android`
- `[c]` -> `spine-c`
- `[cocos2dx]` -> `spine-cocos2dx`
- `[cpp]` -> `spine-cpp`
- `[csharp]` -> `spine-csharp`
- `[flutter]` -> `spine-flutter`
- `[glfw]` -> `spine-glfw`
- `[godot]` -> `spine-godot`
- `[haxe]` -> `spine-haxe`
- `[ios]` -> `spine-ios`
- `[libgdx]` -> `spine-libgdx`
- `[monogame]` -> `spine-monogame`
- `[runtimes]` -> all runtimes (use for repo-wide changes like example re-exports)
- `[sdl]` -> `spine-sdl`
- `[sfml]` -> `spine-sfml`
- `[ts]` -> `spine-ts`
- `[ue]` -> `spine-ue`
- `[unity]` -> `spine-unity`
- `[xna]` -> `spine-xna`

### Prefix selection rules
- If one runtime is changed, use one prefix.
- If multiple runtimes are changed, include multiple prefixes.
- If all runtimes are changed, use `[runtimes]`.
- If shared files at repo root are changed, include the runtime prefix(es) impacted by that change.

## spine-cpp / spine-libgdx test infrastructure

The spine-cpp headless test (`spine-cpp/tests/test.sh`) validates that spine-cpp produces the same parsed skeleton data as spine-libgdx. Both runtimes have a `SkeletonSerializer` that serializes loaded skeleton data to JSON for comparison.

Files:
- `spine-cpp/tests/HeadlessTest.cpp` - C++ test that loads skeleton and serializes to JSON
- `spine-cpp/tests/SkeletonSerializer.h` - C++ JSON serializer for skeleton data
- `spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/HeadlessTest.java` - Java equivalent
- `spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/utils/SkeletonSerializer.java` - Java equivalent

When tests disagree:
1. Check if the skeleton was loaded from JSON or binary. Run both to isolate which parser differs.
2. Compare the SkeletonSerializer implementations. They must output identical JSON structure.
3. Check for unported changes in spine-libgdx (SkeletonBinary.java, SkeletonJson.java) and port them to spine-cpp.
4. Update `spine-cpp/tests/test.sh` expected output if the serializer format changed intentionally.
