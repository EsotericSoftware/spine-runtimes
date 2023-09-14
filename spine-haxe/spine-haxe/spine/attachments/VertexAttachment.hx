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

import spine.Bone;
import spine.Skeleton;
import spine.Slot;

class VertexAttachment extends Attachment {
	private static var nextID:Int = 0;

	public var bones:Array<Int>;
	public var vertices = new Array<Float>();
	public var worldVerticesLength:Int = 0;
	public var id:Int = nextID++;
	public var timelineAttachment:VertexAttachment;

	public function new(name:String) {
		super(name);
		timelineAttachment = this;
	}

	/** Transforms the attachment's local {@link #vertices} to world coordinates. If the slot's {@link Slot#deform} is
	 * not empty, it is used to deform the vertices.
	 *
	 * See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
	 * Runtimes Guide.
	 * @param start The index of the first {@link #vertices} value to transform. Each vertex has 2 values, x and y.
	 * @param count The number of world vertex values to output. Must be <= {@link #worldVerticesLength} - `start`.
	 * @param worldVertices The output world vertices. Must have a length >= `offset` + `count` *
	 *           `stride` / 2.
	 * @param offset The `worldVertices` index to begin writing values.
	 * @param stride The number of `worldVertices` entries between the value pairs written. */
	public function computeWorldVertices(slot:Slot, start:Int, count:Int, worldVertices:Array<Float>, offset:Int, stride:Int):Void {
		count = offset + (count >> 1) * stride;
		var skeleton:Skeleton = slot.skeleton;
		var deform:Array<Float> = slot.deform;

		var v:Int, w:Int, n:Int, i:Int, skip:Int, b:Int, f:Int;
		var vx:Float, vy:Float;
		var wx:Float, wy:Float;
		var bone:Bone;

		if (bones == null) {
			if (deform.length > 0)
				vertices = deform;
			bone = slot.bone;
			var x:Float = bone.worldX;
			var y:Float = bone.worldY;
			var a:Float = bone.a,
				bb:Float = bone.b,
				c:Float = bone.c,
				d:Float = bone.d;
			v = start;
			w = offset;
			while (w < count) {
				vx = vertices[v];
				vy = vertices[v + 1];
				worldVertices[w] = vx * a + vy * bb + x;
				worldVertices[w + 1] = vx * c + vy * d + y;
				v += 2;
				w += stride;
			}
			return;
		}
		v = 0;
		skip = 0;
		i = 0;
		while (i < start) {
			n = bones[v];
			v += n + 1;
			skip += n;
			i += 2;
		}
		var skeletonBones:Array<Bone> = skeleton.bones;
		if (deform.length == 0) {
			w = offset;
			b = skip * 3;
			while (w < count) {
				wx = 0;
				wy = 0;
				n = bones[v++];
				n += v;
				while (v < n) {
					bone = skeletonBones[bones[v]];
					vx = vertices[b];
					vy = vertices[b + 1];
					var weight:Float = vertices[b + 2];
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
			w = offset;
			b = skip * 3;
			f = skip << 1;
			while (w < count) {
				wx = 0;
				wy = 0;
				n = bones[v++];
				n += v;
				while (v < n) {
					bone = skeletonBones[bones[v]];
					vx = vertices[b] + deform[f];
					vy = vertices[b + 1] + deform[f + 1];
					var weight = vertices[b + 2];
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
