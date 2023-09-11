package spine.animation;

import openfl.Vector;
import spine.Event;
import spine.Skeleton;
import spine.Slot;

class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
	private static inline var ENTRIES:Int = 4;
	private static inline var R:Float = 1;
	private static inline var G:Float = 2;
	private static inline var B:Float = 3;

	private var slotIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.alpha + "|" + slotIndex]));
		this.slotIndex = slotIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var slot:Slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;

		var color:Color = slot.color;
		if (time < frames[0]) // Time is before first frame.
		{
			var setup:Color = slot.data.color;
			switch (blend) {
				case MixBlend.setup:
					color.a = setup.a;
				case MixBlend.first:
					color.a += (setup.a - color.a) * alpha;
			}
			return;
		}

		var a:Float = getCurveValue(time);
		if (alpha == 1) {
			color.a = a;
		} else {
			if (blend == MixBlend.setup)
				color.a = slot.data.color.a;
			color.a += (a - color.a) * alpha;
		}
	}
}
