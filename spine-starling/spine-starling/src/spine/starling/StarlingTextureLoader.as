/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
	import starling.display.Image;

	import spine.atlas.AtlasPage;
	import spine.atlas.AtlasRegion;
	import spine.atlas.TextureLoader;

	import starling.textures.Texture;

	import flash.display.Bitmap;
	import flash.display.BitmapData;

	public class StarlingTextureLoader implements TextureLoader {
		public var bitmapDatasOrTextures : Object = {};
		public var singleBitmapDataOrTexture : Object;

		/** @param bitmaps A Bitmap or BitmapData or Texture for an atlas that has only one page, or for a multi page atlas an object where the
		 * key is the image path and the value is the Bitmap or BitmapData or Texture. */
		public function StarlingTextureLoader(bitmapsOrTextures : Object) {
			if (bitmapsOrTextures is BitmapData) {
				singleBitmapDataOrTexture = BitmapData(bitmapsOrTextures);
				return;
			}
			if (bitmapsOrTextures is Bitmap) {
				singleBitmapDataOrTexture = Bitmap(bitmapsOrTextures).bitmapData;
				return;
			}
			if (bitmapsOrTextures is Texture) {
				singleBitmapDataOrTexture = Texture(bitmapsOrTextures);
				return;
			}

			for (var path : * in bitmapsOrTextures) {
				var object : * = bitmapsOrTextures[path];
				var bitmapDataOrTexture : Object;
				if (object is BitmapData)
					bitmapDataOrTexture = BitmapData(object);
				else if (object is Bitmap)
					bitmapDataOrTexture = Bitmap(object).bitmapData;
				else if (object is Texture)
					bitmapDataOrTexture = Texture(object);
				else
					throw new ArgumentError("Object for path \"" + path + "\" must be a Bitmap, BitmapData or Texture: " + object);
				bitmapDatasOrTextures[path] = bitmapDataOrTexture;
			}
		}

		public function loadPage(page : AtlasPage, path : String) : void {
			var bitmapDataOrTexture : Object = singleBitmapDataOrTexture || bitmapDatasOrTextures[path];
			if (!bitmapDataOrTexture)
				throw new ArgumentError("BitmapData/Texture not found with name: " + path);
			if (bitmapDataOrTexture is BitmapData) {
				var bitmapData : BitmapData = BitmapData(bitmapDataOrTexture);
				page.rendererObject = Texture.fromBitmapData(bitmapData);
				page.width = bitmapData.width;
				page.height = bitmapData.height;
			} else {
				var texture : Texture = Texture(bitmapDataOrTexture);
				page.rendererObject = texture;
				page.width = texture.width;
				page.height = texture.height;
			}
		}

		public function loadRegion(region : AtlasRegion) : void {
			var image : Image = new Image(Texture(region.page.rendererObject));
			if (region.rotate) {
				image.setTexCoords(0, region.u, region.v2);
				image.setTexCoords(1, region.u, region.v);
				image.setTexCoords(2, region.u2, region.v2);
				image.setTexCoords(3, region.u2, region.v);
			} else {
				image.setTexCoords(0, region.u, region.v);
				image.setTexCoords(1, region.u2, region.v);
				image.setTexCoords(2, region.u, region.v2);
				image.setTexCoords(3, region.u2, region.v2);
			}
			region.rendererObject = image;
		}

		public function unloadPage(page : AtlasPage) : void {
			Texture(page.rendererObject).dispose();
		}
	}
}