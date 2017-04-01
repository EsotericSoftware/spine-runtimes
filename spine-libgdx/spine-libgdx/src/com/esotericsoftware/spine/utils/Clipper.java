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

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.utils.FloatArray;

public class Clipper {
	private final FloatArray scratch = new FloatArray();

	/** Clips the input triangle against the convex clipping area, which needs to be clockwise. If the triangle lies entirely
	 * within the clipping area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices
	 * list. */
	public boolean clip (float x1, float y1, float x2, float y2, float x3, float y3, FloatArray clippingArea, FloatArray output) {
		FloatArray originalOutput = output;
		boolean clipped = false;

		// Avoid copy at the end.
		FloatArray input = null;
		if (clippingArea.size % 4 >= 2) {
			input = output;
			output = scratch;
		} else
			input = scratch;

		input.clear();
		input.add(x1);
		input.add(y1);
		input.add(x2);
		input.add(y2);
		input.add(x3);
		input.add(y3);
		input.add(x1);
		input.add(y1);
		output.clear();

		float[] clippingVertices = clippingArea.items;
		int clippingVerticesLength = clippingArea.size - 2;
		for (int i = 0; i < clippingVerticesLength; i += 2) {
			float edgeX = clippingVertices[i], edgeY = clippingVertices[i + 1];
			float edgeX2 = clippingVertices[i + 2], edgeY2 = clippingVertices[i + 3];
			float deltaX = edgeX - edgeX2, deltaY = edgeY - edgeY2;

			float[] inputVertices = input.items;
			int inputVerticesLength = input.size - 2;
			int numOutside = 0;
			for (int ii = 0; ii < inputVerticesLength; ii += 2) {
				float inputX = inputVertices[ii], inputY = inputVertices[ii + 1];
				float inputX2 = inputVertices[ii + 2], inputY2 = inputVertices[ii + 3];

				int side = deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0 ? 1 : -1;
				int side2 = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0 ? 1 : -1;
				if (side >= 0) {
					if (side2 >= 0) { // v1 inside, v2 inside
						output.add(inputX2);
						output.add(inputY2);
					} else { // v1 inside, v2 outside
						float c0 = inputY2 - inputY, c2 = inputX2 - inputX;
						float d = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
						float ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / d;
						output.add(edgeX + (edgeX2 - edgeX) * ua);
						output.add(edgeY + (edgeY2 - edgeY) * ua);
						clipped = true;
					}
				} else {
					if (side2 < 0) // v1 outside, v2 outside: no output
						numOutside += 2;
					else { // v1 outside, v2 inside
						float c0 = inputY2 - inputY, c2 = inputX2 - inputX;
						float d = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
						float ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / d;
						output.add(edgeX + (edgeX2 - edgeX) * ua);
						output.add(edgeY + (edgeY2 - edgeY) * ua);
						output.add(inputX2);
						output.add(inputY2);
					}
					clipped = true;
				}
			}

			// Early out if all edges were outside.
			if (numOutside == inputVerticesLength) {
				originalOutput.clear();
				return true;
			}

			output.add(output.items[0]);
			output.add(output.items[1]);

			if (i < clippingVerticesLength - 2) {
				FloatArray temp = output;
				output = input;
				output.clear();
				input = temp;
			}
		}

		if (originalOutput != output) {
			originalOutput.clear();
			originalOutput.addAll(output.items, 0, output.size - 2);
		} else
			originalOutput.setSize(originalOutput.size - 2);

		return clipped;
	}

	static public void makeClockwise (FloatArray polygon) {
		float[] vertices = polygon.items;
		int verticeslength = polygon.size;

		float area = 0, p1x, p1y, p2x, p2y;
		for (int i = 0, n = verticeslength - 3; i < n; i += 2) {
			p1x = vertices[i];
			p1y = vertices[i + 1];
			p2x = vertices[i + 2];
			p2y = vertices[i + 3];
			area += p1x * p2y - p2x * p1y;
		}
		if (area + vertices[verticeslength - 2] * vertices[1] - vertices[0] * vertices[verticeslength - 1] < 0) return;

		for (int i = 0, lastX = verticeslength - 2, n = verticeslength >> 1; i < n; i += 2) {
			float x = vertices[i], y = vertices[i + 1];
			int other = lastX - i;
			vertices[i] = vertices[other];
			vertices[i + 1] = vertices[other + 1];
			vertices[other] = x;
			vertices[other + 1] = y;
		}
	}
}
