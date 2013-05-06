package spine.flash {
import flash.display.Bitmap;
import flash.display.BitmapData;
import flash.display.DisplayObject;
import flash.display.DisplayObjectContainer;
import flash.display.Sprite;
import flash.events.Event;
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
					bitmap.x = -regionAttachment.width / 2; // Registration point.
					bitmap.y = -regionAttachment.height / 2;
					if (region.rotate) {
						bitmap.rotation = 90;
						bitmap.x += region.width;
					}

					wrapper = new Sprite();
					wrapper.addChild(bitmap);
					regionAttachment["wrapper"] = wrapper;
				}
				var bone:Bone = slot.bone;
				var x:Number = regionAttachment.x - region.offsetX;
				var y:Number = regionAttachment.y - region.offsetY;
				wrapper.x = bone.worldX + x * bone.m00 + y * bone.m01;
				wrapper.y = bone.worldY + x * bone.m10 + y * bone.m11;
				wrapper.rotation = -(bone.worldRotation + regionAttachment.rotation);
				wrapper.scaleX = bone.worldScaleX + regionAttachment.scaleX - 1;
				wrapper.scaleY = bone.worldScaleY + regionAttachment.scaleY - 1;
				addChild(wrapper);
			}
		}
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}
}

}

