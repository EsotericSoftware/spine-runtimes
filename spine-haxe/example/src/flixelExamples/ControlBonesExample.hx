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


import flixel.util.FlxSave;
import flixel.math.FlxPoint;
import flixel.util.FlxColor;
import flixel.util.FlxSpriteUtil;
import flixel.FlxSprite;
import flixel.ui.FlxButton;
import flixel.FlxG;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class ControlBonesExample extends FlxState {
	var loadBinary = true;

	private var controlBones = [];
	private	var controls:Array<FlxSprite> = [];
	override public function create():Void {
		FlxG.cameras.bgColor = 0xffa1b2b0;

		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(() -> new EventsExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/stretchyman.atlas"), new FlixelTextureLoader("assets/stretchyman.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/stretchyman-pro.skel") : Assets.getText("assets/stretchyman-pro.json"), atlas);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.scaleX = .5;
		skeletonSprite.scaleY = .5;
		var animation = skeletonSprite.state.setAnimationByName(0, "idle", true).animation;
		skeletonSprite.setBoundingBox(animation);
		skeletonSprite.screenCenter();
		add(skeletonSprite);

		var controlBoneNames = [
			"back-arm-ik-target",
			"back-leg-ik-target",
			"front-arm-ik-target",
			"front-leg-ik-target",
		];

		var radius = 6;
		for (boneName in controlBoneNames) {
			var bone = skeletonSprite.skeleton.findBone(boneName);
			var point = [bone.worldX, bone.worldY];
			skeletonSprite.skeletonToHaxeWorldCoordinates(point);
			var control = new FlxSprite();
			control.makeGraphic(radius * 2, radius * 2, FlxColor.TRANSPARENT, true);
			FlxSpriteUtil.drawCircle(control, radius, radius, radius, 0xffff00ff);
			control.setPosition(point[0] - radius, point[1] - radius);
			controlBones.push(bone);
			controls.push(control);
			add(control);
		}

		var point = [.0, .0];
		skeletonSprite.beforeUpdateWorldTransforms = function (go) {
			for (i in 0...controls.length) {
				var bone = controlBones[i];
				var control = controls[i];
				point[0] = control.x + radius;
				point[1] = control.y + radius;
				go.haxeWorldCoordinatesToBone(point, bone);
				bone.x = point[0];
				bone.y = point[1];
            }
		};

		super.create();
	}

	var mousePosition = FlxPoint.get();
	var offsetX:Float = 0;
	var offsetY:Float = 0;
	var sprite:FlxSprite;
	override public function update(elapsed:Float):Void
	{
		super.update(elapsed);

		mousePosition = FlxG.mouse.getPosition();

		for (control in controls) {
			if (FlxG.mouse.justPressed && control.overlapsPoint(mousePosition))
			{
				sprite = control;
				offsetX = mousePosition.x - sprite.x;
				offsetY = mousePosition.y - sprite.y;
			}
		}

		if (FlxG.mouse.justReleased) sprite = null;

		if (sprite != null)
		{
			sprite.x = mousePosition.x - offsetX;
			sprite.y = mousePosition.y - offsetY;
		}
	}
}
