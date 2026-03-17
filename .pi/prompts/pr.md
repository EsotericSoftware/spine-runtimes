---
description: Review PRs from URLs with structured issue and code analysis
---
You are given one or more GitHub PR URLs: $@

For each PR URL, do the following in order:
1. Read the PR page in full. Include description, all comments, all commits, and all changed files.
2. Identify any linked issues referenced in the PR body, comments, commit messages, or cross links. Read each issue in full, including all comments.
3. Analyze the PR diff (`gh pr diff <url>`). Read all relevant code files in full with no truncation from the current main branch and compare against the diff. Include related code paths that are not in the diff but are required to validate behavior.
4. Check for a changelog entry in `CHANGELOG.md` in the current release section and the affected runtime subsection(s). Report whether an entry exists. If missing, state that a changelog entry is required before merge and that you will add it if the user decides to merge. Verify:
   - Entry is placed under the correct runtime subsection(s).
   - Breaking changes are listed under a breaking changes subsection when applicable.
5. Check whether docs/examples need updates. This is usually required when APIs or behavior changed. Inspect at least:
   - `README.md`
   - Runtime-specific docs/README files under affected runtime directories
   - Relevant example projects under `examples/`
6. Provide a structured review with these sections:
   - Good: solid choices or improvements
   - Bad: concrete issues, regressions, missing tests, or risks
   - Ugly: subtle or high impact problems
7. Add Questions or Assumptions if anything is unclear.
8. Add Change summary and Tests.

Output format per PR:
PR: <url>
Changelog:
- ...
Good:
- ...
Bad:
- ...
Ugly:
- ...
Questions or Assumptions:
- ...
Change summary:
- ...
Tests:
- ...

If no issues are found, say so under Bad and Ugly.
