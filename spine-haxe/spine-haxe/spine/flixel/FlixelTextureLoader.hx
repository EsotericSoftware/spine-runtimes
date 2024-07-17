/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.flixel;

import flixel.graphics.FlxGraphic;
import flixel.FlxG;
import spine.atlas.TextureAtlasPage;
import spine.atlas.TextureAtlasRegion;
import spine.atlas.TextureLoader;
import spine.flixel.SpineTexture;

class FlixelTextureLoader implements TextureLoader
{
	private var basePath:String;

	public function new(prefix:String) {
		basePath = "";
		var slashIndex = prefix.lastIndexOf("/");
		if (slashIndex != -1) {
			basePath = prefix.substring(0, slashIndex);
		}
	}

	public function loadPage(page:TextureAtlasPage, path:String):Void
	{
		var bitmapData = openfl.utils.Assets.getBitmapData(basePath + "/" + path);
		if (bitmapData == null) {
			throw new SpineException("Could not load atlas page texture " + basePath + "/" + path);
		}
		var texture:FlxGraphic = SpineTexture.from(bitmapData);
		// TODO: reset this value to true when destroy skeleton
		// this is needed for sequence, otherwise the previous texture would be detroyed
		texture.destroyOnNoUse = false;
		page.texture = texture;
	}

	public function loadRegion(region:TextureAtlasRegion):Void {
		region.texture = region.page.texture;
	}

	public function unloadPage(page:TextureAtlasPage):Void
	{
		FlxG.bitmap.remove(cast page.texture);
	}
}