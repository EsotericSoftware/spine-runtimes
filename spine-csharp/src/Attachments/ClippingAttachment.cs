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
	public class ClippingAttachment : VertexAttachment {
		internal SlotData endSlot;
		internal bool convex, inverse;

		/// <summary>Clipping is performed between the clipping attachment's slot and the end slot. If null, clipping is done until the end of
		/// the skeleton's rendering.</summary>
		public SlotData EndSlot { get { return endSlot; } set { endSlot = value; } }

		/// <summary>
		/// When true the clipping polygon is treated as convex for more efficient clipping. If the polygon deforms to concave then the
		/// convex hull is used.When false the clipping polygon can be concave and if so has an additional CPU cost.Inverse clipping
		/// always uses convex.
		/// </summary>
		public bool Convex { get { return convex; } set { convex = value; } }

		/// <summary>
		/// When false, everything inside the clipping polygon is visible. When true, everything outside the clipping polygon is
		/// visible and clipping is convex.
		/// </summary>
		public bool Inverse { get { return inverse; } set { inverse = value; } }

		public ClippingAttachment (string name) : base(name) {
		}

		/// <summary>Copy constructor.</summary>
		protected ClippingAttachment (ClippingAttachment other)
			: base(other) {
			endSlot = other.endSlot;
			convex = other.convex;
			inverse = other.inverse;
		}

		public override Attachment Copy () {
			return new ClippingAttachment(this);
		}
	}
}
