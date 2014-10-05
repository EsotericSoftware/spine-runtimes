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

public class GoblinsExample extends Sprite {
	[Embed(source = "goblins-mesh.json", mimeType = "application/octet-stream")]
	static public const GoblinsJson:Class;
	
	[Embed(source = "goblins-mesh.atlas", mimeType = "application/octet-stream")]
	static public const GoblinsAtlas:Class;
	
	[Embed(source = "goblins-mesh.png")]
	static public const GoblinsAtlasTexture:Class;
	
	[Embed(source = "goblins-mesh-starling.xml", mimeType = "application/octet-stream")]
	static public const GoblinsStarlingAtlas:Class;
	
	[Embed(source = "goblins-mesh-starling.png")]
	static public const GoblinsStarlingAtlasTexture:Class;

	private var skeleton:SkeletonAnimation;

	public function GoblinsExample () {
		var useStarlingAtlas:Boolean = true;

		var attachmentLoader:AttachmentLoader;
		if (useStarlingAtlas) {
			var texture:Texture = Texture.fromBitmap(new GoblinsStarlingAtlasTexture());
			var xml:XML = XML(new GoblinsStarlingAtlas());
			var starlingAtlas:TextureAtlas = new TextureAtlas(texture, xml);
			attachmentLoader = new StarlingAtlasAttachmentLoader(starlingAtlas);
		} else {
			var spineAtlas:Atlas = new Atlas(new GoblinsAtlas(), new StarlingTextureLoader(new GoblinsAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);
		}

		var json:SkeletonJson = new SkeletonJson(attachmentLoader);
		var skeletonData:SkeletonData = json.readSkeletonData(new GoblinsJson());

		skeleton = new SkeletonAnimation(skeletonData, true);
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
