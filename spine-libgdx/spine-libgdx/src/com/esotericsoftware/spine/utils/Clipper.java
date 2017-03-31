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
	final FloatArray scratch = new FloatArray();

	/** Clips the input triangle against the convex clipping area, which needs to be clockwise. If the triangle lies entirely within the clipping area, false is
	 * returned. The clipping area must duplicate the first vertex at the end of the vertices list! */
	public boolean clip (float x1, float y1, float x2, float y2, float x3, float y3, FloatArray clippingArea, FloatArray output) {
		final FloatArray originalOutput = output;
		boolean clipped = false;

		FloatArray input = null;
		// avoid copy at the end
		if ((clippingArea.size / 2) % 2 != 0) {
			input = output;
			output = scratch;
		} else {
			input = scratch;
		}

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

		final float[] clippingVertices = clippingArea.items;
		final int clippingVerticesLength = clippingArea.size - 2;
		for (int i = 0; i < clippingVerticesLength; i += 2) {
			float edgeX = clippingVertices[i];
			float edgeY = clippingVertices[i + 1];
			float edgeX2 = clippingVertices[i + 2];
			float edgeY2 = clippingVertices[i + 3];

			final float deltaX = edgeX - edgeX2;
			final float deltaY = edgeY - edgeY2;

			final float[] inputVertices = input.items;
			final int inputVerticesLength = input.size - 2;
			
			int numOutside = 0; 

			for (int j = 0; j < inputVerticesLength; j += 2) {
				final float inputX = inputVertices[j];
				final float inputY = inputVertices[j + 1];
				final float inputX2 = inputVertices[j + 2];
				final float inputY2 = inputVertices[j + 3];

				final int side = deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0 ? 1 : -1;
				final int side2 = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0 ? 1 : -1;

				if (side >= 0) {
					// v1 inside, v2 inside
					if (side2 >= 0) {
						output.add(inputX2);
						output.add(inputY2);
					}
					// v1 inside, v2 outside
					else {						
						float c0 = inputY2 - inputY;
						float c1 = edgeX2 - edgeX;
						float c2 = inputX2 - inputX;
						float c3 = edgeY2 - edgeY;
						float d = c0 * c1 - c2 * c3;

						float ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / d;
						output.add(edgeX + (edgeX2 - edgeX) * ua);
						output.add(edgeY + (edgeY2 - edgeY) * ua);
						clipped = true;
					}
				} else {
					// v1 outside, v2 outside
					if (side2 < 0) {
						// no output
						clipped = true;
						numOutside += 2;
					}
					// v1 outside, v2 inside
					else {
						float c0 = inputY2 - inputY;
						float c1 = edgeX2 - edgeX;
						float c2 = inputX2 - inputX;
						float c3 = edgeY2 - edgeY;
						float d = c0 * c1 - c2 * c3;

						float ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / d;
						output.add(edgeX + (edgeX2 - edgeX) * ua);
						output.add(edgeY + (edgeY2 - edgeY) * ua);
						
						output.add(inputX2);
						output.add(inputY2);
						clipped = true;
					}
				}
			}
			
			// early out if all edges were outside
			if (numOutside == inputVerticesLength) {
				originalOutput.clear();
				return true;
			}

			output.add(output.items[0]);
			output.add(output.items[1]);

			if (i < clippingVerticesLength - 2) {
				FloatArray tmp = output;
				output = input;
				output.clear();
				input = tmp;
			}
		}

		if (originalOutput != output) {
			originalOutput.clear();
			originalOutput.addAll(output.items, 0, output.size);
		}

		originalOutput.setSize(originalOutput.size - 2);

		return clipped;
	}
	
	public static void makeClockwise (FloatArray poly) {
		if (isClockwise(poly)) return;
		
		int lastX = poly.size - 2;
		final float[] polygon = poly.items;
		for (int i = 0, n = poly.size / 2; i < n; i += 2) {
			int other = lastX - i;
			float x = polygon[i];
			float y = polygon[i + 1];
			polygon[i] = polygon[other];
			polygon[i + 1] = polygon[other + 1];
			polygon[other] = x;
			polygon[other + 1] = y;
		}
	}

	public static boolean isClockwise (FloatArray poly) {
		return area(poly) < 0;
	}

	public static float area (FloatArray poly) {
		float area = 0;

		final float[] polyVertices = poly.items;
		final int polySize = poly.size;
		for (int i = 0; i < polySize; i += 2) {
			float x = polyVertices[i];
			float y = polyVertices[i + 1];
			float x2 = polyVertices[(i + 2) % poly.size];
			float y2 = polyVertices[(i + 3) % poly.size];

			area += x * y2 - y * x2;
		}

		return area;
	}
}
