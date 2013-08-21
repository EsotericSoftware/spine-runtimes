
package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import com.badlogic.gdx.graphics.GL11;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.utils.Array;

public class SkeletonRenderer {
	private boolean premultipliedAlpha;

	public void draw (SpriteBatch batch, Skeleton skeleton) {
		boolean premultipliedAlpha = this.premultipliedAlpha;
		int srcFunc = premultipliedAlpha ? GL11.GL_ONE : GL11.GL_SRC_ALPHA;
		batch.setBlendFunction(srcFunc, GL11.GL_ONE_MINUS_SRC_ALPHA);

		boolean additive = false;

		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				regionAttachment.updateVertices(slot, premultipliedAlpha);
				float[] vertices = regionAttachment.getVertices();
				if (slot.data.getAdditiveBlending() != additive) {
					additive = !additive;
					if (additive)
						batch.setBlendFunction(srcFunc, GL11.GL_ONE);
					else
						batch.setBlendFunction(srcFunc, GL11.GL_ONE_MINUS_SRC_ALPHA);
				}
				batch.draw(regionAttachment.getRegion().getTexture(), vertices, 0, 20);
			}
		}
	}

	public void setPremultipliedAlpha (boolean premultipliedAlpha) {
		this.premultipliedAlpha = premultipliedAlpha;
	}
}
