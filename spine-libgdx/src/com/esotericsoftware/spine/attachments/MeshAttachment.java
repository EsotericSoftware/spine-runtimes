/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Bone;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.NumberUtils;

/** Attachment that displays a texture region. */
public class MeshAttachment extends Attachment {
	private TextureRegion region;
	private String path;
	private int hullLength;
	private float[] vertices;
	private short[] triangles;
	private int[] edges;
	private float[] worldVertices;
	private final Color color = new Color(1, 1, 1, 1);
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

	public void updateWorldVertices (Slot slot, boolean premultipliedAlpha) {
		Skeleton skeleton = slot.getSkeleton();
		Color skeletonColor = skeleton.getColor();
		Color slotColor = slot.getColor();
		Color regionColor = color;
		float r = skeletonColor.r * slotColor.r * regionColor.r;
		float g = skeletonColor.g * slotColor.g * regionColor.g;
		float b = skeletonColor.b * slotColor.b * regionColor.b;
		float a = skeletonColor.a * slotColor.a * regionColor.a * 255;
		float color;
		if (premultipliedAlpha) {
			r *= a;
			g *= a;
			b *= a;
		} else {
			r *= 255;
			g *= 255;
			b *= 255;
		}
		color = NumberUtils.intToFloatColor( //
			((int)(a) << 24) //
				| ((int)(b) << 16) //
				| ((int)(g) << 8) //
				| ((int)(r)));

		float[] worldVertices = this.worldVertices;
		float[] vertices = this.vertices;
		Bone bone1 = slot.getBone();
		float x = skeleton.getX();
		float y = skeleton.getY();
		float m00 = bone1.getM00();
		float m01 = bone1.getM01();
		float m10 = bone1.getM10();
		float m11 = bone1.getM11();

		float vx, vy;
		for (int v = 0, w = 0, n = vertices.length; v < n; v += 2, w += 5) {
			vx = vertices[v];
			vy = vertices[v + 1];
			float wx1 = vx * m00 + vy * m01 + x + bone1.getWorldX();
			float wy1 = vx * m10 + vy * m11 + y + bone1.getWorldY();
			worldVertices[w] = wx1;
			worldVertices[w + 1] = wy1;
			worldVertices[w + 2] = Color.WHITE.toFloatBits();
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

		for (int i = 0, w = 3, n = vertices.length; i < n; i += 2, w += 5) {
			worldVertices[w] = uvs[i];
			worldVertices[w + 1] = uvs[i + 1];
		}
	}
}
