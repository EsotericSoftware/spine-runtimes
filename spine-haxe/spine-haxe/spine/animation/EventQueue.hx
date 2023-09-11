package spine.animation;

import spine.Event;

class EventQueue {
	private var objects:Array<Dynamic>;
	private var animationState:AnimationState;

	public var drainDisabled:Bool = false;

	public function new(animationState:AnimationState) {
		this.animationState = animationState;
		objects = new Array<Dynamic>();
	}

	public function start(entry:TrackEntry):Void {
		objects.push(EventType.start);
		objects.push(entry);
		animationState.animationsChanged = true;
	}

	public function interrupt(entry:TrackEntry):Void {
		objects.push(EventType.interrupt);
		objects.push(entry);
	}

	public function end(entry:TrackEntry):Void {
		objects.push(EventType.end);
		objects.push(entry);
		animationState.animationsChanged = true;
	}

	public function dispose(entry:TrackEntry):Void {
		objects.push(EventType.dispose);
		objects.push(entry);
	}

	public function complete(entry:TrackEntry):Void {
		objects.push(EventType.complete);
		objects.push(entry);
	}

	public function event(entry:TrackEntry, event:Event):Void {
		objects.push(EventType.event);
		objects.push(entry);
		objects.push(event);
	}

	public function drain():Void {
		if (drainDisabled)
			return; // Not reentrant.
		drainDisabled = true;

		var i:Int = 0;
		while (i < objects.length) {
			var type:EventType = cast(objects[i], EventType);
			var entry:TrackEntry = cast(objects[i + 1], TrackEntry);
			switch (type) {
				case EventType.start:
					entry.onStart.invoke(entry);
					animationState.onStart.invoke(entry);
				case EventType.interrupt:
					entry.onInterrupt.invoke(entry);
					animationState.onInterrupt.invoke(entry);
				case EventType.end:
					entry.onEnd.invoke(entry);
					animationState.onEnd.invoke(entry);
					entry.onDispose.invoke(entry);
					animationState.onDispose.invoke(entry);
					animationState.trackEntryPool.free(entry);
				case EventType.dispose:
					entry.onDispose.invoke(entry);
					animationState.onDispose.invoke(entry);
					animationState.trackEntryPool.free(entry);
				case EventType.complete:
					entry.onComplete.invoke(entry);
					animationState.onComplete.invoke(entry);
				case EventType.event:
					var event:Event = cast(objects[i++ + 2], Event);
					entry.onEvent.invoke(entry, event);
					animationState.onEvent.invoke(entry, event);
			}
			i += 2;
		}
		clear();

		drainDisabled = false;
	}

	public function clear():Void {
		objects.resize(0);
	}
}
