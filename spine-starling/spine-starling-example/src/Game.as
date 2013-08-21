package {

import spine.AnimationStateData;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingAtlasAttachmentLoader;

import starling.core.Starling;
import starling.display.Sprite;
import starling.events.Touch;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class Game extends Sprite {
	[Embed(source = "spineboy.xml", mimeType = "application/octet-stream")]
	static public const SpineboyAtlasXml:Class;

	[Embed(source = "spineboy.png")]
	static public const SpineboyAtlasTexture:Class;

	[Embed(source = "spineboy.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	private var skeleton:SkeletonAnimation;

	public function Game () {
		var texture:Texture = Texture.fromBitmap(new SpineboyAtlasTexture());
		var xml:XML = XML(new SpineboyAtlasXml());
		var atlas:TextureAtlas = new TextureAtlas(texture, xml);

		var json:SkeletonJson = new SkeletonJson(new StarlingAtlasAttachmentLoader(atlas));
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		var stateData:AnimationStateData = new AnimationStateData(skeletonData);
		stateData.setMixByName("walk", "jump", 0.2);
		stateData.setMixByName("jump", "walk", 0.4);
		stateData.setMixByName("jump", "jump", 0.2);

		skeleton = new SkeletonAnimation(skeletonData);
		skeleton.setAnimationStateData(stateData);
		skeleton.x = 320;
		skeleton.y = 420;
		skeleton.setAnimation("walk", true);
		skeleton.addAnimation("jump", false, 3);
		skeleton.addAnimation("walk", true);

		addChild(skeleton);
		Starling.juggler.add(skeleton);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			skeleton.setAnimation("jump", false);
			skeleton.addAnimation("walk", true);
		}
	}
}
}
