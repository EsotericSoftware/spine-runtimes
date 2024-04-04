/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpineConstant.h"

void SpineConstant::_bind_methods() {
	BIND_ENUM_CONSTANT(MixBlend_Setup)
	BIND_ENUM_CONSTANT(MixBlend_First)
	BIND_ENUM_CONSTANT(MixBlend_Replace)
	BIND_ENUM_CONSTANT(MixBlend_Add)

	BIND_ENUM_CONSTANT(MixDirection_In)
	BIND_ENUM_CONSTANT(MixDirection_Out)

	BIND_ENUM_CONSTANT(Property_Rotate)
	BIND_ENUM_CONSTANT(Property_X)
	BIND_ENUM_CONSTANT(Property_Y)
	BIND_ENUM_CONSTANT(Property_ScaleX)
	BIND_ENUM_CONSTANT(Property_ScaleY)
	BIND_ENUM_CONSTANT(Property_ShearX)
	BIND_ENUM_CONSTANT(Property_ShearY)
	BIND_ENUM_CONSTANT(Property_Rgb)
	BIND_ENUM_CONSTANT(Property_Alpha)
	BIND_ENUM_CONSTANT(Property_Rgb2)
	BIND_ENUM_CONSTANT(Property_Attachment)
	BIND_ENUM_CONSTANT(Property_Deform)
	BIND_ENUM_CONSTANT(Property_Event)
	BIND_ENUM_CONSTANT(Property_DrawOrder)
	BIND_ENUM_CONSTANT(Property_IkConstraint)
	BIND_ENUM_CONSTANT(Property_TransformConstraint)
	BIND_ENUM_CONSTANT(Property_PathConstraintPosition)
	BIND_ENUM_CONSTANT(Property_PathConstraintSpacing)
	BIND_ENUM_CONSTANT(Property_PathConstraintMix)
	BIND_ENUM_CONSTANT(Property_Sequence)

	BIND_ENUM_CONSTANT(Inherit_Normal)
	BIND_ENUM_CONSTANT(Inherit_OnlyTranslation)
	BIND_ENUM_CONSTANT(Inherit_NoRotationOrReflection)
	BIND_ENUM_CONSTANT(Inherit_NoScale)
	BIND_ENUM_CONSTANT(Inherit_NoScaleOrReflection)

	BIND_ENUM_CONSTANT(PositionMode_Fixed)
	BIND_ENUM_CONSTANT(PositionMode_Percent)

	BIND_ENUM_CONSTANT(SpacingMode_Length)
	BIND_ENUM_CONSTANT(SpacingMode_Fixed)
	BIND_ENUM_CONSTANT(SpacingMode_Percent)

	BIND_ENUM_CONSTANT(RotateMode_Tangent)
	BIND_ENUM_CONSTANT(RotateMode_Chain)
	BIND_ENUM_CONSTANT(RotateMode_ChainScale)

	BIND_ENUM_CONSTANT(BlendMode_Normal)
	BIND_ENUM_CONSTANT(BlendMode_Additive)
	BIND_ENUM_CONSTANT(BlendMode_Multiply)
	BIND_ENUM_CONSTANT(BlendMode_Screen)

	BIND_ENUM_CONSTANT(UpdateMode_Process)
	BIND_ENUM_CONSTANT(UpdateMode_Physics)
	BIND_ENUM_CONSTANT(UpdateMode_Manual)

	BIND_ENUM_CONSTANT(BoneMode_Follow)
	BIND_ENUM_CONSTANT(BoneMode_Drive)

	BIND_ENUM_CONSTANT(Physics_None);
	BIND_ENUM_CONSTANT(Physics_Reset);
	BIND_ENUM_CONSTANT(Physics_Update);
	BIND_ENUM_CONSTANT(Physics_Pose);
}
