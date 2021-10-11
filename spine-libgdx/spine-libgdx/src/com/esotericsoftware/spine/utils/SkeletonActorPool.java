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

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Pool;

import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationState.TrackEntry;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.SkeletonRenderer;
import com.esotericsoftware.spine.Skin;

public class SkeletonActorPool extends Pool<SkeletonActor> {
	private SkeletonRenderer renderer;
	SkeletonData skeletonData;
	AnimationStateData stateData;
	private final Pool<Skeleton> skeletonPool;
	private final Pool<AnimationState> statePool;
	private final Array<SkeletonActor> obtained;

	public SkeletonActorPool (SkeletonRenderer renderer, SkeletonData skeletonData, AnimationStateData stateData) {
		this(renderer, skeletonData, stateData, 16, Integer.MAX_VALUE);
	}

	public SkeletonActorPool (SkeletonRenderer renderer, SkeletonData skeletonData, AnimationStateData stateData,
		int initialCapacity, int max) {
		super(initialCapacity, max);

		this.renderer = renderer;
		this.skeletonData = skeletonData;
		this.stateData = stateData;

		obtained = new Array(false, initialCapacity);

		skeletonPool = new Pool<Skeleton>(initialCapacity, max) {
			protected Skeleton newObject () {
				return new Skeleton(SkeletonActorPool.this.skeletonData);
			}

			protected void reset (Skeleton skeleton) {
				skeleton.setColor(Color.WHITE);
				skeleton.setScale(1, 1);
				skeleton.setSkin((Skin)null);
				skeleton.setSkin(SkeletonActorPool.this.skeletonData.getDefaultSkin());
				skeleton.setToSetupPose();
			}
		};

		statePool = new Pool<AnimationState>(initialCapacity, max) {
			protected AnimationState newObject () {
				return new AnimationState(SkeletonActorPool.this.stateData);
			}

			protected void reset (AnimationState state) {
				state.clearTracks();
				state.clearListeners();
			}
		};
	}

	/** Each obtained skeleton actor that is no longer playing an animation is removed from the stage and returned to the pool. */
	public void freeComplete () {
		Object[] obtained = this.obtained.items;
		outer:
		for (int i = this.obtained.size - 1; i >= 0; i--) {
			SkeletonActor actor = (SkeletonActor)obtained[i];
			Array<TrackEntry> tracks = actor.state.getTracks();
			for (int ii = 0, nn = tracks.size; ii < nn; ii++)
				if (tracks.get(ii) != null) continue outer;
			free(actor);
		}
	}

	protected SkeletonActor newObject () {
		SkeletonActor actor = new SkeletonActor();
		actor.setRenderer(renderer);
		return actor;
	}

	/** This pool keeps a reference to the obtained instance, so it should be returned to the pool via {@link #free(SkeletonActor)}
	 * , {@link #freeAll(Array)} or {@link #freeComplete()} to avoid leaking memory. */
	public SkeletonActor obtain () {
		SkeletonActor actor = super.obtain();
		actor.setSkeleton(skeletonPool.obtain());
		actor.setAnimationState(statePool.obtain());
		obtained.add(actor);
		return actor;
	}

	protected void reset (SkeletonActor actor) {
		actor.remove();
		obtained.removeValue(actor, true);
		skeletonPool.free(actor.getSkeleton());
		statePool.free(actor.getAnimationState());
	}

	public Array<SkeletonActor> getObtained () {
		return obtained;
	}
}
