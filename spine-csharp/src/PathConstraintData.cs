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
	public class PathConstraintData : ConstraintData<PathConstraint, PathConstraintPose> {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal SlotData slot;
		internal PositionMode positionMode;
		internal SpacingMode spacingMode;
		internal RotateMode rotateMode;
		internal float offsetRotation;

		public PathConstraintData (string name)
			: base(name, new PathConstraintPose()) {
		}

		override public IConstraint Create (Skeleton skeleton) {
			return new PathConstraint(this, skeleton);
		}

		public ExposedList<BoneData> Bones { get { return bones; } }
		public SlotData Slot { get { return slot; } set { slot = value; } }
		public PositionMode PositionMode { get { return positionMode; } set { positionMode = value; } }
		public SpacingMode SpacingMode { get { return spacingMode; } set { spacingMode = value; } }
		public RotateMode RotateMode { get { return rotateMode; } set { rotateMode = value; } }
		public float OffsetRotation { get { return offsetRotation; } set { offsetRotation = value; } }
	}

	public enum PositionMode {
		Fixed, Percent
	}

	public enum SpacingMode {
		Length, Fixed, Percent, Proportional
	}

	public enum RotateMode {
		Tangent, Chain, ChainScale
	}
}
