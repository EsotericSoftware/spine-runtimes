package spine {

import spine.atlas.Atlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.attachments.AttachmentLoader;
import spine.starling.SkeletonAnimation;
import spine.starling.StarlingAtlasAttachmentLoader;
import spine.starling.StarlingTextureLoader;

import starling.core.Starling;
import starling.display.Sprite;
import starling.events.Touch;
import starling.events.TouchEvent;
import starling.events.TouchPhase;
import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class RaptorExample extends Sprite {
	[Embed(source = "raptor.json", mimeType = "application/octet-stream")]
	static public const RaptorJson:Class;
	
	[Embed(source = "raptor.atlas", mimeType = "application/octet-stream")]
	static public const RaptorAtlas:Class;
	
	[Embed(source = "raptor.png")]
	static public const RaptorAtlasTexture:Class;
	
	private var skeleton:SkeletonAnimation;
	private var gunGrabbed:Boolean;

	public function RaptorExample () {
		var attachmentLoader:AttachmentLoader;
		var spineAtlas:Atlas = new Atlas(new RaptorAtlas(), new StarlingTextureLoader(new RaptorAtlasTexture()));
		attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

		var json:SkeletonJson = new SkeletonJson(attachmentLoader);
		json.scale = 0.5;
		var skeletonData:SkeletonData = json.readSkeletonData(new RaptorJson());

		skeleton = new SkeletonAnimation(skeletonData, true);
		skeleton.x = 320;
		skeleton.y = 560;
		skeleton.state.setAnimationByName(0, "walk", true);

		addChild(skeleton);
		Starling.juggler.add(skeleton);

		addEventListener(TouchEvent.TOUCH, onClick);
	}

	private function onClick (event:TouchEvent) : void {
		var touch:Touch = event.getTouch(this);
		if (touch && touch.phase == TouchPhase.BEGAN) {
			if (gunGrabbed)
				skeleton.skeleton.setToSetupPose();
			else
				skeleton.state.setAnimationByName(1, "gungrab", false);
			gunGrabbed = !gunGrabbed;
		}
	}
}
}
