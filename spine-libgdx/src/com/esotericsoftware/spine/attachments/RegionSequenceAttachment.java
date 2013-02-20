
package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Slot;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.math.MathUtils;

/** Attachment that displays various texture regions over time. */
public class RegionSequenceAttachment extends RegionAttachment {
	private final Mode mode;
	private float frameTime;
	private TextureRegion[] regions;

	/** @param frameTime Time in seconds each frame is shown. */
	public RegionSequenceAttachment (String name, float frameTime, Mode mode) {
		super(name);
		if (mode == null) throw new IllegalArgumentException("mode cannot be null.");

		this.frameTime = frameTime;
		this.mode = mode;
	}

	public void draw (SpriteBatch batch, Slot slot) {
		if (regions == null) throw new IllegalStateException("RegionSequenceAttachment is not resolved: " + this);

		int frameIndex = (int)(slot.getAttachmentTime() / frameTime);
		switch (mode) {
		case forward:
			frameIndex = Math.min(regions.length - 1, frameIndex);
			break;
		case forwardLoop:
			frameIndex = frameIndex % regions.length;
			break;
		case pingPong:
			frameIndex = frameIndex % (regions.length * 2);
			if (frameIndex >= regions.length) frameIndex = regions.length - 1 - (frameIndex - regions.length);
			break;
		case random:
			frameIndex = MathUtils.random(regions.length - 1);
			break;
		case backward:
			frameIndex = Math.max(regions.length - frameIndex - 1, 0);
			break;
		case backwardLoop:
			frameIndex = frameIndex % regions.length;
			frameIndex = regions.length - frameIndex - 1;
			break;
		}
		setRegion(regions[frameIndex]);
		super.draw(batch, slot);
	}

	/** May be null if the attachment is not resolved. */
	public TextureRegion[] getRegions () {
		if (regions == null) throw new IllegalStateException("RegionSequenceAttachment is not resolved: " + this);
		return regions;
	}

	public void setRegions (TextureRegion[] regions) {
		this.regions = regions;
	}

	static public enum Mode {
		forward, backward, forwardLoop, backwardLoop, pingPong, random
	}
}
