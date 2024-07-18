package flixelExamples;


import openfl.Vector;
import flixel.graphics.FlxGraphic;
import flixel.FlxStrip;
import flixel.ui.FlxButton;
import flixel.FlxG;
import flixel.FlxState;
import openfl.utils.Assets;

class BugExample extends FlxState {
	var loadBinary = true;

	var head:FlxStrip = new FlxStrip();
	var pom:FlxStrip = new FlxStrip();
	var patch:FlxStrip = new FlxStrip();

	override public function create():Void {
		var bitmapData = openfl.utils.Assets.getBitmapData("assets/mix-and-match.png");
		var texture:FlxGraphic = FlxG.bitmap.add(bitmapData);
		head.graphic = texture;
		pom.graphic = texture;
		patch.graphic = texture;
		head.vertices = Vector.ofArray([
			0., 0., 100., 0., 100., 100., 0., 100.
		]);
		head.indices = Vector.ofArray([
			0,1,2,2,3,0
		]);
		head.uvtData = Vector.ofArray([
			0.115234375,0.279296875,0.115234375,0.13671875,0.2080078125,0.13671875,0.2080078125,0.279296875
		]);

		patch.vertices = Vector.ofArray([
			-0.3972883003879213,-241.54102616829798,-24.347906632760896,-219.4804656912936,-50.850584500165084,-218.65483854163446,-55.32463497690269,-250.2884010498572,-3.2456740232505874,-256.12600556888134
		]);
		patch.indices = Vector.ofArray([
			3,4,0,1,2,3,0,1,3
		]);
		patch.uvtData = Vector.ofArray([
			0.14951038412982598,0.00390625,0.1689453125,0.06543703563511372,0.1689453125,0.1328125,0.12890625,0.1328125,0.12890625,0.00390625
		]);

		pom.vertices = Vector.ofArray([
			100., 100., 200., 100., 200., 200., 100., 200.
		]);
		pom.indices = Vector.ofArray([
			0,1,2,2,3,0
		]);
		pom.uvtData = Vector.ofArray([
			0.40625,0.330078125,0.40625,0.24609375,0.453125,0.24609375,0.453125,0.330078125
		]);

		patch.color = -9306137;

		head.screenCenter();
		patch.screenCenter();
		pom.screenCenter();
		patch.y +=100;
		add(head);
		add(pom);

		// add(patch); // <--- uncomment this, and head and pom will disappear

		camera.zoom = 1;
		super.create();
	}
}
