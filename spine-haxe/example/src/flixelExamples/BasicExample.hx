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

package flixelExamples;

import flixel.ui.FlxButton;
import flixel.FlxG;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class BasicExample extends FlxState {
	var loadBinary = true;

	var skeletonSprite:SkeletonSprite;
	override public function create():Void {
		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(new SequenceExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/raptor.atlas"), new FlixelTextureLoader("assets/raptor-pro.atlas"));
		var skeletondata = SkeletonData.from(loadBinary ? Assets.getBytes("assets/raptor-pro.skel") : Assets.getText("assets/raptor-pro.json"), atlas, .25);
		var animationStateData = new AnimationStateData(skeletondata);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(skeletondata, animationStateData);
		skeletonSprite.setPosition(
			.5 * FlxG.width - skeletonSprite.width / 2,
			.5 * FlxG.height - skeletonSprite.height / 2
		);

		skeletonSprite.state.setAnimationByName(0, "walk", true);

		add(skeletonSprite);

		// addText("Click anywhere for next scene");

		// addEventListener(TouchEvent.TOUCH, onTouch);
		super.create();

		FlxG.debugger.track(skeletonSprite);
		trace("loaded");
	}

	override public function update(elapsed:Float):Void
		{
			if (FlxG.keys.anyPressed([RIGHT])) {
				skeletonSprite.x += 15;
			}
			if (FlxG.keys.anyPressed([LEFT])) {
				skeletonSprite.x -= 15;
			}
			if (FlxG.keys.anyPressed([UP])) {
				skeletonSprite.y += 15;
			}
			if (FlxG.keys.anyPressed([DOWN])) {
				skeletonSprite.y -= 15;
			}

			super.update(elapsed);
		}

}
