var __extends = (this && this.__extends) || function (d, b) {
	for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
	function __() { this.constructor = d; }
	d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var spine;
(function (spine) {
	var Animation = (function () {
		function Animation(name, timelines, duration) {
			if (name == null)
				throw new Error("name cannot be null.");
			if (timelines == null)
				throw new Error("timelines cannot be null.");
			this.name = name;
			this.timelines = timelines;
			this.duration = duration;
		}
		Animation.prototype.apply = function (skeleton, lastTime, time, loop, events) {
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0)
					lastTime %= this.duration;
			}
			var timelines = this.timelines;
			for (var i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, 1);
		};
		Animation.prototype.mix = function (skeleton, lastTime, time, loop, events, alpha) {
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0)
					lastTime %= this.duration;
			}
			var timelines = this.timelines;
			for (var i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, alpha);
		};
		Animation.binarySearch = function (values, target, step) {
			if (step === void 0) { step = 1; }
			var low = 0;
			var high = values.length / step - 2;
			if (high == 0)
				return step;
			var current = high >>> 1;
			while (true) {
				if (values[(current + 1) * step] <= target)
					low = current + 1;
				else
					high = current;
				if (low == high)
					return (low + 1) * step;
				current = (low + high) >>> 1;
			}
		};
		Animation.linearSearch = function (values, target, step) {
			for (var i = 0, last = values.length - step; i <= last; i += step)
				if (values[i] > target)
					return i;
			return -1;
		};
		return Animation;
	}());
	spine.Animation = Animation;
	var CurveTimeline = (function () {
		function CurveTimeline(frameCount) {
			if (frameCount <= 0)
				throw new Error("frameCount must be > 0: " + frameCount);
			this.curves = spine.Utils.newFloatArray((frameCount - 1) * CurveTimeline.BEZIER_SIZE);
		}
		CurveTimeline.prototype.getFrameCount = function () {
			return this.curves.length / CurveTimeline.BEZIER_SIZE + 1;
		};
		CurveTimeline.prototype.setLinear = function (frameIndex) {
			this.curves[frameIndex * CurveTimeline.BEZIER_SIZE] = CurveTimeline.LINEAR;
		};
		CurveTimeline.prototype.setStepped = function (frameIndex) {
			this.curves[frameIndex * CurveTimeline.BEZIER_SIZE] = CurveTimeline.STEPPED;
		};
		CurveTimeline.prototype.getCurveType = function (frameIndex) {
			var index = frameIndex * CurveTimeline.BEZIER_SIZE;
			if (index == this.curves.length)
				return CurveTimeline.LINEAR;
			var type = this.curves[index];
			if (type == CurveTimeline.LINEAR)
				return CurveTimeline.LINEAR;
			if (type == CurveTimeline.STEPPED)
				return CurveTimeline.STEPPED;
			return CurveTimeline.BEZIER;
		};
		CurveTimeline.prototype.setCurve = function (frameIndex, cx1, cy1, cx2, cy2) {
			var tmpx = (-cx1 * 2 + cx2) * 0.03, tmpy = (-cy1 * 2 + cy2) * 0.03;
			var dddfx = ((cx1 - cx2) * 3 + 1) * 0.006, dddfy = ((cy1 - cy2) * 3 + 1) * 0.006;
			var ddfx = tmpx * 2 + dddfx, ddfy = tmpy * 2 + dddfy;
			var dfx = cx1 * 0.3 + tmpx + dddfx * 0.16666667, dfy = cy1 * 0.3 + tmpy + dddfy * 0.16666667;
			var i = frameIndex * CurveTimeline.BEZIER_SIZE;
			var curves = this.curves;
			curves[i++] = CurveTimeline.BEZIER;
			var x = dfx, y = dfy;
			for (var n = i + CurveTimeline.BEZIER_SIZE - 1; i < n; i += 2) {
				curves[i] = x;
				curves[i + 1] = y;
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				x += dfx;
				y += dfy;
			}
		};
		CurveTimeline.prototype.getCurvePercent = function (frameIndex, percent) {
			percent = spine.MathUtils.clamp(percent, 0, 1);
			var curves = this.curves;
			var i = frameIndex * CurveTimeline.BEZIER_SIZE;
			var type = curves[i];
			if (type == CurveTimeline.LINEAR)
				return percent;
			if (type == CurveTimeline.STEPPED)
				return 0;
			i++;
			var x = 0;
			for (var start = i, n = i + CurveTimeline.BEZIER_SIZE - 1; i < n; i += 2) {
				x = curves[i];
				if (x >= percent) {
					var prevX = void 0, prevY = void 0;
					if (i == start) {
						prevX = 0;
						prevY = 0;
					}
					else {
						prevX = curves[i - 2];
						prevY = curves[i - 1];
					}
					return prevY + (curves[i + 1] - prevY) * (percent - prevX) / (x - prevX);
				}
			}
			var y = curves[i - 1];
			return y + (1 - y) * (percent - x) / (1 - x);
		};
		CurveTimeline.LINEAR = 0;
		CurveTimeline.STEPPED = 1;
		CurveTimeline.BEZIER = 2;
		CurveTimeline.BEZIER_SIZE = 10 * 2 - 1;
		return CurveTimeline;
	}());
	spine.CurveTimeline = CurveTimeline;
	var RotateTimeline = (function (_super) {
		__extends(RotateTimeline, _super);
		function RotateTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount << 1);
		}
		RotateTimeline.prototype.setFrame = function (frameIndex, time, degrees) {
			frameIndex <<= 1;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + RotateTimeline.ROTATION] = degrees;
		};
		RotateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var bone = skeleton.bones[this.boneIndex];
			if (time >= frames[frames.length - RotateTimeline.ENTRIES]) {
				var amount_1 = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION] - bone.rotation;
				while (amount_1 > 180)
					amount_1 -= 360;
				while (amount_1 < -180)
					amount_1 += 360;
				bone.rotation += amount_1 * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, RotateTimeline.ENTRIES);
			var prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));
			var amount = frames[frame + RotateTimeline.ROTATION] - prevRotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			amount = bone.data.rotation + (prevRotation + amount * percent) - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
		};
		RotateTimeline.ENTRIES = 2;
		RotateTimeline.PREV_TIME = -2;
		RotateTimeline.PREV_ROTATION = -1;
		RotateTimeline.ROTATION = 1;
		return RotateTimeline;
	}(CurveTimeline));
	spine.RotateTimeline = RotateTimeline;
	var TranslateTimeline = (function (_super) {
		__extends(TranslateTimeline, _super);
		function TranslateTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount * TranslateTimeline.ENTRIES);
		}
		TranslateTimeline.prototype.setFrame = function (frameIndex, time, x, y) {
			frameIndex *= TranslateTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TranslateTimeline.X] = x;
			this.frames[frameIndex + TranslateTimeline.Y] = y;
		};
		TranslateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var bone = skeleton.bones[this.boneIndex];
			if (time >= frames[frames.length - TranslateTimeline.ENTRIES]) {
				bone.x += (bone.data.x + frames[frames.length + TranslateTimeline.PREV_X] - bone.x) * alpha;
				bone.y += (bone.data.y + frames[frames.length + TranslateTimeline.PREV_Y] - bone.y) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, TranslateTimeline.ENTRIES);
			var prevX = frames[frame + TranslateTimeline.PREV_X];
			var prevY = frames[frame + TranslateTimeline.PREV_Y];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / TranslateTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + TranslateTimeline.PREV_TIME] - frameTime));
			bone.x += (bone.data.x + prevX + (frames[frame + TranslateTimeline.X] - prevX) * percent - bone.x) * alpha;
			bone.y += (bone.data.y + prevY + (frames[frame + TranslateTimeline.Y] - prevY) * percent - bone.y) * alpha;
		};
		TranslateTimeline.ENTRIES = 3;
		TranslateTimeline.PREV_TIME = -3;
		TranslateTimeline.PREV_X = -2;
		TranslateTimeline.PREV_Y = -1;
		TranslateTimeline.X = 1;
		TranslateTimeline.Y = 2;
		return TranslateTimeline;
	}(CurveTimeline));
	spine.TranslateTimeline = TranslateTimeline;
	var ScaleTimeline = (function (_super) {
		__extends(ScaleTimeline, _super);
		function ScaleTimeline(frameCount) {
			_super.call(this, frameCount);
		}
		ScaleTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var bone = skeleton.bones[this.boneIndex];
			if (time >= frames[frames.length - ScaleTimeline.ENTRIES]) {
				bone.scaleX += (bone.data.scaleX * frames[frames.length + ScaleTimeline.PREV_X] - bone.scaleX) * alpha;
				bone.scaleY += (bone.data.scaleY * frames[frames.length + ScaleTimeline.PREV_Y] - bone.scaleY) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, ScaleTimeline.ENTRIES);
			var prevX = frames[frame + ScaleTimeline.PREV_X];
			var prevY = frames[frame + ScaleTimeline.PREV_Y];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / ScaleTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + ScaleTimeline.PREV_TIME] - frameTime));
			bone.scaleX += (bone.data.scaleX * (prevX + (frames[frame + ScaleTimeline.X] - prevX) * percent) - bone.scaleX) * alpha;
			bone.scaleY += (bone.data.scaleY * (prevY + (frames[frame + ScaleTimeline.Y] - prevY) * percent) - bone.scaleY) * alpha;
		};
		return ScaleTimeline;
	}(TranslateTimeline));
	spine.ScaleTimeline = ScaleTimeline;
	var ShearTimeline = (function (_super) {
		__extends(ShearTimeline, _super);
		function ShearTimeline(frameCount) {
			_super.call(this, frameCount);
		}
		ShearTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var bone = skeleton.bones[this.boneIndex];
			if (time >= frames[frames.length - ShearTimeline.ENTRIES]) {
				bone.shearX += (bone.data.shearX + frames[frames.length + ShearTimeline.PREV_X] - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + frames[frames.length + ShearTimeline.PREV_Y] - bone.shearY) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, ShearTimeline.ENTRIES);
			var prevX = frames[frame + ShearTimeline.PREV_X];
			var prevY = frames[frame + ShearTimeline.PREV_Y];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / ShearTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + ShearTimeline.PREV_TIME] - frameTime));
			bone.shearX += (bone.data.shearX + (prevX + (frames[frame + ShearTimeline.X] - prevX) * percent) - bone.shearX) * alpha;
			bone.shearY += (bone.data.shearY + (prevY + (frames[frame + ShearTimeline.Y] - prevY) * percent) - bone.shearY) * alpha;
		};
		return ShearTimeline;
	}(TranslateTimeline));
	spine.ShearTimeline = ShearTimeline;
	var ColorTimeline = (function (_super) {
		__extends(ColorTimeline, _super);
		function ColorTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount * ColorTimeline.ENTRIES);
		}
		ColorTimeline.prototype.setFrame = function (frameIndex, time, r, g, b, a) {
			frameIndex *= ColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + ColorTimeline.R] = r;
			this.frames[frameIndex + ColorTimeline.G] = g;
			this.frames[frameIndex + ColorTimeline.B] = b;
			this.frames[frameIndex + ColorTimeline.A] = a;
		};
		ColorTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var r = 0, g = 0, b = 0, a = 0;
			if (time >= frames[frames.length - ColorTimeline.ENTRIES]) {
				var i = frames.length;
				r = frames[i + ColorTimeline.PREV_R];
				g = frames[i + ColorTimeline.PREV_G];
				b = frames[i + ColorTimeline.PREV_B];
				a = frames[i + ColorTimeline.PREV_A];
			}
			else {
				var frame = Animation.binarySearch(frames, time, ColorTimeline.ENTRIES);
				r = frames[frame + ColorTimeline.PREV_R];
				g = frames[frame + ColorTimeline.PREV_G];
				b = frames[frame + ColorTimeline.PREV_B];
				a = frames[frame + ColorTimeline.PREV_A];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / ColorTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + ColorTimeline.PREV_TIME] - frameTime));
				r += (frames[frame + ColorTimeline.R] - r) * percent;
				g += (frames[frame + ColorTimeline.G] - g) * percent;
				b += (frames[frame + ColorTimeline.B] - b) * percent;
				a += (frames[frame + ColorTimeline.A] - a) * percent;
			}
			var color = skeleton.slots[this.slotIndex].color;
			if (alpha < 1)
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			else
				color.set(r, g, b, a);
		};
		ColorTimeline.ENTRIES = 5;
		ColorTimeline.PREV_TIME = -5;
		ColorTimeline.PREV_R = -4;
		ColorTimeline.PREV_G = -3;
		ColorTimeline.PREV_B = -2;
		ColorTimeline.PREV_A = -1;
		ColorTimeline.R = 1;
		ColorTimeline.G = 2;
		ColorTimeline.B = 3;
		ColorTimeline.A = 4;
		return ColorTimeline;
	}(CurveTimeline));
	spine.ColorTimeline = ColorTimeline;
	var AttachmentTimeline = (function () {
		function AttachmentTimeline(frameCount) {
			this.frames = spine.Utils.newFloatArray(frameCount);
			this.attachmentNames = new Array(frameCount);
		}
		AttachmentTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		AttachmentTimeline.prototype.setFrame = function (frameIndex, time, attachmentName) {
			this.frames[frameIndex] = time;
			this.attachmentNames[frameIndex] = attachmentName;
		};
		AttachmentTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var frameIndex = 0;
			if (time >= frames[frames.length - 1])
				frameIndex = frames.length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;
			var attachmentName = this.attachmentNames[frameIndex];
			skeleton.slots[this.slotIndex]
				.setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
		};
		return AttachmentTimeline;
	}());
	spine.AttachmentTimeline = AttachmentTimeline;
	var EventTimeline = (function () {
		function EventTimeline(frameCount) {
			this.frames = spine.Utils.newFloatArray(frameCount);
			this.events = new Array(frameCount);
		}
		EventTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		EventTimeline.prototype.setFrame = function (frameIndex, event) {
			this.frames[frameIndex] = event.time;
			this.events[frameIndex] = event;
		};
		EventTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			if (firedEvents == null)
				return;
			var frames = this.frames;
			var frameCount = this.frames.length;
			if (lastTime > time) {
				this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha);
				lastTime = -1;
			}
			else if (lastTime >= frames[frameCount - 1])
				return;
			if (time < frames[0])
				return;
			var frame = 0;
			if (lastTime < frames[0])
				frame = 0;
			else {
				frame = Animation.binarySearch(frames, lastTime);
				var frameTime = frames[frame];
				while (frame > 0) {
					if (frames[frame - 1] != frameTime)
						break;
					frame--;
				}
			}
			for (; frame < frameCount && time >= frames[frame]; frame++)
				firedEvents.push(this.events[frame]);
		};
		return EventTimeline;
	}());
	spine.EventTimeline = EventTimeline;
	var DrawOrderTimeline = (function () {
		function DrawOrderTimeline(frameCount) {
			this.frames = spine.Utils.newFloatArray(frameCount);
			this.drawOrders = new Array(frameCount);
		}
		DrawOrderTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		DrawOrderTimeline.prototype.setFrame = function (frameIndex, time, drawOrder) {
			this.frames[frameIndex] = time;
			this.drawOrders[frameIndex] = drawOrder;
		};
		DrawOrderTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var frame = 0;
			if (time >= frames[frames.length - 1])
				frame = frames.length - 1;
			else
				frame = Animation.binarySearch(frames, time) - 1;
			var drawOrder = skeleton.drawOrder;
			var slots = skeleton.slots;
			var drawOrderToSetupIndex = this.drawOrders[frame];
			if (drawOrderToSetupIndex == null)
				spine.Utils.arrayCopy(slots, 0, drawOrder, 0, slots.length);
			else {
				for (var i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		};
		return DrawOrderTimeline;
	}());
	spine.DrawOrderTimeline = DrawOrderTimeline;
	var DeformTimeline = (function (_super) {
		__extends(DeformTimeline, _super);
		function DeformTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount);
			this.frameVertices = new Array(frameCount);
		}
		DeformTimeline.prototype.setFrame = function (frameIndex, time, vertices) {
			this.frames[frameIndex] = time;
			this.frameVertices[frameIndex] = vertices;
		};
		DeformTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var slot = skeleton.slots[this.slotIndex];
			var slotAttachment = slot.getAttachment();
			if (!(slotAttachment instanceof spine.VertexAttachment) || !slotAttachment.applyDeform(this.attachment))
				return;
			var frames = this.frames;
			if (time < frames[0])
				return;
			var frameVertices = this.frameVertices;
			var vertexCount = frameVertices[0].length;
			var verticesArray = slot.attachmentVertices;
			if (verticesArray.length != vertexCount)
				alpha = 1;
			var vertices = spine.Utils.setArraySize(verticesArray, vertexCount);
			if (time >= frames[frames.length - 1]) {
				var lastVertices = frameVertices[frames.length - 1];
				if (alpha < 1) {
					for (var i = 0; i < vertexCount; i++)
						vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
				}
				else
					spine.Utils.arrayCopy(lastVertices, 0, vertices, 0, vertexCount);
				return;
			}
			var frame = Animation.binarySearch(frames, time);
			var prevVertices = frameVertices[frame - 1];
			var nextVertices = frameVertices[frame];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));
			if (alpha < 1) {
				for (var i = 0; i < vertexCount; i++) {
					var prev = prevVertices[i];
					vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
				}
			}
			else {
				for (var i = 0; i < vertexCount; i++) {
					var prev = prevVertices[i];
					vertices[i] = prev + (nextVertices[i] - prev) * percent;
				}
			}
		};
		return DeformTimeline;
	}(CurveTimeline));
	spine.DeformTimeline = DeformTimeline;
	var IkConstraintTimeline = (function (_super) {
		__extends(IkConstraintTimeline, _super);
		function IkConstraintTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount * IkConstraintTimeline.ENTRIES);
		}
		IkConstraintTimeline.prototype.setFrame = function (frameIndex, time, mix, bendDirection) {
			frameIndex *= IkConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + IkConstraintTimeline.MIX] = mix;
			this.frames[frameIndex + IkConstraintTimeline.BEND_DIRECTION] = bendDirection;
		};
		IkConstraintTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var constraint = skeleton.ikConstraints[this.ikConstraintIndex];
			if (time >= frames[frames.length - IkConstraintTimeline.ENTRIES]) {
				constraint.mix += (frames[frames.length + IkConstraintTimeline.PREV_MIX] - constraint.mix) * alpha;
				constraint.bendDirection = Math.floor(frames[frames.length + IkConstraintTimeline.PREV_BEND_DIRECTION]);
				return;
			}
			var frame = Animation.binarySearch(frames, time, IkConstraintTimeline.ENTRIES);
			var mix = frames[frame + IkConstraintTimeline.PREV_MIX];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / IkConstraintTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + IkConstraintTimeline.PREV_TIME] - frameTime));
			constraint.mix += (mix + (frames[frame + IkConstraintTimeline.MIX] - mix) * percent - constraint.mix) * alpha;
			constraint.bendDirection = Math.floor(frames[frame + IkConstraintTimeline.PREV_BEND_DIRECTION]);
		};
		IkConstraintTimeline.ENTRIES = 3;
		IkConstraintTimeline.PREV_TIME = -3;
		IkConstraintTimeline.PREV_MIX = -2;
		IkConstraintTimeline.PREV_BEND_DIRECTION = -1;
		IkConstraintTimeline.MIX = 1;
		IkConstraintTimeline.BEND_DIRECTION = 2;
		return IkConstraintTimeline;
	}(CurveTimeline));
	spine.IkConstraintTimeline = IkConstraintTimeline;
	var TransformConstraintTimeline = (function (_super) {
		__extends(TransformConstraintTimeline, _super);
		function TransformConstraintTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount * TransformConstraintTimeline.ENTRIES);
		}
		TransformConstraintTimeline.prototype.setFrame = function (frameIndex, time, rotateMix, translateMix, scaleMix, shearMix) {
			frameIndex *= TransformConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TransformConstraintTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + TransformConstraintTimeline.TRANSLATE] = translateMix;
			this.frames[frameIndex + TransformConstraintTimeline.SCALE] = scaleMix;
			this.frames[frameIndex + TransformConstraintTimeline.SHEAR] = shearMix;
		};
		TransformConstraintTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var constraint = skeleton.transformConstraints[this.transformConstraintIndex];
			if (time >= frames[frames.length - TransformConstraintTimeline.ENTRIES]) {
				var i = frames.length;
				constraint.rotateMix += (frames[i + TransformConstraintTimeline.PREV_ROTATE] - constraint.rotateMix) * alpha;
				constraint.translateMix += (frames[i + TransformConstraintTimeline.PREV_TRANSLATE] - constraint.translateMix) * alpha;
				constraint.scaleMix += (frames[i + TransformConstraintTimeline.PREV_SCALE] - constraint.scaleMix) * alpha;
				constraint.shearMix += (frames[i + TransformConstraintTimeline.PREV_SHEAR] - constraint.shearMix) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, TransformConstraintTimeline.ENTRIES);
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / TransformConstraintTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + TransformConstraintTimeline.PREV_TIME] - frameTime));
			var rotate = frames[frame + TransformConstraintTimeline.PREV_ROTATE];
			var translate = frames[frame + TransformConstraintTimeline.PREV_TRANSLATE];
			var scale = frames[frame + TransformConstraintTimeline.PREV_SCALE];
			var shear = frames[frame + TransformConstraintTimeline.PREV_SHEAR];
			constraint.rotateMix += (rotate + (frames[frame + TransformConstraintTimeline.ROTATE] - rotate) * percent - constraint.rotateMix) * alpha;
			constraint.translateMix += (translate + (frames[frame + TransformConstraintTimeline.TRANSLATE] - translate) * percent - constraint.translateMix)
				* alpha;
			constraint.scaleMix += (scale + (frames[frame + TransformConstraintTimeline.SCALE] - scale) * percent - constraint.scaleMix) * alpha;
			constraint.shearMix += (shear + (frames[frame + TransformConstraintTimeline.SHEAR] - shear) * percent - constraint.shearMix) * alpha;
		};
		TransformConstraintTimeline.ENTRIES = 5;
		TransformConstraintTimeline.PREV_TIME = -5;
		TransformConstraintTimeline.PREV_ROTATE = -4;
		TransformConstraintTimeline.PREV_TRANSLATE = -3;
		TransformConstraintTimeline.PREV_SCALE = -2;
		TransformConstraintTimeline.PREV_SHEAR = -1;
		TransformConstraintTimeline.ROTATE = 1;
		TransformConstraintTimeline.TRANSLATE = 2;
		TransformConstraintTimeline.SCALE = 3;
		TransformConstraintTimeline.SHEAR = 4;
		return TransformConstraintTimeline;
	}(CurveTimeline));
	spine.TransformConstraintTimeline = TransformConstraintTimeline;
	var PathConstraintPositionTimeline = (function (_super) {
		__extends(PathConstraintPositionTimeline, _super);
		function PathConstraintPositionTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount * PathConstraintPositionTimeline.ENTRIES);
		}
		PathConstraintPositionTimeline.prototype.setFrame = function (frameIndex, time, value) {
			frameIndex *= PathConstraintPositionTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintPositionTimeline.VALUE] = value;
		};
		PathConstraintPositionTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (time >= frames[frames.length - PathConstraintPositionTimeline.ENTRIES]) {
				var i = frames.length;
				constraint.position += (frames[i + PathConstraintPositionTimeline.PREV_VALUE] - constraint.position) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, PathConstraintPositionTimeline.ENTRIES);
			var position = frames[frame + PathConstraintPositionTimeline.PREV_VALUE];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / PathConstraintPositionTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PathConstraintPositionTimeline.PREV_TIME] - frameTime));
			constraint.position += (position + (frames[frame + PathConstraintPositionTimeline.VALUE] - position) * percent - constraint.position) * alpha;
		};
		PathConstraintPositionTimeline.ENTRIES = 2;
		PathConstraintPositionTimeline.PREV_TIME = -2;
		PathConstraintPositionTimeline.PREV_VALUE = -1;
		PathConstraintPositionTimeline.VALUE = 1;
		return PathConstraintPositionTimeline;
	}(CurveTimeline));
	spine.PathConstraintPositionTimeline = PathConstraintPositionTimeline;
	var PathConstraintSpacingTimeline = (function (_super) {
		__extends(PathConstraintSpacingTimeline, _super);
		function PathConstraintSpacingTimeline(frameCount) {
			_super.call(this, frameCount);
		}
		PathConstraintSpacingTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (time >= frames[frames.length - PathConstraintSpacingTimeline.ENTRIES]) {
				var i = frames.length;
				constraint.spacing += (frames[i + PathConstraintSpacingTimeline.PREV_VALUE] - constraint.spacing) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, PathConstraintSpacingTimeline.ENTRIES);
			var spacing = frames[frame + PathConstraintSpacingTimeline.PREV_VALUE];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / PathConstraintSpacingTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PathConstraintSpacingTimeline.PREV_TIME] - frameTime));
			constraint.spacing += (spacing + (frames[frame + PathConstraintSpacingTimeline.VALUE] - spacing) * percent - constraint.spacing) * alpha;
		};
		return PathConstraintSpacingTimeline;
	}(PathConstraintPositionTimeline));
	spine.PathConstraintSpacingTimeline = PathConstraintSpacingTimeline;
	var PathConstraintMixTimeline = (function (_super) {
		__extends(PathConstraintMixTimeline, _super);
		function PathConstraintMixTimeline(frameCount) {
			_super.call(this, frameCount);
			this.frames = spine.Utils.newFloatArray(frameCount * PathConstraintMixTimeline.ENTRIES);
		}
		PathConstraintMixTimeline.prototype.setFrame = function (frameIndex, time, rotateMix, translateMix) {
			frameIndex *= PathConstraintMixTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintMixTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + PathConstraintMixTimeline.TRANSLATE] = translateMix;
		};
		PathConstraintMixTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
			var frames = this.frames;
			if (time < frames[0])
				return;
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (time >= frames[frames.length - PathConstraintMixTimeline.ENTRIES]) {
				var i = frames.length;
				constraint.rotateMix += (frames[i + PathConstraintMixTimeline.PREV_ROTATE] - constraint.rotateMix) * alpha;
				constraint.translateMix += (frames[i + PathConstraintMixTimeline.PREV_TRANSLATE] - constraint.translateMix) * alpha;
				return;
			}
			var frame = Animation.binarySearch(frames, time, PathConstraintMixTimeline.ENTRIES);
			var rotate = frames[frame + PathConstraintMixTimeline.PREV_ROTATE];
			var translate = frames[frame + PathConstraintMixTimeline.PREV_TRANSLATE];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / PathConstraintMixTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PathConstraintMixTimeline.PREV_TIME] - frameTime));
			constraint.rotateMix += (rotate + (frames[frame + PathConstraintMixTimeline.ROTATE] - rotate) * percent - constraint.rotateMix) * alpha;
			constraint.translateMix += (translate + (frames[frame + PathConstraintMixTimeline.TRANSLATE] - translate) * percent - constraint.translateMix)
				* alpha;
		};
		PathConstraintMixTimeline.ENTRIES = 3;
		PathConstraintMixTimeline.PREV_TIME = -3;
		PathConstraintMixTimeline.PREV_ROTATE = -2;
		PathConstraintMixTimeline.PREV_TRANSLATE = -1;
		PathConstraintMixTimeline.ROTATE = 1;
		PathConstraintMixTimeline.TRANSLATE = 2;
		return PathConstraintMixTimeline;
	}(CurveTimeline));
	spine.PathConstraintMixTimeline = PathConstraintMixTimeline;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var AnimationState = (function () {
		function AnimationState(data) {
			if (data === void 0) { data = null; }
			this.tracks = new Array();
			this.events = new Array();
			this.listeners = new Array();
			this.timeScale = 1;
			if (data == null)
				throw new Error("data cannot be null.");
			this.data = data;
		}
		AnimationState.prototype.update = function (delta) {
			delta *= this.timeScale;
			for (var i = 0; i < this.tracks.length; i++) {
				var current = this.tracks[i];
				if (current == null)
					continue;
				var next = current.next;
				if (next != null) {
					var nextTime = current.lastTime - next.delay;
					if (nextTime >= 0) {
						var nextDelta = delta * next.timeScale;
						next.time = nextTime + nextDelta;
						current.time += delta * current.timeScale;
						this.setCurrent(i, next);
						next.time -= nextDelta;
						current = next;
					}
				}
				else if (!current.loop && current.lastTime >= current.endTime) {
					this.clearTrack(i);
					continue;
				}
				current.time += delta * current.timeScale;
				if (current.previous != null) {
					var previousDelta = delta * current.previous.timeScale;
					current.previous.time += previousDelta;
					current.mixTime += previousDelta;
				}
			}
		};
		AnimationState.prototype.apply = function (skeleton) {
			var events = this.events;
			var listenerCount = this.listeners.length;
			for (var i = 0; i < this.tracks.length; i++) {
				var current = this.tracks[i];
				if (current == null)
					continue;
				events.length = 0;
				var time = current.time;
				var lastTime = current.lastTime;
				var endTime = current.endTime;
				var loop = current.loop;
				if (!loop && time > endTime)
					time = endTime;
				var previous = current.previous;
				if (previous == null)
					current.animation.mix(skeleton, lastTime, time, loop, events, current.mix);
				else {
					var previousTime = previous.time;
					if (!previous.loop && previousTime > previous.endTime)
						previousTime = previous.endTime;
					previous.animation.apply(skeleton, previousTime, previousTime, previous.loop, null);
					var alpha = current.mixTime / current.mixDuration * current.mix;
					if (alpha >= 1) {
						alpha = 1;
						current.previous = null;
					}
					current.animation.mix(skeleton, lastTime, time, loop, events, alpha);
				}
				for (var ii = 0, nn = events.length; ii < nn; ii++) {
					var event_1 = events[ii];
					if (current.listener != null)
						current.listener.event(i, event_1);
					for (var iii = 0; iii < listenerCount; iii++)
						this.listeners[iii].event(i, event_1);
				}
				if (loop ? (lastTime % endTime > time % endTime) : (lastTime < endTime && time >= endTime)) {
					var count = spine.MathUtils.toInt(time / endTime);
					if (current.listener != null)
						current.listener.complete(i, count);
					for (var ii = 0, nn = this.listeners.length; ii < nn; ii++)
						this.listeners[ii].complete(i, count);
				}
				current.lastTime = current.time;
			}
		};
		AnimationState.prototype.clearTracks = function () {
			for (var i = 0, n = this.tracks.length; i < n; i++)
				this.clearTrack(i);
			this.tracks.length = 0;
		};
		AnimationState.prototype.clearTrack = function (trackIndex) {
			if (trackIndex >= this.tracks.length)
				return;
			var current = this.tracks[trackIndex];
			if (current == null)
				return;
			if (current.listener != null)
				current.listener.end(trackIndex);
			for (var i = 0, n = this.listeners.length; i < n; i++)
				this.listeners[i].end(trackIndex);
			this.tracks[trackIndex] = null;
			this.freeAll(current);
		};
		AnimationState.prototype.freeAll = function (entry) {
			while (entry != null) {
				var next = entry.next;
				entry = next;
			}
		};
		AnimationState.prototype.expandToIndex = function (index) {
			if (index < this.tracks.length)
				return this.tracks[index];
			spine.Utils.setArraySize(this.tracks, index - this.tracks.length + 1, null);
			this.tracks.length = index + 1;
			return null;
		};
		AnimationState.prototype.setCurrent = function (index, entry) {
			var current = this.expandToIndex(index);
			if (current != null) {
				var previous = current.previous;
				current.previous = null;
				if (current.listener != null)
					current.listener.end(index);
				for (var i = 0, n = this.listeners.length; i < n; i++)
					this.listeners[i].end(index);
				entry.mixDuration = this.data.getMix(current.animation, entry.animation);
				if (entry.mixDuration > 0) {
					entry.mixTime = 0;
					if (previous != null && current.mixTime / current.mixDuration < 0.5) {
						entry.previous = previous;
						previous = current;
					}
					else
						entry.previous = current;
				}
			}
			this.tracks[index] = entry;
			if (entry.listener != null)
				entry.listener.start(index);
			for (var i = 0, n = this.listeners.length; i < n; i++)
				this.listeners[i].start(index);
		};
		AnimationState.prototype.setAnimation = function (trackIndex, animationName, loop) {
			var animation = this.data.skeletonData.findAnimation(animationName);
			if (animation == null)
				throw new Error("Animation not found: " + animationName);
			return this.setAnimationWith(trackIndex, animation, loop);
		};
		AnimationState.prototype.setAnimationWith = function (trackIndex, animation, loop) {
			var current = this.expandToIndex(trackIndex);
			if (current != null)
				this.freeAll(current.next);
			var entry = new TrackEntry();
			entry.animation = animation;
			entry.loop = loop;
			entry.endTime = animation.duration;
			this.setCurrent(trackIndex, entry);
			return entry;
		};
		AnimationState.prototype.addAnimation = function (trackIndex, animationName, loop, delay) {
			var animation = this.data.skeletonData.findAnimation(animationName);
			if (animation == null)
				throw new Error("Animation not found: " + animationName);
			return this.addAnimationWith(trackIndex, animation, loop, delay);
		};
		AnimationState.prototype.addAnimationWith = function (trackIndex, animation, loop, delay) {
			var entry = new TrackEntry();
			entry.animation = animation;
			entry.loop = loop;
			entry.endTime = animation.duration;
			var last = this.expandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
				last.next = entry;
			}
			else
				this.tracks[trackIndex] = entry;
			if (delay <= 0) {
				if (last != null)
					delay += last.endTime - this.data.getMix(last.animation, animation);
				else
					delay = 0;
			}
			entry.delay = delay;
			return entry;
		};
		AnimationState.prototype.getCurrent = function (trackIndex) {
			if (trackIndex >= this.tracks.length)
				return null;
			return this.tracks[trackIndex];
		};
		AnimationState.prototype.addListener = function (listener) {
			if (listener == null)
				throw new Error("listener cannot be null.");
			this.listeners.push(listener);
		};
		AnimationState.prototype.removeListener = function (listener) {
			var index = this.listeners.indexOf(listener);
			if (index >= 0)
				this.listeners.splice(index, 1);
		};
		AnimationState.prototype.clearListeners = function () {
			this.listeners.length = 0;
		};
		return AnimationState;
	}());
	spine.AnimationState = AnimationState;
	var TrackEntry = (function () {
		function TrackEntry() {
			this.loop = false;
			this.delay = 0;
			this.time = 0;
			this.lastTime = -1;
			this.endTime = 0;
			this.timeScale = 1;
			this.mixTime = 0;
			this.mixDuration = 0;
			this.mix = 1;
		}
		TrackEntry.prototype.reset = function () {
			this.next = null;
			this.previous = null;
			this.animation = null;
			this.listener = null;
			this.timeScale = 1;
			this.lastTime = -1;
			this.time = 0;
		};
		TrackEntry.prototype.isComplete = function () {
			return this.time >= this.endTime;
		};
		return TrackEntry;
	}());
	spine.TrackEntry = TrackEntry;
	var AnimationStateAdapter = (function () {
		function AnimationStateAdapter() {
		}
		AnimationStateAdapter.prototype.event = function (trackIndex, event) {
		};
		AnimationStateAdapter.prototype.complete = function (trackIndex, loopCount) {
		};
		AnimationStateAdapter.prototype.start = function (trackIndex) {
		};
		AnimationStateAdapter.prototype.end = function (trackIndex) {
		};
		return AnimationStateAdapter;
	}());
	spine.AnimationStateAdapter = AnimationStateAdapter;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var AnimationStateData = (function () {
		function AnimationStateData(skeletonData) {
			this.animationToMixTime = {};
			this.defaultMix = 0;
			if (skeletonData == null)
				throw new Error("skeletonData cannot be null.");
			this.skeletonData = skeletonData;
		}
		AnimationStateData.prototype.setMix = function (fromName, toName, duration) {
			var from = this.skeletonData.findAnimation(fromName);
			if (from == null)
				throw new Error("Animation not found: " + fromName);
			var to = this.skeletonData.findAnimation(toName);
			if (to == null)
				throw new Error("Animation not found: " + toName);
			this.setMixWith(from, to, duration);
		};
		AnimationStateData.prototype.setMixWith = function (from, to, duration) {
			if (from == null)
				throw new Error("from cannot be null.");
			if (to == null)
				throw new Error("to cannot be null.");
			var key = from.name + to.name;
			this.animationToMixTime[key] = duration;
		};
		AnimationStateData.prototype.getMix = function (from, to) {
			var key = from.name + to.name;
			var value = this.animationToMixTime[key];
			return value === undefined ? this.defaultMix : value;
		};
		return AnimationStateData;
	}());
	spine.AnimationStateData = AnimationStateData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var AssetManager = (function () {
		function AssetManager(textureLoader, pathPrefix) {
			if (pathPrefix === void 0) { pathPrefix = ""; }
			this.assets = {};
			this.errors = {};
			this.toLoad = 0;
			this.loaded = 0;
			this.textureLoader = textureLoader;
			this.pathPrefix = pathPrefix;
		}
		AssetManager.prototype.loadText = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.pathPrefix + path;
			this.toLoad++;
			var request = new XMLHttpRequest();
			request.onreadystatechange = function () {
				if (request.readyState == XMLHttpRequest.DONE) {
					if (request.status >= 200 && request.status < 300) {
						if (success)
							success(path, request.responseText);
						_this.assets[path] = request.responseText;
					}
					else {
						if (error)
							error(path, "Couldn't load text " + path + ": status " + request.status + ", " + request.responseText);
						_this.errors[path] = "Couldn't load text " + path + ": status " + request.status + ", " + request.responseText;
					}
					_this.toLoad--;
					_this.loaded++;
				}
			};
			request.open("GET", path, true);
			request.send();
		};
		AssetManager.prototype.loadTexture = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.pathPrefix + path;
			this.toLoad++;
			var img = new Image();
			img.src = path;
			img.crossOrigin = "anonymous";
			img.onload = function (ev) {
				if (success)
					success(path, img);
				var texture = _this.textureLoader(img);
				_this.assets[path] = texture;
				_this.toLoad--;
				_this.loaded++;
			};
			img.onerror = function (ev) {
				if (error)
					error(path, "Couldn't load image " + path);
				_this.errors[path] = "Couldn't load image " + path;
				_this.toLoad--;
				_this.loaded++;
			};
		};
		AssetManager.prototype.get = function (path) {
			path = this.pathPrefix + path;
			return this.assets[path];
		};
		AssetManager.prototype.remove = function (path) {
			path = this.pathPrefix + path;
			var asset = this.assets[path];
			if (asset.dispose)
				asset.dispose();
			this.assets[path] = null;
		};
		AssetManager.prototype.removeAll = function () {
			for (var key in this.assets) {
				var asset = this.assets[key];
				if (asset.dispose)
					asset.dispose();
			}
			this.assets = {};
		};
		AssetManager.prototype.isLoadingComplete = function () {
			return this.toLoad == 0;
		};
		AssetManager.prototype.getToLoad = function () {
			return this.toLoad;
		};
		AssetManager.prototype.getLoaded = function () {
			return this.loaded;
		};
		AssetManager.prototype.dispose = function () {
			this.removeAll();
		};
		AssetManager.prototype.hasErrors = function () {
			return Object.keys(this.errors).length > 0;
		};
		AssetManager.prototype.getErrors = function () {
			return this.errors;
		};
		return AssetManager;
	}());
	spine.AssetManager = AssetManager;
})(spine || (spine = {}));
var spine;
(function (spine) {
	(function (BlendMode) {
		BlendMode[BlendMode["Normal"] = 0] = "Normal";
		BlendMode[BlendMode["Additive"] = 1] = "Additive";
		BlendMode[BlendMode["Multiply"] = 2] = "Multiply";
		BlendMode[BlendMode["Screen"] = 3] = "Screen";
	})(spine.BlendMode || (spine.BlendMode = {}));
	var BlendMode = spine.BlendMode;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Bone = (function () {
		function Bone(data, skeleton, parent) {
			this.children = new Array();
			this.x = 0;
			this.y = 0;
			this.rotation = 0;
			this.scaleX = 0;
			this.scaleY = 0;
			this.shearX = 0;
			this.shearY = 0;
			this.appliedRotation = 0;
			this.a = 0;
			this.b = 0;
			this.worldX = 0;
			this.c = 0;
			this.d = 0;
			this.worldY = 0;
			this.worldSignX = 0;
			this.worldSignY = 0;
			this.sorted = false;
			if (data == null)
				throw new Error("data cannot be null.");
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			this.setToSetupPose();
		}
		Bone.prototype.update = function () {
			this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
		};
		Bone.prototype.updateWorldTransform = function () {
			this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
		};
		Bone.prototype.updateWorldTransformWith = function (x, y, rotation, scaleX, scaleY, shearX, shearY) {
			this.appliedRotation = rotation;
			var rotationY = rotation + 90 + shearY;
			var la = spine.MathUtils.cosDeg(rotation + shearX) * scaleX, lb = spine.MathUtils.cosDeg(rotationY) * scaleY;
			var lc = spine.MathUtils.sinDeg(rotation + shearX) * scaleX, ld = spine.MathUtils.sinDeg(rotationY) * scaleY;
			var parent = this.parent;
			if (parent == null) {
				var skeleton = this.skeleton;
				if (skeleton.flipX) {
					x = -x;
					la = -la;
					lb = -lb;
				}
				if (skeleton.flipY) {
					y = -y;
					lc = -lc;
					ld = -ld;
				}
				this.a = la;
				this.b = lb;
				this.c = lc;
				this.d = ld;
				this.worldX = x;
				this.worldY = y;
				this.worldSignX = spine.MathUtils.signum(scaleX);
				this.worldSignY = spine.MathUtils.signum(scaleY);
				return;
			}
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			this.worldX = pa * x + pb * y + parent.worldX;
			this.worldY = pc * x + pd * y + parent.worldY;
			this.worldSignX = parent.worldSignX * spine.MathUtils.signum(scaleX);
			this.worldSignY = parent.worldSignY * spine.MathUtils.signum(scaleY);
			if (this.data.inheritRotation && this.data.inheritScale) {
				this.a = pa * la + pb * lc;
				this.b = pa * lb + pb * ld;
				this.c = pc * la + pd * lc;
				this.d = pc * lb + pd * ld;
			}
			else {
				if (this.data.inheritRotation) {
					pa = 1;
					pb = 0;
					pc = 0;
					pd = 1;
					do {
						var cos = spine.MathUtils.cosDeg(parent.appliedRotation), sin = spine.MathUtils.sinDeg(parent.appliedRotation);
						var temp = pa * cos + pb * sin;
						pb = pb * cos - pa * sin;
						pa = temp;
						temp = pc * cos + pd * sin;
						pd = pd * cos - pc * sin;
						pc = temp;
						if (!parent.data.inheritRotation)
							break;
						parent = parent.parent;
					} while (parent != null);
					this.a = pa * la + pb * lc;
					this.b = pa * lb + pb * ld;
					this.c = pc * la + pd * lc;
					this.d = pc * lb + pd * ld;
				}
				else if (this.data.inheritScale) {
					pa = 1;
					pb = 0;
					pc = 0;
					pd = 1;
					do {
						var cos = spine.MathUtils.cosDeg(parent.appliedRotation), sin = spine.MathUtils.sinDeg(parent.appliedRotation);
						var psx = parent.scaleX, psy = parent.scaleY;
						var za = cos * psx, zb = sin * psy, zc = sin * psx, zd = cos * psy;
						var temp = pa * za + pb * zc;
						pb = pb * zd - pa * zb;
						pa = temp;
						temp = pc * za + pd * zc;
						pd = pd * zd - pc * zb;
						pc = temp;
						if (psx >= 0)
							sin = -sin;
						temp = pa * cos + pb * sin;
						pb = pb * cos - pa * sin;
						pa = temp;
						temp = pc * cos + pd * sin;
						pd = pd * cos - pc * sin;
						pc = temp;
						if (!parent.data.inheritScale)
							break;
						parent = parent.parent;
					} while (parent != null);
					this.a = pa * la + pb * lc;
					this.b = pa * lb + pb * ld;
					this.c = pc * la + pd * lc;
					this.d = pc * lb + pd * ld;
				}
				else {
					this.a = la;
					this.b = lb;
					this.c = lc;
					this.d = ld;
				}
				if (this.skeleton.flipX) {
					this.a = -this.a;
					this.b = -this.b;
				}
				if (this.skeleton.flipY) {
					this.c = -this.c;
					this.d = -this.d;
				}
			}
		};
		Bone.prototype.setToSetupPose = function () {
			var data = this.data;
			this.x = data.x;
			this.y = data.y;
			this.rotation = data.rotation;
			this.scaleX = data.scaleX;
			this.scaleY = data.scaleY;
			this.shearX = data.shearX;
			this.shearY = data.shearY;
		};
		Bone.prototype.getWorldRotationX = function () {
			return Math.atan2(this.c, this.a) * spine.MathUtils.radDeg;
		};
		Bone.prototype.getWorldRotationY = function () {
			return Math.atan2(this.d, this.b) * spine.MathUtils.radDeg;
		};
		Bone.prototype.getWorldScaleX = function () {
			return Math.sqrt(this.a * this.a + this.b * this.b) * this.worldSignX;
		};
		Bone.prototype.getWorldScaleY = function () {
			return Math.sqrt(this.c * this.c + this.d * this.d) * this.worldSignY;
		};
		Bone.prototype.worldToLocalRotationX = function () {
			var parent = this.parent;
			if (parent == null)
				return this.rotation;
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, a = this.a, c = this.c;
			return Math.atan2(pa * c - pc * a, pd * a - pb * c) * spine.MathUtils.radDeg;
		};
		Bone.prototype.worldToLocalRotationY = function () {
			var parent = this.parent;
			if (parent == null)
				return this.rotation;
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, b = this.b, d = this.d;
			return Math.atan2(pa * d - pc * b, pd * b - pb * d) * spine.MathUtils.radDeg;
		};
		Bone.prototype.rotateWorld = function (degrees) {
			var a = this.a, b = this.b, c = this.c, d = this.d;
			var cos = spine.MathUtils.cosDeg(degrees), sin = spine.MathUtils.sinDeg(degrees);
			this.a = cos * a - sin * c;
			this.b = cos * b - sin * d;
			this.c = sin * a + cos * c;
			this.d = sin * b + cos * d;
		};
		Bone.prototype.updateLocalTransform = function () {
			var parent = this.parent;
			if (parent == null) {
				this.x = this.worldX;
				this.y = this.worldY;
				this.rotation = Math.atan2(this.c, this.a) * spine.MathUtils.radDeg;
				this.scaleX = Math.sqrt(this.a * this.a + this.c * this.c);
				this.scaleY = Math.sqrt(this.b * this.b + this.d * this.d);
				var det = this.a * this.d - this.b * this.c;
				this.shearX = 0;
				this.shearY = Math.atan2(this.a * this.b + this.c * this.d, det) * spine.MathUtils.radDeg;
				return;
			}
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			var pid = 1 / (pa * pd - pb * pc);
			var dx = this.worldX - parent.worldX, dy = this.worldY - parent.worldY;
			this.x = (dx * pd * pid - dy * pb * pid);
			this.y = (dy * pa * pid - dx * pc * pid);
			var ia = pid * pd;
			var id = pid * pa;
			var ib = pid * pb;
			var ic = pid * pc;
			var ra = ia * this.a - ib * this.c;
			var rb = ia * this.b - ib * this.d;
			var rc = id * this.c - ic * this.a;
			var rd = id * this.d - ic * this.b;
			this.shearX = 0;
			this.scaleX = Math.sqrt(ra * ra + rc * rc);
			if (this.scaleX > 0.0001) {
				var det = ra * rd - rb * rc;
				this.scaleY = det / this.scaleX;
				this.shearY = Math.atan2(ra * rb + rc * rd, det) * spine.MathUtils.radDeg;
				this.rotation = Math.atan2(rc, ra) * spine.MathUtils.radDeg;
			}
			else {
				this.scaleX = 0;
				this.scaleY = Math.sqrt(rb * rb + rd * rd);
				this.shearY = 0;
				this.rotation = 90 - Math.atan2(rd, rb) * spine.MathUtils.radDeg;
			}
			this.appliedRotation = this.rotation;
		};
		Bone.prototype.worldToLocal = function (world) {
			var a = this.a, b = this.b, c = this.c, d = this.d;
			var invDet = 1 / (a * d - b * c);
			var x = world.x - this.worldX, y = world.y - this.worldY;
			world.x = (x * d * invDet - y * b * invDet);
			world.y = (y * a * invDet - x * c * invDet);
			return world;
		};
		Bone.prototype.localToWorld = function (local) {
			var x = local.x, y = local.y;
			local.x = x * this.a + y * this.b + this.worldX;
			local.y = x * this.c + y * this.d + this.worldY;
			return local;
		};
		return Bone;
	}());
	spine.Bone = Bone;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var BoneData = (function () {
		function BoneData(index, name, parent) {
			this.x = 0;
			this.y = 0;
			this.rotation = 0;
			this.scaleX = 1;
			this.scaleY = 1;
			this.shearX = 0;
			this.shearY = 0;
			this.inheritRotation = true;
			this.inheritScale = true;
			if (index < 0)
				throw new Error("index must be >= 0.");
			if (name == null)
				throw new Error("name cannot be null.");
			this.index = index;
			this.name = name;
			this.parent = parent;
		}
		return BoneData;
	}());
	spine.BoneData = BoneData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Event = (function () {
		function Event(time, data) {
			if (data == null)
				throw new Error("data cannot be null.");
			this.time = time;
			this.data = data;
		}
		return Event;
	}());
	spine.Event = Event;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var EventData = (function () {
		function EventData(name) {
			this.name = name;
		}
		return EventData;
	}());
	spine.EventData = EventData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var IkConstraint = (function () {
		function IkConstraint(data, skeleton) {
			this.mix = 1;
			this.bendDirection = 0;
			this.level = 0;
			if (data == null)
				throw new Error("data cannot be null.");
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.mix = data.mix;
			this.bendDirection = data.bendDirection;
			this.bones = new Array();
			for (var i = 0; i < data.bones.length; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findBone(data.target.name);
		}
		IkConstraint.prototype.apply = function () {
			this.update();
		};
		IkConstraint.prototype.update = function () {
			var target = this.target;
			var bones = this.bones;
			switch (bones.length) {
				case 1:
					this.apply1(bones[0], target.worldX, target.worldY, this.mix);
					break;
				case 2:
					this.apply2(bones[0], bones[1], target.worldX, target.worldY, this.bendDirection, this.mix);
					break;
			}
		};
		IkConstraint.prototype.apply1 = function (bone, targetX, targetY, alpha) {
			var pp = bone.parent;
			var id = 1 / (pp.a * pp.d - pp.b * pp.c);
			var x = targetX - pp.worldX, y = targetY - pp.worldY;
			var tx = (x * pp.d - y * pp.b) * id - bone.x, ty = (y * pp.a - x * pp.c) * id - bone.y;
			var rotationIK = Math.atan2(ty, tx) * spine.MathUtils.radDeg - bone.shearX - bone.rotation;
			if (bone.scaleX < 0)
				rotationIK += 180;
			if (rotationIK > 180)
				rotationIK -= 360;
			else if (rotationIK < -180)
				rotationIK += 360;
			bone.updateWorldTransformWith(bone.x, bone.y, bone.rotation + rotationIK * alpha, bone.scaleX, bone.scaleY, bone.shearX, bone.shearY);
		};
		IkConstraint.prototype.apply2 = function (parent, child, targetX, targetY, bendDir, alpha) {
			if (alpha == 0) {
				child.updateWorldTransform();
				return;
			}
			var px = parent.x, py = parent.y, psx = parent.scaleX, psy = parent.scaleY, csx = child.scaleX;
			var os1 = 0, os2 = 0, s2 = 0;
			if (psx < 0) {
				psx = -psx;
				os1 = 180;
				s2 = -1;
			}
			else {
				os1 = 0;
				s2 = 1;
			}
			if (psy < 0) {
				psy = -psy;
				s2 = -s2;
			}
			if (csx < 0) {
				csx = -csx;
				os2 = 180;
			}
			else
				os2 = 0;
			var cx = child.x, cy = 0, cwx = 0, cwy = 0, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
			var u = Math.abs(psx - psy) <= 0.0001;
			if (!u) {
				cy = 0;
				cwx = a * cx + parent.worldX;
				cwy = c * cx + parent.worldY;
			}
			else {
				cy = child.y;
				cwx = a * cx + b * cy + parent.worldX;
				cwy = c * cx + d * cy + parent.worldY;
			}
			var pp = parent.parent;
			a = pp.a;
			b = pp.b;
			c = pp.c;
			d = pp.d;
			var id = 1 / (a * d - b * c), x = targetX - pp.worldX, y = targetY - pp.worldY;
			var tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
			x = cwx - pp.worldX;
			y = cwy - pp.worldY;
			var dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
			var l1 = Math.sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1 = 0, a2 = 0;
			outer: if (u) {
				l2 *= psx;
				var cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
				if (cos < -1)
					cos = -1;
				else if (cos > 1)
					cos = 1;
				a2 = Math.acos(cos) * bendDir;
				a = l1 + l2 * cos;
				b = l2 * Math.sin(a2);
				a1 = Math.atan2(ty * a - tx * b, tx * a + ty * b);
			}
			else {
				a = psx * l2;
				b = psy * l2;
				var aa = a * a, bb = b * b, dd = tx * tx + ty * ty, ta = Math.atan2(ty, tx);
				c = bb * l1 * l1 + aa * dd - aa * bb;
				var c1 = -2 * bb * l1, c2 = bb - aa;
				d = c1 * c1 - 4 * c2 * c;
				if (d >= 0) {
					var q = Math.sqrt(d);
					if (c1 < 0)
						q = -q;
					q = -(c1 + q) / 2;
					var r0 = q / c2, r1 = c / q;
					var r = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
					if (r * r <= dd) {
						y = Math.sqrt(dd - r * r) * bendDir;
						a1 = ta - Math.atan2(y, r);
						a2 = Math.atan2(y / psy, (r - l1) / psx);
						break outer;
					}
				}
				var minAngle = 0, minDist = Number.MAX_VALUE, minX = 0, minY = 0;
				var maxAngle = 0, maxDist = 0, maxX = 0, maxY = 0;
				x = l1 + a;
				d = x * x;
				if (d > maxDist) {
					maxAngle = 0;
					maxDist = d;
					maxX = x;
				}
				x = l1 - a;
				d = x * x;
				if (d < minDist) {
					minAngle = spine.MathUtils.PI;
					minDist = d;
					minX = x;
				}
				var angle = Math.acos(-a * l1 / (aa - bb));
				x = a * Math.cos(angle) + l1;
				y = b * Math.sin(angle);
				d = x * x + y * y;
				if (d < minDist) {
					minAngle = angle;
					minDist = d;
					minX = x;
					minY = y;
				}
				if (d > maxDist) {
					maxAngle = angle;
					maxDist = d;
					maxX = x;
					maxY = y;
				}
				if (dd <= (minDist + maxDist) / 2) {
					a1 = ta - Math.atan2(minY * bendDir, minX);
					a2 = minAngle * bendDir;
				}
				else {
					a1 = ta - Math.atan2(maxY * bendDir, maxX);
					a2 = maxAngle * bendDir;
				}
			}
			var os = Math.atan2(cy, cx) * s2;
			var rotation = parent.rotation;
			a1 = (a1 - os) * spine.MathUtils.radDeg + os1 - rotation;
			if (a1 > 180)
				a1 -= 360;
			else if (a1 < -180)
				a1 += 360;
			parent.updateWorldTransformWith(px, py, rotation + a1 * alpha, parent.scaleX, parent.scaleY, 0, 0);
			rotation = child.rotation;
			a2 = ((a2 + os) * spine.MathUtils.radDeg - child.shearX) * s2 + os2 - rotation;
			if (a2 > 180)
				a2 -= 360;
			else if (a2 < -180)
				a2 += 360;
			child.updateWorldTransformWith(cx, cy, rotation + a2 * alpha, child.scaleX, child.scaleY, child.shearX, child.shearY);
		};
		return IkConstraint;
	}());
	spine.IkConstraint = IkConstraint;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var IkConstraintData = (function () {
		function IkConstraintData(name) {
			this.bones = new Array();
			this.bendDirection = 1;
			this.mix = 1;
			this.name = name;
		}
		return IkConstraintData;
	}());
	spine.IkConstraintData = IkConstraintData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var PathConstraint = (function () {
		function PathConstraint(data, skeleton) {
			this.position = 0;
			this.spacing = 0;
			this.rotateMix = 0;
			this.translateMix = 0;
			this.spaces = new Array();
			this.positions = new Array();
			this.world = new Array();
			this.curves = new Array();
			this.lengths = new Array();
			this.segments = new Array();
			if (data == null)
				throw new Error("data cannot be null.");
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.bones = new Array();
			for (var i = 0, n = data.bones.length; i < n; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findSlot(data.target.name);
			this.position = data.position;
			this.spacing = data.spacing;
			this.rotateMix = data.rotateMix;
			this.translateMix = data.translateMix;
		}
		PathConstraint.prototype.apply = function () {
			this.update();
		};
		PathConstraint.prototype.update = function () {
			var attachment = this.target.getAttachment();
			if (!(attachment instanceof spine.PathAttachment))
				return;
			var rotateMix = this.rotateMix, translateMix = this.translateMix;
			var translate = translateMix > 0, rotate = rotateMix > 0;
			if (!translate && !rotate)
				return;
			var data = this.data;
			var spacingMode = data.spacingMode;
			var lengthSpacing = spacingMode == spine.SpacingMode.Length;
			var rotateMode = data.rotateMode;
			var tangents = rotateMode == spine.RotateMode.Tangent, scale = rotateMode == spine.RotateMode.ChainScale;
			var boneCount = this.bones.length, spacesCount = tangents ? boneCount : boneCount + 1;
			var bones = this.bones;
			var spaces = spine.Utils.setArraySize(this.spaces, spacesCount), lengths = null;
			var spacing = this.spacing;
			if (scale || lengthSpacing) {
				if (scale)
					lengths = spine.Utils.setArraySize(this.lengths, boneCount);
				for (var i = 0, n = spacesCount - 1; i < n;) {
					var bone = bones[i];
					var length_1 = bone.data.length, x = length_1 * bone.a, y = length_1 * bone.c;
					length_1 = Math.sqrt(x * x + y * y);
					if (scale)
						lengths[i] = length_1;
					spaces[++i] = lengthSpacing ? Math.max(0, length_1 + spacing) : spacing;
				}
			}
			else {
				for (var i = 1; i < spacesCount; i++)
					spaces[i] = spacing;
			}
			var positions = this.computeWorldPositions(attachment, spacesCount, tangents, data.positionMode == spine.PositionMode.Percent, spacingMode == spine.SpacingMode.Percent);
			var skeleton = this.target.bone.skeleton;
			var skeletonX = skeleton.x, skeletonY = skeleton.y;
			var boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
			var tip = rotateMode == spine.RotateMode.Chain && offsetRotation == 0;
			for (var i = 0, p = 3; i < boneCount; i++, p += 3) {
				var bone = bones[i];
				bone.worldX += (boneX - skeletonX - bone.worldX) * translateMix;
				bone.worldY += (boneY - skeletonY - bone.worldY) * translateMix;
				var x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
				if (scale) {
					var length_2 = lengths[i];
					if (length_2 != 0) {
						var s = (Math.sqrt(dx * dx + dy * dy) / length_2 - 1) * rotateMix + 1;
						bone.a *= s;
						bone.c *= s;
					}
				}
				boneX = x;
				boneY = y;
				if (rotate) {
					var a = bone.a, b = bone.b, c = bone.c, d = bone.d, r = 0, cos = 0, sin = 0;
					if (tangents)
						r = positions[p - 1];
					else if (spaces[i + 1] == 0)
						r = positions[p + 2];
					else
						r = Math.atan2(dy, dx);
					r -= Math.atan2(c, a) - offsetRotation * spine.MathUtils.degRad;
					if (tip) {
						cos = Math.cos(r);
						sin = Math.sin(r);
						var length_3 = bone.data.length;
						boneX += (length_3 * (cos * a - sin * c) - dx) * rotateMix;
						boneY += (length_3 * (sin * a + cos * c) - dy) * rotateMix;
					}
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r *= rotateMix;
					cos = Math.cos(r);
					sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
			}
		};
		PathConstraint.prototype.computeWorldPositions = function (path, spacesCount, tangents, percentPosition, percentSpacing) {
			var target = this.target;
			var position = this.position;
			var spaces = this.spaces, out = spine.Utils.setArraySize(this.positions, spacesCount * 3 + 2), world = null;
			var closed = path.closed;
			var verticesLength = path.worldVerticesLength, curveCount = verticesLength / 6, prevCurve = PathConstraint.NONE;
			if (!path.constantSpeed) {
				var lengths = path.lengths;
				curveCount -= closed ? 1 : 2;
				var pathLength_1 = lengths[curveCount];
				if (percentPosition)
					position *= pathLength_1;
				if (percentSpacing) {
					for (var i = 0; i < spacesCount; i++)
						spaces[i] *= pathLength_1;
				}
				world = spine.Utils.setArraySize(this.world, 8);
				for (var i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
					var space = spaces[i];
					position += space;
					var p = position;
					if (closed) {
						p %= pathLength_1;
						if (p < 0)
							p += pathLength_1;
						curve = 0;
					}
					else if (p < 0) {
						if (prevCurve != PathConstraint.BEFORE) {
							prevCurve = PathConstraint.BEFORE;
							path.computeWorldVerticesWith(target, 2, 4, world, 0);
						}
						this.addBeforePosition(p, world, 0, out, o);
						continue;
					}
					else if (p > pathLength_1) {
						if (prevCurve != PathConstraint.AFTER) {
							prevCurve = PathConstraint.AFTER;
							path.computeWorldVerticesWith(target, verticesLength - 6, 4, world, 0);
						}
						this.addAfterPosition(p - pathLength_1, world, 0, out, o);
						continue;
					}
					for (;; curve++) {
						var length_4 = lengths[curve];
						if (p > length_4)
							continue;
						if (curve == 0)
							p /= length_4;
						else {
							var prev = lengths[curve - 1];
							p = (p - prev) / (length_4 - prev);
						}
						break;
					}
					if (curve != prevCurve) {
						prevCurve = curve;
						if (closed && curve == curveCount) {
							path.computeWorldVerticesWith(target, verticesLength - 4, 4, world, 0);
							path.computeWorldVerticesWith(target, 0, 4, world, 4);
						}
						else
							path.computeWorldVerticesWith(target, curve * 6 + 2, 8, world, 0);
					}
					this.addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o, tangents || (i > 0 && space == 0));
				}
				return out;
			}
			if (closed) {
				verticesLength += 2;
				world = spine.Utils.setArraySize(this.world, verticesLength);
				path.computeWorldVerticesWith(target, 2, verticesLength - 4, world, 0);
				path.computeWorldVerticesWith(target, 0, 2, world, verticesLength - 4);
				world[verticesLength - 2] = world[0];
				world[verticesLength - 1] = world[1];
			}
			else {
				curveCount--;
				verticesLength -= 4;
				world = spine.Utils.setArraySize(this.world, verticesLength);
				path.computeWorldVerticesWith(target, 2, verticesLength, world, 0);
			}
			var curves = spine.Utils.setArraySize(this.curves, curveCount);
			var pathLength = 0;
			var x1 = world[0], y1 = world[1], cx1 = 0, cy1 = 0, cx2 = 0, cy2 = 0, x2 = 0, y2 = 0;
			var tmpx = 0, tmpy = 0, dddfx = 0, dddfy = 0, ddfx = 0, ddfy = 0, dfx = 0, dfy = 0;
			for (var i = 0, w = 2; i < curveCount; i++, w += 6) {
				cx1 = world[w];
				cy1 = world[w + 1];
				cx2 = world[w + 2];
				cy2 = world[w + 3];
				x2 = world[w + 4];
				y2 = world[w + 5];
				tmpx = (x1 - cx1 * 2 + cx2) * 0.1875;
				tmpy = (y1 - cy1 * 2 + cy2) * 0.1875;
				dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375;
				dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375;
				ddfx = tmpx * 2 + dddfx;
				ddfy = tmpy * 2 + dddfy;
				dfx = (cx1 - x1) * 0.75 + tmpx + dddfx * 0.16666667;
				dfy = (cy1 - y1) * 0.75 + tmpy + dddfy * 0.16666667;
				pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
				dfx += ddfx;
				dfy += ddfy;
				pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
				dfx += ddfx + dddfx;
				dfy += ddfy + dddfy;
				pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
				curves[i] = pathLength;
				x1 = x2;
				y1 = y2;
			}
			if (percentPosition)
				position *= pathLength;
			if (percentSpacing) {
				for (var i = 0; i < spacesCount; i++)
					spaces[i] *= pathLength;
			}
			var segments = this.segments;
			var curveLength = 0;
			for (var i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
				var space = spaces[i];
				position += space;
				var p = position;
				if (closed) {
					p %= pathLength;
					if (p < 0)
						p += pathLength;
					curve = 0;
				}
				else if (p < 0) {
					this.addBeforePosition(p, world, 0, out, o);
					continue;
				}
				else if (p > pathLength) {
					this.addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
					continue;
				}
				for (;; curve++) {
					var length_5 = curves[curve];
					if (p > length_5)
						continue;
					if (curve == 0)
						p /= length_5;
					else {
						var prev = curves[curve - 1];
						p = (p - prev) / (length_5 - prev);
					}
					break;
				}
				if (curve != prevCurve) {
					prevCurve = curve;
					var ii = curve * 6;
					x1 = world[ii];
					y1 = world[ii + 1];
					cx1 = world[ii + 2];
					cy1 = world[ii + 3];
					cx2 = world[ii + 4];
					cy2 = world[ii + 5];
					x2 = world[ii + 6];
					y2 = world[ii + 7];
					tmpx = (x1 - cx1 * 2 + cx2) * 0.03;
					tmpy = (y1 - cy1 * 2 + cy2) * 0.03;
					dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006;
					dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006;
					ddfx = tmpx * 2 + dddfx;
					ddfy = tmpy * 2 + dddfy;
					dfx = (cx1 - x1) * 0.3 + tmpx + dddfx * 0.16666667;
					dfy = (cy1 - y1) * 0.3 + tmpy + dddfy * 0.16666667;
					curveLength = Math.sqrt(dfx * dfx + dfy * dfy);
					segments[0] = curveLength;
					for (ii = 1; ii < 8; ii++) {
						dfx += ddfx;
						dfy += ddfy;
						ddfx += dddfx;
						ddfy += dddfy;
						curveLength += Math.sqrt(dfx * dfx + dfy * dfy);
						segments[ii] = curveLength;
					}
					dfx += ddfx;
					dfy += ddfy;
					curveLength += Math.sqrt(dfx * dfx + dfy * dfy);
					segments[8] = curveLength;
					dfx += ddfx + dddfx;
					dfy += ddfy + dddfy;
					curveLength += Math.sqrt(dfx * dfx + dfy * dfy);
					segments[9] = curveLength;
					segment = 0;
				}
				p *= curveLength;
				for (;; segment++) {
					var length_6 = segments[segment];
					if (p > length_6)
						continue;
					if (segment == 0)
						p /= length_6;
					else {
						var prev = segments[segment - 1];
						p = segment + (p - prev) / (length_6 - prev);
					}
					break;
				}
				this.addCurvePosition(p * 0.1, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space == 0));
			}
			return out;
		};
		PathConstraint.prototype.addBeforePosition = function (p, temp, i, out, o) {
			var x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = Math.atan2(dy, dx);
			out[o] = x1 + p * Math.cos(r);
			out[o + 1] = y1 + p * Math.sin(r);
			out[o + 2] = r;
		};
		PathConstraint.prototype.addAfterPosition = function (p, temp, i, out, o) {
			var x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = Math.atan2(dy, dx);
			out[o] = x1 + p * Math.cos(r);
			out[o + 1] = y1 + p * Math.sin(r);
			out[o + 2] = r;
		};
		PathConstraint.prototype.addCurvePosition = function (p, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents) {
			if (p == 0)
				p = 0.0001;
			var tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
			var ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
			var x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
			out[o] = x;
			out[o + 1] = y;
			if (tangents)
				out[o + 2] = Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		};
		PathConstraint.NONE = -1;
		PathConstraint.BEFORE = -2;
		PathConstraint.AFTER = -3;
		return PathConstraint;
	}());
	spine.PathConstraint = PathConstraint;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var PathConstraintData = (function () {
		function PathConstraintData(name) {
			this.bones = new Array();
			this.name = name;
		}
		return PathConstraintData;
	}());
	spine.PathConstraintData = PathConstraintData;
	(function (PositionMode) {
		PositionMode[PositionMode["Fixed"] = 0] = "Fixed";
		PositionMode[PositionMode["Percent"] = 1] = "Percent";
	})(spine.PositionMode || (spine.PositionMode = {}));
	var PositionMode = spine.PositionMode;
	(function (SpacingMode) {
		SpacingMode[SpacingMode["Length"] = 0] = "Length";
		SpacingMode[SpacingMode["Fixed"] = 1] = "Fixed";
		SpacingMode[SpacingMode["Percent"] = 2] = "Percent";
	})(spine.SpacingMode || (spine.SpacingMode = {}));
	var SpacingMode = spine.SpacingMode;
	(function (RotateMode) {
		RotateMode[RotateMode["Tangent"] = 0] = "Tangent";
		RotateMode[RotateMode["Chain"] = 1] = "Chain";
		RotateMode[RotateMode["ChainScale"] = 2] = "ChainScale";
	})(spine.RotateMode || (spine.RotateMode = {}));
	var RotateMode = spine.RotateMode;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Assets = (function () {
		function Assets(clientId) {
			this.toLoad = new Array();
			this.assets = {};
			this.clientId = clientId;
		}
		Assets.prototype.loaded = function () {
			var i = 0;
			for (var v in this.assets)
				i++;
			return i;
		};
		return Assets;
	}());
	var SharedAssetManager = (function () {
		function SharedAssetManager(pathPrefix) {
			if (pathPrefix === void 0) { pathPrefix = ""; }
			this.clientAssets = {};
			this.queuedAssets = {};
			this.rawAssets = {};
			this.errors = {};
			this.pathPrefix = pathPrefix;
		}
		SharedAssetManager.prototype.queueAsset = function (clientId, textureLoader, path) {
			var clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined) {
				clientAssets = new Assets(clientId);
				this.clientAssets[clientId] = clientAssets;
			}
			if (textureLoader !== null)
				clientAssets.textureLoader = textureLoader;
			clientAssets.toLoad.push(path);
			if (this.queuedAssets[path] === path) {
				return false;
			}
			else {
				this.queuedAssets[path] = path;
				return true;
			}
		};
		SharedAssetManager.prototype.loadText = function (clientId, path) {
			var _this = this;
			path = this.pathPrefix + path;
			if (!this.queueAsset(clientId, null, path))
				return;
			var request = new XMLHttpRequest();
			request.onreadystatechange = function () {
				if (request.readyState == XMLHttpRequest.DONE) {
					if (request.status >= 200 && request.status < 300) {
						_this.rawAssets[path] = request.responseText;
					}
					else {
						_this.errors[path] = "Couldn't load text " + path + ": status " + request.status + ", " + request.responseText;
					}
				}
			};
			request.open("GET", path, true);
			request.send();
		};
		SharedAssetManager.prototype.loadJson = function (clientId, path) {
			var _this = this;
			path = this.pathPrefix + path;
			if (!this.queueAsset(clientId, null, path))
				return;
			var request = new XMLHttpRequest();
			request.onreadystatechange = function () {
				if (request.readyState == XMLHttpRequest.DONE) {
					if (request.status >= 200 && request.status < 300) {
						_this.rawAssets[path] = JSON.parse(request.responseText);
					}
					else {
						_this.errors[path] = "Couldn't load text " + path + ": status " + request.status + ", " + request.responseText;
					}
				}
			};
			request.open("GET", path, true);
			request.send();
		};
		SharedAssetManager.prototype.loadTexture = function (clientId, textureLoader, path) {
			var _this = this;
			path = this.pathPrefix + path;
			if (!this.queueAsset(clientId, textureLoader, path))
				return;
			var img = new Image();
			img.src = path;
			img.crossOrigin = "anonymous";
			img.onload = function (ev) {
				_this.rawAssets[path] = img;
			};
			img.onerror = function (ev) {
				_this.errors[path] = "Couldn't load image " + path;
			};
		};
		SharedAssetManager.prototype.get = function (clientId, path) {
			path = this.pathPrefix + path;
			var clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined)
				return true;
			return clientAssets.assets[path];
		};
		SharedAssetManager.prototype.updateClientAssets = function (clientAssets) {
			for (var i = 0; i < clientAssets.toLoad.length; i++) {
				var path = clientAssets.toLoad[i];
				var asset = clientAssets.assets[path];
				if (asset === null || asset === undefined) {
					var rawAsset = this.rawAssets[path];
					if (rawAsset === null || rawAsset === undefined)
						continue;
					if (rawAsset instanceof HTMLImageElement) {
						clientAssets.assets[path] = clientAssets.textureLoader(rawAsset);
					}
					else {
						clientAssets.assets[path] = rawAsset;
					}
				}
			}
		};
		SharedAssetManager.prototype.isLoadingComplete = function (clientId) {
			var clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined)
				return true;
			this.updateClientAssets(clientAssets);
			return clientAssets.toLoad.length == clientAssets.loaded();
		};
		SharedAssetManager.prototype.dispose = function () {
		};
		SharedAssetManager.prototype.hasErrors = function () {
			return Object.keys(this.errors).length > 0;
		};
		SharedAssetManager.prototype.getErrors = function () {
			return this.errors;
		};
		return SharedAssetManager;
	}());
	spine.SharedAssetManager = SharedAssetManager;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Skeleton = (function () {
		function Skeleton(data) {
			this._updateCache = new Array();
			this.time = 0;
			this.flipX = false;
			this.flipY = false;
			this.x = 0;
			this.y = 0;
			if (data == null)
				throw new Error("data cannot be null.");
			this.data = data;
			this.bones = new Array();
			for (var i = 0; i < data.bones.length; i++) {
				var boneData = data.bones[i];
				var bone = void 0;
				if (boneData.parent == null)
					bone = new spine.Bone(boneData, this, null);
				else {
					var parent_1 = this.bones[boneData.parent.index];
					bone = new spine.Bone(boneData, this, parent_1);
					parent_1.children.push(bone);
				}
				this.bones.push(bone);
			}
			this.slots = new Array();
			this.drawOrder = new Array();
			for (var i = 0; i < data.slots.length; i++) {
				var slotData = data.slots[i];
				var bone = this.bones[slotData.boneData.index];
				var slot = new spine.Slot(slotData, bone);
				this.slots.push(slot);
				this.drawOrder.push(slot);
			}
			this.ikConstraints = new Array();
			this.ikConstraintsSorted = new Array();
			for (var i = 0; i < data.ikConstraints.length; i++) {
				var ikConstraintData = data.ikConstraints[i];
				this.ikConstraints.push(new spine.IkConstraint(ikConstraintData, this));
			}
			this.transformConstraints = new Array();
			for (var i = 0; i < data.transformConstraints.length; i++) {
				var transformConstraintData = data.transformConstraints[i];
				this.transformConstraints.push(new spine.TransformConstraint(transformConstraintData, this));
			}
			this.pathConstraints = new Array();
			for (var i = 0; i < data.pathConstraints.length; i++) {
				var pathConstraintData = data.pathConstraints[i];
				this.pathConstraints.push(new spine.PathConstraint(pathConstraintData, this));
			}
			this.color = new spine.Color(1, 1, 1, 1);
			this.updateCache();
		}
		Skeleton.prototype.updateCache = function () {
			var updateCache = this._updateCache;
			updateCache.length = 0;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++)
				bones[i].sorted = false;
			var ikConstraints = this.ikConstraintsSorted;
			ikConstraints.length = 0;
			for (var i = 0; i < this.ikConstraints.length; i++)
				ikConstraints.push(this.ikConstraints[i]);
			var ikCount = ikConstraints.length;
			for (var i = 0, level = 0, n = ikCount; i < n; i++) {
				var ik = ikConstraints[i];
				var bone = ik.bones[0].parent;
				for (level = 0; bone != null; level++)
					bone = bone.parent;
				ik.level = level;
			}
			for (var i = 1, ii = 0; i < ikCount; i++) {
				var ik = ikConstraints[i];
				var level = ik.level;
				for (ii = i - 1; ii >= 0; ii--) {
					var other = ikConstraints[ii];
					if (other.level < level)
						break;
					ikConstraints[ii + 1] = other;
				}
				ikConstraints[ii + 1] = ik;
			}
			for (var i = 0, n = ikConstraints.length; i < n; i++) {
				var constraint = ikConstraints[i];
				var target = constraint.target;
				this.sortBone(target);
				var constrained = constraint.bones;
				var parent_2 = constrained[0];
				this.sortBone(parent_2);
				updateCache.push(constraint);
				this.sortReset(parent_2.children);
				constrained[constrained.length - 1].sorted = true;
			}
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++) {
				var constraint = pathConstraints[i];
				var slot = constraint.target;
				var slotIndex = slot.data.index;
				var slotBone = slot.bone;
				if (this.skin != null)
					this.sortPathConstraintAttachment(this.skin, slotIndex, slotBone);
				if (this.data.defaultSkin != null && this.data.defaultSkin != this.skin)
					this.sortPathConstraintAttachment(this.data.defaultSkin, slotIndex, slotBone);
				for (var ii = 0, nn = this.data.skins.length; ii < nn; ii++)
					this.sortPathConstraintAttachment(this.data.skins[ii], slotIndex, slotBone);
				var attachment = slot.getAttachment();
				if (attachment instanceof spine.PathAttachment)
					this.sortPathConstraintAttachmentWith(attachment, slotBone);
				var constrained = constraint.bones;
				var boneCount = constrained.length;
				for (var ii = 0; ii < boneCount; ii++)
					this.sortBone(constrained[ii]);
				updateCache.push(constraint);
				for (var ii = 0; ii < boneCount; ii++)
					this.sortReset(constrained[ii].children);
				for (var ii = 0; ii < boneCount; ii++)
					constrained[ii].sorted = true;
			}
			var transformConstraints = this.transformConstraints;
			for (var i = 0, n = transformConstraints.length; i < n; i++) {
				var constraint = transformConstraints[i];
				this.sortBone(constraint.target);
				var constrained = constraint.bones;
				var boneCount = constrained.length;
				for (var ii = 0; ii < boneCount; ii++)
					this.sortBone(constrained[ii]);
				updateCache.push(constraint);
				for (var ii = 0; ii < boneCount; ii++)
					this.sortReset(constrained[ii].children);
				for (var ii = 0; ii < boneCount; ii++)
					constrained[ii].sorted = true;
			}
			for (var i = 0, n = bones.length; i < n; i++)
				this.sortBone(bones[i]);
		};
		Skeleton.prototype.sortPathConstraintAttachment = function (skin, slotIndex, slotBone) {
			var attachments = skin.attachments[slotIndex];
			if (!attachments)
				return;
			for (var key in attachments) {
				this.sortPathConstraintAttachmentWith(attachments[key], slotBone);
			}
		};
		Skeleton.prototype.sortPathConstraintAttachmentWith = function (attachment, slotBone) {
			if (!(attachment instanceof spine.PathAttachment))
				return;
			var pathBones = attachment.bones;
			if (pathBones == null)
				this.sortBone(slotBone);
			else {
				var bones = this.bones;
				for (var i = 0; i < pathBones.length; i++) {
					var boneIndex = pathBones[i];
					this.sortBone(bones[boneIndex]);
				}
			}
		};
		Skeleton.prototype.sortBone = function (bone) {
			if (bone.sorted)
				return;
			var parent = bone.parent;
			if (parent != null)
				this.sortBone(parent);
			bone.sorted = true;
			this._updateCache.push(bone);
		};
		Skeleton.prototype.sortReset = function (bones) {
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (bone.sorted)
					this.sortReset(bone.children);
				bone.sorted = false;
			}
		};
		Skeleton.prototype.updateWorldTransform = function () {
			var updateCache = this._updateCache;
			for (var i = 0, n = updateCache.length; i < n; i++)
				updateCache[i].update();
		};
		Skeleton.prototype.setToSetupPose = function () {
			this.setBonesToSetupPose();
			this.setSlotsToSetupPose();
		};
		Skeleton.prototype.setBonesToSetupPose = function () {
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++)
				bones[i].setToSetupPose();
			var ikConstraints = this.ikConstraints;
			for (var i = 0, n = ikConstraints.length; i < n; i++) {
				var constraint = ikConstraints[i];
				constraint.bendDirection = constraint.data.bendDirection;
				constraint.mix = constraint.data.mix;
			}
			var transformConstraints = this.transformConstraints;
			for (var i = 0, n = transformConstraints.length; i < n; i++) {
				var constraint = transformConstraints[i];
				var data = constraint.data;
				constraint.rotateMix = data.rotateMix;
				constraint.translateMix = data.translateMix;
				constraint.scaleMix = data.scaleMix;
				constraint.shearMix = data.shearMix;
			}
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++) {
				var constraint = pathConstraints[i];
				var data = constraint.data;
				constraint.position = data.position;
				constraint.spacing = data.spacing;
				constraint.rotateMix = data.rotateMix;
				constraint.translateMix = data.translateMix;
			}
		};
		Skeleton.prototype.setSlotsToSetupPose = function () {
			var slots = this.slots;
			spine.Utils.arrayCopy(slots, 0, this.drawOrder, 0, slots.length);
			for (var i = 0, n = slots.length; i < n; i++)
				slots[i].setToSetupPose();
		};
		Skeleton.prototype.getRootBone = function () {
			if (this.bones.length == 0)
				return null;
			return this.bones[0];
		};
		Skeleton.prototype.findBone = function (boneName) {
			if (boneName == null)
				throw new Error("boneName cannot be null.");
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (bone.data.name == boneName)
					return bone;
			}
			return null;
		};
		Skeleton.prototype.findBoneIndex = function (boneName) {
			if (boneName == null)
				throw new Error("boneName cannot be null.");
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++)
				if (bones[i].data.name == boneName)
					return i;
			return -1;
		};
		Skeleton.prototype.findSlot = function (slotName) {
			if (slotName == null)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++) {
				var slot = slots[i];
				if (slot.data.name == slotName)
					return slot;
			}
			return null;
		};
		Skeleton.prototype.findSlotIndex = function (slotName) {
			if (slotName == null)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++)
				if (slots[i].data.name == slotName)
					return i;
			return -1;
		};
		Skeleton.prototype.setSkinByName = function (skinName) {
			var skin = this.data.findSkin(skinName);
			if (skin == null)
				throw new Error("Skin not found: " + skinName);
			this.setSkin(skin);
		};
		Skeleton.prototype.setSkin = function (newSkin) {
			if (newSkin != null) {
				if (this.skin != null)
					newSkin.attachAll(this, this.skin);
				else {
					var slots = this.slots;
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						var name_1 = slot.data.attachmentName;
						if (name_1 != null) {
							var attachment = newSkin.getAttachment(i, name_1);
							if (attachment != null)
								slot.setAttachment(attachment);
						}
					}
				}
			}
			this.skin = newSkin;
		};
		Skeleton.prototype.getAttachmentByName = function (slotName, attachmentName) {
			return this.getAttachment(this.data.findSlotIndex(slotName), attachmentName);
		};
		Skeleton.prototype.getAttachment = function (slotIndex, attachmentName) {
			if (attachmentName == null)
				throw new Error("attachmentName cannot be null.");
			if (this.skin != null) {
				var attachment = this.skin.getAttachment(slotIndex, attachmentName);
				if (attachment != null)
					return attachment;
			}
			if (this.data.defaultSkin != null)
				return this.data.defaultSkin.getAttachment(slotIndex, attachmentName);
			return null;
		};
		Skeleton.prototype.setAttachment = function (slotName, attachmentName) {
			if (slotName == null)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++) {
				var slot = slots[i];
				if (slot.data.name == slotName) {
					var attachment = null;
					if (attachmentName != null) {
						attachment = this.getAttachment(i, attachmentName);
						if (attachment == null)
							throw new Error("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.setAttachment(attachment);
					return;
				}
			}
			throw new Error("Slot not found: " + slotName);
		};
		Skeleton.prototype.findIkConstraint = function (constraintName) {
			if (constraintName == null)
				throw new Error("constraintName cannot be null.");
			var ikConstraints = this.ikConstraints;
			for (var i = 0, n = ikConstraints.length; i < n; i++) {
				var ikConstraint = ikConstraints[i];
				if (ikConstraint.data.name == constraintName)
					return ikConstraint;
			}
			return null;
		};
		Skeleton.prototype.findTransformConstraint = function (constraintName) {
			if (constraintName == null)
				throw new Error("constraintName cannot be null.");
			var transformConstraints = this.transformConstraints;
			for (var i = 0, n = transformConstraints.length; i < n; i++) {
				var constraint = transformConstraints[i];
				if (constraint.data.name == constraintName)
					return constraint;
			}
			return null;
		};
		Skeleton.prototype.findPathConstraint = function (constraintName) {
			if (constraintName == null)
				throw new Error("constraintName cannot be null.");
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++) {
				var constraint = pathConstraints[i];
				if (constraint.data.name == constraintName)
					return constraint;
			}
			return null;
		};
		Skeleton.prototype.getBounds = function (offset, size) {
			if (offset == null)
				throw new Error("offset cannot be null.");
			if (size == null)
				throw new Error("size cannot be null.");
			var drawOrder = this.drawOrder;
			var minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
			for (var i = 0, n = drawOrder.length; i < n; i++) {
				var slot = drawOrder[i];
				var vertices = null;
				var attachment = slot.getAttachment();
				if (attachment instanceof spine.RegionAttachment)
					vertices = attachment.updateWorldVertices(slot, false);
				else if (attachment instanceof spine.MeshAttachment)
					vertices = attachment.updateWorldVertices(slot, true);
				if (vertices != null) {
					for (var ii = 0, nn = vertices.length; ii < nn; ii += 8) {
						var x = vertices[ii], y = vertices[ii + 1];
						minX = Math.min(minX, x);
						minY = Math.min(minY, y);
						maxX = Math.max(maxX, x);
						maxY = Math.max(maxY, y);
					}
				}
			}
			offset.set(minX, minY);
			size.set(maxX - minX, maxY - minY);
		};
		Skeleton.prototype.update = function (delta) {
			this.time += delta;
		};
		return Skeleton;
	}());
	spine.Skeleton = Skeleton;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SkeletonBounds = (function () {
		function SkeletonBounds() {
			this.minX = 0;
			this.minY = 0;
			this.maxX = 0;
			this.maxY = 0;
			this.boundingBoxes = new Array();
			this.polygons = new Array();
			this.polygonPool = new spine.Pool(function () {
				return spine.Utils.newFloatArray(16);
			});
		}
		SkeletonBounds.prototype.update = function (skeleton, updateAabb) {
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			var boundingBoxes = this.boundingBoxes;
			var polygons = this.polygons;
			var polygonPool = this.polygonPool;
			var slots = skeleton.slots;
			var slotCount = slots.length;
			boundingBoxes.length = 0;
			polygonPool.freeAll(polygons);
			polygons.length = 0;
			for (var i = 0; i < slotCount; i++) {
				var slot = slots[i];
				var attachment = slot.getAttachment();
				if (attachment instanceof spine.BoundingBoxAttachment) {
					var boundingBox = attachment;
					boundingBoxes.push(boundingBox);
					var polygon = polygonPool.obtain();
					if (polygon.length != boundingBox.worldVerticesLength) {
						polygon = spine.Utils.newFloatArray(boundingBox.worldVerticesLength);
					}
					polygons.push(polygon);
					boundingBox.computeWorldVertices(slot, polygon);
				}
			}
			if (updateAabb)
				this.aabbCompute();
		};
		SkeletonBounds.prototype.aabbCompute = function () {
			var minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
			var polygons = this.polygons;
			for (var i = 0, n = polygons.length; i < n; i++) {
				var polygon = polygons[i];
				var vertices = polygon;
				for (var ii = 0, nn = polygon.length; ii < nn; ii += 2) {
					var x = vertices[ii];
					var y = vertices[ii + 1];
					minX = Math.min(minX, x);
					minY = Math.min(minY, y);
					maxX = Math.max(maxX, x);
					maxY = Math.max(maxY, y);
				}
			}
			this.minX = minX;
			this.minY = minY;
			this.maxX = maxX;
			this.maxY = maxY;
		};
		SkeletonBounds.prototype.aabbContainsPoint = function (x, y) {
			return x >= this.minX && x <= this.maxX && y >= this.minY && y <= this.maxY;
		};
		SkeletonBounds.prototype.aabbIntersectsSegment = function (x1, y1, x2, y2) {
			var minX = this.minX;
			var minY = this.minY;
			var maxX = this.maxX;
			var maxY = this.maxY;
			if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
				return false;
			var m = (y2 - y1) / (x2 - x1);
			var y = m * (minX - x1) + y1;
			if (y > minY && y < maxY)
				return true;
			y = m * (maxX - x1) + y1;
			if (y > minY && y < maxY)
				return true;
			var x = (minY - y1) / m + x1;
			if (x > minX && x < maxX)
				return true;
			x = (maxY - y1) / m + x1;
			if (x > minX && x < maxX)
				return true;
			return false;
		};
		SkeletonBounds.prototype.aabbIntersectsSkeleton = function (bounds) {
			return this.minX < bounds.maxX && this.maxX > bounds.minX && this.minY < bounds.maxY && this.maxY > bounds.minY;
		};
		SkeletonBounds.prototype.containsPoint = function (x, y) {
			var polygons = this.polygons;
			for (var i = 0, n = polygons.length; i < n; i++)
				if (this.containsPointPolygon(polygons[i], x, y))
					return this.boundingBoxes[i];
			return null;
		};
		SkeletonBounds.prototype.containsPointPolygon = function (polygon, x, y) {
			var vertices = polygon;
			var nn = polygon.length;
			var prevIndex = nn - 2;
			var inside = false;
			for (var ii = 0; ii < nn; ii += 2) {
				var vertexY = vertices[ii + 1];
				var prevY = vertices[prevIndex + 1];
				if ((vertexY < y && prevY >= y) || (prevY < y && vertexY >= y)) {
					var vertexX = vertices[ii];
					if (vertexX + (y - vertexY) / (prevY - vertexY) * (vertices[prevIndex] - vertexX) < x)
						inside = !inside;
				}
				prevIndex = ii;
			}
			return inside;
		};
		SkeletonBounds.prototype.intersectsSegment = function (x1, y1, x2, y2) {
			var polygons = this.polygons;
			for (var i = 0, n = polygons.length; i < n; i++)
				if (this.intersectsSegmentPolygon(polygons[i], x1, y1, x2, y2))
					return this.boundingBoxes[i];
			return null;
		};
		SkeletonBounds.prototype.intersectsSegmentPolygon = function (polygon, x1, y1, x2, y2) {
			var vertices = polygon;
			var nn = polygon.length;
			var width12 = x1 - x2, height12 = y1 - y2;
			var det1 = x1 * y2 - y1 * x2;
			var x3 = vertices[nn - 2], y3 = vertices[nn - 1];
			for (var ii = 0; ii < nn; ii += 2) {
				var x4 = vertices[ii], y4 = vertices[ii + 1];
				var det2 = x3 * y4 - y3 * x4;
				var width34 = x3 - x4, height34 = y3 - y4;
				var det3 = width12 * height34 - height12 * width34;
				var x = (det1 * width34 - width12 * det2) / det3;
				if (((x >= x3 && x <= x4) || (x >= x4 && x <= x3)) && ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))) {
					var y = (det1 * height34 - height12 * det2) / det3;
					if (((y >= y3 && y <= y4) || (y >= y4 && y <= y3)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1)))
						return true;
				}
				x3 = x4;
				y3 = y4;
			}
			return false;
		};
		SkeletonBounds.prototype.getPolygon = function (boundingBox) {
			if (boundingBox == null)
				throw new Error("boundingBox cannot be null.");
			var index = this.boundingBoxes.indexOf(boundingBox);
			return index == -1 ? null : this.polygons[index];
		};
		SkeletonBounds.prototype.getWidth = function () {
			return this.maxX - this.minX;
		};
		SkeletonBounds.prototype.getHeight = function () {
			return this.maxY - this.minY;
		};
		return SkeletonBounds;
	}());
	spine.SkeletonBounds = SkeletonBounds;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SkeletonData = (function () {
		function SkeletonData() {
			this.bones = new Array();
			this.slots = new Array();
			this.skins = new Array();
			this.events = new Array();
			this.animations = new Array();
			this.ikConstraints = new Array();
			this.transformConstraints = new Array();
			this.pathConstraints = new Array();
		}
		SkeletonData.prototype.findBone = function (boneName) {
			if (boneName == null)
				throw new Error("boneName cannot be null.");
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (bone.name == boneName)
					return bone;
			}
			return null;
		};
		SkeletonData.prototype.findBoneIndex = function (boneName) {
			if (boneName == null)
				throw new Error("boneName cannot be null.");
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++)
				if (bones[i].name == boneName)
					return i;
			return -1;
		};
		SkeletonData.prototype.findSlot = function (slotName) {
			if (slotName == null)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++) {
				var slot = slots[i];
				if (slot.name == slotName)
					return slot;
			}
			return null;
		};
		SkeletonData.prototype.findSlotIndex = function (slotName) {
			if (slotName == null)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++)
				if (slots[i].name == slotName)
					return i;
			return -1;
		};
		SkeletonData.prototype.findSkin = function (skinName) {
			if (skinName == null)
				throw new Error("skinName cannot be null.");
			var skins = this.skins;
			for (var i = 0, n = skins.length; i < n; i++) {
				var skin = skins[i];
				if (skin.name == skinName)
					return skin;
			}
			return null;
		};
		SkeletonData.prototype.findEvent = function (eventDataName) {
			if (eventDataName == null)
				throw new Error("eventDataName cannot be null.");
			var events = this.events;
			for (var i = 0, n = events.length; i < n; i++) {
				var event_2 = events[i];
				if (event_2.name == eventDataName)
					return event_2;
			}
			return null;
		};
		SkeletonData.prototype.findAnimation = function (animationName) {
			if (animationName == null)
				throw new Error("animationName cannot be null.");
			var animations = this.animations;
			for (var i = 0, n = animations.length; i < n; i++) {
				var animation = animations[i];
				if (animation.name == animationName)
					return animation;
			}
			return null;
		};
		SkeletonData.prototype.findIkConstraint = function (constraintName) {
			if (constraintName == null)
				throw new Error("constraintName cannot be null.");
			var ikConstraints = this.ikConstraints;
			for (var i = 0, n = ikConstraints.length; i < n; i++) {
				var constraint = ikConstraints[i];
				if (constraint.name == constraintName)
					return constraint;
			}
			return null;
		};
		SkeletonData.prototype.findTransformConstraint = function (constraintName) {
			if (constraintName == null)
				throw new Error("constraintName cannot be null.");
			var transformConstraints = this.transformConstraints;
			for (var i = 0, n = transformConstraints.length; i < n; i++) {
				var constraint = transformConstraints[i];
				if (constraint.name == constraintName)
					return constraint;
			}
			return null;
		};
		SkeletonData.prototype.findPathConstraint = function (constraintName) {
			if (constraintName == null)
				throw new Error("constraintName cannot be null.");
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++) {
				var constraint = pathConstraints[i];
				if (constraint.name == constraintName)
					return constraint;
			}
			return null;
		};
		SkeletonData.prototype.findPathConstraintIndex = function (pathConstraintName) {
			if (pathConstraintName == null)
				throw new Error("pathConstraintName cannot be null.");
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++)
				if (pathConstraints[i].name == pathConstraintName)
					return i;
			return -1;
		};
		return SkeletonData;
	}());
	spine.SkeletonData = SkeletonData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SkeletonJson = (function () {
		function SkeletonJson(attachmentLoader) {
			this.scale = 1;
			this.linkedMeshes = new Array();
			this.attachmentLoader = attachmentLoader;
		}
		SkeletonJson.prototype.readSkeletonData = function (json) {
			var scale = this.scale;
			var skeletonData = new spine.SkeletonData();
			var root = typeof (json) === "string" ? JSON.parse(json) : json;
			var skeletonMap = root.skeleton;
			if (skeletonMap != null) {
				skeletonData.hash = skeletonMap.hash;
				skeletonData.version = skeletonMap.spine;
				skeletonData.width = skeletonMap.width;
				skeletonData.height = skeletonMap.height;
				skeletonData.imagesPath = skeletonMap.images;
			}
			if (root.bones) {
				for (var i = 0; i < root.bones.length; i++) {
					var boneMap = root.bones[i];
					var parent_3 = null;
					var parentName = this.getValue(boneMap, "parent", null);
					if (parentName != null) {
						parent_3 = skeletonData.findBone(parentName);
						if (parent_3 == null)
							throw new Error("Parent bone not found: " + parentName);
					}
					var data = new spine.BoneData(skeletonData.bones.length, boneMap.name, parent_3);
					data.length = this.getValue(boneMap, "length", 0) * scale;
					data.x = this.getValue(boneMap, "x", 0) * scale;
					data.y = this.getValue(boneMap, "y", 0) * scale;
					data.rotation = this.getValue(boneMap, "rotation", 0);
					data.scaleX = this.getValue(boneMap, "scaleX", 1);
					data.scaleY = this.getValue(boneMap, "scaleY", 1);
					data.shearX = this.getValue(boneMap, "shearX", 0);
					data.shearY = this.getValue(boneMap, "shearY", 0);
					data.inheritRotation = this.getValue(boneMap, "inheritRotation", true);
					data.inheritScale = this.getValue(boneMap, "inheritScale", true);
					skeletonData.bones.push(data);
				}
			}
			if (root.slots) {
				for (var i = 0; i < root.slots.length; i++) {
					var slotMap = root.slots[i];
					var slotName = slotMap.name;
					var boneName = slotMap.bone;
					var boneData = skeletonData.findBone(boneName);
					if (boneData == null)
						throw new Error("Slot bone not found: " + boneName);
					var data = new spine.SlotData(skeletonData.slots.length, slotName, boneData);
					var color = this.getValue(slotMap, "color", null);
					if (color != null)
						data.color.setFromString(color);
					data.attachmentName = this.getValue(slotMap, "attachment", null);
					data.blendMode = SkeletonJson.blendModeFromString(this.getValue(slotMap, "blend", "normal"));
					skeletonData.slots.push(data);
				}
			}
			if (root.ik) {
				for (var i = 0; i < root.ik.length; i++) {
					var constraintMap = root.ik[i];
					var data = new spine.IkConstraintData(constraintMap.name);
					for (var j = 0; j < constraintMap.bones.length; j++) {
						var boneName = constraintMap.bones[j];
						var bone = skeletonData.findBone(boneName);
						if (bone == null)
							throw new Error("IK bone not found: " + boneName);
						data.bones.push(bone);
					}
					var targetName = constraintMap.target;
					data.target = skeletonData.findBone(targetName);
					if (data.target == null)
						throw new Error("IK target bone not found: " + targetName);
					data.bendDirection = this.getValue(constraintMap, "bendPositive", true) ? 1 : -1;
					data.mix = this.getValue(constraintMap, "mix", 1);
					skeletonData.ikConstraints.push(data);
				}
			}
			if (root.transform) {
				for (var i = 0; i < root.transform.length; i++) {
					var constraintMap = root.transform[i];
					var data = new spine.TransformConstraintData(constraintMap.name);
					for (var j = 0; j < constraintMap.bones.length; j++) {
						var boneName = constraintMap.bones[j];
						var bone = skeletonData.findBone(boneName);
						if (bone == null)
							throw new Error("Transform constraint bone not found: " + boneName);
						data.bones.push(bone);
					}
					var targetName = constraintMap.target;
					data.target = skeletonData.findBone(targetName);
					if (data.target == null)
						throw new Error("Transform constraint target bone not found: " + targetName);
					data.offsetRotation = this.getValue(constraintMap, "rotation", 0);
					data.offsetX = this.getValue(constraintMap, "x", 0) * scale;
					data.offsetY = this.getValue(constraintMap, "y", 0) * scale;
					data.offsetScaleX = this.getValue(constraintMap, "scaleX", 0);
					data.offsetScaleY = this.getValue(constraintMap, "scaleY", 0);
					data.offsetShearY = this.getValue(constraintMap, "shearY", 0);
					data.rotateMix = this.getValue(constraintMap, "rotateMix", 1);
					data.translateMix = this.getValue(constraintMap, "translateMix", 1);
					data.scaleMix = this.getValue(constraintMap, "scaleMix", 1);
					data.shearMix = this.getValue(constraintMap, "shearMix", 1);
					skeletonData.transformConstraints.push(data);
				}
			}
			if (root.path) {
				for (var i = 0; i < root.path.length; i++) {
					var constraintMap = root.path[i];
					var data = new spine.PathConstraintData(constraintMap.name);
					for (var j = 0; j < constraintMap.bones.length; j++) {
						var boneName = constraintMap.bones[j];
						var bone = skeletonData.findBone(boneName);
						if (bone == null)
							throw new Error("Transform constraint bone not found: " + boneName);
						data.bones.push(bone);
					}
					var targetName = constraintMap.target;
					data.target = skeletonData.findSlot(targetName);
					if (data.target == null)
						throw new Error("Path target slot not found: " + targetName);
					data.positionMode = SkeletonJson.positionModeFromString(this.getValue(constraintMap, "positionMode", "percent"));
					data.spacingMode = SkeletonJson.spacingModeFromString(this.getValue(constraintMap, "spacingMode", "length"));
					data.rotateMode = SkeletonJson.rotateModeFromString(this.getValue(constraintMap, "rotateMode", "tangent"));
					data.offsetRotation = this.getValue(constraintMap, "rotation", 0);
					data.position = this.getValue(constraintMap, "position", 0);
					if (data.positionMode == spine.PositionMode.Fixed)
						data.position *= scale;
					data.spacing = this.getValue(constraintMap, "spacing", 0);
					if (data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed)
						data.spacing *= scale;
					data.rotateMix = this.getValue(constraintMap, "rotateMix", 1);
					data.translateMix = this.getValue(constraintMap, "translateMix", 1);
					skeletonData.pathConstraints.push(data);
				}
			}
			if (root.skins) {
				for (var skinName in root.skins) {
					var skinMap = root.skins[skinName];
					var skin = new spine.Skin(skinName);
					for (var slotName in skinMap) {
						var slotIndex = skeletonData.findSlotIndex(slotName);
						if (slotIndex == -1)
							throw new Error("Slot not found: " + slotName);
						var slotMap = skinMap[slotName];
						for (var entryName in slotMap) {
							var attachment = this.readAttachment(slotMap[entryName], skin, slotIndex, entryName);
							if (attachment != null)
								skin.addAttachment(slotIndex, entryName, attachment);
						}
					}
					skeletonData.skins.push(skin);
					if (skin.name == "default")
						skeletonData.defaultSkin = skin;
				}
			}
			for (var i = 0, n = this.linkedMeshes.length; i < n; i++) {
				var linkedMesh = this.linkedMeshes[i];
				var skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null)
					throw new Error("Skin not found: " + linkedMesh.skin);
				var parent_4 = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent_4 == null)
					throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.setParentMesh(parent_4);
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;
			if (root.events) {
				for (var eventName in root.events) {
					var eventMap = root.events[eventName];
					var data = new spine.EventData(eventName);
					data.intValue = this.getValue(eventMap, "int", 0);
					data.floatValue = this.getValue(eventMap, "float", 0);
					data.stringValue = this.getValue(eventMap, "string", null);
					skeletonData.events.push(data);
				}
			}
			if (root.animations) {
				for (var animationName in root.animations) {
					var animationMap = root.animations[animationName];
					this.readAnimation(animationMap, animationName, skeletonData);
				}
			}
			return skeletonData;
		};
		SkeletonJson.prototype.readAttachment = function (map, skin, slotIndex, name) {
			var scale = this.scale;
			name = this.getValue(map, "name", name);
			var type = this.getValue(map, "type", "region");
			switch (type) {
				case "region": {
					var path = this.getValue(map, "path", name);
					var region = this.attachmentLoader.newRegionAttachment(skin, name, path);
					if (region == null)
						return null;
					region.path = path;
					region.x = this.getValue(map, "x", 0) * scale;
					region.y = this.getValue(map, "y", 0) * scale;
					region.scaleX = this.getValue(map, "scaleX", 1);
					region.scaleY = this.getValue(map, "scaleY", 1);
					region.rotation = this.getValue(map, "rotation", 0);
					region.width = map.width * scale;
					region.height = map.height * scale;
					var color = this.getValue(map, "color", null);
					if (color != null)
						region.color.setFromString(color);
					region.updateOffset();
					return region;
				}
				case "boundingbox": {
					var box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
					if (box == null)
						return null;
					this.readVertices(map, box, map.vertexCount << 1);
					var color = this.getValue(map, "color", null);
					if (color != null)
						box.color.setFromString(color);
					return box;
				}
				case "mesh":
				case "linkedmesh": {
					var path = this.getValue(map, "path", name);
					var mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (mesh == null)
						return null;
					mesh.path = path;
					var color = this.getValue(map, "color", null);
					if (color != null)
						mesh.color.setFromString(color);
					var parent_5 = this.getValue(map, "parent", null);
					if (parent_5 != null) {
						mesh.inheritDeform = this.getValue(map, "deform", true);
						this.linkedMeshes.push(new LinkedMesh(mesh, this.getValue(map, "skin", null), slotIndex, parent_5));
						return mesh;
					}
					var uvs = map.uvs;
					this.readVertices(map, mesh, uvs.length);
					mesh.triangles = map.triangles;
					mesh.regionUVs = uvs;
					mesh.updateUVs();
					mesh.hullLength = this.getValue(map, "hull", 0) * 2;
					return mesh;
				}
				case "path": {
					var path = this.attachmentLoader.newPathAttachment(skin, name);
					if (path == null)
						return null;
					path.closed = this.getValue(map, "closed", false);
					path.constantSpeed = this.getValue(map, "constantSpeed", true);
					var vertexCount = map.vertexCount;
					this.readVertices(map, path, vertexCount << 1);
					var lengths = spine.Utils.newArray(vertexCount / 3, 0);
					for (var i = 0; i < map.lengths.length; i++)
						lengths[i++] = map.lengths[i] * scale;
					path.lengths = lengths;
					var color = this.getValue(map, "color", null);
					if (color != null)
						path.color.setFromString(color);
					return path;
				}
			}
			return null;
		};
		SkeletonJson.prototype.readVertices = function (map, attachment, verticesLength) {
			var scale = this.scale;
			attachment.worldVerticesLength = verticesLength;
			var vertices = map.vertices;
			if (verticesLength == vertices.length) {
				if (scale != 1) {
					for (var i = 0, n = vertices.length; i < n; i++)
						vertices[i] *= scale;
				}
				attachment.vertices = spine.Utils.toFloatArray(vertices);
				return;
			}
			var weights = new Array();
			var bones = new Array();
			for (var i = 0, n = vertices.length; i < n;) {
				var boneCount = vertices[i++];
				bones.push(boneCount);
				for (var nn = i + boneCount * 4; i < nn; i += 4) {
					bones.push(vertices[i]);
					weights.push(vertices[i + 1] * scale);
					weights.push(vertices[i + 2] * scale);
					weights.push(vertices[i + 3]);
				}
			}
			attachment.bones = bones;
			attachment.vertices = spine.Utils.toFloatArray(weights);
		};
		SkeletonJson.prototype.readAnimation = function (map, name, skeletonData) {
			var scale = this.scale;
			var timelines = new Array();
			var duration = 0;
			if (map.slots) {
				for (var slotName in map.slots) {
					var slotMap = map.slots[slotName];
					var slotIndex = skeletonData.findSlotIndex(slotName);
					if (slotIndex == -1)
						throw new Error("Slot not found: " + slotName);
					for (var timelineName in slotMap) {
						var timelineMap = slotMap[timelineName];
						if (timelineName == "color") {
							var timeline = new spine.ColorTimeline(timelineMap.length);
							timeline.slotIndex = slotIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								var color = new spine.Color();
								color.setFromString(valueMap.color);
								timeline.setFrame(frameIndex, valueMap.time, color.r, color.g, color.b, color.a);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.ColorTimeline.ENTRIES]);
						}
						else if (timelineName = "attachment") {
							var timeline = new spine.AttachmentTimeline(timelineMap.length);
							timeline.slotIndex = slotIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								timeline.setFrame(frameIndex++, valueMap.time, valueMap.name);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[timeline.getFrameCount() - 1]);
						}
						else
							throw new Error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}
			if (map.bones) {
				for (var boneName in map.bones) {
					var boneMap = map.bones[boneName];
					var boneIndex = skeletonData.findBoneIndex(boneName);
					if (boneIndex == -1)
						throw new Error("Bone not found: " + boneName);
					for (var timelineName in boneMap) {
						var timelineMap = boneMap[timelineName];
						if (timelineName === "rotate") {
							var timeline = new spine.RotateTimeline(timelineMap.length);
							timeline.boneIndex = boneIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								timeline.setFrame(frameIndex, valueMap.time, valueMap.angle);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.RotateTimeline.ENTRIES]);
						}
						else if (timelineName === "translate" || timelineName === "scale" || timelineName === "shear") {
							var timeline = null;
							var timelineScale = 1;
							if (timelineName === "scale")
								timeline = new spine.ScaleTimeline(timelineMap.length);
							else if (timelineName === "shear")
								timeline = new spine.ShearTimeline(timelineMap.length);
							else {
								timeline = new spine.TranslateTimeline(timelineMap.length);
								timelineScale = scale;
							}
							timeline.boneIndex = boneIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								var x = this.getValue(valueMap, "x", 0), y = this.getValue(valueMap, "y", 0);
								timeline.setFrame(frameIndex, valueMap.time, x * timelineScale, y * timelineScale);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.TranslateTimeline.ENTRIES]);
						}
						else
							throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
					}
				}
			}
			if (map.ik) {
				for (var constraintName in map.ik) {
					var constraintMap = map.ik[constraintName];
					var constraint = skeletonData.findIkConstraint(constraintName);
					var timeline = new spine.IkConstraintTimeline(constraintMap.length);
					timeline.ikConstraintIndex = skeletonData.ikConstraints.indexOf(constraint);
					var frameIndex = 0;
					for (var i = 0; i < constraintMap.length; i++) {
						var valueMap = constraintMap[i];
						timeline.setFrame(frameIndex, valueMap.time, this.getValue(valueMap, "mix", 1), this.getValue(valueMap, "bendPositive", true) ? 1 : -1);
						this.readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.push(timeline);
					duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.IkConstraintTimeline.ENTRIES]);
				}
			}
			if (map.transform) {
				for (var constraintName in map.transform) {
					var constraintMap = map.transform[constraintName];
					var constraint = skeletonData.findTransformConstraint(constraintName);
					var timeline = new spine.TransformConstraintTimeline(constraintMap.length);
					timeline.transformConstraintIndex = skeletonData.transformConstraints.indexOf(constraint);
					var frameIndex = 0;
					for (var i = 0; i < constraintMap.length; i++) {
						var valueMap = constraintMap[i];
						timeline.setFrame(frameIndex, valueMap.time, this.getValue(valueMap, "rotateMix", 1), this.getValue(valueMap, "translateMix", 1), this.getValue(valueMap, "scaleMix", 1), this.getValue(valueMap, "shearMix", 1));
						this.readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.push(timeline);
					duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.TransformConstraintTimeline.ENTRIES]);
				}
			}
			if (map.paths) {
				for (var constraintName in map.paths) {
					var constraintMap = map.paths[constraintName];
					var index = skeletonData.findPathConstraintIndex(constraintName);
					if (index == -1)
						throw new Error("Path constraint not found: " + constraintName);
					var data = skeletonData.pathConstraints[index];
					for (var timelineName in constraintMap) {
						var timelineMap = constraintMap[timelineName];
						if (timelineName === "position" || timelineName === "spacing") {
							var timeline = null;
							var timelineScale = 1;
							if (timelineName === "spacing") {
								timeline = new spine.PathConstraintSpacingTimeline(timelineMap.length);
								if (data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed)
									timelineScale = scale;
							}
							else {
								timeline = new spine.PathConstraintPositionTimeline(timelineMap.length);
								if (data.positionMode == spine.PositionMode.Fixed)
									timelineScale = scale;
							}
							timeline.pathConstraintIndex = index;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								timeline.setFrame(frameIndex, valueMap.time, this.getValue(valueMap, timelineName, 0) * timelineScale);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.PathConstraintPositionTimeline.ENTRIES]);
						}
						else if (timelineName === "mix") {
							var timeline = new spine.PathConstraintMixTimeline(timelineMap.length);
							timeline.pathConstraintIndex = index;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								timeline.setFrame(frameIndex, valueMap.time, this.getValue(valueMap, "rotateMix", 1), this.getValue(valueMap, "translateMix", 1));
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.PathConstraintMixTimeline.ENTRIES]);
						}
					}
				}
			}
			if (map.deform) {
				for (var deformName in map.deform) {
					var deformMap = map.deform[deformName];
					var skin = skeletonData.findSkin(deformName);
					if (skin == null)
						throw new Error("Skin not found: " + deformName);
					for (var slotName in deformMap) {
						var slotMap = deformMap[slotName];
						var slotIndex = skeletonData.findSlotIndex(slotName);
						if (slotIndex == -1)
							throw new Error("Slot not found: " + slotMap.name);
						for (var timelineName in slotMap) {
							var timelineMap = slotMap[timelineName];
							var attachment = skin.getAttachment(slotIndex, timelineName);
							if (attachment == null)
								throw new Error("Deform attachment not found: " + timelineMap.name);
							var weighted = attachment.bones != null;
							var vertices = attachment.vertices;
							var deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;
							var timeline = new spine.DeformTimeline(timelineMap.length);
							timeline.slotIndex = slotIndex;
							timeline.attachment = attachment;
							var frameIndex = 0;
							for (var j = 0; j < timelineMap.length; j++) {
								var valueMap = timelineMap[j];
								var deform = void 0;
								var verticesValue = this.getValue(valueMap, "vertices", null);
								if (verticesValue == null)
									deform = weighted ? spine.Utils.newFloatArray(deformLength) : vertices;
								else {
									deform = spine.Utils.newFloatArray(deformLength);
									var start = this.getValue(valueMap, "offset", 0);
									spine.Utils.arrayCopy(verticesValue, 0, deform, start, verticesValue.length);
									if (scale != 1) {
										for (var i = start, n = i + verticesValue.length; i < n; i++)
											deform[i] *= scale;
									}
									if (!weighted) {
										for (var i = 0; i < deformLength; i++)
											deform[i] += vertices[i];
									}
								}
								timeline.setFrame(frameIndex, valueMap.time, deform);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[timeline.getFrameCount() - 1]);
						}
					}
				}
			}
			var drawOrderNode = map.drawOrder;
			if (drawOrderNode == null)
				drawOrderNode = map.draworder;
			if (drawOrderNode != null) {
				var timeline = new spine.DrawOrderTimeline(drawOrderNode.length);
				var slotCount = skeletonData.slots.length;
				var frameIndex = 0;
				for (var j = 0; j < drawOrderNode.length; j++) {
					var drawOrderMap = drawOrderNode[j];
					var drawOrder = null;
					var offsets = this.getValue(drawOrderMap, "offsets", null);
					if (offsets != null) {
						drawOrder = spine.Utils.newArray(slotCount, -1);
						var unchanged = spine.Utils.newArray(slotCount - offsets.length, 0);
						var originalIndex = 0, unchangedIndex = 0;
						for (var i = 0; i < offsets.length; i++) {
							var offsetMap = offsets[i];
							var slotIndex = skeletonData.findSlotIndex(offsetMap.slot);
							if (slotIndex == -1)
								throw new Error("Slot not found: " + offsetMap.slot);
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							drawOrder[originalIndex + offsetMap.offset] = originalIndex++;
						}
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						for (var i = slotCount - 1; i >= 0; i--)
							if (drawOrder[i] == -1)
								drawOrder[i] = unchanged[--unchangedIndex];
					}
					timeline.setFrame(frameIndex++, drawOrderMap.time, drawOrder);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[timeline.getFrameCount() - 1]);
			}
			if (map.events) {
				var timeline = new spine.EventTimeline(map.events.length);
				var frameIndex = 0;
				for (var i = 0; i < map.events.length; i++) {
					var eventMap = map.events[i];
					var eventData = skeletonData.findEvent(eventMap.name);
					if (eventData == null)
						throw new Error("Event not found: " + eventMap.name);
					var event_3 = new spine.Event(eventMap.time, eventData);
					event_3.intValue = this.getValue(eventMap, "int", eventData.intValue);
					event_3.floatValue = this.getValue(eventMap, "float", eventData.floatValue);
					event_3.stringValue = this.getValue(eventMap, "string", eventData.stringValue);
					timeline.setFrame(frameIndex++, event_3);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[timeline.getFrameCount() - 1]);
			}
			if (isNaN(duration)) {
				throw new Error("Error while parsing animation, duration is NaN");
			}
			skeletonData.animations.push(new spine.Animation(name, timelines, duration));
		};
		SkeletonJson.prototype.readCurve = function (map, timeline, frameIndex) {
			if (!map.curve)
				return;
			if (map.curve === "stepped")
				timeline.setStepped(frameIndex);
			else if (Object.prototype.toString.call(map.curve) === '[object Array]') {
				var curve = map.curve;
				timeline.setCurve(frameIndex, curve[0], curve[1], curve[2], curve[3]);
			}
		};
		SkeletonJson.prototype.getValue = function (map, prop, defaultValue) {
			return map[prop] !== undefined ? map[prop] : defaultValue;
		};
		SkeletonJson.blendModeFromString = function (str) {
			str = str.toLowerCase();
			if (str == "normal")
				return spine.BlendMode.Normal;
			if (str == "additive")
				return spine.BlendMode.Additive;
			if (str == "multiply")
				return spine.BlendMode.Multiply;
			if (str == "screen")
				return spine.BlendMode.Screen;
			throw new Error("Unknown blend mode: " + str);
		};
		SkeletonJson.positionModeFromString = function (str) {
			str = str.toLowerCase();
			if (str == "fixed")
				return spine.PositionMode.Fixed;
			if (str == "percent")
				return spine.PositionMode.Percent;
			throw new Error("Unknown position mode: " + str);
		};
		SkeletonJson.spacingModeFromString = function (str) {
			str = str.toLowerCase();
			if (str == "length")
				return spine.SpacingMode.Length;
			if (str == "fixed")
				return spine.SpacingMode.Fixed;
			if (str == "percent")
				return spine.SpacingMode.Percent;
			throw new Error("Unknown position mode: " + str);
		};
		SkeletonJson.rotateModeFromString = function (str) {
			str = str.toLowerCase();
			if (str == "tangent")
				return spine.RotateMode.Tangent;
			if (str == "chain")
				return spine.RotateMode.Chain;
			if (str == "chainscale")
				return spine.RotateMode.ChainScale;
			throw new Error("Unknown rotate mode: " + str);
		};
		return SkeletonJson;
	}());
	spine.SkeletonJson = SkeletonJson;
	var LinkedMesh = (function () {
		function LinkedMesh(mesh, skin, slotIndex, parent) {
			this.mesh = mesh;
			this.skin = skin;
			this.slotIndex = slotIndex;
			this.parent = parent;
		}
		return LinkedMesh;
	}());
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Skin = (function () {
		function Skin(name) {
			this.attachments = new Array();
			if (name == null)
				throw new Error("name cannot be null.");
			this.name = name;
		}
		Skin.prototype.addAttachment = function (slotIndex, name, attachment) {
			if (attachment == null)
				throw new Error("attachment cannot be null.");
			var attachments = this.attachments;
			if (slotIndex >= attachments.length)
				attachments.length = slotIndex + 1;
			if (!attachments[slotIndex])
				attachments[slotIndex] = {};
			attachments[slotIndex][name] = attachment;
		};
		Skin.prototype.getAttachment = function (slotIndex, name) {
			var dictionary = this.attachments[slotIndex];
			return dictionary ? dictionary[name] : null;
		};
		Skin.prototype.attachAll = function (skeleton, oldSkin) {
			var slotIndex = 0;
			for (var i = 0; i < skeleton.slots.length; i++) {
				var slot = skeleton.slots[i];
				var slotAttachment = slot.getAttachment();
				if (slotAttachment && slotIndex < oldSkin.attachments.length) {
					var dictionary = oldSkin.attachments[slotIndex];
					for (var key in dictionary) {
						var skinAttachment = dictionary[key];
						if (slotAttachment == skinAttachment) {
							var attachment = this.getAttachment(slotIndex, name);
							if (attachment != null)
								slot.setAttachment(attachment);
							break;
						}
					}
				}
				slotIndex++;
			}
		};
		return Skin;
	}());
	spine.Skin = Skin;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Slot = (function () {
		function Slot(data, bone) {
			this.attachmentVertices = new Array();
			if (data == null)
				throw new Error("data cannot be null.");
			if (bone == null)
				throw new Error("bone cannot be null.");
			this.data = data;
			this.bone = bone;
			this.color = new spine.Color();
			this.setToSetupPose();
		}
		Slot.prototype.getAttachment = function () {
			return this.attachment;
		};
		Slot.prototype.setAttachment = function (attachment) {
			if (this.attachment == attachment)
				return;
			this.attachment = attachment;
			this.attachmentTime = this.bone.skeleton.time;
			this.attachmentVertices.length = 0;
		};
		Slot.prototype.setAttachmentTime = function (time) {
			this.attachmentTime = this.bone.skeleton.time - time;
		};
		Slot.prototype.getAttachmentTime = function () {
			return this.bone.skeleton.time - this.attachmentTime;
		};
		Slot.prototype.setToSetupPose = function () {
			this.color.setFromColor(this.data.color);
			if (this.data.attachmentName == null)
				this.attachment = null;
			else {
				this.attachment = null;
				this.setAttachment(this.bone.skeleton.getAttachment(this.data.index, this.data.attachmentName));
			}
		};
		return Slot;
	}());
	spine.Slot = Slot;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SlotData = (function () {
		function SlotData(index, name, boneData) {
			this.color = new spine.Color(1, 1, 1, 1);
			if (index < 0)
				throw new Error("index must be >= 0.");
			if (name == null)
				throw new Error("name cannot be null.");
			if (boneData == null)
				throw new Error("boneData cannot be null.");
			this.index = index;
			this.name = name;
			this.boneData = boneData;
		}
		return SlotData;
	}());
	spine.SlotData = SlotData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Texture = (function () {
		function Texture(image) {
			this._image = image;
		}
		Texture.prototype.getImage = function () {
			return this._image;
		};
		Texture.filterFromString = function (text) {
			switch (text.toLowerCase()) {
				case "nearest": return TextureFilter.Nearest;
				case "linear": return TextureFilter.Linear;
				case "mipmap": return TextureFilter.MipMap;
				case "mipmapnearestnearest": return TextureFilter.MipMapNearestNearest;
				case "mipmaplinearnearest": return TextureFilter.MipMapLinearNearest;
				case "mipmapnearestlinear": return TextureFilter.MipMapNearestLinear;
				case "mipmaplinearlinear": return TextureFilter.MipMapLinearLinear;
				default: throw new Error("Unknown texture filter " + text);
			}
		};
		Texture.wrapFromString = function (text) {
			switch (text.toLowerCase()) {
				case "mirroredtepeat": return TextureWrap.MirroredRepeat;
				case "clamptoedge": return TextureWrap.ClampToEdge;
				case "repeat": return TextureWrap.Repeat;
				default: throw new Error("Unknown texture wrap " + text);
			}
		};
		return Texture;
	}());
	spine.Texture = Texture;
	(function (TextureFilter) {
		TextureFilter[TextureFilter["Nearest"] = 9728] = "Nearest";
		TextureFilter[TextureFilter["Linear"] = 9729] = "Linear";
		TextureFilter[TextureFilter["MipMap"] = 9987] = "MipMap";
		TextureFilter[TextureFilter["MipMapNearestNearest"] = 9984] = "MipMapNearestNearest";
		TextureFilter[TextureFilter["MipMapLinearNearest"] = 9985] = "MipMapLinearNearest";
		TextureFilter[TextureFilter["MipMapNearestLinear"] = 9986] = "MipMapNearestLinear";
		TextureFilter[TextureFilter["MipMapLinearLinear"] = 9987] = "MipMapLinearLinear";
	})(spine.TextureFilter || (spine.TextureFilter = {}));
	var TextureFilter = spine.TextureFilter;
	(function (TextureWrap) {
		TextureWrap[TextureWrap["MirroredRepeat"] = 33648] = "MirroredRepeat";
		TextureWrap[TextureWrap["ClampToEdge"] = 33071] = "ClampToEdge";
		TextureWrap[TextureWrap["Repeat"] = 10497] = "Repeat";
	})(spine.TextureWrap || (spine.TextureWrap = {}));
	var TextureWrap = spine.TextureWrap;
	var TextureRegion = (function () {
		function TextureRegion() {
			this.u = 0;
			this.v = 0;
			this.u2 = 0;
			this.v2 = 0;
			this.width = 0;
			this.height = 0;
			this.rotate = false;
			this.offsetX = 0;
			this.offsetY = 0;
			this.originalWidth = 0;
			this.originalHeight = 0;
		}
		return TextureRegion;
	}());
	spine.TextureRegion = TextureRegion;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var TextureAtlas = (function () {
		function TextureAtlas(atlasText, textureLoader) {
			this.pages = new Array();
			this.regions = new Array();
			this.load(atlasText, textureLoader);
		}
		TextureAtlas.prototype.load = function (atlasText, textureLoader) {
			if (textureLoader == null)
				throw new Error("textureLoader cannot be null.");
			var reader = new TextureAtlasReader(atlasText);
			var tuple = new Array(4);
			var page = null;
			while (true) {
				var line = reader.readLine();
				if (line == null)
					break;
				line = line.trim();
				if (line.length == 0)
					page = null;
				else if (!page) {
					page = new TextureAtlasPage();
					page.name = line;
					if (reader.readTuple(tuple) == 2) {
						page.width = parseInt(tuple[0]);
						page.height = parseInt(tuple[1]);
						reader.readTuple(tuple);
					}
					reader.readTuple(tuple);
					page.minFilter = spine.Texture.filterFromString(tuple[0]);
					page.magFilter = spine.Texture.filterFromString(tuple[1]);
					var direction = reader.readValue();
					page.uWrap = spine.TextureWrap.ClampToEdge;
					page.vWrap = spine.TextureWrap.ClampToEdge;
					if (direction == "x")
						page.uWrap = spine.TextureWrap.Repeat;
					else if (direction == "y")
						page.vWrap = spine.TextureWrap.Repeat;
					else if (direction == "xy")
						page.uWrap = page.vWrap = spine.TextureWrap.Repeat;
					page.texture = textureLoader(line);
					page.texture.setFilters(page.minFilter, page.magFilter);
					page.texture.setWraps(page.uWrap, page.vWrap);
					page.width = page.texture.getImage().width;
					page.height = page.texture.getImage().height;
					this.pages.push(page);
				}
				else {
					var region = new TextureAtlasRegion();
					region.name = line;
					region.page = page;
					region.rotate = reader.readValue() == "true";
					reader.readTuple(tuple);
					var x = parseInt(tuple[0]);
					var y = parseInt(tuple[1]);
					reader.readTuple(tuple);
					var width = parseInt(tuple[0]);
					var height = parseInt(tuple[1]);
					region.u = x / page.width;
					region.v = y / page.height;
					if (region.rotate) {
						region.u2 = (x + height) / page.width;
						region.v2 = (y + width) / page.height;
					}
					else {
						region.u2 = (x + width) / page.width;
						region.v2 = (y + height) / page.height;
					}
					region.x = x;
					region.y = y;
					region.width = Math.abs(width);
					region.height = Math.abs(height);
					if (reader.readTuple(tuple) == 4) {
						if (reader.readTuple(tuple) == 4) {
							reader.readTuple(tuple);
						}
					}
					region.originalWidth = parseInt(tuple[0]);
					region.originalHeight = parseInt(tuple[1]);
					reader.readTuple(tuple);
					region.offsetX = parseInt(tuple[0]);
					region.offsetY = parseInt(tuple[1]);
					region.index = parseInt(reader.readValue());
					region.texture = page.texture;
					this.regions.push(region);
				}
			}
		};
		TextureAtlas.prototype.findRegion = function (name) {
			for (var i = 0; i < this.regions.length; i++) {
				if (this.regions[i].name == name) {
					return this.regions[i];
				}
			}
			return null;
		};
		TextureAtlas.prototype.dispose = function () {
			for (var i = 0; i < this.pages.length; i++) {
				this.pages[i].texture.dispose();
			}
		};
		return TextureAtlas;
	}());
	spine.TextureAtlas = TextureAtlas;
	var TextureAtlasReader = (function () {
		function TextureAtlasReader(text) {
			this.index = 0;
			this.lines = text.split(/\r\n|\r|\n/);
		}
		TextureAtlasReader.prototype.readLine = function () {
			if (this.index >= this.lines.length)
				return null;
			return this.lines[this.index++];
		};
		TextureAtlasReader.prototype.readValue = function () {
			var line = this.readLine();
			var colon = line.indexOf(":");
			if (colon == -1)
				throw new Error("Invalid line: " + line);
			return line.substring(colon + 1).trim();
		};
		TextureAtlasReader.prototype.readTuple = function (tuple) {
			var line = this.readLine();
			var colon = line.indexOf(":");
			if (colon == -1)
				throw new Error("Invalid line: " + line);
			var i = 0, lastMatch = colon + 1;
			for (; i < 3; i++) {
				var comma = line.indexOf(",", lastMatch);
				if (comma == -1)
					break;
				tuple[i] = line.substr(lastMatch, comma - lastMatch).trim();
				lastMatch = comma + 1;
			}
			tuple[i] = line.substring(lastMatch).trim();
			return i + 1;
		};
		return TextureAtlasReader;
	}());
	var TextureAtlasPage = (function () {
		function TextureAtlasPage() {
		}
		return TextureAtlasPage;
	}());
	spine.TextureAtlasPage = TextureAtlasPage;
	var TextureAtlasRegion = (function (_super) {
		__extends(TextureAtlasRegion, _super);
		function TextureAtlasRegion() {
			_super.apply(this, arguments);
		}
		return TextureAtlasRegion;
	}(spine.TextureRegion));
	spine.TextureAtlasRegion = TextureAtlasRegion;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var TextureAtlasAttachmentLoader = (function () {
		function TextureAtlasAttachmentLoader(atlas) {
			this.atlas = atlas;
		}
		TextureAtlasAttachmentLoader.prototype.newRegionAttachment = function (skin, name, path) {
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			region.renderObject = region;
			var attachment = new spine.RegionAttachment(name);
			attachment.setRegion(region);
			attachment.region = region;
			return attachment;
		};
		TextureAtlasAttachmentLoader.prototype.newMeshAttachment = function (skin, name, path) {
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new Error("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			region.renderObject = region;
			var attachment = new spine.MeshAttachment(name);
			attachment.region = region;
			return attachment;
		};
		TextureAtlasAttachmentLoader.prototype.newBoundingBoxAttachment = function (skin, name) {
			return new spine.BoundingBoxAttachment(name);
		};
		TextureAtlasAttachmentLoader.prototype.newPathAttachment = function (skin, name) {
			return new spine.PathAttachment(name);
		};
		return TextureAtlasAttachmentLoader;
	}());
	spine.TextureAtlasAttachmentLoader = TextureAtlasAttachmentLoader;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var TransformConstraint = (function () {
		function TransformConstraint(data, skeleton) {
			this.rotateMix = 0;
			this.translateMix = 0;
			this.scaleMix = 0;
			this.shearMix = 0;
			this.temp = new spine.Vector2();
			if (data == null)
				throw new Error("data cannot be null.");
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.rotateMix = data.rotateMix;
			this.translateMix = data.translateMix;
			this.scaleMix = data.scaleMix;
			this.shearMix = data.shearMix;
			this.bones = new Array();
			for (var i = 0; i < data.bones.length; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findBone(data.target.name);
		}
		TransformConstraint.prototype.apply = function () {
			this.update();
		};
		TransformConstraint.prototype.update = function () {
			var rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			var target = this.target;
			var ta = target.a, tb = target.b, tc = target.c, td = target.d;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (rotateMix > 0) {
					var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					var r = Math.atan2(tc, ta) - Math.atan2(c, a) + this.data.offsetRotation * spine.MathUtils.degRad;
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r *= rotateMix;
					var cos = Math.cos(r), sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
				if (translateMix > 0) {
					var temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += (temp.x - bone.worldX) * translateMix;
					bone.worldY += (temp.y - bone.worldY) * translateMix;
				}
				if (scaleMix > 0) {
					var bs = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
					var ts = Math.sqrt(ta * ta + tc * tc);
					var s = bs > 0.00001 ? (bs + (ts - bs + this.data.offsetScaleX) * scaleMix) / bs : 0;
					bone.a *= s;
					bone.c *= s;
					bs = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					ts = Math.sqrt(tb * tb + td * td);
					s = bs > 0.00001 ? (bs + (ts - bs + this.data.offsetScaleY) * scaleMix) / bs : 0;
					bone.b *= s;
					bone.d *= s;
				}
				if (shearMix > 0) {
					var b = bone.b, d = bone.d;
					var by = Math.atan2(d, b);
					var r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r = by + (r + this.data.offsetShearY * spine.MathUtils.degRad) * shearMix;
					var s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
				}
			}
		};
		return TransformConstraint;
	}());
	spine.TransformConstraint = TransformConstraint;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var TransformConstraintData = (function () {
		function TransformConstraintData(name) {
			this.bones = new Array();
			this.rotateMix = 0;
			this.translateMix = 0;
			this.scaleMix = 0;
			this.shearMix = 0;
			this.offsetRotation = 0;
			this.offsetX = 0;
			this.offsetY = 0;
			this.offsetScaleX = 0;
			this.offsetScaleY = 0;
			this.offsetShearY = 0;
			if (name == null)
				throw new Error("name cannot be null.");
			this.name = name;
		}
		return TransformConstraintData;
	}());
	spine.TransformConstraintData = TransformConstraintData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Color = (function () {
		function Color(r, g, b, a) {
			if (r === void 0) { r = 0; }
			if (g === void 0) { g = 0; }
			if (b === void 0) { b = 0; }
			if (a === void 0) { a = 0; }
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
		Color.prototype.set = function (r, g, b, a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
			this.clamp();
			return this;
		};
		Color.prototype.setFromColor = function (c) {
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
			this.a = c.a;
			return this;
		};
		Color.prototype.setFromString = function (hex) {
			hex = hex.charAt(0) == '#' ? hex.substr(1) : hex;
			this.r = parseInt(hex.substr(0, 2), 16) / 255.0;
			this.g = parseInt(hex.substr(2, 2), 16) / 255.0;
			this.b = parseInt(hex.substr(4, 2), 16) / 255.0;
			this.a = (hex.length != 8 ? 255 : parseInt(hex.substr(6, 2), 16)) / 255.0;
			return this;
		};
		Color.prototype.add = function (r, g, b, a) {
			this.r += r;
			this.g += g;
			this.b += b;
			this.a += a;
			this.clamp();
			return this;
		};
		Color.prototype.clamp = function () {
			if (this.r < 0)
				this.r = 0;
			else if (this.r > 1)
				this.r = 1;
			if (this.g < 0)
				this.g = 0;
			else if (this.g > 1)
				this.g = 1;
			if (this.b < 0)
				this.b = 0;
			else if (this.b > 1)
				this.b = 1;
			if (this.a < 0)
				this.a = 0;
			else if (this.a > 1)
				this.a = 1;
			return this;
		};
		Color.WHITE = new Color(1, 1, 1, 1);
		Color.RED = new Color(1, 0, 0, 1);
		Color.GREEN = new Color(0, 1, 0, 1);
		Color.BLUE = new Color(0, 0, 1, 1);
		Color.MAGENTA = new Color(1, 0, 1, 1);
		return Color;
	}());
	spine.Color = Color;
	var MathUtils = (function () {
		function MathUtils() {
		}
		MathUtils.clamp = function (value, min, max) {
			if (value < min)
				return min;
			if (value > max)
				return max;
			return value;
		};
		MathUtils.cosDeg = function (degrees) {
			return Math.cos(degrees * MathUtils.degRad);
		};
		MathUtils.sinDeg = function (degrees) {
			return Math.sin(degrees * MathUtils.degRad);
		};
		MathUtils.signum = function (value) {
			return value >= 0 ? 1 : -1;
		};
		MathUtils.toInt = function (x) {
			return x > 0 ? Math.floor(x) : Math.ceil(x);
		};
		MathUtils.cbrt = function (x) {
			var y = Math.pow(Math.abs(x), 1 / 3);
			return x < 0 ? -y : y;
		};
		MathUtils.PI = 3.1415927;
		MathUtils.PI2 = MathUtils.PI * 2;
		MathUtils.radiansToDegrees = 180 / MathUtils.PI;
		MathUtils.radDeg = MathUtils.radiansToDegrees;
		MathUtils.degreesToRadians = MathUtils.PI / 180;
		MathUtils.degRad = MathUtils.degreesToRadians;
		return MathUtils;
	}());
	spine.MathUtils = MathUtils;
	var Utils = (function () {
		function Utils() {
		}
		Utils.arrayCopy = function (source, sourceStart, dest, destStart, numElements) {
			for (var i = sourceStart, j = destStart; i < sourceStart + numElements; i++, j++) {
				dest[j] = source[i];
			}
		};
		Utils.setArraySize = function (array, size, value) {
			if (value === void 0) { value = 0; }
			var oldSize = array.length;
			if (oldSize == size)
				return array;
			array.length = size;
			if (oldSize < size) {
				for (var i = oldSize; i < size; i++)
					array[i] = value;
			}
			return array;
		};
		Utils.newArray = function (size, defaultValue) {
			var array = new Array(size);
			for (var i = 0; i < size; i++)
				array[i] = defaultValue;
			return array;
		};
		Utils.newFloatArray = function (size) {
			if (Utils.SUPPORTS_TYPED_ARRAYS) {
				return new Float32Array(size);
			}
			else {
				var array = new Array(size);
				for (var i = 0; i < array.length; i++)
					array[i] = 0;
				return array;
			}
		};
		Utils.toFloatArray = function (array) {
			return Utils.SUPPORTS_TYPED_ARRAYS ? new Float32Array(array) : array;
		};
		Utils.SUPPORTS_TYPED_ARRAYS = typeof (Float32Array) !== "undefined";
		return Utils;
	}());
	spine.Utils = Utils;
	var DebugUtils = (function () {
		function DebugUtils() {
		}
		DebugUtils.logBones = function (skeleton) {
			for (var i = 0; i < skeleton.bones.length; i++) {
				var bone = skeleton.bones[i];
				console.log(bone.data.name + ", " + bone.a + ", " + bone.b + ", " + bone.c + ", " + bone.d + ", " + bone.worldX + ", " + bone.worldY);
			}
		};
		return DebugUtils;
	}());
	spine.DebugUtils = DebugUtils;
	var Pool = (function () {
		function Pool(instantiator) {
			this.items = new Array();
			this.instantiator = instantiator;
		}
		Pool.prototype.obtain = function () {
			return this.items.length > 0 ? this.items.pop() : this.instantiator();
		};
		Pool.prototype.free = function (item) {
			this.items.push(item);
		};
		Pool.prototype.freeAll = function (items) {
			for (var i = 0; i < items.length; i++)
				this.items[i] = items[i];
		};
		Pool.prototype.clear = function () {
			this.items.length = 0;
		};
		return Pool;
	}());
	spine.Pool = Pool;
	var Vector2 = (function () {
		function Vector2(x, y) {
			if (x === void 0) { x = 0; }
			if (y === void 0) { y = 0; }
			this.x = x;
			this.y = y;
		}
		Vector2.prototype.set = function (x, y) {
			this.x = x;
			this.y = y;
			return this;
		};
		Vector2.prototype.length = function () {
			var x = this.x;
			var y = this.y;
			return Math.sqrt(x * x + y * y);
		};
		Vector2.prototype.normalize = function () {
			var len = this.length();
			if (len != 0) {
				this.x /= len;
				this.y /= len;
			}
			return this;
		};
		return Vector2;
	}());
	spine.Vector2 = Vector2;
	var TimeKeeper = (function () {
		function TimeKeeper() {
			this.maxDelta = 0.064;
			this.framesPerSecond = 0;
			this.delta = 0;
			this.totalTime = 0;
			this.lastTime = Date.now() / 1000;
			this.frameCount = 0;
			this.frameTime = 0;
		}
		TimeKeeper.prototype.update = function () {
			var now = Date.now() / 1000;
			this.delta = now - this.lastTime;
			this.frameTime += this.delta;
			this.totalTime += this.delta;
			if (this.delta > this.maxDelta)
				this.delta = this.maxDelta;
			this.lastTime = now;
			this.frameCount++;
			if (this.frameTime > 1) {
				this.framesPerSecond = this.frameCount / this.frameTime;
				this.frameTime = 0;
				this.frameCount = 0;
			}
		};
		return TimeKeeper;
	}());
	spine.TimeKeeper = TimeKeeper;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Attachment = (function () {
		function Attachment(name) {
			if (name == null)
				throw new Error("name cannot be null.");
			this.name = name;
		}
		return Attachment;
	}());
	spine.Attachment = Attachment;
	var VertexAttachment = (function (_super) {
		__extends(VertexAttachment, _super);
		function VertexAttachment(name) {
			_super.call(this, name);
			this.worldVerticesLength = 0;
		}
		VertexAttachment.prototype.computeWorldVertices = function (slot, worldVertices) {
			this.computeWorldVerticesWith(slot, 0, this.worldVerticesLength, worldVertices, 0);
		};
		VertexAttachment.prototype.computeWorldVerticesWith = function (slot, start, count, worldVertices, offset) {
			count += offset;
			var skeleton = slot.bone.skeleton;
			var x = skeleton.x, y = skeleton.y;
			var deformArray = slot.attachmentVertices;
			var vertices = this.vertices;
			var bones = this.bones;
			if (bones == null) {
				if (deformArray.length > 0)
					vertices = deformArray;
				var bone = slot.bone;
				x += bone.worldX;
				y += bone.worldY;
				var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (var v_1 = start, w = offset; w < count; v_1 += 2, w += 2) {
					var vx = vertices[v_1], vy = vertices[v_1 + 1];
					worldVertices[w] = vx * a + vy * b + x;
					worldVertices[w + 1] = vx * c + vy * d + y;
				}
				return;
			}
			var v = 0, skip = 0;
			for (var i = 0; i < start; i += 2) {
				var n = bones[v];
				v += n + 1;
				skip += n;
			}
			var skeletonBones = skeleton.bones;
			if (deformArray.length == 0) {
				for (var w = offset, b = skip * 3; w < count; w += 2) {
					var wx = x, wy = y;
					var n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3) {
						var bone = skeletonBones[bones[v]];
						var vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			}
			else {
				var deform = deformArray;
				for (var w = offset, b = skip * 3, f = skip << 1; w < count; w += 2) {
					var wx = x, wy = y;
					var n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3, f += 2) {
						var bone = skeletonBones[bones[v]];
						var vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			}
		};
		VertexAttachment.prototype.applyDeform = function (sourceAttachment) {
			return this == sourceAttachment;
		};
		return VertexAttachment;
	}(Attachment));
	spine.VertexAttachment = VertexAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	(function (AttachmentType) {
		AttachmentType[AttachmentType["Region"] = 0] = "Region";
		AttachmentType[AttachmentType["BoundingBox"] = 1] = "BoundingBox";
		AttachmentType[AttachmentType["Mesh"] = 2] = "Mesh";
		AttachmentType[AttachmentType["LinkedMesh"] = 3] = "LinkedMesh";
		AttachmentType[AttachmentType["Path"] = 4] = "Path";
	})(spine.AttachmentType || (spine.AttachmentType = {}));
	var AttachmentType = spine.AttachmentType;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var BoundingBoxAttachment = (function (_super) {
		__extends(BoundingBoxAttachment, _super);
		function BoundingBoxAttachment(name) {
			_super.call(this, name);
			this.color = new spine.Color(1, 1, 1, 1);
		}
		return BoundingBoxAttachment;
	}(spine.VertexAttachment));
	spine.BoundingBoxAttachment = BoundingBoxAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var MeshAttachment = (function (_super) {
		__extends(MeshAttachment, _super);
		function MeshAttachment(name) {
			_super.call(this, name);
			this.color = new spine.Color(1, 1, 1, 1);
			this.inheritDeform = false;
			this.tempColor = new spine.Color(0, 0, 0, 0);
		}
		MeshAttachment.prototype.updateUVs = function () {
			var regionUVs = this.regionUVs;
			var verticesLength = regionUVs.length;
			var worldVerticesLength = (verticesLength >> 1) * 8;
			if (this.worldVertices == null || this.worldVertices.length != worldVerticesLength)
				this.worldVertices = spine.Utils.newFloatArray(worldVerticesLength);
			var u = 0, v = 0, width = 0, height = 0;
			if (this.region == null) {
				u = v = 0;
				width = height = 1;
			}
			else {
				u = this.region.u;
				v = this.region.v;
				width = this.region.u2 - u;
				height = this.region.v2 - v;
			}
			if (this.region.rotate) {
				for (var i = 0, w = 6; i < verticesLength; i += 2, w += 8) {
					this.worldVertices[w] = u + regionUVs[i + 1] * width;
					this.worldVertices[w + 1] = v + height - regionUVs[i] * height;
				}
			}
			else {
				for (var i = 0, w = 6; i < verticesLength; i += 2, w += 8) {
					this.worldVertices[w] = u + regionUVs[i] * width;
					this.worldVertices[w + 1] = v + regionUVs[i + 1] * height;
				}
			}
		};
		MeshAttachment.prototype.updateWorldVertices = function (slot, premultipliedAlpha) {
			var skeleton = slot.bone.skeleton;
			var skeletonColor = skeleton.color, slotColor = slot.color, meshColor = this.color;
			var alpha = skeletonColor.a * slotColor.a * meshColor.a;
			var multiplier = premultipliedAlpha ? alpha : 1;
			var color = this.tempColor;
			color.set(skeletonColor.r * slotColor.r * meshColor.r * multiplier, skeletonColor.g * slotColor.g * meshColor.g * multiplier, skeletonColor.b * slotColor.b * meshColor.b * multiplier, alpha);
			var x = skeleton.x, y = skeleton.y;
			var deformArray = slot.attachmentVertices;
			var vertices = this.vertices, worldVertices = this.worldVertices;
			var bones = this.bones;
			if (bones == null) {
				var verticesLength = vertices.length;
				if (deformArray.length > 0)
					vertices = deformArray;
				var bone = slot.bone;
				x += bone.worldX;
				y += bone.worldY;
				var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (var v = 0, w = 0; v < verticesLength; v += 2, w += 8) {
					var vx = vertices[v], vy = vertices[v + 1];
					worldVertices[w] = vx * a + vy * b + x;
					worldVertices[w + 1] = vx * c + vy * d + y;
					worldVertices[w + 2] = color.r;
					worldVertices[w + 3] = color.g;
					worldVertices[w + 4] = color.b;
					worldVertices[w + 5] = color.a;
				}
				return worldVertices;
			}
			var skeletonBones = skeleton.bones;
			if (deformArray.length == 0) {
				for (var w = 0, v = 0, b = 0, n = bones.length; v < n; w += 8) {
					var wx = x, wy = y;
					var nn = bones[v++] + v;
					for (; v < nn; v++, b += 3) {
						var bone = skeletonBones[bones[v]];
						var vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
					worldVertices[w + 2] = color.r;
					worldVertices[w + 3] = color.g;
					worldVertices[w + 4] = color.b;
					worldVertices[w + 5] = color.a;
				}
			}
			else {
				var deform = deformArray;
				for (var w = 0, v = 0, b = 0, f = 0, n = bones.length; v < n; w += 8) {
					var wx = x, wy = y;
					var nn = bones[v++] + v;
					for (; v < nn; v++, b += 3, f += 2) {
						var bone = skeletonBones[bones[v]];
						var vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
					worldVertices[w + 2] = color.r;
					worldVertices[w + 3] = color.g;
					worldVertices[w + 4] = color.b;
					worldVertices[w + 5] = color.a;
				}
			}
			return worldVertices;
		};
		MeshAttachment.prototype.applyDeform = function (sourceAttachment) {
			return this == sourceAttachment || (this.inheritDeform && this.parentMesh == sourceAttachment);
		};
		MeshAttachment.prototype.getParentMesh = function () {
			return this.parentMesh;
		};
		MeshAttachment.prototype.setParentMesh = function (parentMesh) {
			this.parentMesh = parentMesh;
			if (parentMesh != null) {
				this.bones = parentMesh.bones;
				this.vertices = parentMesh.vertices;
				this.regionUVs = parentMesh.regionUVs;
				this.triangles = parentMesh.triangles;
				this.hullLength = parentMesh.hullLength;
			}
		};
		return MeshAttachment;
	}(spine.VertexAttachment));
	spine.MeshAttachment = MeshAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var PathAttachment = (function (_super) {
		__extends(PathAttachment, _super);
		function PathAttachment(name) {
			_super.call(this, name);
			this.closed = false;
			this.constantSpeed = false;
			this.color = new spine.Color(1, 1, 1, 1);
		}
		return PathAttachment;
	}(spine.VertexAttachment));
	spine.PathAttachment = PathAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var RegionAttachment = (function (_super) {
		__extends(RegionAttachment, _super);
		function RegionAttachment(name) {
			_super.call(this, name);
			this.x = 0;
			this.y = 0;
			this.scaleX = 1;
			this.scaleY = 1;
			this.rotation = 0;
			this.width = 0;
			this.height = 0;
			this.color = new spine.Color(1, 1, 1, 1);
			this.offset = spine.Utils.newFloatArray(8);
			this.vertices = spine.Utils.newFloatArray(8 * 4);
			this.tempColor = new spine.Color(1, 1, 1, 1);
		}
		RegionAttachment.prototype.setRegion = function (region) {
			var vertices = this.vertices;
			if (region.rotate) {
				vertices[RegionAttachment.U2] = region.u;
				vertices[RegionAttachment.V2] = region.v2;
				vertices[RegionAttachment.U3] = region.u;
				vertices[RegionAttachment.V3] = region.v;
				vertices[RegionAttachment.U4] = region.u2;
				vertices[RegionAttachment.V4] = region.v;
				vertices[RegionAttachment.U1] = region.u2;
				vertices[RegionAttachment.V1] = region.v2;
			}
			else {
				vertices[RegionAttachment.U1] = region.u;
				vertices[RegionAttachment.V1] = region.v2;
				vertices[RegionAttachment.U2] = region.u;
				vertices[RegionAttachment.V2] = region.v;
				vertices[RegionAttachment.U3] = region.u2;
				vertices[RegionAttachment.V3] = region.v;
				vertices[RegionAttachment.U4] = region.u2;
				vertices[RegionAttachment.V4] = region.v2;
			}
		};
		RegionAttachment.prototype.updateOffset = function () {
			var regionScaleX = this.width / this.region.originalWidth * this.scaleX;
			var regionScaleY = this.height / this.region.originalHeight * this.scaleY;
			var localX = -this.width / 2 * this.scaleX + this.region.offsetX * regionScaleX;
			var localY = -this.height / 2 * this.scaleY + this.region.offsetY * regionScaleY;
			var localX2 = localX + this.region.width * regionScaleX;
			var localY2 = localY + this.region.height * regionScaleY;
			var radians = this.rotation * Math.PI / 180;
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
			offset[RegionAttachment.OX1] = localXCos - localYSin;
			offset[RegionAttachment.OY1] = localYCos + localXSin;
			offset[RegionAttachment.OX2] = localXCos - localY2Sin;
			offset[RegionAttachment.OY2] = localY2Cos + localXSin;
			offset[RegionAttachment.OX3] = localX2Cos - localY2Sin;
			offset[RegionAttachment.OY3] = localY2Cos + localX2Sin;
			offset[RegionAttachment.OX4] = localX2Cos - localYSin;
			offset[RegionAttachment.OY4] = localYCos + localX2Sin;
		};
		RegionAttachment.prototype.updateWorldVertices = function (slot, premultipliedAlpha) {
			var skeleton = slot.bone.skeleton;
			var skeletonColor = skeleton.color;
			var slotColor = slot.color;
			var regionColor = this.color;
			var alpha = skeletonColor.a * slotColor.a * regionColor.a;
			var multiplier = premultipliedAlpha ? alpha : 1;
			var color = this.tempColor;
			color.set(skeletonColor.r * slotColor.r * regionColor.r * multiplier, skeletonColor.g * slotColor.g * regionColor.g * multiplier, skeletonColor.b * slotColor.b * regionColor.b * multiplier, alpha);
			var vertices = this.vertices;
			var offset = this.offset;
			var bone = slot.bone;
			var x = skeleton.x + bone.worldX, y = skeleton.y + bone.worldY;
			var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			var offsetX = 0, offsetY = 0;
			offsetX = offset[RegionAttachment.OX1];
			offsetY = offset[RegionAttachment.OY1];
			vertices[RegionAttachment.X1] = offsetX * a + offsetY * b + x;
			vertices[RegionAttachment.Y1] = offsetX * c + offsetY * d + y;
			vertices[RegionAttachment.C1R] = color.r;
			vertices[RegionAttachment.C1G] = color.g;
			vertices[RegionAttachment.C1B] = color.b;
			vertices[RegionAttachment.C1A] = color.a;
			offsetX = offset[RegionAttachment.OX2];
			offsetY = offset[RegionAttachment.OY2];
			vertices[RegionAttachment.X2] = offsetX * a + offsetY * b + x;
			vertices[RegionAttachment.Y2] = offsetX * c + offsetY * d + y;
			vertices[RegionAttachment.C2R] = color.r;
			vertices[RegionAttachment.C2G] = color.g;
			vertices[RegionAttachment.C2B] = color.b;
			vertices[RegionAttachment.C2A] = color.a;
			offsetX = offset[RegionAttachment.OX3];
			offsetY = offset[RegionAttachment.OY3];
			vertices[RegionAttachment.X3] = offsetX * a + offsetY * b + x;
			vertices[RegionAttachment.Y3] = offsetX * c + offsetY * d + y;
			vertices[RegionAttachment.C3R] = color.r;
			vertices[RegionAttachment.C3G] = color.g;
			vertices[RegionAttachment.C3B] = color.b;
			vertices[RegionAttachment.C3A] = color.a;
			offsetX = offset[RegionAttachment.OX4];
			offsetY = offset[RegionAttachment.OY4];
			vertices[RegionAttachment.X4] = offsetX * a + offsetY * b + x;
			vertices[RegionAttachment.Y4] = offsetX * c + offsetY * d + y;
			vertices[RegionAttachment.C4R] = color.r;
			vertices[RegionAttachment.C4G] = color.g;
			vertices[RegionAttachment.C4B] = color.b;
			vertices[RegionAttachment.C4A] = color.a;
			return vertices;
		};
		RegionAttachment.OX1 = 0;
		RegionAttachment.OY1 = 1;
		RegionAttachment.OX2 = 2;
		RegionAttachment.OY2 = 3;
		RegionAttachment.OX3 = 4;
		RegionAttachment.OY3 = 5;
		RegionAttachment.OX4 = 6;
		RegionAttachment.OY4 = 7;
		RegionAttachment.X1 = 0;
		RegionAttachment.Y1 = 1;
		RegionAttachment.C1R = 2;
		RegionAttachment.C1G = 3;
		RegionAttachment.C1B = 4;
		RegionAttachment.C1A = 5;
		RegionAttachment.U1 = 6;
		RegionAttachment.V1 = 7;
		RegionAttachment.X2 = 8;
		RegionAttachment.Y2 = 9;
		RegionAttachment.C2R = 10;
		RegionAttachment.C2G = 11;
		RegionAttachment.C2B = 12;
		RegionAttachment.C2A = 13;
		RegionAttachment.U2 = 14;
		RegionAttachment.V2 = 15;
		RegionAttachment.X3 = 16;
		RegionAttachment.Y3 = 17;
		RegionAttachment.C3R = 18;
		RegionAttachment.C3G = 19;
		RegionAttachment.C3B = 20;
		RegionAttachment.C3A = 21;
		RegionAttachment.U3 = 22;
		RegionAttachment.V3 = 23;
		RegionAttachment.X4 = 24;
		RegionAttachment.Y4 = 25;
		RegionAttachment.C4R = 26;
		RegionAttachment.C4G = 27;
		RegionAttachment.C4B = 28;
		RegionAttachment.C4A = 29;
		RegionAttachment.U4 = 30;
		RegionAttachment.V4 = 31;
		return RegionAttachment;
	}(spine.Attachment));
	spine.RegionAttachment = RegionAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var AssetManager = (function (_super) {
			__extends(AssetManager, _super);
			function AssetManager(gl, pathPrefix) {
				if (pathPrefix === void 0) { pathPrefix = ""; }
				_super.call(this, function (image) {
					return new spine.webgl.GLTexture(gl, image);
				}, pathPrefix);
			}
			return AssetManager;
		}(spine.AssetManager));
		webgl.AssetManager = AssetManager;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var OrthoCamera = (function () {
			function OrthoCamera(viewportWidth, viewportHeight) {
				this.position = new webgl.Vector3(0, 0, 0);
				this.direction = new webgl.Vector3(0, 0, -1);
				this.up = new webgl.Vector3(0, 1, 0);
				this.near = 0;
				this.far = 100;
				this.zoom = 1;
				this.viewportWidth = 0;
				this.viewportHeight = 0;
				this.projectionView = new webgl.Matrix4();
				this.inverseProjectionView = new webgl.Matrix4();
				this.projection = new webgl.Matrix4();
				this.view = new webgl.Matrix4();
				this.tmp = new webgl.Vector3();
				this.viewportWidth = viewportWidth;
				this.viewportHeight = viewportHeight;
				this.update();
			}
			OrthoCamera.prototype.update = function () {
				var projection = this.projection;
				var view = this.view;
				var projectionView = this.projectionView;
				var inverseProjectionView = this.inverseProjectionView;
				var zoom = this.zoom, viewportWidth = this.viewportWidth, viewportHeight = this.viewportHeight;
				projection.ortho(zoom * (-viewportWidth / 2), zoom * (viewportWidth / 2), zoom * (-viewportHeight / 2), zoom * (viewportHeight / 2), this.near, this.far);
				view.lookAt(this.position, this.direction, this.up);
				projectionView.set(projection.values);
				projectionView.multiply(view);
				inverseProjectionView.set(projectionView.values).invert();
			};
			OrthoCamera.prototype.screenToWorld = function (screenCoords, screenWidth, screenHeight) {
				var x = screenCoords.x, y = screenHeight - screenCoords.y - 1;
				var tmp = this.tmp;
				tmp.x = (2 * x) / screenWidth - 1;
				tmp.y = (2 * y) / screenHeight - 1;
				tmp.z = (2 * screenCoords.z) - 1;
				tmp.project(this.inverseProjectionView);
				screenCoords.set(tmp.x, tmp.y, tmp.z);
				return screenCoords;
			};
			OrthoCamera.prototype.setViewport = function (viewportWidth, viewportHeight) {
				this.viewportWidth = viewportWidth;
				this.viewportHeight = viewportHeight;
			};
			return OrthoCamera;
		}());
		webgl.OrthoCamera = OrthoCamera;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var GLTexture = (function (_super) {
			__extends(GLTexture, _super);
			function GLTexture(gl, image, useMipMaps) {
				if (useMipMaps === void 0) { useMipMaps = false; }
				_super.call(this, image);
				this.boundUnit = 0;
				this.gl = gl;
				this.texture = gl.createTexture();
				this.update(useMipMaps);
			}
			GLTexture.prototype.setFilters = function (minFilter, magFilter) {
				var gl = this.gl;
				this.bind();
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, minFilter);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, magFilter);
			};
			GLTexture.prototype.setWraps = function (uWrap, vWrap) {
				var gl = this.gl;
				this.bind();
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, uWrap);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, vWrap);
			};
			GLTexture.prototype.update = function (useMipMaps) {
				var gl = this.gl;
				this.bind();
				gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this._image);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, useMipMaps ? gl.LINEAR_MIPMAP_LINEAR : gl.LINEAR);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
				if (useMipMaps)
					gl.generateMipmap(gl.TEXTURE_2D);
			};
			GLTexture.prototype.bind = function (unit) {
				if (unit === void 0) { unit = 0; }
				var gl = this.gl;
				this.boundUnit = unit;
				gl.activeTexture(gl.TEXTURE0 + unit);
				gl.bindTexture(gl.TEXTURE_2D, this.texture);
			};
			GLTexture.prototype.unbind = function () {
				var gl = this.gl;
				gl.activeTexture(gl.TEXTURE0 + this.boundUnit);
				gl.bindTexture(gl.TEXTURE_2D, null);
			};
			GLTexture.prototype.dispose = function () {
				var gl = this.gl;
				gl.deleteTexture(this.texture);
			};
			return GLTexture;
		}(spine.Texture));
		webgl.GLTexture = GLTexture;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var Input = (function () {
			function Input(element) {
				this.lastX = 0;
				this.lastY = 0;
				this.buttonDown = false;
				this.currTouch = null;
				this.touchesPool = new spine.Pool(function () {
					return new spine.webgl.Touch(0, 0, 0);
				});
				this.listeners = new Array();
				this.element = element;
				this.setupCallbacks(element);
			}
			Input.prototype.setupCallbacks = function (element) {
				var _this = this;
				element.addEventListener("mousedown", function (ev) {
					if (ev instanceof MouseEvent) {
						var rect = element.getBoundingClientRect();
						var x = ev.clientX - rect.left;
						var y = ev.clientY - rect.top;
						var listeners = _this.listeners;
						for (var i = 0; i < listeners.length; i++) {
							listeners[i].down(x, y);
						}
						_this.lastX = x;
						_this.lastY = y;
						_this.buttonDown = true;
					}
				}, true);
				element.addEventListener("mousemove", function (ev) {
					if (ev instanceof MouseEvent) {
						var rect = element.getBoundingClientRect();
						var x = ev.clientX - rect.left;
						var y = ev.clientY - rect.top;
						var listeners = _this.listeners;
						for (var i = 0; i < listeners.length; i++) {
							if (_this.buttonDown) {
								listeners[i].dragged(x, y);
							}
							else {
								listeners[i].moved(x, y);
							}
						}
						_this.lastX = x;
						_this.lastY = y;
					}
				}, true);
				element.addEventListener("mouseup", function (ev) {
					if (ev instanceof MouseEvent) {
						var rect = element.getBoundingClientRect();
						var x = ev.clientX - rect.left;
						var y = ev.clientY - rect.top;
						var listeners = _this.listeners;
						for (var i = 0; i < listeners.length; i++) {
							listeners[i].up(x, y);
						}
						_this.lastX = x;
						_this.lastY = y;
						_this.buttonDown = false;
					}
				}, true);
				element.addEventListener("touchstart", function (ev) {
					if (_this.currTouch != null)
						return;
					var touches = ev.changedTouches;
					for (var i = 0; i < touches.length; i++) {
						var touch = touches[i];
						var rect = element.getBoundingClientRect();
						var x = touch.clientX - rect.left;
						var y = touch.clientY - rect.top;
						_this.currTouch = _this.touchesPool.obtain();
						_this.currTouch.identifier = touch.identifier;
						_this.currTouch.x = x;
						_this.currTouch.y = y;
						break;
					}
					var listeners = _this.listeners;
					for (var i_1 = 0; i_1 < listeners.length; i_1++) {
						listeners[i_1].down(_this.currTouch.x, _this.currTouch.y);
					}
					console.log("Start " + _this.currTouch.x + ", " + _this.currTouch.y);
					_this.lastX = _this.currTouch.x;
					_this.lastY = _this.currTouch.y;
					_this.buttonDown = true;
					ev.preventDefault();
				}, false);
				element.addEventListener("touchend", function (ev) {
					var touches = ev.changedTouches;
					for (var i = 0; i < touches.length; i++) {
						var touch = touches[i];
						if (_this.currTouch.identifier === touch.identifier) {
							var rect = element.getBoundingClientRect();
							var x = _this.currTouch.x = touch.clientX - rect.left;
							var y = _this.currTouch.y = touch.clientY - rect.top;
							_this.touchesPool.free(_this.currTouch);
							var listeners = _this.listeners;
							for (var i_2 = 0; i_2 < listeners.length; i_2++) {
								listeners[i_2].up(x, y);
							}
							console.log("End " + x + ", " + y);
							_this.lastX = x;
							_this.lastY = y;
							_this.buttonDown = false;
							_this.currTouch = null;
							break;
						}
					}
					ev.preventDefault();
				}, false);
				element.addEventListener("touchcancel", function (ev) {
					var touches = ev.changedTouches;
					for (var i = 0; i < touches.length; i++) {
						var touch = touches[i];
						if (_this.currTouch.identifier === touch.identifier) {
							var rect = element.getBoundingClientRect();
							var x = _this.currTouch.x = touch.clientX - rect.left;
							var y = _this.currTouch.y = touch.clientY - rect.top;
							_this.touchesPool.free(_this.currTouch);
							var listeners = _this.listeners;
							for (var i_3 = 0; i_3 < listeners.length; i_3++) {
								listeners[i_3].up(x, y);
							}
							console.log("End " + x + ", " + y);
							_this.lastX = x;
							_this.lastY = y;
							_this.buttonDown = false;
							_this.currTouch = null;
							break;
						}
					}
					ev.preventDefault();
				}, false);
				element.addEventListener("touchmove", function (ev) {
					if (_this.currTouch == null)
						return;
					var touches = ev.changedTouches;
					for (var i = 0; i < touches.length; i++) {
						var touch = touches[i];
						if (_this.currTouch.identifier === touch.identifier) {
							var rect = element.getBoundingClientRect();
							var x = touch.clientX - rect.left;
							var y = touch.clientY - rect.top;
							var listeners = _this.listeners;
							for (var i_4 = 0; i_4 < listeners.length; i_4++) {
								listeners[i_4].dragged(x, y);
							}
							console.log("Drag " + x + ", " + y);
							_this.lastX = _this.currTouch.x = x;
							_this.lastY = _this.currTouch.y = y;
							break;
						}
					}
					ev.preventDefault();
				}, false);
			};
			Input.prototype.addListener = function (listener) {
				this.listeners.push(listener);
			};
			Input.prototype.removeListener = function (listener) {
				var idx = this.listeners.indexOf(listener);
				if (idx > -1) {
					this.listeners.splice(idx, 1);
				}
			};
			return Input;
		}());
		webgl.Input = Input;
		var Touch = (function () {
			function Touch(identifier, x, y) {
				this.identifier = identifier;
				this.x = x;
				this.y = y;
			}
			return Touch;
		}());
		webgl.Touch = Touch;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var LoadingScreen = (function () {
			function LoadingScreen(renderer) {
				this.logo = null;
				this.spinner = null;
				this.angle = 0;
				this.fadeOut = 0;
				this.timeKeeper = new spine.TimeKeeper();
				this.backgroundColor = new spine.Color(0.135, 0.135, 0.135, 1);
				this.tempColor = new spine.Color();
				this.firstDraw = 0;
				this.renderer = renderer;
				this.timeKeeper.maxDelta = 9;
				if (LoadingScreen.logoImg === null) {
					var isSafari = navigator.userAgent.indexOf("Safari") > -1;
					LoadingScreen.logoImg = new Image();
					LoadingScreen.logoImg.src = LoadingScreen.SPINE_LOGO_DATA;
					if (!isSafari)
						LoadingScreen.logoImg.crossOrigin = "anonymous";
					LoadingScreen.logoImg.onload = function (ev) {
						LoadingScreen.loaded++;
					};
					LoadingScreen.spinnerImg = new Image();
					LoadingScreen.spinnerImg.src = LoadingScreen.SPINNER_DATA;
					if (!isSafari)
						LoadingScreen.spinnerImg.crossOrigin = "anonymous";
					LoadingScreen.spinnerImg.onload = function (ev) {
						LoadingScreen.loaded++;
					};
				}
			}
			LoadingScreen.prototype.draw = function (complete) {
				if (complete === void 0) { complete = false; }
				if (complete && this.fadeOut > LoadingScreen.FADE_SECONDS)
					return;
				this.timeKeeper.update();
				var a = Math.abs(Math.sin(this.timeKeeper.totalTime + 0.75));
				this.angle -= this.timeKeeper.delta * 360 * (1 + 1.5 * Math.pow(a, 5));
				var renderer = this.renderer;
				var canvas = renderer.canvas;
				var gl = renderer.gl;
				var oldX = renderer.camera.position.x, oldY = renderer.camera.position.y;
				renderer.camera.position.set(canvas.width / 2, canvas.height / 2, 0);
				renderer.camera.viewportWidth = canvas.width;
				renderer.camera.viewportHeight = canvas.height;
				renderer.resize(webgl.ResizeMode.Stretch);
				if (!complete) {
					gl.clearColor(this.backgroundColor.r, this.backgroundColor.g, this.backgroundColor.b, this.backgroundColor.a);
					gl.clear(gl.COLOR_BUFFER_BIT);
					this.tempColor.a = 1;
				}
				else {
					this.fadeOut += this.timeKeeper.delta * (this.timeKeeper.totalTime < 1 ? 2 : 1);
					if (this.fadeOut > LoadingScreen.FADE_SECONDS) {
						renderer.camera.position.set(oldX, oldY, 0);
						return;
					}
					a = 1 - this.fadeOut / LoadingScreen.FADE_SECONDS;
					this.tempColor.setFromColor(this.backgroundColor);
					this.tempColor.a = 1 - (a - 1) * (a - 1);
					renderer.begin();
					renderer.quad(true, 0, 0, canvas.width, 0, canvas.width, canvas.height, 0, canvas.height, this.tempColor, this.tempColor, this.tempColor, this.tempColor);
					renderer.end();
				}
				this.tempColor.set(1, 1, 1, this.tempColor.a);
				if (LoadingScreen.loaded != 2)
					return;
				if (this.logo === null) {
					this.logo = new webgl.GLTexture(renderer.gl, LoadingScreen.logoImg);
					this.spinner = new webgl.GLTexture(renderer.gl, LoadingScreen.spinnerImg);
				}
				this.logo.update(false);
				this.spinner.update(false);
				var logoWidth = this.logo.getImage().width;
				var logoHeight = this.logo.getImage().height;
				var spinnerWidth = this.spinner.getImage().width;
				var spinnerHeight = this.spinner.getImage().height;
				renderer.batcher.setBlendMode(WebGLRenderingContext.SRC_ALPHA, WebGLRenderingContext.ONE_MINUS_SRC_ALPHA);
				renderer.begin();
				renderer.drawTexture(this.logo, (canvas.width - logoWidth) / 2, (canvas.height - logoHeight) / 2, logoWidth, logoHeight, this.tempColor);
				renderer.drawTextureRotated(this.spinner, (canvas.width - spinnerWidth) / 2, (canvas.height - spinnerHeight) / 2, spinnerWidth, spinnerHeight, spinnerWidth / 2, spinnerHeight / 2, this.angle, this.tempColor);
				renderer.end();
				renderer.camera.position.set(oldX, oldY, 0);
			};
			LoadingScreen.FADE_SECONDS = 1;
			LoadingScreen.loaded = 0;
			LoadingScreen.spinnerImg = null;
			LoadingScreen.logoImg = null;
			LoadingScreen.SPINNER_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKAAAAChCAMAAAB3TUS6AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAYNQTFRFAAAA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AA/0AAkTDRyAAAAIB0Uk5TAAABAgMEBQYHCAkKCwwODxAREhMUFRYXGBkaHB0eICEiIyQlJicoKSorLC0uLzAxMjM0Nzg5Ojs8PT4/QEFDRUlKS0xNTk9QUlRWWFlbXF1eYWJjZmhscHF0d3h5e3x+f4CIiYuMj5GSlJWXm56io6arr7rAxcjO0dXe6Onr8fmb5sOOAAADuElEQVQYGe3B+3vTVBwH4M/3nCRt13br2Lozhug2q25gYQubcxqVKYoMCYoKjEsUdSpeiBc0Kl7yp9t2za39pely7PF5zvuiQKc+/e2f8K+f9g2oyQ77Ag4VGX+HketQ0XYYe0JQ0CdhogwF+WFiBgr6JkxUoKCDMMGgoP0w9gdUtB3GfoCKVsPYAVQ0H8YuQUWVMHYGKuJhrAklPQkjJpT0bdj3O9S0FfZ9ADXxP8MjVSiqFfa8B2VVV8+df14QtB4iwn+BpuZEgyM38WMQHDYhnbkgukrIh5ygZ48glyn6KshlL+jbhVRcxCzk0ApiC5CI5kVsgTAy9jiI/WxBGmqIFBMjqwYphwRZaiLNwsjqQdoVSFISGRwjM4OMFUjBRcYCYWT0XZD2SwUS0LzIKCGH2SDja0LxKiJjCrm0gowVFI6aIs1CTouPg5QvUTgSKXMMuVUeBSmEopFITBPGwO8HCYbCTYtImTAWejuI3CMUjmZFT5NjbM/9GvQcMkhADdFRIxxD7aug4wGDFGSVTcLx0MzutQ2CpmmapmmapmmapmmapmmaphWBmGFV6rNNcaLC0GUuv3LROftUo8wJk0a10207sVED6IIf+9673LIwQeW2PaCEJX/A+xYmhTbtQUu46g96SJgQZg9Zwxf+EAMTwuwhm3jkD7EwIdweBn+YhQlh9pA2HvpDTEwIs4es4GN/CMekNOxBJ9D2B10nTAyfW7fT1hjYgZ/xYIUwUcycaiwuv2h3tOcZADr7ud/12c0ru2cWSwQ1UAcixIgImqZpmqZpmqZpmqZpmqZp2v8HMSIcF186t8oghbnlOJt1wnHwl7yOGxwSlHacrjWG8dVuej03OApn7jhHtiyMiZa9yD6haLYTebWOsbDXvQRHwchJWSTkV/rQS+EoWttJaTHkJe56KXcJRZt20jY48nnBy9hE4WjLSbvAkIfwMm5zFG/KyWgRRke3vYwGZDjpZHCMruJltCAFrTtpVYxu1ktzCHKwbSdlGqOreynXGGQpOylljI5uebFbBuSZc2IbhBxmvcj9GiSiZ52+HQO5nPb6TkIqajs9L5eQk7jnddxZgGT0jNOxYSI36+Kdj9oG5OPV6QpB6yJuGAYnqIrecLveYlDUKffIOtREl90+BiWV3cgMlNR0I09DSS030oaSttzILpT0phu5BBWRmyAoiLkJgoIMN8GgoJKb4FBQzU0YUFDdTRhQUNVNcCjIdBMEBdE7buQ8lFRz+97lUFN5fe+qu//aMkeB/gU2ae9y2HgbngAAAABJRU5ErkJggg==";
			LoadingScreen.SPINE_LOGO_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAFIAAAAZCAYAAACis3k0AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAtNJREFUaN7tmT2I1EAUxwN+oWgRT0HFKo0WCkJ6ObmAWFwZbCxsXGysLNJaiCyIoDaSwk4ETzvhmnBaCRbBWoQ01ho4PwotjP8cE337mMy8TLK757mBH3fLTWbe/PbN53neNniqZW8FvAVvQAqugwvgDDgO9niLRyTyJagM/ACPF6bsIl9ZRDac/Cc6tLn5xQdRQ496QlKPLxD5QCDxO9jtGM8QfYoIgUlgCipGCRJL5VvlyOdCU09iEXkCfLSIfCrs7Fab6nOsiafu06iDwES9w/uU1QnDC+ekkVS9vEaDsgVeB0d+z1VDtOGxRaYPboP3Gokb4GgXkZp4chZPJKgvZ3U0XkriK/TIt9YUDllFgTAjGwoaoHqfBhMI58yD4BQ4V6/aHYdfxToftvw9F2SiVroawU2/Cv5C4Thv0KB9S5nxlOd4STxjwUjzSdYlgrYijw2BsEfgsaFcM09lhiys94xXQQwugcvgJrgFLjrEE7WUiTuWCQzt/ZXN7FfqGwuGClyVy2xZAFmfDQvNtwFFSspMDGsD+UTWqu1KoVmVooFEJgKRXw0if85RpISEzwsjzeqWzkjkC4PIJ3MUmQgITAHlQwTFhnZhELkEntfZRwR+AvfAgXmJHOqU02XligWT8ppg67NXbdCXeq7afUQ6L8C2DalEZNt2YyQ94Qy8/ekjMpBMbfyl5iTjG7YAI8cNecROAb4kJmTjaXAF3AGvwQewOiuRxEtlSaT4j2h2lMsUueQEoMlIKpTvAmKhxPMtC876jEX6rE8l8TNx/KVbn6xlWU9NWcSDUsO4NGWpQOTZFpHPOooMXcswmW2XFk3ixb2v0Nq+XVKP00QNaffBLyWwBI/AkTlfMYZDXMf12kc6yjwEjoFdO/5me5oi/6tnyhlZX6OtgmX1c2Uh0k3khmbB2b9TRfpd/jfTUeRDJvHdYg5wE7kPXAN3wQ1weDvH+xufEgpi5qIl3QAAAABJRU5ErkJggg==";
			return LoadingScreen;
		}());
		webgl.LoadingScreen = LoadingScreen;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		webgl.M00 = 0;
		webgl.M01 = 4;
		webgl.M02 = 8;
		webgl.M03 = 12;
		webgl.M10 = 1;
		webgl.M11 = 5;
		webgl.M12 = 9;
		webgl.M13 = 13;
		webgl.M20 = 2;
		webgl.M21 = 6;
		webgl.M22 = 10;
		webgl.M23 = 14;
		webgl.M30 = 3;
		webgl.M31 = 7;
		webgl.M32 = 11;
		webgl.M33 = 15;
		var Matrix4 = (function () {
			function Matrix4() {
				this.temp = new Float32Array(16);
				this.values = new Float32Array(16);
				var v = this.values;
				v[webgl.M00] = 1;
				v[webgl.M11] = 1;
				v[webgl.M22] = 1;
				v[webgl.M33] = 1;
			}
			Matrix4.prototype.set = function (values) {
				this.values.set(values);
				return this;
			};
			Matrix4.prototype.transpose = function () {
				var t = this.temp;
				var v = this.values;
				t[webgl.M00] = v[webgl.M00];
				t[webgl.M01] = v[webgl.M10];
				t[webgl.M02] = v[webgl.M20];
				t[webgl.M03] = v[webgl.M30];
				t[webgl.M10] = v[webgl.M01];
				t[webgl.M11] = v[webgl.M11];
				t[webgl.M12] = v[webgl.M21];
				t[webgl.M13] = v[webgl.M31];
				t[webgl.M20] = v[webgl.M02];
				t[webgl.M21] = v[webgl.M12];
				t[webgl.M22] = v[webgl.M22];
				t[webgl.M23] = v[webgl.M32];
				t[webgl.M30] = v[webgl.M03];
				t[webgl.M31] = v[webgl.M13];
				t[webgl.M32] = v[webgl.M23];
				t[webgl.M33] = v[webgl.M33];
				return this.set(t);
			};
			Matrix4.prototype.identity = function () {
				var v = this.values;
				v[webgl.M00] = 1;
				v[webgl.M01] = 0;
				v[webgl.M02] = 0;
				v[webgl.M03] = 0;
				v[webgl.M10] = 0;
				v[webgl.M11] = 1;
				v[webgl.M12] = 0;
				v[webgl.M13] = 0;
				v[webgl.M20] = 0;
				v[webgl.M21] = 0;
				v[webgl.M22] = 1;
				v[webgl.M23] = 0;
				v[webgl.M30] = 0;
				v[webgl.M31] = 0;
				v[webgl.M32] = 0;
				v[webgl.M33] = 1;
				return this;
			};
			Matrix4.prototype.invert = function () {
				var v = this.values;
				var t = this.temp;
				var l_det = v[webgl.M30] * v[webgl.M21] * v[webgl.M12] * v[webgl.M03] - v[webgl.M20] * v[webgl.M31] * v[webgl.M12] * v[webgl.M03] - v[webgl.M30] * v[webgl.M11] * v[webgl.M22] * v[webgl.M03]
					+ v[webgl.M10] * v[webgl.M31] * v[webgl.M22] * v[webgl.M03] + v[webgl.M20] * v[webgl.M11] * v[webgl.M32] * v[webgl.M03] - v[webgl.M10] * v[webgl.M21] * v[webgl.M32] * v[webgl.M03]
					- v[webgl.M30] * v[webgl.M21] * v[webgl.M02] * v[webgl.M13] + v[webgl.M20] * v[webgl.M31] * v[webgl.M02] * v[webgl.M13] + v[webgl.M30] * v[webgl.M01] * v[webgl.M22] * v[webgl.M13]
					- v[webgl.M00] * v[webgl.M31] * v[webgl.M22] * v[webgl.M13] - v[webgl.M20] * v[webgl.M01] * v[webgl.M32] * v[webgl.M13] + v[webgl.M00] * v[webgl.M21] * v[webgl.M32] * v[webgl.M13]
					+ v[webgl.M30] * v[webgl.M11] * v[webgl.M02] * v[webgl.M23] - v[webgl.M10] * v[webgl.M31] * v[webgl.M02] * v[webgl.M23] - v[webgl.M30] * v[webgl.M01] * v[webgl.M12] * v[webgl.M23]
					+ v[webgl.M00] * v[webgl.M31] * v[webgl.M12] * v[webgl.M23] + v[webgl.M10] * v[webgl.M01] * v[webgl.M32] * v[webgl.M23] - v[webgl.M00] * v[webgl.M11] * v[webgl.M32] * v[webgl.M23]
					- v[webgl.M20] * v[webgl.M11] * v[webgl.M02] * v[webgl.M33] + v[webgl.M10] * v[webgl.M21] * v[webgl.M02] * v[webgl.M33] + v[webgl.M20] * v[webgl.M01] * v[webgl.M12] * v[webgl.M33]
					- v[webgl.M00] * v[webgl.M21] * v[webgl.M12] * v[webgl.M33] - v[webgl.M10] * v[webgl.M01] * v[webgl.M22] * v[webgl.M33] + v[webgl.M00] * v[webgl.M11] * v[webgl.M22] * v[webgl.M33];
				if (l_det == 0)
					throw new Error("non-invertible matrix");
				var inv_det = 1.0 / l_det;
				t[webgl.M00] = v[webgl.M12] * v[webgl.M23] * v[webgl.M31] - v[webgl.M13] * v[webgl.M22] * v[webgl.M31] + v[webgl.M13] * v[webgl.M21] * v[webgl.M32]
					- v[webgl.M11] * v[webgl.M23] * v[webgl.M32] - v[webgl.M12] * v[webgl.M21] * v[webgl.M33] + v[webgl.M11] * v[webgl.M22] * v[webgl.M33];
				t[webgl.M01] = v[webgl.M03] * v[webgl.M22] * v[webgl.M31] - v[webgl.M02] * v[webgl.M23] * v[webgl.M31] - v[webgl.M03] * v[webgl.M21] * v[webgl.M32]
					+ v[webgl.M01] * v[webgl.M23] * v[webgl.M32] + v[webgl.M02] * v[webgl.M21] * v[webgl.M33] - v[webgl.M01] * v[webgl.M22] * v[webgl.M33];
				t[webgl.M02] = v[webgl.M02] * v[webgl.M13] * v[webgl.M31] - v[webgl.M03] * v[webgl.M12] * v[webgl.M31] + v[webgl.M03] * v[webgl.M11] * v[webgl.M32]
					- v[webgl.M01] * v[webgl.M13] * v[webgl.M32] - v[webgl.M02] * v[webgl.M11] * v[webgl.M33] + v[webgl.M01] * v[webgl.M12] * v[webgl.M33];
				t[webgl.M03] = v[webgl.M03] * v[webgl.M12] * v[webgl.M21] - v[webgl.M02] * v[webgl.M13] * v[webgl.M21] - v[webgl.M03] * v[webgl.M11] * v[webgl.M22]
					+ v[webgl.M01] * v[webgl.M13] * v[webgl.M22] + v[webgl.M02] * v[webgl.M11] * v[webgl.M23] - v[webgl.M01] * v[webgl.M12] * v[webgl.M23];
				t[webgl.M10] = v[webgl.M13] * v[webgl.M22] * v[webgl.M30] - v[webgl.M12] * v[webgl.M23] * v[webgl.M30] - v[webgl.M13] * v[webgl.M20] * v[webgl.M32]
					+ v[webgl.M10] * v[webgl.M23] * v[webgl.M32] + v[webgl.M12] * v[webgl.M20] * v[webgl.M33] - v[webgl.M10] * v[webgl.M22] * v[webgl.M33];
				t[webgl.M11] = v[webgl.M02] * v[webgl.M23] * v[webgl.M30] - v[webgl.M03] * v[webgl.M22] * v[webgl.M30] + v[webgl.M03] * v[webgl.M20] * v[webgl.M32]
					- v[webgl.M00] * v[webgl.M23] * v[webgl.M32] - v[webgl.M02] * v[webgl.M20] * v[webgl.M33] + v[webgl.M00] * v[webgl.M22] * v[webgl.M33];
				t[webgl.M12] = v[webgl.M03] * v[webgl.M12] * v[webgl.M30] - v[webgl.M02] * v[webgl.M13] * v[webgl.M30] - v[webgl.M03] * v[webgl.M10] * v[webgl.M32]
					+ v[webgl.M00] * v[webgl.M13] * v[webgl.M32] + v[webgl.M02] * v[webgl.M10] * v[webgl.M33] - v[webgl.M00] * v[webgl.M12] * v[webgl.M33];
				t[webgl.M13] = v[webgl.M02] * v[webgl.M13] * v[webgl.M20] - v[webgl.M03] * v[webgl.M12] * v[webgl.M20] + v[webgl.M03] * v[webgl.M10] * v[webgl.M22]
					- v[webgl.M00] * v[webgl.M13] * v[webgl.M22] - v[webgl.M02] * v[webgl.M10] * v[webgl.M23] + v[webgl.M00] * v[webgl.M12] * v[webgl.M23];
				t[webgl.M20] = v[webgl.M11] * v[webgl.M23] * v[webgl.M30] - v[webgl.M13] * v[webgl.M21] * v[webgl.M30] + v[webgl.M13] * v[webgl.M20] * v[webgl.M31]
					- v[webgl.M10] * v[webgl.M23] * v[webgl.M31] - v[webgl.M11] * v[webgl.M20] * v[webgl.M33] + v[webgl.M10] * v[webgl.M21] * v[webgl.M33];
				t[webgl.M21] = v[webgl.M03] * v[webgl.M21] * v[webgl.M30] - v[webgl.M01] * v[webgl.M23] * v[webgl.M30] - v[webgl.M03] * v[webgl.M20] * v[webgl.M31]
					+ v[webgl.M00] * v[webgl.M23] * v[webgl.M31] + v[webgl.M01] * v[webgl.M20] * v[webgl.M33] - v[webgl.M00] * v[webgl.M21] * v[webgl.M33];
				t[webgl.M22] = v[webgl.M01] * v[webgl.M13] * v[webgl.M30] - v[webgl.M03] * v[webgl.M11] * v[webgl.M30] + v[webgl.M03] * v[webgl.M10] * v[webgl.M31]
					- v[webgl.M00] * v[webgl.M13] * v[webgl.M31] - v[webgl.M01] * v[webgl.M10] * v[webgl.M33] + v[webgl.M00] * v[webgl.M11] * v[webgl.M33];
				t[webgl.M23] = v[webgl.M03] * v[webgl.M11] * v[webgl.M20] - v[webgl.M01] * v[webgl.M13] * v[webgl.M20] - v[webgl.M03] * v[webgl.M10] * v[webgl.M21]
					+ v[webgl.M00] * v[webgl.M13] * v[webgl.M21] + v[webgl.M01] * v[webgl.M10] * v[webgl.M23] - v[webgl.M00] * v[webgl.M11] * v[webgl.M23];
				t[webgl.M30] = v[webgl.M12] * v[webgl.M21] * v[webgl.M30] - v[webgl.M11] * v[webgl.M22] * v[webgl.M30] - v[webgl.M12] * v[webgl.M20] * v[webgl.M31]
					+ v[webgl.M10] * v[webgl.M22] * v[webgl.M31] + v[webgl.M11] * v[webgl.M20] * v[webgl.M32] - v[webgl.M10] * v[webgl.M21] * v[webgl.M32];
				t[webgl.M31] = v[webgl.M01] * v[webgl.M22] * v[webgl.M30] - v[webgl.M02] * v[webgl.M21] * v[webgl.M30] + v[webgl.M02] * v[webgl.M20] * v[webgl.M31]
					- v[webgl.M00] * v[webgl.M22] * v[webgl.M31] - v[webgl.M01] * v[webgl.M20] * v[webgl.M32] + v[webgl.M00] * v[webgl.M21] * v[webgl.M32];
				t[webgl.M32] = v[webgl.M02] * v[webgl.M11] * v[webgl.M30] - v[webgl.M01] * v[webgl.M12] * v[webgl.M30] - v[webgl.M02] * v[webgl.M10] * v[webgl.M31]
					+ v[webgl.M00] * v[webgl.M12] * v[webgl.M31] + v[webgl.M01] * v[webgl.M10] * v[webgl.M32] - v[webgl.M00] * v[webgl.M11] * v[webgl.M32];
				t[webgl.M33] = v[webgl.M01] * v[webgl.M12] * v[webgl.M20] - v[webgl.M02] * v[webgl.M11] * v[webgl.M20] + v[webgl.M02] * v[webgl.M10] * v[webgl.M21]
					- v[webgl.M00] * v[webgl.M12] * v[webgl.M21] - v[webgl.M01] * v[webgl.M10] * v[webgl.M22] + v[webgl.M00] * v[webgl.M11] * v[webgl.M22];
				v[webgl.M00] = t[webgl.M00] * inv_det;
				v[webgl.M01] = t[webgl.M01] * inv_det;
				v[webgl.M02] = t[webgl.M02] * inv_det;
				v[webgl.M03] = t[webgl.M03] * inv_det;
				v[webgl.M10] = t[webgl.M10] * inv_det;
				v[webgl.M11] = t[webgl.M11] * inv_det;
				v[webgl.M12] = t[webgl.M12] * inv_det;
				v[webgl.M13] = t[webgl.M13] * inv_det;
				v[webgl.M20] = t[webgl.M20] * inv_det;
				v[webgl.M21] = t[webgl.M21] * inv_det;
				v[webgl.M22] = t[webgl.M22] * inv_det;
				v[webgl.M23] = t[webgl.M23] * inv_det;
				v[webgl.M30] = t[webgl.M30] * inv_det;
				v[webgl.M31] = t[webgl.M31] * inv_det;
				v[webgl.M32] = t[webgl.M32] * inv_det;
				v[webgl.M33] = t[webgl.M33] * inv_det;
				return this;
			};
			Matrix4.prototype.determinant = function () {
				var v = this.values;
				return v[webgl.M30] * v[webgl.M21] * v[webgl.M12] * v[webgl.M03] - v[webgl.M20] * v[webgl.M31] * v[webgl.M12] * v[webgl.M03] - v[webgl.M30] * v[webgl.M11] * v[webgl.M22] * v[webgl.M03]
					+ v[webgl.M10] * v[webgl.M31] * v[webgl.M22] * v[webgl.M03] + v[webgl.M20] * v[webgl.M11] * v[webgl.M32] * v[webgl.M03] - v[webgl.M10] * v[webgl.M21] * v[webgl.M32] * v[webgl.M03]
					- v[webgl.M30] * v[webgl.M21] * v[webgl.M02] * v[webgl.M13] + v[webgl.M20] * v[webgl.M31] * v[webgl.M02] * v[webgl.M13] + v[webgl.M30] * v[webgl.M01] * v[webgl.M22] * v[webgl.M13]
					- v[webgl.M00] * v[webgl.M31] * v[webgl.M22] * v[webgl.M13] - v[webgl.M20] * v[webgl.M01] * v[webgl.M32] * v[webgl.M13] + v[webgl.M00] * v[webgl.M21] * v[webgl.M32] * v[webgl.M13]
					+ v[webgl.M30] * v[webgl.M11] * v[webgl.M02] * v[webgl.M23] - v[webgl.M10] * v[webgl.M31] * v[webgl.M02] * v[webgl.M23] - v[webgl.M30] * v[webgl.M01] * v[webgl.M12] * v[webgl.M23]
					+ v[webgl.M00] * v[webgl.M31] * v[webgl.M12] * v[webgl.M23] + v[webgl.M10] * v[webgl.M01] * v[webgl.M32] * v[webgl.M23] - v[webgl.M00] * v[webgl.M11] * v[webgl.M32] * v[webgl.M23]
					- v[webgl.M20] * v[webgl.M11] * v[webgl.M02] * v[webgl.M33] + v[webgl.M10] * v[webgl.M21] * v[webgl.M02] * v[webgl.M33] + v[webgl.M20] * v[webgl.M01] * v[webgl.M12] * v[webgl.M33]
					- v[webgl.M00] * v[webgl.M21] * v[webgl.M12] * v[webgl.M33] - v[webgl.M10] * v[webgl.M01] * v[webgl.M22] * v[webgl.M33] + v[webgl.M00] * v[webgl.M11] * v[webgl.M22] * v[webgl.M33];
			};
			Matrix4.prototype.translate = function (x, y, z) {
				var v = this.values;
				v[webgl.M03] += x;
				v[webgl.M13] += y;
				v[webgl.M23] += z;
				return this;
			};
			Matrix4.prototype.copy = function () {
				return new Matrix4().set(this.values);
			};
			Matrix4.prototype.projection = function (near, far, fovy, aspectRatio) {
				this.identity();
				var l_fd = (1.0 / Math.tan((fovy * (Math.PI / 180)) / 2.0));
				var l_a1 = (far + near) / (near - far);
				var l_a2 = (2 * far * near) / (near - far);
				var v = this.values;
				v[webgl.M00] = l_fd / aspectRatio;
				v[webgl.M10] = 0;
				v[webgl.M20] = 0;
				v[webgl.M30] = 0;
				v[webgl.M01] = 0;
				v[webgl.M11] = l_fd;
				v[webgl.M21] = 0;
				v[webgl.M31] = 0;
				v[webgl.M02] = 0;
				v[webgl.M12] = 0;
				v[webgl.M22] = l_a1;
				v[webgl.M32] = -1;
				v[webgl.M03] = 0;
				v[webgl.M13] = 0;
				v[webgl.M23] = l_a2;
				v[webgl.M33] = 0;
				return this;
			};
			Matrix4.prototype.ortho2d = function (x, y, width, height) {
				return this.ortho(x, x + width, y, y + height, 0, 1);
			};
			Matrix4.prototype.ortho = function (left, right, bottom, top, near, far) {
				this.identity();
				var x_orth = 2 / (right - left);
				var y_orth = 2 / (top - bottom);
				var z_orth = -2 / (far - near);
				var tx = -(right + left) / (right - left);
				var ty = -(top + bottom) / (top - bottom);
				var tz = -(far + near) / (far - near);
				var v = this.values;
				v[webgl.M00] = x_orth;
				v[webgl.M10] = 0;
				v[webgl.M20] = 0;
				v[webgl.M30] = 0;
				v[webgl.M01] = 0;
				v[webgl.M11] = y_orth;
				v[webgl.M21] = 0;
				v[webgl.M31] = 0;
				v[webgl.M02] = 0;
				v[webgl.M12] = 0;
				v[webgl.M22] = z_orth;
				v[webgl.M32] = 0;
				v[webgl.M03] = tx;
				v[webgl.M13] = ty;
				v[webgl.M23] = tz;
				v[webgl.M33] = 1;
				return this;
			};
			Matrix4.prototype.multiply = function (matrix) {
				var t = this.temp;
				var v = this.values;
				var m = matrix.values;
				t[webgl.M00] = v[webgl.M00] * m[webgl.M00] + v[webgl.M01] * m[webgl.M10] + v[webgl.M02] * m[webgl.M20] + v[webgl.M03] * m[webgl.M30];
				t[webgl.M01] = v[webgl.M00] * m[webgl.M01] + v[webgl.M01] * m[webgl.M11] + v[webgl.M02] * m[webgl.M21] + v[webgl.M03] * m[webgl.M31];
				t[webgl.M02] = v[webgl.M00] * m[webgl.M02] + v[webgl.M01] * m[webgl.M12] + v[webgl.M02] * m[webgl.M22] + v[webgl.M03] * m[webgl.M32];
				t[webgl.M03] = v[webgl.M00] * m[webgl.M03] + v[webgl.M01] * m[webgl.M13] + v[webgl.M02] * m[webgl.M23] + v[webgl.M03] * m[webgl.M33];
				t[webgl.M10] = v[webgl.M10] * m[webgl.M00] + v[webgl.M11] * m[webgl.M10] + v[webgl.M12] * m[webgl.M20] + v[webgl.M13] * m[webgl.M30];
				t[webgl.M11] = v[webgl.M10] * m[webgl.M01] + v[webgl.M11] * m[webgl.M11] + v[webgl.M12] * m[webgl.M21] + v[webgl.M13] * m[webgl.M31];
				t[webgl.M12] = v[webgl.M10] * m[webgl.M02] + v[webgl.M11] * m[webgl.M12] + v[webgl.M12] * m[webgl.M22] + v[webgl.M13] * m[webgl.M32];
				t[webgl.M13] = v[webgl.M10] * m[webgl.M03] + v[webgl.M11] * m[webgl.M13] + v[webgl.M12] * m[webgl.M23] + v[webgl.M13] * m[webgl.M33];
				t[webgl.M20] = v[webgl.M20] * m[webgl.M00] + v[webgl.M21] * m[webgl.M10] + v[webgl.M22] * m[webgl.M20] + v[webgl.M23] * m[webgl.M30];
				t[webgl.M21] = v[webgl.M20] * m[webgl.M01] + v[webgl.M21] * m[webgl.M11] + v[webgl.M22] * m[webgl.M21] + v[webgl.M23] * m[webgl.M31];
				t[webgl.M22] = v[webgl.M20] * m[webgl.M02] + v[webgl.M21] * m[webgl.M12] + v[webgl.M22] * m[webgl.M22] + v[webgl.M23] * m[webgl.M32];
				t[webgl.M23] = v[webgl.M20] * m[webgl.M03] + v[webgl.M21] * m[webgl.M13] + v[webgl.M22] * m[webgl.M23] + v[webgl.M23] * m[webgl.M33];
				t[webgl.M30] = v[webgl.M30] * m[webgl.M00] + v[webgl.M31] * m[webgl.M10] + v[webgl.M32] * m[webgl.M20] + v[webgl.M33] * m[webgl.M30];
				t[webgl.M31] = v[webgl.M30] * m[webgl.M01] + v[webgl.M31] * m[webgl.M11] + v[webgl.M32] * m[webgl.M21] + v[webgl.M33] * m[webgl.M31];
				t[webgl.M32] = v[webgl.M30] * m[webgl.M02] + v[webgl.M31] * m[webgl.M12] + v[webgl.M32] * m[webgl.M22] + v[webgl.M33] * m[webgl.M32];
				t[webgl.M33] = v[webgl.M30] * m[webgl.M03] + v[webgl.M31] * m[webgl.M13] + v[webgl.M32] * m[webgl.M23] + v[webgl.M33] * m[webgl.M33];
				return this.set(this.temp);
			};
			Matrix4.prototype.multiplyLeft = function (matrix) {
				var t = this.temp;
				var v = this.values;
				var m = matrix.values;
				t[webgl.M00] = m[webgl.M00] * v[webgl.M00] + m[webgl.M01] * v[webgl.M10] + m[webgl.M02] * v[webgl.M20] + m[webgl.M03] * v[webgl.M30];
				t[webgl.M01] = m[webgl.M00] * v[webgl.M01] + m[webgl.M01] * v[webgl.M11] + m[webgl.M02] * v[webgl.M21] + m[webgl.M03] * v[webgl.M31];
				t[webgl.M02] = m[webgl.M00] * v[webgl.M02] + m[webgl.M01] * v[webgl.M12] + m[webgl.M02] * v[webgl.M22] + m[webgl.M03] * v[webgl.M32];
				t[webgl.M03] = m[webgl.M00] * v[webgl.M03] + m[webgl.M01] * v[webgl.M13] + m[webgl.M02] * v[webgl.M23] + m[webgl.M03] * v[webgl.M33];
				t[webgl.M10] = m[webgl.M10] * v[webgl.M00] + m[webgl.M11] * v[webgl.M10] + m[webgl.M12] * v[webgl.M20] + m[webgl.M13] * v[webgl.M30];
				t[webgl.M11] = m[webgl.M10] * v[webgl.M01] + m[webgl.M11] * v[webgl.M11] + m[webgl.M12] * v[webgl.M21] + m[webgl.M13] * v[webgl.M31];
				t[webgl.M12] = m[webgl.M10] * v[webgl.M02] + m[webgl.M11] * v[webgl.M12] + m[webgl.M12] * v[webgl.M22] + m[webgl.M13] * v[webgl.M32];
				t[webgl.M13] = m[webgl.M10] * v[webgl.M03] + m[webgl.M11] * v[webgl.M13] + m[webgl.M12] * v[webgl.M23] + m[webgl.M13] * v[webgl.M33];
				t[webgl.M20] = m[webgl.M20] * v[webgl.M00] + m[webgl.M21] * v[webgl.M10] + m[webgl.M22] * v[webgl.M20] + m[webgl.M23] * v[webgl.M30];
				t[webgl.M21] = m[webgl.M20] * v[webgl.M01] + m[webgl.M21] * v[webgl.M11] + m[webgl.M22] * v[webgl.M21] + m[webgl.M23] * v[webgl.M31];
				t[webgl.M22] = m[webgl.M20] * v[webgl.M02] + m[webgl.M21] * v[webgl.M12] + m[webgl.M22] * v[webgl.M22] + m[webgl.M23] * v[webgl.M32];
				t[webgl.M23] = m[webgl.M20] * v[webgl.M03] + m[webgl.M21] * v[webgl.M13] + m[webgl.M22] * v[webgl.M23] + m[webgl.M23] * v[webgl.M33];
				t[webgl.M30] = m[webgl.M30] * v[webgl.M00] + m[webgl.M31] * v[webgl.M10] + m[webgl.M32] * v[webgl.M20] + m[webgl.M33] * v[webgl.M30];
				t[webgl.M31] = m[webgl.M30] * v[webgl.M01] + m[webgl.M31] * v[webgl.M11] + m[webgl.M32] * v[webgl.M21] + m[webgl.M33] * v[webgl.M31];
				t[webgl.M32] = m[webgl.M30] * v[webgl.M02] + m[webgl.M31] * v[webgl.M12] + m[webgl.M32] * v[webgl.M22] + m[webgl.M33] * v[webgl.M32];
				t[webgl.M33] = m[webgl.M30] * v[webgl.M03] + m[webgl.M31] * v[webgl.M13] + m[webgl.M32] * v[webgl.M23] + m[webgl.M33] * v[webgl.M33];
				return this.set(this.temp);
			};
			Matrix4.prototype.lookAt = function (position, direction, up) {
				Matrix4.initTemps();
				var xAxis = Matrix4.xAxis, yAxis = Matrix4.yAxis, zAxis = Matrix4.zAxis;
				zAxis.setFrom(direction).normalize();
				xAxis.setFrom(direction).normalize();
				xAxis.cross(up).normalize();
				yAxis.setFrom(xAxis).cross(zAxis).normalize();
				this.identity();
				var val = this.values;
				val[webgl.M00] = xAxis.x;
				val[webgl.M01] = xAxis.y;
				val[webgl.M02] = xAxis.z;
				val[webgl.M10] = yAxis.x;
				val[webgl.M11] = yAxis.y;
				val[webgl.M12] = yAxis.z;
				val[webgl.M20] = -zAxis.x;
				val[webgl.M21] = -zAxis.y;
				val[webgl.M22] = -zAxis.z;
				Matrix4.tmpMatrix.identity();
				Matrix4.tmpMatrix.values[webgl.M03] = -position.x;
				Matrix4.tmpMatrix.values[webgl.M13] = -position.y;
				Matrix4.tmpMatrix.values[webgl.M23] = -position.z;
				this.multiply(Matrix4.tmpMatrix);
				return this;
			};
			Matrix4.initTemps = function () {
				if (Matrix4.xAxis === null)
					Matrix4.xAxis = new webgl.Vector3();
				if (Matrix4.yAxis === null)
					Matrix4.yAxis = new webgl.Vector3();
				if (Matrix4.zAxis === null)
					Matrix4.zAxis = new webgl.Vector3();
			};
			Matrix4.xAxis = null;
			Matrix4.yAxis = null;
			Matrix4.zAxis = null;
			Matrix4.tmpMatrix = new Matrix4();
			return Matrix4;
		}());
		webgl.Matrix4 = Matrix4;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var Mesh = (function () {
			function Mesh(gl, attributes, maxVertices, maxIndices) {
				this.attributes = attributes;
				this.verticesLength = 0;
				this.dirtyVertices = false;
				this.indicesLength = 0;
				this.dirtyIndices = false;
				this.elementsPerVertex = 0;
				this.gl = gl;
				this.elementsPerVertex = 0;
				for (var i = 0; i < attributes.length; i++) {
					this.elementsPerVertex += attributes[i].numElements;
				}
				this.vertices = new Float32Array(maxVertices * this.elementsPerVertex);
				this.indices = new Uint16Array(maxIndices);
			}
			Mesh.prototype.getAttributes = function () { return this.attributes; };
			Mesh.prototype.maxVertices = function () { return this.vertices.length / this.elementsPerVertex; };
			Mesh.prototype.numVertices = function () { return this.verticesLength / this.elementsPerVertex; };
			Mesh.prototype.setVerticesLength = function (length) {
				this.dirtyVertices = true;
				this.verticesLength = length;
			};
			Mesh.prototype.getVertices = function () { return this.vertices; };
			Mesh.prototype.maxIndices = function () { return this.indices.length; };
			Mesh.prototype.numIndices = function () { return this.indicesLength; };
			Mesh.prototype.setIndicesLength = function (length) {
				this.dirtyIndices = true;
				this.indicesLength = length;
			};
			Mesh.prototype.getIndices = function () { return this.indices; };
			;
			Mesh.prototype.setVertices = function (vertices) {
				this.dirtyVertices = true;
				if (vertices.length > this.vertices.length)
					throw Error("Mesh can't store more than " + this.maxVertices() + " vertices");
				this.vertices.set(vertices, 0);
				this.verticesLength = vertices.length;
			};
			Mesh.prototype.setIndices = function (indices) {
				this.dirtyIndices = true;
				if (indices.length > this.indices.length)
					throw Error("Mesh can't store more than " + this.maxIndices() + " indices");
				this.indices.set(indices, 0);
				this.indicesLength = indices.length;
			};
			Mesh.prototype.draw = function (shader, primitiveType) {
				this.drawWithOffset(shader, primitiveType, 0, this.indicesLength > 0 ? this.indicesLength : this.verticesLength / this.elementsPerVertex);
			};
			Mesh.prototype.drawWithOffset = function (shader, primitiveType, offset, count) {
				var gl = this.gl;
				if (this.dirtyVertices || this.dirtyIndices)
					this.update();
				this.bind(shader);
				if (this.indicesLength > 0)
					gl.drawElements(primitiveType, count, gl.UNSIGNED_SHORT, offset * 2);
				else
					gl.drawArrays(primitiveType, offset, count);
				this.unbind(shader);
			};
			Mesh.prototype.bind = function (shader) {
				var gl = this.gl;
				gl.bindBuffer(gl.ARRAY_BUFFER, this.verticesBuffer);
				var offset = 0;
				for (var i = 0; i < this.attributes.length; i++) {
					var attrib = this.attributes[i];
					var location_1 = shader.getAttributeLocation(attrib.name);
					gl.enableVertexAttribArray(location_1);
					gl.vertexAttribPointer(location_1, attrib.numElements, gl.FLOAT, false, this.elementsPerVertex * 4, offset * 4);
					offset += attrib.numElements;
				}
				if (this.indicesLength > 0)
					gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indicesBuffer);
			};
			Mesh.prototype.unbind = function (shader) {
				var gl = this.gl;
				for (var i = 0; i < this.attributes.length; i++) {
					var attrib = this.attributes[i];
					var location_2 = shader.getAttributeLocation(attrib.name);
					gl.disableVertexAttribArray(location_2);
				}
				gl.bindBuffer(gl.ARRAY_BUFFER, null);
				if (this.indicesLength > 0)
					gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);
			};
			Mesh.prototype.update = function () {
				var gl = this.gl;
				if (this.dirtyVertices) {
					if (!this.verticesBuffer) {
						this.verticesBuffer = gl.createBuffer();
					}
					gl.bindBuffer(gl.ARRAY_BUFFER, this.verticesBuffer);
					gl.bufferData(gl.ARRAY_BUFFER, this.vertices.subarray(0, this.verticesLength), gl.STATIC_DRAW);
					this.dirtyVertices = false;
				}
				if (this.dirtyIndices) {
					if (!this.indicesBuffer) {
						this.indicesBuffer = gl.createBuffer();
					}
					gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indicesBuffer);
					gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, this.indices.subarray(0, this.indicesLength), gl.STATIC_DRAW);
					this.dirtyIndices = false;
				}
			};
			Mesh.prototype.dispose = function () {
				var gl = this.gl;
				gl.deleteBuffer(this.verticesBuffer);
				gl.deleteBuffer(this.indicesBuffer);
			};
			return Mesh;
		}());
		webgl.Mesh = Mesh;
		var VertexAttribute = (function () {
			function VertexAttribute(name, type, numElements) {
				this.name = name;
				this.type = type;
				this.numElements = numElements;
			}
			return VertexAttribute;
		}());
		webgl.VertexAttribute = VertexAttribute;
		var Position2Attribute = (function (_super) {
			__extends(Position2Attribute, _super);
			function Position2Attribute() {
				_super.call(this, webgl.Shader.POSITION, VertexAttributeType.Float, 2);
			}
			return Position2Attribute;
		}(VertexAttribute));
		webgl.Position2Attribute = Position2Attribute;
		var Position3Attribute = (function (_super) {
			__extends(Position3Attribute, _super);
			function Position3Attribute() {
				_super.call(this, webgl.Shader.POSITION, VertexAttributeType.Float, 3);
			}
			return Position3Attribute;
		}(VertexAttribute));
		webgl.Position3Attribute = Position3Attribute;
		var TexCoordAttribute = (function (_super) {
			__extends(TexCoordAttribute, _super);
			function TexCoordAttribute(unit) {
				if (unit === void 0) { unit = 0; }
				_super.call(this, webgl.Shader.TEXCOORDS + (unit == 0 ? "" : unit), VertexAttributeType.Float, 2);
			}
			return TexCoordAttribute;
		}(VertexAttribute));
		webgl.TexCoordAttribute = TexCoordAttribute;
		var ColorAttribute = (function (_super) {
			__extends(ColorAttribute, _super);
			function ColorAttribute() {
				_super.call(this, webgl.Shader.COLOR, VertexAttributeType.Float, 4);
			}
			return ColorAttribute;
		}(VertexAttribute));
		webgl.ColorAttribute = ColorAttribute;
		(function (VertexAttributeType) {
			VertexAttributeType[VertexAttributeType["Float"] = 0] = "Float";
		})(webgl.VertexAttributeType || (webgl.VertexAttributeType = {}));
		var VertexAttributeType = webgl.VertexAttributeType;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var PolygonBatcher = (function () {
			function PolygonBatcher(gl, maxVertices) {
				if (maxVertices === void 0) { maxVertices = 10920; }
				this.isDrawing = false;
				this.shader = null;
				this.lastTexture = null;
				this.verticesLength = 0;
				this.indicesLength = 0;
				this.srcBlend = WebGLRenderingContext.SRC_ALPHA;
				this.dstBlend = WebGLRenderingContext.ONE_MINUS_SRC_ALPHA;
				if (maxVertices > 10920)
					throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
				this.gl = gl;
				this.mesh = new webgl.Mesh(gl, [new webgl.Position2Attribute(), new webgl.ColorAttribute(), new webgl.TexCoordAttribute()], maxVertices, maxVertices * 3);
			}
			PolygonBatcher.prototype.begin = function (shader) {
				var gl = this.gl;
				if (this.isDrawing)
					throw new Error("PolygonBatch is already drawing. Call PolygonBatch.end() before calling PolygonBatch.begin()");
				this.drawCalls = 0;
				this.shader = shader;
				this.lastTexture = null;
				this.isDrawing = true;
				gl.enable(gl.BLEND);
				gl.blendFunc(this.srcBlend, this.dstBlend);
			};
			PolygonBatcher.prototype.setBlendMode = function (srcBlend, dstBlend) {
				var gl = this.gl;
				this.srcBlend = srcBlend;
				this.dstBlend = dstBlend;
				if (this.isDrawing) {
					this.flush();
					gl.blendFunc(this.srcBlend, this.dstBlend);
				}
			};
			PolygonBatcher.prototype.draw = function (texture, vertices, indices) {
				if (texture != this.lastTexture) {
					this.flush();
					this.lastTexture = texture;
					texture.bind();
				}
				else if (this.verticesLength + vertices.length > this.mesh.getVertices().length ||
					this.indicesLength + indices.length > this.mesh.getIndices().length) {
					this.flush();
				}
				var indexStart = this.mesh.numVertices();
				this.mesh.getVertices().set(vertices, this.verticesLength);
				this.verticesLength += vertices.length;
				this.mesh.setVerticesLength(this.verticesLength);
				var indicesArray = this.mesh.getIndices();
				for (var i = this.indicesLength, j = 0; j < indices.length; i++, j++)
					indicesArray[i] = indices[j] + indexStart;
				this.indicesLength += indices.length;
				this.mesh.setIndicesLength(this.indicesLength);
			};
			PolygonBatcher.prototype.flush = function () {
				var gl = this.gl;
				if (this.verticesLength == 0)
					return;
				this.mesh.draw(this.shader, gl.TRIANGLES);
				this.verticesLength = 0;
				this.indicesLength = 0;
				this.mesh.setVerticesLength(0);
				this.mesh.setIndicesLength(0);
				this.drawCalls++;
			};
			PolygonBatcher.prototype.end = function () {
				var gl = this.gl;
				if (!this.isDrawing)
					throw new Error("PolygonBatch is not drawing. Call PolygonBatch.begin() before calling PolygonBatch.end()");
				if (this.verticesLength > 0 || this.indicesLength > 0)
					this.flush();
				this.shader = null;
				this.lastTexture = null;
				this.isDrawing = false;
				gl.disable(gl.BLEND);
			};
			PolygonBatcher.prototype.getDrawCalls = function () { return this.drawCalls; };
			PolygonBatcher.prototype.dispose = function () {
				this.mesh.dispose();
			};
			return PolygonBatcher;
		}());
		webgl.PolygonBatcher = PolygonBatcher;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var SceneRenderer = (function () {
			function SceneRenderer(canvas, gl) {
				this.activeRenderer = null;
				this.QUAD = [
					0, 0, 1, 1, 1, 1, 0, 0,
					0, 0, 1, 1, 1, 1, 0, 0,
					0, 0, 1, 1, 1, 1, 0, 0,
					0, 0, 1, 1, 1, 1, 0, 0,
				];
				this.QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
				this.WHITE = new spine.Color(1, 1, 1, 1);
				this.canvas = canvas;
				this.gl = gl;
				this.camera = new webgl.OrthoCamera(canvas.width, canvas.height);
				this.batcherShader = webgl.Shader.newColoredTextured(gl);
				this.batcher = new webgl.PolygonBatcher(gl);
				this.shapesShader = webgl.Shader.newColored(gl);
				this.shapes = new webgl.ShapeRenderer(gl);
				this.skeletonRenderer = new webgl.SkeletonRenderer(gl);
				this.skeletonDebugRenderer = new webgl.SkeletonDebugRenderer(gl);
			}
			SceneRenderer.prototype.begin = function () {
				this.camera.update();
				this.enableRenderer(this.batcher);
			};
			SceneRenderer.prototype.drawSkeleton = function (skeleton, premultipliedAlpha) {
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				this.enableRenderer(this.batcher);
				this.skeletonRenderer.premultipliedAlpha = premultipliedAlpha;
				this.skeletonRenderer.draw(this.batcher, skeleton);
			};
			SceneRenderer.prototype.drawSkeletonDebug = function (skeleton, premultipliedAlpha, ignoredBones) {
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				if (ignoredBones === void 0) { ignoredBones = null; }
				this.enableRenderer(this.shapes);
				this.skeletonDebugRenderer.premultipliedAlpha = premultipliedAlpha;
				this.skeletonDebugRenderer.draw(this.shapes, skeleton, ignoredBones);
			};
			SceneRenderer.prototype.drawTexture = function (texture, x, y, width, height, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = this.WHITE;
				var quad = this.QUAD;
				quad[0] = x;
				quad[1] = y;
				quad[2] = color.r;
				quad[3] = color.g;
				quad[4] = color.b;
				quad[5] = color.a;
				quad[6] = 0;
				quad[7] = 1;
				quad[8] = x + width;
				quad[9] = y;
				quad[10] = color.r;
				quad[11] = color.g;
				quad[12] = color.b;
				quad[13] = color.a;
				quad[14] = 1;
				quad[15] = 1;
				quad[16] = x + width;
				quad[17] = y + height;
				quad[18] = color.r;
				quad[19] = color.g;
				quad[20] = color.b;
				quad[21] = color.a;
				quad[22] = 1;
				quad[23] = 0;
				quad[24] = x;
				quad[25] = y + height;
				quad[26] = color.r;
				quad[27] = color.g;
				quad[28] = color.b;
				quad[29] = color.a;
				quad[30] = 0;
				quad[31] = 0;
				this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawTextureRotated = function (texture, x, y, width, height, pivotX, pivotY, angle, color, premultipliedAlpha) {
				if (color === void 0) { color = null; }
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = this.WHITE;
				var quad = this.QUAD;
				var worldOriginX = x + pivotX;
				var worldOriginY = y + pivotY;
				var fx = -pivotX;
				var fy = -pivotY;
				var fx2 = width - pivotX;
				var fy2 = height - pivotY;
				var p1x = fx;
				var p1y = fy;
				var p2x = fx;
				var p2y = fy2;
				var p3x = fx2;
				var p3y = fy2;
				var p4x = fx2;
				var p4y = fy;
				var x1 = 0;
				var y1 = 0;
				var x2 = 0;
				var y2 = 0;
				var x3 = 0;
				var y3 = 0;
				var x4 = 0;
				var y4 = 0;
				if (angle != 0) {
					var cos = spine.MathUtils.cosDeg(angle);
					var sin = spine.MathUtils.sinDeg(angle);
					x1 = cos * p1x - sin * p1y;
					y1 = sin * p1x + cos * p1y;
					x4 = cos * p2x - sin * p2y;
					y4 = sin * p2x + cos * p2y;
					x3 = cos * p3x - sin * p3y;
					y3 = sin * p3x + cos * p3y;
					x2 = x3 + (x1 - x4);
					y2 = y3 + (y1 - y4);
				}
				else {
					x1 = p1x;
					y1 = p1y;
					x4 = p2x;
					y4 = p2y;
					x3 = p3x;
					y3 = p3y;
					x2 = p4x;
					y2 = p4y;
				}
				x1 += worldOriginX;
				y1 += worldOriginY;
				x2 += worldOriginX;
				y2 += worldOriginY;
				x3 += worldOriginX;
				y3 += worldOriginY;
				x4 += worldOriginX;
				y4 += worldOriginY;
				quad[0] = x1;
				quad[1] = y1;
				quad[2] = color.r;
				quad[3] = color.g;
				quad[4] = color.b;
				quad[5] = color.a;
				quad[6] = 0;
				quad[7] = 1;
				quad[8] = x2;
				quad[9] = y2;
				quad[10] = color.r;
				quad[11] = color.g;
				quad[12] = color.b;
				quad[13] = color.a;
				quad[14] = 1;
				quad[15] = 1;
				quad[16] = x3;
				quad[17] = y3;
				quad[18] = color.r;
				quad[19] = color.g;
				quad[20] = color.b;
				quad[21] = color.a;
				quad[22] = 1;
				quad[23] = 0;
				quad[24] = x4;
				quad[25] = y4;
				quad[26] = color.r;
				quad[27] = color.g;
				quad[28] = color.b;
				quad[29] = color.a;
				quad[30] = 0;
				quad[31] = 0;
				this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawRegion = function (region, x, y, width, height, color, premultipliedAlpha) {
				if (color === void 0) { color = null; }
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = this.WHITE;
				var quad = this.QUAD;
				quad[0] = x;
				quad[1] = y;
				quad[2] = color.r;
				quad[3] = color.g;
				quad[4] = color.b;
				quad[5] = color.a;
				quad[6] = region.u;
				quad[7] = region.v2;
				quad[8] = x + width;
				quad[9] = y;
				quad[10] = color.r;
				quad[11] = color.g;
				quad[12] = color.b;
				quad[13] = color.a;
				quad[14] = region.u2;
				quad[15] = region.v2;
				quad[16] = x + width;
				quad[17] = y + height;
				quad[18] = color.r;
				quad[19] = color.g;
				quad[20] = color.b;
				quad[21] = color.a;
				quad[22] = region.u2;
				quad[23] = region.v;
				quad[24] = x;
				quad[25] = y + height;
				quad[26] = color.r;
				quad[27] = color.g;
				quad[28] = color.b;
				quad[29] = color.a;
				quad[30] = region.u;
				quad[31] = region.v;
				this.batcher.draw(region.texture, quad, this.QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.line = function (x, y, x2, y2, color, color2) {
				if (color === void 0) { color = null; }
				if (color2 === void 0) { color2 = null; }
				this.enableRenderer(this.shapes);
				this.shapes.line(x, y, x2, y2, color);
			};
			SceneRenderer.prototype.triangle = function (filled, x, y, x2, y2, x3, y3, color, color2, color3) {
				if (color === void 0) { color = null; }
				if (color2 === void 0) { color2 = null; }
				if (color3 === void 0) { color3 = null; }
				this.enableRenderer(this.shapes);
				this.shapes.triangle(filled, x, y, x2, y2, x3, y3, color, color2, color3);
			};
			SceneRenderer.prototype.quad = function (filled, x, y, x2, y2, x3, y3, x4, y4, color, color2, color3, color4) {
				if (color === void 0) { color = null; }
				if (color2 === void 0) { color2 = null; }
				if (color3 === void 0) { color3 = null; }
				if (color4 === void 0) { color4 = null; }
				this.enableRenderer(this.shapes);
				this.shapes.quad(filled, x, y, x2, y2, x3, y3, x4, y4, color, color2, color3, color4);
			};
			SceneRenderer.prototype.rect = function (filled, x, y, width, height, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.shapes);
				this.shapes.rect(filled, x, y, width, height, color);
			};
			SceneRenderer.prototype.rectLine = function (filled, x1, y1, x2, y2, width, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.shapes);
				this.shapes.rectLine(filled, x1, y1, x2, y2, width, color);
			};
			SceneRenderer.prototype.polygon = function (polygonVertices, offset, count, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.shapes);
				this.shapes.polygon(polygonVertices, offset, count, color);
			};
			SceneRenderer.prototype.circle = function (filled, x, y, radius, color, segments) {
				if (color === void 0) { color = null; }
				if (segments === void 0) { segments = 0; }
				this.enableRenderer(this.shapes);
				this.shapes.circle(filled, x, y, radius, color, segments);
			};
			SceneRenderer.prototype.curve = function (x1, y1, cx1, cy1, cx2, cy2, x2, y2, segments, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.shapes);
				this.shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, segments, color);
			};
			SceneRenderer.prototype.end = function () {
				if (this.activeRenderer === this.batcher)
					this.batcher.end();
				else if (this.activeRenderer === this.shapes)
					this.shapes.end();
				this.activeRenderer = null;
			};
			SceneRenderer.prototype.resize = function (resizeMode) {
				var canvas = this.canvas;
				var w = canvas.clientWidth;
				var h = canvas.clientHeight;
				if (canvas.width != w || canvas.height != h) {
					canvas.width = w;
					canvas.height = h;
				}
				this.gl.viewport(0, 0, canvas.width, canvas.height);
				if (resizeMode === ResizeMode.Stretch) {
				}
				else if (resizeMode === ResizeMode.Expand) {
					this.camera.setViewport(w, h);
				}
				else if (resizeMode === ResizeMode.Fit) {
					var sourceWidth = canvas.width, sourceHeight = canvas.height;
					var targetWidth = this.camera.viewportWidth, targetHeight = this.camera.viewportHeight;
					var targetRatio = targetHeight / targetWidth;
					var sourceRatio = sourceHeight / sourceWidth;
					var scale = targetRatio < sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
					this.camera.viewportWidth = sourceWidth * scale;
					this.camera.viewportHeight = sourceHeight * scale;
				}
				this.camera.update();
			};
			SceneRenderer.prototype.enableRenderer = function (renderer) {
				if (this.activeRenderer === renderer)
					return;
				this.end();
				if (renderer instanceof webgl.PolygonBatcher) {
					this.batcherShader.bind();
					this.batcherShader.setUniform4x4f(webgl.Shader.MVP_MATRIX, this.camera.projectionView.values);
					this.batcher.begin(this.batcherShader);
					this.activeRenderer = this.batcher;
				}
				else if (renderer instanceof webgl.ShapeRenderer) {
					this.shapesShader.bind();
					this.shapesShader.setUniform4x4f(webgl.Shader.MVP_MATRIX, this.camera.projectionView.values);
					this.shapes.begin(this.shapesShader);
					this.activeRenderer = this.shapes;
				}
				else {
					this.activeRenderer = this.skeletonDebugRenderer;
				}
			};
			SceneRenderer.prototype.dispose = function () {
				this.batcher.dispose();
				this.batcherShader.dispose();
				this.shapes.dispose();
				this.shapesShader.dispose();
				this.skeletonDebugRenderer.dispose();
			};
			return SceneRenderer;
		}());
		webgl.SceneRenderer = SceneRenderer;
		(function (ResizeMode) {
			ResizeMode[ResizeMode["Stretch"] = 0] = "Stretch";
			ResizeMode[ResizeMode["Expand"] = 1] = "Expand";
			ResizeMode[ResizeMode["Fit"] = 2] = "Fit";
		})(webgl.ResizeMode || (webgl.ResizeMode = {}));
		var ResizeMode = webgl.ResizeMode;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var Shader = (function () {
			function Shader(gl, vertexShader, fragmentShader) {
				this.vertexShader = vertexShader;
				this.fragmentShader = fragmentShader;
				this.vs = null;
				this.fs = null;
				this.program = null;
				this.tmp2x2 = new Float32Array(2 * 2);
				this.tmp3x3 = new Float32Array(3 * 3);
				this.tmp4x4 = new Float32Array(4 * 4);
				this.gl = gl;
				this.compile();
			}
			Shader.prototype.getProgram = function () { return this.program; };
			Shader.prototype.getVertexShader = function () { return this.vertexShader; };
			Shader.prototype.getFragmentShader = function () { return this.fragmentShader; };
			Shader.prototype.compile = function () {
				var gl = this.gl;
				try {
					this.vs = this.compileShader(gl.VERTEX_SHADER, this.vertexShader);
					this.fs = this.compileShader(gl.FRAGMENT_SHADER, this.fragmentShader);
					this.program = this.compileProgram(this.vs, this.fs);
				}
				catch (e) {
					this.dispose();
					throw e;
				}
			};
			Shader.prototype.compileShader = function (type, source) {
				var gl = this.gl;
				var shader = gl.createShader(type);
				gl.shaderSource(shader, source);
				gl.compileShader(shader);
				if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
					var error = "Couldn't compile shader: " + gl.getShaderInfoLog(shader);
					gl.deleteShader(shader);
					throw new Error(error);
				}
				return shader;
			};
			Shader.prototype.compileProgram = function (vs, fs) {
				var gl = this.gl;
				var program = gl.createProgram();
				gl.attachShader(program, vs);
				gl.attachShader(program, fs);
				gl.linkProgram(program);
				if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
					var error = "Couldn't compile shader program: " + gl.getProgramInfoLog(program);
					gl.deleteProgram(program);
					throw new Error(error);
				}
				return program;
			};
			Shader.prototype.bind = function () {
				this.gl.useProgram(this.program);
			};
			Shader.prototype.unbind = function () {
				this.gl.useProgram(null);
			};
			Shader.prototype.setUniformi = function (uniform, value) {
				this.gl.uniform1i(this.getUniformLocation(uniform), value);
			};
			Shader.prototype.setUniformf = function (uniform, value) {
				this.gl.uniform1f(this.getUniformLocation(uniform), value);
			};
			Shader.prototype.setUniform2f = function (uniform, value, value2) {
				this.gl.uniform2f(this.getUniformLocation(uniform), value, value2);
			};
			Shader.prototype.setUniform3f = function (uniform, value, value2, value3) {
				this.gl.uniform3f(this.getUniformLocation(uniform), value, value2, value3);
			};
			Shader.prototype.setUniform4f = function (uniform, value, value2, value3, value4) {
				this.gl.uniform4f(this.getUniformLocation(uniform), value, value2, value3, value4);
			};
			Shader.prototype.setUniform2x2f = function (uniform, value) {
				var gl = this.gl;
				this.tmp2x2.set(value);
				gl.uniformMatrix2fv(this.getUniformLocation(uniform), false, this.tmp2x2);
			};
			Shader.prototype.setUniform3x3f = function (uniform, value) {
				var gl = this.gl;
				this.tmp3x3.set(value);
				gl.uniformMatrix3fv(this.getUniformLocation(uniform), false, this.tmp3x3);
			};
			Shader.prototype.setUniform4x4f = function (uniform, value) {
				var gl = this.gl;
				this.tmp4x4.set(value);
				gl.uniformMatrix4fv(this.getUniformLocation(uniform), false, this.tmp4x4);
			};
			Shader.prototype.getUniformLocation = function (uniform) {
				var gl = this.gl;
				var location = gl.getUniformLocation(this.program, uniform);
				if (!location)
					throw new Error("Couldn't find location for uniform " + uniform);
				return location;
			};
			Shader.prototype.getAttributeLocation = function (attribute) {
				var gl = this.gl;
				var location = gl.getAttribLocation(this.program, attribute);
				if (location == -1)
					throw new Error("Couldn't find location for attribute " + attribute);
				return location;
			};
			Shader.prototype.dispose = function () {
				var gl = this.gl;
				if (this.vs) {
					gl.deleteShader(this.vs);
					this.vs = null;
				}
				if (this.fs) {
					gl.deleteShader(this.fs);
					this.fs = null;
				}
				if (this.program) {
					gl.deleteProgram(this.program);
					this.program = null;
				}
			};
			Shader.newColoredTextured = function (gl) {
				var vs = "\n\t\t\t\tattribute vec4 " + Shader.POSITION + ";\n\t\t\t\tattribute vec4 " + Shader.COLOR + ";\n\t\t\t\tattribute vec2 " + Shader.TEXCOORDS + ";\n\t\t\t\tuniform mat4 " + Shader.MVP_MATRIX + ";\n\t\t\t\tvarying vec4 v_color;\n\t\t\t\tvarying vec2 v_texCoords;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tv_color = " + Shader.COLOR + ";\n\t\t\t\t\tv_texCoords = " + Shader.TEXCOORDS + ";\n\t\t\t\t\tgl_Position =	 " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n\t\t\t\t}\n\t\t\t";
				var fs = "\n\t\t\t\t#ifdef GL_ES\n\t\t\t\t\t#define LOWP lowp\n\t\t\t\t\tprecision mediump float;\n\t\t\t\t#else\n\t\t\t\t\t#define LOWP\n\t\t\t\t#endif\n\t\t\t\tvarying LOWP vec4 v_color;\n\t\t\t\tvarying vec2 v_texCoords;\n\t\t\t\tuniform sampler2D u_texture;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tgl_FragColor = v_color * texture2D(u_texture, v_texCoords);\n\t\t\t\t}\n\t\t\t";
				return new Shader(gl, vs, fs);
			};
			Shader.newColored = function (gl) {
				var vs = "\n\t\t\t\tattribute vec4 " + Shader.POSITION + ";\n\t\t\t\tattribute vec4 " + Shader.COLOR + ";\n\t\t\t\tuniform mat4 " + Shader.MVP_MATRIX + ";\n\t\t\t\tvarying vec4 v_color;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tv_color = " + Shader.COLOR + ";\n\t\t\t\t\tgl_Position =	 " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n\t\t\t\t}\n\t\t\t";
				var fs = "\n\t\t\t\t#ifdef GL_ES\n\t\t\t\t\t#define LOWP lowp\n\t\t\t\t\tprecision mediump float;\n\t\t\t\t#else\n\t\t\t\t\t#define LOWP\n\t\t\t\t#endif\n\t\t\t\tvarying LOWP vec4 v_color;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tgl_FragColor = v_color;\n\t\t\t\t}\n\t\t\t";
				return new Shader(gl, vs, fs);
			};
			Shader.MVP_MATRIX = "u_projTrans";
			Shader.POSITION = "a_position";
			Shader.COLOR = "a_color";
			Shader.TEXCOORDS = "a_texCoords";
			Shader.SAMPLER = "u_texture";
			return Shader;
		}());
		webgl.Shader = Shader;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var ShapeRenderer = (function () {
			function ShapeRenderer(gl, maxVertices) {
				if (maxVertices === void 0) { maxVertices = 10920; }
				this.isDrawing = false;
				this.shapeType = ShapeType.Filled;
				this.color = new spine.Color(1, 1, 1, 1);
				this.vertexIndex = 0;
				this.tmp = new spine.Vector2();
				this.srcBlend = WebGLRenderingContext.SRC_ALPHA;
				this.dstBlend = WebGLRenderingContext.ONE_MINUS_SRC_ALPHA;
				if (maxVertices > 10920)
					throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
				this.gl = gl;
				this.mesh = new webgl.Mesh(gl, [new webgl.Position2Attribute(), new webgl.ColorAttribute()], maxVertices, 0);
			}
			ShapeRenderer.prototype.begin = function (shader) {
				if (this.isDrawing)
					throw new Error("ShapeRenderer.begin() has already been called");
				this.shader = shader;
				this.vertexIndex = 0;
				this.isDrawing = true;
				var gl = this.gl;
				gl.enable(gl.BLEND);
				gl.blendFunc(this.srcBlend, this.dstBlend);
			};
			ShapeRenderer.prototype.setBlendMode = function (srcBlend, dstBlend) {
				var gl = this.gl;
				this.srcBlend = srcBlend;
				this.dstBlend = dstBlend;
				if (this.isDrawing) {
					this.flush();
					gl.blendFunc(this.srcBlend, this.dstBlend);
				}
			};
			ShapeRenderer.prototype.setColor = function (color) {
				this.color.setFromColor(color);
			};
			ShapeRenderer.prototype.setColorWith = function (r, g, b, a) {
				this.color.set(r, g, b, a);
			};
			ShapeRenderer.prototype.point = function (x, y, color) {
				if (color === void 0) { color = null; }
				this.check(ShapeType.Point, 1);
				if (color === null)
					color = this.color;
				this.vertex(x, y, color);
			};
			ShapeRenderer.prototype.line = function (x, y, x2, y2, color) {
				if (color === void 0) { color = null; }
				this.check(ShapeType.Line, 2);
				var vertices = this.mesh.getVertices();
				var idx = this.vertexIndex;
				if (color === null)
					color = this.color;
				this.vertex(x, y, color);
				this.vertex(x2, y2, color);
			};
			ShapeRenderer.prototype.triangle = function (filled, x, y, x2, y2, x3, y3, color, color2, color3) {
				if (color === void 0) { color = null; }
				if (color2 === void 0) { color2 = null; }
				if (color3 === void 0) { color3 = null; }
				this.check(filled ? ShapeType.Filled : ShapeType.Line, 3);
				var vertices = this.mesh.getVertices();
				var idx = this.vertexIndex;
				if (color === null)
					color = this.color;
				if (color2 === null)
					color2 = this.color;
				if (color3 === null)
					color3 = this.color;
				if (filled) {
					this.vertex(x, y, color);
					this.vertex(x2, y2, color2);
					this.vertex(x3, y3, color3);
				}
				else {
					this.vertex(x, y, color);
					this.vertex(x2, y2, color2);
					this.vertex(x2, y2, color);
					this.vertex(x3, y3, color2);
					this.vertex(x3, y3, color);
					this.vertex(x, y, color2);
				}
			};
			ShapeRenderer.prototype.quad = function (filled, x, y, x2, y2, x3, y3, x4, y4, color, color2, color3, color4) {
				if (color === void 0) { color = null; }
				if (color2 === void 0) { color2 = null; }
				if (color3 === void 0) { color3 = null; }
				if (color4 === void 0) { color4 = null; }
				this.check(filled ? ShapeType.Filled : ShapeType.Line, 3);
				var vertices = this.mesh.getVertices();
				var idx = this.vertexIndex;
				if (color === null)
					color = this.color;
				if (color2 === null)
					color2 = this.color;
				if (color3 === null)
					color3 = this.color;
				if (color4 === null)
					color4 = this.color;
				if (filled) {
					this.vertex(x, y, color);
					this.vertex(x2, y2, color2);
					this.vertex(x3, y3, color3);
					this.vertex(x3, y3, color3);
					this.vertex(x4, y4, color4);
					this.vertex(x, y, color);
				}
				else {
					this.vertex(x, y, color);
					this.vertex(x2, y2, color2);
					this.vertex(x2, y2, color2);
					this.vertex(x3, y3, color3);
					this.vertex(x3, y3, color3);
					this.vertex(x4, y4, color4);
					this.vertex(x4, y4, color4);
					this.vertex(x, y, color);
				}
			};
			ShapeRenderer.prototype.rect = function (filled, x, y, width, height, color) {
				if (color === void 0) { color = null; }
				this.quad(filled, x, y, x + width, y, x + width, y + height, x, y + height, color, color, color, color);
			};
			ShapeRenderer.prototype.rectLine = function (filled, x1, y1, x2, y2, width, color) {
				if (color === void 0) { color = null; }
				this.check(filled ? ShapeType.Filled : ShapeType.Line, 8);
				if (color === null)
					color = this.color;
				var t = this.tmp.set(y2 - y1, x1 - x2);
				t.normalize();
				width *= 0.5;
				var tx = t.x * width;
				var ty = t.y * width;
				if (!filled) {
					this.vertex(x1 + tx, y1 + ty, color);
					this.vertex(x1 - tx, y1 - ty, color);
					this.vertex(x2 + tx, y2 + ty, color);
					this.vertex(x2 - tx, y2 - ty, color);
					this.vertex(x2 + tx, y2 + ty, color);
					this.vertex(x1 + tx, y1 + ty, color);
					this.vertex(x2 - tx, y2 - ty, color);
					this.vertex(x1 - tx, y1 - ty, color);
				}
				else {
					this.vertex(x1 + tx, y1 + ty, color);
					this.vertex(x1 - tx, y1 - ty, color);
					this.vertex(x2 + tx, y2 + ty, color);
					this.vertex(x2 - tx, y2 - ty, color);
					this.vertex(x2 + tx, y2 + ty, color);
					this.vertex(x1 - tx, y1 - ty, color);
				}
			};
			ShapeRenderer.prototype.x = function (x, y, size) {
				this.line(x - size, y - size, x + size, y + size);
				this.line(x - size, y + size, x + size, y - size);
			};
			ShapeRenderer.prototype.polygon = function (polygonVertices, offset, count, color) {
				if (color === void 0) { color = null; }
				if (count < 3)
					throw new Error("Polygon must contain at least 3 vertices");
				this.check(ShapeType.Line, count * 2);
				if (color === null)
					color = this.color;
				var vertices = this.mesh.getVertices();
				var idx = this.vertexIndex;
				offset <<= 1;
				count <<= 1;
				var firstX = polygonVertices[offset];
				var firstY = polygonVertices[offset + 1];
				var last = offset + count;
				for (var i = offset, n = offset + count - 2; i < n; i += 2) {
					var x1 = polygonVertices[i];
					var y1 = polygonVertices[i + 1];
					var x2 = 0;
					var y2 = 0;
					if (i + 2 >= last) {
						x2 = firstX;
						y2 = firstY;
					}
					else {
						x2 = polygonVertices[i + 2];
						y2 = polygonVertices[i + 3];
					}
					this.vertex(x1, y1, color);
					this.vertex(x2, y2, color);
				}
			};
			ShapeRenderer.prototype.circle = function (filled, x, y, radius, color, segments) {
				if (color === void 0) { color = null; }
				if (segments === void 0) { segments = 0; }
				if (segments === 0)
					segments = Math.max(1, (6 * spine.MathUtils.cbrt(radius)) | 0);
				if (segments <= 0)
					throw new Error("segments must be > 0.");
				if (color === null)
					color = this.color;
				var angle = 2 * spine.MathUtils.PI / segments;
				var cos = Math.cos(angle);
				var sin = Math.sin(angle);
				var cx = radius, cy = 0;
				if (!filled) {
					this.check(ShapeType.Line, segments * 2 + 2);
					for (var i = 0; i < segments; i++) {
						this.vertex(x + cx, y + cy, color);
						var temp_1 = cx;
						cx = cos * cx - sin * cy;
						cy = sin * temp_1 + cos * cy;
						this.vertex(x + cx, y + cy, color);
					}
					this.vertex(x + cx, y + cy, color);
				}
				else {
					this.check(ShapeType.Filled, segments * 3 + 3);
					segments--;
					for (var i = 0; i < segments; i++) {
						this.vertex(x, y, color);
						this.vertex(x + cx, y + cy, color);
						var temp_2 = cx;
						cx = cos * cx - sin * cy;
						cy = sin * temp_2 + cos * cy;
						this.vertex(x + cx, y + cy, color);
					}
					this.vertex(x, y, color);
					this.vertex(x + cx, y + cy, color);
				}
				var temp = cx;
				cx = radius;
				cy = 0;
				this.vertex(x + cx, y + cy, color);
			};
			ShapeRenderer.prototype.curve = function (x1, y1, cx1, cy1, cx2, cy2, x2, y2, segments, color) {
				if (color === void 0) { color = null; }
				this.check(ShapeType.Line, segments * 2 + 2);
				if (color === null)
					color = this.color;
				var subdiv_step = 1 / segments;
				var subdiv_step2 = subdiv_step * subdiv_step;
				var subdiv_step3 = subdiv_step * subdiv_step * subdiv_step;
				var pre1 = 3 * subdiv_step;
				var pre2 = 3 * subdiv_step2;
				var pre4 = 6 * subdiv_step2;
				var pre5 = 6 * subdiv_step3;
				var tmp1x = x1 - cx1 * 2 + cx2;
				var tmp1y = y1 - cy1 * 2 + cy2;
				var tmp2x = (cx1 - cx2) * 3 - x1 + x2;
				var tmp2y = (cy1 - cy2) * 3 - y1 + y2;
				var fx = x1;
				var fy = y1;
				var dfx = (cx1 - x1) * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
				var dfy = (cy1 - y1) * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
				var ddfx = tmp1x * pre4 + tmp2x * pre5;
				var ddfy = tmp1y * pre4 + tmp2y * pre5;
				var dddfx = tmp2x * pre5;
				var dddfy = tmp2y * pre5;
				while (segments-- > 0) {
					this.vertex(fx, fy, color);
					fx += dfx;
					fy += dfy;
					dfx += ddfx;
					dfy += ddfy;
					ddfx += dddfx;
					ddfy += dddfy;
					this.vertex(fx, fy, color);
				}
				this.vertex(fx, fy, color);
				this.vertex(x2, y2, color);
			};
			ShapeRenderer.prototype.vertex = function (x, y, color) {
				var idx = this.vertexIndex;
				var vertices = this.mesh.getVertices();
				vertices[idx++] = x;
				vertices[idx++] = y;
				vertices[idx++] = color.r;
				vertices[idx++] = color.g;
				vertices[idx++] = color.b;
				vertices[idx++] = color.a;
				this.vertexIndex = idx;
			};
			ShapeRenderer.prototype.end = function () {
				if (!this.isDrawing)
					throw new Error("ShapeRenderer.begin() has not been called");
				this.flush();
				this.gl.disable(this.gl.BLEND);
				this.isDrawing = false;
			};
			ShapeRenderer.prototype.flush = function () {
				if (this.vertexIndex == 0)
					return;
				this.mesh.setVerticesLength(this.vertexIndex);
				this.mesh.draw(this.shader, this.shapeType);
				this.vertexIndex = 0;
			};
			ShapeRenderer.prototype.check = function (shapeType, numVertices) {
				if (!this.isDrawing)
					throw new Error("ShapeRenderer.begin() has not been called");
				if (this.shapeType == shapeType) {
					if (this.mesh.maxVertices() - this.mesh.numVertices() < numVertices)
						this.flush();
					else
						return;
				}
				else {
					this.flush();
					this.shapeType = shapeType;
				}
			};
			ShapeRenderer.prototype.dispose = function () {
				this.mesh.dispose();
			};
			return ShapeRenderer;
		}());
		webgl.ShapeRenderer = ShapeRenderer;
		(function (ShapeType) {
			ShapeType[ShapeType["Point"] = WebGLRenderingContext.POINTS] = "Point";
			ShapeType[ShapeType["Line"] = WebGLRenderingContext.LINES] = "Line";
			ShapeType[ShapeType["Filled"] = WebGLRenderingContext.TRIANGLES] = "Filled";
		})(webgl.ShapeType || (webgl.ShapeType = {}));
		var ShapeType = webgl.ShapeType;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var SkeletonDebugRenderer = (function () {
			function SkeletonDebugRenderer(gl) {
				this.boneLineColor = new spine.Color(1, 0, 0, 1);
				this.boneOriginColor = new spine.Color(0, 1, 0, 1);
				this.attachmentLineColor = new spine.Color(0, 0, 1, 0.5);
				this.triangleLineColor = new spine.Color(1, 0.64, 0, 0.5);
				this.pathColor = new spine.Color().setFromString("FF7F00");
				this.aabbColor = new spine.Color(0, 1, 0, 0.5);
				this.drawBones = true;
				this.drawRegionAttachments = true;
				this.drawBoundingBoxes = true;
				this.drawMeshHull = true;
				this.drawMeshTriangles = true;
				this.drawPaths = true;
				this.drawSkeletonXY = false;
				this.premultipliedAlpha = false;
				this.scale = 1;
				this.boneWidth = 2;
				this.bounds = new spine.SkeletonBounds();
				this.temp = new Array();
				this.gl = gl;
			}
			SkeletonDebugRenderer.prototype.draw = function (shapes, skeleton, ignoredBones) {
				if (ignoredBones === void 0) { ignoredBones = null; }
				var skeletonX = skeleton.x;
				var skeletonY = skeleton.y;
				var gl = this.gl;
				var srcFunc = this.premultipliedAlpha ? gl.ONE : gl.SRC_ALPHA;
				shapes.setBlendMode(srcFunc, gl.ONE_MINUS_SRC_ALPHA);
				var bones = skeleton.bones;
				if (this.drawBones) {
					shapes.setColor(this.boneLineColor);
					for (var i = 0, n = bones.length; i < n; i++) {
						var bone = bones[i];
						if (ignoredBones && ignoredBones.indexOf(bone.data.name) > -1)
							continue;
						if (bone.parent == null)
							continue;
						var x = skeletonX + bone.data.length * bone.a + bone.worldX;
						var y = skeletonY + bone.data.length * bone.c + bone.worldY;
						shapes.rectLine(true, skeletonX + bone.worldX, skeletonY + bone.worldY, x, y, this.boneWidth * this.scale);
					}
					if (this.drawSkeletonXY)
						shapes.x(skeletonX, skeletonY, 4 * this.scale);
				}
				if (this.drawRegionAttachments) {
					shapes.setColor(this.attachmentLineColor);
					var slots = skeleton.slots;
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						var attachment = slot.getAttachment();
						if (attachment instanceof spine.RegionAttachment) {
							var regionAttachment = attachment;
							var vertices = regionAttachment.updateWorldVertices(slot, false);
							shapes.line(vertices[spine.RegionAttachment.X1], vertices[spine.RegionAttachment.Y1], vertices[spine.RegionAttachment.X2], vertices[spine.RegionAttachment.Y2]);
							shapes.line(vertices[spine.RegionAttachment.X2], vertices[spine.RegionAttachment.Y2], vertices[spine.RegionAttachment.X3], vertices[spine.RegionAttachment.Y3]);
							shapes.line(vertices[spine.RegionAttachment.X3], vertices[spine.RegionAttachment.Y3], vertices[spine.RegionAttachment.X4], vertices[spine.RegionAttachment.Y4]);
							shapes.line(vertices[spine.RegionAttachment.X4], vertices[spine.RegionAttachment.Y4], vertices[spine.RegionAttachment.X1], vertices[spine.RegionAttachment.Y1]);
						}
					}
				}
				if (this.drawMeshHull || this.drawMeshTriangles) {
					var slots = skeleton.slots;
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						var attachment = slot.getAttachment();
						if (!(attachment instanceof spine.MeshAttachment))
							continue;
						var mesh = attachment;
						mesh.updateWorldVertices(slot, false);
						var vertices = mesh.worldVertices;
						var triangles = mesh.triangles;
						var hullLength = mesh.hullLength;
						if (this.drawMeshTriangles) {
							shapes.setColor(this.triangleLineColor);
							for (var ii = 0, nn = triangles.length; ii < nn; ii += 3) {
								var v1 = triangles[ii] * 8, v2 = triangles[ii + 1] * 8, v3 = triangles[ii + 2] * 8;
								shapes.triangle(false, vertices[v1], vertices[v1 + 1], vertices[v2], vertices[v2 + 1], vertices[v3], vertices[v3 + 1]);
							}
						}
						if (this.drawMeshHull && hullLength > 0) {
							shapes.setColor(this.attachmentLineColor);
							hullLength = (hullLength >> 1) * 8;
							var lastX = vertices[hullLength - 8], lastY = vertices[hullLength - 7];
							for (var ii = 0, nn = hullLength; ii < nn; ii += 8) {
								var x = vertices[ii], y = vertices[ii + 1];
								shapes.line(x, y, lastX, lastY);
								lastX = x;
								lastY = y;
							}
						}
					}
				}
				if (this.drawBoundingBoxes) {
					var bounds = this.bounds;
					bounds.update(skeleton, true);
					shapes.setColor(this.aabbColor);
					shapes.rect(false, bounds.minX, bounds.minY, bounds.getWidth(), bounds.getHeight());
					var polygons = bounds.polygons;
					var boxes = bounds.boundingBoxes;
					for (var i = 0, n = polygons.length; i < n; i++) {
						var polygon = polygons[i];
						shapes.setColor(boxes[i].color);
						shapes.polygon(polygon, 0, polygon.length);
					}
				}
				if (this.drawPaths) {
					var slots = skeleton.slots;
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						var attachment = slot.getAttachment();
						if (!(attachment instanceof spine.PathAttachment))
							continue;
						var path = attachment;
						var nn = path.worldVerticesLength;
						var world = this.temp = spine.Utils.setArraySize(this.temp, nn, 0);
						path.computeWorldVertices(slot, world);
						var color = this.pathColor;
						var x1 = world[2], y1 = world[3], x2 = 0, y2 = 0;
						if (path.closed) {
							shapes.setColor(color);
							var cx1 = world[0], cy1 = world[1], cx2 = world[nn - 2], cy2 = world[nn - 1];
							x2 = world[nn - 4];
							y2 = world[nn - 3];
							shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
							shapes.setColor(SkeletonDebugRenderer.LIGHT_GRAY);
							shapes.line(x1, y1, cx1, cy1);
							shapes.line(x2, y2, cx2, cy2);
						}
						nn -= 4;
						for (var ii = 4; ii < nn; ii += 6) {
							var cx1 = world[ii], cy1 = world[ii + 1], cx2 = world[ii + 2], cy2 = world[ii + 3];
							x2 = world[ii + 4];
							y2 = world[ii + 5];
							shapes.setColor(color);
							shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
							shapes.setColor(SkeletonDebugRenderer.LIGHT_GRAY);
							shapes.line(x1, y1, cx1, cy1);
							shapes.line(x2, y2, cx2, cy2);
							x1 = x2;
							y1 = y2;
						}
					}
				}
				if (this.drawBones) {
					shapes.setColor(this.boneOriginColor);
					for (var i = 0, n = bones.length; i < n; i++) {
						var bone = bones[i];
						if (ignoredBones && ignoredBones.indexOf(bone.data.name) > -1)
							continue;
						shapes.circle(true, skeletonX + bone.worldX, skeletonY + bone.worldY, 3 * this.scale, SkeletonDebugRenderer.GREEN, 8);
					}
				}
			};
			SkeletonDebugRenderer.prototype.dispose = function () {
			};
			SkeletonDebugRenderer.LIGHT_GRAY = new spine.Color(192 / 255, 192 / 255, 192 / 255, 1);
			SkeletonDebugRenderer.GREEN = new spine.Color(0, 1, 0, 1);
			return SkeletonDebugRenderer;
		}());
		webgl.SkeletonDebugRenderer = SkeletonDebugRenderer;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var SkeletonRenderer = (function () {
			function SkeletonRenderer(gl) {
				this.premultipliedAlpha = false;
				this.gl = gl;
			}
			SkeletonRenderer.prototype.draw = function (batcher, skeleton) {
				var premultipliedAlpha = this.premultipliedAlpha;
				var blendMode = null;
				var vertices = null;
				var triangles = null;
				var drawOrder = skeleton.drawOrder;
				for (var i = 0, n = drawOrder.length; i < n; i++) {
					var slot = drawOrder[i];
					var attachment = slot.getAttachment();
					var texture = null;
					if (attachment instanceof spine.RegionAttachment) {
						var region = attachment;
						vertices = region.updateWorldVertices(slot, premultipliedAlpha);
						triangles = SkeletonRenderer.QUAD_TRIANGLES;
						texture = region.region.renderObject.texture;
					}
					else if (attachment instanceof spine.MeshAttachment) {
						var mesh = attachment;
						vertices = mesh.updateWorldVertices(slot, premultipliedAlpha);
						triangles = mesh.triangles;
						texture = mesh.region.renderObject.texture;
					}
					else
						continue;
					if (texture != null) {
						var slotBlendMode = slot.data.blendMode;
						if (slotBlendMode != blendMode) {
							blendMode = slotBlendMode;
							batcher.setBlendMode(webgl.getSourceGLBlendMode(this.gl, blendMode, premultipliedAlpha), webgl.getDestGLBlendMode(this.gl, blendMode));
						}
						batcher.draw(texture, vertices, triangles);
					}
				}
			};
			SkeletonRenderer.QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
			return SkeletonRenderer;
		}());
		webgl.SkeletonRenderer = SkeletonRenderer;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var Vector3 = (function () {
			function Vector3(x, y, z) {
				if (x === void 0) { x = 0; }
				if (y === void 0) { y = 0; }
				if (z === void 0) { z = 0; }
				this.x = 0;
				this.y = 0;
				this.z = 0;
				this.x = x;
				this.y = y;
				this.z = z;
			}
			Vector3.prototype.setFrom = function (v) {
				this.x = v.x;
				this.y = v.y;
				this.z = v.z;
				return this;
			};
			Vector3.prototype.set = function (x, y, z) {
				this.x = x;
				this.y = y;
				this.z = z;
				return this;
			};
			Vector3.prototype.add = function (v) {
				this.x += v.x;
				this.y += v.y;
				this.z += v.z;
				return this;
			};
			Vector3.prototype.sub = function (v) {
				this.x -= v.x;
				this.y -= v.y;
				this.z -= v.z;
				return this;
			};
			Vector3.prototype.scale = function (s) {
				this.x *= s;
				this.y *= s;
				this.z *= s;
				return this;
			};
			Vector3.prototype.normalize = function () {
				var len = this.length();
				if (len == 0)
					return this;
				len = 1 / len;
				this.x *= len;
				this.y *= len;
				this.z *= len;
				return this;
			};
			Vector3.prototype.cross = function (v) {
				return this.set(this.y * v.z - this.z * v.y, this.z * v.x - this.x * v.z, this.x * v.y - this.y * v.x);
			};
			Vector3.prototype.multiply = function (matrix) {
				var l_mat = matrix.values;
				return this.set(this.x * l_mat[webgl.M00] + this.y * l_mat[webgl.M01] + this.z * l_mat[webgl.M02] + l_mat[webgl.M03], this.x * l_mat[webgl.M10] + this.y * l_mat[webgl.M11] + this.z * l_mat[webgl.M12] + l_mat[webgl.M13], this.x * l_mat[webgl.M20] + this.y * l_mat[webgl.M21] + this.z * l_mat[webgl.M22] + l_mat[webgl.M23]);
			};
			Vector3.prototype.project = function (matrix) {
				var l_mat = matrix.values;
				var l_w = 1 / (this.x * l_mat[webgl.M30] + this.y * l_mat[webgl.M31] + this.z * l_mat[webgl.M32] + l_mat[webgl.M33]);
				return this.set((this.x * l_mat[webgl.M00] + this.y * l_mat[webgl.M01] + this.z * l_mat[webgl.M02] + l_mat[webgl.M03]) * l_w, (this.x * l_mat[webgl.M10] + this.y * l_mat[webgl.M11] + this.z * l_mat[webgl.M12] + l_mat[webgl.M13]) * l_w, (this.x * l_mat[webgl.M20] + this.y * l_mat[webgl.M21] + this.z * l_mat[webgl.M22] + l_mat[webgl.M23]) * l_w);
			};
			Vector3.prototype.dot = function (v) {
				return this.x * v.x + this.y * v.y + this.z * v.z;
			};
			Vector3.prototype.length = function () {
				return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
			};
			Vector3.prototype.distance = function (v) {
				var a = v.x - this.x;
				var b = v.y - this.y;
				var c = v.z - this.z;
				return Math.sqrt(a * a + b * b + c * c);
			};
			return Vector3;
		}());
		webgl.Vector3 = Vector3;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		function getSourceGLBlendMode(gl, blendMode, premultipliedAlpha) {
			if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
			switch (blendMode) {
				case spine.BlendMode.Normal: return premultipliedAlpha ? gl.ONE : gl.SRC_ALPHA;
				case spine.BlendMode.Additive: return premultipliedAlpha ? gl.ONE : gl.SRC_ALPHA;
				case spine.BlendMode.Multiply: return gl.DST_COLOR;
				case spine.BlendMode.Screen: return gl.ONE;
				default: throw new Error("Unknown blend mode: " + blendMode);
			}
		}
		webgl.getSourceGLBlendMode = getSourceGLBlendMode;
		function getDestGLBlendMode(gl, blendMode) {
			switch (blendMode) {
				case spine.BlendMode.Normal: return gl.ONE_MINUS_SRC_ALPHA;
				case spine.BlendMode.Additive: return gl.ONE;
				case spine.BlendMode.Multiply: return gl.ONE_MINUS_SRC_ALPHA;
				case spine.BlendMode.Screen: return gl.ONE_MINUS_SRC_ALPHA;
				default: throw new Error("Unknown blend mode: " + blendMode);
			}
		}
		webgl.getDestGLBlendMode = getDestGLBlendMode;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SpineWidget = (function () {
		function SpineWidget(element, config) {
			var _this = this;
			this.mvp = new spine.webgl.Matrix4();
			this.paused = false;
			this.lastFrameTime = Date.now() / 1000.0;
			this.backgroundColor = new spine.Color();
			this.loaded = false;
			this.bounds = { offset: new spine.Vector2(), size: new spine.Vector2() };
			if (!element)
				throw new Error("Please provide a DOM element, e.g. document.getElementById('myelement')");
			if (!config)
				throw new Error("Please provide a configuration, specifying at least the json file, atlas file and animation name");
			var elementId = element;
			if (typeof (element) === "string")
				element = document.getElementById(element);
			if (element == null)
				throw new Error("Element " + elementId + " does not exist");
			this.validateConfig(config);
			var canvas = this.canvas = document.createElement("canvas");
			canvas.style.width = "100%";
			canvas.style.height = "100%";
			element.appendChild(canvas);
			canvas.width = element.clientWidth;
			canvas.height = element.clientHeight;
			var webglConfig = { alpha: false };
			var gl = this.gl = (canvas.getContext("webgl", webglConfig) || canvas.getContext("experimental-webgl", webglConfig));
			this.shader = spine.webgl.Shader.newColoredTextured(gl);
			this.batcher = new spine.webgl.PolygonBatcher(gl);
			this.mvp.ortho2d(0, 0, canvas.width - 1, canvas.height - 1);
			this.skeletonRenderer = new spine.webgl.SkeletonRenderer(gl);
			this.debugShader = spine.webgl.Shader.newColored(gl);
			this.debugRenderer = new spine.webgl.SkeletonDebugRenderer(gl);
			this.shapes = new spine.webgl.ShapeRenderer(gl);
			var assets = this.assetManager = new spine.webgl.AssetManager(gl);
			assets.loadText(config.atlas);
			assets.loadText(config.json);
			assets.loadTexture(config.atlas.replace(".atlas", ".png"));
			requestAnimationFrame(function () { _this.load(); });
		}
		SpineWidget.prototype.validateConfig = function (config) {
			if (!config.atlas)
				throw new Error("Please specify config.atlas");
			if (!config.json)
				throw new Error("Please specify config.json");
			if (!config.animation)
				throw new Error("Please specify config.animationName");
			if (!config.scale)
				config.scale = 1.0;
			if (!config.skin)
				config.skin = "default";
			if (config.loop === undefined)
				config.loop = true;
			if (!config.x)
				config.x = 0;
			if (!config.y)
				config.y = 0;
			if (config.fitToCanvas === undefined)
				config.fitToCanvas = true;
			if (!config.backgroundColor)
				config.backgroundColor = "#555555";
			if (!config.imagesPath) {
				var index = config.atlas.lastIndexOf("/");
				if (index != -1) {
					config.imagesPath = config.atlas.substr(0, index) + "/";
				}
				else {
					config.imagesPath = "";
				}
			}
			if (!config.premultipliedAlpha === undefined)
				config.premultipliedAlpha = false;
			if (!config.debug === undefined)
				config.debug = false;
			this.backgroundColor.setFromString(config.backgroundColor);
			this.config = config;
		};
		SpineWidget.prototype.load = function () {
			var _this = this;
			var assetManager = this.assetManager;
			var imagesPath = this.config.imagesPath;
			var config = this.config;
			if (assetManager.isLoadingComplete()) {
				if (assetManager.hasErrors()) {
					if (config.error)
						config.error(this, "Failed to load assets: " + JSON.stringify(assetManager.getErrors()));
					else
						throw new Error("Failed to load assets: " + JSON.stringify(assetManager.getErrors()));
				}
				var atlas = new spine.TextureAtlas(this.assetManager.get(this.config.atlas), function (path) {
					var texture = assetManager.get(imagesPath + path);
					return texture;
				});
				var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
				var skeletonJson = new spine.SkeletonJson(atlasLoader);
				skeletonJson.scale = config.scale;
				var skeletonData = skeletonJson.readSkeletonData(assetManager.get(config.json));
				var skeleton = this.skeleton = new spine.Skeleton(skeletonData);
				var bounds = this.bounds;
				skeleton.setSkinByName(config.skin);
				skeleton.setToSetupPose();
				skeleton.updateWorldTransform();
				skeleton.getBounds(bounds.offset, bounds.size);
				if (!config.fitToCanvas) {
					skeleton.x = config.x;
					skeleton.y = config.y;
				}
				var animationState = this.state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
				animationState.setAnimation(0, config.animation, true);
				if (config.success)
					config.success(this);
				this.loaded = true;
				requestAnimationFrame(function () { _this.render(); });
			}
			else
				requestAnimationFrame(function () { _this.load(); });
		};
		SpineWidget.prototype.render = function () {
			var _this = this;
			var now = Date.now() / 1000;
			var delta = now - this.lastFrameTime;
			if (delta > 0.1)
				delta = 0;
			this.lastFrameTime = now;
			var gl = this.gl;
			var color = this.backgroundColor;
			this.resize();
			gl.clearColor(color.r, color.g, color.b, color.a);
			gl.clear(gl.COLOR_BUFFER_BIT);
			var state = this.state;
			var skeleton = this.skeleton;
			var premultipliedAlpha = this.config.premultipliedAlpha;
			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
			var shader = this.shader;
			var batcher = this.batcher;
			var skeletonRenderer = this.skeletonRenderer;
			shader.bind();
			shader.setUniformi(spine.webgl.Shader.SAMPLER, 0);
			shader.setUniform4x4f(spine.webgl.Shader.MVP_MATRIX, this.mvp.values);
			batcher.begin(shader);
			skeletonRenderer.premultipliedAlpha = premultipliedAlpha;
			skeletonRenderer.draw(batcher, skeleton);
			batcher.end();
			shader.unbind();
			if (this.config.debug) {
				var shader_1 = this.debugShader;
				var shapes = this.shapes;
				var renderer = this.debugRenderer;
				shader_1.bind();
				shader_1.setUniform4x4f(spine.webgl.Shader.MVP_MATRIX, this.mvp.values);
				renderer.premultipliedAlpha = premultipliedAlpha;
				shapes.begin(shader_1);
				renderer.draw(shapes, skeleton);
				shapes.end();
				shader_1.unbind();
			}
			if (!this.paused)
				requestAnimationFrame(function () { _this.render(); });
		};
		SpineWidget.prototype.resize = function () {
			var canvas = this.canvas;
			var w = canvas.clientWidth;
			var h = canvas.clientHeight;
			var bounds = this.bounds;
			if (canvas.width != w || canvas.height != h) {
				canvas.width = w;
				canvas.height = h;
			}
			if (this.config.fitToCanvas) {
				var centerX = bounds.offset.x + bounds.size.x / 2;
				var centerY = bounds.offset.y + bounds.size.y / 2;
				var scaleX = bounds.size.x / canvas.width;
				var scaleY = bounds.size.y / canvas.height;
				var scale = Math.max(scaleX, scaleY) * 1.2;
				if (scale < 1)
					scale = 1;
				var width = canvas.width * scale;
				var height = canvas.height * scale;
				this.skeleton.x = this.skeleton.y = 0;
				this.mvp.ortho2d(centerX - width / 2, centerY - height / 2, width, height);
			}
			else {
				this.mvp.ortho2d(0, 0, canvas.width - 1, canvas.height - 1);
			}
			this.gl.viewport(0, 0, canvas.width, canvas.height);
		};
		SpineWidget.prototype.pause = function () {
			this.paused = true;
		};
		SpineWidget.prototype.play = function () {
			var _this = this;
			this.paused = false;
			requestAnimationFrame(function () { _this.render(); });
		};
		SpineWidget.prototype.isPlaying = function () {
			return !this.paused;
		};
		SpineWidget.prototype.setAnimation = function (animationName) {
			if (!this.loaded)
				throw new Error("Widget isn't loaded yet");
			this.skeleton.setToSetupPose();
			this.state.setAnimation(0, animationName, this.config.loop);
		};
		SpineWidget.loadWidgets = function () {
			var widgets = document.getElementsByClassName("spine-widget");
			for (var i = 0; i < widgets.length; i++) {
				SpineWidget.loadWidget(widgets[i]);
			}
		};
		SpineWidget.loadWidget = function (widget) {
			var config = new SpineWidgetConfig();
			config.atlas = widget.getAttribute("data-atlas");
			config.json = widget.getAttribute("data-json");
			config.animation = widget.getAttribute("data-animation");
			if (widget.getAttribute("data-images-path"))
				config.imagesPath = widget.getAttribute("data-images-path");
			if (widget.getAttribute("data-skin"))
				config.skin = widget.getAttribute("data-skin");
			if (widget.getAttribute("data-loop"))
				config.loop = widget.getAttribute("data-loop") === "true";
			if (widget.getAttribute("data-scale"))
				config.scale = parseFloat(widget.getAttribute("data-scale"));
			if (widget.getAttribute("data-x"))
				config.x = parseFloat(widget.getAttribute("data-x"));
			if (widget.getAttribute("data-y"))
				config.y = parseFloat(widget.getAttribute("data-y"));
			if (widget.getAttribute("data-fit-to-canvas"))
				config.fitToCanvas = widget.getAttribute("data-fit-to-canvas") === "true";
			if (widget.getAttribute("data-background-color"))
				config.backgroundColor = widget.getAttribute("data-background-color");
			if (widget.getAttribute("data-premultiplied-alpha"))
				config.premultipliedAlpha = widget.getAttribute("data-premultiplied-alpha") === "true";
			if (widget.getAttribute("data-debug"))
				config.debug = widget.getAttribute("data-debug") === "true";
			new spine.SpineWidget(widget, config);
		};
		SpineWidget.ready = function () {
			if (SpineWidget.pageLoaded)
				return;
			SpineWidget.pageLoaded = true;
			SpineWidget.loadWidgets();
		};
		SpineWidget.setupDOMListener = function () {
			if (document.addEventListener) {
				document.addEventListener("DOMContentLoaded", SpineWidget.ready, false);
				window.addEventListener("load", SpineWidget.ready, false);
			}
			else {
				document.attachEvent("onreadystatechange", function readyStateChange() {
					if (document.readyState === "complete")
						SpineWidget.ready();
				});
				window.attachEvent("onload", SpineWidget.ready);
			}
		};
		SpineWidget.pageLoaded = false;
		return SpineWidget;
	}());
	spine.SpineWidget = SpineWidget;
	var SpineWidgetConfig = (function () {
		function SpineWidgetConfig() {
			this.skin = "default";
			this.loop = true;
			this.scale = 1.0;
			this.x = 0;
			this.y = 0;
			this.fitToCanvas = true;
			this.backgroundColor = "#555555";
			this.premultipliedAlpha = false;
			this.debug = false;
		}
		return SpineWidgetConfig;
	}());
	spine.SpineWidgetConfig = SpineWidgetConfig;
})(spine || (spine = {}));
spine.SpineWidget.setupDOMListener();
//# sourceMappingURL=spine-widget.js.map