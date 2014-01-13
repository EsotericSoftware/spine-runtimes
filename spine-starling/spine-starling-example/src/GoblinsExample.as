package {

import spine.Event;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.animation.AnimationStateData;
import spine.atlas.Atlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.starling.StarlingTextureLoader;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingAtlasAttachmentLoader;

import starling.core.Starling;
import starling.display.Sprite;
import starling.events.Touch;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class GoblinsExample extends Sprite {
	[Embed(source = "goblins.atlas", mimeType = "application/octet-stream")]
	static public const SpineboyAtlasFile:Class;

	[Embed(source = "goblins.png")]
	static public const SpineboyAtlasTexture:Class;

	[Embed(source = "goblins.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	private var skeleton:SkeletonAnimation;

	public function GoblinsExample () {
		var atlas:Atlas = new Atlas(new SpineboyAtlasFile(), new StarlingTextureLoader(new SpineboyAtlasTexture()));
		var json:SkeletonJson = new SkeletonJson(new AtlasAttachmentLoader(atlas));
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		skeleton = new SkeletonAnimation(skeletonData);
		skeleton.x = 320;
		skeleton.y = 420;
		skeleton.skeleton.skinName = "goblin";
		skeleton.skeleton.setSlotsToSetupPose();
		skeleton.state.setAnimationByName(0, "walk", true);

		addChild(skeleton);
		Starling.juggler.add(skeleton);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			skeleton.skeleton.skinName = skeleton.skeleton.skin.name == "goblin" ? "goblingirl" : "goblin";
			skeleton.skeleton.setSlotsToSetupPose();
		}
	}
}
}
