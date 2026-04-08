/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System;

namespace Spine {

	/// <summary>
	/// <para>
	/// Adjusts the rotation, translation, and scale of the constrained bones so they follow a <see cref="PathAttachment"/>.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-path-constraints">Path constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class PathConstraint : Constraint<PathConstraint, PathConstraintData, PathConstraintPose> {
		const int NONE = -1, BEFORE = -2, AFTER = -3;
		const float Epsilon = 0.00001f;

		internal readonly ExposedList<BonePose> bones;
		internal Slot slot;

		internal readonly ExposedList<float> spaces = new ExposedList<float>(), positions = new ExposedList<float>();
		internal readonly ExposedList<float> world = new ExposedList<float>(), curves = new ExposedList<float>(), lengths = new ExposedList<float>();
		internal readonly float[] segments = new float[10];

		public PathConstraint (PathConstraintData data, Skeleton skeleton)
			: base(data, new PathConstraintPose(), new PathConstraintPose()) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			bones = new ExposedList<BonePose>(data.Bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.bones.Items[boneData.index].constrainedPose);

			slot = skeleton.slots.Items[data.slot.index];
		}

		override public IConstraint Copy (Skeleton skeleton) {
			var copy = new PathConstraint(data, skeleton);
			copy.pose.Set(pose);
			return copy;
		}

		public static void ArraysFill (float[] a, int fromIndex, int toIndex, float val) {
			for (int i = fromIndex; i < toIndex; i++)
				a[i] = val;
		}

		override public void Update (Skeleton skeleton, Physics physics) {
			PathAttachment pathAttachment = slot.appliedPose.Attachment as PathAttachment;
			if (pathAttachment == null) return;

			PathConstraintPose p = appliedPose;
			float mixRotate = p.mixRotate, mixX = p.mixX, mixY = p.mixY;
			if (mixRotate == 0 && mixX == 0 && mixY == 0) return;

			PathConstraintData data = this.data;
			bool tangents = data.rotateMode == RotateMode.Tangent, scale = data.rotateMode == RotateMode.ChainScale;
			int boneCount = this.bones.Count, spacesCount = tangents ? boneCount : boneCount + 1;
			BonePose[] bonesItems = this.bones.Items;
			float[] spaces = this.spaces.EnsureSize(spacesCount).Items, lengths = scale ? this.lengths.EnsureSize(boneCount).Items : null;
			float spacing = p.spacing;
			switch (data.spacingMode) {
			case SpacingMode.Percent:
				if (scale) {
					for (int i = 0, n = spacesCount - 1; i < n; i++) {
						BonePose bone = bonesItems[i];
						float setupLength = bone.bone.data.length;
						float x = setupLength * bone.a, y = setupLength * bone.c;
						lengths[i] = (float)Math.Sqrt(x * x + y * y);
					}
				}
				ArraysFill(spaces, 1, spacesCount, spacing);
				break;
			case SpacingMode.Proportional: {
				float sum = 0;
				for (int i = 0, n = spacesCount - 1; i < n;) {
					BonePose bone = bonesItems[i];
					float setupLength = bone.bone.data.length;
					if (setupLength < Epsilon) {
						if (scale) lengths[i] = 0;
						spaces[++i] = spacing;
					} else {
						float x = setupLength * bone.a, y = setupLength * bone.c;
						float length = (float)Math.Sqrt(x * x + y * y);
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
			}
			default: {
				bool lengthSpacing = data.spacingMode == SpacingMode.Length;
				for (int i = 0, n = spacesCount - 1; i < n;) {
					BonePose bone = bonesItems[i];
					float setupLength = bone.bone.data.length;
					if (setupLength < PathConstraint.Epsilon) {
						if (scale) lengths[i] = 0;
						spaces[++i] = spacing;
					} else {
						float x = setupLength * bone.a, y = setupLength * bone.c;
						float length = (float)Math.Sqrt(x * x + y * y);
						if (scale) lengths[i] = length;
						spaces[++i] = (lengthSpacing ? Math.Max(0, setupLength + spacing) : spacing) * length / setupLength;
					}
				}
				break;
			}
			}

			float[] positions = ComputeWorldPositions(skeleton, pathAttachment, spacesCount, tangents);
			float boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
			bool tip;
			if (offsetRotation == 0) {
				tip = data.rotateMode == RotateMode.Chain;
			} else {
				tip = false;
				BonePose bone = slot.bone.appliedPose;
				offsetRotation *= bone.a * bone.d - bone.b * bone.c > 0 ? MathUtils.DegRad : -MathUtils.DegRad;
			}
			for (int i = 0, ip = 3, u = skeleton.update; i < boneCount; i++, ip += 3) {
				BonePose bone = bonesItems[i];
				bone.worldX += (boneX - bone.worldX) * mixX;
				bone.worldY += (boneY - bone.worldY) * mixY;
				float x = positions[ip], y = positions[ip + 1], dx = x - boneX, dy = y - boneY;
				if (scale) {
					float length = lengths[i];
					if (length >= Epsilon) {
						float s = ((float)Math.Sqrt(dx * dx + dy * dy) / length - 1) * mixRotate + 1;
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
					else if (spaces[i + 1] < Epsilon)
						r = positions[ip + 2];
					else
						r = MathUtils.Atan2(dy, dx);
					r -= MathUtils.Atan2(c, a);
					if (tip) {
						cos = MathUtils.Cos(r);
						sin = MathUtils.Sin(r);
						float length = bone.bone.data.length;
						boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
						boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
					} else
						r += offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					r *= mixRotate;
					cos = MathUtils.Cos(r);
					sin = MathUtils.Sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
				bone.ModifyWorld(u);
			}
		}

		float[] ComputeWorldPositions (Skeleton skeleton, PathAttachment path, int spacesCount, bool tangents) {
			Slot slot = this.slot;
			float position = appliedPose.position;
			float[] spaces = this.spaces.Items, output = this.positions.EnsureSize(spacesCount * 3 + 2).Items, world;
			bool closed = path.Closed;
			int verticesLength = path.WorldVerticesLength, curveCount = verticesLength / 6, prevCurve = NONE;

			float pathLength, multiplier;
			if (!path.ConstantSpeed) {
				float[] lengths = path.Lengths;
				curveCount -= closed ? 1 : 2;
				pathLength = lengths[curveCount];

				if (data.positionMode == PositionMode.Percent) position *= pathLength;

				switch (data.spacingMode) {
				case SpacingMode.Percent:
					multiplier = pathLength;
					break;
				case SpacingMode.Proportional:
					multiplier = pathLength / spacesCount;
					break;
				default:
					multiplier = 1;
					break;
				}

				world = this.world.EnsureSize(8).Items;
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
							path.ComputeWorldVertices(skeleton, slot, 2, 4, world, 0, 2);
						}
						AddBeforePosition(p, world, 0, output, o);
						continue;
					} else if (p > pathLength) {
						if (prevCurve != AFTER) {
							prevCurve = AFTER;
							path.ComputeWorldVertices(skeleton, slot, verticesLength - 6, 4, world, 0, 2);
						}
						AddAfterPosition(p - pathLength, world, 0, output, o);
						continue;
					}

					// Determine curve containing position.
					for (; ; curve++) {
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
							path.ComputeWorldVertices(skeleton, slot, verticesLength - 4, 4, world, 0, 2);
							path.ComputeWorldVertices(skeleton, slot, 0, 4, world, 4, 2);
						} else
							path.ComputeWorldVertices(skeleton, slot, curve * 6 + 2, 8, world, 0, 2);
					}
					AddCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], output, o,
						tangents || (i > 0 && space < Epsilon));
				}
				return output;
			}

			// World vertices.
			if (closed) {
				verticesLength += 2;
				world = this.world.EnsureSize(verticesLength).Items;
				path.ComputeWorldVertices(skeleton, slot, 2, verticesLength - 4, world, 0, 2);
				path.ComputeWorldVertices(skeleton, slot, 0, 2, world, verticesLength - 4, 2);
				world[verticesLength - 2] = world[0];
				world[verticesLength - 1] = world[1];
			} else {
				curveCount--;
				verticesLength -= 4;
				world = this.world.EnsureSize(verticesLength).Items;
				path.ComputeWorldVertices(skeleton, slot, 2, verticesLength, world, 0, 2);
			}

			// Curve lengths.
			float[] curves = this.curves.EnsureSize(curveCount).Items;
			pathLength = 0;
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
				pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
				dfx += ddfx;
				dfy += ddfy;
				pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
				dfx += ddfx + dddfx;
				dfy += ddfy + dddfy;
				pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
				curves[i] = pathLength;
				x1 = x2;
				y1 = y2;
			}

			if (data.positionMode == PositionMode.Percent) position *= pathLength;

			switch (data.spacingMode) {
			case SpacingMode.Percent:
				multiplier = pathLength;
				break;
			case SpacingMode.Proportional:
				multiplier = pathLength / spacesCount;
				break;
			default:
				multiplier = 1;
				break;
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
					segment = 0;
				} else if (p < 0) {
					AddBeforePosition(p, world, 0, output, o);
					continue;
				} else if (p > pathLength) {
					AddAfterPosition(p - pathLength, world, verticesLength - 4, output, o);
					continue;
				}

				// Determine curve containing position.
				for (; ; curve++) {
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
					curveLength = (float)Math.Sqrt(dfx * dfx + dfy * dfy);
					segments[0] = curveLength;
					for (ii = 1; ii < 8; ii++) {
						dfx += ddfx;
						dfy += ddfy;
						ddfx += dddfx;
						ddfy += dddfy;
						curveLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
						segments[ii] = curveLength;
					}
					dfx += ddfx;
					dfy += ddfy;
					curveLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
					segments[8] = curveLength;
					dfx += ddfx + dddfx;
					dfy += ddfy + dddfy;
					curveLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
					segments[9] = curveLength;
					segment = 0;
				}

				// Weight by segment length.
				p *= curveLength;
				for (; ; segment++) {
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
				AddCurvePosition(p * 0.1f, x1, y1, cx1, cy1, cx2, cy2, x2, y2, output, o, tangents || (i > 0 && space < PathConstraint.Epsilon));
			}
			return output;
		}

		static void AddBeforePosition (float p, float[] temp, int i, float[] output, int o) {
			float x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = MathUtils.Atan2(dy, dx);
			output[o] = x1 + p * MathUtils.Cos(r);
			output[o + 1] = y1 + p * MathUtils.Sin(r);
			output[o + 2] = r;
		}

		static void AddAfterPosition (float p, float[] temp, int i, float[] output, int o) {
			float x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = MathUtils.Atan2(dy, dx);
			output[o] = x1 + p * MathUtils.Cos(r);
			output[o + 1] = y1 + p * MathUtils.Sin(r);
			output[o + 2] = r;
		}

		static void AddCurvePosition (float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2,
			float[] output, int o, bool tangents) {
			if (p < PathConstraint.Epsilon || float.IsNaN(p)) {
				output[o] = x1;
				output[o + 1] = y1;
				output[o + 2] = (float)Math.Atan2(cy1 - y1, cx1 - x1);
				return;
			}
			float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
			float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
			float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
			output[o] = x;
			output[o + 1] = y;
			if (tangents) {
				if (p < 0.001f)
					output[o + 2] = (float)Math.Atan2(cy1 - y1, cx1 - x1);
				else
					output[o + 2] = (float)Math.Atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
			}
		}

		override public void Sort (Skeleton skeleton) {
			int slotIndex = slot.Data.index;
			Bone slotBone = slot.bone;
			if (skeleton.skin != null) SortPathSlot(skeleton, skeleton.skin, slotIndex, slotBone);
			if (skeleton.data.defaultSkin != null && skeleton.data.defaultSkin != skeleton.skin)
				SortPathSlot(skeleton, skeleton.data.defaultSkin, slotIndex, slotBone);
			SortPath(skeleton, slot.pose.attachment, slotBone);
			BonePose[] bones = this.bones.Items;
			int boneCount = this.bones.Count;
			for (int i = 0; i < boneCount; i++) {
				Bone bone = bones[i].bone;
				skeleton.SortBone(bone);
				skeleton.Constrained(bone);
			}
			skeleton.updateCache.Add(this);
			for (int i = 0; i < boneCount; i++)
				skeleton.SortReset(bones[i].bone.children);
			for (int i = 0; i < boneCount; i++)
				bones[i].bone.sorted = true;
		}

		private void SortPathSlot (Skeleton skeleton, Skin skin, int slotIndex, Bone slotBone) {
			foreach (Skin.SkinEntry entry in skin.Attachments)
				if (entry.SlotIndex == slotIndex) SortPath(skeleton, entry.Attachment, slotBone);
		}

		private void SortPath (Skeleton skeleton, Attachment attachment, Bone slotBone) {
			if (!(attachment is PathAttachment)) return;
			int[] pathBones = ((PathAttachment)attachment).bones;
			if (pathBones == null)
				skeleton.SortBone(slotBone);
			else {
				Bone[] bones = skeleton.bones.Items;
				for (int i = 0, n = pathBones.Length; i < n;) {
					int nn = pathBones[i++];
					nn += i;
					while (i < nn)
						skeleton.SortBone(bones[pathBones[i++]]);
				}
			}
		}

		override public bool IsSourceActive { get { return slot.bone.active; } }

		/// <summary>The bones that will be modified by this path constraint.</summary>
		public ExposedList<BonePose> Bones { get { return bones; } }
		/// <summary>The slot whose path attachment will be used to constrained the bones.</summary>
		public Slot Slot { get { return slot; } set { slot = value; } }
	}
}
