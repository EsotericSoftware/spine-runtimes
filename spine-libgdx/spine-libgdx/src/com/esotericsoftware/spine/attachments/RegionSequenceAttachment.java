/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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

	public void updateWorldVertices (Slot slot, boolean premultipliedAlpha) {
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

		super.updateWorldVertices(slot, premultipliedAlpha);
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
