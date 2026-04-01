/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package flixelExamples;

import flixel.ui.FlxButton;
import flixel.math.FlxPoint;
import flixel.util.FlxColor;
import flixel.util.FlxSpriteUtil;
import spine.boundsprovider.SetupPoseBoundsProvider;
import spine.boundsprovider.SkinsAndAnimationBoundsProvider;
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

class FlixelState extends FlxState {
	var spineSprite:SkeletonSprite;
	var sprite:FlxSprite;
	var sprite2:FlxSprite;
	var myText:FlxText;
	var group:FlxSpriteGroup;
	var justSetWalking = false;

	var jumping = false;

	var scale = 4;
	var speed:Float;

	var skeletonOrigin:FlxSprite;
	var gameObjectOrigin:FlxSprite;
	var radius = 3;
	var rootPoint = [.0, .0];

	override public function create():Void {
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

		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(() -> new BasicExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		// creating a sprite for the floor
		var floor = new FlxSprite();
		floor.loadGraphic(FlxGraphic.fromRectangle(FlxG.width, FlxG.height - 100, 0xff822f02));
		floor.y = FlxG.height - 100;
		add(floor);

		// instructions
		var groupInstructions = new FlxSpriteGroup();
		groupInstructions.setPosition(50, 405);
		groupInstructions.add(new FlxText(0, 0, 200, "Left/Right - Move", 16));
		groupInstructions.add(new FlxText(0, 25, 150, "Space - Jump", 16));
		groupInstructions.add(new FlxText(200, 25, 400, "Click the button for the next example", 16));
		add(groupInstructions);

		// loading spineboy
		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new FlixelTextureLoader("assets/spineboy.atlas"));
		var skeletondata = SkeletonData.from(Assets.getText("assets/spineboy-pro.json"), atlas, 1);

		var animationStateData = new AnimationStateData(skeletondata);
		// spineSprite = new SkeletonSprite(skeletondata, animationStateData, new SetupPoseBoundsProvider());

		spineSprite = new SkeletonSprite(skeletondata, animationStateData, new SkinsAndAnimationBoundsProvider("walk"));

		spineSprite.scale = new FlxPoint(1 / scale, 1 / scale);

		// positioning spineboy
		spineSprite.screenCenter();
		spineSprite.y += spineSprite.height / 2;

		var control3 = new FlxSprite();
		control3.makeGraphic(radius * 2, radius * 2, FlxColor.TRANSPARENT, true);
		FlxSpriteUtil.drawCircle(control3, radius, radius, radius, 0xff5500ff);
		add(control3);
		control3.setPosition(FlxG.width / 2, FlxG.height / 2);

		skeletonOrigin = new FlxSprite();
		skeletonOrigin.makeGraphic(radius * 2, radius * 2, FlxColor.TRANSPARENT, true);
		FlxSpriteUtil.drawCircle(skeletonOrigin, radius, radius, radius, 0xff5500ff);
		add(skeletonOrigin);

		gameObjectOrigin = new FlxSprite();
		gameObjectOrigin.makeGraphic(radius * 2, radius * 2, FlxColor.TRANSPARENT, true);
		FlxSpriteUtil.drawCircle(gameObjectOrigin, radius, radius, radius, 0xffff9100);
		add(gameObjectOrigin);

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

		// // setting y offset function to move object body while jumping
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
		var tmpPoint = [.0, .0];
		spineSprite.afterUpdateWorldTransforms = spineSprite -> {
			if (jumping) {
				tmpPoint[1] = hip.appliedPose.worldY;
				spineSprite.skeletonToHaxeWorldCoordinates(tmpPoint);
				diff -= (tmpPoint[1]);
				spineSprite.offsetY += diff;
				spineSprite.y -= diff;
			}

			tmpPoint[1] = hip.appliedPose.worldY;
			spineSprite.skeletonToHaxeWorldCoordinates(tmpPoint);
			diff = tmpPoint[1];
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

	override public function update(elapsed:Float):Void {
		// spineSprite.calculateBounds();
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

		if (FlxG.keys.anyPressed([UP, DOWN])) {
			if (FlxG.keys.anyPressed([UP])) {
				if (spineSprite.flipY == true)
					spineSprite.flipY = false;
			}
			if (FlxG.keys.anyPressed([DOWN])) {
				if (spineSprite.flipY == false)
					spineSprite.flipY = true;
			}
		}

		if (FlxG.keys.anyPressed([RIGHT, LEFT])) {
			justSetIdle = false;
			var flipped = false;
			var deltaX;
			if (FlxG.keys.anyPressed([RIGHT])) {
				if (spineSprite.flipX == true)
					flipped = true;
				spineSprite.flipX = false;
			}
			if (FlxG.keys.anyPressed([LEFT])) {
				if (spineSprite.flipX == false)
					flipped = true;
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

		var rootBone = spineSprite.skeleton.findBone("root");
		rootPoint[0] = rootBone.appliedPose.worldX;
		rootPoint[1] = rootBone.appliedPose.worldY;
		spineSprite.skeletonToHaxeWorldCoordinates(rootPoint);
		skeletonOrigin.setPosition(rootPoint[0] - radius, rootPoint[1] - radius);
		gameObjectOrigin.setPosition(spineSprite.x - radius, spineSprite.y - radius);

		super.update(elapsed);
	}
}
