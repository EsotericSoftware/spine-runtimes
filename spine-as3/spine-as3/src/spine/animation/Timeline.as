/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package spine.animation {
	import spine.Event;
	import spine.Skeleton;

	public class Timeline {
		public var propertyIds : Vector.<String>;
		public var frames : Vector.<Number>;

		public function Timeline(frameCount : int, propertyIds : Array) {
			this.propertyIds = new Vector.<String>(propertyIds.length, true);
			for (var i : int = 0, n : int = propertyIds.length; i < n; i++)
				this.propertyIds[i] = propertyIds[i];
			frames = new Vector.<Number>(frameCount * getFrameEntries(), true);
		}

		public function getFrameEntries() : int {
			return 1;
		}

		public function getFrameCount() : int {
			return frames.length / getFrameEntries();
		}

		public function getDuration() : Number {
			return frames[frames.length - getFrameEntries()];
		}

		public function apply (skeleton: Skeleton, lastTime: Number, time: Number, events: Vector.<Event>, alpha: Number, blend: MixBlend, direction: MixDirection) : void {
		}

		static internal function search (frames : Vector.<Number>, time : Number) : int {
			var n : int = frames.length;
			for (var i : int = 1; i < n; i++)
				if (frames[i] > time) return i - 1;
			return n - 1;
		}

		static internal function search2 (values : Vector.<Number>, time : Number, step: int) : int {
			var n : int = values.length;
			for (var i : int = step; i < n; i += step)
				if (values[i] > time) return i - step;
			return n - step;
		}
	}
}
