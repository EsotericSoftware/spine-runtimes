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


import flixel.text.FlxText;
import flixel.math.FlxPoint;
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

class CelestialCircusExample extends FlxState {
	var loadBinary = true;

	var skeletonSprite:SkeletonSprite;
	override public function create():Void {
		FlxG.cameras.bgColor = 0xffa1b2b0;

		var button = new FlxButton(0, 0, "Next scene", () -> FlxG.switchState(() -> new SnowglobeExample()));
		button.setPosition(FlxG.width * .75, FlxG.height / 10);
		add(button);

		var atlas = new TextureAtlas(Assets.getText("assets/celestial-circus.atlas"), new FlixelTextureLoader("assets/celestial-circus.atlas"));
		var data = SkeletonData.from(loadBinary ? Assets.getBytes("assets/celestial-circus-pro.skel") : Assets.getText("assets/celestial-circus-pro.json"), atlas, .15);
		var animationStateData = new AnimationStateData(data);
		animationStateData.defaultMix = 0.25;

		skeletonSprite = new SkeletonSprite(data, animationStateData);
		skeletonSprite.screenCenter();
		skeletonSprite.state.setAnimationByName(0, "eyeblink-long", true);
		add(skeletonSprite);

		add(new FlxText(50, 50, 200, "Drag Celeste to move her around", 16));

		super.create();
	}

	var mousePosition = FlxPoint.get();
	var dragging:Bool = false;
	var lastX:Float = 0;
	var lastY:Float = 0;
	override public function update(elapsed:Float):Void
	{
		super.update(elapsed);

		mousePosition = FlxG.mouse.getPosition();

		if (FlxG.mouse.justPressed && skeletonSprite.overlapsPoint(mousePosition))
		{
			dragging = true;
			lastX = mousePosition.x;
		  	lastY = mousePosition.y;
		}

		if (FlxG.mouse.justReleased) dragging = false;

		if (dragging)
		{
			skeletonSprite.x += mousePosition.x - lastX;
			skeletonSprite.y += mousePosition.y - lastY;
			skeletonSprite.skeleton.physicsTranslate(
				mousePosition.x - lastX,
				mousePosition.y - lastY,
			);
			lastX = mousePosition.x;
            lastY = mousePosition.y;
		}

	}
}
