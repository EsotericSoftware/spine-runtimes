var __extends = (this && this.__extends) || (function () {
	var extendStatics = function (d, b) {
		extendStatics = Object.setPrototypeOf ||
			({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
			function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
		return extendStatics(d, b);
	};
	return function (d, b) {
		extendStatics(d, b);
		function __() { this.constructor = d; }
		d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
	};
})();
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
			this.timelineIds = [];
			for (var i = 0; i < timelines.length; i++)
				this.timelineIds[timelines[i].getPropertyId()] = true;
			this.duration = duration;
		}
		Animation.prototype.hasTimeline = function (id) {
			return this.timelineIds[id] == true;
		};
		Animation.prototype.apply = function (skeleton, lastTime, time, loop, events, alpha, blend, direction) {
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0)
					lastTime %= this.duration;
			}
			var timelines = this.timelines;
			for (var i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, alpha, blend, direction);
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
	var MixBlend;
	(function (MixBlend) {
		MixBlend[MixBlend["setup"] = 0] = "setup";
		MixBlend[MixBlend["first"] = 1] = "first";
		MixBlend[MixBlend["replace"] = 2] = "replace";
		MixBlend[MixBlend["add"] = 3] = "add";
	})(MixBlend = spine.MixBlend || (spine.MixBlend = {}));
	var MixDirection;
	(function (MixDirection) {
		MixDirection[MixDirection["mixIn"] = 0] = "mixIn";
		MixDirection[MixDirection["mixOut"] = 1] = "mixOut";
	})(MixDirection = spine.MixDirection || (spine.MixDirection = {}));
	var TimelineType;
	(function (TimelineType) {
		TimelineType[TimelineType["rotate"] = 0] = "rotate";
		TimelineType[TimelineType["translate"] = 1] = "translate";
		TimelineType[TimelineType["scale"] = 2] = "scale";
		TimelineType[TimelineType["shear"] = 3] = "shear";
		TimelineType[TimelineType["attachment"] = 4] = "attachment";
		TimelineType[TimelineType["color"] = 5] = "color";
		TimelineType[TimelineType["deform"] = 6] = "deform";
		TimelineType[TimelineType["event"] = 7] = "event";
		TimelineType[TimelineType["drawOrder"] = 8] = "drawOrder";
		TimelineType[TimelineType["ikConstraint"] = 9] = "ikConstraint";
		TimelineType[TimelineType["transformConstraint"] = 10] = "transformConstraint";
		TimelineType[TimelineType["pathConstraintPosition"] = 11] = "pathConstraintPosition";
		TimelineType[TimelineType["pathConstraintSpacing"] = 12] = "pathConstraintSpacing";
		TimelineType[TimelineType["pathConstraintMix"] = 13] = "pathConstraintMix";
		TimelineType[TimelineType["twoColor"] = 14] = "twoColor";
	})(TimelineType = spine.TimelineType || (spine.TimelineType = {}));
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
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount << 1);
			return _this;
		}
		RotateTimeline.prototype.getPropertyId = function () {
			return (TimelineType.rotate << 24) + this.boneIndex;
		};
		RotateTimeline.prototype.setFrame = function (frameIndex, time, degrees) {
			frameIndex <<= 1;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + RotateTimeline.ROTATION] = degrees;
		};
		RotateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var frames = this.frames;
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.rotation = bone.data.rotation;
						return;
					case MixBlend.first:
						var r_1 = bone.data.rotation - bone.rotation;
						bone.rotation += (r_1 - (16384 - ((16384.499999999996 - r_1 / 360) | 0)) * 360) * alpha;
				}
				return;
			}
			if (time >= frames[frames.length - RotateTimeline.ENTRIES]) {
				var r_2 = frames[frames.length + RotateTimeline.PREV_ROTATION];
				switch (blend) {
					case MixBlend.setup:
						bone.rotation = bone.data.rotation + r_2 * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						r_2 += bone.data.rotation - bone.rotation;
						r_2 -= (16384 - ((16384.499999999996 - r_2 / 360) | 0)) * 360;
					case MixBlend.add:
						bone.rotation += r_2 * alpha;
				}
				return;
			}
			var frame = Animation.binarySearch(frames, time, RotateTimeline.ENTRIES);
			var prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));
			var r = frames[frame + RotateTimeline.ROTATION] - prevRotation;
			r = prevRotation + (r - (16384 - ((16384.499999999996 - r / 360) | 0)) * 360) * percent;
			switch (blend) {
				case MixBlend.setup:
					bone.rotation = bone.data.rotation + (r - (16384 - ((16384.499999999996 - r / 360) | 0)) * 360) * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					r += bone.data.rotation - bone.rotation;
				case MixBlend.add:
					bone.rotation += (r - (16384 - ((16384.499999999996 - r / 360) | 0)) * 360) * alpha;
			}
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
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * TranslateTimeline.ENTRIES);
			return _this;
		}
		TranslateTimeline.prototype.getPropertyId = function () {
			return (TimelineType.translate << 24) + this.boneIndex;
		};
		TranslateTimeline.prototype.setFrame = function (frameIndex, time, x, y) {
			frameIndex *= TranslateTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TranslateTimeline.X] = x;
			this.frames[frameIndex + TranslateTimeline.Y] = y;
		};
		TranslateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var frames = this.frames;
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.x = bone.data.x;
						bone.y = bone.data.y;
						return;
					case MixBlend.first:
						bone.x += (bone.data.x - bone.x) * alpha;
						bone.y += (bone.data.y - bone.y) * alpha;
				}
				return;
			}
			var x = 0, y = 0;
			if (time >= frames[frames.length - TranslateTimeline.ENTRIES]) {
				x = frames[frames.length + TranslateTimeline.PREV_X];
				y = frames[frames.length + TranslateTimeline.PREV_Y];
			}
			else {
				var frame = Animation.binarySearch(frames, time, TranslateTimeline.ENTRIES);
				x = frames[frame + TranslateTimeline.PREV_X];
				y = frames[frame + TranslateTimeline.PREV_Y];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / TranslateTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + TranslateTimeline.PREV_TIME] - frameTime));
				x += (frames[frame + TranslateTimeline.X] - x) * percent;
				y += (frames[frame + TranslateTimeline.Y] - y) * percent;
			}
			switch (blend) {
				case MixBlend.setup:
					bone.x = bone.data.x + x * alpha;
					bone.y = bone.data.y + y * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					bone.x += (bone.data.x + x - bone.x) * alpha;
					bone.y += (bone.data.y + y - bone.y) * alpha;
					break;
				case MixBlend.add:
					bone.x += x * alpha;
					bone.y += y * alpha;
			}
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
			return _super.call(this, frameCount) || this;
		}
		ScaleTimeline.prototype.getPropertyId = function () {
			return (TimelineType.scale << 24) + this.boneIndex;
		};
		ScaleTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var frames = this.frames;
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.scaleX = bone.data.scaleX;
						bone.scaleY = bone.data.scaleY;
						return;
					case MixBlend.first:
						bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
						bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				}
				return;
			}
			var x = 0, y = 0;
			if (time >= frames[frames.length - ScaleTimeline.ENTRIES]) {
				x = frames[frames.length + ScaleTimeline.PREV_X] * bone.data.scaleX;
				y = frames[frames.length + ScaleTimeline.PREV_Y] * bone.data.scaleY;
			}
			else {
				var frame = Animation.binarySearch(frames, time, ScaleTimeline.ENTRIES);
				x = frames[frame + ScaleTimeline.PREV_X];
				y = frames[frame + ScaleTimeline.PREV_Y];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / ScaleTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + ScaleTimeline.PREV_TIME] - frameTime));
				x = (x + (frames[frame + ScaleTimeline.X] - x) * percent) * bone.data.scaleX;
				y = (y + (frames[frame + ScaleTimeline.Y] - y) * percent) * bone.data.scaleY;
			}
			if (alpha == 1) {
				if (blend == MixBlend.add) {
					bone.scaleX += x - bone.data.scaleX;
					bone.scaleY += y - bone.data.scaleY;
				}
				else {
					bone.scaleX = x;
					bone.scaleY = y;
				}
			}
			else {
				var bx = 0, by = 0;
				if (direction == MixDirection.mixOut) {
					switch (blend) {
						case MixBlend.setup:
							bx = bone.data.scaleX;
							by = bone.data.scaleY;
							bone.scaleX = bx + (Math.abs(x) * spine.MathUtils.signum(bx) - bx) * alpha;
							bone.scaleY = by + (Math.abs(y) * spine.MathUtils.signum(by) - by) * alpha;
							break;
						case MixBlend.first:
						case MixBlend.replace:
							bx = bone.scaleX;
							by = bone.scaleY;
							bone.scaleX = bx + (Math.abs(x) * spine.MathUtils.signum(bx) - bx) * alpha;
							bone.scaleY = by + (Math.abs(y) * spine.MathUtils.signum(by) - by) * alpha;
							break;
						case MixBlend.add:
							bx = bone.scaleX;
							by = bone.scaleY;
							bone.scaleX = bx + (Math.abs(x) * spine.MathUtils.signum(bx) - bone.data.scaleX) * alpha;
							bone.scaleY = by + (Math.abs(y) * spine.MathUtils.signum(by) - bone.data.scaleY) * alpha;
					}
				}
				else {
					switch (blend) {
						case MixBlend.setup:
							bx = Math.abs(bone.data.scaleX) * spine.MathUtils.signum(x);
							by = Math.abs(bone.data.scaleY) * spine.MathUtils.signum(y);
							bone.scaleX = bx + (x - bx) * alpha;
							bone.scaleY = by + (y - by) * alpha;
							break;
						case MixBlend.first:
						case MixBlend.replace:
							bx = Math.abs(bone.scaleX) * spine.MathUtils.signum(x);
							by = Math.abs(bone.scaleY) * spine.MathUtils.signum(y);
							bone.scaleX = bx + (x - bx) * alpha;
							bone.scaleY = by + (y - by) * alpha;
							break;
						case MixBlend.add:
							bx = spine.MathUtils.signum(x);
							by = spine.MathUtils.signum(y);
							bone.scaleX = Math.abs(bone.scaleX) * bx + (x - Math.abs(bone.data.scaleX) * bx) * alpha;
							bone.scaleY = Math.abs(bone.scaleY) * by + (y - Math.abs(bone.data.scaleY) * by) * alpha;
					}
				}
			}
		};
		return ScaleTimeline;
	}(TranslateTimeline));
	spine.ScaleTimeline = ScaleTimeline;
	var ShearTimeline = (function (_super) {
		__extends(ShearTimeline, _super);
		function ShearTimeline(frameCount) {
			return _super.call(this, frameCount) || this;
		}
		ShearTimeline.prototype.getPropertyId = function () {
			return (TimelineType.shear << 24) + this.boneIndex;
		};
		ShearTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var frames = this.frames;
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.shearX = bone.data.shearX;
						bone.shearY = bone.data.shearY;
						return;
					case MixBlend.first:
						bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
						bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
				}
				return;
			}
			var x = 0, y = 0;
			if (time >= frames[frames.length - ShearTimeline.ENTRIES]) {
				x = frames[frames.length + ShearTimeline.PREV_X];
				y = frames[frames.length + ShearTimeline.PREV_Y];
			}
			else {
				var frame = Animation.binarySearch(frames, time, ShearTimeline.ENTRIES);
				x = frames[frame + ShearTimeline.PREV_X];
				y = frames[frame + ShearTimeline.PREV_Y];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / ShearTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + ShearTimeline.PREV_TIME] - frameTime));
				x = x + (frames[frame + ShearTimeline.X] - x) * percent;
				y = y + (frames[frame + ShearTimeline.Y] - y) * percent;
			}
			switch (blend) {
				case MixBlend.setup:
					bone.shearX = bone.data.shearX + x * alpha;
					bone.shearY = bone.data.shearY + y * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
					break;
				case MixBlend.add:
					bone.shearX += x * alpha;
					bone.shearY += y * alpha;
			}
		};
		return ShearTimeline;
	}(TranslateTimeline));
	spine.ShearTimeline = ShearTimeline;
	var ColorTimeline = (function (_super) {
		__extends(ColorTimeline, _super);
		function ColorTimeline(frameCount) {
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * ColorTimeline.ENTRIES);
			return _this;
		}
		ColorTimeline.prototype.getPropertyId = function () {
			return (TimelineType.color << 24) + this.slotIndex;
		};
		ColorTimeline.prototype.setFrame = function (frameIndex, time, r, g, b, a) {
			frameIndex *= ColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + ColorTimeline.R] = r;
			this.frames[frameIndex + ColorTimeline.G] = g;
			this.frames[frameIndex + ColorTimeline.B] = b;
			this.frames[frameIndex + ColorTimeline.A] = a;
		};
		ColorTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						slot.color.setFromColor(slot.data.color);
						return;
					case MixBlend.first:
						var color = slot.color, setup = slot.data.color;
						color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha, (setup.a - color.a) * alpha);
				}
				return;
			}
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
			if (alpha == 1)
				slot.color.set(r, g, b, a);
			else {
				var color = slot.color;
				if (blend == MixBlend.setup)
					color.setFromColor(slot.data.color);
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
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
	var TwoColorTimeline = (function (_super) {
		__extends(TwoColorTimeline, _super);
		function TwoColorTimeline(frameCount) {
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * TwoColorTimeline.ENTRIES);
			return _this;
		}
		TwoColorTimeline.prototype.getPropertyId = function () {
			return (TimelineType.twoColor << 24) + this.slotIndex;
		};
		TwoColorTimeline.prototype.setFrame = function (frameIndex, time, r, g, b, a, r2, g2, b2) {
			frameIndex *= TwoColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TwoColorTimeline.R] = r;
			this.frames[frameIndex + TwoColorTimeline.G] = g;
			this.frames[frameIndex + TwoColorTimeline.B] = b;
			this.frames[frameIndex + TwoColorTimeline.A] = a;
			this.frames[frameIndex + TwoColorTimeline.R2] = r2;
			this.frames[frameIndex + TwoColorTimeline.G2] = g2;
			this.frames[frameIndex + TwoColorTimeline.B2] = b2;
		};
		TwoColorTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						slot.color.setFromColor(slot.data.color);
						slot.darkColor.setFromColor(slot.data.darkColor);
						return;
					case MixBlend.first:
						var light = slot.color, dark = slot.darkColor, setupLight = slot.data.color, setupDark = slot.data.darkColor;
						light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha, (setupLight.a - light.a) * alpha);
						dark.add((setupDark.r - dark.r) * alpha, (setupDark.g - dark.g) * alpha, (setupDark.b - dark.b) * alpha, 0);
				}
				return;
			}
			var r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
			if (time >= frames[frames.length - TwoColorTimeline.ENTRIES]) {
				var i = frames.length;
				r = frames[i + TwoColorTimeline.PREV_R];
				g = frames[i + TwoColorTimeline.PREV_G];
				b = frames[i + TwoColorTimeline.PREV_B];
				a = frames[i + TwoColorTimeline.PREV_A];
				r2 = frames[i + TwoColorTimeline.PREV_R2];
				g2 = frames[i + TwoColorTimeline.PREV_G2];
				b2 = frames[i + TwoColorTimeline.PREV_B2];
			}
			else {
				var frame = Animation.binarySearch(frames, time, TwoColorTimeline.ENTRIES);
				r = frames[frame + TwoColorTimeline.PREV_R];
				g = frames[frame + TwoColorTimeline.PREV_G];
				b = frames[frame + TwoColorTimeline.PREV_B];
				a = frames[frame + TwoColorTimeline.PREV_A];
				r2 = frames[frame + TwoColorTimeline.PREV_R2];
				g2 = frames[frame + TwoColorTimeline.PREV_G2];
				b2 = frames[frame + TwoColorTimeline.PREV_B2];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / TwoColorTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + TwoColorTimeline.PREV_TIME] - frameTime));
				r += (frames[frame + TwoColorTimeline.R] - r) * percent;
				g += (frames[frame + TwoColorTimeline.G] - g) * percent;
				b += (frames[frame + TwoColorTimeline.B] - b) * percent;
				a += (frames[frame + TwoColorTimeline.A] - a) * percent;
				r2 += (frames[frame + TwoColorTimeline.R2] - r2) * percent;
				g2 += (frames[frame + TwoColorTimeline.G2] - g2) * percent;
				b2 += (frames[frame + TwoColorTimeline.B2] - b2) * percent;
			}
			if (alpha == 1) {
				slot.color.set(r, g, b, a);
				slot.darkColor.set(r2, g2, b2, 1);
			}
			else {
				var light = slot.color, dark = slot.darkColor;
				if (blend == MixBlend.setup) {
					light.setFromColor(slot.data.color);
					dark.setFromColor(slot.data.darkColor);
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.add((r2 - dark.r) * alpha, (g2 - dark.g) * alpha, (b2 - dark.b) * alpha, 0);
			}
		};
		TwoColorTimeline.ENTRIES = 8;
		TwoColorTimeline.PREV_TIME = -8;
		TwoColorTimeline.PREV_R = -7;
		TwoColorTimeline.PREV_G = -6;
		TwoColorTimeline.PREV_B = -5;
		TwoColorTimeline.PREV_A = -4;
		TwoColorTimeline.PREV_R2 = -3;
		TwoColorTimeline.PREV_G2 = -2;
		TwoColorTimeline.PREV_B2 = -1;
		TwoColorTimeline.R = 1;
		TwoColorTimeline.G = 2;
		TwoColorTimeline.B = 3;
		TwoColorTimeline.A = 4;
		TwoColorTimeline.R2 = 5;
		TwoColorTimeline.G2 = 6;
		TwoColorTimeline.B2 = 7;
		return TwoColorTimeline;
	}(CurveTimeline));
	spine.TwoColorTimeline = TwoColorTimeline;
	var AttachmentTimeline = (function () {
		function AttachmentTimeline(frameCount) {
			this.frames = spine.Utils.newFloatArray(frameCount);
			this.attachmentNames = new Array(frameCount);
		}
		AttachmentTimeline.prototype.getPropertyId = function () {
			return (TimelineType.attachment << 24) + this.slotIndex;
		};
		AttachmentTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		AttachmentTimeline.prototype.setFrame = function (frameIndex, time, attachmentName) {
			this.frames[frameIndex] = time;
			this.attachmentNames[frameIndex] = attachmentName;
		};
		AttachmentTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup)
					this.setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}
			var frames = this.frames;
			if (time < frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first)
					this.setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}
			var frameIndex = 0;
			if (time >= frames[frames.length - 1])
				frameIndex = frames.length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;
			var attachmentName = this.attachmentNames[frameIndex];
			skeleton.slots[this.slotIndex]
				.setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
		};
		AttachmentTimeline.prototype.setAttachment = function (skeleton, slot, attachmentName) {
			slot.attachment = attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName);
		};
		return AttachmentTimeline;
	}());
	spine.AttachmentTimeline = AttachmentTimeline;
	var zeros = null;
	var DeformTimeline = (function (_super) {
		__extends(DeformTimeline, _super);
		function DeformTimeline(frameCount) {
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount);
			_this.frameVertices = new Array(frameCount);
			if (zeros == null)
				zeros = spine.Utils.newFloatArray(64);
			return _this;
		}
		DeformTimeline.prototype.getPropertyId = function () {
			return (TimelineType.deform << 27) + +this.attachment.id + this.slotIndex;
		};
		DeformTimeline.prototype.setFrame = function (frameIndex, time, vertices) {
			this.frames[frameIndex] = time;
			this.frameVertices[frameIndex] = vertices;
		};
		DeformTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var slotAttachment = slot.getAttachment();
			if (!(slotAttachment instanceof spine.VertexAttachment) || !(slotAttachment.deformAttachment == this.attachment))
				return;
			var deformArray = slot.deform;
			if (deformArray.length == 0)
				blend = MixBlend.setup;
			var frameVertices = this.frameVertices;
			var vertexCount = frameVertices[0].length;
			var frames = this.frames;
			if (time < frames[0]) {
				var vertexAttachment = slotAttachment;
				switch (blend) {
					case MixBlend.setup:
						deformArray.length = 0;
						return;
					case MixBlend.first:
						if (alpha == 1) {
							deformArray.length = 0;
							break;
						}
						var deform_1 = spine.Utils.setArraySize(deformArray, vertexCount);
						if (vertexAttachment.bones == null) {
							var setupVertices = vertexAttachment.vertices;
							for (var i = 0; i < vertexCount; i++)
								deform_1[i] += (setupVertices[i] - deform_1[i]) * alpha;
						}
						else {
							alpha = 1 - alpha;
							for (var i = 0; i < vertexCount; i++)
								deform_1[i] *= alpha;
						}
				}
				return;
			}
			var deform = spine.Utils.setArraySize(deformArray, vertexCount);
			if (time >= frames[frames.length - 1]) {
				var lastVertices = frameVertices[frames.length - 1];
				if (alpha == 1) {
					if (blend == MixBlend.add) {
						var vertexAttachment = slotAttachment;
						if (vertexAttachment.bones == null) {
							var setupVertices = vertexAttachment.vertices;
							for (var i_1 = 0; i_1 < vertexCount; i_1++) {
								deform[i_1] += lastVertices[i_1] - setupVertices[i_1];
							}
						}
						else {
							for (var i_2 = 0; i_2 < vertexCount; i_2++)
								deform[i_2] += lastVertices[i_2];
						}
					}
					else {
						spine.Utils.arrayCopy(lastVertices, 0, deform, 0, vertexCount);
					}
				}
				else {
					switch (blend) {
						case MixBlend.setup: {
							var vertexAttachment_1 = slotAttachment;
							if (vertexAttachment_1.bones == null) {
								var setupVertices = vertexAttachment_1.vertices;
								for (var i_3 = 0; i_3 < vertexCount; i_3++) {
									var setup = setupVertices[i_3];
									deform[i_3] = setup + (lastVertices[i_3] - setup) * alpha;
								}
							}
							else {
								for (var i_4 = 0; i_4 < vertexCount; i_4++)
									deform[i_4] = lastVertices[i_4] * alpha;
							}
							break;
						}
						case MixBlend.first:
						case MixBlend.replace:
							for (var i_5 = 0; i_5 < vertexCount; i_5++)
								deform[i_5] += (lastVertices[i_5] - deform[i_5]) * alpha;
							break;
						case MixBlend.add:
							var vertexAttachment = slotAttachment;
							if (vertexAttachment.bones == null) {
								var setupVertices = vertexAttachment.vertices;
								for (var i_6 = 0; i_6 < vertexCount; i_6++) {
									deform[i_6] += (lastVertices[i_6] - setupVertices[i_6]) * alpha;
								}
							}
							else {
								for (var i_7 = 0; i_7 < vertexCount; i_7++)
									deform[i_7] += lastVertices[i_7] * alpha;
							}
					}
				}
				return;
			}
			var frame = Animation.binarySearch(frames, time);
			var prevVertices = frameVertices[frame - 1];
			var nextVertices = frameVertices[frame];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));
			if (alpha == 1) {
				if (blend == MixBlend.add) {
					var vertexAttachment = slotAttachment;
					if (vertexAttachment.bones == null) {
						var setupVertices = vertexAttachment.vertices;
						for (var i_8 = 0; i_8 < vertexCount; i_8++) {
							var prev = prevVertices[i_8];
							deform[i_8] += prev + (nextVertices[i_8] - prev) * percent - setupVertices[i_8];
						}
					}
					else {
						for (var i_9 = 0; i_9 < vertexCount; i_9++) {
							var prev = prevVertices[i_9];
							deform[i_9] += prev + (nextVertices[i_9] - prev) * percent;
						}
					}
				}
				else {
					for (var i_10 = 0; i_10 < vertexCount; i_10++) {
						var prev = prevVertices[i_10];
						deform[i_10] = prev + (nextVertices[i_10] - prev) * percent;
					}
				}
			}
			else {
				switch (blend) {
					case MixBlend.setup: {
						var vertexAttachment_2 = slotAttachment;
						if (vertexAttachment_2.bones == null) {
							var setupVertices = vertexAttachment_2.vertices;
							for (var i_11 = 0; i_11 < vertexCount; i_11++) {
								var prev = prevVertices[i_11], setup = setupVertices[i_11];
								deform[i_11] = setup + (prev + (nextVertices[i_11] - prev) * percent - setup) * alpha;
							}
						}
						else {
							for (var i_12 = 0; i_12 < vertexCount; i_12++) {
								var prev = prevVertices[i_12];
								deform[i_12] = (prev + (nextVertices[i_12] - prev) * percent) * alpha;
							}
						}
						break;
					}
					case MixBlend.first:
					case MixBlend.replace:
						for (var i_13 = 0; i_13 < vertexCount; i_13++) {
							var prev = prevVertices[i_13];
							deform[i_13] += (prev + (nextVertices[i_13] - prev) * percent - deform[i_13]) * alpha;
						}
						break;
					case MixBlend.add:
						var vertexAttachment = slotAttachment;
						if (vertexAttachment.bones == null) {
							var setupVertices = vertexAttachment.vertices;
							for (var i_14 = 0; i_14 < vertexCount; i_14++) {
								var prev = prevVertices[i_14];
								deform[i_14] += (prev + (nextVertices[i_14] - prev) * percent - setupVertices[i_14]) * alpha;
							}
						}
						else {
							for (var i_15 = 0; i_15 < vertexCount; i_15++) {
								var prev = prevVertices[i_15];
								deform[i_15] += (prev + (nextVertices[i_15] - prev) * percent) * alpha;
							}
						}
				}
			}
		};
		return DeformTimeline;
	}(CurveTimeline));
	spine.DeformTimeline = DeformTimeline;
	var EventTimeline = (function () {
		function EventTimeline(frameCount) {
			this.frames = spine.Utils.newFloatArray(frameCount);
			this.events = new Array(frameCount);
		}
		EventTimeline.prototype.getPropertyId = function () {
			return TimelineType.event << 24;
		};
		EventTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		EventTimeline.prototype.setFrame = function (frameIndex, event) {
			this.frames[frameIndex] = event.time;
			this.events[frameIndex] = event;
		};
		EventTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			if (firedEvents == null)
				return;
			var frames = this.frames;
			var frameCount = this.frames.length;
			if (lastTime > time) {
				this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha, blend, direction);
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
		DrawOrderTimeline.prototype.getPropertyId = function () {
			return TimelineType.drawOrder << 24;
		};
		DrawOrderTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		DrawOrderTimeline.prototype.setFrame = function (frameIndex, time, drawOrder) {
			this.frames[frameIndex] = time;
			this.drawOrders[frameIndex] = drawOrder;
		};
		DrawOrderTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var drawOrder = skeleton.drawOrder;
			var slots = skeleton.slots;
			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup)
					spine.Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}
			var frames = this.frames;
			if (time < frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first)
					spine.Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}
			var frame = 0;
			if (time >= frames[frames.length - 1])
				frame = frames.length - 1;
			else
				frame = Animation.binarySearch(frames, time) - 1;
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
	var IkConstraintTimeline = (function (_super) {
		__extends(IkConstraintTimeline, _super);
		function IkConstraintTimeline(frameCount) {
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * IkConstraintTimeline.ENTRIES);
			return _this;
		}
		IkConstraintTimeline.prototype.getPropertyId = function () {
			return (TimelineType.ikConstraint << 24) + this.ikConstraintIndex;
		};
		IkConstraintTimeline.prototype.setFrame = function (frameIndex, time, mix, softness, bendDirection, compress, stretch) {
			frameIndex *= IkConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + IkConstraintTimeline.MIX] = mix;
			this.frames[frameIndex + IkConstraintTimeline.SOFTNESS] = softness;
			this.frames[frameIndex + IkConstraintTimeline.BEND_DIRECTION] = bendDirection;
			this.frames[frameIndex + IkConstraintTimeline.COMPRESS] = compress ? 1 : 0;
			this.frames[frameIndex + IkConstraintTimeline.STRETCH] = stretch ? 1 : 0;
		};
		IkConstraintTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var frames = this.frames;
			var constraint = skeleton.ikConstraints[this.ikConstraintIndex];
			if (!constraint.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						constraint.mix = constraint.data.mix;
						constraint.softness = constraint.data.softness;
						constraint.bendDirection = constraint.data.bendDirection;
						constraint.compress = constraint.data.compress;
						constraint.stretch = constraint.data.stretch;
						return;
					case MixBlend.first:
						constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
						constraint.softness += (constraint.data.softness - constraint.softness) * alpha;
						constraint.bendDirection = constraint.data.bendDirection;
						constraint.compress = constraint.data.compress;
						constraint.stretch = constraint.data.stretch;
				}
				return;
			}
			if (time >= frames[frames.length - IkConstraintTimeline.ENTRIES]) {
				if (blend == MixBlend.setup) {
					constraint.mix = constraint.data.mix + (frames[frames.length + IkConstraintTimeline.PREV_MIX] - constraint.data.mix) * alpha;
					constraint.softness = constraint.data.softness
						+ (frames[frames.length + IkConstraintTimeline.PREV_SOFTNESS] - constraint.data.softness) * alpha;
					if (direction == MixDirection.mixOut) {
						constraint.bendDirection = constraint.data.bendDirection;
						constraint.compress = constraint.data.compress;
						constraint.stretch = constraint.data.stretch;
					}
					else {
						constraint.bendDirection = frames[frames.length + IkConstraintTimeline.PREV_BEND_DIRECTION];
						constraint.compress = frames[frames.length + IkConstraintTimeline.PREV_COMPRESS] != 0;
						constraint.stretch = frames[frames.length + IkConstraintTimeline.PREV_STRETCH] != 0;
					}
				}
				else {
					constraint.mix += (frames[frames.length + IkConstraintTimeline.PREV_MIX] - constraint.mix) * alpha;
					constraint.softness += (frames[frames.length + IkConstraintTimeline.PREV_SOFTNESS] - constraint.softness) * alpha;
					if (direction == MixDirection.mixIn) {
						constraint.bendDirection = frames[frames.length + IkConstraintTimeline.PREV_BEND_DIRECTION];
						constraint.compress = frames[frames.length + IkConstraintTimeline.PREV_COMPRESS] != 0;
						constraint.stretch = frames[frames.length + IkConstraintTimeline.PREV_STRETCH] != 0;
					}
				}
				return;
			}
			var frame = Animation.binarySearch(frames, time, IkConstraintTimeline.ENTRIES);
			var mix = frames[frame + IkConstraintTimeline.PREV_MIX];
			var softness = frames[frame + IkConstraintTimeline.PREV_SOFTNESS];
			var frameTime = frames[frame];
			var percent = this.getCurvePercent(frame / IkConstraintTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + IkConstraintTimeline.PREV_TIME] - frameTime));
			if (blend == MixBlend.setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + IkConstraintTimeline.MIX] - mix) * percent - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness
					+ (softness + (frames[frame + IkConstraintTimeline.SOFTNESS] - softness) * percent - constraint.data.softness) * alpha;
				if (direction == MixDirection.mixOut) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				}
				else {
					constraint.bendDirection = frames[frame + IkConstraintTimeline.PREV_BEND_DIRECTION];
					constraint.compress = frames[frame + IkConstraintTimeline.PREV_COMPRESS] != 0;
					constraint.stretch = frames[frame + IkConstraintTimeline.PREV_STRETCH] != 0;
				}
			}
			else {
				constraint.mix += (mix + (frames[frame + IkConstraintTimeline.MIX] - mix) * percent - constraint.mix) * alpha;
				constraint.softness += (softness + (frames[frame + IkConstraintTimeline.SOFTNESS] - softness) * percent - constraint.softness) * alpha;
				if (direction == MixDirection.mixIn) {
					constraint.bendDirection = frames[frame + IkConstraintTimeline.PREV_BEND_DIRECTION];
					constraint.compress = frames[frame + IkConstraintTimeline.PREV_COMPRESS] != 0;
					constraint.stretch = frames[frame + IkConstraintTimeline.PREV_STRETCH] != 0;
				}
			}
		};
		IkConstraintTimeline.ENTRIES = 6;
		IkConstraintTimeline.PREV_TIME = -6;
		IkConstraintTimeline.PREV_MIX = -5;
		IkConstraintTimeline.PREV_SOFTNESS = -4;
		IkConstraintTimeline.PREV_BEND_DIRECTION = -3;
		IkConstraintTimeline.PREV_COMPRESS = -2;
		IkConstraintTimeline.PREV_STRETCH = -1;
		IkConstraintTimeline.MIX = 1;
		IkConstraintTimeline.SOFTNESS = 2;
		IkConstraintTimeline.BEND_DIRECTION = 3;
		IkConstraintTimeline.COMPRESS = 4;
		IkConstraintTimeline.STRETCH = 5;
		return IkConstraintTimeline;
	}(CurveTimeline));
	spine.IkConstraintTimeline = IkConstraintTimeline;
	var TransformConstraintTimeline = (function (_super) {
		__extends(TransformConstraintTimeline, _super);
		function TransformConstraintTimeline(frameCount) {
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * TransformConstraintTimeline.ENTRIES);
			return _this;
		}
		TransformConstraintTimeline.prototype.getPropertyId = function () {
			return (TimelineType.transformConstraint << 24) + this.transformConstraintIndex;
		};
		TransformConstraintTimeline.prototype.setFrame = function (frameIndex, time, rotateMix, translateMix, scaleMix, shearMix) {
			frameIndex *= TransformConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TransformConstraintTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + TransformConstraintTimeline.TRANSLATE] = translateMix;
			this.frames[frameIndex + TransformConstraintTimeline.SCALE] = scaleMix;
			this.frames[frameIndex + TransformConstraintTimeline.SHEAR] = shearMix;
		};
		TransformConstraintTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var frames = this.frames;
			var constraint = skeleton.transformConstraints[this.transformConstraintIndex];
			if (!constraint.active)
				return;
			if (time < frames[0]) {
				var data = constraint.data;
				switch (blend) {
					case MixBlend.setup:
						constraint.rotateMix = data.rotateMix;
						constraint.translateMix = data.translateMix;
						constraint.scaleMix = data.scaleMix;
						constraint.shearMix = data.shearMix;
						return;
					case MixBlend.first:
						constraint.rotateMix += (data.rotateMix - constraint.rotateMix) * alpha;
						constraint.translateMix += (data.translateMix - constraint.translateMix) * alpha;
						constraint.scaleMix += (data.scaleMix - constraint.scaleMix) * alpha;
						constraint.shearMix += (data.shearMix - constraint.shearMix) * alpha;
				}
				return;
			}
			var rotate = 0, translate = 0, scale = 0, shear = 0;
			if (time >= frames[frames.length - TransformConstraintTimeline.ENTRIES]) {
				var i = frames.length;
				rotate = frames[i + TransformConstraintTimeline.PREV_ROTATE];
				translate = frames[i + TransformConstraintTimeline.PREV_TRANSLATE];
				scale = frames[i + TransformConstraintTimeline.PREV_SCALE];
				shear = frames[i + TransformConstraintTimeline.PREV_SHEAR];
			}
			else {
				var frame = Animation.binarySearch(frames, time, TransformConstraintTimeline.ENTRIES);
				rotate = frames[frame + TransformConstraintTimeline.PREV_ROTATE];
				translate = frames[frame + TransformConstraintTimeline.PREV_TRANSLATE];
				scale = frames[frame + TransformConstraintTimeline.PREV_SCALE];
				shear = frames[frame + TransformConstraintTimeline.PREV_SHEAR];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / TransformConstraintTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + TransformConstraintTimeline.PREV_TIME] - frameTime));
				rotate += (frames[frame + TransformConstraintTimeline.ROTATE] - rotate) * percent;
				translate += (frames[frame + TransformConstraintTimeline.TRANSLATE] - translate) * percent;
				scale += (frames[frame + TransformConstraintTimeline.SCALE] - scale) * percent;
				shear += (frames[frame + TransformConstraintTimeline.SHEAR] - shear) * percent;
			}
			if (blend == MixBlend.setup) {
				var data = constraint.data;
				constraint.rotateMix = data.rotateMix + (rotate - data.rotateMix) * alpha;
				constraint.translateMix = data.translateMix + (translate - data.translateMix) * alpha;
				constraint.scaleMix = data.scaleMix + (scale - data.scaleMix) * alpha;
				constraint.shearMix = data.shearMix + (shear - data.shearMix) * alpha;
			}
			else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
				constraint.scaleMix += (scale - constraint.scaleMix) * alpha;
				constraint.shearMix += (shear - constraint.shearMix) * alpha;
			}
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
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * PathConstraintPositionTimeline.ENTRIES);
			return _this;
		}
		PathConstraintPositionTimeline.prototype.getPropertyId = function () {
			return (TimelineType.pathConstraintPosition << 24) + this.pathConstraintIndex;
		};
		PathConstraintPositionTimeline.prototype.setFrame = function (frameIndex, time, value) {
			frameIndex *= PathConstraintPositionTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintPositionTimeline.VALUE] = value;
		};
		PathConstraintPositionTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var frames = this.frames;
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						constraint.position = constraint.data.position;
						return;
					case MixBlend.first:
						constraint.position += (constraint.data.position - constraint.position) * alpha;
				}
				return;
			}
			var position = 0;
			if (time >= frames[frames.length - PathConstraintPositionTimeline.ENTRIES])
				position = frames[frames.length + PathConstraintPositionTimeline.PREV_VALUE];
			else {
				var frame = Animation.binarySearch(frames, time, PathConstraintPositionTimeline.ENTRIES);
				position = frames[frame + PathConstraintPositionTimeline.PREV_VALUE];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / PathConstraintPositionTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PathConstraintPositionTimeline.PREV_TIME] - frameTime));
				position += (frames[frame + PathConstraintPositionTimeline.VALUE] - position) * percent;
			}
			if (blend == MixBlend.setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
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
			return _super.call(this, frameCount) || this;
		}
		PathConstraintSpacingTimeline.prototype.getPropertyId = function () {
			return (TimelineType.pathConstraintSpacing << 24) + this.pathConstraintIndex;
		};
		PathConstraintSpacingTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var frames = this.frames;
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						constraint.spacing = constraint.data.spacing;
						return;
					case MixBlend.first:
						constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
				}
				return;
			}
			var spacing = 0;
			if (time >= frames[frames.length - PathConstraintSpacingTimeline.ENTRIES])
				spacing = frames[frames.length + PathConstraintSpacingTimeline.PREV_VALUE];
			else {
				var frame = Animation.binarySearch(frames, time, PathConstraintSpacingTimeline.ENTRIES);
				spacing = frames[frame + PathConstraintSpacingTimeline.PREV_VALUE];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / PathConstraintSpacingTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PathConstraintSpacingTimeline.PREV_TIME] - frameTime));
				spacing += (frames[frame + PathConstraintSpacingTimeline.VALUE] - spacing) * percent;
			}
			if (blend == MixBlend.setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		};
		return PathConstraintSpacingTimeline;
	}(PathConstraintPositionTimeline));
	spine.PathConstraintSpacingTimeline = PathConstraintSpacingTimeline;
	var PathConstraintMixTimeline = (function (_super) {
		__extends(PathConstraintMixTimeline, _super);
		function PathConstraintMixTimeline(frameCount) {
			var _this = _super.call(this, frameCount) || this;
			_this.frames = spine.Utils.newFloatArray(frameCount * PathConstraintMixTimeline.ENTRIES);
			return _this;
		}
		PathConstraintMixTimeline.prototype.getPropertyId = function () {
			return (TimelineType.pathConstraintMix << 24) + this.pathConstraintIndex;
		};
		PathConstraintMixTimeline.prototype.setFrame = function (frameIndex, time, rotateMix, translateMix) {
			frameIndex *= PathConstraintMixTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintMixTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + PathConstraintMixTimeline.TRANSLATE] = translateMix;
		};
		PathConstraintMixTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var frames = this.frames;
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active)
				return;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						constraint.rotateMix = constraint.data.rotateMix;
						constraint.translateMix = constraint.data.translateMix;
						return;
					case MixBlend.first:
						constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
						constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
				}
				return;
			}
			var rotate = 0, translate = 0;
			if (time >= frames[frames.length - PathConstraintMixTimeline.ENTRIES]) {
				rotate = frames[frames.length + PathConstraintMixTimeline.PREV_ROTATE];
				translate = frames[frames.length + PathConstraintMixTimeline.PREV_TRANSLATE];
			}
			else {
				var frame = Animation.binarySearch(frames, time, PathConstraintMixTimeline.ENTRIES);
				rotate = frames[frame + PathConstraintMixTimeline.PREV_ROTATE];
				translate = frames[frame + PathConstraintMixTimeline.PREV_TRANSLATE];
				var frameTime = frames[frame];
				var percent = this.getCurvePercent(frame / PathConstraintMixTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PathConstraintMixTimeline.PREV_TIME] - frameTime));
				rotate += (frames[frame + PathConstraintMixTimeline.ROTATE] - rotate) * percent;
				translate += (frames[frame + PathConstraintMixTimeline.TRANSLATE] - translate) * percent;
			}
			if (blend == MixBlend.setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			}
			else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
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
			this.tracks = new Array();
			this.timeScale = 1;
			this.unkeyedState = 0;
			this.events = new Array();
			this.listeners = new Array();
			this.queue = new EventQueue(this);
			this.propertyIDs = new spine.IntSet();
			this.animationsChanged = false;
			this.trackEntryPool = new spine.Pool(function () { return new TrackEntry(); });
			this.data = data;
		}
		AnimationState.prototype.update = function (delta) {
			delta *= this.timeScale;
			var tracks = this.tracks;
			for (var i = 0, n = tracks.length; i < n; i++) {
				var current = tracks[i];
				if (current == null)
					continue;
				current.animationLast = current.nextAnimationLast;
				current.trackLast = current.nextTrackLast;
				var currentDelta = delta * current.timeScale;
				if (current.delay > 0) {
					current.delay -= currentDelta;
					if (current.delay > 0)
						continue;
					currentDelta = -current.delay;
					current.delay = 0;
				}
				var next = current.next;
				if (next != null) {
					var nextTime = current.trackLast - next.delay;
					if (nextTime >= 0) {
						next.delay = 0;
						next.trackTime += current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
						current.trackTime += currentDelta;
						this.setCurrent(i, next, true);
						while (next.mixingFrom != null) {
							next.mixTime += delta;
							next = next.mixingFrom;
						}
						continue;
					}
				}
				else if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
					tracks[i] = null;
					this.queue.end(current);
					this.disposeNext(current);
					continue;
				}
				if (current.mixingFrom != null && this.updateMixingFrom(current, delta)) {
					var from = current.mixingFrom;
					current.mixingFrom = null;
					if (from != null)
						from.mixingTo = null;
					while (from != null) {
						this.queue.end(from);
						from = from.mixingFrom;
					}
				}
				current.trackTime += currentDelta;
			}
			this.queue.drain();
		};
		AnimationState.prototype.updateMixingFrom = function (to, delta) {
			var from = to.mixingFrom;
			if (from == null)
				return true;
			var finished = this.updateMixingFrom(from, delta);
			from.animationLast = from.nextAnimationLast;
			from.trackLast = from.nextTrackLast;
			if (to.mixTime > 0 && to.mixTime >= to.mixDuration) {
				if (from.totalAlpha == 0 || to.mixDuration == 0) {
					to.mixingFrom = from.mixingFrom;
					if (from.mixingFrom != null)
						from.mixingFrom.mixingTo = to;
					to.interruptAlpha = from.interruptAlpha;
					this.queue.end(from);
				}
				return finished;
			}
			from.trackTime += delta * from.timeScale;
			to.mixTime += delta;
			return false;
		};
		AnimationState.prototype.apply = function (skeleton) {
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			if (this.animationsChanged)
				this._animationsChanged();
			var events = this.events;
			var tracks = this.tracks;
			var applied = false;
			for (var i_16 = 0, n_1 = tracks.length; i_16 < n_1; i_16++) {
				var current = tracks[i_16];
				if (current == null || current.delay > 0)
					continue;
				applied = true;
				var blend = i_16 == 0 ? spine.MixBlend.first : current.mixBlend;
				var mix = current.alpha;
				if (current.mixingFrom != null)
					mix *= this.applyMixingFrom(current, skeleton, blend);
				else if (current.trackTime >= current.trackEnd && current.next == null)
					mix = 0;
				var animationLast = current.animationLast, animationTime = current.getAnimationTime();
				var timelineCount = current.animation.timelines.length;
				var timelines = current.animation.timelines;
				if ((i_16 == 0 && mix == 1) || blend == spine.MixBlend.add) {
					for (var ii = 0; ii < timelineCount; ii++) {
						spine.Utils.webkit602BugfixHelper(mix, blend);
						var timeline = timelines[ii];
						if (timeline instanceof spine.AttachmentTimeline)
							this.applyAttachmentTimeline(timeline, skeleton, animationTime, blend, true);
						else
							timeline.apply(skeleton, animationLast, animationTime, events, mix, blend, spine.MixDirection.mixIn);
					}
				}
				else {
					var timelineMode = current.timelineMode;
					var firstFrame = current.timelinesRotation.length == 0;
					if (firstFrame)
						spine.Utils.setArraySize(current.timelinesRotation, timelineCount << 1, null);
					var timelinesRotation = current.timelinesRotation;
					for (var ii = 0; ii < timelineCount; ii++) {
						var timeline_1 = timelines[ii];
						var timelineBlend = timelineMode[ii] == AnimationState.SUBSEQUENT ? blend : spine.MixBlend.setup;
						if (timeline_1 instanceof spine.RotateTimeline) {
							this.applyRotateTimeline(timeline_1, skeleton, animationTime, mix, timelineBlend, timelinesRotation, ii << 1, firstFrame);
						}
						else if (timeline_1 instanceof spine.AttachmentTimeline) {
							this.applyAttachmentTimeline(timeline_1, skeleton, animationTime, blend, true);
						}
						else {
							spine.Utils.webkit602BugfixHelper(mix, blend);
							timeline_1.apply(skeleton, animationLast, animationTime, events, mix, timelineBlend, spine.MixDirection.mixIn);
						}
					}
				}
				this.queueEvents(current, animationTime);
				events.length = 0;
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}
			var setupState = this.unkeyedState + AnimationState.SETUP;
			var slots = skeleton.slots;
			for (var i = 0, n = skeleton.slots.length; i < n; i++) {
				var slot = slots[i];
				if (slot.attachmentState == setupState) {
					var attachmentName = slot.data.attachmentName;
					slot.attachment = (attachmentName == null ? null : skeleton.getAttachment(slot.data.index, attachmentName));
				}
			}
			this.unkeyedState += 2;
			this.queue.drain();
			return applied;
		};
		AnimationState.prototype.applyMixingFrom = function (to, skeleton, blend) {
			var from = to.mixingFrom;
			if (from.mixingFrom != null)
				this.applyMixingFrom(from, skeleton, blend);
			var mix = 0;
			if (to.mixDuration == 0) {
				mix = 1;
				if (blend == spine.MixBlend.first)
					blend = spine.MixBlend.setup;
			}
			else {
				mix = to.mixTime / to.mixDuration;
				if (mix > 1)
					mix = 1;
				if (blend != spine.MixBlend.first)
					blend = from.mixBlend;
			}
			var events = mix < from.eventThreshold ? this.events : null;
			var attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
			var animationLast = from.animationLast, animationTime = from.getAnimationTime();
			var timelineCount = from.animation.timelines.length;
			var timelines = from.animation.timelines;
			var alphaHold = from.alpha * to.interruptAlpha, alphaMix = alphaHold * (1 - mix);
			if (blend == spine.MixBlend.add) {
				for (var i = 0; i < timelineCount; i++)
					timelines[i].apply(skeleton, animationLast, animationTime, events, alphaMix, blend, spine.MixDirection.mixOut);
			}
			else {
				var timelineMode = from.timelineMode;
				var timelineHoldMix = from.timelineHoldMix;
				var firstFrame = from.timelinesRotation.length == 0;
				if (firstFrame)
					spine.Utils.setArraySize(from.timelinesRotation, timelineCount << 1, null);
				var timelinesRotation = from.timelinesRotation;
				from.totalAlpha = 0;
				for (var i = 0; i < timelineCount; i++) {
					var timeline = timelines[i];
					var direction = spine.MixDirection.mixOut;
					var timelineBlend = void 0;
					var alpha = 0;
					switch (timelineMode[i]) {
						case AnimationState.SUBSEQUENT:
							if (!drawOrder && timeline instanceof spine.DrawOrderTimeline)
								continue;
							timelineBlend = blend;
							alpha = alphaMix;
							break;
						case AnimationState.FIRST:
							timelineBlend = spine.MixBlend.setup;
							alpha = alphaMix;
							break;
						case AnimationState.HOLD_SUBSEQUENT:
							timelineBlend = blend;
							alpha = alphaHold;
							break;
						case AnimationState.HOLD_FIRST:
							timelineBlend = spine.MixBlend.setup;
							alpha = alphaHold;
							break;
						default:
							timelineBlend = spine.MixBlend.setup;
							var holdMix = timelineHoldMix[i];
							alpha = alphaHold * Math.max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
							break;
					}
					from.totalAlpha += alpha;
					if (timeline instanceof spine.RotateTimeline)
						this.applyRotateTimeline(timeline, skeleton, animationTime, alpha, timelineBlend, timelinesRotation, i << 1, firstFrame);
					else if (timeline instanceof spine.AttachmentTimeline)
						this.applyAttachmentTimeline(timeline, skeleton, animationTime, timelineBlend, attachments);
					else {
						spine.Utils.webkit602BugfixHelper(alpha, blend);
						if (drawOrder && timeline instanceof spine.DrawOrderTimeline && timelineBlend == spine.MixBlend.setup)
							direction = spine.MixDirection.mixIn;
						timeline.apply(skeleton, animationLast, animationTime, events, alpha, timelineBlend, direction);
					}
				}
			}
			if (to.mixDuration > 0)
				this.queueEvents(from, animationTime);
			this.events.length = 0;
			from.nextAnimationLast = animationTime;
			from.nextTrackLast = from.trackTime;
			return mix;
		};
		AnimationState.prototype.applyAttachmentTimeline = function (timeline, skeleton, time, blend, attachments) {
			var slot = skeleton.slots[timeline.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = timeline.frames;
			if (time < frames[0]) {
				if (blend == spine.MixBlend.setup || blend == spine.MixBlend.first)
					this.setAttachment(skeleton, slot, slot.data.attachmentName, attachments);
			}
			else {
				var frameIndex;
				if (time >= frames[frames.length - 1])
					frameIndex = frames.length - 1;
				else
					frameIndex = spine.Animation.binarySearch(frames, time) - 1;
				this.setAttachment(skeleton, slot, timeline.attachmentNames[frameIndex], attachments);
			}
			if (slot.attachmentState <= this.unkeyedState)
				slot.attachmentState = this.unkeyedState + AnimationState.SETUP;
		};
		AnimationState.prototype.setAttachment = function (skeleton, slot, attachmentName, attachments) {
			slot.attachment = attachmentName == null ? null : skeleton.getAttachment(slot.data.index, attachmentName);
			if (attachments)
				slot.attachmentState = this.unkeyedState + AnimationState.CURRENT;
		};
		AnimationState.prototype.applyRotateTimeline = function (timeline, skeleton, time, alpha, blend, timelinesRotation, i, firstFrame) {
			if (firstFrame)
				timelinesRotation[i] = 0;
			if (alpha == 1) {
				timeline.apply(skeleton, 0, time, null, 1, blend, spine.MixDirection.mixIn);
				return;
			}
			var rotateTimeline = timeline;
			var frames = rotateTimeline.frames;
			var bone = skeleton.bones[rotateTimeline.boneIndex];
			if (!bone.active)
				return;
			var r1 = 0, r2 = 0;
			if (time < frames[0]) {
				switch (blend) {
					case spine.MixBlend.setup:
						bone.rotation = bone.data.rotation;
					default:
						return;
					case spine.MixBlend.first:
						r1 = bone.rotation;
						r2 = bone.data.rotation;
				}
			}
			else {
				r1 = blend == spine.MixBlend.setup ? bone.data.rotation : bone.rotation;
				if (time >= frames[frames.length - spine.RotateTimeline.ENTRIES])
					r2 = bone.data.rotation + frames[frames.length + spine.RotateTimeline.PREV_ROTATION];
				else {
					var frame = spine.Animation.binarySearch(frames, time, spine.RotateTimeline.ENTRIES);
					var prevRotation = frames[frame + spine.RotateTimeline.PREV_ROTATION];
					var frameTime = frames[frame];
					var percent = rotateTimeline.getCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + spine.RotateTimeline.PREV_TIME] - frameTime));
					r2 = frames[frame + spine.RotateTimeline.ROTATION] - prevRotation;
					r2 -= (16384 - ((16384.499999999996 - r2 / 360) | 0)) * 360;
					r2 = prevRotation + r2 * percent + bone.data.rotation;
					r2 -= (16384 - ((16384.499999999996 - r2 / 360) | 0)) * 360;
				}
			}
			var total = 0, diff = r2 - r1;
			diff -= (16384 - ((16384.499999999996 - diff / 360) | 0)) * 360;
			if (diff == 0) {
				total = timelinesRotation[i];
			}
			else {
				var lastTotal = 0, lastDiff = 0;
				if (firstFrame) {
					lastTotal = 0;
					lastDiff = diff;
				}
				else {
					lastTotal = timelinesRotation[i];
					lastDiff = timelinesRotation[i + 1];
				}
				var current = diff > 0, dir = lastTotal >= 0;
				if (spine.MathUtils.signum(lastDiff) != spine.MathUtils.signum(diff) && Math.abs(lastDiff) <= 90) {
					if (Math.abs(lastTotal) > 180)
						lastTotal += 360 * spine.MathUtils.signum(lastTotal);
					dir = current;
				}
				total = diff + lastTotal - lastTotal % 360;
				if (dir != current)
					total += 360 * spine.MathUtils.signum(lastTotal);
				timelinesRotation[i] = total;
			}
			timelinesRotation[i + 1] = diff;
			r1 += total * alpha;
			bone.rotation = r1 - (16384 - ((16384.499999999996 - r1 / 360) | 0)) * 360;
		};
		AnimationState.prototype.queueEvents = function (entry, animationTime) {
			var animationStart = entry.animationStart, animationEnd = entry.animationEnd;
			var duration = animationEnd - animationStart;
			var trackLastWrapped = entry.trackLast % duration;
			var events = this.events;
			var i = 0, n = events.length;
			for (; i < n; i++) {
				var event_1 = events[i];
				if (event_1.time < trackLastWrapped)
					break;
				if (event_1.time > animationEnd)
					continue;
				this.queue.event(entry, event_1);
			}
			var complete = false;
			if (entry.loop)
				complete = duration == 0 || trackLastWrapped > entry.trackTime % duration;
			else
				complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
			if (complete)
				this.queue.complete(entry);
			for (; i < n; i++) {
				var event_2 = events[i];
				if (event_2.time < animationStart)
					continue;
				this.queue.event(entry, events[i]);
			}
		};
		AnimationState.prototype.clearTracks = function () {
			var oldDrainDisabled = this.queue.drainDisabled;
			this.queue.drainDisabled = true;
			for (var i = 0, n = this.tracks.length; i < n; i++)
				this.clearTrack(i);
			this.tracks.length = 0;
			this.queue.drainDisabled = oldDrainDisabled;
			this.queue.drain();
		};
		AnimationState.prototype.clearTrack = function (trackIndex) {
			if (trackIndex >= this.tracks.length)
				return;
			var current = this.tracks[trackIndex];
			if (current == null)
				return;
			this.queue.end(current);
			this.disposeNext(current);
			var entry = current;
			while (true) {
				var from = entry.mixingFrom;
				if (from == null)
					break;
				this.queue.end(from);
				entry.mixingFrom = null;
				entry.mixingTo = null;
				entry = from;
			}
			this.tracks[current.trackIndex] = null;
			this.queue.drain();
		};
		AnimationState.prototype.setCurrent = function (index, current, interrupt) {
			var from = this.expandToIndex(index);
			this.tracks[index] = current;
			if (from != null) {
				if (interrupt)
					this.queue.interrupt(from);
				current.mixingFrom = from;
				from.mixingTo = current;
				current.mixTime = 0;
				if (from.mixingFrom != null && from.mixDuration > 0)
					current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);
				from.timelinesRotation.length = 0;
			}
			this.queue.start(current);
		};
		AnimationState.prototype.setAnimation = function (trackIndex, animationName, loop) {
			var animation = this.data.skeletonData.findAnimation(animationName);
			if (animation == null)
				throw new Error("Animation not found: " + animationName);
			return this.setAnimationWith(trackIndex, animation, loop);
		};
		AnimationState.prototype.setAnimationWith = function (trackIndex, animation, loop) {
			if (animation == null)
				throw new Error("animation cannot be null.");
			var interrupt = true;
			var current = this.expandToIndex(trackIndex);
			if (current != null) {
				if (current.nextTrackLast == -1) {
					this.tracks[trackIndex] = current.mixingFrom;
					this.queue.interrupt(current);
					this.queue.end(current);
					this.disposeNext(current);
					current = current.mixingFrom;
					interrupt = false;
				}
				else
					this.disposeNext(current);
			}
			var entry = this.trackEntry(trackIndex, animation, loop, current);
			this.setCurrent(trackIndex, entry, interrupt);
			this.queue.drain();
			return entry;
		};
		AnimationState.prototype.addAnimation = function (trackIndex, animationName, loop, delay) {
			var animation = this.data.skeletonData.findAnimation(animationName);
			if (animation == null)
				throw new Error("Animation not found: " + animationName);
			return this.addAnimationWith(trackIndex, animation, loop, delay);
		};
		AnimationState.prototype.addAnimationWith = function (trackIndex, animation, loop, delay) {
			if (animation == null)
				throw new Error("animation cannot be null.");
			var last = this.expandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
			}
			var entry = this.trackEntry(trackIndex, animation, loop, last);
			if (last == null) {
				this.setCurrent(trackIndex, entry, true);
				this.queue.drain();
			}
			else {
				last.next = entry;
				if (delay <= 0) {
					var duration = last.animationEnd - last.animationStart;
					if (duration != 0) {
						if (last.loop)
							delay += duration * (1 + ((last.trackTime / duration) | 0));
						else
							delay += Math.max(duration, last.trackTime);
						delay -= this.data.getMix(last.animation, animation);
					}
					else
						delay = last.trackTime;
				}
			}
			entry.delay = delay;
			return entry;
		};
		AnimationState.prototype.setEmptyAnimation = function (trackIndex, mixDuration) {
			var entry = this.setAnimationWith(trackIndex, AnimationState.emptyAnimation, false);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		};
		AnimationState.prototype.addEmptyAnimation = function (trackIndex, mixDuration, delay) {
			if (delay <= 0)
				delay -= mixDuration;
			var entry = this.addAnimationWith(trackIndex, AnimationState.emptyAnimation, false, delay);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		};
		AnimationState.prototype.setEmptyAnimations = function (mixDuration) {
			var oldDrainDisabled = this.queue.drainDisabled;
			this.queue.drainDisabled = true;
			for (var i = 0, n = this.tracks.length; i < n; i++) {
				var current = this.tracks[i];
				if (current != null)
					this.setEmptyAnimation(current.trackIndex, mixDuration);
			}
			this.queue.drainDisabled = oldDrainDisabled;
			this.queue.drain();
		};
		AnimationState.prototype.expandToIndex = function (index) {
			if (index < this.tracks.length)
				return this.tracks[index];
			spine.Utils.ensureArrayCapacity(this.tracks, index + 1, null);
			this.tracks.length = index + 1;
			return null;
		};
		AnimationState.prototype.trackEntry = function (trackIndex, animation, loop, last) {
			var entry = this.trackEntryPool.obtain();
			entry.trackIndex = trackIndex;
			entry.animation = animation;
			entry.loop = loop;
			entry.holdPrevious = false;
			entry.eventThreshold = 0;
			entry.attachmentThreshold = 0;
			entry.drawOrderThreshold = 0;
			entry.animationStart = 0;
			entry.animationEnd = animation.duration;
			entry.animationLast = -1;
			entry.nextAnimationLast = -1;
			entry.delay = 0;
			entry.trackTime = 0;
			entry.trackLast = -1;
			entry.nextTrackLast = -1;
			entry.trackEnd = Number.MAX_VALUE;
			entry.timeScale = 1;
			entry.alpha = 1;
			entry.interruptAlpha = 1;
			entry.mixTime = 0;
			entry.mixDuration = last == null ? 0 : this.data.getMix(last.animation, animation);
			entry.mixBlend = spine.MixBlend.replace;
			return entry;
		};
		AnimationState.prototype.disposeNext = function (entry) {
			var next = entry.next;
			while (next != null) {
				this.queue.dispose(next);
				next = next.next;
			}
			entry.next = null;
		};
		AnimationState.prototype._animationsChanged = function () {
			this.animationsChanged = false;
			this.propertyIDs.clear();
			for (var i = 0, n = this.tracks.length; i < n; i++) {
				var entry = this.tracks[i];
				if (entry == null)
					continue;
				while (entry.mixingFrom != null)
					entry = entry.mixingFrom;
				do {
					if (entry.mixingFrom == null || entry.mixBlend != spine.MixBlend.add)
						this.computeHold(entry);
					entry = entry.mixingTo;
				} while (entry != null);
			}
		};
		AnimationState.prototype.computeHold = function (entry) {
			var to = entry.mixingTo;
			var timelines = entry.animation.timelines;
			var timelinesCount = entry.animation.timelines.length;
			var timelineMode = spine.Utils.setArraySize(entry.timelineMode, timelinesCount);
			entry.timelineHoldMix.length = 0;
			var timelineDipMix = spine.Utils.setArraySize(entry.timelineHoldMix, timelinesCount);
			var propertyIDs = this.propertyIDs;
			if (to != null && to.holdPrevious) {
				for (var i = 0; i < timelinesCount; i++) {
					timelineMode[i] = propertyIDs.add(timelines[i].getPropertyId()) ? AnimationState.HOLD_FIRST : AnimationState.HOLD_SUBSEQUENT;
				}
				return;
			}
			outer: for (var i = 0; i < timelinesCount; i++) {
				var timeline = timelines[i];
				var id = timeline.getPropertyId();
				if (!propertyIDs.add(id))
					timelineMode[i] = AnimationState.SUBSEQUENT;
				else if (to == null || timeline instanceof spine.AttachmentTimeline || timeline instanceof spine.DrawOrderTimeline
					|| timeline instanceof spine.EventTimeline || !to.animation.hasTimeline(id)) {
					timelineMode[i] = AnimationState.FIRST;
				}
				else {
					for (var next = to.mixingTo; next != null; next = next.mixingTo) {
						if (next.animation.hasTimeline(id))
							continue;
						if (entry.mixDuration > 0) {
							timelineMode[i] = AnimationState.HOLD_MIX;
							timelineDipMix[i] = next;
							continue outer;
						}
						break;
					}
					timelineMode[i] = AnimationState.HOLD_FIRST;
				}
			}
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
		AnimationState.prototype.clearListenerNotifications = function () {
			this.queue.clear();
		};
		AnimationState.emptyAnimation = new spine.Animation("<empty>", [], 0);
		AnimationState.SUBSEQUENT = 0;
		AnimationState.FIRST = 1;
		AnimationState.HOLD_SUBSEQUENT = 2;
		AnimationState.HOLD_FIRST = 3;
		AnimationState.HOLD_MIX = 4;
		AnimationState.SETUP = 1;
		AnimationState.CURRENT = 2;
		return AnimationState;
	}());
	spine.AnimationState = AnimationState;
	var TrackEntry = (function () {
		function TrackEntry() {
			this.mixBlend = spine.MixBlend.replace;
			this.timelineMode = new Array();
			this.timelineHoldMix = new Array();
			this.timelinesRotation = new Array();
		}
		TrackEntry.prototype.reset = function () {
			this.next = null;
			this.mixingFrom = null;
			this.mixingTo = null;
			this.animation = null;
			this.listener = null;
			this.timelineMode.length = 0;
			this.timelineHoldMix.length = 0;
			this.timelinesRotation.length = 0;
		};
		TrackEntry.prototype.getAnimationTime = function () {
			if (this.loop) {
				var duration = this.animationEnd - this.animationStart;
				if (duration == 0)
					return this.animationStart;
				return (this.trackTime % duration) + this.animationStart;
			}
			return Math.min(this.trackTime + this.animationStart, this.animationEnd);
		};
		TrackEntry.prototype.setAnimationLast = function (animationLast) {
			this.animationLast = animationLast;
			this.nextAnimationLast = animationLast;
		};
		TrackEntry.prototype.isComplete = function () {
			return this.trackTime >= this.animationEnd - this.animationStart;
		};
		TrackEntry.prototype.resetRotationDirections = function () {
			this.timelinesRotation.length = 0;
		};
		return TrackEntry;
	}());
	spine.TrackEntry = TrackEntry;
	var EventQueue = (function () {
		function EventQueue(animState) {
			this.objects = [];
			this.drainDisabled = false;
			this.animState = animState;
		}
		EventQueue.prototype.start = function (entry) {
			this.objects.push(EventType.start);
			this.objects.push(entry);
			this.animState.animationsChanged = true;
		};
		EventQueue.prototype.interrupt = function (entry) {
			this.objects.push(EventType.interrupt);
			this.objects.push(entry);
		};
		EventQueue.prototype.end = function (entry) {
			this.objects.push(EventType.end);
			this.objects.push(entry);
			this.animState.animationsChanged = true;
		};
		EventQueue.prototype.dispose = function (entry) {
			this.objects.push(EventType.dispose);
			this.objects.push(entry);
		};
		EventQueue.prototype.complete = function (entry) {
			this.objects.push(EventType.complete);
			this.objects.push(entry);
		};
		EventQueue.prototype.event = function (entry, event) {
			this.objects.push(EventType.event);
			this.objects.push(entry);
			this.objects.push(event);
		};
		EventQueue.prototype.drain = function () {
			if (this.drainDisabled)
				return;
			this.drainDisabled = true;
			var objects = this.objects;
			var listeners = this.animState.listeners;
			for (var i = 0; i < objects.length; i += 2) {
				var type = objects[i];
				var entry = objects[i + 1];
				switch (type) {
					case EventType.start:
						if (entry.listener != null && entry.listener.start)
							entry.listener.start(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].start)
								listeners[ii].start(entry);
						break;
					case EventType.interrupt:
						if (entry.listener != null && entry.listener.interrupt)
							entry.listener.interrupt(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].interrupt)
								listeners[ii].interrupt(entry);
						break;
					case EventType.end:
						if (entry.listener != null && entry.listener.end)
							entry.listener.end(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].end)
								listeners[ii].end(entry);
					case EventType.dispose:
						if (entry.listener != null && entry.listener.dispose)
							entry.listener.dispose(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].dispose)
								listeners[ii].dispose(entry);
						this.animState.trackEntryPool.free(entry);
						break;
					case EventType.complete:
						if (entry.listener != null && entry.listener.complete)
							entry.listener.complete(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].complete)
								listeners[ii].complete(entry);
						break;
					case EventType.event:
						var event_3 = objects[i++ + 2];
						if (entry.listener != null && entry.listener.event)
							entry.listener.event(entry, event_3);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].event)
								listeners[ii].event(entry, event_3);
						break;
				}
			}
			this.clear();
			this.drainDisabled = false;
		};
		EventQueue.prototype.clear = function () {
			this.objects.length = 0;
		};
		return EventQueue;
	}());
	spine.EventQueue = EventQueue;
	var EventType;
	(function (EventType) {
		EventType[EventType["start"] = 0] = "start";
		EventType[EventType["interrupt"] = 1] = "interrupt";
		EventType[EventType["end"] = 2] = "end";
		EventType[EventType["dispose"] = 3] = "dispose";
		EventType[EventType["complete"] = 4] = "complete";
		EventType[EventType["event"] = 5] = "event";
	})(EventType = spine.EventType || (spine.EventType = {}));
	var AnimationStateAdapter = (function () {
		function AnimationStateAdapter() {
		}
		AnimationStateAdapter.prototype.start = function (entry) {
		};
		AnimationStateAdapter.prototype.interrupt = function (entry) {
		};
		AnimationStateAdapter.prototype.end = function (entry) {
		};
		AnimationStateAdapter.prototype.dispose = function (entry) {
		};
		AnimationStateAdapter.prototype.complete = function (entry) {
		};
		AnimationStateAdapter.prototype.event = function (entry, event) {
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
			var key = from.name + "." + to.name;
			this.animationToMixTime[key] = duration;
		};
		AnimationStateData.prototype.getMix = function (from, to) {
			var key = from.name + "." + to.name;
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
			this.rawDataUris = {};
			this.textureLoader = textureLoader;
			this.pathPrefix = pathPrefix;
		}
		AssetManager.prototype.downloadText = function (url, success, error) {
			var request = new XMLHttpRequest();
			request.overrideMimeType("text/html");
			if (this.rawDataUris[url])
				url = this.rawDataUris[url];
			request.open("GET", url, true);
			request.onload = function () {
				if (request.status == 200) {
					success(request.responseText);
				}
				else {
					error(request.status, request.responseText);
				}
			};
			request.onerror = function () {
				error(request.status, request.responseText);
			};
			request.send();
		};
		AssetManager.prototype.downloadBinary = function (url, success, error) {
			var request = new XMLHttpRequest();
			if (this.rawDataUris[url])
				url = this.rawDataUris[url];
			request.open("GET", url, true);
			request.responseType = "arraybuffer";
			request.onload = function () {
				if (request.status == 200) {
					success(new Uint8Array(request.response));
				}
				else {
					error(request.status, request.responseText);
				}
			};
			request.onerror = function () {
				error(request.status, request.responseText);
			};
			request.send();
		};
		AssetManager.prototype.setRawDataURI = function (path, data) {
			this.rawDataUris[this.pathPrefix + path] = data;
		};
		AssetManager.prototype.loadBinary = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.pathPrefix + path;
			this.toLoad++;
			this.downloadBinary(path, function (data) {
				_this.assets[path] = data;
				if (success)
					success(path, data);
				_this.toLoad--;
				_this.loaded++;
			}, function (state, responseText) {
				_this.errors[path] = "Couldn't load binary " + path + ": status " + status + ", " + responseText;
				if (error)
					error(path, "Couldn't load binary " + path + ": status " + status + ", " + responseText);
				_this.toLoad--;
				_this.loaded++;
			});
		};
		AssetManager.prototype.loadText = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.pathPrefix + path;
			this.toLoad++;
			this.downloadText(path, function (data) {
				_this.assets[path] = data;
				if (success)
					success(path, data);
				_this.toLoad--;
				_this.loaded++;
			}, function (state, responseText) {
				_this.errors[path] = "Couldn't load text " + path + ": status " + status + ", " + responseText;
				if (error)
					error(path, "Couldn't load text " + path + ": status " + status + ", " + responseText);
				_this.toLoad--;
				_this.loaded++;
			});
		};
		AssetManager.prototype.loadTexture = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.pathPrefix + path;
			var storagePath = path;
			this.toLoad++;
			var img = new Image();
			img.crossOrigin = "anonymous";
			img.onload = function (ev) {
				var texture = _this.textureLoader(img);
				_this.assets[storagePath] = texture;
				_this.toLoad--;
				_this.loaded++;
				if (success)
					success(path, img);
			};
			img.onerror = function (ev) {
				_this.errors[path] = "Couldn't load image " + path;
				_this.toLoad--;
				_this.loaded++;
				if (error)
					error(path, "Couldn't load image " + path);
			};
			if (this.rawDataUris[path])
				path = this.rawDataUris[path];
			img.src = path;
		};
		AssetManager.prototype.loadTextureAtlas = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			var parent = path.lastIndexOf("/") >= 0 ? path.substring(0, path.lastIndexOf("/")) : "";
			path = this.pathPrefix + path;
			this.toLoad++;
			this.downloadText(path, function (atlasData) {
				var pagesLoaded = { count: 0 };
				var atlasPages = new Array();
				try {
					var atlas = new spine.TextureAtlas(atlasData, function (path) {
						atlasPages.push(parent == "" ? path : parent + "/" + path);
						var image = document.createElement("img");
						image.width = 16;
						image.height = 16;
						return new spine.FakeTexture(image);
					});
				}
				catch (e) {
					var ex = e;
					_this.errors[path] = "Couldn't load texture atlas " + path + ": " + ex.message;
					if (error)
						error(path, "Couldn't load texture atlas " + path + ": " + ex.message);
					_this.toLoad--;
					_this.loaded++;
					return;
				}
				var _loop_1 = function (atlasPage) {
					var pageLoadError = false;
					_this.loadTexture(atlasPage, function (imagePath, image) {
						pagesLoaded.count++;
						if (pagesLoaded.count == atlasPages.length) {
							if (!pageLoadError) {
								try {
									var atlas = new spine.TextureAtlas(atlasData, function (path) {
										return _this.get(parent == "" ? path : parent + "/" + path);
									});
									_this.assets[path] = atlas;
									if (success)
										success(path, atlas);
									_this.toLoad--;
									_this.loaded++;
								}
								catch (e) {
									var ex = e;
									_this.errors[path] = "Couldn't load texture atlas " + path + ": " + ex.message;
									if (error)
										error(path, "Couldn't load texture atlas " + path + ": " + ex.message);
									_this.toLoad--;
									_this.loaded++;
								}
							}
							else {
								_this.errors[path] = "Couldn't load texture atlas page " + imagePath + "} of atlas " + path;
								if (error)
									error(path, "Couldn't load texture atlas page " + imagePath + " of atlas " + path);
								_this.toLoad--;
								_this.loaded++;
							}
						}
					}, function (imagePath, errorMessage) {
						pageLoadError = true;
						pagesLoaded.count++;
						if (pagesLoaded.count == atlasPages.length) {
							_this.errors[path] = "Couldn't load texture atlas page " + imagePath + "} of atlas " + path;
							if (error)
								error(path, "Couldn't load texture atlas page " + imagePath + " of atlas " + path);
							_this.toLoad--;
							_this.loaded++;
						}
					});
				};
				for (var _i = 0, atlasPages_1 = atlasPages; _i < atlasPages_1.length; _i++) {
					var atlasPage = atlasPages_1[_i];
					_loop_1(atlasPage);
				}
			}, function (state, responseText) {
				_this.errors[path] = "Couldn't load texture atlas " + path + ": status " + status + ", " + responseText;
				if (error)
					error(path, "Couldn't load texture atlas " + path + ": status " + status + ", " + responseText);
				_this.toLoad--;
				_this.loaded++;
			});
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
	var AtlasAttachmentLoader = (function () {
		function AtlasAttachmentLoader(atlas) {
			this.atlas = atlas;
		}
		AtlasAttachmentLoader.prototype.newRegionAttachment = function (skin, name, path) {
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			region.renderObject = region;
			var attachment = new spine.RegionAttachment(name);
			attachment.setRegion(region);
			return attachment;
		};
		AtlasAttachmentLoader.prototype.newMeshAttachment = function (skin, name, path) {
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new Error("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			region.renderObject = region;
			var attachment = new spine.MeshAttachment(name);
			attachment.region = region;
			return attachment;
		};
		AtlasAttachmentLoader.prototype.newBoundingBoxAttachment = function (skin, name) {
			return new spine.BoundingBoxAttachment(name);
		};
		AtlasAttachmentLoader.prototype.newPathAttachment = function (skin, name) {
			return new spine.PathAttachment(name);
		};
		AtlasAttachmentLoader.prototype.newPointAttachment = function (skin, name) {
			return new spine.PointAttachment(name);
		};
		AtlasAttachmentLoader.prototype.newClippingAttachment = function (skin, name) {
			return new spine.ClippingAttachment(name);
		};
		return AtlasAttachmentLoader;
	}());
	spine.AtlasAttachmentLoader = AtlasAttachmentLoader;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var BlendMode;
	(function (BlendMode) {
		BlendMode[BlendMode["Normal"] = 0] = "Normal";
		BlendMode[BlendMode["Additive"] = 1] = "Additive";
		BlendMode[BlendMode["Multiply"] = 2] = "Multiply";
		BlendMode[BlendMode["Screen"] = 3] = "Screen";
	})(BlendMode = spine.BlendMode || (spine.BlendMode = {}));
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
			this.ax = 0;
			this.ay = 0;
			this.arotation = 0;
			this.ascaleX = 0;
			this.ascaleY = 0;
			this.ashearX = 0;
			this.ashearY = 0;
			this.appliedValid = false;
			this.a = 0;
			this.b = 0;
			this.c = 0;
			this.d = 0;
			this.worldY = 0;
			this.worldX = 0;
			this.sorted = false;
			this.active = false;
			if (data == null)
				throw new Error("data cannot be null.");
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			this.setToSetupPose();
		}
		Bone.prototype.isActive = function () {
			return this.active;
		};
		Bone.prototype.update = function () {
			this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
		};
		Bone.prototype.updateWorldTransform = function () {
			this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
		};
		Bone.prototype.updateWorldTransformWith = function (x, y, rotation, scaleX, scaleY, shearX, shearY) {
			this.ax = x;
			this.ay = y;
			this.arotation = rotation;
			this.ascaleX = scaleX;
			this.ascaleY = scaleY;
			this.ashearX = shearX;
			this.ashearY = shearY;
			this.appliedValid = true;
			var parent = this.parent;
			if (parent == null) {
				var skeleton = this.skeleton;
				var rotationY = rotation + 90 + shearY;
				var sx = skeleton.scaleX;
				var sy = skeleton.scaleY;
				this.a = spine.MathUtils.cosDeg(rotation + shearX) * scaleX * sx;
				this.b = spine.MathUtils.cosDeg(rotationY) * scaleY * sx;
				this.c = spine.MathUtils.sinDeg(rotation + shearX) * scaleX * sy;
				this.d = spine.MathUtils.sinDeg(rotationY) * scaleY * sy;
				this.worldX = x * sx + skeleton.x;
				this.worldY = y * sy + skeleton.y;
				return;
			}
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			this.worldX = pa * x + pb * y + parent.worldX;
			this.worldY = pc * x + pd * y + parent.worldY;
			switch (this.data.transformMode) {
				case spine.TransformMode.Normal: {
					var rotationY = rotation + 90 + shearY;
					var la = spine.MathUtils.cosDeg(rotation + shearX) * scaleX;
					var lb = spine.MathUtils.cosDeg(rotationY) * scaleY;
					var lc = spine.MathUtils.sinDeg(rotation + shearX) * scaleX;
					var ld = spine.MathUtils.sinDeg(rotationY) * scaleY;
					this.a = pa * la + pb * lc;
					this.b = pa * lb + pb * ld;
					this.c = pc * la + pd * lc;
					this.d = pc * lb + pd * ld;
					return;
				}
				case spine.TransformMode.OnlyTranslation: {
					var rotationY = rotation + 90 + shearY;
					this.a = spine.MathUtils.cosDeg(rotation + shearX) * scaleX;
					this.b = spine.MathUtils.cosDeg(rotationY) * scaleY;
					this.c = spine.MathUtils.sinDeg(rotation + shearX) * scaleX;
					this.d = spine.MathUtils.sinDeg(rotationY) * scaleY;
					break;
				}
				case spine.TransformMode.NoRotationOrReflection: {
					var s = pa * pa + pc * pc;
					var prx = 0;
					if (s > 0.0001) {
						s = Math.abs(pa * pd - pb * pc) / s;
						pa /= this.skeleton.scaleX;
						pc /= this.skeleton.scaleY;
						pb = pc * s;
						pd = pa * s;
						prx = Math.atan2(pc, pa) * spine.MathUtils.radDeg;
					}
					else {
						pa = 0;
						pc = 0;
						prx = 90 - Math.atan2(pd, pb) * spine.MathUtils.radDeg;
					}
					var rx = rotation + shearX - prx;
					var ry = rotation + shearY - prx + 90;
					var la = spine.MathUtils.cosDeg(rx) * scaleX;
					var lb = spine.MathUtils.cosDeg(ry) * scaleY;
					var lc = spine.MathUtils.sinDeg(rx) * scaleX;
					var ld = spine.MathUtils.sinDeg(ry) * scaleY;
					this.a = pa * la - pb * lc;
					this.b = pa * lb - pb * ld;
					this.c = pc * la + pd * lc;
					this.d = pc * lb + pd * ld;
					break;
				}
				case spine.TransformMode.NoScale:
				case spine.TransformMode.NoScaleOrReflection: {
					var cos = spine.MathUtils.cosDeg(rotation);
					var sin = spine.MathUtils.sinDeg(rotation);
					var za = (pa * cos + pb * sin) / this.skeleton.scaleX;
					var zc = (pc * cos + pd * sin) / this.skeleton.scaleY;
					var s = Math.sqrt(za * za + zc * zc);
					if (s > 0.00001)
						s = 1 / s;
					za *= s;
					zc *= s;
					s = Math.sqrt(za * za + zc * zc);
					if (this.data.transformMode == spine.TransformMode.NoScale
						&& (pa * pd - pb * pc < 0) != (this.skeleton.scaleX < 0 != this.skeleton.scaleY < 0))
						s = -s;
					var r = Math.PI / 2 + Math.atan2(zc, za);
					var zb = Math.cos(r) * s;
					var zd = Math.sin(r) * s;
					var la = spine.MathUtils.cosDeg(shearX) * scaleX;
					var lb = spine.MathUtils.cosDeg(90 + shearY) * scaleY;
					var lc = spine.MathUtils.sinDeg(shearX) * scaleX;
					var ld = spine.MathUtils.sinDeg(90 + shearY) * scaleY;
					this.a = za * la + zb * lc;
					this.b = za * lb + zb * ld;
					this.c = zc * la + zd * lc;
					this.d = zc * lb + zd * ld;
					break;
				}
			}
			this.a *= this.skeleton.scaleX;
			this.b *= this.skeleton.scaleX;
			this.c *= this.skeleton.scaleY;
			this.d *= this.skeleton.scaleY;
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
			return Math.sqrt(this.a * this.a + this.c * this.c);
		};
		Bone.prototype.getWorldScaleY = function () {
			return Math.sqrt(this.b * this.b + this.d * this.d);
		};
		Bone.prototype.updateAppliedTransform = function () {
			this.appliedValid = true;
			var parent = this.parent;
			if (parent == null) {
				this.ax = this.worldX;
				this.ay = this.worldY;
				this.arotation = Math.atan2(this.c, this.a) * spine.MathUtils.radDeg;
				this.ascaleX = Math.sqrt(this.a * this.a + this.c * this.c);
				this.ascaleY = Math.sqrt(this.b * this.b + this.d * this.d);
				this.ashearX = 0;
				this.ashearY = Math.atan2(this.a * this.b + this.c * this.d, this.a * this.d - this.b * this.c) * spine.MathUtils.radDeg;
				return;
			}
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			var pid = 1 / (pa * pd - pb * pc);
			var dx = this.worldX - parent.worldX, dy = this.worldY - parent.worldY;
			this.ax = (dx * pd * pid - dy * pb * pid);
			this.ay = (dy * pa * pid - dx * pc * pid);
			var ia = pid * pd;
			var id = pid * pa;
			var ib = pid * pb;
			var ic = pid * pc;
			var ra = ia * this.a - ib * this.c;
			var rb = ia * this.b - ib * this.d;
			var rc = id * this.c - ic * this.a;
			var rd = id * this.d - ic * this.b;
			this.ashearX = 0;
			this.ascaleX = Math.sqrt(ra * ra + rc * rc);
			if (this.ascaleX > 0.0001) {
				var det = ra * rd - rb * rc;
				this.ascaleY = det / this.ascaleX;
				this.ashearY = Math.atan2(ra * rb + rc * rd, det) * spine.MathUtils.radDeg;
				this.arotation = Math.atan2(rc, ra) * spine.MathUtils.radDeg;
			}
			else {
				this.ascaleX = 0;
				this.ascaleY = Math.sqrt(rb * rb + rd * rd);
				this.ashearY = 0;
				this.arotation = 90 - Math.atan2(rd, rb) * spine.MathUtils.radDeg;
			}
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
		Bone.prototype.worldToLocalRotation = function (worldRotation) {
			var sin = spine.MathUtils.sinDeg(worldRotation), cos = spine.MathUtils.cosDeg(worldRotation);
			return Math.atan2(this.a * sin - this.c * cos, this.d * cos - this.b * sin) * spine.MathUtils.radDeg + this.rotation - this.shearX;
		};
		Bone.prototype.localToWorldRotation = function (localRotation) {
			localRotation -= this.rotation - this.shearX;
			var sin = spine.MathUtils.sinDeg(localRotation), cos = spine.MathUtils.cosDeg(localRotation);
			return Math.atan2(cos * this.c + sin * this.d, cos * this.a + sin * this.b) * spine.MathUtils.radDeg;
		};
		Bone.prototype.rotateWorld = function (degrees) {
			var a = this.a, b = this.b, c = this.c, d = this.d;
			var cos = spine.MathUtils.cosDeg(degrees), sin = spine.MathUtils.sinDeg(degrees);
			this.a = cos * a - sin * c;
			this.b = cos * b - sin * d;
			this.c = sin * a + cos * c;
			this.d = sin * b + cos * d;
			this.appliedValid = false;
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
			this.transformMode = TransformMode.Normal;
			this.skinRequired = false;
			this.color = new spine.Color();
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
	var TransformMode;
	(function (TransformMode) {
		TransformMode[TransformMode["Normal"] = 0] = "Normal";
		TransformMode[TransformMode["OnlyTranslation"] = 1] = "OnlyTranslation";
		TransformMode[TransformMode["NoRotationOrReflection"] = 2] = "NoRotationOrReflection";
		TransformMode[TransformMode["NoScale"] = 3] = "NoScale";
		TransformMode[TransformMode["NoScaleOrReflection"] = 4] = "NoScaleOrReflection";
	})(TransformMode = spine.TransformMode || (spine.TransformMode = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var ConstraintData = (function () {
		function ConstraintData(name, order, skinRequired) {
			this.name = name;
			this.order = order;
			this.skinRequired = skinRequired;
		}
		return ConstraintData;
	}());
	spine.ConstraintData = ConstraintData;
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
			this.bendDirection = 0;
			this.compress = false;
			this.stretch = false;
			this.mix = 1;
			this.softness = 0;
			this.active = false;
			if (data == null)
				throw new Error("data cannot be null.");
			if (skeleton == null)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.mix = data.mix;
			this.softness = data.softness;
			this.bendDirection = data.bendDirection;
			this.compress = data.compress;
			this.stretch = data.stretch;
			this.bones = new Array();
			for (var i = 0; i < data.bones.length; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findBone(data.target.name);
		}
		IkConstraint.prototype.isActive = function () {
			return this.active;
		};
		IkConstraint.prototype.apply = function () {
			this.update();
		};
		IkConstraint.prototype.update = function () {
			var target = this.target;
			var bones = this.bones;
			switch (bones.length) {
				case 1:
					this.apply1(bones[0], target.worldX, target.worldY, this.compress, this.stretch, this.data.uniform, this.mix);
					break;
				case 2:
					this.apply2(bones[0], bones[1], target.worldX, target.worldY, this.bendDirection, this.stretch, this.softness, this.mix);
					break;
			}
		};
		IkConstraint.prototype.apply1 = function (bone, targetX, targetY, compress, stretch, uniform, alpha) {
			if (!bone.appliedValid)
				bone.updateAppliedTransform();
			var p = bone.parent;
			var pa = p.a, pb = p.b, pc = p.c, pd = p.d;
			var rotationIK = -bone.ashearX - bone.arotation, tx = 0, ty = 0;
			switch (bone.data.transformMode) {
				case spine.TransformMode.OnlyTranslation:
					tx = targetX - bone.worldX;
					ty = targetY - bone.worldY;
					break;
				case spine.TransformMode.NoRotationOrReflection:
					var s = Math.abs(pa * pd - pb * pc) / (pa * pa + pc * pc);
					var sa = pa / bone.skeleton.scaleX;
					var sc = pc / bone.skeleton.scaleY;
					pb = -sc * s * bone.skeleton.scaleX;
					pd = sa * s * bone.skeleton.scaleY;
					rotationIK += Math.atan2(sc, sa) * spine.MathUtils.radDeg;
				default:
					var x = targetX - p.worldX, y = targetY - p.worldY;
					var d = pa * pd - pb * pc;
					tx = (x * pd - y * pb) / d - bone.ax;
					ty = (y * pa - x * pc) / d - bone.ay;
			}
			rotationIK += Math.atan2(ty, tx) * spine.MathUtils.radDeg;
			if (bone.ascaleX < 0)
				rotationIK += 180;
			if (rotationIK > 180)
				rotationIK -= 360;
			else if (rotationIK < -180)
				rotationIK += 360;
			var sx = bone.ascaleX, sy = bone.ascaleY;
			if (compress || stretch) {
				switch (bone.data.transformMode) {
					case spine.TransformMode.NoScale:
					case spine.TransformMode.NoScaleOrReflection:
						tx = targetX - bone.worldX;
						ty = targetY - bone.worldY;
				}
				var b = bone.data.length * sx, dd = Math.sqrt(tx * tx + ty * ty);
				if ((compress && dd < b) || (stretch && dd > b) && b > 0.0001) {
					var s = (dd / b - 1) * alpha + 1;
					sx *= s;
					if (uniform)
						sy *= s;
				}
			}
			bone.updateWorldTransformWith(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, sx, sy, bone.ashearX, bone.ashearY);
		};
		IkConstraint.prototype.apply2 = function (parent, child, targetX, targetY, bendDir, stretch, softness, alpha) {
			if (alpha == 0) {
				child.updateWorldTransform();
				return;
			}
			if (!parent.appliedValid)
				parent.updateAppliedTransform();
			if (!child.appliedValid)
				child.updateAppliedTransform();
			var px = parent.ax, py = parent.ay, psx = parent.ascaleX, sx = psx, psy = parent.ascaleY, csx = child.ascaleX;
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
			var cx = child.ax, cy = 0, cwx = 0, cwy = 0, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
			var u = Math.abs(psx - psy) <= 0.0001;
			if (!u) {
				cy = 0;
				cwx = a * cx + parent.worldX;
				cwy = c * cx + parent.worldY;
			}
			else {
				cy = child.ay;
				cwx = a * cx + b * cy + parent.worldX;
				cwy = c * cx + d * cy + parent.worldY;
			}
			var pp = parent.parent;
			a = pp.a;
			b = pp.b;
			c = pp.c;
			d = pp.d;
			var id = 1 / (a * d - b * c), x = cwx - pp.worldX, y = cwy - pp.worldY;
			var dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
			var l1 = Math.sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1, a2;
			if (l1 < 0.0001) {
				this.apply1(parent, targetX, targetY, false, stretch, false, alpha);
				child.updateWorldTransformWith(cx, cy, 0, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
				return;
			}
			x = targetX - pp.worldX;
			y = targetY - pp.worldY;
			var tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
			var dd = tx * tx + ty * ty;
			if (softness != 0) {
				softness *= psx * (csx + 1) / 2;
				var td = Math.sqrt(dd), sd = td - l1 - l2 * psx + softness;
				if (sd > 0) {
					var p = Math.min(1, sd / (softness * 2)) - 1;
					p = (sd - softness * (1 - p * p)) / td;
					tx -= p * tx;
					ty -= p * ty;
					dd = tx * tx + ty * ty;
				}
			}
			outer: if (u) {
				l2 *= psx;
				var cos = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
				if (cos < -1)
					cos = -1;
				else if (cos > 1) {
					cos = 1;
					if (stretch)
						sx *= (Math.sqrt(dd) / (l1 + l2) - 1) * alpha + 1;
				}
				a2 = Math.acos(cos) * bendDir;
				a = l1 + l2 * cos;
				b = l2 * Math.sin(a2);
				a1 = Math.atan2(ty * a - tx * b, tx * a + ty * b);
			}
			else {
				a = psx * l2;
				b = psy * l2;
				var aa = a * a, bb = b * b, ta = Math.atan2(ty, tx);
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
				var minAngle = spine.MathUtils.PI, minX = l1 - a, minDist = minX * minX, minY = 0;
				var maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
				c = -a * l1 / (aa - bb);
				if (c >= -1 && c <= 1) {
					c = Math.acos(c);
					x = a * Math.cos(c) + l1;
					y = b * Math.sin(c);
					d = x * x + y * y;
					if (d < minDist) {
						minAngle = c;
						minDist = d;
						minX = x;
						minY = y;
					}
					if (d > maxDist) {
						maxAngle = c;
						maxDist = d;
						maxX = x;
						maxY = y;
					}
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
			var rotation = parent.arotation;
			a1 = (a1 - os) * spine.MathUtils.radDeg + os1 - rotation;
			if (a1 > 180)
				a1 -= 360;
			else if (a1 < -180)
				a1 += 360;
			parent.updateWorldTransformWith(px, py, rotation + a1 * alpha, sx, parent.ascaleY, 0, 0);
			rotation = child.arotation;
			a2 = ((a2 + os) * spine.MathUtils.radDeg - child.ashearX) * s2 + os2 - rotation;
			if (a2 > 180)
				a2 -= 360;
			else if (a2 < -180)
				a2 += 360;
			child.updateWorldTransformWith(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
		};
		return IkConstraint;
	}());
	spine.IkConstraint = IkConstraint;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var IkConstraintData = (function (_super) {
		__extends(IkConstraintData, _super);
		function IkConstraintData(name) {
			var _this = _super.call(this, name, 0, false) || this;
			_this.bones = new Array();
			_this.bendDirection = 1;
			_this.compress = false;
			_this.stretch = false;
			_this.uniform = false;
			_this.mix = 1;
			_this.softness = 0;
			return _this;
		}
		return IkConstraintData;
	}(spine.ConstraintData));
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
			this.active = false;
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
		PathConstraint.prototype.isActive = function () {
			return this.active;
		};
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
			var percentSpacing = data.spacingMode == spine.SpacingMode.Percent;
			var rotateMode = data.rotateMode;
			var tangents = rotateMode == spine.RotateMode.Tangent, scale = rotateMode == spine.RotateMode.ChainScale;
			var boneCount = this.bones.length, spacesCount = tangents ? boneCount : boneCount + 1;
			var bones = this.bones;
			var spaces = spine.Utils.setArraySize(this.spaces, spacesCount), lengths = null;
			var spacing = this.spacing;
			if (scale || !percentSpacing) {
				if (scale)
					lengths = spine.Utils.setArraySize(this.lengths, boneCount);
				var lengthSpacing = data.spacingMode == spine.SpacingMode.Length;
				for (var i = 0, n = spacesCount - 1; i < n;) {
					var bone = bones[i];
					var setupLength = bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (scale)
							lengths[i] = 0;
						spaces[++i] = 0;
					}
					else if (percentSpacing) {
						if (scale) {
							var x = setupLength * bone.a, y = setupLength * bone.c;
							var length_1 = Math.sqrt(x * x + y * y);
							lengths[i] = length_1;
						}
						spaces[++i] = spacing;
					}
					else {
						var x = setupLength * bone.a, y = setupLength * bone.c;
						var length_2 = Math.sqrt(x * x + y * y);
						if (scale)
							lengths[i] = length_2;
						spaces[++i] = (lengthSpacing ? setupLength + spacing : spacing) * length_2 / setupLength;
					}
				}
			}
			else {
				for (var i = 1; i < spacesCount; i++)
					spaces[i] = spacing;
			}
			var positions = this.computeWorldPositions(attachment, spacesCount, tangents, data.positionMode == spine.PositionMode.Percent, percentSpacing);
			var boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
			var tip = false;
			if (offsetRotation == 0)
				tip = rotateMode == spine.RotateMode.Chain;
			else {
				tip = false;
				var p = this.target.bone;
				offsetRotation *= p.a * p.d - p.b * p.c > 0 ? spine.MathUtils.degRad : -spine.MathUtils.degRad;
			}
			for (var i = 0, p = 3; i < boneCount; i++, p += 3) {
				var bone = bones[i];
				bone.worldX += (boneX - bone.worldX) * translateMix;
				bone.worldY += (boneY - bone.worldY) * translateMix;
				var x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
				if (scale) {
					var length_3 = lengths[i];
					if (length_3 != 0) {
						var s = (Math.sqrt(dx * dx + dy * dy) / length_3 - 1) * rotateMix + 1;
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
					r -= Math.atan2(c, a);
					if (tip) {
						cos = Math.cos(r);
						sin = Math.sin(r);
						var length_4 = bone.data.length;
						boneX += (length_4 * (cos * a - sin * c) - dx) * rotateMix;
						boneY += (length_4 * (sin * a + cos * c) - dy) * rotateMix;
					}
					else {
						r += offsetRotation;
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
				bone.appliedValid = false;
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
					for (var i = 1; i < spacesCount; i++)
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
							path.computeWorldVertices(target, 2, 4, world, 0, 2);
						}
						this.addBeforePosition(p, world, 0, out, o);
						continue;
					}
					else if (p > pathLength_1) {
						if (prevCurve != PathConstraint.AFTER) {
							prevCurve = PathConstraint.AFTER;
							path.computeWorldVertices(target, verticesLength - 6, 4, world, 0, 2);
						}
						this.addAfterPosition(p - pathLength_1, world, 0, out, o);
						continue;
					}
					for (;; curve++) {
						var length_5 = lengths[curve];
						if (p > length_5)
							continue;
						if (curve == 0)
							p /= length_5;
						else {
							var prev = lengths[curve - 1];
							p = (p - prev) / (length_5 - prev);
						}
						break;
					}
					if (curve != prevCurve) {
						prevCurve = curve;
						if (closed && curve == curveCount) {
							path.computeWorldVertices(target, verticesLength - 4, 4, world, 0, 2);
							path.computeWorldVertices(target, 0, 4, world, 4, 2);
						}
						else
							path.computeWorldVertices(target, curve * 6 + 2, 8, world, 0, 2);
					}
					this.addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o, tangents || (i > 0 && space == 0));
				}
				return out;
			}
			if (closed) {
				verticesLength += 2;
				world = spine.Utils.setArraySize(this.world, verticesLength);
				path.computeWorldVertices(target, 2, verticesLength - 4, world, 0, 2);
				path.computeWorldVertices(target, 0, 2, world, verticesLength - 4, 2);
				world[verticesLength - 2] = world[0];
				world[verticesLength - 1] = world[1];
			}
			else {
				curveCount--;
				verticesLength -= 4;
				world = spine.Utils.setArraySize(this.world, verticesLength);
				path.computeWorldVertices(target, 2, verticesLength, world, 0, 2);
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
			else
				position *= pathLength / path.lengths[curveCount - 1];
			if (percentSpacing) {
				for (var i = 1; i < spacesCount; i++)
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
					var length_6 = curves[curve];
					if (p > length_6)
						continue;
					if (curve == 0)
						p /= length_6;
					else {
						var prev = curves[curve - 1];
						p = (p - prev) / (length_6 - prev);
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
					var length_7 = segments[segment];
					if (p > length_7)
						continue;
					if (segment == 0)
						p /= length_7;
					else {
						var prev = segments[segment - 1];
						p = segment + (p - prev) / (length_7 - prev);
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
			if (p == 0 || isNaN(p)) {
				out[o] = x1;
				out[o + 1] = y1;
				out[o + 2] = Math.atan2(cy1 - y1, cx1 - x1);
				return;
			}
			var tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
			var ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
			var x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
			out[o] = x;
			out[o + 1] = y;
			if (tangents) {
				if (p < 0.001)
					out[o + 2] = Math.atan2(cy1 - y1, cx1 - x1);
				else
					out[o + 2] = Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
			}
		};
		PathConstraint.NONE = -1;
		PathConstraint.BEFORE = -2;
		PathConstraint.AFTER = -3;
		PathConstraint.epsilon = 0.00001;
		return PathConstraint;
	}());
	spine.PathConstraint = PathConstraint;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var PathConstraintData = (function (_super) {
		__extends(PathConstraintData, _super);
		function PathConstraintData(name) {
			var _this = _super.call(this, name, 0, false) || this;
			_this.bones = new Array();
			return _this;
		}
		return PathConstraintData;
	}(spine.ConstraintData));
	spine.PathConstraintData = PathConstraintData;
	var PositionMode;
	(function (PositionMode) {
		PositionMode[PositionMode["Fixed"] = 0] = "Fixed";
		PositionMode[PositionMode["Percent"] = 1] = "Percent";
	})(PositionMode = spine.PositionMode || (spine.PositionMode = {}));
	var SpacingMode;
	(function (SpacingMode) {
		SpacingMode[SpacingMode["Length"] = 0] = "Length";
		SpacingMode[SpacingMode["Fixed"] = 1] = "Fixed";
		SpacingMode[SpacingMode["Percent"] = 2] = "Percent";
	})(SpacingMode = spine.SpacingMode || (spine.SpacingMode = {}));
	var RotateMode;
	(function (RotateMode) {
		RotateMode[RotateMode["Tangent"] = 0] = "Tangent";
		RotateMode[RotateMode["Chain"] = 1] = "Chain";
		RotateMode[RotateMode["ChainScale"] = 2] = "ChainScale";
	})(RotateMode = spine.RotateMode || (spine.RotateMode = {}));
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
			request.overrideMimeType("text/html");
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
			request.overrideMimeType("text/html");
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
			img.crossOrigin = "anonymous";
			img.onload = function (ev) {
				_this.rawAssets[path] = img;
			};
			img.onerror = function (ev) {
				_this.errors[path] = "Couldn't load image " + path;
			};
			img.src = path;
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
			this.updateCacheReset = new Array();
			this.time = 0;
			this.scaleX = 1;
			this.scaleY = 1;
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
			this.updateCacheReset.length = 0;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				bone.sorted = bone.data.skinRequired;
				bone.active = !bone.sorted;
			}
			if (this.skin != null) {
				var skinBones = this.skin.bones;
				for (var i = 0, n = this.skin.bones.length; i < n; i++) {
					var bone = this.bones[skinBones[i].index];
					do {
						bone.sorted = false;
						bone.active = true;
						bone = bone.parent;
					} while (bone != null);
				}
			}
			var ikConstraints = this.ikConstraints;
			var transformConstraints = this.transformConstraints;
			var pathConstraints = this.pathConstraints;
			var ikCount = ikConstraints.length, transformCount = transformConstraints.length, pathCount = pathConstraints.length;
			var constraintCount = ikCount + transformCount + pathCount;
			outer: for (var i = 0; i < constraintCount; i++) {
				for (var ii = 0; ii < ikCount; ii++) {
					var constraint = ikConstraints[ii];
					if (constraint.data.order == i) {
						this.sortIkConstraint(constraint);
						continue outer;
					}
				}
				for (var ii = 0; ii < transformCount; ii++) {
					var constraint = transformConstraints[ii];
					if (constraint.data.order == i) {
						this.sortTransformConstraint(constraint);
						continue outer;
					}
				}
				for (var ii = 0; ii < pathCount; ii++) {
					var constraint = pathConstraints[ii];
					if (constraint.data.order == i) {
						this.sortPathConstraint(constraint);
						continue outer;
					}
				}
			}
			for (var i = 0, n = bones.length; i < n; i++)
				this.sortBone(bones[i]);
		};
		Skeleton.prototype.sortIkConstraint = function (constraint) {
			constraint.active = constraint.target.isActive() && (!constraint.data.skinRequired || (this.skin != null && spine.Utils.contains(this.skin.constraints, constraint.data, true)));
			if (!constraint.active)
				return;
			var target = constraint.target;
			this.sortBone(target);
			var constrained = constraint.bones;
			var parent = constrained[0];
			this.sortBone(parent);
			if (constrained.length > 1) {
				var child = constrained[constrained.length - 1];
				if (!(this._updateCache.indexOf(child) > -1))
					this.updateCacheReset.push(child);
			}
			this._updateCache.push(constraint);
			this.sortReset(parent.children);
			constrained[constrained.length - 1].sorted = true;
		};
		Skeleton.prototype.sortPathConstraint = function (constraint) {
			constraint.active = constraint.target.bone.isActive() && (!constraint.data.skinRequired || (this.skin != null && spine.Utils.contains(this.skin.constraints, constraint.data, true)));
			if (!constraint.active)
				return;
			var slot = constraint.target;
			var slotIndex = slot.data.index;
			var slotBone = slot.bone;
			if (this.skin != null)
				this.sortPathConstraintAttachment(this.skin, slotIndex, slotBone);
			if (this.data.defaultSkin != null && this.data.defaultSkin != this.skin)
				this.sortPathConstraintAttachment(this.data.defaultSkin, slotIndex, slotBone);
			for (var i = 0, n = this.data.skins.length; i < n; i++)
				this.sortPathConstraintAttachment(this.data.skins[i], slotIndex, slotBone);
			var attachment = slot.getAttachment();
			if (attachment instanceof spine.PathAttachment)
				this.sortPathConstraintAttachmentWith(attachment, slotBone);
			var constrained = constraint.bones;
			var boneCount = constrained.length;
			for (var i = 0; i < boneCount; i++)
				this.sortBone(constrained[i]);
			this._updateCache.push(constraint);
			for (var i = 0; i < boneCount; i++)
				this.sortReset(constrained[i].children);
			for (var i = 0; i < boneCount; i++)
				constrained[i].sorted = true;
		};
		Skeleton.prototype.sortTransformConstraint = function (constraint) {
			constraint.active = constraint.target.isActive() && (!constraint.data.skinRequired || (this.skin != null && spine.Utils.contains(this.skin.constraints, constraint.data, true)));
			if (!constraint.active)
				return;
			this.sortBone(constraint.target);
			var constrained = constraint.bones;
			var boneCount = constrained.length;
			if (constraint.data.local) {
				for (var i = 0; i < boneCount; i++) {
					var child = constrained[i];
					this.sortBone(child.parent);
					if (!(this._updateCache.indexOf(child) > -1))
						this.updateCacheReset.push(child);
				}
			}
			else {
				for (var i = 0; i < boneCount; i++) {
					this.sortBone(constrained[i]);
				}
			}
			this._updateCache.push(constraint);
			for (var ii = 0; ii < boneCount; ii++)
				this.sortReset(constrained[ii].children);
			for (var ii = 0; ii < boneCount; ii++)
				constrained[ii].sorted = true;
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
				var i = 0;
				while (i < pathBones.length) {
					var boneCount = pathBones[i++];
					for (var n = i + boneCount; i < n; i++) {
						var boneIndex = pathBones[i];
						this.sortBone(bones[boneIndex]);
					}
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
				if (!bone.active)
					continue;
				if (bone.sorted)
					this.sortReset(bone.children);
				bone.sorted = false;
			}
		};
		Skeleton.prototype.updateWorldTransform = function () {
			var updateCacheReset = this.updateCacheReset;
			for (var i = 0, n = updateCacheReset.length; i < n; i++) {
				var bone = updateCacheReset[i];
				bone.ax = bone.x;
				bone.ay = bone.y;
				bone.arotation = bone.rotation;
				bone.ascaleX = bone.scaleX;
				bone.ascaleY = bone.scaleY;
				bone.ashearX = bone.shearX;
				bone.ashearY = bone.shearY;
				bone.appliedValid = true;
			}
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
				constraint.mix = constraint.data.mix;
				constraint.softness = constraint.data.softness;
				constraint.bendDirection = constraint.data.bendDirection;
				constraint.compress = constraint.data.compress;
				constraint.stretch = constraint.data.stretch;
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
			if (newSkin == this.skin)
				return;
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
			this.updateCache();
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
		Skeleton.prototype.getBounds = function (offset, size, temp) {
			if (temp === void 0) { temp = new Array(2); }
			if (offset == null)
				throw new Error("offset cannot be null.");
			if (size == null)
				throw new Error("size cannot be null.");
			var drawOrder = this.drawOrder;
			var minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
			for (var i = 0, n = drawOrder.length; i < n; i++) {
				var slot = drawOrder[i];
				if (!slot.bone.active)
					continue;
				var verticesLength = 0;
				var vertices = null;
				var attachment = slot.getAttachment();
				if (attachment instanceof spine.RegionAttachment) {
					verticesLength = 8;
					vertices = spine.Utils.setArraySize(temp, verticesLength, 0);
					attachment.computeWorldVertices(slot.bone, vertices, 0, 2);
				}
				else if (attachment instanceof spine.MeshAttachment) {
					var mesh = attachment;
					verticesLength = mesh.worldVerticesLength;
					vertices = spine.Utils.setArraySize(temp, verticesLength, 0);
					mesh.computeWorldVertices(slot, 0, verticesLength, vertices, 0, 2);
				}
				if (vertices != null) {
					for (var ii = 0, nn = vertices.length; ii < nn; ii += 2) {
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
	var SkeletonBinary = (function () {
		function SkeletonBinary(attachmentLoader) {
			this.scale = 1;
			this.linkedMeshes = new Array();
			this.attachmentLoader = attachmentLoader;
		}
		SkeletonBinary.prototype.readSkeletonData = function (binary) {
			var scale = this.scale;
			var skeletonData = new spine.SkeletonData();
			skeletonData.name = "";
			var input = new BinaryInput(binary);
			skeletonData.hash = input.readString();
			skeletonData.version = input.readString();
			if ("3.8.75" == skeletonData.version)
				throw new Error("Unsupported skeleton data, please export with a newer version of Spine.");
			skeletonData.x = input.readFloat();
			skeletonData.y = input.readFloat();
			skeletonData.width = input.readFloat();
			skeletonData.height = input.readFloat();
			var nonessential = input.readBoolean();
			if (nonessential) {
				skeletonData.fps = input.readFloat();
				skeletonData.imagesPath = input.readString();
				skeletonData.audioPath = input.readString();
			}
			var n = 0;
			n = input.readInt(true);
			for (var i = 0; i < n; i++)
				input.strings.push(input.readString());
			n = input.readInt(true);
			for (var i = 0; i < n; i++) {
				var name_2 = input.readString();
				var parent_2 = i == 0 ? null : skeletonData.bones[input.readInt(true)];
				var data = new spine.BoneData(i, name_2, parent_2);
				data.rotation = input.readFloat();
				data.x = input.readFloat() * scale;
				data.y = input.readFloat() * scale;
				data.scaleX = input.readFloat();
				data.scaleY = input.readFloat();
				data.shearX = input.readFloat();
				data.shearY = input.readFloat();
				data.length = input.readFloat() * scale;
				data.transformMode = SkeletonBinary.TransformModeValues[input.readInt(true)];
				data.skinRequired = input.readBoolean();
				if (nonessential)
					spine.Color.rgba8888ToColor(data.color, input.readInt32());
				skeletonData.bones.push(data);
			}
			n = input.readInt(true);
			for (var i = 0; i < n; i++) {
				var slotName = input.readString();
				var boneData = skeletonData.bones[input.readInt(true)];
				var data = new spine.SlotData(i, slotName, boneData);
				spine.Color.rgba8888ToColor(data.color, input.readInt32());
				var darkColor = input.readInt32();
				if (darkColor != -1)
					spine.Color.rgb888ToColor(data.darkColor = new spine.Color(), darkColor);
				data.attachmentName = input.readStringRef();
				data.blendMode = SkeletonBinary.BlendModeValues[input.readInt(true)];
				skeletonData.slots.push(data);
			}
			n = input.readInt(true);
			for (var i = 0, nn = void 0; i < n; i++) {
				var data = new spine.IkConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (var ii = 0; ii < nn; ii++)
					data.bones.push(skeletonData.bones[input.readInt(true)]);
				data.target = skeletonData.bones[input.readInt(true)];
				data.mix = input.readFloat();
				data.softness = input.readFloat() * scale;
				data.bendDirection = input.readByte();
				data.compress = input.readBoolean();
				data.stretch = input.readBoolean();
				data.uniform = input.readBoolean();
				skeletonData.ikConstraints.push(data);
			}
			n = input.readInt(true);
			for (var i = 0, nn = void 0; i < n; i++) {
				var data = new spine.TransformConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (var ii = 0; ii < nn; ii++)
					data.bones.push(skeletonData.bones[input.readInt(true)]);
				data.target = skeletonData.bones[input.readInt(true)];
				data.local = input.readBoolean();
				data.relative = input.readBoolean();
				data.offsetRotation = input.readFloat();
				data.offsetX = input.readFloat() * scale;
				data.offsetY = input.readFloat() * scale;
				data.offsetScaleX = input.readFloat();
				data.offsetScaleY = input.readFloat();
				data.offsetShearY = input.readFloat();
				data.rotateMix = input.readFloat();
				data.translateMix = input.readFloat();
				data.scaleMix = input.readFloat();
				data.shearMix = input.readFloat();
				skeletonData.transformConstraints.push(data);
			}
			n = input.readInt(true);
			for (var i = 0, nn = void 0; i < n; i++) {
				var data = new spine.PathConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (var ii = 0; ii < nn; ii++)
					data.bones.push(skeletonData.bones[input.readInt(true)]);
				data.target = skeletonData.slots[input.readInt(true)];
				data.positionMode = SkeletonBinary.PositionModeValues[input.readInt(true)];
				data.spacingMode = SkeletonBinary.SpacingModeValues[input.readInt(true)];
				data.rotateMode = SkeletonBinary.RotateModeValues[input.readInt(true)];
				data.offsetRotation = input.readFloat();
				data.position = input.readFloat();
				if (data.positionMode == spine.PositionMode.Fixed)
					data.position *= scale;
				data.spacing = input.readFloat();
				if (data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed)
					data.spacing *= scale;
				data.rotateMix = input.readFloat();
				data.translateMix = input.readFloat();
				skeletonData.pathConstraints.push(data);
			}
			var defaultSkin = this.readSkin(input, skeletonData, true, nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.push(defaultSkin);
			}
			{
				var i = skeletonData.skins.length;
				spine.Utils.setArraySize(skeletonData.skins, n = i + input.readInt(true));
				for (; i < n; i++)
					skeletonData.skins[i] = this.readSkin(input, skeletonData, false, nonessential);
			}
			n = this.linkedMeshes.length;
			for (var i = 0; i < n; i++) {
				var linkedMesh = this.linkedMeshes[i];
				var skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null)
					throw new Error("Skin not found: " + linkedMesh.skin);
				var parent_3 = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent_3 == null)
					throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? parent_3 : linkedMesh.mesh;
				linkedMesh.mesh.setParentMesh(parent_3);
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;
			n = input.readInt(true);
			for (var i = 0; i < n; i++) {
				var data = new spine.EventData(input.readStringRef());
				data.intValue = input.readInt(false);
				data.floatValue = input.readFloat();
				data.stringValue = input.readString();
				data.audioPath = input.readString();
				if (data.audioPath != null) {
					data.volume = input.readFloat();
					data.balance = input.readFloat();
				}
				skeletonData.events.push(data);
			}
			n = input.readInt(true);
			for (var i = 0; i < n; i++)
				skeletonData.animations.push(this.readAnimation(input, input.readString(), skeletonData));
			return skeletonData;
		};
		SkeletonBinary.prototype.readSkin = function (input, skeletonData, defaultSkin, nonessential) {
			var skin = null;
			var slotCount = 0;
			if (defaultSkin) {
				slotCount = input.readInt(true);
				if (slotCount == 0)
					return null;
				skin = new spine.Skin("default");
			}
			else {
				skin = new spine.Skin(input.readStringRef());
				skin.bones.length = input.readInt(true);
				for (var i = 0, n = skin.bones.length; i < n; i++)
					skin.bones[i] = skeletonData.bones[input.readInt(true)];
				for (var i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.ikConstraints[input.readInt(true)]);
				for (var i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.transformConstraints[input.readInt(true)]);
				for (var i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.pathConstraints[input.readInt(true)]);
				slotCount = input.readInt(true);
			}
			for (var i = 0; i < slotCount; i++) {
				var slotIndex = input.readInt(true);
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var name_3 = input.readStringRef();
					var attachment = this.readAttachment(input, skeletonData, skin, slotIndex, name_3, nonessential);
					if (attachment != null)
						skin.setAttachment(slotIndex, name_3, attachment);
				}
			}
			return skin;
		};
		SkeletonBinary.prototype.readAttachment = function (input, skeletonData, skin, slotIndex, attachmentName, nonessential) {
			var scale = this.scale;
			var name = input.readStringRef();
			if (name == null)
				name = attachmentName;
			var typeIndex = input.readByte();
			var type = SkeletonBinary.AttachmentTypeValues[typeIndex];
			switch (type) {
				case spine.AttachmentType.Region: {
					var path = input.readStringRef();
					var rotation = input.readFloat();
					var x = input.readFloat();
					var y = input.readFloat();
					var scaleX = input.readFloat();
					var scaleY = input.readFloat();
					var width = input.readFloat();
					var height = input.readFloat();
					var color = input.readInt32();
					if (path == null)
						path = name;
					var region = this.attachmentLoader.newRegionAttachment(skin, name, path);
					if (region == null)
						return null;
					region.path = path;
					region.x = x * scale;
					region.y = y * scale;
					region.scaleX = scaleX;
					region.scaleY = scaleY;
					region.rotation = rotation;
					region.width = width * scale;
					region.height = height * scale;
					spine.Color.rgba8888ToColor(region.color, color);
					region.updateOffset();
					return region;
				}
				case spine.AttachmentType.BoundingBox: {
					var vertexCount = input.readInt(true);
					var vertices = this.readVertices(input, vertexCount);
					var color = nonessential ? input.readInt32() : 0;
					var box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
					if (box == null)
						return null;
					box.worldVerticesLength = vertexCount << 1;
					box.vertices = vertices.vertices;
					box.bones = vertices.bones;
					if (nonessential)
						spine.Color.rgba8888ToColor(box.color, color);
					return box;
				}
				case spine.AttachmentType.Mesh: {
					var path = input.readStringRef();
					var color = input.readInt32();
					var vertexCount = input.readInt(true);
					var uvs = this.readFloatArray(input, vertexCount << 1, 1);
					var triangles = this.readShortArray(input);
					var vertices = this.readVertices(input, vertexCount);
					var hullLength = input.readInt(true);
					var edges = null;
					var width = 0, height = 0;
					if (nonessential) {
						edges = this.readShortArray(input);
						width = input.readFloat();
						height = input.readFloat();
					}
					if (path == null)
						path = name;
					var mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (mesh == null)
						return null;
					mesh.path = path;
					spine.Color.rgba8888ToColor(mesh.color, color);
					mesh.bones = vertices.bones;
					mesh.vertices = vertices.vertices;
					mesh.worldVerticesLength = vertexCount << 1;
					mesh.triangles = triangles;
					mesh.regionUVs = uvs;
					mesh.updateUVs();
					mesh.hullLength = hullLength << 1;
					if (nonessential) {
						mesh.edges = edges;
						mesh.width = width * scale;
						mesh.height = height * scale;
					}
					return mesh;
				}
				case spine.AttachmentType.LinkedMesh: {
					var path = input.readStringRef();
					var color = input.readInt32();
					var skinName = input.readStringRef();
					var parent_4 = input.readStringRef();
					var inheritDeform = input.readBoolean();
					var width = 0, height = 0;
					if (nonessential) {
						width = input.readFloat();
						height = input.readFloat();
					}
					if (path == null)
						path = name;
					var mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (mesh == null)
						return null;
					mesh.path = path;
					spine.Color.rgba8888ToColor(mesh.color, color);
					if (nonessential) {
						mesh.width = width * scale;
						mesh.height = height * scale;
					}
					this.linkedMeshes.push(new LinkedMesh(mesh, skinName, slotIndex, parent_4, inheritDeform));
					return mesh;
				}
				case spine.AttachmentType.Path: {
					var closed_1 = input.readBoolean();
					var constantSpeed = input.readBoolean();
					var vertexCount = input.readInt(true);
					var vertices = this.readVertices(input, vertexCount);
					var lengths = spine.Utils.newArray(vertexCount / 3, 0);
					for (var i = 0, n = lengths.length; i < n; i++)
						lengths[i] = input.readFloat() * scale;
					var color = nonessential ? input.readInt32() : 0;
					var path = this.attachmentLoader.newPathAttachment(skin, name);
					if (path == null)
						return null;
					path.closed = closed_1;
					path.constantSpeed = constantSpeed;
					path.worldVerticesLength = vertexCount << 1;
					path.vertices = vertices.vertices;
					path.bones = vertices.bones;
					path.lengths = lengths;
					if (nonessential)
						spine.Color.rgba8888ToColor(path.color, color);
					return path;
				}
				case spine.AttachmentType.Point: {
					var rotation = input.readFloat();
					var x = input.readFloat();
					var y = input.readFloat();
					var color = nonessential ? input.readInt32() : 0;
					var point = this.attachmentLoader.newPointAttachment(skin, name);
					if (point == null)
						return null;
					point.x = x * scale;
					point.y = y * scale;
					point.rotation = rotation;
					if (nonessential)
						spine.Color.rgba8888ToColor(point.color, color);
					return point;
				}
				case spine.AttachmentType.Clipping: {
					var endSlotIndex = input.readInt(true);
					var vertexCount = input.readInt(true);
					var vertices = this.readVertices(input, vertexCount);
					var color = nonessential ? input.readInt32() : 0;
					var clip = this.attachmentLoader.newClippingAttachment(skin, name);
					if (clip == null)
						return null;
					clip.endSlot = skeletonData.slots[endSlotIndex];
					clip.worldVerticesLength = vertexCount << 1;
					clip.vertices = vertices.vertices;
					clip.bones = vertices.bones;
					if (nonessential)
						spine.Color.rgba8888ToColor(clip.color, color);
					return clip;
				}
			}
			return null;
		};
		SkeletonBinary.prototype.readVertices = function (input, vertexCount) {
			var verticesLength = vertexCount << 1;
			var vertices = new Vertices();
			var scale = this.scale;
			if (!input.readBoolean()) {
				vertices.vertices = this.readFloatArray(input, verticesLength, scale);
				return vertices;
			}
			var weights = new Array();
			var bonesArray = new Array();
			for (var i = 0; i < vertexCount; i++) {
				var boneCount = input.readInt(true);
				bonesArray.push(boneCount);
				for (var ii = 0; ii < boneCount; ii++) {
					bonesArray.push(input.readInt(true));
					weights.push(input.readFloat() * scale);
					weights.push(input.readFloat() * scale);
					weights.push(input.readFloat());
				}
			}
			vertices.vertices = spine.Utils.toFloatArray(weights);
			vertices.bones = bonesArray;
			return vertices;
		};
		SkeletonBinary.prototype.readFloatArray = function (input, n, scale) {
			var array = new Array(n);
			if (scale == 1) {
				for (var i = 0; i < n; i++)
					array[i] = input.readFloat();
			}
			else {
				for (var i = 0; i < n; i++)
					array[i] = input.readFloat() * scale;
			}
			return array;
		};
		SkeletonBinary.prototype.readShortArray = function (input) {
			var n = input.readInt(true);
			var array = new Array(n);
			for (var i = 0; i < n; i++)
				array[i] = input.readShort();
			return array;
		};
		SkeletonBinary.prototype.readAnimation = function (input, name, skeletonData) {
			var timelines = new Array();
			var scale = this.scale;
			var duration = 0;
			var tempColor1 = new spine.Color();
			var tempColor2 = new spine.Color();
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var slotIndex = input.readInt(true);
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var timelineType = input.readByte();
					var frameCount = input.readInt(true);
					switch (timelineType) {
						case SkeletonBinary.SLOT_ATTACHMENT: {
							var timeline = new spine.AttachmentTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
								timeline.setFrame(frameIndex, input.readFloat(), input.readStringRef());
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[frameCount - 1]);
							break;
						}
						case SkeletonBinary.SLOT_COLOR: {
							var timeline = new spine.ColorTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								var time = input.readFloat();
								spine.Color.rgba8888ToColor(tempColor1, input.readInt32());
								timeline.setFrame(frameIndex, time, tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a);
								if (frameIndex < frameCount - 1)
									this.readCurve(input, frameIndex, timeline);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.ColorTimeline.ENTRIES]);
							break;
						}
						case SkeletonBinary.SLOT_TWO_COLOR: {
							var timeline = new spine.TwoColorTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								var time = input.readFloat();
								spine.Color.rgba8888ToColor(tempColor1, input.readInt32());
								spine.Color.rgb888ToColor(tempColor2, input.readInt32());
								timeline.setFrame(frameIndex, time, tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a, tempColor2.r, tempColor2.g, tempColor2.b);
								if (frameIndex < frameCount - 1)
									this.readCurve(input, frameIndex, timeline);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.TwoColorTimeline.ENTRIES]);
							break;
						}
					}
				}
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var boneIndex = input.readInt(true);
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var timelineType = input.readByte();
					var frameCount = input.readInt(true);
					switch (timelineType) {
						case SkeletonBinary.BONE_ROTATE: {
							var timeline = new spine.RotateTimeline(frameCount);
							timeline.boneIndex = boneIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.setFrame(frameIndex, input.readFloat(), input.readFloat());
								if (frameIndex < frameCount - 1)
									this.readCurve(input, frameIndex, timeline);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.RotateTimeline.ENTRIES]);
							break;
						}
						case SkeletonBinary.BONE_TRANSLATE:
						case SkeletonBinary.BONE_SCALE:
						case SkeletonBinary.BONE_SHEAR: {
							var timeline = void 0;
							var timelineScale = 1;
							if (timelineType == SkeletonBinary.BONE_SCALE)
								timeline = new spine.ScaleTimeline(frameCount);
							else if (timelineType == SkeletonBinary.BONE_SHEAR)
								timeline = new spine.ShearTimeline(frameCount);
							else {
								timeline = new spine.TranslateTimeline(frameCount);
								timelineScale = scale;
							}
							timeline.boneIndex = boneIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale, input.readFloat() * timelineScale);
								if (frameIndex < frameCount - 1)
									this.readCurve(input, frameIndex, timeline);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.TranslateTimeline.ENTRIES]);
							break;
						}
					}
				}
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var index = input.readInt(true);
				var frameCount = input.readInt(true);
				var timeline = new spine.IkConstraintTimeline(frameCount);
				timeline.ikConstraintIndex = index;
				for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat() * scale, input.readByte(), input.readBoolean(), input.readBoolean());
					if (frameIndex < frameCount - 1)
						this.readCurve(input, frameIndex, timeline);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.IkConstraintTimeline.ENTRIES]);
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var index = input.readInt(true);
				var frameCount = input.readInt(true);
				var timeline = new spine.TransformConstraintTimeline(frameCount);
				timeline.transformConstraintIndex = index;
				for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat());
					if (frameIndex < frameCount - 1)
						this.readCurve(input, frameIndex, timeline);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.TransformConstraintTimeline.ENTRIES]);
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var index = input.readInt(true);
				var data = skeletonData.pathConstraints[index];
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var timelineType = input.readByte();
					var frameCount = input.readInt(true);
					switch (timelineType) {
						case SkeletonBinary.PATH_POSITION:
						case SkeletonBinary.PATH_SPACING: {
							var timeline = void 0;
							var timelineScale = 1;
							if (timelineType == SkeletonBinary.PATH_SPACING) {
								timeline = new spine.PathConstraintSpacingTimeline(frameCount);
								if (data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed)
									timelineScale = scale;
							}
							else {
								timeline = new spine.PathConstraintPositionTimeline(frameCount);
								if (data.positionMode == spine.PositionMode.Fixed)
									timelineScale = scale;
							}
							timeline.pathConstraintIndex = index;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale);
								if (frameIndex < frameCount - 1)
									this.readCurve(input, frameIndex, timeline);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.PathConstraintPositionTimeline.ENTRIES]);
							break;
						}
						case SkeletonBinary.PATH_MIX: {
							var timeline = new spine.PathConstraintMixTimeline(frameCount);
							timeline.pathConstraintIndex = index;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat());
								if (frameIndex < frameCount - 1)
									this.readCurve(input, frameIndex, timeline);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(frameCount - 1) * spine.PathConstraintMixTimeline.ENTRIES]);
							break;
						}
					}
				}
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var skin = skeletonData.skins[input.readInt(true)];
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var slotIndex = input.readInt(true);
					for (var iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
						var attachment = skin.getAttachment(slotIndex, input.readStringRef());
						var weighted = attachment.bones != null;
						var vertices = attachment.vertices;
						var deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;
						var frameCount = input.readInt(true);
						var timeline = new spine.DeformTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						timeline.attachment = attachment;
						for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							var time = input.readFloat();
							var deform = void 0;
							var end = input.readInt(true);
							if (end == 0)
								deform = weighted ? spine.Utils.newFloatArray(deformLength) : vertices;
							else {
								deform = spine.Utils.newFloatArray(deformLength);
								var start = input.readInt(true);
								end += start;
								if (scale == 1) {
									for (var v = start; v < end; v++)
										deform[v] = input.readFloat();
								}
								else {
									for (var v = start; v < end; v++)
										deform[v] = input.readFloat() * scale;
								}
								if (!weighted) {
									for (var v = 0, vn = deform.length; v < vn; v++)
										deform[v] += vertices[v];
								}
							}
							timeline.setFrame(frameIndex, time, deform);
							if (frameIndex < frameCount - 1)
								this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[frameCount - 1]);
					}
				}
			}
			var drawOrderCount = input.readInt(true);
			if (drawOrderCount > 0) {
				var timeline = new spine.DrawOrderTimeline(drawOrderCount);
				var slotCount = skeletonData.slots.length;
				for (var i = 0; i < drawOrderCount; i++) {
					var time = input.readFloat();
					var offsetCount = input.readInt(true);
					var drawOrder = spine.Utils.newArray(slotCount, 0);
					for (var ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					var unchanged = spine.Utils.newArray(slotCount - offsetCount, 0);
					var originalIndex = 0, unchangedIndex = 0;
					for (var ii = 0; ii < offsetCount; ii++) {
						var slotIndex = input.readInt(true);
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						drawOrder[originalIndex + input.readInt(true)] = originalIndex++;
					}
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					for (var ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1)
							drawOrder[ii] = unchanged[--unchangedIndex];
					timeline.setFrame(i, time, drawOrder);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[drawOrderCount - 1]);
			}
			var eventCount = input.readInt(true);
			if (eventCount > 0) {
				var timeline = new spine.EventTimeline(eventCount);
				for (var i = 0; i < eventCount; i++) {
					var time = input.readFloat();
					var eventData = skeletonData.events[input.readInt(true)];
					var event_4 = new spine.Event(time, eventData);
					event_4.intValue = input.readInt(false);
					event_4.floatValue = input.readFloat();
					event_4.stringValue = input.readBoolean() ? input.readString() : eventData.stringValue;
					if (event_4.data.audioPath != null) {
						event_4.volume = input.readFloat();
						event_4.balance = input.readFloat();
					}
					timeline.setFrame(i, event_4);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[eventCount - 1]);
			}
			return new spine.Animation(name, timelines, duration);
		};
		SkeletonBinary.prototype.readCurve = function (input, frameIndex, timeline) {
			switch (input.readByte()) {
				case SkeletonBinary.CURVE_STEPPED:
					timeline.setStepped(frameIndex);
					break;
				case SkeletonBinary.CURVE_BEZIER:
					this.setCurve(timeline, frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat());
					break;
			}
		};
		SkeletonBinary.prototype.setCurve = function (timeline, frameIndex, cx1, cy1, cx2, cy2) {
			timeline.setCurve(frameIndex, cx1, cy1, cx2, cy2);
		};
		SkeletonBinary.AttachmentTypeValues = [0, 1, 2, 3, 4, 5, 6];
		SkeletonBinary.TransformModeValues = [spine.TransformMode.Normal, spine.TransformMode.OnlyTranslation, spine.TransformMode.NoRotationOrReflection, spine.TransformMode.NoScale, spine.TransformMode.NoScaleOrReflection];
		SkeletonBinary.PositionModeValues = [spine.PositionMode.Fixed, spine.PositionMode.Percent];
		SkeletonBinary.SpacingModeValues = [spine.SpacingMode.Length, spine.SpacingMode.Fixed, spine.SpacingMode.Percent];
		SkeletonBinary.RotateModeValues = [spine.RotateMode.Tangent, spine.RotateMode.Chain, spine.RotateMode.ChainScale];
		SkeletonBinary.BlendModeValues = [spine.BlendMode.Normal, spine.BlendMode.Additive, spine.BlendMode.Multiply, spine.BlendMode.Screen];
		SkeletonBinary.BONE_ROTATE = 0;
		SkeletonBinary.BONE_TRANSLATE = 1;
		SkeletonBinary.BONE_SCALE = 2;
		SkeletonBinary.BONE_SHEAR = 3;
		SkeletonBinary.SLOT_ATTACHMENT = 0;
		SkeletonBinary.SLOT_COLOR = 1;
		SkeletonBinary.SLOT_TWO_COLOR = 2;
		SkeletonBinary.PATH_POSITION = 0;
		SkeletonBinary.PATH_SPACING = 1;
		SkeletonBinary.PATH_MIX = 2;
		SkeletonBinary.CURVE_LINEAR = 0;
		SkeletonBinary.CURVE_STEPPED = 1;
		SkeletonBinary.CURVE_BEZIER = 2;
		return SkeletonBinary;
	}());
	spine.SkeletonBinary = SkeletonBinary;
	var BinaryInput = (function () {
		function BinaryInput(data, strings, index, buffer) {
			if (strings === void 0) { strings = new Array(); }
			if (index === void 0) { index = 0; }
			if (buffer === void 0) { buffer = new DataView(data.buffer); }
			this.strings = strings;
			this.index = index;
			this.buffer = buffer;
		}
		BinaryInput.prototype.readByte = function () {
			return this.buffer.getInt8(this.index++);
		};
		BinaryInput.prototype.readShort = function () {
			var value = this.buffer.getInt16(this.index);
			this.index += 2;
			return value;
		};
		BinaryInput.prototype.readInt32 = function () {
			var value = this.buffer.getInt32(this.index);
			this.index += 4;
			return value;
		};
		BinaryInput.prototype.readInt = function (optimizePositive) {
			var b = this.readByte();
			var result = b & 0x7F;
			if ((b & 0x80) != 0) {
				b = this.readByte();
				result |= (b & 0x7F) << 7;
				if ((b & 0x80) != 0) {
					b = this.readByte();
					result |= (b & 0x7F) << 14;
					if ((b & 0x80) != 0) {
						b = this.readByte();
						result |= (b & 0x7F) << 21;
						if ((b & 0x80) != 0) {
							b = this.readByte();
							result |= (b & 0x7F) << 28;
						}
					}
				}
			}
			return optimizePositive ? result : ((result >>> 1) ^ -(result & 1));
		};
		BinaryInput.prototype.readStringRef = function () {
			var index = this.readInt(true);
			return index == 0 ? null : this.strings[index - 1];
		};
		BinaryInput.prototype.readString = function () {
			var byteCount = this.readInt(true);
			switch (byteCount) {
				case 0:
					return null;
				case 1:
					return "";
			}
			byteCount--;
			var chars = "";
			var charCount = 0;
			for (var i = 0; i < byteCount;) {
				var b = this.readByte();
				switch (b >> 4) {
					case 12:
					case 13:
						chars += String.fromCharCode(((b & 0x1F) << 6 | this.readByte() & 0x3F));
						i += 2;
						break;
					case 14:
						chars += String.fromCharCode(((b & 0x0F) << 12 | (this.readByte() & 0x3F) << 6 | this.readByte() & 0x3F));
						i += 3;
						break;
					default:
						chars += String.fromCharCode(b);
						i++;
				}
			}
			return chars;
		};
		BinaryInput.prototype.readFloat = function () {
			var value = this.buffer.getFloat32(this.index);
			this.index += 4;
			return value;
		};
		BinaryInput.prototype.readBoolean = function () {
			return this.readByte() != 0;
		};
		return BinaryInput;
	}());
	var LinkedMesh = (function () {
		function LinkedMesh(mesh, skin, slotIndex, parent, inheritDeform) {
			this.mesh = mesh;
			this.skin = skin;
			this.slotIndex = slotIndex;
			this.parent = parent;
			this.inheritDeform = inheritDeform;
		}
		return LinkedMesh;
	}());
	var Vertices = (function () {
		function Vertices(bones, vertices) {
			if (bones === void 0) { bones = null; }
			if (vertices === void 0) { vertices = null; }
			this.bones = bones;
			this.vertices = vertices;
		}
		return Vertices;
	}());
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
				if (!slot.bone.active)
					continue;
				var attachment = slot.getAttachment();
				if (attachment instanceof spine.BoundingBoxAttachment) {
					var boundingBox = attachment;
					boundingBoxes.push(boundingBox);
					var polygon = polygonPool.obtain();
					if (polygon.length != boundingBox.worldVerticesLength) {
						polygon = spine.Utils.newFloatArray(boundingBox.worldVerticesLength);
					}
					polygons.push(polygon);
					boundingBox.computeWorldVertices(slot, 0, boundingBox.worldVerticesLength, polygon, 0, 2);
				}
			}
			if (updateAabb) {
				this.aabbCompute();
			}
			else {
				this.minX = Number.POSITIVE_INFINITY;
				this.minY = Number.POSITIVE_INFINITY;
				this.maxX = Number.NEGATIVE_INFINITY;
				this.maxY = Number.NEGATIVE_INFINITY;
			}
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
	var SkeletonClipping = (function () {
		function SkeletonClipping() {
			this.triangulator = new spine.Triangulator();
			this.clippingPolygon = new Array();
			this.clipOutput = new Array();
			this.clippedVertices = new Array();
			this.clippedTriangles = new Array();
			this.scratch = new Array();
		}
		SkeletonClipping.prototype.clipStart = function (slot, clip) {
			if (this.clipAttachment != null)
				return 0;
			this.clipAttachment = clip;
			var n = clip.worldVerticesLength;
			var vertices = spine.Utils.setArraySize(this.clippingPolygon, n);
			clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
			var clippingPolygon = this.clippingPolygon;
			SkeletonClipping.makeClockwise(clippingPolygon);
			var clippingPolygons = this.clippingPolygons = this.triangulator.decompose(clippingPolygon, this.triangulator.triangulate(clippingPolygon));
			for (var i = 0, n_2 = clippingPolygons.length; i < n_2; i++) {
				var polygon = clippingPolygons[i];
				SkeletonClipping.makeClockwise(polygon);
				polygon.push(polygon[0]);
				polygon.push(polygon[1]);
			}
			return clippingPolygons.length;
		};
		SkeletonClipping.prototype.clipEndWithSlot = function (slot) {
			if (this.clipAttachment != null && this.clipAttachment.endSlot == slot.data)
				this.clipEnd();
		};
		SkeletonClipping.prototype.clipEnd = function () {
			if (this.clipAttachment == null)
				return;
			this.clipAttachment = null;
			this.clippingPolygons = null;
			this.clippedVertices.length = 0;
			this.clippedTriangles.length = 0;
			this.clippingPolygon.length = 0;
		};
		SkeletonClipping.prototype.isClipping = function () {
			return this.clipAttachment != null;
		};
		SkeletonClipping.prototype.clipTriangles = function (vertices, verticesLength, triangles, trianglesLength, uvs, light, dark, twoColor) {
			var clipOutput = this.clipOutput, clippedVertices = this.clippedVertices;
			var clippedTriangles = this.clippedTriangles;
			var polygons = this.clippingPolygons;
			var polygonsCount = this.clippingPolygons.length;
			var vertexSize = twoColor ? 12 : 8;
			var index = 0;
			clippedVertices.length = 0;
			clippedTriangles.length = 0;
			outer: for (var i = 0; i < trianglesLength; i += 3) {
				var vertexOffset = triangles[i] << 1;
				var x1 = vertices[vertexOffset], y1 = vertices[vertexOffset + 1];
				var u1 = uvs[vertexOffset], v1 = uvs[vertexOffset + 1];
				vertexOffset = triangles[i + 1] << 1;
				var x2 = vertices[vertexOffset], y2 = vertices[vertexOffset + 1];
				var u2 = uvs[vertexOffset], v2 = uvs[vertexOffset + 1];
				vertexOffset = triangles[i + 2] << 1;
				var x3 = vertices[vertexOffset], y3 = vertices[vertexOffset + 1];
				var u3 = uvs[vertexOffset], v3 = uvs[vertexOffset + 1];
				for (var p = 0; p < polygonsCount; p++) {
					var s = clippedVertices.length;
					if (this.clip(x1, y1, x2, y2, x3, y3, polygons[p], clipOutput)) {
						var clipOutputLength = clipOutput.length;
						if (clipOutputLength == 0)
							continue;
						var d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1;
						var d = 1 / (d0 * d2 + d1 * (y1 - y3));
						var clipOutputCount = clipOutputLength >> 1;
						var clipOutputItems = this.clipOutput;
						var clippedVerticesItems = spine.Utils.setArraySize(clippedVertices, s + clipOutputCount * vertexSize);
						for (var ii = 0; ii < clipOutputLength; ii += 2) {
							var x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
							clippedVerticesItems[s] = x;
							clippedVerticesItems[s + 1] = y;
							clippedVerticesItems[s + 2] = light.r;
							clippedVerticesItems[s + 3] = light.g;
							clippedVerticesItems[s + 4] = light.b;
							clippedVerticesItems[s + 5] = light.a;
							var c0 = x - x3, c1 = y - y3;
							var a = (d0 * c0 + d1 * c1) * d;
							var b = (d4 * c0 + d2 * c1) * d;
							var c = 1 - a - b;
							clippedVerticesItems[s + 6] = u1 * a + u2 * b + u3 * c;
							clippedVerticesItems[s + 7] = v1 * a + v2 * b + v3 * c;
							if (twoColor) {
								clippedVerticesItems[s + 8] = dark.r;
								clippedVerticesItems[s + 9] = dark.g;
								clippedVerticesItems[s + 10] = dark.b;
								clippedVerticesItems[s + 11] = dark.a;
							}
							s += vertexSize;
						}
						s = clippedTriangles.length;
						var clippedTrianglesItems = spine.Utils.setArraySize(clippedTriangles, s + 3 * (clipOutputCount - 2));
						clipOutputCount--;
						for (var ii = 1; ii < clipOutputCount; ii++) {
							clippedTrianglesItems[s] = index;
							clippedTrianglesItems[s + 1] = (index + ii);
							clippedTrianglesItems[s + 2] = (index + ii + 1);
							s += 3;
						}
						index += clipOutputCount + 1;
					}
					else {
						var clippedVerticesItems = spine.Utils.setArraySize(clippedVertices, s + 3 * vertexSize);
						clippedVerticesItems[s] = x1;
						clippedVerticesItems[s + 1] = y1;
						clippedVerticesItems[s + 2] = light.r;
						clippedVerticesItems[s + 3] = light.g;
						clippedVerticesItems[s + 4] = light.b;
						clippedVerticesItems[s + 5] = light.a;
						if (!twoColor) {
							clippedVerticesItems[s + 6] = u1;
							clippedVerticesItems[s + 7] = v1;
							clippedVerticesItems[s + 8] = x2;
							clippedVerticesItems[s + 9] = y2;
							clippedVerticesItems[s + 10] = light.r;
							clippedVerticesItems[s + 11] = light.g;
							clippedVerticesItems[s + 12] = light.b;
							clippedVerticesItems[s + 13] = light.a;
							clippedVerticesItems[s + 14] = u2;
							clippedVerticesItems[s + 15] = v2;
							clippedVerticesItems[s + 16] = x3;
							clippedVerticesItems[s + 17] = y3;
							clippedVerticesItems[s + 18] = light.r;
							clippedVerticesItems[s + 19] = light.g;
							clippedVerticesItems[s + 20] = light.b;
							clippedVerticesItems[s + 21] = light.a;
							clippedVerticesItems[s + 22] = u3;
							clippedVerticesItems[s + 23] = v3;
						}
						else {
							clippedVerticesItems[s + 6] = u1;
							clippedVerticesItems[s + 7] = v1;
							clippedVerticesItems[s + 8] = dark.r;
							clippedVerticesItems[s + 9] = dark.g;
							clippedVerticesItems[s + 10] = dark.b;
							clippedVerticesItems[s + 11] = dark.a;
							clippedVerticesItems[s + 12] = x2;
							clippedVerticesItems[s + 13] = y2;
							clippedVerticesItems[s + 14] = light.r;
							clippedVerticesItems[s + 15] = light.g;
							clippedVerticesItems[s + 16] = light.b;
							clippedVerticesItems[s + 17] = light.a;
							clippedVerticesItems[s + 18] = u2;
							clippedVerticesItems[s + 19] = v2;
							clippedVerticesItems[s + 20] = dark.r;
							clippedVerticesItems[s + 21] = dark.g;
							clippedVerticesItems[s + 22] = dark.b;
							clippedVerticesItems[s + 23] = dark.a;
							clippedVerticesItems[s + 24] = x3;
							clippedVerticesItems[s + 25] = y3;
							clippedVerticesItems[s + 26] = light.r;
							clippedVerticesItems[s + 27] = light.g;
							clippedVerticesItems[s + 28] = light.b;
							clippedVerticesItems[s + 29] = light.a;
							clippedVerticesItems[s + 30] = u3;
							clippedVerticesItems[s + 31] = v3;
							clippedVerticesItems[s + 32] = dark.r;
							clippedVerticesItems[s + 33] = dark.g;
							clippedVerticesItems[s + 34] = dark.b;
							clippedVerticesItems[s + 35] = dark.a;
						}
						s = clippedTriangles.length;
						var clippedTrianglesItems = spine.Utils.setArraySize(clippedTriangles, s + 3);
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (index + 1);
						clippedTrianglesItems[s + 2] = (index + 2);
						index += 3;
						continue outer;
					}
				}
			}
		};
		SkeletonClipping.prototype.clip = function (x1, y1, x2, y2, x3, y3, clippingArea, output) {
			var originalOutput = output;
			var clipped = false;
			var input = null;
			if (clippingArea.length % 4 >= 2) {
				input = output;
				output = this.scratch;
			}
			else
				input = this.scratch;
			input.length = 0;
			input.push(x1);
			input.push(y1);
			input.push(x2);
			input.push(y2);
			input.push(x3);
			input.push(y3);
			input.push(x1);
			input.push(y1);
			output.length = 0;
			var clippingVertices = clippingArea;
			var clippingVerticesLast = clippingArea.length - 4;
			for (var i = 0;; i += 2) {
				var edgeX = clippingVertices[i], edgeY = clippingVertices[i + 1];
				var edgeX2 = clippingVertices[i + 2], edgeY2 = clippingVertices[i + 3];
				var deltaX = edgeX - edgeX2, deltaY = edgeY - edgeY2;
				var inputVertices = input;
				var inputVerticesLength = input.length - 2, outputStart = output.length;
				for (var ii = 0; ii < inputVerticesLength; ii += 2) {
					var inputX = inputVertices[ii], inputY = inputVertices[ii + 1];
					var inputX2 = inputVertices[ii + 2], inputY2 = inputVertices[ii + 3];
					var side2 = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0;
					if (deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0) {
						if (side2) {
							output.push(inputX2);
							output.push(inputY2);
							continue;
						}
						var c0 = inputY2 - inputY, c2 = inputX2 - inputX;
						var s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
						if (Math.abs(s) > 0.000001) {
							var ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
							output.push(edgeX + (edgeX2 - edgeX) * ua);
							output.push(edgeY + (edgeY2 - edgeY) * ua);
						}
						else {
							output.push(edgeX);
							output.push(edgeY);
						}
					}
					else if (side2) {
						var c0 = inputY2 - inputY, c2 = inputX2 - inputX;
						var s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
						if (Math.abs(s) > 0.000001) {
							var ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
							output.push(edgeX + (edgeX2 - edgeX) * ua);
							output.push(edgeY + (edgeY2 - edgeY) * ua);
						}
						else {
							output.push(edgeX);
							output.push(edgeY);
						}
						output.push(inputX2);
						output.push(inputY2);
					}
					clipped = true;
				}
				if (outputStart == output.length) {
					originalOutput.length = 0;
					return true;
				}
				output.push(output[0]);
				output.push(output[1]);
				if (i == clippingVerticesLast)
					break;
				var temp = output;
				output = input;
				output.length = 0;
				input = temp;
			}
			if (originalOutput != output) {
				originalOutput.length = 0;
				for (var i = 0, n = output.length - 2; i < n; i++)
					originalOutput[i] = output[i];
			}
			else
				originalOutput.length = originalOutput.length - 2;
			return clipped;
		};
		SkeletonClipping.makeClockwise = function (polygon) {
			var vertices = polygon;
			var verticeslength = polygon.length;
			var area = vertices[verticeslength - 2] * vertices[1] - vertices[0] * vertices[verticeslength - 1], p1x = 0, p1y = 0, p2x = 0, p2y = 0;
			for (var i = 0, n = verticeslength - 3; i < n; i += 2) {
				p1x = vertices[i];
				p1y = vertices[i + 1];
				p2x = vertices[i + 2];
				p2y = vertices[i + 3];
				area += p1x * p2y - p2x * p1y;
			}
			if (area < 0)
				return;
			for (var i = 0, lastX = verticeslength - 2, n = verticeslength >> 1; i < n; i += 2) {
				var x = vertices[i], y = vertices[i + 1];
				var other = lastX - i;
				vertices[i] = vertices[other];
				vertices[i + 1] = vertices[other + 1];
				vertices[other] = x;
				vertices[other + 1] = y;
			}
		};
		return SkeletonClipping;
	}());
	spine.SkeletonClipping = SkeletonClipping;
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
			this.fps = 0;
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
				var event_5 = events[i];
				if (event_5.name == eventDataName)
					return event_5;
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
				if ("3.8.75" == skeletonData.version)
					throw new Error("Unsupported skeleton data, please export with a newer version of Spine.");
				skeletonData.x = skeletonMap.x;
				skeletonData.y = skeletonMap.y;
				skeletonData.width = skeletonMap.width;
				skeletonData.height = skeletonMap.height;
				skeletonData.fps = skeletonMap.fps;
				skeletonData.imagesPath = skeletonMap.images;
			}
			if (root.bones) {
				for (var i = 0; i < root.bones.length; i++) {
					var boneMap = root.bones[i];
					var parent_5 = null;
					var parentName = this.getValue(boneMap, "parent", null);
					if (parentName != null) {
						parent_5 = skeletonData.findBone(parentName);
						if (parent_5 == null)
							throw new Error("Parent bone not found: " + parentName);
					}
					var data = new spine.BoneData(skeletonData.bones.length, boneMap.name, parent_5);
					data.length = this.getValue(boneMap, "length", 0) * scale;
					data.x = this.getValue(boneMap, "x", 0) * scale;
					data.y = this.getValue(boneMap, "y", 0) * scale;
					data.rotation = this.getValue(boneMap, "rotation", 0);
					data.scaleX = this.getValue(boneMap, "scaleX", 1);
					data.scaleY = this.getValue(boneMap, "scaleY", 1);
					data.shearX = this.getValue(boneMap, "shearX", 0);
					data.shearY = this.getValue(boneMap, "shearY", 0);
					data.transformMode = SkeletonJson.transformModeFromString(this.getValue(boneMap, "transform", "normal"));
					data.skinRequired = this.getValue(boneMap, "skin", false);
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
					var dark = this.getValue(slotMap, "dark", null);
					if (dark != null) {
						data.darkColor = new spine.Color(1, 1, 1, 1);
						data.darkColor.setFromString(dark);
					}
					data.attachmentName = this.getValue(slotMap, "attachment", null);
					data.blendMode = SkeletonJson.blendModeFromString(this.getValue(slotMap, "blend", "normal"));
					skeletonData.slots.push(data);
				}
			}
			if (root.ik) {
				for (var i = 0; i < root.ik.length; i++) {
					var constraintMap = root.ik[i];
					var data = new spine.IkConstraintData(constraintMap.name);
					data.order = this.getValue(constraintMap, "order", 0);
					data.skinRequired = this.getValue(constraintMap, "skin", false);
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
					data.mix = this.getValue(constraintMap, "mix", 1);
					data.softness = this.getValue(constraintMap, "softness", 0) * scale;
					data.bendDirection = this.getValue(constraintMap, "bendPositive", true) ? 1 : -1;
					data.compress = this.getValue(constraintMap, "compress", false);
					data.stretch = this.getValue(constraintMap, "stretch", false);
					data.uniform = this.getValue(constraintMap, "uniform", false);
					skeletonData.ikConstraints.push(data);
				}
			}
			if (root.transform) {
				for (var i = 0; i < root.transform.length; i++) {
					var constraintMap = root.transform[i];
					var data = new spine.TransformConstraintData(constraintMap.name);
					data.order = this.getValue(constraintMap, "order", 0);
					data.skinRequired = this.getValue(constraintMap, "skin", false);
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
					data.local = this.getValue(constraintMap, "local", false);
					data.relative = this.getValue(constraintMap, "relative", false);
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
					data.order = this.getValue(constraintMap, "order", 0);
					data.skinRequired = this.getValue(constraintMap, "skin", false);
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
				for (var i = 0; i < root.skins.length; i++) {
					var skinMap = root.skins[i];
					var skin = new spine.Skin(skinMap.name);
					if (skinMap.bones) {
						for (var ii = 0; ii < skinMap.bones.length; ii++) {
							var bone = skeletonData.findBone(skinMap.bones[ii]);
							if (bone == null)
								throw new Error("Skin bone not found: " + skinMap.bones[i]);
							skin.bones.push(bone);
						}
					}
					if (skinMap.ik) {
						for (var ii = 0; ii < skinMap.ik.length; ii++) {
							var constraint = skeletonData.findIkConstraint(skinMap.ik[ii]);
							if (constraint == null)
								throw new Error("Skin IK constraint not found: " + skinMap.ik[i]);
							skin.constraints.push(constraint);
						}
					}
					if (skinMap.transform) {
						for (var ii = 0; ii < skinMap.transform.length; ii++) {
							var constraint = skeletonData.findTransformConstraint(skinMap.transform[ii]);
							if (constraint == null)
								throw new Error("Skin transform constraint not found: " + skinMap.transform[i]);
							skin.constraints.push(constraint);
						}
					}
					if (skinMap.path) {
						for (var ii = 0; ii < skinMap.path.length; ii++) {
							var constraint = skeletonData.findPathConstraint(skinMap.path[ii]);
							if (constraint == null)
								throw new Error("Skin path constraint not found: " + skinMap.path[i]);
							skin.constraints.push(constraint);
						}
					}
					for (var slotName in skinMap.attachments) {
						var slot = skeletonData.findSlot(slotName);
						if (slot == null)
							throw new Error("Slot not found: " + slotName);
						var slotMap = skinMap.attachments[slotName];
						for (var entryName in slotMap) {
							var attachment = this.readAttachment(slotMap[entryName], skin, slot.index, entryName, skeletonData);
							if (attachment != null)
								skin.setAttachment(slot.index, entryName, attachment);
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
				var parent_6 = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent_6 == null)
					throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? parent_6 : linkedMesh.mesh;
				linkedMesh.mesh.setParentMesh(parent_6);
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;
			if (root.events) {
				for (var eventName in root.events) {
					var eventMap = root.events[eventName];
					var data = new spine.EventData(eventName);
					data.intValue = this.getValue(eventMap, "int", 0);
					data.floatValue = this.getValue(eventMap, "float", 0);
					data.stringValue = this.getValue(eventMap, "string", "");
					data.audioPath = this.getValue(eventMap, "audio", null);
					if (data.audioPath != null) {
						data.volume = this.getValue(eventMap, "volume", 1);
						data.balance = this.getValue(eventMap, "balance", 0);
					}
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
		SkeletonJson.prototype.readAttachment = function (map, skin, slotIndex, name, skeletonData) {
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
					mesh.width = this.getValue(map, "width", 0) * scale;
					mesh.height = this.getValue(map, "height", 0) * scale;
					var parent_7 = this.getValue(map, "parent", null);
					if (parent_7 != null) {
						this.linkedMeshes.push(new LinkedMesh(mesh, this.getValue(map, "skin", null), slotIndex, parent_7, this.getValue(map, "deform", true)));
						return mesh;
					}
					var uvs = map.uvs;
					this.readVertices(map, mesh, uvs.length);
					mesh.triangles = map.triangles;
					mesh.regionUVs = uvs;
					mesh.updateUVs();
					mesh.edges = this.getValue(map, "edges", null);
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
						lengths[i] = map.lengths[i] * scale;
					path.lengths = lengths;
					var color = this.getValue(map, "color", null);
					if (color != null)
						path.color.setFromString(color);
					return path;
				}
				case "point": {
					var point = this.attachmentLoader.newPointAttachment(skin, name);
					if (point == null)
						return null;
					point.x = this.getValue(map, "x", 0) * scale;
					point.y = this.getValue(map, "y", 0) * scale;
					point.rotation = this.getValue(map, "rotation", 0);
					var color = this.getValue(map, "color", null);
					if (color != null)
						point.color.setFromString(color);
					return point;
				}
				case "clipping": {
					var clip = this.attachmentLoader.newClippingAttachment(skin, name);
					if (clip == null)
						return null;
					var end = this.getValue(map, "end", null);
					if (end != null) {
						var slot = skeletonData.findSlot(end);
						if (slot == null)
							throw new Error("Clipping end slot not found: " + end);
						clip.endSlot = slot;
					}
					var vertexCount = map.vertexCount;
					this.readVertices(map, clip, vertexCount << 1);
					var color = this.getValue(map, "color", null);
					if (color != null)
						clip.color.setFromString(color);
					return clip;
				}
			}
			return null;
		};
		SkeletonJson.prototype.readVertices = function (map, attachment, verticesLength) {
			var scale = this.scale;
			attachment.worldVerticesLength = verticesLength;
			var vertices = map.vertices;
			if (verticesLength == vertices.length) {
				var scaledVertices = spine.Utils.toFloatArray(vertices);
				if (scale != 1) {
					for (var i = 0, n = vertices.length; i < n; i++)
						scaledVertices[i] *= scale;
				}
				attachment.vertices = scaledVertices;
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
						if (timelineName == "attachment") {
							var timeline = new spine.AttachmentTimeline(timelineMap.length);
							timeline.slotIndex = slotIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								timeline.setFrame(frameIndex++, this.getValue(valueMap, "time", 0), valueMap.name);
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[timeline.getFrameCount() - 1]);
						}
						else if (timelineName == "color") {
							var timeline = new spine.ColorTimeline(timelineMap.length);
							timeline.slotIndex = slotIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								var color = new spine.Color();
								color.setFromString(valueMap.color);
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), color.r, color.g, color.b, color.a);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.ColorTimeline.ENTRIES]);
						}
						else if (timelineName == "twoColor") {
							var timeline = new spine.TwoColorTimeline(timelineMap.length);
							timeline.slotIndex = slotIndex;
							var frameIndex = 0;
							for (var i = 0; i < timelineMap.length; i++) {
								var valueMap = timelineMap[i];
								var light = new spine.Color();
								var dark = new spine.Color();
								light.setFromString(valueMap.light);
								dark.setFromString(valueMap.dark);
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), light.r, light.g, light.b, light.a, dark.r, dark.g, dark.b);
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.TwoColorTimeline.ENTRIES]);
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
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), this.getValue(valueMap, "angle", 0));
								this.readCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.push(timeline);
							duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.RotateTimeline.ENTRIES]);
						}
						else if (timelineName === "translate" || timelineName === "scale" || timelineName === "shear") {
							var timeline = null;
							var timelineScale = 1, defaultValue = 0;
							if (timelineName === "scale") {
								timeline = new spine.ScaleTimeline(timelineMap.length);
								defaultValue = 1;
							}
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
								var x = this.getValue(valueMap, "x", defaultValue), y = this.getValue(valueMap, "y", defaultValue);
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), x * timelineScale, y * timelineScale);
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
						timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), this.getValue(valueMap, "mix", 1), this.getValue(valueMap, "softness", 0) * scale, this.getValue(valueMap, "bendPositive", true) ? 1 : -1, this.getValue(valueMap, "compress", false), this.getValue(valueMap, "stretch", false));
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
						timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), this.getValue(valueMap, "rotateMix", 1), this.getValue(valueMap, "translateMix", 1), this.getValue(valueMap, "scaleMix", 1), this.getValue(valueMap, "shearMix", 1));
						this.readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.push(timeline);
					duration = Math.max(duration, timeline.frames[(timeline.getFrameCount() - 1) * spine.TransformConstraintTimeline.ENTRIES]);
				}
			}
			if (map.path) {
				for (var constraintName in map.path) {
					var constraintMap = map.path[constraintName];
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
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), this.getValue(valueMap, timelineName, 0) * timelineScale);
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
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), this.getValue(valueMap, "rotateMix", 1), this.getValue(valueMap, "translateMix", 1));
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
								timeline.setFrame(frameIndex, this.getValue(valueMap, "time", 0), deform);
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
					timeline.setFrame(frameIndex++, this.getValue(drawOrderMap, "time", 0), drawOrder);
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
					var event_6 = new spine.Event(spine.Utils.toSinglePrecision(this.getValue(eventMap, "time", 0)), eventData);
					event_6.intValue = this.getValue(eventMap, "int", eventData.intValue);
					event_6.floatValue = this.getValue(eventMap, "float", eventData.floatValue);
					event_6.stringValue = this.getValue(eventMap, "string", eventData.stringValue);
					if (event_6.data.audioPath != null) {
						event_6.volume = this.getValue(eventMap, "volume", 1);
						event_6.balance = this.getValue(eventMap, "balance", 0);
					}
					timeline.setFrame(frameIndex++, event_6);
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
			if (!map.hasOwnProperty("curve"))
				return;
			if (map.curve == "stepped")
				timeline.setStepped(frameIndex);
			else {
				var curve = map.curve;
				timeline.setCurve(frameIndex, curve, this.getValue(map, "c2", 0), this.getValue(map, "c3", 1), this.getValue(map, "c4", 1));
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
		SkeletonJson.transformModeFromString = function (str) {
			str = str.toLowerCase();
			if (str == "normal")
				return spine.TransformMode.Normal;
			if (str == "onlytranslation")
				return spine.TransformMode.OnlyTranslation;
			if (str == "norotationorreflection")
				return spine.TransformMode.NoRotationOrReflection;
			if (str == "noscale")
				return spine.TransformMode.NoScale;
			if (str == "noscaleorreflection")
				return spine.TransformMode.NoScaleOrReflection;
			throw new Error("Unknown transform mode: " + str);
		};
		return SkeletonJson;
	}());
	spine.SkeletonJson = SkeletonJson;
	var LinkedMesh = (function () {
		function LinkedMesh(mesh, skin, slotIndex, parent, inheritDeform) {
			this.mesh = mesh;
			this.skin = skin;
			this.slotIndex = slotIndex;
			this.parent = parent;
			this.inheritDeform = inheritDeform;
		}
		return LinkedMesh;
	}());
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SkinEntry = (function () {
		function SkinEntry(slotIndex, name, attachment) {
			this.slotIndex = slotIndex;
			this.name = name;
			this.attachment = attachment;
		}
		return SkinEntry;
	}());
	spine.SkinEntry = SkinEntry;
	var Skin = (function () {
		function Skin(name) {
			this.attachments = new Array();
			this.bones = Array();
			this.constraints = new Array();
			if (name == null)
				throw new Error("name cannot be null.");
			this.name = name;
		}
		Skin.prototype.setAttachment = function (slotIndex, name, attachment) {
			if (attachment == null)
				throw new Error("attachment cannot be null.");
			var attachments = this.attachments;
			if (slotIndex >= attachments.length)
				attachments.length = slotIndex + 1;
			if (!attachments[slotIndex])
				attachments[slotIndex] = {};
			attachments[slotIndex][name] = attachment;
		};
		Skin.prototype.addSkin = function (skin) {
			for (var i = 0; i < skin.bones.length; i++) {
				var bone = skin.bones[i];
				var contained = false;
				for (var j = 0; j < this.bones.length; j++) {
					if (this.bones[j] == bone) {
						contained = true;
						break;
					}
				}
				if (!contained)
					this.bones.push(bone);
			}
			for (var i = 0; i < skin.constraints.length; i++) {
				var constraint = skin.constraints[i];
				var contained = false;
				for (var j = 0; j < this.constraints.length; j++) {
					if (this.constraints[j] == constraint) {
						contained = true;
						break;
					}
				}
				if (!contained)
					this.constraints.push(constraint);
			}
			var attachments = skin.getAttachments();
			for (var i = 0; i < attachments.length; i++) {
				var attachment = attachments[i];
				this.setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
			}
		};
		Skin.prototype.copySkin = function (skin) {
			for (var i = 0; i < skin.bones.length; i++) {
				var bone = skin.bones[i];
				var contained = false;
				for (var j = 0; j < this.bones.length; j++) {
					if (this.bones[j] == bone) {
						contained = true;
						break;
					}
				}
				if (!contained)
					this.bones.push(bone);
			}
			for (var i = 0; i < skin.constraints.length; i++) {
				var constraint = skin.constraints[i];
				var contained = false;
				for (var j = 0; j < this.constraints.length; j++) {
					if (this.constraints[j] == constraint) {
						contained = true;
						break;
					}
				}
				if (!contained)
					this.constraints.push(constraint);
			}
			var attachments = skin.getAttachments();
			for (var i = 0; i < attachments.length; i++) {
				var attachment = attachments[i];
				if (attachment.attachment == null)
					continue;
				if (attachment.attachment instanceof spine.MeshAttachment) {
					attachment.attachment = attachment.attachment.newLinkedMesh();
					this.setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
				}
				else {
					attachment.attachment = attachment.attachment.copy();
					this.setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
				}
			}
		};
		Skin.prototype.getAttachment = function (slotIndex, name) {
			var dictionary = this.attachments[slotIndex];
			return dictionary ? dictionary[name] : null;
		};
		Skin.prototype.removeAttachment = function (slotIndex, name) {
			var dictionary = this.attachments[slotIndex];
			if (dictionary)
				dictionary[name] = null;
		};
		Skin.prototype.getAttachments = function () {
			var entries = new Array();
			for (var i = 0; i < this.attachments.length; i++) {
				var slotAttachments = this.attachments[i];
				if (slotAttachments) {
					for (var name_4 in slotAttachments) {
						var attachment = slotAttachments[name_4];
						if (attachment)
							entries.push(new SkinEntry(i, name_4, attachment));
					}
				}
			}
			return entries;
		};
		Skin.prototype.getAttachmentsForSlot = function (slotIndex, attachments) {
			var slotAttachments = this.attachments[slotIndex];
			if (slotAttachments) {
				for (var name_5 in slotAttachments) {
					var attachment = slotAttachments[name_5];
					if (attachment)
						attachments.push(new SkinEntry(slotIndex, name_5, attachment));
				}
			}
		};
		Skin.prototype.clear = function () {
			this.attachments.length = 0;
			this.bones.length = 0;
			this.constraints.length = 0;
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
							var attachment = this.getAttachment(slotIndex, key);
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
			this.deform = new Array();
			if (data == null)
				throw new Error("data cannot be null.");
			if (bone == null)
				throw new Error("bone cannot be null.");
			this.data = data;
			this.bone = bone;
			this.color = new spine.Color();
			this.darkColor = data.darkColor == null ? null : new spine.Color();
			this.setToSetupPose();
		}
		Slot.prototype.getSkeleton = function () {
			return this.bone.skeleton;
		};
		Slot.prototype.getAttachment = function () {
			return this.attachment;
		};
		Slot.prototype.setAttachment = function (attachment) {
			if (this.attachment == attachment)
				return;
			this.attachment = attachment;
			this.attachmentTime = this.bone.skeleton.time;
			this.deform.length = 0;
		};
		Slot.prototype.setAttachmentTime = function (time) {
			this.attachmentTime = this.bone.skeleton.time - time;
		};
		Slot.prototype.getAttachmentTime = function () {
			return this.bone.skeleton.time - this.attachmentTime;
		};
		Slot.prototype.setToSetupPose = function () {
			this.color.setFromColor(this.data.color);
			if (this.darkColor != null)
				this.darkColor.setFromColor(this.data.darkColor);
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
	var TextureFilter;
	(function (TextureFilter) {
		TextureFilter[TextureFilter["Nearest"] = 9728] = "Nearest";
		TextureFilter[TextureFilter["Linear"] = 9729] = "Linear";
		TextureFilter[TextureFilter["MipMap"] = 9987] = "MipMap";
		TextureFilter[TextureFilter["MipMapNearestNearest"] = 9984] = "MipMapNearestNearest";
		TextureFilter[TextureFilter["MipMapLinearNearest"] = 9985] = "MipMapLinearNearest";
		TextureFilter[TextureFilter["MipMapNearestLinear"] = 9986] = "MipMapNearestLinear";
		TextureFilter[TextureFilter["MipMapLinearLinear"] = 9987] = "MipMapLinearLinear";
	})(TextureFilter = spine.TextureFilter || (spine.TextureFilter = {}));
	var TextureWrap;
	(function (TextureWrap) {
		TextureWrap[TextureWrap["MirroredRepeat"] = 33648] = "MirroredRepeat";
		TextureWrap[TextureWrap["ClampToEdge"] = 33071] = "ClampToEdge";
		TextureWrap[TextureWrap["Repeat"] = 10497] = "Repeat";
	})(TextureWrap = spine.TextureWrap || (spine.TextureWrap = {}));
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
	var FakeTexture = (function (_super) {
		__extends(FakeTexture, _super);
		function FakeTexture() {
			return _super !== null && _super.apply(this, arguments) || this;
		}
		FakeTexture.prototype.setFilters = function (minFilter, magFilter) { };
		FakeTexture.prototype.setWraps = function (uWrap, vWrap) { };
		FakeTexture.prototype.dispose = function () { };
		return FakeTexture;
	}(Texture));
	spine.FakeTexture = FakeTexture;
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
					var rotateValue = reader.readValue();
					if (rotateValue.toLocaleLowerCase() == "true") {
						region.degrees = 90;
					}
					else if (rotateValue.toLocaleLowerCase() == "false") {
						region.degrees = 0;
					}
					else {
						region.degrees = parseFloat(rotateValue);
					}
					region.rotate = region.degrees == 90;
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
			return _super !== null && _super.apply(this, arguments) || this;
		}
		return TextureAtlasRegion;
	}(spine.TextureRegion));
	spine.TextureAtlasRegion = TextureAtlasRegion;
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
			this.active = false;
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
		TransformConstraint.prototype.isActive = function () {
			return this.active;
		};
		TransformConstraint.prototype.apply = function () {
			this.update();
		};
		TransformConstraint.prototype.update = function () {
			if (this.data.local) {
				if (this.data.relative)
					this.applyRelativeLocal();
				else
					this.applyAbsoluteLocal();
			}
			else {
				if (this.data.relative)
					this.applyRelativeWorld();
				else
					this.applyAbsoluteWorld();
			}
		};
		TransformConstraint.prototype.applyAbsoluteWorld = function () {
			var rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			var target = this.target;
			var ta = target.a, tb = target.b, tc = target.c, td = target.d;
			var degRadReflect = ta * td - tb * tc > 0 ? spine.MathUtils.degRad : -spine.MathUtils.degRad;
			var offsetRotation = this.data.offsetRotation * degRadReflect;
			var offsetShearY = this.data.offsetShearY * degRadReflect;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				var modified = false;
				if (rotateMix != 0) {
					var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					var r = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
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
					modified = true;
				}
				if (translateMix != 0) {
					var temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += (temp.x - bone.worldX) * translateMix;
					bone.worldY += (temp.y - bone.worldY) * translateMix;
					modified = true;
				}
				if (scaleMix > 0) {
					var s = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
					var ts = Math.sqrt(ta * ta + tc * tc);
					if (s > 0.00001)
						s = (s + (ts - s + this.data.offsetScaleX) * scaleMix) / s;
					bone.a *= s;
					bone.c *= s;
					s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					ts = Math.sqrt(tb * tb + td * td);
					if (s > 0.00001)
						s = (s + (ts - s + this.data.offsetScaleY) * scaleMix) / s;
					bone.b *= s;
					bone.d *= s;
					modified = true;
				}
				if (shearMix > 0) {
					var b = bone.b, d = bone.d;
					var by = Math.atan2(d, b);
					var r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r = by + (r + offsetShearY) * shearMix;
					var s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
					modified = true;
				}
				if (modified)
					bone.appliedValid = false;
			}
		};
		TransformConstraint.prototype.applyRelativeWorld = function () {
			var rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			var target = this.target;
			var ta = target.a, tb = target.b, tc = target.c, td = target.d;
			var degRadReflect = ta * td - tb * tc > 0 ? spine.MathUtils.degRad : -spine.MathUtils.degRad;
			var offsetRotation = this.data.offsetRotation * degRadReflect, offsetShearY = this.data.offsetShearY * degRadReflect;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				var modified = false;
				if (rotateMix != 0) {
					var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					var r = Math.atan2(tc, ta) + offsetRotation;
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
					modified = true;
				}
				if (translateMix != 0) {
					var temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += temp.x * translateMix;
					bone.worldY += temp.y * translateMix;
					modified = true;
				}
				if (scaleMix > 0) {
					var s = (Math.sqrt(ta * ta + tc * tc) - 1 + this.data.offsetScaleX) * scaleMix + 1;
					bone.a *= s;
					bone.c *= s;
					s = (Math.sqrt(tb * tb + td * td) - 1 + this.data.offsetScaleY) * scaleMix + 1;
					bone.b *= s;
					bone.d *= s;
					modified = true;
				}
				if (shearMix > 0) {
					var r = Math.atan2(td, tb) - Math.atan2(tc, ta);
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					var b = bone.b, d = bone.d;
					r = Math.atan2(d, b) + (r - spine.MathUtils.PI / 2 + offsetShearY) * shearMix;
					var s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
					modified = true;
				}
				if (modified)
					bone.appliedValid = false;
			}
		};
		TransformConstraint.prototype.applyAbsoluteLocal = function () {
			var rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			var target = this.target;
			if (!target.appliedValid)
				target.updateAppliedTransform();
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (!bone.appliedValid)
					bone.updateAppliedTransform();
				var rotation = bone.arotation;
				if (rotateMix != 0) {
					var r = target.arotation - rotation + this.data.offsetRotation;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					rotation += r * rotateMix;
				}
				var x = bone.ax, y = bone.ay;
				if (translateMix != 0) {
					x += (target.ax - x + this.data.offsetX) * translateMix;
					y += (target.ay - y + this.data.offsetY) * translateMix;
				}
				var scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (scaleMix != 0) {
					if (scaleX > 0.00001)
						scaleX = (scaleX + (target.ascaleX - scaleX + this.data.offsetScaleX) * scaleMix) / scaleX;
					if (scaleY > 0.00001)
						scaleY = (scaleY + (target.ascaleY - scaleY + this.data.offsetScaleY) * scaleMix) / scaleY;
				}
				var shearY = bone.ashearY;
				if (shearMix != 0) {
					var r = target.ashearY - shearY + this.data.offsetShearY;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					bone.shearY += r * shearMix;
				}
				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		};
		TransformConstraint.prototype.applyRelativeLocal = function () {
			var rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			var target = this.target;
			if (!target.appliedValid)
				target.updateAppliedTransform();
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (!bone.appliedValid)
					bone.updateAppliedTransform();
				var rotation = bone.arotation;
				if (rotateMix != 0)
					rotation += (target.arotation + this.data.offsetRotation) * rotateMix;
				var x = bone.ax, y = bone.ay;
				if (translateMix != 0) {
					x += (target.ax + this.data.offsetX) * translateMix;
					y += (target.ay + this.data.offsetY) * translateMix;
				}
				var scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (scaleMix != 0) {
					if (scaleX > 0.00001)
						scaleX *= ((target.ascaleX - 1 + this.data.offsetScaleX) * scaleMix) + 1;
					if (scaleY > 0.00001)
						scaleY *= ((target.ascaleY - 1 + this.data.offsetScaleY) * scaleMix) + 1;
				}
				var shearY = bone.ashearY;
				if (shearMix != 0)
					shearY += (target.ashearY + this.data.offsetShearY) * shearMix;
				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		};
		return TransformConstraint;
	}());
	spine.TransformConstraint = TransformConstraint;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var TransformConstraintData = (function (_super) {
		__extends(TransformConstraintData, _super);
		function TransformConstraintData(name) {
			var _this = _super.call(this, name, 0, false) || this;
			_this.bones = new Array();
			_this.rotateMix = 0;
			_this.translateMix = 0;
			_this.scaleMix = 0;
			_this.shearMix = 0;
			_this.offsetRotation = 0;
			_this.offsetX = 0;
			_this.offsetY = 0;
			_this.offsetScaleX = 0;
			_this.offsetScaleY = 0;
			_this.offsetShearY = 0;
			_this.relative = false;
			_this.local = false;
			return _this;
		}
		return TransformConstraintData;
	}(spine.ConstraintData));
	spine.TransformConstraintData = TransformConstraintData;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Triangulator = (function () {
		function Triangulator() {
			this.convexPolygons = new Array();
			this.convexPolygonsIndices = new Array();
			this.indicesArray = new Array();
			this.isConcaveArray = new Array();
			this.triangles = new Array();
			this.polygonPool = new spine.Pool(function () {
				return new Array();
			});
			this.polygonIndicesPool = new spine.Pool(function () {
				return new Array();
			});
		}
		Triangulator.prototype.triangulate = function (verticesArray) {
			var vertices = verticesArray;
			var vertexCount = verticesArray.length >> 1;
			var indices = this.indicesArray;
			indices.length = 0;
			for (var i = 0; i < vertexCount; i++)
				indices[i] = i;
			var isConcave = this.isConcaveArray;
			isConcave.length = 0;
			for (var i = 0, n = vertexCount; i < n; ++i)
				isConcave[i] = Triangulator.isConcave(i, vertexCount, vertices, indices);
			var triangles = this.triangles;
			triangles.length = 0;
			while (vertexCount > 3) {
				var previous = vertexCount - 1, i = 0, next = 1;
				while (true) {
					outer: if (!isConcave[i]) {
						var p1 = indices[previous] << 1, p2 = indices[i] << 1, p3 = indices[next] << 1;
						var p1x = vertices[p1], p1y = vertices[p1 + 1];
						var p2x = vertices[p2], p2y = vertices[p2 + 1];
						var p3x = vertices[p3], p3y = vertices[p3 + 1];
						for (var ii = (next + 1) % vertexCount; ii != previous; ii = (ii + 1) % vertexCount) {
							if (!isConcave[ii])
								continue;
							var v = indices[ii] << 1;
							var vx = vertices[v], vy = vertices[v + 1];
							if (Triangulator.positiveArea(p3x, p3y, p1x, p1y, vx, vy)) {
								if (Triangulator.positiveArea(p1x, p1y, p2x, p2y, vx, vy)) {
									if (Triangulator.positiveArea(p2x, p2y, p3x, p3y, vx, vy))
										break outer;
								}
							}
						}
						break;
					}
					if (next == 0) {
						do {
							if (!isConcave[i])
								break;
							i--;
						} while (i > 0);
						break;
					}
					previous = i;
					i = next;
					next = (next + 1) % vertexCount;
				}
				triangles.push(indices[(vertexCount + i - 1) % vertexCount]);
				triangles.push(indices[i]);
				triangles.push(indices[(i + 1) % vertexCount]);
				indices.splice(i, 1);
				isConcave.splice(i, 1);
				vertexCount--;
				var previousIndex = (vertexCount + i - 1) % vertexCount;
				var nextIndex = i == vertexCount ? 0 : i;
				isConcave[previousIndex] = Triangulator.isConcave(previousIndex, vertexCount, vertices, indices);
				isConcave[nextIndex] = Triangulator.isConcave(nextIndex, vertexCount, vertices, indices);
			}
			if (vertexCount == 3) {
				triangles.push(indices[2]);
				triangles.push(indices[0]);
				triangles.push(indices[1]);
			}
			return triangles;
		};
		Triangulator.prototype.decompose = function (verticesArray, triangles) {
			var vertices = verticesArray;
			var convexPolygons = this.convexPolygons;
			this.polygonPool.freeAll(convexPolygons);
			convexPolygons.length = 0;
			var convexPolygonsIndices = this.convexPolygonsIndices;
			this.polygonIndicesPool.freeAll(convexPolygonsIndices);
			convexPolygonsIndices.length = 0;
			var polygonIndices = this.polygonIndicesPool.obtain();
			polygonIndices.length = 0;
			var polygon = this.polygonPool.obtain();
			polygon.length = 0;
			var fanBaseIndex = -1, lastWinding = 0;
			for (var i = 0, n = triangles.length; i < n; i += 3) {
				var t1 = triangles[i] << 1, t2 = triangles[i + 1] << 1, t3 = triangles[i + 2] << 1;
				var x1 = vertices[t1], y1 = vertices[t1 + 1];
				var x2 = vertices[t2], y2 = vertices[t2 + 1];
				var x3 = vertices[t3], y3 = vertices[t3 + 1];
				var merged = false;
				if (fanBaseIndex == t1) {
					var o = polygon.length - 4;
					var winding1 = Triangulator.winding(polygon[o], polygon[o + 1], polygon[o + 2], polygon[o + 3], x3, y3);
					var winding2 = Triangulator.winding(x3, y3, polygon[0], polygon[1], polygon[2], polygon[3]);
					if (winding1 == lastWinding && winding2 == lastWinding) {
						polygon.push(x3);
						polygon.push(y3);
						polygonIndices.push(t3);
						merged = true;
					}
				}
				if (!merged) {
					if (polygon.length > 0) {
						convexPolygons.push(polygon);
						convexPolygonsIndices.push(polygonIndices);
					}
					else {
						this.polygonPool.free(polygon);
						this.polygonIndicesPool.free(polygonIndices);
					}
					polygon = this.polygonPool.obtain();
					polygon.length = 0;
					polygon.push(x1);
					polygon.push(y1);
					polygon.push(x2);
					polygon.push(y2);
					polygon.push(x3);
					polygon.push(y3);
					polygonIndices = this.polygonIndicesPool.obtain();
					polygonIndices.length = 0;
					polygonIndices.push(t1);
					polygonIndices.push(t2);
					polygonIndices.push(t3);
					lastWinding = Triangulator.winding(x1, y1, x2, y2, x3, y3);
					fanBaseIndex = t1;
				}
			}
			if (polygon.length > 0) {
				convexPolygons.push(polygon);
				convexPolygonsIndices.push(polygonIndices);
			}
			for (var i = 0, n = convexPolygons.length; i < n; i++) {
				polygonIndices = convexPolygonsIndices[i];
				if (polygonIndices.length == 0)
					continue;
				var firstIndex = polygonIndices[0];
				var lastIndex = polygonIndices[polygonIndices.length - 1];
				polygon = convexPolygons[i];
				var o = polygon.length - 4;
				var prevPrevX = polygon[o], prevPrevY = polygon[o + 1];
				var prevX = polygon[o + 2], prevY = polygon[o + 3];
				var firstX = polygon[0], firstY = polygon[1];
				var secondX = polygon[2], secondY = polygon[3];
				var winding = Triangulator.winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);
				for (var ii = 0; ii < n; ii++) {
					if (ii == i)
						continue;
					var otherIndices = convexPolygonsIndices[ii];
					if (otherIndices.length != 3)
						continue;
					var otherFirstIndex = otherIndices[0];
					var otherSecondIndex = otherIndices[1];
					var otherLastIndex = otherIndices[2];
					var otherPoly = convexPolygons[ii];
					var x3 = otherPoly[otherPoly.length - 2], y3 = otherPoly[otherPoly.length - 1];
					if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex)
						continue;
					var winding1 = Triangulator.winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3);
					var winding2 = Triangulator.winding(x3, y3, firstX, firstY, secondX, secondY);
					if (winding1 == winding && winding2 == winding) {
						otherPoly.length = 0;
						otherIndices.length = 0;
						polygon.push(x3);
						polygon.push(y3);
						polygonIndices.push(otherLastIndex);
						prevPrevX = prevX;
						prevPrevY = prevY;
						prevX = x3;
						prevY = y3;
						ii = 0;
					}
				}
			}
			for (var i = convexPolygons.length - 1; i >= 0; i--) {
				polygon = convexPolygons[i];
				if (polygon.length == 0) {
					convexPolygons.splice(i, 1);
					this.polygonPool.free(polygon);
					polygonIndices = convexPolygonsIndices[i];
					convexPolygonsIndices.splice(i, 1);
					this.polygonIndicesPool.free(polygonIndices);
				}
			}
			return convexPolygons;
		};
		Triangulator.isConcave = function (index, vertexCount, vertices, indices) {
			var previous = indices[(vertexCount + index - 1) % vertexCount] << 1;
			var current = indices[index] << 1;
			var next = indices[(index + 1) % vertexCount] << 1;
			return !this.positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[next], vertices[next + 1]);
		};
		Triangulator.positiveArea = function (p1x, p1y, p2x, p2y, p3x, p3y) {
			return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0;
		};
		Triangulator.winding = function (p1x, p1y, p2x, p2y, p3x, p3y) {
			var px = p2x - p1x, py = p2y - p1y;
			return p3x * py - p3y * px + px * p1y - p1x * py >= 0 ? 1 : -1;
		};
		return Triangulator;
	}());
	spine.Triangulator = Triangulator;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var IntSet = (function () {
		function IntSet() {
			this.array = new Array();
		}
		IntSet.prototype.add = function (value) {
			var contains = this.contains(value);
			this.array[value | 0] = value | 0;
			return !contains;
		};
		IntSet.prototype.contains = function (value) {
			return this.array[value | 0] != undefined;
		};
		IntSet.prototype.remove = function (value) {
			this.array[value | 0] = undefined;
		};
		IntSet.prototype.clear = function () {
			this.array.length = 0;
		};
		return IntSet;
	}());
	spine.IntSet = IntSet;
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
		Color.rgba8888ToColor = function (color, value) {
			color.r = ((value & 0xff000000) >>> 24) / 255;
			color.g = ((value & 0x00ff0000) >>> 16) / 255;
			color.b = ((value & 0x0000ff00) >>> 8) / 255;
			color.a = ((value & 0x000000ff)) / 255;
		};
		Color.rgb888ToColor = function (color, value) {
			color.r = ((value & 0x00ff0000) >>> 16) / 255;
			color.g = ((value & 0x0000ff00) >>> 8) / 255;
			color.b = ((value & 0x000000ff)) / 255;
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
			return value > 0 ? 1 : value < 0 ? -1 : 0;
		};
		MathUtils.toInt = function (x) {
			return x > 0 ? Math.floor(x) : Math.ceil(x);
		};
		MathUtils.cbrt = function (x) {
			var y = Math.pow(Math.abs(x), 1 / 3);
			return x < 0 ? -y : y;
		};
		MathUtils.randomTriangular = function (min, max) {
			return MathUtils.randomTriangularWith(min, max, (min + max) * 0.5);
		};
		MathUtils.randomTriangularWith = function (min, max, mode) {
			var u = Math.random();
			var d = max - min;
			if (u <= (mode - min) / d)
				return min + Math.sqrt(u * d * (mode - min));
			return max - Math.sqrt((1 - u) * d * (max - mode));
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
	var Interpolation = (function () {
		function Interpolation() {
		}
		Interpolation.prototype.apply = function (start, end, a) {
			return start + (end - start) * this.applyInternal(a);
		};
		return Interpolation;
	}());
	spine.Interpolation = Interpolation;
	var Pow = (function (_super) {
		__extends(Pow, _super);
		function Pow(power) {
			var _this = _super.call(this) || this;
			_this.power = 2;
			_this.power = power;
			return _this;
		}
		Pow.prototype.applyInternal = function (a) {
			if (a <= 0.5)
				return Math.pow(a * 2, this.power) / 2;
			return Math.pow((a - 1) * 2, this.power) / (this.power % 2 == 0 ? -2 : 2) + 1;
		};
		return Pow;
	}(Interpolation));
	spine.Pow = Pow;
	var PowOut = (function (_super) {
		__extends(PowOut, _super);
		function PowOut(power) {
			return _super.call(this, power) || this;
		}
		PowOut.prototype.applyInternal = function (a) {
			return Math.pow(a - 1, this.power) * (this.power % 2 == 0 ? -1 : 1) + 1;
		};
		return PowOut;
	}(Pow));
	spine.PowOut = PowOut;
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
		Utils.ensureArrayCapacity = function (array, size, value) {
			if (value === void 0) { value = 0; }
			if (array.length >= size)
				return array;
			return Utils.setArraySize(array, size, value);
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
		Utils.newShortArray = function (size) {
			if (Utils.SUPPORTS_TYPED_ARRAYS) {
				return new Int16Array(size);
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
		Utils.toSinglePrecision = function (value) {
			return Utils.SUPPORTS_TYPED_ARRAYS ? Math.fround(value) : value;
		};
		Utils.webkit602BugfixHelper = function (alpha, blend) {
		};
		Utils.contains = function (array, element, identity) {
			if (identity === void 0) { identity = true; }
			for (var i = 0; i < array.length; i++) {
				if (array[i] == element)
					return true;
			}
			return false;
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
			if (item.reset)
				item.reset();
			this.items.push(item);
		};
		Pool.prototype.freeAll = function (items) {
			for (var i = 0; i < items.length; i++) {
				this.free(items[i]);
			}
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
	var WindowedMean = (function () {
		function WindowedMean(windowSize) {
			if (windowSize === void 0) { windowSize = 32; }
			this.addedValues = 0;
			this.lastValue = 0;
			this.mean = 0;
			this.dirty = true;
			this.values = new Array(windowSize);
		}
		WindowedMean.prototype.hasEnoughData = function () {
			return this.addedValues >= this.values.length;
		};
		WindowedMean.prototype.addValue = function (value) {
			if (this.addedValues < this.values.length)
				this.addedValues++;
			this.values[this.lastValue++] = value;
			if (this.lastValue > this.values.length - 1)
				this.lastValue = 0;
			this.dirty = true;
		};
		WindowedMean.prototype.getMean = function () {
			if (this.hasEnoughData()) {
				if (this.dirty) {
					var mean = 0;
					for (var i = 0; i < this.values.length; i++) {
						mean += this.values[i];
					}
					this.mean = mean / this.values.length;
					this.dirty = false;
				}
				return this.mean;
			}
			else {
				return 0;
			}
		};
		return WindowedMean;
	}());
	spine.WindowedMean = WindowedMean;
})(spine || (spine = {}));
(function () {
	if (!Math.fround) {
		Math.fround = (function (array) {
			return function (x) {
				return array[0] = x, array[0];
			};
		})(new Float32Array(1));
	}
})();
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
			var _this = _super.call(this, name) || this;
			_this.id = (VertexAttachment.nextID++ & 65535) << 11;
			_this.worldVerticesLength = 0;
			_this.deformAttachment = _this;
			return _this;
		}
		VertexAttachment.prototype.computeWorldVertices = function (slot, start, count, worldVertices, offset, stride) {
			count = offset + (count >> 1) * stride;
			var skeleton = slot.bone.skeleton;
			var deformArray = slot.deform;
			var vertices = this.vertices;
			var bones = this.bones;
			if (bones == null) {
				if (deformArray.length > 0)
					vertices = deformArray;
				var bone = slot.bone;
				var x = bone.worldX;
				var y = bone.worldY;
				var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (var v_1 = start, w = offset; w < count; v_1 += 2, w += stride) {
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
				for (var w = offset, b = skip * 3; w < count; w += stride) {
					var wx = 0, wy = 0;
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
				for (var w = offset, b = skip * 3, f = skip << 1; w < count; w += stride) {
					var wx = 0, wy = 0;
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
		VertexAttachment.prototype.copyTo = function (attachment) {
			if (this.bones != null) {
				attachment.bones = new Array(this.bones.length);
				spine.Utils.arrayCopy(this.bones, 0, attachment.bones, 0, this.bones.length);
			}
			else
				attachment.bones = null;
			if (this.vertices != null) {
				attachment.vertices = spine.Utils.newFloatArray(this.vertices.length);
				spine.Utils.arrayCopy(this.vertices, 0, attachment.vertices, 0, this.vertices.length);
			}
			else
				attachment.vertices = null;
			attachment.worldVerticesLength = this.worldVerticesLength;
			attachment.deformAttachment = this.deformAttachment;
		};
		VertexAttachment.nextID = 0;
		return VertexAttachment;
	}(Attachment));
	spine.VertexAttachment = VertexAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var AttachmentType;
	(function (AttachmentType) {
		AttachmentType[AttachmentType["Region"] = 0] = "Region";
		AttachmentType[AttachmentType["BoundingBox"] = 1] = "BoundingBox";
		AttachmentType[AttachmentType["Mesh"] = 2] = "Mesh";
		AttachmentType[AttachmentType["LinkedMesh"] = 3] = "LinkedMesh";
		AttachmentType[AttachmentType["Path"] = 4] = "Path";
		AttachmentType[AttachmentType["Point"] = 5] = "Point";
		AttachmentType[AttachmentType["Clipping"] = 6] = "Clipping";
	})(AttachmentType = spine.AttachmentType || (spine.AttachmentType = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var BoundingBoxAttachment = (function (_super) {
		__extends(BoundingBoxAttachment, _super);
		function BoundingBoxAttachment(name) {
			var _this = _super.call(this, name) || this;
			_this.color = new spine.Color(1, 1, 1, 1);
			return _this;
		}
		BoundingBoxAttachment.prototype.copy = function () {
			var copy = new BoundingBoxAttachment(name);
			this.copyTo(copy);
			copy.color.setFromColor(this.color);
			return copy;
		};
		return BoundingBoxAttachment;
	}(spine.VertexAttachment));
	spine.BoundingBoxAttachment = BoundingBoxAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var ClippingAttachment = (function (_super) {
		__extends(ClippingAttachment, _super);
		function ClippingAttachment(name) {
			var _this = _super.call(this, name) || this;
			_this.color = new spine.Color(0.2275, 0.2275, 0.8078, 1);
			return _this;
		}
		ClippingAttachment.prototype.copy = function () {
			var copy = new ClippingAttachment(name);
			this.copyTo(copy);
			copy.endSlot = this.endSlot;
			copy.color.setFromColor(this.color);
			return copy;
		};
		return ClippingAttachment;
	}(spine.VertexAttachment));
	spine.ClippingAttachment = ClippingAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var MeshAttachment = (function (_super) {
		__extends(MeshAttachment, _super);
		function MeshAttachment(name) {
			var _this = _super.call(this, name) || this;
			_this.color = new spine.Color(1, 1, 1, 1);
			_this.tempColor = new spine.Color(0, 0, 0, 0);
			return _this;
		}
		MeshAttachment.prototype.updateUVs = function () {
			var regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.length != regionUVs.length)
				this.uvs = spine.Utils.newFloatArray(regionUVs.length);
			var uvs = this.uvs;
			var n = this.uvs.length;
			var u = this.region.u, v = this.region.v, width = 0, height = 0;
			if (this.region instanceof spine.TextureAtlasRegion) {
				var region = this.region;
				var textureWidth = region.texture.getImage().width, textureHeight = region.texture.getImage().height;
				switch (region.degrees) {
					case 90:
						u -= (region.originalHeight - region.offsetY - region.height) / textureWidth;
						v -= (region.originalWidth - region.offsetX - region.width) / textureHeight;
						width = region.originalHeight / textureWidth;
						height = region.originalWidth / textureHeight;
						for (var i = 0; i < n; i += 2) {
							uvs[i] = u + regionUVs[i + 1] * width;
							uvs[i + 1] = v + (1 - regionUVs[i]) * height;
						}
						return;
					case 180:
						u -= (region.originalWidth - region.offsetX - region.width) / textureWidth;
						v -= region.offsetY / textureHeight;
						width = region.originalWidth / textureWidth;
						height = region.originalHeight / textureHeight;
						for (var i = 0; i < n; i += 2) {
							uvs[i] = u + (1 - regionUVs[i]) * width;
							uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
						}
						return;
					case 270:
						u -= region.offsetY / textureWidth;
						v -= region.offsetX / textureHeight;
						width = region.originalHeight / textureWidth;
						height = region.originalWidth / textureHeight;
						for (var i = 0; i < n; i += 2) {
							uvs[i] = u + (1 - regionUVs[i + 1]) * width;
							uvs[i + 1] = v + regionUVs[i] * height;
						}
						return;
				}
				u -= region.offsetX / textureWidth;
				v -= (region.originalHeight - region.offsetY - region.height) / textureHeight;
				width = region.originalWidth / textureWidth;
				height = region.originalHeight / textureHeight;
			}
			else if (this.region == null) {
				u = v = 0;
				width = height = 1;
			}
			else {
				width = this.region.u2 - u;
				height = this.region.v2 - v;
			}
			for (var i = 0; i < n; i += 2) {
				uvs[i] = u + regionUVs[i] * width;
				uvs[i + 1] = v + regionUVs[i + 1] * height;
			}
		};
		MeshAttachment.prototype.getParentMesh = function () {
			return this.parentMesh;
		};
		MeshAttachment.prototype.setParentMesh = function (parentMesh) {
			this.parentMesh = parentMesh;
			if (parentMesh != null) {
				this.bones = parentMesh.bones;
				this.vertices = parentMesh.vertices;
				this.worldVerticesLength = parentMesh.worldVerticesLength;
				this.regionUVs = parentMesh.regionUVs;
				this.triangles = parentMesh.triangles;
				this.hullLength = parentMesh.hullLength;
				this.worldVerticesLength = parentMesh.worldVerticesLength;
			}
		};
		MeshAttachment.prototype.copy = function () {
			if (this.parentMesh != null)
				return this.newLinkedMesh();
			var copy = new MeshAttachment(this.name);
			copy.region = this.region;
			copy.path = this.path;
			copy.color.setFromColor(this.color);
			this.copyTo(copy);
			copy.regionUVs = new Array(this.regionUVs.length);
			spine.Utils.arrayCopy(this.regionUVs, 0, copy.regionUVs, 0, this.regionUVs.length);
			copy.uvs = new Array(this.uvs.length);
			spine.Utils.arrayCopy(this.uvs, 0, copy.uvs, 0, this.uvs.length);
			copy.triangles = new Array(this.triangles.length);
			spine.Utils.arrayCopy(this.triangles, 0, copy.triangles, 0, this.triangles.length);
			copy.hullLength = this.hullLength;
			if (this.edges != null) {
				copy.edges = new Array(this.edges.length);
				spine.Utils.arrayCopy(this.edges, 0, copy.edges, 0, this.edges.length);
			}
			copy.width = this.width;
			copy.height = this.height;
			return copy;
		};
		MeshAttachment.prototype.newLinkedMesh = function () {
			var copy = new MeshAttachment(this.name);
			copy.region = this.region;
			copy.path = this.path;
			copy.color.setFromColor(this.color);
			copy.deformAttachment = this.deformAttachment;
			copy.setParentMesh(this.parentMesh != null ? this.parentMesh : this);
			copy.updateUVs();
			return copy;
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
			var _this = _super.call(this, name) || this;
			_this.closed = false;
			_this.constantSpeed = false;
			_this.color = new spine.Color(1, 1, 1, 1);
			return _this;
		}
		PathAttachment.prototype.copy = function () {
			var copy = new PathAttachment(name);
			this.copyTo(copy);
			copy.lengths = new Array(this.lengths.length);
			spine.Utils.arrayCopy(this.lengths, 0, copy.lengths, 0, this.lengths.length);
			copy.closed = closed;
			copy.constantSpeed = this.constantSpeed;
			copy.color.setFromColor(this.color);
			return copy;
		};
		return PathAttachment;
	}(spine.VertexAttachment));
	spine.PathAttachment = PathAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var PointAttachment = (function (_super) {
		__extends(PointAttachment, _super);
		function PointAttachment(name) {
			var _this = _super.call(this, name) || this;
			_this.color = new spine.Color(0.38, 0.94, 0, 1);
			return _this;
		}
		PointAttachment.prototype.computeWorldPosition = function (bone, point) {
			point.x = this.x * bone.a + this.y * bone.b + bone.worldX;
			point.y = this.x * bone.c + this.y * bone.d + bone.worldY;
			return point;
		};
		PointAttachment.prototype.computeWorldRotation = function (bone) {
			var cos = spine.MathUtils.cosDeg(this.rotation), sin = spine.MathUtils.sinDeg(this.rotation);
			var x = cos * bone.a + sin * bone.b;
			var y = cos * bone.c + sin * bone.d;
			return Math.atan2(y, x) * spine.MathUtils.radDeg;
		};
		PointAttachment.prototype.copy = function () {
			var copy = new PointAttachment(name);
			copy.x = this.x;
			copy.y = this.y;
			copy.rotation = this.rotation;
			copy.color.setFromColor(this.color);
			return copy;
		};
		return PointAttachment;
	}(spine.VertexAttachment));
	spine.PointAttachment = PointAttachment;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var RegionAttachment = (function (_super) {
		__extends(RegionAttachment, _super);
		function RegionAttachment(name) {
			var _this = _super.call(this, name) || this;
			_this.x = 0;
			_this.y = 0;
			_this.scaleX = 1;
			_this.scaleY = 1;
			_this.rotation = 0;
			_this.width = 0;
			_this.height = 0;
			_this.color = new spine.Color(1, 1, 1, 1);
			_this.offset = spine.Utils.newFloatArray(8);
			_this.uvs = spine.Utils.newFloatArray(8);
			_this.tempColor = new spine.Color(1, 1, 1, 1);
			return _this;
		}
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
		RegionAttachment.prototype.setRegion = function (region) {
			this.region = region;
			var uvs = this.uvs;
			if (region.rotate) {
				uvs[2] = region.u;
				uvs[3] = region.v2;
				uvs[4] = region.u;
				uvs[5] = region.v;
				uvs[6] = region.u2;
				uvs[7] = region.v;
				uvs[0] = region.u2;
				uvs[1] = region.v2;
			}
			else {
				uvs[0] = region.u;
				uvs[1] = region.v2;
				uvs[2] = region.u;
				uvs[3] = region.v;
				uvs[4] = region.u2;
				uvs[5] = region.v;
				uvs[6] = region.u2;
				uvs[7] = region.v2;
			}
		};
		RegionAttachment.prototype.computeWorldVertices = function (bone, worldVertices, offset, stride) {
			var vertexOffset = this.offset;
			var x = bone.worldX, y = bone.worldY;
			var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			var offsetX = 0, offsetY = 0;
			offsetX = vertexOffset[RegionAttachment.OX1];
			offsetY = vertexOffset[RegionAttachment.OY1];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
			offsetX = vertexOffset[RegionAttachment.OX2];
			offsetY = vertexOffset[RegionAttachment.OY2];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
			offsetX = vertexOffset[RegionAttachment.OX3];
			offsetY = vertexOffset[RegionAttachment.OY3];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
			offsetX = vertexOffset[RegionAttachment.OX4];
			offsetY = vertexOffset[RegionAttachment.OY4];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		};
		RegionAttachment.prototype.copy = function () {
			var copy = new RegionAttachment(this.name);
			copy.region = this.region;
			copy.rendererObject = this.rendererObject;
			copy.path = this.path;
			copy.x = this.x;
			copy.y = this.y;
			copy.scaleX = this.scaleX;
			copy.scaleY = this.scaleY;
			copy.rotation = this.rotation;
			copy.width = this.width;
			copy.height = this.height;
			spine.Utils.arrayCopy(this.uvs, 0, copy.uvs, 0, 8);
			spine.Utils.arrayCopy(this.offset, 0, copy.offset, 0, 8);
			copy.color.setFromColor(this.color);
			return copy;
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
	var JitterEffect = (function () {
		function JitterEffect(jitterX, jitterY) {
			this.jitterX = 0;
			this.jitterY = 0;
			this.jitterX = jitterX;
			this.jitterY = jitterY;
		}
		JitterEffect.prototype.begin = function (skeleton) {
		};
		JitterEffect.prototype.transform = function (position, uv, light, dark) {
			position.x += spine.MathUtils.randomTriangular(-this.jitterX, this.jitterY);
			position.y += spine.MathUtils.randomTriangular(-this.jitterX, this.jitterY);
		};
		JitterEffect.prototype.end = function () {
		};
		return JitterEffect;
	}());
	spine.JitterEffect = JitterEffect;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SwirlEffect = (function () {
		function SwirlEffect(radius) {
			this.centerX = 0;
			this.centerY = 0;
			this.radius = 0;
			this.angle = 0;
			this.worldX = 0;
			this.worldY = 0;
			this.radius = radius;
		}
		SwirlEffect.prototype.begin = function (skeleton) {
			this.worldX = skeleton.x + this.centerX;
			this.worldY = skeleton.y + this.centerY;
		};
		SwirlEffect.prototype.transform = function (position, uv, light, dark) {
			var radAngle = this.angle * spine.MathUtils.degreesToRadians;
			var x = position.x - this.worldX;
			var y = position.y - this.worldY;
			var dist = Math.sqrt(x * x + y * y);
			if (dist < this.radius) {
				var theta = SwirlEffect.interpolation.apply(0, radAngle, (this.radius - dist) / this.radius);
				var cos = Math.cos(theta);
				var sin = Math.sin(theta);
				position.x = cos * x - sin * y + this.worldX;
				position.y = sin * x + cos * y + this.worldY;
			}
		};
		SwirlEffect.prototype.end = function () {
		};
		SwirlEffect.interpolation = new spine.PowOut(2);
		return SwirlEffect;
	}());
	spine.SwirlEffect = SwirlEffect;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var AssetManager = (function (_super) {
			__extends(AssetManager, _super);
			function AssetManager(context, pathPrefix) {
				if (pathPrefix === void 0) { pathPrefix = ""; }
				return _super.call(this, function (image) {
					return new spine.webgl.GLTexture(context, image);
				}, pathPrefix) || this;
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
			function GLTexture(context, image, useMipMaps) {
				if (useMipMaps === void 0) { useMipMaps = false; }
				var _this = _super.call(this, image) || this;
				_this.texture = null;
				_this.boundUnit = 0;
				_this.useMipMaps = false;
				_this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
				_this.useMipMaps = useMipMaps;
				_this.restore();
				_this.context.addRestorable(_this);
				return _this;
			}
			GLTexture.prototype.setFilters = function (minFilter, magFilter) {
				var gl = this.context.gl;
				this.bind();
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, minFilter);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, GLTexture.validateMagFilter(magFilter));
			};
			GLTexture.validateMagFilter = function (magFilter) {
				switch (magFilter) {
					case spine.TextureFilter.MipMap:
					case spine.TextureFilter.MipMapLinearLinear:
					case spine.TextureFilter.MipMapLinearNearest:
					case spine.TextureFilter.MipMapNearestLinear:
					case spine.TextureFilter.MipMapNearestNearest:
						return spine.TextureFilter.Linear;
					default:
						return magFilter;
				}
			};
			GLTexture.prototype.setWraps = function (uWrap, vWrap) {
				var gl = this.context.gl;
				this.bind();
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, uWrap);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, vWrap);
			};
			GLTexture.prototype.update = function (useMipMaps) {
				var gl = this.context.gl;
				if (!this.texture) {
					this.texture = this.context.gl.createTexture();
				}
				this.bind();
				if (GLTexture.DISABLE_UNPACK_PREMULTIPLIED_ALPHA_WEBGL)
					gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
				gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this._image);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, useMipMaps ? gl.LINEAR_MIPMAP_LINEAR : gl.LINEAR);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
				gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
				if (useMipMaps)
					gl.generateMipmap(gl.TEXTURE_2D);
			};
			GLTexture.prototype.restore = function () {
				this.texture = null;
				this.update(this.useMipMaps);
			};
			GLTexture.prototype.bind = function (unit) {
				if (unit === void 0) { unit = 0; }
				var gl = this.context.gl;
				this.boundUnit = unit;
				gl.activeTexture(gl.TEXTURE0 + unit);
				gl.bindTexture(gl.TEXTURE_2D, this.texture);
			};
			GLTexture.prototype.unbind = function () {
				var gl = this.context.gl;
				gl.activeTexture(gl.TEXTURE0 + this.boundUnit);
				gl.bindTexture(gl.TEXTURE_2D, null);
			};
			GLTexture.prototype.dispose = function () {
				this.context.removeRestorable(this);
				var gl = this.context.gl;
				gl.deleteTexture(this.texture);
			};
			GLTexture.DISABLE_UNPACK_PREMULTIPLIED_ALPHA_WEBGL = false;
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
				var mouseDown = function (ev) {
					if (ev instanceof MouseEvent) {
						var rect = element.getBoundingClientRect();
						var x = ev.clientX - rect.left;
						var y = ev.clientY - rect.top;
						var listeners = _this.listeners;
						for (var i = 0; i < listeners.length; i++) {
							if (listeners[i].down)
								listeners[i].down(x, y);
						}
						_this.lastX = x;
						_this.lastY = y;
						_this.buttonDown = true;
						document.addEventListener("mousemove", mouseMove);
						document.addEventListener("mouseup", mouseUp);
					}
				};
				var mouseMove = function (ev) {
					if (ev instanceof MouseEvent) {
						var rect = element.getBoundingClientRect();
						var x = ev.clientX - rect.left;
						var y = ev.clientY - rect.top;
						var listeners = _this.listeners;
						for (var i = 0; i < listeners.length; i++) {
							if (_this.buttonDown) {
								if (listeners[i].dragged)
									listeners[i].dragged(x, y);
							}
							else {
								if (listeners[i].moved)
									listeners[i].moved(x, y);
							}
						}
						_this.lastX = x;
						_this.lastY = y;
					}
				};
				var mouseUp = function (ev) {
					if (ev instanceof MouseEvent) {
						var rect = element.getBoundingClientRect();
						var x = ev.clientX - rect.left;
						var y = ev.clientY - rect.top;
						var listeners = _this.listeners;
						for (var i = 0; i < listeners.length; i++) {
							if (listeners[i].up)
								listeners[i].up(x, y);
						}
						_this.lastX = x;
						_this.lastY = y;
						_this.buttonDown = false;
						document.removeEventListener("mousemove", mouseMove);
						document.removeEventListener("mouseup", mouseUp);
					}
				};
				element.addEventListener("mousedown", mouseDown, true);
				element.addEventListener("mousemove", mouseMove, true);
				element.addEventListener("mouseup", mouseUp, true);
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
					for (var i_17 = 0; i_17 < listeners.length; i_17++) {
						if (listeners[i_17].down)
							listeners[i_17].down(_this.currTouch.x, _this.currTouch.y);
					}
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
							for (var i_18 = 0; i_18 < listeners.length; i_18++) {
								if (listeners[i_18].up)
									listeners[i_18].up(x, y);
							}
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
							for (var i_19 = 0; i_19 < listeners.length; i_19++) {
								if (listeners[i_19].up)
									listeners[i_19].up(x, y);
							}
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
							for (var i_20 = 0; i_20 < listeners.length; i_20++) {
								if (listeners[i_20].dragged)
									listeners[i_20].dragged(x, y);
							}
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
				this.angle -= this.timeKeeper.delta / 1.4 * 360 * (1 + 1.5 * Math.pow(a, 5));
				var renderer = this.renderer;
				var canvas = renderer.canvas;
				var gl = renderer.context.gl;
				renderer.resize(webgl.ResizeMode.Stretch);
				var oldX = renderer.camera.position.x, oldY = renderer.camera.position.y;
				renderer.camera.position.set(canvas.width / 2, canvas.height / 2, 0);
				renderer.camera.viewportWidth = canvas.width;
				renderer.camera.viewportHeight = canvas.height;
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
					this.logo = new webgl.GLTexture(renderer.context, LoadingScreen.logoImg);
					this.spinner = new webgl.GLTexture(renderer.context, LoadingScreen.spinnerImg);
				}
				this.logo.update(false);
				this.spinner.update(false);
				var logoWidth = this.logo.getImage().width;
				var logoHeight = this.logo.getImage().height;
				var spinnerWidth = this.spinner.getImage().width;
				var spinnerHeight = this.spinner.getImage().height;
				renderer.batcher.setBlendMode(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
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
			LoadingScreen.SPINNER_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKMAAACjCAYAAADmbK6AAAAACXBIWXMAAAsTAAALEwEAmpwYAAALB2lUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIiB4bWxuczpleGlmPSJodHRwOi8vbnMuYWRvYmUuY29tL2V4aWYvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxNS41IChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTYtMDktMDhUMTQ6MjU6MTIrMDI6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMTEtMTVUMTY6NDA6NTkrMDE6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTExLTE1VDE2OjQwOjU5KzAxOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpmZDhlNTljMC02NGJjLTIxNGQtODAyZi1jZDlhODJjM2ZjMGMiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDpmYmNmZWJlYS03MjY2LWE0NGQtOTI4NS0wOTJmNGNhYzk4ZWEiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2UiIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiIHRpZmY6T3JpZW50YXRpb249IjEiIHRpZmY6WFJlc29sdXRpb249IjcyMDAwMC8xMDAwMCIgdGlmZjpZUmVzb2x1dGlvbj0iNzIwMDAwLzEwMDAwIiB0aWZmOlJlc29sdXRpb25Vbml0PSIyIiBleGlmOkNvbG9yU3BhY2U9IjY1NTM1IiBleGlmOlBpeGVsWERpbWVuc2lvbj0iMjk3IiBleGlmOlBpeGVsWURpbWVuc2lvbj0iMjQyIj4gPHhtcE1NOkhpc3Rvcnk+IDxyZGY6U2VxPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY3JlYXRlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2UiIHN0RXZ0OndoZW49IjIwMTYtMDktMDhUMTQ6MjU6MTIrMDI6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE1LjUgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDpiNThlMTlkNi0xYTRjLTQyNDEtODU0ZC01MDVlZjYxMjRhODQiIHN0RXZ0OndoZW49IjIwMTgtMTEtMTVUMTY6NDA6MjMrMDE6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjQ3YzYzYzIwLWJkYjgtYzM0YS1hYzMyLWQ5MDdjOWEyOTA0MCIgc3RFdnQ6d2hlbj0iMjAxOC0xMS0xNVQxNjo0MDo1OSswMTowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY29udmVydGVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJkZXJpdmVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJjb252ZXJ0ZWQgZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6ZmQ4ZTU5YzAtNjRiYy0yMTRkLTgwMmYtY2Q5YTgyYzNmYzBjIiBzdEV2dDp3aGVuPSIyMDE4LTExLTE1VDE2OjQwOjU5KzAxOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDo0N2M2M2MyMC1iZGI4LWMzNGEtYWMzMi1kOTA3YzlhMjkwNDAiIHN0UmVmOmRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDo2OWRmZjljYy01YzFiLWE5NDctOTc3OS03ODgxZjM0ODk3MDMiIHN0UmVmOm9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2UiLz4gPHBob3Rvc2hvcDpEb2N1bWVudEFuY2VzdG9ycz4gPHJkZjpCYWc+IDxyZGY6bGk+eG1wLmRpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2U8L3JkZjpsaT4gPC9yZGY6QmFnPiA8L3Bob3Rvc2hvcDpEb2N1bWVudEFuY2VzdG9ycz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7qS4aQAAAKZElEQVR42u2de4xVxR3HP8dd3rQryPKo4dGNbtVAQRa1YB93E1tTS7VYqCBiSWhsqGltSx+0xD60tKBorYnNkkBtFUt9xJaGNGlty6EqRAK1KlalshK2C8tzpcIigpz+MbPr5e5y987dM2fv4/tJbjC7v3P2+JvPnTMzZ85MEEURQhQClUpB7gRBAECUYiYwH6gDqoEKoA1oBDYCy4OQJgB92R3yq2S5yRilWASs6CZ0DzA5CNmn/ObOOUpB7kQpRgNLcwj9AHCnMiYZfXIT0C/H2DlRSs0gyeiPaQ6xg4FapUwy+mKUY/wwpUwy+uK4Y/xhpUwy+mKfY3yTUiYZfdHiENsahBxRyiSjL5odYncpXZLRJ3sdYhuVLslYKDKqZpSMBXObVs0oGQumA6OaUTL6Iwg5CBzNMXy7MiYZffNCDjH7g5DdSpVk9M36mGKEZOwxq4Fj3cT8UmmSjEm0Gw8At2UJaQhCtilTeeRWM5EdkmVfOwCIUtQBE4AqILC1ZQuwPgjpSKryWwgy1gfZfjsQ886IKFY2xO9N0jOR69srDOAtzCyYFuCUSrcg6AOcBIYCY4C3gVeT+uNJyvg94GPAxzFjcDuBl4C/AP+UBwXBR4AaYDYwDvgr8Drwi1KScRnwXfut6wNcYT+7Ma97LgX+JRd6jfOAucAXgCvTfl4DvAuMtJVJ0cu41IoYWRHTGWM/1TZmq/2fF8nR14r4U2BQF7+LgMW2k7bY54X4Htr5EvD99s5SlriPArcAY+VGsh1YYDpwMzAgSwy2svhWscpYA/wkx9gKm5S5wBA5kgjnAJcDX7NNpVxcWAZMLUYZJwHDHeKrgXnAdWjZlSS4BLgVuMzRlxt9eeNTxsG2veFyy7gQWAR8Sq54byfeYDssAx3LqLabJldBytgMHMjjuPHAQvTOsU++aJtE/fI4dpevTqZPGV+2veN8+DTwIHCBr29hmVJhJXwA+GAex7cBjxZjm7EFWAL8DfeX39s7NPOy9PKEO7XAV+k8xJYLrcDPgL8Xo4xgJqIuA7bkeXw9ZsBVxMMMYEqex64FfuO7e++bTcAPgD8Bpx2PvRSYKIdi61DOs3edXImAV4Cv2zJsKnYZ24B/AJ+xteRrwAmHBF4mj2JhEnCRg4QnrYh3YZ5NH/J9gUmP5zXYtsdsW+Pl8vffkEex8I5D7HHgGeBhe0dLhKRlbMJM298NXI8Z68rGk8AGeRQLu4DHMGOL2dgJPA78AXguyQvsjScdrTYp2zBDPzfbXl7mmNc64B7MFCbRc/bbfPYHrs343WnbZHsG+BXwZ8y65JS6jOnfwPuBg8BnMQtxjsWsh/0IsNJ2fkR8bAHutbfhG2x7vp9tDzZiFs5/Non2YaHJ2N6OWQf8BxiBeRx4EDPZ9nm544WNVsLtwFWYJ2Wh/fmO3ryw3noHpiv6YyZ5NsuXROhrRypeAv7nfHQJvAOTjbclYuJ3pWcL6YL03rSQjEJIRiEZhZCMQjIKIRmFZBRCMgrJKIRkFJJRCMkoJKMQklFIRiEkoxCSUUhGISSjkIxCSEYhGYWQjEIyCiEZhWQUQjIKySiEZBSSUQjJKCSjEAVCJUAQmCWPoxSjgZuAaZgF348D+zD7ADYDe+2nGWgJQg52dVJvSzOLgqHdmU5ln2IYZou9861Do+x/j8Ss2z7AOrQJWBOEZtetKIrMmt5BEBClWAQsxW3b16OY/QHXA6uD0GzpG0VRPmt6i2KSMeyQrxpYgNl4dCJmV7NcOQEsCULu6ZCR+mAmZiOannAMuC0IWS0Zy0PGKMUCzFZug3p4ullsiJ5obzPOj+H6BgGrohR1KqrSx5bzqhhE7PCvXcY4BZqgoioL4iznunQZq2M8cZXKqSyIs5yr02WsiPHEaiyWSbMxxnNVpMvYFuOJj6mcyoI4y7ktXcbGGE/conIqC+Is58Z0GTfGdNIGzJijKH3W2/KOg43pMi4n//2F92P2KJ4ShCwMQvT4pRwajCFRELIQmGLLf3+ep9pj/TvjCcwI4E5gDp1H0VsxO7k3Zvy7PQjZnXl2DXqXhYydiFKMAcYD44CajH+HZIQfBdYCtwch+854HJh2wkqgFhgGHAaagpAjLhcqGctTxqxOpKgCRgNDMXuK7whCTqU7U9khz3ucAv59xomUe9FVhePGEfs5q1eaQiYKBskoJKMQklFIRiEko5CMQkhGIRmFkIxCMgohGYVkFEIyCskohGQUklEIySiEZBSSUQjJKCSjEJJRSEYhJKOQjEJIRiEZhZCMQjIKIRmFZBSijGXMvIZ+KpZEaF8qeygwHOjb2xdUWQBJqQL6ADOBi4GHMGuGH5Iv3hiG2SJtIWaV4mZgB/AadF6jvVxkvAKzv3UdMNX+bDJm9fx10PV+1qLHIl4P3GLzfh3QBLwKbAZ+DJwuFxkDm5CZmN0Vzsv4/TTMyviVwGOYnRZEPAwBZgDfAC5K+/lo+5kKXAjcBzwPnCz1NuP77LfxO12I2M7FNmFXE+++huVOPfDNDBEz25FzgHuBa4Bzk8x/0jJeCiwCFmP2BsnGh4BbgYFyKDZmZRExnTpbGcywHZySuk0PsbeAG4HZDt+2C6yMb8mjWHgXs+NFd5v09Ac+AYzC7An0EPBKqdSM1wDfBqY7Vvubk263lDhPYHamypVa4MvAHUCq2GvGgcB8YAEwKQ/5nwa33blEVrYDLwJXOhxzLvBJzDhkK/BCMdaMA4C5wF2Y4RrXv7UF+KO9tYh42A08msfoRxVwLfBDYGwxyliLGUMclMexL9rOy075EyvvAKuBlcCbeTa3Pl+MMk7GbP/qyiHg18BWueOFNnu3ymeP8X62h11dbDKm7K3a9Zv7e+BJOeOVRmCNvQO5cgmdt4AueBkH5zCE0FWHpQH4r3zxzlPAw3kcdxg4VmwybnaMfx1YAWxTpyURjtj24wpHuZ7C0yNanzL+FnjZIX4lsEGOJEorcDewKcf4vTb+ZLHJuAeYBxzvJm4/8CPg58AJ+ZE4BzBDNk93k//jwOeAN4qxNw1m5sdV9jZwtlvv48ADujX3GpFtUt0OhPZnJzN63wdtOW7xeSFJPJvehBnBv8/2ricAp2wb8UHgETRvsRDYCiy3IrbPCWi0Mt4BPOf7AoIoivycub5TR/rDmBkjs4Df2fbHJjlQcLwfuNyW13rMXILOkyQ2REUtI5jnnG+mNRFOF3Gh1dlavgozhHUMaLEFGJWImBVnbT4VlYwlSBCYL1iUYgGw6ixhDUHIwo4GmfIrGX3JGKWotj3KbM/cpwQh2yRjYfWmS5EFdD/54ytKk2RMgukxxQjJ2GMm5hAzPEoxRqmSjN6IUgwj9xkr45UxyeiTkQ6x45QuyeiT8x1ia5QuyeiTUaoZJWMxyqiaUTIWzG1aNaNkLJgOzJAoRZVSJhl9McIxfrRSJhl94fq241ClTDL6Yq9jvCYNS0ZvuEwGPopZmlhIRi+sIfeXxtYGIaeUMsnohSCkCViSQ+gezAtOwiW/mvzpkKz3ZnrPxCz1V4dZd6YC8+JSI2YNm+VWXE2ulYyiGPk/nslB8d6ayMkAAAAASUVORK5CYII=";
			LoadingScreen.SPINE_LOGO_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKUAAABsCAYAAAALzHKmAAAACXBIWXMAAAsTAAALEwEAmpwYAAALB2lUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIiB4bWxuczpleGlmPSJodHRwOi8vbnMuYWRvYmUuY29tL2V4aWYvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxNS41IChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTYtMDktMDhUMTQ6MjU6MTIrMDI6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMTEtMTVUMTY6NDA6NTkrMDE6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTExLTE1VDE2OjQwOjU5KzAxOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDowMTdhZGQ3Ni04OTZlLThlNGUtYmM5MS00ZjEyNjI1YjA3MjgiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDplMTViNGE2ZS1hMDg3LWEzNDktODdhOS1mNDYzYjE2MzQ0Y2MiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2UiIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiIHRpZmY6T3JpZW50YXRpb249IjEiIHRpZmY6WFJlc29sdXRpb249IjcyMDAwMC8xMDAwMCIgdGlmZjpZUmVzb2x1dGlvbj0iNzIwMDAwLzEwMDAwIiB0aWZmOlJlc29sdXRpb25Vbml0PSIyIiBleGlmOkNvbG9yU3BhY2U9IjY1NTM1IiBleGlmOlBpeGVsWERpbWVuc2lvbj0iMjk3IiBleGlmOlBpeGVsWURpbWVuc2lvbj0iMjQyIj4gPHhtcE1NOkhpc3Rvcnk+IDxyZGY6U2VxPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY3JlYXRlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2UiIHN0RXZ0OndoZW49IjIwMTYtMDktMDhUMTQ6MjU6MTIrMDI6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE1LjUgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDpiNThlMTlkNi0xYTRjLTQyNDEtODU0ZC01MDVlZjYxMjRhODQiIHN0RXZ0OndoZW49IjIwMTgtMTEtMTVUMTY6NDA6MjMrMDE6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjJlNjJiMWM2LWIxYzQtNDk0MC04MDMxLWU4ZDkyNTBmODJjNSIgc3RFdnQ6d2hlbj0iMjAxOC0xMS0xNVQxNjo0MDo1OSswMTowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY29udmVydGVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJkZXJpdmVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJjb252ZXJ0ZWQgZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6MDE3YWRkNzYtODk2ZS04ZTRlLWJjOTEtNGYxMjYyNWIwNzI4IiBzdEV2dDp3aGVuPSIyMDE4LTExLTE1VDE2OjQwOjU5KzAxOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDoyZTYyYjFjNi1iMWM0LTQ5NDAtODAzMS1lOGQ5MjUwZjgyYzUiIHN0UmVmOmRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDo2OWRmZjljYy01YzFiLWE5NDctOTc3OS03ODgxZjM0ODk3MDMiIHN0UmVmOm9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2UiLz4gPHBob3Rvc2hvcDpEb2N1bWVudEFuY2VzdG9ycz4gPHJkZjpCYWc+IDxyZGY6bGk+eG1wLmRpZDowODMzNWIyYy04NzYyLWQzNGMtOTBhOS02ODJjYjJmYTQ2M2U8L3JkZjpsaT4gPC9yZGY6QmFnPiA8L3Bob3Rvc2hvcDpEb2N1bWVudEFuY2VzdG9ycz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz5ayrctAAATYUlEQVR42u2dfVQV553Hv88AXq5uAAlJ0CBem912jQh60kZ8y0tdC5soJnoaXzC4Tdz4cjya1GN206Zqsu3Jpm6yeM5uTG3iaYGoJNFdEY3GaFGD0p4mqS9AXpoV0OZFUOHS3usFuc/+Idde8M7M8zr3gsw5HOCZZ2aemecz39/LPPMMMLAMLDG2kIFzjqmFDiDZP6AkN3gf0gEob8x2kj4MCx2AMnbb1BcVld6IwJJ+0oYb2YTT/gYq6WPHJP3gmtA+Biztr1CSKLevLytprCkh7ctQkj4KsK590hiGlsbSOcVCR5I+BC7pA6BEAzQaq1DqhFFH3Vg16TSG4KHRgNPpyFd1XdIHAyrdCkhjADgaTSiJw/VIP1BSp6GhUQSOOgmlkzASxSqq2zpQB+ClGiGlUb65tAUZOmDUAa5u5XRSgajibVRCR3VCSRyoQwSBE/EvYy3YkYGESuwrpuAkDgPJCg4RhFVUNUkMw6hK6agDcFInoSQxAqNqWHVdD6fUhQqUsfiaVCN41IlOUBEx88JIJCCU8T+tttOR6pEFUgRQXoCVrydRAJJw/G+2jig6llN+p0wnsZpYXsAoxzGognYzryeagBRRR8L5t4iCRsvflDHnIopINcCpGkzlUOoCkqWcKABdlznXZa5lTK7Z/6zlvMeXXqdTCVWoI696ygZN0YZSp/KxQCijmiJgUp3gyQBpVy4Kq4gPqhpWlQrCCxgPeLz70wqmyqcksgELS5kKQEWCIBn1FEn7qFBKKgmnajCloZQtlwWSZR0PoCJBkJMDMnT4iSxlsQCmFJQidVUASQS3ZSlXadqhWDVkTCoLiDKw8t40XOU6oFQBJMtvkSBJ1ITLqKaOgIbVF+y9jd3/omAqVUtViigTTfMAyKqqKnxOlWZcFEzVZjrSb11gaodSRiVVAikCo4hKyjzpkh3No8tf1AUmrxnXCmW0gSSCcIqki4hipbTqGNU+IwuMqsAUfSLVoywezi46gGSFU8Sk86bBKOd1oJzrwuuEQLIbBU8sfiPC37DYhuW8pEfex3NcQBUqyVrO+7edeZdNIfFCSi22oZwdSkzUk1jAaQcrGMA0O34kUJXAaAYl0aSMkRQMjODxAArGct6onPf68CgLbGCkNv4r4axrp4wwUUc7CAnDdkzXJ14SNFHVEQFNRjHtbg7ZoMfuOlHGDiG9/DPCCDgLjDBROFgon50ZV6mQ1/YVzwmgSniJhFryAMpybB4TLjJLRqTOZPUbZYIrwmiqZYC02lboXOIV0C3qm5nVZQGSSCiuaETOe5PygEg4AbXyM1lhJIxqqiWYUQklUaiShMGc2gFpBbDdcXl9StHXka38KVZ/i8V35DXzZibcClIWtRS90ZQpJa/ysZhtHiBV+pk8imm2TjTFwxsQWIHL42PaRd4iroW0ksZLKAFv5MoKbyQQVZl1mShc5LxYOo4Fxt4KyZPysXMhrOrwqKWyHGa8wiCHVSXtzDaxgYSA36xDEk4V5lvGpxRVIZb8pZ0Z571x7My6Up9S17SBhMGvjASfocCUi0TkvOaZMJh11vSPGVSEcT0s1JYyKKnu1BABQOMloeJ9ssMCg53phoKUkVDQs2MMcvNSsZICwfYufPZVB+o/86HxbAAXP/ah9Z2LuPSnAK5wqB1PLlIkmGEBkzVbwKuWolkE6ddXeYeb2akfEfwRTRnZRf89/r84Bf81NB73WtDQ+VUHKocfw1ob35J3QAXrYApq8X94edBmvVUZS9si/Qbr/wacWXgeN/LCCAHAQ+sNhvqhOiQOcNucZMKwQXh42XCkM95AELjZRFNjRCAPSxSmAbXlKXlNOlF0wj2WoqKi5Hnz5mdTGiQA8OCDDx4T6aiNGzeOufnmm5MBoKysrHbfvn3tVhf40hX8MSked1u1LUhx+e1mXGBIz1znC77xxtaJhmFQwzDo3LmPHBdJ6ezZs2cqIVf3UVt7unH16tWNsB4gwpItsPKdlSfTZd4EZH1MKKJkEX8WLfqnlPXr1/8oNTV1QQ8QgsG2pqamX+TkZG+OtP/y8jcn5efnb+nq6vKmpg7NfeONrZOmT5++3uVyZYTvp76+vjg3d8IWs2vy2DDcsunvUDrIQLrZBT3fgXduO4ZnrEx1aWlpbkHBrM0AkJyclFVZWZl3990TngpvT1dXl7e29vRLU6dOLTcxmT3+P3Hi5NLMzMwlhmEkh7fH7/cfraqqemHevLknTMy10yZci/mO2rR5GzZs2JaamrogGAy2Xbx4cWtTU9OLXq93r2EYyR6P52kLdQQAxMXFJR05cvSRGTNmvOZyuTJ8Pl+d1+utCa0fPXr0kydOnHzSzFRu+RLNM09j7qc+vHY5iIbe7Wu7gt8t+wwbGG9YAEBV1eHvT516z0uh9vj9/tpQW7Ozc54rL39zkt1Dh6+/Pl/h8XieNgwjORAInGpqanqxvb19TzAYbHO73VPz8vK2vfXW29kKUnuOLIZitYWFryjlq1RXV890uVxjAWD37oqFo0Z5fjR2bNYvRozIWLFx48b7zpw5s8EmqgYA5OTkrA8EAud2767452HD0ueOGJHxxLp16x7w+Xx1AODxeB5buXLlCDOf9d2L8H7rd3jFfQSzv/MBpjx7BrP/4yzmP1qP76W8j6U7m3HJzpoEg8Fr5ePHj1/n8/nqtmx5fe6wYemPpKffNreysnJxaP2999672sqi/eEPJ5YkJiZmAcDhw1WP3nrrLQVjx2Ztysi4ffmqVSunBAKBU4ZhJE+bNu1VDj81qosRZfVjyU0CABk6dGgmAHR2djYVFRWdCl+3du1Pzo0bl7PZDPxwCHw+X11R0aOPLFy4sCa0vrj4P8+9++7+jaE6P/jBY3NYgrTft8P3s0Y0rPkcn5R9jRaGtNR159zdnieeeuqpulBZYeGCmsbGxtcBwO12jzFT3Iceejh55MiRTwBAQ0PDzwsKCqrDj1NSUuL98MMPX+hW3pHvvXdwqoK+1jELs3KlVGHmbZPVgUBHGwAkJCRklpSUjBW9MB988PvXwwKaa3UWLVpUEwgEzgFAamrqnWYppZ+Owt8eHoeCfdmY/vYYTH43B9/76Nt4tP5uLHlrDCbyntd77x0oPnDggLd3nbNnz9aG/i4vf3NipG1XrFgxKeRD7tq1a2+k4+Tn570fDAbbAOD222/P5uwTJ9/41BJ9izaOKXVQXFxcWVxc/IxhGMmzZj20+5NPPn21vLx8+9q1Pzlrd/xwpWxtbfWawev3+//kcrkyUlJSJpi1618z8cs4guRIx/mmG34Aky2i0+si1bC29VgX1s4e7Q+vl5aWNiJUmJ2dnVlRUTGiWxUpAISi8M7OzqaQ66O4r7UM4HDyxTEpn+XXv/5V2/Tp/1CYn/+PryQkJGSmp6cvXbVq1dLFixdX19TUbJ49++Fjsvm1L774oqYbSMtcpOk6YrqOuwND6S7W/dx///0l6CdLfBQVkntZuHDhqfnz58/84Q9XP5iZmbkgMTExa8iQIZOnTZs2+fPP/2/7HXd8Y63uNrR04vitgzAt0rqvOnAADgyCjbScOXNmAyGEAoBhGNd+E4Jrqrl//77KGwlK6hSY27Zta922bdtWANsrKiomT5iQ+y+JiYlZaWlp83bs2LlvzpzZx0X3PXz48Nyr/utV3zLS8vgn+Onr3wK9ZRDuI93X7wpFW9Nl7J51GpsQpY+4jxuX8yqsHy9SxMAH5p1KCfGAq3R/BQUF1cuXLy8KOfKjRo3KipDQ7bGkpKQkmbXrpptuGg0AXq+33uyglRfQdtsxPJ15HJOL6pE/4xS+m3AY373jt3j59F/gtzn369oUUrXedQn5a3lYnR7n5fP5rvmdW7ZsyXKYHW1fVjMcbqjyLyjs2PF2W0dHx1nWHdx117cfz8vLS+q9r4MHD82Ji4tLAoDm5uY6WM/6gHMBdJZ+jfN7LqAVzn0cqceyb9871X/NZ9433+6GjCXwoqWUvJ1hCUFjY9O/19XVLSssLOwR+R469JsHQsnjy5cvtyHSY6swNRo8ePCdpaVl5WVlZbmhstLS0gnjx49fBVx9vPfssz/eEaFN17VrrQee34zDA59OwIrWKdjsvwf/uysL90TYhjKCyzPvOH3++efPtrS0bO+OxOedOHFyaaR9VldXz2hsbHpRQf9R8E05I8RFvNM+oY1Pavpik8vlykxJSSl85ZVNz7z00svvB4NBEhcXlxwG5OlJkyZuh/mLUSGTVzd48OA7Z84s+OX5883nuvd97Znz0aNH/u3gwYPeCBexRwDzq7/HXYvS8VrvE5mSjO8DOGzRCT0nc+oOTnp3bASzHrFD16xZs2HTpk1ZiYmJWR6P5+lLl1qXBAKBU6H1brd7Snh1sD2rjqqJNxw6sOzkobSqquoFv99/NHShhwwZMjkEZEtLy/Zly5YtMrubwzv40KFDL3/00UfPdXV1eV0uV0YIyEAgcK6iYtcTs2bN2m+iCD3KvuyAN1LDr1D8xSSwuFYW3p7m5mavHRQXLlxoM1FdunPnjtbly5cXNTQ0/DwYDLYZhpHsdrunhH6Aq4MyPv744yWM6kwZ1VFr7tDub7P/HR8lBIAUFRWlRBi2Fn6DXXec0CghAKisrFxcWLjgOABSVlY2MQRG92M+rhfHGnKxZmQiFgAgXRTeLzuwf+Vn+O//aUErg2ljnemMdZQOBUBLSkrGpqXdkhQCPz8/7wjYBveKjBLinenN1nIAoCpHnvNOEGD2zo0RATKrdbZvPJaXvzk5BOXevXsfnz9/Xg3jednlYsnEJAz5hhvuPRdwsfUKuhhUHzYdZjWvJAuwlBE8ltHoVnDa3UDCUKp8omM3QwPrdlb7sVuHSD5luLns/ttquhIzGCP6eMe9aD/uRTtnMAfoeSXCDkie9rGabuX+qFOPGSMFHdREgVjA6w0N7xt2PLNWUCur8ZwHnu8kYWTbFfiS4zHY3wX/nFr8llEZRGG0U1Fq4xebKR+PD6kN1mg80bEC1Awyq1dCbUG0UEpWv9sUrCcz8OOkePR4Xp79N7jr5J8RsIFSdo5yW//SQkV5VZIKmmKhaDxeEkKr90/AYM5Z1NIOFtuX4ktLS08TQhZRSklpaWkt+N+tNl28XfhjOJS+LtSf/DMuC4Aoo5i8QFKbDIFTSfbIT7M4Ah2WYEck+FH9Zh/AN+EVU6RtBuo3B2PQ1tGYlZYAT3sXvljXgMqdzWiTMN0qfEuegEVHlC38eq1IR7BOJgAOIKEATqt9mKWw7CJuFZPx83x+xA5Klq8+iAIJsL8kZrdOGso4zo5gnQhV9qsOVuMheYbYs3yvmmc9lagn+iUGarMPVsW0y5FSAUXXYuLjBXZMBLdhmU02UtBjFQzx+ps850EtoLfzpbnVgUN5VOQxWdVR9MtmUiki1Skhq3wiTIBkgRMCKR/CWM6bV+W581kHL7DkMXk+1sQKJK9VcWQEEq/5FjXhIsGF7Ddt7MDhufAqTBYFlHzuWORLYpRBSXnNtowvKaWULDN42W3D+hkNMOQhAfNEN8/stay5U5nv3/AGPLI5TFa/kgrUlb05uW7gOEF1UqWWdhOk8kS9Ks0uT3BDGbbn8Sl54VTla1qZZ542Sy9xnGkgcAAkOoMukQBT1L+TMfci7gGvOecxsSzmXTaYYTk/nuvODSVLmchH5cH5t+hMuyyjuFmdedFXGyij/waoiXhlHlOyHgsMbY5q9G3le/LOu83ywSHRNBXLY1GRtA9vwMPaqU59wVZFG6DoWkkppajS8XyHW8V3t4lEekP09VS7kTp2Ebmsvyli0kWyBSqsyHVlcYIAyviWsmASThhVBjY84wtZ9suaK5RJy4iaaNa8pVKVNINSRi11gSkSheu4o82UkAVmnhymKIgi0TnA/8hRNPKmqqHkVUsnwBR91Meqjiocd5ZASgQKFT4nT1DDA6TUdSOaymXAFEkniZp7FSOBdAU9LOkVqgBQp4BkLieKgLUqkzXvVuDx7EMEQl35URHoIAmODMAqFJIZyjjNKqriE8a8yXynAxsIdgRrp/KabxkYow6kjFKIqqjKZDnhvAFELYNO8w3Jjuc15yLmmjWoUQZlnIT5UgGmjGqyjLtUrXy6oGRRTl2QivqwrJaJG2KZ5DQvsKwmmccHZVVD2fSSLmXk6XxRSHgVU5U6iqqnFJSyYKqAU+QGiJVAh2oClUdhqeLjSgOpSjFkTbwOVRXNGEDB9aCSwFIFHa3DFZBRfi1Q6gBTFk4Rs63zGijrFIg/ylRt7lW3m6kOUagQqiJ5orFONKJtHR0ok/vUAaPKOrbRt2owZZVTJmhRDaKOYW26I1st06yoBFKmk4jD61UCShSfq1OdpTLgUDW6R8t87rqcfZ1BlMr6uq6Vjhf2owGvozDKmG9dyiQCeTSAiwXVdNIP1A2uls7QkYhW/fgzVgIeXVOe6ISFOnSOjjn+uuHsK5F2NM1hLG/jSGfpjoSdjLSJg7Cp7FjaR7ZzXEGcinBJDF8DnZ1Ho7wPrYNadHdINGCLdVMdrU6nMdimqHYgiaF2kn4IXJ8FMJY6iPRxsPqTksbc55ZJP2vHgOnuYwD2tU4k/eycaT891g0F5YDZ7qfQ3SidTAZgG4By4FwHgBtYBpYbZ/l/2EJnC9N0gaQAAAAASUVORK5CYII=";
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
			function Mesh(context, attributes, maxVertices, maxIndices) {
				this.attributes = attributes;
				this.verticesLength = 0;
				this.dirtyVertices = false;
				this.indicesLength = 0;
				this.dirtyIndices = false;
				this.elementsPerVertex = 0;
				this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
				this.elementsPerVertex = 0;
				for (var i = 0; i < attributes.length; i++) {
					this.elementsPerVertex += attributes[i].numElements;
				}
				this.vertices = new Float32Array(maxVertices * this.elementsPerVertex);
				this.indices = new Uint16Array(maxIndices);
				this.context.addRestorable(this);
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
			Mesh.prototype.getVertexSizeInFloats = function () {
				var size = 0;
				for (var i = 0; i < this.attributes.length; i++) {
					var attribute = this.attributes[i];
					size += attribute.numElements;
				}
				return size;
			};
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
				var gl = this.context.gl;
				if (this.dirtyVertices || this.dirtyIndices)
					this.update();
				this.bind(shader);
				if (this.indicesLength > 0) {
					gl.drawElements(primitiveType, count, gl.UNSIGNED_SHORT, offset * 2);
				}
				else {
					gl.drawArrays(primitiveType, offset, count);
				}
				this.unbind(shader);
			};
			Mesh.prototype.bind = function (shader) {
				var gl = this.context.gl;
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
				var gl = this.context.gl;
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
				var gl = this.context.gl;
				if (this.dirtyVertices) {
					if (!this.verticesBuffer) {
						this.verticesBuffer = gl.createBuffer();
					}
					gl.bindBuffer(gl.ARRAY_BUFFER, this.verticesBuffer);
					gl.bufferData(gl.ARRAY_BUFFER, this.vertices.subarray(0, this.verticesLength), gl.DYNAMIC_DRAW);
					this.dirtyVertices = false;
				}
				if (this.dirtyIndices) {
					if (!this.indicesBuffer) {
						this.indicesBuffer = gl.createBuffer();
					}
					gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indicesBuffer);
					gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, this.indices.subarray(0, this.indicesLength), gl.DYNAMIC_DRAW);
					this.dirtyIndices = false;
				}
			};
			Mesh.prototype.restore = function () {
				this.verticesBuffer = null;
				this.indicesBuffer = null;
				this.update();
			};
			Mesh.prototype.dispose = function () {
				this.context.removeRestorable(this);
				var gl = this.context.gl;
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
				return _super.call(this, webgl.Shader.POSITION, VertexAttributeType.Float, 2) || this;
			}
			return Position2Attribute;
		}(VertexAttribute));
		webgl.Position2Attribute = Position2Attribute;
		var Position3Attribute = (function (_super) {
			__extends(Position3Attribute, _super);
			function Position3Attribute() {
				return _super.call(this, webgl.Shader.POSITION, VertexAttributeType.Float, 3) || this;
			}
			return Position3Attribute;
		}(VertexAttribute));
		webgl.Position3Attribute = Position3Attribute;
		var TexCoordAttribute = (function (_super) {
			__extends(TexCoordAttribute, _super);
			function TexCoordAttribute(unit) {
				if (unit === void 0) { unit = 0; }
				return _super.call(this, webgl.Shader.TEXCOORDS + (unit == 0 ? "" : unit), VertexAttributeType.Float, 2) || this;
			}
			return TexCoordAttribute;
		}(VertexAttribute));
		webgl.TexCoordAttribute = TexCoordAttribute;
		var ColorAttribute = (function (_super) {
			__extends(ColorAttribute, _super);
			function ColorAttribute() {
				return _super.call(this, webgl.Shader.COLOR, VertexAttributeType.Float, 4) || this;
			}
			return ColorAttribute;
		}(VertexAttribute));
		webgl.ColorAttribute = ColorAttribute;
		var Color2Attribute = (function (_super) {
			__extends(Color2Attribute, _super);
			function Color2Attribute() {
				return _super.call(this, webgl.Shader.COLOR2, VertexAttributeType.Float, 4) || this;
			}
			return Color2Attribute;
		}(VertexAttribute));
		webgl.Color2Attribute = Color2Attribute;
		var VertexAttributeType;
		(function (VertexAttributeType) {
			VertexAttributeType[VertexAttributeType["Float"] = 0] = "Float";
		})(VertexAttributeType = webgl.VertexAttributeType || (webgl.VertexAttributeType = {}));
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var PolygonBatcher = (function () {
			function PolygonBatcher(context, twoColorTint, maxVertices) {
				if (twoColorTint === void 0) { twoColorTint = true; }
				if (maxVertices === void 0) { maxVertices = 10920; }
				this.isDrawing = false;
				this.shader = null;
				this.lastTexture = null;
				this.verticesLength = 0;
				this.indicesLength = 0;
				if (maxVertices > 10920)
					throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
				this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
				var attributes = twoColorTint ?
					[new webgl.Position2Attribute(), new webgl.ColorAttribute(), new webgl.TexCoordAttribute(), new webgl.Color2Attribute()] :
					[new webgl.Position2Attribute(), new webgl.ColorAttribute(), new webgl.TexCoordAttribute()];
				this.mesh = new webgl.Mesh(context, attributes, maxVertices, maxVertices * 3);
				this.srcBlend = this.context.gl.SRC_ALPHA;
				this.dstBlend = this.context.gl.ONE_MINUS_SRC_ALPHA;
			}
			PolygonBatcher.prototype.begin = function (shader) {
				var gl = this.context.gl;
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
				var gl = this.context.gl;
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
				var gl = this.context.gl;
				if (this.verticesLength == 0)
					return;
				this.lastTexture.bind();
				this.mesh.draw(this.shader, gl.TRIANGLES);
				this.verticesLength = 0;
				this.indicesLength = 0;
				this.mesh.setVerticesLength(0);
				this.mesh.setIndicesLength(0);
				this.drawCalls++;
			};
			PolygonBatcher.prototype.end = function () {
				var gl = this.context.gl;
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
			function SceneRenderer(canvas, context, twoColorTint) {
				if (twoColorTint === void 0) { twoColorTint = true; }
				this.twoColorTint = false;
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
				this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
				this.twoColorTint = twoColorTint;
				this.camera = new webgl.OrthoCamera(canvas.width, canvas.height);
				this.batcherShader = twoColorTint ? webgl.Shader.newTwoColoredTextured(this.context) : webgl.Shader.newColoredTextured(this.context);
				this.batcher = new webgl.PolygonBatcher(this.context, twoColorTint);
				this.shapesShader = webgl.Shader.newColored(this.context);
				this.shapes = new webgl.ShapeRenderer(this.context);
				this.skeletonRenderer = new webgl.SkeletonRenderer(this.context, twoColorTint);
				this.skeletonDebugRenderer = new webgl.SkeletonDebugRenderer(this.context);
			}
			SceneRenderer.prototype.begin = function () {
				this.camera.update();
				this.enableRenderer(this.batcher);
			};
			SceneRenderer.prototype.drawSkeleton = function (skeleton, premultipliedAlpha, slotRangeStart, slotRangeEnd) {
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				if (slotRangeStart === void 0) { slotRangeStart = -1; }
				if (slotRangeEnd === void 0) { slotRangeEnd = -1; }
				this.enableRenderer(this.batcher);
				this.skeletonRenderer.premultipliedAlpha = premultipliedAlpha;
				this.skeletonRenderer.draw(this.batcher, skeleton, slotRangeStart, slotRangeEnd);
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
				var i = 0;
				quad[i++] = x;
				quad[i++] = y;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 0;
				quad[i++] = 1;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x + width;
				quad[i++] = y;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 1;
				quad[i++] = 1;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x + width;
				quad[i++] = y + height;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 1;
				quad[i++] = 0;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x;
				quad[i++] = y + height;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 0;
				quad[i++] = 0;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawTextureUV = function (texture, x, y, width, height, u, v, u2, v2, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = this.WHITE;
				var quad = this.QUAD;
				var i = 0;
				quad[i++] = x;
				quad[i++] = y;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = u;
				quad[i++] = v;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x + width;
				quad[i++] = y;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = u2;
				quad[i++] = v;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x + width;
				quad[i++] = y + height;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = u2;
				quad[i++] = v2;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x;
				quad[i++] = y + height;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = u;
				quad[i++] = v2;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
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
				var i = 0;
				quad[i++] = x1;
				quad[i++] = y1;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 0;
				quad[i++] = 1;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x2;
				quad[i++] = y2;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 1;
				quad[i++] = 1;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x3;
				quad[i++] = y3;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 1;
				quad[i++] = 0;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x4;
				quad[i++] = y4;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = 0;
				quad[i++] = 0;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawRegion = function (region, x, y, width, height, color, premultipliedAlpha) {
				if (color === void 0) { color = null; }
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = this.WHITE;
				var quad = this.QUAD;
				var i = 0;
				quad[i++] = x;
				quad[i++] = y;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = region.u;
				quad[i++] = region.v2;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x + width;
				quad[i++] = y;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = region.u2;
				quad[i++] = region.v2;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x + width;
				quad[i++] = y + height;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = region.u2;
				quad[i++] = region.v;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
				quad[i++] = x;
				quad[i++] = y + height;
				quad[i++] = color.r;
				quad[i++] = color.g;
				quad[i++] = color.b;
				quad[i++] = color.a;
				quad[i++] = region.u;
				quad[i++] = region.v;
				if (this.twoColorTint) {
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
					quad[i++] = 0;
				}
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
				this.context.gl.viewport(0, 0, canvas.width, canvas.height);
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
					this.batcherShader.setUniformi("u_texture", 0);
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
		var ResizeMode;
		(function (ResizeMode) {
			ResizeMode[ResizeMode["Stretch"] = 0] = "Stretch";
			ResizeMode[ResizeMode["Expand"] = 1] = "Expand";
			ResizeMode[ResizeMode["Fit"] = 2] = "Fit";
		})(ResizeMode = webgl.ResizeMode || (webgl.ResizeMode = {}));
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var Shader = (function () {
			function Shader(context, vertexShader, fragmentShader) {
				this.vertexShader = vertexShader;
				this.fragmentShader = fragmentShader;
				this.vs = null;
				this.fs = null;
				this.program = null;
				this.tmp2x2 = new Float32Array(2 * 2);
				this.tmp3x3 = new Float32Array(3 * 3);
				this.tmp4x4 = new Float32Array(4 * 4);
				this.vsSource = vertexShader;
				this.fsSource = fragmentShader;
				this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
				this.context.addRestorable(this);
				this.compile();
			}
			Shader.prototype.getProgram = function () { return this.program; };
			Shader.prototype.getVertexShader = function () { return this.vertexShader; };
			Shader.prototype.getFragmentShader = function () { return this.fragmentShader; };
			Shader.prototype.getVertexShaderSource = function () { return this.vsSource; };
			Shader.prototype.getFragmentSource = function () { return this.fsSource; };
			Shader.prototype.compile = function () {
				var gl = this.context.gl;
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
				var gl = this.context.gl;
				var shader = gl.createShader(type);
				gl.shaderSource(shader, source);
				gl.compileShader(shader);
				if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
					var error = "Couldn't compile shader: " + gl.getShaderInfoLog(shader);
					gl.deleteShader(shader);
					if (!gl.isContextLost())
						throw new Error(error);
				}
				return shader;
			};
			Shader.prototype.compileProgram = function (vs, fs) {
				var gl = this.context.gl;
				var program = gl.createProgram();
				gl.attachShader(program, vs);
				gl.attachShader(program, fs);
				gl.linkProgram(program);
				if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
					var error = "Couldn't compile shader program: " + gl.getProgramInfoLog(program);
					gl.deleteProgram(program);
					if (!gl.isContextLost())
						throw new Error(error);
				}
				return program;
			};
			Shader.prototype.restore = function () {
				this.compile();
			};
			Shader.prototype.bind = function () {
				this.context.gl.useProgram(this.program);
			};
			Shader.prototype.unbind = function () {
				this.context.gl.useProgram(null);
			};
			Shader.prototype.setUniformi = function (uniform, value) {
				this.context.gl.uniform1i(this.getUniformLocation(uniform), value);
			};
			Shader.prototype.setUniformf = function (uniform, value) {
				this.context.gl.uniform1f(this.getUniformLocation(uniform), value);
			};
			Shader.prototype.setUniform2f = function (uniform, value, value2) {
				this.context.gl.uniform2f(this.getUniformLocation(uniform), value, value2);
			};
			Shader.prototype.setUniform3f = function (uniform, value, value2, value3) {
				this.context.gl.uniform3f(this.getUniformLocation(uniform), value, value2, value3);
			};
			Shader.prototype.setUniform4f = function (uniform, value, value2, value3, value4) {
				this.context.gl.uniform4f(this.getUniformLocation(uniform), value, value2, value3, value4);
			};
			Shader.prototype.setUniform2x2f = function (uniform, value) {
				var gl = this.context.gl;
				this.tmp2x2.set(value);
				gl.uniformMatrix2fv(this.getUniformLocation(uniform), false, this.tmp2x2);
			};
			Shader.prototype.setUniform3x3f = function (uniform, value) {
				var gl = this.context.gl;
				this.tmp3x3.set(value);
				gl.uniformMatrix3fv(this.getUniformLocation(uniform), false, this.tmp3x3);
			};
			Shader.prototype.setUniform4x4f = function (uniform, value) {
				var gl = this.context.gl;
				this.tmp4x4.set(value);
				gl.uniformMatrix4fv(this.getUniformLocation(uniform), false, this.tmp4x4);
			};
			Shader.prototype.getUniformLocation = function (uniform) {
				var gl = this.context.gl;
				var location = gl.getUniformLocation(this.program, uniform);
				if (!location && !gl.isContextLost())
					throw new Error("Couldn't find location for uniform " + uniform);
				return location;
			};
			Shader.prototype.getAttributeLocation = function (attribute) {
				var gl = this.context.gl;
				var location = gl.getAttribLocation(this.program, attribute);
				if (location == -1 && !gl.isContextLost())
					throw new Error("Couldn't find location for attribute " + attribute);
				return location;
			};
			Shader.prototype.dispose = function () {
				this.context.removeRestorable(this);
				var gl = this.context.gl;
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
			Shader.newColoredTextured = function (context) {
				var vs = "\n\t\t\t\tattribute vec4 " + Shader.POSITION + ";\n\t\t\t\tattribute vec4 " + Shader.COLOR + ";\n\t\t\t\tattribute vec2 " + Shader.TEXCOORDS + ";\n\t\t\t\tuniform mat4 " + Shader.MVP_MATRIX + ";\n\t\t\t\tvarying vec4 v_color;\n\t\t\t\tvarying vec2 v_texCoords;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tv_color = " + Shader.COLOR + ";\n\t\t\t\t\tv_texCoords = " + Shader.TEXCOORDS + ";\n\t\t\t\t\tgl_Position = " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n\t\t\t\t}\n\t\t\t";
				var fs = "\n\t\t\t\t#ifdef GL_ES\n\t\t\t\t\t#define LOWP lowp\n\t\t\t\t\tprecision mediump float;\n\t\t\t\t#else\n\t\t\t\t\t#define LOWP\n\t\t\t\t#endif\n\t\t\t\tvarying LOWP vec4 v_color;\n\t\t\t\tvarying vec2 v_texCoords;\n\t\t\t\tuniform sampler2D u_texture;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tgl_FragColor = v_color * texture2D(u_texture, v_texCoords);\n\t\t\t\t}\n\t\t\t";
				return new Shader(context, vs, fs);
			};
			Shader.newTwoColoredTextured = function (context) {
				var vs = "\n\t\t\t\tattribute vec4 " + Shader.POSITION + ";\n\t\t\t\tattribute vec4 " + Shader.COLOR + ";\n\t\t\t\tattribute vec4 " + Shader.COLOR2 + ";\n\t\t\t\tattribute vec2 " + Shader.TEXCOORDS + ";\n\t\t\t\tuniform mat4 " + Shader.MVP_MATRIX + ";\n\t\t\t\tvarying vec4 v_light;\n\t\t\t\tvarying vec4 v_dark;\n\t\t\t\tvarying vec2 v_texCoords;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tv_light = " + Shader.COLOR + ";\n\t\t\t\t\tv_dark = " + Shader.COLOR2 + ";\n\t\t\t\t\tv_texCoords = " + Shader.TEXCOORDS + ";\n\t\t\t\t\tgl_Position = " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n\t\t\t\t}\n\t\t\t";
				var fs = "\n\t\t\t\t#ifdef GL_ES\n\t\t\t\t\t#define LOWP lowp\n\t\t\t\t\tprecision mediump float;\n\t\t\t\t#else\n\t\t\t\t\t#define LOWP\n\t\t\t\t#endif\n\t\t\t\tvarying LOWP vec4 v_light;\n\t\t\t\tvarying LOWP vec4 v_dark;\n\t\t\t\tvarying vec2 v_texCoords;\n\t\t\t\tuniform sampler2D u_texture;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tvec4 texColor = texture2D(u_texture, v_texCoords);\n\t\t\t\t\tgl_FragColor.a = texColor.a * v_light.a;\n\t\t\t\t\tgl_FragColor.rgb = ((texColor.a - 1.0) * v_dark.a + 1.0 - texColor.rgb) * v_dark.rgb + texColor.rgb * v_light.rgb;\n\t\t\t\t}\n\t\t\t";
				return new Shader(context, vs, fs);
			};
			Shader.newColored = function (context) {
				var vs = "\n\t\t\t\tattribute vec4 " + Shader.POSITION + ";\n\t\t\t\tattribute vec4 " + Shader.COLOR + ";\n\t\t\t\tuniform mat4 " + Shader.MVP_MATRIX + ";\n\t\t\t\tvarying vec4 v_color;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tv_color = " + Shader.COLOR + ";\n\t\t\t\t\tgl_Position = " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n\t\t\t\t}\n\t\t\t";
				var fs = "\n\t\t\t\t#ifdef GL_ES\n\t\t\t\t\t#define LOWP lowp\n\t\t\t\t\tprecision mediump float;\n\t\t\t\t#else\n\t\t\t\t\t#define LOWP\n\t\t\t\t#endif\n\t\t\t\tvarying LOWP vec4 v_color;\n\n\t\t\t\tvoid main () {\n\t\t\t\t\tgl_FragColor = v_color;\n\t\t\t\t}\n\t\t\t";
				return new Shader(context, vs, fs);
			};
			Shader.MVP_MATRIX = "u_projTrans";
			Shader.POSITION = "a_position";
			Shader.COLOR = "a_color";
			Shader.COLOR2 = "a_color2";
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
			function ShapeRenderer(context, maxVertices) {
				if (maxVertices === void 0) { maxVertices = 10920; }
				this.isDrawing = false;
				this.shapeType = ShapeType.Filled;
				this.color = new spine.Color(1, 1, 1, 1);
				this.vertexIndex = 0;
				this.tmp = new spine.Vector2();
				if (maxVertices > 10920)
					throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
				this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
				this.mesh = new webgl.Mesh(context, [new webgl.Position2Attribute(), new webgl.ColorAttribute()], maxVertices, 0);
				this.srcBlend = this.context.gl.SRC_ALPHA;
				this.dstBlend = this.context.gl.ONE_MINUS_SRC_ALPHA;
			}
			ShapeRenderer.prototype.begin = function (shader) {
				if (this.isDrawing)
					throw new Error("ShapeRenderer.begin() has already been called");
				this.shader = shader;
				this.vertexIndex = 0;
				this.isDrawing = true;
				var gl = this.context.gl;
				gl.enable(gl.BLEND);
				gl.blendFunc(this.srcBlend, this.dstBlend);
			};
			ShapeRenderer.prototype.setBlendMode = function (srcBlend, dstBlend) {
				var gl = this.context.gl;
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
				this.context.gl.disable(this.context.gl.BLEND);
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
		var ShapeType;
		(function (ShapeType) {
			ShapeType[ShapeType["Point"] = 0] = "Point";
			ShapeType[ShapeType["Line"] = 1] = "Line";
			ShapeType[ShapeType["Filled"] = 4] = "Filled";
		})(ShapeType = webgl.ShapeType || (webgl.ShapeType = {}));
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var webgl;
	(function (webgl) {
		var SkeletonDebugRenderer = (function () {
			function SkeletonDebugRenderer(context) {
				this.boneLineColor = new spine.Color(1, 0, 0, 1);
				this.boneOriginColor = new spine.Color(0, 1, 0, 1);
				this.attachmentLineColor = new spine.Color(0, 0, 1, 0.5);
				this.triangleLineColor = new spine.Color(1, 0.64, 0, 0.5);
				this.pathColor = new spine.Color().setFromString("FF7F00");
				this.clipColor = new spine.Color(0.8, 0, 0, 2);
				this.aabbColor = new spine.Color(0, 1, 0, 0.5);
				this.drawBones = true;
				this.drawRegionAttachments = true;
				this.drawBoundingBoxes = true;
				this.drawMeshHull = true;
				this.drawMeshTriangles = true;
				this.drawPaths = true;
				this.drawSkeletonXY = false;
				this.drawClipping = true;
				this.premultipliedAlpha = false;
				this.scale = 1;
				this.boneWidth = 2;
				this.bounds = new spine.SkeletonBounds();
				this.temp = new Array();
				this.vertices = spine.Utils.newFloatArray(2 * 1024);
				this.context = context instanceof webgl.ManagedWebGLRenderingContext ? context : new webgl.ManagedWebGLRenderingContext(context);
			}
			SkeletonDebugRenderer.prototype.draw = function (shapes, skeleton, ignoredBones) {
				if (ignoredBones === void 0) { ignoredBones = null; }
				var skeletonX = skeleton.x;
				var skeletonY = skeleton.y;
				var gl = this.context.gl;
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
							var vertices = this.vertices;
							regionAttachment.computeWorldVertices(slot.bone, vertices, 0, 2);
							shapes.line(vertices[0], vertices[1], vertices[2], vertices[3]);
							shapes.line(vertices[2], vertices[3], vertices[4], vertices[5]);
							shapes.line(vertices[4], vertices[5], vertices[6], vertices[7]);
							shapes.line(vertices[6], vertices[7], vertices[0], vertices[1]);
						}
					}
				}
				if (this.drawMeshHull || this.drawMeshTriangles) {
					var slots = skeleton.slots;
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						if (!slot.bone.active)
							continue;
						var attachment = slot.getAttachment();
						if (!(attachment instanceof spine.MeshAttachment))
							continue;
						var mesh = attachment;
						var vertices = this.vertices;
						mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, 2);
						var triangles = mesh.triangles;
						var hullLength = mesh.hullLength;
						if (this.drawMeshTriangles) {
							shapes.setColor(this.triangleLineColor);
							for (var ii = 0, nn = triangles.length; ii < nn; ii += 3) {
								var v1 = triangles[ii] * 2, v2 = triangles[ii + 1] * 2, v3 = triangles[ii + 2] * 2;
								shapes.triangle(false, vertices[v1], vertices[v1 + 1], vertices[v2], vertices[v2 + 1], vertices[v3], vertices[v3 + 1]);
							}
						}
						if (this.drawMeshHull && hullLength > 0) {
							shapes.setColor(this.attachmentLineColor);
							hullLength = (hullLength >> 1) * 2;
							var lastX = vertices[hullLength - 2], lastY = vertices[hullLength - 1];
							for (var ii = 0, nn = hullLength; ii < nn; ii += 2) {
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
						if (!slot.bone.active)
							continue;
						var attachment = slot.getAttachment();
						if (!(attachment instanceof spine.PathAttachment))
							continue;
						var path = attachment;
						var nn = path.worldVerticesLength;
						var world = this.temp = spine.Utils.setArraySize(this.temp, nn, 0);
						path.computeWorldVertices(slot, 0, nn, world, 0, 2);
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
				if (this.drawClipping) {
					var slots = skeleton.slots;
					shapes.setColor(this.clipColor);
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						if (!slot.bone.active)
							continue;
						var attachment = slot.getAttachment();
						if (!(attachment instanceof spine.ClippingAttachment))
							continue;
						var clip = attachment;
						var nn = clip.worldVerticesLength;
						var world = this.temp = spine.Utils.setArraySize(this.temp, nn, 0);
						clip.computeWorldVertices(slot, 0, nn, world, 0, 2);
						for (var i_21 = 0, n_3 = world.length; i_21 < n_3; i_21 += 2) {
							var x = world[i_21];
							var y = world[i_21 + 1];
							var x2 = world[(i_21 + 2) % world.length];
							var y2 = world[(i_21 + 3) % world.length];
							shapes.line(x, y, x2, y2);
						}
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
		var Renderable = (function () {
			function Renderable(vertices, numVertices, numFloats) {
				this.vertices = vertices;
				this.numVertices = numVertices;
				this.numFloats = numFloats;
			}
			return Renderable;
		}());
		;
		var SkeletonRenderer = (function () {
			function SkeletonRenderer(context, twoColorTint) {
				if (twoColorTint === void 0) { twoColorTint = true; }
				this.premultipliedAlpha = false;
				this.vertexEffect = null;
				this.tempColor = new spine.Color();
				this.tempColor2 = new spine.Color();
				this.vertexSize = 2 + 2 + 4;
				this.twoColorTint = false;
				this.renderable = new Renderable(null, 0, 0);
				this.clipper = new spine.SkeletonClipping();
				this.temp = new spine.Vector2();
				this.temp2 = new spine.Vector2();
				this.temp3 = new spine.Color();
				this.temp4 = new spine.Color();
				this.twoColorTint = twoColorTint;
				if (twoColorTint)
					this.vertexSize += 4;
				this.vertices = spine.Utils.newFloatArray(this.vertexSize * 1024);
			}
			SkeletonRenderer.prototype.draw = function (batcher, skeleton, slotRangeStart, slotRangeEnd) {
				if (slotRangeStart === void 0) { slotRangeStart = -1; }
				if (slotRangeEnd === void 0) { slotRangeEnd = -1; }
				var clipper = this.clipper;
				var premultipliedAlpha = this.premultipliedAlpha;
				var twoColorTint = this.twoColorTint;
				var blendMode = null;
				var tempPos = this.temp;
				var tempUv = this.temp2;
				var tempLight = this.temp3;
				var tempDark = this.temp4;
				var renderable = this.renderable;
				var uvs = null;
				var triangles = null;
				var drawOrder = skeleton.drawOrder;
				var attachmentColor = null;
				var skeletonColor = skeleton.color;
				var vertexSize = twoColorTint ? 12 : 8;
				var inRange = false;
				if (slotRangeStart == -1)
					inRange = true;
				for (var i = 0, n = drawOrder.length; i < n; i++) {
					var clippedVertexSize = clipper.isClipping() ? 2 : vertexSize;
					var slot = drawOrder[i];
					if (!slot.bone.active) {
						clipper.clipEndWithSlot(slot);
						continue;
					}
					if (slotRangeStart >= 0 && slotRangeStart == slot.data.index) {
						inRange = true;
					}
					if (!inRange) {
						clipper.clipEndWithSlot(slot);
						continue;
					}
					if (slotRangeEnd >= 0 && slotRangeEnd == slot.data.index) {
						inRange = false;
					}
					var attachment = slot.getAttachment();
					var texture = null;
					if (attachment instanceof spine.RegionAttachment) {
						var region = attachment;
						renderable.vertices = this.vertices;
						renderable.numVertices = 4;
						renderable.numFloats = clippedVertexSize << 2;
						region.computeWorldVertices(slot.bone, renderable.vertices, 0, clippedVertexSize);
						triangles = SkeletonRenderer.QUAD_TRIANGLES;
						uvs = region.uvs;
						texture = region.region.renderObject.texture;
						attachmentColor = region.color;
					}
					else if (attachment instanceof spine.MeshAttachment) {
						var mesh = attachment;
						renderable.vertices = this.vertices;
						renderable.numVertices = (mesh.worldVerticesLength >> 1);
						renderable.numFloats = renderable.numVertices * clippedVertexSize;
						if (renderable.numFloats > renderable.vertices.length) {
							renderable.vertices = this.vertices = spine.Utils.newFloatArray(renderable.numFloats);
						}
						mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, renderable.vertices, 0, clippedVertexSize);
						triangles = mesh.triangles;
						texture = mesh.region.renderObject.texture;
						uvs = mesh.uvs;
						attachmentColor = mesh.color;
					}
					else if (attachment instanceof spine.ClippingAttachment) {
						var clip = (attachment);
						clipper.clipStart(slot, clip);
						continue;
					}
					else {
						clipper.clipEndWithSlot(slot);
						continue;
					}
					if (texture != null) {
						var slotColor = slot.color;
						var finalColor = this.tempColor;
						finalColor.r = skeletonColor.r * slotColor.r * attachmentColor.r;
						finalColor.g = skeletonColor.g * slotColor.g * attachmentColor.g;
						finalColor.b = skeletonColor.b * slotColor.b * attachmentColor.b;
						finalColor.a = skeletonColor.a * slotColor.a * attachmentColor.a;
						if (premultipliedAlpha) {
							finalColor.r *= finalColor.a;
							finalColor.g *= finalColor.a;
							finalColor.b *= finalColor.a;
						}
						var darkColor = this.tempColor2;
						if (slot.darkColor == null)
							darkColor.set(0, 0, 0, 1.0);
						else {
							if (premultipliedAlpha) {
								darkColor.r = slot.darkColor.r * finalColor.a;
								darkColor.g = slot.darkColor.g * finalColor.a;
								darkColor.b = slot.darkColor.b * finalColor.a;
							}
							else {
								darkColor.setFromColor(slot.darkColor);
							}
							darkColor.a = premultipliedAlpha ? 1.0 : 0.0;
						}
						var slotBlendMode = slot.data.blendMode;
						if (slotBlendMode != blendMode) {
							blendMode = slotBlendMode;
							batcher.setBlendMode(webgl.WebGLBlendModeConverter.getSourceGLBlendMode(blendMode, premultipliedAlpha), webgl.WebGLBlendModeConverter.getDestGLBlendMode(blendMode));
						}
						if (clipper.isClipping()) {
							clipper.clipTriangles(renderable.vertices, renderable.numFloats, triangles, triangles.length, uvs, finalColor, darkColor, twoColorTint);
							var clippedVertices = new Float32Array(clipper.clippedVertices);
							var clippedTriangles = clipper.clippedTriangles;
							if (this.vertexEffect != null) {
								var vertexEffect = this.vertexEffect;
								var verts = clippedVertices;
								if (!twoColorTint) {
									for (var v = 0, n_4 = clippedVertices.length; v < n_4; v += vertexSize) {
										tempPos.x = verts[v];
										tempPos.y = verts[v + 1];
										tempLight.set(verts[v + 2], verts[v + 3], verts[v + 4], verts[v + 5]);
										tempUv.x = verts[v + 6];
										tempUv.y = verts[v + 7];
										tempDark.set(0, 0, 0, 0);
										vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
										verts[v] = tempPos.x;
										verts[v + 1] = tempPos.y;
										verts[v + 2] = tempLight.r;
										verts[v + 3] = tempLight.g;
										verts[v + 4] = tempLight.b;
										verts[v + 5] = tempLight.a;
										verts[v + 6] = tempUv.x;
										verts[v + 7] = tempUv.y;
									}
								}
								else {
									for (var v = 0, n_5 = clippedVertices.length; v < n_5; v += vertexSize) {
										tempPos.x = verts[v];
										tempPos.y = verts[v + 1];
										tempLight.set(verts[v + 2], verts[v + 3], verts[v + 4], verts[v + 5]);
										tempUv.x = verts[v + 6];
										tempUv.y = verts[v + 7];
										tempDark.set(verts[v + 8], verts[v + 9], verts[v + 10], verts[v + 11]);
										vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
										verts[v] = tempPos.x;
										verts[v + 1] = tempPos.y;
										verts[v + 2] = tempLight.r;
										verts[v + 3] = tempLight.g;
										verts[v + 4] = tempLight.b;
										verts[v + 5] = tempLight.a;
										verts[v + 6] = tempUv.x;
										verts[v + 7] = tempUv.y;
										verts[v + 8] = tempDark.r;
										verts[v + 9] = tempDark.g;
										verts[v + 10] = tempDark.b;
										verts[v + 11] = tempDark.a;
									}
								}
							}
							batcher.draw(texture, clippedVertices, clippedTriangles);
						}
						else {
							var verts = renderable.vertices;
							if (this.vertexEffect != null) {
								var vertexEffect = this.vertexEffect;
								if (!twoColorTint) {
									for (var v = 0, u = 0, n_6 = renderable.numFloats; v < n_6; v += vertexSize, u += 2) {
										tempPos.x = verts[v];
										tempPos.y = verts[v + 1];
										tempUv.x = uvs[u];
										tempUv.y = uvs[u + 1];
										tempLight.setFromColor(finalColor);
										tempDark.set(0, 0, 0, 0);
										vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
										verts[v] = tempPos.x;
										verts[v + 1] = tempPos.y;
										verts[v + 2] = tempLight.r;
										verts[v + 3] = tempLight.g;
										verts[v + 4] = tempLight.b;
										verts[v + 5] = tempLight.a;
										verts[v + 6] = tempUv.x;
										verts[v + 7] = tempUv.y;
									}
								}
								else {
									for (var v = 0, u = 0, n_7 = renderable.numFloats; v < n_7; v += vertexSize, u += 2) {
										tempPos.x = verts[v];
										tempPos.y = verts[v + 1];
										tempUv.x = uvs[u];
										tempUv.y = uvs[u + 1];
										tempLight.setFromColor(finalColor);
										tempDark.setFromColor(darkColor);
										vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
										verts[v] = tempPos.x;
										verts[v + 1] = tempPos.y;
										verts[v + 2] = tempLight.r;
										verts[v + 3] = tempLight.g;
										verts[v + 4] = tempLight.b;
										verts[v + 5] = tempLight.a;
										verts[v + 6] = tempUv.x;
										verts[v + 7] = tempUv.y;
										verts[v + 8] = tempDark.r;
										verts[v + 9] = tempDark.g;
										verts[v + 10] = tempDark.b;
										verts[v + 11] = tempDark.a;
									}
								}
							}
							else {
								if (!twoColorTint) {
									for (var v = 2, u = 0, n_8 = renderable.numFloats; v < n_8; v += vertexSize, u += 2) {
										verts[v] = finalColor.r;
										verts[v + 1] = finalColor.g;
										verts[v + 2] = finalColor.b;
										verts[v + 3] = finalColor.a;
										verts[v + 4] = uvs[u];
										verts[v + 5] = uvs[u + 1];
									}
								}
								else {
									for (var v = 2, u = 0, n_9 = renderable.numFloats; v < n_9; v += vertexSize, u += 2) {
										verts[v] = finalColor.r;
										verts[v + 1] = finalColor.g;
										verts[v + 2] = finalColor.b;
										verts[v + 3] = finalColor.a;
										verts[v + 4] = uvs[u];
										verts[v + 5] = uvs[u + 1];
										verts[v + 6] = darkColor.r;
										verts[v + 7] = darkColor.g;
										verts[v + 8] = darkColor.b;
										verts[v + 9] = darkColor.a;
									}
								}
							}
							var view = renderable.vertices.subarray(0, renderable.numFloats);
							batcher.draw(texture, view, triangles);
						}
					}
					clipper.clipEndWithSlot(slot);
				}
				clipper.clipEnd();
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
		var ManagedWebGLRenderingContext = (function () {
			function ManagedWebGLRenderingContext(canvasOrContext, contextConfig) {
				var _this = this;
				if (contextConfig === void 0) { contextConfig = { alpha: "true" }; }
				this.restorables = new Array();
				if (canvasOrContext instanceof HTMLCanvasElement) {
					var canvas = canvasOrContext;
					this.gl = (canvas.getContext("webgl2", contextConfig) || canvas.getContext("webgl", contextConfig));
					this.canvas = canvas;
					canvas.addEventListener("webglcontextlost", function (e) {
						var event = e;
						if (e) {
							e.preventDefault();
						}
					});
					canvas.addEventListener("webglcontextrestored", function (e) {
						for (var i = 0, n = _this.restorables.length; i < n; i++) {
							_this.restorables[i].restore();
						}
					});
				}
				else {
					this.gl = canvasOrContext;
					this.canvas = this.gl.canvas;
				}
			}
			ManagedWebGLRenderingContext.prototype.addRestorable = function (restorable) {
				this.restorables.push(restorable);
			};
			ManagedWebGLRenderingContext.prototype.removeRestorable = function (restorable) {
				var index = this.restorables.indexOf(restorable);
				if (index > -1)
					this.restorables.splice(index, 1);
			};
			return ManagedWebGLRenderingContext;
		}());
		webgl.ManagedWebGLRenderingContext = ManagedWebGLRenderingContext;
		var WebGLBlendModeConverter = (function () {
			function WebGLBlendModeConverter() {
			}
			WebGLBlendModeConverter.getDestGLBlendMode = function (blendMode) {
				switch (blendMode) {
					case spine.BlendMode.Normal: return WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA;
					case spine.BlendMode.Additive: return WebGLBlendModeConverter.ONE;
					case spine.BlendMode.Multiply: return WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA;
					case spine.BlendMode.Screen: return WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA;
					default: throw new Error("Unknown blend mode: " + blendMode);
				}
			};
			WebGLBlendModeConverter.getSourceGLBlendMode = function (blendMode, premultipliedAlpha) {
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				switch (blendMode) {
					case spine.BlendMode.Normal: return premultipliedAlpha ? WebGLBlendModeConverter.ONE : WebGLBlendModeConverter.SRC_ALPHA;
					case spine.BlendMode.Additive: return premultipliedAlpha ? WebGLBlendModeConverter.ONE : WebGLBlendModeConverter.SRC_ALPHA;
					case spine.BlendMode.Multiply: return WebGLBlendModeConverter.DST_COLOR;
					case spine.BlendMode.Screen: return WebGLBlendModeConverter.ONE;
					default: throw new Error("Unknown blend mode: " + blendMode);
				}
			};
			WebGLBlendModeConverter.ZERO = 0;
			WebGLBlendModeConverter.ONE = 1;
			WebGLBlendModeConverter.SRC_COLOR = 0x0300;
			WebGLBlendModeConverter.ONE_MINUS_SRC_COLOR = 0x0301;
			WebGLBlendModeConverter.SRC_ALPHA = 0x0302;
			WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA = 0x0303;
			WebGLBlendModeConverter.DST_ALPHA = 0x0304;
			WebGLBlendModeConverter.ONE_MINUS_DST_ALPHA = 0x0305;
			WebGLBlendModeConverter.DST_COLOR = 0x0306;
			return WebGLBlendModeConverter;
		}());
		webgl.WebGLBlendModeConverter = WebGLBlendModeConverter;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
	var Popup = (function () {
		function Popup(player, parent, htmlContent) {
			this.player = player;
			this.dom = createElement("\n\t\t\t\t<div class=\"spine-player-popup spine-player-hidden\">\n\t\t\t\t</div>\n\t\t\t");
			this.dom.innerHTML = htmlContent;
			parent.appendChild(this.dom);
		}
		Popup.prototype.show = function (dismissedListener) {
			var _this = this;
			this.dom.classList.remove("spine-player-hidden");
			var dismissed = false;
			var resize = function () {
				if (!dismissed)
					requestAnimationFrame(resize);
				var bottomOffset = Math.abs(_this.dom.getBoundingClientRect().bottom - _this.player.getBoundingClientRect().bottom);
				var rightOffset = Math.abs(_this.dom.getBoundingClientRect().right - _this.player.getBoundingClientRect().right);
				var maxHeight = _this.player.clientHeight - bottomOffset - rightOffset;
				_this.dom.style.maxHeight = maxHeight + "px";
			};
			requestAnimationFrame(resize);
			var justClicked = true;
			var windowClickListener = function (event) {
				if (justClicked) {
					justClicked = false;
					return;
				}
				if (!isContained(_this.dom, event.target)) {
					_this.dom.remove();
					window.removeEventListener("click", windowClickListener);
					dismissedListener();
					dismissed = true;
				}
			};
			window.addEventListener("click", windowClickListener);
		};
		return Popup;
	}());
	var Switch = (function () {
		function Switch(text) {
			this.text = text;
			this.enabled = false;
		}
		Switch.prototype.render = function () {
			var _this = this;
			this["switch"] = createElement("\n\t\t\t\t<div class=\"spine-player-switch\">\n\t\t\t\t\t<span class=\"spine-player-switch-text\">" + this.text + "</span>\n\t\t\t\t\t<div class=\"spine-player-switch-knob-area\">\n\t\t\t\t\t\t<div class=\"spine-player-switch-knob\"></div>\n\t\t\t\t\t</div>\n\t\t\t\t</div>\n\t\t\t");
			this["switch"].addEventListener("click", function () {
				_this.setEnabled(!_this.enabled);
				if (_this.change)
					_this.change(_this.enabled);
			});
			return this["switch"];
		};
		Switch.prototype.setEnabled = function (enabled) {
			if (enabled)
				this["switch"].classList.add("active");
			else
				this["switch"].classList.remove("active");
			this.enabled = enabled;
		};
		Switch.prototype.isEnabled = function () {
			return this.enabled;
		};
		return Switch;
	}());
	var Slider = (function () {
		function Slider(snaps, snapPercentage, big) {
			if (snaps === void 0) { snaps = 0; }
			if (snapPercentage === void 0) { snapPercentage = 0.1; }
			if (big === void 0) { big = false; }
			this.snaps = snaps;
			this.snapPercentage = snapPercentage;
			this.big = big;
		}
		Slider.prototype.render = function () {
			var _this = this;
			this.slider = createElement("\n\t\t\t\t<div class=\"spine-player-slider " + (this.big ? "big" : "") + "\">\n\t\t\t\t\t<div class=\"spine-player-slider-value\"></div>\n\t\t\t\t\t<!--<div class=\"spine-player-slider-knob\"></div>-->\n\t\t\t\t</div>\n\t\t\t");
			this.value = findWithClass(this.slider, "spine-player-slider-value")[0];
			this.setValue(0);
			var input = new spine.webgl.Input(this.slider);
			var dragging = false;
			input.addListener({
				down: function (x, y) {
					dragging = true;
					_this.value.classList.add("hovering");
				},
				up: function (x, y) {
					dragging = false;
					var percentage = x / _this.slider.clientWidth;
					percentage = percentage = Math.max(0, Math.min(percentage, 1));
					_this.setValue(x / _this.slider.clientWidth);
					if (_this.change)
						_this.change(percentage);
					_this.value.classList.remove("hovering");
				},
				moved: function (x, y) {
					if (dragging) {
						var percentage = x / _this.slider.clientWidth;
						percentage = Math.max(0, Math.min(percentage, 1));
						percentage = _this.setValue(x / _this.slider.clientWidth);
						if (_this.change)
							_this.change(percentage);
					}
				},
				dragged: function (x, y) {
					var percentage = x / _this.slider.clientWidth;
					percentage = Math.max(0, Math.min(percentage, 1));
					percentage = _this.setValue(x / _this.slider.clientWidth);
					if (_this.change)
						_this.change(percentage);
				}
			});
			return this.slider;
		};
		Slider.prototype.setValue = function (percentage) {
			percentage = Math.max(0, Math.min(1, percentage));
			if (this.snaps > 0) {
				var modulo = percentage % (1 / this.snaps);
				if (modulo < (1 / this.snaps) * this.snapPercentage) {
					percentage = percentage - modulo;
				}
				else if (modulo > (1 / this.snaps) - (1 / this.snaps) * this.snapPercentage) {
					percentage = percentage - modulo + (1 / this.snaps);
				}
				percentage = Math.max(0, Math.min(1, percentage));
			}
			this.value.style.width = "" + (percentage * 100) + "%";
			return percentage;
		};
		return Slider;
	}());
	var SpinePlayer = (function () {
		function SpinePlayer(parent, config) {
			this.config = config;
			this.time = new spine.TimeKeeper();
			this.paused = true;
			this.playTime = 0;
			this.speed = 1;
			this.animationViewports = {};
			this.currentViewport = null;
			this.previousViewport = null;
			this.viewportTransitionStart = 0;
			this.stopRequestAnimationFrame = false;
			this.cancelId = 0;
			if (typeof parent === "string")
				this.parent = document.getElementById(parent);
			else
				this.parent = parent;
			this.parent.appendChild(this.render());
		}
		SpinePlayer.prototype.validateConfig = function (config) {
			if (!config)
				throw new Error("Please pass a configuration to new.spine.SpinePlayer().");
			if (!config.jsonUrl && !config.skelUrl)
				throw new Error("Please specify the URL of the skeleton JSON or .skel file.");
			if (!config.atlasUrl)
				throw new Error("Please specify the URL of the atlas file.");
			if (!config.alpha)
				config.alpha = false;
			if (!config.backgroundColor)
				config.backgroundColor = "#000000";
			if (!config.fullScreenBackgroundColor)
				config.fullScreenBackgroundColor = config.backgroundColor;
			if (typeof config.premultipliedAlpha === "undefined")
				config.premultipliedAlpha = true;
			if (!config.success)
				config.success = function (widget) { };
			if (!config.error)
				config.error = function (widget, msg) { };
			if (!config.debug)
				config.debug = {
					bones: false,
					regions: false,
					meshes: false,
					bounds: false,
					clipping: false,
					paths: false,
					points: false,
					hulls: false
				};
			if (typeof config.debug.bones === "undefined")
				config.debug.bones = false;
			if (typeof config.debug.bounds === "undefined")
				config.debug.bounds = false;
			if (typeof config.debug.clipping === "undefined")
				config.debug.clipping = false;
			if (typeof config.debug.hulls === "undefined")
				config.debug.hulls = false;
			if (typeof config.debug.paths === "undefined")
				config.debug.paths = false;
			if (typeof config.debug.points === "undefined")
				config.debug.points = false;
			if (typeof config.debug.regions === "undefined")
				config.debug.regions = false;
			if (typeof config.debug.meshes === "undefined")
				config.debug.meshes = false;
			if (config.animations && config.animation) {
				if (config.animations.indexOf(config.animation) < 0)
					throw new Error("Default animation '" + config.animation + "' is not contained in the list of selectable animations " + escapeHtml(JSON.stringify(this.config.animations)) + ".");
			}
			if (config.skins && config.skin) {
				if (config.skins.indexOf(config.skin) < 0)
					throw new Error("Default skin '" + config.skin + "' is not contained in the list of selectable skins " + escapeHtml(JSON.stringify(this.config.skins)) + ".");
			}
			if (!config.controlBones)
				config.controlBones = [];
			if (typeof config.showControls === "undefined")
				config.showControls = true;
			if (typeof config.defaultMix === "undefined")
				config.defaultMix = 0.25;
			return config;
		};
		SpinePlayer.prototype.showError = function (error) {
			var errorDom = findWithClass(this.dom, "spine-player-error")[0];
			errorDom.classList.remove("spine-player-hidden");
			errorDom.innerHTML = "<p style=\"text-align: center; align-self: center;\">" + error + "</p>";
			this.config.error(this, error);
		};
		SpinePlayer.prototype.render = function () {
			var _this = this;
			var config = this.config;
			var dom = this.dom = createElement("\n\t\t\t\t<div class=\"spine-player\">\n\t\t\t\t\t<canvas class=\"spine-player-canvas\"></canvas>\n\t\t\t\t\t<div class=\"spine-player-error spine-player-hidden\"></div>\n\t\t\t\t\t<div class=\"spine-player-controls spine-player-popup-parent spine-player-controls-hidden\">\n\t\t\t\t\t\t<div class=\"spine-player-timeline\">\n\t\t\t\t\t\t</div>\n\t\t\t\t\t\t<div class=\"spine-player-buttons\">\n\t\t\t\t\t\t\t<button id=\"spine-player-button-play-pause\" class=\"spine-player-button spine-player-button-icon-pause\"></button>\n\t\t\t\t\t\t\t<div class=\"spine-player-button-spacer\"></div>\n\t\t\t\t\t\t\t<button id=\"spine-player-button-speed\" class=\"spine-player-button spine-player-button-icon-speed\"></button>\n\t\t\t\t\t\t\t<button id=\"spine-player-button-animation\" class=\"spine-player-button spine-player-button-icon-animations\"></button>\n\t\t\t\t\t\t\t<button id=\"spine-player-button-skin\" class=\"spine-player-button spine-player-button-icon-skins\"></button>\n\t\t\t\t\t\t\t<button id=\"spine-player-button-settings\" class=\"spine-player-button spine-player-button-icon-settings\"></button>\n\t\t\t\t\t\t\t<button id=\"spine-player-button-fullscreen\" class=\"spine-player-button spine-player-button-icon-fullscreen\"></button>\n\t\t\t\t\t\t\t<img id=\"spine-player-button-logo\" class=\"spine-player-button-icon-spine-logo\" src=\"data:image/svg+xml,%3Csvg%20id%3D%22Spine_Logo%22%20data-name%3D%22Spine%20Logo%22%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%20104%2031.16%22%3E%3Cdefs%3E%3Cstyle%3E.cls-1%7Bfill%3A%23fff%3B%7D.cls-2%7Bfill%3A%23ff4000%3B%7D%3C%2Fstyle%3E%3C%2Fdefs%3E%3Ctitle%3Espine-logo-white%3C%2Ftitle%3E%3Cpath%20id%3D%22e%22%20class%3D%22cls-1%22%20d%3D%22M104%2C12.68a1.31%2C1.31%2C0%2C0%2C1-.37%2C1%2C1.28%2C1.28%2C0%2C0%2C1-.85.31H91.57a10.51%2C10.51%2C0%2C0%2C0%2C.29%2C2.55%2C4.92%2C4.92%2C0%2C0%2C0%2C1%2C2A4.27%2C4.27%2C0%2C0%2C0%2C94.5%2C19.8a6.89%2C6.89%2C0%2C0%2C0%2C2.6.44%2C10.66%2C10.66%2C0%2C0%2C0%2C2.17-.2%2C12.81%2C12.81%2C0%2C0%2C0%2C1.64-.44q.69-.25%2C1.14-.44a1.87%2C1.87%2C0%2C0%2C1%2C.68-.2A.44.44%2C0%2C0%2C1%2C103%2C19a.43.43%2C0%2C0%2C1%2C.16.2%2C1.38%2C1.38%2C0%2C0%2C1%2C.09.37%2C4.89%2C4.89%2C0%2C0%2C1%2C0%2C.58%2C4.14%2C4.14%2C0%2C0%2C1%2C0%2C.43v.32a.83.83%2C0%2C0%2C1-.09.26%2C1.1%2C1.1%2C0%2C0%2C1-.17.22%2C2.77%2C2.77%2C0%2C0%2C1-.61.34%2C8.94%2C8.94%2C0%2C0%2C1-1.32.46%2C18.54%2C18.54%2C0%2C0%2C1-1.88.41%2C13.78%2C13.78%2C0%2C0%2C1-2.28.18%2C10.55%2C10.55%2C0%2C0%2C1-3.68-.59%2C6.82%2C6.82%2C0%2C0%2C1-2.66-1.74%2C7.44%2C7.44%2C0%2C0%2C1-1.63-2.89%2C13.48%2C13.48%2C0%2C0%2C1-.55-4%2C12.76%2C12.76%2C0%2C0%2C1%2C.57-3.94%2C8.35%2C8.35%2C0%2C0%2C1%2C1.64-3%2C7.15%2C7.15%2C0%2C0%2C1%2C2.58-1.87%2C8.47%2C8.47%2C0%2C0%2C1%2C3.39-.65%2C8.19%2C8.19%2C0%2C0%2C1%2C3.41.64%2C6.46%2C6.46%2C0%2C0%2C1%2C2.32%2C1.73A7%2C7%2C0%2C0%2C1%2C103.59%2C9a11.17%2C11.17%2C0%2C0%2C1%2C.43%2C3.13Zm-3.14-.93a5.69%2C5.69%2C0%2C0%2C0-1.09-3.86%2C4.17%2C4.17%2C0%2C0%2C0-3.42-1.4%2C4.52%2C4.52%2C0%2C0%2C0-2%2C.44%2C4.41%2C4.41%2C0%2C0%2C0-1.47%2C1.15A5.29%2C5.29%2C0%2C0%2C0%2C92%2C9.75a7%2C7%2C0%2C0%2C0-.36%2C2Z%22%2F%3E%3Cpath%20id%3D%22n%22%20class%3D%22cls-1%22%20d%3D%22M80.68%2C21.94a.42.42%2C0%2C0%2C1-.08.26.59.59%2C0%2C0%2C1-.25.18%2C1.74%2C1.74%2C0%2C0%2C1-.47.11%2C6.31%2C6.31%2C0%2C0%2C1-.76%2C0%2C6.5%2C6.5%2C0%2C0%2C1-.78%2C0%2C1.74%2C1.74%2C0%2C0%2C1-.47-.11.59.59%2C0%2C0%2C1-.25-.18.42.42%2C0%2C0%2C1-.08-.26V12a9.8%2C9.8%2C0%2C0%2C0-.23-2.35%2C4.86%2C4.86%2C0%2C0%2C0-.66-1.53%2C2.88%2C2.88%2C0%2C0%2C0-1.13-1%2C3.57%2C3.57%2C0%2C0%2C0-1.6-.34%2C4%2C4%2C0%2C0%2C0-2.35.83A12.71%2C12.71%2C0%2C0%2C0%2C69.11%2C10v11.9a.42.42%2C0%2C0%2C1-.08.26.59.59%2C0%2C0%2C1-.25.18%2C1.74%2C1.74%2C0%2C0%2C1-.47.11%2C6.51%2C6.51%2C0%2C0%2C1-.78%2C0%2C6.31%2C6.31%2C0%2C0%2C1-.76%2C0%2C1.88%2C1.88%2C0%2C0%2C1-.48-.11.52.52%2C0%2C0%2C1-.25-.18.46.46%2C0%2C0%2C1-.07-.26v-17A.53.53%2C0%2C0%2C1%2C66%2C4.69a.5.5%2C0%2C0%2C1%2C.23-.19%2C1.28%2C1.28%2C0%2C0%2C1%2C.44-.11%2C8.53%2C8.53%2C0%2C0%2C1%2C1.39%2C0%2C1.12%2C1.12%2C0%2C0%2C1%2C.43.11.6.6%2C0%2C0%2C1%2C.22.19.47.47%2C0%2C0%2C1%2C.07.26V7.2a10.46%2C10.46%2C0%2C0%2C1%2C2.87-2.36%2C6.17%2C6.17%2C0%2C0%2C1%2C2.88-.75%2C6.41%2C6.41%2C0%2C0%2C1%2C2.87.58%2C5.16%2C5.16%2C0%2C0%2C1%2C1.88%2C1.54%2C6.15%2C6.15%2C0%2C0%2C1%2C1%2C2.26%2C13.46%2C13.46%2C0%2C0%2C1%2C.31%2C3.11Z%22%2F%3E%3Cg%20id%3D%22i%22%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M43.35%2C2.86c.09%2C2.6%2C1.89%2C4%2C5.48%2C4.61%2C3%2C.48%2C5.79.24%2C6.69-2.37%2C1.75-5.09-2.4-3.82-6-4.39S43.21-1.32%2C43.35%2C2.86Z%22%2F%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M44.43%2C13.55c.33%2C1.94%2C2.14%2C3.06%2C4.91%2C3s4.84-1.16%2C5.13-3.25c.53-3.88-2.53-2.38-5.3-2.3S43.77%2C9.74%2C44.43%2C13.55Z%22%2F%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M48%2C22.44c.55%2C1.45%2C2.06%2C2.06%2C4.1%2C1.63s3.45-1.11%2C3.33-2.76c-.21-3.06-2.22-2.1-4.26-1.66S47%2C19.6%2C48%2C22.44Z%22%2F%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M49.78%2C29.22c.16%2C1.22%2C1.22%2C2%2C2.88%2C1.93s2.92-.67%2C3.13-2c.4-2.43-1.46-1.53-3.12-1.51S49.5%2C26.82%2C49.78%2C29.22Z%22%2F%3E%3C%2Fg%3E%3Cpath%20id%3D%22p%22%20class%3D%22cls-1%22%20d%3D%22M35.28%2C13.16a15.33%2C15.33%2C0%2C0%2C1-.48%2C4%2C8.75%2C8.75%2C0%2C0%2C1-1.42%2C3%2C6.35%2C6.35%2C0%2C0%2C1-2.32%2C1.91%2C7.14%2C7.14%2C0%2C0%2C1-3.16.67%2C6.1%2C6.1%2C0%2C0%2C1-1.4-.15%2C5.34%2C5.34%2C0%2C0%2C1-1.26-.47A7.29%2C7.29%2C0%2C0%2C1%2C24%2C21.31q-.61-.49-1.29-1.15v8.51a.47.47%2C0%2C0%2C1-.08.26.56.56%2C0%2C0%2C1-.25.19%2C1.74%2C1.74%2C0%2C0%2C1-.47.11%2C6.47%2C6.47%2C0%2C0%2C1-.78%2C0%2C6.26%2C6.26%2C0%2C0%2C1-.76%2C0%2C1.89%2C1.89%2C0%2C0%2C1-.48-.11.49.49%2C0%2C0%2C1-.25-.19.51.51%2C0%2C0%2C1-.07-.26V4.91a.57.57%2C0%2C0%2C1%2C.06-.27.46.46%2C0%2C0%2C1%2C.23-.18%2C1.47%2C1.47%2C0%2C0%2C1%2C.44-.1%2C7.41%2C7.41%2C0%2C0%2C1%2C1.3%2C0%2C1.45%2C1.45%2C0%2C0%2C1%2C.43.1.52.52%2C0%2C0%2C1%2C.24.18.51.51%2C0%2C0%2C1%2C.07.27V7.2a18.06%2C18.06%2C0%2C0%2C1%2C1.49-1.38%2C9%2C9%2C0%2C0%2C1%2C1.45-1%2C6.82%2C6.82%2C0%2C0%2C1%2C1.49-.59%2C7.09%2C7.09%2C0%2C0%2C1%2C4.78.52%2C6%2C6%2C0%2C0%2C1%2C2.13%2C2%2C8.79%2C8.79%2C0%2C0%2C1%2C1.2%2C2.9A15.72%2C15.72%2C0%2C0%2C1%2C35.28%2C13.16ZM32%2C13.52a15.64%2C15.64%2C0%2C0%2C0-.2-2.53%2C7.32%2C7.32%2C0%2C0%2C0-.69-2.17%2C4.06%2C4.06%2C0%2C0%2C0-1.3-1.51%2C3.49%2C3.49%2C0%2C0%2C0-2-.57%2C4.1%2C4.1%2C0%2C0%2C0-1.2.18%2C4.92%2C4.92%2C0%2C0%2C0-1.2.57%2C8.54%2C8.54%2C0%2C0%2C0-1.28%2C1A15.77%2C15.77%2C0%2C0%2C0%2C22.76%2C10v6.77a13.53%2C13.53%2C0%2C0%2C0%2C2.46%2C2.4%2C4.12%2C4.12%2C0%2C0%2C0%2C2.44.83%2C3.56%2C3.56%2C0%2C0%2C0%2C2-.57A4.28%2C4.28%2C0%2C0%2C0%2C31%2C18a7.58%2C7.58%2C0%2C0%2C0%2C.77-2.12A11.43%2C11.43%2C0%2C0%2C0%2C32%2C13.52Z%22%2F%3E%3Cpath%20id%3D%22s%22%20class%3D%22cls-1%22%20d%3D%22M12%2C17.3a5.39%2C5.39%2C0%2C0%2C1-.48%2C2.33%2C4.73%2C4.73%2C0%2C0%2C1-1.37%2C1.72%2C6.19%2C6.19%2C0%2C0%2C1-2.12%2C1.06%2C9.62%2C9.62%2C0%2C0%2C1-2.71.36%2C10.38%2C10.38%2C0%2C0%2C1-3.21-.5%2C7.63%2C7.63%2C0%2C0%2C1-1.11-.45%2C3.25%2C3.25%2C0%2C0%2C1-.66-.43%2C1.09%2C1.09%2C0%2C0%2C1-.3-.53A3.59%2C3.59%2C0%2C0%2C1%2C0%2C19.93a4.06%2C4.06%2C0%2C0%2C1%2C0-.61%2C2%2C2%2C0%2C0%2C1%2C.09-.4.42.42%2C0%2C0%2C1%2C.16-.22.43.43%2C0%2C0%2C1%2C.24-.07%2C1.35%2C1.35%2C0%2C0%2C1%2C.61.26q.41.26%2C1%2C.56A9.22%2C9.22%2C0%2C0%2C0%2C3.51%2C20a6.25%2C6.25%2C0%2C0%2C0%2C1.87.26%2C5.62%2C5.62%2C0%2C0%2C0%2C1.44-.17%2C3.48%2C3.48%2C0%2C0%2C0%2C1.12-.5%2C2.23%2C2.23%2C0%2C0%2C0%2C.73-.84%2C2.68%2C2.68%2C0%2C0%2C0%2C.26-1.21%2C2%2C2%2C0%2C0%2C0-.37-1.21%2C3.55%2C3.55%2C0%2C0%2C0-1-.87A8.09%2C8.09%2C0%2C0%2C0%2C6.2%2C14.8l-1.56-.61a16%2C16%2C0%2C0%2C1-1.57-.73%2C6%2C6%2C0%2C0%2C1-1.37-1%2C4.52%2C4.52%2C0%2C0%2C1-1-1.4%2C4.69%2C4.69%2C0%2C0%2C1-.37-2A4.88%2C4.88%2C0%2C0%2C1%2C.72%2C7.19%2C4.46%2C4.46%2C0%2C0%2C1%2C1.88%2C5.58%2C5.83%2C5.83%2C0%2C0%2C1%2C3.82%2C4.47%2C8.06%2C8.06%2C0%2C0%2C1%2C6.53%2C4a8.28%2C8.28%2C0%2C0%2C1%2C1.36.11%2C9.36%2C9.36%2C0%2C0%2C1%2C1.23.28%2C5.92%2C5.92%2C0%2C0%2C1%2C.94.37%2C4.09%2C4.09%2C0%2C0%2C1%2C.59.35%2C1%2C1%2C0%2C0%2C1%2C.26.26.83.83%2C0%2C0%2C1%2C.09.26%2C1.32%2C1.32%2C0%2C0%2C0%2C.06.35%2C3.87%2C3.87%2C0%2C0%2C1%2C0%2C.51%2C4.76%2C4.76%2C0%2C0%2C1%2C0%2C.56%2C1.39%2C1.39%2C0%2C0%2C1-.09.39.5.5%2C0%2C0%2C1-.16.22.35.35%2C0%2C0%2C1-.21.07%2C1%2C1%2C0%2C0%2C1-.49-.21%2C7%2C7%2C0%2C0%2C0-.83-.44%2C9.26%2C9.26%2C0%2C0%2C0-1.2-.44A5.49%2C5.49%2C0%2C0%2C0%2C6.5%2C6.48a4.93%2C4.93%2C0%2C0%2C0-1.4.18%2C2.69%2C2.69%2C0%2C0%2C0-1%2C.51A2.16%2C2.16%2C0%2C0%2C0%2C3.51%2C8a2.43%2C2.43%2C0%2C0%2C0-.2%2C1%2C2%2C2%2C0%2C0%2C0%2C.38%2C1.24%2C3.6%2C3.6%2C0%2C0%2C0%2C1%2C.88%2C8.25%2C8.25%2C0%2C0%2C0%2C1.38.68l1.58.62q.8.32%2C1.59.72a6%2C6%2C0%2C0%2C1%2C1.39%2C1%2C4.37%2C4.37%2C0%2C0%2C1%2C1%2C1.36A4.46%2C4.46%2C0%2C0%2C1%2C12%2C17.3Z%22%2F%3E%3C%2Fsvg%3E\"/>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</div>\n\t\t\t\t</div>\n\t\t\t");
			try {
				this.config = this.validateConfig(config);
			}
			catch (e) {
				this.showError(e);
				return dom;
			}
			try {
				this.canvas = findWithClass(dom, "spine-player-canvas")[0];
				var webglConfig = { alpha: config.alpha };
				this.context = new spine.webgl.ManagedWebGLRenderingContext(this.canvas, webglConfig);
				this.sceneRenderer = new spine.webgl.SceneRenderer(this.canvas, this.context, true);
				this.loadingScreen = new spine.webgl.LoadingScreen(this.sceneRenderer);
			}
			catch (e) {
				this.showError("Sorry, your browser does not support WebGL.<br><br>Please use the latest version of Firefox, Chrome, Edge, or Safari.");
				return dom;
			}
			this.assetManager = new spine.webgl.AssetManager(this.context);
			if (config.rawDataURIs) {
				for (var path in config.rawDataURIs) {
					var data = config.rawDataURIs[path];
					this.assetManager.setRawDataURI(path, data);
				}
			}
			if (config.jsonUrl)
				this.assetManager.loadText(config.jsonUrl);
			else
				this.assetManager.loadBinary(config.skelUrl);
			this.assetManager.loadTextureAtlas(config.atlasUrl);
			if (config.backgroundImage && config.backgroundImage.url)
				this.assetManager.loadTexture(config.backgroundImage.url);
			requestAnimationFrame(function () { return _this.drawFrame(); });
			this.playerControls = findWithClass(dom, "spine-player-controls")[0];
			var timeline = findWithClass(dom, "spine-player-timeline")[0];
			this.timelineSlider = new Slider();
			timeline.appendChild(this.timelineSlider.render());
			this.playButton = findWithId(dom, "spine-player-button-play-pause")[0];
			var speedButton = findWithId(dom, "spine-player-button-speed")[0];
			this.animationButton = findWithId(dom, "spine-player-button-animation")[0];
			this.skinButton = findWithId(dom, "spine-player-button-skin")[0];
			var settingsButton = findWithId(dom, "spine-player-button-settings")[0];
			var fullscreenButton = findWithId(dom, "spine-player-button-fullscreen")[0];
			var logoButton = findWithId(dom, "spine-player-button-logo")[0];
			this.playButton.onclick = function () {
				if (_this.paused)
					_this.play();
				else
					_this.pause();
			};
			speedButton.onclick = function () {
				_this.showSpeedDialog(speedButton);
			};
			this.animationButton.onclick = function () {
				_this.showAnimationsDialog(_this.animationButton);
			};
			this.skinButton.onclick = function () {
				_this.showSkinsDialog(_this.skinButton);
			};
			settingsButton.onclick = function () {
				_this.showSettingsDialog(settingsButton);
			};
			var oldWidth = this.canvas.clientWidth;
			var oldHeight = this.canvas.clientHeight;
			var oldStyleWidth = this.canvas.style.width;
			var oldStyleHeight = this.canvas.style.height;
			var isFullscreen = false;
			fullscreenButton.onclick = function () {
				var fullscreenChanged = function () {
					isFullscreen = !isFullscreen;
					if (!isFullscreen) {
						_this.canvas.style.width = "" + oldWidth + "px";
						_this.canvas.style.height = "" + oldHeight + "px";
						_this.drawFrame(false);
						requestAnimationFrame(function () {
							_this.canvas.style.width = oldStyleWidth;
							_this.canvas.style.height = oldStyleHeight;
						});
					}
				};
				var doc = document;
				dom.onfullscreenchange = fullscreenChanged;
				dom.onwebkitfullscreenchange = fullscreenChanged;
				if (doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement) {
					if (doc.exitFullscreen)
						doc.exitFullscreen();
					else if (doc.mozCancelFullScreen)
						doc.mozCancelFullScreen();
					else if (doc.webkitExitFullscreen)
						doc.webkitExitFullscreen();
					else if (doc.msExitFullscreen)
						doc.msExitFullscreen();
				}
				else {
					oldWidth = _this.canvas.clientWidth;
					oldHeight = _this.canvas.clientHeight;
					oldStyleWidth = _this.canvas.style.width;
					oldStyleHeight = _this.canvas.style.height;
					var player = dom;
					if (player.requestFullscreen)
						player.requestFullscreen();
					else if (player.webkitRequestFullScreen)
						player.webkitRequestFullScreen();
					else if (player.mozRequestFullScreen)
						player.mozRequestFullScreen();
					else if (player.msRequestFullscreen)
						player.msRequestFullscreen();
				}
			};
			logoButton.onclick = function () {
				window.open("http://esotericsoftware.com");
			};
			window.onresize = function () {
				_this.drawFrame(false);
			};
			return dom;
		};
		SpinePlayer.prototype.showSpeedDialog = function (speedButton) {
			var _this = this;
			if (this.lastPopup)
				this.lastPopup.dom.remove();
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Speed") {
				this.lastPopup = null;
				speedButton.classList.remove("spine-player-button-icon-speed-selected");
				return;
			}
			var popup = new Popup(this.dom, this.playerControls, "\n\t\t\t\t<div class=\"spine-player-popup-title\">Speed</div>\n\t\t\t\t<hr>\n\t\t\t\t<div class=\"spine-player-row\" style=\"user-select: none; align-items: center; padding: 8px;\">\n\t\t\t\t\t<div class=\"spine-player-column\">\n\t\t\t\t\t\t<div class=\"spine-player-speed-slider\" style=\"margin-bottom: 4px;\"></div>\n\t\t\t\t\t\t<div class=\"spine-player-row\" style=\"justify-content: space-between;\">\n\t\t\t\t\t\t\t<div>0.1x</div>\n\t\t\t\t\t\t\t<div>1x</div>\n\t\t\t\t\t\t\t<div>2x</div>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</div>\n\t\t\t\t</div>\n\t\t\t");
			var sliderParent = findWithClass(popup.dom, "spine-player-speed-slider")[0];
			var slider = new Slider(2, 0.1, true);
			sliderParent.appendChild(slider.render());
			slider.setValue(this.speed / 2);
			slider.change = function (percentage) {
				_this.speed = percentage * 2;
			};
			speedButton.classList.add("spine-player-button-icon-speed-selected");
			popup.show(function () {
				speedButton.classList.remove("spine-player-button-icon-speed-selected");
				popup.dom.remove();
				_this.lastPopup = null;
			});
			this.lastPopup = popup;
		};
		SpinePlayer.prototype.showAnimationsDialog = function (animationsButton) {
			var _this = this;
			if (this.lastPopup)
				this.lastPopup.dom.remove();
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Animations") {
				this.lastPopup = null;
				animationsButton.classList.remove("spine-player-button-icon-animations-selected");
				return;
			}
			if (!this.skeleton || this.skeleton.data.animations.length == 0)
				return;
			var popup = new Popup(this.dom, this.playerControls, "\n\t\t\t\t<div class=\"spine-player-popup-title\">Animations</div>\n\t\t\t\t<hr>\n\t\t\t\t<ul class=\"spine-player-list\"></ul>\n\t\t\t");
			var rows = findWithClass(popup.dom, "spine-player-list")[0];
			this.skeleton.data.animations.forEach(function (animation) {
				if (_this.config.animations && _this.config.animations.indexOf(animation.name) < 0) {
					return;
				}
				var row = createElement("\n\t\t\t\t\t<li class=\"spine-player-list-item selectable\">\n\t\t\t\t\t\t<div class=\"selectable-circle\">\n\t\t\t\t\t\t</div>\n\t\t\t\t\t\t<div class=\"selectable-text\">\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</li>\n\t\t\t\t");
				if (animation.name == _this.config.animation)
					row.classList.add("selected");
				findWithClass(row, "selectable-text")[0].innerText = animation.name;
				rows.appendChild(row);
				row.onclick = function () {
					removeClass(rows.children, "selected");
					row.classList.add("selected");
					_this.config.animation = animation.name;
					_this.playTime = 0;
					_this.setAnimation(animation.name);
				};
			});
			animationsButton.classList.add("spine-player-button-icon-animations-selected");
			popup.show(function () {
				animationsButton.classList.remove("spine-player-button-icon-animations-selected");
				popup.dom.remove();
				_this.lastPopup = null;
			});
			this.lastPopup = popup;
		};
		SpinePlayer.prototype.showSkinsDialog = function (skinButton) {
			var _this = this;
			if (this.lastPopup)
				this.lastPopup.dom.remove();
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Skins") {
				this.lastPopup = null;
				skinButton.classList.remove("spine-player-button-icon-skins-selected");
				return;
			}
			if (!this.skeleton || this.skeleton.data.animations.length == 0)
				return;
			var popup = new Popup(this.dom, this.playerControls, "\n\t\t\t\t<div class=\"spine-player-popup-title\">Skins</div>\n\t\t\t\t<hr>\n\t\t\t\t<ul class=\"spine-player-list\"></ul>\n\t\t\t");
			var rows = findWithClass(popup.dom, "spine-player-list")[0];
			this.skeleton.data.skins.forEach(function (skin) {
				if (_this.config.skins && _this.config.skins.indexOf(skin.name) < 0) {
					return;
				}
				var row = createElement("\n\t\t\t\t\t<li class=\"spine-player-list-item selectable\">\n\t\t\t\t\t\t<div class=\"selectable-circle\">\n\t\t\t\t\t\t</div>\n\t\t\t\t\t\t<div class=\"selectable-text\">\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</li>\n\t\t\t\t");
				if (skin.name == _this.config.skin)
					row.classList.add("selected");
				findWithClass(row, "selectable-text")[0].innerText = skin.name;
				rows.appendChild(row);
				row.onclick = function () {
					removeClass(rows.children, "selected");
					row.classList.add("selected");
					_this.config.skin = skin.name;
					_this.skeleton.setSkinByName(_this.config.skin);
					_this.skeleton.setSlotsToSetupPose();
				};
			});
			skinButton.classList.add("spine-player-button-icon-skins-selected");
			popup.show(function () {
				skinButton.classList.remove("spine-player-button-icon-skins-selected");
				popup.dom.remove();
				_this.lastPopup = null;
			});
			this.lastPopup = popup;
		};
		SpinePlayer.prototype.showSettingsDialog = function (settingsButton) {
			var _this = this;
			if (this.lastPopup)
				this.lastPopup.dom.remove();
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Debug") {
				this.lastPopup = null;
				settingsButton.classList.remove("spine-player-button-icon-settings-selected");
				return;
			}
			if (!this.skeleton || this.skeleton.data.animations.length == 0)
				return;
			var popup = new Popup(this.dom, this.playerControls, "\n\t\t\t\t<div class=\"spine-player-popup-title\">Debug</div>\n\t\t\t\t<hr>\n\t\t\t\t<ul class=\"spine-player-list\">\n\t\t\t\t</li>\n\t\t\t");
			var rows = findWithClass(popup.dom, "spine-player-list")[0];
			var makeItem = function (label, name) {
				var row = createElement("<li class=\"spine-player-list-item\"></li>");
				var s = new Switch(label);
				row.appendChild(s.render());
				s.setEnabled(_this.config.debug[name]);
				s.change = function (value) {
					_this.config.debug[name] = value;
				};
				rows.appendChild(row);
			};
			makeItem("Bones", "bones");
			makeItem("Regions", "regions");
			makeItem("Meshes", "meshes");
			makeItem("Bounds", "bounds");
			makeItem("Paths", "paths");
			makeItem("Clipping", "clipping");
			makeItem("Points", "points");
			makeItem("Hulls", "hulls");
			settingsButton.classList.add("spine-player-button-icon-settings-selected");
			popup.show(function () {
				settingsButton.classList.remove("spine-player-button-icon-settings-selected");
				popup.dom.remove();
				_this.lastPopup = null;
			});
			this.lastPopup = popup;
		};
		SpinePlayer.prototype.drawFrame = function (requestNextFrame) {
			var _this = this;
			if (requestNextFrame === void 0) { requestNextFrame = true; }
			if (requestNextFrame && !this.stopRequestAnimationFrame)
				requestAnimationFrame(function () { return _this.drawFrame(); });
			var ctx = this.context;
			var gl = ctx.gl;
			var doc = document;
			var isFullscreen = doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement;
			var bg = new spine.Color().setFromString(isFullscreen ? this.config.fullScreenBackgroundColor : this.config.backgroundColor);
			gl.clearColor(bg.r, bg.g, bg.b, bg.a);
			gl.clear(gl.COLOR_BUFFER_BIT);
			this.loadingScreen.backgroundColor.setFromColor(bg);
			this.loadingScreen.draw(this.assetManager.isLoadingComplete());
			if (this.assetManager.isLoadingComplete() && this.skeleton == null)
				this.loadSkeleton();
			this.sceneRenderer.resize(spine.webgl.ResizeMode.Expand);
			if (this.loaded) {
				if (!this.paused && this.config.animation) {
					this.time.update();
					var delta = this.time.delta * this.speed;
					var animationDuration = this.animationState.getCurrent(0).animation.duration;
					this.playTime += delta;
					while (this.playTime >= animationDuration && animationDuration != 0) {
						this.playTime -= animationDuration;
					}
					this.playTime = Math.max(0, Math.min(this.playTime, animationDuration));
					this.timelineSlider.setValue(this.playTime / animationDuration);
					this.animationState.update(delta);
					this.animationState.apply(this.skeleton);
				}
				this.skeleton.updateWorldTransform();
				var viewport = {
					x: this.currentViewport.x - this.currentViewport.padLeft,
					y: this.currentViewport.y - this.currentViewport.padBottom,
					width: this.currentViewport.width + this.currentViewport.padLeft + this.currentViewport.padRight,
					height: this.currentViewport.height + this.currentViewport.padBottom + this.currentViewport.padTop
				};
				var transitionAlpha = ((performance.now() - this.viewportTransitionStart) / 1000) / this.config.viewport.transitionTime;
				if (this.previousViewport && transitionAlpha < 1) {
					var oldViewport = {
						x: this.previousViewport.x - this.previousViewport.padLeft,
						y: this.previousViewport.y - this.previousViewport.padBottom,
						width: this.previousViewport.width + this.previousViewport.padLeft + this.previousViewport.padRight,
						height: this.previousViewport.height + this.previousViewport.padBottom + this.previousViewport.padTop
					};
					viewport = {
						x: oldViewport.x + (viewport.x - oldViewport.x) * transitionAlpha,
						y: oldViewport.y + (viewport.y - oldViewport.y) * transitionAlpha,
						width: oldViewport.width + (viewport.width - oldViewport.width) * transitionAlpha,
						height: oldViewport.height + (viewport.height - oldViewport.height) * transitionAlpha
					};
				}
				var viewportSize = this.scale(viewport.width, viewport.height, this.canvas.width, this.canvas.height);
				this.sceneRenderer.camera.zoom = viewport.width / viewportSize.x;
				this.sceneRenderer.camera.position.x = viewport.x + viewport.width / 2;
				this.sceneRenderer.camera.position.y = viewport.y + viewport.height / 2;
				this.sceneRenderer.begin();
				if (this.config.backgroundImage && this.config.backgroundImage.url) {
					var bgImage = this.assetManager.get(this.config.backgroundImage.url);
					if (!(this.config.backgroundImage.hasOwnProperty("x") && this.config.backgroundImage.hasOwnProperty("y") && this.config.backgroundImage.hasOwnProperty("width") && this.config.backgroundImage.hasOwnProperty("height"))) {
						this.sceneRenderer.drawTexture(bgImage, viewport.x, viewport.y, viewport.width, viewport.height);
					}
					else {
						this.sceneRenderer.drawTexture(bgImage, this.config.backgroundImage.x, this.config.backgroundImage.y, this.config.backgroundImage.width, this.config.backgroundImage.height);
					}
				}
				this.sceneRenderer.drawSkeleton(this.skeleton, this.config.premultipliedAlpha);
				this.sceneRenderer.skeletonDebugRenderer.drawBones = this.config.debug.bones;
				this.sceneRenderer.skeletonDebugRenderer.drawBoundingBoxes = this.config.debug.bounds;
				this.sceneRenderer.skeletonDebugRenderer.drawClipping = this.config.debug.clipping;
				this.sceneRenderer.skeletonDebugRenderer.drawMeshHull = this.config.debug.hulls;
				this.sceneRenderer.skeletonDebugRenderer.drawPaths = this.config.debug.paths;
				this.sceneRenderer.skeletonDebugRenderer.drawRegionAttachments = this.config.debug.regions;
				this.sceneRenderer.skeletonDebugRenderer.drawMeshTriangles = this.config.debug.meshes;
				this.sceneRenderer.drawSkeletonDebug(this.skeleton, this.config.premultipliedAlpha);
				var controlBones = this.config.controlBones;
				var selectedBones = this.selectedBones;
				var skeleton = this.skeleton;
				gl.lineWidth(2);
				for (var i = 0; i < controlBones.length; i++) {
					var bone = skeleton.findBone(controlBones[i]);
					if (!bone)
						continue;
					var colorInner = selectedBones[i] !== null ? SpinePlayer.HOVER_COLOR_INNER : SpinePlayer.NON_HOVER_COLOR_INNER;
					var colorOuter = selectedBones[i] !== null ? SpinePlayer.HOVER_COLOR_OUTER : SpinePlayer.NON_HOVER_COLOR_OUTER;
					this.sceneRenderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);
					this.sceneRenderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);
				}
				gl.lineWidth(1);
				if (this.config.viewport.debugRender) {
					this.sceneRenderer.rect(false, this.currentViewport.x, this.currentViewport.y, this.currentViewport.width, this.currentViewport.height, spine.Color.GREEN);
					this.sceneRenderer.rect(false, viewport.x, viewport.y, viewport.width, viewport.height, spine.Color.RED);
				}
				this.sceneRenderer.end();
				this.sceneRenderer.camera.zoom = 0;
			}
		};
		SpinePlayer.prototype.scale = function (sourceWidth, sourceHeight, targetWidth, targetHeight) {
			var targetRatio = targetHeight / targetWidth;
			var sourceRatio = sourceHeight / sourceWidth;
			var scale = targetRatio > sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
			var temp = new spine.Vector2();
			temp.x = sourceWidth * scale;
			temp.y = sourceHeight * scale;
			return temp;
		};
		SpinePlayer.prototype.loadSkeleton = function () {
			var _this = this;
			if (this.loaded)
				return;
			if (this.assetManager.hasErrors()) {
				this.showError("Error: assets could not be loaded.<br><br>" + escapeHtml(JSON.stringify(this.assetManager.getErrors())));
				return;
			}
			var atlas = this.assetManager.get(this.config.atlasUrl);
			var skeletonData;
			if (this.config.jsonUrl) {
				var jsonText = this.assetManager.get(this.config.jsonUrl);
				var json = new spine.SkeletonJson(new spine.AtlasAttachmentLoader(atlas));
				try {
					skeletonData = json.readSkeletonData(jsonText);
				}
				catch (e) {
					this.showError("Error: could not load skeleton .json.<br><br>" + escapeHtml(JSON.stringify(e)));
					return;
				}
			}
			else {
				var binaryData = this.assetManager.get(this.config.skelUrl);
				var binary = new spine.SkeletonBinary(new spine.AtlasAttachmentLoader(atlas));
				try {
					skeletonData = binary.readSkeletonData(binaryData);
				}
				catch (e) {
					this.showError("Error: could not load skeleton .skel.<br><br>" + escapeHtml(JSON.stringify(e)));
					return;
				}
			}
			this.skeleton = new spine.Skeleton(skeletonData);
			var stateData = new spine.AnimationStateData(skeletonData);
			stateData.defaultMix = this.config.defaultMix;
			this.animationState = new spine.AnimationState(stateData);
			if (this.config.controlBones) {
				this.config.controlBones.forEach(function (bone) {
					if (!skeletonData.findBone(bone)) {
						_this.showError("Error: control bone '" + bone + "' does not exist in skeleton.");
					}
				});
			}
			if (!this.config.skin) {
				if (skeletonData.skins.length > 0) {
					this.config.skin = skeletonData.skins[0].name;
				}
			}
			if (this.config.skins && this.config.skin.length > 0) {
				this.config.skins.forEach(function (skin) {
					if (!_this.skeleton.data.findSkin(skin)) {
						_this.showError("Error: skin '" + skin + "' in selectable skin list does not exist in skeleton.");
						return;
					}
				});
			}
			if (this.config.skin) {
				if (!this.skeleton.data.findSkin(this.config.skin)) {
					this.showError("Error: skin '" + this.config.skin + "' does not exist in skeleton.");
					return;
				}
				this.skeleton.setSkinByName(this.config.skin);
				this.skeleton.setSlotsToSetupPose();
			}
			if (!this.config.viewport) {
				this.config.viewport = {
					animations: {},
					debugRender: false,
					transitionTime: 0.2
				};
			}
			if (typeof this.config.viewport.debugRender === "undefined")
				this.config.viewport.debugRender = false;
			if (typeof this.config.viewport.transitionTime === "undefined")
				this.config.viewport.transitionTime = 0.2;
			if (!this.config.viewport.animations) {
				this.config.viewport.animations = {};
			}
			else {
				Object.getOwnPropertyNames(this.config.viewport.animations).forEach(function (animation) {
					if (!skeletonData.findAnimation(animation)) {
						_this.showError("Error: animation '" + animation + "' for which a viewport was specified does not exist in skeleton.");
						return;
					}
				});
			}
			if (this.config.animations && this.config.animations.length > 0) {
				this.config.animations.forEach(function (animation) {
					if (!_this.skeleton.data.findAnimation(animation)) {
						_this.showError("Error: animation '" + animation + "' in selectable animation list does not exist in skeleton.");
						return;
					}
				});
				if (!this.config.animation) {
					this.config.animation = this.config.animations[0];
				}
			}
			if (!this.config.animation) {
				if (skeletonData.animations.length > 0) {
					this.config.animation = skeletonData.animations[0].name;
				}
			}
			if (this.config.animation) {
				if (!skeletonData.findAnimation(this.config.animation)) {
					this.showError("Error: animation '" + this.config.animation + "' does not exist in skeleton.");
					return;
				}
				this.play();
				this.timelineSlider.change = function (percentage) {
					_this.pause();
					var animationDuration = _this.animationState.getCurrent(0).animation.duration;
					var time = animationDuration * percentage;
					_this.animationState.update(time - _this.playTime);
					_this.animationState.apply(_this.skeleton);
					_this.skeleton.updateWorldTransform();
					_this.playTime = time;
				};
			}
			this.setupInput();
			if (skeletonData.skins.length == 1 || (this.config.skins && this.config.skins.length == 1))
				this.skinButton.classList.add("spine-player-hidden");
			if (skeletonData.animations.length == 1 || (this.config.animations && this.config.animations.length == 1))
				this.animationButton.classList.add("spine-player-hidden");
			this.config.success(this);
			this.loaded = true;
		};
		SpinePlayer.prototype.setupInput = function () {
			var _this = this;
			var controlBones = this.config.controlBones;
			var selectedBones = this.selectedBones = new Array(this.config.controlBones.length);
			var canvas = this.canvas;
			var input = new spine.webgl.Input(canvas);
			var target = null;
			var coords = new spine.webgl.Vector3();
			var temp = new spine.webgl.Vector3();
			var temp2 = new spine.Vector2();
			var skeleton = this.skeleton;
			var renderer = this.sceneRenderer;
			input.addListener({
				down: function (x, y) {
					for (var i = 0; i < controlBones.length; i++) {
						var bone = skeleton.findBone(controlBones[i]);
						if (!bone)
							continue;
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
							target = bone;
						}
					}
				},
				up: function (x, y) {
					if (target) {
						target = null;
					}
					else {
						if (!_this.config.showControls)
							return;
						if (_this.paused)
							_this.play();
						else
							_this.pause();
					}
				},
				dragged: function (x, y) {
					if (target != null) {
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						if (target.parent !== null) {
							target.parent.worldToLocal(temp2.set(coords.x - skeleton.x, coords.y - skeleton.y));
							target.x = temp2.x;
							target.y = temp2.y;
						}
						else {
							target.x = coords.x - skeleton.x;
							target.y = coords.y - skeleton.y;
						}
					}
				},
				moved: function (x, y) {
					for (var i = 0; i < controlBones.length; i++) {
						var bone = skeleton.findBone(controlBones[i]);
						if (!bone)
							continue;
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
							selectedBones[i] = bone;
						}
						else {
							selectedBones[i] = null;
						}
					}
				}
			});
			var mouseOverControls = true;
			var mouseOverCanvas = false;
			document.addEventListener("mousemove", function (ev) {
				if (ev instanceof MouseEvent) {
					handleHover(ev.clientX, ev.clientY);
				}
			});
			document.addEventListener("touchmove", function (ev) {
				if (ev instanceof TouchEvent) {
					var touches = ev.changedTouches;
					if (touches.length > 0) {
						var touch = touches[0];
						handleHover(touch.clientX, touch.clientY);
					}
				}
			});
			var handleHover = function (mouseX, mouseY) {
				if (!_this.config.showControls)
					return;
				var popup = findWithClass(_this.dom, "spine-player-popup");
				mouseOverControls = overlap(mouseX, mouseY, _this.playerControls.getBoundingClientRect());
				mouseOverCanvas = overlap(mouseX, mouseY, _this.canvas.getBoundingClientRect());
				clearTimeout(_this.cancelId);
				var hide = popup.length == 0 && !mouseOverControls && !mouseOverCanvas && !_this.paused;
				if (hide) {
					_this.playerControls.classList.add("spine-player-controls-hidden");
				}
				else {
					_this.playerControls.classList.remove("spine-player-controls-hidden");
				}
				if (!mouseOverControls && popup.length == 0 && !_this.paused) {
					var remove = function () {
						if (!_this.paused)
							_this.playerControls.classList.add("spine-player-controls-hidden");
					};
					_this.cancelId = setTimeout(remove, 1000);
				}
			};
			var overlap = function (mouseX, mouseY, rect) {
				var x = mouseX - rect.left;
				var y = mouseY - rect.top;
				return x >= 0 && x <= rect.width && y >= 0 && y <= rect.height;
			};
		};
		SpinePlayer.prototype.play = function () {
			var _this = this;
			this.paused = false;
			var remove = function () {
				if (!_this.paused)
					_this.playerControls.classList.add("spine-player-controls-hidden");
			};
			this.cancelId = setTimeout(remove, 1000);
			this.playButton.classList.remove("spine-player-button-icon-play");
			this.playButton.classList.add("spine-player-button-icon-pause");
			if (this.config.animation) {
				if (!this.animationState.getCurrent(0)) {
					this.setAnimation(this.config.animation);
				}
			}
		};
		SpinePlayer.prototype.pause = function () {
			this.paused = true;
			this.playerControls.classList.remove("spine-player-controls-hidden");
			clearTimeout(this.cancelId);
			this.playButton.classList.remove("spine-player-button-icon-pause");
			this.playButton.classList.add("spine-player-button-icon-play");
		};
		SpinePlayer.prototype.setAnimation = function (animation) {
			this.previousViewport = this.currentViewport;
			var animViewport = this.calculateAnimationViewport(animation);
			var viewport = {
				x: animViewport.x,
				y: animViewport.y,
				width: animViewport.width,
				height: animViewport.height,
				padLeft: "10%",
				padRight: "10%",
				padTop: "10%",
				padBottom: "10%"
			};
			var globalViewport = this.config.viewport;
			if (typeof globalViewport.x !== "undefined" && typeof globalViewport.y !== "undefined" && typeof globalViewport.width !== "undefined" && typeof globalViewport.height !== "undefined") {
				viewport.x = globalViewport.x;
				viewport.y = globalViewport.y;
				viewport.width = globalViewport.width;
				viewport.height = globalViewport.height;
			}
			if (typeof globalViewport.padLeft !== "undefined")
				viewport.padLeft = globalViewport.padLeft;
			if (typeof globalViewport.padRight !== "undefined")
				viewport.padRight = globalViewport.padRight;
			if (typeof globalViewport.padTop !== "undefined")
				viewport.padTop = globalViewport.padTop;
			if (typeof globalViewport.padBottom !== "undefined")
				viewport.padBottom = globalViewport.padBottom;
			var userAnimViewport = this.config.viewport.animations[animation];
			if (userAnimViewport) {
				if (typeof userAnimViewport.x !== "undefined" && typeof userAnimViewport.y !== "undefined" && typeof userAnimViewport.width !== "undefined" && typeof userAnimViewport.height !== "undefined") {
					viewport.x = userAnimViewport.x;
					viewport.y = userAnimViewport.y;
					viewport.width = userAnimViewport.width;
					viewport.height = userAnimViewport.height;
				}
				if (typeof userAnimViewport.padLeft !== "undefined")
					viewport.padLeft = userAnimViewport.padLeft;
				if (typeof userAnimViewport.padRight !== "undefined")
					viewport.padRight = userAnimViewport.padRight;
				if (typeof userAnimViewport.padTop !== "undefined")
					viewport.padTop = userAnimViewport.padTop;
				if (typeof userAnimViewport.padBottom !== "undefined")
					viewport.padBottom = userAnimViewport.padBottom;
			}
			viewport.padLeft = this.percentageToWorldUnit(viewport.width, viewport.padLeft);
			viewport.padRight = this.percentageToWorldUnit(viewport.width, viewport.padRight);
			viewport.padBottom = this.percentageToWorldUnit(viewport.height, viewport.padBottom);
			viewport.padTop = this.percentageToWorldUnit(viewport.height, viewport.padTop);
			this.currentViewport = viewport;
			this.viewportTransitionStart = performance.now();
			this.animationState.clearTracks();
			this.skeleton.setToSetupPose();
			this.animationState.setAnimation(0, animation, true);
		};
		SpinePlayer.prototype.percentageToWorldUnit = function (size, percentageOrAbsolute) {
			if (typeof percentageOrAbsolute === "string") {
				return size * parseFloat(percentageOrAbsolute.substr(0, percentageOrAbsolute.length - 1)) / 100;
			}
			else {
				return percentageOrAbsolute;
			}
		};
		SpinePlayer.prototype.calculateAnimationViewport = function (animationName) {
			var animation = this.skeleton.data.findAnimation(animationName);
			this.animationState.clearTracks();
			this.skeleton.setToSetupPose();
			this.animationState.setAnimationWith(0, animation, true);
			var steps = 100;
			var stepTime = animation.duration > 0 ? animation.duration / steps : 0;
			var minX = 100000000;
			var maxX = -100000000;
			var minY = 100000000;
			var maxY = -100000000;
			var offset = new spine.Vector2();
			var size = new spine.Vector2();
			for (var i = 0; i < steps; i++) {
				this.animationState.update(stepTime);
				this.animationState.apply(this.skeleton);
				this.skeleton.updateWorldTransform();
				this.skeleton.getBounds(offset, size);
				minX = Math.min(offset.x, minX);
				maxX = Math.max(offset.x + size.x, maxX);
				minY = Math.min(offset.y, minY);
				maxY = Math.max(offset.y + size.y, maxY);
			}
			offset.x = minX;
			offset.y = minY;
			size.x = maxX - minX;
			size.y = maxY - minY;
			return {
				x: offset.x,
				y: offset.y,
				width: size.x,
				height: size.y
			};
		};
		SpinePlayer.prototype.stopRendering = function () {
			this.stopRequestAnimationFrame = true;
		};
		SpinePlayer.HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.25);
		SpinePlayer.HOVER_COLOR_OUTER = new spine.Color(1, 1, 1, 1);
		SpinePlayer.NON_HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.5);
		SpinePlayer.NON_HOVER_COLOR_OUTER = new spine.Color(1, 0, 0, 0.8);
		return SpinePlayer;
	}());
	spine.SpinePlayer = SpinePlayer;
	function isContained(dom, needle) {
		if (dom === needle)
			return true;
		var findRecursive = function (dom, needle) {
			for (var i = 0; i < dom.children.length; i++) {
				var child = dom.children[i];
				if (child === needle)
					return true;
				if (findRecursive(child, needle))
					return true;
			}
			return false;
		};
		return findRecursive(dom, needle);
	}
	function findWithId(dom, id) {
		var found = new Array();
		var findRecursive = function (dom, id, found) {
			for (var i = 0; i < dom.children.length; i++) {
				var child = dom.children[i];
				if (child.id === id)
					found.push(child);
				findRecursive(child, id, found);
			}
		};
		findRecursive(dom, id, found);
		return found;
	}
	function findWithClass(dom, className) {
		var found = new Array();
		var findRecursive = function (dom, className, found) {
			for (var i = 0; i < dom.children.length; i++) {
				var child = dom.children[i];
				if (child.classList.contains(className))
					found.push(child);
				findRecursive(child, className, found);
			}
		};
		findRecursive(dom, className, found);
		return found;
	}
	function createElement(html) {
		var dom = document.createElement("div");
		dom.innerHTML = html;
		return dom.children[0];
	}
	function removeClass(elements, clazz) {
		for (var i = 0; i < elements.length; i++) {
			elements[i].classList.remove(clazz);
		}
	}
	function escapeHtml(str) {
		if (!str)
			return "";
		return str
			.replace(/&/g, "&amp;")
			.replace(/</g, "&lt;")
			.replace(/>/g, "&gt;")
			.replace(/"/g, "&#34;")
			.replace(/'/g, "&#39;");
	}
})(spine || (spine = {}));
var spine;
(function (spine) {
	var SpinePlayerEditor = (function () {
		function SpinePlayerEditor(parent) {
			this.prefix = "<html>\n<head>\n<style>\nbody {\n\tmargin: 0px;\n}\n</style>\n</head>\n<body>".trim();
			this.postfix = "</body>";
			this.timerId = 0;
			this.render(parent);
		}
		SpinePlayerEditor.prototype.render = function (parent) {
			var _this = this;
			var dom = "\n\t\t\t\t<div class=\"spine-player-editor-container\">\n\t\t\t\t\t<div class=\"spine-player-editor-code\"></div>\n\t\t\t\t\t<iframe class=\"spine-player-editor-player\"></iframe>\n\t\t\t\t</div>\n\t\t\t";
			parent.innerHTML = dom;
			var codeElement = parent.getElementsByClassName("spine-player-editor-code")[0];
			this.player = parent.getElementsByClassName("spine-player-editor-player")[0];
			requestAnimationFrame(function () {
				_this.code = CodeMirror(codeElement, {
					lineNumbers: true,
					tabSize: 3,
					indentUnit: 3,
					indentWithTabs: true,
					scrollBarStyle: "native",
					mode: "htmlmixed",
					theme: "monokai"
				});
				_this.code.on("change", function () {
					_this.startPlayer();
				});
				_this.setCode(SpinePlayerEditor.DEFAULT_CODE);
			});
		};
		SpinePlayerEditor.prototype.setPreAndPostfix = function (prefix, postfix) {
			this.prefix = prefix;
			this.postfix = postfix;
			this.startPlayer();
		};
		SpinePlayerEditor.prototype.setCode = function (code) {
			this.code.setValue(code);
			this.startPlayer();
		};
		SpinePlayerEditor.prototype.startPlayer = function () {
			var _this = this;
			clearTimeout(this.timerId);
			this.timerId = setTimeout(function () {
				var code = _this.code.getDoc().getValue();
				code = _this.prefix + code + _this.postfix;
				code = window.btoa(code);
				_this.player.src = "";
				_this.player.src = "data:text/html;base64," + code;
			}, 500);
		};
		SpinePlayerEditor.DEFAULT_CODE = "\n<script src=\"https://esotericsoftware.com/files/spine-player/3.7/spine-player.js\"></script>\n<link rel=\"stylesheet\" href=\"https://esotericsoftware.com/files/spine-player/3.7/spine-player.css\">\n\n<div id=\"player-container\" style=\"width: 100%; height: 100vh;\"></div>\n\n<script>\nnew spine.SpinePlayer(\"player-container\", {\n\tjsonUrl: \"https://esotericsoftware.com/files/examples/spineboy/export/spineboy-pro.json\",\n\tatlasUrl: \"https://esotericsoftware.com/files/examples/spineboy/export/spineboy-pma.atlas\"\n});\n</script>\n\t\t".trim();
		return SpinePlayerEditor;
	}());
	spine.SpinePlayerEditor = SpinePlayerEditor;
})(spine || (spine = {}));
//# sourceMappingURL=spine-player.js.map