package flixelExamples;

import flixel.ui.FlxButton;
import flixel.group.FlxSpriteGroup;
import flixel.FlxSprite;
import flixel.graphics.FlxGraphic;
import spine.animation.AnimationStateData;
import openfl.Assets;
import spine.atlas.TextureAtlas;
import spine.SkeletonData;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxG;
import flixel.FlxState;
import flixel.text.FlxText;

class FlixelState extends FlxState
{
	var spineSprite:SkeletonSprite;
	var sprite:FlxSprite;
	var sprite2:FlxSprite;
	var myText:FlxText;
	var group:FlxSpriteGroup;
	var justSetWalking = false;

	var jumping = false;

	var scale = 4;
	var speed:Float;

	override public function create():Void
	{
		// FlxG.switchState(new AnimationBoundExample());
		// FlxG.switchState(new MixAndMatchExample());
		// FlxG.switchState(new SequenceExample());
		// FlxG.switchState(new BasicExample());

		FlxG.cameras.bgColor = 0xffa1b2b0;

		// setting speed of spineboy (450 is the speed to not let him slide)
		speed = 450 / scale;

		// creating a group
		group = new FlxSpriteGroup();
		group.setPosition(50, 50);
		add(group);

		// creating the sprite to check overlapping
		sprite = new FlxSprite();
		sprite.loadGraphic(FlxGraphic.fromRectangle(150, 100, 0xff8d008d));
		group.add(sprite);

		// creating the text to display overlapping state
		myText = new FlxText(0, 25, 150, "", 16);
        myText.alignment = CENTER;
        group.add(myText);

		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new BasicExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		// creating a sprite for the floor
		var floor = new FlxSprite();
		floor.loadGraphic(FlxGraphic.fromRectangle(FlxG.width, FlxG.height - 100, 0xff822f02));
		floor.y = FlxG.height - 100;
		add(floor);

		// loading spineboy
		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new FlixelTextureLoader("assets/spineboy.atlas"));
		var skeletondata = SkeletonData.from(Assets.getText("assets/spineboy-pro.json"), atlas, 1/scale);
		var animationStateData = new AnimationStateData(skeletondata);
		spineSprite = new SkeletonSprite(skeletondata, animationStateData);

		// positioning spineboy
		spineSprite.setPosition(.5 * FlxG.width, .5 * FlxG.height);

		// setting mix times
		animationStateData.defaultMix = 0.5;
		animationStateData.setMixByName("idle", "walk", 0.1);
		animationStateData.setMixByName("walk", "idle", 0.1);
		animationStateData.setMixByName("idle", "idle-turn", 0.05);
		animationStateData.setMixByName("idle-turn", "idle", 0.05);
		animationStateData.setMixByName("idle-turn", "walk", 0.3);
		animationStateData.setMixByName("idle", "jump", 0);
		animationStateData.setMixByName("jump", "idle", 0.05);
		animationStateData.setMixByName("jump", "walk", 0.05);
		animationStateData.setMixByName("walk", "jump", 0.05);

		// setting idle animation
		spineSprite.state.setAnimationByName(0, "idle", true);

		// setting y offset function to move object body while jumping
		var hip = spineSprite.skeleton.findBone("hip");
		var initialY = 0.;
		var initialOffsetY = 0.;
		spineSprite.state.onStart.add(entry -> {
			if (entry.animation.name == "jump") {
				initialY = spineSprite.y;
				initialOffsetY = spineSprite.offsetY;
			}
		});
		spineSprite.state.onComplete.add(entry -> {
			if (entry.animation.name == "jump") {
				jumping = false;
				spineSprite.y = initialY;
				spineSprite.offsetY = initialOffsetY;
			}
		});
		var diff = .0;
		spineSprite.afterUpdateWorldTransforms = spineSprite -> {
			if (jumping) {
				diff -= hip.y;
				spineSprite.offsetY -= diff;
				spineSprite.y += diff;
			}
			diff = hip.y;
		}

		// adding spineboy to the stage
		add(spineSprite);

		// FlxG.debugger.visible = !FlxG.debugger.visible;
		// debug ui
		// FlxG.debugger.visible = true;
		// FlxG.debugger.drawDebug = true;
		// FlxG.log.redirectTraces = true;

		// FlxG.debugger.track(spineSprite);
		// FlxG.watch.add(spineSprite, "width");
		// FlxG.watch.add(spineSprite, "offsetY");
		// FlxG.watch.add(spineSprite, "y");
		// FlxG.watch.add(this, "jumping");
		super.create();
	}

	var justSetIdle = true;
	override public function update(elapsed:Float):Void
	{
		if (FlxG.overlap(spineSprite, group)) {
			myText.text = "Overlapping";
		} else {
			myText.text = "Non overlapping";
		}

		if (!jumping && FlxG.keys.anyJustPressed([SPACE])) {
			spineSprite.state.setAnimationByName(0, "jump", false);
			jumping = true;
			justSetIdle = false;
			justSetWalking = false;
		}

		if (FlxG.keys.anyJustPressed([J])) {
			// spineSprite.antialiasing = !spineSprite.antialiasing;
			FlxG.debugger.visible = !FlxG.debugger.visible;
		}

		if (FlxG.keys.anyPressed([RIGHT, LEFT])) {
			justSetIdle = false;
			var flipped = false;
			var deltaX;
			if (FlxG.keys.anyPressed([RIGHT])) {
				if (spineSprite.flipX == true) flipped = true;
				spineSprite.flipX = false;
			}
			if (FlxG.keys.anyPressed([LEFT])) {
				if (spineSprite.flipX == false) flipped = true;
				spineSprite.flipX = true;
			}

			deltaX = (spineSprite.flipX == false ? 1 : -1) * speed * elapsed;
			spineSprite.x += deltaX;

			if (!jumping && !justSetWalking) {
				justSetWalking = true;
				if (flipped) {
					spineSprite.state.setAnimationByName(0, "idle-turn", false);
					spineSprite.state.addAnimationByName(0, "walk", true, 0);
				} else {
					spineSprite.state.setAnimationByName(0, "walk", true);
				}
			}

		} else if (!jumping && !justSetIdle) {
			justSetWalking = false;
			justSetIdle = true;
			spineSprite.state.setAnimationByName(0, "idle", true);
		}


		super.update(elapsed);
	}
}