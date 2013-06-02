
package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import static com.badlogic.gdx.graphics.g2d.SpriteBatch.*;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.utils.Array;

public class SkeletonRendererDebug {
	static private final Color slotLineColor = new Color(0, 0, 1, 0.5f);

	private ShapeRenderer renderer;

	public SkeletonRendererDebug () {
		renderer = new ShapeRenderer();
	}

	public void draw (Skeleton skeleton) {
		float skeletonX = skeleton.getX();
		float skeletonY = skeleton.getY();

		Gdx.gl.glEnable(GL10.GL_BLEND);
		renderer.begin(ShapeType.Line);

		renderer.setColor(Color.RED);
		Array<Bone> bones = skeleton.getBones();
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			if (bone.parent == null) continue;
			float x = skeletonX + bone.data.length * bone.m00 + bone.worldX;
			float y = skeletonY + bone.data.length * bone.m10 + bone.worldY;
			renderer.line(skeletonX + bone.worldX, skeletonY + bone.worldY, x, y);
		}

		renderer.setColor(slotLineColor);
		Array<Slot> slots = skeleton.getSlots();
		for (int i = 0, n = slots.size; i < n; i++) {
			Slot slot = slots.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				regionAttachment.updateVertices(slot);
				float[] vertices = regionAttachment.getVertices();
				renderer.line(vertices[X1], vertices[Y1], vertices[X2], vertices[Y2]);
				renderer.line(vertices[X2], vertices[Y2], vertices[X3], vertices[Y3]);
				renderer.line(vertices[X3], vertices[Y3], vertices[X4], vertices[Y4]);
				renderer.line(vertices[X4], vertices[Y4], vertices[X1], vertices[Y1]);
			}
		}

		renderer.end();

		renderer.setColor(Color.GREEN);
		renderer.begin(ShapeType.Filled);
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			renderer.setColor(Color.GREEN);
			renderer.circle(skeletonX + bone.worldX, skeletonY + bone.worldY, 3);
		}
		renderer.end();
	}

	public ShapeRenderer getShapeRenderer () {
		return renderer;
	}
}
