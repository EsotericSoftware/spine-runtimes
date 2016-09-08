/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.examples {
import spine.*;
import spine.atlas.Atlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.attachments.AttachmentLoader;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingAtlasAttachmentLoader;
import spine.starling.StarlingTextureLoader;

import starling.core.Starling;
import starling.display.Sprite;
import starling.events.Touch;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class GoblinsExample extends Sprite {
	[Embed(source = "/goblins-mesh.json", mimeType = "application/octet-stream")]
	static public const GoblinsJson:Class;
	
	[Embed(source = "/goblins.atlas", mimeType = "application/octet-stream")]
	static public const GoblinsAtlas:Class;
	
	[Embed(source = "/goblins.png")]
	static public const GoblinsAtlasTexture:Class;
	
	[Embed(source = "/goblins-mesh-starling.xml", mimeType = "application/octet-stream")]
	static public const GoblinsStarlingAtlas:Class;
	
	[Embed(source = "/goblins-mesh-starling.png")]
	static public const GoblinsStarlingAtlasTexture:Class;

	private var skeleton:SkeletonAnimation;

	public function GoblinsExample () {
		var useStarlingAtlas:Boolean = false;

		var attachmentLoader:AttachmentLoader;
		if (useStarlingAtlas) {
			var texture:Texture = Texture.fromBitmap(new GoblinsStarlingAtlasTexture());
			var xml:XML = XML(new GoblinsStarlingAtlas());
			var starlingAtlas:TextureAtlas = new TextureAtlas(texture, xml);
			attachmentLoader = new StarlingAtlasAttachmentLoader(starlingAtlas);
		} else {
			var spineAtlas:Atlas = new Atlas(new GoblinsAtlas(), new StarlingTextureLoader(new GoblinsAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);
		}

		var json:SkeletonJson = new SkeletonJson(attachmentLoader);
		var skeletonData:SkeletonData = json.readSkeletonData(new GoblinsJson());

		skeleton = new SkeletonAnimation(skeletonData);
		skeleton.x = 320;
		skeleton.y = 420;
		skeleton.skeleton.skinName = "goblin";
		skeleton.skeleton.setSlotsToSetupPose();
		skeleton.state.setAnimationByName(0, "walk", true);

		addChild(skeleton);
		Starling.juggler.add(skeleton);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			skeleton.skeleton.skinName = skeleton.skeleton.skin.name == "goblin" ? "goblingirl" : "goblin";
			skeleton.skeleton.setSlotsToSetupPose();
		}
	}
}
}
