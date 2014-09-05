
package {

import flash.display.Sprite;

import starling.core.Starling;

[SWF(width = "640", height = "480", frameRate = "60", backgroundColor = "#dddddd")]
public class Main extends Sprite {
	private var _starling:Starling;
	static public var useStarlingAtlas:Boolean;

	public function Main () {
		var example:Class;
		//example = SpineboyExample;
		example = GoblinsExample;
		useStarlingAtlas = true;

		_starling = new Starling(example, stage);
		_starling.enableErrorChecking = true;
		_starling.showStats = true;
		_starling.start();
	}
}

}
