
package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import static com.badlogic.gdx.graphics.g2d.SpriteBatch.*;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.utils.Array;

public class SkeletonRendererDebug {
	static private final Color boneLineColor = Color.RED;
	static private final Color boneOriginColor = Color.GREEN;
	static private final Color regionAttachmentLineColor = new Color(0, 0, 1, 0.5f);
	static private final Color boundingBoxColor = new Color(0, 1, 0, 0.8f);
	static private final Color aabbColor = new Color(0, 1, 0, 0.5f);

	private final ShapeRenderer renderer;
	private boolean drawBones = true, drawRegionAttachments = true, drawBoundingBoxes = true;
	private final SkeletonBounds bounds = new SkeletonBounds();

	public SkeletonRendererDebug () {
		renderer = new ShapeRenderer();
	}

	public void draw (Skeleton skeleton) {
		float skeletonX = skeleton.getX();
		float skeletonY = skeleton.getY();

		Gdx.gl.glEnable(GL10.GL_BLEND);
		ShapeRenderer renderer = this.renderer;
		renderer.begin(ShapeType.Line);

		Array<Bone> bones = skeleton.getBones();
		if (drawBones) {
			renderer.setColor(boneLineColor);
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				if (bone.parent == null) continue;
				float x = skeletonX + bone.data.length * bone.m00 + bone.worldX;
				float y = skeletonY + bone.data.length * bone.m10 + bone.worldY;
				renderer.line(skeletonX + bone.worldX, skeletonY + bone.worldY, x, y);
			}
		}

		if (drawRegionAttachments) {
			renderer.setColor(regionAttachmentLineColor);
			Array<Slot> slots = skeleton.getSlots();
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (attachment instanceof RegionAttachment) {
					RegionAttachment regionAttachment = (RegionAttachment)attachment;
					regionAttachment.updateVertices(slot, false);
					float[] vertices = regionAttachment.getVertices();
					renderer.line(vertices[X1], vertices[Y1], vertices[X2], vertices[Y2]);
					renderer.line(vertices[X2], vertices[Y2], vertices[X3], vertices[Y3]);
					renderer.line(vertices[X3], vertices[Y3], vertices[X4], vertices[Y4]);
					renderer.line(vertices[X4], vertices[Y4], vertices[X1], vertices[Y1]);
				}
			}
		}

		if (drawBoundingBoxes) {
			SkeletonBounds bounds = this.bounds;
			bounds.update(skeleton);
			renderer.setColor(aabbColor);
			renderer.rect(bounds.getMinX(), bounds.getMinY(), bounds.getWidth(), bounds.getHeight());
			renderer.setColor(boundingBoxColor);
			Array<BoundingBoxAttachment> boundingBoxes = bounds.getBoundingBoxAttachments();
			for (int i = 0, n = boundingBoxes.size; i < n; i++)
				renderer.polygon(boundingBoxes.get(i).getVertices());
		}

		renderer.end();
		renderer.begin(ShapeType.Filled);

		if (drawBones) {
			renderer.setColor(boneOriginColor);
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				renderer.setColor(Color.GREEN);
				renderer.circle(skeletonX + bone.worldX, skeletonY + bone.worldY, 3);
			}
		}

		renderer.end();
	}

	public ShapeRenderer getShapeRenderer () {
		return renderer;
	}

	public void setBones (boolean bones) {
		this.drawBones = bones;
	}

	public void setRegionAttachments (boolean regionAttachments) {
		this.drawRegionAttachments = regionAttachments;
	}

	public void setBoundingBoxes (boolean boundingBoxes) {
		this.drawBoundingBoxes = boundingBoxes;
	}
}
