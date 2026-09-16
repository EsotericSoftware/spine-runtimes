# Corrections of Fable 5.1: On-demand secondary textures

Keep the simpler implementation rather than revert to Astra. The requested corrections and compatibility simplifications are now implemented.

## Current status

- **Completed:** original findings #1, #2, #3, #5, #6, #7 (build validation), and #8, plus shared-placeholder regeneration, asset-editing exception cleanup, and older-Unity `[MainTexture]` resolution.
- **Open:** the async-completion overwrite race (follow-up #2 below).
- **Intentionally deferred:** original finding #4 and `SkeletonGraphic` secondary-texture support, as requested.
- **Verification pending:** real Unity editor/build integration tests. Isolated checks do not establish actual importer/GUID behavior.

## Original findings and resolutions

The problem descriptions below record the original review findings, not the current behavior of completed items. Source links point to the current relevant methods.

### 1. [done] Removing a property can delete another loader’s placeholder

**[GenericOnDemandTextureLoaderInspector.cs:676](spine-unity/Modules/com.esotericsoftware.spine.on-demand-loading/Editor/GenericOnDemandTextureLoaderInspector.cs#L676)**

Placeholder paths are derived from the source texture, so two loaders using the same normal/mask texture share its placeholder asset.

`DeleteUnusedPlaceholderTextures` only checks the current loader’s new map. Removing `_BumpMap` from loader A therefore deletes the placeholder still referenced by loader B. Regeneration’s unconditional deletion has the same ownership problem.

**Fix:** either check other loaders before deletion or leave shared placeholder assets intact. Astra also had the regeneration problem; its deletion approach should not be copied unchanged.

**Resolution:** deletion now checks other loaders through the non-generic `OnDemandTextureLoader.IsPlaceholderTexture` API, including different generic backend types. Regeneration retains shared assets instead of deleting them. Direct non-generic custom loaders must override `IsPlaceholderTexture` to expose their placeholder ownership.

### 2. [done] A deliberately cleared secondary texture gets restored unintentionally

**[GenericOnDemandTextureLoader.cs:594](spine-unity/Modules/com.esotericsoftware.spine.on-demand-loading/Runtime/GenericOnDemandTextureLoader.cs#L594)**

Reproduction:

1. Generate a mapping for an assigned normal map.
2. Clear the material’s normal-map property without regenerating.
3. Build.

Validation accepts `currentTexture == null`. Placeholder assignment then unconditionally fills the atlas material’s secondary slot, and post-build restoration assigns the old normal map back.

This changes both the build’s appearance and the editor material.

**Fix:** distinguish missing-main-texture recovery from intentionally empty secondary properties. Null secondary slots should not be filled from stale mappings.

**Resolution:** placeholder assignment skips cleared secondary slots. Editor restoration replaces placeholders and can recover a missing main texture, without restoring intentionally cleared secondary textures.

### 3. [done] The material scan stops after the first matching loader

**[SpineBuildProcessor.cs:346](spine-unity/Assets/Spine/Editor/spine-unity/Editor/Utility/SpineBuildProcessor.cs#L346)**

Suppose loaders A and B have different main textures but share a normal map. When scanning a copy of B’s material:

- A replaces the shared normal map.
- The loop breaks.
- B’s main texture remains a direct high-resolution reference.

Consequently, a secondary-texture match can prevent on-demand loading of the much larger main texture. The subsequent warning misleadingly says its property is not covered.

**Fix:** allow multiple loaders to process each material and retain multiple restoration records. Astra handled this correctly.

**Resolution:** the scan allows multiple loaders to replace matching properties of a material and retains their restoration records.

### 4. [NOT DOING] Main-texture matching can select the wrong secondary mappings

**[GenericOnDemandTextureLoader.cs:609](spine-unity/Modules/com.esotericsoftware.spine.on-demand-loading/Runtime/GenericOnDemandTextureLoader.cs#L609)**

For atlas materials, `mapIndex` is replaced by the first entry matching the main texture. All secondary placeholders then come from that entry.

If two atlas materials share a diffuse texture but have different normal maps, the second receives the first material’s normal placeholder. Post-build restoration does not repair it: the new “only replace null or this entry’s placeholder” check rejects that wrong placeholder.

**Fix:** use the material’s existing map index for atlas materials. For other materials, match each property independently. Alternatively, explicitly reject this configuration during validation.

### 5. [done] Existing subclass overrides are bypassed

**[GenericOnDemandTextureLoader.cs:320](spine-unity/Modules/com.esotericsoftware.spine.on-demand-loading/Runtime/GenericOnDemandTextureLoader.cs#L320)**

The new plural overload calls the private indexed `AssignPlaceholderTexture` overload, including for index zero.

Thus a subclass of `GenericOnDemandTextureLoader` overriding the existing public `AssignPlaceholderTexture(Material, out Texture)` no longer controls main-texture replacement during the project-wide build scan.

**Fix:** route index zero through the existing virtual method. Astra explicitly preserved this dispatch. This matters for the intended non-breaking update.

**Resolution:** the plural overload dispatches main-texture replacement through the existing public virtual overload.

### 6. [done] Setup and validation disagree about ignored properties

**[GenericOnDemandTextureLoaderInspector.cs:191](spine-unity/Modules/com.esotericsoftware.spine.on-demand-loading/Editor/GenericOnDemandTextureLoaderInspector.cs#L191)**

Setup ignores duplicate properties and `_MainTex`, but property-ID generation and validation still process those slots.

For example, listing `_BumpMap` twice leaves an empty second mapping that validation reports as missing. Hitting **Regenerate**, as instructed, cannot fix it.

Additionally, `_MainTex` is not necessarily the main property when a shader declares `[MainTexture] _BaseMap`. `Add All Textures` can add that legitimate secondary `_MainTex` property, only for setup to ignore it.

**Fix:** apply consistent filtering throughout, and identify the actual main property per material rather than globally excluding `_MainTex`.

**Resolution:** setup, property-ID generation, and validation use consistent filtering and the per-shader main-texture resolver. The resolver's older-Unity version guard is now corrected as described in follow-up #1 below.

### 7. [done: build validation] Addressable registration errors do not invalidate the loader

**[AddressablesTextureLoaderInspector.cs:60](spine-unity/Modules/com.esotericsoftware.spine.addressables/Editor/AddressablesTextureLoaderInspector.cs#L60)**

The registration check logs an error but still constructs the reference and returns success. Generic setup also ignores the returned boolean.

`ValidateSetup` only checks `EditorTexture`, which can resolve even when the texture is not Addressable. Therefore the build still substitutes placeholders, but runtime loading fails. Removing Addressable registration after setup also escapes build validation.

**Fix:** incorporate registration validity into build validation and honor setup failures. Astra handled initial setup failure more defensively.

**Resolution:** Addressables registers `validateTargetReference`, so build validation detects missing Addressable registration, including registration via parent folders. Invalid loaders use the recovery/fallback handling in #8. Setup still logs registration errors rather than aborting immediately; build validation is the safety mechanism.

### 8. [done] Skipping an invalid loader does not guarantee full-resolution fallback

**[SpineBuildProcessor.cs:228](spine-unity/Assets/Spine/Editor/spine-unity/Editor/Utility/SpineBuildProcessor.cs#L228)**

The fallback assumes materials already contain their target textures.

After testing with **Assign Placeholders**, or an interrupted build, an invalid loader can still have placeholders assigned. Skipping it neither restores those materials nor disables its runtime loading path.

The warning’s promise—full-resolution textures included instead—is therefore not guaranteed.

**Fix:** recover existing placeholders before permitting fallback; if recovery is impossible, fail the build rather than claim it is safe. Astra avoided this particular situation by rejecting invalid builds.

**Resolution:** skipped loaders are recovered before build placeholders are assigned. Recovery covers atlas/blend materials and scans material assets/subassets once, regardless of the normal material-scan setting. Known placeholders that cannot be restored cause a `BuildFailedException`. Successful recoveries are saved even when another material causes the build to abort. Editor placeholder detection also covers shader properties removed or renamed in the loader's configuration.

## Additional completed corrections

- **Shared-placeholder regeneration:** explicit regeneration now refreshes image bytes and importer settings in place, once per placeholder path, without copying or replacing `.meta` files. Other loaders keep their references; shared settings follow the loader most recently regenerated. Actual GUID/importer behavior still requires Unity integration testing.
- **Exception cleanup:** `PreprocessOnDemandTextureLoaders` and skipped-loader recovery balance `StartAssetEditing`/`StopAssetEditing` using `finally`.
- **Play-mode compatibility cleanup:** the module inspector now uses `playModeStateChanged` and `PlayModeStateChange` unconditionally, removing its pre-2017.2 callback branches.
- **Texture-property enumeration:** the attempted all-version `ShaderUtil` simplification was reverted to avoid deprecated API usage on modern Unity. Unity 2018.1+ uses `Material.GetTexturePropertyNames()` filtered by `material.HasProperty`, excluding saved entries not declared by the current shader. `ShaderUtil` is retained only for the older core compatibility branch. Neither path deletes material data.

## Follow-up status

These numbers refer to the remaining-issues discussion, not the original findings above. Follow-up #1 is implemented; #2 remains open.

### 1. [done] `[MainTexture]` resolution on Unity 2019.3–2020.x

**[GenericOnDemandTextureLoader.cs:169](spine-unity/Modules/com.esotericsoftware.spine.on-demand-loading/Runtime/GenericOnDemandTextureLoader.cs#L169)**

The main-texture resolver previously used shader-property flags only under `UNITY_2021_1_OR_NEWER`. Earlier versions fell back to `_MainTex`, even though Unity 2019.3 already supports `[MainTexture]` and all APIs used by the flagged-property path.

Before the fix, for a shader declaring `[MainTexture] _BaseMap` and a separate `_MainTex`, Unity's `material.mainTexture` selected `_BaseMap`, while the loader selected `_MainTex`. Without `_MainTex`, the loader returned -1 even though the shader had a valid main texture. This could reject valid mappings or prevent correct main-texture replacement/loading. The editor-only `ShaderUtil` enumeration cleanup did not fix this separate runtime resolver.

**Resolution:** the guard is now `UNITY_2019_3_OR_NEWER`, retaining the `_MainTex` fallback below 2019.3. The existing per-shader ID cache and public API are unchanged.

Verified against Unity 2019.3 documentation: [GetPropertyCount](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Shader.GetPropertyCount.html), [GetPropertyNameId](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Shader.GetPropertyNameId.html), [GetPropertyFlags](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Shader.GetPropertyFlags.html), [MainTexture flag](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Rendering.ShaderPropertyFlags.MainTexture.html), and [Material.mainTexture](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Material-mainTexture.html).

Regression checks should cover a normal `_MainTex` shader, `[MainTexture] _BaseMap` with and without another `_MainTex` property, and a shader without any main texture, on the older and newer branches.

### 2. Async completion can overwrite a subsequent material change

**[AddressablesTextureLoader.cs:91](spine-unity/Modules/com.esotericsoftware.spine.addressables/Runtime/AddressablesTextureLoader.cs#L91)**

Example: a normal-map placeholder starts loading; user code changes or clears `_BumpMap` while the request is pending; the completion callback unconditionally assigns the loaded texture to `_BumpMap`, overwriting that change.

The callback already checks object/request validity and request identity, but does not check whether the material property still contains the expected placeholder. This is a pre-existing main-texture limitation that also applies to secondary textures now.

**Minimal correctness fix:** only replace the material property if it still contains the expected placeholder, while preserving request lifetime and callback behavior. Test user changes/clears during a pending request, ordinary successful replacement, and cancelled/replaced requests.

**Separate optional improvement:** completion updates only the initiating material. Other consumers of the same request receive the loaded texture when they next poll. Updating all still-eligible tracked consumers immediately would remove that delay, but is not necessary to fix the overwrite race and should not require adopting Astra's broader routing design.

## Deferred by request

- Original finding #4: atlas materials sharing a diffuse texture but using different secondary mappings.
- `SkeletonGraphic` secondary-texture support: leave unchanged for now; it is not part of the active follow-ups.

## What is better than Astra

The staged approach has worthwhile improvements:

- Much simpler property routing and runtime tracking.
- Automatic list updates and **Add All Textures**.
- Correctly importing normal-map placeholders temporarily as `Default` before reading pixels.
- Preserving non-null user texture changes during editor restoration.

Retain these choices and the implemented safety fixes—not Astra’s broader renamed-property machinery.

## Review verification

- Initial review: static inspection plus four in-memory control-flow reproductions.
- Follow-up fixes: 13 isolated C# checks passed (seven recovery/sharing checks and six regeneration checks), using extracted implementation methods and stubbed Unity APIs.
- Compatibility checks: the core loader, generic runtime, and build processor passed C# 4 compilation checks with Unity 2017.1 symbols against API stubs. This is not a full package compilation in Unity 2017.1, and the module inspector now intentionally uses the newer callbacks.
- Simplification checks: four isolated callback assertions passed with stubbed APIs. Six texture-property assertions exercised the `ShaderUtil` path; the modern filtered-material branch was subsequently restored to avoid deprecated API usage.
- Main-texture guard fix: 45 isolated resolver assertions passed across Unity 2018.3, 2019.2, 2019.3, 2020.3, and 2021.1+ symbol configurations, compiled as C# 4 with stubbed APIs. Checks cover null inputs, ordinary/missing main textures, flagged properties with and without `_MainTex` in either order, and per-shader caching. Older configurations omit the newer shader APIs from their stubs.
- Scoped whitespace checks passed.
- **Still required:** real Unity compilation and editor/build tests, particularly copied-material recovery/build rejection, in-place placeholder GUID/importer preservation, and the main-texture/async cases above. No actual Unity integration tests have been run.
