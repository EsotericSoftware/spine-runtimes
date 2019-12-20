/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package com.esotericsoftware.spine.utils;

import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonRenderer;

import com.badlogic.gdx.graphics.g2d.Batch;
import com.badlogic.gdx.scenes.scene2d.utils.BaseDrawable;

/** A scene2d drawable that draws a skeleton. The animation state and skeleton must be updated each frame, or
 * {@link #update(float)} called each frame. */
public class SkeletonDrawable extends BaseDrawable {
	private SkeletonRenderer renderer;
	private Skeleton skeleton;
	AnimationState state;
	private boolean resetBlendFunction = true;

	/** Creates an uninitialized SkeletonDrawable. The renderer, skeleton, and animation state must be set before use. */
	public SkeletonDrawable () {
	}

	public SkeletonDrawable (SkeletonRenderer renderer, Skeleton skeleton, AnimationState state) {
		this.renderer = renderer;
		this.skeleton = skeleton;
		this.state = state;
	}

	public void update (float delta) {
		state.update(delta);
		state.apply(skeleton);
	}

	public void draw (Batch batch, float x, float y, float width, float height) {
		int blendSrc = batch.getBlendSrcFunc(), blendDst = batch.getBlendDstFunc();
		int blendSrcAlpha = batch.getBlendSrcFuncAlpha(), blendDstAlpha = batch.getBlendDstFuncAlpha();

		skeleton.setPosition(x, y);
		skeleton.updateWorldTransform();
		renderer.draw(batch, skeleton);

		if (resetBlendFunction) batch.setBlendFunctionSeparate(blendSrc, blendDst, blendSrcAlpha, blendDstAlpha);
	}

	public SkeletonRenderer getRenderer () {
		return renderer;
	}

	public void setRenderer (SkeletonRenderer renderer) {
		this.renderer = renderer;
	}

	public Skeleton getSkeleton () {
		return skeleton;
	}

	public void setSkeleton (Skeleton skeleton) {
		this.skeleton = skeleton;
	}

	public AnimationState getAnimationState () {
		return state;
	}

	public void setAnimationState (AnimationState state) {
		this.state = state;
	}

	public boolean getResetBlendFunction () {
		return resetBlendFunction;
	}

	/** If false, the blend function will be left as whatever {@link SkeletonRenderer#draw(Batch, Skeleton)} set. This can reduce
	 * batch flushes in some cases, but means other rendering may need to first set the blend function. Default is true. */
	public void setResetBlendFunction (boolean resetBlendFunction) {
		this.resetBlendFunction = resetBlendFunction;
	}
}
