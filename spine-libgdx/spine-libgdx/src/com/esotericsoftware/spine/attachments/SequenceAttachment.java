/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.math.MathUtils;

import com.esotericsoftware.spine.Slot;

/** An attachment that applies a sequence of texture atlas regions to a region or mesh attachment.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-sequences">Sequence attachments</a> in the Spine User Guide. */
public class SequenceAttachment<T extends Attachment & TextureRegionAttachment> extends Attachment {
	private T attachment;
	private String path;
	private int frameCount;
	private float frameTime;
	private SequenceMode mode;
	private TextureRegion[] regions;

	public SequenceAttachment (String name) {
		super(name);
	}

	/** Updates the {@link #attachment} with the {@link #regions region} for the slot's {@link Slot#getAttachmentTime()} and
	 * returns it. */
	public T updateAttachment (Slot slot) {
		int index = (int)(slot.getAttachmentTime() / frameTime);
		switch (mode) {
		case forward:
			index = Math.min(frameCount - 1, index);
			break;
		case backward:
			index = Math.max(frameCount - index - 1, 0);
			break;
		case forwardLoop:
			index = index % frameCount;
			break;
		case backwardLoop:
			index = frameCount - (index % frameCount) - 1;
			break;
		case pingPong:
			index = index % (frameCount << 1);
			if (index >= frameCount) index = frameCount - 1 - (index - frameCount);
			break;
		case random:
			index = MathUtils.random(frameCount - 1);
		}
		attachment.setRegion(regions[index]);
		attachment.updateRegion();
		return attachment;
	}

	public void setAttachment (T attachment) {
		this.attachment = attachment;
	}

	public T getAttachment () {
		return attachment;
	}

	/** The prefix used to find the {@link #regions} for this attachment. */
	public String getPath () {
		return path;
	}

	public void setPath (String path) {
		this.path = path;
	}

	public SequenceMode getMode () {
		return mode;
	}

	public void setMode (SequenceMode mode) {
		if (mode == null) throw new IllegalArgumentException("mode cannot be null.");
		this.mode = mode;
	}

	public int getFrameCount () {
		return frameCount;
	}

	public void setFrameCount (int frameCount) {
		this.frameCount = frameCount;
	}

	/** The time in seconds each frame is shown. */
	public float getFrameTime () {
		return frameTime;
	}

	public void setFrameTime (float frameTime) {
		this.frameTime = frameTime;
	}

	public TextureRegion[] getRegions () {
		if (regions == null) throw new IllegalStateException("Regions have not been set: " + name);
		return regions;
	}

	public void setRegions (TextureRegion[] regions) {
		if (regions == null) throw new IllegalArgumentException("regions cannot be null.");
		this.regions = regions;
	}

	public Attachment copy () {
		SequenceAttachment copy = new SequenceAttachment(name);
		copy.attachment = attachment.copy();
		copy.path = path;
		copy.frameCount = frameCount;
		copy.frameTime = frameTime;
		copy.frameTime = frameTime;
		copy.mode = mode;
		copy.regions = regions;
		return copy;
	}

	static public enum SequenceMode {
		forward, backward, forwardLoop, backwardLoop, pingPong, random;

		static public final SequenceMode[] values = values();
	}
}
