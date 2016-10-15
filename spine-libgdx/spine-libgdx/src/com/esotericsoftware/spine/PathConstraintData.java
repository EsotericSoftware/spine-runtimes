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

public class PathConstraintData {
	final String name;
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

	public Array<BoneData> getBones () {
		return bones;
	}

	public SlotData getTarget () {
		return target;
	}

	public void setTarget (SlotData target) {
		this.target = target;
	}

	public PositionMode getPositionMode () {
		return positionMode;
	}

	public void setPositionMode (PositionMode positionMode) {
		this.positionMode = positionMode;
	}

	public SpacingMode getSpacingMode () {
		return spacingMode;
	}

	public void setSpacingMode (SpacingMode spacingMode) {
		this.spacingMode = spacingMode;
	}

	public RotateMode getRotateMode () {
		return rotateMode;
	}

	public void setRotateMode (RotateMode rotateMode) {
		this.rotateMode = rotateMode;
	}

	public float getOffsetRotation () {
		return offsetRotation;
	}

	public void setOffsetRotation (float offsetRotation) {
		this.offsetRotation = offsetRotation;
	}

	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	public float getSpacing () {
		return spacing;
	}

	public void setSpacing (float spacing) {
		this.spacing = spacing;
	}

	public float getRotateMix () {
		return rotateMix;
	}

	public void setRotateMix (float rotateMix) {
		this.rotateMix = rotateMix;
	}

	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	static public enum PositionMode {
		fixed, percent;

		static public final PositionMode[] values = PositionMode.values();
	}

	static public enum SpacingMode {
		length, fixed, percent;

		static public final SpacingMode[] values = SpacingMode.values();
	}

	static public enum RotateMode {
		tangent, chain, chainScale;

		static public final RotateMode[] values = RotateMode.values();
	}
}
