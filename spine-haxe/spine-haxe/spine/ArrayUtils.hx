package spine;

class ArrayUtils {
	public static function resize<T>(array:Array<T>, count:Int, value:T) {
		if (count < 0)
			count = 0;
		array.resize(count);
		for (i in 0...count) {
			array[i] = value;
		}
		return array;
	}
}
