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
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector2;
import com.esotericsoftware.spine.Slot;

public class PathAttachment extends VertexAttachment {
	// Nonessential.
	final Color color = new Color(1, 0.5f, 0, 1);

	float[] worldVertices, lengths;
	boolean closed, constantSpeed;

	public PathAttachment (String name) {
		super(name);
	}

	public void computeWorldVertices (Slot slot, float[] worldVertices) {
		super.computeWorldVertices(slot, worldVertices);
	}

	public void computeWorldPosition (Slot slot, float position, Vector2 worldPosition, Vector2 tangent) {
		float x1, y1, cx1, cy1, cx2, cy2, x2, y2;
		if (!constantSpeed) {
			int curves = worldVerticesLength / 6;
			if (closed) {
				position = position % 1;
				if (position < 0) position += 1;
			} else {
				position = MathUtils.clamp(position, 0, 1);
				curves--;
			}
			int curve = position < 1 ? (int)(curves * position) : curves - 1;
			position = (position - curve / (float)curves) * curves;

			float[] worldVertices = this.worldVertices;
			if (closed && curve == curves - 1) {
				super.computeWorldVertices(slot, curves * 6 - 4, 4, worldVertices, 0);
				super.computeWorldVertices(slot, 0, 4, worldVertices, 4);
			} else
				super.computeWorldVertices(slot, curve * 6 + 2, 8, worldVertices, 0);

			x1 = worldVertices[0];
			y1 = worldVertices[1];
			cx1 = worldVertices[2];
			cy1 = worldVertices[3];
			cx2 = worldVertices[4];
			cy2 = worldVertices[5];
			x2 = worldVertices[6];
			y2 = worldVertices[7];
		} else {
			float[] worldVertices = this.worldVertices;
			int verticesLength;
			if (closed) {
				verticesLength = worldVerticesLength;
				super.computeWorldVertices(slot, 2, verticesLength - 2, worldVertices, 0);
				super.computeWorldVertices(slot, 0, 2, worldVertices, verticesLength - 2);
				worldVertices[verticesLength] = worldVertices[0];
				worldVertices[verticesLength + 1] = worldVertices[1];
				verticesLength += 2;
			} else {
				verticesLength = worldVerticesLength - 4;
				super.computeWorldVertices(slot, 2, verticesLength, worldVertices, 0);
			}

			// Curve lengths.
			float[] lengths = this.lengths;
			float length = 0;
			x1 = worldVertices[0];
			y1 = worldVertices[1];
			float tmpx, tmpy, dddfx, dddfy, ddfx, ddfy, dfx, dfy;
			for (int i = 0, w = 2; w < verticesLength; i++, w += 6) {
				cx1 = worldVertices[w];
				cy1 = worldVertices[w + 1];
				cx2 = worldVertices[w + 2];
				cy2 = worldVertices[w + 3];
				x2 = worldVertices[w + 4];
				y2 = worldVertices[w + 5];
				tmpx = (x1 - cx1 * 2 + cx2) * 0.1875f;
				tmpy = (y1 - cy1 * 2 + cy2) * 0.1875f;
				dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375f;
				dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375f;
				ddfx = tmpx * 2 + dddfx;
				ddfy = tmpy * 2 + dddfy;
				dfx = (cx1 - x1) * 0.75f + tmpx + dddfx * 0.16666667f;
				dfy = (cy1 - y1) * 0.75f + tmpy + dddfy * 0.16666667f;
				length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
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
				lengths[i] = length;
				x1 = x2;
				y1 = y2;
			}
			position *= length;

			if (closed) {
				position = position % length;
				if (position < 0) position += length;
			} else if (position < 0 || position > length) {
				// Outside curve.
				if (position < 0) {
					x1 = worldVertices[0];
					y1 = worldVertices[1];
					cx1 = worldVertices[2] - x1;
					cy1 = worldVertices[3] - y1;
				} else {
					x1 = worldVertices[verticesLength - 2];
					y1 = worldVertices[verticesLength - 1];
					cx1 = x1 - worldVertices[verticesLength - 4];
					cy1 = y1 - worldVertices[verticesLength - 3];
					position -= length;
				}
				float r = MathUtils.atan2(cy1, cx1);
				float cos = MathUtils.cos(r), sin = MathUtils.sin(r);
				worldPosition.x = x1 + position * cos;
				worldPosition.y = y1 + position * sin;
				if (tangent != null) {
					tangent.x = worldPosition.x - cos;
					tangent.y = worldPosition.y - sin;
				}
				return;
			}

			// Determine curve containing position.
			int curve;
			length = lengths[0];
			if (position <= length) {
				curve = 0;
				position /= length;
			} else {
				for (curve = 1;; curve++) {
					length = lengths[curve];
					if (position <= length) {
						float prev = lengths[curve - 1];
						position = (position - prev) / (length - prev);
						break;
					}
				}
				curve *= 6;
			}

			// Curve segment lengths.
			x1 = worldVertices[curve];
			y1 = worldVertices[curve + 1];
			cx1 = worldVertices[curve + 2];
			cy1 = worldVertices[curve + 3];
			cx2 = worldVertices[curve + 4];
			cy2 = worldVertices[curve + 5];
			x2 = worldVertices[curve + 6];
			y2 = worldVertices[curve + 7];
			tmpx = (x1 - cx1 * 2 + cx2) * 0.03f;
			tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
			dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f;
			dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
			ddfx = tmpx * 2 + dddfx;
			ddfy = tmpy * 2 + dddfy;
			dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f;
			dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
			length = (float)Math.sqrt(dfx * dfx + dfy * dfy);
			lengths[0] = length;
			for (int i = 1; i < 8; i++) {
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
				lengths[i] = length;
			}
			dfx += ddfx;
			dfy += ddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			lengths[8] = length;
			dfx += ddfx + dddfx;
			dfy += ddfy + dddfy;
			length += (float)Math.sqrt(dfx * dfx + dfy * dfy);
			lengths[9] = length;

			// Weight by segment length.
			position *= length;
			length = lengths[0];
			if (position <= length)
				position = 0.1f * position / length;
			else {
				for (int i = 1;; i++) {
					length = lengths[i];
					if (position <= length) {
						float prev = lengths[i - 1];
						position = 0.1f * (i + (position - prev) / (length - prev));
						break;
					}
				}
			}
		}

		// Calculate point and tangent.
		position += 0.0001f;
		float tt = position * position, ttt = tt * position, u = 1 - position, uu = u * u, uuu = uu * u;
		float ut = u * position, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * position;
		worldPosition.x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt;
		worldPosition.y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		if (tangent != null) {
			tangent.x = x1 * uu + cx1 * ut * 2 + cx2 * tt;
			tangent.y = y1 * uu + cy1 * ut * 2 + cy2 * tt;
		}
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
		worldVertices = new float[Math.max(2, worldVerticesLength + 4)];
		lengths = new float[Math.max(10, worldVerticesLength / 6)];
	}
}
