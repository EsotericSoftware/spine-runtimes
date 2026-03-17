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

package starlingExamples;

import starling.filters.BlurFilter;
import spine.boundsprovider.SkinsAndAnimationBoundsProvider;
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
import starling.display.Quad;

class BoundsProviderExample extends Scene {
	var loadBinary = false;
	var skeletonSpriteClipping:SkeletonSprite;
	var skeletonSpriteNoClipping:SkeletonSprite;
	var quad:Quad;
	var quadNoClipping:Quad;
	private var movement = new openfl.geom.Point();

	public function load():Void {
		background.color = 0x333333;
		var scale = .4;

		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new StarlingTextureLoader("assets/spineboy.atlas"));
		var skeletondata = SkeletonData.from(Assets.getText("assets/spineboy-pro.json"), atlas, .5);

		var stateDataClipping = new AnimationStateData(skeletondata);
		skeletonSpriteClipping = new SkeletonSprite(skeletondata, stateDataClipping, new SkinsAndAnimationBoundsProvider("portal", null));
		skeletonSpriteClipping.scale = scale;
		skeletonSpriteClipping.x = Starling.current.stage.stageWidth / 4 * 3;
		skeletonSpriteClipping.y = Starling.current.stage.stageHeight / 2;
		skeletonSpriteClipping.state.setAnimationByName(0, "portal", true);
		skeletonSpriteClipping.filter = new BlurFilter();

		var bounds = skeletonSpriteClipping.bounds;
		quad = new Quad(bounds.width, bounds.height, 0xc70000);
		quad.x = bounds.x;
		quad.y = bounds.y;
		addChild(quad);
		addChild(skeletonSpriteClipping);

		var stateDataNoClipping = new AnimationStateData(skeletondata);
		skeletonSpriteNoClipping = new SkeletonSprite(skeletondata, stateDataNoClipping, new SkinsAndAnimationBoundsProvider("portal", null, true));
		skeletonSpriteNoClipping.scale = scale;
		skeletonSpriteNoClipping.x = Starling.current.stage.stageWidth / 4;
		skeletonSpriteNoClipping.y = Starling.current.stage.stageHeight / 2;
		skeletonSpriteNoClipping.state.setAnimationByName(0, "portal", true);
		skeletonSpriteNoClipping.filter = new BlurFilter();

		bounds = skeletonSpriteNoClipping.bounds;
		quadNoClipping = new Quad(bounds.width, bounds.height, 0xc70000);
		quadNoClipping.x = bounds.x;
		quadNoClipping.y = bounds.y;
		addChild(quadNoClipping);
		addChild(skeletonSpriteNoClipping);

		addText("Bounds with clipping", 40, 350);
		addText("Bounds without clipping", 400, 350);
		addText("Bounds created with SkinsAndAnimationBoundsProvider", 240, 400);
		addText("The blur filter shows also the correcntess of the bounds.", 240, 450);
		addText("You can move the elements around to see the bounds is always correct.", 240, 500);

		juggler.add(skeletonSpriteClipping);
		juggler.add(skeletonSpriteNoClipping);
		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	public function onTouch(e:TouchEvent) {
		var skeletonTouch = e.getTouch(skeletonSpriteClipping);
		var skeletonTouch2 = e.getTouch(skeletonSpriteNoClipping);
		if (skeletonTouch != null) {
			if (skeletonTouch.phase == TouchPhase.MOVED) {
				skeletonTouch.getMovement(this, movement);
				skeletonSpriteClipping.x += movement.x;
				skeletonSpriteClipping.y += movement.y;

				var sBounds = skeletonSpriteClipping.bounds;
				quad.x = sBounds.x;
				quad.y = sBounds.y;
			}
		} else if (skeletonTouch2 != null) {
			if (skeletonTouch2.phase == TouchPhase.MOVED) {
				skeletonTouch2.getMovement(this, movement);
				skeletonSpriteNoClipping.x += movement.x;
				skeletonSpriteNoClipping.y += movement.y;

				var sBounds = skeletonSpriteNoClipping.bounds;
				quadNoClipping.x = sBounds.x;
				quadNoClipping.y = sBounds.y;
			}
		} else {
			var sceneTouch = e.getTouch(this);
			if (sceneTouch != null && sceneTouch.phase == TouchPhase.ENDED) {
				SceneManager.getInstance().switchScene(new ControlBonesExample());
			}
		}
	}
}
