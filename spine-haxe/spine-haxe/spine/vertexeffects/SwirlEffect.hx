package spine.vertexeffects;

import spine.interpolation.Pow;
import spine.Interpolation;
import spine.MathUtils;
import spine.Skeleton;
import spine.Vertex;
import spine.VertexEffect;

class SwirlEffect implements VertexEffect {
	private var worldX:Float = 0;
	private var worldY:Float = 0;
	private var _radius:Float = 0;
	private var _angle:Float = 0;
	private var _interpolation:Interpolation;
	private var _centerX:Float = 0;
	private var _centerY:Float = 0;

	public function new(radius:Float) {
		this._interpolation = new Pow(2);
		this._radius = radius;
	}

	public function begin(skeleton:Skeleton):Void {
		worldX = skeleton.x + _centerX;
		worldY = skeleton.y + _centerY;
	}

	public function transform(vertex:Vertex):Void {
		var x:Float = vertex.x - worldX;
		var y:Float = vertex.y - worldY;
		var dist:Float = Math.sqrt(x * x + y * y);
		if (dist < radius) {
			var theta:Float = interpolation.apply(0, angle, (radius - dist) / radius);
			var cos:Float = Math.cos(theta), sin:Float = Math.sin(theta);
			vertex.x = cos * x - sin * y + worldX;
			vertex.y = sin * x + cos * y + worldY;
		}
	}

	public function end():Void {}

	public var radius(get, set):Float;

	private function get_radius():Float {
		return _radius;
	}

	private function set_radius(radius:Float):Float {
		_radius = radius;
		return _radius;
	}

	public var angle(get, set):Float;

	private function get_angle():Float {
		return _angle;
	}

	private function set_angle(angle:Float):Float {
		_angle = angle * MathUtils.degRad;
		return _angle;
	}

	public var centerX(get, set):Float;

	private function get_centerX():Float {
		return _centerX;
	}

	private function set_centerX(centerX:Float):Float {
		_centerX = centerX;
		return _centerX;
	}

	public var centerY(get, set):Float;

	private function get_centerY():Float {
		return _centerY;
	}

	private function set_centerY(centerY:Float):Float {
		_centerY = centerY;
		return _centerY;
	}

	public var interpolation(get, set):Interpolation;

	private function get_interpolation():Interpolation {
		return _interpolation;
	}

	private function set_interpolation(interpolation:Interpolation):Interpolation {
		_interpolation = interpolation;
		return _interpolation;
	}
}
