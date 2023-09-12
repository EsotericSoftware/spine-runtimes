package spine.animation;

import openfl.Vector;
import spine.animation.Listeners.EventListeners;
import spine.Poolable;

class TrackEntry implements Poolable {
	public var animation:Animation;
	public var next:TrackEntry;
	public var previous:TrackEntry;
	public var mixingFrom:TrackEntry;
	public var mixingTo:TrackEntry;
	public var onStart:Listeners = new Listeners();
	public var onInterrupt:Listeners = new Listeners();
	public var onEnd:Listeners = new Listeners();
	public var onDispose:Listeners = new Listeners();
	public var onComplete:Listeners = new Listeners();
	public var onEvent:EventListeners = new EventListeners();
	public var trackIndex:Int = 0;
	public var loop:Bool = false;
	public var reverse:Bool = false;
	public var holdPrevious:Bool = false;
	public var eventThreshold:Float = 0;
	public var attachmentThreshold:Float = 0;
	public var drawOrderThreshold:Float = 0;
	public var animationStart:Float = 0;
	public var animationEnd:Float = 0;
	public var animationLast:Float = 0;
	public var nextAnimationLast:Float = 0;
	public var delay:Float = 0;
	public var trackTime:Float = 0;
	public var trackLast:Float = 0;
	public var nextTrackLast:Float = 0;
	public var trackEnd:Float = 0;
	public var timeScale:Float = 0;
	public var alpha:Float = 0;
	public var mixTime:Float = 0;
	public var mixDuration:Float = 0;
	public var interruptAlpha:Float = 0;
	public var totalAlpha:Float = 0;
	public var mixBlend:MixBlend = MixBlend.replace;
	public var timelineMode:Vector<Int> = new Vector<Int>();
	public var timelineHoldMix:Vector<TrackEntry> = new Vector<TrackEntry>();
	public var timelinesRotation:Vector<Float> = new Vector<Float>();
	public var shortestRotation = false;

	public function new() {}

	public function getAnimationTime():Float {
		if (loop) {
			var duration:Float = animationEnd - animationStart;
			if (duration == 0)
				return animationStart;
			return (trackTime % duration) + animationStart;
		}
		return Math.min(trackTime + animationStart, animationEnd);
	}

	/** If this track entry is non-looping, the track time in seconds when {@link #getAnimationEnd()} is reached, or the current
	 * {@link #getTrackTime()} if it has already been reached. If this track entry is looping, the track time when this
	 * animation will reach its next {@link #getAnimationEnd()} (the next loop completion). */
	public function getTrackComplete():Float {
		var duration:Float = animationEnd - animationStart;
		if (duration != 0) {
			if (loop)
				return duration * (1 + Std.int(trackTime / duration)); // Completion of next loop.
			if (trackTime < duration)
				return duration; // Before duration.
		}
		return trackTime; // Next update.
	}

	public function reset():Void {
		next = null;
		previous = null;
		mixingFrom = null;
		mixingTo = null;
		animation = null;
		onStart.listeners.length = 0;
		onInterrupt.listeners.length = 0;
		onEnd.listeners.length = 0;
		onDispose.listeners.length = 0;
		onComplete.listeners.length = 0;
		onEvent.listeners.length = 0;
		timelineMode.length = 0;
		timelineHoldMix.length = 0;
		timelinesRotation.length = 0;
	}

	public function resetRotationDirection():Void {
		timelinesRotation.length = 0;
	}
}
