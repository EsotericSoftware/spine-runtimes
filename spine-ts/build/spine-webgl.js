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
            this.curves = new Array((frameCount - 1) * CurveTimeline.BEZIER_SIZE);
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
        /** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
         * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
         * the difference between the keyframe's values. */
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
                    var prevX, prevY;
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
            return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
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
            this.frames = new Array(frameCount << 1);
        }
        /** Sets the time and angle of the specified keyframe. */
        RotateTimeline.prototype.setFrame = function (frameIndex, time, degrees) {
            frameIndex <<= 1;
            this.frames[frameIndex] = time;
            this.frames[frameIndex + RotateTimeline.ROTATION] = degrees;
        };
        RotateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
            var frames = this.frames;
            if (time < frames[0])
                return; // Time is before first frame.
            var bone = skeleton.bones.get(this.boneIndex);
            if (time >= frames[frames.length - RotateTimeline.ENTRIES]) {
                var amount_1 = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION] - bone.rotation;
                while (amount_1 > 180)
                    amount_1 -= 360;
                while (amount_1 < -180)
                    amount_1 += 360;
                bone.rotation += amount_1 * alpha;
                return;
            }
            // Interpolate between the previous frame and the current frame.
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
            this.frames = new Array(frameCount * TranslateTimeline.ENTRIES);
        }
        /** Sets the time and value of the specified keyframe. */
        TranslateTimeline.prototype.setFrame = function (frameIndex, time, x, y) {
            frameIndex *= TranslateTimeline.ENTRIES;
            this.frames[frameIndex] = time;
            this.frames[frameIndex + TranslateTimeline.X] = x;
            this.frames[frameIndex + TranslateTimeline.Y] = y;
        };
        TranslateTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
            var frames = this.frames;
            if (time < frames[0])
                return; // Time is before first frame.
            var bone = skeleton.bones.get(this.boneIndex);
            if (time >= frames[frames.length - TranslateTimeline.ENTRIES]) {
                bone.x += (bone.data.x + frames[frames.length + TranslateTimeline.PREV_X] - bone.x) * alpha;
                bone.y += (bone.data.y + frames[frames.length + TranslateTimeline.PREV_Y] - bone.y) * alpha;
                return;
            }
            // Interpolate between the previous frame and the current frame.
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
                return; // Time is before first frame.
            var bone = skeleton.bones.get(this.boneIndex);
            if (time >= frames[frames.length - ScaleTimeline.ENTRIES]) {
                bone.scaleX += (bone.data.scaleX * frames[frames.length + ScaleTimeline.PREV_X] - bone.scaleX) * alpha;
                bone.scaleY += (bone.data.scaleY * frames[frames.length + ScaleTimeline.PREV_Y] - bone.scaleY) * alpha;
                return;
            }
            // Interpolate between the previous frame and the current frame.
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
                return; // Time is before first frame.
            var bone = skeleton.bones.get(this.boneIndex);
            if (time >= frames[frames.length - ShearTimeline.ENTRIES]) {
                bone.shearX += (bone.data.shearX + frames[frames.length + ShearTimeline.PREV_X] - bone.shearX) * alpha;
                bone.shearY += (bone.data.shearY + frames[frames.length + ShearTimeline.PREV_Y] - bone.shearY) * alpha;
                return;
            }
            // Interpolate between the previous frame and the current frame.
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
            this.frames = new Array(frameCount * ColorTimeline.ENTRIES);
        }
        /** Sets the time and value of the specified keyframe. */
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
                return; // Time is before first frame.
            var r = 0, g = 0, b = 0, a = 0;
            if (time >= frames[frames.length - ColorTimeline.ENTRIES]) {
                var i = frames.length;
                r = frames[i + ColorTimeline.PREV_R];
                g = frames[i + ColorTimeline.PREV_G];
                b = frames[i + ColorTimeline.PREV_B];
                a = frames[i + ColorTimeline.PREV_A];
            }
            else {
                // Interpolate between the previous frame and the current frame.
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
            var color = skeleton.slots.get(this.slotIndex).color;
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
            this.frames = new Array(frameCount);
            this.attachmentNames = new Array(frameCount);
        }
        AttachmentTimeline.prototype.getFrameCount = function () {
            return this.frames.length;
        };
        /** Sets the time and value of the specified keyframe. */
        AttachmentTimeline.prototype.setFrame = function (frameIndex, time, attachmentName) {
            this.frames[frameIndex] = time;
            this.attachmentNames[frameIndex] = attachmentName;
        };
        AttachmentTimeline.prototype.apply = function (skeleton, lastTime, time, events, alpha) {
            var frames = this.frames;
            if (time < frames[0])
                return; // Time is before first frame.
            var frameIndex = 0;
            if (time >= frames[frames.length - 1])
                frameIndex = frames.length - 1;
            else
                frameIndex = Animation.binarySearch(frames, time, 1) - 1;
            var attachmentName = this.attachmentNames[frameIndex];
            skeleton.slots.get(this.slotIndex)
                .setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
        };
        return AttachmentTimeline;
    }());
    spine.AttachmentTimeline = AttachmentTimeline;
    var EventTimeline = (function () {
        function EventTimeline(frameCount) {
            this.frames = new Array(frameCount);
            this.events = new Array(frameCount);
        }
        EventTimeline.prototype.getFrameCount = function () {
            return frames.length;
        };
        /** Sets the time of the specified keyframe. */
        EventTimeline.prototype.setFrame = function (frameIndex, event) {
            this.frames[frameIndex] = event.time;
            this.events[frameIndex] = event;
        };
        /** Fires events for frames > lastTime and <= time. */
        EventTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
            if (firedEvents == null)
                return;
            var frames = this.frames;
            var frameCount = frames.length;
            if (lastTime > time) {
                this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha);
                lastTime = -1;
            }
            else if (lastTime >= frames[frameCount - 1])
                return;
            if (time < frames[0])
                return; // Time is before first frame.
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
            this.frames = new Array(frameCount);
            this.drawOrders = new Array(frameCount);
        }
        DrawOrderTimeline.prototype.getFrameCount = function () {
            return frames.length;
        };
        /** Sets the time of the specified keyframe.
         * @param drawOrder May be null to use bind pose draw order. */
        DrawOrderTimeline.prototype.setFrame = function (frameIndex, time, drawOrder) {
            this.frames[frameIndex] = time;
            this.drawOrders[frameIndex] = drawOrder;
        };
        DrawOrderTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
            var frames = this.frames;
            if (time < frames[0])
                return; // Time is before first frame.
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
            this.frames = new Array(frameCount);
            this.frameVertices = new Array(frameCount);
        }
        /** Sets the time of the specified keyframe. */
        DeformTimeline.prototype.setFrame = function (frameIndex, time, vertices) {
            this.frames[frameIndex] = time;
            this.frameVertices[frameIndex] = vertices;
        };
        DeformTimeline.prototype.apply = function (skeleton, lastTime, time, firedEvents, alpha) {
            var slot = skeleton.slots.get(this.slotIndex);
            var slotAttachment = slot.attachment;
            // FIXME
            // if (!(slotAttachment instanceof VertexAttachment) || !((VertexAttachment)slotAttachment).applyDeform(attachment)) return;
            var frames = this.frames;
            if (time < frames[0])
                return; // Time is before first frame.
            var frameVertices = this.frameVertices;
            var vertexCount = frameVertices[0].length;
            var verticesArray = slot.getAttachmentVertices();
            if (verticesArray.length != vertexCount)
                alpha = 1; // Don't mix from uninitialized slot vertices.
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
            // Interpolate between the previous frame and the current frame.
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
    var BoneData = (function () {
        function BoneData() {
            this.x = 0;
            this.y = 0;
            this.rotation = 0;
            this.scaleX = 1;
            this.scaleY = 1;
            this.shearX = 0;
            this.shearY = 0;
            this.inheritRotation = true;
            this.inheritScale = true;
        }
        BoneData.prototype.BoneData = function (index, name, parent) {
            if (index < 0)
                throw new Error("index must be >= 0.");
            if (name == null)
                throw new Error("name cannot be null.");
            this.index = index;
            this.name = name;
            this.parent = parent;
        };
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
        function EventData() {
        }
        return EventData;
    }());
    spine.EventData = EventData;
})(spine || (spine = {}));
var spine;
(function (spine) {
    var IkConstraintData = (function () {
        function IkConstraintData() {
            this.bones = new Array();
            this.bendDirection = 1;
            this.mix = 1;
        }
        return IkConstraintData;
    }());
    spine.IkConstraintData = IkConstraintData;
})(spine || (spine = {}));
var spine;
(function (spine) {
    var PathConstraintData = (function () {
        function PathConstraintData() {
            this.bones = new Array();
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
    var SkeletonData = (function () {
        function SkeletonData() {
            this.bones = new Array(); // Ordered parents first.
            this.slots = new Array(); // Setup pose draw order.
            this.skins = new Array();
            this.events = new Array();
            this.animations = new Array();
            this.ikConstraints = new Array();
            this.transformConstraints = new Array();
            this.pathConstraints = new Array();
        }
        return SkeletonData;
    }());
    spine.SkeletonData = SkeletonData;
})(spine || (spine = {}));
var spine;
(function (spine) {
    var SlotData = (function () {
        function SlotData() {
            this.color = new spine.Color(1, 1, 1, 1);
        }
        return SlotData;
    }());
    spine.SlotData = SlotData;
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
        };
        Color.prototype.add = function (r, g, b, a) {
            this.r += r;
            this.g += g;
            this.b += b;
            this.a += a;
            this.clamp();
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
        Utils.setArraySize = function (array, size) {
            var oldSize = array.length;
            array.length = size;
            if (oldSize < size) {
                for (var i = oldSize; i < size; i++)
                    array[i] = 0;
            }
            return array;
        };
        return Utils;
    }());
    spine.Utils = Utils;
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var AssetManager = (function () {
            function AssetManager() {
                this._assets = {};
                this._errors = {};
                this._toLoad = 0;
                this._loaded = 0;
            }
            AssetManager.prototype.loadText = function (path, success, error) {
                var _this = this;
                this._toLoad++;
                var request = new XMLHttpRequest();
                request.onreadystatechange = function () {
                    if (request.readyState == XMLHttpRequest.DONE) {
                        if (request.status >= 200 && request.status < 300) {
                            if (success)
                                success(path, request.responseText);
                            _this._assets[path] = request.responseText;
                        }
                        else {
                            if (error)
                                error(path, "Couldn't load text " + path + ": status " + request.status + ", " + request.responseBody);
                            _this._errors[path] = "Couldn't load text " + path + ": status " + request.status + ", " + request.responseBody;
                        }
                        _this._toLoad--;
                        _this._loaded++;
                    }
                };
                request.open("GET", path, true);
                request.send();
            };
            AssetManager.prototype.loadTexture = function (path, success, error) {
                var _this = this;
                this._toLoad++;
                var img = new Image();
                img.src = path;
                img.onload = function (ev) {
                    if (success)
                        success(path, img);
                    var texture = new webgl.Texture(img);
                    _this._assets[path] = texture;
                    _this._toLoad--;
                    _this._loaded++;
                };
                img.onerror = function (ev) {
                    _this._errors[path] = "Couldn't load image " + path;
                    _this._toLoad--;
                    _this._loaded++;
                };
            };
            AssetManager.prototype.get = function (path) {
                return this._assets[path];
            };
            AssetManager.prototype.remove = function (path) {
                var asset = this._assets[path];
                if (asset instanceof webgl.Texture) {
                    asset.dispose();
                }
                this._assets[path] = null;
            };
            AssetManager.prototype.removeAll = function () {
                for (var key in this._assets) {
                    var asset = this._assets[key];
                    if (asset instanceof webgl.Texture)
                        asset.dispose();
                }
                this._assets = {};
            };
            AssetManager.prototype.isLoadingComplete = function () {
                return this._toLoad == 0;
            };
            AssetManager.prototype.toLoad = function () {
                return this._toLoad;
            };
            AssetManager.prototype.loaded = function () {
                return this._loaded;
            };
            AssetManager.prototype.dispose = function () {
                this.removeAll();
            };
            return AssetManager;
        }());
        webgl.AssetManager = AssetManager;
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
                this.values[webgl.M00] = 1;
                this.values[webgl.M11] = 1;
                this.values[webgl.M22] = 1;
                this.values[webgl.M33] = 1;
            }
            Matrix4.prototype.set = function (values) {
                this.values.set(values);
                return this;
            };
            Matrix4.prototype.transpose = function () {
                this.temp[webgl.M00] = this.values[webgl.M00];
                this.temp[webgl.M01] = this.values[webgl.M10];
                this.temp[webgl.M02] = this.values[webgl.M20];
                this.temp[webgl.M03] = this.values[webgl.M30];
                this.temp[webgl.M10] = this.values[webgl.M01];
                this.temp[webgl.M11] = this.values[webgl.M11];
                this.temp[webgl.M12] = this.values[webgl.M21];
                this.temp[webgl.M13] = this.values[webgl.M31];
                this.temp[webgl.M20] = this.values[webgl.M02];
                this.temp[webgl.M21] = this.values[webgl.M12];
                this.temp[webgl.M22] = this.values[webgl.M22];
                this.temp[webgl.M23] = this.values[webgl.M32];
                this.temp[webgl.M30] = this.values[webgl.M03];
                this.temp[webgl.M31] = this.values[webgl.M13];
                this.temp[webgl.M32] = this.values[webgl.M23];
                this.temp[webgl.M33] = this.values[webgl.M33];
                return this.set(this.temp);
            };
            Matrix4.prototype.identity = function () {
                this.values[webgl.M00] = 1;
                this.values[webgl.M01] = 0;
                this.values[webgl.M02] = 0;
                this.values[webgl.M03] = 0;
                this.values[webgl.M10] = 0;
                this.values[webgl.M11] = 1;
                this.values[webgl.M12] = 0;
                this.values[webgl.M13] = 0;
                this.values[webgl.M20] = 0;
                this.values[webgl.M21] = 0;
                this.values[webgl.M22] = 1;
                this.values[webgl.M23] = 0;
                this.values[webgl.M30] = 0;
                this.values[webgl.M31] = 0;
                this.values[webgl.M32] = 0;
                this.values[webgl.M33] = 1;
                return this;
            };
            Matrix4.prototype.invert = function () {
                var l_det = this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M11]
                    * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M10] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M10]
                    * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M13] + this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M02] * this.values[webgl.M13]
                    + this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M32]
                    * this.values[webgl.M13] + this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M13] + this.values[webgl.M30] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M10] * this.values[webgl.M31]
                    * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M10]
                    * this.values[webgl.M01] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M33]
                    + this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M33] + this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M12]
                    * this.values[webgl.M33] - this.values[webgl.M10] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M33];
                if (l_det == 0)
                    throw new Error("non-invertible matrix");
                var inv_det = 1.0 / l_det;
                this.temp[webgl.M00] = this.values[webgl.M12] * this.values[webgl.M23] * this.values[webgl.M31] - this.values[webgl.M13] * this.values[webgl.M22] * this.values[webgl.M31] + this.values[webgl.M13] * this.values[webgl.M21] * this.values[webgl.M32] - this.values[webgl.M11]
                    * this.values[webgl.M23] * this.values[webgl.M32] - this.values[webgl.M12] * this.values[webgl.M21] * this.values[webgl.M33] + this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M01] = this.values[webgl.M03] * this.values[webgl.M22] * this.values[webgl.M31] - this.values[webgl.M02] * this.values[webgl.M23] * this.values[webgl.M31] - this.values[webgl.M03] * this.values[webgl.M21] * this.values[webgl.M32] + this.values[webgl.M01]
                    * this.values[webgl.M23] * this.values[webgl.M32] + this.values[webgl.M02] * this.values[webgl.M21] * this.values[webgl.M33] - this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M02] = this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M31] - this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M31] + this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M32] - this.values[webgl.M01]
                    * this.values[webgl.M13] * this.values[webgl.M32] - this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M33] + this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M33];
                this.temp[webgl.M03] = this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M21] - this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M21] - this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M22] + this.values[webgl.M01]
                    * this.values[webgl.M13] * this.values[webgl.M22] + this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M23] - this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M23];
                this.temp[webgl.M10] = this.values[webgl.M13] * this.values[webgl.M22] * this.values[webgl.M30] - this.values[webgl.M12] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M13] * this.values[webgl.M20] * this.values[webgl.M32] + this.values[webgl.M10]
                    * this.values[webgl.M23] * this.values[webgl.M32] + this.values[webgl.M12] * this.values[webgl.M20] * this.values[webgl.M33] - this.values[webgl.M10] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M11] = this.values[webgl.M02] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M22] * this.values[webgl.M30] + this.values[webgl.M03] * this.values[webgl.M20] * this.values[webgl.M32] - this.values[webgl.M00]
                    * this.values[webgl.M23] * this.values[webgl.M32] - this.values[webgl.M02] * this.values[webgl.M20] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M12] = this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M30] - this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M32] + this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M32] + this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M12] * this.values[webgl.M33];
                this.temp[webgl.M13] = this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M20] - this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M20] + this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M22] - this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M22] - this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M23] + this.values[webgl.M00] * this.values[webgl.M12] * this.values[webgl.M23];
                this.temp[webgl.M20] = this.values[webgl.M11] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M13] * this.values[webgl.M21] * this.values[webgl.M30] + this.values[webgl.M13] * this.values[webgl.M20] * this.values[webgl.M31] - this.values[webgl.M10]
                    * this.values[webgl.M23] * this.values[webgl.M31] - this.values[webgl.M11] * this.values[webgl.M20] * this.values[webgl.M33] + this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M33];
                this.temp[webgl.M21] = this.values[webgl.M03] * this.values[webgl.M21] * this.values[webgl.M30] - this.values[webgl.M01] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M20] * this.values[webgl.M31] + this.values[webgl.M00]
                    * this.values[webgl.M23] * this.values[webgl.M31] + this.values[webgl.M01] * this.values[webgl.M20] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M33];
                this.temp[webgl.M22] = this.values[webgl.M01] * this.values[webgl.M13] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M30] + this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M31] - this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M31] - this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M33];
                this.temp[webgl.M23] = this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M20] - this.values[webgl.M01] * this.values[webgl.M13] * this.values[webgl.M20] - this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M21] + this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M21] + this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M23] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M23];
                this.temp[webgl.M30] = this.values[webgl.M12] * this.values[webgl.M21] * this.values[webgl.M30] - this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M30] - this.values[webgl.M12] * this.values[webgl.M20] * this.values[webgl.M31] + this.values[webgl.M10]
                    * this.values[webgl.M22] * this.values[webgl.M31] + this.values[webgl.M11] * this.values[webgl.M20] * this.values[webgl.M32] - this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M32];
                this.temp[webgl.M31] = this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M30] - this.values[webgl.M02] * this.values[webgl.M21] * this.values[webgl.M30] + this.values[webgl.M02] * this.values[webgl.M20] * this.values[webgl.M31] - this.values[webgl.M00]
                    * this.values[webgl.M22] * this.values[webgl.M31] - this.values[webgl.M01] * this.values[webgl.M20] * this.values[webgl.M32] + this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M32];
                this.temp[webgl.M32] = this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M30] - this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M30] - this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M31] + this.values[webgl.M00]
                    * this.values[webgl.M12] * this.values[webgl.M31] + this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M32] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M32];
                this.temp[webgl.M33] = this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M20] - this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M20] + this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M21] - this.values[webgl.M00]
                    * this.values[webgl.M12] * this.values[webgl.M21] - this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M22] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M22];
                this.values[webgl.M00] = this.temp[webgl.M00] * inv_det;
                this.values[webgl.M01] = this.temp[webgl.M01] * inv_det;
                this.values[webgl.M02] = this.temp[webgl.M02] * inv_det;
                this.values[webgl.M03] = this.temp[webgl.M03] * inv_det;
                this.values[webgl.M10] = this.temp[webgl.M10] * inv_det;
                this.values[webgl.M11] = this.temp[webgl.M11] * inv_det;
                this.values[webgl.M12] = this.temp[webgl.M12] * inv_det;
                this.values[webgl.M13] = this.temp[webgl.M13] * inv_det;
                this.values[webgl.M20] = this.temp[webgl.M20] * inv_det;
                this.values[webgl.M21] = this.temp[webgl.M21] * inv_det;
                this.values[webgl.M22] = this.temp[webgl.M22] * inv_det;
                this.values[webgl.M23] = this.temp[webgl.M23] * inv_det;
                this.values[webgl.M30] = this.temp[webgl.M30] * inv_det;
                this.values[webgl.M31] = this.temp[webgl.M31] * inv_det;
                this.values[webgl.M32] = this.temp[webgl.M32] * inv_det;
                this.values[webgl.M33] = this.temp[webgl.M33] * inv_det;
                return this;
            };
            Matrix4.prototype.determinant = function () {
                return this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M11]
                    * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M10] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M10]
                    * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M13] + this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M02] * this.values[webgl.M13]
                    + this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M32]
                    * this.values[webgl.M13] + this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M13] + this.values[webgl.M30] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M10] * this.values[webgl.M31]
                    * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M10]
                    * this.values[webgl.M01] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M33]
                    + this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M33] + this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M12]
                    * this.values[webgl.M33] - this.values[webgl.M10] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M33];
            };
            Matrix4.prototype.translate = function (x, y, z) {
                this.values[webgl.M03] += x;
                this.values[webgl.M13] += y;
                this.values[webgl.M23] += z;
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
                this.values[webgl.M00] = l_fd / aspectRatio;
                this.values[webgl.M10] = 0;
                this.values[webgl.M20] = 0;
                this.values[webgl.M30] = 0;
                this.values[webgl.M01] = 0;
                this.values[webgl.M11] = l_fd;
                this.values[webgl.M21] = 0;
                this.values[webgl.M31] = 0;
                this.values[webgl.M02] = 0;
                this.values[webgl.M12] = 0;
                this.values[webgl.M22] = l_a1;
                this.values[webgl.M32] = -1;
                this.values[webgl.M03] = 0;
                this.values[webgl.M13] = 0;
                this.values[webgl.M23] = l_a2;
                this.values[webgl.M33] = 0;
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
                this.values[webgl.M00] = x_orth;
                this.values[webgl.M10] = 0;
                this.values[webgl.M20] = 0;
                this.values[webgl.M30] = 0;
                this.values[webgl.M01] = 0;
                this.values[webgl.M11] = y_orth;
                this.values[webgl.M21] = 0;
                this.values[webgl.M31] = 0;
                this.values[webgl.M02] = 0;
                this.values[webgl.M12] = 0;
                this.values[webgl.M22] = z_orth;
                this.values[webgl.M32] = 0;
                this.values[webgl.M03] = tx;
                this.values[webgl.M13] = ty;
                this.values[webgl.M23] = tz;
                this.values[webgl.M33] = 1;
                return this;
            };
            Matrix4.prototype.multiply = function (matrix) {
                this.temp[webgl.M00] = this.values[webgl.M00] * matrix.values[webgl.M00] + this.values[webgl.M01] * matrix.values[webgl.M10] + this.values[webgl.M02] * matrix.values[webgl.M20] + this.values[webgl.M03]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M01] = this.values[webgl.M00] * matrix.values[webgl.M01] + this.values[webgl.M01] * matrix.values[webgl.M11] + this.values[webgl.M02] * matrix.values[webgl.M21] + this.values[webgl.M03]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M02] = this.values[webgl.M00] * matrix.values[webgl.M02] + this.values[webgl.M01] * matrix.values[webgl.M12] + this.values[webgl.M02] * matrix.values[webgl.M22] + this.values[webgl.M03]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M03] = this.values[webgl.M00] * matrix.values[webgl.M03] + this.values[webgl.M01] * matrix.values[webgl.M13] + this.values[webgl.M02] * matrix.values[webgl.M23] + this.values[webgl.M03]
                    * matrix.values[webgl.M33];
                this.temp[webgl.M10] = this.values[webgl.M10] * matrix.values[webgl.M00] + this.values[webgl.M11] * matrix.values[webgl.M10] + this.values[webgl.M12] * matrix.values[webgl.M20] + this.values[webgl.M13]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M11] = this.values[webgl.M10] * matrix.values[webgl.M01] + this.values[webgl.M11] * matrix.values[webgl.M11] + this.values[webgl.M12] * matrix.values[webgl.M21] + this.values[webgl.M13]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M12] = this.values[webgl.M10] * matrix.values[webgl.M02] + this.values[webgl.M11] * matrix.values[webgl.M12] + this.values[webgl.M12] * matrix.values[webgl.M22] + this.values[webgl.M13]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M13] = this.values[webgl.M10] * matrix.values[webgl.M03] + this.values[webgl.M11] * matrix.values[webgl.M13] + this.values[webgl.M12] * matrix.values[webgl.M23] + this.values[webgl.M13]
                    * matrix.values[webgl.M33];
                this.temp[webgl.M20] = this.values[webgl.M20] * matrix.values[webgl.M00] + this.values[webgl.M21] * matrix.values[webgl.M10] + this.values[webgl.M22] * matrix.values[webgl.M20] + this.values[webgl.M23]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M21] = this.values[webgl.M20] * matrix.values[webgl.M01] + this.values[webgl.M21] * matrix.values[webgl.M11] + this.values[webgl.M22] * matrix.values[webgl.M21] + this.values[webgl.M23]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M22] = this.values[webgl.M20] * matrix.values[webgl.M02] + this.values[webgl.M21] * matrix.values[webgl.M12] + this.values[webgl.M22] * matrix.values[webgl.M22] + this.values[webgl.M23]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M23] = this.values[webgl.M20] * matrix.values[webgl.M03] + this.values[webgl.M21] * matrix.values[webgl.M13] + this.values[webgl.M22] * matrix.values[webgl.M23] + this.values[webgl.M23]
                    * matrix.values[webgl.M33];
                this.temp[webgl.M30] = this.values[webgl.M30] * matrix.values[webgl.M00] + this.values[webgl.M31] * matrix.values[webgl.M10] + this.values[webgl.M32] * matrix.values[webgl.M20] + this.values[webgl.M33]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M31] = this.values[webgl.M30] * matrix.values[webgl.M01] + this.values[webgl.M31] * matrix.values[webgl.M11] + this.values[webgl.M32] * matrix.values[webgl.M21] + this.values[webgl.M33]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M32] = this.values[webgl.M30] * matrix.values[webgl.M02] + this.values[webgl.M31] * matrix.values[webgl.M12] + this.values[webgl.M32] * matrix.values[webgl.M22] + this.values[webgl.M33]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M33] = this.values[webgl.M30] * matrix.values[webgl.M03] + this.values[webgl.M31] * matrix.values[webgl.M13] + this.values[webgl.M32] * matrix.values[webgl.M23] + this.values[webgl.M33]
                    * matrix.values[webgl.M33];
                return this.set(this.temp);
            };
            Matrix4.prototype.multiplyLeft = function (matrix) {
                this.temp[webgl.M00] = matrix.values[webgl.M00] * this.values[webgl.M00] + matrix.values[webgl.M01] * this.values[webgl.M10] + matrix.values[webgl.M02] * this.values[webgl.M20] + matrix.values[webgl.M03]
                    * this.values[webgl.M30];
                this.temp[webgl.M01] = matrix.values[webgl.M00] * this.values[webgl.M01] + matrix.values[webgl.M01] * this.values[webgl.M11] + matrix.values[webgl.M02] * this.values[webgl.M21] + matrix.values[webgl.M03]
                    * this.values[webgl.M31];
                this.temp[webgl.M02] = matrix.values[webgl.M00] * this.values[webgl.M02] + matrix.values[webgl.M01] * this.values[webgl.M12] + matrix.values[webgl.M02] * this.values[webgl.M22] + matrix.values[webgl.M03]
                    * this.values[webgl.M32];
                this.temp[webgl.M03] = matrix.values[webgl.M00] * this.values[webgl.M03] + matrix.values[webgl.M01] * this.values[webgl.M13] + matrix.values[webgl.M02] * this.values[webgl.M23] + matrix.values[webgl.M03]
                    * this.values[webgl.M33];
                this.temp[webgl.M10] = matrix.values[webgl.M10] * this.values[webgl.M00] + matrix.values[webgl.M11] * this.values[webgl.M10] + matrix.values[webgl.M12] * this.values[webgl.M20] + matrix.values[webgl.M13]
                    * this.values[webgl.M30];
                this.temp[webgl.M11] = matrix.values[webgl.M10] * this.values[webgl.M01] + matrix.values[webgl.M11] * this.values[webgl.M11] + matrix.values[webgl.M12] * this.values[webgl.M21] + matrix.values[webgl.M13]
                    * this.values[webgl.M31];
                this.temp[webgl.M12] = matrix.values[webgl.M10] * this.values[webgl.M02] + matrix.values[webgl.M11] * this.values[webgl.M12] + matrix.values[webgl.M12] * this.values[webgl.M22] + matrix.values[webgl.M13]
                    * this.values[webgl.M32];
                this.temp[webgl.M13] = matrix.values[webgl.M10] * this.values[webgl.M03] + matrix.values[webgl.M11] * this.values[webgl.M13] + matrix.values[webgl.M12] * this.values[webgl.M23] + matrix.values[webgl.M13]
                    * this.values[webgl.M33];
                this.temp[webgl.M20] = matrix.values[webgl.M20] * this.values[webgl.M00] + matrix.values[webgl.M21] * this.values[webgl.M10] + matrix.values[webgl.M22] * this.values[webgl.M20] + matrix.values[webgl.M23]
                    * this.values[webgl.M30];
                this.temp[webgl.M21] = matrix.values[webgl.M20] * this.values[webgl.M01] + matrix.values[webgl.M21] * this.values[webgl.M11] + matrix.values[webgl.M22] * this.values[webgl.M21] + matrix.values[webgl.M23]
                    * this.values[webgl.M31];
                this.temp[webgl.M22] = matrix.values[webgl.M20] * this.values[webgl.M02] + matrix.values[webgl.M21] * this.values[webgl.M12] + matrix.values[webgl.M22] * this.values[webgl.M22] + matrix.values[webgl.M23]
                    * this.values[webgl.M32];
                this.temp[webgl.M23] = matrix.values[webgl.M20] * this.values[webgl.M03] + matrix.values[webgl.M21] * this.values[webgl.M13] + matrix.values[webgl.M22] * this.values[webgl.M23] + matrix.values[webgl.M23]
                    * this.values[webgl.M33];
                this.temp[webgl.M30] = matrix.values[webgl.M30] * this.values[webgl.M00] + matrix.values[webgl.M31] * this.values[webgl.M10] + matrix.values[webgl.M32] * this.values[webgl.M20] + matrix.values[webgl.M33]
                    * this.values[webgl.M30];
                this.temp[webgl.M31] = matrix.values[webgl.M30] * this.values[webgl.M01] + matrix.values[webgl.M31] * this.values[webgl.M11] + matrix.values[webgl.M32] * this.values[webgl.M21] + matrix.values[webgl.M33]
                    * this.values[webgl.M31];
                this.temp[webgl.M32] = matrix.values[webgl.M30] * this.values[webgl.M02] + matrix.values[webgl.M31] * this.values[webgl.M12] + matrix.values[webgl.M32] * this.values[webgl.M22] + matrix.values[webgl.M33]
                    * this.values[webgl.M32];
                this.temp[webgl.M33] = matrix.values[webgl.M30] * this.values[webgl.M03] + matrix.values[webgl.M31] * this.values[webgl.M13] + matrix.values[webgl.M32] * this.values[webgl.M23] + matrix.values[webgl.M33]
                    * this.values[webgl.M33];
                return this.set(this.temp);
            };
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
            function Mesh(_attributes, maxVertices, maxIndices) {
                this._attributes = _attributes;
                this._verticesLength = 0;
                this._dirtyVertices = false;
                this._indicesLength = 0;
                this._dirtyIndices = false;
                this._elementsPerVertex = 0;
                this._elementsPerVertex = 0;
                for (var i = 0; i < _attributes.length; i++) {
                    this._elementsPerVertex += _attributes[i].numElements;
                }
                this._vertices = new Float32Array(maxVertices * this._elementsPerVertex);
                this._indices = new Uint16Array(maxIndices);
            }
            Mesh.prototype.attributes = function () { return this._attributes; };
            Mesh.prototype.maxVertices = function () { return this._vertices.length / this._elementsPerVertex; };
            Mesh.prototype.numVertices = function () { return this._verticesLength / this._elementsPerVertex; };
            Mesh.prototype.setVerticesLength = function (length) {
                this._dirtyVertices = true;
                this._verticesLength = length;
            };
            Mesh.prototype.vertices = function () { return this._vertices; };
            Mesh.prototype.maxIndices = function () { return this._indices.length; };
            Mesh.prototype.numIndices = function () { return this._indicesLength; };
            Mesh.prototype.setIndicesLength = function (length) {
                this._dirtyIndices = true;
                this._indicesLength = length;
            };
            Mesh.prototype.indices = function () { return this._indices; };
            ;
            Mesh.prototype.setVertices = function (vertices) {
                this._dirtyVertices = true;
                if (vertices.length > this._vertices.length)
                    throw Error("Mesh can't store more than " + this.maxVertices() + " vertices");
                this._vertices.set(vertices, 0);
                this._verticesLength = vertices.length;
            };
            Mesh.prototype.setIndices = function (indices) {
                this._dirtyIndices = true;
                if (indices.length > this._indices.length)
                    throw Error("Mesh can't store more than " + this.maxIndices() + " indices");
                this._indices.set(indices, 0);
                this._indicesLength = indices.length;
            };
            Mesh.prototype.draw = function (shader, primitiveType) {
                this.drawWithOffset(shader, primitiveType, 0, this._indicesLength > 0 ? this._indicesLength : this._verticesLength);
            };
            Mesh.prototype.drawWithOffset = function (shader, primitiveType, offset, count) {
                if (this._dirtyVertices || this._dirtyIndices)
                    this.update();
                this.bind(shader);
                if (this._indicesLength > 0)
                    webgl.gl.drawElements(primitiveType, count, webgl.gl.UNSIGNED_SHORT, offset * 2);
                else
                    webgl.gl.drawArrays(primitiveType, offset, count);
                this.unbind(shader);
            };
            Mesh.prototype.bind = function (shader) {
                webgl.gl.bindBuffer(webgl.gl.ARRAY_BUFFER, this._verticesBuffer);
                var offset = 0;
                for (var i = 0; i < this._attributes.length; i++) {
                    var attrib = this._attributes[i];
                    var location_1 = shader.getAttributeLocation(attrib.name);
                    webgl.gl.enableVertexAttribArray(location_1);
                    webgl.gl.vertexAttribPointer(location_1, attrib.numElements, webgl.gl.FLOAT, false, this._elementsPerVertex * 4, offset * 4);
                    offset += attrib.numElements;
                }
                if (this._indicesLength > 0)
                    webgl.gl.bindBuffer(webgl.gl.ELEMENT_ARRAY_BUFFER, this._indicesBuffer);
            };
            Mesh.prototype.unbind = function (shader) {
                for (var i = 0; i < this._attributes.length; i++) {
                    var attrib = this._attributes[i];
                    var location_2 = shader.getAttributeLocation(attrib.name);
                    webgl.gl.disableVertexAttribArray(location_2);
                }
                webgl.gl.bindBuffer(webgl.gl.ARRAY_BUFFER, null);
                if (this._indicesLength > 0)
                    webgl.gl.bindBuffer(webgl.gl.ELEMENT_ARRAY_BUFFER, null);
            };
            Mesh.prototype.update = function () {
                if (this._dirtyVertices) {
                    if (!this._verticesBuffer) {
                        this._verticesBuffer = webgl.gl.createBuffer();
                    }
                    webgl.gl.bindBuffer(webgl.gl.ARRAY_BUFFER, this._verticesBuffer);
                    webgl.gl.bufferData(webgl.gl.ARRAY_BUFFER, this._vertices.subarray(0, this._verticesLength), webgl.gl.STATIC_DRAW);
                    this._dirtyVertices = false;
                }
                if (this._dirtyIndices) {
                    if (!this._indicesBuffer) {
                        this._indicesBuffer = webgl.gl.createBuffer();
                    }
                    webgl.gl.bindBuffer(webgl.gl.ELEMENT_ARRAY_BUFFER, this._indicesBuffer);
                    webgl.gl.bufferData(webgl.gl.ELEMENT_ARRAY_BUFFER, this._indices.subarray(0, this._indicesLength), webgl.gl.STATIC_DRAW);
                    this._dirtyIndices = false;
                }
            };
            Mesh.prototype.dispose = function () {
                webgl.gl.deleteBuffer(this._verticesBuffer);
                webgl.gl.deleteBuffer(this._indicesBuffer);
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
            function PolygonBatcher(maxVertices) {
                if (maxVertices === void 0) { maxVertices = 10920; }
                this._drawCalls = 0;
                this._drawing = false;
                this._shader = null;
                this._lastTexture = null;
                this._verticesLength = 0;
                this._indicesLength = 0;
                this._srcBlend = webgl.gl.SRC_ALPHA;
                this._dstBlend = webgl.gl.ONE_MINUS_SRC_ALPHA;
                if (maxVertices > 10920)
                    throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
                this._mesh = new webgl.Mesh([new webgl.Position2Attribute(), new webgl.ColorAttribute(), new webgl.TexCoordAttribute()], maxVertices, maxVertices * 3);
            }
            PolygonBatcher.prototype.begin = function (shader) {
                if (this._drawing)
                    throw new Error("PolygonBatch is already drawing. Call PolygonBatch.end() before calling PolygonBatch.begin()");
                this._drawCalls = 0;
                this._shader = shader;
                this._lastTexture = null;
                this._drawing = true;
                webgl.gl.enable(webgl.gl.BLEND);
                webgl.gl.blendFunc(this._srcBlend, this._dstBlend);
            };
            PolygonBatcher.prototype.setBlendMode = function (srcBlend, dstBlend) {
                this._srcBlend = srcBlend;
                this._dstBlend = dstBlend;
                if (this._drawing) {
                    this.flush();
                    webgl.gl.blendFunc(this._srcBlend, this._dstBlend);
                }
            };
            PolygonBatcher.prototype.draw = function (texture, vertices, indices) {
                if (texture != this._lastTexture) {
                    this.flush();
                    this._lastTexture = texture;
                    texture.bind();
                }
                else if (this._verticesLength + vertices.length > this._mesh.vertices().length ||
                    this._indicesLength + indices.length > this._mesh.indices().length) {
                    this.flush();
                }
                var indexStart = this._mesh.numVertices();
                this._mesh.vertices().set(vertices, this._verticesLength);
                this._verticesLength += vertices.length;
                this._mesh.setVerticesLength(this._verticesLength);
                var indicesArray = this._mesh.indices();
                for (var i = this._indicesLength, j = 0; j < indices.length; i++, j++) {
                    indicesArray[i] = indices[j] + indexStart;
                }
                this._indicesLength += indices.length;
                this._mesh.setIndicesLength(this._indicesLength);
            };
            PolygonBatcher.prototype.flush = function () {
                if (this._verticesLength == 0)
                    return;
                this._mesh.draw(this._shader, webgl.gl.TRIANGLES);
                this._verticesLength = 0;
                this._indicesLength = 0;
                this._mesh.setVerticesLength(0);
                this._mesh.setIndicesLength(0);
                this._drawCalls++;
            };
            PolygonBatcher.prototype.end = function () {
                if (!this._drawing)
                    throw new Error("PolygonBatch is not drawing. Call PolygonBatch.begin() before calling PolygonBatch.end()");
                if (this._verticesLength > 0 || this._indicesLength > 0)
                    this.flush();
                this._shader = null;
                this._lastTexture = null;
                this._drawing = false;
                webgl.gl.disable(webgl.gl.BLEND);
            };
            PolygonBatcher.prototype.drawCalls = function () { return this._drawCalls; };
            return PolygonBatcher;
        }());
        webgl.PolygonBatcher = PolygonBatcher;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Shader = (function () {
            function Shader(_vertexShader, _fragmentShader) {
                this._vertexShader = _vertexShader;
                this._fragmentShader = _fragmentShader;
                this._vs = null;
                this._fs = null;
                this._program = null;
                this._tmp2x2 = new Float32Array(2 * 2);
                this._tmp3x3 = new Float32Array(3 * 3);
                this._tmp4x4 = new Float32Array(4 * 4);
                this.compile();
            }
            Shader.prototype.program = function () { return this._program; };
            Shader.prototype.vertexShader = function () { return this._vertexShader; };
            Shader.prototype.fragmentShader = function () { return this._fragmentShader; };
            Shader.prototype.compile = function () {
                var gl = spine.webgl.gl;
                try {
                    this._vs = this.compileShader(gl.VERTEX_SHADER, this._vertexShader);
                    this._fs = this.compileShader(gl.FRAGMENT_SHADER, this._fragmentShader);
                    this._program = this.compileProgram(this._vs, this._fs);
                }
                catch (e) {
                    this.dispose();
                    throw e;
                }
            };
            Shader.prototype.compileShader = function (type, source) {
                var shader = webgl.gl.createShader(type);
                webgl.gl.shaderSource(shader, source);
                webgl.gl.compileShader(shader);
                if (!webgl.gl.getShaderParameter(shader, webgl.gl.COMPILE_STATUS)) {
                    var error = "Couldn't compile shader: " + webgl.gl.getShaderInfoLog(shader);
                    webgl.gl.deleteShader(shader);
                    throw new Error(error);
                }
                return shader;
            };
            Shader.prototype.compileProgram = function (vs, fs) {
                var program = webgl.gl.createProgram();
                webgl.gl.attachShader(program, vs);
                webgl.gl.attachShader(program, fs);
                webgl.gl.linkProgram(program);
                if (!webgl.gl.getProgramParameter(program, webgl.gl.LINK_STATUS)) {
                    var error = "Couldn't compile shader program: " + webgl.gl.getProgramInfoLog(program);
                    webgl.gl.deleteProgram(program);
                    throw new Error(error);
                }
                return program;
            };
            Shader.prototype.bind = function () {
                webgl.gl.useProgram(this._program);
            };
            Shader.prototype.unbind = function () {
                webgl.gl.useProgram(null);
            };
            Shader.prototype.setUniformi = function (uniform, value) {
                webgl.gl.uniform1i(this.getUniformLocation(uniform), value);
            };
            Shader.prototype.setUniformf = function (uniform, value) {
                webgl.gl.uniform1f(this.getUniformLocation(uniform), value);
            };
            Shader.prototype.setUniform2f = function (uniform, value, value2) {
                webgl.gl.uniform2f(this.getUniformLocation(uniform), value, value2);
            };
            Shader.prototype.setUniform3f = function (uniform, value, value2, value3) {
                webgl.gl.uniform3f(this.getUniformLocation(uniform), value, value2, value3);
            };
            Shader.prototype.setUniform4f = function (uniform, value, value2, value3, value4) {
                webgl.gl.uniform4f(this.getUniformLocation(uniform), value, value2, value3, value4);
            };
            Shader.prototype.setUniform2x2f = function (uniform, value) {
                this._tmp2x2.set(value);
                webgl.gl.uniformMatrix2fv(this.getUniformLocation(uniform), false, this._tmp2x2);
            };
            Shader.prototype.setUniform3x3f = function (uniform, value) {
                this._tmp3x3.set(value);
                webgl.gl.uniformMatrix3fv(this.getUniformLocation(uniform), false, this._tmp3x3);
            };
            Shader.prototype.setUniform4x4f = function (uniform, value) {
                this._tmp4x4.set(value);
                webgl.gl.uniformMatrix4fv(this.getUniformLocation(uniform), false, this._tmp4x4);
            };
            Shader.prototype.getUniformLocation = function (uniform) {
                var location = webgl.gl.getUniformLocation(this._program, uniform);
                if (!location)
                    throw new Error("Couldn't find location for uniform " + uniform);
                return location;
            };
            Shader.prototype.getAttributeLocation = function (attribute) {
                var location = webgl.gl.getAttribLocation(this._program, attribute);
                if (location == -1)
                    throw new Error("Couldn't find location for attribute " + attribute);
                return location;
            };
            Shader.prototype.dispose = function () {
                if (this._vs) {
                    webgl.gl.deleteShader(this._vs);
                    this._vs = null;
                }
                if (this._fs) {
                    webgl.gl.deleteShader(this._fs);
                    this._fs = null;
                }
                if (this._program) {
                    webgl.gl.deleteProgram(this._program);
                    this._program = null;
                }
            };
            Shader.newColoredTextured = function () {
                var vs = "\n                attribute vec4 " + Shader.POSITION + ";\n                attribute vec4 " + Shader.COLOR + ";\n                attribute vec2 " + Shader.TEXCOORDS + ";\n                uniform mat4 " + Shader.MVP_MATRIX + ";\n                varying vec4 v_color;\n                varying vec2 v_texCoords;\n            \n                void main() {                    \n                    v_color = " + Shader.COLOR + ";                    \n                    v_texCoords = " + Shader.TEXCOORDS + ";\n                    gl_Position =  " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n                }\n            ";
                var fs = "\n                #ifdef GL_ES\n\t\t\t        #define LOWP lowp\n\t\t\t        precision mediump float;\n\t\t\t    #else\n\t\t\t        #define LOWP \n\t\t\t    #endif\n\t\t\t    varying LOWP vec4 v_color;\n\t\t\t    varying vec2 v_texCoords;\n\t\t\t    uniform sampler2D u_texture;\n\n\t\t\t    void main() {\t\t\t    \n\t\t\t        gl_FragColor = v_color * texture2D(u_texture, v_texCoords);\n\t\t\t    }\n            ";
                return new Shader(vs, fs);
            };
            Shader.newColored = function () {
                var vs = "\n                attribute vec4 " + Shader.POSITION + ";\n                attribute vec4 " + Shader.COLOR + ";            \n                uniform mat4 " + Shader.MVP_MATRIX + ";\n                varying vec4 v_color;                \n            \n                void main() {                    \n                    v_color = " + Shader.COLOR + ";                    \n                    gl_Position =  " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n                }\n            ";
                var fs = "\n                #ifdef GL_ES\n\t\t\t        #define LOWP lowp\n\t\t\t        precision mediump float;\n\t\t\t    #else\n\t\t\t        #define LOWP\n\t\t\t    #endif\n\t\t\t    varying LOWP vec4 v_color;\t\t\t    \t\t\t    \n\n\t\t\t    void main() {\t\t\t    \n\t\t\t        gl_FragColor = v_color;\n\t\t\t    }\n            ";
                return new Shader(vs, fs);
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
        var Texture = (function () {
            function Texture(image, useMipMaps) {
                if (useMipMaps === void 0) { useMipMaps = false; }
                this._boundUnit = 0;
                this._texture = webgl.gl.createTexture();
                this._image = image;
                this.update(useMipMaps);
            }
            Texture.prototype.getImage = function () {
                return this._image;
            };
            Texture.prototype.setFilters = function (minFilter, magFilter) {
                this.bind();
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_MIN_FILTER, minFilter);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_MAG_FILTER, magFilter);
            };
            Texture.prototype.setWraps = function (uWrap, vWrap) {
                this.bind();
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_WRAP_S, uWrap);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_WRAP_T, vWrap);
            };
            Texture.prototype.update = function (useMipMaps) {
                this.bind();
                webgl.gl.texImage2D(webgl.gl.TEXTURE_2D, 0, webgl.gl.RGBA, webgl.gl.RGBA, webgl.gl.UNSIGNED_BYTE, this._image);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_MAG_FILTER, webgl.gl.LINEAR);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_MIN_FILTER, useMipMaps ? webgl.gl.LINEAR_MIPMAP_LINEAR : webgl.gl.LINEAR);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_WRAP_S, webgl.gl.CLAMP_TO_EDGE);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_WRAP_T, webgl.gl.CLAMP_TO_EDGE);
                if (useMipMaps)
                    webgl.gl.generateMipmap(webgl.gl.TEXTURE_2D);
            };
            Texture.prototype.bind = function (unit) {
                if (unit === void 0) { unit = 0; }
                this._boundUnit = unit;
                webgl.gl.activeTexture(webgl.gl.TEXTURE0 + unit);
                webgl.gl.bindTexture(webgl.gl.TEXTURE_2D, this._texture);
            };
            Texture.prototype.unbind = function () {
                webgl.gl.activeTexture(webgl.gl.TEXTURE0 + this._boundUnit);
                webgl.gl.bindTexture(webgl.gl.TEXTURE_2D, null);
            };
            Texture.prototype.dispose = function () {
                webgl.gl.deleteTexture(this._texture);
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
        webgl.Texture = Texture;
        (function (TextureFilter) {
            TextureFilter[TextureFilter["Nearest"] = WebGLRenderingContext.NEAREST] = "Nearest";
            TextureFilter[TextureFilter["Linear"] = WebGLRenderingContext.LINEAR] = "Linear";
            TextureFilter[TextureFilter["MipMap"] = WebGLRenderingContext.LINEAR_MIPMAP_LINEAR] = "MipMap";
            TextureFilter[TextureFilter["MipMapNearestNearest"] = WebGLRenderingContext.NEAREST_MIPMAP_NEAREST] = "MipMapNearestNearest";
            TextureFilter[TextureFilter["MipMapLinearNearest"] = WebGLRenderingContext.LINEAR_MIPMAP_NEAREST] = "MipMapLinearNearest";
            TextureFilter[TextureFilter["MipMapNearestLinear"] = WebGLRenderingContext.NEAREST_MIPMAP_LINEAR] = "MipMapNearestLinear";
            TextureFilter[TextureFilter["MipMapLinearLinear"] = WebGLRenderingContext.LINEAR_MIPMAP_LINEAR] = "MipMapLinearLinear";
        })(webgl.TextureFilter || (webgl.TextureFilter = {}));
        var TextureFilter = webgl.TextureFilter;
        (function (TextureWrap) {
            TextureWrap[TextureWrap["MirroredRepeat"] = WebGLRenderingContext.MIRRORED_REPEAT] = "MirroredRepeat";
            TextureWrap[TextureWrap["ClampToEdge"] = WebGLRenderingContext.CLAMP_TO_EDGE] = "ClampToEdge";
            TextureWrap[TextureWrap["Repeat"] = WebGLRenderingContext.REPEAT] = "Repeat";
        })(webgl.TextureWrap || (webgl.TextureWrap = {}));
        var TextureWrap = webgl.TextureWrap;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
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
                        // page.format = Format[tuple[0]]; we don't need format in WebGL
                        reader.readTuple(tuple);
                        page.minFilter = webgl.Texture.filterFromString(tuple[0]);
                        page.magFilter = webgl.Texture.filterFromString(tuple[1]);
                        var direction = reader.readValue();
                        page.uWrap = webgl.TextureWrap.ClampToEdge;
                        page.vWrap = webgl.TextureWrap.ClampToEdge;
                        if (direction == "x")
                            page.uWrap = webgl.TextureWrap.Repeat;
                        else if (direction == "y")
                            page.vWrap = webgl.TextureWrap.Repeat;
                        else if (direction == "xy")
                            page.uWrap = page.vWrap = webgl.TextureWrap.Repeat;
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
                            // region.splits = new Vector.<int>(parseInt(tuple[0]), parseInt(tuple[1]), parseInt(tuple[2]), parseInt(tuple[3]));
                            if (reader.readTuple(tuple) == 4) {
                                //region.pads = Vector.<int>(parseInt(tuple[0]), parseInt(tuple[1]), parseInt(tuple[2]), parseInt(tuple[3]));
                                reader.readTuple(tuple);
                            }
                        }
                        region.originalWidth = parseInt(tuple[0]);
                        region.originalHeight = parseInt(tuple[1]);
                        reader.readTuple(tuple);
                        region.offsetX = parseInt(tuple[0]);
                        region.offsetY = parseInt(tuple[1]);
                        region.index = parseInt(reader.readValue());
                        // FIXME
                        // textureLoader.loadRegion(region);
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
        webgl.TextureAtlas = TextureAtlas;
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
        webgl.TextureAtlasPage = TextureAtlasPage;
        var TextureAtlasRegion = (function () {
            function TextureAtlasRegion() {
            }
            return TextureAtlasRegion;
        }());
        webgl.TextureAtlasRegion = TextureAtlasRegion;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Vector3 = (function () {
            function Vector3() {
                this.x = 0;
                this.y = 0;
                this.z = 0;
            }
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
        function init(gl) {
            if (!gl || !(gl instanceof WebGLRenderingContext))
                throw Error("Expected a WebGLRenderingContext");
            spine.webgl.gl = gl;
        }
        webgl.init = init;
        function getSourceGLBlendMode(blendMode, premultipliedAlpha) {
            if (premultipliedAlpha === void 0) { premultipliedAlpha = false; }
            switch (blendMode) {
                case spine.BlendMode.Normal: return premultipliedAlpha ? webgl.gl.ONE : webgl.gl.SRC_ALPHA;
                case spine.BlendMode.Additive: return premultipliedAlpha ? webgl.gl.ONE : webgl.gl.SRC_ALPHA;
                case spine.BlendMode.Multiply: return webgl.gl.DST_COLOR;
                case spine.BlendMode.Screen: return webgl.gl.ONE;
                default: throw new Error("Unknown blend mode: " + blendMode);
            }
        }
        webgl.getSourceGLBlendMode = getSourceGLBlendMode;
        function getDestGLBlendMode(blendMode) {
            switch (blendMode) {
                case spine.BlendMode.Normal: return webgl.gl.ONE_MINUS_SRC_ALPHA;
                case spine.BlendMode.Additive: return webgl.gl.ONE;
                case spine.BlendMode.Multiply: return webgl.gl.ONE_MINUS_SRC_ALPHA;
                case spine.BlendMode.Screen: return webgl.gl.ONE_MINUS_SRC_ALPHA;
                default: throw new Error("Unknown blend mode: " + blendMode);
            }
        }
        webgl.getDestGLBlendMode = getDestGLBlendMode;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
//# sourceMappingURL=spine-webgl.js.map