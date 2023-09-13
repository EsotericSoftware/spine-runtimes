import starling.utils.Color;
import starling.display.Quad;
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

	public function new() {
		super();
		Starling.current.juggler.add(juggler);
	}

	abstract public function load():Void;

	public override function dispose():Void {
		juggler.purge();
		Starling.current.juggler.remove(juggler);
		super.dispose();
	}
}
