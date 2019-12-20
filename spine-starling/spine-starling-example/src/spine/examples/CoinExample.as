/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
			this.y = 300;

			skeleton = new SkeletonAnimation(skeletonData);
			skeleton.state.setAnimationByName(0, "animation", true);
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
				parent.addChild(new MixAndMatchExample());
			}
		}
	}
}
