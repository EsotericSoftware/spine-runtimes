/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Bone;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.NumberUtils;

/** Attachment that displays a texture region. */
public class SkinnedMeshAttachment extends Attachment {
	private TextureRegion region;
	private String path;
	private int[] bones;
	private float[] weights, regionUVs;
	private short[] triangles;
	private float[] worldVertices;
	private final Color color = new Color(1, 1, 1, 1);
	private int hullLength;

	// Nonessential.
	private int[] edges;
	private float width, height;

	public SkinnedMeshAttachment (String name) {
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

	public void updateUVs () {
		float[] regionUVs = this.regionUVs;
		int verticesLength = regionUVs.length;
		int worldVerticesLength = verticesLength / 2 * 5;
		if (worldVertices == null || worldVertices.length != worldVerticesLength) worldVertices = new float[worldVerticesLength];

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
		if (region instanceof AtlasRegion && ((AtlasRegion)region).rotate) {
			for (int i = 0, w = 3; i < verticesLength; i += 2, w += 5) {
				worldVertices[w] = u + regionUVs[i + 1] * width;
				worldVertices[w + 1] = v + height - regionUVs[i] * height;
			}
		} else {
			for (int i = 0, w = 3; i < verticesLength; i += 2, w += 5) {
				worldVertices[w] = u + regionUVs[i] * width;
				worldVertices[w + 1] = v + regionUVs[i + 1] * height;
			}
		}
	}

	public void updateWorldVertices (Slot slot, boolean premultipliedAlpha) {
		Skeleton skeleton = slot.getSkeleton();
		Color skeletonColor = skeleton.getColor();
		Color meshColor = slot.getColor();
		Color regionColor = color;
		float a = skeletonColor.a * meshColor.a * regionColor.a * 255;
		float multiplier = premultipliedAlpha ? a : 255;
		float color = NumberUtils.intToFloatColor( //
			((int)a << 24) //
				| ((int)(skeletonColor.b * meshColor.b * regionColor.b * multiplier) << 16) //
				| ((int)(skeletonColor.g * meshColor.g * regionColor.g * multiplier) << 8) //
				| (int)(skeletonColor.r * meshColor.r * regionColor.r * multiplier));

		float[] worldVertices = this.worldVertices;
		float x = skeleton.getX(), y = skeleton.getY();
		Object[] skeletonBones = skeleton.getBones().items;
		float[] weights = this.weights;
		int[] bones = this.bones;

		FloatArray ffdArray = slot.getAttachmentVertices();
		if (ffdArray.size == 0) {
			for (int w = 0, v = 0, b = 0, n = bones.length; v < n; w += 5) {
				float wx = 0, wy = 0;
				int nn = bones[v++] + v;
				for (; v < nn; v++, b += 3) {
					Bone bone = (Bone)skeletonBones[bones[v]];
					float vx = weights[b], vy = weights[b + 1], weight = weights[b + 2];
					wx += (vx * bone.getM00() + vy * bone.getM01() + bone.getWorldX()) * weight;
					wy += (vx * bone.getM10() + vy * bone.getM11() + bone.getWorldY()) * weight;
				}
				worldVertices[w] = wx + x;
				worldVertices[w + 1] = wy + y;
				worldVertices[w + 2] = color;
			}
		} else {
			float[] ffd = ffdArray.items;
			for (int w = 0, v = 0, b = 0, f = 0, n = bones.length; v < n; w += 5) {
				float wx = 0, wy = 0;
				int nn = bones[v++] + v;
				for (; v < nn; v++, b += 3, f += 2) {
					Bone bone = (Bone)skeletonBones[bones[v]];
					float vx = weights[b] + ffd[f], vy = weights[b + 1] + ffd[f + 1], weight = weights[b + 2];
					wx += (vx * bone.getM00() + vy * bone.getM01() + bone.getWorldX()) * weight;
					wy += (vx * bone.getM10() + vy * bone.getM11() + bone.getWorldY()) * weight;
				}
				worldVertices[w] = wx + x;
				worldVertices[w + 1] = wy + y;
				worldVertices[w + 2] = color;
			}
		}
	}

	public float[] getWorldVertices () {
		return worldVertices;
	}

	public int[] getBones () {
		return bones;
	}

	/** For each vertex, the number of bones affecting the vertex followed by that many bone indices. Ie: count, boneIndex, ... */
	public void setBones (int[] bones) {
		this.bones = bones;
	}

	public float[] getWeights () {
		return weights;
	}

	/** For each bone affecting the vertex, the vertex position in the bone's coordinate system and the weight for the bone's
	 * influence. Ie: x, y, weight, ... */
	public void setWeights (float[] weights) {
		this.weights = weights;
	}

	public short[] getTriangles () {
		return triangles;
	}

	/** Vertex number triplets which describe the mesh's triangulation. */
	public void setTriangles (short[] triangles) {
		this.triangles = triangles;
	}

	public float[] getRegionUVs () {
		return regionUVs;
	}

	/** For each vertex, a texure coordinate pair. Ie: u, v, ... */
	public void setRegionUVs (float[] regionUVs) {
		this.regionUVs = regionUVs;
	}

	public Color getColor () {
		return color;
	}

	public String getPath () {
		return path;
	}

	public void setPath (String path) {
		this.path = path;
	}

	public int getHullLength () {
		return hullLength;
	}

	public void setHullLength (int hullLength) {
		this.hullLength = hullLength;
	}

	public void setEdges (int[] edges) {
		this.edges = edges;
	}

	public int[] getEdges () {
		return edges;
	}

	public float getWidth () {
		return width;
	}

	public void setWidth (float width) {
		this.width = width;
	}

	public float getHeight () {
		return height;
	}

	public void setHeight (float height) {
		this.height = height;
	}
}
