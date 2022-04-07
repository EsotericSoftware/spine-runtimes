/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { AttachmentLoader } from "./attachments/AttachmentLoader";
import { BoundingBoxAttachment } from "./attachments/BoundingBoxAttachment";
import { ClippingAttachment } from "./attachments/ClippingAttachment";
import { MeshAttachment } from "./attachments/MeshAttachment";
import { PathAttachment } from "./attachments/PathAttachment";
import { PointAttachment } from "./attachments/PointAttachment";
import { RegionAttachment } from "./attachments/RegionAttachment";
import { Skin } from "./Skin";
import { TextureAtlas } from "./TextureAtlas";
import { Sequence } from "./attachments/Sequence"

/** An {@link AttachmentLoader} that configures attachments using texture regions from an {@link TextureAtlas}.
 *
 * See [Loading skeleton data](http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data) in the
 * Spine Runtimes Guide. */
export class AtlasAttachmentLoader implements AttachmentLoader {
	atlas: TextureAtlas;

	constructor (atlas: TextureAtlas) {
		this.atlas = atlas;
	}

	loadSequence (name: string, basePath: string, sequence: Sequence) {
		let regions = sequence.regions;
		for (let i = 0, n = regions.length; i < n; i++) {
			let path = sequence.getPath(basePath, i);
			let region = this.atlas.findRegion(path);
			if (region == null) throw new Error("Region not found in atlas: " + path + " (sequence: " + name + ")");
			regions[i] = region;
			regions[i].renderObject = regions[i];
		}
	}

	newRegionAttachment (skin: Skin, name: string, path: string, sequence: Sequence): RegionAttachment {
		let attachment = new RegionAttachment(name, path);
		if (sequence != null) {
			this.loadSequence(name, path, sequence);
		} else {
			let region = this.atlas.findRegion(path);
			if (!region) throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			region.renderObject = region;
			attachment.region = region;
		}
		return attachment;
	}

	newMeshAttachment (skin: Skin, name: string, path: string, sequence: Sequence): MeshAttachment {
		let attachment = new MeshAttachment(name, path);
		if (sequence != null) {
			this.loadSequence(name, path, sequence);
		} else {
			let region = this.atlas.findRegion(path);
			if (!region) throw new Error("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			region.renderObject = region;
			attachment.region = region;
		}
		return attachment;
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
