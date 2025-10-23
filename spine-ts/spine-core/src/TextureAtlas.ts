/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import type { AssetManagerBase } from "./AssetManagerBase.js";
import { type Texture, TextureFilter, TextureRegion, TextureWrap } from "./Texture.js";
import { type Disposable, type StringMap, Utils } from "./Utils.js";

export class TextureAtlas implements Disposable {
	pages = [] as TextureAtlasPage[];
	regions = [] as TextureAtlasRegion[];

	constructor (atlasText: string) {
		const reader = new TextureAtlasReader(atlasText);
		const entry = new Array<string>(4);

		const pageFields: StringMap<(page: TextureAtlasPage) => void> = {};
		pageFields.size = (page: TextureAtlasPage) => {
			page.width = parseInt(entry[1]);
			page.height = parseInt(entry[2]);
		};
		pageFields.format = () => {
			// page.format = Format[tuple[0]]; we don't need format in WebGL
		};
		pageFields.filter = (page: TextureAtlasPage) => {
			page.minFilter = Utils.enumValue(TextureFilter, entry[1]);
			page.magFilter = Utils.enumValue(TextureFilter, entry[2]);
		};
		pageFields.repeat = (page: TextureAtlasPage) => {
			if (entry[1].indexOf('x') !== -1) page.uWrap = TextureWrap.Repeat;
			if (entry[1].indexOf('y') !== -1) page.vWrap = TextureWrap.Repeat;
		};
		pageFields.pma = (page: TextureAtlasPage) => {
			page.pma = entry[1] === "true";
		};

		var regionFields: StringMap<(region: TextureAtlasRegion) => void> = {};
		regionFields.xy = (region: TextureAtlasRegion) => { // Deprecated, use bounds.
			region.x = parseInt(entry[1]);
			region.y = parseInt(entry[2]);
		};
		regionFields.size = (region: TextureAtlasRegion) => { // Deprecated, use bounds.
			region.width = parseInt(entry[1]);
			region.height = parseInt(entry[2]);
		};
		regionFields.bounds = (region: TextureAtlasRegion) => {
			region.x = parseInt(entry[1]);
			region.y = parseInt(entry[2]);
			region.width = parseInt(entry[3]);
			region.height = parseInt(entry[4]);
		};
		regionFields.offset = (region: TextureAtlasRegion) => { // Deprecated, use offsets.
			region.offsetX = parseInt(entry[1]);
			region.offsetY = parseInt(entry[2]);
		};
		regionFields.orig = (region: TextureAtlasRegion) => { // Deprecated, use offsets.
			region.originalWidth = parseInt(entry[1]);
			region.originalHeight = parseInt(entry[2]);
		};
		regionFields.offsets = (region: TextureAtlasRegion) => {
			region.offsetX = parseInt(entry[1]);
			region.offsetY = parseInt(entry[2]);
			region.originalWidth = parseInt(entry[3]);
			region.originalHeight = parseInt(entry[4]);
		};
		regionFields.rotate = (region: TextureAtlasRegion) => {
			const value = entry[1];
			if (value === "true")
				region.degrees = 90;
			else if (value !== "false")
				region.degrees = parseInt(value);
		};
		regionFields.index = (region: TextureAtlasRegion) => {
			region.index = parseInt(entry[1]);
		};

		let line = reader.readLine();
		// Ignore empty lines before first entry.
		while (line && line.trim().length === 0)
			line = reader.readLine();
		// Header entries.
		while (true) {
			if (!line || line.trim().length === 0) break;
			if (reader.readEntry(entry, line) === 0) break; // Silently ignore all header fields.
			line = reader.readLine();
		}

		// Page and region entries.
		let page: TextureAtlasPage | null = null;
		let names: string[] | null = null;
		let values: number[][] | null = null;
		while (true) {
			if (line === null) break;
			if (line.trim().length === 0) {
				page = null;
				line = reader.readLine();
			} else if (!page) {
				page = new TextureAtlasPage(line.trim());
				while (true) {
					if (reader.readEntry(entry, line = reader.readLine()) === 0) break;
					const field = pageFields[entry[0]];
					if (field) field(page);
				}
				this.pages.push(page);
			} else {
				const region = new TextureAtlasRegion(page, line);

				while (true) {
					const count = reader.readEntry(entry, line = reader.readLine());
					if (count === 0) break;
					const field = regionFields[entry[0]];
					if (field)
						field(region);
					else {
						if (!names) names = [];
						if (!values) values = [];
						names.push(entry[0]);
						const entryValues: number[] = [];
						for (let i = 0; i < count; i++)
							entryValues.push(parseInt(entry[i + 1]));
						values.push(entryValues);
					}
				}
				if (region.originalWidth === 0 && region.originalHeight === 0) {
					region.originalWidth = region.width;
					region.originalHeight = region.height;
				}
				if (names && names.length > 0 && values && values.length > 0) {
					region.names = names;
					region.values = values;
					names = null;
					values = null;
				}
				region.u = region.x / page.width;
				region.v = region.y / page.height;
				if (region.degrees === 90) {
					region.u2 = (region.x + region.height) / page.width;
					region.v2 = (region.y + region.width) / page.height;
				} else {
					region.u2 = (region.x + region.width) / page.width;
					region.v2 = (region.y + region.height) / page.height;
				}
				this.regions.push(region);
			}
		}
	}

	findRegion (name: string): TextureAtlasRegion | null {
		for (let i = 0; i < this.regions.length; i++) {
			if (this.regions[i].name === name) {
				return this.regions[i];
			}
		}
		return null;
	}

	setTextures (assetManager: AssetManagerBase, pathPrefix: string = "") {
		for (const page of this.pages)
			page.setTexture(assetManager.get(pathPrefix + page.name) as Texture);
	}

	dispose () {
		for (let i = 0; i < this.pages.length; i++) {
			this.pages[i].texture?.dispose();
		}
	}
}

class TextureAtlasReader {
	lines: Array<string>;
	index: number = 0;

	constructor (text: string) {
		this.lines = text.split(/\r\n|\r|\n/);
	}

	readLine (): string | null {
		if (this.index >= this.lines.length)
			return null;
		return this.lines[this.index++];
	}

	readEntry (entry: string[], line: string | null): number {
		if (!line) return 0;
		line = line.trim();
		if (line.length === 0) return 0;

		const colon = line.indexOf(':');
		if (colon === -1) return 0;
		entry[0] = line.substr(0, colon).trim();
		for (let i = 1, lastMatch = colon + 1; ; i++) {
			const comma = line.indexOf(',', lastMatch);
			if (comma === -1) {
				entry[i] = line.substr(lastMatch).trim();
				return i;
			}
			entry[i] = line.substr(lastMatch, comma - lastMatch).trim();
			lastMatch = comma + 1;
			if (i === 4) return 4;
		}
	}
}

export class TextureAtlasPage {
	name: string;
	minFilter: TextureFilter = TextureFilter.Nearest;
	magFilter: TextureFilter = TextureFilter.Nearest;
	uWrap: TextureWrap = TextureWrap.ClampToEdge;
	vWrap: TextureWrap = TextureWrap.ClampToEdge;
	texture: Texture | null = null;
	width: number = 0;
	height: number = 0;
	pma: boolean = false;
	regions = [] as TextureAtlasRegion[];

	constructor (name: string) {
		this.name = name;
	}

	setTexture (texture: Texture) {
		this.texture = texture;
		texture.setFilters(this.minFilter, this.magFilter);
		texture.setWraps(this.uWrap, this.vWrap);
		for (const region of this.regions)
			region.texture = texture;
	}
}

export class TextureAtlasRegion extends TextureRegion {
	page: TextureAtlasPage;
	name: string;
	x: number = 0;
	y: number = 0;
	offsetX: number = 0;
	offsetY: number = 0;
	originalWidth: number = 0;
	originalHeight: number = 0;
	index: number = 0;
	degrees: number = 0;
	names: string[] | null = null;
	values: number[][] | null = null;

	constructor (page: TextureAtlasPage, name: string) {
		super();
		this.page = page;
		this.name = name;
		page.regions.push(this);
	}
}
