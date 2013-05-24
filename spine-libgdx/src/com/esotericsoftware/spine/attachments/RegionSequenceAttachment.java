/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Slot;

import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.math.MathUtils;

/** Attachment that displays various texture regions over time. */
public class RegionSequenceAttachment extends RegionAttachment {
	private Mode mode;
	private float frameTime;
	private TextureRegion[] regions;

	public RegionSequenceAttachment (String name) {
		super(name);
	}

	public void updateVertices (Slot slot) {
		if (regions == null) throw new IllegalStateException("Regions have not been set: " + this);

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

		super.updateVertices(slot);
	}

	public TextureRegion[] getRegions () {
		if (regions == null) throw new IllegalStateException("Regions have not been set: " + this);
		return regions;
	}

	public void setRegions (TextureRegion[] regions) {
		this.regions = regions;
	}

	/** Sets the time in seconds each frame is shown. */
	public void setFrameTime (float frameTime) {
		this.frameTime = frameTime;
	}

	public void setMode (Mode mode) {
		this.mode = mode;
	}

	static public enum Mode {
		forward, backward, forwardLoop, backwardLoop, pingPong, random
	}
}
