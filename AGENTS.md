# Spine Runtimes Agent Rules

## Minimal operating rules
- Keep changes scoped to the requested task.
- Do not commit unless the user explicitly asks.
- Before editing, read files in full, especially if the read tool truncates them.
- Follow existing code style in touched files (naming, type usage, control flow, and error handling patterns).

## Files to never commit
- **NEVER** commit Eclipse settings files (`.settings/`, `.classpath`, `.project`). These are IDE-specific and must not be checked in. If `git status` shows changes to these files, revert them before committing.
- **NEVER** commit the vendored spine-cpp copies inside `spine-ue/`, `spine-godot/`, `spine-flutter/`, or `spine-ios/`. These are managed by setup/build scripts and are gitignored. Only commit changes to the wrapper/binding code in those runtimes, not the vendored spine-cpp sources.

## CHANGELOG.md
- When making API changes, adding features, or fixing bugs, add entries to `CHANGELOG.md` under the appropriate runtime section(s) in the `# 4.3` block.
- Use `- **Additions**` for new features, `- **Breaking changes**` for API changes, `- **Bug fixes**` for fixes.
- Add entries for every runtime affected, including downstream runtimes that get regenerated bindings (C, Flutter, iOS, Godot).

## spine-godot clean builds
- For a clean Godot 4.x module build against the latest local `spine-cpp`, use the setup script before building so the vendored `spine-godot/spine_godot/spine-cpp` copy is refreshed from repo root:
  ```bash
  cd spine-godot/build
  ./setup.sh 4.6.1-stable false false
  ./build-v4.sh false
  ```
- For a clean Godot 4.x GDExtension build against the latest local `spine-cpp`, use:
  ```bash
  cd spine-godot/build
  ./setup-extension.sh 4.6.1-stable false
  ./build-extension.sh macos
  ```
- `setup.sh` and `setup-extension.sh` must remove `spine-godot/spine_godot/spine-cpp` before copying `../spine-cpp`, otherwise stale vendored files can survive across builds.
- When reproducing CI failures for Godot 4.x, match the current workflows' Godot tag first. At the time of writing, that is `4.6.1-stable`.

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

## Porting from spine-libgdx to spine-cpp
- Process libgdx commits oldest first.
- Only port one libgdx commit at a time unless explicitly asked to batch commits.
- Start each porting session on a new local branch created from the current base branch. Keep the branch local and use it to accumulate the per-commit port history until it is later squash merged.
- For each commit:
  1. Identify the oldest unported `spine-libgdx` commit in scope.
  2. View the diff for that commit in `spine-libgdx`.
  3. Map the changed Java files to the corresponding `spine-cpp` `.h` and `.cpp` files.
  4. Read the relevant target files in full before editing.
  5. If the mapping, intent, or equivalent spine-cpp API is unclear, stop and ask questions before changing code.
  6. Keep the port scoped to that commit only.
  7. After the port and validation for that libgdx commit are complete, make a local commit immediately so porting state is explicit and resumable.
  8. The commit subject must include the libgdx source commit hash being ported.
- Porting rules:
  - If the libgdx commit changes code, port the code.
  - If the libgdx commit is docs only, port only the relevant docs.
  - Convert Javadocs to Doxygen-style comments where appropriate for `spine-cpp`.
  - When docs reference Java-specific APIs, rewrite them to the equivalent `spine-cpp` APIs instead of copying them literally.
  - Preserve existing `spine-cpp` style, naming, control flow, and error handling.
  - Keep behavior aligned with the Java change.

### After each ported commit
1. Compile `spine-cpp` directly:
   ```bash
   cd spine-cpp
   ./build.sh clean debug
   ```
2. Regenerate downstream bindings from the repo root so `spine-c`, `spine-flutter`, and `spine-ios` stay in sync:
   ```bash
   ./generate-bindings.sh
   ```
3. Run the `spine-cpp` smoke tests:
   ```bash
   cd spine-cpp
   ./tests/test.sh
   ```
4. Run the snapshot harness to confirm Java and C++ functional equivalence for assets that exercise the change. Choose the skeleton, atlas, and animation based on the code that changed. For parser-related work, run both binary and JSON inputs.
   Example:
   ```bash
   cd tests
   ./test.sh java ../examples/spineboy/export/spineboy-pro.skel ../examples/spineboy/export/spineboy-pma.atlas walk > /tmp/spine-java.json
   ./test.sh cpp ../examples/spineboy/export/spineboy-pro.skel ../examples/spineboy/export/spineboy-pma.atlas walk > /tmp/spine-cpp.json
   diff -u /tmp/spine-java.json /tmp/spine-cpp.json
   ```
5. Compile `spine-glfw` and all examples:
   ```bash
   cd spine-glfw
   ./build.sh clean debug
   ```
6. When a graphical session is available, launch all `spine-glfw` examples in one bash call from `spine-glfw/` so relative asset paths resolve correctly and the user can verify behavior manually. Keep the `cd spine-glfw` scoped over the entire multi-process launch, eg with a subshell, so every backgrounded example inherits the same working directory.
   ```bash
   (cd spine-glfw; \
   mapfile -t bins < <(find ./build/debug -maxdepth 1 -type f -perm -111 -name 'spine-glfw-*' | sort); \
   for bin in "${bins[@]}"; do \
   	"$bin" & \
   done; \
   wait)
   ```
   Do not assume a hardcoded example list is exhaustive. New `spine-glfw` examples may be added in the future, so inspect the current executables in `spine-glfw/build/debug/` and include any new example binaries instead of assuming an older command still covers everything.
7. If the port changes the public API, investigate downstream wrapper runtimes before considering the port complete:
   - `spine-godot`: check whether Godot script bindings need methods or properties added, changed, or removed in the wrapper classes and `ClassDB::bind_method` registrations.
   - `spine-ue`: check whether Unreal wrappers need methods or properties added, changed, or removed in the Blueprint-exposed `UFUNCTION` and `UPROPERTY` APIs.

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
