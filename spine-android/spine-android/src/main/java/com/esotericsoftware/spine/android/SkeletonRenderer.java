
package com.esotericsoftware.spine.android;

import com.badlogic.gdx.graphics.Color;
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

import android.graphics.Canvas;

public class SkeletonRenderer {
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

	public Array<RenderCommand> render (Skeleton skeleton) {
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

		return commandList;
	}

	public void render (Canvas canvas, Skeleton skeleton, float x, float y) {
		canvas.save();
		canvas.translate(x, y);
		Array<RenderCommand> commands = render(skeleton);
		for (int i = 0; i < commands.size; i++) {
			RenderCommand command = commands.get(i);
			canvas.drawVertices(Canvas.VertexMode.TRIANGLES, command.vertices.size, command.vertices.items, 0, command.uvs.items, 0,
				command.colors.items, 0, command.indices.items, 0, command.indices.size, command.texture.getPaint(command.blendMode));
		}
		canvas.restore();
	}
}
