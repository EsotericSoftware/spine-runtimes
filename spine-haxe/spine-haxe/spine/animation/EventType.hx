package spine.animation;

class EventType {
	public static var start(default, never):EventType = new EventType();
	public static var interrupt(default, never):EventType = new EventType();
	public static var end(default, never):EventType = new EventType();
	public static var dispose(default, never):EventType = new EventType();
	public static var complete(default, never):EventType = new EventType();
	public static var event(default, never):EventType = new EventType();

	private function new() {}
}
