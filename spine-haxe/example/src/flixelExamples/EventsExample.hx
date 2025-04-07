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


import flixel.text.FlxText;
import flixel.ui.FlxButton;
import flixel.FlxG;
import flixel.group.FlxSpriteGroup;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class EventsExample extends FlxState {
	var loadBinary = true;

	override public function create():Void {
		FlxG.cameras.bgColor = 0xffa1b2b0;

		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(() -> new FlixelState()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new FlixelTextureLoader("assets/spineboy.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/spineboy-pro.skel") : Assets.getText("assets/spineboy-pro.json"), atlas, .25);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(data, animationStateData);

		// add callback to the AnimationState
		skeletonSprite.state.onStart.add(entry -> log('Started animation ${entry.animation.name}'));
		skeletonSprite.state.onInterrupt.add(entry -> log('Interrupted animation ${entry.animation.name}'));
		skeletonSprite.state.onEnd.add(entry -> log('Ended animation ${entry.animation.name}'));
		skeletonSprite.state.onDispose.add(entry -> log('Disposed animation ${entry.animation.name}'));
		skeletonSprite.state.onComplete.add(entry -> log('Completed animation ${entry.animation.name}'));

		skeletonSprite.state.setAnimationByName(0, "walk", true);

		var trackEntry = skeletonSprite.state.addAnimationByName(0, "run", true, 3);
		skeletonSprite.setBoundingBox(trackEntry.animation);

		skeletonSprite.setBoundingBox();
		skeletonSprite.screenCenter();
		skeletonSprite.skeleton.setBonesToSetupPose();
		add(skeletonSprite);

		trackEntry.onEvent.add(
			(entry, event) -> log('Custom event for ${entry.animation.name}: ${event.data.name}'));


		add(textContainer);
		super.create();
	}

	private var textContainer = new FlxSpriteGroup();
	private var logs = new Array<FlxText>();
	private var logsNumber = 0;
	private var yOffset = 12;
	private function log(text:String) {
		var length = logs.length;
		var newLog = new FlxText(250, 30, text);
		newLog.x = 50;
		newLog.y = 20 + yOffset * logsNumber++;
		newLog.color = 0xffffffff;
		textContainer.add(newLog);
		if (logs.length < 35) {
			logs.push(newLog);
		} else {
			logs.shift().destroy();
			logs.push(newLog);
			textContainer.y -= yOffset;
		}
	}
}
