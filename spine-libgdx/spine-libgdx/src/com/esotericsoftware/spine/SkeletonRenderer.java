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

package com.esotericsoftware.spine;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.Batch;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.glutils.ImmediateModeRenderer;
import com.badlogic.gdx.graphics.glutils.ImmediateModeRenderer20;
import com.badlogic.gdx.math.Matrix4;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Disposable;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.NumberUtils;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.SkeletonAttachment;
import com.esotericsoftware.spine.utils.Clipper;
import com.esotericsoftware.spine.utils.ConvexDecomposer;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

public class SkeletonRenderer implements Disposable {
	static private final short[] quadTriangles = {0, 1, 2, 2, 3, 0};

	private boolean softwareClipping = true;
	private boolean premultipliedAlpha;
	private final FloatArray vertices = new FloatArray(32);

	private ClippingAttachment clipAttachment;
	private Clipper clipper = new Clipper();
	private ConvexDecomposer decomposer = new ConvexDecomposer();
	private FloatArray clippingPolygon = new FloatArray(400);
	private Array<FloatArray> convexClippingPolygons;
	private FloatArray clipOutput = new FloatArray(400);
	private FloatArray clippedVertices = new FloatArray(400);
	private ShortArray clippedTriangles = new ShortArray(400);
	private final Matrix4 combinedMatrix = new Matrix4();
	private ImmediateModeRenderer renderer;

	public void draw (Batch batch, Skeleton skeleton) {
		boolean premultipliedAlpha = this.premultipliedAlpha;
		float[] vertices = this.vertices.items;
		Color skeletonColor = skeleton.color;
		float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;
		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment region = (RegionAttachment)attachment;
				region.computeWorldVertices(slot.getBone(), vertices, 0, 5);
				Color color = region.getColor(), slotColor = slot.getColor();
				float alpha = a * slotColor.a * color.a * 255;
				float c = NumberUtils.intToFloatColor(((int)alpha << 24) //
					| ((int)(b * slotColor.b * color.b * alpha) << 16) //
					| ((int)(g * slotColor.g * color.g * alpha) << 8) //
					| (int)(r * slotColor.r * color.r * alpha));
				float[] uvs = region.getUVs();
				for (int u = 0, v = 2; u < 8; u += 2, v += 5) {
					vertices[v] = c;
					vertices[v + 1] = uvs[u];
					vertices[v + 2] = uvs[u + 1];
				}

				BlendMode blendMode = slot.data.getBlendMode();
				batch.setBlendFunction(blendMode.getSource(premultipliedAlpha), blendMode.getDest());
				batch.draw(region.getRegion().getTexture(), vertices, 0, 20);

			} else if (attachment instanceof ClippingAttachment) {
				ClippingAttachment clip = (ClippingAttachment)attachment;
				if (!softwareClipping) batch.end();
				clipStart(batch.getProjectionMatrix(), batch.getTransformMatrix(), slot, clip);
				if (!softwareClipping) batch.begin();
				continue;

			} else if (attachment instanceof MeshAttachment) {
				throw new RuntimeException("SkeletonMeshRenderer is required to render meshes.");

			} else if (attachment instanceof SkeletonAttachment) {
				Skeleton attachmentSkeleton = ((SkeletonAttachment)attachment).getSkeleton();
				if (attachmentSkeleton != null) {
					Bone bone = slot.getBone();
					Bone rootBone = attachmentSkeleton.getRootBone();
					float oldScaleX = rootBone.getScaleX();
					float oldScaleY = rootBone.getScaleY();
					float oldRotation = rootBone.getRotation();
					attachmentSkeleton.setPosition(bone.getWorldX(), bone.getWorldY());
					// rootBone.setScaleX(1 + bone.getWorldScaleX() -
					// oldScaleX);
					// rootBone.setScaleY(1 + bone.getWorldScaleY() -
					// oldScaleY);
					// Set shear.
					rootBone.setRotation(oldRotation + bone.getWorldRotationX());
					attachmentSkeleton.updateWorldTransform();

					draw(batch, attachmentSkeleton);

					attachmentSkeleton.setX(0);
					attachmentSkeleton.setY(0);
					rootBone.setScaleX(oldScaleX);
					rootBone.setScaleY(oldScaleY);
					rootBone.setRotation(oldRotation);
				}
			}

			if (clipAttachment != null && i == clipAttachment.getEndSlot()) {
				batch.flush();
				clipEnd();
			}
		}
		if (clipAttachment != null) {
			if (!softwareClipping) batch.flush();
			clipEnd();
		}
	}

	@SuppressWarnings("null")
	public void draw (PolygonSpriteBatch batch, Skeleton skeleton) {
		boolean premultipliedAlpha = this.premultipliedAlpha;
		BlendMode blendMode = null;
		int verticesLength = 0;
		float[] vertices = null, uvs = null;
		short[] triangles = null;
		Texture texture = null;
		Color color = null, skeletonColor = skeleton.color;
		float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;
		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			final int vertexSize = (softwareClipping && clipAttachment != null) ? 2 : 5;
			Slot slot = drawOrder.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment region = (RegionAttachment)attachment;
				verticesLength = vertexSize << 2;
				vertices = this.vertices.items;
				region.computeWorldVertices(slot.getBone(), vertices, 0, vertexSize);
				triangles = quadTriangles;
				texture = region.getRegion().getTexture();
				uvs = region.getUVs();
				color = region.getColor();

			} else if (attachment instanceof MeshAttachment) {
				MeshAttachment mesh = (MeshAttachment)attachment;
				int count = mesh.getWorldVerticesLength();
				verticesLength = (count >> 1) * vertexSize;
				vertices = this.vertices.setSize(verticesLength);
				mesh.computeWorldVertices(slot, 0, count, vertices, 0, vertexSize);
				triangles = mesh.getTriangles();
				texture = mesh.getRegion().getTexture();
				uvs = mesh.getUVs();
				color = mesh.getColor();

			} else if (attachment instanceof ClippingAttachment) {
				ClippingAttachment clip = (ClippingAttachment)attachment;
				if (!softwareClipping) batch.end();
				clipStart(batch.getProjectionMatrix(), batch.getTransformMatrix(), slot, clip);
				if (!softwareClipping) batch.begin();
				continue;

			} else if (attachment instanceof SkeletonAttachment) {
				Skeleton attachmentSkeleton = ((SkeletonAttachment)attachment).getSkeleton();
				if (attachmentSkeleton != null) {
					Bone bone = slot.getBone();
					Bone rootBone = attachmentSkeleton.getRootBone();
					float oldScaleX = rootBone.getScaleX();
					float oldScaleY = rootBone.getScaleY();
					float oldRotation = rootBone.getRotation();
					attachmentSkeleton.setPosition(bone.getWorldX(), bone.getWorldY());
					// rootBone.setScaleX(1 + bone.getWorldScaleX() -
					// oldScaleX);
					// rootBone.setScaleY(1 + bone.getWorldScaleY() -
					// oldScaleY);
					// Also set shear.
					rootBone.setRotation(oldRotation + bone.getWorldRotationX());
					attachmentSkeleton.updateWorldTransform();

					draw(batch, attachmentSkeleton);

					attachmentSkeleton.setPosition(0, 0);
					rootBone.setScaleX(oldScaleX);
					rootBone.setScaleY(oldScaleY);
					rootBone.setRotation(oldRotation);
				}
			}

			if (texture != null) {
				Color slotColor = slot.getColor();
				float alpha = a * slotColor.a * color.a * 255;
				float c = NumberUtils.intToFloatColor(((int)alpha << 24) //
					| ((int)(b * slotColor.b * color.b * alpha) << 16) //
					| ((int)(g * slotColor.g * color.g * alpha) << 8) //
					| (int)(r * slotColor.r * color.r * alpha));

				BlendMode slotBlendMode = slot.data.getBlendMode();
				if (slotBlendMode != blendMode) {
					blendMode = slotBlendMode;
					batch.setBlendFunction(blendMode.getSource(premultipliedAlpha), blendMode.getDest());
				}
				if (softwareClipping) {
					if (clipAttachment != null) {
						clipSoftware(vertices, 0, verticesLength, triangles, 0, triangles.length, uvs, 0, c, false, clippedVertices,
							clippedTriangles);
						batch.draw(texture, clippedVertices.items, 0, clippedVertices.size, clippedTriangles.items, 0,
							clippedTriangles.size);
					} else {
						for (int v = 2, u = 0; v < verticesLength; v += 5, u += 2) {
							vertices[v] = c;
							vertices[v + 1] = uvs[u];
							vertices[v + 2] = uvs[u + 1];
						}
						batch.draw(texture, vertices, 0, verticesLength, triangles, 0, triangles.length);
					}
				} else {
					for (int v = 2, u = 0; v < verticesLength; v += 5, u += 2) {
						vertices[v] = c;
						vertices[v + 1] = uvs[u];
						vertices[v + 2] = uvs[u + 1];
					}
					batch.draw(texture, vertices, 0, verticesLength, triangles, 0, triangles.length);
				}
			}

			if (clipAttachment != null && i == clipAttachment.getEndSlot()) {
				if (!softwareClipping) batch.flush();
				clipEnd();
			}
		}
		if (clipAttachment != null) {
			if (!softwareClipping) batch.flush();
			clipEnd();
		}
	}

	@SuppressWarnings("null")
	public void draw (TwoColorPolygonBatch batch, Skeleton skeleton) {
		boolean premultipliedAlpha = this.premultipliedAlpha;
		BlendMode blendMode = null;
		int verticesLength = 0;
		float[] vertices = null, uvs = null;
		short[] triangles = null;
		Texture texture = null;
		Color color = null, skeletonColor = skeleton.color;
		float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;
		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			final int vertexSize = (softwareClipping && clipAttachment != null) ? 2 : 6;
			Slot slot = drawOrder.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment region = (RegionAttachment)attachment;
				verticesLength = vertexSize << 2;
				vertices = this.vertices.items;
				region.computeWorldVertices(slot.getBone(), vertices, 0, vertexSize);
				triangles = quadTriangles;
				texture = region.getRegion().getTexture();
				uvs = region.getUVs();
				color = region.getColor();

			} else if (attachment instanceof MeshAttachment) {
				MeshAttachment mesh = (MeshAttachment)attachment;
				int count = mesh.getWorldVerticesLength();
				verticesLength = count * (vertexSize >> 1);
				vertices = this.vertices.setSize(verticesLength);
				mesh.computeWorldVertices(slot, 0, count, vertices, 0, vertexSize);
				triangles = mesh.getTriangles();
				texture = mesh.getRegion().getTexture();
				uvs = mesh.getUVs();
				color = mesh.getColor();

			} else if (attachment instanceof ClippingAttachment) {
				ClippingAttachment clip = (ClippingAttachment)attachment;
				if (!softwareClipping) batch.end();
				clipStart(batch.getProjectionMatrix(), batch.getTransformMatrix(), slot, clip);
				if (!softwareClipping) batch.begin();
				continue;

			} else if (attachment instanceof SkeletonAttachment) {
				Skeleton attachmentSkeleton = ((SkeletonAttachment)attachment).getSkeleton();
				if (attachmentSkeleton != null) {
					Bone bone = slot.getBone();
					Bone rootBone = attachmentSkeleton.getRootBone();
					float oldScaleX = rootBone.getScaleX();
					float oldScaleY = rootBone.getScaleY();
					float oldRotation = rootBone.getRotation();
					attachmentSkeleton.setPosition(bone.getWorldX(), bone.getWorldY());
					// rootBone.setScaleX(1 + bone.getWorldScaleX() -
					// oldScaleX);
					// rootBone.setScaleY(1 + bone.getWorldScaleY() -
					// oldScaleY);
					// Also set shear.
					rootBone.setRotation(oldRotation + bone.getWorldRotationX());
					attachmentSkeleton.updateWorldTransform();

					draw(batch, attachmentSkeleton);

					attachmentSkeleton.setPosition(0, 0);
					rootBone.setScaleX(oldScaleX);
					rootBone.setScaleY(oldScaleY);
					rootBone.setRotation(oldRotation);
				}
			}

			if (texture != null) {
				Color lightColor = slot.getColor();
				float alpha = a * lightColor.a * color.a * 255;
				float light = NumberUtils.intToFloatColor(((int)alpha << 24) //
					| ((int)(b * lightColor.b * color.b * alpha) << 16) //
					| ((int)(g * lightColor.g * color.g * alpha) << 8) //
					| (int)(r * lightColor.r * color.r * alpha));
				Color darkColor = slot.getDarkColor();
				if (darkColor == null) darkColor = Color.BLACK;
				float dark = NumberUtils.intToFloatColor( //
					((int)(b * darkColor.b * color.b * 255) << 16) //
						| ((int)(g * darkColor.g * color.g * 255) << 8) //
						| (int)(r * darkColor.r * color.r * 255));

				BlendMode slotBlendMode = slot.data.getBlendMode();
				if (slotBlendMode != blendMode) {
					blendMode = slotBlendMode;
					batch.setBlendFunction(blendMode.getSource(premultipliedAlpha), blendMode.getDest());
				}

				if (softwareClipping) {
					if (clipAttachment != null) {
						clipSoftware(vertices, 0, verticesLength, triangles, 0, triangles.length, uvs, dark, light, true,
							clippedVertices, clippedTriangles);
						batch.draw(texture, clippedVertices.items, 0, clippedVertices.size, clippedTriangles.items, 0,
							clippedTriangles.size);
					} else {
						for (int v = 2, u = 0; v < verticesLength; v += 6, u += 2) {
							vertices[v] = light;
							vertices[v + 1] = dark;
							vertices[v + 2] = uvs[u];
							vertices[v + 3] = uvs[u + 1];
						}
						batch.draw(texture, vertices, 0, verticesLength, triangles, 0, triangles.length);
					}
				} else {
					for (int v = 2, u = 0; v < verticesLength; v += 6, u += 2) {
						vertices[v] = light;
						vertices[v + 1] = dark;
						vertices[v + 2] = uvs[u];
						vertices[v + 3] = uvs[u + 1];
					}
					batch.draw(texture, vertices, 0, verticesLength, triangles, 0, triangles.length);
				}
			}

			if (clipAttachment != null && i == clipAttachment.getEndSlot()) {
				if (!softwareClipping) batch.flush();
				clipEnd();
			}
		}
		if (clipAttachment != null) {
			if (!softwareClipping) batch.flush();
			clipEnd();
		}
	}

	private void clipStart (Matrix4 transformMatrix, Matrix4 projectionMatrix, Slot slot, ClippingAttachment clip) {
		if (clipAttachment != null) return;
		clipAttachment = clip;

		if (!softwareClipping) {
			int n = clip.getWorldVerticesLength();
			float[] vertices = this.vertices.setSize(n);
			clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);

			Gdx.gl.glClearStencil(0);
			Gdx.gl.glClear(GL20.GL_STENCIL_BUFFER_BIT);
			Gdx.gl.glEnable(GL20.GL_STENCIL_TEST);
			Gdx.gl.glStencilFunc(GL20.GL_NEVER, 0, 1);
			Gdx.gl.glStencilOp(GL20.GL_INVERT, GL20.GL_INVERT, GL20.GL_INVERT);
			Gdx.gl.glColorMask(false, false, false, false);

			if (renderer == null || renderer.getMaxVertices() < n)
				renderer = new ImmediateModeRenderer20(Math.max(100, n), false, false, 0);
			renderer.begin(combinedMatrix.set(projectionMatrix).mul(transformMatrix), GL20.GL_TRIANGLE_FAN);
			renderer.vertex(vertices[0], vertices[1], 0);
			for (int i = 2; i < n; i += 2)
				renderer.vertex(vertices[i], vertices[i + 1], 0);
			renderer.end();

			Gdx.gl.glColorMask(true, true, true, true);
			Gdx.gl.glStencilFunc(false ? GL20.GL_NOTEQUAL : GL20.GL_EQUAL, 1, 1);
			Gdx.gl.glStencilOp(GL20.GL_KEEP, GL20.GL_KEEP, GL20.GL_KEEP);
		} else {
			int n = clip.getWorldVerticesLength();
			float[] vertices = this.clippingPolygon.setSize(n);
			clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
			convexClippingPolygons = decomposer.decompose(clippingPolygon);
			for (FloatArray poly : convexClippingPolygons) {
				Clipper.makeClockwise(poly);
				poly.add(poly.items[0]);
				poly.add(poly.items[1]);
			}
		}
	}

	private void clipEnd () {
		clippedVertices.clear();
		clippedTriangles.clear();
		clippingPolygon.clear();
		convexClippingPolygons = null;
		clipAttachment = null;
		if (!softwareClipping) Gdx.gl.glDisable(GL20.GL_STENCIL_TEST);
	}

	private void clipSoftware (final float[] vertices, final int offset, final int verticesLength, final short[] triangles,
		final int triangleOffset, final int trianglesLength, final float uvs[], final float dark, final float light,
		final boolean twoColor, final FloatArray clippedVertices, final ShortArray clippedTriangles) {
		short idx = 0;
		clippedVertices.clear();
		clippedTriangles.clear();
		for (FloatArray convexClippingPolygon : convexClippingPolygons) {
			for (int i = 0; i < trianglesLength; i += 3) {
				int vertexOffset = triangles[i] << 1;
				float x1 = vertices[vertexOffset];
				float y1 = vertices[vertexOffset + 1];
				float u1 = uvs[vertexOffset];
				float v1 = uvs[vertexOffset + 1];

				vertexOffset = triangles[i + 1] << 1;
				float x2 = vertices[vertexOffset];
				float y2 = vertices[vertexOffset + 1];
				float u2 = uvs[vertexOffset];
				float v2 = uvs[vertexOffset + 1];

				vertexOffset = triangles[i + 2] << 1;
				float x3 = vertices[vertexOffset];
				float y3 = vertices[vertexOffset + 1];
				float u3 = uvs[vertexOffset];
				float v3 = uvs[vertexOffset + 1];

				boolean clipped = clipper.clip(x1, y1, x2, y2, x3, y3, convexClippingPolygon, clipOutput);
				if (clipped) {
					if (clipOutput.size == 0) continue;
					float d0 = y2 - y3;
					float d1 = x3 - x2;
					float d2 = x1 - x3;
					float d3 = y1 - y3;
					float d4 = y3 - y1;

					float denom = 1 / (d0 * d2 + d1 * d3);

					float[] clipVertices = clipOutput.items;
					int s = clippedVertices.size;
					clippedVertices.setSize(s + (clipOutput.size >> 1) * (twoColor ? 6 : 5));
					final float[] clippedVerticesArray = clippedVertices.items;

					for (int j = 0, n = clipOutput.size; j < n; j += 2) {
						float x = clipVertices[j];
						float y = clipVertices[j + 1];

						float c0 = x - x3;
						float c1 = y - y3;
						float a = (d0 * c0 + d1 * c1) * denom;
						float b = (d4 * c0 + d2 * c1) * denom;
						float c = 1.0f - a - b;

						float u = u1 * a + u2 * b + u3 * c;
						float v = v1 * a + v2 * b + v3 * c;
						clippedVerticesArray[s++] = x;
						clippedVerticesArray[s++] = y;
						clippedVerticesArray[s++] = light;
						if (twoColor) clippedVerticesArray[s++] = dark;
						clippedVerticesArray[s++] = u;
						clippedVerticesArray[s++] = v;
					}

					s = clippedTriangles.size;
					clippedTriangles.setSize(s + 3 * ((clipOutput.size >> 1) - 2));
					final short[] clippedTrianglesArray = clippedTriangles.items;

					for (int j = 1, n = (clipOutput.size >> 1) - 1; j < n; j++) {
						clippedTrianglesArray[s++] = idx;
						clippedTrianglesArray[s++] = (short)(idx + j);
						clippedTrianglesArray[s++] = (short)(idx + j + 1);
					}

					idx += clipOutput.size >> 1;
				} else {
					int s = clippedVertices.size;
					clippedVertices.setSize(s + 3 * (twoColor ? 6 : 5));
					final float[] clippedVerticesArray = clippedVertices.items;

					if (!twoColor) {
						clippedVerticesArray[s] = x1;
						clippedVerticesArray[s + 1] = y1;
						clippedVerticesArray[s + 2] = light;
						clippedVerticesArray[s + 3] = u1;
						clippedVerticesArray[s + 4] = v1;

						clippedVerticesArray[s + 5] = x2;
						clippedVerticesArray[s + 6] = y2;
						clippedVerticesArray[s + 7] = light;
						clippedVerticesArray[s + 8] = u2;
						clippedVerticesArray[s + 9] = v2;

						clippedVerticesArray[s + 10] = x3;
						clippedVerticesArray[s + 11] = y3;
						clippedVerticesArray[s + 12] = light;
						clippedVerticesArray[s + 13] = u3;
						clippedVerticesArray[s + 14] = v3;
					} else {
						clippedVerticesArray[s] = x1;
						clippedVerticesArray[s + 1] = y1;
						clippedVerticesArray[s + 2] = light;
						clippedVerticesArray[s + 3] = dark;
						clippedVerticesArray[s + 4] = u1;
						clippedVerticesArray[s + 5] = v1;

						clippedVerticesArray[s + 6] = x2;
						clippedVerticesArray[s + 7] = y2;
						clippedVerticesArray[s + 8] = light;
						clippedVerticesArray[s + 9] = dark;
						clippedVerticesArray[s + 10] = u2;
						clippedVerticesArray[s + 11] = v2;

						clippedVerticesArray[s + 12] = x3;
						clippedVerticesArray[s + 13] = y3;
						clippedVerticesArray[s + 14] = light;
						clippedVerticesArray[s + 15] = dark;
						clippedVerticesArray[s + 16] = u3;
						clippedVerticesArray[s + 17] = v3;
					}

					s = clippedTriangles.size;
					clippedTriangles.setSize(s + 3);
					final short[] clippedTrianglesArray = clippedTriangles.items;
					clippedTrianglesArray[s] = idx++;
					clippedTrianglesArray[s + 1] = idx++;
					clippedTrianglesArray[s + 2] = idx++;
				}
			}
		}
	}

	public void setPremultipliedAlpha (boolean premultipliedAlpha) {
		this.premultipliedAlpha = premultipliedAlpha;
	}

	public void dispose () {
		renderer.dispose();
	}

	public boolean getSoftwareClipping () {
		return softwareClipping;
	}

	public void setSoftwareClipping (boolean softwareClipping) {
		this.softwareClipping = softwareClipping;
	}
}
