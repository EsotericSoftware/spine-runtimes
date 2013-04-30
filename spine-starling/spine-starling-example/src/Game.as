package {

import spine.SkeletonAnimationSprite;
import spine.SkeletonData;
import spine.StarlingSkeletonJson;

import starling.display.Sprite;
import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class Game extends Sprite {
	[Embed(source = "spineboy.xml", mimeType = "application/octet-stream")]
	static public const SpineboyAtlasXml:Class;
	
	[Embed(source = "spineboy.png")]
	static public const SpineboyAtlasTexture:Class;
	
	[Embed(source = "spineboy.json", mimeType = "application/octet-stream")]
	static public const SpineboyJson:Class;

	public function Game () {
		var texture:Texture = Texture.fromBitmap(new SpineboyAtlasTexture());
		var xml:XML = XML(new SpineboyAtlasXml());
		var atlas:TextureAtlas = new TextureAtlas(texture, xml);

		var json:StarlingSkeletonJson = new StarlingSkeletonJson(atlas);
		var skeletonData:SkeletonData = json.readSkeletonData(new SpineboyJson());

		var skeleton:SkeletonAnimationSprite = new SkeletonAnimationSprite(skeletonData);
		skeleton.x = 320;
		skeleton.y = 420;
		skeleton.width = 100;
		skeleton.height = 100;
		skeleton.setAnimation("walk", true);
		addChild(skeleton);
	}
}
}
