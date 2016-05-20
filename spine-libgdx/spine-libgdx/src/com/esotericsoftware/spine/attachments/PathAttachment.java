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

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.Vector2;
import com.esotericsoftware.spine.Slot;

public class PathAttachment extends VertexAttachment {
	// Nonessential.
	final Color color = new Color(1, 0.5f, 0, 1);
	final Vector2 temp = new Vector2();
	float[] worldVertices, lengths;
	int totalLength;
	boolean closed;

	public PathAttachment (String name) {
		super(name);
	}

	public void computeWorldVertices (Slot slot, float[] worldVertices) {
		super.computeWorldVertices(slot, worldVertices);
	}

	public Vector2 computeWorldPosition (Slot slot, float position) {
		float[] worldVertices = this.worldVertices;
		super.computeWorldVertices(slot, worldVertices);

		int curve = 0;
		float t = 0;
		if (closed) {
			// BOZO - closed boolean used to turn off fancy calculations for now.
			int curves = (worldVerticesLength >> 2) - 1;
			curve = position < 1 ? (int)(curves * position) : curves - 1;
			t = (position - curve / (float)curves) * curves;
		} else {
			// Compute lengths of all curves.
			totalLength = 0;
			float[] lengths = this.lengths;
			float x1 = worldVertices[0], y1 = worldVertices[1];
			float cx1 = x1 + (x1 - worldVertices[2]), cy1 = y1 + (y1 - worldVertices[3]);
			for (int i = 0, w = 4, n = worldVerticesLength; w < n; i += 6, w += 4) {
				float x2 = worldVertices[w], y2 = worldVertices[w + 1];
				float cx2 = worldVertices[w + 2], cy2 = worldVertices[w + 3];
				addLengths(i, x1, y1, cx1, cy1, cx2, cy2, x2, y2);
				x1 = x2;
				y1 = y2;
				cx1 = x2 + (x2 - cx2);
				cy1 = y2 + (y2 - cy2);
			}

			// Determine curve containing position.
			float target = totalLength * position, distance = 0;
			for (int i = 5;; i += 6) {
				float curveLength = lengths[i];
				if (distance + curveLength > target) {
					curve = i / 6;
					t = (target - distance) / curveLength;
					break;
				}
				distance += curveLength;
			}

			// Adjust t for constant speed using lengths of curves as weights.
			for (int i = curve * 6, n = i + 5; i < n; i++) {
				float bezierPercent = lengths[i];
				if (t > bezierPercent) {
					float linearPercent = 0.75f - 0.25f * (i - curve * 6 - 1);
					float bezierPercentNext = lengths[i - 1];
					t = linearPercent + 0.25f * ((t - bezierPercent) / (bezierPercentNext - bezierPercent));
					break;
				}
			}
		}

		// Calculate bezier point.
		int i = curve << 2;
		float x1 = worldVertices[i], y1 = worldVertices[i + 1];
		float cx1 = x1 + (x1 - worldVertices[i + 2]), cy1 = y1 + (y1 - worldVertices[i + 3]);
		float x2 = worldVertices[i + 4], y2 = worldVertices[i + 5];
		float cx2 = worldVertices[i + 6], cy2 = worldVertices[i + 7];
		float tt = t * t, ttt = tt * t, t3 = t * 3;
		float x = (x1 + t * (-x1 * 3 + t * (3 * x1 - x1 * t))) + t * (3 * cx1 + t * (-6 * cx1 + cx1 * t3))
			+ tt * (cx2 * 3 - cx2 * t3) + x2 * ttt;
		float y = (y1 + t * (-y1 * 3 + t * (3 * y1 - y1 * t))) + t * (3 * cy1 + t * (-6 * cy1 + cy1 * t3))
			+ tt * (cy2 * 3 - cy2 * t3) + y2 * ttt;
		return temp.set(x, y);
	}

	private void addLengths (int index, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2) {
		float tmp1x = x1 - cx1 * 2 + cx2, tmp1y = y1 - cy1 * 2 + cy2;
		float tmp2x = (cx1 - cx2) * 3 - x1 + x2, tmp2y = (cy1 - cy2) * 3 - y1 + y2;
		float dfx = (cx1 - x1) * 0.75f + tmp1x * 0.1875f + tmp2x * 0.015625f;
		float dfy = (cy1 - y1) * 0.75f + tmp1y * 0.1875f + tmp2y * 0.015625f;
		float ddfx = tmp1x * 0.375f + tmp2x * 0.09375f, ddfy = tmp1y * 0.375f + tmp2y * 0.09375f;
		float dddfx = tmp2x * 0.09375f, dddfy = tmp2y * 0.09375f;
		float length0 = (float)Math.sqrt(dfx * dfx + dfy * dfy);
		dfx += ddfx;
		dfy += ddfy;
		ddfx += dddfx;
		ddfy += dddfy;
		float length1 = length0 + (float)Math.sqrt(dfx * dfx + dfy * dfy);
		dfx += ddfx;
		dfy += ddfy;
		float length2 = length1 + (float)Math.sqrt(dfx * dfx + dfy * dfy);
		dfx += ddfx + dddfx;
		dfy += ddfy + dddfy;
		float total = length2 + (float)Math.sqrt(dfx * dfx + dfy * dfy);
		totalLength += total;
		float[] lengths = this.lengths;
		lengths[index] = 1;
		lengths[index + 1] = length2 / total;
		lengths[index + 2] = length1 / total;
		lengths[index + 3] = length0 / total;
		lengths[index + 4] = 0;
		lengths[index + 5] = total;
	}

	public Color getColor () {
		return color;
	}

	public boolean getClosed () {
		return closed;
	}

	public void setClosed (boolean closed) {
		this.closed = closed;
	}

	public void setWorldVerticesLength (int worldVerticesLength) {
		super.setWorldVerticesLength(worldVerticesLength);
		worldVertices = new float[worldVerticesLength];
		lengths = new float[(worldVerticesLength >> 2) * 6];
	}
}
