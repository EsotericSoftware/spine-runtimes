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
	import spine.Skin;
	import spine.animation.MixBlend;
	import spine.animation.TrackEntry;
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

	public class OwlExample extends Sprite {
		[Embed(source = "/owl-pro.json", mimeType = "application/octet-stream")]
		static public const OwlJson : Class;

		[Embed(source = "/owl.atlas", mimeType = "application/octet-stream")]
		static public const OwlAtlas : Class;

		[Embed(source = "/owl.png")]
		static public const OwlAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;
		
		private var left: TrackEntry;
		private var right: TrackEntry;
		private var up: TrackEntry;
		private var down: TrackEntry;

		public function OwlExample() {
			var attachmentLoader : AttachmentLoader;
			var spineAtlas : Atlas = new Atlas(new OwlAtlas(), new StarlingTextureLoader(new OwlAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

			var json : SkeletonJson = new SkeletonJson(attachmentLoader);
			json.scale = 0.5;
			var skeletonData : SkeletonData = json.readSkeletonData(new OwlJson());

			this.x = 400;
			this.y = 400;

			skeleton = new SkeletonAnimation(skeletonData);
			skeleton.state.setAnimationByName(0, "idle", true);
			skeleton.state.setAnimationByName(1, "blink", true);
			left = skeleton.state.setAnimationByName(2, "left", true);
			right = skeleton.state.setAnimationByName(3, "right", true);
			up = skeleton.state.setAnimationByName(4, "up", true);
			down = skeleton.state.setAnimationByName(5, "down", true);
			
			left.alpha = right.alpha = up.alpha = down.alpha = 0;			
			left.mixBlend = right.mixBlend = up.mixBlend = down.mixBlend = MixBlend.add;
			
			skeleton.state.timeScale = 0.5;
			skeleton.state.update(0.25);
			skeleton.state.apply(skeleton.skeleton);
			skeleton.skeleton.updateWorldTransform();

			addChild(skeleton);
			Starling.juggler.add(skeleton);

			addEventListener(TouchEvent.TOUCH, onTouch);			
		}

		private function onTouch(event : TouchEvent) : void {
			var touch : Touch = event.getTouch(this);
			if (touch && touch.phase == TouchPhase.ENDED) {
				var parent : DisplayObjectContainer = this.parent;
				this.removeFromParent(true);
				parent.addChild(new SpineboyExample());
			}
			
			if (touch && touch.phase == TouchPhase.HOVER) {
				var x : Number = touch.globalX / 800.0;
				left.alpha = (Math.max(x, 0.5) - 0.5) * 2;
				right.alpha = (0.5 - Math.min(x, 0.5)) * 2;

				var y : Number = touch.globalY / 600.0;
				down.alpha = (Math.max(y, 0.5) - 0.5) * 2;
				up.alpha = (0.5 - Math.min(y, 0.5)) * 2;
			}
		}		
	}
}
