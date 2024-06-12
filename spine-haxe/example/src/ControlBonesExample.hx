/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

import openfl.geom.Point;
import Scene.SceneManager;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;
import spine.starling.SkeletonSprite;
import spine.starling.StarlingTextureLoader;
import starling.core.Starling;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.display.Canvas;

class ControlBonesExample extends Scene {
	var loadBinary = true;
	
	var skeletonSprite:SkeletonSprite;
	private var movement = new openfl.geom.Point();
	private var controlBones = [];
	private	var controls = [];

	public function load():Void {
		var atlas = new TextureAtlas(Assets.getText("assets/stretchyman.atlas"), new StarlingTextureLoader("assets/stretchyman.atlas"));
		var skeletondata = SkeletonData.from(loadBinary ? Assets.getBytes("assets/stretchyman-pro.skel") : Assets.getText("assets/stretchyman-pro.json"), atlas);
		var animationStateData = new AnimationStateData(skeletondata);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(skeletondata, animationStateData);

		var bounds = skeletonSprite.skeleton.getBounds();
		skeletonSprite.scale = Starling.current.stage.stageWidth / bounds.width * 0.25;
		skeletonSprite.x = Starling.current.stage.stageWidth / 2;
		skeletonSprite.y = Starling.current.stage.stageHeight * 0.9;

		skeletonSprite.state.setAnimationByName(0, "idle", true);

		addChild(skeletonSprite);
		juggler.add(skeletonSprite);

		addText("Drag purple circles or stretchyhman.");
		addText("Click background for next scene", 10, 30);

		var controlBoneNames = [
			"back-arm-ik-target",
			"back-leg-ik-target",
			"front-arm-ik-target",
			"front-leg-ik-target",
		];

		for (boneName in controlBoneNames) {
			var bone = skeletonSprite.skeleton.findBone(boneName);
			var point = [bone.worldX, bone.worldY];
			skeletonSprite.skeletonToHaxeWorldCoordinates(point);

			var control:Canvas = new Canvas();
			control.name = boneName;
			control.beginFill(0xff00ff);
			control.drawCircle(0, 0, 6);
			control.endFill();

			control.x = point[0];
			control.y = point[1];
			controlBones.push(bone);
			controls.push(control);
			addChild(control);
		}

		var point = [.0, .0];
		skeletonSprite.beforeUpdateWorldTransforms = function (go) {
			for (i in 0...controls.length) {
				var bone = controlBones[i];
				var control = controls[i];
				point[0] = control.x;
				point[1] = control.y;
				go.haxeWorldCoordinatesToBone(point, bone);
				bone.x = point[0];
				bone.y = point[1];
            }
		};

		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	public function onTouch(e:TouchEvent) {
		var touchBackground = true;
		for (control in controls) {
			var touchControl = e.getTouch(control);
			if (touchControl != null) {
				touchBackground = false;
				if (touchControl.phase == TouchPhase.MOVED) {
					touchControl.getMovement(this, movement);
					control.x += movement.x;
					control.y += movement.y;
				}
			}
		}

		var touchSkeleton = e.getTouch(skeletonSprite);
		if (touchSkeleton != null) {
			touchBackground = false;
			if (touchSkeleton.phase == TouchPhase.MOVED) {
				touchSkeleton.getMovement(this, movement);
				skeletonSprite.skeleton.x += movement.x / skeletonSprite.scale;
				skeletonSprite.skeleton.y += movement.y / skeletonSprite.scale;
			}
		}
		
		if (touchBackground) {
			var sceneTouch = e.getTouch(this);
			if (sceneTouch != null && sceneTouch.phase == TouchPhase.ENDED) {
				SceneManager.getInstance().switchScene(new EventsExample());
			}
		}
	}
}
