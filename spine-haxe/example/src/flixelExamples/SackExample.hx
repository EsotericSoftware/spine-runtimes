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

class SackExample extends FlxState {
	var loadBinary = false;

	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new CelestialCircusExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/sack.atlas"), new FlixelTextureLoader("assets/sack.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/sack-pro.skel") : Assets.getText("assets/sack-pro.json"), atlas, .25);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.screenCenter();
		skeletonSprite.x -= 100;
		skeletonSprite.state.setAnimationByName(0, "cape-follow-example", true);
		add(skeletonSprite);

		super.create();
	}
}
