/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.esotericsoftware.spine.Animation.DeformTimeline;

/** An attachment that displays a textured mesh. A mesh has hull vertices and internal vertices within the hull. Holes are not
 * supported. Each vertex has UVs (texture coordinates) and triangles are used to map an image on to the mesh.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-meshes">Mesh attachments</a> in the Spine User Guide. */
public class MeshAttachment extends VertexAttachment {
	private TextureRegion region;
	private String path;
	private float[] regionUVs, uvs;
	private short[] triangles;
	private final Color color = new Color(1, 1, 1, 1);
	private int hullLength;
	private MeshAttachment parentMesh;
	private boolean inheritDeform;

	// Nonessential.
	private short[] edges;
	private float width, height;

	public MeshAttachment (String name) {
		super(name);
	}

	public void setRegion (TextureRegion region) {
		if (region == null) throw new IllegalArgumentException("region cannot be null.");
		this.region = region;
	}

	public TextureRegion getRegion () {
		if (region == null) throw new IllegalStateException("Region has not been set: " + this);
		return region;
	}

	/** Calculates {@link #uvs} using {@link #regionUVs} and the {@link #region}. Must be called after changing the region UVs or
	 * region. */
	public void updateUVs () {
		float u, v, width, height;
		if (region == null) {
			u = v = 0;
			width = height = 1;
		} else {
			u = region.getU();
			v = region.getV();
			width = region.getU2() - u;
			height = region.getV2() - v;
		}
		float[] regionUVs = this.regionUVs;
		if (this.uvs == null || this.uvs.length != regionUVs.length) this.uvs = new float[regionUVs.length];
		float[] uvs = this.uvs;
		if (region instanceof AtlasRegion && ((AtlasRegion)region).rotate) {
			for (int i = 0, n = uvs.length; i < n; i += 2) {
				uvs[i] = u + regionUVs[i + 1] * width;
				uvs[i + 1] = v + height - regionUVs[i] * height;
			}
		} else {
			for (int i = 0, n = uvs.length; i < n; i += 2) {
				uvs[i] = u + regionUVs[i] * width;
				uvs[i + 1] = v + regionUVs[i + 1] * height;
			}
		}
	}

	/** Returns true if the <code>sourceAttachment</code> is this mesh, else returns true if {@link #inheritDeform} is true and the
	 * the <code>sourceAttachment</code> is the {@link #parentMesh}. */
	public boolean applyDeform (VertexAttachment sourceAttachment) {
		return this == sourceAttachment || (inheritDeform && parentMesh == sourceAttachment);
	}

	/** Triplets of vertex indices which describe the mesh's triangulation. */
	public short[] getTriangles () {
		return triangles;
	}

	public void setTriangles (short[] triangles) {
		this.triangles = triangles;
	}

	/** The UV pair for each vertex, normalized within the texture region. */
	public float[] getRegionUVs () {
		return regionUVs;
	}

	/** Sets the texture coordinates for the region. The values are u,v pairs for each vertex. */
	public void setRegionUVs (float[] regionUVs) {
		this.regionUVs = regionUVs;
	}

	/** The UV pair for each vertex, normalized within the entire texture.
	 * <p>
	 * See {@link #updateUVs}. */
	public float[] getUVs () {
		return uvs;
	}

	public void setUVs (float[] uvs) {
		this.uvs = uvs;
	}

	/** The color to tint the mesh. */
	public Color getColor () {
		return color;
	}

	/** The name of the texture region for this attachment. */
	public String getPath () {
		return path;
	}

	public void setPath (String path) {
		this.path = path;
	}

	/** The number of entries at the beginning of {@link #vertices} that make up the mesh hull. */
	public int getHullLength () {
		return hullLength;
	}

	public void setHullLength (int hullLength) {
		this.hullLength = hullLength;
	}

	public void setEdges (short[] edges) {
		this.edges = edges;
	}

	/** Vertex index pairs describing edges for controling triangulation. Mesh triangles will never cross edges. Only available if
	 * nonessential data was exported. Triangulation is not performed at runtime. */
	public short[] getEdges () {
		return edges;
	}

	/** The width of the mesh's image. Available only when nonessential data was exported. */
	public float getWidth () {
		return width;
	}

	public void setWidth (float width) {
		this.width = width;
	}

	/** The height of the mesh's image. Available only when nonessential data was exported. */
	public float getHeight () {
		return height;
	}

	public void setHeight (float height) {
		this.height = height;
	}

	/** The parent mesh if this is a linked mesh, else null. A linked mesh shares the {@link #bones}, {@link #vertices},
	 * {@link #regionUVs}, {@link #triangles}, {@link #hullLength}, {@link #edges}, {@link #width}, and {@link #height} with the
	 * parent mesh, but may have a different {@link #name} or {@link #path} (and therefore a different texture). */
	public MeshAttachment getParentMesh () {
		return parentMesh;
	}

	/** @param parentMesh May be null. */
	public void setParentMesh (MeshAttachment parentMesh) {
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

	/** When this is a linked mesh (see {@link #parentMesh}), if true, any {@link DeformTimeline} for the {@link #parentMesh} is
	 * also applied to this mesh. If false, this linked mesh may have its own deform timelines.
	 * <p>
	 * See {@link #applyDeform(VertexAttachment)}. */
	public boolean getInheritDeform () {
		return inheritDeform;
	}

	public void setInheritDeform (boolean inheritDeform) {
		this.inheritDeform = inheritDeform;
	}
}
