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

import java.util.concurrent.atomic.AtomicInteger;

import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.Bone;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;

/** Base class for an attachment with vertices that are transformed by one or more bones and can be deformed by a slot's
 * {@link Slot#getAttachmentVertices()}. */
public class VertexAttachment extends Attachment {
	static private final AtomicInteger nextID = new AtomicInteger();

	private final int id = (nextID.getAndIncrement() & 65535) << 11;
	int[] bones;
	float[] vertices;
	int worldVerticesLength;

	public VertexAttachment (String name) {
		super(name);
	}

	/** Transforms the attachment's local {@link #getVertices()} to world coordinates. If the slot has
	 * {@link Slot#getAttachmentVertices()}, they are used to deform the vertices.
	 * <p>
	 * See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide.
	 * @param start The index of the first {@link #getVertices()} value to transform. Each vertex has 2 values, x and y.
	 * @param count The number of world vertex values to output. Must be <= {@link #getWorldVerticesLength()} - <code>start</code>.
	 * @param worldVertices The output world vertices. Must have a length >= <code>offset</code> + <code>count</code> *
	 *           <code>stride</code> / 2.
	 * @param offset The <code>worldVertices</code> index to begin writing values.
	 * @param stride The number of <code>worldVertices</code> entries between the value pairs written. */
	public void computeWorldVertices (Slot slot, int start, int count, float[] worldVertices, int offset, int stride) {
		count = offset + (count >> 1) * stride;
		Skeleton skeleton = slot.getSkeleton();
		FloatArray deformArray = slot.getAttachmentVertices();
		float[] vertices = this.vertices;
		int[] bones = this.bones;
		if (bones == null) {
			if (deformArray.size > 0) vertices = deformArray.items;
			Bone bone = slot.getBone();
			float x = bone.getWorldX(), y = bone.getWorldY();
			float a = bone.getA(), b = bone.getB(), c = bone.getC(), d = bone.getD();
			for (int v = start, w = offset; w < count; v += 2, w += stride) {
				float vx = vertices[v], vy = vertices[v + 1];
				worldVertices[w] = vx * a + vy * b + x;
				worldVertices[w + 1] = vx * c + vy * d + y;
			}
			return;
		}
		int v = 0, skip = 0;
		for (int i = 0; i < start; i += 2) {
			int n = bones[v];
			v += n + 1;
			skip += n;
		}
		Object[] skeletonBones = skeleton.getBones().items;
		if (deformArray.size == 0) {
			for (int w = offset, b = skip * 3; w < count; w += stride) {
				float wx = 0, wy = 0;
				int n = bones[v++];
				n += v;
				for (; v < n; v++, b += 3) {
					Bone bone = (Bone)skeletonBones[bones[v]];
					float vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
					wx += (vx * bone.getA() + vy * bone.getB() + bone.getWorldX()) * weight;
					wy += (vx * bone.getC() + vy * bone.getD() + bone.getWorldY()) * weight;
				}
				worldVertices[w] = wx;
				worldVertices[w + 1] = wy;
			}
		} else {
			float[] deform = deformArray.items;
			for (int w = offset, b = skip * 3, f = skip << 1; w < count; w += stride) {
				float wx = 0, wy = 0;
				int n = bones[v++];
				n += v;
				for (; v < n; v++, b += 3, f += 2) {
					Bone bone = (Bone)skeletonBones[bones[v]];
					float vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
					wx += (vx * bone.getA() + vy * bone.getB() + bone.getWorldX()) * weight;
					wy += (vx * bone.getC() + vy * bone.getD() + bone.getWorldY()) * weight;
				}
				worldVertices[w] = wx;
				worldVertices[w + 1] = wy;
			}
		}
	}

	/** Returns true if a deform originally applied to the specified attachment should be applied to this attachment. The default
	 * implementation returns true only when <code>sourceAttachment</code> is this attachment. */
	public boolean applyDeform (VertexAttachment sourceAttachment) {
		return this == sourceAttachment;
	}

	/** The bones which affect the {@link #getVertices()}. The array entries are, for each vertex, the number of bones affecting
	 * the vertex followed by that many bone indices, which is the index of the bone in {@link Skeleton#getBones()}. Will be null
	 * if this attachment has no weights. */
	public int[] getBones () {
		return bones;
	}

	/** @param bones May be null if this attachment has no weights. */
	public void setBones (int[] bones) {
		this.bones = bones;
	}

	/** The vertex positions in the bone's coordinate system. For a non-weighted attachment, the values are <code>x,y</code>
	 * entries for each vertex. For a weighted attachment, the values are <code>x,y,weight</code> entries for each bone affecting
	 * each vertex. */
	public float[] getVertices () {
		return vertices;
	}

	public void setVertices (float[] vertices) {
		this.vertices = vertices;
	}

	/** The maximum number of world vertex values that can be output by
	 * {@link #computeWorldVertices(Slot, int, int, float[], int, int)} using the <code>count</code> parameter. */
	public int getWorldVerticesLength () {
		return worldVerticesLength;
	}

	public void setWorldVerticesLength (int worldVerticesLength) {
		this.worldVerticesLength = worldVerticesLength;
	}

	/** Returns a unique ID for this attachment. */
	public int getId () {
		return id;
	}
}
