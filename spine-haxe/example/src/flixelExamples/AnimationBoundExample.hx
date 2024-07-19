package flixelExamples;


import flixel.util.FlxColor;
import flixel.text.FlxText;
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

class AnimationBoundExample extends FlxState {
	var loadBinary = true;

	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> {
			FlxG.debugger.drawDebug = false;
			FlxG.switchState(new ControlBonesExample());
		});
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new FlixelTextureLoader("assets/spineboy.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/spineboy-pro.skel") : Assets.getText("assets/spineboy-pro.json"), atlas, .2);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSpriteClipping = new SkeletonSprite(data, animationStateData);
		var animationClipping = skeletonSpriteClipping.state.setAnimationByName(0, "portal", true).animation;
		skeletonSpriteClipping.update(0);
		skeletonSpriteClipping.setBoundingBox(animationClipping, true);
		skeletonSpriteClipping.screenCenter();
		skeletonSpriteClipping.x = FlxG.width / 4 - skeletonSpriteClipping.width / 2;
		add(skeletonSpriteClipping);
		var textClipping = new FlxText();
		textClipping.text = "Animation bound with clipping";
		textClipping.size = 12;
		textClipping.x = skeletonSpriteClipping.x + skeletonSpriteClipping.width / 2 - textClipping.width / 2;
		textClipping.y = skeletonSpriteClipping.y + skeletonSpriteClipping.height + 20;
		textClipping.setBorderStyle(FlxTextBorderStyle.OUTLINE, FlxColor.RED, 2);
		add(textClipping);

		var skeletonSpriteNoClipping = new SkeletonSprite(data, animationStateData);
		var animationClipping = skeletonSpriteNoClipping.state.setAnimationByName(0, "portal", true).animation;
		skeletonSpriteNoClipping.update(0);
		skeletonSpriteNoClipping.setBoundingBox(animationClipping, false);
		skeletonSpriteNoClipping.screenCenter();
		skeletonSpriteNoClipping.x = FlxG.width / 4 * 3 - skeletonSpriteClipping.width / 2 - 50;
		add(skeletonSpriteNoClipping);
		var textNoClipping = new FlxText();
		textNoClipping.text = "Animation bound without clipping";
		textNoClipping.size = 12;
		textNoClipping.x = skeletonSpriteNoClipping.x + skeletonSpriteNoClipping.width / 2 - textNoClipping.width / 2;
		textNoClipping.y = skeletonSpriteNoClipping.y + skeletonSpriteNoClipping.height + 20;
		textNoClipping.setBorderStyle(FlxTextBorderStyle.OUTLINE, FlxColor.RED, 2);
		add(textNoClipping);

		var textInstruction = new FlxText();
		textInstruction.text = "Red rectangle is the animation bound";
		textInstruction.size = 12;
		textInstruction.screenCenter();
		textInstruction.y = textNoClipping.y + 40;
		textInstruction.setBorderStyle(FlxTextBorderStyle.OUTLINE, FlxColor.RED, 2);
		add(textInstruction);

		FlxG.debugger.drawDebug = true;

		super.create();
	}
}
