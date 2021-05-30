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
	import spine.attachments.Attachment;
	import spine.attachments.VertexAttachment;
	import spine.Event;
	import spine.Skeleton;
	import spine.Slot;

	public class DeformTimeline extends CurveTimeline implements SlotTimeline {
		public var slotIndex : int;

		/** The attachment that will be deformed. */
		public var attachment : VertexAttachment;

		/** The vertices for each key frame. */
		public var vertices : Vector.<Vector.<Number>>;

		public function DeformTimeline (frameCount : int, bezierCount : int, slotIndex : int, attachment : VertexAttachment) {
			super(frameCount, bezierCount, [
				Property.deform + "|" + slotIndex + "|" + attachment.id
			]);
			this.slotIndex = slotIndex;
			this.attachment = attachment;
			vertices = new Vector.<Vector.<Number>>(frameCount, true);
		}

		public override function getFrameCount () : int {
			return frames.length;
		}

		public function getSlotIndex() : int {
			return slotIndex;
		}

		/** Sets the time in seconds and the vertices for the specified key frame.
		 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
		public function setFrame (frame : int, time : Number, vertices : Vector.<Number>) : void {
			frames[frame] = time;
			this.vertices[frame] = vertices;
		}

		/** @param value1 Ignored (0 is used for a deform timeline).
		 * @param value2 Ignored (1 is used for a deform timeline). */
		public override function setBezier (bezier : int, frame: int, value : Number, time1 : Number, value1 : Number, cx1 : Number, cy1: Number, cx2 : Number,
			cy2 : Number, time2 : Number, value2 : Number) : void {
			var curves : Vector.<Number> = this.curves;
			var i : int = getFrameCount() + bezier * BEZIER_SIZE;
			if (value == 0) curves[frame] = BEZIER + i;
			var tmpx : Number = (time1 - cx1 * 2 + cx2) * 0.03, tmpy : Number = cy2 * 0.03 - cy1 * 0.06;
			var dddx : Number = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy : Number = (cy1 - cy2 + 0.33333333) * 0.018;
			var ddx : Number = tmpx * 2 + dddx, ddy : Number = tmpy * 2 + dddy;
			var dx : Number = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy : Number = cy1 * 0.3 + tmpy + dddy * 0.16666667;
			var x : Number = time1 + dx, y : Number = dy;
			for (var n : int = i + BEZIER_SIZE; i < n; i += 2) {
				curves[i] = x;
				curves[i + 1] = y;
				dx += ddx;
				dy += ddy;
				ddx += dddx;
				ddy += dddy;
				x += dx;
				y += dy;
			}
		}

		private function getCurvePercent (time : Number, frame : int) : Number {
			var curves : Vector.<Number> = this.curves;
			var i : int = curves[frame];
			var x : Number;
			switch (i) {
			case LINEAR:
				x = frames[frame];
				return (time - x) / (frames[frame + getFrameEntries()] - x);
			case STEPPED:
				return 0;
			}
			i -= BEZIER;
			if (curves[i] > time) {
				x = frames[frame];
				return curves[i + 1] * (time - x) / (curves[i] - x);
			}
			var n : int = i + BEZIER_SIZE, y : Number;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					x = curves[i - 2];
					y = curves[i - 1];
					return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
				}
			}
			x = curves[n - 2];
			y = curves[n - 1];
			return y + (1 - y) * (time - x) / (frames[frame + getFrameEntries()] - x);
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var slot : Slot = skeleton.slots[slotIndex];
			if (!slot.bone.active) return;
			var slotAttachment : Attachment = slot.attachment;
			if (!(slotAttachment is VertexAttachment) || VertexAttachment(slotAttachment).deformAttachment != attachment) return;
			var vertexAttachment : VertexAttachment = VertexAttachment(slotAttachment);

			var deform : Vector.<Number> = slot.deform;
			if (deform.length == 0) blend = MixBlend.setup;

			var vertices : Vector.<Vector.<Number>> = this.vertices;
			var vertexCount : int = vertices[0].length;

			var i : int, setupVertices : Vector.<Number>;

			var frames : Vector.<Number> = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					deform.length = 0;
					return;
				case MixBlend.first:
					if (alpha == 1) {
						deform.length = 0;
						return;
					}
					deform.length = vertexCount;
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions.
						setupVertices = vertexAttachment.vertices;
						for (i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
				}
				return;
			}

			deform.length = vertexCount;
			var setup : Number;

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				var lastVertices : Vector.<Number> = vertices[frames.length - 1];
				if (alpha == 1) {
					if (blend == MixBlend.add) {
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							setupVertices = vertexAttachment.vertices;
							for (i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] - setupVertices[i];
						} else {
							// Weighted deform offsets, with alpha.
							for (i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else {
						for (i = 0; i < vertexCount; i++)
							deform[i] = lastVertices[i];
					}
				} else {
					switch (blend) {
					case MixBlend.setup: {
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							setupVertices = vertexAttachment.vertices;
							for (i = 0; i < vertexCount; i++) {
								setup = setupVertices[i];
								deform[i] = setup + (lastVertices[i] - setup) * alpha;
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (i = 0; i < vertexCount; i++)
								deform[i] = lastVertices[i] * alpha;
						}
						break;
					}
					case MixBlend.first:
					case MixBlend.replace:
						for (i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
						break;
					case MixBlend.add:
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							setupVertices = vertexAttachment.vertices;
							for (i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						} else {
							// Weighted deform offsets, with alpha.
							for (i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] * alpha;
						}
					}
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			var frame : int = search(frames, time);
			var percent : Number = getCurvePercent(time, frame);
			var prevVertices : Vector.<Number> = vertices[frame], prev : Number;
			var nextVertices : Vector.<Number> = vertices[frame + 1];

			if (alpha == 1) {
				if (blend == MixBlend.add) {
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						setupVertices = vertexAttachment.vertices;
						for (i = 0; i < vertexCount; i++) {
							prev = prevVertices[i];
							deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (i = 0; i < vertexCount; i++) {
							prev = prevVertices[i];
							deform[i] += prev + (nextVertices[i] - prev) * percent;
						}
					}
				} else {
					for (i = 0; i < vertexCount; i++) {
						prev = prevVertices[i];
						deform[i] = prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else {
				switch (blend) {
				case MixBlend.setup: {
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						setupVertices = vertexAttachment.vertices;
						for (i = 0; i < vertexCount; i++) {
							prev = prevVertices[i];
							setup = setupVertices[i];
							deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (i = 0; i < vertexCount; i++) {
							prev = prevVertices[i];
							deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
					break;
				}
				case MixBlend.first:
				case MixBlend.replace:
					for (i = 0; i < vertexCount; i++) {
						prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
					break;
				case MixBlend.add:
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						setupVertices = vertexAttachment.vertices;
						for (i = 0; i < vertexCount; i++) {
							prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (i = 0; i < vertexCount; i++) {
							prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
				}
			}
		}
	}
}
