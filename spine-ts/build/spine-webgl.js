var __extends = (this && this.__extends) || (function () {
	var extendStatics = function (d, b) {
		extendStatics = Object.setPrototypeOf ||
			({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
			function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
		return extendStatics(d, b);
	};
	return function (d, b) {
		if (typeof b !== "function" && b !== null)
			throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
		extendStatics(d, b);
		function __() { this.constructor = d; }
		d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
	};
})();
var spine;
(function (spine) {
	var Animation = (function () {
		function Animation(name, timelines, duration) {
			if (!name)
				throw new Error("name cannot be null.");
			this.name = name;
			this.setTimelines(timelines);
			this.duration = duration;
		}
		Animation.prototype.setTimelines = function (timelines) {
			if (!timelines)
				throw new Error("timelines cannot be null.");
			this.timelines = timelines;
			this.timelineIds = new spine.StringSet();
			for (var i = 0; i < timelines.length; i++)
				this.timelineIds.addAll(timelines[i].getPropertyIds());
		};
		Animation.prototype.hasTimeline = function (ids) {
			for (var i = 0; i < ids.length; i++)
				if (this.timelineIds.contains(ids[i]))
					return true;
			return false;
		};
		Animation.prototype.apply = function (skeleton, lastTime, time, loop, events, alpha, blend, direction) {
			if (!skeleton)
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
	var Property = {
		rotate: 0,
		x: 1,
		y: 2,
		scaleX: 3,
		scaleY: 4,
		shearX: 5,
		shearY: 6,
		rgb: 7,
		alpha: 8,
		rgb2: 9,
		attachment: 10,
		deform: 11,
		event: 12,
		drawOrder: 13,
		ikConstraint: 14,
		transformConstraint: 15,
		pathConstraintPosition: 16,
		pathConstraintSpacing: 17,
		pathConstraintMix: 18
	};
	var Timeline = (function () {
		function Timeline(frameCount, propertyIds) {
			this.propertyIds = propertyIds;
			this.frames = spine.Utils.newFloatArray(frameCount * this.getFrameEntries());
		}
		Timeline.prototype.getPropertyIds = function () {
			return this.propertyIds;
		};
		Timeline.prototype.getFrameEntries = function () {
			return 1;
		};
		Timeline.prototype.getFrameCount = function () {
			return this.frames.length / this.getFrameEntries();
		};
		Timeline.prototype.getDuration = function () {
			return this.frames[this.frames.length - this.getFrameEntries()];
		};
		Timeline.search1 = function (frames, time) {
			var n = frames.length;
			for (var i = 1; i < n; i++)
				if (frames[i] > time)
					return i - 1;
			return n - 1;
		};
		Timeline.search = function (frames, time, step) {
			var n = frames.length;
			for (var i = step; i < n; i += step)
				if (frames[i] > time)
					return i - step;
			return n - step;
		};
		return Timeline;
	}());
	spine.Timeline = Timeline;
	var CurveTimeline = (function (_super) {
		__extends(CurveTimeline, _super);
		function CurveTimeline(frameCount, bezierCount, propertyIds) {
			var _this = _super.call(this, frameCount, propertyIds) || this;
			_this.curves = spine.Utils.newFloatArray(frameCount + bezierCount * 18);
			_this.curves[frameCount - 1] = 1;
			return _this;
		}
		CurveTimeline.prototype.setLinear = function (frame) {
			this.curves[frame] = 0;
		};
		CurveTimeline.prototype.setStepped = function (frame) {
			this.curves[frame] = 1;
		};
		CurveTimeline.prototype.shrink = function (bezierCount) {
			var size = this.getFrameCount() + bezierCount * 18;
			if (this.curves.length > size) {
				var newCurves = spine.Utils.newFloatArray(size);
				spine.Utils.arrayCopy(this.curves, 0, newCurves, 0, size);
				this.curves = newCurves;
			}
		};
		CurveTimeline.prototype.setBezier = function (bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2) {
			var curves = this.curves;
			var i = this.getFrameCount() + bezier * 18;
			if (value == 0)
				curves[frame] = 2 + i;
			var tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = (value1 - cy1 * 2 + cy2) * 0.03;
			var dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
			var ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
			var dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667;
			var x = time1 + dx, y = value1 + dy;
			for (var n = i + 18; i < n; i += 2) {
				curves[i] = x;
				curves[i + 1] = y;
				dx += ddx;
				dy += ddy;
				ddx += dddx;
				ddy += dddy;
				x += dx;
				y += dy;
			}
		};
		CurveTimeline.prototype.getBezierValue = function (time, frameIndex, valueOffset, i) {
			var curves = this.curves;
			if (curves[i] > time) {
				var x_1 = this.frames[frameIndex], y_1 = this.frames[frameIndex + valueOffset];
				return y_1 + (time - x_1) / (curves[i] - x_1) * (curves[i + 1] - y_1);
			}
			var n = i + 18;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					var x_2 = curves[i - 2], y_2 = curves[i - 1];
					return y_2 + (time - x_2) / (curves[i] - x_2) * (curves[i + 1] - y_2);
				}
			}
			frameIndex += this.getFrameEntries();
			var x = curves[n - 2], y = curves[n - 1];
			return y + (time - x) / (this.frames[frameIndex] - x) * (this.frames[frameIndex + valueOffset] - y);
		};
		return CurveTimeline;
	}(Timeline));
	spine.CurveTimeline = CurveTimeline;
	var CurveTimeline1 = (function (_super) {
		__extends(CurveTimeline1, _super);
		function CurveTimeline1(frameCount, bezierCount, propertyId) {
			return _super.call(this, frameCount, bezierCount, [propertyId]) || this;
		}
		CurveTimeline1.prototype.getFrameEntries = function () {
			return 2;
		};
		CurveTimeline1.prototype.setFrame = function (frame, time, value) {
			frame <<= 1;
			this.frames[frame] = time;
			this.frames[frame + 1] = value;
		};
		CurveTimeline1.prototype.getCurveValue = function (time) {
			var frames = this.frames;
			var i = frames.length - 2;
			for (var ii = 2; ii <= i; ii += 2) {
				if (frames[ii] > time) {
					i = ii - 2;
					break;
				}
			}
			var curveType = this.curves[i >> 1];
			switch (curveType) {
				case 0:
					var before = frames[i], value = frames[i + 1];
					return value + (time - before) / (frames[i + 2] - before) * (frames[i + 2 + 1] - value);
				case 1:
					return frames[i + 1];
			}
			return this.getBezierValue(time, i, 1, curveType - 2);
		};
		return CurveTimeline1;
	}(CurveTimeline));
	spine.CurveTimeline1 = CurveTimeline1;
	var CurveTimeline2 = (function (_super) {
		__extends(CurveTimeline2, _super);
		function CurveTimeline2(frameCount, bezierCount, propertyId1, propertyId2) {
			return _super.call(this, frameCount, bezierCount, [propertyId1, propertyId2]) || this;
		}
		CurveTimeline2.prototype.getFrameEntries = function () {
			return 3;
		};
		CurveTimeline2.prototype.setFrame = function (frame, time, value1, value2) {
			frame *= 3;
			this.frames[frame] = time;
			this.frames[frame + 1] = value1;
			this.frames[frame + 2] = value2;
		};
		return CurveTimeline2;
	}(CurveTimeline));
	spine.CurveTimeline2 = CurveTimeline2;
	var RotateTimeline = (function (_super) {
		__extends(RotateTimeline, _super);
		function RotateTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.rotate + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		RotateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.rotation = bone.data.rotation;
						return;
					case MixBlend.first:
						bone.rotation += (bone.data.rotation - bone.rotation) * alpha;
				}
				return;
			}
			var r = this.getCurveValue(time);
			switch (blend) {
				case MixBlend.setup:
					bone.rotation = bone.data.rotation + r * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					r += bone.data.rotation - bone.rotation;
				case MixBlend.add:
					bone.rotation += r * alpha;
			}
		};
		return RotateTimeline;
	}(CurveTimeline1));
	spine.RotateTimeline = RotateTimeline;
	var TranslateTimeline = (function (_super) {
		__extends(TranslateTimeline, _super);
		function TranslateTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.x + "|" + boneIndex, Property.y + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		TranslateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
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
			var i = Timeline.search(frames, time, 3);
			var curveType = this.curves[i / 3];
			switch (curveType) {
				case 0:
					var before = frames[i];
					x = frames[i + 1];
					y = frames[i + 2];
					var t = (time - before) / (frames[i + 3] - before);
					x += (frames[i + 3 + 1] - x) * t;
					y += (frames[i + 3 + 2] - y) * t;
					break;
				case 1:
					x = frames[i + 1];
					y = frames[i + 2];
					break;
				default:
					x = this.getBezierValue(time, i, 1, curveType - 2);
					y = this.getBezierValue(time, i, 2, curveType + 18 - 2);
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
		return TranslateTimeline;
	}(CurveTimeline2));
	spine.TranslateTimeline = TranslateTimeline;
	var TranslateXTimeline = (function (_super) {
		__extends(TranslateXTimeline, _super);
		function TranslateXTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.x + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		TranslateXTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.x = bone.data.x;
						return;
					case MixBlend.first:
						bone.x += (bone.data.x - bone.x) * alpha;
				}
				return;
			}
			var x = this.getCurveValue(time);
			switch (blend) {
				case MixBlend.setup:
					bone.x = bone.data.x + x * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					bone.x += (bone.data.x + x - bone.x) * alpha;
					break;
				case MixBlend.add:
					bone.x += x * alpha;
			}
		};
		return TranslateXTimeline;
	}(CurveTimeline1));
	spine.TranslateXTimeline = TranslateXTimeline;
	var TranslateYTimeline = (function (_super) {
		__extends(TranslateYTimeline, _super);
		function TranslateYTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.y + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		TranslateYTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.y = bone.data.y;
						return;
					case MixBlend.first:
						bone.y += (bone.data.y - bone.y) * alpha;
				}
				return;
			}
			var y = this.getCurveValue(time);
			switch (blend) {
				case MixBlend.setup:
					bone.y = bone.data.y + y * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					bone.y += (bone.data.y + y - bone.y) * alpha;
					break;
				case MixBlend.add:
					bone.y += y * alpha;
			}
		};
		return TranslateYTimeline;
	}(CurveTimeline1));
	spine.TranslateYTimeline = TranslateYTimeline;
	var ScaleTimeline = (function (_super) {
		__extends(ScaleTimeline, _super);
		function ScaleTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.scaleX + "|" + boneIndex, Property.scaleY + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		ScaleTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
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
			var x, y;
			var i = Timeline.search(frames, time, 3);
			var curveType = this.curves[i / 3];
			switch (curveType) {
				case 0:
					var before = frames[i];
					x = frames[i + 1];
					y = frames[i + 2];
					var t = (time - before) / (frames[i + 3] - before);
					x += (frames[i + 3 + 1] - x) * t;
					y += (frames[i + 3 + 2] - y) * t;
					break;
				case 1:
					x = frames[i + 1];
					y = frames[i + 2];
					break;
				default:
					x = this.getBezierValue(time, i, 1, curveType - 2);
					y = this.getBezierValue(time, i, 2, curveType + 18 - 2);
			}
			x *= bone.data.scaleX;
			y *= bone.data.scaleY;
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
	}(CurveTimeline2));
	spine.ScaleTimeline = ScaleTimeline;
	var ScaleXTimeline = (function (_super) {
		__extends(ScaleXTimeline, _super);
		function ScaleXTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.scaleX + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		ScaleXTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.scaleX = bone.data.scaleX;
						return;
					case MixBlend.first:
						bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
				}
				return;
			}
			var x = this.getCurveValue(time) * bone.data.scaleX;
			if (alpha == 1) {
				if (blend == MixBlend.add)
					bone.scaleX += x - bone.data.scaleX;
				else
					bone.scaleX = x;
			}
			else {
				var bx = 0;
				if (direction == MixDirection.mixOut) {
					switch (blend) {
						case MixBlend.setup:
							bx = bone.data.scaleX;
							bone.scaleX = bx + (Math.abs(x) * spine.MathUtils.signum(bx) - bx) * alpha;
							break;
						case MixBlend.first:
						case MixBlend.replace:
							bx = bone.scaleX;
							bone.scaleX = bx + (Math.abs(x) * spine.MathUtils.signum(bx) - bx) * alpha;
							break;
						case MixBlend.add:
							bx = bone.scaleX;
							bone.scaleX = bx + (Math.abs(x) * spine.MathUtils.signum(bx) - bone.data.scaleX) * alpha;
					}
				}
				else {
					switch (blend) {
						case MixBlend.setup:
							bx = Math.abs(bone.data.scaleX) * spine.MathUtils.signum(x);
							bone.scaleX = bx + (x - bx) * alpha;
							break;
						case MixBlend.first:
						case MixBlend.replace:
							bx = Math.abs(bone.scaleX) * spine.MathUtils.signum(x);
							bone.scaleX = bx + (x - bx) * alpha;
							break;
						case MixBlend.add:
							bx = spine.MathUtils.signum(x);
							bone.scaleX = Math.abs(bone.scaleX) * bx + (x - Math.abs(bone.data.scaleX) * bx) * alpha;
					}
				}
			}
		};
		return ScaleXTimeline;
	}(CurveTimeline1));
	spine.ScaleXTimeline = ScaleXTimeline;
	var ScaleYTimeline = (function (_super) {
		__extends(ScaleYTimeline, _super);
		function ScaleYTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.scaleY + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		ScaleYTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.scaleY = bone.data.scaleY;
						return;
					case MixBlend.first:
						bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				}
				return;
			}
			var y = this.getCurveValue(time) * bone.data.scaleY;
			if (alpha == 1) {
				if (blend == MixBlend.add)
					bone.scaleY += y - bone.data.scaleY;
				else
					bone.scaleY = y;
			}
			else {
				var by = 0;
				if (direction == MixDirection.mixOut) {
					switch (blend) {
						case MixBlend.setup:
							by = bone.data.scaleY;
							bone.scaleY = by + (Math.abs(y) * spine.MathUtils.signum(by) - by) * alpha;
							break;
						case MixBlend.first:
						case MixBlend.replace:
							by = bone.scaleY;
							bone.scaleY = by + (Math.abs(y) * spine.MathUtils.signum(by) - by) * alpha;
							break;
						case MixBlend.add:
							by = bone.scaleY;
							bone.scaleY = by + (Math.abs(y) * spine.MathUtils.signum(by) - bone.data.scaleY) * alpha;
					}
				}
				else {
					switch (blend) {
						case MixBlend.setup:
							by = Math.abs(bone.data.scaleY) * spine.MathUtils.signum(y);
							bone.scaleY = by + (y - by) * alpha;
							break;
						case MixBlend.first:
						case MixBlend.replace:
							by = Math.abs(bone.scaleY) * spine.MathUtils.signum(y);
							bone.scaleY = by + (y - by) * alpha;
							break;
						case MixBlend.add:
							by = spine.MathUtils.signum(y);
							bone.scaleY = Math.abs(bone.scaleY) * by + (y - Math.abs(bone.data.scaleY) * by) * alpha;
					}
				}
			}
		};
		return ScaleYTimeline;
	}(CurveTimeline1));
	spine.ScaleYTimeline = ScaleYTimeline;
	var ShearTimeline = (function (_super) {
		__extends(ShearTimeline, _super);
		function ShearTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.shearX + "|" + boneIndex, Property.shearY + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		ShearTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
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
			var i = Timeline.search(frames, time, 3);
			var curveType = this.curves[i / 3];
			switch (curveType) {
				case 0:
					var before = frames[i];
					x = frames[i + 1];
					y = frames[i + 2];
					var t = (time - before) / (frames[i + 3] - before);
					x += (frames[i + 3 + 1] - x) * t;
					y += (frames[i + 3 + 2] - y) * t;
					break;
				case 1:
					x = frames[i + 1];
					y = frames[i + 2];
					break;
				default:
					x = this.getBezierValue(time, i, 1, curveType - 2);
					y = this.getBezierValue(time, i, 2, curveType + 18 - 2);
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
	}(CurveTimeline2));
	spine.ShearTimeline = ShearTimeline;
	var ShearXTimeline = (function (_super) {
		__extends(ShearXTimeline, _super);
		function ShearXTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.shearX + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		ShearXTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.shearX = bone.data.shearX;
						return;
					case MixBlend.first:
						bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
				}
				return;
			}
			var x = this.getCurveValue(time);
			switch (blend) {
				case MixBlend.setup:
					bone.shearX = bone.data.shearX + x * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
					break;
				case MixBlend.add:
					bone.shearX += x * alpha;
			}
		};
		return ShearXTimeline;
	}(CurveTimeline1));
	spine.ShearXTimeline = ShearXTimeline;
	var ShearYTimeline = (function (_super) {
		__extends(ShearYTimeline, _super);
		function ShearYTimeline(frameCount, bezierCount, boneIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.shearY + "|" + boneIndex) || this;
			_this.boneIndex = 0;
			_this.boneIndex = boneIndex;
			return _this;
		}
		ShearYTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var bone = skeleton.bones[this.boneIndex];
			if (!bone.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						bone.shearY = bone.data.shearY;
						return;
					case MixBlend.first:
						bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
				}
				return;
			}
			var y = this.getCurveValue(time);
			switch (blend) {
				case MixBlend.setup:
					bone.shearY = bone.data.shearY + y * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
					break;
				case MixBlend.add:
					bone.shearY += y * alpha;
			}
		};
		return ShearYTimeline;
	}(CurveTimeline1));
	spine.ShearYTimeline = ShearYTimeline;
	var RGBATimeline = (function (_super) {
		__extends(RGBATimeline, _super);
		function RGBATimeline(frameCount, bezierCount, slotIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.alpha + "|" + slotIndex
			]) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			return _this;
		}
		RGBATimeline.prototype.getFrameEntries = function () {
			return 5;
		};
		RGBATimeline.prototype.setFrame = function (frame, time, r, g, b, a) {
			frame *= 5;
			this.frames[frame] = time;
			this.frames[frame + 1] = r;
			this.frames[frame + 2] = g;
			this.frames[frame + 3] = b;
			this.frames[frame + 4] = a;
		};
		RGBATimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = this.frames;
			var color = slot.color;
			if (time < frames[0]) {
				var setup = slot.data.color;
				switch (blend) {
					case MixBlend.setup:
						color.setFromColor(setup);
						return;
					case MixBlend.first:
						color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha, (setup.a - color.a) * alpha);
				}
				return;
			}
			var r = 0, g = 0, b = 0, a = 0;
			var i = Timeline.search(frames, time, 5);
			var curveType = this.curves[i / 5];
			switch (curveType) {
				case 0:
					var before = frames[i];
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					a = frames[i + 4];
					var t = (time - before) / (frames[i + 5] - before);
					r += (frames[i + 5 + 1] - r) * t;
					g += (frames[i + 5 + 2] - g) * t;
					b += (frames[i + 5 + 3] - b) * t;
					a += (frames[i + 5 + 4] - a) * t;
					break;
				case 1:
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					a = frames[i + 4];
					break;
				default:
					r = this.getBezierValue(time, i, 1, curveType - 2);
					g = this.getBezierValue(time, i, 2, curveType + 18 - 2);
					b = this.getBezierValue(time, i, 3, curveType + 18 * 2 - 2);
					a = this.getBezierValue(time, i, 4, curveType + 18 * 3 - 2);
			}
			if (alpha == 1)
				color.set(r, g, b, a);
			else {
				if (blend == MixBlend.setup)
					color.setFromColor(slot.data.color);
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		};
		return RGBATimeline;
	}(CurveTimeline));
	spine.RGBATimeline = RGBATimeline;
	var RGBTimeline = (function (_super) {
		__extends(RGBTimeline, _super);
		function RGBTimeline(frameCount, bezierCount, slotIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex
			]) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			return _this;
		}
		RGBTimeline.prototype.getFrameEntries = function () {
			return 4;
		};
		RGBTimeline.prototype.setFrame = function (frame, time, r, g, b) {
			frame <<= 2;
			this.frames[frame] = time;
			this.frames[frame + 1] = r;
			this.frames[frame + 2] = g;
			this.frames[frame + 3] = b;
		};
		RGBTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = this.frames;
			var color = slot.color;
			if (time < frames[0]) {
				var setup = slot.data.color;
				switch (blend) {
					case MixBlend.setup:
						color.r = setup.r;
						color.g = setup.g;
						color.b = setup.b;
						return;
					case MixBlend.first:
						color.r += (setup.r - color.r) * alpha;
						color.g += (setup.g - color.g) * alpha;
						color.b += (setup.b - color.b) * alpha;
				}
				return;
			}
			var r = 0, g = 0, b = 0;
			var i = Timeline.search(frames, time, 4);
			var curveType = this.curves[i >> 2];
			switch (curveType) {
				case 0:
					var before = frames[i];
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					var t = (time - before) / (frames[i + 4] - before);
					r += (frames[i + 4 + 1] - r) * t;
					g += (frames[i + 4 + 2] - g) * t;
					b += (frames[i + 4 + 3] - b) * t;
					break;
				case 1:
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					break;
				default:
					r = this.getBezierValue(time, i, 1, curveType - 2);
					g = this.getBezierValue(time, i, 2, curveType + 18 - 2);
					b = this.getBezierValue(time, i, 3, curveType + 18 * 2 - 2);
			}
			if (alpha == 1) {
				color.r = r;
				color.g = g;
				color.b = b;
			}
			else {
				if (blend == MixBlend.setup) {
					var setup = slot.data.color;
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
				}
				color.r += (r - color.r) * alpha;
				color.g += (g - color.g) * alpha;
				color.b += (b - color.b) * alpha;
			}
		};
		return RGBTimeline;
	}(CurveTimeline));
	spine.RGBTimeline = RGBTimeline;
	var AlphaTimeline = (function (_super) {
		__extends(AlphaTimeline, _super);
		function AlphaTimeline(frameCount, bezierCount, slotIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.alpha + "|" + slotIndex) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			return _this;
		}
		AlphaTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var color = slot.color;
			if (time < this.frames[0]) {
				var setup = slot.data.color;
				switch (blend) {
					case MixBlend.setup:
						color.a = setup.a;
						return;
					case MixBlend.first:
						color.a += (setup.a - color.a) * alpha;
				}
				return;
			}
			var a = this.getCurveValue(time);
			if (alpha == 1)
				color.a = a;
			else {
				if (blend == MixBlend.setup)
					color.a = slot.data.color.a;
				color.a += (a - color.a) * alpha;
			}
		};
		return AlphaTimeline;
	}(CurveTimeline1));
	spine.AlphaTimeline = AlphaTimeline;
	var RGBA2Timeline = (function (_super) {
		__extends(RGBA2Timeline, _super);
		function RGBA2Timeline(frameCount, bezierCount, slotIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.alpha + "|" + slotIndex,
				Property.rgb2 + "|" + slotIndex
			]) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			return _this;
		}
		RGBA2Timeline.prototype.getFrameEntries = function () {
			return 8;
		};
		RGBA2Timeline.prototype.setFrame = function (frame, time, r, g, b, a, r2, g2, b2) {
			frame <<= 3;
			this.frames[frame] = time;
			this.frames[frame + 1] = r;
			this.frames[frame + 2] = g;
			this.frames[frame + 3] = b;
			this.frames[frame + 4] = a;
			this.frames[frame + 5] = r2;
			this.frames[frame + 6] = g2;
			this.frames[frame + 7] = b2;
		};
		RGBA2Timeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = this.frames;
			var light = slot.color, dark = slot.darkColor;
			if (time < frames[0]) {
				var setupLight = slot.data.color, setupDark = slot.data.darkColor;
				switch (blend) {
					case MixBlend.setup:
						light.setFromColor(setupLight);
						dark.r = setupDark.r;
						dark.g = setupDark.g;
						dark.b = setupDark.b;
						return;
					case MixBlend.first:
						light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha, (setupLight.a - light.a) * alpha);
						dark.r += (setupDark.r - dark.r) * alpha;
						dark.g += (setupDark.g - dark.g) * alpha;
						dark.b += (setupDark.b - dark.b) * alpha;
				}
				return;
			}
			var r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
			var i = Timeline.search(frames, time, 8);
			var curveType = this.curves[i >> 3];
			switch (curveType) {
				case 0:
					var before = frames[i];
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					a = frames[i + 4];
					r2 = frames[i + 5];
					g2 = frames[i + 6];
					b2 = frames[i + 7];
					var t = (time - before) / (frames[i + 8] - before);
					r += (frames[i + 8 + 1] - r) * t;
					g += (frames[i + 8 + 2] - g) * t;
					b += (frames[i + 8 + 3] - b) * t;
					a += (frames[i + 8 + 4] - a) * t;
					r2 += (frames[i + 8 + 5] - r2) * t;
					g2 += (frames[i + 8 + 6] - g2) * t;
					b2 += (frames[i + 8 + 7] - b2) * t;
					break;
				case 1:
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					a = frames[i + 4];
					r2 = frames[i + 5];
					g2 = frames[i + 6];
					b2 = frames[i + 7];
					break;
				default:
					r = this.getBezierValue(time, i, 1, curveType - 2);
					g = this.getBezierValue(time, i, 2, curveType + 18 - 2);
					b = this.getBezierValue(time, i, 3, curveType + 18 * 2 - 2);
					a = this.getBezierValue(time, i, 4, curveType + 18 * 3 - 2);
					r2 = this.getBezierValue(time, i, 5, curveType + 18 * 4 - 2);
					g2 = this.getBezierValue(time, i, 6, curveType + 18 * 5 - 2);
					b2 = this.getBezierValue(time, i, 7, curveType + 18 * 6 - 2);
			}
			if (alpha == 1) {
				light.set(r, g, b, a);
				dark.r = r2;
				dark.g = g2;
				dark.b = b2;
			}
			else {
				if (blend == MixBlend.setup) {
					light.setFromColor(slot.data.color);
					var setupDark = slot.data.darkColor;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.r += (r2 - dark.r) * alpha;
				dark.g += (g2 - dark.g) * alpha;
				dark.b += (b2 - dark.b) * alpha;
			}
		};
		return RGBA2Timeline;
	}(CurveTimeline));
	spine.RGBA2Timeline = RGBA2Timeline;
	var RGB2Timeline = (function (_super) {
		__extends(RGB2Timeline, _super);
		function RGB2Timeline(frameCount, bezierCount, slotIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.rgb2 + "|" + slotIndex
			]) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			return _this;
		}
		RGB2Timeline.prototype.getFrameEntries = function () {
			return 7;
		};
		RGB2Timeline.prototype.setFrame = function (frame, time, r, g, b, r2, g2, b2) {
			frame *= 7;
			this.frames[frame] = time;
			this.frames[frame + 1] = r;
			this.frames[frame + 2] = g;
			this.frames[frame + 3] = b;
			this.frames[frame + 4] = r2;
			this.frames[frame + 5] = g2;
			this.frames[frame + 6] = b2;
		};
		RGB2Timeline.prototype.apply = function (skeleton, lastTime, time, events, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var frames = this.frames;
			var light = slot.color, dark = slot.darkColor;
			if (time < frames[0]) {
				var setupLight = slot.data.color, setupDark = slot.data.darkColor;
				switch (blend) {
					case MixBlend.setup:
						light.r = setupLight.r;
						light.g = setupLight.g;
						light.b = setupLight.b;
						dark.r = setupDark.r;
						dark.g = setupDark.g;
						dark.b = setupDark.b;
						return;
					case MixBlend.first:
						light.r += (setupLight.r - light.r) * alpha;
						light.g += (setupLight.g - light.g) * alpha;
						light.b += (setupLight.b - light.b) * alpha;
						dark.r += (setupDark.r - dark.r) * alpha;
						dark.g += (setupDark.g - dark.g) * alpha;
						dark.b += (setupDark.b - dark.b) * alpha;
				}
				return;
			}
			var r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
			var i = Timeline.search(frames, time, 7);
			var curveType = this.curves[i / 7];
			switch (curveType) {
				case 0:
					var before = frames[i];
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					r2 = frames[i + 4];
					g2 = frames[i + 5];
					b2 = frames[i + 6];
					var t = (time - before) / (frames[i + 7] - before);
					r += (frames[i + 7 + 1] - r) * t;
					g += (frames[i + 7 + 2] - g) * t;
					b += (frames[i + 7 + 3] - b) * t;
					r2 += (frames[i + 7 + 4] - r2) * t;
					g2 += (frames[i + 7 + 5] - g2) * t;
					b2 += (frames[i + 7 + 6] - b2) * t;
					break;
				case 1:
					r = frames[i + 1];
					g = frames[i + 2];
					b = frames[i + 3];
					r2 = frames[i + 4];
					g2 = frames[i + 5];
					b2 = frames[i + 6];
					break;
				default:
					r = this.getBezierValue(time, i, 1, curveType - 2);
					g = this.getBezierValue(time, i, 2, curveType + 18 - 2);
					b = this.getBezierValue(time, i, 3, curveType + 18 * 2 - 2);
					r2 = this.getBezierValue(time, i, 4, curveType + 18 * 3 - 2);
					g2 = this.getBezierValue(time, i, 5, curveType + 18 * 4 - 2);
					b2 = this.getBezierValue(time, i, 6, curveType + 18 * 5 - 2);
			}
			if (alpha == 1) {
				light.r = r;
				light.g = g;
				light.b = b;
				dark.r = r2;
				dark.g = g2;
				dark.b = b2;
			}
			else {
				if (blend == MixBlend.setup) {
					var setupLight = slot.data.color, setupDark = slot.data.darkColor;
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
				}
				light.r += (r - light.r) * alpha;
				light.g += (g - light.g) * alpha;
				light.b += (b - light.b) * alpha;
				dark.r += (r2 - dark.r) * alpha;
				dark.g += (g2 - dark.g) * alpha;
				dark.b += (b2 - dark.b) * alpha;
			}
		};
		return RGB2Timeline;
	}(CurveTimeline));
	spine.RGB2Timeline = RGB2Timeline;
	var AttachmentTimeline = (function (_super) {
		__extends(AttachmentTimeline, _super);
		function AttachmentTimeline(frameCount, slotIndex) {
			var _this = _super.call(this, frameCount, [
				Property.attachment + "|" + slotIndex
			]) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			_this.attachmentNames = new Array(frameCount);
			return _this;
		}
		AttachmentTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		AttachmentTimeline.prototype.setFrame = function (frame, time, attachmentName) {
			this.frames[frame] = time;
			this.attachmentNames[frame] = attachmentName;
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
			if (time < this.frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first)
					this.setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}
			this.setAttachment(skeleton, slot, this.attachmentNames[Timeline.search1(this.frames, time)]);
		};
		AttachmentTimeline.prototype.setAttachment = function (skeleton, slot, attachmentName) {
			slot.setAttachment(!attachmentName ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
		};
		return AttachmentTimeline;
	}(Timeline));
	spine.AttachmentTimeline = AttachmentTimeline;
	var DeformTimeline = (function (_super) {
		__extends(DeformTimeline, _super);
		function DeformTimeline(frameCount, bezierCount, slotIndex, attachment) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.deform + "|" + slotIndex + "|" + attachment.id
			]) || this;
			_this.slotIndex = 0;
			_this.slotIndex = slotIndex;
			_this.attachment = attachment;
			_this.vertices = new Array(frameCount);
			return _this;
		}
		DeformTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		DeformTimeline.prototype.setFrame = function (frame, time, vertices) {
			this.frames[frame] = time;
			this.vertices[frame] = vertices;
		};
		DeformTimeline.prototype.setBezier = function (bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2) {
			var curves = this.curves;
			var i = this.getFrameCount() + bezier * 18;
			if (value == 0)
				curves[frame] = 2 + i;
			var tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = cy2 * 0.03 - cy1 * 0.06;
			var dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = (cy1 - cy2 + 0.33333333) * 0.018;
			var ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
			var dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = cy1 * 0.3 + tmpy + dddy * 0.16666667;
			var x = time1 + dx, y = dy;
			for (var n = i + 18; i < n; i += 2) {
				curves[i] = x;
				curves[i + 1] = y;
				dx += ddx;
				dy += ddy;
				ddx += dddx;
				ddy += dddy;
				x += dx;
				y += dy;
			}
		};
		DeformTimeline.prototype.getCurvePercent = function (time, frame) {
			var curves = this.curves;
			var i = curves[frame];
			switch (i) {
				case 0:
					var x_3 = this.frames[frame];
					return (time - x_3) / (this.frames[frame + this.getFrameEntries()] - x_3);
				case 1:
					return 0;
			}
			i -= 2;
			if (curves[i] > time) {
				var x_4 = this.frames[frame];
				return curves[i + 1] * (time - x_4) / (curves[i] - x_4);
			}
			var n = i + 18;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					var x_5 = curves[i - 2], y_3 = curves[i - 1];
					return y_3 + (time - x_5) / (curves[i] - x_5) * (curves[i + 1] - y_3);
				}
			}
			var x = curves[n - 2], y = curves[n - 1];
			return y + (1 - y) * (time - x) / (this.frames[frame + this.getFrameEntries()] - x);
		};
		DeformTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active)
				return;
			var slotAttachment = slot.getAttachment();
			if (!(slotAttachment instanceof spine.VertexAttachment) || slotAttachment.deformAttachment != this.attachment)
				return;
			var deform = slot.deform;
			if (deform.length == 0)
				blend = MixBlend.setup;
			var vertices = this.vertices;
			var vertexCount = vertices[0].length;
			var frames = this.frames;
			if (time < frames[0]) {
				var vertexAttachment = slotAttachment;
				switch (blend) {
					case MixBlend.setup:
						deform.length = 0;
						return;
					case MixBlend.first:
						if (alpha == 1) {
							deform.length = 0;
							return;
						}
						deform.length = vertexCount;
						if (!vertexAttachment.bones) {
							var setupVertices = vertexAttachment.vertices;
							for (var i = 0; i < vertexCount; i++)
								deform[i] += (setupVertices[i] - deform[i]) * alpha;
						}
						else {
							alpha = 1 - alpha;
							for (var i = 0; i < vertexCount; i++)
								deform[i] *= alpha;
						}
				}
				return;
			}
			deform.length = vertexCount;
			if (time >= frames[frames.length - 1]) {
				var lastVertices = vertices[frames.length - 1];
				if (alpha == 1) {
					if (blend == MixBlend.add) {
						var vertexAttachment = slotAttachment;
						if (!vertexAttachment.bones) {
							var setupVertices = vertexAttachment.vertices;
							for (var i_1 = 0; i_1 < vertexCount; i_1++)
								deform[i_1] += lastVertices[i_1] - setupVertices[i_1];
						}
						else {
							for (var i_2 = 0; i_2 < vertexCount; i_2++)
								deform[i_2] += lastVertices[i_2];
						}
					}
					else
						spine.Utils.arrayCopy(lastVertices, 0, deform, 0, vertexCount);
				}
				else {
					switch (blend) {
						case MixBlend.setup: {
							var vertexAttachment_1 = slotAttachment;
							if (!vertexAttachment_1.bones) {
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
							if (!vertexAttachment.bones) {
								var setupVertices = vertexAttachment.vertices;
								for (var i_6 = 0; i_6 < vertexCount; i_6++)
									deform[i_6] += (lastVertices[i_6] - setupVertices[i_6]) * alpha;
							}
							else {
								for (var i_7 = 0; i_7 < vertexCount; i_7++)
									deform[i_7] += lastVertices[i_7] * alpha;
							}
					}
				}
				return;
			}
			var frame = Timeline.search1(frames, time);
			var percent = this.getCurvePercent(time, frame);
			var prevVertices = vertices[frame];
			var nextVertices = vertices[frame + 1];
			if (alpha == 1) {
				if (blend == MixBlend.add) {
					var vertexAttachment = slotAttachment;
					if (!vertexAttachment.bones) {
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
						if (!vertexAttachment_2.bones) {
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
						if (!vertexAttachment.bones) {
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
	var EventTimeline = (function (_super) {
		__extends(EventTimeline, _super);
		function EventTimeline(frameCount) {
			var _this = _super.call(this, frameCount, EventTimeline.propertyIds) || this;
			_this.events = new Array(frameCount);
			return _this;
		}
		EventTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		EventTimeline.prototype.setFrame = function (frame, event) {
			this.frames[frame] = event.time;
			this.events[frame] = event;
		};
		EventTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			if (!firedEvents)
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
			var i = 0;
			if (lastTime < frames[0])
				i = 0;
			else {
				i = Timeline.search1(frames, lastTime) + 1;
				var frameTime = frames[i];
				while (i > 0) {
					if (frames[i - 1] != frameTime)
						break;
					i--;
				}
			}
			for (; i < frameCount && time >= frames[i]; i++)
				firedEvents.push(this.events[i]);
		};
		EventTimeline.propertyIds = ["" + Property.event];
		return EventTimeline;
	}(Timeline));
	spine.EventTimeline = EventTimeline;
	var DrawOrderTimeline = (function (_super) {
		__extends(DrawOrderTimeline, _super);
		function DrawOrderTimeline(frameCount) {
			var _this = _super.call(this, frameCount, DrawOrderTimeline.propertyIds) || this;
			_this.drawOrders = new Array(frameCount);
			return _this;
		}
		DrawOrderTimeline.prototype.getFrameCount = function () {
			return this.frames.length;
		};
		DrawOrderTimeline.prototype.setFrame = function (frame, time, drawOrder) {
			this.frames[frame] = time;
			this.drawOrders[frame] = drawOrder;
		};
		DrawOrderTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup)
					spine.Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}
			if (time < this.frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first)
					spine.Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}
			var drawOrderToSetupIndex = this.drawOrders[Timeline.search1(this.frames, time)];
			if (!drawOrderToSetupIndex)
				spine.Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
			else {
				var drawOrder = skeleton.drawOrder;
				var slots = skeleton.slots;
				for (var i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		};
		DrawOrderTimeline.propertyIds = ["" + Property.drawOrder];
		return DrawOrderTimeline;
	}(Timeline));
	spine.DrawOrderTimeline = DrawOrderTimeline;
	var IkConstraintTimeline = (function (_super) {
		__extends(IkConstraintTimeline, _super);
		function IkConstraintTimeline(frameCount, bezierCount, ikConstraintIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.ikConstraint + "|" + ikConstraintIndex
			]) || this;
			_this.ikConstraintIndex = ikConstraintIndex;
			return _this;
		}
		IkConstraintTimeline.prototype.getFrameEntries = function () {
			return 6;
		};
		IkConstraintTimeline.prototype.setFrame = function (frame, time, mix, softness, bendDirection, compress, stretch) {
			frame *= 6;
			this.frames[frame] = time;
			this.frames[frame + 1] = mix;
			this.frames[frame + 2] = softness;
			this.frames[frame + 3] = bendDirection;
			this.frames[frame + 4] = compress ? 1 : 0;
			this.frames[frame + 5] = stretch ? 1 : 0;
		};
		IkConstraintTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var constraint = skeleton.ikConstraints[this.ikConstraintIndex];
			if (!constraint.active)
				return;
			var frames = this.frames;
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
			var mix = 0, softness = 0;
			var i = Timeline.search(frames, time, 6);
			var curveType = this.curves[i / 6];
			switch (curveType) {
				case 0:
					var before = frames[i];
					mix = frames[i + 1];
					softness = frames[i + 2];
					var t = (time - before) / (frames[i + 6] - before);
					mix += (frames[i + 6 + 1] - mix) * t;
					softness += (frames[i + 6 + 2] - softness) * t;
					break;
				case 1:
					mix = frames[i + 1];
					softness = frames[i + 2];
					break;
				default:
					mix = this.getBezierValue(time, i, 1, curveType - 2);
					softness = this.getBezierValue(time, i, 2, curveType + 18 - 2);
			}
			if (blend == MixBlend.setup) {
				constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha;
				if (direction == MixDirection.mixOut) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				}
				else {
					constraint.bendDirection = frames[i + 3];
					constraint.compress = frames[i + 4] != 0;
					constraint.stretch = frames[i + 5] != 0;
				}
			}
			else {
				constraint.mix += (mix - constraint.mix) * alpha;
				constraint.softness += (softness - constraint.softness) * alpha;
				if (direction == MixDirection.mixIn) {
					constraint.bendDirection = frames[i + 3];
					constraint.compress = frames[i + 4] != 0;
					constraint.stretch = frames[i + 5] != 0;
				}
			}
		};
		return IkConstraintTimeline;
	}(CurveTimeline));
	spine.IkConstraintTimeline = IkConstraintTimeline;
	var TransformConstraintTimeline = (function (_super) {
		__extends(TransformConstraintTimeline, _super);
		function TransformConstraintTimeline(frameCount, bezierCount, transformConstraintIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.transformConstraint + "|" + transformConstraintIndex
			]) || this;
			_this.transformConstraintIndex = transformConstraintIndex;
			return _this;
		}
		TransformConstraintTimeline.prototype.getFrameEntries = function () {
			return 7;
		};
		TransformConstraintTimeline.prototype.setFrame = function (frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY) {
			var frames = this.frames;
			frame *= 7;
			frames[frame] = time;
			frames[frame + 1] = mixRotate;
			frames[frame + 2] = mixX;
			frames[frame + 3] = mixY;
			frames[frame + 4] = mixScaleX;
			frames[frame + 5] = mixScaleY;
			frames[frame + 6] = mixShearY;
		};
		TransformConstraintTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var constraint = skeleton.transformConstraints[this.transformConstraintIndex];
			if (!constraint.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				var data = constraint.data;
				switch (blend) {
					case MixBlend.setup:
						constraint.mixRotate = data.mixRotate;
						constraint.mixX = data.mixX;
						constraint.mixY = data.mixY;
						constraint.mixScaleX = data.mixScaleX;
						constraint.mixScaleY = data.mixScaleY;
						constraint.mixShearY = data.mixShearY;
						return;
					case MixBlend.first:
						constraint.mixRotate += (data.mixRotate - constraint.mixRotate) * alpha;
						constraint.mixX += (data.mixX - constraint.mixX) * alpha;
						constraint.mixY += (data.mixY - constraint.mixY) * alpha;
						constraint.mixScaleX += (data.mixScaleX - constraint.mixScaleX) * alpha;
						constraint.mixScaleY += (data.mixScaleY - constraint.mixScaleY) * alpha;
						constraint.mixShearY += (data.mixShearY - constraint.mixShearY) * alpha;
				}
				return;
			}
			var rotate, x, y, scaleX, scaleY, shearY;
			var i = Timeline.search(frames, time, 7);
			var curveType = this.curves[i / 7];
			switch (curveType) {
				case 0:
					var before = frames[i];
					rotate = frames[i + 1];
					x = frames[i + 2];
					y = frames[i + 3];
					scaleX = frames[i + 4];
					scaleY = frames[i + 5];
					shearY = frames[i + 6];
					var t = (time - before) / (frames[i + 7] - before);
					rotate += (frames[i + 7 + 1] - rotate) * t;
					x += (frames[i + 7 + 2] - x) * t;
					y += (frames[i + 7 + 3] - y) * t;
					scaleX += (frames[i + 7 + 4] - scaleX) * t;
					scaleY += (frames[i + 7 + 5] - scaleY) * t;
					shearY += (frames[i + 7 + 6] - shearY) * t;
					break;
				case 1:
					rotate = frames[i + 1];
					x = frames[i + 2];
					y = frames[i + 3];
					scaleX = frames[i + 4];
					scaleY = frames[i + 5];
					shearY = frames[i + 6];
					break;
				default:
					rotate = this.getBezierValue(time, i, 1, curveType - 2);
					x = this.getBezierValue(time, i, 2, curveType + 18 - 2);
					y = this.getBezierValue(time, i, 3, curveType + 18 * 2 - 2);
					scaleX = this.getBezierValue(time, i, 4, curveType + 18 * 3 - 2);
					scaleY = this.getBezierValue(time, i, 5, curveType + 18 * 4 - 2);
					shearY = this.getBezierValue(time, i, 6, curveType + 18 * 5 - 2);
			}
			if (blend == MixBlend.setup) {
				var data = constraint.data;
				constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
				constraint.mixX = data.mixX + (x - data.mixX) * alpha;
				constraint.mixY = data.mixY + (y - data.mixY) * alpha;
				constraint.mixScaleX = data.mixScaleX + (scaleX - data.mixScaleX) * alpha;
				constraint.mixScaleY = data.mixScaleY + (scaleY - data.mixScaleY) * alpha;
				constraint.mixShearY = data.mixShearY + (shearY - data.mixShearY) * alpha;
			}
			else {
				constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
				constraint.mixX += (x - constraint.mixX) * alpha;
				constraint.mixY += (y - constraint.mixY) * alpha;
				constraint.mixScaleX += (scaleX - constraint.mixScaleX) * alpha;
				constraint.mixScaleY += (scaleY - constraint.mixScaleY) * alpha;
				constraint.mixShearY += (shearY - constraint.mixShearY) * alpha;
			}
		};
		return TransformConstraintTimeline;
	}(CurveTimeline));
	spine.TransformConstraintTimeline = TransformConstraintTimeline;
	var PathConstraintPositionTimeline = (function (_super) {
		__extends(PathConstraintPositionTimeline, _super);
		function PathConstraintPositionTimeline(frameCount, bezierCount, pathConstraintIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.pathConstraintPosition + "|" + pathConstraintIndex) || this;
			_this.pathConstraintIndex = pathConstraintIndex;
			return _this;
		}
		PathConstraintPositionTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active)
				return;
			var frames = this.frames;
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
			var position = this.getCurveValue(time);
			if (blend == MixBlend.setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		};
		return PathConstraintPositionTimeline;
	}(CurveTimeline1));
	spine.PathConstraintPositionTimeline = PathConstraintPositionTimeline;
	var PathConstraintSpacingTimeline = (function (_super) {
		__extends(PathConstraintSpacingTimeline, _super);
		function PathConstraintSpacingTimeline(frameCount, bezierCount, pathConstraintIndex) {
			var _this = _super.call(this, frameCount, bezierCount, Property.pathConstraintSpacing + "|" + pathConstraintIndex) || this;
			_this.pathConstraintIndex = 0;
			_this.pathConstraintIndex = pathConstraintIndex;
			return _this;
		}
		PathConstraintSpacingTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active)
				return;
			var frames = this.frames;
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
			var spacing = this.getCurveValue(time);
			if (blend == MixBlend.setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		};
		return PathConstraintSpacingTimeline;
	}(CurveTimeline1));
	spine.PathConstraintSpacingTimeline = PathConstraintSpacingTimeline;
	var PathConstraintMixTimeline = (function (_super) {
		__extends(PathConstraintMixTimeline, _super);
		function PathConstraintMixTimeline(frameCount, bezierCount, pathConstraintIndex) {
			var _this = _super.call(this, frameCount, bezierCount, [
				Property.pathConstraintMix + "|" + pathConstraintIndex
			]) || this;
			_this.pathConstraintIndex = 0;
			_this.pathConstraintIndex = pathConstraintIndex;
			return _this;
		}
		PathConstraintMixTimeline.prototype.getFrameEntries = function () {
			return 4;
		};
		PathConstraintMixTimeline.prototype.setFrame = function (frame, time, mixRotate, mixX, mixY) {
			var frames = this.frames;
			frame <<= 2;
			frames[frame] = time;
			frames[frame + 1] = mixRotate;
			frames[frame + 2] = mixX;
			frames[frame + 3] = mixY;
		};
		PathConstraintMixTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha, blend, direction) {
			var constraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active)
				return;
			var frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
					case MixBlend.setup:
						constraint.mixRotate = constraint.data.mixRotate;
						constraint.mixX = constraint.data.mixX;
						constraint.mixY = constraint.data.mixY;
						return;
					case MixBlend.first:
						constraint.mixRotate += (constraint.data.mixRotate - constraint.mixRotate) * alpha;
						constraint.mixX += (constraint.data.mixX - constraint.mixX) * alpha;
						constraint.mixY += (constraint.data.mixY - constraint.mixY) * alpha;
				}
				return;
			}
			var rotate, x, y;
			var i = Timeline.search(frames, time, 4);
			var curveType = this.curves[i >> 2];
			switch (curveType) {
				case 0:
					var before = frames[i];
					rotate = frames[i + 1];
					x = frames[i + 2];
					y = frames[i + 3];
					var t = (time - before) / (frames[i + 4] - before);
					rotate += (frames[i + 4 + 1] - rotate) * t;
					x += (frames[i + 4 + 2] - x) * t;
					y += (frames[i + 4 + 3] - y) * t;
					break;
				case 1:
					rotate = frames[i + 1];
					x = frames[i + 2];
					y = frames[i + 3];
					break;
				default:
					rotate = this.getBezierValue(time, i, 1, curveType - 2);
					x = this.getBezierValue(time, i, 2, curveType + 18 - 2);
					y = this.getBezierValue(time, i, 3, curveType + 18 * 2 - 2);
			}
			if (blend == MixBlend.setup) {
				var data = constraint.data;
				constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
				constraint.mixX = data.mixX + (x - data.mixX) * alpha;
				constraint.mixY = data.mixY + (y - data.mixY) * alpha;
			}
			else {
				constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
				constraint.mixX += (x - constraint.mixX) * alpha;
				constraint.mixY += (y - constraint.mixY) * alpha;
			}
		};
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
			this.propertyIDs = new spine.StringSet();
			this.animationsChanged = false;
			this.trackEntryPool = new spine.Pool(function () { return new TrackEntry(); });
			this.data = data;
		}
		AnimationState.emptyAnimation = function () {
			if (!_emptyAnimation)
				_emptyAnimation = new spine.Animation("<empty>", [], 0);
			return _emptyAnimation;
		};
		AnimationState.prototype.update = function (delta) {
			delta *= this.timeScale;
			var tracks = this.tracks;
			for (var i = 0, n = tracks.length; i < n; i++) {
				var current = tracks[i];
				if (!current)
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
				if (next) {
					var nextTime = current.trackLast - next.delay;
					if (nextTime >= 0) {
						next.delay = 0;
						next.trackTime += current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
						current.trackTime += currentDelta;
						this.setCurrent(i, next, true);
						while (next.mixingFrom) {
							next.mixTime += delta;
							next = next.mixingFrom;
						}
						continue;
					}
				}
				else if (current.trackLast >= current.trackEnd && !current.mixingFrom) {
					tracks[i] = null;
					this.queue.end(current);
					this.clearNext(current);
					continue;
				}
				if (current.mixingFrom && this.updateMixingFrom(current, delta)) {
					var from = current.mixingFrom;
					current.mixingFrom = null;
					if (from)
						from.mixingTo = null;
					while (from) {
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
			if (!from)
				return true;
			var finished = this.updateMixingFrom(from, delta);
			from.animationLast = from.nextAnimationLast;
			from.trackLast = from.nextTrackLast;
			if (to.mixTime > 0 && to.mixTime >= to.mixDuration) {
				if (from.totalAlpha == 0 || to.mixDuration == 0) {
					to.mixingFrom = from.mixingFrom;
					if (from.mixingFrom)
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
			if (!skeleton)
				throw new Error("skeleton cannot be null.");
			if (this.animationsChanged)
				this._animationsChanged();
			var events = this.events;
			var tracks = this.tracks;
			var applied = false;
			for (var i_16 = 0, n_1 = tracks.length; i_16 < n_1; i_16++) {
				var current = tracks[i_16];
				if (!current || current.delay > 0)
					continue;
				applied = true;
				var blend = i_16 == 0 ? spine.MixBlend.first : current.mixBlend;
				var mix = current.alpha;
				if (current.mixingFrom)
					mix *= this.applyMixingFrom(current, skeleton, blend);
				else if (current.trackTime >= current.trackEnd && !current.next)
					mix = 0;
				var animationLast = current.animationLast, animationTime = current.getAnimationTime(), applyTime = animationTime;
				var applyEvents = events;
				if (current.reverse) {
					applyTime = current.animation.duration - applyTime;
					applyEvents = null;
				}
				var timelines = current.animation.timelines;
				var timelineCount = timelines.length;
				if ((i_16 == 0 && mix == 1) || blend == spine.MixBlend.add) {
					for (var ii = 0; ii < timelineCount; ii++) {
						spine.Utils.webkit602BugfixHelper(mix, blend);
						var timeline = timelines[ii];
						if (timeline instanceof spine.AttachmentTimeline)
							this.applyAttachmentTimeline(timeline, skeleton, applyTime, blend, true);
						else
							timeline.apply(skeleton, animationLast, applyTime, applyEvents, mix, blend, spine.MixDirection.mixIn);
					}
				}
				else {
					var timelineMode = current.timelineMode;
					var firstFrame = current.timelinesRotation.length != timelineCount << 1;
					if (firstFrame)
						current.timelinesRotation.length = timelineCount << 1;
					for (var ii = 0; ii < timelineCount; ii++) {
						var timeline_1 = timelines[ii];
						var timelineBlend = timelineMode[ii] == SUBSEQUENT ? blend : spine.MixBlend.setup;
						if (timeline_1 instanceof spine.RotateTimeline) {
							this.applyRotateTimeline(timeline_1, skeleton, applyTime, mix, timelineBlend, current.timelinesRotation, ii << 1, firstFrame);
						}
						else if (timeline_1 instanceof spine.AttachmentTimeline) {
							this.applyAttachmentTimeline(timeline_1, skeleton, applyTime, blend, true);
						}
						else {
							spine.Utils.webkit602BugfixHelper(mix, blend);
							timeline_1.apply(skeleton, animationLast, applyTime, applyEvents, mix, timelineBlend, spine.MixDirection.mixIn);
						}
					}
				}
				this.queueEvents(current, animationTime);
				events.length = 0;
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}
			var setupState = this.unkeyedState + SETUP;
			var slots = skeleton.slots;
			for (var i = 0, n = skeleton.slots.length; i < n; i++) {
				var slot = slots[i];
				if (slot.attachmentState == setupState) {
					var attachmentName = slot.data.attachmentName;
					slot.setAttachment(!attachmentName ? null : skeleton.getAttachment(slot.data.index, attachmentName));
				}
			}
			this.unkeyedState += 2;
			this.queue.drain();
			return applied;
		};
		AnimationState.prototype.applyMixingFrom = function (to, skeleton, blend) {
			var from = to.mixingFrom;
			if (from.mixingFrom)
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
			var attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
			var timelines = from.animation.timelines;
			var timelineCount = timelines.length;
			var alphaHold = from.alpha * to.interruptAlpha, alphaMix = alphaHold * (1 - mix);
			var animationLast = from.animationLast, animationTime = from.getAnimationTime(), applyTime = animationTime;
			var events = null;
			if (from.reverse)
				applyTime = from.animation.duration - applyTime;
			else if (mix < from.eventThreshold)
				events = this.events;
			if (blend == spine.MixBlend.add) {
				for (var i = 0; i < timelineCount; i++)
					timelines[i].apply(skeleton, animationLast, applyTime, events, alphaMix, blend, spine.MixDirection.mixOut);
			}
			else {
				var timelineMode = from.timelineMode;
				var timelineHoldMix = from.timelineHoldMix;
				var firstFrame = from.timelinesRotation.length != timelineCount << 1;
				if (firstFrame)
					from.timelinesRotation.length = timelineCount << 1;
				from.totalAlpha = 0;
				for (var i = 0; i < timelineCount; i++) {
					var timeline = timelines[i];
					var direction = spine.MixDirection.mixOut;
					var timelineBlend = void 0;
					var alpha = 0;
					switch (timelineMode[i]) {
						case SUBSEQUENT:
							if (!drawOrder && timeline instanceof spine.DrawOrderTimeline)
								continue;
							timelineBlend = blend;
							alpha = alphaMix;
							break;
						case FIRST:
							timelineBlend = spine.MixBlend.setup;
							alpha = alphaMix;
							break;
						case HOLD_SUBSEQUENT:
							timelineBlend = blend;
							alpha = alphaHold;
							break;
						case HOLD_FIRST:
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
						this.applyRotateTimeline(timeline, skeleton, applyTime, alpha, timelineBlend, from.timelinesRotation, i << 1, firstFrame);
					else if (timeline instanceof spine.AttachmentTimeline)
						this.applyAttachmentTimeline(timeline, skeleton, applyTime, timelineBlend, attachments);
					else {
						spine.Utils.webkit602BugfixHelper(alpha, blend);
						if (drawOrder && timeline instanceof spine.DrawOrderTimeline && timelineBlend == spine.MixBlend.setup)
							direction = spine.MixDirection.mixIn;
						timeline.apply(skeleton, animationLast, applyTime, events, alpha, timelineBlend, direction);
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
			if (time < timeline.frames[0]) {
				if (blend == spine.MixBlend.setup || blend == spine.MixBlend.first)
					this.setAttachment(skeleton, slot, slot.data.attachmentName, attachments);
			}
			else
				this.setAttachment(skeleton, slot, timeline.attachmentNames[spine.Timeline.search1(timeline.frames, time)], attachments);
			if (slot.attachmentState <= this.unkeyedState)
				slot.attachmentState = this.unkeyedState + SETUP;
		};
		AnimationState.prototype.setAttachment = function (skeleton, slot, attachmentName, attachments) {
			slot.setAttachment(!attachmentName ? null : skeleton.getAttachment(slot.data.index, attachmentName));
			if (attachments)
				slot.attachmentState = this.unkeyedState + CURRENT;
		};
		AnimationState.prototype.applyRotateTimeline = function (timeline, skeleton, time, alpha, blend, timelinesRotation, i, firstFrame) {
			if (firstFrame)
				timelinesRotation[i] = 0;
			if (alpha == 1) {
				timeline.apply(skeleton, 0, time, null, 1, blend, spine.MixDirection.mixIn);
				return;
			}
			var bone = skeleton.bones[timeline.boneIndex];
			if (!bone.active)
				return;
			var frames = timeline.frames;
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
				r2 = bone.data.rotation + timeline.getCurveValue(time);
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
			bone.rotation = r1 + total * alpha;
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
				this.queue.event(entry, event_2);
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
			if (!current)
				return;
			this.queue.end(current);
			this.clearNext(current);
			var entry = current;
			while (true) {
				var from = entry.mixingFrom;
				if (!from)
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
			current.previous = null;
			if (from) {
				if (interrupt)
					this.queue.interrupt(from);
				current.mixingFrom = from;
				from.mixingTo = current;
				current.mixTime = 0;
				if (from.mixingFrom && from.mixDuration > 0)
					current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);
				from.timelinesRotation.length = 0;
			}
			this.queue.start(current);
		};
		AnimationState.prototype.setAnimation = function (trackIndex, animationName, loop) {
			if (loop === void 0) { loop = false; }
			var animation = this.data.skeletonData.findAnimation(animationName);
			if (!animation)
				throw new Error("Animation not found: " + animationName);
			return this.setAnimationWith(trackIndex, animation, loop);
		};
		AnimationState.prototype.setAnimationWith = function (trackIndex, animation, loop) {
			if (loop === void 0) { loop = false; }
			if (!animation)
				throw new Error("animation cannot be null.");
			var interrupt = true;
			var current = this.expandToIndex(trackIndex);
			if (current) {
				if (current.nextTrackLast == -1) {
					this.tracks[trackIndex] = current.mixingFrom;
					this.queue.interrupt(current);
					this.queue.end(current);
					this.clearNext(current);
					current = current.mixingFrom;
					interrupt = false;
				}
				else
					this.clearNext(current);
			}
			var entry = this.trackEntry(trackIndex, animation, loop, current);
			this.setCurrent(trackIndex, entry, interrupt);
			this.queue.drain();
			return entry;
		};
		AnimationState.prototype.addAnimation = function (trackIndex, animationName, loop, delay) {
			if (loop === void 0) { loop = false; }
			if (delay === void 0) { delay = 0; }
			var animation = this.data.skeletonData.findAnimation(animationName);
			if (!animation)
				throw new Error("Animation not found: " + animationName);
			return this.addAnimationWith(trackIndex, animation, loop, delay);
		};
		AnimationState.prototype.addAnimationWith = function (trackIndex, animation, loop, delay) {
			if (loop === void 0) { loop = false; }
			if (delay === void 0) { delay = 0; }
			if (!animation)
				throw new Error("animation cannot be null.");
			var last = this.expandToIndex(trackIndex);
			if (last) {
				while (last.next)
					last = last.next;
			}
			var entry = this.trackEntry(trackIndex, animation, loop, last);
			if (!last) {
				this.setCurrent(trackIndex, entry, true);
				this.queue.drain();
			}
			else {
				last.next = entry;
				entry.previous = last;
				if (delay <= 0)
					delay += last.getTrackComplete() - entry.mixDuration;
			}
			entry.delay = delay;
			return entry;
		};
		AnimationState.prototype.setEmptyAnimation = function (trackIndex, mixDuration) {
			if (mixDuration === void 0) { mixDuration = 0; }
			var entry = this.setAnimationWith(trackIndex, AnimationState.emptyAnimation(), false);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		};
		AnimationState.prototype.addEmptyAnimation = function (trackIndex, mixDuration, delay) {
			if (mixDuration === void 0) { mixDuration = 0; }
			if (delay === void 0) { delay = 0; }
			var entry = this.addAnimationWith(trackIndex, AnimationState.emptyAnimation(), false, delay <= 0 ? 1 : delay);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			if (delay <= 0 && entry.previous)
				entry.delay = entry.previous.getTrackComplete() - entry.mixDuration + delay;
			return entry;
		};
		AnimationState.prototype.setEmptyAnimations = function (mixDuration) {
			if (mixDuration === void 0) { mixDuration = 0; }
			var oldDrainDisabled = this.queue.drainDisabled;
			this.queue.drainDisabled = true;
			for (var i = 0, n = this.tracks.length; i < n; i++) {
				var current = this.tracks[i];
				if (current)
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
			entry.mixDuration = !last ? 0 : this.data.getMix(last.animation, animation);
			entry.mixBlend = spine.MixBlend.replace;
			return entry;
		};
		AnimationState.prototype.clearNext = function (entry) {
			var next = entry.next;
			while (next) {
				this.queue.dispose(next);
				next = next.next;
			}
			entry.next = null;
		};
		AnimationState.prototype._animationsChanged = function () {
			this.animationsChanged = false;
			this.propertyIDs.clear();
			var tracks = this.tracks;
			for (var i = 0, n = tracks.length; i < n; i++) {
				var entry = tracks[i];
				if (!entry)
					continue;
				while (entry.mixingFrom)
					entry = entry.mixingFrom;
				do {
					if (!entry.mixingTo || entry.mixBlend != spine.MixBlend.add)
						this.computeHold(entry);
					entry = entry.mixingTo;
				} while (entry);
			}
		};
		AnimationState.prototype.computeHold = function (entry) {
			var to = entry.mixingTo;
			var timelines = entry.animation.timelines;
			var timelinesCount = entry.animation.timelines.length;
			var timelineMode = entry.timelineMode;
			timelineMode.length = timelinesCount;
			var timelineHoldMix = entry.timelineHoldMix;
			timelineHoldMix.length = 0;
			var propertyIDs = this.propertyIDs;
			if (to && to.holdPrevious) {
				for (var i = 0; i < timelinesCount; i++)
					timelineMode[i] = propertyIDs.addAll(timelines[i].getPropertyIds()) ? HOLD_FIRST : HOLD_SUBSEQUENT;
				return;
			}
			outer: for (var i = 0; i < timelinesCount; i++) {
				var timeline = timelines[i];
				var ids = timeline.getPropertyIds();
				if (!propertyIDs.addAll(ids))
					timelineMode[i] = SUBSEQUENT;
				else if (!to || timeline instanceof spine.AttachmentTimeline || timeline instanceof spine.DrawOrderTimeline
					|| timeline instanceof spine.EventTimeline || !to.animation.hasTimeline(ids)) {
					timelineMode[i] = FIRST;
				}
				else {
					for (var next = to.mixingTo; next; next = next.mixingTo) {
						if (next.animation.hasTimeline(ids))
							continue;
						if (entry.mixDuration > 0) {
							timelineMode[i] = HOLD_MIX;
							timelineHoldMix[i] = next;
							continue outer;
						}
						break;
					}
					timelineMode[i] = HOLD_FIRST;
				}
			}
		};
		AnimationState.prototype.getCurrent = function (trackIndex) {
			if (trackIndex >= this.tracks.length)
				return null;
			return this.tracks[trackIndex];
		};
		AnimationState.prototype.addListener = function (listener) {
			if (!listener)
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
			this.previous = null;
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
		TrackEntry.prototype.getTrackComplete = function () {
			var duration = this.animationEnd - this.animationStart;
			if (duration != 0) {
				if (this.loop)
					return duration * (1 + ((this.trackTime / duration) | 0));
				if (this.trackTime < duration)
					return duration;
			}
			return this.trackTime;
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
						if (entry.listener && entry.listener.start)
							entry.listener.start(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].start)
								listeners[ii].start(entry);
						break;
					case EventType.interrupt:
						if (entry.listener && entry.listener.interrupt)
							entry.listener.interrupt(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].interrupt)
								listeners[ii].interrupt(entry);
						break;
					case EventType.end:
						if (entry.listener && entry.listener.end)
							entry.listener.end(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].end)
								listeners[ii].end(entry);
					case EventType.dispose:
						if (entry.listener && entry.listener.dispose)
							entry.listener.dispose(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].dispose)
								listeners[ii].dispose(entry);
						this.animState.trackEntryPool.free(entry);
						break;
					case EventType.complete:
						if (entry.listener && entry.listener.complete)
							entry.listener.complete(entry);
						for (var ii = 0; ii < listeners.length; ii++)
							if (listeners[ii].complete)
								listeners[ii].complete(entry);
						break;
					case EventType.event:
						var event_3 = objects[i++ + 2];
						if (entry.listener && entry.listener.event)
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
	var SUBSEQUENT = 0;
	var FIRST = 1;
	var HOLD_SUBSEQUENT = 2;
	var HOLD_FIRST = 3;
	var HOLD_MIX = 4;
	var SETUP = 1;
	var CURRENT = 2;
	var _emptyAnimation = null;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var AnimationStateData = (function () {
		function AnimationStateData(skeletonData) {
			this.animationToMixTime = {};
			this.defaultMix = 0;
			if (!skeletonData)
				throw new Error("skeletonData cannot be null.");
			this.skeletonData = skeletonData;
		}
		AnimationStateData.prototype.setMix = function (fromName, toName, duration) {
			var from = this.skeletonData.findAnimation(fromName);
			if (!from)
				throw new Error("Animation not found: " + fromName);
			var to = this.skeletonData.findAnimation(toName);
			if (!to)
				throw new Error("Animation not found: " + toName);
			this.setMixWith(from, to, duration);
		};
		AnimationStateData.prototype.setMixWith = function (from, to, duration) {
			if (!from)
				throw new Error("from cannot be null.");
			if (!to)
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
		function AssetManager(textureLoader, pathPrefix, downloader) {
			if (pathPrefix === void 0) { pathPrefix = ""; }
			if (downloader === void 0) { downloader = null; }
			this.assets = {};
			this.errors = {};
			this.toLoad = 0;
			this.loaded = 0;
			this.textureLoader = textureLoader;
			this.pathPrefix = pathPrefix;
			this.downloader = downloader || new Downloader();
		}
		AssetManager.prototype.start = function (path) {
			this.toLoad++;
			return this.pathPrefix + path;
		};
		AssetManager.prototype.success = function (callback, path, asset) {
			this.toLoad--;
			this.loaded++;
			this.assets[path] = asset;
			if (callback)
				callback(path, asset);
		};
		AssetManager.prototype.error = function (callback, path, message) {
			this.toLoad--;
			this.loaded++;
			this.errors[path] = message;
			if (callback)
				callback(path, message);
		};
		AssetManager.prototype.setRawDataURI = function (path, data) {
			this.downloader.rawDataUris[this.pathPrefix + path] = data;
		};
		AssetManager.prototype.loadBinary = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.start(path);
			this.downloader.downloadBinary(path, function (data) {
				_this.success(success, path, data);
			}, function (status, responseText) {
				_this.error(error, path, "Couldn't load binary " + path + ": status " + status + ", " + responseText);
			});
		};
		AssetManager.prototype.loadText = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.start(path);
			this.downloader.downloadText(path, function (data) {
				_this.success(success, path, data);
			}, function (status, responseText) {
				_this.error(error, path, "Couldn't load text " + path + ": status " + status + ", " + responseText);
			});
		};
		AssetManager.prototype.loadJson = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.start(path);
			this.downloader.downloadJson(path, function (data) {
				_this.success(success, path, data);
			}, function (status, responseText) {
				_this.error(error, path, "Couldn't load JSON " + path + ": status " + status + ", " + responseText);
			});
		};
		AssetManager.prototype.loadTexture = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			path = this.start(path);
			var isBrowser = !!(typeof window !== 'undefined' && typeof navigator !== 'undefined' && window.document);
			var isWebWorker = !isBrowser && typeof importScripts !== 'undefined';
			if (isWebWorker) {
				fetch(path, { mode: "cors" }).then(function (response) {
					if (response.ok)
						return response.blob();
					_this.error(error, path, "Couldn't load image: " + path);
					return null;
				}).then(function (blob) {
					return blob ? createImageBitmap(blob, { premultiplyAlpha: "none", colorSpaceConversion: "none" }) : null;
				}).then(function (bitmap) {
					if (bitmap)
						_this.success(success, path, _this.textureLoader(bitmap));
				});
			}
			else {
				var image_1 = new Image();
				image_1.crossOrigin = "anonymous";
				image_1.onload = function () {
					_this.success(success, path, _this.textureLoader(image_1));
				};
				image_1.onerror = function () {
					_this.error(error, path, "Couldn't load image: " + path);
				};
				if (this.downloader.rawDataUris[path])
					path = this.downloader.rawDataUris[path];
				image_1.src = path;
			}
		};
		AssetManager.prototype.loadTextureAtlas = function (path, success, error) {
			var _this = this;
			if (success === void 0) { success = null; }
			if (error === void 0) { error = null; }
			var index = path.lastIndexOf("/");
			var parent = index >= 0 ? path.substring(0, index + 1) : "";
			path = this.start(path);
			this.downloader.downloadText(path, function (atlasText) {
				try {
					var atlas_1 = new spine.TextureAtlas(atlasText);
					var toLoad_1 = atlas_1.pages.length, abort_1 = false;
					var _loop_1 = function (page) {
						_this.loadTexture(parent + page.name, function (imagePath, texture) {
							if (!abort_1) {
								page.setTexture(texture);
								if (--toLoad_1 == 0)
									_this.success(success, path, atlas_1);
							}
						}, function (imagePath, message) {
							if (!abort_1)
								_this.error(error, path, "Couldn't load texture atlas " + path + " page image: " + imagePath);
							abort_1 = true;
						});
					};
					for (var _i = 0, _a = atlas_1.pages; _i < _a.length; _i++) {
						var page = _a[_i];
						_loop_1(page);
					}
				}
				catch (e) {
					_this.error(error, path, "Couldn't parse texture atlas " + path + ": " + e.message);
				}
			}, function (status, responseText) {
				_this.error(error, path, "Couldn't load texture atlas " + path + ": status " + status + ", " + responseText);
			});
		};
		AssetManager.prototype.get = function (path) {
			return this.assets[this.pathPrefix + path];
		};
		AssetManager.prototype.require = function (path) {
			path = this.pathPrefix + path;
			var asset = this.assets[path];
			if (asset)
				return asset;
			var error = this.errors[path];
			throw Error("Asset not found: " + path + (error ? "\n" + error : ""));
		};
		AssetManager.prototype.remove = function (path) {
			path = this.pathPrefix + path;
			var asset = this.assets[path];
			if (asset.dispose)
				asset.dispose();
			delete this.assets[path];
			return asset;
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
	var Downloader = (function () {
		function Downloader() {
			this.callbacks = {};
			this.rawDataUris = {};
		}
		Downloader.prototype.downloadText = function (url, success, error) {
			var _this = this;
			if (this.rawDataUris[url])
				url = this.rawDataUris[url];
			if (this.start(url, success, error))
				return;
			var request = new XMLHttpRequest();
			request.overrideMimeType("text/html");
			request.open("GET", url, true);
			var done = function () {
				_this.finish(url, request.status, request.responseText);
			};
			request.onload = done;
			request.onerror = done;
			request.send();
		};
		Downloader.prototype.downloadJson = function (url, success, error) {
			this.downloadText(url, function (data) {
				success(JSON.parse(data));
			}, error);
		};
		Downloader.prototype.downloadBinary = function (url, success, error) {
			var _this = this;
			if (this.rawDataUris[url])
				url = this.rawDataUris[url];
			if (this.start(url, success, error))
				return;
			var request = new XMLHttpRequest();
			request.open("GET", url, true);
			request.responseType = "arraybuffer";
			var onerror = function () {
				_this.finish(url, request.status, request.responseText);
			};
			request.onload = function () {
				if (request.status == 200)
					_this.finish(url, 200, new Uint8Array(request.response));
				else
					onerror();
			};
			request.onerror = onerror;
			request.send();
		};
		Downloader.prototype.start = function (url, success, error) {
			var callbacks = this.callbacks[url];
			try {
				if (callbacks)
					return true;
				this.callbacks[url] = callbacks = [];
			}
			finally {
				callbacks.push(success, error);
			}
		};
		Downloader.prototype.finish = function (url, status, data) {
			var callbacks = this.callbacks[url];
			delete this.callbacks[url];
			var args = status == 200 ? [data] : [status, data];
			for (var i = args.length - 1, n = callbacks.length; i < n; i += 2)
				callbacks[i].apply(null, args);
		};
		return Downloader;
	}());
	spine.Downloader = Downloader;
})(spine || (spine = {}));
var spine;
(function (spine) {
	var AtlasAttachmentLoader = (function () {
		function AtlasAttachmentLoader(atlas) {
			this.atlas = atlas;
		}
		AtlasAttachmentLoader.prototype.newRegionAttachment = function (skin, name, path) {
			var region = this.atlas.findRegion(path);
			if (!region)
				throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			region.renderObject = region;
			var attachment = new spine.RegionAttachment(name);
			attachment.setRegion(region);
			return attachment;
		};
		AtlasAttachmentLoader.prototype.newMeshAttachment = function (skin, name, path) {
			var region = this.atlas.findRegion(path);
			if (!region)
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
			this.a = 0;
			this.b = 0;
			this.c = 0;
			this.d = 0;
			this.worldY = 0;
			this.worldX = 0;
			this.sorted = false;
			this.active = false;
			if (!data)
				throw new Error("data cannot be null.");
			if (!skeleton)
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
			this.updateWorldTransformWith(this.ax, this.ay, this.arotation, this.ascaleX, this.ascaleY, this.ashearX, this.ashearY);
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
			var parent = this.parent;
			if (!parent) {
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
			var parent = this.parent;
			if (!parent) {
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
			var invDet = 1 / (this.a * this.d - this.b * this.c);
			var x = world.x - this.worldX, y = world.y - this.worldY;
			world.x = x * this.d * invDet - y * this.b * invDet;
			world.y = y * this.a * invDet - x * this.c * invDet;
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
			if (!name)
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
			if (!data)
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
			if (!data)
				throw new Error("data cannot be null.");
			if (!skeleton)
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
		IkConstraint.prototype.update = function () {
			if (this.mix == 0)
				return;
			var target = this.target;
			var bones = this.bones;
			switch (bones.length) {
				case 1:
					this.apply1(bones[0], target.worldX, target.worldY, this.compress, this.stretch, this.data.uniform, this.mix);
					break;
				case 2:
					this.apply2(bones[0], bones[1], target.worldX, target.worldY, this.bendDirection, this.stretch, this.data.uniform, this.softness, this.mix);
					break;
			}
		};
		IkConstraint.prototype.apply1 = function (bone, targetX, targetY, compress, stretch, uniform, alpha) {
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
		IkConstraint.prototype.apply2 = function (parent, child, targetX, targetY, bendDir, stretch, uniform, softness, alpha) {
			var px = parent.ax, py = parent.ay, psx = parent.ascaleX, psy = parent.ascaleY, sx = psx, sy = psy, csx = child.ascaleX;
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
			if (!u || stretch) {
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
				softness *= psx * (csx + 1) * 0.5;
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
				if (cos < -1) {
					cos = -1;
					a2 = Math.PI * bendDir;
				}
				else if (cos > 1) {
					cos = 1;
					a2 = 0;
					if (stretch) {
						a = (Math.sqrt(dd) / (l1 + l2) - 1) * alpha + 1;
						sx *= a;
						if (uniform)
							sy *= a;
					}
				}
				else
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
					q = -(c1 + q) * 0.5;
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
				if (dd <= (minDist + maxDist) * 0.5) {
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
			parent.updateWorldTransformWith(px, py, rotation + a1 * alpha, sx, sy, 0, 0);
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
			this.mixRotate = 0;
			this.mixX = 0;
			this.mixY = 0;
			this.spaces = new Array();
			this.positions = new Array();
			this.world = new Array();
			this.curves = new Array();
			this.lengths = new Array();
			this.segments = new Array();
			this.active = false;
			if (!data)
				throw new Error("data cannot be null.");
			if (!skeleton)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.bones = new Array();
			for (var i = 0, n = data.bones.length; i < n; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findSlot(data.target.name);
			this.position = data.position;
			this.spacing = data.spacing;
			this.mixRotate = data.mixRotate;
			this.mixX = data.mixX;
			this.mixY = data.mixY;
		}
		PathConstraint.prototype.isActive = function () {
			return this.active;
		};
		PathConstraint.prototype.update = function () {
			var attachment = this.target.getAttachment();
			if (!(attachment instanceof spine.PathAttachment))
				return;
			var mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY;
			if (mixRotate == 0 && mixX == 0 && mixY == 0)
				return;
			var data = this.data;
			var tangents = data.rotateMode == spine.RotateMode.Tangent, scale = data.rotateMode == spine.RotateMode.ChainScale;
			var bones = this.bones;
			var boneCount = bones.length, spacesCount = tangents ? boneCount : boneCount + 1;
			var spaces = spine.Utils.setArraySize(this.spaces, spacesCount), lengths = scale ? this.lengths = spine.Utils.setArraySize(this.lengths, boneCount) : null;
			var spacing = this.spacing;
			switch (data.spacingMode) {
				case spine.SpacingMode.Percent:
					if (scale) {
						for (var i = 0, n = spacesCount - 1; i < n; i++) {
							var bone = bones[i];
							var setupLength = bone.data.length;
							if (setupLength < PathConstraint.epsilon)
								lengths[i] = 0;
							else {
								var x = setupLength * bone.a, y = setupLength * bone.c;
								lengths[i] = Math.sqrt(x * x + y * y);
							}
						}
					}
					spine.Utils.arrayFill(spaces, 1, spacesCount, spacing);
					break;
				case spine.SpacingMode.Proportional:
					var sum = 0;
					for (var i = 0; i < boneCount;) {
						var bone = bones[i];
						var setupLength = bone.data.length;
						if (setupLength < PathConstraint.epsilon) {
							if (scale)
								lengths[i] = 0;
							spaces[++i] = spacing;
						}
						else {
							var x = setupLength * bone.a, y = setupLength * bone.c;
							var length_1 = Math.sqrt(x * x + y * y);
							if (scale)
								lengths[i] = length_1;
							spaces[++i] = length_1;
							sum += length_1;
						}
					}
					if (sum > 0) {
						sum = spacesCount / sum * spacing;
						for (var i = 1; i < spacesCount; i++)
							spaces[i] *= sum;
					}
					break;
				default:
					var lengthSpacing = data.spacingMode == spine.SpacingMode.Length;
					for (var i = 0, n = spacesCount - 1; i < n;) {
						var bone = bones[i];
						var setupLength = bone.data.length;
						if (setupLength < PathConstraint.epsilon) {
							if (scale)
								lengths[i] = 0;
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
			var positions = this.computeWorldPositions(attachment, spacesCount, tangents);
			var boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
			var tip = false;
			if (offsetRotation == 0)
				tip = data.rotateMode == spine.RotateMode.Chain;
			else {
				tip = false;
				var p = this.target.bone;
				offsetRotation *= p.a * p.d - p.b * p.c > 0 ? spine.MathUtils.degRad : -spine.MathUtils.degRad;
			}
			for (var i = 0, p = 3; i < boneCount; i++, p += 3) {
				var bone = bones[i];
				bone.worldX += (boneX - bone.worldX) * mixX;
				bone.worldY += (boneY - bone.worldY) * mixY;
				var x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
				if (scale) {
					var length_3 = lengths[i];
					if (length_3 != 0) {
						var s = (Math.sqrt(dx * dx + dy * dy) / length_3 - 1) * mixRotate + 1;
						bone.a *= s;
						bone.c *= s;
					}
				}
				boneX = x;
				boneY = y;
				if (mixRotate > 0) {
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
						boneX += (length_4 * (cos * a - sin * c) - dx) * mixRotate;
						boneY += (length_4 * (sin * a + cos * c) - dy) * mixRotate;
					}
					else {
						r += offsetRotation;
					}
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r *= mixRotate;
					cos = Math.cos(r);
					sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
				bone.updateAppliedTransform();
			}
		};
		PathConstraint.prototype.computeWorldPositions = function (path, spacesCount, tangents) {
			var target = this.target;
			var position = this.position;
			var spaces = this.spaces, out = spine.Utils.setArraySize(this.positions, spacesCount * 3 + 2), world = null;
			var closed = path.closed;
			var verticesLength = path.worldVerticesLength, curveCount = verticesLength / 6, prevCurve = PathConstraint.NONE;
			if (!path.constantSpeed) {
				var lengths = path.lengths;
				curveCount -= closed ? 1 : 2;
				var pathLength_1 = lengths[curveCount];
				if (this.data.positionMode == spine.PositionMode.Percent)
					position *= pathLength_1;
				var multiplier_1;
				switch (this.data.spacingMode) {
					case spine.SpacingMode.Percent:
						multiplier_1 = pathLength_1;
						break;
					case spine.SpacingMode.Proportional:
						multiplier_1 = pathLength_1 / spacesCount;
						break;
					default:
						multiplier_1 = 1;
				}
				world = spine.Utils.setArraySize(this.world, 8);
				for (var i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
					var space = spaces[i] * multiplier_1;
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
			if (this.data.positionMode == spine.PositionMode.Percent)
				position *= pathLength;
			var multiplier;
			switch (this.data.spacingMode) {
				case spine.SpacingMode.Percent:
					multiplier = pathLength;
					break;
				case spine.SpacingMode.Proportional:
					multiplier = pathLength / spacesCount;
					break;
				default:
					multiplier = 1;
			}
			var segments = this.segments;
			var curveLength = 0;
			for (var i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
				var space = spaces[i] * multiplier;
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
			_this.mixRotate = 0;
			_this.mixX = 0;
			_this.mixY = 0;
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
		SpacingMode[SpacingMode["Proportional"] = 3] = "Proportional";
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
	var Skeleton = (function () {
		function Skeleton(data) {
			this._updateCache = new Array();
			this.time = 0;
			this.scaleX = 1;
			this.scaleY = 1;
			this.x = 0;
			this.y = 0;
			if (!data)
				throw new Error("data cannot be null.");
			this.data = data;
			this.bones = new Array();
			for (var i = 0; i < data.bones.length; i++) {
				var boneData = data.bones[i];
				var bone = void 0;
				if (!boneData.parent)
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
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				bone.sorted = bone.data.skinRequired;
				bone.active = !bone.sorted;
			}
			if (this.skin) {
				var skinBones = this.skin.bones;
				for (var i = 0, n = this.skin.bones.length; i < n; i++) {
					var bone = this.bones[skinBones[i].index];
					do {
						bone.sorted = false;
						bone.active = true;
						bone = bone.parent;
					} while (bone);
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
			constraint.active = constraint.target.isActive() && (!constraint.data.skinRequired || (this.skin && spine.Utils.contains(this.skin.constraints, constraint.data, true)));
			if (!constraint.active)
				return;
			var target = constraint.target;
			this.sortBone(target);
			var constrained = constraint.bones;
			var parent = constrained[0];
			this.sortBone(parent);
			if (constrained.length == 1) {
				this._updateCache.push(constraint);
				this.sortReset(parent.children);
			}
			else {
				var child = constrained[constrained.length - 1];
				this.sortBone(child);
				this._updateCache.push(constraint);
				this.sortReset(parent.children);
				child.sorted = true;
			}
		};
		Skeleton.prototype.sortPathConstraint = function (constraint) {
			constraint.active = constraint.target.bone.isActive() && (!constraint.data.skinRequired || (this.skin && spine.Utils.contains(this.skin.constraints, constraint.data, true)));
			if (!constraint.active)
				return;
			var slot = constraint.target;
			var slotIndex = slot.data.index;
			var slotBone = slot.bone;
			if (this.skin)
				this.sortPathConstraintAttachment(this.skin, slotIndex, slotBone);
			if (this.data.defaultSkin && this.data.defaultSkin != this.skin)
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
			constraint.active = constraint.target.isActive() && (!constraint.data.skinRequired || (this.skin && spine.Utils.contains(this.skin.constraints, constraint.data, true)));
			if (!constraint.active)
				return;
			this.sortBone(constraint.target);
			var constrained = constraint.bones;
			var boneCount = constrained.length;
			if (constraint.data.local) {
				for (var i = 0; i < boneCount; i++) {
					var child = constrained[i];
					this.sortBone(child.parent);
					this.sortBone(child);
				}
			}
			else {
				for (var i = 0; i < boneCount; i++) {
					this.sortBone(constrained[i]);
				}
			}
			this._updateCache.push(constraint);
			for (var i = 0; i < boneCount; i++)
				this.sortReset(constrained[i].children);
			for (var i = 0; i < boneCount; i++)
				constrained[i].sorted = true;
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
			if (!pathBones)
				this.sortBone(slotBone);
			else {
				var bones = this.bones;
				for (var i = 0, n = pathBones.length; i < n;) {
					var nn = pathBones[i++];
					nn += i;
					while (i < nn)
						this.sortBone(bones[pathBones[i++]]);
				}
			}
		};
		Skeleton.prototype.sortBone = function (bone) {
			if (bone.sorted)
				return;
			var parent = bone.parent;
			if (parent)
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
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				bone.ax = bone.x;
				bone.ay = bone.y;
				bone.arotation = bone.rotation;
				bone.ascaleX = bone.scaleX;
				bone.ascaleY = bone.scaleY;
				bone.ashearX = bone.shearX;
				bone.ashearY = bone.shearY;
			}
			var updateCache = this._updateCache;
			for (var i = 0, n = updateCache.length; i < n; i++)
				updateCache[i].update();
		};
		Skeleton.prototype.updateWorldTransformWith = function (parent) {
			var rootBone = this.getRootBone();
			var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			rootBone.worldX = pa * this.x + pb * this.y + parent.worldX;
			rootBone.worldY = pc * this.x + pd * this.y + parent.worldY;
			var rotationY = rootBone.rotation + 90 + rootBone.shearY;
			var la = spine.MathUtils.cosDeg(rootBone.rotation + rootBone.shearX) * rootBone.scaleX;
			var lb = spine.MathUtils.cosDeg(rotationY) * rootBone.scaleY;
			var lc = spine.MathUtils.sinDeg(rootBone.rotation + rootBone.shearX) * rootBone.scaleX;
			var ld = spine.MathUtils.sinDeg(rotationY) * rootBone.scaleY;
			rootBone.a = (pa * la + pb * lc) * this.scaleX;
			rootBone.b = (pa * lb + pb * ld) * this.scaleX;
			rootBone.c = (pc * la + pd * lc) * this.scaleY;
			rootBone.d = (pc * lb + pd * ld) * this.scaleY;
			var updateCache = this._updateCache;
			for (var i = 0, n = updateCache.length; i < n; i++) {
				var updatable = updateCache[i];
				if (updatable != rootBone)
					updatable.update();
			}
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
				constraint.mixRotate = data.mixRotate;
				constraint.mixX = data.mixX;
				constraint.mixY = data.mixY;
				constraint.mixScaleX = data.mixScaleX;
				constraint.mixScaleY = data.mixScaleY;
				constraint.mixShearY = data.mixShearY;
			}
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++) {
				var constraint = pathConstraints[i];
				var data = constraint.data;
				constraint.position = data.position;
				constraint.spacing = data.spacing;
				constraint.mixRotate = data.mixRotate;
				constraint.mixX = data.mixX;
				constraint.mixY = data.mixY;
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
			if (!boneName)
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
			if (!boneName)
				throw new Error("boneName cannot be null.");
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++)
				if (bones[i].data.name == boneName)
					return i;
			return -1;
		};
		Skeleton.prototype.findSlot = function (slotName) {
			if (!slotName)
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
			if (!slotName)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++)
				if (slots[i].data.name == slotName)
					return i;
			return -1;
		};
		Skeleton.prototype.setSkinByName = function (skinName) {
			var skin = this.data.findSkin(skinName);
			if (!skin)
				throw new Error("Skin not found: " + skinName);
			this.setSkin(skin);
		};
		Skeleton.prototype.setSkin = function (newSkin) {
			if (newSkin == this.skin)
				return;
			if (newSkin) {
				if (this.skin)
					newSkin.attachAll(this, this.skin);
				else {
					var slots = this.slots;
					for (var i = 0, n = slots.length; i < n; i++) {
						var slot = slots[i];
						var name_1 = slot.data.attachmentName;
						if (name_1) {
							var attachment = newSkin.getAttachment(i, name_1);
							if (attachment)
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
			if (!attachmentName)
				throw new Error("attachmentName cannot be null.");
			if (this.skin) {
				var attachment = this.skin.getAttachment(slotIndex, attachmentName);
				if (attachment)
					return attachment;
			}
			if (this.data.defaultSkin)
				return this.data.defaultSkin.getAttachment(slotIndex, attachmentName);
			return null;
		};
		Skeleton.prototype.setAttachment = function (slotName, attachmentName) {
			if (!slotName)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++) {
				var slot = slots[i];
				if (slot.data.name == slotName) {
					var attachment = null;
					if (attachmentName) {
						attachment = this.getAttachment(i, attachmentName);
						if (!attachment)
							throw new Error("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.setAttachment(attachment);
					return;
				}
			}
			throw new Error("Slot not found: " + slotName);
		};
		Skeleton.prototype.findIkConstraint = function (constraintName) {
			if (!constraintName)
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
			if (!constraintName)
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
			if (!constraintName)
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
			if (!offset)
				throw new Error("offset cannot be null.");
			if (!size)
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
				if (vertices) {
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
			var lowHash = input.readInt32();
			var highHash = input.readInt32();
			skeletonData.hash = highHash == 0 && lowHash == 0 ? null : highHash.toString(16) + lowHash.toString(16);
			skeletonData.version = input.readString();
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
				data.transformMode = input.readInt(true);
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
				data.blendMode = input.readInt(true);
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
				data.mixRotate = input.readFloat();
				data.mixX = input.readFloat();
				data.mixY = input.readFloat();
				data.mixScaleX = input.readFloat();
				data.mixScaleY = input.readFloat();
				data.mixShearY = input.readFloat();
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
				data.positionMode = input.readInt(true);
				data.spacingMode = input.readInt(true);
				data.rotateMode = input.readInt(true);
				data.offsetRotation = input.readFloat();
				data.position = input.readFloat();
				if (data.positionMode == spine.PositionMode.Fixed)
					data.position *= scale;
				data.spacing = input.readFloat();
				if (data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed)
					data.spacing *= scale;
				data.mixRotate = input.readFloat();
				data.mixX = input.readFloat();
				data.mixY = input.readFloat();
				skeletonData.pathConstraints.push(data);
			}
			var defaultSkin = this.readSkin(input, skeletonData, true, nonessential);
			if (defaultSkin) {
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
				var skin = !linkedMesh.skin ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				var parent_3 = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
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
				if (data.audioPath) {
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
					if (attachment)
						skin.setAttachment(slotIndex, name_3, attachment);
				}
			}
			return skin;
		};
		SkeletonBinary.prototype.readAttachment = function (input, skeletonData, skin, slotIndex, attachmentName, nonessential) {
			var scale = this.scale;
			var name = input.readStringRef();
			if (!name)
				name = attachmentName;
			switch (input.readByte()) {
				case AttachmentType.Region: {
					var path = input.readStringRef();
					var rotation = input.readFloat();
					var x = input.readFloat();
					var y = input.readFloat();
					var scaleX = input.readFloat();
					var scaleY = input.readFloat();
					var width = input.readFloat();
					var height = input.readFloat();
					var color = input.readInt32();
					if (!path)
						path = name;
					var region = this.attachmentLoader.newRegionAttachment(skin, name, path);
					if (!region)
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
				case AttachmentType.BoundingBox: {
					var vertexCount = input.readInt(true);
					var vertices = this.readVertices(input, vertexCount);
					var color = nonessential ? input.readInt32() : 0;
					var box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
					if (!box)
						return null;
					box.worldVerticesLength = vertexCount << 1;
					box.vertices = vertices.vertices;
					box.bones = vertices.bones;
					if (nonessential)
						spine.Color.rgba8888ToColor(box.color, color);
					return box;
				}
				case AttachmentType.Mesh: {
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
					if (!path)
						path = name;
					var mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (!mesh)
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
				case AttachmentType.LinkedMesh: {
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
					if (!path)
						path = name;
					var mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (!mesh)
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
				case AttachmentType.Path: {
					var closed_1 = input.readBoolean();
					var constantSpeed = input.readBoolean();
					var vertexCount = input.readInt(true);
					var vertices = this.readVertices(input, vertexCount);
					var lengths = spine.Utils.newArray(vertexCount / 3, 0);
					for (var i = 0, n = lengths.length; i < n; i++)
						lengths[i] = input.readFloat() * scale;
					var color = nonessential ? input.readInt32() : 0;
					var path = this.attachmentLoader.newPathAttachment(skin, name);
					if (!path)
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
				case AttachmentType.Point: {
					var rotation = input.readFloat();
					var x = input.readFloat();
					var y = input.readFloat();
					var color = nonessential ? input.readInt32() : 0;
					var point = this.attachmentLoader.newPointAttachment(skin, name);
					if (!point)
						return null;
					point.x = x * scale;
					point.y = y * scale;
					point.rotation = rotation;
					if (nonessential)
						spine.Color.rgba8888ToColor(point.color, color);
					return point;
				}
				case AttachmentType.Clipping: {
					var endSlotIndex = input.readInt(true);
					var vertexCount = input.readInt(true);
					var vertices = this.readVertices(input, vertexCount);
					var color = nonessential ? input.readInt32() : 0;
					var clip = this.attachmentLoader.newClippingAttachment(skin, name);
					if (!clip)
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
			var scale = this.scale;
			var verticesLength = vertexCount << 1;
			var vertices = new Vertices();
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
			input.readInt(true);
			var timelines = new Array();
			var scale = this.scale;
			var tempColor1 = new spine.Color();
			var tempColor2 = new spine.Color();
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var slotIndex = input.readInt(true);
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var timelineType = input.readByte();
					var frameCount = input.readInt(true);
					var frameLast = frameCount - 1;
					switch (timelineType) {
						case SLOT_ATTACHMENT: {
							var timeline = new spine.AttachmentTimeline(frameCount, slotIndex);
							for (var frame = 0; frame < frameCount; frame++)
								timeline.setFrame(frame, input.readFloat(), input.readStringRef());
							timelines.push(timeline);
							break;
						}
						case SLOT_RGBA: {
							var bezierCount = input.readInt(true);
							var timeline = new spine.RGBATimeline(frameCount, bezierCount, slotIndex);
							var time = input.readFloat();
							var r = input.readUnsignedByte() / 255.0;
							var g = input.readUnsignedByte() / 255.0;
							var b = input.readUnsignedByte() / 255.0;
							var a = input.readUnsignedByte() / 255.0;
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, r, g, b, a);
								if (frame == frameLast)
									break;
								var time2 = input.readFloat();
								var r2 = input.readUnsignedByte() / 255.0;
								var g2 = input.readUnsignedByte() / 255.0;
								var b2 = input.readUnsignedByte() / 255.0;
								var a2 = input.readUnsignedByte() / 255.0;
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
										setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
										setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
										setBezier(input, timeline, bezier++, frame, 3, time, time2, a, a2, 1);
								}
								time = time2;
								r = r2;
								g = g2;
								b = b2;
								a = a2;
							}
							timelines.push(timeline);
							break;
						}
						case SLOT_RGB: {
							var bezierCount = input.readInt(true);
							var timeline = new spine.RGBTimeline(frameCount, bezierCount, slotIndex);
							var time = input.readFloat();
							var r = input.readUnsignedByte() / 255.0;
							var g = input.readUnsignedByte() / 255.0;
							var b = input.readUnsignedByte() / 255.0;
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, r, g, b);
								if (frame == frameLast)
									break;
								var time2 = input.readFloat();
								var r2 = input.readUnsignedByte() / 255.0;
								var g2 = input.readUnsignedByte() / 255.0;
								var b2 = input.readUnsignedByte() / 255.0;
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
										setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
										setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								}
								time = time2;
								r = r2;
								g = g2;
								b = b2;
							}
							timelines.push(timeline);
							break;
						}
						case SLOT_RGBA2: {
							var bezierCount = input.readInt(true);
							var timeline = new spine.RGBA2Timeline(frameCount, bezierCount, slotIndex);
							var time = input.readFloat();
							var r = input.readUnsignedByte() / 255.0;
							var g = input.readUnsignedByte() / 255.0;
							var b = input.readUnsignedByte() / 255.0;
							var a = input.readUnsignedByte() / 255.0;
							var r2 = input.readUnsignedByte() / 255.0;
							var g2 = input.readUnsignedByte() / 255.0;
							var b2 = input.readUnsignedByte() / 255.0;
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, r, g, b, a, r2, g2, b2);
								if (frame == frameLast)
									break;
								var time2 = input.readFloat();
								var nr = input.readUnsignedByte() / 255.0;
								var ng = input.readUnsignedByte() / 255.0;
								var nb = input.readUnsignedByte() / 255.0;
								var na = input.readUnsignedByte() / 255.0;
								var nr2 = input.readUnsignedByte() / 255.0;
								var ng2 = input.readUnsignedByte() / 255.0;
								var nb2 = input.readUnsignedByte() / 255.0;
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
										setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
										setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
										setBezier(input, timeline, bezier++, frame, 3, time, time2, a, na, 1);
										setBezier(input, timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
										setBezier(input, timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
										setBezier(input, timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
								}
								time = time2;
								r = nr;
								g = ng;
								b = nb;
								a = na;
								r2 = nr2;
								g2 = ng2;
								b2 = nb2;
							}
							timelines.push(timeline);
							break;
						}
						case SLOT_RGB2: {
							var bezierCount = input.readInt(true);
							var timeline = new spine.RGB2Timeline(frameCount, bezierCount, slotIndex);
							var time = input.readFloat();
							var r = input.readUnsignedByte() / 255.0;
							var g = input.readUnsignedByte() / 255.0;
							var b = input.readUnsignedByte() / 255.0;
							var r2 = input.readUnsignedByte() / 255.0;
							var g2 = input.readUnsignedByte() / 255.0;
							var b2 = input.readUnsignedByte() / 255.0;
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
								if (frame == frameLast)
									break;
								var time2 = input.readFloat();
								var nr = input.readUnsignedByte() / 255.0;
								var ng = input.readUnsignedByte() / 255.0;
								var nb = input.readUnsignedByte() / 255.0;
								var nr2 = input.readUnsignedByte() / 255.0;
								var ng2 = input.readUnsignedByte() / 255.0;
								var nb2 = input.readUnsignedByte() / 255.0;
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
										setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
										setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
										setBezier(input, timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
										setBezier(input, timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
										setBezier(input, timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
								}
								time = time2;
								r = nr;
								g = ng;
								b = nb;
								r2 = nr2;
								g2 = ng2;
								b2 = nb2;
							}
							timelines.push(timeline);
							break;
						}
						case SLOT_ALPHA: {
							var timeline = new spine.AlphaTimeline(frameCount, input.readInt(true), slotIndex);
							var time = input.readFloat(), a = input.readUnsignedByte() / 255;
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, a);
								if (frame == frameLast)
									break;
								var time2 = input.readFloat();
								var a2 = input.readUnsignedByte() / 255;
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, a, a2, 1);
								}
								time = time2;
								a = a2;
							}
							timelines.push(timeline);
							break;
						}
					}
				}
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var boneIndex = input.readInt(true);
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
					switch (type) {
						case BONE_ROTATE:
							timelines.push(readTimeline1(input, new spine.RotateTimeline(frameCount, bezierCount, boneIndex), 1));
							break;
						case BONE_TRANSLATE:
							timelines.push(readTimeline2(input, new spine.TranslateTimeline(frameCount, bezierCount, boneIndex), scale));
							break;
						case BONE_TRANSLATEX:
							timelines.push(readTimeline1(input, new spine.TranslateXTimeline(frameCount, bezierCount, boneIndex), scale));
							break;
						case BONE_TRANSLATEY:
							timelines.push(readTimeline1(input, new spine.TranslateYTimeline(frameCount, bezierCount, boneIndex), scale));
							break;
						case BONE_SCALE:
							timelines.push(readTimeline2(input, new spine.ScaleTimeline(frameCount, bezierCount, boneIndex), 1));
							break;
						case BONE_SCALEX:
							timelines.push(readTimeline1(input, new spine.ScaleXTimeline(frameCount, bezierCount, boneIndex), 1));
							break;
						case BONE_SCALEY:
							timelines.push(readTimeline1(input, new spine.ScaleYTimeline(frameCount, bezierCount, boneIndex), 1));
							break;
						case BONE_SHEAR:
							timelines.push(readTimeline2(input, new spine.ShearTimeline(frameCount, bezierCount, boneIndex), 1));
							break;
						case BONE_SHEARX:
							timelines.push(readTimeline1(input, new spine.ShearXTimeline(frameCount, bezierCount, boneIndex), 1));
							break;
						case BONE_SHEARY:
							timelines.push(readTimeline1(input, new spine.ShearYTimeline(frameCount, bezierCount, boneIndex), 1));
					}
				}
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
				var timeline = new spine.IkConstraintTimeline(frameCount, input.readInt(true), index);
				var time = input.readFloat(), mix = input.readFloat(), softness = input.readFloat() * scale;
				for (var frame = 0, bezier = 0;; frame++) {
					timeline.setFrame(frame, time, mix, softness, input.readByte(), input.readBoolean(), input.readBoolean());
					if (frame == frameLast)
						break;
					var time2 = input.readFloat(), mix2 = input.readFloat(), softness2 = input.readFloat() * scale;
					switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
					}
					time = time2;
					mix = mix2;
					softness = softness2;
				}
				timelines.push(timeline);
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
				var timeline = new spine.TransformConstraintTimeline(frameCount, input.readInt(true), index);
				var time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat(), mixScaleX = input.readFloat(), mixScaleY = input.readFloat(), mixShearY = input.readFloat();
				for (var frame = 0, bezier = 0;; frame++) {
					timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
					if (frame == frameLast)
						break;
					var time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat(), mixScaleX2 = input.readFloat(), mixScaleY2 = input.readFloat(), mixShearY2 = input.readFloat();
					switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
							setBezier(input, timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
							setBezier(input, timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
					}
					time = time2;
					mixRotate = mixRotate2;
					mixX = mixX2;
					mixY = mixY2;
					mixScaleX = mixScaleX2;
					mixScaleY = mixScaleY2;
					mixShearY = mixShearY2;
				}
				timelines.push(timeline);
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var index = input.readInt(true);
				var data = skeletonData.pathConstraints[index];
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					switch (input.readByte()) {
						case PATH_POSITION:
							timelines
								.push(readTimeline1(input, new spine.PathConstraintPositionTimeline(input.readInt(true), input.readInt(true), index), data.positionMode == spine.PositionMode.Fixed ? scale : 1));
							break;
						case PATH_SPACING:
							timelines
								.push(readTimeline1(input, new spine.PathConstraintSpacingTimeline(input.readInt(true), input.readInt(true), index), data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed ? scale : 1));
							break;
						case PATH_MIX:
							var timeline = new spine.PathConstraintMixTimeline(input.readInt(true), input.readInt(true), index);
							var time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat();
							for (var frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
								timeline.setFrame(frame, time, mixRotate, mixX, mixY);
								if (frame == frameLast)
									break;
								var time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat();
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
										setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
										setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
								}
								time = time2;
								mixRotate = mixRotate2;
								mixX = mixX2;
								mixY = mixY2;
							}
							timelines.push(timeline);
					}
				}
			}
			for (var i = 0, n = input.readInt(true); i < n; i++) {
				var skin = skeletonData.skins[input.readInt(true)];
				for (var ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var slotIndex = input.readInt(true);
					for (var iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
						var attachmentName = input.readStringRef();
						var attachment = skin.getAttachment(slotIndex, attachmentName);
						var weighted = attachment.bones;
						var vertices = attachment.vertices;
						var deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;
						var frameCount = input.readInt(true);
						var frameLast = frameCount - 1;
						var bezierCount = input.readInt(true);
						var timeline = new spine.DeformTimeline(frameCount, bezierCount, slotIndex, attachment);
						var time = input.readFloat();
						for (var frame = 0, bezier = 0;; frame++) {
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
							timeline.setFrame(frame, time, deform);
							if (frame == frameLast)
								break;
							var time2 = input.readFloat();
							switch (input.readByte()) {
								case CURVE_STEPPED:
									timeline.setStepped(frame);
									break;
								case CURVE_BEZIER:
									setBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
							}
							time = time2;
						}
						timelines.push(timeline);
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
					if (event_4.data.audioPath) {
						event_4.volume = input.readFloat();
						event_4.balance = input.readFloat();
					}
					timeline.setFrame(i, event_4);
				}
				timelines.push(timeline);
			}
			var duration = 0;
			for (var i = 0, n = timelines.length; i < n; i++)
				duration = Math.max(duration, timelines[i].getDuration());
			return new spine.Animation(name, timelines, duration);
		};
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
		BinaryInput.prototype.readUnsignedByte = function () {
			return this.buffer.getUint8(this.index++);
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
	var AttachmentType;
	(function (AttachmentType) {
		AttachmentType[AttachmentType["Region"] = 0] = "Region";
		AttachmentType[AttachmentType["BoundingBox"] = 1] = "BoundingBox";
		AttachmentType[AttachmentType["Mesh"] = 2] = "Mesh";
		AttachmentType[AttachmentType["LinkedMesh"] = 3] = "LinkedMesh";
		AttachmentType[AttachmentType["Path"] = 4] = "Path";
		AttachmentType[AttachmentType["Point"] = 5] = "Point";
		AttachmentType[AttachmentType["Clipping"] = 6] = "Clipping";
	})(AttachmentType || (AttachmentType = {}));
	function readTimeline1(input, timeline, scale) {
		var time = input.readFloat(), value = input.readFloat() * scale;
		for (var frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
			timeline.setFrame(frame, time, value);
			if (frame == frameLast)
				break;
			var time2 = input.readFloat(), value2 = input.readFloat() * scale;
			switch (input.readByte()) {
				case CURVE_STEPPED:
					timeline.setStepped(frame);
					break;
				case CURVE_BEZIER:
					setBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, 1);
			}
			time = time2;
			value = value2;
		}
		return timeline;
	}
	function readTimeline2(input, timeline, scale) {
		var time = input.readFloat(), value1 = input.readFloat() * scale, value2 = input.readFloat() * scale;
		for (var frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
			timeline.setFrame(frame, time, value1, value2);
			if (frame == frameLast)
				break;
			var time2 = input.readFloat(), nvalue1 = input.readFloat() * scale, nvalue2 = input.readFloat() * scale;
			switch (input.readByte()) {
				case CURVE_STEPPED:
					timeline.setStepped(frame);
					break;
				case CURVE_BEZIER:
					setBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
					setBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
		}
		return timeline;
	}
	function setBezier(input, timeline, bezier, frame, value, time1, time2, value1, value2, scale) {
		timeline.setBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(), input.readFloat() * scale, time2, value2);
	}
	var BONE_ROTATE = 0;
	var BONE_TRANSLATE = 1;
	var BONE_TRANSLATEX = 2;
	var BONE_TRANSLATEY = 3;
	var BONE_SCALE = 4;
	var BONE_SCALEX = 5;
	var BONE_SCALEY = 6;
	var BONE_SHEAR = 7;
	var BONE_SHEARX = 8;
	var BONE_SHEARY = 9;
	var SLOT_ATTACHMENT = 0;
	var SLOT_RGBA = 1;
	var SLOT_RGB = 2;
	var SLOT_RGBA2 = 3;
	var SLOT_RGB2 = 4;
	var SLOT_ALPHA = 5;
	var PATH_POSITION = 0;
	var PATH_SPACING = 1;
	var PATH_MIX = 2;
	var CURVE_LINEAR = 0;
	var CURVE_STEPPED = 1;
	var CURVE_BEZIER = 2;
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
			if (!skeleton)
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
			if (!boundingBox)
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
			if (this.clipAttachment)
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
			if (this.clipAttachment && this.clipAttachment.endSlot == slot.data)
				this.clipEnd();
		};
		SkeletonClipping.prototype.clipEnd = function () {
			if (!this.clipAttachment)
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
			if (!boneName)
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
			if (!boneName)
				throw new Error("boneName cannot be null.");
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++)
				if (bones[i].name == boneName)
					return i;
			return -1;
		};
		SkeletonData.prototype.findSlot = function (slotName) {
			if (!slotName)
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
			if (!slotName)
				throw new Error("slotName cannot be null.");
			var slots = this.slots;
			for (var i = 0, n = slots.length; i < n; i++)
				if (slots[i].name == slotName)
					return i;
			return -1;
		};
		SkeletonData.prototype.findSkin = function (skinName) {
			if (!skinName)
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
			if (!eventDataName)
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
			if (!animationName)
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
			if (!constraintName)
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
			if (!constraintName)
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
			if (!constraintName)
				throw new Error("constraintName cannot be null.");
			var pathConstraints = this.pathConstraints;
			for (var i = 0, n = pathConstraints.length; i < n; i++) {
				var constraint = pathConstraints[i];
				if (constraint.name == constraintName)
					return constraint;
			}
			return null;
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
			if (skeletonMap) {
				skeletonData.hash = skeletonMap.hash;
				skeletonData.version = skeletonMap.spine;
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
					var parentName = getValue(boneMap, "parent", null);
					if (parentName)
						parent_5 = skeletonData.findBone(parentName);
					var data = new spine.BoneData(skeletonData.bones.length, boneMap.name, parent_5);
					data.length = getValue(boneMap, "length", 0) * scale;
					data.x = getValue(boneMap, "x", 0) * scale;
					data.y = getValue(boneMap, "y", 0) * scale;
					data.rotation = getValue(boneMap, "rotation", 0);
					data.scaleX = getValue(boneMap, "scaleX", 1);
					data.scaleY = getValue(boneMap, "scaleY", 1);
					data.shearX = getValue(boneMap, "shearX", 0);
					data.shearY = getValue(boneMap, "shearY", 0);
					data.transformMode = spine.Utils.enumValue(spine.TransformMode, getValue(boneMap, "transform", "Normal"));
					data.skinRequired = getValue(boneMap, "skin", false);
					var color = getValue(boneMap, "color", null);
					if (color)
						data.color.setFromString(color);
					skeletonData.bones.push(data);
				}
			}
			if (root.slots) {
				for (var i = 0; i < root.slots.length; i++) {
					var slotMap = root.slots[i];
					var boneData = skeletonData.findBone(slotMap.bone);
					var data = new spine.SlotData(skeletonData.slots.length, slotMap.name, boneData);
					var color = getValue(slotMap, "color", null);
					if (color)
						data.color.setFromString(color);
					var dark = getValue(slotMap, "dark", null);
					if (dark)
						data.darkColor = spine.Color.fromString(dark);
					data.attachmentName = getValue(slotMap, "attachment", null);
					data.blendMode = spine.Utils.enumValue(spine.BlendMode, getValue(slotMap, "blend", "normal"));
					skeletonData.slots.push(data);
				}
			}
			if (root.ik) {
				for (var i = 0; i < root.ik.length; i++) {
					var constraintMap = root.ik[i];
					var data = new spine.IkConstraintData(constraintMap.name);
					data.order = getValue(constraintMap, "order", 0);
					data.skinRequired = getValue(constraintMap, "skin", false);
					for (var ii = 0; ii < constraintMap.bones.length; ii++)
						data.bones.push(skeletonData.findBone(constraintMap.bones[ii]));
					data.target = skeletonData.findBone(constraintMap.target);
					data.mix = getValue(constraintMap, "mix", 1);
					data.softness = getValue(constraintMap, "softness", 0) * scale;
					data.bendDirection = getValue(constraintMap, "bendPositive", true) ? 1 : -1;
					data.compress = getValue(constraintMap, "compress", false);
					data.stretch = getValue(constraintMap, "stretch", false);
					data.uniform = getValue(constraintMap, "uniform", false);
					skeletonData.ikConstraints.push(data);
				}
			}
			if (root.transform) {
				for (var i = 0; i < root.transform.length; i++) {
					var constraintMap = root.transform[i];
					var data = new spine.TransformConstraintData(constraintMap.name);
					data.order = getValue(constraintMap, "order", 0);
					data.skinRequired = getValue(constraintMap, "skin", false);
					for (var ii = 0; ii < constraintMap.bones.length; ii++)
						data.bones.push(skeletonData.findBone(constraintMap.bones[ii]));
					var targetName = constraintMap.target;
					data.target = skeletonData.findBone(targetName);
					data.local = getValue(constraintMap, "local", false);
					data.relative = getValue(constraintMap, "relative", false);
					data.offsetRotation = getValue(constraintMap, "rotation", 0);
					data.offsetX = getValue(constraintMap, "x", 0) * scale;
					data.offsetY = getValue(constraintMap, "y", 0) * scale;
					data.offsetScaleX = getValue(constraintMap, "scaleX", 0);
					data.offsetScaleY = getValue(constraintMap, "scaleY", 0);
					data.offsetShearY = getValue(constraintMap, "shearY", 0);
					data.mixRotate = getValue(constraintMap, "mixRotate", 1);
					data.mixX = getValue(constraintMap, "mixX", 1);
					data.mixY = getValue(constraintMap, "mixY", data.mixX);
					data.mixScaleX = getValue(constraintMap, "mixScaleX", 1);
					data.mixScaleY = getValue(constraintMap, "mixScaleY", data.mixScaleX);
					data.mixShearY = getValue(constraintMap, "mixShearY", 1);
					skeletonData.transformConstraints.push(data);
				}
			}
			if (root.path) {
				for (var i = 0; i < root.path.length; i++) {
					var constraintMap = root.path[i];
					var data = new spine.PathConstraintData(constraintMap.name);
					data.order = getValue(constraintMap, "order", 0);
					data.skinRequired = getValue(constraintMap, "skin", false);
					for (var ii = 0; ii < constraintMap.bones.length; ii++)
						data.bones.push(skeletonData.findBone(constraintMap.bones[ii]));
					var targetName = constraintMap.target;
					data.target = skeletonData.findSlot(targetName);
					data.positionMode = spine.Utils.enumValue(spine.PositionMode, getValue(constraintMap, "positionMode", "Percent"));
					data.spacingMode = spine.Utils.enumValue(spine.SpacingMode, getValue(constraintMap, "spacingMode", "Length"));
					data.rotateMode = spine.Utils.enumValue(spine.RotateMode, getValue(constraintMap, "rotateMode", "Tangent"));
					data.offsetRotation = getValue(constraintMap, "rotation", 0);
					data.position = getValue(constraintMap, "position", 0);
					if (data.positionMode == spine.PositionMode.Fixed)
						data.position *= scale;
					data.spacing = getValue(constraintMap, "spacing", 0);
					if (data.spacingMode == spine.SpacingMode.Length || data.spacingMode == spine.SpacingMode.Fixed)
						data.spacing *= scale;
					data.mixRotate = getValue(constraintMap, "mixRotate", 1);
					data.mixX = getValue(constraintMap, "mixX", 1);
					data.mixY = getValue(constraintMap, "mixY", data.mixX);
					skeletonData.pathConstraints.push(data);
				}
			}
			if (root.skins) {
				for (var i = 0; i < root.skins.length; i++) {
					var skinMap = root.skins[i];
					var skin = new spine.Skin(skinMap.name);
					if (skinMap.bones) {
						for (var ii = 0; ii < skinMap.bones.length; ii++)
							skin.bones.push(skeletonData.findBone(skinMap.bones[ii]));
					}
					if (skinMap.ik) {
						for (var ii = 0; ii < skinMap.ik.length; ii++)
							skin.constraints.push(skeletonData.findIkConstraint(skinMap.ik[ii]));
					}
					if (skinMap.transform) {
						for (var ii = 0; ii < skinMap.transform.length; ii++)
							skin.constraints.push(skeletonData.findTransformConstraint(skinMap.transform[ii]));
					}
					if (skinMap.path) {
						for (var ii = 0; ii < skinMap.path.length; ii++)
							skin.constraints.push(skeletonData.findPathConstraint(skinMap.path[ii]));
					}
					for (var slotName in skinMap.attachments) {
						var slot = skeletonData.findSlot(slotName);
						var slotMap = skinMap.attachments[slotName];
						for (var entryName in slotMap) {
							var attachment = this.readAttachment(slotMap[entryName], skin, slot.index, entryName, skeletonData);
							if (attachment)
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
				var skin = !linkedMesh.skin ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				var parent_6 = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? parent_6 : linkedMesh.mesh;
				linkedMesh.mesh.setParentMesh(parent_6);
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;
			if (root.events) {
				for (var eventName in root.events) {
					var eventMap = root.events[eventName];
					var data = new spine.EventData(eventName);
					data.intValue = getValue(eventMap, "int", 0);
					data.floatValue = getValue(eventMap, "float", 0);
					data.stringValue = getValue(eventMap, "string", "");
					data.audioPath = getValue(eventMap, "audio", null);
					if (data.audioPath) {
						data.volume = getValue(eventMap, "volume", 1);
						data.balance = getValue(eventMap, "balance", 0);
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
			name = getValue(map, "name", name);
			switch (getValue(map, "type", "region")) {
				case "region": {
					var path = getValue(map, "path", name);
					var region = this.attachmentLoader.newRegionAttachment(skin, name, path);
					if (!region)
						return null;
					region.path = path;
					region.x = getValue(map, "x", 0) * scale;
					region.y = getValue(map, "y", 0) * scale;
					region.scaleX = getValue(map, "scaleX", 1);
					region.scaleY = getValue(map, "scaleY", 1);
					region.rotation = getValue(map, "rotation", 0);
					region.width = map.width * scale;
					region.height = map.height * scale;
					var color = getValue(map, "color", null);
					if (color)
						region.color.setFromString(color);
					region.updateOffset();
					return region;
				}
				case "boundingbox": {
					var box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
					if (!box)
						return null;
					this.readVertices(map, box, map.vertexCount << 1);
					var color = getValue(map, "color", null);
					if (color)
						box.color.setFromString(color);
					return box;
				}
				case "mesh":
				case "linkedmesh": {
					var path = getValue(map, "path", name);
					var mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (!mesh)
						return null;
					mesh.path = path;
					var color = getValue(map, "color", null);
					if (color)
						mesh.color.setFromString(color);
					mesh.width = getValue(map, "width", 0) * scale;
					mesh.height = getValue(map, "height", 0) * scale;
					var parent_7 = getValue(map, "parent", null);
					if (parent_7) {
						this.linkedMeshes.push(new LinkedMesh(mesh, getValue(map, "skin", null), slotIndex, parent_7, getValue(map, "deform", true)));
						return mesh;
					}
					var uvs = map.uvs;
					this.readVertices(map, mesh, uvs.length);
					mesh.triangles = map.triangles;
					mesh.regionUVs = uvs;
					mesh.updateUVs();
					mesh.edges = getValue(map, "edges", null);
					mesh.hullLength = getValue(map, "hull", 0) * 2;
					return mesh;
				}
				case "path": {
					var path = this.attachmentLoader.newPathAttachment(skin, name);
					if (!path)
						return null;
					path.closed = getValue(map, "closed", false);
					path.constantSpeed = getValue(map, "constantSpeed", true);
					var vertexCount = map.vertexCount;
					this.readVertices(map, path, vertexCount << 1);
					var lengths = spine.Utils.newArray(vertexCount / 3, 0);
					for (var i = 0; i < map.lengths.length; i++)
						lengths[i] = map.lengths[i] * scale;
					path.lengths = lengths;
					var color = getValue(map, "color", null);
					if (color)
						path.color.setFromString(color);
					return path;
				}
				case "point": {
					var point = this.attachmentLoader.newPointAttachment(skin, name);
					if (!point)
						return null;
					point.x = getValue(map, "x", 0) * scale;
					point.y = getValue(map, "y", 0) * scale;
					point.rotation = getValue(map, "rotation", 0);
					var color = getValue(map, "color", null);
					if (color)
						point.color.setFromString(color);
					return point;
				}
				case "clipping": {
					var clip = this.attachmentLoader.newClippingAttachment(skin, name);
					if (!clip)
						return null;
					var end = getValue(map, "end", null);
					if (end)
						clip.endSlot = skeletonData.findSlot(end);
					var vertexCount = map.vertexCount;
					this.readVertices(map, clip, vertexCount << 1);
					var color = getValue(map, "color", null);
					if (color)
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
			if (map.slots) {
				for (var slotName in map.slots) {
					var slotMap = map.slots[slotName];
					var slotIndex = skeletonData.findSlotIndex(slotName);
					for (var timelineName in slotMap) {
						var timelineMap = slotMap[timelineName];
						if (!timelineMap)
							continue;
						if (timelineName == "attachment") {
							var timeline = new spine.AttachmentTimeline(timelineMap.length, slotIndex);
							for (var frame = 0; frame < timelineMap.length; frame++) {
								var keyMap = timelineMap[frame];
								timeline.setFrame(frame, getValue(keyMap, "time", 0), keyMap.name);
							}
							timelines.push(timeline);
						}
						else if (timelineName == "rgba") {
							var timeline = new spine.RGBATimeline(timelineMap.length, timelineMap.length << 2, slotIndex);
							var keyMap = timelineMap[0];
							var time = getValue(keyMap, "time", 0);
							var color = spine.Color.fromString(keyMap.color);
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color.a);
								var nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								var time2 = getValue(nextMap, "time", 0);
								var newColor = spine.Color.fromString(nextMap.color);
								var curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
								}
								time = time2;
								color = newColor;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
						else if (timelineName == "rgb") {
							var timeline = new spine.RGBTimeline(timelineMap.length, timelineMap.length * 3, slotIndex);
							var keyMap = timelineMap[0];
							var time = getValue(keyMap, "time", 0);
							var color = spine.Color.fromString(keyMap.color);
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b);
								var nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								var time2 = getValue(nextMap, "time", 0);
								var newColor = spine.Color.fromString(nextMap.color);
								var curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
								}
								time = time2;
								color = newColor;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
						else if (timelineName == "alpha") {
							timelines.push(readTimeline1(timelineMap, new spine.AlphaTimeline(timelineMap.length, timelineMap.length, slotIndex), 0, 1));
						}
						else if (timelineName == "rgba2") {
							var timeline = new spine.RGBA2Timeline(timelineMap.length, timelineMap.length * 7, slotIndex);
							var keyMap = timelineMap[0];
							var time = getValue(keyMap, "time", 0);
							var color = spine.Color.fromString(keyMap.light);
							var color2 = spine.Color.fromString(keyMap.dark);
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color.a, color2.r, color2.g, color2.b);
								var nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								var time2 = getValue(nextMap, "time", 0);
								var newColor = spine.Color.fromString(nextMap.light);
								var newColor2 = spine.Color.fromString(nextMap.dark);
								var curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.r, newColor2.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.g, newColor2.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 6, time, time2, color2.b, newColor2.b, 1);
								}
								time = time2;
								color = newColor;
								color2 = newColor2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
						else if (timelineName == "rgb2") {
							var timeline = new spine.RGB2Timeline(timelineMap.length, timelineMap.length * 6, slotIndex);
							var keyMap = timelineMap[0];
							var time = getValue(keyMap, "time", 0);
							var color = spine.Color.fromString(keyMap.light);
							var color2 = spine.Color.fromString(keyMap.dark);
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color2.r, color2.g, color2.b);
								var nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								var time2 = getValue(nextMap, "time", 0);
								var newColor = spine.Color.fromString(nextMap.light);
								var newColor2 = spine.Color.fromString(nextMap.dark);
								var curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color2.r, newColor2.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.g, newColor2.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.b, newColor2.b, 1);
								}
								time = time2;
								color = newColor;
								color2 = newColor2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
					}
				}
			}
			if (map.bones) {
				for (var boneName in map.bones) {
					var boneMap = map.bones[boneName];
					var boneIndex = skeletonData.findBoneIndex(boneName);
					for (var timelineName in boneMap) {
						var timelineMap = boneMap[timelineName];
						if (timelineMap.length == 0)
							continue;
						if (timelineName === "rotate") {
							timelines.push(readTimeline1(timelineMap, new spine.RotateTimeline(timelineMap.length, timelineMap.length, boneIndex), 0, 1));
						}
						else if (timelineName === "translate") {
							var timeline = new spine.TranslateTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
							timelines.push(readTimeline2(timelineMap, timeline, "x", "y", 0, scale));
						}
						else if (timelineName === "translatex") {
							var timeline = new spine.TranslateXTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 0, scale));
						}
						else if (timelineName === "translatey") {
							var timeline = new spine.TranslateYTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 0, scale));
						}
						else if (timelineName === "scale") {
							var timeline = new spine.ScaleTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
							timelines.push(readTimeline2(timelineMap, timeline, "x", "y", 1, 1));
						}
						else if (timelineName === "scalex") {
							var timeline = new spine.ScaleXTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 1, 1));
						}
						else if (timelineName === "scaley") {
							var timeline = new spine.ScaleYTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 1, 1));
						}
						else if (timelineName === "shear") {
							var timeline = new spine.ShearTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
							timelines.push(readTimeline2(timelineMap, timeline, "x", "y", 0, 1));
						}
						else if (timelineName === "shearx") {
							var timeline = new spine.ShearXTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 0, 1));
						}
						else if (timelineName === "sheary") {
							var timeline = new spine.ShearYTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 0, 1));
						}
					}
				}
			}
			if (map.ik) {
				for (var constraintName in map.ik) {
					var constraintMap = map.ik[constraintName];
					var keyMap = constraintMap[0];
					if (!keyMap)
						continue;
					var constraint = skeletonData.findIkConstraint(constraintName);
					var constraintIndex = skeletonData.ikConstraints.indexOf(constraint);
					var timeline = new spine.IkConstraintTimeline(constraintMap.length, constraintMap.length << 1, constraintIndex);
					var time = getValue(keyMap, "time", 0);
					var mix = getValue(keyMap, "mix", 1);
					var softness = getValue(keyMap, "softness", 0) * scale;
					for (var frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, mix, softness, getValue(keyMap, "bendPositive", true) ? 1 : -1, getValue(keyMap, "compress", false), getValue(keyMap, "stretch", false));
						var nextMap = constraintMap[frame + 1];
						if (!nextMap) {
							timeline.shrink(bezier);
							break;
						}
						var time2 = getValue(nextMap, "time", 0);
						var mix2 = getValue(nextMap, "mix", 1);
						var softness2 = getValue(nextMap, "softness", 0) * scale;
						var curve = keyMap.curve;
						if (curve) {
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale);
						}
						time = time2;
						mix = mix2;
						softness = softness2;
						keyMap = nextMap;
					}
					timelines.push(timeline);
				}
			}
			if (map.transform) {
				for (var constraintName in map.transform) {
					var timelineMap = map.transform[constraintName];
					var keyMap = timelineMap[0];
					if (!keyMap)
						continue;
					var constraint = skeletonData.findTransformConstraint(constraintName);
					var constraintIndex = skeletonData.transformConstraints.indexOf(constraint);
					var timeline = new spine.TransformConstraintTimeline(timelineMap.length, timelineMap.length << 2, constraintIndex);
					var time = getValue(keyMap, "time", 0);
					var mixRotate = getValue(keyMap, "mixRotate", 1);
					var mixX = getValue(keyMap, "mixX", 1);
					var mixY = getValue(keyMap, "mixY", mixX);
					var mixScaleX = getValue(keyMap, "mixScaleX", 1);
					var mixScaleY = getValue(keyMap, "mixScaleY", mixScaleX);
					var mixShearY = getValue(keyMap, "mixShearY", 1);
					for (var frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
						var nextMap = timelineMap[frame + 1];
						if (!nextMap) {
							timeline.shrink(bezier);
							break;
						}
						var time2 = getValue(nextMap, "time", 0);
						var mixRotate2 = getValue(nextMap, "mixRotate", 1);
						var mixX2 = getValue(nextMap, "mixX", 1);
						var mixY2 = getValue(nextMap, "mixY", mixX2);
						var mixScaleX2 = getValue(nextMap, "mixScaleX", 1);
						var mixScaleY2 = getValue(nextMap, "mixScaleY", mixScaleX2);
						var mixShearY2 = getValue(nextMap, "mixShearY", 1);
						var curve = keyMap.curve;
						if (curve) {
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
						}
						time = time2;
						mixRotate = mixRotate2;
						mixX = mixX2;
						mixY = mixY2;
						mixScaleX = mixScaleX2;
						mixScaleY = mixScaleY2;
						mixScaleX = mixScaleX2;
						keyMap = nextMap;
					}
					timelines.push(timeline);
				}
			}
			if (map.path) {
				for (var constraintName in map.path) {
					var constraintMap = map.path[constraintName];
					var constraint = skeletonData.findPathConstraint(constraintName);
					var constraintIndex = skeletonData.pathConstraints.indexOf(constraint);
					for (var timelineName in constraintMap) {
						var timelineMap = constraintMap[timelineName];
						var keyMap = timelineMap[0];
						if (!keyMap)
							continue;
						if (timelineName === "position") {
							var timeline = new spine.PathConstraintPositionTimeline(timelineMap.length, timelineMap.length, constraintIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 0, constraint.positionMode == spine.PositionMode.Fixed ? scale : 1));
						}
						else if (timelineName === "spacing") {
							var timeline = new spine.PathConstraintSpacingTimeline(timelineMap.length, timelineMap.length, constraintIndex);
							timelines.push(readTimeline1(timelineMap, timeline, 0, constraint.spacingMode == spine.SpacingMode.Length || constraint.spacingMode == spine.SpacingMode.Fixed ? scale : 1));
						}
						else if (timelineName === "mix") {
							var timeline = new spine.PathConstraintMixTimeline(timelineMap.size, timelineMap.size * 3, constraintIndex);
							var time = getValue(keyMap, "time", 0);
							var mixRotate = getValue(keyMap, "mixRotate", 1);
							var mixX = getValue(keyMap, "mixX", 1);
							var mixY = getValue(keyMap, "mixY", mixX);
							for (var frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, mixRotate, mixX, mixY);
								var nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								var time2 = getValue(nextMap, "time", 0);
								var mixRotate2 = getValue(nextMap, "mixRotate", 1);
								var mixX2 = getValue(nextMap, "mixX", 1);
								var mixY2 = getValue(nextMap, "mixY", mixX2);
								var curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
								}
								time = time2;
								mixRotate = mixRotate2;
								mixX = mixX2;
								mixY = mixY2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
					}
				}
			}
			if (map.deform) {
				for (var deformName in map.deform) {
					var deformMap = map.deform[deformName];
					var skin = skeletonData.findSkin(deformName);
					for (var slotName in deformMap) {
						var slotMap = deformMap[slotName];
						var slotIndex = skeletonData.findSlotIndex(slotName);
						for (var timelineName in slotMap) {
							var timelineMap = slotMap[timelineName];
							var keyMap = timelineMap[0];
							if (!keyMap)
								continue;
							var attachment = skin.getAttachment(slotIndex, timelineName);
							var weighted = attachment.bones;
							var vertices = attachment.vertices;
							var deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;
							var timeline = new spine.DeformTimeline(timelineMap.length, timelineMap.length, slotIndex, attachment);
							var time = getValue(keyMap, "time", 0);
							for (var frame = 0, bezier = 0;; frame++) {
								var deform = void 0;
								var verticesValue = getValue(keyMap, "vertices", null);
								if (!verticesValue)
									deform = weighted ? spine.Utils.newFloatArray(deformLength) : vertices;
								else {
									deform = spine.Utils.newFloatArray(deformLength);
									var start = getValue(keyMap, "offset", 0);
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
								timeline.setFrame(frame, time, deform);
								var nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								var time2 = getValue(nextMap, "time", 0);
								var curve = keyMap.curve;
								if (curve)
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1);
								time = time2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
					}
				}
			}
			if (map.drawOrder) {
				var timeline = new spine.DrawOrderTimeline(map.drawOrder.length);
				var slotCount = skeletonData.slots.length;
				var frame = 0;
				for (var i = 0; i < map.drawOrder.length; i++, frame++) {
					var drawOrderMap = map.drawOrder[i];
					var drawOrder = null;
					var offsets = getValue(drawOrderMap, "offsets", null);
					if (offsets) {
						drawOrder = spine.Utils.newArray(slotCount, -1);
						var unchanged = spine.Utils.newArray(slotCount - offsets.length, 0);
						var originalIndex = 0, unchangedIndex = 0;
						for (var ii = 0; ii < offsets.length; ii++) {
							var offsetMap = offsets[ii];
							var slotIndex = skeletonData.findSlotIndex(offsetMap.slot);
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							drawOrder[originalIndex + offsetMap.offset] = originalIndex++;
						}
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						for (var ii = slotCount - 1; ii >= 0; ii--)
							if (drawOrder[ii] == -1)
								drawOrder[ii] = unchanged[--unchangedIndex];
					}
					timeline.setFrame(frame, getValue(drawOrderMap, "time", 0), drawOrder);
				}
				timelines.push(timeline);
			}
			if (map.events) {
				var timeline = new spine.EventTimeline(map.events.length);
				var frame = 0;
				for (var i = 0; i < map.events.length; i++, frame++) {
					var eventMap = map.events[i];
					var eventData = skeletonData.findEvent(eventMap.name);
					var event_6 = new spine.Event(spine.Utils.toSinglePrecision(getValue(eventMap, "time", 0)), eventData);
					event_6.intValue = getValue(eventMap, "int", eventData.intValue);
					event_6.floatValue = getValue(eventMap, "float", eventData.floatValue);
					event_6.stringValue = getValue(eventMap, "string", eventData.stringValue);
					if (event_6.data.audioPath) {
						event_6.volume = getValue(eventMap, "volume", 1);
						event_6.balance = getValue(eventMap, "balance", 0);
					}
					timeline.setFrame(frame, event_6);
				}
				timelines.push(timeline);
			}
			var duration = 0;
			for (var i = 0, n = timelines.length; i < n; i++)
				duration = Math.max(duration, timelines[i].getDuration());
			skeletonData.animations.push(new spine.Animation(name, timelines, duration));
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
	function readTimeline1(keys, timeline, defaultValue, scale) {
		var keyMap = keys[0];
		var time = getValue(keyMap, "time", 0);
		var value = getValue(keyMap, "value", defaultValue) * scale;
		var bezier = 0;
		for (var frame = 0;; frame++) {
			timeline.setFrame(frame, time, value);
			var nextMap = keys[frame + 1];
			if (!nextMap) {
				timeline.shrink(bezier);
				return timeline;
			}
			var time2 = getValue(nextMap, "time", 0);
			var value2 = getValue(nextMap, "value", defaultValue) * scale;
			if (keyMap.curve)
				bezier = readCurve(keyMap.curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
			time = time2;
			value = value2;
			keyMap = nextMap;
		}
	}
	function readTimeline2(keys, timeline, name1, name2, defaultValue, scale) {
		var keyMap = keys[0];
		var time = getValue(keyMap, "time", 0);
		var value1 = getValue(keyMap, name1, defaultValue) * scale;
		var value2 = getValue(keyMap, name2, defaultValue) * scale;
		var bezier = 0;
		for (var frame = 0;; frame++) {
			timeline.setFrame(frame, time, value1, value2);
			var nextMap = keys[frame + 1];
			if (!nextMap) {
				timeline.shrink(bezier);
				return timeline;
			}
			var time2 = getValue(nextMap, "time", 0);
			var nvalue1 = getValue(nextMap, name1, defaultValue) * scale;
			var nvalue2 = getValue(nextMap, name2, defaultValue) * scale;
			var curve = keyMap.curve;
			if (curve) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
			keyMap = nextMap;
		}
	}
	function readCurve(curve, timeline, bezier, frame, value, time1, time2, value1, value2, scale) {
		if (curve == "stepped") {
			timeline.setStepped(frame);
			return bezier;
		}
		var i = value << 2;
		var cx1 = curve[i];
		var cy1 = curve[i + 1] * scale;
		var cx2 = curve[i + 2];
		var cy2 = curve[i + 3] * scale;
		timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
		return bezier + 1;
	}
	function getValue(map, property, defaultValue) {
		return map[property] !== undefined ? map[property] : defaultValue;
	}
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
			if (!name)
				throw new Error("name cannot be null.");
			this.name = name;
		}
		Skin.prototype.setAttachment = function (slotIndex, name, attachment) {
			if (!attachment)
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
				for (var ii = 0; ii < this.bones.length; ii++) {
					if (this.bones[ii] == bone) {
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
				for (var ii = 0; ii < this.constraints.length; ii++) {
					if (this.constraints[ii] == constraint) {
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
				for (var ii = 0; ii < this.bones.length; ii++) {
					if (this.bones[ii] == bone) {
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
				for (var ii = 0; ii < this.constraints.length; ii++) {
					if (this.constraints[ii] == constraint) {
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
				if (!attachment.attachment)
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
							if (attachment)
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
			if (!data)
				throw new Error("data cannot be null.");
			if (!bone)
				throw new Error("bone cannot be null.");
			this.data = data;
			this.bone = bone;
			this.color = new spine.Color();
			this.darkColor = !data.darkColor ? null : new spine.Color();
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
			if (!(attachment instanceof spine.VertexAttachment) || !(this.attachment instanceof spine.VertexAttachment)
				|| attachment.deformAttachment != this.attachment.deformAttachment) {
				this.deform.length = 0;
			}
			this.attachment = attachment;
			this.attachmentTime = this.bone.skeleton.time;
		};
		Slot.prototype.setAttachmentTime = function (time) {
			this.attachmentTime = this.bone.skeleton.time - time;
		};
		Slot.prototype.getAttachmentTime = function () {
			return this.bone.skeleton.time - this.attachmentTime;
		};
		Slot.prototype.setToSetupPose = function () {
			this.color.setFromColor(this.data.color);
			if (this.darkColor)
				this.darkColor.setFromColor(this.data.darkColor);
			if (!this.data.attachmentName)
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
			if (!name)
				throw new Error("name cannot be null.");
			if (!boneData)
				throw new Error("boneData cannot be null.");
			this.index = index;
			this.name = name;
			this.boneData = boneData;
		}
		return SlotData;
	}());
	spine.SlotData = SlotData;
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
	var Texture = (function () {
		function Texture(image) {
			this._image = image;
		}
		Texture.prototype.getImage = function () {
			return this._image;
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
			this.degrees = 0;
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
		function TextureAtlas(atlasText) {
			this.pages = new Array();
			this.regions = new Array();
			var reader = new TextureAtlasReader(atlasText);
			var entry = new Array(4);
			var page = null;
			var region = null;
			var pageFields = {};
			pageFields["size"] = function () {
				page.width = parseInt(entry[1]);
				page.height = parseInt(entry[2]);
			};
			pageFields["format"] = function () {
			};
			pageFields["filter"] = function () {
				page.minFilter = spine.Utils.enumValue(spine.TextureFilter, entry[1]);
				page.magFilter = spine.Utils.enumValue(spine.TextureFilter, entry[2]);
			};
			pageFields["repeat"] = function () {
				if (entry[1].indexOf('x') != -1)
					page.uWrap = spine.TextureWrap.Repeat;
				if (entry[1].indexOf('y') != -1)
					page.vWrap = spine.TextureWrap.Repeat;
			};
			pageFields["pma"] = function () {
				page.pma = entry[1] == "true";
			};
			var regionFields = {};
			regionFields["xy"] = function () {
				region.x = parseInt(entry[1]);
				region.y = parseInt(entry[2]);
			};
			regionFields["size"] = function () {
				region.width = parseInt(entry[1]);
				region.height = parseInt(entry[2]);
			};
			regionFields["bounds"] = function () {
				region.x = parseInt(entry[1]);
				region.y = parseInt(entry[2]);
				region.width = parseInt(entry[3]);
				region.height = parseInt(entry[4]);
			};
			regionFields["offset"] = function () {
				region.offsetX = parseInt(entry[1]);
				region.offsetY = parseInt(entry[2]);
			};
			regionFields["orig"] = function () {
				region.originalWidth = parseInt(entry[1]);
				region.originalHeight = parseInt(entry[2]);
			};
			regionFields["offsets"] = function () {
				region.offsetX = parseInt(entry[1]);
				region.offsetY = parseInt(entry[2]);
				region.originalWidth = parseInt(entry[3]);
				region.originalHeight = parseInt(entry[4]);
			};
			regionFields["rotate"] = function () {
				var value = entry[1];
				if (value == "true")
					region.degrees = 90;
				else if (value != "false")
					region.degrees = parseInt(value);
			};
			regionFields["index"] = function () {
				region.index = parseInt(entry[1]);
			};
			var line = reader.readLine();
			while (line && line.trim().length == 0)
				line = reader.readLine();
			while (true) {
				if (!line || line.trim().length == 0)
					break;
				if (reader.readEntry(entry, line) == 0)
					break;
				line = reader.readLine();
			}
			var names = null;
			var values = null;
			while (true) {
				if (line === null)
					break;
				if (line.trim().length == 0) {
					page = null;
					line = reader.readLine();
				}
				else if (!page) {
					page = new TextureAtlasPage();
					page.name = line.trim();
					while (true) {
						if (reader.readEntry(entry, line = reader.readLine()) == 0)
							break;
						var field = pageFields[entry[0]];
						if (field)
							field();
					}
					this.pages.push(page);
				}
				else {
					region = new TextureAtlasRegion();
					region.page = page;
					region.name = line;
					while (true) {
						var count = reader.readEntry(entry, line = reader.readLine());
						if (count == 0)
							break;
						var field = regionFields[entry[0]];
						if (field)
							field();
						else {
							if (!names) {
								names = [];
								values = [];
							}
							names.push(entry[0]);
							var entryValues = [];
							for (var i = 0; i < count; i++)
								entryValues.push(parseInt(entry[i + 1]));
							values.push(entryValues);
						}
					}
					if (region.originalWidth == 0 && region.originalHeight == 0) {
						region.originalWidth = region.width;
						region.originalHeight = region.height;
					}
					if (names && names.length > 0) {
						region.names = names;
						region.values = values;
						names = null;
						values = null;
					}
					region.u = region.x / page.width;
					region.v = region.y / page.height;
					if (region.degrees == 90) {
						region.u2 = (region.x + region.height) / page.width;
						region.v2 = (region.y + region.width) / page.height;
					}
					else {
						region.u2 = (region.x + region.width) / page.width;
						region.v2 = (region.y + region.height) / page.height;
					}
					this.regions.push(region);
				}
			}
		}
		TextureAtlas.prototype.findRegion = function (name) {
			for (var i = 0; i < this.regions.length; i++) {
				if (this.regions[i].name == name) {
					return this.regions[i];
				}
			}
			return null;
		};
		TextureAtlas.prototype.setTextures = function (assetManager, pathPrefix) {
			if (pathPrefix === void 0) { pathPrefix = ""; }
			for (var _i = 0, _a = this.pages; _i < _a.length; _i++) {
				var page = _a[_i];
				page.setTexture(assetManager.get(pathPrefix + page.name));
			}
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
		TextureAtlasReader.prototype.readEntry = function (entry, line) {
			if (!line)
				return 0;
			line = line.trim();
			if (line.length == 0)
				return 0;
			var colon = line.indexOf(':');
			if (colon == -1)
				return 0;
			entry[0] = line.substr(0, colon).trim();
			for (var i = 1, lastMatch = colon + 1;; i++) {
				var comma = line.indexOf(',', lastMatch);
				if (comma == -1) {
					entry[i] = line.substr(lastMatch).trim();
					return i;
				}
				entry[i] = line.substr(lastMatch, comma - lastMatch).trim();
				lastMatch = comma + 1;
				if (i == 4)
					return 4;
			}
		};
		return TextureAtlasReader;
	}());
	var TextureAtlasPage = (function () {
		function TextureAtlasPage() {
			this.minFilter = spine.TextureFilter.Nearest;
			this.magFilter = spine.TextureFilter.Nearest;
			this.uWrap = spine.TextureWrap.ClampToEdge;
			this.vWrap = spine.TextureWrap.ClampToEdge;
		}
		TextureAtlasPage.prototype.setTexture = function (texture) {
			this.texture = texture;
			texture.setFilters(this.minFilter, this.magFilter);
			texture.setWraps(this.uWrap, this.vWrap);
		};
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
			this.mixRotate = 0;
			this.mixX = 0;
			this.mixY = 0;
			this.mixScaleX = 0;
			this.mixScaleY = 0;
			this.mixShearY = 0;
			this.temp = new spine.Vector2();
			this.active = false;
			if (!data)
				throw new Error("data cannot be null.");
			if (!skeleton)
				throw new Error("skeleton cannot be null.");
			this.data = data;
			this.mixRotate = data.mixRotate;
			this.mixX = data.mixX;
			this.mixY = data.mixY;
			this.mixScaleX = data.mixScaleX;
			this.mixScaleY = data.mixScaleY;
			this.mixShearY = data.mixShearY;
			this.bones = new Array();
			for (var i = 0; i < data.bones.length; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findBone(data.target.name);
		}
		TransformConstraint.prototype.isActive = function () {
			return this.active;
		};
		TransformConstraint.prototype.update = function () {
			if (this.mixRotate == 0 && this.mixX == 0 && this.mixY == 0 && this.mixScaleX == 0 && this.mixScaleX == 0 && this.mixShearY == 0)
				return;
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
			var mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX, mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
			var translate = mixX != 0 || mixY != 0;
			var target = this.target;
			var ta = target.a, tb = target.b, tc = target.c, td = target.d;
			var degRadReflect = ta * td - tb * tc > 0 ? spine.MathUtils.degRad : -spine.MathUtils.degRad;
			var offsetRotation = this.data.offsetRotation * degRadReflect;
			var offsetShearY = this.data.offsetShearY * degRadReflect;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (mixRotate != 0) {
					var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					var r = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r *= mixRotate;
					var cos = Math.cos(r), sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
				if (translate) {
					var temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += (temp.x - bone.worldX) * mixX;
					bone.worldY += (temp.y - bone.worldY) * mixY;
				}
				if (mixScaleX != 0) {
					var s = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
					if (s != 0)
						s = (s + (Math.sqrt(ta * ta + tc * tc) - s + this.data.offsetScaleX) * mixScaleX) / s;
					bone.a *= s;
					bone.c *= s;
				}
				if (mixScaleY != 0) {
					var s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					if (s != 0)
						s = (s + (Math.sqrt(tb * tb + td * td) - s + this.data.offsetScaleY) * mixScaleY) / s;
					bone.b *= s;
					bone.d *= s;
				}
				if (mixShearY > 0) {
					var b = bone.b, d = bone.d;
					var by = Math.atan2(d, b);
					var r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r = by + (r + offsetShearY) * mixShearY;
					var s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
				}
				bone.updateAppliedTransform();
			}
		};
		TransformConstraint.prototype.applyRelativeWorld = function () {
			var mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX, mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
			var translate = mixX != 0 || mixY != 0;
			var target = this.target;
			var ta = target.a, tb = target.b, tc = target.c, td = target.d;
			var degRadReflect = ta * td - tb * tc > 0 ? spine.MathUtils.degRad : -spine.MathUtils.degRad;
			var offsetRotation = this.data.offsetRotation * degRadReflect, offsetShearY = this.data.offsetShearY * degRadReflect;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				if (mixRotate != 0) {
					var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					var r = Math.atan2(tc, ta) + offsetRotation;
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					r *= mixRotate;
					var cos = Math.cos(r), sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
				if (translate) {
					var temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += temp.x * mixX;
					bone.worldY += temp.y * mixY;
				}
				if (mixScaleX != 0) {
					var s = (Math.sqrt(ta * ta + tc * tc) - 1 + this.data.offsetScaleX) * mixScaleX + 1;
					bone.a *= s;
					bone.c *= s;
				}
				if (mixScaleY != 0) {
					var s = (Math.sqrt(tb * tb + td * td) - 1 + this.data.offsetScaleY) * mixScaleY + 1;
					bone.b *= s;
					bone.d *= s;
				}
				if (mixShearY > 0) {
					var r = Math.atan2(td, tb) - Math.atan2(tc, ta);
					if (r > spine.MathUtils.PI)
						r -= spine.MathUtils.PI2;
					else if (r < -spine.MathUtils.PI)
						r += spine.MathUtils.PI2;
					var b = bone.b, d = bone.d;
					r = Math.atan2(d, b) + (r - spine.MathUtils.PI / 2 + offsetShearY) * mixShearY;
					var s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
				}
				bone.updateAppliedTransform();
			}
		};
		TransformConstraint.prototype.applyAbsoluteLocal = function () {
			var mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX, mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
			var target = this.target;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				var rotation = bone.arotation;
				if (mixRotate != 0) {
					var r = target.arotation - rotation + this.data.offsetRotation;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					rotation += r * mixRotate;
				}
				var x = bone.ax, y = bone.ay;
				x += (target.ax - x + this.data.offsetX) * mixX;
				y += (target.ay - y + this.data.offsetY) * mixY;
				var scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (mixScaleX != 0 && scaleX != 0)
					scaleX = (scaleX + (target.ascaleX - scaleX + this.data.offsetScaleX) * mixScaleX) / scaleX;
				if (mixScaleY != 0 && scaleY != 0)
					scaleY = (scaleY + (target.ascaleY - scaleY + this.data.offsetScaleY) * mixScaleY) / scaleY;
				var shearY = bone.ashearY;
				if (mixShearY != 0) {
					var r = target.ashearY - shearY + this.data.offsetShearY;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					shearY += r * mixShearY;
				}
				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		};
		TransformConstraint.prototype.applyRelativeLocal = function () {
			var mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX, mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
			var target = this.target;
			var bones = this.bones;
			for (var i = 0, n = bones.length; i < n; i++) {
				var bone = bones[i];
				var rotation = bone.arotation + (target.arotation + this.data.offsetRotation) * mixRotate;
				var x = bone.ax + (target.ax + this.data.offsetX) * mixX;
				var y = bone.ay + (target.ay + this.data.offsetY) * mixY;
				var scaleX = (bone.ascaleX * ((target.ascaleX - 1 + this.data.offsetScaleX) * mixScaleX) + 1);
				var scaleY = (bone.ascaleY * ((target.ascaleY - 1 + this.data.offsetScaleY) * mixScaleY) + 1);
				var shearY = bone.ashearY + (target.ashearY + this.data.offsetShearY) * mixShearY;
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
			_this.mixRotate = 0;
			_this.mixX = 0;
			_this.mixY = 0;
			_this.mixScaleX = 0;
			_this.mixScaleY = 0;
			_this.mixShearY = 0;
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
	var StringSet = (function () {
		function StringSet() {
			this.entries = {};
			this.size = 0;
		}
		StringSet.prototype.add = function (value) {
			var contains = this.entries[value];
			this.entries[value] = true;
			if (!contains) {
				this.size++;
				return true;
			}
			return false;
		};
		StringSet.prototype.addAll = function (values) {
			var oldSize = this.size;
			for (var i = 0, n = values.length; i < n; i++)
				this.add(values[i]);
			return oldSize != this.size;
		};
		StringSet.prototype.contains = function (value) {
			return this.entries[value];
		};
		StringSet.prototype.clear = function () {
			this.entries = {};
			this.size = 0;
		};
		return StringSet;
	}());
	spine.StringSet = StringSet;
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
			return this.clamp();
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
			this.r = parseInt(hex.substr(0, 2), 16) / 255;
			this.g = parseInt(hex.substr(2, 2), 16) / 255;
			this.b = parseInt(hex.substr(4, 2), 16) / 255;
			this.a = hex.length != 8 ? 1 : parseInt(hex.substr(6, 2), 16) / 255;
			return this;
		};
		Color.prototype.add = function (r, g, b, a) {
			this.r += r;
			this.g += g;
			this.b += b;
			this.a += a;
			return this.clamp();
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
		Color.fromString = function (hex) {
			return new Color().setFromString(hex);
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
		Utils.arrayFill = function (array, fromIndex, toIndex, value) {
			for (var i = fromIndex; i < toIndex; i++)
				array[i] = value;
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
			if (Utils.SUPPORTS_TYPED_ARRAYS)
				return new Float32Array(size);
			else {
				var array = new Array(size);
				for (var i = 0; i < array.length; i++)
					array[i] = 0;
				return array;
			}
		};
		Utils.newShortArray = function (size) {
			if (Utils.SUPPORTS_TYPED_ARRAYS)
				return new Int16Array(size);
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
			for (var i = 0; i < array.length; i++)
				if (array[i] == element)
					return true;
			return false;
		};
		Utils.enumValue = function (type, name) {
			return type[name[0].toUpperCase() + name.slice(1)];
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
			for (var i = 0; i < items.length; i++)
				this.free(items[i]);
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
					for (var i = 0; i < this.values.length; i++)
						mean += this.values[i];
					this.mean = mean / this.values.length;
					this.dirty = false;
				}
				return this.mean;
			}
			return 0;
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
			if (!name)
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
			_this.id = VertexAttachment.nextID++;
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
			if (!bones) {
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
			if (this.bones) {
				attachment.bones = new Array(this.bones.length);
				spine.Utils.arrayCopy(this.bones, 0, attachment.bones, 0, this.bones.length);
			}
			else
				attachment.bones = null;
			if (this.vertices) {
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
	var BoundingBoxAttachment = (function (_super) {
		__extends(BoundingBoxAttachment, _super);
		function BoundingBoxAttachment(name) {
			var _this = _super.call(this, name) || this;
			_this.color = new spine.Color(1, 1, 1, 1);
			return _this;
		}
		BoundingBoxAttachment.prototype.copy = function () {
			var copy = new BoundingBoxAttachment(this.name);
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
			var copy = new ClippingAttachment(this.name);
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
			if (!this.uvs || this.uvs.length != regionUVs.length)
				this.uvs = spine.Utils.newFloatArray(regionUVs.length);
			var uvs = this.uvs;
			var n = this.uvs.length;
			var u = this.region.u, v = this.region.v, width = 0, height = 0;
			if (this.region instanceof spine.TextureAtlasRegion) {
				var region = this.region, image = region.page.texture.getImage();
				var textureWidth = image.width, textureHeight = image.height;
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
			else if (!this.region) {
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
			if (parentMesh) {
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
			if (this.parentMesh)
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
			if (this.edges) {
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
			copy.setParentMesh(this.parentMesh ? this.parentMesh : this);
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
			var copy = new PathAttachment(this.name);
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
			var copy = new PointAttachment(this.name);
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
			var region = this.region;
			var regionScaleX = this.width / this.region.originalWidth * this.scaleX;
			var regionScaleY = this.height / this.region.originalHeight * this.scaleY;
			var localX = -this.width / 2 * this.scaleX + this.region.offsetX * regionScaleX;
			var localY = -this.height / 2 * this.scaleY + this.region.offsetY * regionScaleY;
			var localX2 = localX + this.region.width * regionScaleX;
			var localY2 = localY + this.region.height * regionScaleY;
			var radians = this.rotation * Math.PI / 180;
			var cos = Math.cos(radians);
			var sin = Math.sin(radians);
			var x = this.x, y = this.y;
			var localXCos = localX * cos + x;
			var localXSin = localX * sin;
			var localYCos = localY * cos + y;
			var localYSin = localY * sin;
			var localX2Cos = localX2 * cos + x;
			var localX2Sin = localX2 * sin;
			var localY2Cos = localY2 * cos + y;
			var localY2Sin = localY2 * sin;
			var offset = this.offset;
			offset[0] = localXCos - localYSin;
			offset[1] = localYCos + localXSin;
			offset[2] = localXCos - localY2Sin;
			offset[3] = localY2Cos + localXSin;
			offset[4] = localX2Cos - localY2Sin;
			offset[5] = localY2Cos + localX2Sin;
			offset[6] = localX2Cos - localYSin;
			offset[7] = localYCos + localX2Sin;
		};
		RegionAttachment.prototype.setRegion = function (region) {
			this.region = region;
			var uvs = this.uvs;
			if (region.degrees == 90) {
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
			offsetX = vertexOffset[0];
			offsetY = vertexOffset[1];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
			offsetX = vertexOffset[2];
			offsetY = vertexOffset[3];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
			offsetX = vertexOffset[4];
			offsetY = vertexOffset[5];
			worldVertices[offset] = offsetX * a + offsetY * b + x;
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
			offsetX = vertexOffset[6];
			offsetY = vertexOffset[7];
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
			function AssetManager(context, pathPrefix, downloader) {
				if (pathPrefix === void 0) { pathPrefix = ""; }
				if (downloader === void 0) { downloader = null; }
				return _super.call(this, function (image) {
					return new spine.webgl.GLTexture(context, image);
				}, pathPrefix, downloader) || this;
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
				screenCoords.x = (2 * x) / screenWidth - 1;
				screenCoords.y = (2 * y) / screenHeight - 1;
				screenCoords.z = (2 * screenCoords.z) - 1;
				screenCoords.project(this.inverseProjectionView);
				return screenCoords;
			};
			OrthoCamera.prototype.worldToScreen = function (worldCoords, screenWidth, screenHeight) {
				worldCoords.project(this.projectionView);
				worldCoords.x = screenWidth * (worldCoords.x + 1) / 2;
				worldCoords.y = screenHeight * (worldCoords.y + 1) / 2;
				worldCoords.z = (worldCoords.z + 1) / 2;
				return worldCoords;
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
				if (!this.texture)
					this.texture = this.context.gl.createTexture();
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
				this.listeners = new Array();
				this.touchesPool = new spine.Pool(function () {
					return new spine.webgl.Touch(0, 0, 0);
				});
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
						for (var i = 0; i < listeners.length; i++)
							if (listeners[i].down)
								listeners[i].down(x, y);
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
						for (var i = 0; i < listeners.length; i++)
							if (listeners[i].up)
								listeners[i].up(x, y);
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
					if (!_this.currTouch) {
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
					}
					ev.preventDefault();
				}, false);
				element.addEventListener("touchend", function (ev) {
					if (_this.currTouch) {
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
					}
					ev.preventDefault();
				}, false);
				element.addEventListener("touchcancel", function (ev) {
					if (_this.currTouch) {
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
					}
					ev.preventDefault();
				}, false);
				element.addEventListener("touchmove", function (ev) {
					if (_this.currTouch) {
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
		var spinnerImage;
		var logoImage;
		var loaded = 0;
		var FADE_IN = 1, FADE_OUT = 1;
		var logoWidth = 165, logoHeight = 108, spinnerSize = 163;
		var LoadingScreen = (function () {
			function LoadingScreen(renderer) {
				this.logo = null;
				this.spinner = null;
				this.angle = 0;
				this.fadeOut = 0;
				this.fadeIn = 0;
				this.timeKeeper = new spine.TimeKeeper();
				this.backgroundColor = new spine.Color(0.135, 0.135, 0.135, 1);
				this.tempColor = new spine.Color();
				this.renderer = renderer;
				this.timeKeeper.maxDelta = 9;
				if (!logoImage) {
					var isSafari = navigator.userAgent.indexOf("Safari") > -1;
					var onload_1 = function () { return loaded++; };
					logoImage = new Image();
					logoImage.src = SPINE_LOGO_DATA;
					if (!isSafari)
						logoImage.crossOrigin = "anonymous";
					logoImage.onload = onload_1;
					spinnerImage = new Image();
					spinnerImage.src = SPINNER_DATA;
					if (!isSafari)
						spinnerImage.crossOrigin = "anonymous";
					spinnerImage.onload = onload_1;
				}
			}
			LoadingScreen.prototype.draw = function (complete) {
				if (complete === void 0) { complete = false; }
				if (loaded < 2 || (complete && this.fadeOut > FADE_OUT))
					return;
				this.timeKeeper.update();
				var a = Math.abs(Math.sin(this.timeKeeper.totalTime + 0.25));
				this.angle -= this.timeKeeper.delta * 200 * (1 + 1.5 * Math.pow(a, 5));
				var tempColor = this.tempColor;
				var renderer = this.renderer;
				var canvas = renderer.canvas;
				var gl = renderer.context.gl;
				renderer.resize(webgl.ResizeMode.Expand);
				renderer.camera.position.set(canvas.width / 2, canvas.height / 2, 0);
				renderer.batcher.setBlendMode(gl.ONE, gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
				if (complete) {
					this.fadeOut += this.timeKeeper.delta * (this.timeKeeper.totalTime < 1 ? 2 : 1);
					if (this.fadeOut > FADE_OUT)
						return;
					tempColor.setFromColor(this.backgroundColor);
					a = 1 - this.fadeOut / FADE_OUT;
					a = 1 - (a - 1) * (a - 1);
					tempColor.a *= a;
					if (tempColor.a > 0) {
						renderer.camera.zoom = 1;
						renderer.begin();
						renderer.quad(true, 0, 0, canvas.width, 0, canvas.width, canvas.height, 0, canvas.height, tempColor, tempColor, tempColor, tempColor);
						renderer.end();
					}
				}
				else {
					this.fadeIn += this.timeKeeper.delta;
					if (this.backgroundColor.a > 0) {
						gl.clearColor(this.backgroundColor.r, this.backgroundColor.g, this.backgroundColor.b, this.backgroundColor.a);
						gl.clear(gl.COLOR_BUFFER_BIT);
					}
					a = 1;
				}
				a *= Math.min(this.fadeIn / FADE_IN, 1);
				tempColor.set(a, a, a, a);
				if (!this.logo) {
					this.logo = new webgl.GLTexture(renderer.context, logoImage);
					this.spinner = new webgl.GLTexture(renderer.context, spinnerImage);
				}
				renderer.camera.zoom = Math.max(1, spinnerSize / canvas.height);
				renderer.begin();
				renderer.drawTexture(this.logo, (canvas.width - logoWidth) / 2, (canvas.height - logoHeight) / 2, logoWidth, logoHeight, tempColor);
				renderer.drawTextureRotated(this.spinner, (canvas.width - spinnerSize) / 2, (canvas.height - spinnerSize) / 2, spinnerSize, spinnerSize, spinnerSize / 2, spinnerSize / 2, this.angle, tempColor);
				renderer.end();
			};
			return LoadingScreen;
		}());
		webgl.LoadingScreen = LoadingScreen;
		var SPINNER_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKMAAACjCAYAAADmbK6AAAALKElEQVR42u2de2iW5R/GPzuqcwfnnKfNmafl5tTNHWzqNi3DEMQykcAoJSsySkspjSIk0iD/07Kf4R+FnVBDUTshZGpWUEJaaiWFgZlUFmXmIe3HNXthyebeZ77P9H13ffBG8Y8H7ut7vff93N/7fu4vGGPiFZiez/Qtw9lytJajfzfw9z/j+efPOv7cV8W+lUNY2a8T/ayTCRsWFLJA5rtUO1LLkV5p9LJeJizQiHeqnlOtmVFtdTGrrZkJCxYXsTgaI6r9MY4/UpNItW4mFDaXsTlaM6qVZlBq3UwofFrJp0HMWJ9DvXUzobCznJ1BzFjWlTLrZkJh/TDWBzFjTgo51s2EgnKI0Rrx+FiOWzNzVaym91Syx5qZsGBWb2ZFa0ZN6dbMhAWTcpkUrRmXD2K5NTNhgVbH0Zpxbl/mWjMTFvRIo0e0ZpzcncnWzISKtvmiMWNRJ4qslwmVXRXsas2Ix8ZwzFqZsGFREYtaM+Oaa1ljrUzYkJ9G/ok6TlzKjJWZVFor0y7c1Zu7WjLiqiGsskamXdHopyT4vALmzS9k/t19uHtKHlOSIMn6xAtARjIZ1sFcUSZ0Y4La+G6M18hS2IlCn4a+WoC0JNL0d/dUupdnUj40g6EJ2VEdMnhrOG/p5f/jUXz8SgmvaGU6KpNRNsLVQV0OdXf24s63h/P2gWoOrBjMCr2GJFQnnxnIM3q5P1PPmaYv+4ev4/C6UtbpV2gzXCkgL5W8Bwt48OIc6ul6Tp+s4+SyASxLiI4+PYCn1bHzDZxvaQW6vZzto7MYnQIpNkf7kp5EuozYUroqEjcNKHHd0Tl9mBPN1pk+hFeieGBnBtog7UXjsj9pWg+m6duecw2cay1OC/uxMC47KmP9OIYfoz1YoC20J/rzRG4quTZK2EAyJGs20qwUbYw0aNRmUxtvfUW/uEtNzc1NB1/X8LVyd15hh82F43AvD+VlXcsSJEZa1CQ3ejleAO7oxR3RDP0XN91X4+NXYb8nkv7UNTwV7e0YTdu7I3g33t7tuaEbNwSZpps2fSyvs4M2Tjhot+jb0Xzbltj8r5j/xVt/6Z1Ob93U1ZYO691EhhzchcHeXosVjcNZysyezLw4xRZt05R+fTeuj8vOj+zKyG0j2aZcVVs6v+QalnjrMFZASQYl2nBoSyz06e3j/Xk8rgWYmMvEICu2pm1HOTuc7okV8FgRj0XukwzanhvCc/F+72TjoQjdObN1OFuDLmh0xP+WHtxiI10ukJlCprb4guiv1fP+avZrS1C7NAkliHZjDtZwMMgqbukAltpMlwuMy2FcEBPqvfLLar5Uqi0hBdEwryy+Mv5n6zkbjTBa+dlMlwvUZFETZKGiFM7tvbhdJ3gSVRO0wzIjnxmvl/J6a6JsGMYGrahtpssFeqbR841S3mhN80OjOaSDEdqd6SjaMKgzgzRK7q1ib3PT9sYyNo7JZoyNFNvRcVMZmy7WOvIuryv/Zvdmdt90+nY0bRp3AvROohFwdwW7dTG7RFlbwlqdrbOBYg005NAQmZU0HWt1rXMBH1Xw0dQ8pmqzoaPmdhun7bHZjNVe9qP9eFQfO1VkUmHjhAVUZ1GtnKFSbjrkrPfy4i4UW5t/6ZxM54J0CqxFe81KpGsQyE4h23oYY4wxxhhjjDHGGGOMMcYYY4wxxhhjjDHGGGOMMcYYY4wxxhhjjDHGGGOMMcYYY4wxxhhjjLna+bdOy+IiFquIpGq16Pb79cNYv3IIK/X/ugx+Ui6TVKvYVU9Nc8gX8od8Ir/IN/KPfCQ/yVfyl/6/pfJvLChkQdD6wyqntquCXYuKWJSfRr6D0dEAxV3xlw/khyD+kd/ku/88cHo+09tS3LBpO1HHCVUqcIA6CqB4K+6X6x35L/JM2loXurlWmUmlA5XogOIcK8/If5HncrSWo7F6cKIWPjT/RXGOlWfkv8hzaWsN4uaaysE6WIkOKM6x8oz8F3kusXqo2vxC5jtYiQ4ozrH0TeS5qIZcrB7qkrwdA8U5Vp6R/yLPZV8V+2L14Cl5THGwEh1QnGPlGfkv8lyUlIzFQ1cNYVVHrcjZ0VCcFe9Y+Eb+izy3ceclUl43aFN52DXXssYpnY6a4qFS8ZcP2uIf+e7inRh6pdFrdTGrm8uiHx/L8T2V7NGWzvJBLJ/bl7mTuzO5qBNFDoiJID/IF/KHfCK/yDfyT3O7d/KbfNfS80hNIrU0g9L6HOq1x5iTQo6FNpeLfCQ/yVfyl3xmXYwxxhhjjDHGGGOMMcYYY4wxxhhjjDHGGGOMMcYYY4wxxhhjjDHGGGOMMcYYY4wxxhhjjDHNk9z4JwJ0SqKTdQkbyEwhU393T6V7zzR6pieR3tE1ITeVXImhe6BXDGZFdRbVeank2TBhcaEMr0rwbixj49IBLL2/L/ffmMuNHfqO9tFZjJYBd1ewO3Lx+IcVfKhqna5nHZYR6XFPH+5R3eeI5t9fx/fvjeC9Jdew5OKZKqFR/RDVKL6vL/f9PJafmyvHsL+a/ff24V6NmjZQbGchVbY6UM2BluqHv1rCqzVZ1KQlkZboepCdQvacPsz5bjTfXao+yMEaDt7Wk9tSIMVGig3TejCtJSM2bSpkPjWPqd1S6Zao+lORSYWmgkOjORRNwZqd5ezMSiHLRooNr5XwWjS6/1XHX9vL2T67N7M1iyXa9JCrYjVrS1gbpJyw6hBfsmiNCYT0P9/A+Wj1/6qGr5YNYFlJBiWJogEzezLz/ZG8/9s4fgtSyuvNYbyp1IONFBtu7sHNv4/j9yAxUHWrdaWsG9+N8XHd+YxkMpSy+aySz841cC5oXbmHCnnI74yxAgZ3YbDeB4PEQCOpBpFNZWwa2ZWRcdnxLsl00crtRB0n2lLg8JNRfDKoM4NsolgBSmw/UMADba1+qpmqfyf6x1u/0a/og3I+aEunP6/i86osqmygcGarF4p54dex/Bo0LqfqOfVwIQ/HW5/RSkwV1oN2WLlHTc82TljAwM4M1O5LWwYKZTjibYXNS0N5KcjKTe10PadfLObFuJwK4ozp+UzXDBTUjL+M5ZcBnRkQV53dMIwNQTu6bSTbVEzbi5awuVByd2E/FgaN0Tc1fKOzBHHV2aAdVSdv6s5NNkp7cSH/++xAng2yyHx+CM/H21YhfdPp+0U1X0TbSZnXx8faG9Aop0MS0cToh1p+iLcpOkLj9t/JOk5eqoPHxnDsyf486an5yqCDK7XZ1O4oZ4dWyy3FSXHUAYq47uyYbMZoGmhpG3DlEFb6uNiVBhpyaHhnBO8oJmfqOROJjzIiP43hJ8UxITqqX56S2Hur2KsOnq3nrE6PPNKPRwrSKbAZrjTQNZmuE7oxYXMZmxWbw9dxWFu4W4ezVedOE6qzI7oyYkY+M7TPeWsPbk2UX1qioSN+E3OZqOR2cReKE+qQRFN0Pi7y73g/UawU1KzezJpXwLz5hczX1ueUPKYkNb6GJQZ+j7/aAfRZREsv+quGsMoamXZBW2Gt5eU0alorEzYsKmJRa/m4NdeyxlqZsCGa84DKnVorEzboC7podis69DfIJmwufHMc7famvvmxZiYsKOtKWbRm1OcW1syEBboSJFozLh/EcmtmwgIluaM14/phrLdmJixYXMTiaM24p5I91syEBTphFOR7Y2tmwgJNvUFOr+tov3UzoaAv44KYUatv62ZCoemdhtG0+hzqrZsJBR08DWLG0gxKrZu50qvpxos3U5NItW4mFPp1ot+lPlpq2lYXs9qamVBZUMiC1ox4pJYjvlfStAu6GmTLcLboMtPIV4/6im5fFfuUi9QIap2MiWP+D96R1vPmsD/fAAAAAElFTkSuQmCC";
		var SPINE_LOGO_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKUAAABsCAYAAAALzHKmAAAQ7klEQVR42u3da4xdVRUA4D0zd2Y6nZY59DVtZ1puS9+lzC0YAi2UQ8AAQczFpPgA9VIeIQbirRqJ0cBUBVGjDr/QCKSNRSMmpuODxAdxqhgwxkhFjf6Sxh/+UUNVNGJCzR7uTvas7LXX2q9zzp3em6y0gTN3Zu75utZe5+yztxC9V+/Ve5X9En1Vjd7J6SFbLNF7naPw+l2jh7YHMBWssqMHtlsRdim4qsLtIawaPiHEQOLoNrA9iIkQDnRrVA1qD2LZ8ISoxYqKo13sQAtBWBayQWZUAXGRQM9JjCngDVY0UqJNDbQrMcaGmArdUKpIjbYiQLsCZCyIMQBy8QwnilR4Q5AuCpxFYvRFmBLbEiwKwpsSaWycVQGZBKMrwBjA9BhxDe57u2L2hOoKNCbOrgAZitEVYUxoKSMErQvSxYIzCkguRg5EF4AUhqUlhy/YUKSxcRaKsioYOQhD4I0yYxkR3PcJBcuFysmgsXAWBTMJyBCMIQh9kGGxXIXvexCQXbHGAMrBWTbM2CCpMSMLIxehC77lSJwXGth7M0FzoVJIXXDWQnGmhOkL0ic7YhhtWdGGkAuPAjUWGoF4faCmwBkbZmyUqUCyMqMLRA4+E6IsdTjidUHKBYrh9CnpRcH0ypKxsyOGEYNIIeTCOz91OIJ1QYoB5eAMyZo+MFNnyVTZ0YiRC9EGEMOyAgshxErHsL2XK1gOUgwohtM1a5YNM7Rsu4K0ZkcbRm4mpPBRwFZ5hg9eCqkrUB+csWGGogzNkqEgrdnRhpGLkINudaLgwvVB6oqzCjCTZElb2Y4B0gUjBtEG0ARnDRLjjoG9DwcshtQGlIPTljVjwUySLWNkyRCQVHa0ZUUTRAwgF91a33BEy0VKAcVwwqwZC2bqbOlUurllOxQkJzNyINoAYqjWhYYjWg5SCiiG05Q1U8FMjTIoS8YE6YORi1BHtJ4KIcQEEtTXUWAxpK44YVlPBdO1jCdFWTZIE8bVGEYMIRPcZGBw4HKQcoBiOE1ZMzbMgVQwU6JMAdKEEcuIJogUvg1YCCE2gsCO42DlIIVAKZwpYJaSLVNnSU6XjYHUmxhTmXaFaMO3EYkLqMC+FsLlIoVAKZzMrEnBhJeLfLNlKMrYWRIDCbOkFSSVHbHM6AKRC6/ODUewNqQ+OLlZkxpjUmV8MBbMUJSxyzY3Q1IgTRgxiBRCHdem0KDAUkBh9sRwwjEnAXMMgTnKhFlUtiwkS5rGka4g9SaGgxFmRC7AzTCEEBcyA36dDSsXqAtOLkysjGMwY5XxVChjZ0kuSCo7YlkRQsQQYtDEFsegsLoCxUp7Kpgps2UslFGzJGccSYHUmxhOZqwzEZqAbfUMCisHKIZzgsCpl3MTzMwTpi1bYp2477gyFKUxS7qWbdjY2EBS2dGE0QQRA7gNCyHEdhjE8RhUDlBT9tzgmjWZMFNlSy+Urk1OzCyJlW0XkK4YOQh1cDtcgwBrQmoDWkdgboBZE8mYsJSHlnFbJ+5bwmOPJ7lZkirbC8aRsMvmgtTHjBhGE0QbwJ2egUE1ITVlUC5OmDVdYNrKuN70xM6WoShjlW4464dbtiFIWK6x7GjESEHUUe0iYncnbMdQSE0Z1ITTNOb0hRmjjLtmS9dmJ2rp1jtuKktyyrb6YLEMCUHq2dGG0QQRQ7f72kzc+cJecerne8Wvv7JNPHPvenEkz8Sh3UtFc92QyGt9Yko/HgOLAIUZlItTz5ouMF3KuE+2jFLCQ1D6lm6fLMkBacuOJowYRBUXyfjuHjF3NhdnsfjvAfH6E9vFt9XxKgikEKgJZyyYalzOLeMu2bLbULI6bh+QGwmQ+rgRlumdGEQAao+K56bEL2woVUwOiev0r8OAUjiJrMmFCbvykGxZRAmPitK1dHM7bohyMsuyi/I8f0+e57fJYIKEZXpXo9E4mOf5XTKyLLvCBLETF8uY2SKepkC+dpX4T02Ivepr4HvZcOZ5fmee54fyPL+DmTUhzAs6n4n8bN5dr9f3YdkSg8nsxG0lPBVKVpNjG0/aGhzfLDmRZdnumZmZp8+c+cdZPV555fSr7Xb7s0jJ3i5Pcue4MxKkPPkvvXTqz/B92u32l0wYOzG1fkhcd/py8Rcbyq/vFM/KY1WA95h/3zzP71bfU6JsNpsfgj+P/FlbrdaDGExYyuXvLz8H+DudODH700ajcSM3W6Yu4alQ1spCOTd38jcKocTZbh9+9NixY99XJ8AEUkcpo9W64yH197m5k7+bnZ19QT+J09NHntQhwji/Jg58qi6++ofLxJ8gSFneVw2Ka4QQDfh1Ok4dZavVmtZ/nrm5k7/Vf55O1tRhboUw5+ZOvqyOl5+R/FyOHj32PYVU/tloNG5IXcKrhJIzngwp3fNjomazea/64BuNxts646f50lWv169utw9/DmtqdJQyZFaSJVuV6nq9fqMEof5/vV6/CYBqgJDlee+yAbF/+4i4ZWqZeNfaIfHWzn+Hx0KcEuU9+s8jv3ej0bhVlXOZydX/k0iRMeb8P0D5e6tj8zy/Xb9UJIc56h/yqVOnXul8lmuZ2bJslKmbHG7XrbpCmCXFRLvdfqQD6jTS3Jiy5I4OykM6ADV+1Eu1DmV6evopBORexzDi1L+X/HnGxsb2w3Hm9PSRJ9QxWPOTZdmlKht2hi+w6dkox5bqffI8fye3hDteGqKaHVsHXihKl0tB+h0cY+lute54AGRKDCW89LNTRynHb7ChUWVVjetOnJh9EYBUyPZeNCoOtsbFQwdXi4/esELcd+tq8cCHJ8UXp+viy9efLz7AgamjlKXc1AA1m83DoIRDlFubzeb96hhZLVTlgJ24gttutx+ONa50bHZKRenaeTs1OfpAfnr6yOOdE7EZdNwmlKocntXLNkA5JTGq47Ds+Lf94lWsyfnXleLfnIwJUN4DOnNYwuUxh2A3Ln9XULrfK8t3J27Tu3BVwiOjXJqoAy8UZej1yclGo3GTLN+gu3w+z/P3YaWbQqk3Ne12e4ZC+c8rxWsYytcPiP9RpZxCqWDKnxOiBNlyAUpOnGsoh4tA2Rm8X9xqtT6md5wyZmYe+0YRKL+1S/wYQ3n8zctBl5SBUv5djivfjMOPduIzcizeiYfr9foVvUwZG+XCuzibZKnSceZ5/v4QlKp8y7ElhnJlTeTP7BI/kllRYfzrfvHqFy4UX1vaL/aVlSmROzwbwdS29T2UcEwZF+V8ozM2lu1VY812u/15akypGh3TmFJesJbHHD167IdUxz3YJy5bNySuX1mbvy55CbMLtzU6tjGlsdFptVqfUMc0Go23F4wy1l2dSnbfvpMwVPe9WWVLDsrOJaF9MFu2Wq1PqmNkGce67xiXhTjdNwdlvV6/BgxfbPfBfVCetxi6b9/rlCup65QzM48dl2OjLMv26CibzeZ96sTIzEFdpwQXz9U1yrtVlpR/Zll2Fec65Y6l4pbbx8XHH9kknvzJlPjlHy8Tp29eKT5ou0aJoIT3w3dBlLDzVpfAJEZ1XOdaJZxnOSlvPMjPzxFljIvng914RwebsjYO7uhMyHu46sOfnf3Oz2TXDW6vvYxdFoIXz3Wc8J5zs9n8iOn2IrxTc2BM3Glqdp7dI553uaOjxrhwcob+MyuUpjs6WZZdon8OcigjPx8V+u+GTWFTSWEx3WYcdJ225jNDSE4q0GHCzlueHOyujn6bUWYgeb9ZZUaQPe+GzQ+Gc8+oOGhC+c1d4gfI16n3XDAhQ7+9qE9l01E2Go132GYKyXE1NiFDTcpoNpv3LOYJGWXNErJNW9sEp63p2RKiVPMn1bS1DgxsyhoGdGpmizj+xtXiDYnx7/vFmce3iWdW1cTVGEY4hQ2ZW0nNq8Qm/M6XbXm3S100lwGedFybuvNOibLI+ZS2ceU4eAxiEuvCkfmU8ycToDxETe6FgCBQHeqyAbFvfEhcO7BwDuXFCEbTZF840XeHK0jYcbs2OIGle0mVJ/mmnClEPQqxyTY5I8/zFhif7fSZee4bnrPOU4AssnRXHaVTCTd14dRDY3UbTIiSeFhsN/aMjgnqthFx880rxX3yATL5p3y4LPXzOaBkUyBjZMlYpbtQlIOBD475ZEusjMNSvkXe6VEoJVDkeZ2dzIfIFsRzU+JF2OyM9M9fTC/6SUYOyFQPjQ2nWiUjxnPfw5EeHqMWIqAeIFsAU847lJM2JM6xsewt1OIDLs99P7ZFHNdB/upS8XtPiD7PfLuCXJNolYyyFiNI/Zit65ItrOVafFbHcFohY7hPTN21Tjz4uc3iqfsnxKdX1MTl1OoYRFaMsToGB6Trw2JFP/OdZC2hJZ7ZkrMoAbbSGmelDJ91hFKuJeS7jlBMkJnrAqqJlgMUZS/dArPlGHNdSg5M3xXXtvquuEatvIYtDRhpxbUJuIgqsU5lGWtUploK0KuEU9mSW8YpmFQ556xNuYW7NiW13B+FkMiKHIy+C6eGgBxJvMR0oSv5hi6+z4HJyZoU0M2RVvDlrOQbcxVfX5AhZbuqy0v7ZstYMLHlAVlLTF9ALLbvu9Y5Zylpn/XOsd0ibIvxr2KCLHpp6SCUIdnSZSF+WzfOhem6GD+1KwR3Z4jNjrtDpNoZwmWd8yrupZN6Hx3fbMmFSe0Swdq2ZIPjxk1112Duo8OBGLrBkw/IoncdK2XHsdC9dHz204m50xh3tzFq1zFqtzHXrfCw7OgDsqyNnZLszVijsmXgrmNcmGtS78lIoMX2aJz03fKO2sDJddPQSCDPiQ1DfWBycY6XtXstc2PQKuxgG2McmXTPb9/9vmuJYXKyJrWjbeg+3xPM4O73nWqvbyw7xgZZSJbEUBa157cNJjdr2vb+5iA1YV3HxYscj30PDCEHIgcjtfm8K8hSsmRotkwFk5s1TTghUAopB6xrjHMBBkI0YYTZ0dZlxwLpkiWDULpmy5gwqayZgZNkA7oKQQCxctByYg0XIIEQQuRitGVHblMTA2ShKGPDpC6wu+DEgJqg2rDGDBtAF4Q6RAojp1xXGmSMbImVcR+YWNY04eQCtUG1ofUJ2/uvcETIgUhhdAE5GAlkKShjwHTNmhhODKgJqQ2sC14uOgyfD0IbRF+MlQaZAiZWyn2yJsTJATqGnHQO2Jhh+xlsACFCG0QbRtdyzQFZCZSxYPpmTS7Q5cjJHYNBYIkZpu99HoUQ/o4QIYSIZUZfjJ4ZMjZI32wZBDMU5yhy8pZTULl4XYP5fagMyEVoy4oupTpGduwnkloSlKEwY+AcQU4MhRTD6ovXBRwFzwWgCSEF0QVjJUGmgEllTS5OLlCIlIN1mS9mx/cZ5eLDALpCTI2RAhkTZQqYoTgpoCPECbaBHQ2ETL3PUl98ECAXYijG0OyYAmQoTG7W5ODkAF1CnVgm2JQx4okPA+gCMTbGskBGgRmaOblAh5GTORIrfKFx4VH4EIAxIXIxlg2SBbMvECY3e7oApbDaIgQu5/2HmeEKEINYiwSRi7EQkLFgumZOCuggctKGI4ULZN/vMeSLj0AYMytWEqMLzFg4fYDaoKaC6wvOFR4FkIPQFaILxrJAOsHsc/zlfYDWXE8qF22s8Pz5KHxcgEVALBtjJXBSSEOwFhk1Zgy4hitCT4hVw+gFs8/zwxqIBbUgyK7fcyA0PD9XX4iVxhiC0xdof6STWCsoBmKF7+cVCWFXQYyBMxRpf+STX1b0x45AhN0OMSrOGEirhrY/dfQAdjvS7oy+WCF6r1RIFxXWvlTRg1YVqFWBmxZbD99ig9pt0YPQw9rD1nstVri9V+/Ve3XrS/wfim4P5fIFxLoAAAAASUVORK5CYII=";
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
				var gl = this.context.gl;
				this.srcColorBlend = gl.SRC_ALPHA;
				this.srcAlphaBlend = gl.ONE;
				this.dstBlend = gl.ONE_MINUS_SRC_ALPHA;
			}
			PolygonBatcher.prototype.begin = function (shader) {
				if (this.isDrawing)
					throw new Error("PolygonBatch is already drawing. Call PolygonBatch.end() before calling PolygonBatch.begin()");
				this.drawCalls = 0;
				this.shader = shader;
				this.lastTexture = null;
				this.isDrawing = true;
				var gl = this.context.gl;
				gl.enable(gl.BLEND);
				gl.blendFuncSeparate(this.srcColorBlend, this.dstBlend, this.srcAlphaBlend, this.dstBlend);
			};
			PolygonBatcher.prototype.setBlendMode = function (srcColorBlend, srcAlphaBlend, dstBlend) {
				this.srcColorBlend = srcColorBlend;
				this.srcAlphaBlend = srcAlphaBlend;
				this.dstBlend = dstBlend;
				if (this.isDrawing) {
					this.flush();
					var gl = this.context.gl;
					gl.blendFuncSeparate(srcColorBlend, dstBlend, srcAlphaBlend, dstBlend);
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
				if (this.verticesLength == 0)
					return;
				this.lastTexture.bind();
				this.mesh.draw(this.shader, this.context.gl.TRIANGLES);
				this.verticesLength = 0;
				this.indicesLength = 0;
				this.mesh.setVerticesLength(0);
				this.mesh.setIndicesLength(0);
				this.drawCalls++;
			};
			PolygonBatcher.prototype.end = function () {
				if (!this.isDrawing)
					throw new Error("PolygonBatch is not drawing. Call PolygonBatch.begin() before calling PolygonBatch.end()");
				if (this.verticesLength > 0 || this.indicesLength > 0)
					this.flush();
				this.shader = null;
				this.lastTexture = null;
				this.isDrawing = false;
				var gl = this.context.gl;
				gl.disable(gl.BLEND);
			};
			PolygonBatcher.prototype.getDrawCalls = function () {
				return this.drawCalls;
			};
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
		var quad = [
			0, 0, 1, 1, 1, 1, 0, 0,
			0, 0, 1, 1, 1, 1, 0, 0,
			0, 0, 1, 1, 1, 1, 0, 0,
			0, 0, 1, 1, 1, 1, 0, 0,
		];
		var QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
		var WHITE = new spine.Color(1, 1, 1, 1);
		var SceneRenderer = (function () {
			function SceneRenderer(canvas, context, twoColorTint) {
				if (twoColorTint === void 0) { twoColorTint = true; }
				this.twoColorTint = false;
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
					color = WHITE;
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
					quad[i] = 0;
				}
				this.batcher.draw(texture, quad, QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawTextureUV = function (texture, x, y, width, height, u, v, u2, v2, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = WHITE;
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
					quad[i] = 0;
				}
				this.batcher.draw(texture, quad, QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawTextureRotated = function (texture, x, y, width, height, pivotX, pivotY, angle, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = WHITE;
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
					quad[i] = 0;
				}
				this.batcher.draw(texture, quad, QUAD_TRIANGLES);
			};
			SceneRenderer.prototype.drawRegion = function (region, x, y, width, height, color) {
				if (color === void 0) { color = null; }
				this.enableRenderer(this.batcher);
				if (color === null)
					color = WHITE;
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
					quad[i] = 0;
				}
				this.batcher.draw(region.page.texture, quad, QUAD_TRIANGLES);
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
				var dpr = window.devicePixelRatio || 1;
				var w = Math.round(canvas.clientWidth * dpr);
				var h = Math.round(canvas.clientHeight * dpr);
				if (canvas.width != w || canvas.height != h) {
					canvas.width = w;
					canvas.height = h;
				}
				this.context.gl.viewport(0, 0, canvas.width, canvas.height);
				if (resizeMode === ResizeMode.Expand)
					this.camera.setViewport(w, h);
				else if (resizeMode === ResizeMode.Fit) {
					var sourceWidth = canvas.width, sourceHeight = canvas.height;
					var targetWidth = this.camera.viewportWidth, targetHeight = this.camera.viewportHeight;
					var targetRatio = targetHeight / targetWidth;
					var sourceRatio = sourceHeight / sourceWidth;
					var scale = targetRatio < sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
					this.camera.setViewport(sourceWidth * scale, sourceHeight * scale);
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
				else
					this.activeRenderer = this.skeletonDebugRenderer;
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
				var gl = this.context.gl;
				this.srcColorBlend = gl.SRC_ALPHA;
				this.srcAlphaBlend = gl.ONE;
				this.dstBlend = gl.ONE_MINUS_SRC_ALPHA;
			}
			ShapeRenderer.prototype.begin = function (shader) {
				if (this.isDrawing)
					throw new Error("ShapeRenderer.begin() has already been called");
				this.shader = shader;
				this.vertexIndex = 0;
				this.isDrawing = true;
				var gl = this.context.gl;
				gl.enable(gl.BLEND);
				gl.blendFuncSeparate(this.srcColorBlend, this.dstBlend, this.srcAlphaBlend, this.dstBlend);
			};
			ShapeRenderer.prototype.setBlendMode = function (srcColorBlend, srcAlphaBlend, dstBlend) {
				this.srcColorBlend = srcColorBlend;
				this.srcAlphaBlend = srcAlphaBlend;
				this.dstBlend = dstBlend;
				if (this.isDrawing) {
					this.flush();
					var gl = this.context.gl;
					gl.blendFuncSeparate(srcColorBlend, dstBlend, srcAlphaBlend, dstBlend);
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
				var gl = this.context.gl;
				gl.disable(gl.BLEND);
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
				shapes.setBlendMode(srcFunc, gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
				var bones = skeleton.bones;
				if (this.drawBones) {
					shapes.setColor(this.boneLineColor);
					for (var i = 0, n = bones.length; i < n; i++) {
						var bone = bones[i];
						if (ignoredBones && ignoredBones.indexOf(bone.data.name) > -1)
							continue;
						if (!bone.parent)
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
						texture = region.region.renderObject.page.texture;
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
						texture = mesh.region.renderObject.page.texture;
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
					if (texture) {
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
						if (!slot.darkColor)
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
							batcher.setBlendMode(webgl.WebGLBlendModeConverter.getSourceColorGLBlendMode(blendMode, premultipliedAlpha), webgl.WebGLBlendModeConverter.getSourceAlphaGLBlendMode(blendMode), webgl.WebGLBlendModeConverter.getDestGLBlendMode(blendMode));
						}
						if (clipper.isClipping()) {
							clipper.clipTriangles(renderable.vertices, renderable.numFloats, triangles, triangles.length, uvs, finalColor, darkColor, twoColorTint);
							var clippedVertices = new Float32Array(clipper.clippedVertices);
							var clippedTriangles = clipper.clippedTriangles;
							if (this.vertexEffect) {
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
							if (this.vertexEffect) {
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
				if (contextConfig === void 0) { contextConfig = { alpha: "true" }; }
				this.restorables = new Array();
				if (!((canvasOrContext instanceof WebGLRenderingContext) || (canvasOrContext instanceof WebGL2RenderingContext)))
					this.setupCanvas(canvasOrContext, contextConfig);
				else {
					this.gl = canvasOrContext;
					this.canvas = this.gl.canvas;
				}
			}
			ManagedWebGLRenderingContext.prototype.setupCanvas = function (canvas, contextConfig) {
				var _this = this;
				this.gl = (canvas.getContext("webgl2", contextConfig) || canvas.getContext("webgl", contextConfig));
				this.canvas = canvas;
				canvas.addEventListener("webglcontextlost", function (e) {
					var event = e;
					if (e)
						e.preventDefault();
				});
				canvas.addEventListener("webglcontextrestored", function (e) {
					for (var i = 0, n = _this.restorables.length; i < n; i++)
						_this.restorables[i].restore();
				});
			};
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
		var ONE = 1;
		var ONE_MINUS_SRC_COLOR = 0x0301;
		var SRC_ALPHA = 0x0302;
		var ONE_MINUS_SRC_ALPHA = 0x0303;
		var ONE_MINUS_DST_ALPHA = 0x0305;
		var DST_COLOR = 0x0306;
		var WebGLBlendModeConverter = (function () {
			function WebGLBlendModeConverter() {
			}
			WebGLBlendModeConverter.getDestGLBlendMode = function (blendMode) {
				switch (blendMode) {
					case spine.BlendMode.Normal: return ONE_MINUS_SRC_ALPHA;
					case spine.BlendMode.Additive: return ONE;
					case spine.BlendMode.Multiply: return ONE_MINUS_SRC_ALPHA;
					case spine.BlendMode.Screen: return ONE_MINUS_SRC_ALPHA;
					default: throw new Error("Unknown blend mode: " + blendMode);
				}
			};
			WebGLBlendModeConverter.getSourceColorGLBlendMode = function (blendMode, premultipliedAlpha) {
				if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
				switch (blendMode) {
					case spine.BlendMode.Normal: return premultipliedAlpha ? ONE : SRC_ALPHA;
					case spine.BlendMode.Additive: return premultipliedAlpha ? ONE : SRC_ALPHA;
					case spine.BlendMode.Multiply: return DST_COLOR;
					case spine.BlendMode.Screen: return ONE;
					default: throw new Error("Unknown blend mode: " + blendMode);
				}
			};
			WebGLBlendModeConverter.getSourceAlphaGLBlendMode = function (blendMode) {
				switch (blendMode) {
					case spine.BlendMode.Normal: return ONE;
					case spine.BlendMode.Additive: return ONE;
					case spine.BlendMode.Multiply: return ONE_MINUS_SRC_ALPHA;
					case spine.BlendMode.Screen: return ONE_MINUS_SRC_COLOR;
					default: throw new Error("Unknown blend mode: " + blendMode);
				}
			};
			return WebGLBlendModeConverter;
		}());
		webgl.WebGLBlendModeConverter = WebGLBlendModeConverter;
	})(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
//# sourceMappingURL=spine-webgl.js.map