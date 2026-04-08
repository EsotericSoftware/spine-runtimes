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
	public interface IPosedActive {
		bool Active { get; set; }
	}

	/// <summary>A posed object that may be active or inactive.</summary>
	public class PosedActive<D, P> : Posed<D, P>, IPosedActive
		where D : PosedData<P>
		where P : IPose<P> {

		internal bool active;

		protected PosedActive (D data, P pose, P constrained)
			: base(data, pose, constrained) {
			SetupPose();
		}

		/// <summary>Returns false when this constraint won't be updated by
		/// <see cref="Skeleton.UpdateWorldTransform(Physics)"/> because a skin is required and the
		/// <see cref="Skeleton.Skin">active skin</see> does not contain this item. See <see cref="Skin.Bones"/>,
		/// <see cref="Skin.Constraints"/>, <see cref="PosedData{P}.SkinRequired"/>, and <see cref="Skeleton.UpdateCache()"/>.</summary>
		public bool Active { get { return active; } set { active = value; } }
	}
}
