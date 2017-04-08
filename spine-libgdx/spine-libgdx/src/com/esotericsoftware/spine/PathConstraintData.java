/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

/** Stores the setup pose for a {@link PathConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-path-constraints">Path constraints</a> in the Spine User Guide. */
public class PathConstraintData {
	final String name;
	int order;
	final Array<BoneData> bones = new Array();
	SlotData target;
	PositionMode positionMode;
	SpacingMode spacingMode;
	RotateMode rotateMode;
	float offsetRotation;
	float position, spacing, rotateMix, translateMix;

	public PathConstraintData (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	/** The path constraint's name, which is unique within the skeleton. */
	public String getName () {
		return name;
	}

	/** See {@link Constraint#getOrder()}. */
	public int getOrder () {
		return order;
	}

	public void setOrder (int order) {
		this.order = order;
	}

	/** The bones that will be modified by this path constraint. */
	public Array<BoneData> getBones () {
		return bones;
	}

	/** The slot whose path attachment will be used to constrained the bones. */
	public SlotData getTarget () {
		return target;
	}

	public void setTarget (SlotData target) {
		this.target = target;
	}

	/** The mode for positioning the first bone on the path. */
	public PositionMode getPositionMode () {
		return positionMode;
	}

	public void setPositionMode (PositionMode positionMode) {
		this.positionMode = positionMode;
	}

	/** The mode for positioning the bones after the first bone on the path. */
	public SpacingMode getSpacingMode () {
		return spacingMode;
	}

	public void setSpacingMode (SpacingMode spacingMode) {
		this.spacingMode = spacingMode;
	}

	/** The mode for adjusting the rotation of the bones. */
	public RotateMode getRotateMode () {
		return rotateMode;
	}

	public void setRotateMode (RotateMode rotateMode) {
		this.rotateMode = rotateMode;
	}

	/** An offset added to the constrained bone rotation. */
	public float getOffsetRotation () {
		return offsetRotation;
	}

	public void setOffsetRotation (float offsetRotation) {
		this.offsetRotation = offsetRotation;
	}

	/** The position along the path. */
	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	/** The spacing between bones. */
	public float getSpacing () {
		return spacing;
	}

	public void setSpacing (float spacing) {
		this.spacing = spacing;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
	public float getRotateMix () {
		return rotateMix;
	}

	public void setRotateMix (float rotateMix) {
		this.rotateMix = rotateMix;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translations. */
	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public String toString () {
		return name;
	}

	/** Controls how the first bone is positioned along the path.
	 * <p>
	 * See <a href="http://esotericsoftware.com/spine-path-constraints#Position-mode">Position mode</a> in the Spine User Guide. */
	static public enum PositionMode {
		fixed, percent;

		static public final PositionMode[] values = PositionMode.values();
	}

	/** Controls how bones after the first bone are positioned along the path.
	 * <p>
	 * See <a href="http://esotericsoftware.com/spine-path-constraints#Spacing-mode">Spacing mode</a> in the Spine User Guide. */
	static public enum SpacingMode {
		length, fixed, percent;

		static public final SpacingMode[] values = SpacingMode.values();
	}

	/** Controls how bones are rotated, translated, and scaled to match the path.
	 * <p>
	 * See <a href="http://esotericsoftware.com/spine-path-constraints#Rotate-mode">Rotate mode</a> in the Spine User Guide. */
	static public enum RotateMode {
		tangent, chain, chainScale;

		static public final RotateMode[] values = RotateMode.values();
	}
}
