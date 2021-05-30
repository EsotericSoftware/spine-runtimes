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
	/** The base class for a {@link CurveTimeline} which sets two properties. */
	public class CurveTimeline2 extends CurveTimeline {
		static internal const ENTRIES : Number = 3;
		static internal const VALUE1 : Number = 1;
		static internal const VALUE2 : Number = 2;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public function CurveTimeline2 (frameCount : int, bezierCount : int, propertyIds : Array) {
			super(frameCount, bezierCount, propertyIds);
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		/** Sets the time and values for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public function setFrame(frame : int, time : Number, value1 : Number, value2 : Number) : void {
			frame *= ENTRIES;
			var frames : Vector.<Number> = this.frames;
			frames[frame] = time;
			frames[frame + VALUE1] = value1;
			frames[frame + VALUE2] = value2;
		}
	}
}
