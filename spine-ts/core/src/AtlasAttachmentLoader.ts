/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class AtlasAttachmentLoader implements AttachmentLoader {
		atlas: TextureAtlas;

		constructor (atlas: TextureAtlas) {
			this.atlas = atlas;
		}

		/** @return May be null to not load an attachment. */
		newRegionAttachment (skin: Skin, name: string, path: string): RegionAttachment {
			let region = this.atlas.findRegion(path);
			if (region == null) throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			region.renderObject = region;
			let attachment = new RegionAttachment(name);
			attachment.setRegion(region);
			return attachment;
		}

		/** @return May be null to not load an attachment. */
		newMeshAttachment (skin: Skin, name: string, path: string) : MeshAttachment {
			let region = this.atlas.findRegion(path);
			if (region == null) throw new Error("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			region.renderObject = region;
			let attachment = new MeshAttachment(name);
			attachment.region = region;
			return attachment;
		}

		/** @return May be null to not load an attachment. */
		newBoundingBoxAttachment (skin: Skin, name: string) : BoundingBoxAttachment {
			return new BoundingBoxAttachment(name);
		}

		/** @return May be null to not load an attachment */
		newPathAttachment (skin: Skin, name: string): PathAttachment {
			return new PathAttachment(name);
		}

		newPointAttachment(skin: Skin, name: string): PointAttachment {
			return new PointAttachment(name);
		}

		newClippingAttachment(skin: Skin, name: string): ClippingAttachment {
			return new ClippingAttachment(name);
		}
	}
}
