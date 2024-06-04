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

package;

import starlingExample.BasicExample;
import starlingExample.Scene.SceneManager;
import starling.core.Starling;
import flixel.FlxG;
import flixel.FlxGame;

import openfl.display.Sprite;
import openfl.text.TextField;
import openfl.text.TextFormat;
import openfl.events.MouseEvent;

import openfl.geom.Rectangle;
import starling.events.Event;

class Main extends Sprite {
    private var background:Sprite;
	private var flixelButton:Sprite;
    private var starlingButton:Sprite;
	private var uiContainer:Sprite;

	private static inline var ratio = 4;
    private static inline var STAGE_WIDTH:Int = 100 * ratio;
    private static inline var STAGE_HEIGHT:Int = 200 * ratio;
    private static inline var BUTTON_WIDTH:Int = 80 * ratio;
    private static inline var BUTTON_HEIGHT:Int = 40 * ratio;
    private static inline var BUTTON_SPACING:Int = 20 * ratio;

    public function new() {
        super();
        addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
    }

    private function onAddedToStage(e:Event):Void {
        removeEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
        createUI();
        centerUI();
        stage.addEventListener(Event.RESIZE, onResize);
    }

    private function createUI():Void {
        uiContainer = new Sprite();
        addChild(uiContainer);

        background = new Sprite();
        background.graphics.beginFill(0xA2A2A2);
        background.graphics.drawRect(0, 0, STAGE_WIDTH, STAGE_HEIGHT);
        background.graphics.endFill();
        uiContainer.addChild(background);

        flixelButton = createButton("Flixel", 0xFF0000);
        uiContainer.addChild(flixelButton);

        starlingButton = createButton("Starling", 0x00FF00);
        uiContainer.addChild(starlingButton);

        positionButtons();

        flixelButton.addEventListener(MouseEvent.CLICK, onFlixelClick);
        starlingButton.addEventListener(MouseEvent.CLICK, onStarlingClick);
    }

    private function createButton(label:String, color:Int):Sprite {
		var button = new Sprite();
		var g = button.graphics;

		g.beginFill(color);
		g.drawRoundRect(0, 0, BUTTON_WIDTH, BUTTON_HEIGHT, 10, 10);
		g.endFill();

		// Add button text
		var tf = new TextField();
		var format = new TextFormat("_sans", 14 * ratio, 0x000000, true, null, null, null, null, "center");
		tf.defaultTextFormat = format;
		tf.text = label;
		tf.width = BUTTON_WIDTH;
		tf.height = BUTTON_HEIGHT;
		tf.mouseEnabled = false;
		tf.selectable = false;

		tf.y = (BUTTON_HEIGHT - tf.textHeight) / 2;

		button.addChild(tf);

		return button;
	}

    private function positionButtons():Void {
        var totalHeight = (BUTTON_HEIGHT * 2) + BUTTON_SPACING;
        var startY = (STAGE_HEIGHT - totalHeight) / 2;

        flixelButton.x = (STAGE_WIDTH - BUTTON_WIDTH) / 2;
        flixelButton.y = startY + BUTTON_HEIGHT + BUTTON_SPACING;

        starlingButton.x = (STAGE_WIDTH - BUTTON_WIDTH) / 2;
        starlingButton.y = startY;
    }

	private function centerUI():Void {
        uiContainer.x = (stage.stageWidth - STAGE_WIDTH) / 2;
        uiContainer.y = (stage.stageHeight - STAGE_HEIGHT) / 2;
    }

    private function onResize(e:Event):Void {
        centerUI();
    }

    private function onFlixelClick(e:MouseEvent):Void {
        trace("Launching Flixel game");
		destroyUI();
		addChild(new FlxGame(640, 480, FlixelState));
		FlxG.autoPause = false;
    }

	private function destroyUI():Void {
        flixelButton.removeEventListener(MouseEvent.CLICK, onFlixelClick);
        starlingButton.removeEventListener(MouseEvent.CLICK, onStarlingClick);
        stage.removeEventListener(Event.RESIZE, onResize);

        removeChild(uiContainer);

        background = null;
        flixelButton = null;
        starlingButton = null;
        uiContainer = null;
    }

	private var starlingSingleton:Starling;
    private function onStarlingClick(e:MouseEvent):Void {
        trace("Launching Starling game");
		starlingSingleton = new Starling(starling.display.Sprite, stage, new Rectangle(0, 0, 800, 600));
		starlingSingleton.supportHighResolutions = true;
		starlingSingleton.addEventListener(Event.ROOT_CREATED, onStarlingRootCreated);
    }

	private function onStarlingRootCreated(event:Event):Void {
		destroyUI();
		starlingSingleton.removeEventListener(Event.ROOT_CREATED, onStarlingRootCreated);
		starlingSingleton.start();
		Starling.current.stage.color = 0x000000;

		SceneManager.getInstance().switchScene(new BasicExample());
	}
}