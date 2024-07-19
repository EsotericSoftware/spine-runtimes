package flixelExamples;


import flixel.util.FlxSave;
import flixel.math.FlxPoint;
import flixel.util.FlxColor;
import flixel.util.FlxSpriteUtil;
import flixel.FlxSprite;
import flixel.ui.FlxButton;
import flixel.FlxG;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class ControlBonesExample extends FlxState {
	var loadBinary = true;

	private var controlBones = [];
	private	var controls:Array<FlxSprite> = [];
	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new EventsExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/stretchyman.atlas"), new FlixelTextureLoader("assets/stretchyman.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/stretchyman-pro.skel") : Assets.getText("assets/stretchyman-pro.json"), atlas);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.scaleX = .5;
		skeletonSprite.scaleY = .5;
		var animation = skeletonSprite.state.setAnimationByName(0, "idle", true).animation;
		skeletonSprite.setBoundingBox(animation);
		skeletonSprite.screenCenter();
		add(skeletonSprite);

		var controlBoneNames = [
			"back-arm-ik-target",
			"back-leg-ik-target",
			"front-arm-ik-target",
			"front-leg-ik-target",
		];

		var radius = 6;
		for (boneName in controlBoneNames) {
			var bone = skeletonSprite.skeleton.findBone(boneName);
			var point = [bone.worldX, bone.worldY];
			skeletonSprite.skeletonToHaxeWorldCoordinates(point);
			var control = new FlxSprite();
			control.makeGraphic(radius * 2, radius * 2, FlxColor.TRANSPARENT, true);
			FlxSpriteUtil.drawCircle(control, radius, radius, radius, 0xffff00ff);
			control.setPosition(point[0] - radius, point[1] - radius);
			controlBones.push(bone);
			controls.push(control);
			add(control);
		}

		var point = [.0, .0];
		skeletonSprite.beforeUpdateWorldTransforms = function (go) {
			for (i in 0...controls.length) {
				var bone = controlBones[i];
				var control = controls[i];
				point[0] = control.x + radius;
				point[1] = control.y + radius;
				go.haxeWorldCoordinatesToBone(point, bone);
				bone.x = point[0];
				bone.y = point[1];
            }
		};

		super.create();
	}

	var mousePosition = FlxPoint.get();
	var offsetX:Float = 0;
	var offsetY:Float = 0;
	var sprite:FlxSprite;
	override public function update(elapsed:Float):Void
	{
		super.update(elapsed);

		mousePosition = FlxG.mouse.getPosition();

		for (control in controls) {
			if (FlxG.mouse.justPressed && control.overlapsPoint(mousePosition))
			{
				sprite = control;
				offsetX = mousePosition.x - sprite.x;
				offsetY = mousePosition.y - sprite.y;
			}
		}

		if (FlxG.mouse.justReleased) sprite = null;

		if (sprite != null)
		{
			sprite.x = mousePosition.x - offsetX;
			sprite.y = mousePosition.y - offsetY;
		}
	}
}
