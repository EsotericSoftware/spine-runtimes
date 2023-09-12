package spine.attachments;

import openfl.Vector;
import spine.Color;
import spine.atlas.TextureAtlasRegion;

class MeshAttachment extends VertexAttachment implements HasTextureRegion {
	public var region:TextureRegion;
	public var path:String;
	public var regionUVs = new Vector<Float>();
	public var uvs = new Vector<Float>();
	public var triangles = new Vector<Int>();
	public var color:Color = new Color(1, 1, 1, 1);
	public var width:Float = 0;
	public var height:Float = 0;
	public var hullLength:Int = 0;
	public var edges = new Vector<Int>();
	public var rendererObject:Dynamic;
	public var sequence:Sequence;

	private var _parentMesh:MeshAttachment;

	public function new(name:String, path:String) {
		super(name);
		this.path = path;
	}

	public function updateRegion():Void {
		if (region == null) {
			throw new SpineException("Region not set.");
			return;
		}
		var regionUVs = this.regionUVs;
		if (uvs.length != regionUVs.length)
			uvs = new Vector<Float>(regionUVs.length, true);
		var n = uvs.length;
		var u = region.u, v = region.v, width:Float = 0, height:Float = 0;
		if (Std.isOfType(region, TextureAtlasRegion)) {
			var atlasRegion:TextureAtlasRegion = cast(region, TextureAtlasRegion);
			var textureWidth = atlasRegion.page.width,
				textureHeight = atlasRegion.page.height;
			switch (atlasRegion.degrees) {
				case 90:
					u -= (region.originalHeight - region.offsetY - region.height) / textureWidth;
					v -= (region.originalWidth - region.offsetX - region.width) / textureHeight;
					width = region.originalHeight / textureWidth;
					height = region.originalWidth / textureHeight;
					var i = 0;
					while (i < n) {
						uvs[i] = u + regionUVs[i + 1] * width;
						uvs[i + 1] = v + (1 - regionUVs[i]) * height;
						i += 2;
					}
					return;
				case 180:
					u -= (region.originalWidth - region.offsetX - region.width) / textureWidth;
					v -= region.offsetY / textureHeight;
					width = region.originalWidth / textureWidth;
					height = region.originalHeight / textureHeight;
					var i = 0;
					while (i < n) {
						uvs[i] = u + (1 - regionUVs[i]) * width;
						uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
						i += 2;
					}
					return;
				case 270:
					u -= region.offsetY / textureWidth;
					v -= region.offsetX / textureHeight;
					width = region.originalHeight / textureWidth;
					height = region.originalWidth / textureHeight;
					var i = 0;
					while (i < n) {
						uvs[i] = u + (1 - regionUVs[i + 1]) * width;
						uvs[i + 1] = v + regionUVs[i] * height;
					}
					return;
			}
			u -= region.offsetX / textureWidth;
			v -= (region.originalHeight - region.offsetY - region.height) / textureHeight;
			width = region.originalWidth / textureWidth;
			height = region.originalHeight / textureHeight;
		} else if (region == null) {
			u = v = 0;
			width = height = 1;
		} else {
			width = this.region.u2 - u;
			height = this.region.v2 - v;
		}
		var i = 0;
		while (i < n) {
			uvs[i] = u + regionUVs[i] * width;
			uvs[i + 1] = v + regionUVs[i + 1] * height;
			i += 2;
		}
	}

	public var parentMesh(get, set):MeshAttachment;

	private function get_parentMesh():MeshAttachment {
		return _parentMesh;
	}

	private function set_parentMesh(parentMesh:MeshAttachment):MeshAttachment {
		_parentMesh = parentMesh;
		if (parentMesh != null) {
			bones = parentMesh.bones;
			vertices = parentMesh.vertices;
			worldVerticesLength = parentMesh.worldVerticesLength;
			regionUVs = parentMesh.regionUVs;
			triangles = parentMesh.triangles;
			hullLength = parentMesh.hullLength;
			edges = parentMesh.edges;
			width = parentMesh.width;
			height = parentMesh.height;
		}
		return _parentMesh;
	}

	override public function copy():Attachment {
		if (parentMesh != null)
			return newLinkedMesh();

		var copy:MeshAttachment = new MeshAttachment(name, this.path);
		copy.region = region;
		copy.color.setFromColor(color);
		copy.rendererObject = rendererObject;

		this.copyTo(copy);
		copy.regionUVs = regionUVs.concat();
		copy.uvs = uvs.concat();
		copy.triangles = triangles.concat();
		copy.hullLength = hullLength;

		copy.sequence = sequence != null ? sequence.copy() : null;

		if (edges != null) {
			copy.edges = edges.concat();
		}
		copy.width = width;
		copy.height = height;

		return copy;
	}

	public override function computeWorldVertices(slot:Slot, start:Int, count:Int, worldVertices:Vector<Float>, offset:Int, stride:Int):Void {
		if (sequence != null)
			sequence.apply(slot, this);
		super.computeWorldVertices(slot, start, count, worldVertices, offset, stride);
	}

	public function newLinkedMesh():MeshAttachment {
		var copy:MeshAttachment = new MeshAttachment(name, path);
		copy.rendererObject = rendererObject;
		copy.region = region;
		copy.color.setFromColor(color);
		copy.timelineAttachment = timelineAttachment;
		copy.parentMesh = this.parentMesh != null ? this.parentMesh : this;
		if (copy.region != null)
			copy.updateRegion();
		return copy;
	}
}
