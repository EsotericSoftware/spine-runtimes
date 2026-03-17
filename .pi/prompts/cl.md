---
description: Audit changelog entries before release
---
Audit changelog entries for all commits since the last release.

## Process

1. **Find the last release tag:**
   ```bash
   git tag --sort=-version:refname | head -1
   ```

2. **List all commits since that tag:**
   ```bash
   git log <tag>..HEAD --oneline
   ```

3. **Read the current top release section in `CHANGELOG.md`:**
   - Treat this section as the release target.
   - Identify the runtime subsections that exist (for example `C`, `C++`, `C#`, `Java`, `TypeScript`, `Unity`, `UE`, etc.).

4. **For each commit, check:**
   - Skip: changelog-only updates, doc-only changes, or release housekeeping.
   - Determine affected runtime(s) and folders (`git show <hash> --stat`).
   - Verify a changelog entry exists in `CHANGELOG.md` under the correct runtime subsection.
   - Verify breaking API/behavior changes are listed under a breaking changes subsection for that runtime when applicable.

5. **Cross-runtime coverage rule:**
   - If a shared or cross-cutting change impacts multiple runtimes, ensure each impacted runtime subsection has an entry.

6. **Report:**
   - List commits with missing entries.
   - List entries that are misplaced (wrong runtime subsection or wrong change type).
   - Add any missing entries directly.

## Changelog structure reference

- Top-level section is the release version (for example `# 4.2`).
- Runtime subsections are grouped under that version.
- Runtime subsections may contain grouped headings such as `Additions`, `Breaking changes`, `Fixes`, etc.
- Keep wording concise and runtime-specific.
