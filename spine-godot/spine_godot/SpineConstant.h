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

#pragma once

#include "SpineCommon.h"

class SpineConstant : public Object {
	GDCLASS(SpineConstant, Object);

protected:
	static void _bind_methods();

public:
	enum MixBlend {
		MixBlend_Setup = 0,
		MixBlend_First,
		MixBlend_Replace,
		MixBlend_Add
	};

	enum MixDirection {
		MixDirection_In = 0,
		MixDirection_Out
	};

	enum PropertyId {
		Property_Rotate = 1 << 0,
		Property_X = 1 << 1,
		Property_Y = 1 << 2,
		Property_ScaleX = 1 << 3,
		Property_ScaleY = 1 << 4,
		Property_ShearX = 1 << 5,
		Property_ShearY = 1 << 6,
		Property_Rgb = 1 << 7,
		Property_Alpha = 1 << 8,
		Property_Rgb2 = 1 << 9,
		Property_Attachment = 1 << 10,
		Property_Deform = 1 << 11,
		Property_Event = 1 << 12,
		Property_DrawOrder = 1 << 13,
		Property_IkConstraint = 1 << 14,
		Property_TransformConstraint = 1 << 15,
		Property_PathConstraintPosition = 1 << 16,
		Property_PathConstraintSpacing = 1 << 17,
		Property_PathConstraintMix = 1 << 18,
		Property_Sequence = 1 << 19
	};

	enum Inherit {
		Inherit_Normal = 0,
		Inherit_OnlyTranslation,
		Inherit_NoRotationOrReflection,
		Inherit_NoScale,
		Inherit_NoScaleOrReflection
	};

	enum PositionMode {
		PositionMode_Fixed = 0,
		PositionMode_Percent
	};

	enum SpacingMode {
		SpacingMode_Length = 0,
		SpacingMode_Fixed,
		SpacingMode_Percent
	};

	enum RotateMode {
		RotateMode_Tangent = 0,
		RotateMode_Chain,
		RotateMode_ChainScale
	};

	enum BlendMode {
		BlendMode_Normal = 0,
		BlendMode_Additive,
		BlendMode_Multiply,
		BlendMode_Screen
	};

	enum UpdateMode {
		UpdateMode_Process,
		UpdateMode_Physics,
		UpdateMode_Manual
	};

	enum BoneMode {
		BoneMode_Follow,
		BoneMode_Drive
	};

	enum Physics {
		Physics_None,
		Physics_Reset,
		Physics_Update,
		Physics_Pose
	};
};

VARIANT_ENUM_CAST(SpineConstant::MixBlend)
VARIANT_ENUM_CAST(SpineConstant::MixDirection)
VARIANT_ENUM_CAST(SpineConstant::PropertyId)
VARIANT_ENUM_CAST(SpineConstant::Inherit)
VARIANT_ENUM_CAST(SpineConstant::PositionMode)
VARIANT_ENUM_CAST(SpineConstant::SpacingMode)
VARIANT_ENUM_CAST(SpineConstant::RotateMode)
VARIANT_ENUM_CAST(SpineConstant::BlendMode)
VARIANT_ENUM_CAST(SpineConstant::UpdateMode)
VARIANT_ENUM_CAST(SpineConstant::BoneMode)
VARIANT_ENUM_CAST(SpineConstant::Physics)
