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

class SnowglobeExample extends FlxState {
	var loadBinary = false;

	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new CloudPotExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/snowglobe.atlas"), new FlixelTextureLoader("assets/snowglobe.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/snowglobe-pro.skel") : Assets.getText("assets/snowglobe-pro.json"), atlas, .125);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.screenCenter();
		skeletonSprite.state.setAnimationByName(0, "shake", true);
		add(skeletonSprite);

		super.create();
	}
}
