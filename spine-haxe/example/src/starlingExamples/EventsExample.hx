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

import spine.animation.TrackEntry;
import starlingExamples.Scene.SceneManager;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;
import spine.starling.SkeletonSprite;
import spine.starling.StarlingTextureLoader;
import starling.core.Starling;
import starling.display.DisplayObjectContainer;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.text.TextField;

class EventsExample extends Scene {
	var loadBinary = true;

	public function load():Void {
		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new StarlingTextureLoader("assets/spineboy-pro.atlas"));
		var skeletondata = SkeletonData.from(loadBinary ? Assets.getBytes("assets/spineboy-pro.skel") : Assets.getText("assets/spineboy-pro.json"), atlas, .5);
		var animationStateData = new AnimationStateData(skeletondata);
		animationStateData.defaultMix = 0.25;

		var skeletonSprite = new SkeletonSprite(skeletondata, animationStateData);
		skeletonSprite.x = Starling.current.stage.stageWidth / 2;
		skeletonSprite.y = Starling.current.stage.stageHeight * 0.8;

		// add callback to the AnimationState
		skeletonSprite.state.onStart.add(entry -> log('Started animation ${entry.animation.name}'));
		skeletonSprite.state.onInterrupt.add(entry -> log('Interrupted animation ${entry.animation.name}'));
		skeletonSprite.state.onEnd.add(entry -> log('Ended animation ${entry.animation.name}'));
		skeletonSprite.state.onDispose.add(entry -> log('Disposed animation ${entry.animation.name}'));
		skeletonSprite.state.onComplete.add(entry -> log('Completed animation ${entry.animation.name}'));

		// add callback to the TrackEntry
		skeletonSprite.state.setAnimationByName(0, "walk", true);
		var trackEntry = skeletonSprite.state.addAnimationByName(0, "run", true, 3);
		trackEntry.onEvent.add(
			(entry, event) -> log('Custom event for ${entry.animation.name}: ${event.data.name}'));

		addChild(skeletonSprite);
		juggler.add(skeletonSprite);

		addText("Click anywhere for next scene");

		addChild(textContainer);

		addEventListener(TouchEvent.TOUCH, onTouch);
	}

	private var textContainer = new DisplayObjectContainer();
	private var logs = new Array<TextField>();
	private var logsNumber = 0;
	private var yOffset = 12;
	private function log(text:String) {
		var length = logs.length;
		var newLog = new TextField(250, 30, text);
		newLog.x = 550;
		newLog.y = 20 + yOffset * logsNumber++;
		newLog.format.color = 0xffffffff;
		textContainer.addChild(newLog);
		if (logs.length < 45) {
			logs.push(newLog);
		} else {
			logs.shift().dispose();
			logs.push(newLog);
			textContainer.y -= yOffset;
		}
	}

	public function onTouch(e:TouchEvent) {
		var touch = e.getTouch(this);
		if (touch != null && touch.phase == TouchPhase.ENDED) {
			SceneManager.getInstance().switchScene(new BasicExample());
		}
	}
}
