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

package starlingExamples;

import starlingExamples.Scene.SceneManager;
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

class SackExample extends Scene {
	var loadBinary = false;

	public function load():Void {
		background.color = 0x333333;

		var atlas = new TextureAtlas(Assets.getText("assets/sack.atlas"), new StarlingTextureLoader("assets/sack.atlas"));
		var skeletondata = SkeletonData.from(Assets.getText("assets/sack-pro.json"), atlas);

		var animationStateData = new AnimationStateData(skeletondata);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(skeletondata, animationStateData);
		skeletonSprite.skeleton.updateWorldTransform(Physics.update);

		skeletonSprite.scale = 0.2;
		skeletonSprite.x = Starling.current.stage.stageWidth / 2;
		skeletonSprite.y = Starling.current.stage.stageHeight/ 2;

		skeletonSprite.state.setAnimationByName(0, "cape-follow-example", true);

		addChild(skeletonSprite);
		juggler.add(skeletonSprite);

		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	public function onTouch(e:TouchEvent) {
		var touch = e.getTouch(this);
		if (touch != null && touch.phase == TouchPhase.ENDED) {
			SceneManager.getInstance().switchScene(new CelestialCircusExample());
		}
	}
}
