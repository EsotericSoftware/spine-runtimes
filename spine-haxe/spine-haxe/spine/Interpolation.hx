package spine;

class Interpolation {
	private function applyInternal(a:Float):Float {
		return a;
	}

	public function apply(start:Float, end:Float, a:Float):Float {
		return start + (end - start) * applyInternal(a);
	}
}
