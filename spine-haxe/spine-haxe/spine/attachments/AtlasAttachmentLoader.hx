package spine.attachments;

import openfl.errors.ArgumentError;
import spine.atlas.Atlas;
import spine.atlas.AtlasRegion;
import spine.Skin;

class AtlasAttachmentLoader implements AttachmentLoader {
	private var atlas:Atlas;

	public function new(atlas:Atlas) {
		if (atlas == null) {
			throw new ArgumentError("atlas cannot be null.");
		}
		this.atlas = atlas;
	}

	public function newRegionAttachment(skin:Skin, name:String, path:String):RegionAttachment {
		var region:AtlasRegion = atlas.findRegion(path);
		if (region == null) {
			trace("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			return null;
		}
		var attachment:RegionAttachment = new RegionAttachment(name);
		attachment.rendererObject = region;
		attachment.setUVs(region.u, region.v, region.u2, region.v2, region.degrees);
		attachment.regionOffsetX = region.offsetX;
		attachment.regionOffsetY = region.offsetY;
		attachment.regionWidth = region.width;
		attachment.regionHeight = region.height;
		attachment.regionOriginalWidth = region.originalWidth;
		attachment.regionOriginalHeight = region.originalHeight;
		return attachment;
	}

	public function newMeshAttachment(skin:Skin, name:String, path:String):MeshAttachment {
		var region:AtlasRegion = atlas.findRegion(path);
		if (region == null) {
			trace("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			return null;
		}

		var attachment:MeshAttachment = new MeshAttachment(name);
		attachment.rendererObject = region;
		attachment.regionU = region.u;
		attachment.regionV = region.v;
		attachment.regionU2 = region.u2;
		attachment.regionV2 = region.v2;
		attachment.regionDegrees = region.degrees;
		attachment.regionOffsetX = region.offsetX;
		attachment.regionOffsetY = region.offsetY;
		attachment.regionWidth = region.width;
		attachment.regionHeight = region.height;
		attachment.regionOriginalWidth = region.originalWidth;
		attachment.regionOriginalHeight = region.originalHeight;
		return attachment;
	}

	public function newBoundingBoxAttachment(skin:Skin, name:String):BoundingBoxAttachment {
		return new BoundingBoxAttachment(name);
	}

	public function newPathAttachment(skin:Skin, name:String):PathAttachment {
		return new PathAttachment(name);
	}

	public function newPointAttachment(skin:Skin, name:String):PointAttachment {
		return new PointAttachment(name);
	}

	public function newClippingAttachment(skin:Skin, name:String):ClippingAttachment {
		return new ClippingAttachment(name);
	}

	static public function nextPOT(value:Int):Int {
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		return value + 1;
	}
}
