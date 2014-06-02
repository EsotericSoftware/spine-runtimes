package {

import spine.Event;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.animation.AnimationStateData;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingAtlasAttachmentLoader;

import starling.core.Starling;
import starling.display.Sprite;
import starling.events.Touch;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class StarlingAtlasExample extends Sprite {
	[Embed(source = "spineboy-starling.xml", mimeType = "application/octet-stream")]
	static public const SpineboyAtlasXml:Class;

	[Embed(source = "spineboy-starling.png")]
	static public const SpineboyAtlasTexture:Class;

	[Embed(source = "spineboy-starling.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	private var skeleton:SkeletonAnimation;

	public function StarlingAtlasExample () {
		var texture:Texture = Texture.fromBitmap(new SpineboyAtlasTexture());
		var xml:XML = XML(new SpineboyAtlasXml());
		var atlas:TextureAtlas = new TextureAtlas(texture, xml);

		var json:SkeletonJson = new SkeletonJson(new StarlingAtlasAttachmentLoader(atlas));
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		var stateData:AnimationStateData = new AnimationStateData(skeletonData);
		stateData.setMixByName("walk", "jump", 0.2);
		stateData.setMixByName("jump", "walk", 0.4);
		stateData.setMixByName("jump", "jump", 0.2);

		skeleton = new SkeletonAnimation(skeletonData, false, stateData);
		skeleton.x = 320;
		skeleton.y = 420;
		
		skeleton.state.onStart.add(function (trackIndex:int) : void {
			trace(trackIndex + " start: " + skeleton.state.getCurrent(trackIndex));
		});
		skeleton.state.onEnd.add(function (trackIndex:int) : void {
			trace(trackIndex + " end: " + skeleton.state.getCurrent(trackIndex));
		});
		skeleton.state.onComplete.add(function (trackIndex:int, count:int) : void {
			trace(trackIndex + " complete: " + skeleton.state.getCurrent(trackIndex) + ", " + count);
		});
		skeleton.state.onEvent.add(function (trackIndex:int, event:Event) : void {
			trace(trackIndex + " event: " + skeleton.state.getCurrent(trackIndex) + ", "
				+ event.data.name + ": " + event.intValue + ", " + event.floatValue + ", " + event.stringValue);
		});

		skeleton.state.setAnimationByName(0, "walk", true);
		skeleton.state.addAnimationByName(0, "jump", false, 3);
		skeleton.state.addAnimationByName(0, "walk", true, 0);

		addChild(skeleton);
		Starling.juggler.add(skeleton);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			skeleton.state.setAnimationByName(0, "jump", false);
			skeleton.state.addAnimationByName(0, "walk", true, 0);
		}
	}
}
}
