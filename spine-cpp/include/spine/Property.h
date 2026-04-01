/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#ifndef Spine_Property_h
#define Spine_Property_h

namespace spine {
	typedef long long PropertyId;
	enum Property {
		Property_Rotate = 0,
		Property_X,
		Property_Y,
		Property_ScaleX,
		Property_ScaleY,
		Property_ShearX,
		Property_ShearY,
		Property_Inherit,
		Property_Rgb,
		Property_Alpha,
		Property_Rgb2,
		Property_Attachment,
		Property_Deform,
		Property_Event,
		Property_DrawOrder,
		Property_IkConstraint,
		Property_TransformConstraint,
		Property_PathConstraintPosition,
		Property_PathConstraintSpacing,
		Property_PathConstraintMix,
		Property_PhysicsConstraintInertia,
		Property_PhysicsConstraintStrength,
		Property_PhysicsConstraintDamping,
		Property_PhysicsConstraintMass,
		Property_PhysicsConstraintWind,
		Property_PhysicsConstraintGravity,
		Property_PhysicsConstraintMix,
		Property_PhysicsConstraintReset,
		Property_Sequence,
		Property_SliderTime,
		Property_SliderMix,
		Property_DrawOrderFolder
	};
}

#endif /* Spine_Property_h */
