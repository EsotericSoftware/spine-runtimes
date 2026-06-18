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

/** A bounds provider that calculates the bounding box from the setup pose. */
class SetupPoseBoundsProvider extends BoundsProvider {
	private var clipping:Bool;

	/**
	 * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
	 */
	public function new(clipping = false) {
		this.clipping = clipping;
	}

	public function calculateBounds(gameObject:BoundsGameObject, out:BoundsRectangle):BoundsRectangle {
		var prevSkeleton = gameObject.skeleton;
		if (prevSkeleton == null) {
			zeroRectangle(out);
			return out;
		}

		// Make a copy of  skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		var skeleton = new Skeleton(prevSkeleton.data);
		skeleton.scaleX = prevSkeleton.scaleX;
		skeleton.scaleY = prevSkeleton.scaleY * Bone.yDir;
		skeleton.setupPose();
		skeleton.updateWorldTransform(Physics.update);
		var newBounds = skeleton.getBounds(clipping ? new SkeletonClipping() : null);
		if (newBounds.width == Math.NEGATIVE_INFINITY) {
			zeroRectangle(out);
			return out;
		}
		out.x = newBounds.x;
		out.y = newBounds.y;
		out.width = newBounds.width;
		out.height = newBounds.height;
		return out;
	}
}
