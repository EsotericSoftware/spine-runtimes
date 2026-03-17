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

package spine;

import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;

/** Holds texture regions, UVs, and vertex offsets for rendering a region or mesh attachment. Regions must
 * be populated and update() called before use. */
class Sequence {
	private static var _nextID = 0;

	/** Returns a unique ID for this attachment. */
	public var id = _nextID++;

	public var regions:Array<TextureRegion>;
	public var pathSuffix:Bool;
	public var uvs:Array<Array<Float>>;
	public var offsets:Array<Array<Float>>;
	public var start = 0;
	public var digits = 0;

	/** The index of the region to show for the setup pose. */
	public var setupIndex = 0;

	public function new(count:Int, pathSuffix:Bool) {
		this.regions = new Array<TextureRegion>();
		this.regions.resize(count);
		this.pathSuffix = pathSuffix;
	}

	/** Copy constructor. */
	public function copy():Sequence {
		var regionCount = this.regions.length;
		var copy = new Sequence(regionCount, this.pathSuffix);
		for (i in 0...regionCount)
			copy.regions[i] = this.regions[i];
		copy.start = this.start;
		copy.digits = this.digits;
		copy.setupIndex = this.setupIndex;

		if (this.uvs != null) {
			var length = this.uvs[0].length;
			copy.uvs = new Array<Array<Float>>();
			copy.uvs.resize(regionCount);
			for (i in 0...regionCount) {
				copy.uvs[i] = new Array<Float>();
				copy.uvs[i].resize(length);
				for (j in 0...length)
					copy.uvs[i][j] = this.uvs[i][j];
			}
		}
		if (this.offsets != null) {
			copy.offsets = new Array<Array<Float>>();
			copy.offsets.resize(regionCount);
			for (i in 0...regionCount) {
				copy.offsets[i] = new Array<Float>();
				copy.offsets[i].resize(8);
				for (j in 0...8)
					copy.offsets[i][j] = this.offsets[i][j];
			}
		}

		return copy;
	}

	/** Computes UVs and offsets for the specified attachment. Must be called if the regions or attachment properties are
	 * changed. */
	public function update(attachment:HasSequence):Void {
		var regionCount = this.regions.length;
		if (Std.isOfType(attachment, RegionAttachment)) {
			var region:RegionAttachment = cast(attachment, RegionAttachment);
			this.uvs = new Array<Array<Float>>();
			this.uvs.resize(regionCount);
			this.offsets = new Array<Array<Float>>();
			this.offsets.resize(regionCount);
			for (i in 0...regionCount) {
				this.uvs[i] = new Array<Float>();
				this.uvs[i].resize(8);
				this.offsets[i] = new Array<Float>();
				this.offsets[i].resize(8);
				RegionAttachment.computeUVs(this.regions[i], region.x, region.y, region.scaleX, region.scaleY, region.rotation,
					region.width, region.height, this.offsets[i], this.uvs[i]);
			}
		} else if (Std.isOfType(attachment, MeshAttachment)) {
			var mesh:MeshAttachment = cast(attachment, MeshAttachment);
			var regionUVs = mesh.regionUVs;
			this.uvs = new Array<Array<Float>>();
			this.uvs.resize(regionCount);
			this.offsets = null;
			for (i in 0...regionCount) {
				this.uvs[i] = new Array<Float>();
				this.uvs[i].resize(regionUVs.length);
				MeshAttachment.computeUVs(this.regions[i], regionUVs, this.uvs[i]);
			}
		}
	}

	public function resolveIndex(pose:SlotPose):Int {
		var index:Int = pose.sequenceIndex;
		if (index == -1) index = this.setupIndex;
		if (index >= this.regions.length) index = this.regions.length - 1;
		return index;
	}

	public function getUVs(index:Int):Array<Float> {
		return this.uvs[index];
	}

	public function getHasPathSuffix():Bool {
		return this.pathSuffix;
	}

	public function getPath(basePath:String, index:Int):String {
		if (!this.pathSuffix) return basePath;
		var result = basePath;
		var frame = Std.string(this.start + index);
		for (i in 0...(this.digits - frame.length))
			result += "0";
		result += frame;
		return result;
	}
}
