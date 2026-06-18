
package com.esotericsoftware.spine.utils;

import java.util.HashMap;
import java.util.Map;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;

import com.esotericsoftware.spine.Animation;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationState.TrackEntry;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Bone;
import com.esotericsoftware.spine.BoneData;
import com.esotericsoftware.spine.BonePose;
import com.esotericsoftware.spine.Constraint;
import com.esotericsoftware.spine.ConstraintData;
import com.esotericsoftware.spine.Event;
import com.esotericsoftware.spine.EventData;
import com.esotericsoftware.spine.IkConstraint;
import com.esotericsoftware.spine.IkConstraintData;
import com.esotericsoftware.spine.IkConstraintPose;
import com.esotericsoftware.spine.PathConstraint;
import com.esotericsoftware.spine.PathConstraintData;
import com.esotericsoftware.spine.PathConstraintPose;
import com.esotericsoftware.spine.PhysicsConstraint;
import com.esotericsoftware.spine.PhysicsConstraintData;
import com.esotericsoftware.spine.PhysicsConstraintPose;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.Skin;
import com.esotericsoftware.spine.Skin.SkinEntry;
import com.esotericsoftware.spine.Slider;
import com.esotericsoftware.spine.SliderData;
import com.esotericsoftware.spine.SliderPose;
import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.SlotData;
import com.esotericsoftware.spine.SlotPose;
import com.esotericsoftware.spine.TransformConstraint;
import com.esotericsoftware.spine.TransformConstraintData;
import com.esotericsoftware.spine.TransformConstraintData.FromProperty;
import com.esotericsoftware.spine.TransformConstraintData.ToProperty;
import com.esotericsoftware.spine.TransformConstraintPose;
import com.esotericsoftware.spine.Update;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.Sequence;
import com.esotericsoftware.spine.attachments.VertexAttachment;

public class SkeletonSerializer {
	private final Map<Object, String> visitedObjects = new HashMap();
	private int nextId = 1;
	private JsonWriter json;

	public String serializeSkeletonData (SkeletonData data) {
		visitedObjects.clear();
		nextId = 1;
		json = new JsonWriter();
		writeSkeletonData(data);
		json.close();
		return json.getString();
	}

	public String serializeSkeleton (Skeleton skeleton) {
		visitedObjects.clear();
		nextId = 1;
		json = new JsonWriter();
		writeSkeleton(skeleton);
		json.close();
		return json.getString();
	}

	public String serializeAnimationState (AnimationState state) {
		visitedObjects.clear();
		nextId = 1;
		json = new JsonWriter();
		writeAnimationState(state);
		json.close();
		return json.getString();
	}

	private void writeAnimation (Animation obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<Animation-" + obj.getName() + ">" : "<Animation-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Animation");

		json.writeName("timelines");
		json.writeArrayStart();
		for (Timeline item : obj.getTimelines()) {
			writeTimeline(item);
		}
		json.writeArrayEnd();

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("bones");
		writeIntArray(obj.getBones());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writeAlphaTimeline (Animation.AlphaTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<AlphaTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AlphaTimeline");

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeAttachmentTimeline (Animation.AttachmentTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<AttachmentTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AttachmentTimeline");

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("attachmentNames");
		json.writeArrayStart();
		for (String item : obj.getAttachmentNames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeDeformTimeline (Animation.DeformTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<DeformTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("DeformTimeline");

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("attachment");
		writeVertexAttachment(obj.getAttachment());

		json.writeName("vertices");
		json.writeArrayStart();
		for (float[] nestedArray : obj.getVertices()) {
			json.writeArrayStart();
			for (float elem : nestedArray) {
				json.writeValue(elem);
			}
			json.writeArrayEnd();
		}
		json.writeArrayEnd();

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeDrawOrderTimeline (Animation.DrawOrderTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<DrawOrderTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("DrawOrderTimeline");

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("drawOrders");
		json.writeArrayStart();
		for (int[] nestedArray : obj.getDrawOrders()) {
			json.writeArrayStart();
			for (int elem : nestedArray) {
				json.writeValue(elem);
			}
			json.writeArrayEnd();
		}
		json.writeArrayEnd();

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeEventTimeline (Animation.EventTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<EventTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("EventTimeline");

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("events");
		json.writeArrayStart();
		for (Event item : obj.getEvents()) {
			writeEvent(item);
		}
		json.writeArrayEnd();

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeIkConstraintTimeline (Animation.IkConstraintTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<IkConstraintTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraintTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeInheritTimeline (Animation.InheritTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<InheritTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("InheritTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePathConstraintMixTimeline (Animation.PathConstraintMixTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PathConstraintMixTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintMixTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePathConstraintPositionTimeline (Animation.PathConstraintPositionTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PathConstraintPositionTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintPositionTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePathConstraintSpacingTimeline (Animation.PathConstraintSpacingTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PathConstraintSpacingTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintSpacingTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintDampingTimeline (Animation.PhysicsConstraintDampingTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintDampingTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintDampingTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintGravityTimeline (Animation.PhysicsConstraintGravityTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintGravityTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintGravityTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintInertiaTimeline (Animation.PhysicsConstraintInertiaTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintInertiaTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintInertiaTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintMassTimeline (Animation.PhysicsConstraintMassTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintMassTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintMassTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintMixTimeline (Animation.PhysicsConstraintMixTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintMixTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintMixTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintResetTimeline (Animation.PhysicsConstraintResetTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintResetTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintResetTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintStrengthTimeline (Animation.PhysicsConstraintStrengthTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintStrengthTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintStrengthTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintWindTimeline (Animation.PhysicsConstraintWindTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintWindTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintWindTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeRGB2Timeline (Animation.RGB2Timeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<RGB2Timeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGB2Timeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeRGBA2Timeline (Animation.RGBA2Timeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<RGBA2Timeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGBA2Timeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeRGBATimeline (Animation.RGBATimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<RGBATimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGBATimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeRGBTimeline (Animation.RGBTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<RGBTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGBTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeRotateTimeline (Animation.RotateTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<RotateTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RotateTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeScaleTimeline (Animation.ScaleTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ScaleTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ScaleTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeScaleXTimeline (Animation.ScaleXTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ScaleXTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ScaleXTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeScaleYTimeline (Animation.ScaleYTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ScaleYTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ScaleYTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeSequenceTimeline (Animation.SequenceTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<SequenceTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SequenceTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("attachment");
		writeAttachment(obj.getAttachment());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeShearTimeline (Animation.ShearTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ShearTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ShearTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeShearXTimeline (Animation.ShearXTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ShearXTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ShearXTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeShearYTimeline (Animation.ShearYTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ShearYTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ShearYTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeSliderMixTimeline (Animation.SliderMixTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<SliderMixTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderMixTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeSliderTimeline (Animation.SliderTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<SliderTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderTimeline");

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeTimeline (Animation.Timeline obj) {
		if (obj instanceof Animation.AlphaTimeline) {
			writeAlphaTimeline((Animation.AlphaTimeline)obj);
		} else if (obj instanceof Animation.AttachmentTimeline) {
			writeAttachmentTimeline((Animation.AttachmentTimeline)obj);
		} else if (obj instanceof Animation.DeformTimeline) {
			writeDeformTimeline((Animation.DeformTimeline)obj);
		} else if (obj instanceof Animation.DrawOrderTimeline) {
			writeDrawOrderTimeline((Animation.DrawOrderTimeline)obj);
		} else if (obj instanceof Animation.EventTimeline) {
			writeEventTimeline((Animation.EventTimeline)obj);
		} else if (obj instanceof Animation.IkConstraintTimeline) {
			writeIkConstraintTimeline((Animation.IkConstraintTimeline)obj);
		} else if (obj instanceof Animation.InheritTimeline) {
			writeInheritTimeline((Animation.InheritTimeline)obj);
		} else if (obj instanceof Animation.PathConstraintMixTimeline) {
			writePathConstraintMixTimeline((Animation.PathConstraintMixTimeline)obj);
		} else if (obj instanceof Animation.PathConstraintPositionTimeline) {
			writePathConstraintPositionTimeline((Animation.PathConstraintPositionTimeline)obj);
		} else if (obj instanceof Animation.PathConstraintSpacingTimeline) {
			writePathConstraintSpacingTimeline((Animation.PathConstraintSpacingTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintDampingTimeline) {
			writePhysicsConstraintDampingTimeline((Animation.PhysicsConstraintDampingTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintGravityTimeline) {
			writePhysicsConstraintGravityTimeline((Animation.PhysicsConstraintGravityTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintInertiaTimeline) {
			writePhysicsConstraintInertiaTimeline((Animation.PhysicsConstraintInertiaTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintMassTimeline) {
			writePhysicsConstraintMassTimeline((Animation.PhysicsConstraintMassTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintMixTimeline) {
			writePhysicsConstraintMixTimeline((Animation.PhysicsConstraintMixTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintResetTimeline) {
			writePhysicsConstraintResetTimeline((Animation.PhysicsConstraintResetTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintStrengthTimeline) {
			writePhysicsConstraintStrengthTimeline((Animation.PhysicsConstraintStrengthTimeline)obj);
		} else if (obj instanceof Animation.PhysicsConstraintWindTimeline) {
			writePhysicsConstraintWindTimeline((Animation.PhysicsConstraintWindTimeline)obj);
		} else if (obj instanceof Animation.RGB2Timeline) {
			writeRGB2Timeline((Animation.RGB2Timeline)obj);
		} else if (obj instanceof Animation.RGBA2Timeline) {
			writeRGBA2Timeline((Animation.RGBA2Timeline)obj);
		} else if (obj instanceof Animation.RGBATimeline) {
			writeRGBATimeline((Animation.RGBATimeline)obj);
		} else if (obj instanceof Animation.RGBTimeline) {
			writeRGBTimeline((Animation.RGBTimeline)obj);
		} else if (obj instanceof Animation.RotateTimeline) {
			writeRotateTimeline((Animation.RotateTimeline)obj);
		} else if (obj instanceof Animation.ScaleTimeline) {
			writeScaleTimeline((Animation.ScaleTimeline)obj);
		} else if (obj instanceof Animation.ScaleXTimeline) {
			writeScaleXTimeline((Animation.ScaleXTimeline)obj);
		} else if (obj instanceof Animation.ScaleYTimeline) {
			writeScaleYTimeline((Animation.ScaleYTimeline)obj);
		} else if (obj instanceof Animation.SequenceTimeline) {
			writeSequenceTimeline((Animation.SequenceTimeline)obj);
		} else if (obj instanceof Animation.ShearTimeline) {
			writeShearTimeline((Animation.ShearTimeline)obj);
		} else if (obj instanceof Animation.ShearXTimeline) {
			writeShearXTimeline((Animation.ShearXTimeline)obj);
		} else if (obj instanceof Animation.ShearYTimeline) {
			writeShearYTimeline((Animation.ShearYTimeline)obj);
		} else if (obj instanceof Animation.SliderMixTimeline) {
			writeSliderMixTimeline((Animation.SliderMixTimeline)obj);
		} else if (obj instanceof Animation.SliderTimeline) {
			writeSliderTimeline((Animation.SliderTimeline)obj);
		} else if (obj instanceof Animation.TransformConstraintTimeline) {
			writeTransformConstraintTimeline((Animation.TransformConstraintTimeline)obj);
		} else if (obj instanceof Animation.TranslateTimeline) {
			writeTranslateTimeline((Animation.TranslateTimeline)obj);
		} else if (obj instanceof Animation.TranslateXTimeline) {
			writeTranslateXTimeline((Animation.TranslateXTimeline)obj);
		} else if (obj instanceof Animation.TranslateYTimeline) {
			writeTranslateYTimeline((Animation.TranslateYTimeline)obj);
		} else {
			throw new RuntimeException("Unknown Timeline type: " + obj.getClass().getName());
		}
	}

	private void writeTransformConstraintTimeline (Animation.TransformConstraintTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TransformConstraintTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraintTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("constraintIndex");
		json.writeValue(obj.getConstraintIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeTranslateTimeline (Animation.TranslateTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TranslateTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TranslateTimeline");

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeTranslateXTimeline (Animation.TranslateXTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TranslateXTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TranslateXTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeTranslateYTimeline (Animation.TranslateYTimeline obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TranslateYTimeline-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TranslateYTimeline");

		json.writeName("boneIndex");
		json.writeValue(obj.getBoneIndex());

		json.writeName("frameEntries");
		json.writeValue(obj.getFrameEntries());

		json.writeName("propertyIds");
		json.writeArrayStart();
		for (long item : obj.getPropertyIds()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frames");
		json.writeArrayStart();
		for (float item : obj.getFrames()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("frameCount");
		json.writeValue(obj.getFrameCount());

		json.writeName("duration");
		json.writeValue(obj.getDuration());

		json.writeObjectEnd();
	}

	private void writeAnimationState (AnimationState obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<AnimationState-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AnimationState");

		json.writeName("timeScale");
		json.writeValue(obj.getTimeScale());

		json.writeName("data");
		writeAnimationStateData(obj.getData());

		json.writeName("tracks");
		json.writeArrayStart();
		for (TrackEntry item : obj.getTracks()) {
			writeTrackEntry(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeTrackEntry (AnimationState.TrackEntry obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TrackEntry-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TrackEntry");

		json.writeName("trackIndex");
		json.writeValue(obj.getTrackIndex());

		json.writeName("animation");
		writeAnimation(obj.getAnimation());

		json.writeName("loop");
		json.writeValue(obj.getLoop());

		json.writeName("delay");
		json.writeValue(obj.getDelay());

		json.writeName("trackTime");
		json.writeValue(obj.getTrackTime());

		json.writeName("trackEnd");
		json.writeValue(obj.getTrackEnd());

		json.writeName("trackComplete");
		json.writeValue(obj.getTrackComplete());

		json.writeName("animationStart");
		json.writeValue(obj.getAnimationStart());

		json.writeName("animationEnd");
		json.writeValue(obj.getAnimationEnd());

		json.writeName("animationLast");
		json.writeValue(obj.getAnimationLast());

		json.writeName("animationTime");
		json.writeValue(obj.getAnimationTime());

		json.writeName("timeScale");
		json.writeValue(obj.getTimeScale());

		json.writeName("alpha");
		json.writeValue(obj.getAlpha());

		json.writeName("eventThreshold");
		json.writeValue(obj.getEventThreshold());

		json.writeName("alphaAttachmentThreshold");
		json.writeValue(obj.getAlphaAttachmentThreshold());

		json.writeName("mixAttachmentThreshold");
		json.writeValue(obj.getMixAttachmentThreshold());

		json.writeName("mixDrawOrderThreshold");
		json.writeValue(obj.getMixDrawOrderThreshold());

		json.writeName("next");
		if (obj.getNext() == null) {
			json.writeNull();
		} else {
			writeTrackEntry(obj.getNext());
		}

		json.writeName("previous");
		if (obj.getPrevious() == null) {
			json.writeNull();
		} else {
			writeTrackEntry(obj.getPrevious());
		}

		json.writeName("mixTime");
		json.writeValue(obj.getMixTime());

		json.writeName("mixDuration");
		json.writeValue(obj.getMixDuration());

		json.writeName("additive");
		json.writeValue(obj.getAdditive());

		json.writeName("mixingFrom");
		if (obj.getMixingFrom() == null) {
			json.writeNull();
		} else {
			writeTrackEntry(obj.getMixingFrom());
		}

		json.writeName("mixingTo");
		if (obj.getMixingTo() == null) {
			json.writeNull();
		} else {
			writeTrackEntry(obj.getMixingTo());
		}

		json.writeName("shortestRotation");
		json.writeValue(obj.getShortestRotation());

		json.writeName("reverse");
		json.writeValue(obj.getReverse());

		json.writeObjectEnd();
	}

	private void writeAnimationStateData (AnimationStateData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<AnimationStateData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AnimationStateData");

		json.writeName("skeletonData");
		writeSkeletonData(obj.getSkeletonData());

		json.writeName("defaultMix");
		json.writeValue(obj.getDefaultMix());

		json.writeObjectEnd();
	}

	private void writeAttachment (Attachment obj) {
		if (obj instanceof BoundingBoxAttachment) {
			writeBoundingBoxAttachment((BoundingBoxAttachment)obj);
		} else if (obj instanceof ClippingAttachment) {
			writeClippingAttachment((ClippingAttachment)obj);
		} else if (obj instanceof MeshAttachment) {
			writeMeshAttachment((MeshAttachment)obj);
		} else if (obj instanceof PathAttachment) {
			writePathAttachment((PathAttachment)obj);
		} else if (obj instanceof PointAttachment) {
			writePointAttachment((PointAttachment)obj);
		} else if (obj instanceof RegionAttachment) {
			writeRegionAttachment((RegionAttachment)obj);
		} else {
			throw new RuntimeException("Unknown Attachment type: " + obj.getClass().getName());
		}
	}

	private void writeBone (Bone obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<Bone-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Bone");

		json.writeName("parent");
		if (obj.getParent() == null) {
			json.writeNull();
		} else {
			writeBone(obj.getParent());
		}

		json.writeName("children");
		json.writeArrayStart();
		for (Bone item : obj.getChildren()) {
			writeBone(item);
		}
		json.writeArrayEnd();

		json.writeName("data");
		writeBoneData(obj.getData());

		json.writeName("pose");
		writeBonePose(obj.getPose());

		json.writeName("appliedPose");
		writeBonePose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writeBoneData (BoneData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<BoneData-" + obj.getName() + ">" : "<BoneData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BoneData");

		json.writeName("index");
		json.writeValue(obj.getIndex());

		json.writeName("parent");
		if (obj.getParent() == null) {
			json.writeNull();
		} else {
			writeBoneData(obj.getParent());
		}

		json.writeName("length");
		json.writeValue(obj.getLength());

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("icon");
		json.writeValue(obj.getIcon());

		json.writeName("iconSize");
		json.writeValue(obj.getIconSize());

		json.writeName("iconRotation");
		json.writeValue(obj.getIconRotation());

		json.writeName("visible");
		json.writeValue(obj.getVisible());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writeBonePose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writeBonePose (BonePose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<BonePose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BonePose");

		json.writeName("a");
		json.writeValue(obj.getA());

		json.writeName("b");
		json.writeValue(obj.getB());

		json.writeName("c");
		json.writeValue(obj.getC());

		json.writeName("d");
		json.writeValue(obj.getD());

		json.writeName("worldX");
		json.writeValue(obj.getWorldX());

		json.writeName("worldY");
		json.writeValue(obj.getWorldY());

		json.writeName("worldRotationX");
		json.writeValue(obj.getWorldRotationX());

		json.writeName("worldRotationY");
		json.writeValue(obj.getWorldRotationY());

		json.writeName("worldScaleX");
		json.writeValue(obj.getWorldScaleX());

		json.writeName("worldScaleY");
		json.writeValue(obj.getWorldScaleY());

		json.writeName("x");
		json.writeValue(obj.getX());

		json.writeName("y");
		json.writeValue(obj.getY());

		json.writeName("rotation");
		json.writeValue(obj.getRotation());

		json.writeName("scaleX");
		json.writeValue(obj.getScaleX());

		json.writeName("scaleY");
		json.writeValue(obj.getScaleY());

		json.writeName("shearX");
		json.writeValue(obj.getShearX());

		json.writeName("shearY");
		json.writeValue(obj.getShearY());

		json.writeName("inherit");
		json.writeValue(obj.getInherit().name());

		json.writeObjectEnd();
	}

	private void writeBoundingBoxAttachment (BoundingBoxAttachment obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<BoundingBoxAttachment-" + obj.getName() + ">"
			: "<BoundingBoxAttachment-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BoundingBoxAttachment");

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("bones");
		if (obj.getBones() == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (int item : obj.getBones()) {
				json.writeValue(item);
			}
			json.writeArrayEnd();
		}

		json.writeName("vertices");
		json.writeArrayStart();
		for (float item : obj.getVertices()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("worldVerticesLength");
		json.writeValue(obj.getWorldVerticesLength());

		json.writeName("timelineAttachment");
		if (obj.getTimelineAttachment() == null) {
			json.writeNull();
		} else {
			writeAttachment(obj.getTimelineAttachment());
		}

		json.writeName("timelineSlots");
		json.writeArrayStart();
		for (int item : obj.getTimelineSlots()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("id");
		json.writeValue(obj.getId());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writeClippingAttachment (ClippingAttachment obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<ClippingAttachment-" + obj.getName() + ">"
			: "<ClippingAttachment-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ClippingAttachment");

		json.writeName("endSlot");
		if (obj.getEndSlot() == null) {
			json.writeNull();
		} else {
			writeSlotData(obj.getEndSlot());
		}

		json.writeName("inverse");
		json.writeValue(obj.getInverse());

		json.writeName("convex");
		json.writeValue(obj.getConvex());

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("bones");
		if (obj.getBones() == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (int item : obj.getBones()) {
				json.writeValue(item);
			}
			json.writeArrayEnd();
		}

		json.writeName("vertices");
		json.writeArrayStart();
		for (float item : obj.getVertices()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("worldVerticesLength");
		json.writeValue(obj.getWorldVerticesLength());

		json.writeName("timelineAttachment");
		if (obj.getTimelineAttachment() == null) {
			json.writeNull();
		} else {
			writeAttachment(obj.getTimelineAttachment());
		}

		json.writeName("timelineSlots");
		json.writeArrayStart();
		for (int item : obj.getTimelineSlots()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("id");
		json.writeValue(obj.getId());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writeConstraint (Constraint obj) {
		if (obj instanceof IkConstraint) {
			writeIkConstraint((IkConstraint)obj);
		} else if (obj instanceof PathConstraint) {
			writePathConstraint((PathConstraint)obj);
		} else if (obj instanceof PhysicsConstraint) {
			writePhysicsConstraint((PhysicsConstraint)obj);
		} else if (obj instanceof Slider) {
			writeSlider((Slider)obj);
		} else if (obj instanceof TransformConstraint) {
			writeTransformConstraint((TransformConstraint)obj);
		} else {
			throw new RuntimeException("Unknown Constraint type: " + obj.getClass().getName());
		}
	}

	private void writeConstraintData (ConstraintData obj) {
		if (obj instanceof IkConstraintData) {
			writeIkConstraintData((IkConstraintData)obj);
		} else if (obj instanceof PathConstraintData) {
			writePathConstraintData((PathConstraintData)obj);
		} else if (obj instanceof PhysicsConstraintData) {
			writePhysicsConstraintData((PhysicsConstraintData)obj);
		} else if (obj instanceof SliderData) {
			writeSliderData((SliderData)obj);
		} else if (obj instanceof TransformConstraintData) {
			writeTransformConstraintData((TransformConstraintData)obj);
		} else {
			throw new RuntimeException("Unknown ConstraintData type: " + obj.getClass().getName());
		}
	}

	private void writeEvent (Event obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<Event-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Event");

		json.writeName("int");
		json.writeValue(obj.getInt());

		json.writeName("float");
		json.writeValue(obj.getFloat());

		json.writeName("string");
		json.writeValue(obj.getString());

		json.writeName("volume");
		json.writeValue(obj.getVolume());

		json.writeName("balance");
		json.writeValue(obj.getBalance());

		json.writeName("time");
		json.writeValue(obj.getTime());

		json.writeName("data");
		writeEventData(obj.getData());

		json.writeObjectEnd();
	}

	private void writeEventData (EventData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<EventData-" + obj.getName() + ">" : "<EventData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("EventData");

		Event setup = obj.getSetupPose();
		json.writeName("int");
		json.writeValue(setup.getInt());

		json.writeName("float");
		json.writeValue(setup.getFloat());

		json.writeName("string");
		json.writeValue(setup.getString());

		json.writeName("audioPath");
		json.writeValue(obj.getAudioPath());

		json.writeName("volume");
		json.writeValue(setup.getVolume());

		json.writeName("balance");
		json.writeValue(setup.getBalance());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writeIkConstraint (IkConstraint obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<IkConstraint-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraint");

		json.writeName("bones");
		json.writeArrayStart();
		for (BonePose item : obj.getBones()) {
			writeBonePose(item);
		}
		json.writeArrayEnd();

		json.writeName("target");
		writeBone(obj.getTarget());

		json.writeName("data");
		writeIkConstraintData(obj.getData());

		json.writeName("pose");
		writeIkConstraintPose(obj.getPose());

		json.writeName("appliedPose");
		writeIkConstraintPose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writeIkConstraintData (IkConstraintData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<IkConstraintData-" + obj.getName() + ">"
			: "<IkConstraintData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraintData");

		json.writeName("bones");
		json.writeArrayStart();
		for (BoneData item : obj.getBones()) {
			writeBoneData(item);
		}
		json.writeArrayEnd();

		json.writeName("target");
		writeBoneData(obj.getTarget());

		json.writeName("scaleY");
		json.writeValue(obj.getScaleYMode().name());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writeIkConstraintPose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writeIkConstraintPose (IkConstraintPose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<IkConstraintPose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraintPose");

		json.writeName("mix");
		json.writeValue(obj.getMix());

		json.writeName("softness");
		json.writeValue(obj.getSoftness());

		json.writeName("bendDirection");
		json.writeValue(obj.getBendDirection());

		json.writeName("compress");
		json.writeValue(obj.getCompress());

		json.writeName("stretch");
		json.writeValue(obj.getStretch());

		json.writeObjectEnd();
	}

	private void writeMeshAttachment (MeshAttachment obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<MeshAttachment-" + obj.getName() + ">" : "<MeshAttachment-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("MeshAttachment");

		json.writeName("region");
		Sequence sequence = obj.getSequence();
		TextureRegion region = sequence.getRegion(sequence.getSetupIndex());
		if (region == null) {
			json.writeNull();
		} else {
			writeTextureRegion(region);
		}

		json.writeName("triangles");
		json.writeArrayStart();
		for (short item : obj.getTriangles()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("regionUVs");
		json.writeArrayStart();
		for (float item : obj.getRegionUVs()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("uVs");
		json.writeArrayStart();
		float[] uvs = sequence.getUVs(sequence.getSetupIndex());
		if (uvs != null) {
			for (float item : uvs)
				json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("path");
		json.writeValue(obj.getPath());

		json.writeName("hullLength");
		json.writeValue(obj.getHullLength());

		json.writeName("edges");
		if (obj.getEdges() == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (short item : obj.getEdges()) {
				json.writeValue(item);
			}
			json.writeArrayEnd();
		}

		json.writeName("width");
		json.writeValue(obj.getWidth());

		json.writeName("height");
		json.writeValue(obj.getHeight());

		json.writeName("sequence");
		writeSequence(obj.getSequence());

		json.writeName("sourceMesh");
		if (obj.getSourceMesh() == null) {
			json.writeNull();
		} else {
			writeMeshAttachment(obj.getSourceMesh());
		}

		json.writeName("bones");
		if (obj.getBones() == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (int item : obj.getBones()) {
				json.writeValue(item);
			}
			json.writeArrayEnd();
		}

		json.writeName("vertices");
		json.writeArrayStart();
		for (float item : obj.getVertices()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("worldVerticesLength");
		json.writeValue(obj.getWorldVerticesLength());

		json.writeName("timelineAttachment");
		if (obj.getTimelineAttachment() == null) {
			json.writeNull();
		} else {
			writeAttachment(obj.getTimelineAttachment());
		}

		json.writeName("timelineSlots");
		json.writeArrayStart();
		for (int item : obj.getTimelineSlots()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("id");
		json.writeValue(obj.getId());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writePathAttachment (PathAttachment obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<PathAttachment-" + obj.getName() + ">" : "<PathAttachment-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathAttachment");

		json.writeName("closed");
		json.writeValue(obj.getClosed());

		json.writeName("constantSpeed");
		json.writeValue(obj.getConstantSpeed());

		json.writeName("lengths");
		json.writeArrayStart();
		for (float item : obj.getLengths()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("bones");
		if (obj.getBones() == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (int item : obj.getBones()) {
				json.writeValue(item);
			}
			json.writeArrayEnd();
		}

		json.writeName("vertices");
		json.writeArrayStart();
		for (float item : obj.getVertices()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("worldVerticesLength");
		json.writeValue(obj.getWorldVerticesLength());

		json.writeName("timelineAttachment");
		if (obj.getTimelineAttachment() == null) {
			json.writeNull();
		} else {
			writeAttachment(obj.getTimelineAttachment());
		}

		json.writeName("timelineSlots");
		json.writeArrayStart();
		for (int item : obj.getTimelineSlots()) {
			json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("id");
		json.writeValue(obj.getId());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writePathConstraint (PathConstraint obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PathConstraint-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraint");

		json.writeName("bones");
		json.writeArrayStart();
		for (BonePose item : obj.getBones()) {
			writeBonePose(item);
		}
		json.writeArrayEnd();

		json.writeName("slot");
		writeSlot(obj.getSlot());

		json.writeName("data");
		writePathConstraintData(obj.getData());

		json.writeName("pose");
		writePathConstraintPose(obj.getPose());

		json.writeName("appliedPose");
		writePathConstraintPose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writePathConstraintData (PathConstraintData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<PathConstraintData-" + obj.getName() + ">"
			: "<PathConstraintData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintData");

		json.writeName("bones");
		json.writeArrayStart();
		for (BoneData item : obj.getBones()) {
			writeBoneData(item);
		}
		json.writeArrayEnd();

		json.writeName("slot");
		writeSlotData(obj.getSlot());

		json.writeName("positionMode");
		json.writeValue(obj.getPositionMode().name());

		json.writeName("spacingMode");
		json.writeValue(obj.getSpacingMode().name());

		json.writeName("rotateMode");
		json.writeValue(obj.getRotateMode().name());

		json.writeName("offsetRotation");
		json.writeValue(obj.getOffsetRotation());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writePathConstraintPose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writePathConstraintPose (PathConstraintPose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PathConstraintPose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintPose");

		json.writeName("position");
		json.writeValue(obj.getPosition());

		json.writeName("spacing");
		json.writeValue(obj.getSpacing());

		json.writeName("mixRotate");
		json.writeValue(obj.getMixRotate());

		json.writeName("mixX");
		json.writeValue(obj.getMixX());

		json.writeName("mixY");
		json.writeValue(obj.getMixY());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraint (PhysicsConstraint obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraint-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraint");

		json.writeName("bone");
		writeBonePose(obj.getBone());

		json.writeName("data");
		writePhysicsConstraintData(obj.getData());

		json.writeName("pose");
		writePhysicsConstraintPose(obj.getPose());

		json.writeName("appliedPose");
		writePhysicsConstraintPose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintData (PhysicsConstraintData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<PhysicsConstraintData-" + obj.getName() + ">"
			: "<PhysicsConstraintData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintData");

		json.writeName("bone");
		writeBoneData(obj.getBone());

		json.writeName("step");
		json.writeValue(obj.getStep());

		json.writeName("x");
		json.writeValue(obj.getX());

		json.writeName("y");
		json.writeValue(obj.getY());

		json.writeName("rotate");
		json.writeValue(obj.getRotate());

		json.writeName("scaleX");
		json.writeValue(obj.getScaleX());

		json.writeName("shearX");
		json.writeValue(obj.getShearX());

		json.writeName("limit");
		json.writeValue(obj.getLimit());

		json.writeName("inertiaGlobal");
		json.writeValue(obj.getInertiaGlobal());

		json.writeName("strengthGlobal");
		json.writeValue(obj.getStrengthGlobal());

		json.writeName("dampingGlobal");
		json.writeValue(obj.getDampingGlobal());

		json.writeName("massGlobal");
		json.writeValue(obj.getMassGlobal());

		json.writeName("windGlobal");
		json.writeValue(obj.getWindGlobal());

		json.writeName("gravityGlobal");
		json.writeValue(obj.getGravityGlobal());

		json.writeName("mixGlobal");
		json.writeValue(obj.getMixGlobal());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writePhysicsConstraintPose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writePhysicsConstraintPose (PhysicsConstraintPose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<PhysicsConstraintPose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintPose");

		json.writeName("inertia");
		json.writeValue(obj.getInertia());

		json.writeName("strength");
		json.writeValue(obj.getStrength());

		json.writeName("damping");
		json.writeValue(obj.getDamping());

		json.writeName("massInverse");
		json.writeValue(obj.getMassInverse());

		json.writeName("wind");
		json.writeValue(obj.getWind());

		json.writeName("gravity");
		json.writeValue(obj.getGravity());

		json.writeName("mix");
		json.writeValue(obj.getMix());

		json.writeObjectEnd();
	}

	private void writePointAttachment (PointAttachment obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<PointAttachment-" + obj.getName() + ">"
			: "<PointAttachment-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PointAttachment");

		json.writeName("x");
		json.writeValue(obj.getX());

		json.writeName("y");
		json.writeValue(obj.getY());

		json.writeName("rotation");
		json.writeValue(obj.getRotation());

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writeRegionAttachment (RegionAttachment obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<RegionAttachment-" + obj.getName() + ">"
			: "<RegionAttachment-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RegionAttachment");

		Sequence sequence = obj.getSequence();
		int setupIndex = sequence.getSetupIndex();
		TextureRegion region = sequence.getRegion(setupIndex);

		json.writeName("region");
		if (region == null) {
			json.writeNull();
		} else {
			writeTextureRegion(region);
		}

		json.writeName("offset");
		json.writeArrayStart();
		float[] offset = sequence.getOffsets(setupIndex);
		if (offset != null) {
			for (float item : offset)
				json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("uVs");
		json.writeArrayStart();
		float[] uvs = sequence.getUVs(setupIndex);
		if (uvs != null) {
			for (float item : uvs)
				json.writeValue(item);
		}
		json.writeArrayEnd();

		json.writeName("x");
		json.writeValue(obj.getX());

		json.writeName("y");
		json.writeValue(obj.getY());

		json.writeName("scaleX");
		json.writeValue(obj.getScaleX());

		json.writeName("scaleY");
		json.writeValue(obj.getScaleY());

		json.writeName("rotation");
		json.writeValue(obj.getRotation());

		json.writeName("width");
		json.writeValue(obj.getWidth());

		json.writeName("height");
		json.writeValue(obj.getHeight());

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("path");
		json.writeValue(obj.getPath());

		json.writeName("sequence");
		writeSequence(obj.getSequence());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeObjectEnd();
	}

	private void writeSequence (Sequence obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<Sequence-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Sequence");

		json.writeName("start");
		json.writeValue(obj.getStart());

		json.writeName("digits");
		json.writeValue(obj.getDigits());

		json.writeName("setupIndex");
		json.writeValue(obj.getSetupIndex());

		json.writeName("regions");
		json.writeArrayStart();
		for (TextureRegion item : obj.getRegions()) {
			writeTextureRegion(item);
		}
		json.writeArrayEnd();

		json.writeName("id");
		json.writeValue(obj.getId());

		json.writeObjectEnd();
	}

	private void writeSkeleton (Skeleton obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<Skeleton-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Skeleton");

		json.writeName("data");
		writeSkeletonData(obj.getData());

		json.writeName("bones");
		json.writeArrayStart();
		for (Bone item : obj.getBones()) {
			writeBone(item);
		}
		json.writeArrayEnd();

		json.writeName("updateCache");
		json.writeArrayStart();
		for (Update item : obj.getUpdateCache()) {
			writeUpdate(item);
		}
		json.writeArrayEnd();

		json.writeName("rootBone");
		writeBone(obj.getRootBone());

		json.writeName("slots");
		json.writeArrayStart();
		for (Slot item : obj.getSlots()) {
			writeSlot(item);
		}
		json.writeArrayEnd();

		json.writeName("drawOrderPose");
		json.writeArrayStart();
		for (Slot item : obj.getDrawOrder().getPose())
			writeSlot(item);
		json.writeArrayEnd();

		json.writeName("drawOrderApplied");
		json.writeArrayStart();
		for (Slot item : obj.getDrawOrder().getAppliedPose())
			writeSlot(item);
		json.writeArrayEnd();

		json.writeName("skin");
		if (obj.getSkin() == null) {
			json.writeNull();
		} else {
			writeSkin(obj.getSkin());
		}

		json.writeName("constraints");
		json.writeArrayStart();
		for (Constraint item : obj.getConstraints()) {
			writeConstraint(item);
		}
		json.writeArrayEnd();

		json.writeName("physicsConstraints");
		json.writeArrayStart();
		for (PhysicsConstraint item : obj.getPhysicsConstraints()) {
			writePhysicsConstraint(item);
		}
		json.writeArrayEnd();

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("scaleX");
		json.writeValue(obj.getScaleX());

		json.writeName("scaleY");
		json.writeValue(obj.getScaleY());

		json.writeName("x");
		json.writeValue(obj.getX());

		json.writeName("y");
		json.writeValue(obj.getY());

		json.writeName("windX");
		json.writeValue(obj.getWindX());

		json.writeName("windY");
		json.writeValue(obj.getWindY());

		json.writeName("gravityX");
		json.writeValue(obj.getGravityX());

		json.writeName("gravityY");
		json.writeValue(obj.getGravityY());

		json.writeName("time");
		json.writeValue(obj.getTime());

		json.writeObjectEnd();
	}

	private void writeSkeletonData (SkeletonData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<SkeletonData-" + obj.getName() + ">" : "<SkeletonData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SkeletonData");

		json.writeName("bones");
		json.writeArrayStart();
		for (BoneData item : obj.getBones()) {
			writeBoneData(item);
		}
		json.writeArrayEnd();

		json.writeName("slots");
		json.writeArrayStart();
		for (SlotData item : obj.getSlots()) {
			writeSlotData(item);
		}
		json.writeArrayEnd();

		json.writeName("defaultSkin");
		if (obj.getDefaultSkin() == null) {
			json.writeNull();
		} else {
			writeSkin(obj.getDefaultSkin());
		}

		json.writeName("skins");
		json.writeArrayStart();
		for (Skin item : obj.getSkins()) {
			writeSkin(item);
		}
		json.writeArrayEnd();

		json.writeName("events");
		json.writeArrayStart();
		for (EventData item : obj.getEvents()) {
			writeEventData(item);
		}
		json.writeArrayEnd();

		json.writeName("animations");
		json.writeArrayStart();
		for (Animation item : obj.getAnimations()) {
			writeAnimation(item);
		}
		json.writeArrayEnd();

		json.writeName("constraints");
		json.writeArrayStart();
		for (ConstraintData item : obj.getConstraints()) {
			writeConstraintData(item);
		}
		json.writeArrayEnd();

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("x");
		json.writeValue(obj.getX());

		json.writeName("y");
		json.writeValue(obj.getY());

		json.writeName("width");
		json.writeValue(obj.getWidth());

		json.writeName("height");
		json.writeValue(obj.getHeight());

		json.writeName("referenceScale");
		json.writeValue(obj.getReferenceScale());

		json.writeName("version");
		json.writeValue(obj.getVersion());

		json.writeName("hash");
		json.writeValue(obj.getHash());

		json.writeName("imagesPath");
		json.writeValue(obj.getImagesPath());

		json.writeName("audioPath");
		json.writeValue(obj.getAudioPath());

		json.writeName("fps");
		json.writeValue(obj.getFps());

		json.writeObjectEnd();
	}

	private void writeSkin (Skin obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<Skin-" + obj.getName() + ">" : "<Skin-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Skin");

		json.writeName("attachments");
		var sortedAttachments = new Array<SkinEntry>(obj.getAttachments());
		sortedAttachments.sort( (a, b) -> Integer.compare(a.getSlotIndex(), b.getSlotIndex()));
		json.writeArrayStart();
		for (SkinEntry item : sortedAttachments) {
			writeSkinEntry(item);
		}
		json.writeArrayEnd();

		json.writeName("bones");
		json.writeArrayStart();
		for (BoneData item : obj.getBones()) {
			writeBoneData(item);
		}
		json.writeArrayEnd();

		json.writeName("constraints");
		json.writeArrayStart();
		for (ConstraintData item : obj.getConstraints()) {
			writeConstraintData(item);
		}
		json.writeArrayEnd();

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeObjectEnd();
	}

	private void writeSkinEntry (Skin.SkinEntry obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getPlaceholder() != null ? "<SkinEntry-" + obj.getPlaceholder() + ">"
			: "<SkinEntry-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SkinEntry");

		json.writeName("slotIndex");
		json.writeValue(obj.getSlotIndex());

		json.writeName("name");
		json.writeValue(obj.getPlaceholder());

		json.writeName("attachment");
		writeAttachment(obj.getAttachment());

		json.writeObjectEnd();
	}

	private void writeSlider (Slider obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<Slider-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Slider");

		json.writeName("bone");
		writeBone(obj.getBone());

		json.writeName("data");
		writeSliderData(obj.getData());

		json.writeName("pose");
		writeSliderPose(obj.getPose());

		json.writeName("appliedPose");
		writeSliderPose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writeSliderData (SliderData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<SliderData-" + obj.getName() + ">" : "<SliderData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderData");

		json.writeName("animation");
		writeAnimation(obj.getAnimation());

		json.writeName("additive");
		json.writeValue(obj.getAdditive());

		json.writeName("loop");
		json.writeValue(obj.getLoop());

		json.writeName("bone");
		if (obj.getBone() == null) {
			json.writeNull();
		} else {
			writeBoneData(obj.getBone());
		}

		json.writeName("property");
		if (obj.getProperty() == null) {
			json.writeNull();
		} else {
			writeFromProperty(obj.getProperty());
		}

		json.writeName("offset");
		json.writeValue(obj.getOffset());

		json.writeName("scale");
		json.writeValue(obj.getScale());

		json.writeName("max");
		json.writeValue(obj.getMax());

		json.writeName("local");
		json.writeValue(obj.getLocal());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writeSliderPose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writeSliderPose (SliderPose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<SliderPose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderPose");

		json.writeName("time");
		json.writeValue(obj.getTime());

		json.writeName("mix");
		json.writeValue(obj.getMix());

		json.writeObjectEnd();
	}

	private void writeSlot (Slot obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<Slot-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Slot");

		json.writeName("bone");
		writeBone(obj.getBone());

		json.writeName("data");
		writeSlotData(obj.getData());

		json.writeName("pose");
		writeSlotPose(obj.getPose());

		json.writeName("appliedPose");
		writeSlotPose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writeSlotData (SlotData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<SlotData-" + obj.getName() + ">" : "<SlotData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SlotData");

		json.writeName("index");
		json.writeValue(obj.getIndex());

		json.writeName("boneData");
		writeBoneData(obj.getBoneData());

		json.writeName("attachmentName");
		json.writeValue(obj.getAttachmentName());

		json.writeName("blendMode");
		json.writeValue(obj.getBlendMode().name());

		json.writeName("visible");
		json.writeValue(obj.getVisible());

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writeSlotPose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writeSlotPose (SlotPose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<SlotPose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SlotPose");

		json.writeName("color");
		writeColor(obj.getColor());

		json.writeName("darkColor");
		if (obj.getDarkColor() == null) {
			json.writeNull();
		} else {
			writeColor(obj.getDarkColor());
		}

		json.writeName("attachment");
		if (obj.getAttachment() == null) {
			json.writeNull();
		} else {
			writeAttachment(obj.getAttachment());
		}

		json.writeName("sequenceIndex");
		json.writeValue(obj.getSequenceIndex());

		json.writeName("deform");
		writeFloatArray(obj.getDeform());

		json.writeObjectEnd();
	}

	private void writeTransformConstraint (TransformConstraint obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TransformConstraint-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraint");

		json.writeName("bones");
		json.writeArrayStart();
		for (BonePose item : obj.getBones()) {
			writeBonePose(item);
		}
		json.writeArrayEnd();

		json.writeName("source");
		writeBone(obj.getSource());

		json.writeName("data");
		writeTransformConstraintData(obj.getData());

		json.writeName("pose");
		writeTransformConstraintPose(obj.getPose());

		json.writeName("appliedPose");
		writeTransformConstraintPose(obj.getAppliedPose());

		json.writeObjectEnd();
	}

	private void writeTransformConstraintData (TransformConstraintData obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = obj.getName() != null ? "<TransformConstraintData-" + obj.getName() + ">"
			: "<TransformConstraintData-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraintData");

		json.writeName("bones");
		json.writeArrayStart();
		for (BoneData item : obj.getBones()) {
			writeBoneData(item);
		}
		json.writeArrayEnd();

		json.writeName("source");
		writeBoneData(obj.getSource());

		json.writeName("offsetRotation");
		json.writeValue(obj.getOffsetRotation());

		json.writeName("offsetX");
		json.writeValue(obj.getOffsetX());

		json.writeName("offsetY");
		json.writeValue(obj.getOffsetY());

		json.writeName("offsetScaleX");
		json.writeValue(obj.getOffsetScaleX());

		json.writeName("offsetScaleY");
		json.writeValue(obj.getOffsetScaleY());

		json.writeName("offsetShearY");
		json.writeValue(obj.getOffsetShearY());

		json.writeName("localSource");
		json.writeValue(obj.getLocalSource());

		json.writeName("localTarget");
		json.writeValue(obj.getLocalTarget());

		json.writeName("additive");
		json.writeValue(obj.getAdditive());

		json.writeName("clamp");
		json.writeValue(obj.getClamp());

		json.writeName("properties");
		json.writeArrayStart();
		for (FromProperty item : obj.getProperties()) {
			writeFromProperty(item);
		}
		json.writeArrayEnd();

		json.writeName("name");
		json.writeValue(obj.getName());

		json.writeName("setupPose");
		writeTransformConstraintPose(obj.getSetupPose());

		json.writeName("skinRequired");
		json.writeValue(obj.getSkinRequired());

		json.writeObjectEnd();
	}

	private void writeFromProperty (TransformConstraintData.FromProperty obj) {
		if (obj instanceof TransformConstraintData.FromRotate) {
			writeFromRotate((TransformConstraintData.FromRotate)obj);
		} else if (obj instanceof TransformConstraintData.FromScaleX) {
			writeFromScaleX((TransformConstraintData.FromScaleX)obj);
		} else if (obj instanceof TransformConstraintData.FromScaleY) {
			writeFromScaleY((TransformConstraintData.FromScaleY)obj);
		} else if (obj instanceof TransformConstraintData.FromShearY) {
			writeFromShearY((TransformConstraintData.FromShearY)obj);
		} else if (obj instanceof TransformConstraintData.FromX) {
			writeFromX((TransformConstraintData.FromX)obj);
		} else if (obj instanceof TransformConstraintData.FromY) {
			writeFromY((TransformConstraintData.FromY)obj);
		} else {
			throw new RuntimeException("Unknown FromProperty type: " + obj.getClass().getName());
		}
	}

	private void writeFromRotate (TransformConstraintData.FromRotate obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<FromRotate-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromRotate");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("to");
		json.writeArrayStart();
		for (ToProperty item : obj.to) {
			writeToProperty(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeFromScaleX (TransformConstraintData.FromScaleX obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<FromScaleX-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromScaleX");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("to");
		json.writeArrayStart();
		for (ToProperty item : obj.to) {
			writeToProperty(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeFromScaleY (TransformConstraintData.FromScaleY obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<FromScaleY-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromScaleY");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("to");
		json.writeArrayStart();
		for (ToProperty item : obj.to) {
			writeToProperty(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeFromShearY (TransformConstraintData.FromShearY obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<FromShearY-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromShearY");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("to");
		json.writeArrayStart();
		for (ToProperty item : obj.to) {
			writeToProperty(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeFromX (TransformConstraintData.FromX obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<FromX-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromX");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("to");
		json.writeArrayStart();
		for (ToProperty item : obj.to) {
			writeToProperty(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeFromY (TransformConstraintData.FromY obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<FromY-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromY");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("to");
		json.writeArrayStart();
		for (ToProperty item : obj.to) {
			writeToProperty(item);
		}
		json.writeArrayEnd();

		json.writeObjectEnd();
	}

	private void writeToProperty (TransformConstraintData.ToProperty obj) {
		if (obj instanceof TransformConstraintData.ToRotate) {
			writeToRotate((TransformConstraintData.ToRotate)obj);
		} else if (obj instanceof TransformConstraintData.ToScaleX) {
			writeToScaleX((TransformConstraintData.ToScaleX)obj);
		} else if (obj instanceof TransformConstraintData.ToScaleY) {
			writeToScaleY((TransformConstraintData.ToScaleY)obj);
		} else if (obj instanceof TransformConstraintData.ToShearY) {
			writeToShearY((TransformConstraintData.ToShearY)obj);
		} else if (obj instanceof TransformConstraintData.ToX) {
			writeToX((TransformConstraintData.ToX)obj);
		} else if (obj instanceof TransformConstraintData.ToY) {
			writeToY((TransformConstraintData.ToY)obj);
		} else {
			throw new RuntimeException("Unknown ToProperty type: " + obj.getClass().getName());
		}
	}

	private void writeToRotate (TransformConstraintData.ToRotate obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ToRotate-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToRotate");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("max");
		json.writeValue(obj.max);

		json.writeName("scale");
		json.writeValue(obj.scale);

		json.writeObjectEnd();
	}

	private void writeToScaleX (TransformConstraintData.ToScaleX obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ToScaleX-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToScaleX");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("max");
		json.writeValue(obj.max);

		json.writeName("scale");
		json.writeValue(obj.scale);

		json.writeObjectEnd();
	}

	private void writeToScaleY (TransformConstraintData.ToScaleY obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ToScaleY-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToScaleY");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("max");
		json.writeValue(obj.max);

		json.writeName("scale");
		json.writeValue(obj.scale);

		json.writeObjectEnd();
	}

	private void writeToShearY (TransformConstraintData.ToShearY obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ToShearY-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToShearY");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("max");
		json.writeValue(obj.max);

		json.writeName("scale");
		json.writeValue(obj.scale);

		json.writeObjectEnd();
	}

	private void writeToX (TransformConstraintData.ToX obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ToX-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToX");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("max");
		json.writeValue(obj.max);

		json.writeName("scale");
		json.writeValue(obj.scale);

		json.writeObjectEnd();
	}

	private void writeToY (TransformConstraintData.ToY obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<ToY-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToY");

		json.writeName("offset");
		json.writeValue(obj.offset);

		json.writeName("max");
		json.writeValue(obj.max);

		json.writeName("scale");
		json.writeValue(obj.scale);

		json.writeObjectEnd();
	}

	private void writeTransformConstraintPose (TransformConstraintPose obj) {
		if (visitedObjects.containsKey(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}
		String refString = "<TransformConstraintPose-" + (nextId++) + ">";
		visitedObjects.put(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraintPose");

		json.writeName("mixRotate");
		json.writeValue(obj.getMixRotate());

		json.writeName("mixX");
		json.writeValue(obj.getMixX());

		json.writeName("mixY");
		json.writeValue(obj.getMixY());

		json.writeName("mixScaleX");
		json.writeValue(obj.getMixScaleX());

		json.writeName("mixScaleY");
		json.writeValue(obj.getMixScaleY());

		json.writeName("mixShearY");
		json.writeValue(obj.getMixShearY());

		json.writeObjectEnd();
	}

	private void writeUpdate (Update obj) {
		if (obj instanceof BonePose) {
			writeBonePose((BonePose)obj);
		} else if (obj instanceof IkConstraint) {
			writeIkConstraint((IkConstraint)obj);
		} else if (obj instanceof PathConstraint) {
			writePathConstraint((PathConstraint)obj);
		} else if (obj instanceof PhysicsConstraint) {
			writePhysicsConstraint((PhysicsConstraint)obj);
		} else if (obj instanceof Slider) {
			writeSlider((Slider)obj);
		} else if (obj instanceof TransformConstraint) {
			writeTransformConstraint((TransformConstraint)obj);
		} else {
			throw new RuntimeException("Unknown Update type: " + obj.getClass().getName());
		}
	}

	private void writeVertexAttachment (VertexAttachment obj) {
		if (obj instanceof BoundingBoxAttachment) {
			writeBoundingBoxAttachment((BoundingBoxAttachment)obj);
		} else if (obj instanceof ClippingAttachment) {
			writeClippingAttachment((ClippingAttachment)obj);
		} else if (obj instanceof MeshAttachment) {
			writeMeshAttachment((MeshAttachment)obj);
		} else if (obj instanceof PathAttachment) {
			writePathAttachment((PathAttachment)obj);
		} else {
			throw new RuntimeException("Unknown VertexAttachment type: " + obj.getClass().getName());
		}
	}

	private void writeColor (Color obj) {
		if (obj == null) {
			json.writeNull();
		} else {
			json.writeObjectStart();
			json.writeName("r");
			json.writeValue(obj.r);
			json.writeName("g");
			json.writeValue(obj.g);
			json.writeName("b");
			json.writeValue(obj.b);
			json.writeName("a");
			json.writeValue(obj.a);
			json.writeObjectEnd();
		}
	}

	private void writeTextureRegion (TextureRegion obj) {
		if (obj == null) {
			json.writeNull();
		} else {
			json.writeObjectStart();
			json.writeName("u");
			json.writeValue(obj.getU());
			json.writeName("v");
			json.writeValue(obj.getV());
			json.writeName("u2");
			json.writeValue(obj.getU2());
			json.writeName("v2");
			json.writeValue(obj.getV2());
			json.writeName("width");
			json.writeValue(obj.getRegionWidth());
			json.writeName("height");
			json.writeValue(obj.getRegionHeight());
			json.writeObjectEnd();
		}
	}

	private void writeIntArray (IntArray obj) {
		if (obj == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (int i = 0; i < obj.size; i++) {
				json.writeValue(obj.get(i));
			}
			json.writeArrayEnd();
		}
	}

	private void writeFloatArray (FloatArray obj) {
		if (obj == null) {
			json.writeNull();
		} else {
			json.writeArrayStart();
			for (int i = 0; i < obj.size; i++) {
				json.writeValue(obj.get(i));
			}
			json.writeArrayEnd();
		}
	}
}
