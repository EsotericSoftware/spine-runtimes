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

package spine.flash {
	import flash.utils.Dictionary;
	import flash.display.Bitmap;
	import flash.display.BitmapData;
	import flash.display.BlendMode;
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import flash.utils.getTimer;

	import spine.Bone;
	import spine.Skeleton;
	import spine.SkeletonData;
	import spine.Slot;
	import spine.atlas.AtlasRegion;
	import spine.attachments.RegionAttachment;

	public class SkeletonSprite extends Sprite {
		static private var blendModes : Vector.<String> = new <String>[BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];
		private var _skeleton : Skeleton;
		public var timeScale : Number = 1;
		private var lastTime : int;
		private var wrappers : Dictionary = new Dictionary(true);

		public function SkeletonSprite(skeletonData : SkeletonData) {
			Bone.yDown = true;

			_skeleton = new Skeleton(skeletonData);
			_skeleton.updateWorldTransform();

			lastTime = getTimer();
			addEventListener(Event.ADDED_TO_STAGE, onAdd);
      		addEventListener(Event.REMOVED_FROM_STAGE, onRemove);
		}
		protected function onRemove(e:Event) : void {
			removeEventListener(Event.ENTER_FRAME, enterFrame);
		}
		   
		public function clearListeners() : void {
			removeEventListener(Event.ADDED_TO_STAGE, onAdd);
			removeEventListener(Event.REMOVED_FROM_STAGE, onRemove);
		}

		protected function onAdd(event:Event) : void {      
      		lastTime = getTimer();
			enterFrame(null);
			addEventListener(Event.ENTER_FRAME, enterFrame);
		}

		private function enterFrame(event : Event) : void {
			var time : int = getTimer();
			advanceTime((time - lastTime) / 1000);
			lastTime = time;
		}

		public function advanceTime(delta : Number) : void {
			_skeleton.update(delta * timeScale);

			removeChildren();
			var drawOrder : Vector.<Slot> = skeleton.drawOrder;
			for (var i : int = 0, n : int = drawOrder.length; i < n; i++) {
				var slot : Slot = drawOrder[i];
				if (!slot.bone.active) continue;
				var regionAttachment : RegionAttachment = slot.attachment as RegionAttachment;
				if (!regionAttachment) continue;

				var wrapper : Sprite = wrappers[regionAttachment];
				if (!wrapper) {
					var region : AtlasRegion = AtlasRegion(regionAttachment.rendererObject);
					var regionHeight : Number = region.rotate ? region.width : region.height;
					var regionData : BitmapData = region.rendererObject as BitmapData;
					if (!regionData) {
						var bitmapData : BitmapData = region.page.rendererObject as BitmapData;
						var regionWidth : Number = region.rotate ? region.height : region.width;
						regionData = new BitmapData(regionWidth, regionHeight);
						regionData.copyPixels(bitmapData, new Rectangle(region.x, region.y, regionWidth, regionHeight), new Point());
						region.rendererObject = regionData;
					}

					var bitmap : Bitmap = new Bitmap(regionData);
					bitmap.smoothing = true;

					// Rotate and scale using default registration point (top left corner, y-down, CW) instead of image center.
					bitmap.rotation = -regionAttachment.rotation;
					bitmap.scaleX = regionAttachment.scaleX * (regionAttachment.width / region.width);
					bitmap.scaleY = regionAttachment.scaleY * (regionAttachment.height / region.height);

					// Position using attachment translation, shifted as if scale and rotation were at image center.
					var radians : Number = -regionAttachment.rotation * Math.PI / 180;
					var cos : Number = Math.cos(radians);
					var sin : Number = Math.sin(radians);
					var shiftX : Number = -regionAttachment.width / 2 * regionAttachment.scaleX;
					var shiftY : Number = -regionAttachment.height / 2 * regionAttachment.scaleY;
					if (region.rotate) {
						bitmap.rotation += 90;
						shiftX += regionHeight * (regionAttachment.width / region.width);
					}
					bitmap.x = regionAttachment.x + shiftX * cos - shiftY * sin;
					bitmap.y = -regionAttachment.y + shiftX * sin + shiftY * cos;

					// Use bone as registration point.
					wrapper = new Sprite();
					wrapper.transform.colorTransform = new ColorTransform();
					wrapper.addChild(bitmap);
					wrappers[regionAttachment] = wrapper;
				}

				wrapper.blendMode = blendModes[slot.data.blendMode.ordinal];

				var colorTransform : ColorTransform = wrapper.transform.colorTransform;
				colorTransform.redMultiplier = skeleton.color.r * slot.color.r * regionAttachment.color.r;
				colorTransform.greenMultiplier = skeleton.color.g * slot.color.g * regionAttachment.color.g;
				colorTransform.blueMultiplier = skeleton.color.b * slot.color.b * regionAttachment.color.b;
				colorTransform.alphaMultiplier = skeleton.color.a * slot.color.a * regionAttachment.color.a;
				wrapper.transform.colorTransform = colorTransform;

				var bone : Bone = slot.bone;
				var scaleX : Number = skeleton.scaleX;
				var scaleY : Number = skeleton.scaleY;

				wrapper.x = bone.worldX;
				wrapper.y = bone.worldY;
				wrapper.rotation = bone.worldRotationX * scaleX * scaleX;
				wrapper.scaleX = bone.worldScaleX * scaleX;
				wrapper.scaleY = bone.worldScaleY * scaleY;
				addChild(wrapper);
			}
		}

		public function get skeleton() : Skeleton {
			return _skeleton;
		}
	}
}
