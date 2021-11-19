/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
using System.Collections.Generic;

namespace Spine {
	public class PathAttachment : VertexAttachment {
		internal float[] lengths;
		internal bool closed, constantSpeed;

		/// <summary>The length in the setup pose from the start of the path to the end of each curve.</summary>
		public float[] Lengths { get { return lengths; } set { lengths = value; } }
		/// <summary>If true, the start and end knots are connected.</summary>
		public bool Closed { get { return closed; } set { closed = value; } }
		/// <summary>If true, additional calculations are performed to make computing positions along the path more accurate and movement along
		/// the path have a constant speed.</summary>
		public bool ConstantSpeed { get { return constantSpeed; } set { constantSpeed = value; } }

		public PathAttachment (String name)
			: base(name) {
		}

		/// <summary>Copy constructor.</summary>
		protected PathAttachment (PathAttachment other)
			: base(other) {

			lengths = new float[other.lengths.Length];
			Array.Copy(other.lengths, 0, lengths, 0, lengths.Length);

			closed = other.closed;
			constantSpeed = other.constantSpeed;
		}

		public override Attachment Copy () {
			return new PathAttachment(this);
		}
	}
}
