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
	import spine.interpolation.Pow;
	import starling.animation.IAnimatable;
	import spine.vertexeffects.SwirlEffect;
	import spine.vertexeffects.JitterEffect;
	import starling.display.DisplayObjectContainer;
	import spine.atlas.Atlas;
	import spine.*;
	import spine.attachments.AtlasAttachmentLoader;
	import spine.attachments.AttachmentLoader;
	import spine.starling.SkeletonAnimation;
	import spine.starling.StarlingTextureLoader;

	import starling.core.Starling;
	import starling.display.Sprite;
	import starling.events.Touch;
	import starling.events.TouchEvent;
	import starling.events.TouchPhase;

	public class RaptorExample extends Sprite implements IAnimatable {
		[Embed(source = "/raptor-pro.json", mimeType = "application/octet-stream")]
		static public const RaptorJson : Class;

		[Embed(source = "/raptor.atlas", mimeType = "application/octet-stream")]
		static public const RaptorAtlas : Class;

		[Embed(source = "/raptor.png")]
		static public const RaptorAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;
		private var gunGrabbed : Boolean;
		private var gunGrabCount : Number = 0;
		
		private var swirl : SwirlEffect;
		private var swirlTime : Number = 0;
		private var pow2 : Interpolation = new Pow(2);

		public function RaptorExample() {
			var attachmentLoader : AttachmentLoader;
			var spineAtlas : Atlas = new Atlas(new RaptorAtlas(), new StarlingTextureLoader(new RaptorAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

			var json : SkeletonJson = new SkeletonJson(attachmentLoader);
			json.scale = 0.5;
			var skeletonData : SkeletonData = json.readSkeletonData(new RaptorJson());

			this.x = 400;
			this.y = 560;

			skeleton = new SkeletonAnimation(skeletonData);
			skeleton.state.setAnimationByName(0, "walk", true);
			skeleton.state.update(0);
			skeleton.state.apply(skeleton.skeleton);
			skeleton.skeleton.updateWorldTransform();
			this.setRequiresRedraw();
			
			// skeleton.vertexEffect = new JitterEffect(10, 10);
			swirl = new SwirlEffect(400);
			swirl.centerY = -200;	
			skeleton.vertexEffect = swirl;

			addChild(skeleton);
			Starling.juggler.add(skeleton);
			Starling.juggler.add(this);

			addEventListener(TouchEvent.TOUCH, onClick);
		}

		private function onClick(event : TouchEvent) : void {
			var touch : Touch = event.getTouch(this);
			if (touch && touch.phase == TouchPhase.BEGAN) {				
				if (gunGrabCount < 2) {
					if (gunGrabbed)
						skeleton.skeleton.setToSetupPose();
					else
						skeleton.state.setAnimationByName(1, "gun-grab", false);
					gunGrabbed = !gunGrabbed;
					gunGrabCount++;
				} else {
					var parent: DisplayObjectContainer = this.parent;
					this.removeFromParent(true);	
					parent.addChild(new TankExample());
				}
			}
		}

		public function advanceTime(time : Number) : void {
			swirlTime += time;
			var percent : Number = swirlTime % 2;
			if (percent > 1) percent = 1 - (percent - 1);
			swirl.angle = pow2.apply(-60, 60, percent);
		}
	}
}
