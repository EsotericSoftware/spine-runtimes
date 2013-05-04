
package {

import flash.display.Sprite;

import spine.AnimationStateData;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.atlas.Atlas;
import spine.attachments.AtlasAttachmentLoader;
import spine.flash.SingleTextureLoader;
import spine.flash.SkeletonAnimationSprite;

[SWF(width = "640", height = "480", frameRate = "60", backgroundColor = "#dddddd")]
public class Main extends Sprite {
	[Embed(source = "spineboy.atlas", mimeType = "application/octet-stream")]
	static public const SpineboyAtlas:Class;

	[Embed(source = "spineboy.png")]
	static public const SpineboyAtlasTexture:Class;

	[Embed(source = "spineboy.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	private var skeleton:SkeletonAnimationSprite;

	public function Main () {
		var atlas:Atlas = new Atlas(new SpineboyAtlas(), new SingleTextureLoader(new SpineboyAtlasTexture()));
		var json:SkeletonJson = new SkeletonJson(new AtlasAttachmentLoader(atlas));
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		var stateData:AnimationStateData = new AnimationStateData(skeletonData);
		stateData.setMixByName("walk", "jump", 0.2);
		stateData.setMixByName("jump", "walk", 0.4);
		stateData.setMixByName("jump", "jump", 0.2);

		skeleton = new SkeletonAnimationSprite(skeletonData);
		skeleton.setAnimationStateData(stateData);
		skeleton.x = 320;
		skeleton.y = 420;
		skeleton.setAnimation("walk", true);
		skeleton.addAnimation("jump", false, 3);
		skeleton.addAnimation("walk", true);

		addChild(skeleton);
	}
}

}
