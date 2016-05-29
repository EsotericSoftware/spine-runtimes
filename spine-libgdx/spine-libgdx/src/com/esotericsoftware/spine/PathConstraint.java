
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
	final FloatArray lengths = new FloatArray(), positions = new FloatArray();
	final FloatArray worldVertices = new FloatArray(), temp = new FloatArray();

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
		if (!translate && !rotate) return;

		PathAttachment path = (PathAttachment)attachment;
		FloatArray lengths = this.lengths;
		lengths.clear();
		lengths.add(0);
		positions.clear();

		Array<Bone> bones = this.bones;
		int boneCount = bones.size;
		if (boneCount == 1) {
			computeWorldPositions(path, rotate);
			Bone bone = bones.first();
			bone.worldX += (positions.first() - bone.worldX) * translateMix;
			bone.worldY += (positions.get(1) - bone.worldY) * translateMix;
			if (rotate) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = positions.get(2) - atan2(c, a) + data.offsetRotation * degRad;
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

		for (int i = 0; i < boneCount; i++)
			lengths.add(bones.get(i).data.length);
		computeWorldPositions(path, false);

		float[] positions = this.positions.items;
		float boneX = positions[0], boneY = positions[1];
		for (int i = 0, p = 2; i < boneCount; i++, p += 2) {
			Bone bone = bones.get(i);
			float x = positions[p], y = positions[p + 1];

			if (scale) {
				float dx = boneX - x, dy = boneY - y, d = (float)Math.sqrt(dx * dx + dy * dy);
				// BOZO - Length not transformed by bone matrix.
				float sx = bone.scaleX + (d / bone.data.length - bone.scaleX) * scaleMix;
				bone.a *= sx;
				bone.c *= sx;
			}

			bone.worldX += (boneX - bone.worldX) * translateMix;
			bone.worldY += (boneY - bone.worldY) * translateMix;

			float r = atan2(y - boneY, x - boneX) + data.offsetRotation * degRad;
			if (data.offsetRotation != 0) {
				boneX = x;
				boneY = y;
			} else {
				// BOZO - Doesn't transform by bone matrix.
				float cos = cos(r), sin = sin(r);
				float length = bone.data.length, mix = rotateMix * (1 - scaleMix);
				boneX = x + (length * cos + boneX - x) * mix;
				boneY = y + (length * sin + boneY - y) * mix;
			}
			if (rotate) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				r -= atan2(c, a);
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
		}
	}

	private void computeWorldPositions (PathAttachment path, boolean tangents) {
		Slot slot = target;
		float position = this.position;
		FloatArray out = positions;
		FloatArray lengths = this.lengths;

		int verticesLength = path.getWorldVerticesLength(), curves = verticesLength / 6;
		boolean closed = path.getClosed();
		float[] vertices;

		if (!path.getConstantSpeed()) {
			if (!closed) curves--;
			float pathLength = path.getLength();
			vertices = worldVertices.setSize(8);
			for (int i = 0, n = lengths.size; i < n; i++) {
				position += lengths.get(i) / pathLength;
				if (closed) {
					position %= 1;
					if (position < 0) position += 1;
				} else if (position < 0 || position > 1) {
					path.computeWorldVertices(slot, 0, 4, vertices, 0);
					path.computeWorldVertices(slot, verticesLength - 4, 4, vertices, 4);
					addOutsidePoint(position * pathLength, vertices, 8, pathLength, out, tangents);
					continue;
				}
				int curve = position < 1 ? (int)(curves * position) : curves - 1;
				if (closed && curve == curves - 1) {
					path.computeWorldVertices(slot, verticesLength - 4, 4, vertices, 0);
					path.computeWorldVertices(slot, 0, 4, vertices, 4);
				} else
					path.computeWorldVertices(slot, curve * 6 + 2, 8, vertices, 0);
				addCurvePoint(vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5], vertices[6], vertices[7],
					(position - curve / (float)curves) * curves, tangents, out);
			}
			return;
		}

		if (closed) {
			verticesLength += 2;
			vertices = worldVertices.setSize(verticesLength);
			path.computeWorldVertices(slot, 2, verticesLength - 4, vertices, 0);
			path.computeWorldVertices(slot, 0, 2, vertices, verticesLength - 4);
			vertices[verticesLength - 2] = vertices[0];
			vertices[verticesLength - 1] = vertices[1];
		} else {
			verticesLength -= 4;
			vertices = worldVertices.setSize(verticesLength);
			path.computeWorldVertices(slot, 2, verticesLength, vertices, 0);
		}

		// Curve lengths.
		temp.setSize(10 + curves); // BOZO - Combine with worldVertices?
		float[] temp = this.temp.items;
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
				addOutsidePoint(p, vertices, verticesLength, pathLength, out, tangents);
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

			addCurvePoint(x1, y1, cx1, cy1, cx2, cy2, x2, y2, p, tangents, out);
		}
	}

	private void addOutsidePoint (float position, float[] vertices, int verticesLength, float pathLength, FloatArray out,
		boolean tangents) {
		float x1, y1, x2, y2;
		if (position < 0) {
			x1 = vertices[0];
			y1 = vertices[1];
			x2 = vertices[2] - x1;
			y2 = vertices[3] - y1;
		} else {
			x1 = vertices[verticesLength - 2];
			y1 = vertices[verticesLength - 1];
			x2 = x1 - vertices[verticesLength - 4];
			y2 = y1 - vertices[verticesLength - 3];
			position -= pathLength;
		}
		float r = atan2(y2, x2);
		out.add(x1 + position * cos(r));
		out.add(y1 + position * sin(r));
		if (tangents) out.add(r + PI);
	}

	private void addCurvePoint (float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2, float position,
		boolean tangents, FloatArray out) {
		if (position == 0) position = 0.0001f;
		float tt = position * position, ttt = tt * position, u = 1 - position, uu = u * u, uuu = uu * u;
		float ut = u * position, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * position;
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
