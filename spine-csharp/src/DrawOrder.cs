/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Spine {
	/// <summary>
	/// Stores the skeleton's draw order, which is the order that each slot's attachment is rendered.
	/// </summary>
	public class DrawOrder {
		internal readonly ExposedList<Slot> setupPose, pose, constrainedPose;
		internal ExposedList<Slot> appliedPose;

		public DrawOrder (ExposedList<Slot> setupPose) {
			this.setupPose = setupPose;
			pose = new ExposedList<Slot>(setupPose);
			constrainedPose = new ExposedList<Slot>();
			appliedPose = pose;
		}

		/// <summary>Sets the unconstrained draw order to the setup pose order.</summary>
		public void SetupPose () {
			pose.EnsureSize(setupPose.Count);
			Array.Copy(setupPose.Items, 0, pose.Items, 0, setupPose.Count);
		}

		/// <summary>The unconstrained draw order, set by animations and application code.</summary>
		public ExposedList<Slot> Pose {
			get {
				return pose;
			}
		}

		/// <summary>
		/// The constrained draw order for rendering. If no constraints modify the draw order, this is the same as <see cref="pose"/>.
		/// Otherwise it is a copy of <see cref="pose"/> modified by constraints.
		/// </summary>
		public ExposedList<Slot> AppliedPose {
			get {
				return appliedPose;
			}
		}

		/// <summary>
		/// Sets the applied pose to the unconstrained pose, for when no constraints will modify the draw order.
		/// </summary>
		internal void Unconstrained () {
			appliedPose = pose;
		}

		/// <summary>
		/// Sets the applied pose to the constrained pose, in anticipation of the applied pose being modified by constraints.
		/// </summary>
		internal void Constrained () {
			appliedPose = constrainedPose;
		}

		/// <summary>
		/// Copies the unconstrained pose to the constrained pose, as a starting point for constraints to be applied.
		/// </summary>
		internal void ResetConstrained () { // Port: resetConstrained
			constrainedPose.EnsureSize(pose.Count);
			Array.Copy(pose.Items, 0, constrainedPose.Items, 0, pose.Count);
		}
	}
}
