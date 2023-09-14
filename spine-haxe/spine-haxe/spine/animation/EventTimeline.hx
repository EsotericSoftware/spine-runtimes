package spine.animation;

import openfl.Vector;
import spine.animation.Timeline;
import spine.Event;
import spine.Skeleton;

class EventTimeline extends Timeline {
	public var events:Vector<Event>;

	public function new(frameCount:Int) {
		super(frameCount, Vector.ofArray([Std.string(Property.event)]));
		events = new Vector<Event>(frameCount, true);
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	/** Sets the time in seconds and the event for the specified key frame. */
	public function setFrame(frame:Int, event:Event):Void {
		frames[frame] = event.time;
		events[frame] = event;
	}

	/** Fires events for frames > `lastTime` and <= `time`. */
	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		if (events == null)
			return;

		var frameCount:Int = frames.length;

		if (lastTime > time) // Fire events after last time for looped animations.
		{
			apply(skeleton, lastTime, 2147483647, events, alpha, blend, direction);
			lastTime = -1;
		} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
		{
			return;
		}

		if (time < frames[0]) // Time is before first frame.
		{
			return;
		}

		var frame:Int;
		var i:Int = 0;
		if (lastTime >= frames[0]) {
			i = Timeline.search1(frames, lastTime) + 1;
			var frameTime:Float = frames[i];
			while (i > 0) // Fire multiple events with the same frame.
			{
				if (frames[i - 1] != frameTime)
					break;
				i--;
			}
		}
		while (i < frameCount && time >= frames[i]) {
			events.push(this.events[i]);
			i++;
		}
	}
}
