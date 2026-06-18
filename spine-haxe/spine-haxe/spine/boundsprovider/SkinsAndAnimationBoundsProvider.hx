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

package spine.boundsprovider;

import spine.animation.AnimationState;
import spine.boundsprovider.BoundsProvider.BoundsGameObject;
import spine.boundsprovider.BoundsProvider.BoundsRectangle;

/** A bounds provider that calculates the bounding box by taking the maximumg bounding box for a combination of skins and specific animation. */
class SkinsAndAnimationBoundsProvider extends BoundsProvider {
	private var animation:String;
	private var skins:Array<String>;
	private var timeStep:Float;
	private var clipping:Bool;

	/**
	 * @param animation The animation to use for calculating the bounds. If null, the setup pose is used.
	 * @param skins The skins to use for calculating the bounds. If empty, the default skin is used.
	 * @param timeStep The time step to use for calculating the bounds. A smaller time step means more precision, but slower calculation.
	 * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
	 */
	public function new(?animation:String, ?skins:Array<String>, clipping = false, timeStep:Float = 0.05) {
		if (skins == null)
			skins = [];
		this.animation = animation;
		this.skins = skins;
		this.timeStep = timeStep;
		this.clipping = clipping;
	}

	public function calculateBounds(gameObject:BoundsGameObject, out:BoundsRectangle):BoundsRectangle {
		var prevSkeleton = gameObject.skeleton;
		var state = gameObject.state;
		if (prevSkeleton == null || state == null) {
			zeroRectangle(out);
			return out;
		}

		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		var animationState = new AnimationState(state.data);
		var skeleton = new Skeleton(prevSkeleton.data);
		skeleton.scaleX = prevSkeleton.scaleX;
		skeleton.scaleY = prevSkeleton.scaleY * Bone.yDir;
		var clipper = clipping ? new SkeletonClipping() : null;
		var data = skeleton.data;
		if (skins.length > 0) {
			var customSkin = new Skin("custom-skin");
			for (skinName in skins) {
				var skin = data.findSkin(skinName);

				if (skin == null)
					continue;
				customSkin.addSkin(skin);
			}
			skeleton.skin = customSkin;
		}
		skeleton.setupPose();
		var animation = this.animation != null ? data.findAnimation(this.animation) : null;

		if (animation == null) {
			skeleton.updateWorldTransform(Physics.update);
			var newBounds = skeleton.getBounds(clipper);
			out.x = newBounds.x;
			out.y = newBounds.y;
			out.width = newBounds.width;
			out.height = newBounds.height;
			return out;
		}

		var minX = Math.POSITIVE_INFINITY,
			minY = Math.POSITIVE_INFINITY,
			maxX = Math.NEGATIVE_INFINITY,
			maxY = Math.NEGATIVE_INFINITY;
		animationState.clearTracks();
		animationState.setAnimation(0, animation, false);
		var steps = Math.max(animation.duration / this.timeStep, 1.0);
		var i = 0.0;
		while (i < steps) {
			var delta = i > 0 ? this.timeStep : 0;

			animationState.update(delta);
			animationState.apply(skeleton);
			skeleton.update(delta);
			skeleton.updateWorldTransform(Physics.update);
			var bounds = skeleton.getBounds(clipper);
			minX = Math.min(minX, bounds.x);
			minY = Math.min(minY, bounds.y);
			maxX = Math.max(maxX, bounds.x + bounds.width);
			maxY = Math.max(maxY, bounds.y + bounds.height);
			i++;
		}
		out.x = minX;
		out.y = minY;
		out.width = maxX - minX;
		out.height = maxY - minY;
		return out;
	}
}
