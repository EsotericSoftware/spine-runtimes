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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.g2d.TextureRegion;

import com.esotericsoftware.spine.Slot;

public class Sequence {
	static private int nextID;

	private final int id = nextID();
	private final TextureRegion[] regions;
	private int start, digits, setupIndex;

	public Sequence (int count) {
		regions = new TextureRegion[count];
	}

	/** Copy constructor. */
	protected Sequence (Sequence other) {
		regions = new TextureRegion[other.regions.length];
		arraycopy(other.regions, 0, regions, 0, regions.length);

		start = other.start;
		digits = other.digits;
		setupIndex = other.setupIndex;
	}

	public void apply (Slot slot, HasTextureRegion attachment) {
		int index = slot.getSequenceIndex();
		if (index == -1) index = setupIndex;
		if (index >= regions.length) index = regions.length - 1;
		TextureRegion region = regions[index];
		if (attachment.getRegion() != region) {
			attachment.setRegion(region);
			attachment.updateRegion();
		}
	}

	public String getPath (String basePath, int index) {
		StringBuilder buffer = new StringBuilder(basePath.length() + digits);
		buffer.append(basePath);
		String frame = Integer.toString(start + index);
		for (int i = digits - frame.length(); i > 0; i--)
			buffer.append('0');
		buffer.append(frame);
		return buffer.toString();
	}

	public int getStart () {
		return start;
	}

	public void setStart (int start) {
		this.start = start;
	}

	public int getDigits () {
		return digits;
	}

	public void setDigits (int digits) {
		this.digits = digits;
	}

	/** The index of the region to show for the setup pose. */
	public int getSetupIndex () {
		return setupIndex;
	}

	public void setSetupIndex (int index) {
		this.setupIndex = index;
	}

	public TextureRegion[] getRegions () {
		return regions;
	}

	/** Returns a unique ID for this attachment. */
	public int getId () {
		return id;
	}

	static private synchronized int nextID () {
		return nextID++;
	}

	static public enum SequenceMode {
		hold, once, loop, pingpong, onceReverse, loopReverse, pingpongReverse;

		static public final SequenceMode[] values = values();
	}
}
