/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.flash {
import flash.display.Bitmap;
import flash.display.BitmapData;
import flash.display.BlendMode;
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
import spine.attachments.Attachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.SkinnedMeshAttachment;

public class SkeletonSprite extends Sprite {
	static private var tempPoint:Point = new Point();
	static private var tempMatrix:Matrix = new Matrix();
	static private var blendModes:Vector.<String> = new <String>[
		BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];

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
            var attachment:Attachment = slot.attachment;
            
			if (attachment is RegionAttachment || attachment is SkinnedMeshAttachment || attachment is MeshAttachment) {
                var wrapper:Sprite = new Sprite();
                wrapper.alpha = slot.a;
                wrapper.blendMode = blendModes[slot.data.blendMode.ordinal];
                
                var colorTransform:ColorTransform = wrapper.transform.colorTransform;
                colorTransform.redMultiplier = skeleton.r * slot.r * attachment["r"];
                colorTransform.greenMultiplier = skeleton.g * slot.g * attachment["g"];
                colorTransform.blueMultiplier = skeleton.b * slot.b * attachment["b"];
                colorTransform.alphaMultiplier = skeleton.a * slot.a * attachment["a"];
                wrapper.transform.colorTransform = colorTransform;
                
                var bitmapData:BitmapData = attachment["rendererObject"].page.rendererObject as BitmapData
                var vertices:Vector.<Number> = new Vector.<Number>();
                attachment["computeWorldVertices"](0, 0, slot, vertices);
                wrapper.graphics.beginBitmapFill(bitmapData, null, true, true);
                wrapper.graphics.drawTriangles(vertices, attachment["triangles"], attachment["uvs"]);
                wrapper.graphics.endFill();
			}
            addChild(wrapper);
		}
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}
}

}
