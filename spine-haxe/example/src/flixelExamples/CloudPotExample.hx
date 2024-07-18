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

class CloudPotExample extends FlxState {
	var loadBinary = true;

	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new AnimationBoundExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/cloud-pot.atlas"), new FlixelTextureLoader("assets/cloud-pot.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/cloud-pot.skel") : Assets.getText("assets/cloud-pot.json"), atlas, .25);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.screenCenter();
		skeletonSprite.state.setAnimationByName(0, "playing-in-the-rain", true);
		add(skeletonSprite);

		super.create();
	}
}
