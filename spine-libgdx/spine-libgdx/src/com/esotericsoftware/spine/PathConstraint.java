
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.PathAttachment;

public class PathConstraint implements Updatable {
	final PathConstraintData data;
	Bone bone;
	Slot target;
	float position, rotateMix, translateMix;
	final Vector2 worldPosition = new Vector2(), tangent = new Vector2();

	public PathConstraint (PathConstraintData data, Skeleton skeleton) {
		this.data = data;
		position = data.position;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;
		bone = skeleton.findBone(data.bone.name);
		target = skeleton.findSlot(data.target.name);
	}

	/** Copy constructor. */
	public PathConstraint (PathConstraint constraint, Skeleton skeleton) {
		data = constraint.data;
		bone = skeleton.bones.get(constraint.bone.data.index);
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
		PathAttachment path = (PathAttachment)attachment;

		float translateMix = this.translateMix;
		if (translateMix > 0) {
			path.computeWorldPosition(target, position, worldPosition, null);
			bone.worldX += (worldPosition.x - bone.worldX) * translateMix;
			bone.worldY += (worldPosition.y - bone.worldY) * translateMix;
		}

		float rotateMix = this.rotateMix;
		if (rotateMix > 0) {
			path.computeWorldPosition(target, position, worldPosition, tangent);
			float r = worldPosition.sub(tangent).angle() - bone.getWorldRotationX();
			float cos = cosDeg(r), sin = sinDeg(r);
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			bone.a = cos * a - sin * c;
			bone.b = cos * b - sin * d;
			bone.c = sin * a + cos * c;
			bone.d = sin * b + cos * d;
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

	public Bone getBone () {
		return bone;
	}

	public void setBone (Bone bone) {
		this.bone = bone;
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
