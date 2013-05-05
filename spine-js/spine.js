
var spine = {};

spine.BoneData = function (name, parent) {
	this.name = name;
	this.parent = parent;
};
spine.BoneData.prototype = {
	length: 0,
	x: 0, y: 0,
	rotation: 0,
	scaleX: 1, scaleY: 1
};

spine.SlotData = function (name, boneData) {
	this.name = name;
	this.boneData = boneData;
};
spine.SlotData.prototype = {
	r: 1, g: 1, b: 1, a: 1,
	attachmentName: null
};

spine.Bone = function (boneData, parent) {
	this.data = boneData;
	this.parent = parent;
};
spine.Bone.yDown = false;
spine.Bone.prototype = {
	x: 0, y: 0,
	rotation: 0,
	scaleX: 1, scaleY: 1,
	m00: 0, m01: 0, worldX: 0, // a b x
	m10: 0, m11: 0, worldY: 0, // c d y
	worldRotation: 0,
	worldScaleX: 1, worldScaleY: 1,
	updateWorldTransform: function (flipX, flipY) {
		var parent = this.parent;
		if (parent != null) {
			this.worldX = this.x * parent.m00 + this.y * parent.m01 + parent.worldX;
			this.worldY = this.x * parent.m10 + this.y * parent.m11 + parent.worldY;
			this.worldScaleX = parent.worldScaleX * this.scaleX;
			this.worldScaleY = parent.worldScaleY * this.scaleY;
			this.worldRotation = parent.worldRotation + this.rotation;
		} else {
			this.worldX = this.x;
			this.worldY = this.y;
			this.worldScaleX = this.scaleX;
			this.worldScaleY = this.scaleY;
			this.worldRotation = this.rotation;
		}
		var cos = worldRotation * Math.PI / 180;
		var sin = worldRotation * Math.PI / 180;
		this.m00 = cos * this.worldScaleX;
		this.m10 = sin * this.worldScaleX;
		this.m01 = -sin * this.worldScaleY;
		this.m11 = cos * this.worldScaleY;
		if (flipX) {
			this.m00 = -this.m00;
			this.m01 = -this.m01;
		}
		if (flipY) {
			this.m10 = -this.m10;
			this.m11 = -this.m11;
		}
		if (spine.Bone.yDown) {
			this.m10 = -this.m10;
			this.m11 = -this.m11;
		}
	},
	setToSetupPose: function () {
		var data = this.data;
		this.x = data.x;
		this.y = data.y;
		this.rotation = data.rotation;
		this.scaleX = data.scaleX;
		this.scaleY = data.scaleY;
	}
};

spine.Slot = function (slotData, skeleton, bone) {
	this.data = slotData;
	this.skeleton = skeleton;
	this.bone = bone;
};
spine.Slot.prototype = {
	r: 1, g: 1, b: 1, a: 1,
	_attachmentTime: 0,
	attachment: null,
	setAttachment: function (attachment) {
		this.attachment = attachment;
		this._attachmentTime = this.skeleton.time;
	},
	setAttachmentTime: function (time) {
		this._attachmentTime = this.skeleton.time - time;
	},
	getAttachmentTime: function () {
		return this.skeleton.time - this._attachmentTime;
	},
	setToSetupPose: function () {
		var data = this.data;
		this.r = data.r;
		this.g = data.g;
		this.b = data.b;
		this.a = data.a;
		
		var slots = this.skeleton.slots;
		for (var i = 0, n = slots.length; i < n; i++) {
			if (slots[i] == this) {
				this.setAttachment(!data.attachmentName ? null : this.skeleton.getAttachment(i, data.attachmentName));
				break;
			}
		}
	}
};

spine.Skin = function (name) {
	this.name = name;
	this.attachments = {};
};
spine.Skin.prototype = {
	addAttachment: function (slotIndex, name, attachment) {
		attachments[slotIndex + ":" + name] = attachment;
	},
	getAttachment: function (slotIndex, name) {
		return attachments[slotIndex + ":" + name];
	},
	_attachAll: function (skeleton, oldSkin) {
		for (var key in oldSkin.attachments) {
			var colon = key.indexOf(":");
			var slotIndex = parseInt(key.substring(0, colon));
			var name = key.substring(colon + 1);
			var slot = skeleton.slots[slotIndex];
			if (slot.attachment.name == name) {
				var attachment = this.getAttachment(slotIndex, name);
				if (attachment) slot.setAttachment(attachment);
			}
		}
	}
};

spine.Animation = function (name, timelines, duration) {
	this.name = name;
	this.timelines = timelines;
	this.duration = duration;
}
spine.Animation.prototype = {
	apply: function (skeleton, time, loop) {
		if (loop && this.duration != 0) time %= this.duration;
		for (var i = 0, n = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, time, 1);
	},
	mix: function (skeleton, time, loop, alpha) {
		if (loop && this.duration != 0) time %= this.duration;
		for (var i = 0, n = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, time, alpha);
	}
};

spine.binarySearch = function (values, target, step) {
	var low = 0;
	var high = Math.floor(values.length / step) - 2;
	if (high == 0) return step;
	var current = high >>> 1;
	while (true) {
		if (values[(current + 1) * step] <= target)
			low = current + 1;
		else
			high = current;
		if (low == high) return (low + 1) * step;
		current = (low + high) >>> 1;
	}
}
spine.linearSearch = function (values, target, step) {
	for (var i = 0, last = values.length - step; i <= last; i += step)
		if (values[i] > target) return i;
	return -1;
}

spine.Curves = function (frameCount) {
	this.curves = []; // dfx, dfy, ddfx, ddfy, dddfx, dddfy, ...
	this.curves.length = (frameCount - 1) * 6;
}
spine.Curves.prototype = {
	setLinear: function (frameIndex) {
		this.curves[frameIndex * 6] = 0/*LINEAR*/;
	},
	setStepped: function (frameIndex) {
		this.curves[frameIndex * 6] = -1/*STEPPED*/;
	},
	/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
	 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
	 * the difference between the keyframe's values. */
	setCurve: function (frameIndex, cx1, cy1, cx2, cy2) {
		var subdiv_step = 1 / 10/*BEZIER_SEGMENTS*/;
		var subdiv_step2 = subdiv_step * subdiv_step;
		var subdiv_step3 = subdiv_step2 * subdiv_step;
		var pre1 = 3 * subdiv_step;
		var pre2 = 3 * subdiv_step2;
		var pre4 = 6 * subdiv_step2;
		var pre5 = 6 * subdiv_step3;
		var tmp1x = -cx1 * 2 + cx2;
		var tmp1y = -cy1 * 2 + cy2;
		var tmp2x = (cx1 - cx2) * 3 + 1;
		var tmp2y = (cy1 - cy2) * 3 + 1;
		var i = frameIndex * 6;
		var curves = this.curves;
		curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
		curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
		curves[i + 2] = tmp1x * pre4 + tmp2x * pre5;
		curves[i + 3] = tmp1y * pre4 + tmp2y * pre5;
		curves[i + 4] = tmp2x * pre5;
		curves[i + 5] = tmp2y * pre5;
	},
	getCurvePercent: function (frameIndex, percent) {
		percent = percent < 0 ? 0 : (percent > 1 ? 1 : percent);
		var curveIndex = frameIndex * 6;
		var curves = this.curves;
		var dfx = curves[curveIndex];
		if (dfx == 0/*LINEAR*/) return percent;
		if (dfx == -1/*STEPPED*/) return 0;
		var dfy = curves[curveIndex + 1];
		var ddfx = curves[curveIndex + 2];
		var ddfy = curves[curveIndex + 3];
		var dddfx = curves[curveIndex + 4];
		var dddfy = curves[curveIndex + 5];
		var x = dfx, y = dfy;
		var i = 10/*BEZIER_SEGMENTS*/ - 2;
		while (true) {
			if (x >= percent) {
				var lastX = x - dfx;
				var lastY = y - dfy;
				return lastY + (y - lastY) * (percent - lastX) / (x - lastX);
			}
			if (i == 0) break;
			i--;
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
			x += dfx;
			y += dfy;
		}
		return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
	}
}

spine.RotateTimeline = function (frameCount) {
	this.curves = new Curves(frameCount);
	this.frames = []; // time, angle, ...
	this.frames.length = frameCount * 2;
}
spine.RotateTimeline.prototype = {
	boneIndex: 0,
	getFrameCount: function () {
		return this.frames.length / 2;
	},
	setFrame: function (frameIndex, time, angle) {
		frameIndex *= 2;
		this.frames[frameIndex] = time;
		this.frames[frameIndex + 1] = angle;
	},
	apply: function (skeleton, time, alpha) {
		var frames = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var bone = skeleton.bones[this.boneIndex];

		if (time >= frames[frames.length - 2]) { // Time is after last frame.
			var amount = bone.data.rotation + frames[frames.length - 1] - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex = spine.binarySearch(frames, time, 2);
		var lastFrameValue = frames[frameIndex - 1];
		var frameTime = frames[frameIndex];
		var percent = 1 - (time - frameTime) / (frames[frameIndex - 2/*LAST_FRAME_TIME*/] - frameTime);
		percent = this.curves.getCurvePercent(frameIndex / 2 - 1, percent);

		var amount = frames[frameIndex + 1/*FRAME_VALUE*/] - lastFrameValue;
		while (amount > 180)
			amount -= 360;
		while (amount < -180)
			amount += 360;
		amount = bone.data.rotation + (lastFrameValue + amount * percent) - bone.rotation;
		while (amount > 180)
			amount -= 360;
		while (amount < -180)
			amount += 360;
		bone.rotation += amount * alpha;
	}
}

spine.TranslateTimeline = function (frameCount) {
	this.curves = new Curves(frameCount);
	this.frames = []; // time, x, y, ...
	this.frames.length = frameCount * 3;
}
spine.TranslateTimeline.prototype = {
	boneIndex: 0,
	getFrameCount: function () {
		return this.frames.length / 3;
	},
	setFrame: function (frameIndex, time, x, y) {
		frameIndex *= 3;
		this.frames[frameIndex] = time;
		this.frames[frameIndex + 1] = x;
		this.frames[frameIndex + 2] = y;
	},
	apply: function (skeleton, time, alpha) {
		var frames = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var bone = skeleton.bones[this.boneIndex];

		if (time >= frames[frames.length - 3]) { // Time is after last frame.
			bone.x += (bone.data.x + frames[frames.length - 2] - bone.x) * alpha;
			bone.y += (bone.data.y + frames[frames.length - 1] - bone.y) * alpha;
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex = spine.binarySearch(frames, time, 3);
		var lastFrameX = frames[frameIndex - 2];
		var lastFrameY = frames[frameIndex - 1];
		var frameTime = frames[frameIndex];
		var percent = 1 - (time - frameTime) / (frames[frameIndex + -3/*LAST_FRAME_TIME*/] - frameTime);
		percent = this.curves.getCurvePercent(frameIndex / 3 - 1, percent);

		bone.x += (bone.data.x + lastFrameX + (frames[frameIndex + 1/*FRAME_X*/] - lastFrameX) * percent - bone.x) * alpha;
		bone.y += (bone.data.y + lastFrameY + (frames[frameIndex + 2/*FRAME_Y*/] - lastFrameY) * percent - bone.y) * alpha;
	}
}

spine.ScaleTimeline = function (frameCount) {
	this.curves = new Curves(frameCount);
	this.frames = []; // time, x, y, ...
	this.frames.length = frameCount * 3;
}
spine.ScaleTimeline.prototype = {
	boneIndex: 0,
	getFrameCount: function () {
		return this.frames.length / 3;
	},
	setFrame: function (frameIndex, time, x, y) {
		frameIndex *= 3;
		this.frames[frameIndex] = time;
		this.frames[frameIndex + 1] = x;
		this.frames[frameIndex + 2] = y;
	},
	apply: function (skeleton, time, alpha) {
		var frames = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var bone = skeleton.bones[this.boneIndex];

		if (time >= frames[frames.length - 3]) { // Time is after last frame.
			bone.scaleX += (bone.data.scaleX - 1 + frames[frames.length - 2] - bone.scaleX) * alpha;
			bone.scaleY += (bone.data.scaleY - 1 + frames[frames.length - 1] - bone.scaleY) * alpha;
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex = spine.binarySearch(frames, time, 3);
		var lastFrameX = frames[frameIndex - 2];
		var lastFrameY = frames[frameIndex - 1];
		var frameTime = frames[frameIndex];
		var percent = 1 - (time - frameTime) / (frames[frameIndex + -3/*LAST_FRAME_TIME*/] - frameTime);
		percent = this.curves.getCurvePercent(frameIndex / 3 - 1, percent);

		bone.scaleX += (bone.data.scaleX - 1 + lastFrameX + (frames[frameIndex + 1/*FRAME_X*/] - lastFrameX) * percent - bone.scaleX) * alpha;
		bone.scaleY += (bone.data.scaleY - 1 + lastFrameY + (frames[frameIndex + 2/*FRAME_Y*/] - lastFrameY) * percent - bone.scaleY) * alpha;
	}
}

spine.ColorTimeline = function (frameCount) {
	this.curves = new Curves(frameCount);
	this.frames = []; // time, r, g, b, a, ...
	this.frames.length = frameCount * 5;
}
spine.ColorTimeline.prototype = {
	slotIndex: 0,
	getFrameCount: function () {
		return this.frames.length / 2;
	},
	setFrame: function (frameIndex, time, x, y) {
		frameIndex *= 5;
		this.frames[frameIndex] = time;
		this.frames[frameIndex + 1] = r;
		this.frames[frameIndex + 2] = g;
		this.frames[frameIndex + 3] = b;
		this.frames[frameIndex + 4] = a;
	},
	apply: function (skeleton, time, alpha) {
		var frames = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var slot = skeleton.slots[this.slotIndex];

		if (time >= frames[frames.length - 5]) { // Time is after last frame.
			var i = frames.length - 1;
			slot.r = frames[i - 3];
			slot.g = frames[i - 2];
			slot.b = frames[i - 1];
			slot.a = frames[i];
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex = spine.binarySearch(frames, time, 5);
		var lastFrameR = frames[frameIndex - 4];
		var lastFrameG = frames[frameIndex - 3];
		var lastFrameB = frames[frameIndex - 2];
		var lastFrameA = frames[frameIndex - 1];
		var frameTime = frames[frameIndex];
		var percent = 1 - (time - frameTime) / (frames[frameIndex - 5/*LAST_FRAME_TIME*/] - frameTime);
		percent = this.curves.getCurvePercent(frameIndex / 5 - 1, percent);

		var r = lastFrameR + (frames[frameIndex + 1/*FRAME_R*/] - lastFrameR) * percent;
		var g = lastFrameG + (frames[frameIndex + 2/*FRAME_G*/] - lastFrameG) * percent;
		var b = lastFrameB + (frames[frameIndex + 3/*FRAME_B*/] - lastFrameB) * percent;
		var a = lastFrameA + (frames[frameIndex + 4/*FRAME_A*/] - lastFrameA) * percent;
		if (alpha < 1) {
			slot.r += (r - slot.r) * alpha;
			slot.g += (g - slot.g) * alpha;
			slot.b += (b - slot.b) * alpha;
			slot.a += (a - slot.a) * alpha;
		} else {
			slot.r = r;
			slot.g = g;
			slot.b = b;
			slot.a = a;
		}
	}
}

spine.AttachmentTimeline = function (frameCount) {
	this.curves = new Curves(frameCount);
	this.frames = []; // time, ...
	this.frames.length = frameCount;
	this.attachmentNames = []; // time, ...
	this.attachmentNames.length = frameCount;
}
spine.AttachmentTimeline.prototype = {
	slotIndex: 0,
	getFrameCount: function () {
		return this.frames.length / 2;
	},
	setFrame: function (frameIndex, time, attachmentName) {
		this.frames[frameIndex] = time;
		this.attachmentNames[frameIndex] = attachmentName;
	},
	apply: function (skeleton, time, alpha) {
		var frames = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var frameIndex;
		if (time >= frames[frames.length - 1]) // Time is after last frame.
			frameIndex = frames.length - 1;
		else
			frameIndex = spine.binarySearch(frames, time, 1) - 1;

		var attachmentName = this.attachmentNames[frameIndex];
		skeleton.slots[this.slotIndex].setAttachment(!attachmentName ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
	}
}

spine.SkeletonData = function () {
	this.bones = [];
	this.slots = [];
	this.skins = [];
	this.animations = [];
}
spine.SkeletonData.prototype = {
	defaultSkin: null,
	/** @return May be null. */
	findBone: function (boneName) {
		var bones = this.bones;
		for (var i = 0, n = bones.length; i < n; i++)
			if (bones[i] == boneName) return bones[i];
		return null;
	},
	/** @return -1 if the bone was not found. */
	findBoneIndex: function (boneName) {
		var bones = this.bones;
		for (var i = 0, n = bones.length; i < n; i++)
			if (bones[i] == boneName) return i;
		return -1;
	},
	/** @return May be null. */
	findSlot: function (slotName) {
		var slots = this.slots;
		for (var i = 0, n = slots.length; i < n; i++) {
			if (slots[i].name == slotName) return slot[i];
		}
		return null;
	},
	/** @return -1 if the bone was not found. */
	findSlotIndex: function (slotName) {
		var slots = this.slots;
		for (var i = 0, n = slots.length; i < n; i++)
			if (slots.get(i).name == slotName) return i;
		return -1;
	},
	/** @return May be null. */
	findSkin: function (skinName) {
		var skins = this.skins;
		for (var i = 0, n = skins.length; i < n; i++)
			if (skins[i].name == skinName) return skins[i];
		return null;
	},
	/** @return May be null. */
	findAnimation: function (animationName) {
		var animations = this.animations;
		for (var i = 0, n = animations.length; i < n; i++)
			if (animations[i].name == animationName) return animations[i];
		return null;
	}
}

spine.Skeleton = function (skeletonData) {
	this.data = skeletonData;

	this.bones = [];
	for (var i = 0, n = skeletonData.bones.length; i < n; i++) {
		var boneData = skeletonData.bones[i];
		var parent = !boneData.parent ? null : bones[skeletonData.bones.indexOf(boneData.parent)];
		bones.push(new Bone(boneData, parent));
	}

	this.slots = [];
	this.drawOrder = [];
	for (var i = 0, n = skeletonData.slots.length; i < n; i++) {
		var slotData = skeletonData.slots[i];
		var bone = this.bones[skeletonData.bones.indexOf(slotData.boneData)];
		var slot = new Slot(slotData, this, bone);
		slots.push(slot);
		drawOrder.push(slot);
	}
}
spine.Skeleton.prototype = {
	skin: null,
	r: 1, g: 1, b: 1, a: 1,
	time: 0,
	flipX: false, flipY: false,
	/** Updates the world transform for each bone. */
	updateWorldTransform: function () {
		var flipX = this.flipX;
		var flipY = this.flipY;
		var bones = this.bones;
		for (var i = 0, n = bones.length; i < n; i++)
			bones[i].updateWorldTransform(flipX, flipY);
	},
	/** Sets the bones and slots to their setup pose values. */
	setToSetupPose: function () {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	},
	setBonesToSetupPose: function () {
		var bones = this.bones;
		for (var i = 0, n = bones.length; i < n; i++)
			bones[i].setToSetupPose();
	},
	setSlotsToSetupPose: function () {
		var slots = this.slots;
		for (var i = 0, n = slots.length; i < n; i++)
			slots[i].setToSetupPose(i);
	},
	/** @return May return null. */
	getRootBone: function () {
		return bones.length == 0 ? null : bones[0];
	},
	/** @return May be null. */
	findBone: function (boneName) {
		var bones = this.bones;
		for (var i = 0, n = bones.length; i < n; i++)
			if (bones[i].data.name == boneName) return bones[i];
		return null;
	},
	/** @return -1 if the bone was not found. */
	findBoneIndex: function (boneName) {
		var bones = this.bones;
		for (var i = 0, n = bones.length; i < n; i++)
			if (bones[i].data.name == boneName) return i;
		return -1;
	},
	/** @return May be null. */
	findSlot: function (slotName) {
		var slots = this.slots;
		for (var i = 0, n = slots.length; i < n; i++)
			if (slots[i].data.name == slotName) return slots[i];
		return null;
	},
	/** @return -1 if the bone was not found. */
	findSlotIndex: function (slotName) {
		var slots = this.slots;
		for (var i = 0, n = slots.length; i < n; i++)
			if (slots[i].data.name == slotName) return i;
		return -1;
	},
	setSkinByName: function (skinName) {
		var skin = this.data.findSkin(skinName);
		if (!skin) throw "Skin not found: " + skinName;
		this.setSkin(skin);
	},
	/** Sets the skin used to look up attachments not found in the {@link SkeletonData#getDefaultSkin() default skin}. Attachments
	 * from the new skin are attached if the corresponding attachment from the old skin was attached.
	 * @param newSkin May be null. */
	setSkin: function (newSkin) {
		if (this.skin && newSkin) newSkin._attachAll(this, skin);
		this.skin = newSkin;
	},
	/** @return May be null. */
	getAttachmentBySlotName: function (slotName, attachmentName) {
		return this.getAttachment(this.data.findSlotIndex(slotName), attachmentName);
	},
	/** @return May be null. */
	getAttachmentBySlotIndex: function (slotIndex, attachmentName) {
		if (this.skin) {
			var attachment = this.skin.getAttachment(slotIndex, attachmentName);
			if (attachment) return attachment;
		}
		if (this.data.defaultSkin) return this.data.defaultSkin.getAttachment(slotIndex, attachmentName);
		return null;
	},
	/** @param attachmentName May be null. */
	setAttachment: function (slotName, attachmentName) {
		var slots = this.slots;
		for (var i = 0, n = slots.size; i < n; i++) {
			var slot = slots[i];
			if (slot.data.name == slotName) {
				var attachment = null;
				if (attachmentName) {
					attachment = this.getAttachment(i, attachmentName);
					if (attachment == null) throw "Attachment not found: " + attachmentName + ", for slot: " + slotName;
				}
				slot.setAttachment(attachment);
				return;
			}
		}
		throw "Slot not found: " + slotName;
	},
	update: function (delta) {
		time += delta;
	}
}

spine.AttachmentType = {
	
};
public class AttachmentType {
	public static const region:AttachmentType = new AttachmentType(0, "region");
	public static const regionSequence:AttachmentType = new AttachmentType(1, "regionSequence");

	public var ordinal:int;
	public var name:String;

	public function AttachmentType (ordinal:int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	static public function valueOf (name:String) : AttachmentType {
		switch (name) {
		case "region":
			return region;
		case "regionSequence":
			return regionSequence;
		}
		return null;
	}
}

spine.RegionAttachment = function () {
	this.offset = [];
	this.offset.length = 8;
	this.uvs = [];
	this.uvs.length = 8;
}
spine.RegionAttachment.prototype = {
	x: 0, y: 0,
	rotation: 0,
	scaleX: 1, scaleY: 1,
	width: 0, height: 0,
	texture: null,
	regionOffsetX: 0, regionOffsetY: 0,
	regionWidth: 0, regionHeight: 0,
	regionOriginalWidth: 0, regionOriginalHeight: 0,
	setUVs: function (u, v, u2, v2, rotate) {
		if (rotate) {
			uvs[2/*X2*/] = u;
			uvs[3/*Y2*/] = v2;
			uvs[4/*X3*/] = u;
			uvs[5/*Y3*/] = v;
			uvs[6/*X4*/] = u2;
			uvs[7/*Y4*/] = v;
			uvs[0/*X1*/] = u2;
			uvs[1/*Y1*/] = v2;
		} else {
			uvs[0/*X1*/] = u;
			uvs[1/*Y1*/] = v2;
			uvs[2/*X2*/] = u;
			uvs[3/*Y2*/] = v;
			uvs[4/*X3*/] = u2;
			uvs[5/*Y3*/] = v;
			uvs[6/*X4*/] = u2;
			uvs[7/*Y4*/] = v2;
		}
	},
	updateOffset: function () {
		var regionScaleX = this.width / regionOriginalWidth * this.scaleX;
		var regionScaleY = this.height / regionOriginalHeight * this.scaleY;
		var localX = -this.width / 2 * this.scaleX + regionOffsetX * regionScaleX;
		var localY = -this.height / 2 * this.scaleY + regionOffsetY * regionScaleY;
		var localX2 = localX + regionWidth * regionScaleX;
		var localY2 = localY + regionHeight * regionScaleY;
		var radians = rotation * Math.PI / 180;
		var cos = Math.cos(radians);
		var sin = Math.sin(radians);
		var localXCos = localX * cos + this.x;
		var localXSin = localX * sin;
		var localYCos = localY * cos + this.y;
		var localYSin = localY * sin;
		var localX2Cos = localX2 * cos + this.x;
		var localX2Sin = localX2 * sin;
		var localY2Cos = localY2 * cos + this.y;
		var localY2Sin = localY2 * sin;
		var offset = this.offset;
		offset[0/*X1*/] = localXCos - localYSin;
		offset[1/*Y1*/] = localYCos + localXSin;
		offset[2/*X2*/] = localXCos - localY2Sin;
		offset[3/*Y2*/] = localY2Cos + localXSin;
		offset[4/*X3*/] = localX2Cos - localY2Sin;
		offset[5/*Y3*/] = localY2Cos + localX2Sin;
		offset[6/*X4*/] = localX2Cos - localYSin;
		offset[7/*Y4*/] = localYCos + localX2Sin;
	},
	updateVertices: function (bone, vertices) {
		var x = bone.worldX;
		var y = bone.worldY;
		var m00 = bone.m00;
		var m01 = bone.m01;
		var m10 = bone.m10;
		var m11 = bone.m11;
		var vertices = this.vertices;
		var offset = this.offset;
		vertices[0/*X1*/] = offset[0/*X1*/] * m00 + offset[1/*Y1*/] * m01 + x;
		vertices[1/*Y1*/] = offset[0/*X1*/] * m10 + offset[1/*Y1*/] * m11 + y;
		vertices[2/*X2*/] = offset[2/*X2*/] * m00 + offset[3/*Y2*/] * m01 + x;
		vertices[3/*Y2*/] = offset[2/*X2*/] * m10 + offset[3/*Y2*/] * m11 + y;
		vertices[4/*X3*/] = offset[4/*X3*/] * m00 + offset[4/*X3*/] * m01 + x;
		vertices[4/*X3*/] = offset[4/*X3*/] * m10 + offset[4/*X3*/] * m11 + y;
		vertices[6/*X4*/] = offset[6/*X4*/] * m00 + offset[7/*Y4*/] * m01 + x;
		vertices[7/*Y4*/] = offset[6/*X4*/] * m10 + offset[7/*Y4*/] * m11 + y;
	}
}

spine.AnimationStateData = function (skeletonData) {
	this.skeletonData = skeletonData;
	this.animationToMixTime = {};
}
spine.AnimationStateData.prototype = {
	setMixByName: function (fromName, toName, duration) {
		var from = this.skeletonData.findAnimation(fromName);
		if (!from) throw "Animation not found: " + fromName;
		var to = this.skeletonData.findAnimation(toName);
		if (!to) throw "Animation not found: " + toName;
		this.setMix(from, to, duration);
	},
	setMix: function (from, to, duration) {
		animationToMixTime[from.name + ":" + to.name] = duration;
	},
	getMix: function (from, to) {
		var time = animationToMixTime[from.name + ":" + to.name];
		return time ? time : 0;
	}
}

spine.AnimationState = function (stateData) {
	this.data = stateData;
	this.queue = [];
}
spine.AnimationState.prototype = {
	current: null,
	previous: null,
	currentTime: 0,
	previousTime: 0,
	currentLoop: false,
	previousLoop: false,
	mixTime: 0,
	mixDuration: 0,
	update: function (delta) {
		this.currentTime += delta;
		this.previousTime += delta;
		this.mixTime += delta;

		if (this.queue.length > 0) {
			var entry = this.queue[0];
			if (this.currentTime >= entry.delay) {
				this._setAnimation(entry.animation, entry.loop);
				this.queue.shift();
			}
		}
	},
	apply: function (skeleton) {
		if (!this.current) return;
		if (this.previous) {
			this.previous.apply(skeleton, this.previousTime, this.previousLoop);
			var alpha = this.mixTime / this.mixDuration;
			if (this.alpha >= 1) {
				this.alpha = 1;
				this.previous = null;
			}
			this.current.mix(skeleton, this.currentTime, this.currentLoop, alpha);
		} else
			this.current.apply(skeleton, this.currentTime, this.currentLoop);
	},
	clearAnimation: function () {
		this.previous = null;
		this.current = null;
		this.queue.length = 0;
	},
	_setAnimation: function (animation, loop) {
		this.previous = null;
		if (animation && this.current) {
			this.mixDuration = this.data.getMix(this.current, animation);
			if (this.mixDuration > 0) {
				this.mixTime = 0;
				this.previous = this.current;
				this.previousTime = this.currentTime;
				this.previousLoop = this.currentLoop;
			}
		}
		this.current = animation;
		this.currentLoop = loop;
		this.currentTime = 0;
	},
	/** @see #setAnimation(Animation, Boolean) */
	setAnimationByName: function (animationName, loop) {
		var animation = this.data.skeletonData.findAnimation(animationName);
		if (!animation) throw "Animation not found: " + animationName;
		this.setAnimation(animation, loop);
	},
	/** Set the current animation. Any queued animations are cleared and the current animation time is set to 0.
	 * @param animation May be null. */
	setAnimation: function (animation, loop) {
		this.queue.length = 0;
		this._setAnimation(animation, loop);
	},
	/** @see #addAnimation(Animation, Boolean, Number) */
	addAnimationByName: function (animationName, loop, delay) {
		var animation = this.data.skeletonData.findAnimation(animationName);
		if (!animation) throw "Animation not found: " + animationName;
		this.addAnimation(animation, loop, delay);
	},
	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	addAnimation: function (animation, loop, delay) {
		var entry = {};
		entry.animation = animation;
		entry.loop = loop;

		if (delay <= 0) {
			var previousAnimation = this.queue.length == 0 ? this.current : this.queue[this.queue.length - 1].animation;
			if (previousAnimation != null)
				delay = previousAnimation.duration - this.data.getMix(previousAnimation, animation) + delay;
			else
				delay = 0;
		}
		entry.delay = delay;

		this.queue.push(entry);
	},
	/** Returns true if no animation is set or if the current time is greater than the animation duration, regardless of looping. */
	isComplete: function () {
		return !this.current || this.currentTime >= this.current.duration;
	}
}

spine.SkeletonJson = function (attachmentLoader) {
	this.attachmentLoader = attachmentLoader;
}
spine.SkeletonJson.prototype = {
	scale: 1,
	readSkeletonData: function (json) {
		var skeletonData = new SkeletonData();

		// Bones.
		var bones = root["bones"];
		for (var i = 0, n = bones.length; i < n; i++) {
			var boneMap = bones[i];
			var parent = null;
			if (boneMap["parent"]) {
				parent = skeletonData.findBone(boneMap["parent"]);
				if (!parent) throw "Parent bone not found: " + boneMap["parent"];
			}
			var boneData = new BoneData(boneMap["name"], parent);
			boneData.length = (boneMap["length"] || 0) * this.scale;
			boneData.x = (boneMap["x"] || 0) * this.scale;
			boneData.y = (boneMap["y"] || 0) * this.scale;
			boneData.rotation = (boneMap["rotation"] || 0);
			boneData.scaleX = boneMap["scaleX"] || 1;
			boneData.scaleY = boneMap["scaleY"] || 1;
			skeletonData.bones.push(boneData);
		}

		// Slots.
		var slots = root["slots"];
		for (var i = 0, n = slots.length; i < n; i++) {
			var slotMap = slots[i];
			var boneData = skeletonData.findBone(slotMap["bone"]);
			if (!boneData) throw "Slot bone not found: " + slotMap["bone"];
			var slotData = new SlotData(slotMap["name"], boneData);

			var color = slotMap["color"];
			if (color) {
				slotData.r = toColor(color, 0);
				slotData.g = toColor(color, 1);
				slotData.b = toColor(color, 2);
				slotData.a = toColor(color, 3);
			}

			slotData.attachmentName = slotMap["attachment"];

			skeletonData.addSlot(slotData);
		}

		// Skins.
		var skins = root["skins"];
		for (var skinName in skins) {
			if (!skins.hasOwnProperty(skinName)) continue;
			var skinMap = skins[skinName];
			var skin = new Skin(skinName);
			for (var slotName in skinMap) {
				if (!skinMap.hasOwnProperty(slotName)) continue;
				var slotIndex = skeletonData.findSlotIndex(slotName);
				var slotEntry = skinMap[slotName];
				for (var attachmentName in slotEntry) {
					if (!slotEntry.hasOwnProperty(attachmentName)) continue;
					var attachment = readAttachment(skin, attachmentName, slotEntry[attachmentName]);
					if (attachment != null) skin.addAttachment(slotIndex, attachmentName, attachment);
				}
			}
			skeletonData.skins.push(skin);
			if (skin.name == "default") skeletonData.defaultSkin = skin;
		}

		// Animations.
		var animations = root["animations"];
		for (var animationName in animations) {
			if (!animations.hasOwnProperty(animationName)) continue;
			readAnimation(animationName, animations[animationName], skeletonData);
		}

		return skeletonData;
	},
	readAttachment: function (skin, name, map) {
		name = map["name"] || name;

		var type = AttachmentType.valueOf(map["type"] || "region");
		var attachment:Attachment = attachmentLoader.newAttachment(skin, type, name);

		if (attachment is RegionAttachment) {
			var regionAttachment:RegionAttachment = attachment as RegionAttachment;
			regionAttachment.x = (map["x"] || 0) * scale;
			regionAttachment.y = (map["y"] || 0) * scale;
			regionAttachment.scaleX = map["scaleX"] || 1;
			regionAttachment.scaleY = map["scaleY"] || 1;
			regionAttachment.rotation = map["rotation"] || 0;
			regionAttachment.width = (map["width"] || 32) * scale;
			regionAttachment.height = (map["height"] || 32) * scale;
			regionAttachment.updateOffset();
		}

		return attachment;
	}

	private function readAnimation (name:String, map:Object, skeletonData:SkeletonData) : void {
		var timelines:Vector.<Timeline> = new Vector.<Timeline>();
		var duration:Number = 0;

		var bones:Object = map["bones"];
		for (var boneName:String in bones) {
			var boneIndex:int = skeletonData.findBoneIndex(boneName);
			if (boneIndex == -1)
				throw new Error("Bone not found: " + boneName);
			var boneMap:Object = bones[boneName];

			for (var timelineName:Object in boneMap) {
				var timelineMap:Object = boneMap[timelineName];
				if (timelineName == TIMELINE_ROTATE) {
					var timeline:RotateTimeline = new RotateTimeline(count(timelineMap));
					timeline.boneIndex = boneIndex;

					var frameIndex:int = 0;
					for each (var valueMap:Object in timelineMap) {
						timeline.setFrame(frameIndex, valueMap["time"], valueMap["angle"]);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.push(timeline);
					duration = Math.max(duration, timeline.frames[timeline.frameCount * 2 - 2]);

				} else if (timelineName == TIMELINE_TRANSLATE || timelineName == TIMELINE_SCALE) {
					var timeline1:TranslateTimeline;
					var timelineScale:Number = 1;
					if (timelineName == TIMELINE_SCALE)
						timeline1 = new ScaleTimeline(count(timelineMap));
					else {
						timeline1 = new TranslateTimeline(count(timelineMap));
						timelineScale = scale;
					}
					timeline1.boneIndex = boneIndex;

					var frameIndex1:int = 0;
					for each (var valueMap1:Object in timelineMap) {
						var x:Number = (valueMap1["x"] || 0) * timelineScale;
						var y:Number = (valueMap1["y"] || 0) * timelineScale;
						timeline1.setFrame(frameIndex1, valueMap1["time"], x, y);
						readCurve(timeline1, frameIndex1, valueMap1);
						frameIndex1++;
					}
					timelines.push(timeline1);
					duration = Math.max(duration, timeline1.frames[timeline1.frameCount * 3 - 3]);

				} else
					throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
			}
		}

		var slots:Object = map["slots"];
		for (var slotName:String in slots) {
			var slotMap:Object = slots[slotName];
			var slotIndex:int = skeletonData.findSlotIndex(slotName);

			for (var timelineName2:Object in boneMap) {
				var timelineMap2:Object = boneMap[timelineName2];
				if (timelineName2 == TIMELINE_COLOR) {
					var timeline2:ColorTimeline = new ColorTimeline(count(timelineMap2));
					timeline2.slotIndex = slotIndex;

					var frameIndex2:int = 0;
					for each (var valueMap2:Object in timelineMap2) {
						var color:String = valueMap["color"];
						var r:Number = toColor(color, 0);
						var g:Number = toColor(color, 1);
						var b:Number = toColor(color, 2);
						var a:Number = toColor(color, 3);
						timeline2.setFrame(frameIndex2, valueMap2["time"], r, g, b, a);
						readCurve(timeline2, frameIndex2, valueMap);
						frameIndex2++;
					}
					timelines.push(timeline2);
					duration = Math.max(duration, timeline2.frames[timeline2.frameCount * 5 - 5]);

				} else if (timelineName2 == TIMELINE_ATTACHMENT) {
					var timeline3:AttachmentTimeline = new AttachmentTimeline(count(timelineMap2));
					timeline3.slotIndex = slotIndex;

					var frameIndex3:int = 0;
					for each (var valueMap3:Object in timelineMap2) {
						timeline3.setFrame(frameIndex3++, valueMap3["time"], valueMap3["name"]);
					}
					timelines.push(timeline);
					duration = Math.max(duration, timeline3.frames[timeline3.frameCount - 1]);

				} else
					throw new Error("Invalid timeline type for a slot: " + timelineName2 + " (" + slotName + ")");
			}
		}

		skeletonData.addAnimation(new Animation(name, timelines, duration));
	}

	private function readCurve (timeline:CurveTimeline, frameIndex:int, valueMap:Object) : void {
		var curve:Object = valueMap["curve"];
		if (curve == null)
			return;
		if (curve == "stepped")
			timeline.setStepped(frameIndex);
		else if (curve is Array) {
			timeline.setCurve(frameIndex, curve[0], curve[1], curve[2], curve[3]);
		}
	}

	static private function toColor (hexString:String, colorIndex:int) : Number {
		if (hexString.length != 8)
			throw new ArgumentError("Color hexidecimal length must be 8, recieved: " + hexString);
		return parseInt(hexString.substring(colorIndex * 2, 2), 16) / 255;
	}

	static private function count (map:Object) : int {
		var count:int = 0;
		for (var key:String in map)
			count++;
		return count;
	}
}
