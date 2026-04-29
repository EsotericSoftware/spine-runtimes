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
	public class SkeletonClipping {
		internal Triangulator triangulator;
		internal readonly ExposedList<float> clippingPolygon = new ExposedList<float>(0);
		internal readonly ExposedList<ExposedList<float>> clippingPolygons = new ExposedList<ExposedList<float>>(1);
		internal readonly ExposedList<float> clipOutput = new ExposedList<float>(0);
		internal readonly ExposedList<float> clippedVertices = new ExposedList<float>(0);
		internal readonly ExposedList<float> clippedUVs = new ExposedList<float>(0);
		internal readonly ExposedList<int> clippedTriangles = new ExposedList<int>(0);
		internal readonly ExposedList<float> inverseVertices = new ExposedList<float>(0);
		internal readonly ExposedList<float> scratch = new ExposedList<float>(0);

		internal ClippingAttachment clipAttachment;
		private bool inverse;

		public ExposedList<float> ClippedVertices { get { return clippedVertices; } }
		public ExposedList<int> ClippedTriangles { get { return clippedTriangles; } }
		public ExposedList<float> ClippedUVs { get { return clippedUVs; } }

		public bool IsClipping { get { return clipAttachment != null; } }

		public int ClipStart (Skeleton skeleton, Slot slot, ClippingAttachment clip) {
			if (clipAttachment != null) return 0;
			clipAttachment = clip;

			int n = clip.worldVerticesLength;
			inverse = clip.Inverse;

			clip.ComputeWorldVertices(skeleton, slot, 0, n, clippingPolygon.EnsureSize(n).Items, 0, 2);
			bool convex = MakeClockwise(clippingPolygon);
			if (convex || inverse || clip.Convex) {
				if (!convex) MakeConvex(clippingPolygon);
				clippingPolygon.Add(clippingPolygon.Items[0]);
				clippingPolygon.Add(clippingPolygon.Items[1]);
				clippingPolygons.Add(clippingPolygon);
			} else {
				if (triangulator == null) triangulator = new Triangulator();
				clippingPolygons.AddRange(triangulator.Decompose(clippingPolygon, triangulator.Triangulate(clippingPolygon)));
			}

			return clippingPolygons.Count;
		}

		public void ClipEnd (Slot slot) {
			if (clipAttachment != null && clipAttachment.endSlot == slot.data) ClipEnd();
		}

		public void ClipEnd () {
			if (clipAttachment == null) return;
			clipAttachment = null;
			clippingPolygons.Clear();
		}

		public bool ClipTriangles (float[] vertices, int[] triangles, int trianglesLength) {
			ExposedList<float> clippedVertices = this.clippedVertices;
			clippedVertices.Count = 0;
			ExposedList<int> clippedTriangles = this.clippedTriangles;
			clippedTriangles.Count = 0;

			int index = 0;
			if (inverse) {
				ExposedList<float> polygon = clippingPolygons.Items[0];
				for (int i = 0; i < trianglesLength; i += 3) {
					int t = triangles[i] << 1;
					float x1 = vertices[t], y1 = vertices[t + 1];
					t = triangles[i + 1] << 1;
					float x2 = vertices[t], y2 = vertices[t + 1];
					t = triangles[i + 2] << 1;
					float x3 = vertices[t], y3 = vertices[t + 1];
					ClipInverse(x1, y1, x2, y2, x3, y3, polygon);

					float[] iv = inverseVertices.Items;
					for (int offset = 0, nn = inverseVertices.Count; offset < nn;) {
						int polygonSize = (int)iv[offset++];
						int vertexCount = polygonSize >> 1, s = clippedVertices.Count;

						float[] cv = clippedVertices.EnsureSize(s + polygonSize).Items;
						Array.Copy(iv, offset, cv, s, polygonSize);

						s = clippedTriangles.Count;
						int[] ct = clippedTriangles.EnsureSize(s + 3 * (vertexCount - 2)).Items;
						for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
							ct[s] = index;
							ct[s + 1] = index + ii;
							ct[s + 2] = index + ii + 1;
						}
						index += vertexCount;
						offset += polygonSize;
					}
				}
				return true;
			}

			ExposedList<float> clipOutput = this.clipOutput;
			ExposedList<float>[] polygons = clippingPolygons.Items;
			int polygonsCount = clippingPolygons.Count;
			float[] clipOutputItems = null;
			for (int i = 0; i < trianglesLength; i += 3) {
				int t = triangles[i] << 1;
				float x1 = vertices[t], y1 = vertices[t + 1];
				t = triangles[i + 1] << 1;
				float x2 = vertices[t], y2 = vertices[t + 1];
				t = triangles[i + 2] << 1;
				float x3 = vertices[t], y3 = vertices[t + 1];
				for (int p = 0; p < polygonsCount; p++) {
					int s = clippedVertices.Count;
					if (Clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
						clipOutputItems = clipOutput.Items;
						int clipOutputLength = clipOutput.Count;
						if (clipOutputLength == 0) continue;
						int clipOutputCount = clipOutputLength >> 1;

						float[] cv = clippedVertices.EnsureSize(s + clipOutputLength).Items;
						Array.Copy(clipOutputItems, 0, cv, s, clipOutputLength);

						s = clippedTriangles.Count;
						int[] ct = clippedTriangles.EnsureSize(s + 3 * (clipOutputCount - 2)).Items;
						for (int ii = 1, nn = clipOutputCount - 1; ii < nn; ii++, s += 3) {
							ct[s] = index;
							ct[s + 1] = index + ii;
							ct[s + 2] = index + ii + 1;
						}
						index += clipOutputCount;
					} else {
						float[] cv = clippedVertices.EnsureSize(s + 3 * 2).Items;
						cv[s] = x1;
						cv[s + 1] = y1;
						cv[s + 2] = x2;
						cv[s + 3] = y2;
						cv[s + 4] = x3;
						cv[s + 5] = y3;

						s = clippedTriangles.Count;
						int[] ct = clippedTriangles.EnsureSize(s + 3).Items;
						ct[s] = index;
						ct[s + 1] = index + 1;
						ct[s + 2] = index + 2;
						index += 3;
						break;
					}
				}
			}
			return clipOutputItems != null;
		}

		// Note: corresponds to clipTrianglesUnpacked() of libgdx reference implementation.
		// spine-csharp has no need to support interleaved vertex data and custom stride.
		public bool ClipTriangles (float[] vertices, int[] triangles, int trianglesLength, float[] uvs) {
			ExposedList<float> clippedVertices = this.clippedVertices;
			clippedVertices.Count = 0;
			ExposedList<int> clippedTriangles = this.clippedTriangles;
			clippedTriangles.Count = 0;
			ExposedList<float> clippedUvs = this.clippedUVs;
			clippedUvs.Count = 0;

			int index = 0;
			if (inverse) {
				ExposedList<float> polygon = clippingPolygons.Items[0];
				for (int i = 0; i < trianglesLength; i += 3) {
					int t0 = triangles[i] << 1, t1 = triangles[i + 1] << 1, t2 = triangles[i + 2] << 1;
					float x1 = vertices[t0], y1 = vertices[t0 + 1];
					float x2 = vertices[t1], y2 = vertices[t1 + 1];
					float x3 = vertices[t2], y3 = vertices[t2 + 1];
					ClipInverse(x1, y1, x2, y2, x3, y3, polygon);
					int nn = inverseVertices.Count;
					if (nn == 0) continue;

					float u1 = uvs[t0], v1 = uvs[t0 + 1];
					float u2 = uvs[t1], v2 = uvs[t1 + 1];
					float u3 = uvs[t2], v3 = uvs[t2 + 1];
					float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
					float[] iv = inverseVertices.Items;
					for (int offset = 0; offset < nn;) {
						int polygonSize = (int)iv[offset++];
						int vertexCount = polygonSize >> 1;

						int s = clippedVertices.Count;
						float[] cv = clippedVertices.EnsureSize(s + polygonSize).Items;
						float[] cu = clippedUvs.EnsureSize(s + polygonSize).Items;
						for (int ii = 0; ii < polygonSize; ii += 2, s += 2) {
							float x = iv[offset + ii], y = iv[offset + ii + 1];
							cv[s] = x;
							cv[s + 1] = y;
							float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
							cu[s] = u1 * a + u2 * b + u3 * c;
							cu[s + 1] = v1 * a + v2 * b + v3 * c;
						}

						s = clippedTriangles.Count;
						int[] ct = clippedTriangles.EnsureSize(s + 3 * (vertexCount - 2)).Items;
						for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
							ct[s] = index;
							ct[s + 1] = index + ii;
							ct[s + 2] = index + ii + 1;
						}
						index += vertexCount;
						offset += polygonSize;
					}
				}
				return true;
			}

			ExposedList<float> clipOutput = this.clipOutput;
			ExposedList<float>[] polygons = clippingPolygons.Items;
			int polygonsCount = clippingPolygons.Count;
			float[] clipOutputItems = null;
			for (int i = 0; i < trianglesLength; i += 3) {
				int t = triangles[i] << 1;
				float x1 = vertices[t], y1 = vertices[t + 1];
				float u1 = uvs[t], v1 = uvs[t + 1];
				t = triangles[i + 1] << 1;
				float x2 = vertices[t], y2 = vertices[t + 1];
				float u2 = uvs[t], v2 = uvs[t + 1];
				t = triangles[i + 2] << 1;
				float x3 = vertices[t], y3 = vertices[t + 1];
				float u3 = uvs[t], v3 = uvs[t + 1];
				float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
				for (int p = 0; p < polygonsCount; p++) {
					int s = clippedVertices.Count;
					if (Clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
						clipOutputItems = clipOutput.Items;
						int clipOutputLength = clipOutput.Count;
						if (clipOutputLength == 0) continue;
						int clipOutputCount = clipOutputLength >> 1;

						float[] cv = clippedVertices.EnsureSize(s + clipOutputCount * 2).Items,
							cu = clippedUvs.EnsureSize(s + clipOutputCount * 2).Items;
						for (int ii = 0; ii < clipOutputLength; ii += 2, s += 2) {
							float x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
							cv[s] = x;
							cv[s + 1] = y;

							float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
							cu[s] = u1 * a + u2 * b + u3 * c;
							cu[s + 1] = v1 * a + v2 * b + v3 * c;
						}

						s = clippedTriangles.Count;
						int[] ct = clippedTriangles.EnsureSize(s + 3 * (clipOutputCount - 2)).Items;
						clipOutputCount--;
						for (int ii = 1; ii < clipOutputCount; ii++, s += 3) {
							ct[s] = index;
							ct[s + 1] = index + ii;
							ct[s + 2] = index + ii + 1;
						}
						index += clipOutputCount + 1;
					} else {
						float[] cv = clippedVertices.EnsureSize(s + 3 * 2).Items;
						cv[s] = x1;
						cv[s + 1] = y1;
						cv[s + 2] = x2;
						cv[s + 3] = y2;
						cv[s + 4] = x3;
						cv[s + 5] = y3;

						float[] cu = clippedUvs.EnsureSize(s + 3 * 2).Items;
						cu[s] = u1;
						cu[s + 1] = v1;
						cu[s + 2] = u2;
						cu[s + 3] = v2;
						cu[s + 4] = u3;
						cu[s + 5] = v3;

						s = clippedTriangles.Count;
						int[] ct = clippedTriangles.EnsureSize(s + 3).Items;
						ct[s] = index;
						ct[s + 1] = index + 1;
						ct[s + 2] = index + 2;
						index += 3;
						break;
					}
				}
			}
			return clipOutputItems != null;
		}

		internal bool Clip (float x1, float y1, float x2, float y2, float x3, float y3, ExposedList<float> polygon) {
			ExposedList<float> originalOutput = clipOutput;
			bool clipped = false;

			ExposedList<float> input, output; // Avoid copy at the end.
			if (polygon.Count % 4 >= 2) {
				input = clipOutput;
				output = scratch;
			} else {
				input = scratch;
				output = clipOutput;
			}

			float[] v = polygon.Items, iv = input.EnsureSize(8).Items;
			iv[0] = x1;
			iv[1] = y1;
			iv[2] = x2;
			iv[3] = y2;
			iv[4] = x3;
			iv[5] = y3;
			iv[6] = x1;
			iv[7] = y1;
			output.Count = 0;

			int last = polygon.Count - 4;
			for (int i = 0; ; i += 2) {
				float edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
				int outputStart = output.Count;
				iv = input.Items;
				for (int ii = 0, nn = input.Count - 2; ii < nn;) {
					x1 = iv[ii];
					y1 = iv[ii + 1];
					ii += 2;
					x2 = iv[ii];
					y2 = iv[ii + 1];
					bool s2 = ey * (edgeX - x2) > ex * (edgeY - y2);
					float s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
					if (s1 > 0) {
						if (s2) {// v1 in, v2 in
							output.Add(x2);
							output.Add(y2);
						} else { // v1 in, v2 out
							float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
							if (t >= 0 && t <= 1) {
								output.Add(x1 + ix * t);
								output.Add(y1 + iy * t);
								clipped = true;
							} else {
								output.Add(x2);
								output.Add(y2);
							}
						}
					} else if (s2) { // v1 out, v2 in
						float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							output.Add(x1 + ix * t);
							output.Add(y1 + iy * t);
							output.Add(x2);
							output.Add(y2);
							clipped = true;
						} else {
							output.Add(x2);
							output.Add(y2);
						}
					} else // v1 out, v2 out
						clipped = true;
				}
				if (outputStart == output.Count) { // All outside.
					originalOutput.Count = 0;
					return true;
				}

				output.Add(output.Items[0]);
				output.Add(output.Items[1]);

				if (i == last) break;
				ExposedList<float> temp = output;
				output = input;
				output.Count = 0;
				input = temp;
			}

			if (originalOutput != output) {
				// Note: reference implementation:
				// originalOutput.Count = 0;
				// originalOutput.AddAll(output.Items, 0, output.Count - 2);
				int count = output.Count - 2;
				originalOutput.EnsureSize(count);
				Array.Copy(output.Items, 0, originalOutput.Items, 0, count);
			} else
				originalOutput.EnsureSize(originalOutput.Count - 2);

			return clipped;
		}

		internal void ClipInverse (float x1, float y1, float x2, float y2, float x3, float y3, ExposedList<float> polygon) {
			inverseVertices.Count = 0;
			inverseVertices.EnsureCapacity(polygon.Count * 3);
			int vLast = polygon.Count - 4;

			ExposedList<float> input, output; // Avoid copy at the end.
			if (polygon.Count % 4 >= 2) {
				input = clipOutput;
				output = scratch;
			} else {
				input = scratch;
				output = clipOutput;
			}

			float[] v = polygon.Items, iv = input.EnsureSize(8).Items;
			iv[0] = x1;
			iv[1] = y1;
			iv[2] = x2;
			iv[3] = y2;
			iv[4] = x3;
			iv[5] = y3;
			iv[6] = x1;
			iv[7] = y1;
			output.Count = 0;

			for (int i = 0; ; i += 2) {
				float edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
				int outputStart = output.Count, fragmentStart = inverseVertices.Count++;
				iv = input.Items;
				for (int ii = 0, nn = input.Count - 2; ii < nn;) {
					x1 = iv[ii];
					y1 = iv[ii + 1];
					ii += 2;
					x2 = iv[ii];
					y2 = iv[ii + 1];
					bool s2 = ey * (edgeX - x2) > ex * (edgeY - y2);
					float s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
					if (s1 > 0) {
						if (s2) { // v1 in, v2 in
							output.Add(x2);
							output.Add(y2);
						} else {
							// v1 in, v2 out
							float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
							if (t >= 0 && t <= 1) {
								float cx = x1 + ix * t, cy = y1 + iy * t;
								output.Add(cx);
								output.Add(cy);
								inverseVertices.Add(cx);
								inverseVertices.Add(cy);
								inverseVertices.Add(x2);
								inverseVertices.Add(y2);
							} else {
								output.Add(x2);
								output.Add(y2);
							}
						}
					} else if (s2) { // v1 out, v2 in
						float dx = x2 - x1, dy = y2 - y1, t = s1 / (dx * ey - dy * ex);
						if (t >= 0 && t <= 1) {
							float cx = x1 + dx * t, cy = y1 + dy * t;
							inverseVertices.Add(cx);
							inverseVertices.Add(cy);
							output.Add(cx);
							output.Add(cy);
							output.Add(x2);
							output.Add(y2);
						} else {
							output.Add(x2);
							output.Add(y2);
						}
					} else { // v1 out, v2 out
						inverseVertices.Add(x2);
						inverseVertices.Add(y2);
					}
				}

				int fragmentSize = inverseVertices.Count - fragmentStart - 1;
				if (fragmentSize >= 6)
					inverseVertices.Items[fragmentStart] = fragmentSize;
				else
					inverseVertices.Count = fragmentStart; // Degenerate.

				if (outputStart == output.Count) break; // All outside.

				output.Add(output.Items[0]);
				output.Add(output.Items[1]);

				if (i == vLast) break;
				ExposedList<float> temp = output;
				output = input;
				output.Count = 0;
				input = temp;
			}
		}

		private bool MakeClockwise (ExposedList<float> polygon) {
			float[] v = polygon.Items;
			int n = polygon.Count;
			bool noCW = true, noCCW = true;
			float area = 0, prevX = v[n - 2], prevY = v[n - 1], currX = v[0], currY = v[1];

			for (int i = 2; i < n; i += 2) {
				float nextX = v[i], nextY = v[i + 1];
				area += currX * nextY - nextX * currY;
				float cross1 = (currX - prevX) * (nextY - currY) - (currY - prevY) * (nextX - currX);
				noCCW &= cross1 <= 0;
				noCW &= cross1 >= 0;
				prevX = currX;
				prevY = currY;
				currX = nextX;
				currY = nextY;
			}
			area += currX * v[1] - v[0] * currY;
			float cross = (currX - prevX) * (v[1] - currY) - (currY - prevY) * (v[0] - currX);
			noCCW &= cross <= 0;
			noCW &= cross >= 0;
			if (area >= 0) {
				for (int i = 0, lastX = n - 2, half = n >> 1; i < half; i += 2) {
					float x = v[i], y = v[i + 1];
					int other = lastX - i;
					v[i] = v[other];
					v[i + 1] = v[other + 1];
					v[other] = x;
					v[other + 1] = y;
				}
				return noCW;
			}
			return noCCW;
		}

		private void MakeConvex (ExposedList<float> polygon) {
			int n = polygon.Count;
			polygon.EnsureCapacity(n);
			float[] v = polygon.Items, sorted = clipOutput.EnsureSize(n).Items;
			sorted[0] = v[0];
			sorted[1] = v[1];
			for (int i = 2; i < n; i += 2) {
				float x = v[i], y = v[i + 1];
				int p = i - 2;
				for (; p >= 0 && (sorted[p] > x || (sorted[p] == x && sorted[p + 1] > y)); p -= 2) {
					sorted[p + 2] = sorted[p];
					sorted[p + 3] = sorted[p + 1];
				}
				sorted[p + 2] = x;
				sorted[p + 3] = y;
			}
			v[0] = sorted[0];
			v[1] = sorted[1];
			v[2] = sorted[2];
			v[3] = sorted[3];
			int s = 4;
			for (int i = 4; i < n; i += 2, s += 2) {
				float x = sorted[i], y = sorted[i + 1];
				while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
					s -= 2;
					if (s == 2) break;
				}
				v[s] = x;
				v[s + 1] = y;
			}
			v[s] = sorted[n - 4];
			v[s + 1] = sorted[n - 3];
			int t = s;
			s += 2;
			for (int i = n - 6; i >= 0; i -= 2, s += 2) {
				float x = sorted[i], y = sorted[i + 1];
				while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
					s -= 2;
					if (s == t) break;
				}
				v[s] = x;
				v[s + 1] = y;
			}
			polygon.Count = s - 2;
		}
	}
}
