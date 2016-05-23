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

	float[] worldVertices, lengths;
	boolean closed;

	public PathAttachment (String name) {
		super(name);
	}

	public void computeWorldVertices (Slot slot, float[] worldVertices) {
		super.computeWorldVertices(slot, worldVertices);
	}

	public void computeWorldPosition (Slot slot, float position, Vector2 out) {
		// BOZO - Remove check?
		if (worldVerticesLength < 12) return;

		float[] worldVertices = this.worldVertices;
		super.computeWorldVertices(slot, worldVertices);

		// Determine curve containing position.
		float pathLength = pathLengths(worldVertices);
		float[] lengths = this.lengths;
		float target = pathLength * position, distance = 0, t = 0;
		int curve = 0;
		for (;; curve++) {
			float length = lengths[curve];
			float nextDistance = distance + length;
			if (nextDistance >= target) {
				t = (target - distance) / length;
				break;
			}
			distance = nextDistance;
		}
		curve *= 6;

		// Adjust t for constant speed using lengths of curves as weights.
		t *= curveLengths(curve, worldVertices);
		for (int i = 1;; i++) {
			float length = lengths[i];
			if (t >= length) {
				t = 1 - 0.1f * i + 0.1f * (t - length) / (lengths[i - 1] - length);
				break;
			}
		}

		// Calculate point.
		float x1 = worldVertices[curve], y1 = worldVertices[curve + 1];
		float cx1 = worldVertices[curve + 4], cy1 = worldVertices[curve + 5];
		float x2 = worldVertices[curve + 6], y2 = worldVertices[curve + 7];
		float cx2 = worldVertices[curve + 8], cy2 = worldVertices[curve + 9];
		float tt = t * t, ttt = tt * t, t3 = t * 3;
		float x = (x1 + t * (-x1 * 3 + t * (3 * x1 - x1 * t))) + t * (3 * cx1 + t * (-6 * cx1 + cx1 * t3))
			+ tt * (cx2 * 3 - cx2 * t3) + x2 * ttt;
		float y = (y1 + t * (-y1 * 3 + t * (3 * y1 - y1 * t))) + t * (3 * cy1 + t * (-6 * cy1 + cy1 * t3))
			+ tt * (cy2 * 3 - cy2 * t3) + y2 * ttt;
		out.set(x, y);
	}

	private float pathLengths (float[] worldVertices) {
		float[] lengths = this.lengths;
		float total = 0;
		float x1 = worldVertices[0], y1 = worldVertices[1];
		for (int i = 0, w = 4, n = 4 + worldVerticesLength - 6; w < n; i++, w += 6) {
			float cx1 = worldVertices[w], cy1 = worldVertices[w + 1];
			float x2 = worldVertices[w + 2], y2 = worldVertices[w + 3];
			float cx2 = worldVertices[w + 4], cy2 = worldVertices[w + 5];
			float tmpx = (x1 - cx1 * 2 + cx2) * 0.1875f, tmpy = (y1 - cy1 * 2 + cy2) * 0.1875f;
			float dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375f, dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375f;
			float ddfx = tmpx * 2 + dddfx, ddfy = tmpy * 2 + dddfy;
			float dfx = (cx1 - x1) * 0.75f + tmpx + dddfx * 0.16666667f, dfy = (cy1 - y1) * 0.75f + tmpy + dddfy * 0.16666667f;
			float length = (float)Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx;
			dfy += ddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx + dddfx;
			dfy += ddfy + dddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			total += length;
			lengths[i] = length;
			x1 = x2;
			y1 = y2;
		}
		return total;
	}

	private float curveLengths (int curve, float[] worldVertices) {
		float x1 = worldVertices[curve], y1 = worldVertices[curve + 1];
		float cx1 = worldVertices[curve + 4], cy1 = worldVertices[curve + 5];
		float x2 = worldVertices[curve + 6], y2 = worldVertices[curve + 7];
		float cx2 = worldVertices[curve + 8], cy2 = worldVertices[curve + 9];
		float tmpx = (x1 - cx1 * 2 + cx2) * 0.03f, tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
		float dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f, dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
		float ddfx = tmpx * 2 + dddfx, ddfy = tmpy * 2 + dddfy;
		float dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f, dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
		float[] lengths = this.lengths;
		lengths[10] = 0;
		float total = 0;
		for (int i = 9; i > 2; i--) {
			total += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			lengths[i] = total;
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
		}
		total += (float)Math.sqrt(dfx * dfx + dfy * dfy);
		lengths[2] = total;
		dfx += ddfx;
		dfy += ddfy;
		total += (float)Math.sqrt(dfx * dfx + dfy * dfy);
		lengths[1] = total;
		dfx += ddfx + dddfx;
		dfy += ddfy + dddfy;
		total += (float)Math.sqrt(dfx * dfx + dfy * dfy);
		lengths[0] = total;
		return total;
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
		// BOZO! - Don't reallocate for editor.
		worldVertices = new float[Math.max(2, worldVerticesLength)];
		lengths = new float[Math.max(11, worldVerticesLength >> 1)];
	}
}
