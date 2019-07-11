/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;

/** Stores the setup pose for a {@link Slot}. */
public class SlotData {
	final int index;
	final String name;
	final BoneData boneData;
	final Color color = new Color(1, 1, 1, 1);
	Color darkColor;
	String attachmentName;
	BlendMode blendMode;

	public SlotData (int index, String name, BoneData boneData) {
		if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		if (boneData == null) throw new IllegalArgumentException("boneData cannot be null.");
		this.index = index;
		this.name = name;
		this.boneData = boneData;
	}

	/** The index of the slot in {@link Skeleton#getSlots()}. */
	public int getIndex () {
		return index;
	}

	/** The name of the slot, which is unique across all slots in the skeleton. */
	public String getName () {
		return name;
	}

	/** The bone this slot belongs to. */
	public BoneData getBoneData () {
		return boneData;
	}

	/** The color used to tint the slot's attachment. If {@link #getDarkColor()} is set, this is used as the light color for two
	 * color tinting. */
	public Color getColor () {
		return color;
	}

	/** The dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
	 * color's alpha is not used. */
	public Color getDarkColor () {
		return darkColor;
	}

	/** @param darkColor May be null. */
	public void setDarkColor (Color darkColor) {
		this.darkColor = darkColor;
	}

	/** @param attachmentName May be null. */
	public void setAttachmentName (String attachmentName) {
		this.attachmentName = attachmentName;
	}

	/** The name of the attachment that is visible for this slot in the setup pose, or null if no attachment is visible. */
	public String getAttachmentName () {
		return attachmentName;
	}

	/** The blend mode for drawing the slot's attachment. */
	public BlendMode getBlendMode () {
		return blendMode;
	}

	public void setBlendMode (BlendMode blendMode) {
		if (blendMode == null) throw new IllegalArgumentException("blendMode cannot be null.");
		this.blendMode = blendMode;
	}

	public String toString () {
		return name;
	}
}
