package flixelExamples;


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

	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new SnowglobeExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/celestial-circus.atlas"), new FlixelTextureLoader("assets/celestial-circus.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/celestial-circus-pro.skel") : Assets.getText("assets/celestial-circus-pro.json"), atlas, .15);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.screenCenter();
		skeletonSprite.state.setAnimationByName(0, "eyeblink-long", true);
		add(skeletonSprite);

		super.create();
	}
}
