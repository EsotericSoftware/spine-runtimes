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

import spine.Skin;
import starlingExamples.Scene.SceneManager;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;
import spine.starling.SkeletonSprite;
import spine.starling.StarlingTextureLoader;
import starling.core.Starling;
import starling.events.TouchEvent;
import starling.events.TouchPhase;

class MixAndMatchExample extends Scene {
	var loadBinary = false;

	public function load():Void {
		var atlas = new TextureAtlas(Assets.getText("assets/mix-and-match.atlas"), new StarlingTextureLoader("assets/mix-and-match.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/mix-and-match-pro.skel") : Assets.getText("assets/mix-and-match-pro.json"), atlas);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);
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

		var bounds = skeletonSprite.skeleton.getBounds();
		skeletonSprite.scale = Starling.current.stage.stageHeight / bounds.height * 0.5;
		skeletonSprite.x = Starling.current.stage.stageWidth / 2;
		skeletonSprite.y = Starling.current.stage.stageHeight * 0.9;
		skeletonSprite.state.setAnimationByName(0, "dance", true);

		addChild(skeletonSprite);
		juggler.add(skeletonSprite);

		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	public function onTouch(e:TouchEvent) {
		var touch = e.getTouch(this);
		if (touch != null && touch.phase == TouchPhase.ENDED) {
			SceneManager.getInstance().switchScene(new TankExample());
		}
	}
}
