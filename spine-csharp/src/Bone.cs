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

	/// <summary>A node in a skeleton's hierarchy with a transform that affects its children and their attachments. A bone has a
	/// number of poses:
	/// <list type="bullet">
	/// <item><see cref="Data"/>: The setup pose.</item>
	/// <item><see cref="Pose"/>: The unconstrained local pose. Set by animations and application code.</item>
	/// <item><see cref="AppliedPose"/>: The local pose to use for rendering. Possibly modified by constraints.</item>
	/// <item>World transform: the local pose combined with the parent world transform. Computed on a pose by
	/// <see cref="BonePose.UpdateWorldTransform(Skeleton)"/> and <see cref="Skeleton.UpdateWorldTransform(Physics)"/>.</item>
	/// </list>
	/// </summary>
	public class Bone : PosedActive<BoneData, BonePose> {
		static public bool yDown;

		internal Bone parent;
		internal ExposedList<Bone> children = new ExposedList<Bone>(4);

		internal bool sorted;

		public Bone (BoneData data, Bone parent)
			: base(data, new BonePose(), new BonePose()) {
			this.parent = parent;
			appliedPose.bone = this;
			constrainedPose.bone = this;
		}

		/// <summary>
		/// Copy constructor. Does not copy the <see cref="Children"/> bones.
		/// </summary>
		public Bone (Bone bone, Bone parent)
			: this(bone.data, parent) {
			pose.Set(bone.pose);
		}

		public Bone Parent { get { return parent; } }
		public ExposedList<Bone> Children { get { return children; } }

	}
}
