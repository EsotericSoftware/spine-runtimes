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

package spine.animation;

import spine.animation.Timeline;
import spine.attachments.Attachment;
import spine.attachments.VertexAttachment;
import spine.Event;
import spine.Skeleton;
import spine.Slot;

class DeformTimeline extends CurveTimeline implements SlotTimeline {
	public var slotIndex:Int = 0;

	/** The attachment that will be deformed. */
	public var attachment:VertexAttachment;

	/** The vertices for each key frame. */
	public var vertices:Array<Array<Float>>;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int, attachment:VertexAttachment) {
		super(frameCount, bezierCount, [Property.deform + "|" + slotIndex + "|" + attachment.id]);
		this.slotIndex = slotIndex;
		this.attachment = attachment;
		vertices = new Array<Array<Float>>();
		vertices.resize(frameCount);
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	/** Sets the time in seconds and the vertices for the specified key frame.
	 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
	public function setFrame(frame:Int, time:Float, verticesOrDeform:Array<Float>):Void {
		frames[frame] = time;
		vertices[frame] = verticesOrDeform;
	}

	/** @param value1 Ignored (0 is used for a deform timeline).
	 * @param value2 Ignored (1 is used for a deform timeline). */
	public override function setBezier(bezier:Int, frame:Int, value:Float, time1:Float, value1:Float, cx1:Float, cy1:Float, cx2:Float, cy2:Float, time2:Float,
			value2:Float):Void {
		var i:Int = getFrameCount() + bezier * CurveTimeline.BEZIER_SIZE;
		if (value == 0)
			curves[frame] = CurveTimeline.BEZIER + i;
		var tmpx:Float = (time1 - cx1 * 2 + cx2) * 0.03,
			tmpy:Float = cy2 * 0.03 - cy1 * 0.06;
		var dddx:Float = ((cx1 - cx2) * 3 - time1 + time2) * 0.006,
			dddy:Float = (cy1 - cy2 + 0.33333333) * 0.018;
		var ddx:Float = tmpx * 2 + dddx, ddy:Float = tmpy * 2 + dddy;
		var dx:Float = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667,
			dy:Float = cy1 * 0.3 + tmpy + dddy * 0.16666667;
		var x:Float = time1 + dx, y:Float = dy;
		var n:Int = i + CurveTimeline.BEZIER_SIZE;
		while (i < n) {
			curves[i] = x;
			curves[i + 1] = y;
			dx += ddx;
			dy += ddy;
			ddx += dddx;
			ddy += dddy;
			x += dx;
			y += dy;

			i += 2;
		}
	}

	private function getCurvePercent(time:Float, frame:Int):Float {
		var i:Int = Std.int(curves[frame]);
		var x:Float;
		switch (i) {
			case CurveTimeline.LINEAR:
				x = frames[frame];
				return (time - x) / (frames[frame + getFrameEntries()] - x);
			case CurveTimeline.STEPPED:
				return 0;
		}
		i -= CurveTimeline.BEZIER;
		if (curves[i] > time) {
			x = frames[frame];
			return curves[i + 1] * (time - x) / (curves[i] - x);
		}
		var n:Int = i + CurveTimeline.BEZIER_SIZE, y:Float;
		i += 2;
		while (i < n) {
			if (curves[i] >= time) {
				x = curves[i - 2];
				y = curves[i - 1];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}

			i += 2;
		}
		x = curves[n - 2];
		y = curves[n - 1];
		return y + (1 - y) * (time - x) / (frames[frame + getFrameEntries()] - x);
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var slot:Slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;
		var slotAttachment:Attachment = slot.attachment;
		if (slotAttachment == null)
			return;
		if (!Std.isOfType(slotAttachment, VertexAttachment) || cast(slotAttachment, VertexAttachment).timelineAttachment != attachment)
			return;

		var deform:Array<Float> = slot.deform;
		if (deform.length == 0)
			blend = MixBlend.setup;

		var vertexCount:Int = vertices[0].length;
		var i:Int, setupVertices:Array<Float>;

		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					deform.resize(0);
				case MixBlend.first:
					if (alpha == 1) {
						deform.resize(0);
						return;
					}
					ArrayUtils.resize(deform, vertexCount, 0);
					var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions.
						setupVertices = vertexAttachment.vertices;
						for (i in 0...vertexCount) {
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
						}
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (i in 0...vertexCount) {
							deform[i] *= alpha;
						}
					}
			}
			return;
		}

		ArrayUtils.resize(deform, vertexCount, 0);
		var setup:Float;
		if (time >= frames[frames.length - 1]) {
			var lastVertices:Array<Float> = vertices[frames.length - 1];
			if (alpha == 1) {
				if (blend == MixBlend.add) {
					var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						setupVertices = vertexAttachment.vertices;
						for (i in 0...vertexCount) {
							deform[i] += lastVertices[i] - setupVertices[i];
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (i in 0...vertexCount) {
							deform[i] += lastVertices[i];
						}
					}
				} else {
					for (i in 0...vertexCount) {
						deform[i] = lastVertices[i];
					}
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							setupVertices = vertexAttachment.vertices;
							for (i in 0...vertexCount) {
								setup = setupVertices[i];
								deform[i] = setup + (lastVertices[i] - setup) * alpha;
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (i in 0...vertexCount) {
								deform[i] = lastVertices[i] * alpha;
							}
						}
					case MixBlend.first, MixBlend.replace:
						for (i in 0...vertexCount) {
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
						}
					case MixBlend.add:
						var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							setupVertices = vertexAttachment.vertices;
							for (i in 0...vertexCount) {
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (i in 0...vertexCount) {
								deform[i] += lastVertices[i] * alpha;
							}
						}
				}
			}
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frame:Int = Timeline.search1(frames, time);
		var percent:Float = getCurvePercent(time, frame);
		var prevVertices:Array<Float> = vertices[frame], prev:Float;
		var nextVertices:Array<Float> = vertices[frame + 1];

		if (alpha == 1) {
			if (blend == MixBlend.add) {
				var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
				if (vertexAttachment.bones == null) {
					// Unweighted vertex positions, with alpha.
					setupVertices = vertexAttachment.vertices;
					for (i in 0...vertexCount) {
						prev = prevVertices[i];
						deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
					}
				} else {
					// Weighted deform offsets, with alpha.
					for (i in 0...vertexCount) {
						prev = prevVertices[i];
						deform[i] += prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else {
				for (i in 0...vertexCount) {
					prev = prevVertices[i];
					deform[i] = prev + (nextVertices[i] - prev) * percent;
				}
			}
		} else {
			switch (blend) {
				case MixBlend.setup:
					var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						setupVertices = vertexAttachment.vertices;
						for (i in 0...vertexCount) {
							prev = prevVertices[i];
							setup = setupVertices[i];
							deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (i in 0...vertexCount) {
							prev = prevVertices[i];
							deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
				case MixBlend.first, MixBlend.replace:
					for (i in 0...vertexCount) {
						prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
				case MixBlend.add:
					var vertexAttachment:VertexAttachment = cast(slotAttachment, VertexAttachment);
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						setupVertices = vertexAttachment.vertices;
						for (i in 0...vertexCount) {
							prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (i in 0...vertexCount) {
							prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
			}
		}
	}
}
