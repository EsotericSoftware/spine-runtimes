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

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.Slot;

public class PathAttachment extends VertexAttachment {
	// Nonessential.
	final Color color = new Color(1, 0.5f, 0, 1);

	float[] worldVertices, temp;
	boolean closed, constantSpeed;

	public PathAttachment (String name) {
		super(name);
	}

	public void computeWorldVertices (Slot slot, float[] worldVertices) {
		super.computeWorldVertices(slot, worldVertices);
	}

	public void computeWorldPositions (Slot slot, float position, FloatArray lengths, FloatArray out, boolean tangents) {
		float[] vertices = this.worldVertices;
		int verticesLength = worldVerticesLength;
		int curves = verticesLength / 6;

		if (!constantSpeed) {
			for (int i = 0, n = lengths.size; i < n; i++) {
				// BOZO - Wrong. Use path length property to give !constantSpeed paths support for oob and multiple bones?
				position += lengths.get(i);
				if (closed) {
					position %= 1;
					if (position < 0) position += 1;
				} else {
					position = clamp(position, 0, 1);
					curves--;
				}
				int curve = position < 1 ? (int)(curves * position) : curves - 1;
				position = (position - curve / (float)curves) * curves;

				if (closed && curve == curves - 1) {
					super.computeWorldVertices(slot, curves * 6 - 4, 4, vertices, 0);
					super.computeWorldVertices(slot, 0, 4, vertices, 4);
				} else
					super.computeWorldVertices(slot, curve * 6 + 2, 8, vertices, 0);

				addPoint(vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5], vertices[6], vertices[7],
					position, tangents, out);
			}
			return;
		}

		if (closed) {
			super.computeWorldVertices(slot, 2, verticesLength - 2, vertices, 0);
			super.computeWorldVertices(slot, 0, 2, vertices, verticesLength - 2);
			vertices[verticesLength] = vertices[0];
			vertices[verticesLength + 1] = vertices[1];
			verticesLength += 2;
		} else {
			verticesLength -= 4;
			super.computeWorldVertices(slot, 2, verticesLength, vertices, 0);
		}

		// Curve lengths.
		float[] temp = this.temp;
		float pathLength = 0, x1 = vertices[0], y1 = vertices[1], cx1, cy1, cx2, cy2, x2, y2;
		float tmpx, tmpy, dddfx, dddfy, ddfx, ddfy, dfx, dfy;
		for (int i = 10, w = 2; w < verticesLength; i++, w += 6) {
			cx1 = vertices[w];
			cy1 = vertices[w + 1];
			cx2 = vertices[w + 2];
			cy2 = vertices[w + 3];
			x2 = vertices[w + 4];
			y2 = vertices[w + 5];
			tmpx = (x1 - cx1 * 2 + cx2) * 0.1875f;
			tmpy = (y1 - cy1 * 2 + cy2) * 0.1875f;
			dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375f;
			dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375f;
			ddfx = tmpx * 2 + dddfx;
			ddfy = tmpy * 2 + dddfy;
			dfx = (cx1 - x1) * 0.75f + tmpx + dddfx * 0.16666667f;
			dfy = (cy1 - y1) * 0.75f + tmpy + dddfy * 0.16666667f;
			pathLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
			pathLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx;
			dfy += ddfy;
			pathLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx + dddfx;
			dfy += ddfy + dddfy;
			pathLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			temp[i] = pathLength;
			x1 = x2;
			y1 = y2;
		}
		position *= pathLength;

		for (int i = 0, n = lengths.size; i < n; i++) {
			position += lengths.get(i);
			float p = position;

			if (closed) {
				p %= pathLength;
				if (p < 0) p += pathLength;
			} else if (p < 0 || p > pathLength) {
				// Outside path.
				if (p < 0) {
					x1 = vertices[0];
					y1 = vertices[1];
					cx1 = vertices[2] - x1;
					cy1 = vertices[3] - y1;
				} else {
					x1 = vertices[verticesLength - 2];
					y1 = vertices[verticesLength - 1];
					cx1 = x1 - vertices[verticesLength - 4];
					cy1 = y1 - vertices[verticesLength - 3];
					p -= pathLength;
				}
				float r = atan2(cy1, cx1);
				out.add(x1 + p * cos(r));
				out.add(y1 + p * sin(r));
				if (tangents) out.add(r + PI);
				continue;
			}

			// Determine curve containing position.
			int curve;
			float length = temp[10];
			if (p <= length) {
				curve = 0;
				p /= length;
			} else {
				for (curve = 11;; curve++) {
					length = temp[curve];
					if (p <= length) {
						float prev = temp[curve - 1];
						p = (p - prev) / (length - prev);
						break;
					}
				}
				curve = (curve - 10) * 6;
			}

			// Curve segment lengths.
			x1 = vertices[curve];
			y1 = vertices[curve + 1];
			cx1 = vertices[curve + 2];
			cy1 = vertices[curve + 3];
			cx2 = vertices[curve + 4];
			cy2 = vertices[curve + 5];
			x2 = vertices[curve + 6];
			y2 = vertices[curve + 7];
			tmpx = (x1 - cx1 * 2 + cx2) * 0.03f;
			tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
			dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f;
			dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
			ddfx = tmpx * 2 + dddfx;
			ddfy = tmpy * 2 + dddfy;
			dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f;
			dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
			length = (float)Math.sqrt(dfx * dfx + dfy * dfy);
			// BOZO! - Don't overwrite curve lengths with segment lengths.
			temp[0] = length;
			for (int ii = 1; ii < 8; ii++) {
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
				temp[ii] = length;
			}
			dfx += ddfx;
			dfy += ddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			temp[8] = length;
			dfx += ddfx + dddfx;
			dfy += ddfy + dddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			temp[9] = length;

			// Weight by segment length.
			p *= length;
			length = temp[0];
			if (p <= length)
				p = 0.1f * p / length;
			else {
				for (int ii = 1;; ii++) {
					length = temp[ii];
					if (p <= length) {
						float prev = temp[ii - 1];
						p = 0.1f * (ii + (p - prev) / (length - prev));
						break;
					}
				}
			}

			addPoint(x1, y1, cx1, cy1, cx2, cy2, x2, y2, p, tangents, out);
		}
	}

	private void addPoint (float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2, float position,
		boolean tangents, FloatArray out) {
		if (position == 0) position = 0.0001f;
		float tt = position * position, ttt = tt * position, u = 1 - position, uu = u * u, uuu = uu * u;
		float ut = u * position, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * position;
		float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out.add(x);
		out.add(y);
		if (tangents) out.add(atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt)));
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

	public boolean getConstantSpeed () {
		return constantSpeed;
	}

	public void setConstantSpeed (boolean constantSpeed) {
		this.constantSpeed = constantSpeed;
	}

	public void setWorldVerticesLength (int worldVerticesLength) {
		super.setWorldVerticesLength(worldVerticesLength);
		// BOZO! - Share working memory?
		worldVertices = new float[Math.max(2, worldVerticesLength + 4)];
		temp = new float[10 + worldVerticesLength / 6];
	}
}
