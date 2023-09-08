package spine;

import openfl.utils.Function;
import openfl.Vector;

@:generic class Pool<T> {
	private var items:Vector<T>;
	private var instantiator:Function;

	public function new(instantiator:Void->T) {
		this.items = new Vector<T>();
		this.instantiator = instantiator;
	}

	public function obtain():T {
		return this.items.length > 0 ? this.items.pop() : this.instantiator();
	}

	public function free(item:T):Void {
		if (Std.isOfType(item, Poolable))
			cast(item, Poolable).reset();
		items.push(item);
	}

	public function freeAll(items:Vector<T>):Void {
		for (item in items) {
			free(item);
		}
	}

	public function clear():Void {
		items.length = 0;
	}
}
