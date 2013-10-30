/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
import flash.display.Bitmap;
import flash.display.BitmapData;
import flash.geom.Point;
import flash.geom.Rectangle;

import spine.atlas.AtlasPage;
import spine.atlas.AtlasRegion;
import spine.atlas.TextureLoader;

import starling.textures.SubTexture;
import starling.textures.Texture;

public class SingleTextureLoader implements TextureLoader {
	private var pageBitmapData:BitmapData;
	
	/** @param object A Bitmap or BitmapData. */
	public function SingleTextureLoader (object:*) {
		if (object is BitmapData)
			pageBitmapData = BitmapData(object);
		else if (object is Bitmap)
			pageBitmapData = Bitmap(object).bitmapData;
		else
			throw new ArgumentError("object must be a Bitmap or BitmapData.");
	}
	
	public function loadPage (page:AtlasPage, path:String) : void {
		page.rendererObject = Texture.fromBitmapData(pageBitmapData);
		page.width = pageBitmapData.width;
		page.height = pageBitmapData.height;
	}

	public function loadRegion (region:AtlasRegion) : void {
		var image:SkeletonImage = new SkeletonImage(Texture(region.page.rendererObject));
		if (region.rotate) {
			image.setTexCoordsTo(0, region.u, region.v2);
			image.setTexCoordsTo(1, region.u, region.v);
			image.setTexCoordsTo(2, region.u2, region.v2);
			image.setTexCoordsTo(3, region.u2, region.v);
		} else {
			image.setTexCoordsTo(0, region.u, region.v);
			image.setTexCoordsTo(1, region.u2, region.v);
			image.setTexCoordsTo(2, region.u, region.v2);
			image.setTexCoordsTo(3, region.u2, region.v2);
		}
		region.rendererObject = image;
	}
	
	public function unloadPage (page:AtlasPage) : void {
		BitmapData(pageBitmapData).dispose();
	}
}

}
