---
name: forum
description: Fetch Spine forum discussion threads. Use when the user shares a forum URL (esotericsoftware.com/forum/d/...) or asks about a forum thread.
---

# Spine Forum

Fetch discussion threads from the Spine forum (Flarum) via its public REST API. No authentication needed.

## Fetch a thread

```bash
{baseDir}/fetch.js <url_or_id>
```

Accepts a full forum URL or just the discussion ID:

```bash
{baseDir}/fetch.js https://esotericsoftware.com/forum/d/29888-spine-ue-world-movement-not-affecting-physics/5
{baseDir}/fetch.js 29888
```

Output includes all posts as plain text with code blocks preserved. Image and video URLs are listed after each post as `[IMAGE]` / `[VIDEO]` lines. Use the `read` tool on image URLs to view them.

### Options

- `--html` - Print raw HTML instead of converting to text
- `--json` - Print the raw JSON API response

## Extracting the discussion ID from a URL

Forum URLs: `https://esotericsoftware.com/forum/d/{ID}-{slug}/{postIndex}`

The ID is the number after `/d/`. The slug and post index are irrelevant for API access.
