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
	public interface IConstraint : IPosedActive, IPosed {
		IConstraintData IData { get; }
		IConstraint Copy (Skeleton skeleton);
		bool IsSourceActive { get; }
		void Sort (Skeleton skeleton);
		void Update (Skeleton skeleton, Physics physics);
	}

	/// <summary>
	/// <para>
	/// Stores the current pose for an IK constraint. An IK constraint adjusts the rotation of 1 or 2 constrained bones so the tip of
	/// the last bone is as close to the target bone as possible.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-ik-constraints">IK constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public abstract class Constraint<T, D, P> : PosedActive<D, P>, IUpdate, IConstraint
		where T : Constraint<T, D, P>
		where D : ConstraintData<T, P>
		where P : IPose<P> {

		public Constraint (D data, P pose, P constrained)
			: base(data, pose, constrained) {
		}

		public IConstraintData IData { get { return data; } }
		abstract public IConstraint Copy (Skeleton skeleton);
		abstract public void Sort (Skeleton skeleton);
		public virtual bool IsSourceActive { get { return true; } }
		abstract public void Update (Skeleton skeleton, Physics physics);
	}
}
