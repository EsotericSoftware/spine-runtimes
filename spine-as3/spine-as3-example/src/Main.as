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
 * 1. A Spine Single User License, Professional License, or Education License must
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

package {

import flash.display.Sprite;

import spine.AnimationStateData;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.atlas.Atlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.flash.SingleTextureLoader;
import spine.flash.SkeletonAnimation;

[SWF(width = "640", height = "480", frameRate = "60", backgroundColor = "#dddddd")]
public class Main extends Sprite {
	[Embed(source = "spineboy.atlas", mimeType = "application/octet-stream")]
	static public const SpineboyAtlas:Class;

	[Embed(source = "spineboy.png")]
	static public const SpineboyAtlasTexture:Class;

	[Embed(source = "spineboy.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	private var skeleton:SkeletonAnimation;

	public function Main () {
		var atlas:Atlas = new Atlas(new SpineboyAtlas(), new SingleTextureLoader(new SpineboyAtlasTexture()));
		var json:SkeletonJson = new SkeletonJson(new AtlasAttachmentLoader(atlas));
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		var stateData:AnimationStateData = new AnimationStateData(skeletonData);
		stateData.setMixByName("walk", "jump", 0.2);
		stateData.setMixByName("jump", "walk", 0.4);
		stateData.setMixByName("jump", "jump", 0.2);

		skeleton = new SkeletonAnimation(skeletonData);
		skeleton.setAnimationStateData(stateData);
		skeleton.x = 320;
		skeleton.y = 420;
		if (true) {
			skeleton.setAnimation("drawOrder", true);
		} else {
			skeleton.setAnimation("walk", true);
			skeleton.addAnimation("jump", false, 3);
			skeleton.addAnimation("walk", true);
		}

		addChild(skeleton);
	}
}

}
