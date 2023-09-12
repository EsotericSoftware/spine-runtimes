package spine.attachments;

import openfl.errors.ArgumentError;
import spine.atlas.TextureAtlas;
import spine.Skin;

class AtlasAttachmentLoader implements AttachmentLoader {
	private var atlas:TextureAtlas;

	public function new(atlas:TextureAtlas) {
		if (atlas == null) {
			throw new ArgumentError("atlas cannot be null.");
		}
		this.atlas = atlas;
	}

	private function loadSequence(name:String, basePath:String, sequence:Sequence) {
		var regions = sequence.regions;
		for (i in 0...regions.length) {
			var path = sequence.getPath(basePath, i);
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new SpineException("Region not found in atlas: " + path + " (sequence: " + name + ")");
			regions[i] = region;
		}
	}

	public function newRegionAttachment(skin:Skin, name:String, path:String, sequence:Sequence):RegionAttachment {
		var attachment = new RegionAttachment(name, path);
		if (sequence != null) {
			this.loadSequence(name, path, sequence);
		} else {
			var region = this.atlas.findRegion(path);
			if (region == null)
				throw new SpineException("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			attachment.region = region;
		}
		return attachment;
	}

	public function newMeshAttachment(skin:Skin, name:String, path:String, sequence:Sequence):MeshAttachment {
		var attachment = new MeshAttachment(name, path);
		if (sequence != null) {
			this.loadSequence(name, path, sequence);
		} else {
			var region = atlas.findRegion(path);
			if (region == null)
				throw new SpineException("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			attachment.region = region;
		}
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
}
