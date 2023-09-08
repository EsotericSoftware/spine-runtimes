package spine.attachments;

import openfl.Vector;
import spine.Color;

class MeshAttachment extends VertexAttachment {
	public var uvs:Vector<Float>;
	public var regionUVs:Vector<Float>;
	public var triangles:Vector<Int>;
	public var color:Color = new Color(1, 1, 1, 1);
	public var hullLength:Int = 0;

	private var _parentMesh:MeshAttachment;

	public var path:String;
	public var rendererObject:Dynamic;
	public var regionU:Float = 0;
	public var regionV:Float = 0;
	public var regionU2:Float = 0;
	public var regionV2:Float = 0;
	public var regionDegrees:Int = 0;
	public var regionOffsetX:Float = 0; // Pixels stripped from the bottom left, unrotated.
	public var regionOffsetY:Float = 0;
	public var regionWidth:Float = 0; // Unrotated, stripped size.
	public var regionHeight:Float = 0;
	public var regionOriginalWidth:Float = 0; // Unrotated, unstripped size.
	public var regionOriginalHeight:Float = 0;
	// Nonessential.
	public var edges:Vector<Int>;
	public var width:Float = 0;
	public var height:Float = 0;

	public function new(name:String) {
		super(name);
	}

	public function updateUVs():Void {
		var i:Int = 0, n:Int = regionUVs.length;
		var u:Float = regionU, v:Float = regionV;
		var width:Float = 0, height:Float = 0;
		var textureWidth:Float, textureHeight:Float;
		if (uvs == null || uvs.length != n)
			uvs = new Vector<Float>(n, true);

		switch (regionDegrees) {
			case 90:
				{
					textureWidth = regionHeight / (regionU2 - regionU);
					textureHeight = regionWidth / (regionV2 - regionV);
					u -= (regionOriginalHeight - regionOffsetY - regionHeight) / textureWidth;
					v -= (regionOriginalWidth - regionOffsetX - regionWidth) / textureHeight;
					width = regionOriginalHeight / textureWidth;
					height = regionOriginalWidth / textureHeight;
					i = 0;
					while (i < n) {
						uvs[i] = u + regionUVs[i + 1] * width;
						uvs[i + 1] = v + (1 - regionUVs[i]) * height;
						i += 2;
					}
				}
			case 180:
				{
					textureWidth = regionWidth / (regionU2 - regionU);
					textureHeight = regionHeight / (regionV2 - regionV);
					u -= (regionOriginalWidth - regionOffsetX - regionWidth) / textureWidth;
					v -= regionOffsetY / textureHeight;
					width = regionOriginalWidth / textureWidth;
					height = regionOriginalHeight / textureHeight;
					i = 0;
					while (i < n) {
						uvs[i] = u + (1 - regionUVs[i]) * width;
						uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
						i += 2;
					}
				}
			case 270:
				{
					textureWidth = regionWidth / (regionU2 - regionU);
					textureHeight = regionHeight / (regionV2 - regionV);
					u -= regionOffsetY / textureWidth;
					v -= regionOffsetX / textureHeight;
					width = regionOriginalHeight / textureWidth;
					height = regionOriginalWidth / textureHeight;
					i = 0;
					while (i < n) {
						uvs[i] = u + (1 - regionUVs[i + 1]) * width;
						uvs[i + 1] = v + regionUVs[i] * height;
						i += 2;
					}
				}
			default:
				{
					textureWidth = regionWidth / (regionU2 - regionU);
					textureHeight = regionHeight / (regionV2 - regionV);
					u -= regionOffsetX / textureWidth;
					v -= (regionOriginalHeight - regionOffsetY - regionHeight) / textureHeight;
					width = regionOriginalWidth / textureWidth;
					height = regionOriginalHeight / textureHeight;
					i = 0;
					while (i < n) {
						uvs[i] = u + regionUVs[i] * width;
						uvs[i + 1] = v + regionUVs[i + 1] * height;
						i += 2;
					}
				}
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
		var copy:MeshAttachment = new MeshAttachment(name);
		copy.rendererObject = rendererObject;
		copy.regionU = regionU;
		copy.regionV = regionV;
		copy.regionU2 = regionU2;
		copy.regionV2 = regionV2;
		copy.regionDegrees = regionDegrees;
		copy.regionOffsetX = regionOffsetX;
		copy.regionOffsetY = regionOffsetY;
		copy.regionWidth = regionWidth;
		copy.regionHeight = regionHeight;
		copy.regionOriginalWidth = regionOriginalWidth;
		copy.regionOriginalHeight = regionOriginalHeight;
		copy.path = path;
		copy.color.setFromColor(color);

		if (parentMesh == null) {
			this.copyTo(copy);
			copy.regionUVs = regionUVs.concat();
			copy.uvs = uvs.concat();
			copy.triangles = triangles.concat();
			copy.hullLength = hullLength;

			// Nonessential.
			if (edges != null) {
				copy.edges = edges.concat();
			}
			copy.width = width;
			copy.height = height;
		} else {
			copy.parentMesh = parentMesh;
			copy.updateUVs();
		}

		return copy;
	}

	public function newLinkedMesh():MeshAttachment {
		var copy:MeshAttachment = new MeshAttachment(name);
		copy.rendererObject = rendererObject;
		copy.regionU = regionU;
		copy.regionV = regionV;
		copy.regionU2 = regionU2;
		copy.regionV2 = regionV2;
		copy.regionDegrees = regionDegrees;
		copy.regionOffsetX = regionOffsetX;
		copy.regionOffsetY = regionOffsetY;
		copy.regionWidth = regionWidth;
		copy.regionHeight = regionHeight;
		copy.regionOriginalWidth = regionOriginalWidth;
		copy.regionOriginalHeight = regionOriginalHeight;
		copy.path = path;
		copy.color.setFromColor(color);
		copy.deformAttachment = deformAttachment;
		copy.parentMesh = parentMesh != null ? parentMesh : this;
		copy.updateUVs();
		return copy;
	}
}
