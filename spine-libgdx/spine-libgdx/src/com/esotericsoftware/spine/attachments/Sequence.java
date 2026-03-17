/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import com.esotericsoftware.spine.SlotPose;

/** Holds texture regions, UVs, and vertex offsets for rendering a region or mesh attachment. {@link #getRegions() Regions} must
 * be populated and {@link #update(HasSequence)} called before use. */
public class Sequence {
	static private int nextID;

	private final int id = nextID();
	private final TextureRegion[] regions;
	private final boolean pathSuffix;
	private float[][] uvs, offsets;
	private int start, digits, setupIndex;

	public Sequence (int count, boolean pathSuffix) {
		regions = new TextureRegion[count];
		this.pathSuffix = pathSuffix;
	}

	/** Copy constructor. */
	protected Sequence (Sequence other) {
		int regionCount = other.regions.length;
		regions = new TextureRegion[regionCount];
		arraycopy(other.regions, 0, regions, 0, regionCount);

		start = other.start;
		digits = other.digits;
		setupIndex = other.setupIndex;
		pathSuffix = other.pathSuffix;

		if (other.uvs != null) {
			int length = other.uvs[0].length;
			uvs = new float[regionCount][length];
			for (int i = 0; i < regionCount; i++)
				arraycopy(other.uvs[i], 0, uvs[i], 0, length);
		}
		if (other.offsets != null) {
			offsets = new float[regionCount][8];
			for (int i = 0; i < regionCount; i++)
				arraycopy(other.offsets[i], 0, offsets[i], 0, 8);
		}
	}

	/** Computes UVs and offsets for the specified attachment. Must be called if the regions or attachment properties are
	 * changed. */
	public void update (HasSequence attachment) {
		int regionCount = regions.length;
		if (attachment instanceof RegionAttachment region) {
			uvs = new float[regionCount][8];
			offsets = new float[regionCount][8];
			for (int i = 0; i < regionCount; i++) {
				RegionAttachment.computeUVs(regions[i], region.x, region.y, region.scaleX, region.scaleY, region.rotation,
					region.width, region.height, offsets[i], uvs[i]);
			}
		} else if (attachment instanceof MeshAttachment mesh) {
			float[] regionUVs = mesh.regionUVs;
			uvs = new float[regionCount][regionUVs.length];
			offsets = null;
			for (int i = 0; i < regionCount; i++)
				MeshAttachment.computeUVs(regions[i], regionUVs, uvs[i]);
		}
	}

	public TextureRegion[] getRegions () {
		return regions;
	}

	public int resolveIndex (SlotPose pose) {
		int index = pose.getSequenceIndex();
		if (index == -1) index = setupIndex;
		if (index >= regions.length) index = regions.length - 1;
		return index;
	}

	public TextureRegion getRegion (int index) {
		return regions[index];
	}

	public float[] getUVs (int index) {
		return uvs[index];
	}

	/** Returns vertex offsets from the center of a {@link RegionAttachment}. Invalid to call for a {@link MeshAttachment}. */
	public float[] getOffsets (int index) {
		return offsets[index];
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

	public boolean hasPathSuffix () {
		return pathSuffix;
	}

	public String getPath (String basePath, int index) {
		if (!pathSuffix) return basePath;
		var buffer = new StringBuilder(basePath.length() + digits);
		buffer.append(basePath);
		String frame = Integer.toString(start + index);
		for (int i = digits - frame.length(); i > 0; i--)
			buffer.append('0');
		buffer.append(frame);
		return buffer.toString();
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
