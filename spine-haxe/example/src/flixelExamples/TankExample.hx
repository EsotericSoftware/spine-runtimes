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

class TankExample extends FlxState {
	var loadBinary = true;

	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new VineExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/tank.atlas"), new FlixelTextureLoader("assets/tank.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/tank-pro.skel") : Assets.getText("assets/tank-pro.json"), atlas, .125);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		var animation = skeletonSprite.state.setAnimationByName(0, "drive", true).animation;
		skeletonSprite.setBoundingBox(animation);
		skeletonSprite.screenCenter();
		add(skeletonSprite);

		super.create();
	}
}
