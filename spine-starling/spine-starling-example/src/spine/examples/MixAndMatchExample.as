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
	import spine.SkeletonBinary;
	import spine.Skin;	
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

	public class MixAndMatchExample extends Sprite {
		[Embed(source = "/mix-and-match-pro.skel", mimeType = "application/octet-stream")]
		static public const MixAndMatchJson : Class;

		[Embed(source = "/mix-and-match.atlas", mimeType = "application/octet-stream")]
		static public const MixAndMatchAtlas : Class;

		[Embed(source = "/mix-and-match.png")]
		static public const MixAndMatchAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;

		public function MixAndMatchExample() {
			var attachmentLoader : AttachmentLoader;
			var spineAtlas : Atlas = new Atlas(new MixAndMatchAtlas(), new StarlingTextureLoader(new MixAndMatchAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

			var binary : SkeletonBinary = new SkeletonBinary(attachmentLoader);
			binary.scale = 0.5;
			var skeletonData : SkeletonData = binary.readSkeletonData(new MixAndMatchJson());

			this.x = 400;
			this.y = 500;

			skeleton = new SkeletonAnimation(skeletonData);
			skeleton.state.setAnimationByName(0, "dance", true);								
			
			// enable two color tinting, which breaks batching between this skeleton
			// and other Starling objects.
			skeleton.twoColorTint = true;
			
			// Create a new skin, by mixing and matching other skins
			// that fit together. Items making up the girl are individual
			// skins. Using the skin API, a new skin is created which is
			// a combination of all these individual item skins.
			var mixAndMatchSkin : Skin = new Skin("custom-girl");
			mixAndMatchSkin.addSkin(skeletonData.findSkin("skin-base"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("nose/short"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("eyelids/girly"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("eyes/violet"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("hair/brown"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("clothes/hoodie-orange"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("legs/pants-jeans"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("accessories/bag"));
			mixAndMatchSkin.addSkin(skeletonData.findSkin("accessories/hat-red-yellow"));
			skeleton.skeleton.skin = mixAndMatchSkin;
			
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
