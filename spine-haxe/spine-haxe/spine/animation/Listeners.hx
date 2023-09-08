package spine.animation;

import openfl.errors.ArgumentError;
import openfl.Vector;

class Listeners {
	private var _listeners:Vector<TrackEntry->Void>;

	public var listeners(get, never):Vector<TrackEntry->Void>;

	private function get_listeners():Vector<TrackEntry->Void> {
		return _listeners;
	}

	public function new() {
		_listeners = new Vector<TrackEntry->Void>();
	}

	public function invoke(entry:TrackEntry) {
		for (listener in _listeners) {
			listener(entry);
		}
	}

	public function add(listener:TrackEntry->Void):Void {
		if (listener == null)
			throw new ArgumentError("listener cannot be null.");
		var indexOf:Int = _listeners.indexOf(listener);
		if (indexOf == -1)
			_listeners.push(listener);
	}

	public function remove(listener:TrackEntry->Void):Void {
		if (listener == null)
			throw new ArgumentError("listener cannot be null.");
		var indexOf:Int = _listeners.indexOf(listener);
		if (indexOf != -1)
			_listeners.splice(indexOf, 1);
	}
}

class EventListeners {
	private var _listeners:Vector<TrackEntry->Event->Void>;

	public var listeners(get, never):Vector<TrackEntry->Event->Void>;

	private function get_listeners():Vector<TrackEntry->Event->Void> {
		return _listeners;
	}

	public function new() {
		_listeners = new Vector<TrackEntry->Event->Void>();
	}

	public function invoke(entry:TrackEntry, event:Event) {
		for (listener in _listeners) {
			listener(entry, event);
		}
	}

	public function add(listener:TrackEntry->Event->Void):Void {
		if (listener == null)
			throw new ArgumentError("listener cannot be null.");
		var indexOf:Int = _listeners.indexOf(listener);
		if (indexOf == -1)
			_listeners.push(listener);
	}

	public function remove(listener:TrackEntry->Event->Void):Void {
		if (listener == null)
			throw new ArgumentError("listener cannot be null.");
		var indexOf:Int = _listeners.indexOf(listener);
		if (indexOf != -1)
			_listeners.splice(indexOf, 1);
	}
}
