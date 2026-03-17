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
		// replaces "object.pose == object.applied" of reference implementation.
		bool PoseEqualsApplied { get; }
		// replaces "object.applied = object.pose" of reference implementation.
		void UsePose ();
		// replaces "object.applied = object.constrained" of reference implementation.
		void UseConstrained ();
		// replaces "object.applied.Set(object.pose)" of reference implementation.
		void ResetConstrained ();
	}

	public interface IPosed {
		void SetupPose ();
	}

	public class Posed<D, P, A> : IPosed, IPosedInternal
		where D : PosedData<P>
		where P : IPose<P>
		where A : P {

		internal readonly D data;
		internal readonly A pose;
		internal readonly A constrained;
		internal A applied;

		public Posed (D data, A pose, A constrained) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;
			this.pose = pose;
			this.constrained = constrained;
			applied = pose;
		}

		public virtual void SetupPose () {
			pose.Set(data.setup);
		}

		bool IPosedInternal.PoseEqualsApplied {
			get { return (object)pose == (object)applied; }
		}

		void IPosedInternal.UsePose () {
			applied = pose;
		}

		void IPosedInternal.UseConstrained () {
			applied = constrained;
		}

		void IPosedInternal.ResetConstrained () {
			applied.Set(pose);
		}

		/// <summary>The constraint's setup pose data.</summary>
		public D Data { get { return data; } }

		public P Pose { get { return pose; } }

		public A AppliedPose { get { return applied; } }

		override public string ToString () {
			return data.name;
		}
	}
}
