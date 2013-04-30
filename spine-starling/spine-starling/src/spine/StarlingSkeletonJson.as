package spine {
import flash.utils.ByteArray;

import spine.Bone;
import spine.SkeletonData;
import spine.SkeletonJson;
import spine.attachments.AttachmentLoader;

import starling.textures.TextureAtlas;

public class StarlingSkeletonJson {
	private var json:SkeletonJson;

	/** @param object A TextureAtlas or AttachmentLoader. */
	public function StarlingSkeletonJson (object:*) {
		if (object is TextureAtlas)
			json = new SkeletonJson(new StarlingAtlasAttachmentLoader(object));
		else if (object is AttachmentLoader)
			json = new SkeletonJson(AttachmentLoader(object));
		else
			throw new Error("object must be a TextureAtlas or AttachmentLoader.");

		Bone.yDown = true;
	}

	/** @param object A String or ByteArray. */
	public function readSkeletonData (object:*, name:String = null) : SkeletonData {
		if (object is String) return json.readSkeletonData(String(object), name);
		if (object is ByteArray) return json.readSkeletonData(object.readUTFBytes(object.length), name);
		throw new Error("object must be a String or ByteArray.");
	}
}

}
