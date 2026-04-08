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

namespace Spine {
	internal interface IPosedInternal {
		// replaces "object.pose == object.appliedPose" of reference implementation.
		bool PoseEqualsApplied { get; }
		// replaces "object.appliedPose = object.pose" of reference implementation.
		void Unconstrained ();
		// replaces "object.appliedPose = object.constrainedPose" of reference implementation.
		void Constrained ();
		// replaces "object.appliedPose.Set(object.pose)" of reference implementation.
		void ResetConstrained ();
	}

	public interface IPosed {
		void SetupPose ();
	}

	/// <summary>The base class for an object with a number of poses:
	/// <list type="bullet">
	/// <item><see cref="Data"/>: The setup pose.</item>
	/// <item><see cref="Pose"/>: The unconstrained pose. Set by animations and application code.</item>
	/// <item><see cref="AppliedPose"/>: The pose to use for rendering. Possibly modified by constraints.</item>
	/// </list>
	/// </summary>
	public class Posed<D, P> : IPosed, IPosedInternal
		where D : PosedData<P>
		where P : IPose<P> {

		internal readonly D data;
		internal readonly P pose, constrainedPose;
		internal P appliedPose;

		protected Posed (D data, P pose, P constrainedPose) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;
			this.pose = pose;
			this.constrainedPose = constrainedPose;
			appliedPose = pose;
		}

		/// <summary>Sets the unconstrained pose to the setup pose.</summary>
		public virtual void SetupPose () {
			pose.Set(data.setupPose);
		}

		bool IPosedInternal.PoseEqualsApplied {
			get { return (object)pose == (object)appliedPose; }
		}

		void IPosedInternal.Unconstrained () {
			appliedPose = pose;
		}

		void IPosedInternal.Constrained () {
			appliedPose = constrainedPose;
		}

		void IPosedInternal.ResetConstrained () {
			appliedPose.Set(pose);
		}

		/// <summary>The setup pose data. May be shared with multiple instances.</summary>
		public D Data { get { return data; } }

		/// <summary>The unconstrained pose for this object, set by animations and application code.</summary>
		public P Pose { get { return pose; } }

		/// <summary>The pose to use for rendering. If no constraints modify this pose, this is the same as <see cref="Pose"/>. Otherwise it is a
		/// copy of <see cref="Pose"/> modified by constraints.</summary>
		public P AppliedPose { get { return appliedPose; } }

		override public string ToString () {
			return data.name;
		}
	}
}
