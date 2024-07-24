/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.android;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.Animation;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.android.utils.SkeletonDataUtils;

import java.io.File;
import java.net.URL;

/** A {@link AndroidSkeletonDrawable} bundles loading updating updating an {@link AndroidTextureAtlas}, {@link Skeleton}, and
 * {@link AnimationState} into a single easy-to-use class.
 *
 * Use the {@link AndroidSkeletonDrawable#fromAsset(String, String, Context)},
 * {@link AndroidSkeletonDrawable#fromFile(File, File)}, or {@link AndroidSkeletonDrawable#fromHttp(URL, URL, File)} methods to
 * construct a {@link AndroidSkeletonDrawable}. To have multiple skeleton drawable instances share the same
 * {@link AndroidTextureAtlas} and {@link SkeletonData}, use the constructor.
 *
 * You can then directly access the {@link AndroidSkeletonDrawable#getAtlas()}, {@link AndroidSkeletonDrawable#getSkeletonData()},
 * {@link AndroidSkeletonDrawable#getSkeleton()}, {@link AndroidSkeletonDrawable#getAnimationStateData()}, and
 * {@link AndroidSkeletonDrawable#getAnimationState()} to query and animate the skeleton. Use the {@link AnimationState} to queue
 * animations on one or more tracks via {@link AnimationState#setAnimation(int, Animation, boolean)} or
 * {@link AnimationState#addAnimation(int, Animation, boolean, float)}.
 *
 * To update the {@link AnimationState} and apply it to the {@link Skeleton}, call the
 * {@link AndroidSkeletonDrawable#update(float)} function, providing it a delta time in seconds to advance the animations.
 *
 * To render the current pose of the {@link Skeleton}, use {@link SkeletonRenderer#render(Skeleton)},
 * {@link SkeletonRenderer#renderToCanvas(Canvas, Array)}, {@link SkeletonRenderer#renderToBitmap(float, float, int, Skeleton)},
 * depending on your needs. */
public class AndroidSkeletonDrawable {

	private final AndroidTextureAtlas atlas;

	private final SkeletonData skeletonData;

	private final Skeleton skeleton;

	private final AnimationStateData animationStateData;

	private final AnimationState animationState;

	/** Constructs a new skeleton drawable from the given (possibly shared) {@link AndroidTextureAtlas} and
	 * {@link SkeletonData}. */
	public AndroidSkeletonDrawable (AndroidTextureAtlas atlas, SkeletonData skeletonData) {
		this.atlas = atlas;
		this.skeletonData = skeletonData;

		skeleton = new Skeleton(skeletonData);
		animationStateData = new AnimationStateData(skeletonData);
		animationState = new AnimationState(animationStateData);

		skeleton.updateWorldTransform(Skeleton.Physics.none);
	}

	/** Updates the {@link AnimationState} using the {@code delta} time given in seconds, applies the animation state to the
	 * {@link Skeleton} and updates the world transforms of the skeleton to calculate its current pose. */
	public void update (float delta) {
		animationState.update(delta);
		animationState.apply(skeleton);

		skeleton.update(delta);
		skeleton.updateWorldTransform(Skeleton.Physics.update);
	}

	/** Get the {@link AndroidTextureAtlas} */
	public AndroidTextureAtlas getAtlas () {
		return atlas;
	}

	/** Get the {@link Skeleton} */
	public Skeleton getSkeleton () {
		return skeleton;
	}

	/** Get the {@link SkeletonData} */
	public SkeletonData getSkeletonData () {
		return skeletonData;
	}

	/** Get the {@link AnimationStateData} */
	public AnimationStateData getAnimationStateData () {
		return animationStateData;
	}

	/** Get the {@link AnimationState} */
	public AnimationState getAnimationState () {
		return animationState;
	}

	/** Constructs a new skeleton drawable from the {@code atlasFileName} and {@code skeletonFileName} from the the apps resources
	 * using {@link Context}.
	 *
	 * Throws an exception in case the data could not be loaded. */
	public static AndroidSkeletonDrawable fromAsset (String atlasFileName, String skeletonFileName, Context context) {
		AndroidTextureAtlas atlas = AndroidTextureAtlas.fromAsset(atlasFileName, context);
		SkeletonData skeletonData = SkeletonDataUtils.fromAsset(atlas, skeletonFileName, context);
		return new AndroidSkeletonDrawable(atlas, skeletonData);
	}

	/** Constructs a new skeleton drawable from the {@code atlasFile} and {@code skeletonFile}.
	 *
	 * Throws an exception in case the data could not be loaded. */
	public static AndroidSkeletonDrawable fromFile (File atlasFile, File skeletonFile) {
		AndroidTextureAtlas atlas = AndroidTextureAtlas.fromFile(atlasFile);
		SkeletonData skeletonData = SkeletonDataUtils.fromFile(atlas, skeletonFile);
		return new AndroidSkeletonDrawable(atlas, skeletonData);
	}

	/** Constructs a new skeleton drawable from the {@code atlasUrl} and {@code skeletonUrl}.
	 *
	 * Throws an exception in case the data could not be loaded. */
	public static AndroidSkeletonDrawable fromHttp (URL atlasUrl, URL skeletonUrl, File targetDirectory) {
		AndroidTextureAtlas atlas = AndroidTextureAtlas.fromHttp(atlasUrl, targetDirectory);
		SkeletonData skeletonData = SkeletonDataUtils.fromHttp(atlas, skeletonUrl, targetDirectory);
		return new AndroidSkeletonDrawable(atlas, skeletonData);
	}
}
