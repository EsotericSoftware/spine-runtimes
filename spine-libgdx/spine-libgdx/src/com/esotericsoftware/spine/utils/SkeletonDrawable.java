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

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.graphics.g2d.Batch;
import com.badlogic.gdx.scenes.scene2d.utils.BaseDrawable;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonRenderer;

/** A scene2d drawable that draws a skeleton. The animation state and skeleton must be updated each frame, or
 * {@link #update(float)} called each frame. */
public class SkeletonDrawable extends BaseDrawable {
	private SkeletonRenderer renderer;
	private Skeleton skeleton;
	AnimationState state;

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
		skeleton.setPosition(x, y);
		skeleton.updateWorldTransform();
		renderer.draw(batch, skeleton);
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
}
