/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.attachments;

import spine.atlas.TextureAtlas;
import spine.Skin;

class AtlasAttachmentLoader implements AttachmentLoader {
	private var atlas:TextureAtlas;

	public function new(atlas:TextureAtlas) {
		if (atlas == null) {
			throw new SpineException("atlas cannot be null.");
		}
		this.atlas = atlas;
	}

	private function loadSequence(name:String, basePath:String, sequence:Sequence) {
		var regions = sequence.regions;
		for (i in 0...regions.length) {
			var path = sequence.getPath(basePath, i);
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new SpineException("Region not found in atlas: " + path + " (sequence: " + name + ")");
			regions[i] = region;
		}
	}

	public function newRegionAttachment(skin:Skin, name:String, path:String, sequence:Sequence):RegionAttachment {
		var attachment = new RegionAttachment(name, path);
		if (sequence != null) {
			this.loadSequence(name, path, sequence);
		} else {
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new SpineException("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			attachment.region = region;
		}
		return attachment;
	}

	public function newMeshAttachment(skin:Skin, name:String, path:String, sequence:Sequence):MeshAttachment {
		var attachment = new MeshAttachment(name, path);
		if (sequence != null) {
			this.loadSequence(name, path, sequence);
		} else {
			var region = atlas.findRegion(path);
			if (region == null)
				throw new SpineException("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			attachment.region = region;
		}
		return attachment;
	}

	public function newBoundingBoxAttachment(skin:Skin, name:String):BoundingBoxAttachment {
		return new BoundingBoxAttachment(name);
	}

	public function newPathAttachment(skin:Skin, name:String):PathAttachment {
		return new PathAttachment(name);
	}

	public function newPointAttachment(skin:Skin, name:String):PointAttachment {
		return new PointAttachment(name);
	}

	public function newClippingAttachment(skin:Skin, name:String):ClippingAttachment {
		return new ClippingAttachment(name);
	}
}
