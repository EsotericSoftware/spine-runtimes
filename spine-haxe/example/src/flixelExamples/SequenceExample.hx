package flixelExamples;


import flixel.ui.FlxButton;
import flixel.FlxG;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class SequenceExample extends FlxState {
	var loadBinary = true;

	var skeletonSprite:SkeletonSprite;
	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new MixAndMatchExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/dragon.atlas"), new FlixelTextureLoader("assets/dragon.atlas"));
		var skeletondata = SkeletonData.from(loadBinary ? Assets.getBytes("assets/dragon-ess.skel") : Assets.getText("assets/dragon-.json"), atlas, .5);
		var animationStateData = new AnimationStateData(skeletondata);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(skeletondata, animationStateData);
		skeletonSprite.screenCenter();
        skeletonSprite.y -= 100;

		skeletonSprite.state.setAnimationByName(0, "flying", true);
		FlxG.debugger.visible = !FlxG.debugger.visible;
		FlxG.debugger.track(skeletonSprite);
		add(skeletonSprite);
		super.create();
	}

}
