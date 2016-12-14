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

/** Stores the setup pose for a {@link TransformConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide. */
public class TransformConstraintData {
	final String name;
	int order;
	final Array<BoneData> bones = new Array();
	BoneData target;
	float rotateMix, translateMix, scaleMix, shearMix;
	float offsetRotation, offsetX, offsetY, offsetScaleX, offsetScaleY, offsetShearY;
	boolean relative, local;

	public TransformConstraintData (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	/** The transform constraint's name, which is unique within the skeleton. */
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

	/** The bones that will be modified by this transform constraint. */
	public Array<BoneData> getBones () {
		return bones;
	}

	/** The target bone whose world transform will be copied to the constrained bones. */
	public BoneData getTarget () {
		return target;
	}

	public void setTarget (BoneData target) {
		if (target == null) throw new IllegalArgumentException("target cannot be null.");
		this.target = target;
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

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scales. */
	public float getScaleMix () {
		return scaleMix;
	}

	public void setScaleMix (float scaleMix) {
		this.scaleMix = scaleMix;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained shears. */
	public float getShearMix () {
		return shearMix;
	}

	public void setShearMix (float shearMix) {
		this.shearMix = shearMix;
	}

	/** An offset added to the constrained bone rotation. */
	public float getOffsetRotation () {
		return offsetRotation;
	}

	public void setOffsetRotation (float offsetRotation) {
		this.offsetRotation = offsetRotation;
	}

	/** An offset added to the constrained bone X translation. */
	public float getOffsetX () {
		return offsetX;
	}

	public void setOffsetX (float offsetX) {
		this.offsetX = offsetX;
	}

	/** An offset added to the constrained bone Y translation. */
	public float getOffsetY () {
		return offsetY;
	}

	public void setOffsetY (float offsetY) {
		this.offsetY = offsetY;
	}

	/** An offset added to the constrained bone scaleX. */
	public float getOffsetScaleX () {
		return offsetScaleX;
	}

	public void setOffsetScaleX (float offsetScaleX) {
		this.offsetScaleX = offsetScaleX;
	}

	/** An offset added to the constrained bone scaleY. */
	public float getOffsetScaleY () {
		return offsetScaleY;
	}

	public void setOffsetScaleY (float offsetScaleY) {
		this.offsetScaleY = offsetScaleY;
	}

	/** An offset added to the constrained bone shearY. */
	public float getOffsetShearY () {
		return offsetShearY;
	}

	public void setOffsetShearY (float offsetShearY) {
		this.offsetShearY = offsetShearY;
	}

	public boolean getRelative () {
		return relative;
	}

	public void setRelative (boolean relative) {
		this.relative = relative;
	}

	public boolean getLocal () {
		return local;
	}

	public void setLocal (boolean local) {
		this.local = local;
	}

	public String toString () {
		return name;
	}
}
