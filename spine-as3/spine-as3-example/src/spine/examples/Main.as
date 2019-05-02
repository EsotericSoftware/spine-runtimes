/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.examples {
	import spine.animation.TrackEntry;
	import flash.display.Sprite;

	import spine.*;
	import spine.animation.AnimationStateData;
	import spine.atlas.Atlas;
	import spine.attachments.AtlasAttachmentLoader;
	import spine.flash.FlashTextureLoader;
	import spine.flash.SkeletonAnimation;

	[SWF(width = "800", height = "600", frameRate = "60", backgroundColor = "#dddddd")]
	public class Main extends Sprite {
		[Embed(source = "/spineboy.atlas", mimeType = "application/octet-stream")]
		static public const SpineboyAtlas : Class;

		[Embed(source = "/spineboy.png")]
		static public const SpineboyAtlasTexture : Class;

		[Embed(source = "/spineboy-ess.json", mimeType = "application/octet-stream")]
		static public const SpineboyJson : Class;
		private var skeleton : SkeletonAnimation;

		public function Main() {
			var atlas : Atlas = new Atlas(new SpineboyAtlas(), new FlashTextureLoader(new SpineboyAtlasTexture()));
			var json : SkeletonJson = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			json.scale = 0.6;
			var skeletonData : SkeletonData = json.readSkeletonData(new SpineboyJson());

			var stateData : AnimationStateData = new AnimationStateData(skeletonData);
			stateData.setMixByName("walk", "jump", 0.2);
			stateData.setMixByName("jump", "run", 0.4);
			stateData.setMixByName("jump", "jump", 0.2);

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

			if (false) {
				skeleton.state.setAnimationByName(0, "test", true);
			} else {
				skeleton.state.setAnimationByName(0, "walk", true);
				skeleton.state.addAnimationByName(0, "jump", false, 3);
				skeleton.state.addAnimationByName(0, "run", true, 0);
			}

			addChild(skeleton);
		}
	}
}
