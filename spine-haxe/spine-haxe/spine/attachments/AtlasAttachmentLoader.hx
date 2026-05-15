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

package spine.attachments;

import spine.atlas.TextureAtlas;
import spine.atlas.TextureAtlasRegion;
import spine.Skin;
import spine.Sequence;

class AtlasAttachmentLoader implements AttachmentLoader {
	private var atlas:TextureAtlas;

	public var allowMissingRegions:Bool;

	public function new(atlas:TextureAtlas, allowMissingRegions = false) {
		if (atlas == null) {
			throw new SpineException("atlas cannot be null.");
		}
		this.atlas = atlas;
		this.allowMissingRegions = allowMissingRegions;
	}

	private function findRegions(name:String, basePath:String, sequence:Sequence):Void {
		var regions = sequence.regions;
		for (i in 0...regions.length)
			regions[i] = findRegion(name, sequence.getPath(basePath, i));
	}

	private function findRegion(name:String, path:String):TextureAtlasRegion {
		var region = atlas.findRegion(path);
		if (region == null && !allowMissingRegions)
			throw new SpineException("Region not found in atlas: " + path + " (attachment: " + name + ")");
		return region;
	}

	public function newRegionAttachment(skin:Skin, placeholder:String, name:String, path:String, sequence:Sequence):RegionAttachment {
		findRegions(name, path, sequence);
		return new RegionAttachment(name, sequence);
	}

	public function newMeshAttachment(skin:Skin, placeholder:String, name:String, path:String, sequence:Sequence):MeshAttachment {
		findRegions(name, path, sequence);
		return new MeshAttachment(name, sequence);
	}

	public function newBoundingBoxAttachment(skin:Skin, placeholder:String, name:String):BoundingBoxAttachment {
		return new BoundingBoxAttachment(name);
	}

	public function newPathAttachment(skin:Skin, placeholder:String, name:String):PathAttachment {
		return new PathAttachment(name);
	}

	public function newPointAttachment(skin:Skin, placeholder:String, name:String):PointAttachment {
		return new PointAttachment(name);
	}

	public function newClippingAttachment(skin:Skin, placeholder:String, name:String):ClippingAttachment {
		return new ClippingAttachment(name);
	}
}
