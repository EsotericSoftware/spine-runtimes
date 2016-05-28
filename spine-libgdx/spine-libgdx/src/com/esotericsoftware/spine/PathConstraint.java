
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.PathAttachment;

public class PathConstraint implements Updatable {
	final PathConstraintData data;
	final Array<Bone> bones;
	Slot target;
	float position, rotateMix, translateMix;
	final FloatArray lengths = new FloatArray(), positions = new FloatArray();
	final Vector2 temp = new Vector2();

	public PathConstraint (PathConstraintData data, Skeleton skeleton) {
		this.data = data;
		position = data.position;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.findBone(boneData.name));

		target = skeleton.findSlot(data.target.name);
	}

	/** Copy constructor. */
	public PathConstraint (PathConstraint constraint, Skeleton skeleton) {
		data = constraint.data;
		bones = new Array(constraint.bones.size);
		for (Bone bone : constraint.bones)
			bones.add(skeleton.bones.get(bone.data.index));
		target = skeleton.slots.get(constraint.target.data.index);
		position = constraint.position;
		rotateMix = constraint.rotateMix;
		translateMix = constraint.translateMix;
	}

	public void apply () {
		update();
	}

	public void update () {
		Attachment attachment = target.getAttachment();
		if (!(attachment instanceof PathAttachment)) return;

		float translateMix = this.translateMix, rotateMix = this.rotateMix;
		boolean translate = translateMix > 0, rotate = rotateMix > 0;
		if (!translate && !rotate) return;

		PathAttachment path = (PathAttachment)attachment;
		FloatArray lengths = this.lengths, positions = this.positions;
		lengths.clear();
		lengths.add(0);
		positions.clear();

		Array<Bone> bones = this.bones;
		int boneCount = bones.size;
		if (boneCount == 1) {
			path.computeWorldPositions(target, position, lengths, positions, rotate);
			Bone bone = bones.first();
			bone.worldX += (positions.first() - bone.worldX) * translateMix;
			bone.worldY += (positions.get(1) - bone.worldY) * translateMix;
			if (rotate) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = positions.get(2) - atan2(c, a) + data.offsetRotation * degRad;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r *= rotateMix;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			return;
		}

		for (int i = 0; i < boneCount; i++)
			lengths.add(bones.get(i).data.length);
		path.computeWorldPositions(target, position, lengths, positions, false);

		Vector2 temp = this.temp;
		float boneX = positions.first(), boneY = positions.get(1);
		for (int i = 0, p = 2; i < boneCount; i++, p += 2) {
			Bone bone = bones.get(i);
			bone.worldX += (boneX - bone.worldX) * translateMix;
			bone.worldY += (boneY - bone.worldY) * translateMix;

			float x = positions.get(p), y = positions.get(p + 1);
			if (rotate) {
				// Scale.
				// float dist = (float)Math.sqrt((boneX - x) * (boneX - x) + (boneY - y) * (boneY - y));
				// bone.scaleX = dist / bone.data.length;
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(y - boneY, x - boneX) - atan2(c, a) + data.offsetRotation * degRad;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r *= rotateMix;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
				if (data.offsetRotation == 0 && rotateMix == 1) {
					// Place at tip.
					bone.localToWorld(temp.set(bone.data.length, 0));
					boneX = temp.x;
					boneY = temp.y;
				} else {
					// Place on path.
					boneX = x;
					boneY = y;
				}
			} else {
				// Place on path.
				boneX = x;
				boneY = y;
			}
		}
	}

	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	public float getRotateMix () {
		return rotateMix;
	}

	public void setRotateMix (float rotateMix) {
		this.rotateMix = rotateMix;
	}

	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public Array<Bone> getBones () {
		return bones;
	}

	public Slot getTarget () {
		return target;
	}

	public void setTarget (Slot target) {
		this.target = target;
	}

	public PathConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
