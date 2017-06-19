/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
 
package spine.examples {
	import spine.*;
	import spine.animation.AnimationStateData;
	import spine.animation.TrackEntry;
	import spine.atlas.Atlas;
	import spine.attachments.AtlasAttachmentLoader;
	import spine.attachments.AttachmentLoader;
	import spine.starling.SkeletonAnimation;
	import spine.starling.StarlingTextureLoader;

	import starling.core.Starling;
	import starling.display.DisplayObjectContainer;
	import starling.display.Sprite;
	import starling.events.Touch;
	import starling.events.TouchEvent;
	import starling.events.TouchPhase;

	public class SpineboyExample extends Sprite {
		[Embed(source = "/spineboy-ess.json", mimeType = "application/octet-stream")]
		static public const SpineboyJson : Class;

		[Embed(source = "/spineboy.atlas", mimeType = "application/octet-stream")]
		static public const SpineboyAtlas : Class;

		[Embed(source = "/spineboy.png")]
		static public const SpineboyAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;		

		public function SpineboyExample() {
			var spineAtlas : Atlas = new Atlas(new SpineboyAtlas(), new StarlingTextureLoader(new SpineboyAtlasTexture()));
			var attachmentLoader : AttachmentLoader = new AtlasAttachmentLoader(spineAtlas);
			var json : SkeletonJson = new SkeletonJson(attachmentLoader);
			json.scale = 0.6;
			var skeletonData : SkeletonData = json.readSkeletonData(new SpineboyJson());

			var stateData : AnimationStateData = new AnimationStateData(skeletonData);
			stateData.setMixByName("run", "jump", 0.4);
			stateData.setMixByName("jump", "run", 0.4);
			stateData.setMixByName("jump", "jump", 0.4);

			skeleton = new SkeletonAnimation(skeletonData, stateData);
			skeleton.x = 400;
			skeleton.y = 560;

			skeleton.state.onStart.add(function(entry : TrackEntry) : void {
				trace(entry.trackIndex + " start: " + entry.animation.name);
			});
			skeleton.state.onInterrupt.add(function(entry : TrackEntry) : void {
				trace(entry.trackIndex + " interrupt: " + entry.animation.name);
			});
			skeleton.state.onEnd.add(function(entry : TrackEntry) : void {
				trace(entry.trackIndex + " end: " + entry.animation.name);
			});
			skeleton.state.onComplete.add(function(entry : TrackEntry) : void {
				trace(entry.trackIndex + " complete: " + entry.animation.name);
			});
			skeleton.state.onDispose.add(function(entry : TrackEntry) : void {
				trace(entry.trackIndex + " dispose: " + entry.animation.name);
			});
			skeleton.state.onEvent.add(function(entry : TrackEntry, event : Event) : void {
				trace(entry.trackIndex + " event: " + entry.animation.name + ", " + event.data.name + ": " + event.intValue + ", " + event.floatValue + ", " + event.stringValue);
			});

			skeleton.skeleton.setToSetupPose();
			skeleton.state.setAnimationByName(0, "run", true);
			skeleton.state.addAnimationByName(0, "jump", false, 3);
			skeleton.state.addAnimationByName(0, "run", true, 0);

			addChild(skeleton);
			Starling.juggler.add(skeleton);

			addEventListener(TouchEvent.TOUCH, onClick);
		}

		private function onClick(event : TouchEvent) : void {
			var touch : Touch = event.getTouch(this);
			if (touch && touch.phase == TouchPhase.BEGAN) {
				var parent: DisplayObjectContainer = this.parent;
				this.removeFromParent(true);			
				parent.addChild(new GoblinsExample());				
			}
		}
	}
}