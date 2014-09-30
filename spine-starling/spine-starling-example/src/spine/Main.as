
package spine {

import flash.display.Sprite;

import starling.core.Starling;

[SWF(width = "800", height = "600", frameRate = "60", backgroundColor = "#dddddd")]
public class Main extends Sprite {
	private var _starling:Starling;

	public function Main () {
		var example:Class;
		//example = SpineboyExample;
		//example = GoblinsExample;
		example = RaptorExample;

		_starling = new Starling(example, stage);
		_starling.enableErrorChecking = true;
		_starling.showStats = true;
		_starling.start();
	}
}

}
