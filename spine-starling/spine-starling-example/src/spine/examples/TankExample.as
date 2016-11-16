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

package spine.examples {
import spine.atlas.Atlas;
import spine.*;
import spine.attachments.AtlasAttachmentLoader;
import spine.attachments.AttachmentLoader;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingTextureLoader;

import starling.core.Starling;
import starling.display.Sprite;

public class TankExample extends Sprite {
	[Embed(source = "/tank.json", mimeType = "application/octet-stream")]
	static public const TankJson:Class;
	
	[Embed(source = "/tank.atlas", mimeType = "application/octet-stream")]
	static public const TankAtlas:Class;
	
	[Embed(source = "/tank.png")]
	static public const TankAtlasTexture:Class;
	
	private var skeleton:SkeletonAnimation;	

	public function TankExample () {
		var attachmentLoader:AttachmentLoader;
		var spineAtlas:Atlas = new Atlas(new TankAtlas(), new StarlingTextureLoader(new TankAtlasTexture()));
		attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

		var json:SkeletonJson = new SkeletonJson(attachmentLoader);
		json.scale = 0.5;
		var skeletonData:SkeletonData = json.readSkeletonData(new TankJson());

		skeleton = new SkeletonAnimation(skeletonData);
		skeleton.x = 400;
		skeleton.y = 560;
		skeleton.state.setAnimationByName(0, "drive", true);

		addChild(skeleton);
		Starling.juggler.add(skeleton);	
	}	
}
}
