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

class MixAndMatchExample extends FlxState {
	var loadBinary = false;
	// var loadBinary = true;

	var skeletonSprite:SkeletonSprite;
	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new TankExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/mix-and-match.atlas"), new FlixelTextureLoader("assets/mix-and-match.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/mix-and-match-pro.skel") : Assets.getText("assets/mix-and-match-pro.json"), atlas, .5);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(data, animationStateData);
		var customSkin = new Skin("custom");
		var skinBase = data.findSkin("skin-base");
		customSkin.addSkin(skinBase);
		customSkin.addSkin(data.findSkin("nose/short"));
		customSkin.addSkin(data.findSkin("eyelids/girly"));
		customSkin.addSkin(data.findSkin("eyes/violet"));
		customSkin.addSkin(data.findSkin("hair/brown"));
		customSkin.addSkin(data.findSkin("clothes/hoodie-orange"));
		customSkin.addSkin(data.findSkin("legs/pants-jeans"));
		customSkin.addSkin(data.findSkin("accessories/bag"));
		customSkin.addSkin(data.findSkin("accessories/hat-red-yellow"));
		skeletonSprite.skeleton.skin = customSkin;

		skeletonSprite.state.update(0);
		var animation = skeletonSprite.state.setAnimationByName(0, "dance", true).animation;
		skeletonSprite.setBoundingBox(animation);
		skeletonSprite.screenCenter();
		add(skeletonSprite);

		super.create();
	}

}
