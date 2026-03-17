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
	/// <summary>
	/// Stores a bone's local pose.
	/// </summary>
	public class BoneLocal : IPose<BoneLocal> {
		internal float x, y, rotation, scaleX, scaleY, shearX, shearY;
		internal Inherit inherit;

		public void Set (BoneLocal pose) {
			if (pose == null) throw new ArgumentNullException("pose", "pose cannot be null.");
			x = pose.x;
			y = pose.y;
			rotation = pose.rotation;
			scaleX = pose.scaleX;
			scaleY = pose.scaleY;
			shearX = pose.shearX;
			shearY = pose.shearY;
			inherit = pose.inherit;
		}

		/// <summary>The local X translation.</summary>
		public float X { get { return x; } set { x = value; } }
		/// <summary>The local Y translation.</summary>
		public float Y { get { return y; } set { y = value; } }
		/// <summary>The local rotation.</summary>
		public float Rotation { get { return rotation; } set { rotation = value; } }

		/// <summary>The local scaleX.</summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }

		/// <summary>The local scaleY.</summary>
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }

		/// <summary>The local shearX.</summary>
		public float ShearX { get { return shearX; } set { shearX = value; } }

		/// <summary>The local shearY.</summary>
		public float ShearY { get { return shearY; } set { shearY = value; } }

		/// <summary>Determines how parent world transforms affect this bone.</summary>
		public Inherit Inherit { get { return inherit; } set { inherit = value; } }
	}
}
