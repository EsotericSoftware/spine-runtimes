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

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.TransformConstraintData.FromProperty;

/** Stores the setup pose for a {@link Slider}. */
public class SliderData extends ConstraintData<Slider, SliderPose> {
	Animation animation;
	boolean additive, loop;
	@Null BoneData bone;
	@Null FromProperty property;
	float offset, scale;
	boolean local;

	// Nonessential.
	float max;

	public SliderData (String name) {
		super(name, new SliderPose());
	}

	public Slider create (Skeleton skeleton) {
		return new Slider(this, skeleton);
	}

	/** The animation the slider will apply. */
	public Animation getAnimation () {
		return animation;
	}

	public void setAnimation (Animation animation) {
		this.animation = animation;
	}

	/** When true, the animation is applied by adding it to the current pose rather than overwriting it. */
	public boolean getAdditive () {
		return additive;
	}

	public void setAdditive (boolean additive) {
		this.additive = additive;
	}

	/** When true, the animation repeats after its duration, otherwise the last frame is used. */
	public boolean getLoop () {
		return loop;
	}

	public void setLoop (boolean loop) {
		this.loop = loop;
	}

	/** When set, the bone's transform property is used to set the slider's {@link SliderPose#time}. */
	public @Null BoneData getBone () {
		return bone;
	}

	public void setBone (@Null BoneData bone) {
		this.bone = bone;
	}

	/** When a bone is set, the specified transform property is used to set the slider's {@link SliderPose#time}. */
	public @Null FromProperty getProperty () {
		return property;
	}

	public void setProperty (@Null FromProperty property) {
		this.property = property;
	}

	/** When a bone is set, the offset is added to the property. */
	public float getOffset () {
		return offset;
	}

	public void setOffset (float offset) {
		this.offset = offset;
	}

	/** When a bone is set, this is the scale of the {@link #property} value in relation to the slider time. */
	public float getScale () {
		return scale;
	}

	public void setScale (float scale) {
		this.scale = scale;
	}

	/** When a bone is set, the maximum slider time for the bone property range, or 0 if nonessential data was not exported. */
	public float getMax () {
		return max;
	}

	public void setMax (float max) {
		this.max = max;
	}

	/** When true and a bone is set, the bone's local transform property is read instead of its world transform. */
	public boolean getLocal () {
		return local;
	}

	public void setLocal (boolean local) {
		this.local = local;
	}
}
