
package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.math.Intersector;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.FloatArray;

public class SutherlandHodgmanClipper {
	Vector2 tmp = new Vector2();
	final FloatArray scratch = new FloatArray();
	
	/**
	 * Clips the input triangle against the convex clipping area. If the triangle lies entirely
	 * within the clipping area, false is returned.
	 */
	public boolean clip (float[] triangle, int offset, int length, int stride, FloatArray clippingArea, FloatArray output) {
		boolean isClockwise = clockwise(clippingArea);
		
		FloatArray originalOutput = output;
		boolean clipped = false;
		
		FloatArray input = scratch;
		input.clear();
		for (int i = offset; i < offset + length; i+= stride) {
			input.add(triangle[i]);
			input.add(triangle[i + 1]);
		}
		output.clear();
		
		for (int i = 0; i < clippingArea.size; i += 2) {
			float edgeX = clippingArea.items[i];
			float edgeY = clippingArea.items[i + 1];
			float edgeX2 = clippingArea.items[(i + 2) % clippingArea.size];
			float edgeY2 = clippingArea.items[(i + 3) % clippingArea.size];
			
			if (!isClockwise) {
				float tmp = edgeX;
				edgeX = edgeX2;
				edgeX2 = tmp;
				
				tmp = edgeY;
				edgeY = edgeY2;
				edgeY2 = tmp;
			}

			for (int j = 0; j < input.size; j += 2) {
				float inputX = input.items[j % input.size];
				float inputY = input.items[(j + 1) % input.size];
				float inputX2 = input.items[(j + 2) % input.size];
				float inputY2 = input.items[(j + 3) % input.size];
				
				int side = pointLineSide(edgeX2, edgeY2, edgeX, edgeY, inputX, inputY);
				int side2 = pointLineSide(edgeX2, edgeY2, edgeX, edgeY, inputX2, inputY2);
				
				// v1 inside, v2 inside
				if (side >= 0 && side2 >= 0) {
					output.add(inputX2);
					output.add(inputY2);
				} 
				// v1 inside, v2 outside
				else if (side >= 0 && side2 < 0) {
					if (!Intersector.intersectLines(edgeX, edgeY, edgeX2, edgeY2, inputX, inputY, inputX2, inputY2, tmp)) {
						throw new RuntimeException("Lines should intersect, but didn't");
					}
					output.add(tmp.x);
					output.add(tmp.y);
					clipped = true;
				}
				// v1 outside, v2 outside
				else if (side < 0 && side2 < 0) {
					// no output
					clipped = true;
				}
				// v1 outside, v2 inside
				else if (side < 0 && side2 >= 0) {
					if (!Intersector.intersectLines(edgeX, edgeY, edgeX2, edgeY2, inputX, inputY, inputX2, inputY2, tmp)) {
						throw new RuntimeException("Lines should intersect, but didn't");
					}
					output.add(tmp.x);
					output.add(tmp.y);
					output.add(inputX2);
					output.add(inputY2);
					clipped = true;
				}
			}
			
			if (i < clippingArea.size - 2) {
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
		
		return clipped;
	}
	
	private int pointLineSide(float lineX, float lineY, float lineX2, float lineY2, float pointX, float pointY) {
		return (int)Math.signum((lineX2 - lineX) * (pointY - lineY) - (lineY2 - lineY) * (pointX - lineX));
	}
	
	public static boolean intersectLines (float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4,
		Vector2 intersection) {
		float d = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
		if (d == 0) return false;

		if (intersection != null) {
			float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / d;
			intersection.set(x1 + (x2 - x1) * ua, y1 + (y2 - y1) * ua);
		}
		return true;
	}

	public static boolean clockwise (FloatArray poly) {
		return area(poly) < 0;
	}

	public static float area (FloatArray poly) {
		float area = 0;

		for (int i = 0; i < poly.size; i += 2) {
			float x = poly.items[i];
			float y = poly.items[i + 1];
			float x2 = poly.items[(i + 2) % poly.size];
			float y2 = poly.items[(i + 3) % poly.size];

			area += x * y2 - y * x2;
		}

		return area;
	}
}
