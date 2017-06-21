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

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.Batch;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.NumberUtils;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.SkeletonAttachment;
import com.esotericsoftware.spine.utils.SkeletonClipping;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

public class SkeletonRenderer {
	static private final short[] quadTriangles = {0, 1, 2, 2, 3, 0};

	private boolean premultipliedAlpha;
	private final FloatArray vertices = new FloatArray(32);
	private final SkeletonClipping clipper = new SkeletonClipping();
	private VertexEffect vertexEffect;
	private final Vector2 temp = new Vector2();
	private final Vector2 temp2 = new Vector2();
	private final Color temp3 = new Color();
	private final Color temp4 = new Color();
	private final Color temp5 = new Color();
	private final Color temp6 = new Color();

	public void draw (Batch batch, Skeleton skeleton) {
		VertexEffect vertexEffect = this.vertexEffect;
		if (vertexEffect != null) vertexEffect.begin(skeleton);

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

				if (vertexEffect != null) applyVertexEffect(vertices, 20, 5, c, 0);

				BlendMode blendMode = slot.data.getBlendMode();
				batch.setBlendFunction(blendMode.getSource(premultipliedAlpha), blendMode.getDest());
				batch.draw(region.getRegion().getTexture(), vertices, 0, 20);

			} else if (attachment instanceof ClippingAttachment) {
				clipper.clipStart(slot, (ClippingAttachment)attachment);
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

			clipper.clipEnd(slot);
		}
		clipper.clipEnd();
		if (vertexEffect != null) vertexEffect.end();
	}

	@SuppressWarnings("null")
	public void draw (PolygonSpriteBatch batch, Skeleton skeleton) {
		Vector2 tempPos = this.temp;
		Vector2 tempUv = this.temp2;
		Color tempLight = this.temp3;
		Color tempDark = this.temp4;
		Color temp5 = this.temp5;
		Color temp6 = this.temp6;
		VertexEffect vertexEffect = this.vertexEffect;
		if (vertexEffect != null) vertexEffect.begin(skeleton);

		boolean premultipliedAlpha = this.premultipliedAlpha;
		BlendMode blendMode = null;
		int verticesLength = 0;
		float[] vertices = null, uvs = null;
		short[] triangles = null;
		Color color = null, skeletonColor = skeleton.color;
		float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;
		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			Texture texture = null;
			int vertexSize = clipper.isClipping() ? 2 : 5;
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
				clipper.clipStart(slot, clip);
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

				if (clipper.isClipping()) {
					clipper.clipTriangles(vertices, verticesLength, triangles, triangles.length, uvs, c, 0, false);
					FloatArray clippedVertices = clipper.getClippedVertices();
					ShortArray clippedTriangles = clipper.getClippedTriangles();
					if (vertexEffect != null) applyVertexEffect(clippedVertices.items, clippedVertices.size, 5, c, 0);
					batch.draw(texture, clippedVertices.items, 0, clippedVertices.size, clippedTriangles.items, 0,
						clippedTriangles.size);
				} else {
					if (vertexEffect != null) {
						temp5.set(NumberUtils.floatToIntColor(c));
						temp6.set(0);
						for (int v = 0, u = 0; v < verticesLength; v += 5, u += 2) {
							tempPos.x = vertices[v];
							tempPos.y = vertices[v + 1];
							tempLight.set(temp5);
							tempDark.set(temp6);
							tempUv.x = uvs[u];
							tempUv.y = uvs[u + 1];							
							vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
							vertices[v] = tempPos.x;
							vertices[v + 1] = tempPos.y;
							vertices[v + 2] = tempLight.toFloatBits();
							vertices[v + 3] = tempUv.x;
							vertices[v + 4] = tempUv.y;
						}
					} else {
						for (int v = 2, u = 0; v < verticesLength; v += 5, u += 2) {
							vertices[v] = c;
							vertices[v + 1] = uvs[u];
							vertices[v + 2] = uvs[u + 1];
						}
					}
					batch.draw(texture, vertices, 0, verticesLength, triangles, 0, triangles.length);
				}
			}

			clipper.clipEnd(slot);
		}
		clipper.clipEnd();
		if (vertexEffect != null) vertexEffect.end();
	}

	@SuppressWarnings("null")
	public void draw (TwoColorPolygonBatch batch, Skeleton skeleton) {
		Vector2 tempPos = this.temp;
		Vector2 tempUv = this.temp2;
		Color tempLight = this.temp3;
		Color tempDark = this.temp4;
		Color temp5 = this.temp5;
		Color temp6 = this.temp6;
		VertexEffect vertexEffect = this.vertexEffect;
		if (vertexEffect != null) vertexEffect.begin(skeleton);

		boolean premultipliedAlpha = this.premultipliedAlpha;
		BlendMode blendMode = null;
		int verticesLength = 0;
		float[] vertices = null, uvs = null;
		short[] triangles = null;
		Color color = null, skeletonColor = skeleton.color;
		float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;
		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			Texture texture = null;
			int vertexSize = clipper.isClipping() ? 2 : 6;
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
				clipper.clipStart(slot, clip);
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
					// rootBone.setScaleX(1 + bone.getWorldScaleX() - oldScaleX);
					// rootBone.setScaleY(1 + bone.getWorldScaleY() - oldScaleY);
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

				if (clipper.isClipping()) {
					clipper.clipTriangles(vertices, verticesLength, triangles, triangles.length, uvs, light, dark, true);
					FloatArray clippedVertices = clipper.getClippedVertices();
					ShortArray clippedTriangles = clipper.getClippedTriangles();
					if (vertexEffect != null) applyVertexEffect(clippedVertices.items, clippedVertices.size, 6, light, dark);
					batch.draw(texture, clippedVertices.items, 0, clippedVertices.size, clippedTriangles.items, 0,
						clippedTriangles.size);
				} else {
					if (vertexEffect != null) {
						temp5.set(NumberUtils.floatToIntColor(light));
						temp6.set(NumberUtils.floatToIntColor(dark));
						for (int v = 0, u = 0; v < verticesLength; v += 6, u += 2) {
							tempPos.x = vertices[v];
							tempPos.y = vertices[v + 1];
							tempLight.set(temp5);
							tempDark.set(temp6);
							tempUv.x = uvs[u];
							tempUv.y = uvs[u + 1];				
							vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
							vertices[v] = tempPos.x;
							vertices[v + 1] = tempPos.y;
							vertices[v + 2] = tempLight.toFloatBits();
							vertices[v + 3] = tempDark.toFloatBits();
							vertices[v + 4] = tempUv.x;
							vertices[v + 5] = tempUv.y;
						}
					} else {
						for (int v = 2, u = 0; v < verticesLength; v += 6, u += 2) {
							vertices[v] = light;
							vertices[v + 1] = dark;
							vertices[v + 2] = uvs[u];
							vertices[v + 3] = uvs[u + 1];
						}
					}
					batch.draw(texture, vertices, 0, verticesLength, triangles, 0, triangles.length);
				}
			}

			clipper.clipEnd(slot);
		}
		clipper.clipEnd();
		if (vertexEffect != null) vertexEffect.end();
	}

	private void applyVertexEffect (float[] vertices, int verticesLength, int stride, float light, float dark) {
		Vector2 tempPos = this.temp;
		Vector2 tempUv = this.temp2;
		Color tempLight = this.temp3;
		Color tempDark = this.temp4;
		Color temp5 = this.temp5;
		Color temp6 = this.temp6;
		VertexEffect vertexEffect = this.vertexEffect;
		temp5.set(NumberUtils.floatToIntColor(light));
		temp6.set(NumberUtils.floatToIntColor(dark));
		if (stride == 5) {
			for (int v = 0; v < verticesLength; v += stride) {
				tempPos.x = vertices[v];
				tempPos.y = vertices[v + 1];
				tempUv.x = vertices[v + 3];
				tempUv.y = vertices[v + 4];
				tempLight.set(temp5);
				tempDark.set(temp6);
				vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
				vertices[v] = tempPos.x;
				vertices[v + 1] = tempPos.y;
				vertices[v + 2] = tempLight.toFloatBits();
				vertices[v + 3] = tempUv.x;
				vertices[v + 4] = tempUv.y;
			}
		} else {
			for (int v = 0; v < verticesLength; v += stride) {
				tempPos.x = vertices[v];
				tempPos.y = vertices[v + 1];
				tempUv.x = vertices[v + 4];
				tempUv.y = vertices[v + 5];
				tempLight.set(temp5);
				tempDark.set(temp6);
				vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
				vertices[v] = tempPos.x;
				vertices[v + 1] = tempPos.y;
				vertices[v + 2] = tempLight.toFloatBits();
				vertices[v + 3] = tempDark.toFloatBits();
				vertices[v + 4] = tempUv.x;
				vertices[v + 5] = tempUv.y;
			}
		}
	}

	public boolean getPremultipliedAlpha () {
		return premultipliedAlpha;
	}

	public void setPremultipliedAlpha (boolean premultipliedAlpha) {
		this.premultipliedAlpha = premultipliedAlpha;
	}

	public VertexEffect getVertexEffect () {
		return vertexEffect;
	}

	public void setVertexEffect (VertexEffect vertexEffect) {
		this.vertexEffect = vertexEffect;
	}

	static public interface VertexEffect {
		public void begin (Skeleton skeleton);

		public void transform (Vector2 position, Vector2 uv, Color color, Color darkColor);

		public void end ();
	}
}
