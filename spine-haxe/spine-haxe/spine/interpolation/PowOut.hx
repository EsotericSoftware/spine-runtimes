package spine.interpolation;

class PowOut extends Pow {
	public function new(power:Float) {
		super(power);
	}

	private override function applyInternal(a:Float):Float {
		return Math.pow(a - 1, power) * (power % 2 == 0 ? -1 : 1) + 1;
	}
}
