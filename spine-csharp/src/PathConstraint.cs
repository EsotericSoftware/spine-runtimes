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

using System;

namespace Spine {
	public class PathConstraint : IUpdatable {
		private const int NONE = -1, BEFORE = -2, AFTER = -3;

		internal PathConstraintData data;
		internal ExposedList<Bone> bones;
		internal Slot target;
		internal float position, spacing, rotateMix, translateMix;

		internal ExposedList<float> spaces = new ExposedList<float>(), positions = new ExposedList<float>();
		internal ExposedList<float> world = new ExposedList<float>(), curves = new ExposedList<float>(), lengths = new ExposedList<float>();
		internal float[] segments = new float[10];

		public float Position { get { return position; } set { position = value; } }
		public float Spacing { get { return spacing; } set { spacing = value; } }
		public float RotateMix { get { return rotateMix; } set { rotateMix = value; } }
		public float TranslateMix { get { return translateMix; } set { translateMix = value; } }
		public ExposedList<Bone> Bones { get { return bones; } }
		public Slot Target { get { return target; } set { target = value; } }
		public PathConstraintData Data { get { return data; } }

		public PathConstraint (PathConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			bones = new ExposedList<Bone>(data.Bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.FindBone(boneData.name));
			target = skeleton.FindSlot(data.target.name);
			position = data.position;
			spacing = data.spacing;
			rotateMix = data.rotateMix;
			translateMix = data.translateMix;
		}

		public void Apply () {
			Update();
		}
			
		public void Update () {
			PathAttachment attachment = target.Attachment as PathAttachment;
			if (attachment == null) return;

			float rotateMix = this.rotateMix, translateMix = this.translateMix;
			bool translate = translateMix > 0, rotate = rotateMix > 0;
			if (!translate && !rotate) return;

			PathConstraintData data = this.data;
			SpacingMode spacingMode = data.spacingMode;
			bool lengthSpacing = spacingMode == SpacingMode.Length;
			RotateMode rotateMode = data.rotateMode;
			bool tangents = rotateMode == RotateMode.Tangent, scale = rotateMode == RotateMode.ChainScale;
			int boneCount = this.bones.Count, spacesCount = tangents ? boneCount : boneCount + 1;
			Bone[] bones = this.bones.Items;
			ExposedList<float> spaces = this.spaces.Resize(spacesCount), lengths = null;
			float spacing = this.spacing;
			if (scale || lengthSpacing) {
				if (scale) lengths = this.lengths.Resize(boneCount);
				for (int i = 0, n = spacesCount - 1; i < n;) {
					Bone bone = bones[i];
					float length = bone.data.length, x = length * bone.a, y = length * bone.c;
					length = (float)Math.Sqrt(x * x + y * y);
					if (scale) lengths.Items[i] = length;
					spaces.Items[++i] = lengthSpacing ? Math.Max(0, length + spacing) : spacing;
				}
			} else {
				for (int i = 1; i < spacesCount; i++)
					spaces.Items[i] = spacing;
			}

			float[] positions = ComputeWorldPositions(attachment, spacesCount, tangents,
				data.positionMode == PositionMode.Percent, spacingMode == SpacingMode.Percent);
			Skeleton skeleton = target.Skeleton;
			float skeletonX = skeleton.x, skeletonY = skeleton.y;
			float boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
			bool tip = rotateMode == RotateMode.Chain && offsetRotation == 0;
			for (int i = 0, p = 3; i < boneCount; i++, p += 3) {
				Bone bone = (Bone)bones[i];
				bone.worldX += (boneX - skeletonX - bone.worldX) * translateMix;
				bone.worldY += (boneY - skeletonY - bone.worldY) * translateMix;
				float x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
				if (scale) {
					float length = lengths.Items[i];
					if (length != 0) {
						float s = ((float)Math.Sqrt(dx * dx + dy * dy) / length - 1) * rotateMix + 1;
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
					else if (spaces.Items[i + 1] == 0)
						r = positions[p + 2];
					else
						r = MathUtils.Atan2(dy, dx);
					r -= MathUtils.Atan2(c, a) - offsetRotation * MathUtils.degRad;
					if (tip) {
						cos = MathUtils.Cos(r);
						sin = MathUtils.Sin(r);
						float length = bone.data.length;
						boneX += (length * (cos * a - sin * c) - dx) * rotateMix;
						boneY += (length * (sin * a + cos * c) - dy) * rotateMix;
					}
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					r *= rotateMix;
					cos = MathUtils.Cos(r);
					sin = MathUtils.Sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
			}
		}

		float[] ComputeWorldPositions (PathAttachment path, int spacesCount, bool tangents, bool percentPosition,
			bool percentSpacing) {

			Slot target = this.target;
			float position = this.position;
			float[] spaces = this.spaces.Items, output = this.positions.Resize(spacesCount * 3 + 2).Items, world;
			bool closed = path.Closed;
			int verticesLength = path.WorldVerticesLength, curveCount = verticesLength / 6, prevCurve = NONE;

			float pathLength;
			if (!path.ConstantSpeed) {
				float[] lengths = path.Lengths;
				curveCount -= closed ? 1 : 2;
				pathLength = lengths[curveCount];
				if (percentPosition) position *= pathLength;
				if (percentSpacing) {
					for (int i = 0; i < spacesCount; i++)
						spaces[i] *= pathLength;
				}
				world = this.world.Resize(8).Items;
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
							path.ComputeWorldVertices(target, 2, 4, world, 0);
						}
						AddBeforePosition(p, world, 0, output, o);
						continue;
					} else if (p > pathLength) {
						if (prevCurve != AFTER) {
							prevCurve = AFTER;
							path.ComputeWorldVertices(target, verticesLength - 6, 4, world, 0);
						}
						AddAfterPosition(p - pathLength, world, 0, output, o);
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
							path.ComputeWorldVertices(target, verticesLength - 4, 4, world, 0);
							path.ComputeWorldVertices(target, 0, 4, world, 4);
						} else
							path.ComputeWorldVertices(target, curve * 6 + 2, 8, world, 0);
					}
					AddCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], output, o,
						tangents || (i > 0 && space == 0));
				}
				return output;
			}

			// World vertices.
			if (closed) {
				verticesLength += 2;
				world = this.world.Resize(verticesLength).Items;
				path.ComputeWorldVertices(target, 2, verticesLength - 4, world, 0);
				path.ComputeWorldVertices(target, 0, 2, world, verticesLength - 4);
				world[verticesLength - 2] = world[0];
				world[verticesLength - 1] = world[1];
			} else {
				curveCount--;
				verticesLength -= 4;
				world = this.world.Resize(verticesLength).Items;
				path.ComputeWorldVertices(target, 2, verticesLength, world, 0);
			}

			// Curve lengths.
			float[] curves = this.curves.Resize(curveCount).Items;
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
					AddBeforePosition(p, world, 0, output, o);
					continue;
				} else if (p > pathLength) {
					AddAfterPosition(p - pathLength, world, verticesLength - 4, output, o);
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
				AddCurvePosition(p * 0.1f, x1, y1, cx1, cy1, cx2, cy2, x2, y2, output, o, tangents || (i > 0 && space == 0));
			}
			return output;
		}

		private void AddBeforePosition (float p, float[] temp, int i, float[] output, int o) {
			float x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = MathUtils.Atan2(dy, dx);
			output[o] = x1 + p * MathUtils.Cos(r);
			output[o + 1] = y1 + p * MathUtils.Sin(r);
			output[o + 2] = r;
		}

		private void AddAfterPosition (float p, float[] temp, int i, float[] output, int o) {
			float x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = MathUtils.Atan2(dy, dx);
			output[o] = x1 + p * MathUtils.Cos(r);
			output[o + 1] = y1 + p * MathUtils.Sin(r);
			output[o + 2] = r;
		}

		private void AddCurvePosition (float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2,
			float[] output, int o, bool tangents) {
			if (p == 0) p = 0.0001f;
			float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
			float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
			float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
			output[o] = x;
			output[o + 1] = y;
			if (tangents) output[o + 2] = (float)Math.Atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		}
	}
}
