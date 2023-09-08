package spine.interpolation;

import spine.Interpolation;

class Pow extends Interpolation {
	private var power:Int = 0;

	public function new(power:Int) {
		this.power = power;
	}

	private override function applyInternal(a:Float):Float {
		if (a <= 0.5)
			return Math.pow(a * 2, power) / 2;
		return Math.pow((a - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
	}
}
