package;

import starling.display.Image;
import haxe.io.Bytes;
import openfl.display.Bitmap;
import openfl.display.BitmapData;
import openfl.display.Sprite;
import openfl.Assets;
import openfl.geom.Rectangle;
import openfl.utils.ByteArray;
import openfl.utils.Endian;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.SkeletonBinary;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingTextureLoader;
import starling.core.Starling;
import starling.events.Event;
import starling.textures.Texture;

class Main extends Sprite {
	private static inline var loadBinary:Bool = false;

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

		loadSpineAnimation();
	}

	private function loadSpineAnimation():Void {
		var textureAtlasBitmapData:BitmapData = Assets.getBitmapData("assets/raptor.png");
		var stAtlas = Assets.getText("assets/raptor.atlas");
		var binaryData = Assets.getBytes("assets/raptor-pro.skel");
		var jsonData = Assets.getText("assets/raptor-pro.json");

		var textureAtlas = Texture.fromBitmapData(textureAtlasBitmapData);
		var textureloader = new StarlingTextureLoader(textureAtlas);
		var atlas = new TextureAtlas(stAtlas, textureloader);

		var skeletondata:SkeletonData;
		if (loadBinary) {
			var skeletonBinary:SkeletonBinary = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
			var bytearray:ByteArray = ByteArray.fromBytes(binaryData);
			bytearray.endian = Endian.BIG_ENDIAN;
			skeletondata = skeletonBinary.readSkeletonData(bytearray);
		} else {
			var skeletonJson:SkeletonJson = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			skeletondata = skeletonJson.readSkeletonData(jsonData);
		}

		var stateData:AnimationStateData = new AnimationStateData(skeletondata);
		stateData.defaultMix = 0.25;

		var skeletonanimation:SkeletonAnimation = new SkeletonAnimation(skeletondata, stateData);
		skeletonanimation.x = Starling.current.stage.stageWidth / 2;
		skeletonanimation.y = Starling.current.stage.stageHeight * 0.5;

		Starling.current.stage.addChild(skeletonanimation);
		Starling.current.juggler.add(skeletonanimation);
		skeletonanimation.state.setAnimationByName(0, "walk", true);
	}
}
