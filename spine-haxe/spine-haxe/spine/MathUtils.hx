package spine;

class MathUtils {
	static public var PI:Float = Math.PI;
	static public var PI2:Float = Math.PI * 2;
	static public var radDeg:Float = 180 / Math.PI;
	static public var degRad:Float = Math.PI / 180;

	static public function cosDeg(degrees:Float):Float {
		return Math.cos(degrees * degRad);
	}

	static public function sinDeg(degrees:Float):Float {
		return Math.sin(degrees * degRad);
	}

	static public function clamp(value:Float, min:Float, max:Float):Float {
		if (value < min)
			return min;
		if (value > max)
			return max;
		return value;
	}

	static public function signum(value:Float):Float {
		return value > 0 ? 1 : value < 0 ? -1 : 0;
	}

	static public function randomTriangular(min:Float, max:Float):Float {
		return randomTriangularWith(min, max, (min + max) * 0.5);
	}

	static public function randomTriangularWith(min:Float, max:Float, mode:Float):Float {
		var u:Float = Math.random();
		var d:Float = max - min;
		if (u <= (mode - min) / d)
			return min + Math.sqrt(u * d * (mode - min));
		return max - Math.sqrt((1 - u) * d * (max - mode));
	}
}
