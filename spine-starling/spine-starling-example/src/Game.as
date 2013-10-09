package {

import spine.AnimationStateData;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.Slot;
import spine.starling.SkeletonAnimation;
import spine.starling.DisplayAttachment;
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
    private var skeleton2:SkeletonAnimation;

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
        
        var additiveStateData:AnimationStateData = new AnimationStateData(skeletonData);
        additiveStateData.additive = true;
        additiveStateData.additiveAlpha = 0.4;

		skeleton = new SkeletonAnimation(skeletonData);
		skeleton.setAnimationStateData(stateData);
        skeleton.addAnimationState(additiveStateData);
		skeleton.x = 220;
		skeleton.y = 420;
		skeleton.setAnimation("walk", true);
		skeleton.addAnimation("jump", false, 3);
		skeleton.addAnimation("walk", true);

		addChild(skeleton);
		Starling.juggler.add(skeleton);
        
        // Add the second skeleton and bind it to the first left hand.
        skeleton2 = new SkeletonAnimation(skeletonData);
        skeleton2.setAnimationStateData(stateData);
        skeleton2.setAnimation("walk", true);
        
        var slot:Slot = skeleton.skeleton.findSlot("left hand");
        slot.attachment = new DisplayAttachment("right hand attachment", skeleton2);

        Starling.juggler.add(skeleton2);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			skeleton.setAnimation("jump", false, 1);
			//skeleton.addAnimation("walk", true);
		}
	}
}
}
