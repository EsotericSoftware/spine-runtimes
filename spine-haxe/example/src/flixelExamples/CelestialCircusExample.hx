package flixelExamples;


import flixel.text.FlxText;
import flixel.math.FlxPoint;
import spine.Skin;
import flixel.ui.FlxButton;
import flixel.FlxG;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class CelestialCircusExample extends FlxState {
	var loadBinary = true;

	var skeletonSprite:SkeletonSprite;
	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new SnowglobeExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/celestial-circus.atlas"), new FlixelTextureLoader("assets/celestial-circus.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/celestial-circus-pro.skel") : Assets.getText("assets/celestial-circus-pro.json"), atlas, .15);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.screenCenter();
		skeletonSprite.state.setAnimationByName(0, "eyeblink-long", true);
		add(skeletonSprite);

		add(new FlxText(50, 50, 200, "Drag Celeste to move her around", 16));

		super.create();
	}

	var mousePosition = FlxPoint.get();
	var dragging:Bool = false;
	var lastX:Float = 0;
	var lastY:Float = 0;
	override public function update(elapsed:Float):Void
	{
		super.update(elapsed);

		mousePosition = FlxG.mouse.getPosition();

		if (FlxG.mouse.justPressed && skeletonSprite.overlapsPoint(mousePosition))
		{
			dragging = true;
			lastX = mousePosition.x;
		  	lastY = mousePosition.y;
		}

		if (FlxG.mouse.justReleased) dragging = false;

		if (dragging)
		{
			skeletonSprite.x += mousePosition.x - lastX;
			skeletonSprite.y += mousePosition.y - lastY;
			skeletonSprite.skeleton.physicsTranslate(
				mousePosition.x - lastX,
				mousePosition.y - lastY,
			);
			lastX = mousePosition.x;
            lastY = mousePosition.y;
		}

	}
}
