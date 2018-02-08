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
	import starling.display.DisplayObjectContainer;
	import starling.events.Touch;
	import starling.events.TouchPhase;
	import starling.core.Starling;
	import starling.events.TouchEvent;
	import starling.display.Sprite;

	import spine.SkeletonData;
	import spine.SkeletonJson;
	import spine.attachments.AtlasAttachmentLoader;
	import spine.starling.StarlingTextureLoader;
	import spine.atlas.Atlas;
	import spine.attachments.AttachmentLoader;
	import spine.starling.SkeletonAnimation;

	public class CoinExample extends Sprite {
		[Embed(source = "/coin-pro.json", mimeType = "application/octet-stream")]
		static public const CoinJson : Class;

		[Embed(source = "/coin.atlas", mimeType = "application/octet-stream")]
		static public const CoinAtlas : Class;

		[Embed(source = "/coin.png")]
		static public const CoinAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;

		public function CoinExample() {
			var attachmentLoader : AttachmentLoader;
			var spineAtlas : Atlas = new Atlas(new CoinAtlas(), new StarlingTextureLoader(new CoinAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

			var json : SkeletonJson = new SkeletonJson(attachmentLoader);
			json.scale = 1;
			var skeletonData : SkeletonData = json.readSkeletonData(new CoinJson());

			this.x = 400;
			this.y = 600;

			skeleton = new SkeletonAnimation(skeletonData);
			skeleton.state.setAnimationByName(0, "rotate", true);
			skeleton.state.timeScale = 0.5;
			skeleton.state.update(0.25);
			skeleton.state.apply(skeleton.skeleton);
			
			// enable two color tinting, which breaks batching between this skeleton
			// and other Starling objects.
			skeleton.twoColorTint = true;
			
			skeleton.skeleton.updateWorldTransform();

			addChild(skeleton);
			Starling.juggler.add(skeleton);

			addEventListener(TouchEvent.TOUCH, onClick);
		}

		private function onClick(event : TouchEvent) : void {
			var touch : Touch = event.getTouch(this);
			if (touch && touch.phase == TouchPhase.BEGAN) {
				var parent : DisplayObjectContainer = this.parent;
				this.removeFromParent(true);
				parent.addChild(new SpineboyExample());
			}
		}
	}
}