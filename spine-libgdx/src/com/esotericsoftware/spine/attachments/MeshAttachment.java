/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Bone;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.NumberUtils;

/** Attachment that displays a texture region. */
public class MeshAttachment extends Attachment {
	private TextureRegion region;
	private String path;
	private float[] vertices;
	private short[] triangles;
	private float[] worldVertices;
	private final Color color = new Color(1, 1, 1, 1);

	// Nonessential.
	private int[] edges;
	private float width, height;
	private int hullLength;

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

	public void updateWorldVertices (Slot slot, boolean premultipliedAlpha) {
		Skeleton skeleton = slot.getSkeleton();
		Color skeletonColor = skeleton.getColor();
		Color slotColor = slot.getColor();
		Color meshColor = color;
		float a = skeletonColor.a * slotColor.a * meshColor.a * 255;
		float multiplier = premultipliedAlpha ? a : 255;
		float color = NumberUtils.intToFloatColor( //
			((int)a << 24) //
				| ((int)(skeletonColor.b * slotColor.b * meshColor.b * multiplier) << 16) //
				| ((int)(skeletonColor.g * slotColor.g * meshColor.g * multiplier) << 8) //
				| (int)(skeletonColor.r * slotColor.r * meshColor.r * multiplier));

		float[] worldVertices = this.worldVertices;
		FloatArray verticesArray = slot.getAttachmentVertices();
		float[] vertices = this.vertices;
		if (verticesArray.size == vertices.length) vertices = verticesArray.items;
		Bone bone = slot.getBone();
		float x = skeleton.getX() + bone.getWorldX(), y = skeleton.getY() + bone.getWorldY();
		float m00 = bone.getM00(), m01 = bone.getM01(), m10 = bone.getM10(), m11 = bone.getM11();
		for (int v = 0, w = 0, n = worldVertices.length; w < n; v += 2, w += 5) {
			float vx = vertices[v];
			float vy = vertices[v + 1];
			worldVertices[w] = vx * m00 + vy * m01 + x;
			worldVertices[w + 1] = vx * m10 + vy * m11 + y;
			worldVertices[w + 2] = color;
		}
	}

	public float[] getWorldVertices () {
		return worldVertices;
	}

	public float[] getVertices () {
		return vertices;
	}

	public short[] getTriangles () {
		return triangles;
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

	public int[] getEdges () {
		return edges;
	}

	public void setEdges (int[] edges) {
		this.edges = edges;
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

	public void setMesh (float[] vertices, short[] triangles, float[] uvs) {
		this.vertices = vertices;
		this.triangles = triangles;

		int worldVerticesLength = vertices.length / 2 * 5;
		if (worldVertices == null || worldVertices.length != worldVerticesLength) worldVertices = new float[worldVerticesLength];

		float u, v, w, h;
		if (region == null) {
			u = v = 0;
			w = h = 1;
		} else {
			u = region.getU();
			v = region.getV();
			w = region.getU2() - u;
			h = region.getV2() - v;
		}
		for (int i = 0, ii = 3, n = vertices.length; i < n; i += 2, ii += 5) {
			worldVertices[ii] = u + uvs[i] * w;
			worldVertices[ii + 1] = v + uvs[i + 1] * h;
		}
	}
}
