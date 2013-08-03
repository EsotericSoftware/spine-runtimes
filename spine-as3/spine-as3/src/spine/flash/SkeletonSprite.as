package spine.flash {
import flash.display.Bitmap;
import flash.display.BitmapData;
import flash.display.DisplayObject;
import flash.display.DisplayObjectContainer;
import flash.display.Sprite;
import flash.events.Event;
import flash.geom.ColorTransform;
import flash.geom.Matrix;
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
	static private var tempPoint:Point = new Point();
	static private var tempMatrix:Matrix = new Matrix();

	private var _skeleton:Skeleton;
	public var timeScale:Number = 1;
	private var lastTime:int;

	public function SkeletonSprite (skeletonData:SkeletonData) {
		Bone.yDown = true;

		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();

		addEventListener(Event.ENTER_FRAME, enterFrame);
	}

	private function enterFrame (event:Event) : void {
		var time:int = getTimer();
		advanceTime((time - lastTime) / 1000);
		lastTime = time;
	}

	public function advanceTime (delta:Number) : void {
		_skeleton.update(delta * timeScale);

		removeChildren();
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		for (var i:int = 0, n:int = drawOrder.length; i < n; i++) {
			var slot:Slot = drawOrder[i];
			var regionAttachment:RegionAttachment = slot.attachment as RegionAttachment;
			if (regionAttachment != null) {
				var wrapper:Sprite = regionAttachment["wrapper"];
				var region:AtlasRegion = AtlasRegion(regionAttachment.rendererObject);
				if (!wrapper) {
					var bitmapData:BitmapData = region.page.rendererObject as BitmapData;
					var regionData:BitmapData;
					if (region.rotate) {
						regionData = new BitmapData(region.height, region.width);
						regionData.copyPixels(bitmapData, //
							new Rectangle(region.x, region.y, region.height, region.width), //
							new Point());
					} else {
						regionData = new BitmapData(region.width, region.height);
						regionData.copyPixels(bitmapData, //
							new Rectangle(region.x, region.y, region.width, region.height), //
							new Point());
					}

					var bitmap:Bitmap = new Bitmap(regionData);
					bitmap.smoothing = true;

					// Rotate and scale using default registration point (top left corner, y-down, CW) instead of image center.
					bitmap.rotation = -regionAttachment.rotation;
					bitmap.scaleX = regionAttachment.scaleX;
					bitmap.scaleY = regionAttachment.scaleY;

					// Position using attachment translation, shifted as if scale and rotation were at image center.
					var radians:Number = -regionAttachment.rotation * Math.PI / 180;
					var cos:Number = Math.cos(radians);
					var sin:Number = Math.sin(radians);
					var shiftX:Number = -regionAttachment.width / 2 * regionAttachment.scaleX;
					var shiftY:Number = -regionAttachment.height / 2 * regionAttachment.scaleY;
					if (region.rotate) {
						bitmap.rotation += 90;
						shiftX += region.width;
					}
					bitmap.x = regionAttachment.x + shiftX * cos - shiftY * sin;
					bitmap.y = -regionAttachment.y + shiftX * sin + shiftY * cos;

					// Use bone as registration point.
					wrapper = new Sprite();
					wrapper.transform.colorTransform = new ColorTransform();
					wrapper.addChild(bitmap);
					regionAttachment["wrapper"] = wrapper;
				}

				var colorTransform:ColorTransform = wrapper.transform.colorTransform;
				colorTransform.redMultiplier = skeleton.r * slot.r;
				colorTransform.greenMultiplier = skeleton.g * slot.g;
				colorTransform.blueMultiplier = skeleton.b * slot.b;
				colorTransform.alphaMultiplier = skeleton.a * slot.a;
				wrapper.transform.colorTransform = colorTransform;

				var bone:Bone = slot.bone;
				wrapper.x = bone.worldX;
				wrapper.y = bone.worldY;
				wrapper.rotation = -bone.worldRotation;
				wrapper.scaleX = bone.worldScaleX;
				wrapper.scaleY = bone.worldScaleY;
				addChild(wrapper);
			}
		}
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}
}

}

