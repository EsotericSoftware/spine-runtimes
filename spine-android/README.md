# spine-android

The spine-android runtime provides Android integration for Spine using `spine-libgdx`.

# Releasing

`spine-android` is released with `spine-libgdx`. Its version comes from `spine-libgdx/gradle.properties`.

1. Set the release version in `spine-libgdx/gradle.properties`:

```
version=4.3.2
```

Do not use `-SNAPSHOT` for a release.

2. Commit and push the release version:

```
git add spine-libgdx/gradle.properties
git commit -m "[libgdx][android] Release 4.3.2"
git push origin 4.3
```

3. Tag that commit and push the tag:

```
git tag spine-libgdx-4.3.2
git push origin spine-libgdx-4.3.2
```

The tag triggers the GitHub Actions release workflow. It publishes `spine-libgdx` first, then `spine-android`.

4. Check the workflow result and Maven Central.

If JReleaser fails before upload/validation, the artifacts were not published. If JReleaser uploads and validates successfully but later times out waiting for Central Portal's publishing step, the GitHub job may fail even though publishing still succeeds. Check Central Portal or Maven Central before retrying.

5. Bump to the next snapshot and push:

```
# spine-libgdx/gradle.properties
version=4.3.3-SNAPSHOT

git add spine-libgdx/gradle.properties
git commit -m "[libgdx][android] Begin 4.3.3-SNAPSHOT"
git push origin 4.3
```
