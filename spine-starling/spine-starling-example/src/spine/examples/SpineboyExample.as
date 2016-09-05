/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.examples {
import spine.animation.AnimationStateData;
import spine.*;
import spine.atlas.Atlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.attachments.AttachmentLoader;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingTextureLoader;

import starling.core.Starling;
import starling.display.Sprite;
import starling.events.Touch;
import starling.events.TouchEvent;
import starling.events.TouchPhase;

public class SpineboyExample extends Sprite {
	[Embed(source = "/spineboy.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	[Embed(source = "/spineboy.atlas", mimeType = "application/octet-stream")]
	static public const SpineboyAtlas:Class;

	[Embed(source = "/spineboy.png")]
	static public const SpineboyAtlasTexture:Class;

	private var skeleton:SkeletonAnimation;

	public function SpineboyExample () {
		var spineAtlas:Atlas = new Atlas(new SpineboyAtlas(), new StarlingTextureLoader(new SpineboyAtlasTexture()));
		var attachmentLoader:AttachmentLoader = new AtlasAttachmentLoader(spineAtlas);
		var json:SkeletonJson = new SkeletonJson(attachmentLoader);
		json.scale = 0.6;
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		var stateData:AnimationStateData = new AnimationStateData(skeletonData);
		stateData.setMixByName("run", "jump", 0.2);
		stateData.setMixByName("jump", "run", 0.4);
		stateData.setMixByName("jump", "jump", 0.2);

		skeleton = new SkeletonAnimation(skeletonData, stateData);
		skeleton.x = 400;
		skeleton.y = 560;
		
		skeleton.state.onStart.add(function (trackIndex:int) : void {
			trace(trackIndex + " start: " + skeleton.state.getCurrent(trackIndex));
		});
		skeleton.state.onEnd.add(function (trackIndex:int) : void {
			trace(trackIndex + " end: " + skeleton.state.getCurrent(trackIndex));
		});
		skeleton.state.onComplete.add(function (trackIndex:int, count:int) : void {
			trace(trackIndex + " complete: " + skeleton.state.getCurrent(trackIndex) + ", " + count);
		});
		skeleton.state.onEvent.add(function (trackIndex:int, event:Event) : void {
			trace(trackIndex + " event: " + skeleton.state.getCurrent(trackIndex) + ", "
				+ event.data.name + ": " + event.intValue + ", " + event.floatValue + ", " + event.stringValue);
		});

		skeleton.skeleton.setToSetupPose();
		skeleton.state.setAnimationByName(0, "run", true);
		skeleton.state.addAnimationByName(0, "jump", false, 3);
		skeleton.state.addAnimationByName(0, "run", true, 0);

		addChild(skeleton);
		Starling.juggler.add(skeleton);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			skeleton.state.setAnimationByName(0, "jump", false);
			skeleton.state.addAnimationByName(0, "run", true, 0);
		}
	}
}
}
