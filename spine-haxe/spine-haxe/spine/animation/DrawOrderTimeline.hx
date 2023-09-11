package spine.animation;

import openfl.Vector;
import spine.Event;
import spine.Skeleton;
import spine.Slot;

class DrawOrderTimeline extends Timeline {
	public var drawOrders:Vector<Vector<Int>>;

	public function new(frameCount:Int) {
		super(frameCount, Vector.ofArray([Std.string(Property.drawOrder)]));
		drawOrders = new Vector<Vector<Int>>(frameCount, true);
	}

	public var frameCount(get, never):Int;

	private function get_frameCount():Int {
		return frames.length;
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame(frame:Int, time:Float, drawOrder:Vector<Int>):Void {
		frames[frame] = time;
		drawOrders[frame] = drawOrder;
	}

	override public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var drawOrder:Vector<Slot> = skeleton.drawOrder;
		var slots:Vector<Slot> = skeleton.slots;
		var i:Int = 0, n:Int = slots.length;

		if (direction == MixDirection.mixOut) {
			if (blend == MixBlend.setup) {
				for (i in 0...n) {
					drawOrder[i] = slots[i];
				}
			}
			return;
		}

		if (time < frames[0]) {
			if (blend == MixBlend.setup || blend == MixBlend.first) {
				for (i in 0...n) {
					drawOrder[i] = slots[i];
				}
			}
			return;
		}

		var drawOrderToSetupIndex:Vector<Int> = drawOrders[Timeline.search1(frames, time)];
		if (drawOrderToSetupIndex == null) {
			for (i in 0...n) {
				drawOrder[i] = slots[i];
			}
		} else {
			for (i in 0...n) {
				drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}
}
