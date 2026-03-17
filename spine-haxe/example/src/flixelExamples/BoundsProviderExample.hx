/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package flixelExamples;

import spine.boundsprovider.SkinsAndAnimationBoundsProvider;
import flixel.util.FlxColor;
import flixel.text.FlxText;
import spine.Skin;
import flixel.ui.FlxButton;
import flixel.FlxG;
import spine.flixel.SkeletonSprite;
import spine.flixel.FlixelTextureLoader;
import flixel.FlxState;
import openfl.utils.Assets;
import spine.SkeletonData;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;

class BoundsProviderExample extends FlxState {
	var loadBinary = true;

	override public function create():Void {
		FlxG.cameras.bgColor = 0xffa1b2b0;

		var button = new FlxButton(0, 0, "Next scene", () -> {
			FlxG.debugger.drawDebug = false;
			FlxG.switchState(() -> new ControlBonesExample());
		});
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/spineboy.atlas"), new FlixelTextureLoader("assets/spineboy.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/spineboy-pro.skel") : Assets.getText("assets/spineboy-pro.json"), atlas, .2);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		var skeletonSpriteClipping = new SkeletonSprite(data, animationStateData, new SkinsAndAnimationBoundsProvider("portal", null, true));
		skeletonSpriteClipping.state.setAnimationByName(0, "portal", true);
		skeletonSpriteClipping.screenCenter();
		skeletonSpriteClipping.x = FlxG.width / 4;
		add(skeletonSpriteClipping);

		var bounds = skeletonSpriteClipping.bounds;
		var textClipping = new FlxText();
		textClipping.text = "Bounds with clipping";
		textClipping.size = 12;
		textClipping.x = bounds.x + skeletonSpriteClipping.width / 2 - textClipping.width / 2;
		textClipping.y = bounds.y + skeletonSpriteClipping.height + 20;
		textClipping.setBorderStyle(FlxTextBorderStyle.OUTLINE, FlxColor.RED, 2);
		add(textClipping);

		var skeletonSpriteNoClipping = new SkeletonSprite(data, animationStateData, new SkinsAndAnimationBoundsProvider("portal"));
		skeletonSpriteNoClipping.state.setAnimationByName(0, "portal", true);
		skeletonSpriteNoClipping.screenCenter();
		skeletonSpriteNoClipping.x = FlxG.width / 4 * 3;
		add(skeletonSpriteNoClipping);

		var bounds = skeletonSpriteNoClipping.bounds;
		var textNoClipping = new FlxText();
		textNoClipping.text = "Bounds without clipping";
		textNoClipping.size = 12;
		textNoClipping.x = bounds.x + skeletonSpriteNoClipping.width / 2 - textNoClipping.width / 2;
		textNoClipping.y = bounds.y + skeletonSpriteNoClipping.height + 20;
		textNoClipping.setBorderStyle(FlxTextBorderStyle.OUTLINE, FlxColor.RED, 2);
		add(textNoClipping);

		var textInstruction = new FlxText();
		textInstruction.text = "Red rectangle is the Spine provider bounds";
		textInstruction.size = 12;
		textInstruction.screenCenter();
		textInstruction.y = textNoClipping.y + 40;
		textInstruction.setBorderStyle(FlxTextBorderStyle.OUTLINE, FlxColor.RED, 2);
		add(textInstruction);

		FlxG.debugger.drawDebug = true;

		super.create();
	}
}
