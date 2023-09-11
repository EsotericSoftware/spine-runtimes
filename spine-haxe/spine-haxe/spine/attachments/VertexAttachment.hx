package spine.attachments;

import openfl.Vector;
import spine.Bone;
import spine.Skeleton;
import spine.Slot;

class VertexAttachment extends Attachment {
	private static var nextID:Int = 0;

	public var bones:Vector<Int>;
	public var vertices = new Vector<Float>();
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
	public function computeWorldVertices(slot:Slot, start:Int, count:Int, worldVertices:Vector<Float>, offset:Int, stride:Int):Void {
		count = offset + (count >> 1) * stride;
		var skeleton:Skeleton = slot.skeleton;
		var deform:Vector<Float> = slot.deform;

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
		var skeletonBones:Vector<Bone> = skeleton.bones;
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
			attachment.bones = bones.concat();
		} else {
			attachment.bones = null;
		}

		if (this.vertices != null) {
			attachment.vertices = vertices.concat();
		}

		attachment.worldVerticesLength = worldVerticesLength;
		attachment.timelineAttachment = timelineAttachment;
	}
}
