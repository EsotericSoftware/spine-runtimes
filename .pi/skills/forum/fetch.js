#!/usr/bin/env node

/**
 * Fetch Spine forum discussion threads via the Flarum REST API.
 *
 * Usage:
 *   fetch.js <url_or_id>          Print all posts as plain text with media URLs
 *   fetch.js <url_or_id> --html   Print raw HTML instead of stripping tags
 *   fetch.js <url_or_id> --json   Print raw JSON API response
 *
 * Examples:
 *   fetch.js https://esotericsoftware.com/forum/d/29888-spine-ue-world-movement-not-affecting-physics/5
 *   fetch.js 29888
 *   fetch.js 29888 --html
 */

const https = require("https");
const http = require("http");

const BASE = "https://esotericsoftware.com/forum/api";

function extractId(arg) {
	const m = arg.match(/\/d\/(\d+)/);
	if (m) return m[1];
	if (/^\d+$/.test(arg)) return arg;
	console.error(`Error: cannot extract discussion ID from: ${arg}`);
	process.exit(1);
}

function fetch(url) {
	return new Promise((resolve, reject) => {
		const lib = url.startsWith("https") ? https : http;
		lib.get(url, { headers: { Accept: "application/json" } }, (res) => {
			if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
				return fetch(res.headers.location).then(resolve, reject);
			}
			const chunks = [];
			res.on("data", (c) => chunks.push(c));
			res.on("end", () => {
				const body = Buffer.concat(chunks).toString();
				try { resolve(JSON.parse(body)); }
				catch (e) { reject(new Error(`JSON parse error: ${e.message}\n${body.slice(0, 200)}`)); }
			});
			res.on("error", reject);
		}).on("error", reject);
	});
}

function decodeEntities(s) {
	return s
		.replace(/&#(\d+);/g, (_, n) => String.fromCharCode(Number(n)))
		.replace(/&#x([0-9a-fA-F]+);/g, (_, n) => String.fromCharCode(parseInt(n, 16)))
		.replace(/&amp;/g, "&")
		.replace(/&lt;/g, "<")
		.replace(/&gt;/g, ">")
		.replace(/&quot;/g, '"')
		.replace(/&apos;/g, "'")
		.replace(/&nbsp;/g, " ");
}

function htmlToText(h) {
	// Code blocks
	h = h.replace(/<pre><code[^>]*>([\s\S]*?)<\/code><\/pre>/gi, (_, code) => {
		return "\n```\n" + decodeEntities(code.replace(/<[^>]+>/g, "")) + "\n```\n";
	});
	// Inline code
	h = h.replace(/<code>(.*?)<\/code>/gi, (_, code) => "`" + decodeEntities(code) + "`");
	// Strip script tags entirely
	h = h.replace(/<script[\s\S]*?<\/script>/gi, "");
	// Line breaks
	h = h.replace(/<br\s*\/?>/gi, "\n");
	// Block boundaries
	h = h.replace(/<\/?(p|div|li|ol|ul|h[1-6]|blockquote)[^>]*>/gi, "\n");
	// Strip remaining tags
	h = h.replace(/<[^>]+>/g, "");
	// Decode entities
	h = decodeEntities(h);
	// Collapse blank lines
	h = h.replace(/\n{3,}/g, "\n\n");
	return h.trim();
}

function extractMedia(h) {
	const imgs = [...h.matchAll(/<img[^>]+src="([^"]+)"/g)].map((m) => m[1]);
	const vids = [...h.matchAll(/<video[^>]+src="([^"]+)"/g)].map((m) => m[1]);
	return { imgs, vids };
}

async function main() {
	const args = process.argv.slice(2);
	if (args.length === 0 || args[0] === "-h" || args[0] === "--help") {
		console.log(`Usage: fetch.js <url_or_id> [--html|--json]`);
		process.exit(0);
	}

	const discId = extractId(args[0]);
	const mode = args.includes("--html") ? "html" : args.includes("--json") ? "json" : "text";

	const url = `${BASE}/discussions/${discId}?include=posts,posts.user`;
	const data = await fetch(url);

	if (mode === "json") {
		console.log(JSON.stringify(data, null, 2));
		return;
	}

	const included = data.included || [];
	const users = {};
	for (const item of included) {
		if (item.type === "users") {
			users[item.id] = item.attributes.displayName || item.attributes.username || "?";
		}
	}

	const posts = included
		.filter((i) => i.type === "posts" && i.attributes.contentType === "comment")
		.sort((a, b) => a.attributes.createdAt.localeCompare(b.attributes.createdAt));

	const title = data.data.attributes.title;
	console.log(`# ${title}\n`);

	for (const p of posts) {
		const uid = p.relationships.user.data.id;
		const author = users[uid] || "?";
		const created = p.attributes.createdAt;
		const rawHtml = p.attributes.contentHtml || "";

		console.log(`## ${author} (${created})\n`);

		if (mode === "html") {
			console.log(rawHtml);
		} else {
			console.log(htmlToText(rawHtml));
		}

		const { imgs, vids } = extractMedia(rawHtml);
		for (const u of imgs) console.log(`\n[IMAGE] ${u}`);
		for (const u of vids) console.log(`\n[VIDEO] ${u}`);

		console.log("\n---\n");
	}
}

main().catch((e) => {
	console.error(e.message);
	process.exit(1);
});
