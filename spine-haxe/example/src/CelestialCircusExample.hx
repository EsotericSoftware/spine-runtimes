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

import spine.BlendMode;
import Scene.SceneManager;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.Physics;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;
import spine.starling.SkeletonSprite;
import spine.starling.StarlingTextureLoader;
import starling.core.Starling;
import starling.events.TouchEvent;
import starling.events.TouchPhase;

class CelestialCircusExample extends Scene {
	var loadBinary = true;

	var skeletonSprite:SkeletonSprite;
	private var movement = new openfl.geom.Point();

	public function load():Void {
		background.color = 0x333333;

		var atlas = new TextureAtlas(Assets.getText("assets/celestial-circus.atlas"), new StarlingTextureLoader("assets/celestial-circus.atlas"));
		var skeletondata = SkeletonData.from(loadBinary ? Assets.getBytes("assets/celestial-circus-pro.skel") : Assets.getText("assets/celestial-circus-pro.json"), atlas);

		var animationStateData = new AnimationStateData(skeletondata);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(skeletondata, animationStateData);
		skeletonSprite.skeleton.updateWorldTransform(Physics.update);
		var bounds = skeletonSprite.skeleton.getBounds();

		skeletonSprite.scale = 0.2;
		skeletonSprite.x = Starling.current.stage.stageWidth / 2;
		skeletonSprite.y = Starling.current.stage.stageHeight / 1.5;

		skeletonSprite.state.setAnimationByName(0, "eyeblink-long", true);

		addText("Drag Celeste to move her around");
		addText("Click background for next scene", 10, 30);

		addChild(skeletonSprite);
		juggler.add(skeletonSprite);

		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	public function onTouch(e:TouchEvent) {
		var skeletonTouch = e.getTouch(skeletonSprite);
		if (skeletonTouch != null) {
			if (skeletonTouch.phase == TouchPhase.MOVED) {
				skeletonTouch.getMovement(this, movement);
				skeletonSprite.x += movement.x;
				skeletonSprite.y += movement.y;
				skeletonSprite.skeleton.physicsTranslate(
					movement.x / skeletonSprite.scale,
					movement.y / skeletonSprite.scale,
				);
			}
		} else {
			var sceneTouch = e.getTouch(this);
			if (sceneTouch != null && sceneTouch.phase == TouchPhase.ENDED) {
				SceneManager.getInstance().switchScene(new SnowglobeExample());
			}
		}


	}

}
