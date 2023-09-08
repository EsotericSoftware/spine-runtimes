package spine.animation;

import openfl.Vector;

class RGBTimeline extends CurveTimeline implements SlotTimeline {
	private static inline var ENTRIES:Int = 4;
	private static inline var R:Int = 1;
	private static inline var G:Int = 2;
	private static inline var B:Int = 3;

	private var slotIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.rgb + "|" + slotIndex]));
		this.slotIndex = slotIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	/** Sets the time in seconds, light, and dark colors for the specified key frame. */
	public function setFrame(frame:Int, time:Float, r:Float, g:Float, b:Float):Void {
		frame <<= 2;
		frames[frame] = time;
		frames[frame + R] = r;
		frames[frame + G] = g;
		frames[frame + B] = b;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var slot:Slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;

		var color:Color = slot.color, setup:Color;
		if (time < frames[0]) {
			setup = slot.data.color;
			switch (blend) {
				case MixBlend.setup:
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
				case MixBlend.first:
					color.r += (setup.r - color.r) * alpha;
					color.g += (setup.g - color.g) * alpha;
					color.b += (setup.b - color.b) * alpha;
			}
			return;
		}

		var r:Float = 0, g:Float = 0, b:Float = 0;
		var i:Int = Timeline.search(frames, time, ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				var t:Float = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
			case CurveTimeline.STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
			default:
				r = getBezierValue(time, i, R, curveType - CurveTimeline.BEZIER);
				g = getBezierValue(time, i, G, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
				b = getBezierValue(time, i, B, curveType + CurveTimeline.BEZIER_SIZE * 2 - CurveTimeline.BEZIER);
		}
		if (alpha == 1) {
			color.r = r;
			color.g = g;
			color.b = b;
		} else {
			if (blend == MixBlend.setup) {
				setup = slot.data.color;
				color.r = setup.r;
				color.g = setup.g;
				color.b = setup.b;
			}
			color.r += (r - color.r) * alpha;
			color.g += (g - color.g) * alpha;
			color.b += (b - color.b) * alpha;
		}
	}
}
