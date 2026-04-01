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

import type { AttachmentLoader } from "./attachments/AttachmentLoader.js";
import { BoundingBoxAttachment } from "./attachments/BoundingBoxAttachment.js";
import { ClippingAttachment } from "./attachments/ClippingAttachment.js";
import { MeshAttachment } from "./attachments/MeshAttachment.js";
import { PathAttachment } from "./attachments/PathAttachment.js";
import { PointAttachment } from "./attachments/PointAttachment.js";
import { RegionAttachment } from "./attachments/RegionAttachment.js";
import type { Sequence } from "./attachments/Sequence.js"
import type { Skin } from "./Skin.js";
import type { TextureAtlas } from "./TextureAtlas.js";

/** An {@link AttachmentLoader} that configures attachments using texture regions from an {@link TextureAtlas}.
 *
 * See [Loading skeleton data](http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data) in the
 * Spine Runtimes Guide. */
export class AtlasAttachmentLoader implements AttachmentLoader {
	atlas: TextureAtlas;
	allowMissingRegions: boolean;

	constructor (atlas: TextureAtlas, allowMissingRegions = false) {
		this.atlas = atlas;
		this.allowMissingRegions = allowMissingRegions;
	}

	/** Sets each {@link Sequence.regions} by calling {@link findRegion} for each texture region using
	 * {@link Sequence.getPath}. */
	protected findRegions (name: string, basePath: string, sequence: Sequence) {
		const regions = sequence.regions;
		for (let i = 0, n = regions.length; i < n; i++)
			regions[i] = this.findRegion(name, sequence.getPath(basePath, i));
	}

	/** Looks for the region with the specified path. If not found and {@link allowMissingRegions} is false, an error is
	 * raised. */
	protected findRegion (name: string, path: string) {
		const region = this.atlas.findRegion(path);
		if (!region && !this.allowMissingRegions)
			throw new Error(`Region not found in atlas: ${path} (attachment: ${name})`);
		return region;
	}

	newRegionAttachment (skin: Skin, name: string, path: string, sequence: Sequence): RegionAttachment {
		this.findRegions(name, path, sequence);
		return new RegionAttachment(name, sequence);
	}

	newMeshAttachment (skin: Skin, name: string, path: string, sequence: Sequence): MeshAttachment {
		this.findRegions(name, path, sequence);
		return new MeshAttachment(name, sequence);
	}

	newBoundingBoxAttachment (skin: Skin, name: string): BoundingBoxAttachment {
		return new BoundingBoxAttachment(name);
	}

	newPathAttachment (skin: Skin, name: string): PathAttachment {
		return new PathAttachment(name);
	}

	newPointAttachment (skin: Skin, name: string): PointAttachment {
		return new PointAttachment(name);
	}

	newClippingAttachment (skin: Skin, name: string): ClippingAttachment {
		return new ClippingAttachment(name);
	}
}
