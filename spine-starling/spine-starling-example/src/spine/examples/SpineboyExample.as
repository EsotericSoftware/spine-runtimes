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
	import starling.display.Image;
	import starling.textures.Texture;
	import flash.display.BitmapData;
	import spine.attachments.BoundingBoxAttachment;
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
		[Embed(source = "/spineboy-pro.json", mimeType = "application/octet-stream")]
		static public const SpineboyJson : Class;

		[Embed(source = "/spineboy.atlas", mimeType = "application/octet-stream")]
		static public const SpineboyAtlas : Class;

		[Embed(source = "/spineboy.png")]
		static public const SpineboyAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;
		private var shape: Shape;	

		public function SpineboyExample() {
			var spineAtlas : Atlas = new Atlas(new SpineboyAtlas(), new StarlingTextureLoader(new SpineboyAtlasTexture()));
			var attachmentLoader : AttachmentLoader = new AtlasAttachmentLoader(spineAtlas);
			var json : SkeletonJson = new SkeletonJson(attachmentLoader);
			json.scale = 0.6;
			var skeletonData : SkeletonData = json.readSkeletonData(new SpineboyJson());

			var stateData : AnimationStateData = new AnimationStateData(skeletonData);
			stateData.setMixByName("walk", "run", 0.4);
			stateData.setMixByName("run", "jump", 0.4);
			stateData.setMixByName("jump", "run", 0.4);
			stateData.setMixByName("jump", "jump", 0.4);

			skeleton = new SkeletonAnimation(skeletonData, stateData);
			skeleton.x = 400;
			skeleton.y = 560;
			skeleton.scale = 0.5;

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
				trace(entry.trackIndex + " event: " + entry.animation.name + ", " + event.data.name + ": " + event.intValue + ", " + event.floatValue + ", " + event.stringValue + ", " + event.volume + ", " + event.balance);
			});

			skeleton.skeleton.setToSetupPose();
			skeleton.state.setAnimationByName(0, "walk", true);
			skeleton.state.addAnimationByName(0, "run", true, 2);
			skeleton.state.addAnimationByName(0, "jump", false, 3);
			skeleton.state.addAnimationByName(0, "run", true, 0);

			addChild(skeleton);
			Starling.juggler.add(skeleton);				
			
			shape = new Shape();
			shape.setVertices(new <Number>[0, 0, 400, 600, 800, 0]);
			shape.setColor(1, 0, 0, 1);
			addChild(shape);
			Starling.juggler.add(shape);

			addEventListener(starling.events.Event.ENTER_FRAME, onUpdate);
			addEventListener(TouchEvent.TOUCH, onClick);
		}
		
		private function onUpdate() : void {
			var slot:Slot = skeleton.skeleton.findSlot("head-bb");
			var bb:BoundingBoxAttachment = skeleton.skeleton.getAttachmentForSlotIndex(slot.data.index, "head") as BoundingBoxAttachment;
			var worldVertices:Vector.<Number> = new Vector.<Number>(bb.worldVerticesLength);
			bb.computeWorldVertices(slot, 0, bb.worldVerticesLength, worldVertices, 0, 2);
			for (var i:int = 0; i < worldVertices.length; i+=2) {
				worldVertices[i] = worldVertices[i] * skeleton.scale + skeleton.x;
				worldVertices[i + 1] = worldVertices[i + 1] * skeleton.scale + skeleton.y;
			}
			shape.setVertices(worldVertices);
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
