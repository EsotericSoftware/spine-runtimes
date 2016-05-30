
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.PathAttachment;

public class PathConstraint implements Updatable {
	final PathConstraintData data;
	final Array<Bone> bones;
	Slot target;
	float position, rotateMix, translateMix, scaleMix;
	final FloatArray lengths = new FloatArray(), positions = new FloatArray(), temp = new FloatArray();

	public PathConstraint (PathConstraintData data, Skeleton skeleton) {
		this.data = data;
		position = data.position;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;
		scaleMix = data.scaleMix;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.findBone(boneData.name));

		target = skeleton.findSlot(data.target.name);
	}

	/** Copy constructor. */
	public PathConstraint (PathConstraint constraint, Skeleton skeleton) {
		data = constraint.data;
		bones = new Array(constraint.bones.size);
		for (Bone bone : constraint.bones)
			bones.add(skeleton.bones.get(bone.data.index));
		target = skeleton.slots.get(constraint.target.data.index);
		position = constraint.position;
		rotateMix = constraint.rotateMix;
		translateMix = constraint.translateMix;
		scaleMix = constraint.scaleMix;
	}

	public void apply () {
		update();
	}

	public void update () {
		Attachment attachment = target.getAttachment();
		if (!(attachment instanceof PathAttachment)) return;

		float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix;
		boolean translate = translateMix > 0, rotate = rotateMix > 0, scale = scaleMix > 0;
		if (!translate && !rotate && !scale) return;

		PathAttachment path = (PathAttachment)attachment;
		FloatArray lengths = this.lengths;
		lengths.clear();
		lengths.add(0);

		Array<Bone> bones = this.bones;
		int boneCount = bones.size;
		if (boneCount == 1) {
			float[] positions = computeWorldPositions(path, rotate);
			Bone bone = bones.first();
			bone.worldX += (positions[0] - bone.worldX) * translateMix;
			bone.worldY += (positions[1] - bone.worldY) * translateMix;
			if (rotate) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = positions[2] - atan2(c, a) + data.offsetRotation * degRad;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r *= rotateMix;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			return;
		}

		for (int i = 0; i < boneCount; i++) {
			Bone bone = bones.get(i);
			float length = bone.data.length;
			float x = length * bone.a, y = length * bone.c;
			lengths.add((float)Math.sqrt(x * x + y * y));
		}
		float[] positions = computeWorldPositions(path, false);

		float boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
		for (int i = 0, p = 2; i < boneCount; i++, p += 2) {
			Bone bone = bones.get(i);
			bone.worldX += (boneX - bone.worldX) * translateMix;
			bone.worldY += (boneY - bone.worldY) * translateMix;
			float x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
			if (scale) {
				float s = ((float)Math.sqrt(dx * dx + dy * dy) / lengths.get(i + 1) - 1) * scaleMix + 1;
				bone.a *= s;
				bone.c *= s;
			}
			if (!rotate) {
				boneX = x;
				boneY = y;
			} else {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(dy, dx) - atan2(c, a) + offsetRotation * degRad, cos, sin;
				if (offsetRotation != 0) {
					boneX = x;
					boneY = y;
				} else { // Mix between on path and at tip.
					cos = cos(r);
					sin = sin(r);
					float length = bone.data.length;
					boneX = x + (length * (cos * a - sin * c) - dx) * rotateMix;
					boneY = y + (length * (sin * a + cos * c) - dy) * rotateMix;
				}
				if (r > PI)
					r -= PI2;
				else if (r < -PI) //
					r += PI2;
				r *= rotateMix;
				cos = cos(r);
				sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
		}
	}

	private float[] computeWorldPositions (PathAttachment path, boolean tangents) {
		Slot target = this.target;
		float position = this.position;
		int lengthCount = lengths.size;
		float[] lengths = this.lengths.items;
		FloatArray positions = this.positions;
		positions.clear();
		boolean closed = path.getClosed();
		int verticesLength = path.getWorldVerticesLength(), curves = verticesLength / 6;
		float[] temp;
		int lastCurve = -1;

		if (!path.getConstantSpeed()) {
			if (!closed) curves--;
			float pathLength = path.getLength();
			temp = this.temp.setSize(8);
			for (int i = 0; i < lengthCount; i++) {
				position += lengths[i] / pathLength;
				if (closed) {
					position %= 1;
					if (position < 0) position += 1;
				} else if (position < 0) {
					if (lastCurve != -2) {
						lastCurve = -2;
						path.computeWorldVertices(target, 2, 4, temp, 0);
					}
					addBeforePosition(position * pathLength, temp, 0, positions, tangents);
					continue;
				} else if (position > 1) {
					if (lastCurve != -3) {
						lastCurve = -3;
						path.computeWorldVertices(target, verticesLength - 6, 4, temp, 0);
					}
					addAfterPosition((position - 1) * pathLength, temp, 0, positions, tangents);
					continue;
				}
				int curve = position < 1 ? (int)(curves * position) : curves - 1;
				if (curve != lastCurve) {
					lastCurve = curve;
					if (closed && curve == curves - 1) {
						path.computeWorldVertices(target, verticesLength - 4, 4, temp, 0);
						path.computeWorldVertices(target, 0, 4, temp, 4);
					} else
						path.computeWorldVertices(target, curve * 6 + 2, 8, temp, 0);
				}
				addCurvePosition((position - curve / (float)curves) * curves, temp[0], temp[1], temp[2], temp[3], temp[4], temp[5],
					temp[6], temp[7], positions, tangents);
			}
			return positions.items;
		}

		// World vertices, verticesStart to verticesStart + verticesLength.
		int verticesStart = 10 + curves;
		temp = this.temp.setSize(verticesStart + verticesLength + 2);
		if (closed) {
			verticesLength += 2;
			int verticesEnd = verticesStart + verticesLength;
			path.computeWorldVertices(target, 2, verticesLength - 4, temp, verticesStart);
			path.computeWorldVertices(target, 0, 2, temp, verticesEnd - 4);
			temp[verticesEnd - 2] = temp[verticesStart];
			temp[verticesEnd - 1] = temp[verticesStart + 1];
		} else {
			verticesStart--;
			verticesLength -= 4;
			path.computeWorldVertices(target, 2, verticesLength, temp, verticesStart);
		}

		// Curve lengths, 10 to verticesStart.
		float pathLength = 0;
		float x1 = temp[verticesStart], y1 = temp[verticesStart + 1], cx1 = 0, cy1 = 0, cx2 = 0, cy2 = 0, x2 = 0, y2 = 0;
		float tmpx, tmpy, dddfx, dddfy, ddfx, ddfy, dfx, dfy;
		for (int i = 10, v = verticesStart + 2; i < verticesStart; i++, v += 6) {
			cx1 = temp[v];
			cy1 = temp[v + 1];
			cx2 = temp[v + 2];
			cy2 = temp[v + 3];
			x2 = temp[v + 4];
			y2 = temp[v + 5];
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

		float curveLength = 0;
		for (int i = 0; i < lengthCount; i++) {
			position += lengths[i];
			float p = position;

			if (closed) {
				p %= pathLength;
				if (p < 0) p += pathLength;
			} else if (p < 0) {
				addBeforePosition(p, temp, verticesStart, positions, tangents);
				continue;
			} else if (p > pathLength) {
				addAfterPosition(p - pathLength, temp, verticesStart + verticesLength - 4, positions, tangents);
				continue;
			}

			// Determine curve containing position.
			int curve;
			float length = temp[10];
			if (p <= length) {
				curve = verticesStart;
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
				curve = verticesStart + (curve - 10) * 6;
			}

			// Curve segment lengths, 0 to 10.
			if (curve != lastCurve) {
				lastCurve = curve;
				x1 = temp[curve];
				y1 = temp[curve + 1];
				cx1 = temp[curve + 2];
				cy1 = temp[curve + 3];
				cx2 = temp[curve + 4];
				cy2 = temp[curve + 5];
				x2 = temp[curve + 6];
				y2 = temp[curve + 7];
				tmpx = (x1 - cx1 * 2 + cx2) * 0.03f;
				tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
				dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f;
				dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
				ddfx = tmpx * 2 + dddfx;
				ddfy = tmpy * 2 + dddfy;
				dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f;
				dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
				curveLength = (float)Math.sqrt(dfx * dfx + dfy * dfy);
				temp[0] = curveLength;
				for (int ii = 1; ii < 8; ii++) {
					dfx += ddfx;
					dfy += ddfy;
					ddfx += dddfx;
					ddfy += dddfy;
					curveLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
					temp[ii] = curveLength;
				}
				dfx += ddfx;
				dfy += ddfy;
				curveLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
				temp[8] = curveLength;
				dfx += ddfx + dddfx;
				dfy += ddfy + dddfy;
				curveLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
				temp[9] = curveLength;
			}

			// Weight by segment length.
			p *= curveLength;
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

			addCurvePosition(p, x1, y1, cx1, cy1, cx2, cy2, x2, y2, positions, tangents);
		}

		return positions.items;
	}

	private void addBeforePosition (float p, float[] temp, int i, FloatArray out, boolean tangents) {
		float x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = atan2(dy, dx);
		out.add(x1 + p * cos(r));
		out.add(y1 + p * sin(r));
		if (tangents) out.add(r + PI);
	}

	private void addAfterPosition (float p, float[] temp, int i, FloatArray out, boolean tangents) {
		float x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = atan2(dy, dx);
		out.add(x1 + p * cos(r));
		out.add(y1 + p * sin(r));
		if (tangents) out.add(r + PI);
	}

	private void addCurvePosition (float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2,
		FloatArray out, boolean tangents) {
		if (p == 0) p = 0.0001f;
		float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
		float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
		float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out.add(x);
		out.add(y);
		if (tangents) out.add(atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt)));
	}

	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	public float getRotateMix () {
		return rotateMix;
	}

	public void setRotateMix (float rotateMix) {
		this.rotateMix = rotateMix;
	}

	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public float getScaleMix () {
		return scaleMix;
	}

	public void setScaleMix (float scaleMix) {
		this.scaleMix = scaleMix;
	}

	public Array<Bone> getBones () {
		return bones;
	}

	public Slot getTarget () {
		return target;
	}

	public void setTarget (Slot target) {
		this.target = target;
	}

	public PathConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
