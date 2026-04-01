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

package spine;

/** The base class for an object with a number of poses:
 * - data: The setup pose.
 * - pose: The unconstrained pose. Set by animations and application code.
 * - appliedPose: The pose to use for rendering. Possibly modified by constraints.
 */
abstract class Posed< //
	D:PosedData<P>, //
	P:Pose<Any>> {
	/** The setup pose data. May be shared with multiple instances. */
	public final data:D;

	/** The unconstrained pose for this object, set by animations and application code. */
	public final pose:P;

	public final constrainedPose:P;

	/** The pose to use for rendering. If no constraints modify this pose, this is the same as pose. Otherwise it is a
	 * copy of pose modified by constraints. */
	public var appliedPose:P;

	public function new(data:D, pose:P, constrainedPose:P) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		this.data = data;
		this.pose = pose;
		this.constrainedPose = constrainedPose;
		appliedPose = pose;
	}

	/** The setup pose data. May be shared with multiple instances. */
	public function getData():D {
		return data;
	}

	/** The unconstrained pose for this object, set by animations and application code. */
	public function getPose():P {
		return pose;
	}

	/** The pose to use for rendering. If no constraints modify this pose, this is the same as pose. Otherwise it is a
	 * copy of pose modified by constraints. */
	public function getAppliedPose():P {
		return appliedPose;
	}

	/** Sets the unconstrained pose to the setup pose. */
	public function setupPose():Void {
		pose.set(data.setupPose);
	}

	/** Sets the applied pose to the unconstrained pose, for when no constraints will modify the pose. */
	public function unconstrained():Void {
		appliedPose = pose;
	}

	/** Sets the applied pose to the constrained pose, in anticipation of the applied pose being modified by constraints. */
	public function constrained():Void {
		appliedPose = constrainedPose;
	}

	/** Sets the constrained pose to the unconstrained pose, as a starting point for constraints to be applied. */
	public function resetConstrained():Void {
		constrainedPose.set(pose);
	}

	public function toString():String {
		return data.name;
	}
}
