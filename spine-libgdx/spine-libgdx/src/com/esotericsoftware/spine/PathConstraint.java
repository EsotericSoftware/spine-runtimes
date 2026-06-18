/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import java.util.Arrays;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;

import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.Skin.SkinEntry;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.PathAttachment;

/** Adjusts the rotation, translation, and scale of the constrained bones so they follow a {@link PathAttachment}.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-path-constraints">Path constraints</a> in the Spine User Guide. */
public class PathConstraint extends Constraint<PathConstraint, PathConstraintData, PathConstraintPose> {
	static final int NONE = -1, BEFORE = -2, AFTER = -3;

	final Array<BonePose> bones;
	Slot slot;

	private final FloatArray spaces = new FloatArray(), positions = new FloatArray();
	private final FloatArray world = new FloatArray(), curves = new FloatArray(), lengths = new FloatArray();
	private final float[] segments = new float[10];

	public PathConstraint (PathConstraintData data, Skeleton skeleton) {
		super(data, new PathConstraintPose(), new PathConstraintPose());
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		bones = new Array(true, data.bones.size, BonePose[]::new);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.items[boneData.index].constrainedPose);

		slot = skeleton.slots.items[data.slot.index];
	}

	public PathConstraint copy (Skeleton skeleton) {
		var copy = new PathConstraint(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	/** Applies the constraint to the constrained bones. */
	public void update (Skeleton skeleton, Physics physics) {
		if (!(slot.appliedPose.attachment instanceof PathAttachment pathAttachment)) return;

		PathConstraintPose p = appliedPose;
		float mixRotate = p.mixRotate, mixX = p.mixX, mixY = p.mixY;
		if (mixRotate == 0 & mixX == 0 & mixY == 0) return;

		PathConstraintData data = this.data;
		boolean tangents = data.rotateMode == RotateMode.tangent, scale = data.rotateMode == RotateMode.chainScale;
		int boneCount = this.bones.size, spacesCount = tangents ? boneCount : boneCount + 1;
		BonePose[] bones = this.bones.items;
		float[] spaces = this.spaces.setSize(spacesCount), lengths = scale ? this.lengths.setSize(boneCount) : null;
		float spacing = p.spacing;

		switch (data.spacingMode) {
		case percent -> {
			if (scale) {
				for (int i = 0, n = spacesCount - 1; i < n; i++) {
					BonePose bone = bones[i];
					float setupLength = bone.bone.data.length;
					float x = setupLength * bone.a, y = setupLength * bone.c;
					lengths[i] = (float)Math.sqrt(x * x + y * y);
				}
			}
			Arrays.fill(spaces, 1, spacesCount, spacing);
		}
		case proportional -> {
			float sum = 0;
			for (int i = 0, n = spacesCount - 1; i < n;) {
				BonePose bone = bones[i];
				float setupLength = bone.bone.data.length;
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
		}
		default -> {
			boolean lengthSpacing = data.spacingMode == SpacingMode.length;
			for (int i = 0, n = spacesCount - 1; i < n;) {
				BonePose bone = bones[i];
				float setupLength = bone.bone.data.length;
				if (setupLength < epsilon) {
					if (scale) lengths[i] = 0;
					spaces[++i] = spacing;
				} else {
					float x = setupLength * bone.a, y = setupLength * bone.c;
					float length = (float)Math.sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = (lengthSpacing ? Math.max(0, setupLength + spacing) : spacing) * length / setupLength;
				}
			}
		}
		}

		float[] positions = computeWorldPositions(skeleton, pathAttachment, spacesCount, tangents);
		float boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
		boolean tip;
		if (offsetRotation == 0)
			tip = data.rotateMode == RotateMode.chain;
		else {
			tip = false;
			BonePose bone = slot.bone.appliedPose;
			offsetRotation *= bone.a * bone.d - bone.b * bone.c > 0 ? degRad : -degRad;
		}
		for (int i = 0, ip = 3, u = skeleton.update; i < boneCount; i++, ip += 3) {
			BonePose bone = bones[i];
			bone.worldX += (boneX - bone.worldX) * mixX;
			bone.worldY += (boneY - bone.worldY) * mixY;
			float x = positions[ip], y = positions[ip + 1], dx = x - boneX, dy = y - boneY;
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
					r = positions[ip - 1];
				else if (spaces[i + 1] < epsilon)
					r = positions[ip + 2];
				else
					r = atan2(dy, dx);
				r -= atan2(c, a);
				if (tip) {
					cos = cos(r);
					sin = sin(r);
					float length = bone.bone.data.length;
					boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
					boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
				} else
					r += offsetRotation;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) //
					r += PI2;
				r *= mixRotate;
				cos = cos(r);
				sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			bone.modifyWorld(u);
		}
	}

	float[] computeWorldPositions (Skeleton skeleton, PathAttachment path, int spacesCount, boolean tangents) {
		Slot slot = this.slot;
		float position = appliedPose.position;
		float[] spaces = this.spaces.items, out = this.positions.setSize(spacesCount * 3 + 2), world;
		boolean closed = path.getClosed();
		int verticesLength = path.getWorldVerticesLength(), curveCount = verticesLength / 6, prevCurve = NONE;

		if (!path.getConstantSpeed()) {
			float[] lengths = path.getLengths();
			curveCount -= closed ? 1 : 2;
			float pathLength = lengths[curveCount];

			if (data.positionMode == PositionMode.percent) position *= pathLength;

			float multiplier = switch (data.spacingMode) {
			case percent -> pathLength;
			case proportional -> pathLength / spacesCount;
			default -> 1;
			};

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
						path.computeWorldVertices(skeleton, slot, 2, 4, world, 0, 2);
					}
					addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					if (prevCurve != AFTER) {
						prevCurve = AFTER;
						path.computeWorldVertices(skeleton, slot, verticesLength - 6, 4, world, 0, 2);
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
						path.computeWorldVertices(skeleton, slot, verticesLength - 4, 4, world, 0, 2);
						path.computeWorldVertices(skeleton, slot, 0, 4, world, 4, 2);
					} else
						path.computeWorldVertices(skeleton, slot, curve * 6 + 2, 8, world, 0, 2);
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
			path.computeWorldVertices(skeleton, slot, 2, verticesLength - 4, world, 0, 2);
			path.computeWorldVertices(skeleton, slot, 0, 2, world, verticesLength - 4, 2);
			world[verticesLength - 2] = world[0];
			world[verticesLength - 1] = world[1];
		} else {
			curveCount--;
			verticesLength -= 4;
			world = this.world.setSize(verticesLength);
			path.computeWorldVertices(skeleton, slot, 2, verticesLength, world, 0, 2);
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

		float multiplier = switch (data.spacingMode) {
		case percent -> pathLength;
		case proportional -> pathLength / spacesCount;
		default -> 1;
		};

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
				segment = 0;
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
		if (p < epsilon || Float.isNaN(p)) {
			out[o] = x1;
			out[o + 1] = y1;
			out[o + 2] = atan2(cy1 - y1, cx1 - x1);
			return;
		}
		float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
		float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
		float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out[o] = x;
		out[o + 1] = y;
		if (tangents) {
			if (p < 0.001f)
				out[o + 2] = atan2(cy1 - y1, cx1 - x1);
			else
				out[o + 2] = atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		}
	}

	void sort (Skeleton skeleton) {
		int slotIndex = slot.getData().index;
		Bone slotBone = slot.bone;
		if (skeleton.skin != null) sortPathSlot(skeleton, skeleton.skin, slotIndex, slotBone);
		if (skeleton.data.defaultSkin != null && skeleton.data.defaultSkin != skeleton.skin)
			sortPathSlot(skeleton, skeleton.data.defaultSkin, slotIndex, slotBone);
		sortPath(skeleton, slot.pose.attachment, slotBone);
		BonePose[] bones = this.bones.items;
		int boneCount = this.bones.size;
		for (int i = 0; i < boneCount; i++) {
			Bone bone = bones[i].bone;
			skeleton.sortBone(bone);
			skeleton.constrained(bone);
		}
		skeleton.updateCache.add(this);
		for (int i = 0; i < boneCount; i++)
			skeleton.sortReset(bones[i].bone.children);
		for (int i = 0; i < boneCount; i++)
			bones[i].bone.sorted = true;
	}

	private void sortPathSlot (Skeleton skeleton, Skin skin, int slotIndex, Bone slotBone) {
		Object[] entries = skin.attachments.orderedItems().items;
		for (int i = 0, n = skin.attachments.size; i < n; i++) {
			var entry = (SkinEntry)entries[i];
			if (entry.slotIndex == slotIndex) sortPath(skeleton, entry.attachment, slotBone);
		}
	}

	private void sortPath (Skeleton skeleton, Attachment attachment, Bone slotBone) {
		if (!(attachment instanceof PathAttachment pathAttachment)) return;
		int[] pathBones = pathAttachment.getBones();
		if (pathBones == null)
			skeleton.sortBone(slotBone);
		else {
			Bone[] bones = skeleton.bones.items;
			for (int i = 0, n = pathBones.length; i < n;) {
				int nn = pathBones[i++];
				nn += i;
				while (i < nn)
					skeleton.sortBone(bones[pathBones[i++]]);
			}
		}
	}

	boolean isSourceActive () {
		return slot.bone.active;
	}

	/** The bones that will be modified by this path constraint. */
	public Array<BonePose> getBones () {
		return bones;
	}

	/** The slot whose path attachment will be used to constrained the bones. */
	public Slot getSlot () {
		return slot;
	}

	public void setSlot (Slot slot) {
		if (slot == null) throw new IllegalArgumentException("slot cannot be null.");
		this.slot = slot;
	}
}
