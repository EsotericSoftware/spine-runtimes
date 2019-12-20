/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
	public class TransformConstraintData : ConstraintData {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal BoneData target;
		internal float rotateMix, translateMix, scaleMix, shearMix;
		internal float offsetRotation, offsetX, offsetY, offsetScaleX, offsetScaleY, offsetShearY;
		internal bool relative, local;

		public ExposedList<BoneData> Bones { get { return bones; } }
		public BoneData Target { get { return target; } set { target = value; } }
		public float RotateMix { get { return rotateMix; } set { rotateMix = value; } }
		public float TranslateMix { get { return translateMix; } set { translateMix = value; } }
		public float ScaleMix { get { return scaleMix; } set { scaleMix = value; } }
		public float ShearMix { get { return shearMix; } set { shearMix = value; } }

		public float OffsetRotation { get { return offsetRotation; } set { offsetRotation = value; } }
		public float OffsetX { get { return offsetX; } set { offsetX = value; } }
		public float OffsetY { get { return offsetY; } set { offsetY = value; } }
		public float OffsetScaleX { get { return offsetScaleX; } set { offsetScaleX = value; } }
		public float OffsetScaleY { get { return offsetScaleY; } set { offsetScaleY = value; } }
		public float OffsetShearY { get { return offsetShearY; } set { offsetShearY = value; } }

		public bool Relative { get { return relative; } set { relative = value; } }
		public bool Local { get { return local; } set { local = value; } }

		public TransformConstraintData (string name) : base(name) {
		}
	}
}
