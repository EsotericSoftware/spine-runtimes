/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Bone;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;

public class BoundingBoxAttachment extends Attachment {
	private float[] points;
	private float[] vertices;

	public BoundingBoxAttachment (String name) {
		super(name);
	}

	public void updateVertices (Slot slot) {
		Bone bone = slot.getBone();
		Skeleton skeleton = slot.getSkeleton();
		float x = bone.getWorldX() + skeleton.getX();
		float y = bone.getWorldY() + skeleton.getY();
		float m00 = bone.getM00();
		float m01 = bone.getM01();
		float m10 = bone.getM10();
		float m11 = bone.getM11();
		float[] vertices = this.vertices;
		float[] points = this.points;
		for (int i = 0, n = points.length; i < n; i += 2) {
			float px = points[i];
			float py = points[i + 1];
			vertices[i] = px * m00 + py * m01 + x;
			vertices[i + 1] = px * m10 + py * m11 + y;
		}
	}

	public float[] getVertices () {
		return vertices;
	}

	public float[] getPoints () {
		return points;
	}

	public void setPoints (float[] points) {
		this.points = points;
		if (vertices == null || vertices.length != points.length) vertices = new float[points.length];
	}
}
