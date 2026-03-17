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

import spine.Bone;
import spine.Skeleton;
import spine.Slot;

/** Base class for an attachment with vertices that are transformed by one or more bones and can be deformed by a slot's
 * spine.SlotPose.deform. */
class VertexAttachment extends Attachment {
	private static var nextID:Int = 0;

	/** The bones which affect the vertices. The array entries are, for each vertex, the number of bones affecting
	 * the vertex followed by that many bone indices, which is the index of the bone in spine.Skeleton.bones. Will be null
	 * if this attachment has no weights. */
	public var bones:Array<Int>;

	/** The vertex positions in the bone's coordinate system. For a non-weighted attachment, the values are `x,y`
	 * entries for each vertex. For a weighted attachment, the values are `x,y,weight` entries for each bone affecting
	 * each vertex. */
	public var vertices = new Array<Float>();

	/** The maximum number of world vertex values that can be output by
	 * computeWorldVertices() using the `count` parameter. */
	public var worldVerticesLength:Int = 0;

	/** Returns a unique ID for this attachment. */
	public var id:Int = nextID++;

	public function new(name:String) {
		super(name);
	}

	/** Transforms the attachment's local vertices to world coordinates. If the slot's spine.SlotPose.deform is
	 * not empty, it is used to deform the vertices.
	 *
	 * @see https://esotericsoftware.com/spine-runtime-skeletons#World-transforms World transforms in the Spine Runtimes Guide
	 * @param start The index of the first vertices value to transform. Each vertex has 2 values, x and y.
	 * @param count The number of world vertex values to output. Must be <= worldVerticesLength - `start`.
	 * @param worldVertices The output world vertices. Must have a length >= `offset` + `count` *
	 *           `stride` / 2.
	 * @param offset The `worldVertices` index to begin writing values.
	 * @param stride The number of `worldVertices` entries between the value pairs written. */
	public function computeWorldVertices(skeleton:Skeleton, slot:Slot, start:Int, count:Int, worldVertices:Array<Float>, offset:Int, stride:Int):Void {
		count = offset + (count >> 1) * stride;
		var deform:Array<Float> = slot.applied.deform;
		var vertices = vertices;
		if (bones == null) {
			if (deform.length > 0)
				vertices = deform;
			var bone = slot.bone.applied;
			var x = bone.worldX, y = bone.worldY;
			var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			var v = start, w = offset;
			while (w < count) {
				var vx = vertices[v], vy = vertices[v + 1];
				worldVertices[w] = vx * a + vy * b + x;
				worldVertices[w + 1] = vx * c + vy * d + y;
				v += 2;
				w += stride;
			}
			return;
		}
		var v = 0, skip = 0, i = 0;
		while (i < start) {
			var n = bones[v];
			v += n + 1;
			skip += n;
			i += 2;
		}
		var skeletonBones = skeleton.bones;
		if (deform.length == 0) {
			var w = offset, b = skip * 3;
			while (w < count) {
				var wx = 0., wy = 0.;
				var n = bones[v++];
				n += v;
				while (v < n) {
					var bone = skeletonBones[bones[v]].applied;
					var vx = vertices[b],
						vy = vertices[b + 1],
						weight = vertices[b + 2];
					wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
					wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					v++;
					b += 3;
				}
				worldVertices[w] = wx;
				worldVertices[w + 1] = wy;
				w += stride;
			}
		} else {
			var w = offset, b = skip * 3, f = skip << 1;
			while (w < count) {
				var wx = 0., wy = 0.;
				var n = bones[v++];
				n += v;
				while (v < n) {
					var bone = skeletonBones[bones[v]].applied;
					var vx = vertices[b] + deform[f],
						vy = vertices[b + 1] + deform[f + 1],
						weight = vertices[b + 2];
					wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
					wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					v++;
					b += 3;
					f += 2;
				}
				worldVertices[w] = wx;
				worldVertices[w + 1] = wy;
				w += stride;
			}
		}
	}

	/** Copy this attachment's data to another attachment. */
	public function copyTo(attachment:VertexAttachment):Void {
		if (bones != null) {
			attachment.bones = bones.copy();
		} else {
			attachment.bones = null;
		}

		if (this.vertices != null) {
			attachment.vertices = vertices.copy();
		}

		attachment.worldVerticesLength = worldVerticesLength;
		attachment.timelineAttachment = timelineAttachment;
	}
}
