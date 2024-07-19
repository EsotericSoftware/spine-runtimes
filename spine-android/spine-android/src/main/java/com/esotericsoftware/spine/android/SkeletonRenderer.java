/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.android;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.BlendMode;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.utils.SkeletonClipping;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;

/**
 * Is responsible to transform the {@link Skeleton} with its current pose to {@link SkeletonRenderer.RenderCommand} commands
 * and render them to a {@link Canvas}.
 */
public class SkeletonRenderer {

	/**
	 * Stores the vertices, indices, and atlas page index to be used for rendering one or more attachments
	 * of a {@link Skeleton} to a {@link Canvas}. See the implementation of {@link SkeletonRenderer#render(Skeleton)} and
	 * {@link SkeletonRenderer#renderToCanvas(Canvas, Array)} on how to use this data to render it to a {@link Canvas}.
	 */
	public static class RenderCommand implements Pool.Poolable {
		FloatArray vertices = new FloatArray(32);
		FloatArray uvs = new FloatArray(32);
		IntArray colors = new IntArray(32);
		ShortArray indices = new ShortArray(32);
		BlendMode blendMode;
		AndroidTexture texture;

		@Override
		public void reset () {
			vertices.setSize(0);
			uvs.setSize(0);
			colors.setSize(0);
			indices.setSize(0);
			blendMode = null;
			texture = null;
		}
	}

	static private final short[] quadTriangles = {0, 1, 2, 2, 3, 0};
	private final SkeletonClipping clipper = new SkeletonClipping();
	private final Pool<RenderCommand> commandPool = new Pool<RenderCommand>(10) {
		@Override
		protected RenderCommand newObject () {
			return new RenderCommand();
		}
	};
	private final Array<RenderCommand> commandList = new Array<RenderCommand>();

	/**
	 * Created the {@link RenderCommand} commands from the skeletons current pose.
	 */
	public Array<RenderCommand> render(Skeleton skeleton) {
		Color color = null, skeletonColor = skeleton.getColor();
		float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;

		commandPool.freeAll(commandList);
		commandList.clear();
		RenderCommand command = commandPool.obtain();
		commandList.add(command);
		int vertexStart = 0;

		Object[] drawOrder = skeleton.getDrawOrder().items;
		for (int i = 0, n = skeleton.getDrawOrder().size; i < n; i++) {
			Slot slot = (Slot)drawOrder[i];
			if (!slot.getBone().isActive()) {
				clipper.clipEnd(slot);
				continue;
			}

			int verticesLength = 0;
			int vertexSize = 2;
			float[] uvs = null;
			short[] indices = null;
			Attachment attachment = slot.getAttachment();
			if (attachment == null) {
				continue;
			}

			if (attachment instanceof RegionAttachment) {
				RegionAttachment region = (RegionAttachment)attachment;
				verticesLength = vertexSize << 2;
				if (region.getSequence() != null) region.getSequence().apply(slot, region);
				AndroidTexture texture = (AndroidTexture)region.getRegion().getTexture();
				BlendMode blendMode = slot.getData().getBlendMode();
				if (command.blendMode == null && command.texture == null) {
					command.blendMode = blendMode;
					command.texture = texture;
				}

				if (command.blendMode != blendMode || command.texture != texture || command.vertices.size + verticesLength > 64000) {
					command = commandPool.obtain();
					commandList.add(command);
					vertexStart = 0;
					command.blendMode = blendMode;
					command.texture = texture;
				}

				command.vertices.setSize(command.vertices.size + verticesLength);
				region.computeWorldVertices(slot, command.vertices.items, vertexStart, vertexSize);
				uvs = region.getUVs();
				indices = quadTriangles;
				color = region.getColor();
			} else if (attachment instanceof MeshAttachment) {
				MeshAttachment mesh = (MeshAttachment)attachment;
				verticesLength = mesh.getWorldVerticesLength();
				if (mesh.getSequence() != null) mesh.getSequence().apply(slot, mesh);
				AndroidTexture texture = (AndroidTexture)mesh.getRegion().getTexture();
				BlendMode blendMode = slot.getData().getBlendMode();

				if (command.blendMode == null && command.texture == null) {
					command.blendMode = blendMode;
					command.texture = texture;
				}

				if (command.blendMode != blendMode || command.texture != texture || command.vertices.size + verticesLength > 64000) {
					command = commandPool.obtain();
					commandList.add(command);
					vertexStart = 0;
					command.blendMode = blendMode;
					command.texture = texture;
				}

				command.vertices.setSize(command.vertices.size + verticesLength);
				mesh.computeWorldVertices(slot, 0, verticesLength, command.vertices.items, vertexStart, vertexSize);
				uvs = mesh.getUVs();
				indices = mesh.getTriangles();
				color = mesh.getColor();
			} else if (attachment instanceof ClippingAttachment) {
				ClippingAttachment clip = (ClippingAttachment)attachment;
				clipper.clipStart(slot, clip);
				continue;
			} else {
				continue;
			}

			Color slotColor = slot.getColor();
			int c = (int)(a * slotColor.a * color.a * 255) << 24 //
				| (int)(r * slotColor.r * color.r * 255) << 16 //
				| (int)(g * slotColor.g * color.g * 255) << 8 //
				| (int)(b * slotColor.b * color.b * 255);

			if (clipper.isClipping()) {
				// FIXME
				throw new RuntimeException("Not implemented, need to split positions, uvs, colors");
				// clipper.clipTriangles(vertices, verticesLength, triangles, triangles.length, uvs, c, 0, false);
				// FloatArray clippedVertices = clipper.getClippedVertices();
				// ShortArray clippedTriangles = clipper.getClippedTriangles();
				// batch.draw(texture, clippedVertices.items, 0, clippedVertices.size, clippedTriangles.items, 0,
				// clippedTriangles.size);
			} else {
				command.uvs.addAll(uvs);
				float[] uvsArray = command.uvs.items;
				for (int ii = vertexStart, w = command.texture.getWidth(), h = command.texture.getHeight(),
					nn = vertexStart + verticesLength; ii < nn; ii += 2) {
					uvsArray[ii] = uvsArray[ii] * w;
					uvsArray[ii + 1] = uvsArray[ii + 1] * h;
				}

				command.colors.setSize(command.colors.size + (verticesLength >> 1));
				int[] colorsArray = command.colors.items;
				for (int ii = vertexStart >> 1, nn = (vertexStart >> 1) + (verticesLength >> 1); ii < nn; ii++) {
					colorsArray[ii] = c;
				}

				int indicesStart = command.indices.size;
				command.indices.addAll(indices);
				int firstIndex = vertexStart >> 1;
				short[] indicesArray = command.indices.items;
				for (int ii = indicesStart, nn = indicesStart + indices.length; ii < nn; ii++) {
					indicesArray[ii] += firstIndex;
				}
			}
			// FIXME wrt clipping
			vertexStart += verticesLength;
			clipper.clipEnd(slot);
		}
		clipper.clipEnd();

		if (commandList.size == 1 && commandList.get(0).vertices.size == 0) {
			commandPool.freeAll(commandList);
			commandList.clear();
		}

		return commandList;
	}

	/**
	 * Renders the {@link RenderCommand} commands created from the skeleton current pose to the given {@link Canvas}.
	 * Does not perform any scaling or fitting.
	 */
	public void renderToCanvas(Canvas canvas, Array<RenderCommand> commands) {
		for (int i = 0; i < commands.size; i++) {
			RenderCommand command = commands.get(i);

			canvas.drawVertices(Canvas.VertexMode.TRIANGLES, command.vertices.size, command.vertices.items, 0, command.uvs.items, 0,
				command.colors.items, 0, command.indices.items, 0, command.indices.size, command.texture.getPaint(command.blendMode));
		}
	}

	/**
	 * Renders the {@link Skeleton} with its current pose to a {@link Bitmap}.
	 *
	 * @param width    The width of the bitmap in pixels.
	 * @param height   The height of the bitmap in pixels.
	 * @param bgColor  The background color.
	 * @param skeleton The skeleton to render.
	 */
	public Bitmap renderToBitmap(float width, float height, int bgColor, Skeleton skeleton) {
		Vector2 offset = new Vector2(0, 0);
		Vector2 size = new Vector2(0, 0);
		FloatArray floatArray = new FloatArray();

		skeleton.getBounds(offset, size, floatArray);

		RectF bounds = new RectF(offset.x, offset.y, offset.x + size.x, offset.y + size.y);
		float scale = (1 / (bounds.width() > bounds.height() ? bounds.width() / width : bounds.height() / height));

		Bitmap bitmap = Bitmap.createBitmap((int) width, (int) height, Bitmap.Config.ARGB_8888);
		Canvas canvas = new Canvas(bitmap);

		Paint paint = new Paint();
		paint.setColor(bgColor);
		paint.setStyle(Paint.Style.FILL);

		// Draw background
		canvas.drawRect(0, 0, width, height, paint);

		// Transform canvas
		canvas.translate(width / 2, height / 2);
		canvas.scale(scale, -scale);
		canvas.translate(-(bounds.left + bounds.width() / 2), -(bounds.top + bounds.height() / 2));

		renderToCanvas(canvas, render(skeleton));

		return bitmap;
	}
}
