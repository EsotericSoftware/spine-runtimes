
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.PathAttachment;

public class PathConstraint implements Updatable {
	static private final int NONE = -1, BEFORE = -2, AFTER = -3;

	final PathConstraintData data;
	final Array<Bone> bones;
	Slot target;
	float position, spacing, rotateMix, translateMix;

	private final FloatArray spaces = new FloatArray(), positions = new FloatArray();
	private final FloatArray world = new FloatArray(), curves = new FloatArray(), lengths = new FloatArray();
	private final float[] segments = new float[10];

	public PathConstraint (PathConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.findBone(boneData.name));
		target = skeleton.findSlot(data.target.name);
		position = data.position;
		spacing = data.spacing;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;
	}

	/** Copy constructor. */
	public PathConstraint (PathConstraint constraint, Skeleton skeleton) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = constraint.data;
		bones = new Array(constraint.bones.size);
		for (Bone bone : constraint.bones)
			bones.add(skeleton.bones.get(bone.data.index));
		target = skeleton.slots.get(constraint.target.data.index);
		position = constraint.position;
		spacing = constraint.spacing;
		rotateMix = constraint.rotateMix;
		translateMix = constraint.translateMix;
	}

	public void apply () {
		update();
	}

	@SuppressWarnings("null")
	public void update () {
		Attachment attachment = target.attachment;
		if (!(attachment instanceof PathAttachment)) return;

		float rotateMix = this.rotateMix, translateMix = this.translateMix;
		boolean translate = translateMix > 0, rotate = rotateMix > 0;
		if (!translate && !rotate) return;

		PathConstraintData data = this.data;
		SpacingMode spacingMode = data.spacingMode;
		boolean lengthSpacing = spacingMode == SpacingMode.length;
		RotateMode rotateMode = data.rotateMode;
		boolean tangents = rotateMode == RotateMode.tangent, scale = rotateMode == RotateMode.chainScale;
		int boneCount = this.bones.size, spacesCount = tangents ? boneCount : boneCount + 1;
		Object[] bones = this.bones.items;
		float[] spaces = this.spaces.setSize(spacesCount), lengths = null;
		float spacing = this.spacing;
		if (scale || lengthSpacing) {
			if (scale) lengths = this.lengths.setSize(boneCount);
			for (int i = 0, n = spacesCount - 1; i < n;) {
				Bone bone = (Bone)bones[i];
				float length = bone.data.length, x = length * bone.a, y = length * bone.c;
				length = (float)Math.sqrt(x * x + y * y);
				if (scale) lengths[i] = length;
				spaces[++i] = lengthSpacing ? Math.max(0, length + spacing) : spacing;
			}
		} else {
			for (int i = 1; i < spacesCount; i++)
				spaces[i] = spacing;
		}

		float[] positions = computeWorldPositions((PathAttachment)attachment, spacesCount, tangents,
			data.positionMode == PositionMode.percent, spacingMode == SpacingMode.percent);
		Skeleton skeleton = target.getSkeleton();
		float skeletonX = skeleton.x, skeletonY = skeleton.y;
		float boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
		boolean tip = rotateMode == RotateMode.chain && offsetRotation == 0;
		for (int i = 0, p = 3; i < boneCount; i++, p += 3) {
			Bone bone = (Bone)bones[i];
			bone.worldX += (boneX - skeletonX - bone.worldX) * translateMix;
			bone.worldY += (boneY - skeletonY - bone.worldY) * translateMix;
			float x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
			if (scale) {
				float length = lengths[i];
				if (length != 0) {
					float s = ((float)Math.sqrt(dx * dx + dy * dy) / length - 1) * rotateMix + 1;
					bone.a *= s;
					bone.c *= s;
				}
			}
			boneX = x;
			boneY = y;
			if (rotate) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d, r, cos, sin;
				if (tangents)
					r = positions[p - 1];
				else if (spaces[i + 1] == 0)
					r = positions[p + 2];
				else
					r = atan2(dy, dx);
				r -= atan2(c, a) - offsetRotation * degRad;
				if (tip) {
					cos = cos(r);
					sin = sin(r);
					float length = bone.data.length;
					boneX += (length * (cos * a - sin * c) - dx) * rotateMix;
					boneY += (length * (sin * a + cos * c) - dy) * rotateMix;
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

	float[] computeWorldPositions (PathAttachment path, int spacesCount, boolean tangents, boolean percentPosition,
		boolean percentSpacing) {
		Slot target = this.target;
		float position = this.position;
		float[] spaces = this.spaces.items, out = this.positions.setSize(spacesCount * 3 + 2), world;
		boolean closed = path.getClosed();
		int verticesLength = path.getWorldVerticesLength(), curveCount = verticesLength / 6, prevCurve = NONE;

		if (!path.getConstantSpeed()) {
			float[] lengths = path.getLengths();
			curveCount -= closed ? 1 : 2;
			float pathLength = lengths[curveCount];
			if (percentPosition) position *= pathLength;
			if (percentSpacing) {
				for (int i = 0; i < spacesCount; i++)
					spaces[i] *= pathLength;
			}
			world = this.world.setSize(8);
			for (int i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
				float space = spaces[i];
				position += space;
				float p = position;

				if (closed) {
					p %= pathLength;
					if (p < 0) p += pathLength;
					curve = 0;
				} else if (p < 0) {
					if (prevCurve != BEFORE) {
						prevCurve = BEFORE;
						path.computeWorldVertices(target, 2, 4, world, 0);
					}
					addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					if (prevCurve != AFTER) {
						prevCurve = AFTER;
						path.computeWorldVertices(target, verticesLength - 6, 4, world, 0);
					}
					addAfterPosition(p - pathLength, world, 0, out, o);
					continue;
				}

				// Determine curve containing position.
				for (;; curve++) {
					float length = lengths[curve];
					if (p > length) continue;
					if (curve == 0)
						p /= length;
					else {
						float prev = lengths[curve - 1];
						p = (p - prev) / (length - prev);
					}
					break;
				}
				if (curve != prevCurve) {
					prevCurve = curve;
					if (closed && curve == curveCount) {
						path.computeWorldVertices(target, verticesLength - 4, 4, world, 0);
						path.computeWorldVertices(target, 0, 4, world, 4);
					} else
						path.computeWorldVertices(target, curve * 6 + 2, 8, world, 0);
				}
				addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o,
					tangents || (i > 0 && space == 0));
			}
			return out;
		}

		// World vertices.
		if (closed) {
			verticesLength += 2;
			world = this.world.setSize(verticesLength);
			path.computeWorldVertices(target, 2, verticesLength - 4, world, 0);
			path.computeWorldVertices(target, 0, 2, world, verticesLength - 4);
			world[verticesLength - 2] = world[0];
			world[verticesLength - 1] = world[1];
		} else {
			curveCount--;
			verticesLength -= 4;
			world = this.world.setSize(verticesLength);
			path.computeWorldVertices(target, 2, verticesLength, world, 0);
		}

		// Curve lengths.
		float[] curves = this.curves.setSize(curveCount);
		float pathLength = 0;
		float x1 = world[0], y1 = world[1], cx1 = 0, cy1 = 0, cx2 = 0, cy2 = 0, x2 = 0, y2 = 0;
		float tmpx, tmpy, dddfx, dddfy, ddfx, ddfy, dfx, dfy;
		for (int i = 0, w = 2; i < curveCount; i++, w += 6) {
			cx1 = world[w];
			cy1 = world[w + 1];
			cx2 = world[w + 2];
			cy2 = world[w + 3];
			x2 = world[w + 4];
			y2 = world[w + 5];
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
			curves[i] = pathLength;
			x1 = x2;
			y1 = y2;
		}
		if (percentPosition) position *= pathLength;
		if (percentSpacing) {
			for (int i = 0; i < spacesCount; i++)
				spaces[i] *= pathLength;
		}

		float[] segments = this.segments;
		float curveLength = 0;
		for (int i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
			float space = spaces[i];
			position += space;
			float p = position;

			if (closed) {
				p %= pathLength;
				if (p < 0) p += pathLength;
				curve = 0;
			} else if (p < 0) {
				addBeforePosition(p, world, 0, out, o);
				continue;
			} else if (p > pathLength) {
				addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
				continue;
			}

			// Determine curve containing position.
			for (;; curve++) {
				float length = curves[curve];
				if (p > length) continue;
				if (curve == 0)
					p /= length;
				else {
					float prev = curves[curve - 1];
					p = (p - prev) / (length - prev);
				}
				break;
			}

			// Curve segment lengths.
			if (curve != prevCurve) {
				prevCurve = curve;
				int ii = curve * 6;
				x1 = world[ii];
				y1 = world[ii + 1];
				cx1 = world[ii + 2];
				cy1 = world[ii + 3];
				cx2 = world[ii + 4];
				cy2 = world[ii + 5];
				x2 = world[ii + 6];
				y2 = world[ii + 7];
				tmpx = (x1 - cx1 * 2 + cx2) * 0.03f;
				tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
				dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f;
				dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
				ddfx = tmpx * 2 + dddfx;
				ddfy = tmpy * 2 + dddfy;
				dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f;
				dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
				curveLength = (float)Math.sqrt(dfx * dfx + dfy * dfy);
				segments[0] = curveLength;
				for (ii = 1; ii < 8; ii++) {
					dfx += ddfx;
					dfy += ddfy;
					ddfx += dddfx;
					ddfy += dddfy;
					curveLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
					segments[ii] = curveLength;
				}
				dfx += ddfx;
				dfy += ddfy;
				curveLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
				segments[8] = curveLength;
				dfx += ddfx + dddfx;
				dfy += ddfy + dddfy;
				curveLength += (float)Math.sqrt(dfx * dfx + dfy * dfy);
				segments[9] = curveLength;
				segment = 0;
			}

			// Weight by segment length.
			p *= curveLength;
			for (;; segment++) {
				float length = segments[segment];
				if (p > length) continue;
				if (segment == 0)
					p /= length;
				else {
					float prev = segments[segment - 1];
					p = segment + (p - prev) / (length - prev);
				}
				break;
			}
			addCurvePosition(p * 0.1f, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space == 0));
		}
		return out;
	}

	private void addBeforePosition (float p, float[] temp, int i, float[] out, int o) {
		float x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = atan2(dy, dx);
		out[o] = x1 + p * cos(r);
		out[o + 1] = y1 + p * sin(r);
		out[o + 2] = r;
	}

	private void addAfterPosition (float p, float[] temp, int i, float[] out, int o) {
		float x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = atan2(dy, dx);
		out[o] = x1 + p * cos(r);
		out[o + 1] = y1 + p * sin(r);
		out[o + 2] = r;
	}

	private void addCurvePosition (float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2,
		float[] out, int o, boolean tangents) {
		if (p == 0) p = 0.0001f;
		float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
		float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
		float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out[o] = x;
		out[o + 1] = y;
		if (tangents) out[o + 2] = atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
	}

	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	public float getSpacing () {
		return spacing;
	}

	public void setSpacing (float spacing) {
		this.spacing = spacing;
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
