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
	using FromProperty = TransformConstraintData.FromProperty;

	/// <summary>
	/// Stores the setup pose for a <see cref="Slider"/>.
	/// </summary>
	public class SliderData : ConstraintData<Slider, SliderPose> {
		internal Animation animation;
		internal bool additive, loop;
		internal BoneData bone;
		internal FromProperty property;
		internal float offset, scale;
		internal bool local;

		public SliderData (string name)
			: base(name, new SliderPose()) {
		}

		override public IConstraint Create (Skeleton skeleton) {
			return new Slider(this, skeleton);
		}

		public Animation Animation { get { return animation; } set { animation = value; } }
		public bool Additive { get { return additive; } set { additive = value; } }
		public bool Loop { get { return loop; } set { loop = value; } }
		/// <summary>May be null.</summary>
		public BoneData Bone { get { return bone; } set { bone = value; } }
		/// <summary>May be null.</summary>
		public FromProperty Property { get { return property; } set { property = value; } }
		public float Offset { get { return offset; } set { offset = value; } }
		public float Scale { get { return scale; } set { scale = value; } }
		public bool Local { get { return local; } set { local = value; } }
	}
}
