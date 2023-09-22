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

package spine;

class Sequence {
	private static var _nextID = 0;

	public var id = _nextID++;
	public var regions:Array<TextureRegion>;
	public var start = 0;
	public var digits = 0;

	/** The index of the region to show for the setup pose. */
	public var setupIndex = 0;

	public function new(count:Int) {
		this.regions = new Array<TextureRegion>();
		this.regions.resize(count);
	}

	public function copy():Sequence {
		var copy = new Sequence(this.regions.length);
		for (i in 0...this.regions.length) {
			copy.regions[i] = this.regions[i];
		}
		copy.start = this.start;
		copy.digits = this.digits;
		copy.setupIndex = this.setupIndex;
		return copy;
	}

	public function apply(slot:Slot, attachment:HasTextureRegion) {
		var index:Int = slot.sequenceIndex;
		if (index == -1)
			index = this.setupIndex;
		if (index >= this.regions.length)
			index = this.regions.length - 1;
		var region = this.regions[index];
		if (attachment.region != region) {
			attachment.region = region;
			attachment.updateRegion();
		}
	}

	public function getPath(basePath:String, index:Int):String {
		var result = basePath;
		var frame = Std.string(this.start + index);

		for (i in 0...(this.digits - frame.length)) {
			result += "0";
		}
		result += frame;
		return result;
	}
}
