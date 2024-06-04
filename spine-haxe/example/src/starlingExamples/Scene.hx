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

package starlingExamples;

import starling.display.Quad;
import starling.text.TextField;
import starling.core.Starling;
import starling.display.Sprite;

class SceneManager {
	private static var instance:SceneManager;

	private var currentScene:Sprite;

	private function new() {
		// Singleton pattern to ensure only one instance of SceneManager
	}

	public static function getInstance():SceneManager {
		if (instance == null) {
			instance = new SceneManager();
		}
		return instance;
	}

	public function switchScene(newScene:Scene):Void {
		if (currentScene != null) {
			currentScene.dispose();
			currentScene.removeFromParent(true);
		}
		currentScene = newScene;
		starling.core.Starling.current.stage.addChild(currentScene);
		newScene.load();
	}
}

abstract class Scene extends Sprite {
	var juggler = new starling.animation.Juggler();

	public var background:Quad;

	public function new() {
		super();
		var stageWidth = Starling.current.stage.stageWidth;
		var stageHeight = Starling.current.stage.stageHeight;
		background = new Quad(stageWidth, stageHeight, 0x0);
		this.addChild(background);
		Starling.current.juggler.add(juggler);
	}

	abstract public function load():Void;

	public override function dispose():Void {
		juggler.purge();
		Starling.current.juggler.remove(juggler);
		super.dispose();
	}

	public function addText(text:String, x:Int = 10, y:Int = 10) {
		var textField = new TextField(250, 30, text);
		textField.x = x;
		textField.y = y;
		textField.format.color = 0xffffffff;
		addChild(textField);
		return textField;
	}
}
