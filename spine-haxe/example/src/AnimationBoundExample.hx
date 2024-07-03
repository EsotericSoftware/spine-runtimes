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
import starling.display.Quad;

class AnimationBoundExample extends Scene {
	var loadBinary = false;
	var skeletonSpriteClipping: SkeletonSprite;
	var skeletonSpriteNoClipping: SkeletonSprite;
	public function load():Void {
		background.color = 0x333333;
		var scale = .2;

		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new StarlingTextureLoader("assets/spineboy.atlas"));
		var skeletondata = SkeletonData.from(Assets.getText("assets/spineboy-pro.json"), atlas);

		var animationStateDataClipping = new AnimationStateData(skeletondata);
		animationStateDataClipping.defaultMix = 0.25;

		skeletonSpriteClipping = new SkeletonSprite(skeletondata, animationStateDataClipping);
		skeletonSpriteClipping.skeleton.updateWorldTransform(Physics.update);
		
		skeletonSpriteClipping.scale = scale;
		skeletonSpriteClipping.x = Starling.current.stage.stageWidth / 3 * 2;
		skeletonSpriteClipping.y = Starling.current.stage.stageHeight / 2;
		
		var animationClipping = skeletonSpriteClipping.state.setAnimationByName(0, "portal", true).animation;
		var animationBoundClipping = skeletonSpriteClipping.getAnimationBounds(animationClipping, true);
		var quad:Quad = new Quad(animationBoundClipping.width * scale, animationBoundClipping.height * scale, 0xc70000);
        quad.x = skeletonSpriteClipping.x + animationBoundClipping.x * scale;
        quad.y = skeletonSpriteClipping.y + animationBoundClipping.y * scale;
		
		var animationStateDataNoClipping = new AnimationStateData(skeletondata);
		animationStateDataNoClipping.defaultMix = 0.25;
		skeletonSpriteNoClipping = new SkeletonSprite(skeletondata, animationStateDataNoClipping);
		skeletonSpriteNoClipping.skeleton.updateWorldTransform(Physics.update);
		skeletonSpriteNoClipping.scale = scale;
		skeletonSpriteNoClipping.x = Starling.current.stage.stageWidth / 3;
		skeletonSpriteNoClipping.y = Starling.current.stage.stageHeight / 2;

		var animationNoClipping = skeletonSpriteNoClipping.state.setAnimationByName(0, "portal", true).animation;
		var animationBoundNoClipping = skeletonSpriteNoClipping.getAnimationBounds(animationNoClipping, false);
		var quadNoClipping:Quad = new Quad(animationBoundNoClipping.width * scale, animationBoundNoClipping.height * scale, 0xc70000);
        quadNoClipping.x = skeletonSpriteNoClipping.x + animationBoundNoClipping.x * scale;
        quadNoClipping.y = skeletonSpriteNoClipping.y + animationBoundNoClipping.y * scale;

		addChild(quad);
		addChild(quadNoClipping);
		addChild(skeletonSpriteClipping);		
		addChild(skeletonSpriteNoClipping);
		addText("Animation bound without clipping", 75, 350);
		addText("Animation bound with clipping", 370, 350);
		addText("Red area is the animation bound", 240, 400);

		juggler.add(skeletonSpriteClipping);
		juggler.add(skeletonSpriteNoClipping);
		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	public function onTouch(e:TouchEvent) {
		var touch = e.getTouch(this);
		if (touch != null && touch.phase == TouchPhase.ENDED) {
			SceneManager.getInstance().switchScene(new ControlBonesExample());
		}
	}
}
