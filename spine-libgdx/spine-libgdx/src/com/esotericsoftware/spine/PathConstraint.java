/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import java.util.Arrays;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;

import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.utils.SpineUtils;

/** Stores the current pose for a path constraint. A path constraint adjusts the rotation, translation, and scale of the
 * constrained bones so they follow a {@link PathAttachment}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-path-constraints">Path constraints</a> in the Spine User Guide. */
public class PathConstraint implements Updatable {
	static private final int NONE = -1, BEFORE = -2, AFTER = -3;
	static private final float epsilon = 0.00001f;

	final PathConstraintData data;
	final Array<Bone> bones;
	Slot target;
	float position, spacing, mixRotate, mixX, mixY;

	boolean active;

	private final FloatArray spaces = new FloatArray(), positions = new FloatArray();
	private final FloatArray world = new FloatArray(), curves = new FloatArray(), lengths = new FloatArray();
	private final float[] segments = new float[10];

	public PathConstraint (PathConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.get(boneData.index));
		target = skeleton.slots.get(data.target.index);
		position = data.position;
		spacing = data.spacing;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
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
		mixRotate = constraint.mixRotate;
		mixX = constraint.mixX;
		mixY = constraint.mixY;
	}

	/** Applies the constraint to the constrained bones. */
	public void update () {
		Attachment attachment = target.attachment;
		if (!(attachment instanceof PathAttachment)) return;

		float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY;
		if (mixRotate == 0 && mixX == 0 && mixY == 0) return;

		PathConstraintData data = this.data;
		boolean tangents = data.rotateMode == RotateMode.tangent, scale = data.rotateMode == RotateMode.chainScale;
		int boneCount = this.bones.size, spacesCount = tangents ? boneCount : boneCount + 1;
		Object[] bones = this.bones.items;
		float[] spaces = this.spaces.setSize(spacesCount), lengths = scale ? this.lengths.setSize(boneCount) : null;
		float spacing = this.spacing;

		switch (data.spacingMode) {
		case percent:
			if (scale) {
				for (int i = 0, n = spacesCount - 1; i < n; i++) {
					Bone bone = (Bone)bones[i];
					float setupLength = bone.data.length;
					if (setupLength < epsilon)
						lengths[i] = 0;
					else {
						float x = setupLength * bone.a, y = setupLength * bone.c;
						lengths[i] = (float)Math.sqrt(x * x + y * y);
					}
				}
			}
			Arrays.fill(spaces, 1, spacesCount, spacing);
			break;
		case proportional:
			float sum = 0;
			for (int i = 0, n = spacesCount - 1; i < n;) {
				Bone bone = (Bone)bones[i];
				float setupLength = bone.data.length;
				if (setupLength < epsilon) {
					if (scale) lengths[i] = 0;
					spaces[++i] = spacing;
				} else {
					float x = setupLength * bone.a, y = setupLength * bone.c;
					float length = (float)Math.sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = length;
					sum += length;
				}
			}
			if (sum > 0) {
				sum = spacesCount / sum * spacing;
				for (int i = 1; i < spacesCount; i++)
					spaces[i] *= sum;
			}
			break;
		default:
			boolean lengthSpacing = data.spacingMode == SpacingMode.length;
			for (int i = 0, n = spacesCount - 1; i < n;) {
				Bone bone = (Bone)bones[i];
				float setupLength = bone.data.length;
				if (setupLength < epsilon) {
					if (scale) lengths[i] = 0;
					spaces[++i] = spacing;
				} else {
					float x = setupLength * bone.a, y = setupLength * bone.c;
					float length = (float)Math.sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = (lengthSpacing ? setupLength + spacing : spacing) * length / setupLength;
				}
			}
		}

		float[] positions = computeWorldPositions((PathAttachment)attachment, spacesCount, tangents);
		float boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
		boolean tip;
		if (offsetRotation == 0)
			tip = data.rotateMode == RotateMode.chain;
		else {
			tip = false;
			Bone p = target.bone;
			offsetRotation *= p.a * p.d - p.b * p.c > 0 ? SpineUtils.degRad : -SpineUtils.degRad;
		}
		for (int i = 0, p = 3; i < boneCount; i++, p += 3) {
			Bone bone = (Bone)bones[i];
			bone.worldX += (boneX - bone.worldX) * mixX;
			bone.worldY += (boneY - bone.worldY) * mixY;
			float x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
			if (scale) {
				float length = lengths[i];
				if (length >= epsilon) {
					float s = ((float)Math.sqrt(dx * dx + dy * dy) / length - 1) * mixRotate + 1;
					bone.a *= s;
					bone.c *= s;
				}
			}
			boneX = x;
			boneY = y;
			if (mixRotate > 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d, r, cos, sin;
				if (tangents)
					r = positions[p - 1];
				else if (spaces[i + 1] < epsilon)
					r = positions[p + 2];
				else
					r = (float)Math.atan2(dy, dx);
				r -= (float)Math.atan2(c, a);
				if (tip) {
					cos = (float)Math.cos(r);
					sin = (float)Math.sin(r);
					float length = bone.data.length;
					boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
					boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
				} else
					r += offsetRotation;
				if (r > SpineUtils.PI)
					r -= SpineUtils.PI2;
				else if (r < -SpineUtils.PI) //
					r += SpineUtils.PI2;
				r *= mixRotate;
				cos = (float)Math.cos(r);
				sin = (float)Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			bone.updateAppliedTransform();
		}
	}

	float[] computeWorldPositions (PathAttachment path, int spacesCount, boolean tangents) {
		Slot target = this.target;
		float position = this.position;
		float[] spaces = this.spaces.items, out = this.positions.setSize(spacesCount * 3 + 2), world;
		boolean closed = path.getClosed();
		int verticesLength = path.getWorldVerticesLength(), curveCount = verticesLength / 6, prevCurve = NONE;

		if (!path.getConstantSpeed()) {
			float[] lengths = path.getLengths();
			curveCount -= closed ? 1 : 2;
			float pathLength = lengths[curveCount];

			if (data.positionMode == PositionMode.percent) position *= pathLength;

			float multiplier;
			switch (data.spacingMode) {
			case percent:
				multiplier = pathLength;
				break;
			case proportional:
				multiplier = pathLength / spacesCount;
				break;
			default:
				multiplier = 1;
			}

			world = this.world.setSize(8);
			for (int i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
				float space = spaces[i] * multiplier;
				position += space;
				float p = position;

				if (closed) {
					p %= pathLength;
					if (p < 0) p += pathLength;
					curve = 0;
				} else if (p < 0) {
					if (prevCurve != BEFORE) {
						prevCurve = BEFORE;
						path.computeWorldVertices(target, 2, 4, world, 0, 2);
					}
					addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					if (prevCurve != AFTER) {
						prevCurve = AFTER;
						path.computeWorldVertices(target, verticesLength - 6, 4, world, 0, 2);
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
						path.computeWorldVertices(target, verticesLength - 4, 4, world, 0, 2);
						path.computeWorldVertices(target, 0, 4, world, 4, 2);
					} else
						path.computeWorldVertices(target, curve * 6 + 2, 8, world, 0, 2);
				}
				addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o,
					tangents || (i > 0 && space < epsilon));
			}
			return out;
		}

		// World vertices.
		if (closed) {
			verticesLength += 2;
			world = this.world.setSize(verticesLength);
			path.computeWorldVertices(target, 2, verticesLength - 4, world, 0, 2);
			path.computeWorldVertices(target, 0, 2, world, verticesLength - 4, 2);
			world[verticesLength - 2] = world[0];
			world[verticesLength - 1] = world[1];
		} else {
			curveCount--;
			verticesLength -= 4;
			world = this.world.setSize(verticesLength);
			path.computeWorldVertices(target, 2, verticesLength, world, 0, 2);
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

		if (data.positionMode == PositionMode.percent) position *= pathLength;

		float multiplier;
		switch (data.spacingMode) {
		case percent:
			multiplier = pathLength;
			break;
		case proportional:
			multiplier = pathLength / spacesCount;
			break;
		default:
			multiplier = 1;
		}

		float[] segments = this.segments;
		float curveLength = 0;
		for (int i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
			float space = spaces[i] * multiplier;
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
			addCurvePosition(p * 0.1f, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space < epsilon));
		}
		return out;
	}

	private void addBeforePosition (float p, float[] temp, int i, float[] out, int o) {
		float x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = (float)Math.atan2(dy, dx);
		out[o] = x1 + p * (float)Math.cos(r);
		out[o + 1] = y1 + p * (float)Math.sin(r);
		out[o + 2] = r;
	}

	private void addAfterPosition (float p, float[] temp, int i, float[] out, int o) {
		float x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = (float)Math.atan2(dy, dx);
		out[o] = x1 + p * (float)Math.cos(r);
		out[o + 1] = y1 + p * (float)Math.sin(r);
		out[o + 2] = r;
	}

	private void addCurvePosition (float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2,
		float[] out, int o, boolean tangents) {
		if (p < epsilon || Float.isNaN(p)) {
			out[o] = x1;
			out[o + 1] = y1;
			out[o + 2] = (float)Math.atan2(cy1 - y1, cx1 - x1);
			return;
		}
		float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
		float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
		float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out[o] = x;
		out[o + 1] = y;
		if (tangents) {
			if (p < 0.001f)
				out[o + 2] = (float)Math.atan2(cy1 - y1, cx1 - x1);
			else
				out[o + 2] = (float)Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		}
	}

	/** The position along the path. */
	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	/** The spacing between bones. */
	public float getSpacing () {
		return spacing;
	}

	public void setSpacing (float spacing) {
		this.spacing = spacing;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotation. */
	public float getMixRotate () {
		return mixRotate;
	}

	public void setMixRotate (float mixRotate) {
		this.mixRotate = mixRotate;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation X. */
	public float getMixX () {
		return mixX;
	}

	public void setMixX (float mixX) {
		this.mixX = mixX;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y. */
	public float getMixY () {
		return mixY;
	}

	public void setMixY (float mixY) {
		this.mixY = mixY;
	}

	/** The bones that will be modified by this path constraint. */
	public Array<Bone> getBones () {
		return bones;
	}

	/** The slot whose path attachment will be used to constrained the bones. */
	public Slot getTarget () {
		return target;
	}

	public void setTarget (Slot target) {
		if (target == null) throw new IllegalArgumentException("target cannot be null.");
		this.target = target;
	}

	public boolean isActive () {
		return active;
	}

	/** The path constraint's setup pose data. */
	public PathConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
