/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.attachments;

import spine.Color;
import spine.Sequence;
import spine.TextureRegion;
import spine.HasSequence;
import spine.atlas.TextureAtlasRegion;
import spine.atlas.TextureAtlasPage;

/** An attachment that displays a textured mesh. A mesh has hull vertices and internal vertices within the hull. Holes are not
 * supported. Each vertex has UVs (texture coordinates) and triangles are used to map an image on to the mesh.
 *
 * @see https://esotericsoftware.com/spine-meshes Mesh attachments in the Spine User Guide */
class MeshAttachment extends VertexAttachment implements HasSequence {
	public var sequence:Sequence;

	/** The UV pair for each vertex, normalized within the texture region. */
	public var regionUVs = new Array<Float>();

	/** Triplets of vertex indices which describe the mesh's triangulation. */
	public var triangles = new Array<Int>();

	/** The number of entries at the beginning of #vertices that make up the mesh hull. */
	public var hullLength:Int = 0;

	/** The name of the texture region for this attachment. */
	public var path:String;

	/** The color to tint the mesh. */
	public var color:Color = new Color(1, 1, 1, 1);

	/** The source mesh if this is a linked mesh, else null. A linked mesh shares the #bones, #vertices,
	 * #regionUVs, #triangles, #hullLength, #edges, #width, and #height with the
	 * source mesh, but may have a different #name or #path (and therefore a different texture). */
	private var _sourceMesh:MeshAttachment;

	/** Vertex index pairs describing edges for controlling triangulation, or null if nonessential data was not exported. Mesh
	 * triangles never cross edges. Triangulation is not performed at runtime. */
	public var edges = new Array<Int>();

	/** The width of the mesh's image, or zero if nonessential data was not exported. */
	public var width:Float = 0;

	/** The height of the mesh's image, or zero if nonessential data was not exported. */
	public var height:Float = 0;

	public var rendererObject:Dynamic;

	public function new(name:String, sequence:Sequence) {
		super(name);
		this.sequence = sequence;
	}

	override public function copy():Attachment {
		if (_sourceMesh != null)
			return newLinkedMesh();

		var copy = new MeshAttachment(name, sequence.copy());
		copy.path = path;
		copy.color.setFromColor(color);
		copy.rendererObject = rendererObject;

		this.copyTo(copy);
		copy.regionUVs = regionUVs.copy();
		copy.triangles = triangles.copy();
		copy.hullLength = hullLength;

		if (edges != null) {
			copy.edges = edges.copy();
		}
		copy.width = width;
		copy.height = height;

		return copy;
	}

	/** Calls Sequence.update() on this attachment's sequence. */
	public function updateSequence():Void {
		sequence.update(this);
	}

	public var sourceMesh(get, set):MeshAttachment;

	private function get_sourceMesh():MeshAttachment {
		return _sourceMesh;
	}

	private function set_sourceMesh(sourceMesh:MeshAttachment):MeshAttachment {
		_sourceMesh = sourceMesh;
		if (sourceMesh != null) {
			bones = sourceMesh.bones;
			vertices = sourceMesh.vertices;
			worldVerticesLength = sourceMesh.worldVerticesLength;
			regionUVs = sourceMesh.regionUVs;
			triangles = sourceMesh.triangles;
			hullLength = sourceMesh.hullLength;
			edges = sourceMesh.edges;
			width = sourceMesh.width;
			height = sourceMesh.height;
		}
		return _sourceMesh;
	}

	/** Returns a new mesh with the sourceMesh set to this mesh's source mesh, if any, else to this mesh. */
	public function newLinkedMesh():MeshAttachment {
		var copy = new MeshAttachment(name, sequence.copy());
		copy.rendererObject = rendererObject;
		copy.timelineAttachment = timelineAttachment;
		copy.path = path;
		copy.color.setFromColor(color);
		copy.sourceMesh = _sourceMesh != null ? _sourceMesh : this;
		copy.updateSequence();
		return copy;
	}

	/** Computes UVs for a mesh attachment.
	 * @param uvs Output array for the computed UVs, same length as regionUVs. */
	public static function computeUVs(region:TextureRegion, regionUVs:Array<Float>, uvs:Array<Float>):Void {
		if (region == null) {
			throw "Region not set.";
			return;
		}
		var n = uvs.length;
		var u = region.u, v = region.v, width:Float = 0, height:Float = 0;
		if (Std.isOfType(region, TextureAtlasRegion)) {
			var atlasRegion:TextureAtlasRegion = cast(region, TextureAtlasRegion),
				page:TextureAtlasPage = atlasRegion.page;
			var textureWidth = page.width, textureHeight = page.height;
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
						i += 2;
					}
					return;
				default:
					u -= region.offsetX / textureWidth;
					v -= (region.originalHeight - region.offsetY - region.height) / textureHeight;
					width = region.originalWidth / textureWidth;
					height = region.originalHeight / textureHeight;
			}
		} else if (region == null) {
			u = v = 0;
			width = height = 1;
		} else {
			width = region.u2 - u;
			height = region.v2 - v;
		}
		var i = 0;
		while (i < n) {
			uvs[i] = u + regionUVs[i] * width;
			uvs[i + 1] = v + regionUVs[i + 1] * height;
			i += 2;
		}
	}
}
