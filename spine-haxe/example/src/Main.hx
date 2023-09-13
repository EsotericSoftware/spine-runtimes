package;

import Scene.SceneManager;
import openfl.display.Sprite;
import openfl.geom.Rectangle;
import starling.core.Starling;
import starling.events.Event;

class Main extends Sprite {
	private var starlingSingleton:Starling;

	public function new() {
		super();

		starlingSingleton = new Starling(starling.display.Sprite, stage, new Rectangle(0, 0, 800, 600));
		starlingSingleton.supportHighResolutions = true;
		starlingSingleton.addEventListener(Event.ROOT_CREATED, onStarlingRootCreated);
	}

	private function onStarlingRootCreated(event:Event):Void {
		starlingSingleton.removeEventListener(Event.ROOT_CREATED, onStarlingRootCreated);
		starlingSingleton.start();
		Starling.current.stage.color = 0x000000;

		SceneManager.getInstance().switchScene(new BasicExample());
	}
}
