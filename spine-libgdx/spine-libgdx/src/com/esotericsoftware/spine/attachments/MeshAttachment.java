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

package com.esotericsoftware.spine.attachments;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.Null;

/** An attachment that displays a textured mesh. A mesh has hull vertices and internal vertices within the hull. Holes are not
 * supported. Each vertex has UVs (texture coordinates) and triangles are used to map an image on to the mesh.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-meshes">Mesh attachments</a> in the Spine User Guide. */
public class MeshAttachment extends VertexAttachment implements HasSequence {
	private final Sequence sequence;
	float[] regionUVs;
	private short[] triangles;
	private int hullLength;
	private String path;
	private final Color color = new Color(1, 1, 1, 1);
	private @Null MeshAttachment parentMesh;

	// Nonessential.
	private @Null short[] edges;
	private float width, height;

	public MeshAttachment (String name, Sequence sequence) {
		super(name);
		if (sequence == null) throw new IllegalArgumentException("sequence cannot be null.");
		this.sequence = sequence;
	}

	/** Copy constructor. Use {@link #newLinkedMesh()} if the other mesh is a linked mesh. */
	protected MeshAttachment (MeshAttachment other) {
		super(other);

		if (parentMesh != null) throw new IllegalArgumentException("Use newLinkedMesh to copy a linked mesh.");

		path = other.path;
		color.set(other.color);

		regionUVs = new float[other.regionUVs.length];
		arraycopy(other.regionUVs, 0, regionUVs, 0, regionUVs.length);

		triangles = new short[other.triangles.length];
		arraycopy(other.triangles, 0, triangles, 0, triangles.length);

		hullLength = other.hullLength;
		sequence = new Sequence(other.sequence);

		// Nonessential.
		if (other.edges != null) {
			edges = new short[other.edges.length];
			arraycopy(other.edges, 0, edges, 0, edges.length);
		}
		width = other.width;
		height = other.height;
	}

	/** The UV pair for each vertex, normalized within the texture region. */
	public float[] getRegionUVs () {
		return regionUVs;
	}

	/** Sets the texture coordinates for the region. The values are u,v pairs for each vertex. */
	public void setRegionUVs (float[] regionUVs) {
		this.regionUVs = regionUVs;
	}

	/** Triplets of vertex indices which describe the mesh's triangulation. */
	public short[] getTriangles () {
		return triangles;
	}

	public void setTriangles (short[] triangles) {
		this.triangles = triangles;
	}

	/** The number of entries at the beginning of {@link #vertices} that make up the mesh hull. */
	public int getHullLength () {
		return hullLength;
	}

	public void setHullLength (int hullLength) {
		this.hullLength = hullLength;
	}

	public Sequence getSequence () {
		return sequence;
	}

	public void updateSequence () {
		sequence.update(this);
	}

	public String getPath () {
		return path;
	}

	public void setPath (String path) {
		this.path = path;
	}

	public Color getColor () {
		return color;
	}

	public void setEdges (short[] edges) {
		this.edges = edges;
	}

	/** Vertex index pairs describing edges for controlling triangulation, or be null if nonessential data was not exported. Mesh
	 * triangles never cross edges. Triangulation is not performed at runtime. */
	public @Null short[] getEdges () {
		return edges;
	}

	/** The width of the mesh's image, or zero if nonessential data was not exported. */
	public float getWidth () {
		return width;
	}

	public void setWidth (float width) {
		this.width = width;
	}

	/** The height of the mesh's image, or zero if nonessential data was not exported. */
	public float getHeight () {
		return height;
	}

	public void setHeight (float height) {
		this.height = height;
	}

	/** The parent mesh if this is a linked mesh, else null. A linked mesh shares the {@link #bones}, {@link #vertices},
	 * {@link #regionUVs}, {@link #triangles}, {@link #hullLength}, {@link #edges}, {@link #width}, and {@link #height} with the
	 * parent mesh, but may have a different {@link #name} or {@link #path} (and therefore a different texture). */
	public @Null MeshAttachment getParentMesh () {
		return parentMesh;
	}

	public void setParentMesh (@Null MeshAttachment parentMesh) {
		this.parentMesh = parentMesh;
		if (parentMesh != null) {
			bones = parentMesh.bones;
			vertices = parentMesh.vertices;
			regionUVs = parentMesh.regionUVs;
			triangles = parentMesh.triangles;
			hullLength = parentMesh.hullLength;
			worldVerticesLength = parentMesh.worldVerticesLength;
			edges = parentMesh.edges;
			width = parentMesh.width;
			height = parentMesh.height;
		}
	}

	/** Returns a new mesh with the {@link #parentMesh} set to this mesh's parent mesh, if any, else to this mesh. */
	public MeshAttachment newLinkedMesh () {
		var mesh = new MeshAttachment(name, new Sequence(sequence));
		mesh.timelineAttachment = timelineAttachment;
		mesh.path = path;
		mesh.color.set(color);
		mesh.setParentMesh(parentMesh != null ? parentMesh : this);
		mesh.updateSequence();
		return mesh;
	}

	public MeshAttachment copy () {
		return parentMesh != null ? newLinkedMesh() : new MeshAttachment(this);
	}

	/** Computes {@link Sequence#getUVs(int) UVs} for a mesh attachment.
	 * @param uvs Output array for the computed UVs, same length as regionUVs. */
	static void computeUVs (@Null TextureRegion region, float[] regionUVs, float[] uvs) {
		int n = uvs.length;
		float u, v, width, height;
		if (region instanceof AtlasRegion r) {
			u = r.getU();
			v = r.getV();
			float textureWidth = r.getTexture().getWidth(), textureHeight = r.getTexture().getHeight();
			switch (r.degrees) {
			case 90 -> {
				u -= (r.originalHeight - r.offsetY - r.packedWidth) / textureWidth;
				v -= (r.originalWidth - r.offsetX - r.packedHeight) / textureHeight;
				width = r.originalHeight / textureWidth;
				height = r.originalWidth / textureHeight;
				for (int i = 0; i < n; i += 2) {
					uvs[i] = u + regionUVs[i + 1] * width;
					uvs[i + 1] = v + (1 - regionUVs[i]) * height;
				}
				return;
			}
			case 180 -> {
				u -= (r.originalWidth - r.offsetX - r.packedWidth) / textureWidth;
				v -= r.offsetY / textureHeight;
				width = r.originalWidth / textureWidth;
				height = r.originalHeight / textureHeight;
				for (int i = 0; i < n; i += 2) {
					uvs[i] = u + (1 - regionUVs[i]) * width;
					uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
				}
				return;
			}
			case 270 -> {
				u -= r.offsetY / textureWidth;
				v -= r.offsetX / textureHeight;
				width = r.originalHeight / textureWidth;
				height = r.originalWidth / textureHeight;
				for (int i = 0; i < n; i += 2) {
					uvs[i] = u + (1 - regionUVs[i + 1]) * width;
					uvs[i + 1] = v + regionUVs[i] * height;
				}
				return;
			}
			default -> {
				u -= r.offsetX / textureWidth;
				v -= (r.originalHeight - r.offsetY - r.packedHeight) / textureHeight;
				width = r.originalWidth / textureWidth;
				height = r.originalHeight / textureHeight;
			}
			}
		} else if (region == null) {
			u = v = 0;
			width = height = 1;
		} else {
			u = region.getU();
			v = region.getV();
			width = region.getU2() - u;
			height = region.getV2() - v;
		}
		for (int i = 0; i < n; i += 2) {
			uvs[i] = u + regionUVs[i] * width;
			uvs[i + 1] = v + regionUVs[i + 1] * height;
		}
	}
}
