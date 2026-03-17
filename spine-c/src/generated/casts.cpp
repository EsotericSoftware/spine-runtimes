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

#include "casts.h"
#include <spine/spine.h>

using namespace spine;

// Upcast function implementations
spine_curve_timeline1 spine_alpha_timeline_cast_to_curve_timeline1(spine_alpha_timeline obj) {
	AlphaTimeline *derived = (AlphaTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_alpha_timeline_cast_to_curve_timeline(spine_alpha_timeline obj) {
	AlphaTimeline *derived = (AlphaTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_alpha_timeline_cast_to_timeline(spine_alpha_timeline obj) {
	AlphaTimeline *derived = (AlphaTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_alpha_timeline_cast_to_slot_timeline(spine_alpha_timeline obj) {
	AlphaTimeline *derived = (AlphaTimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_attachment_loader spine_atlas_attachment_loader_cast_to_attachment_loader(spine_atlas_attachment_loader obj) {
	AtlasAttachmentLoader *derived = (AtlasAttachmentLoader *) obj;
	AttachmentLoader *base = static_cast<AttachmentLoader *>(derived);
	return (spine_attachment_loader) base;
}

spine_texture_region spine_atlas_region_cast_to_texture_region(spine_atlas_region obj) {
	AtlasRegion *derived = (AtlasRegion *) obj;
	TextureRegion *base = static_cast<TextureRegion *>(derived);
	return (spine_texture_region) base;
}

spine_timeline spine_attachment_timeline_cast_to_timeline(spine_attachment_timeline obj) {
	AttachmentTimeline *derived = (AttachmentTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_attachment_timeline_cast_to_slot_timeline(spine_attachment_timeline obj) {
	AttachmentTimeline *derived = (AttachmentTimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_posed spine_bone_cast_to_posed(spine_bone obj) {
	Bone *derived = (Bone *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_bone_cast_to_posed_active(spine_bone obj) {
	Bone *derived = (Bone *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_update spine_bone_cast_to_update(spine_bone obj) {
	Bone *derived = (Bone *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed_data spine_bone_data_cast_to_posed_data(spine_bone_data obj) {
	BoneData *derived = (BoneData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_bone_local spine_bone_pose_cast_to_bone_local(spine_bone_pose obj) {
	BonePose *derived = (BonePose *) obj;
	BoneLocal *base = static_cast<BoneLocal *>(derived);
	return (spine_bone_local) base;
}

spine_update spine_bone_pose_cast_to_update(spine_bone_pose obj) {
	BonePose *derived = (BonePose *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_curve_timeline1 spine_bone_timeline1_cast_to_curve_timeline1(spine_bone_timeline1 obj) {
	BoneTimeline1 *derived = (BoneTimeline1 *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_bone_timeline1_cast_to_curve_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *derived = (BoneTimeline1 *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_bone_timeline1_cast_to_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *derived = (BoneTimeline1 *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_bone_timeline1_cast_to_bone_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *derived = (BoneTimeline1 *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_curve_timeline spine_bone_timeline2_cast_to_curve_timeline(spine_bone_timeline2 obj) {
	BoneTimeline2 *derived = (BoneTimeline2 *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_bone_timeline2_cast_to_timeline(spine_bone_timeline2 obj) {
	BoneTimeline2 *derived = (BoneTimeline2 *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_bone_timeline2_cast_to_bone_timeline(spine_bone_timeline2 obj) {
	BoneTimeline2 *derived = (BoneTimeline2 *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_vertex_attachment spine_bounding_box_attachment_cast_to_vertex_attachment(spine_bounding_box_attachment obj) {
	BoundingBoxAttachment *derived = (BoundingBoxAttachment *) obj;
	VertexAttachment *base = static_cast<VertexAttachment *>(derived);
	return (spine_vertex_attachment) base;
}

spine_attachment spine_bounding_box_attachment_cast_to_attachment(spine_bounding_box_attachment obj) {
	BoundingBoxAttachment *derived = (BoundingBoxAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

spine_vertex_attachment spine_clipping_attachment_cast_to_vertex_attachment(spine_clipping_attachment obj) {
	ClippingAttachment *derived = (ClippingAttachment *) obj;
	VertexAttachment *base = static_cast<VertexAttachment *>(derived);
	return (spine_vertex_attachment) base;
}

spine_attachment spine_clipping_attachment_cast_to_attachment(spine_clipping_attachment obj) {
	ClippingAttachment *derived = (ClippingAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

spine_update spine_constraint_cast_to_update(spine_constraint obj) {
	Constraint *derived = (Constraint *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_curve_timeline1 spine_constraint_timeline1_cast_to_curve_timeline1(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *derived = (ConstraintTimeline1 *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_constraint_timeline1_cast_to_curve_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *derived = (ConstraintTimeline1 *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_constraint_timeline1_cast_to_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *derived = (ConstraintTimeline1 *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_constraint_timeline1_cast_to_constraint_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *derived = (ConstraintTimeline1 *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_timeline spine_curve_timeline_cast_to_timeline(spine_curve_timeline obj) {
	CurveTimeline *derived = (CurveTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_curve_timeline spine_curve_timeline1_cast_to_curve_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *derived = (CurveTimeline1 *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_curve_timeline1_cast_to_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *derived = (CurveTimeline1 *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_curve_timeline spine_deform_timeline_cast_to_slot_curve_timeline(spine_deform_timeline obj) {
	DeformTimeline *derived = (DeformTimeline *) obj;
	SlotCurveTimeline *base = static_cast<SlotCurveTimeline *>(derived);
	return (spine_slot_curve_timeline) base;
}

spine_curve_timeline spine_deform_timeline_cast_to_curve_timeline(spine_deform_timeline obj) {
	DeformTimeline *derived = (DeformTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_deform_timeline_cast_to_timeline(spine_deform_timeline obj) {
	DeformTimeline *derived = (DeformTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_deform_timeline_cast_to_slot_timeline(spine_deform_timeline obj) {
	DeformTimeline *derived = (DeformTimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_timeline spine_draw_order_folder_timeline_cast_to_timeline(spine_draw_order_folder_timeline obj) {
	DrawOrderFolderTimeline *derived = (DrawOrderFolderTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_timeline spine_draw_order_timeline_cast_to_timeline(spine_draw_order_timeline obj) {
	DrawOrderTimeline *derived = (DrawOrderTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_timeline spine_event_timeline_cast_to_timeline(spine_event_timeline obj) {
	EventTimeline *derived = (EventTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_from_property spine_from_rotate_cast_to_from_property(spine_from_rotate obj) {
	FromRotate *derived = (FromRotate *) obj;
	FromProperty *base = static_cast<FromProperty *>(derived);
	return (spine_from_property) base;
}

spine_from_property spine_from_scale_x_cast_to_from_property(spine_from_scale_x obj) {
	FromScaleX *derived = (FromScaleX *) obj;
	FromProperty *base = static_cast<FromProperty *>(derived);
	return (spine_from_property) base;
}

spine_from_property spine_from_scale_y_cast_to_from_property(spine_from_scale_y obj) {
	FromScaleY *derived = (FromScaleY *) obj;
	FromProperty *base = static_cast<FromProperty *>(derived);
	return (spine_from_property) base;
}

spine_from_property spine_from_shear_y_cast_to_from_property(spine_from_shear_y obj) {
	FromShearY *derived = (FromShearY *) obj;
	FromProperty *base = static_cast<FromProperty *>(derived);
	return (spine_from_property) base;
}

spine_from_property spine_from_x_cast_to_from_property(spine_from_x obj) {
	FromX *derived = (FromX *) obj;
	FromProperty *base = static_cast<FromProperty *>(derived);
	return (spine_from_property) base;
}

spine_from_property spine_from_y_cast_to_from_property(spine_from_y obj) {
	FromY *derived = (FromY *) obj;
	FromProperty *base = static_cast<FromProperty *>(derived);
	return (spine_from_property) base;
}

spine_ik_constraint_base spine_ik_constraint_cast_to_ik_constraint_base(spine_ik_constraint obj) {
	IkConstraint *derived = (IkConstraint *) obj;
	IkConstraintBase *base = static_cast<IkConstraintBase *>(derived);
	return (spine_ik_constraint_base) base;
}

spine_posed spine_ik_constraint_cast_to_posed(spine_ik_constraint obj) {
	IkConstraint *derived = (IkConstraint *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_ik_constraint_cast_to_posed_active(spine_ik_constraint obj) {
	IkConstraint *derived = (IkConstraint *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_ik_constraint_cast_to_constraint(spine_ik_constraint obj) {
	IkConstraint *derived = (IkConstraint *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_ik_constraint_cast_to_update(spine_ik_constraint obj) {
	IkConstraint *derived = (IkConstraint *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed spine_ik_constraint_base_cast_to_posed(spine_ik_constraint_base obj) {
	IkConstraintBase *derived = (IkConstraintBase *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_ik_constraint_base_cast_to_posed_active(spine_ik_constraint_base obj) {
	IkConstraintBase *derived = (IkConstraintBase *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_ik_constraint_base_cast_to_constraint(spine_ik_constraint_base obj) {
	IkConstraintBase *derived = (IkConstraintBase *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_ik_constraint_base_cast_to_update(spine_ik_constraint_base obj) {
	IkConstraintBase *derived = (IkConstraintBase *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed_data spine_ik_constraint_data_cast_to_posed_data(spine_ik_constraint_data obj) {
	IkConstraintData *derived = (IkConstraintData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_constraint_data spine_ik_constraint_data_cast_to_constraint_data(spine_ik_constraint_data obj) {
	IkConstraintData *derived = (IkConstraintData *) obj;
	ConstraintData *base = static_cast<ConstraintData *>(derived);
	return (spine_constraint_data) base;
}

spine_curve_timeline spine_ik_constraint_timeline_cast_to_curve_timeline(spine_ik_constraint_timeline obj) {
	IkConstraintTimeline *derived = (IkConstraintTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_ik_constraint_timeline_cast_to_timeline(spine_ik_constraint_timeline obj) {
	IkConstraintTimeline *derived = (IkConstraintTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_ik_constraint_timeline_cast_to_constraint_timeline(spine_ik_constraint_timeline obj) {
	IkConstraintTimeline *derived = (IkConstraintTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_timeline spine_inherit_timeline_cast_to_timeline(spine_inherit_timeline obj) {
	InheritTimeline *derived = (InheritTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_inherit_timeline_cast_to_bone_timeline(spine_inherit_timeline obj) {
	InheritTimeline *derived = (InheritTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_vertex_attachment spine_mesh_attachment_cast_to_vertex_attachment(spine_mesh_attachment obj) {
	MeshAttachment *derived = (MeshAttachment *) obj;
	VertexAttachment *base = static_cast<VertexAttachment *>(derived);
	return (spine_vertex_attachment) base;
}

spine_attachment spine_mesh_attachment_cast_to_attachment(spine_mesh_attachment obj) {
	MeshAttachment *derived = (MeshAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

spine_vertex_attachment spine_path_attachment_cast_to_vertex_attachment(spine_path_attachment obj) {
	PathAttachment *derived = (PathAttachment *) obj;
	VertexAttachment *base = static_cast<VertexAttachment *>(derived);
	return (spine_vertex_attachment) base;
}

spine_attachment spine_path_attachment_cast_to_attachment(spine_path_attachment obj) {
	PathAttachment *derived = (PathAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

spine_path_constraint_base spine_path_constraint_cast_to_path_constraint_base(spine_path_constraint obj) {
	PathConstraint *derived = (PathConstraint *) obj;
	PathConstraintBase *base = static_cast<PathConstraintBase *>(derived);
	return (spine_path_constraint_base) base;
}

spine_posed spine_path_constraint_cast_to_posed(spine_path_constraint obj) {
	PathConstraint *derived = (PathConstraint *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_path_constraint_cast_to_posed_active(spine_path_constraint obj) {
	PathConstraint *derived = (PathConstraint *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_path_constraint_cast_to_constraint(spine_path_constraint obj) {
	PathConstraint *derived = (PathConstraint *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_path_constraint_cast_to_update(spine_path_constraint obj) {
	PathConstraint *derived = (PathConstraint *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed spine_path_constraint_base_cast_to_posed(spine_path_constraint_base obj) {
	PathConstraintBase *derived = (PathConstraintBase *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_path_constraint_base_cast_to_posed_active(spine_path_constraint_base obj) {
	PathConstraintBase *derived = (PathConstraintBase *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_path_constraint_base_cast_to_constraint(spine_path_constraint_base obj) {
	PathConstraintBase *derived = (PathConstraintBase *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_path_constraint_base_cast_to_update(spine_path_constraint_base obj) {
	PathConstraintBase *derived = (PathConstraintBase *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed_data spine_path_constraint_data_cast_to_posed_data(spine_path_constraint_data obj) {
	PathConstraintData *derived = (PathConstraintData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_constraint_data spine_path_constraint_data_cast_to_constraint_data(spine_path_constraint_data obj) {
	PathConstraintData *derived = (PathConstraintData *) obj;
	ConstraintData *base = static_cast<ConstraintData *>(derived);
	return (spine_constraint_data) base;
}

spine_curve_timeline spine_path_constraint_mix_timeline_cast_to_curve_timeline(spine_path_constraint_mix_timeline obj) {
	PathConstraintMixTimeline *derived = (PathConstraintMixTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_path_constraint_mix_timeline_cast_to_timeline(spine_path_constraint_mix_timeline obj) {
	PathConstraintMixTimeline *derived = (PathConstraintMixTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_path_constraint_mix_timeline_cast_to_constraint_timeline(spine_path_constraint_mix_timeline obj) {
	PathConstraintMixTimeline *derived = (PathConstraintMixTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_constraint_timeline1 spine_path_constraint_position_timeline_cast_to_constraint_timeline1(spine_path_constraint_position_timeline obj) {
	PathConstraintPositionTimeline *derived = (PathConstraintPositionTimeline *) obj;
	ConstraintTimeline1 *base = static_cast<ConstraintTimeline1 *>(derived);
	return (spine_constraint_timeline1) base;
}

spine_curve_timeline1 spine_path_constraint_position_timeline_cast_to_curve_timeline1(spine_path_constraint_position_timeline obj) {
	PathConstraintPositionTimeline *derived = (PathConstraintPositionTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_path_constraint_position_timeline_cast_to_curve_timeline(spine_path_constraint_position_timeline obj) {
	PathConstraintPositionTimeline *derived = (PathConstraintPositionTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_path_constraint_position_timeline_cast_to_timeline(spine_path_constraint_position_timeline obj) {
	PathConstraintPositionTimeline *derived = (PathConstraintPositionTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_path_constraint_position_timeline_cast_to_constraint_timeline(spine_path_constraint_position_timeline obj) {
	PathConstraintPositionTimeline *derived = (PathConstraintPositionTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_constraint_timeline1 spine_path_constraint_spacing_timeline_cast_to_constraint_timeline1(spine_path_constraint_spacing_timeline obj) {
	PathConstraintSpacingTimeline *derived = (PathConstraintSpacingTimeline *) obj;
	ConstraintTimeline1 *base = static_cast<ConstraintTimeline1 *>(derived);
	return (spine_constraint_timeline1) base;
}

spine_curve_timeline1 spine_path_constraint_spacing_timeline_cast_to_curve_timeline1(spine_path_constraint_spacing_timeline obj) {
	PathConstraintSpacingTimeline *derived = (PathConstraintSpacingTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_path_constraint_spacing_timeline_cast_to_curve_timeline(spine_path_constraint_spacing_timeline obj) {
	PathConstraintSpacingTimeline *derived = (PathConstraintSpacingTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_path_constraint_spacing_timeline_cast_to_timeline(spine_path_constraint_spacing_timeline obj) {
	PathConstraintSpacingTimeline *derived = (PathConstraintSpacingTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_path_constraint_spacing_timeline_cast_to_constraint_timeline(spine_path_constraint_spacing_timeline obj) {
	PathConstraintSpacingTimeline *derived = (PathConstraintSpacingTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_physics_constraint_base spine_physics_constraint_cast_to_physics_constraint_base(spine_physics_constraint obj) {
	PhysicsConstraint *derived = (PhysicsConstraint *) obj;
	PhysicsConstraintBase *base = static_cast<PhysicsConstraintBase *>(derived);
	return (spine_physics_constraint_base) base;
}

spine_posed spine_physics_constraint_cast_to_posed(spine_physics_constraint obj) {
	PhysicsConstraint *derived = (PhysicsConstraint *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_physics_constraint_cast_to_posed_active(spine_physics_constraint obj) {
	PhysicsConstraint *derived = (PhysicsConstraint *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_physics_constraint_cast_to_constraint(spine_physics_constraint obj) {
	PhysicsConstraint *derived = (PhysicsConstraint *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_physics_constraint_cast_to_update(spine_physics_constraint obj) {
	PhysicsConstraint *derived = (PhysicsConstraint *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed spine_physics_constraint_base_cast_to_posed(spine_physics_constraint_base obj) {
	PhysicsConstraintBase *derived = (PhysicsConstraintBase *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_physics_constraint_base_cast_to_posed_active(spine_physics_constraint_base obj) {
	PhysicsConstraintBase *derived = (PhysicsConstraintBase *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_physics_constraint_base_cast_to_constraint(spine_physics_constraint_base obj) {
	PhysicsConstraintBase *derived = (PhysicsConstraintBase *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_physics_constraint_base_cast_to_update(spine_physics_constraint_base obj) {
	PhysicsConstraintBase *derived = (PhysicsConstraintBase *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_physics_constraint_timeline spine_physics_constraint_damping_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_damping_timeline obj) {
	PhysicsConstraintDampingTimeline *derived = (PhysicsConstraintDampingTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_damping_timeline_cast_to_curve_timeline1(spine_physics_constraint_damping_timeline obj) {
	PhysicsConstraintDampingTimeline *derived = (PhysicsConstraintDampingTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_damping_timeline_cast_to_curve_timeline(spine_physics_constraint_damping_timeline obj) {
	PhysicsConstraintDampingTimeline *derived = (PhysicsConstraintDampingTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_damping_timeline_cast_to_timeline(spine_physics_constraint_damping_timeline obj) {
	PhysicsConstraintDampingTimeline *derived = (PhysicsConstraintDampingTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_damping_timeline_cast_to_constraint_timeline(spine_physics_constraint_damping_timeline obj) {
	PhysicsConstraintDampingTimeline *derived = (PhysicsConstraintDampingTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_posed_data spine_physics_constraint_data_cast_to_posed_data(spine_physics_constraint_data obj) {
	PhysicsConstraintData *derived = (PhysicsConstraintData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_constraint_data spine_physics_constraint_data_cast_to_constraint_data(spine_physics_constraint_data obj) {
	PhysicsConstraintData *derived = (PhysicsConstraintData *) obj;
	ConstraintData *base = static_cast<ConstraintData *>(derived);
	return (spine_constraint_data) base;
}

spine_physics_constraint_timeline spine_physics_constraint_gravity_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_gravity_timeline obj) {
	PhysicsConstraintGravityTimeline *derived = (PhysicsConstraintGravityTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_gravity_timeline_cast_to_curve_timeline1(spine_physics_constraint_gravity_timeline obj) {
	PhysicsConstraintGravityTimeline *derived = (PhysicsConstraintGravityTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_gravity_timeline_cast_to_curve_timeline(spine_physics_constraint_gravity_timeline obj) {
	PhysicsConstraintGravityTimeline *derived = (PhysicsConstraintGravityTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_gravity_timeline_cast_to_timeline(spine_physics_constraint_gravity_timeline obj) {
	PhysicsConstraintGravityTimeline *derived = (PhysicsConstraintGravityTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_gravity_timeline_cast_to_constraint_timeline(spine_physics_constraint_gravity_timeline obj) {
	PhysicsConstraintGravityTimeline *derived = (PhysicsConstraintGravityTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_physics_constraint_timeline spine_physics_constraint_inertia_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_inertia_timeline obj) {
	PhysicsConstraintInertiaTimeline *derived = (PhysicsConstraintInertiaTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_inertia_timeline_cast_to_curve_timeline1(spine_physics_constraint_inertia_timeline obj) {
	PhysicsConstraintInertiaTimeline *derived = (PhysicsConstraintInertiaTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_inertia_timeline_cast_to_curve_timeline(spine_physics_constraint_inertia_timeline obj) {
	PhysicsConstraintInertiaTimeline *derived = (PhysicsConstraintInertiaTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_inertia_timeline_cast_to_timeline(spine_physics_constraint_inertia_timeline obj) {
	PhysicsConstraintInertiaTimeline *derived = (PhysicsConstraintInertiaTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_inertia_timeline_cast_to_constraint_timeline(spine_physics_constraint_inertia_timeline obj) {
	PhysicsConstraintInertiaTimeline *derived = (PhysicsConstraintInertiaTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_physics_constraint_timeline spine_physics_constraint_mass_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_mass_timeline obj) {
	PhysicsConstraintMassTimeline *derived = (PhysicsConstraintMassTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_mass_timeline_cast_to_curve_timeline1(spine_physics_constraint_mass_timeline obj) {
	PhysicsConstraintMassTimeline *derived = (PhysicsConstraintMassTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_mass_timeline_cast_to_curve_timeline(spine_physics_constraint_mass_timeline obj) {
	PhysicsConstraintMassTimeline *derived = (PhysicsConstraintMassTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_mass_timeline_cast_to_timeline(spine_physics_constraint_mass_timeline obj) {
	PhysicsConstraintMassTimeline *derived = (PhysicsConstraintMassTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_mass_timeline_cast_to_constraint_timeline(spine_physics_constraint_mass_timeline obj) {
	PhysicsConstraintMassTimeline *derived = (PhysicsConstraintMassTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_physics_constraint_timeline spine_physics_constraint_mix_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_mix_timeline obj) {
	PhysicsConstraintMixTimeline *derived = (PhysicsConstraintMixTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_mix_timeline_cast_to_curve_timeline1(spine_physics_constraint_mix_timeline obj) {
	PhysicsConstraintMixTimeline *derived = (PhysicsConstraintMixTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_mix_timeline_cast_to_curve_timeline(spine_physics_constraint_mix_timeline obj) {
	PhysicsConstraintMixTimeline *derived = (PhysicsConstraintMixTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_mix_timeline_cast_to_timeline(spine_physics_constraint_mix_timeline obj) {
	PhysicsConstraintMixTimeline *derived = (PhysicsConstraintMixTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_mix_timeline_cast_to_constraint_timeline(spine_physics_constraint_mix_timeline obj) {
	PhysicsConstraintMixTimeline *derived = (PhysicsConstraintMixTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_timeline spine_physics_constraint_reset_timeline_cast_to_timeline(spine_physics_constraint_reset_timeline obj) {
	PhysicsConstraintResetTimeline *derived = (PhysicsConstraintResetTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_reset_timeline_cast_to_constraint_timeline(spine_physics_constraint_reset_timeline obj) {
	PhysicsConstraintResetTimeline *derived = (PhysicsConstraintResetTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_physics_constraint_timeline spine_physics_constraint_strength_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_strength_timeline obj) {
	PhysicsConstraintStrengthTimeline *derived = (PhysicsConstraintStrengthTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_strength_timeline_cast_to_curve_timeline1(spine_physics_constraint_strength_timeline obj) {
	PhysicsConstraintStrengthTimeline *derived = (PhysicsConstraintStrengthTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_strength_timeline_cast_to_curve_timeline(spine_physics_constraint_strength_timeline obj) {
	PhysicsConstraintStrengthTimeline *derived = (PhysicsConstraintStrengthTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_strength_timeline_cast_to_timeline(spine_physics_constraint_strength_timeline obj) {
	PhysicsConstraintStrengthTimeline *derived = (PhysicsConstraintStrengthTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_strength_timeline_cast_to_constraint_timeline(spine_physics_constraint_strength_timeline obj) {
	PhysicsConstraintStrengthTimeline *derived = (PhysicsConstraintStrengthTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_timeline_cast_to_curve_timeline1(spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *derived = (PhysicsConstraintTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_timeline_cast_to_curve_timeline(spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *derived = (PhysicsConstraintTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_timeline_cast_to_timeline(spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *derived = (PhysicsConstraintTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_timeline_cast_to_constraint_timeline(spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *derived = (PhysicsConstraintTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_physics_constraint_timeline spine_physics_constraint_wind_timeline_cast_to_physics_constraint_timeline(
	spine_physics_constraint_wind_timeline obj) {
	PhysicsConstraintWindTimeline *derived = (PhysicsConstraintWindTimeline *) obj;
	PhysicsConstraintTimeline *base = static_cast<PhysicsConstraintTimeline *>(derived);
	return (spine_physics_constraint_timeline) base;
}

spine_curve_timeline1 spine_physics_constraint_wind_timeline_cast_to_curve_timeline1(spine_physics_constraint_wind_timeline obj) {
	PhysicsConstraintWindTimeline *derived = (PhysicsConstraintWindTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_physics_constraint_wind_timeline_cast_to_curve_timeline(spine_physics_constraint_wind_timeline obj) {
	PhysicsConstraintWindTimeline *derived = (PhysicsConstraintWindTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_physics_constraint_wind_timeline_cast_to_timeline(spine_physics_constraint_wind_timeline obj) {
	PhysicsConstraintWindTimeline *derived = (PhysicsConstraintWindTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_physics_constraint_wind_timeline_cast_to_constraint_timeline(spine_physics_constraint_wind_timeline obj) {
	PhysicsConstraintWindTimeline *derived = (PhysicsConstraintWindTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_attachment spine_point_attachment_cast_to_attachment(spine_point_attachment obj) {
	PointAttachment *derived = (PointAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

spine_attachment spine_region_attachment_cast_to_attachment(spine_region_attachment obj) {
	RegionAttachment *derived = (RegionAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

spine_slot_curve_timeline spine_rgb2_timeline_cast_to_slot_curve_timeline(spine_rgb2_timeline obj) {
	RGB2Timeline *derived = (RGB2Timeline *) obj;
	SlotCurveTimeline *base = static_cast<SlotCurveTimeline *>(derived);
	return (spine_slot_curve_timeline) base;
}

spine_curve_timeline spine_rgb2_timeline_cast_to_curve_timeline(spine_rgb2_timeline obj) {
	RGB2Timeline *derived = (RGB2Timeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_rgb2_timeline_cast_to_timeline(spine_rgb2_timeline obj) {
	RGB2Timeline *derived = (RGB2Timeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_rgb2_timeline_cast_to_slot_timeline(spine_rgb2_timeline obj) {
	RGB2Timeline *derived = (RGB2Timeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_slot_curve_timeline spine_rgba2_timeline_cast_to_slot_curve_timeline(spine_rgba2_timeline obj) {
	RGBA2Timeline *derived = (RGBA2Timeline *) obj;
	SlotCurveTimeline *base = static_cast<SlotCurveTimeline *>(derived);
	return (spine_slot_curve_timeline) base;
}

spine_curve_timeline spine_rgba2_timeline_cast_to_curve_timeline(spine_rgba2_timeline obj) {
	RGBA2Timeline *derived = (RGBA2Timeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_rgba2_timeline_cast_to_timeline(spine_rgba2_timeline obj) {
	RGBA2Timeline *derived = (RGBA2Timeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_rgba2_timeline_cast_to_slot_timeline(spine_rgba2_timeline obj) {
	RGBA2Timeline *derived = (RGBA2Timeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_slot_curve_timeline spine_rgba_timeline_cast_to_slot_curve_timeline(spine_rgba_timeline obj) {
	RGBATimeline *derived = (RGBATimeline *) obj;
	SlotCurveTimeline *base = static_cast<SlotCurveTimeline *>(derived);
	return (spine_slot_curve_timeline) base;
}

spine_curve_timeline spine_rgba_timeline_cast_to_curve_timeline(spine_rgba_timeline obj) {
	RGBATimeline *derived = (RGBATimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_rgba_timeline_cast_to_timeline(spine_rgba_timeline obj) {
	RGBATimeline *derived = (RGBATimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_rgba_timeline_cast_to_slot_timeline(spine_rgba_timeline obj) {
	RGBATimeline *derived = (RGBATimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_slot_curve_timeline spine_rgb_timeline_cast_to_slot_curve_timeline(spine_rgb_timeline obj) {
	RGBTimeline *derived = (RGBTimeline *) obj;
	SlotCurveTimeline *base = static_cast<SlotCurveTimeline *>(derived);
	return (spine_slot_curve_timeline) base;
}

spine_curve_timeline spine_rgb_timeline_cast_to_curve_timeline(spine_rgb_timeline obj) {
	RGBTimeline *derived = (RGBTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_rgb_timeline_cast_to_timeline(spine_rgb_timeline obj) {
	RGBTimeline *derived = (RGBTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_rgb_timeline_cast_to_slot_timeline(spine_rgb_timeline obj) {
	RGBTimeline *derived = (RGBTimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_bone_timeline1 spine_rotate_timeline_cast_to_bone_timeline1(spine_rotate_timeline obj) {
	RotateTimeline *derived = (RotateTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_rotate_timeline_cast_to_curve_timeline1(spine_rotate_timeline obj) {
	RotateTimeline *derived = (RotateTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_rotate_timeline_cast_to_curve_timeline(spine_rotate_timeline obj) {
	RotateTimeline *derived = (RotateTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_rotate_timeline_cast_to_timeline(spine_rotate_timeline obj) {
	RotateTimeline *derived = (RotateTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_rotate_timeline_cast_to_bone_timeline(spine_rotate_timeline obj) {
	RotateTimeline *derived = (RotateTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline2 spine_scale_timeline_cast_to_bone_timeline2(spine_scale_timeline obj) {
	ScaleTimeline *derived = (ScaleTimeline *) obj;
	BoneTimeline2 *base = static_cast<BoneTimeline2 *>(derived);
	return (spine_bone_timeline2) base;
}

spine_curve_timeline spine_scale_timeline_cast_to_curve_timeline(spine_scale_timeline obj) {
	ScaleTimeline *derived = (ScaleTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_scale_timeline_cast_to_timeline(spine_scale_timeline obj) {
	ScaleTimeline *derived = (ScaleTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_scale_timeline_cast_to_bone_timeline(spine_scale_timeline obj) {
	ScaleTimeline *derived = (ScaleTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline1 spine_scale_x_timeline_cast_to_bone_timeline1(spine_scale_x_timeline obj) {
	ScaleXTimeline *derived = (ScaleXTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_scale_x_timeline_cast_to_curve_timeline1(spine_scale_x_timeline obj) {
	ScaleXTimeline *derived = (ScaleXTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_scale_x_timeline_cast_to_curve_timeline(spine_scale_x_timeline obj) {
	ScaleXTimeline *derived = (ScaleXTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_scale_x_timeline_cast_to_timeline(spine_scale_x_timeline obj) {
	ScaleXTimeline *derived = (ScaleXTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_scale_x_timeline_cast_to_bone_timeline(spine_scale_x_timeline obj) {
	ScaleXTimeline *derived = (ScaleXTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline1 spine_scale_y_timeline_cast_to_bone_timeline1(spine_scale_y_timeline obj) {
	ScaleYTimeline *derived = (ScaleYTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_scale_y_timeline_cast_to_curve_timeline1(spine_scale_y_timeline obj) {
	ScaleYTimeline *derived = (ScaleYTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_scale_y_timeline_cast_to_curve_timeline(spine_scale_y_timeline obj) {
	ScaleYTimeline *derived = (ScaleYTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_scale_y_timeline_cast_to_timeline(spine_scale_y_timeline obj) {
	ScaleYTimeline *derived = (ScaleYTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_scale_y_timeline_cast_to_bone_timeline(spine_scale_y_timeline obj) {
	ScaleYTimeline *derived = (ScaleYTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_timeline spine_sequence_timeline_cast_to_timeline(spine_sequence_timeline obj) {
	SequenceTimeline *derived = (SequenceTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_sequence_timeline_cast_to_slot_timeline(spine_sequence_timeline obj) {
	SequenceTimeline *derived = (SequenceTimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_bone_timeline2 spine_shear_timeline_cast_to_bone_timeline2(spine_shear_timeline obj) {
	ShearTimeline *derived = (ShearTimeline *) obj;
	BoneTimeline2 *base = static_cast<BoneTimeline2 *>(derived);
	return (spine_bone_timeline2) base;
}

spine_curve_timeline spine_shear_timeline_cast_to_curve_timeline(spine_shear_timeline obj) {
	ShearTimeline *derived = (ShearTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_shear_timeline_cast_to_timeline(spine_shear_timeline obj) {
	ShearTimeline *derived = (ShearTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_shear_timeline_cast_to_bone_timeline(spine_shear_timeline obj) {
	ShearTimeline *derived = (ShearTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline1 spine_shear_x_timeline_cast_to_bone_timeline1(spine_shear_x_timeline obj) {
	ShearXTimeline *derived = (ShearXTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_shear_x_timeline_cast_to_curve_timeline1(spine_shear_x_timeline obj) {
	ShearXTimeline *derived = (ShearXTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_shear_x_timeline_cast_to_curve_timeline(spine_shear_x_timeline obj) {
	ShearXTimeline *derived = (ShearXTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_shear_x_timeline_cast_to_timeline(spine_shear_x_timeline obj) {
	ShearXTimeline *derived = (ShearXTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_shear_x_timeline_cast_to_bone_timeline(spine_shear_x_timeline obj) {
	ShearXTimeline *derived = (ShearXTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline1 spine_shear_y_timeline_cast_to_bone_timeline1(spine_shear_y_timeline obj) {
	ShearYTimeline *derived = (ShearYTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_shear_y_timeline_cast_to_curve_timeline1(spine_shear_y_timeline obj) {
	ShearYTimeline *derived = (ShearYTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_shear_y_timeline_cast_to_curve_timeline(spine_shear_y_timeline obj) {
	ShearYTimeline *derived = (ShearYTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_shear_y_timeline_cast_to_timeline(spine_shear_y_timeline obj) {
	ShearYTimeline *derived = (ShearYTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_shear_y_timeline_cast_to_bone_timeline(spine_shear_y_timeline obj) {
	ShearYTimeline *derived = (ShearYTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_slider_base spine_slider_cast_to_slider_base(spine_slider obj) {
	Slider *derived = (Slider *) obj;
	SliderBase *base = static_cast<SliderBase *>(derived);
	return (spine_slider_base) base;
}

spine_posed spine_slider_cast_to_posed(spine_slider obj) {
	Slider *derived = (Slider *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_slider_cast_to_posed_active(spine_slider obj) {
	Slider *derived = (Slider *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_slider_cast_to_constraint(spine_slider obj) {
	Slider *derived = (Slider *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_slider_cast_to_update(spine_slider obj) {
	Slider *derived = (Slider *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed spine_slider_base_cast_to_posed(spine_slider_base obj) {
	SliderBase *derived = (SliderBase *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_slider_base_cast_to_posed_active(spine_slider_base obj) {
	SliderBase *derived = (SliderBase *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_slider_base_cast_to_constraint(spine_slider_base obj) {
	SliderBase *derived = (SliderBase *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_slider_base_cast_to_update(spine_slider_base obj) {
	SliderBase *derived = (SliderBase *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed_data spine_slider_data_cast_to_posed_data(spine_slider_data obj) {
	SliderData *derived = (SliderData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_constraint_data spine_slider_data_cast_to_constraint_data(spine_slider_data obj) {
	SliderData *derived = (SliderData *) obj;
	ConstraintData *base = static_cast<ConstraintData *>(derived);
	return (spine_constraint_data) base;
}

spine_constraint_timeline1 spine_slider_mix_timeline_cast_to_constraint_timeline1(spine_slider_mix_timeline obj) {
	SliderMixTimeline *derived = (SliderMixTimeline *) obj;
	ConstraintTimeline1 *base = static_cast<ConstraintTimeline1 *>(derived);
	return (spine_constraint_timeline1) base;
}

spine_curve_timeline1 spine_slider_mix_timeline_cast_to_curve_timeline1(spine_slider_mix_timeline obj) {
	SliderMixTimeline *derived = (SliderMixTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_slider_mix_timeline_cast_to_curve_timeline(spine_slider_mix_timeline obj) {
	SliderMixTimeline *derived = (SliderMixTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_slider_mix_timeline_cast_to_timeline(spine_slider_mix_timeline obj) {
	SliderMixTimeline *derived = (SliderMixTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_slider_mix_timeline_cast_to_constraint_timeline(spine_slider_mix_timeline obj) {
	SliderMixTimeline *derived = (SliderMixTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_constraint_timeline1 spine_slider_timeline_cast_to_constraint_timeline1(spine_slider_timeline obj) {
	SliderTimeline *derived = (SliderTimeline *) obj;
	ConstraintTimeline1 *base = static_cast<ConstraintTimeline1 *>(derived);
	return (spine_constraint_timeline1) base;
}

spine_curve_timeline1 spine_slider_timeline_cast_to_curve_timeline1(spine_slider_timeline obj) {
	SliderTimeline *derived = (SliderTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_slider_timeline_cast_to_curve_timeline(spine_slider_timeline obj) {
	SliderTimeline *derived = (SliderTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_slider_timeline_cast_to_timeline(spine_slider_timeline obj) {
	SliderTimeline *derived = (SliderTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_slider_timeline_cast_to_constraint_timeline(spine_slider_timeline obj) {
	SliderTimeline *derived = (SliderTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_posed spine_slot_cast_to_posed(spine_slot obj) {
	Slot *derived = (Slot *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_curve_timeline spine_slot_curve_timeline_cast_to_curve_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *derived = (SlotCurveTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_slot_curve_timeline_cast_to_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *derived = (SlotCurveTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_slot_timeline spine_slot_curve_timeline_cast_to_slot_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *derived = (SlotCurveTimeline *) obj;
	SlotTimeline *base = static_cast<SlotTimeline *>(derived);
	return (spine_slot_timeline) base;
}

spine_posed_data spine_slot_data_cast_to_posed_data(spine_slot_data obj) {
	SlotData *derived = (SlotData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_to_property spine_to_rotate_cast_to_to_property(spine_to_rotate obj) {
	ToRotate *derived = (ToRotate *) obj;
	ToProperty *base = static_cast<ToProperty *>(derived);
	return (spine_to_property) base;
}

spine_to_property spine_to_scale_x_cast_to_to_property(spine_to_scale_x obj) {
	ToScaleX *derived = (ToScaleX *) obj;
	ToProperty *base = static_cast<ToProperty *>(derived);
	return (spine_to_property) base;
}

spine_to_property spine_to_scale_y_cast_to_to_property(spine_to_scale_y obj) {
	ToScaleY *derived = (ToScaleY *) obj;
	ToProperty *base = static_cast<ToProperty *>(derived);
	return (spine_to_property) base;
}

spine_to_property spine_to_shear_y_cast_to_to_property(spine_to_shear_y obj) {
	ToShearY *derived = (ToShearY *) obj;
	ToProperty *base = static_cast<ToProperty *>(derived);
	return (spine_to_property) base;
}

spine_to_property spine_to_x_cast_to_to_property(spine_to_x obj) {
	ToX *derived = (ToX *) obj;
	ToProperty *base = static_cast<ToProperty *>(derived);
	return (spine_to_property) base;
}

spine_to_property spine_to_y_cast_to_to_property(spine_to_y obj) {
	ToY *derived = (ToY *) obj;
	ToProperty *base = static_cast<ToProperty *>(derived);
	return (spine_to_property) base;
}

spine_transform_constraint_base spine_transform_constraint_cast_to_transform_constraint_base(spine_transform_constraint obj) {
	TransformConstraint *derived = (TransformConstraint *) obj;
	TransformConstraintBase *base = static_cast<TransformConstraintBase *>(derived);
	return (spine_transform_constraint_base) base;
}

spine_posed spine_transform_constraint_cast_to_posed(spine_transform_constraint obj) {
	TransformConstraint *derived = (TransformConstraint *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_transform_constraint_cast_to_posed_active(spine_transform_constraint obj) {
	TransformConstraint *derived = (TransformConstraint *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_transform_constraint_cast_to_constraint(spine_transform_constraint obj) {
	TransformConstraint *derived = (TransformConstraint *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_transform_constraint_cast_to_update(spine_transform_constraint obj) {
	TransformConstraint *derived = (TransformConstraint *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed spine_transform_constraint_base_cast_to_posed(spine_transform_constraint_base obj) {
	TransformConstraintBase *derived = (TransformConstraintBase *) obj;
	Posed *base = static_cast<Posed *>(derived);
	return (spine_posed) base;
}

spine_posed_active spine_transform_constraint_base_cast_to_posed_active(spine_transform_constraint_base obj) {
	TransformConstraintBase *derived = (TransformConstraintBase *) obj;
	PosedActive *base = static_cast<PosedActive *>(derived);
	return (spine_posed_active) base;
}

spine_constraint spine_transform_constraint_base_cast_to_constraint(spine_transform_constraint_base obj) {
	TransformConstraintBase *derived = (TransformConstraintBase *) obj;
	Constraint *base = static_cast<Constraint *>(derived);
	return (spine_constraint) base;
}

spine_update spine_transform_constraint_base_cast_to_update(spine_transform_constraint_base obj) {
	TransformConstraintBase *derived = (TransformConstraintBase *) obj;
	Update *base = static_cast<Update *>(derived);
	return (spine_update) base;
}

spine_posed_data spine_transform_constraint_data_cast_to_posed_data(spine_transform_constraint_data obj) {
	TransformConstraintData *derived = (TransformConstraintData *) obj;
	PosedData *base = static_cast<PosedData *>(derived);
	return (spine_posed_data) base;
}

spine_constraint_data spine_transform_constraint_data_cast_to_constraint_data(spine_transform_constraint_data obj) {
	TransformConstraintData *derived = (TransformConstraintData *) obj;
	ConstraintData *base = static_cast<ConstraintData *>(derived);
	return (spine_constraint_data) base;
}

spine_curve_timeline spine_transform_constraint_timeline_cast_to_curve_timeline(spine_transform_constraint_timeline obj) {
	TransformConstraintTimeline *derived = (TransformConstraintTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_transform_constraint_timeline_cast_to_timeline(spine_transform_constraint_timeline obj) {
	TransformConstraintTimeline *derived = (TransformConstraintTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_constraint_timeline spine_transform_constraint_timeline_cast_to_constraint_timeline(spine_transform_constraint_timeline obj) {
	TransformConstraintTimeline *derived = (TransformConstraintTimeline *) obj;
	ConstraintTimeline *base = static_cast<ConstraintTimeline *>(derived);
	return (spine_constraint_timeline) base;
}

spine_bone_timeline2 spine_translate_timeline_cast_to_bone_timeline2(spine_translate_timeline obj) {
	TranslateTimeline *derived = (TranslateTimeline *) obj;
	BoneTimeline2 *base = static_cast<BoneTimeline2 *>(derived);
	return (spine_bone_timeline2) base;
}

spine_curve_timeline spine_translate_timeline_cast_to_curve_timeline(spine_translate_timeline obj) {
	TranslateTimeline *derived = (TranslateTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_translate_timeline_cast_to_timeline(spine_translate_timeline obj) {
	TranslateTimeline *derived = (TranslateTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_translate_timeline_cast_to_bone_timeline(spine_translate_timeline obj) {
	TranslateTimeline *derived = (TranslateTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline1 spine_translate_x_timeline_cast_to_bone_timeline1(spine_translate_x_timeline obj) {
	TranslateXTimeline *derived = (TranslateXTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_translate_x_timeline_cast_to_curve_timeline1(spine_translate_x_timeline obj) {
	TranslateXTimeline *derived = (TranslateXTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_translate_x_timeline_cast_to_curve_timeline(spine_translate_x_timeline obj) {
	TranslateXTimeline *derived = (TranslateXTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_translate_x_timeline_cast_to_timeline(spine_translate_x_timeline obj) {
	TranslateXTimeline *derived = (TranslateXTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_translate_x_timeline_cast_to_bone_timeline(spine_translate_x_timeline obj) {
	TranslateXTimeline *derived = (TranslateXTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_bone_timeline1 spine_translate_y_timeline_cast_to_bone_timeline1(spine_translate_y_timeline obj) {
	TranslateYTimeline *derived = (TranslateYTimeline *) obj;
	BoneTimeline1 *base = static_cast<BoneTimeline1 *>(derived);
	return (spine_bone_timeline1) base;
}

spine_curve_timeline1 spine_translate_y_timeline_cast_to_curve_timeline1(spine_translate_y_timeline obj) {
	TranslateYTimeline *derived = (TranslateYTimeline *) obj;
	CurveTimeline1 *base = static_cast<CurveTimeline1 *>(derived);
	return (spine_curve_timeline1) base;
}

spine_curve_timeline spine_translate_y_timeline_cast_to_curve_timeline(spine_translate_y_timeline obj) {
	TranslateYTimeline *derived = (TranslateYTimeline *) obj;
	CurveTimeline *base = static_cast<CurveTimeline *>(derived);
	return (spine_curve_timeline) base;
}

spine_timeline spine_translate_y_timeline_cast_to_timeline(spine_translate_y_timeline obj) {
	TranslateYTimeline *derived = (TranslateYTimeline *) obj;
	Timeline *base = static_cast<Timeline *>(derived);
	return (spine_timeline) base;
}

spine_bone_timeline spine_translate_y_timeline_cast_to_bone_timeline(spine_translate_y_timeline obj) {
	TranslateYTimeline *derived = (TranslateYTimeline *) obj;
	BoneTimeline *base = static_cast<BoneTimeline *>(derived);
	return (spine_bone_timeline) base;
}

spine_attachment spine_vertex_attachment_cast_to_attachment(spine_vertex_attachment obj) {
	VertexAttachment *derived = (VertexAttachment *) obj;
	Attachment *base = static_cast<Attachment *>(derived);
	return (spine_attachment) base;
}

// Downcast function implementations
spine_alpha_timeline spine_curve_timeline1_cast_to_alpha_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	AlphaTimeline *derived = static_cast<AlphaTimeline *>(base);
	return (spine_alpha_timeline) derived;
}

spine_bone_timeline1 spine_curve_timeline1_cast_to_bone_timeline1(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	BoneTimeline1 *derived = static_cast<BoneTimeline1 *>(base);
	return (spine_bone_timeline1) derived;
}

spine_constraint_timeline1 spine_curve_timeline1_cast_to_constraint_timeline1(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	ConstraintTimeline1 *derived = static_cast<ConstraintTimeline1 *>(base);
	return (spine_constraint_timeline1) derived;
}

spine_path_constraint_position_timeline spine_curve_timeline1_cast_to_path_constraint_position_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PathConstraintPositionTimeline *derived = static_cast<PathConstraintPositionTimeline *>(base);
	return (spine_path_constraint_position_timeline) derived;
}

spine_path_constraint_spacing_timeline spine_curve_timeline1_cast_to_path_constraint_spacing_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PathConstraintSpacingTimeline *derived = static_cast<PathConstraintSpacingTimeline *>(base);
	return (spine_path_constraint_spacing_timeline) derived;
}

spine_physics_constraint_damping_timeline spine_curve_timeline1_cast_to_physics_constraint_damping_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintDampingTimeline *derived = static_cast<PhysicsConstraintDampingTimeline *>(base);
	return (spine_physics_constraint_damping_timeline) derived;
}

spine_physics_constraint_gravity_timeline spine_curve_timeline1_cast_to_physics_constraint_gravity_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintGravityTimeline *derived = static_cast<PhysicsConstraintGravityTimeline *>(base);
	return (spine_physics_constraint_gravity_timeline) derived;
}

spine_physics_constraint_inertia_timeline spine_curve_timeline1_cast_to_physics_constraint_inertia_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintInertiaTimeline *derived = static_cast<PhysicsConstraintInertiaTimeline *>(base);
	return (spine_physics_constraint_inertia_timeline) derived;
}

spine_physics_constraint_mass_timeline spine_curve_timeline1_cast_to_physics_constraint_mass_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintMassTimeline *derived = static_cast<PhysicsConstraintMassTimeline *>(base);
	return (spine_physics_constraint_mass_timeline) derived;
}

spine_physics_constraint_mix_timeline spine_curve_timeline1_cast_to_physics_constraint_mix_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintMixTimeline *derived = static_cast<PhysicsConstraintMixTimeline *>(base);
	return (spine_physics_constraint_mix_timeline) derived;
}

spine_physics_constraint_strength_timeline spine_curve_timeline1_cast_to_physics_constraint_strength_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintStrengthTimeline *derived = static_cast<PhysicsConstraintStrengthTimeline *>(base);
	return (spine_physics_constraint_strength_timeline) derived;
}

spine_physics_constraint_timeline spine_curve_timeline1_cast_to_physics_constraint_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintTimeline *derived = static_cast<PhysicsConstraintTimeline *>(base);
	return (spine_physics_constraint_timeline) derived;
}

spine_physics_constraint_wind_timeline spine_curve_timeline1_cast_to_physics_constraint_wind_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	PhysicsConstraintWindTimeline *derived = static_cast<PhysicsConstraintWindTimeline *>(base);
	return (spine_physics_constraint_wind_timeline) derived;
}

spine_rotate_timeline spine_curve_timeline1_cast_to_rotate_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	RotateTimeline *derived = static_cast<RotateTimeline *>(base);
	return (spine_rotate_timeline) derived;
}

spine_scale_x_timeline spine_curve_timeline1_cast_to_scale_x_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	ScaleXTimeline *derived = static_cast<ScaleXTimeline *>(base);
	return (spine_scale_x_timeline) derived;
}

spine_scale_y_timeline spine_curve_timeline1_cast_to_scale_y_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	ScaleYTimeline *derived = static_cast<ScaleYTimeline *>(base);
	return (spine_scale_y_timeline) derived;
}

spine_shear_x_timeline spine_curve_timeline1_cast_to_shear_x_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	ShearXTimeline *derived = static_cast<ShearXTimeline *>(base);
	return (spine_shear_x_timeline) derived;
}

spine_shear_y_timeline spine_curve_timeline1_cast_to_shear_y_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	ShearYTimeline *derived = static_cast<ShearYTimeline *>(base);
	return (spine_shear_y_timeline) derived;
}

spine_slider_mix_timeline spine_curve_timeline1_cast_to_slider_mix_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	SliderMixTimeline *derived = static_cast<SliderMixTimeline *>(base);
	return (spine_slider_mix_timeline) derived;
}

spine_slider_timeline spine_curve_timeline1_cast_to_slider_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	SliderTimeline *derived = static_cast<SliderTimeline *>(base);
	return (spine_slider_timeline) derived;
}

spine_translate_x_timeline spine_curve_timeline1_cast_to_translate_x_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	TranslateXTimeline *derived = static_cast<TranslateXTimeline *>(base);
	return (spine_translate_x_timeline) derived;
}

spine_translate_y_timeline spine_curve_timeline1_cast_to_translate_y_timeline(spine_curve_timeline1 obj) {
	CurveTimeline1 *base = (CurveTimeline1 *) obj;
	TranslateYTimeline *derived = static_cast<TranslateYTimeline *>(base);
	return (spine_translate_y_timeline) derived;
}

spine_alpha_timeline spine_curve_timeline_cast_to_alpha_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	AlphaTimeline *derived = static_cast<AlphaTimeline *>(base);
	return (spine_alpha_timeline) derived;
}

spine_bone_timeline1 spine_curve_timeline_cast_to_bone_timeline1(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	BoneTimeline1 *derived = static_cast<BoneTimeline1 *>(base);
	return (spine_bone_timeline1) derived;
}

spine_bone_timeline2 spine_curve_timeline_cast_to_bone_timeline2(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	BoneTimeline2 *derived = static_cast<BoneTimeline2 *>(base);
	return (spine_bone_timeline2) derived;
}

spine_constraint_timeline1 spine_curve_timeline_cast_to_constraint_timeline1(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ConstraintTimeline1 *derived = static_cast<ConstraintTimeline1 *>(base);
	return (spine_constraint_timeline1) derived;
}

spine_curve_timeline1 spine_curve_timeline_cast_to_curve_timeline1(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	CurveTimeline1 *derived = static_cast<CurveTimeline1 *>(base);
	return (spine_curve_timeline1) derived;
}

spine_deform_timeline spine_curve_timeline_cast_to_deform_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	DeformTimeline *derived = static_cast<DeformTimeline *>(base);
	return (spine_deform_timeline) derived;
}

spine_ik_constraint_timeline spine_curve_timeline_cast_to_ik_constraint_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	IkConstraintTimeline *derived = static_cast<IkConstraintTimeline *>(base);
	return (spine_ik_constraint_timeline) derived;
}

spine_path_constraint_mix_timeline spine_curve_timeline_cast_to_path_constraint_mix_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PathConstraintMixTimeline *derived = static_cast<PathConstraintMixTimeline *>(base);
	return (spine_path_constraint_mix_timeline) derived;
}

spine_path_constraint_position_timeline spine_curve_timeline_cast_to_path_constraint_position_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PathConstraintPositionTimeline *derived = static_cast<PathConstraintPositionTimeline *>(base);
	return (spine_path_constraint_position_timeline) derived;
}

spine_path_constraint_spacing_timeline spine_curve_timeline_cast_to_path_constraint_spacing_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PathConstraintSpacingTimeline *derived = static_cast<PathConstraintSpacingTimeline *>(base);
	return (spine_path_constraint_spacing_timeline) derived;
}

spine_physics_constraint_damping_timeline spine_curve_timeline_cast_to_physics_constraint_damping_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintDampingTimeline *derived = static_cast<PhysicsConstraintDampingTimeline *>(base);
	return (spine_physics_constraint_damping_timeline) derived;
}

spine_physics_constraint_gravity_timeline spine_curve_timeline_cast_to_physics_constraint_gravity_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintGravityTimeline *derived = static_cast<PhysicsConstraintGravityTimeline *>(base);
	return (spine_physics_constraint_gravity_timeline) derived;
}

spine_physics_constraint_inertia_timeline spine_curve_timeline_cast_to_physics_constraint_inertia_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintInertiaTimeline *derived = static_cast<PhysicsConstraintInertiaTimeline *>(base);
	return (spine_physics_constraint_inertia_timeline) derived;
}

spine_physics_constraint_mass_timeline spine_curve_timeline_cast_to_physics_constraint_mass_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintMassTimeline *derived = static_cast<PhysicsConstraintMassTimeline *>(base);
	return (spine_physics_constraint_mass_timeline) derived;
}

spine_physics_constraint_mix_timeline spine_curve_timeline_cast_to_physics_constraint_mix_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintMixTimeline *derived = static_cast<PhysicsConstraintMixTimeline *>(base);
	return (spine_physics_constraint_mix_timeline) derived;
}

spine_physics_constraint_strength_timeline spine_curve_timeline_cast_to_physics_constraint_strength_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintStrengthTimeline *derived = static_cast<PhysicsConstraintStrengthTimeline *>(base);
	return (spine_physics_constraint_strength_timeline) derived;
}

spine_physics_constraint_timeline spine_curve_timeline_cast_to_physics_constraint_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintTimeline *derived = static_cast<PhysicsConstraintTimeline *>(base);
	return (spine_physics_constraint_timeline) derived;
}

spine_physics_constraint_wind_timeline spine_curve_timeline_cast_to_physics_constraint_wind_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	PhysicsConstraintWindTimeline *derived = static_cast<PhysicsConstraintWindTimeline *>(base);
	return (spine_physics_constraint_wind_timeline) derived;
}

spine_rgb2_timeline spine_curve_timeline_cast_to_rgb2_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	RGB2Timeline *derived = static_cast<RGB2Timeline *>(base);
	return (spine_rgb2_timeline) derived;
}

spine_rgba2_timeline spine_curve_timeline_cast_to_rgba2_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	RGBA2Timeline *derived = static_cast<RGBA2Timeline *>(base);
	return (spine_rgba2_timeline) derived;
}

spine_rgba_timeline spine_curve_timeline_cast_to_rgba_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	RGBATimeline *derived = static_cast<RGBATimeline *>(base);
	return (spine_rgba_timeline) derived;
}

spine_rgb_timeline spine_curve_timeline_cast_to_rgb_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	RGBTimeline *derived = static_cast<RGBTimeline *>(base);
	return (spine_rgb_timeline) derived;
}

spine_rotate_timeline spine_curve_timeline_cast_to_rotate_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	RotateTimeline *derived = static_cast<RotateTimeline *>(base);
	return (spine_rotate_timeline) derived;
}

spine_scale_timeline spine_curve_timeline_cast_to_scale_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ScaleTimeline *derived = static_cast<ScaleTimeline *>(base);
	return (spine_scale_timeline) derived;
}

spine_scale_x_timeline spine_curve_timeline_cast_to_scale_x_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ScaleXTimeline *derived = static_cast<ScaleXTimeline *>(base);
	return (spine_scale_x_timeline) derived;
}

spine_scale_y_timeline spine_curve_timeline_cast_to_scale_y_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ScaleYTimeline *derived = static_cast<ScaleYTimeline *>(base);
	return (spine_scale_y_timeline) derived;
}

spine_shear_timeline spine_curve_timeline_cast_to_shear_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ShearTimeline *derived = static_cast<ShearTimeline *>(base);
	return (spine_shear_timeline) derived;
}

spine_shear_x_timeline spine_curve_timeline_cast_to_shear_x_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ShearXTimeline *derived = static_cast<ShearXTimeline *>(base);
	return (spine_shear_x_timeline) derived;
}

spine_shear_y_timeline spine_curve_timeline_cast_to_shear_y_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	ShearYTimeline *derived = static_cast<ShearYTimeline *>(base);
	return (spine_shear_y_timeline) derived;
}

spine_slider_mix_timeline spine_curve_timeline_cast_to_slider_mix_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	SliderMixTimeline *derived = static_cast<SliderMixTimeline *>(base);
	return (spine_slider_mix_timeline) derived;
}

spine_slider_timeline spine_curve_timeline_cast_to_slider_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	SliderTimeline *derived = static_cast<SliderTimeline *>(base);
	return (spine_slider_timeline) derived;
}

spine_slot_curve_timeline spine_curve_timeline_cast_to_slot_curve_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	SlotCurveTimeline *derived = static_cast<SlotCurveTimeline *>(base);
	return (spine_slot_curve_timeline) derived;
}

spine_transform_constraint_timeline spine_curve_timeline_cast_to_transform_constraint_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	TransformConstraintTimeline *derived = static_cast<TransformConstraintTimeline *>(base);
	return (spine_transform_constraint_timeline) derived;
}

spine_translate_timeline spine_curve_timeline_cast_to_translate_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	TranslateTimeline *derived = static_cast<TranslateTimeline *>(base);
	return (spine_translate_timeline) derived;
}

spine_translate_x_timeline spine_curve_timeline_cast_to_translate_x_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	TranslateXTimeline *derived = static_cast<TranslateXTimeline *>(base);
	return (spine_translate_x_timeline) derived;
}

spine_translate_y_timeline spine_curve_timeline_cast_to_translate_y_timeline(spine_curve_timeline obj) {
	CurveTimeline *base = (CurveTimeline *) obj;
	TranslateYTimeline *derived = static_cast<TranslateYTimeline *>(base);
	return (spine_translate_y_timeline) derived;
}

spine_alpha_timeline spine_timeline_cast_to_alpha_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	AlphaTimeline *derived = static_cast<AlphaTimeline *>(base);
	return (spine_alpha_timeline) derived;
}

spine_attachment_timeline spine_timeline_cast_to_attachment_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	AttachmentTimeline *derived = static_cast<AttachmentTimeline *>(base);
	return (spine_attachment_timeline) derived;
}

spine_bone_timeline1 spine_timeline_cast_to_bone_timeline1(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	BoneTimeline1 *derived = static_cast<BoneTimeline1 *>(base);
	return (spine_bone_timeline1) derived;
}

spine_bone_timeline2 spine_timeline_cast_to_bone_timeline2(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	BoneTimeline2 *derived = static_cast<BoneTimeline2 *>(base);
	return (spine_bone_timeline2) derived;
}

spine_constraint_timeline1 spine_timeline_cast_to_constraint_timeline1(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ConstraintTimeline1 *derived = static_cast<ConstraintTimeline1 *>(base);
	return (spine_constraint_timeline1) derived;
}

spine_curve_timeline spine_timeline_cast_to_curve_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	CurveTimeline *derived = static_cast<CurveTimeline *>(base);
	return (spine_curve_timeline) derived;
}

spine_curve_timeline1 spine_timeline_cast_to_curve_timeline1(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	CurveTimeline1 *derived = static_cast<CurveTimeline1 *>(base);
	return (spine_curve_timeline1) derived;
}

spine_deform_timeline spine_timeline_cast_to_deform_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	DeformTimeline *derived = static_cast<DeformTimeline *>(base);
	return (spine_deform_timeline) derived;
}

spine_draw_order_folder_timeline spine_timeline_cast_to_draw_order_folder_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	DrawOrderFolderTimeline *derived = static_cast<DrawOrderFolderTimeline *>(base);
	return (spine_draw_order_folder_timeline) derived;
}

spine_draw_order_timeline spine_timeline_cast_to_draw_order_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	DrawOrderTimeline *derived = static_cast<DrawOrderTimeline *>(base);
	return (spine_draw_order_timeline) derived;
}

spine_event_timeline spine_timeline_cast_to_event_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	EventTimeline *derived = static_cast<EventTimeline *>(base);
	return (spine_event_timeline) derived;
}

spine_ik_constraint_timeline spine_timeline_cast_to_ik_constraint_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	IkConstraintTimeline *derived = static_cast<IkConstraintTimeline *>(base);
	return (spine_ik_constraint_timeline) derived;
}

spine_inherit_timeline spine_timeline_cast_to_inherit_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	InheritTimeline *derived = static_cast<InheritTimeline *>(base);
	return (spine_inherit_timeline) derived;
}

spine_path_constraint_mix_timeline spine_timeline_cast_to_path_constraint_mix_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PathConstraintMixTimeline *derived = static_cast<PathConstraintMixTimeline *>(base);
	return (spine_path_constraint_mix_timeline) derived;
}

spine_path_constraint_position_timeline spine_timeline_cast_to_path_constraint_position_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PathConstraintPositionTimeline *derived = static_cast<PathConstraintPositionTimeline *>(base);
	return (spine_path_constraint_position_timeline) derived;
}

spine_path_constraint_spacing_timeline spine_timeline_cast_to_path_constraint_spacing_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PathConstraintSpacingTimeline *derived = static_cast<PathConstraintSpacingTimeline *>(base);
	return (spine_path_constraint_spacing_timeline) derived;
}

spine_physics_constraint_damping_timeline spine_timeline_cast_to_physics_constraint_damping_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintDampingTimeline *derived = static_cast<PhysicsConstraintDampingTimeline *>(base);
	return (spine_physics_constraint_damping_timeline) derived;
}

spine_physics_constraint_gravity_timeline spine_timeline_cast_to_physics_constraint_gravity_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintGravityTimeline *derived = static_cast<PhysicsConstraintGravityTimeline *>(base);
	return (spine_physics_constraint_gravity_timeline) derived;
}

spine_physics_constraint_inertia_timeline spine_timeline_cast_to_physics_constraint_inertia_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintInertiaTimeline *derived = static_cast<PhysicsConstraintInertiaTimeline *>(base);
	return (spine_physics_constraint_inertia_timeline) derived;
}

spine_physics_constraint_mass_timeline spine_timeline_cast_to_physics_constraint_mass_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintMassTimeline *derived = static_cast<PhysicsConstraintMassTimeline *>(base);
	return (spine_physics_constraint_mass_timeline) derived;
}

spine_physics_constraint_mix_timeline spine_timeline_cast_to_physics_constraint_mix_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintMixTimeline *derived = static_cast<PhysicsConstraintMixTimeline *>(base);
	return (spine_physics_constraint_mix_timeline) derived;
}

spine_physics_constraint_reset_timeline spine_timeline_cast_to_physics_constraint_reset_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintResetTimeline *derived = static_cast<PhysicsConstraintResetTimeline *>(base);
	return (spine_physics_constraint_reset_timeline) derived;
}

spine_physics_constraint_strength_timeline spine_timeline_cast_to_physics_constraint_strength_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintStrengthTimeline *derived = static_cast<PhysicsConstraintStrengthTimeline *>(base);
	return (spine_physics_constraint_strength_timeline) derived;
}

spine_physics_constraint_timeline spine_timeline_cast_to_physics_constraint_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintTimeline *derived = static_cast<PhysicsConstraintTimeline *>(base);
	return (spine_physics_constraint_timeline) derived;
}

spine_physics_constraint_wind_timeline spine_timeline_cast_to_physics_constraint_wind_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	PhysicsConstraintWindTimeline *derived = static_cast<PhysicsConstraintWindTimeline *>(base);
	return (spine_physics_constraint_wind_timeline) derived;
}

spine_rgb2_timeline spine_timeline_cast_to_rgb2_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	RGB2Timeline *derived = static_cast<RGB2Timeline *>(base);
	return (spine_rgb2_timeline) derived;
}

spine_rgba2_timeline spine_timeline_cast_to_rgba2_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	RGBA2Timeline *derived = static_cast<RGBA2Timeline *>(base);
	return (spine_rgba2_timeline) derived;
}

spine_rgba_timeline spine_timeline_cast_to_rgba_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	RGBATimeline *derived = static_cast<RGBATimeline *>(base);
	return (spine_rgba_timeline) derived;
}

spine_rgb_timeline spine_timeline_cast_to_rgb_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	RGBTimeline *derived = static_cast<RGBTimeline *>(base);
	return (spine_rgb_timeline) derived;
}

spine_rotate_timeline spine_timeline_cast_to_rotate_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	RotateTimeline *derived = static_cast<RotateTimeline *>(base);
	return (spine_rotate_timeline) derived;
}

spine_scale_timeline spine_timeline_cast_to_scale_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ScaleTimeline *derived = static_cast<ScaleTimeline *>(base);
	return (spine_scale_timeline) derived;
}

spine_scale_x_timeline spine_timeline_cast_to_scale_x_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ScaleXTimeline *derived = static_cast<ScaleXTimeline *>(base);
	return (spine_scale_x_timeline) derived;
}

spine_scale_y_timeline spine_timeline_cast_to_scale_y_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ScaleYTimeline *derived = static_cast<ScaleYTimeline *>(base);
	return (spine_scale_y_timeline) derived;
}

spine_sequence_timeline spine_timeline_cast_to_sequence_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	SequenceTimeline *derived = static_cast<SequenceTimeline *>(base);
	return (spine_sequence_timeline) derived;
}

spine_shear_timeline spine_timeline_cast_to_shear_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ShearTimeline *derived = static_cast<ShearTimeline *>(base);
	return (spine_shear_timeline) derived;
}

spine_shear_x_timeline spine_timeline_cast_to_shear_x_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ShearXTimeline *derived = static_cast<ShearXTimeline *>(base);
	return (spine_shear_x_timeline) derived;
}

spine_shear_y_timeline spine_timeline_cast_to_shear_y_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	ShearYTimeline *derived = static_cast<ShearYTimeline *>(base);
	return (spine_shear_y_timeline) derived;
}

spine_slider_mix_timeline spine_timeline_cast_to_slider_mix_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	SliderMixTimeline *derived = static_cast<SliderMixTimeline *>(base);
	return (spine_slider_mix_timeline) derived;
}

spine_slider_timeline spine_timeline_cast_to_slider_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	SliderTimeline *derived = static_cast<SliderTimeline *>(base);
	return (spine_slider_timeline) derived;
}

spine_slot_curve_timeline spine_timeline_cast_to_slot_curve_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	SlotCurveTimeline *derived = static_cast<SlotCurveTimeline *>(base);
	return (spine_slot_curve_timeline) derived;
}

spine_transform_constraint_timeline spine_timeline_cast_to_transform_constraint_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	TransformConstraintTimeline *derived = static_cast<TransformConstraintTimeline *>(base);
	return (spine_transform_constraint_timeline) derived;
}

spine_translate_timeline spine_timeline_cast_to_translate_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	TranslateTimeline *derived = static_cast<TranslateTimeline *>(base);
	return (spine_translate_timeline) derived;
}

spine_translate_x_timeline spine_timeline_cast_to_translate_x_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	TranslateXTimeline *derived = static_cast<TranslateXTimeline *>(base);
	return (spine_translate_x_timeline) derived;
}

spine_translate_y_timeline spine_timeline_cast_to_translate_y_timeline(spine_timeline obj) {
	Timeline *base = (Timeline *) obj;
	TranslateYTimeline *derived = static_cast<TranslateYTimeline *>(base);
	return (spine_translate_y_timeline) derived;
}

spine_alpha_timeline spine_slot_timeline_cast_to_alpha_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	AlphaTimeline *derived = static_cast<AlphaTimeline *>(base);
	return (spine_alpha_timeline) derived;
}

spine_attachment_timeline spine_slot_timeline_cast_to_attachment_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	AttachmentTimeline *derived = static_cast<AttachmentTimeline *>(base);
	return (spine_attachment_timeline) derived;
}

spine_deform_timeline spine_slot_timeline_cast_to_deform_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	DeformTimeline *derived = static_cast<DeformTimeline *>(base);
	return (spine_deform_timeline) derived;
}

spine_rgb2_timeline spine_slot_timeline_cast_to_rgb2_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	RGB2Timeline *derived = static_cast<RGB2Timeline *>(base);
	return (spine_rgb2_timeline) derived;
}

spine_rgba2_timeline spine_slot_timeline_cast_to_rgba2_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	RGBA2Timeline *derived = static_cast<RGBA2Timeline *>(base);
	return (spine_rgba2_timeline) derived;
}

spine_rgba_timeline spine_slot_timeline_cast_to_rgba_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	RGBATimeline *derived = static_cast<RGBATimeline *>(base);
	return (spine_rgba_timeline) derived;
}

spine_rgb_timeline spine_slot_timeline_cast_to_rgb_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	RGBTimeline *derived = static_cast<RGBTimeline *>(base);
	return (spine_rgb_timeline) derived;
}

spine_sequence_timeline spine_slot_timeline_cast_to_sequence_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	SequenceTimeline *derived = static_cast<SequenceTimeline *>(base);
	return (spine_sequence_timeline) derived;
}

spine_slot_curve_timeline spine_slot_timeline_cast_to_slot_curve_timeline(spine_slot_timeline obj) {
	SlotTimeline *base = (SlotTimeline *) obj;
	SlotCurveTimeline *derived = static_cast<SlotCurveTimeline *>(base);
	return (spine_slot_curve_timeline) derived;
}

spine_atlas_attachment_loader spine_attachment_loader_cast_to_atlas_attachment_loader(spine_attachment_loader obj) {
	AttachmentLoader *base = (AttachmentLoader *) obj;
	AtlasAttachmentLoader *derived = static_cast<AtlasAttachmentLoader *>(base);
	return (spine_atlas_attachment_loader) derived;
}

spine_atlas_region spine_texture_region_cast_to_atlas_region(spine_texture_region obj) {
	TextureRegion *base = (TextureRegion *) obj;
	AtlasRegion *derived = static_cast<AtlasRegion *>(base);
	return (spine_atlas_region) derived;
}

spine_bone spine_posed_cast_to_bone(spine_posed obj) {
	Posed *base = (Posed *) obj;
	Bone *derived = static_cast<Bone *>(base);
	return (spine_bone) derived;
}

spine_ik_constraint spine_posed_cast_to_ik_constraint(spine_posed obj) {
	Posed *base = (Posed *) obj;
	IkConstraint *derived = static_cast<IkConstraint *>(base);
	return (spine_ik_constraint) derived;
}

spine_ik_constraint_base spine_posed_cast_to_ik_constraint_base(spine_posed obj) {
	Posed *base = (Posed *) obj;
	IkConstraintBase *derived = static_cast<IkConstraintBase *>(base);
	return (spine_ik_constraint_base) derived;
}

spine_path_constraint spine_posed_cast_to_path_constraint(spine_posed obj) {
	Posed *base = (Posed *) obj;
	PathConstraint *derived = static_cast<PathConstraint *>(base);
	return (spine_path_constraint) derived;
}

spine_path_constraint_base spine_posed_cast_to_path_constraint_base(spine_posed obj) {
	Posed *base = (Posed *) obj;
	PathConstraintBase *derived = static_cast<PathConstraintBase *>(base);
	return (spine_path_constraint_base) derived;
}

spine_physics_constraint spine_posed_cast_to_physics_constraint(spine_posed obj) {
	Posed *base = (Posed *) obj;
	PhysicsConstraint *derived = static_cast<PhysicsConstraint *>(base);
	return (spine_physics_constraint) derived;
}

spine_physics_constraint_base spine_posed_cast_to_physics_constraint_base(spine_posed obj) {
	Posed *base = (Posed *) obj;
	PhysicsConstraintBase *derived = static_cast<PhysicsConstraintBase *>(base);
	return (spine_physics_constraint_base) derived;
}

spine_slider spine_posed_cast_to_slider(spine_posed obj) {
	Posed *base = (Posed *) obj;
	Slider *derived = static_cast<Slider *>(base);
	return (spine_slider) derived;
}

spine_slider_base spine_posed_cast_to_slider_base(spine_posed obj) {
	Posed *base = (Posed *) obj;
	SliderBase *derived = static_cast<SliderBase *>(base);
	return (spine_slider_base) derived;
}

spine_slot spine_posed_cast_to_slot(spine_posed obj) {
	Posed *base = (Posed *) obj;
	Slot *derived = static_cast<Slot *>(base);
	return (spine_slot) derived;
}

spine_transform_constraint spine_posed_cast_to_transform_constraint(spine_posed obj) {
	Posed *base = (Posed *) obj;
	TransformConstraint *derived = static_cast<TransformConstraint *>(base);
	return (spine_transform_constraint) derived;
}

spine_transform_constraint_base spine_posed_cast_to_transform_constraint_base(spine_posed obj) {
	Posed *base = (Posed *) obj;
	TransformConstraintBase *derived = static_cast<TransformConstraintBase *>(base);
	return (spine_transform_constraint_base) derived;
}

spine_bone spine_posed_active_cast_to_bone(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	Bone *derived = static_cast<Bone *>(base);
	return (spine_bone) derived;
}

spine_ik_constraint spine_posed_active_cast_to_ik_constraint(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	IkConstraint *derived = static_cast<IkConstraint *>(base);
	return (spine_ik_constraint) derived;
}

spine_ik_constraint_base spine_posed_active_cast_to_ik_constraint_base(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	IkConstraintBase *derived = static_cast<IkConstraintBase *>(base);
	return (spine_ik_constraint_base) derived;
}

spine_path_constraint spine_posed_active_cast_to_path_constraint(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	PathConstraint *derived = static_cast<PathConstraint *>(base);
	return (spine_path_constraint) derived;
}

spine_path_constraint_base spine_posed_active_cast_to_path_constraint_base(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	PathConstraintBase *derived = static_cast<PathConstraintBase *>(base);
	return (spine_path_constraint_base) derived;
}

spine_physics_constraint spine_posed_active_cast_to_physics_constraint(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	PhysicsConstraint *derived = static_cast<PhysicsConstraint *>(base);
	return (spine_physics_constraint) derived;
}

spine_physics_constraint_base spine_posed_active_cast_to_physics_constraint_base(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	PhysicsConstraintBase *derived = static_cast<PhysicsConstraintBase *>(base);
	return (spine_physics_constraint_base) derived;
}

spine_slider spine_posed_active_cast_to_slider(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	Slider *derived = static_cast<Slider *>(base);
	return (spine_slider) derived;
}

spine_slider_base spine_posed_active_cast_to_slider_base(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	SliderBase *derived = static_cast<SliderBase *>(base);
	return (spine_slider_base) derived;
}

spine_transform_constraint spine_posed_active_cast_to_transform_constraint(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	TransformConstraint *derived = static_cast<TransformConstraint *>(base);
	return (spine_transform_constraint) derived;
}

spine_transform_constraint_base spine_posed_active_cast_to_transform_constraint_base(spine_posed_active obj) {
	PosedActive *base = (PosedActive *) obj;
	TransformConstraintBase *derived = static_cast<TransformConstraintBase *>(base);
	return (spine_transform_constraint_base) derived;
}

spine_bone spine_update_cast_to_bone(spine_update obj) {
	Update *base = (Update *) obj;
	Bone *derived = static_cast<Bone *>(base);
	return (spine_bone) derived;
}

spine_bone_pose spine_update_cast_to_bone_pose(spine_update obj) {
	Update *base = (Update *) obj;
	BonePose *derived = static_cast<BonePose *>(base);
	return (spine_bone_pose) derived;
}

spine_constraint spine_update_cast_to_constraint(spine_update obj) {
	Update *base = (Update *) obj;
	Constraint *derived = static_cast<Constraint *>(base);
	return (spine_constraint) derived;
}

spine_ik_constraint spine_update_cast_to_ik_constraint(spine_update obj) {
	Update *base = (Update *) obj;
	IkConstraint *derived = static_cast<IkConstraint *>(base);
	return (spine_ik_constraint) derived;
}

spine_ik_constraint_base spine_update_cast_to_ik_constraint_base(spine_update obj) {
	Update *base = (Update *) obj;
	IkConstraintBase *derived = static_cast<IkConstraintBase *>(base);
	return (spine_ik_constraint_base) derived;
}

spine_path_constraint spine_update_cast_to_path_constraint(spine_update obj) {
	Update *base = (Update *) obj;
	PathConstraint *derived = static_cast<PathConstraint *>(base);
	return (spine_path_constraint) derived;
}

spine_path_constraint_base spine_update_cast_to_path_constraint_base(spine_update obj) {
	Update *base = (Update *) obj;
	PathConstraintBase *derived = static_cast<PathConstraintBase *>(base);
	return (spine_path_constraint_base) derived;
}

spine_physics_constraint spine_update_cast_to_physics_constraint(spine_update obj) {
	Update *base = (Update *) obj;
	PhysicsConstraint *derived = static_cast<PhysicsConstraint *>(base);
	return (spine_physics_constraint) derived;
}

spine_physics_constraint_base spine_update_cast_to_physics_constraint_base(spine_update obj) {
	Update *base = (Update *) obj;
	PhysicsConstraintBase *derived = static_cast<PhysicsConstraintBase *>(base);
	return (spine_physics_constraint_base) derived;
}

spine_slider spine_update_cast_to_slider(spine_update obj) {
	Update *base = (Update *) obj;
	Slider *derived = static_cast<Slider *>(base);
	return (spine_slider) derived;
}

spine_slider_base spine_update_cast_to_slider_base(spine_update obj) {
	Update *base = (Update *) obj;
	SliderBase *derived = static_cast<SliderBase *>(base);
	return (spine_slider_base) derived;
}

spine_transform_constraint spine_update_cast_to_transform_constraint(spine_update obj) {
	Update *base = (Update *) obj;
	TransformConstraint *derived = static_cast<TransformConstraint *>(base);
	return (spine_transform_constraint) derived;
}

spine_transform_constraint_base spine_update_cast_to_transform_constraint_base(spine_update obj) {
	Update *base = (Update *) obj;
	TransformConstraintBase *derived = static_cast<TransformConstraintBase *>(base);
	return (spine_transform_constraint_base) derived;
}

spine_bone_data spine_posed_data_cast_to_bone_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	BoneData *derived = static_cast<BoneData *>(base);
	return (spine_bone_data) derived;
}

spine_ik_constraint_data spine_posed_data_cast_to_ik_constraint_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	IkConstraintData *derived = static_cast<IkConstraintData *>(base);
	return (spine_ik_constraint_data) derived;
}

spine_path_constraint_data spine_posed_data_cast_to_path_constraint_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	PathConstraintData *derived = static_cast<PathConstraintData *>(base);
	return (spine_path_constraint_data) derived;
}

spine_physics_constraint_data spine_posed_data_cast_to_physics_constraint_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	PhysicsConstraintData *derived = static_cast<PhysicsConstraintData *>(base);
	return (spine_physics_constraint_data) derived;
}

spine_slider_data spine_posed_data_cast_to_slider_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	SliderData *derived = static_cast<SliderData *>(base);
	return (spine_slider_data) derived;
}

spine_slot_data spine_posed_data_cast_to_slot_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	SlotData *derived = static_cast<SlotData *>(base);
	return (spine_slot_data) derived;
}

spine_transform_constraint_data spine_posed_data_cast_to_transform_constraint_data(spine_posed_data obj) {
	PosedData *base = (PosedData *) obj;
	TransformConstraintData *derived = static_cast<TransformConstraintData *>(base);
	return (spine_transform_constraint_data) derived;
}

spine_bone_pose spine_bone_local_cast_to_bone_pose(spine_bone_local obj) {
	BoneLocal *base = (BoneLocal *) obj;
	BonePose *derived = static_cast<BonePose *>(base);
	return (spine_bone_pose) derived;
}

spine_bone_timeline1 spine_bone_timeline_cast_to_bone_timeline1(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	BoneTimeline1 *derived = static_cast<BoneTimeline1 *>(base);
	return (spine_bone_timeline1) derived;
}

spine_bone_timeline2 spine_bone_timeline_cast_to_bone_timeline2(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	BoneTimeline2 *derived = static_cast<BoneTimeline2 *>(base);
	return (spine_bone_timeline2) derived;
}

spine_inherit_timeline spine_bone_timeline_cast_to_inherit_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	InheritTimeline *derived = static_cast<InheritTimeline *>(base);
	return (spine_inherit_timeline) derived;
}

spine_rotate_timeline spine_bone_timeline_cast_to_rotate_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	RotateTimeline *derived = static_cast<RotateTimeline *>(base);
	return (spine_rotate_timeline) derived;
}

spine_scale_timeline spine_bone_timeline_cast_to_scale_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	ScaleTimeline *derived = static_cast<ScaleTimeline *>(base);
	return (spine_scale_timeline) derived;
}

spine_scale_x_timeline spine_bone_timeline_cast_to_scale_x_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	ScaleXTimeline *derived = static_cast<ScaleXTimeline *>(base);
	return (spine_scale_x_timeline) derived;
}

spine_scale_y_timeline spine_bone_timeline_cast_to_scale_y_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	ScaleYTimeline *derived = static_cast<ScaleYTimeline *>(base);
	return (spine_scale_y_timeline) derived;
}

spine_shear_timeline spine_bone_timeline_cast_to_shear_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	ShearTimeline *derived = static_cast<ShearTimeline *>(base);
	return (spine_shear_timeline) derived;
}

spine_shear_x_timeline spine_bone_timeline_cast_to_shear_x_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	ShearXTimeline *derived = static_cast<ShearXTimeline *>(base);
	return (spine_shear_x_timeline) derived;
}

spine_shear_y_timeline spine_bone_timeline_cast_to_shear_y_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	ShearYTimeline *derived = static_cast<ShearYTimeline *>(base);
	return (spine_shear_y_timeline) derived;
}

spine_translate_timeline spine_bone_timeline_cast_to_translate_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	TranslateTimeline *derived = static_cast<TranslateTimeline *>(base);
	return (spine_translate_timeline) derived;
}

spine_translate_x_timeline spine_bone_timeline_cast_to_translate_x_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	TranslateXTimeline *derived = static_cast<TranslateXTimeline *>(base);
	return (spine_translate_x_timeline) derived;
}

spine_translate_y_timeline spine_bone_timeline_cast_to_translate_y_timeline(spine_bone_timeline obj) {
	BoneTimeline *base = (BoneTimeline *) obj;
	TranslateYTimeline *derived = static_cast<TranslateYTimeline *>(base);
	return (spine_translate_y_timeline) derived;
}

spine_bounding_box_attachment spine_vertex_attachment_cast_to_bounding_box_attachment(spine_vertex_attachment obj) {
	VertexAttachment *base = (VertexAttachment *) obj;
	BoundingBoxAttachment *derived = static_cast<BoundingBoxAttachment *>(base);
	return (spine_bounding_box_attachment) derived;
}

spine_clipping_attachment spine_vertex_attachment_cast_to_clipping_attachment(spine_vertex_attachment obj) {
	VertexAttachment *base = (VertexAttachment *) obj;
	ClippingAttachment *derived = static_cast<ClippingAttachment *>(base);
	return (spine_clipping_attachment) derived;
}

spine_mesh_attachment spine_vertex_attachment_cast_to_mesh_attachment(spine_vertex_attachment obj) {
	VertexAttachment *base = (VertexAttachment *) obj;
	MeshAttachment *derived = static_cast<MeshAttachment *>(base);
	return (spine_mesh_attachment) derived;
}

spine_path_attachment spine_vertex_attachment_cast_to_path_attachment(spine_vertex_attachment obj) {
	VertexAttachment *base = (VertexAttachment *) obj;
	PathAttachment *derived = static_cast<PathAttachment *>(base);
	return (spine_path_attachment) derived;
}

spine_bounding_box_attachment spine_attachment_cast_to_bounding_box_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	BoundingBoxAttachment *derived = static_cast<BoundingBoxAttachment *>(base);
	return (spine_bounding_box_attachment) derived;
}

spine_clipping_attachment spine_attachment_cast_to_clipping_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	ClippingAttachment *derived = static_cast<ClippingAttachment *>(base);
	return (spine_clipping_attachment) derived;
}

spine_mesh_attachment spine_attachment_cast_to_mesh_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	MeshAttachment *derived = static_cast<MeshAttachment *>(base);
	return (spine_mesh_attachment) derived;
}

spine_path_attachment spine_attachment_cast_to_path_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	PathAttachment *derived = static_cast<PathAttachment *>(base);
	return (spine_path_attachment) derived;
}

spine_point_attachment spine_attachment_cast_to_point_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	PointAttachment *derived = static_cast<PointAttachment *>(base);
	return (spine_point_attachment) derived;
}

spine_region_attachment spine_attachment_cast_to_region_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	RegionAttachment *derived = static_cast<RegionAttachment *>(base);
	return (spine_region_attachment) derived;
}

spine_vertex_attachment spine_attachment_cast_to_vertex_attachment(spine_attachment obj) {
	Attachment *base = (Attachment *) obj;
	VertexAttachment *derived = static_cast<VertexAttachment *>(base);
	return (spine_vertex_attachment) derived;
}

spine_constraint_timeline1 spine_constraint_timeline_cast_to_constraint_timeline1(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	ConstraintTimeline1 *derived = static_cast<ConstraintTimeline1 *>(base);
	return (spine_constraint_timeline1) derived;
}

spine_ik_constraint_timeline spine_constraint_timeline_cast_to_ik_constraint_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	IkConstraintTimeline *derived = static_cast<IkConstraintTimeline *>(base);
	return (spine_ik_constraint_timeline) derived;
}

spine_path_constraint_mix_timeline spine_constraint_timeline_cast_to_path_constraint_mix_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PathConstraintMixTimeline *derived = static_cast<PathConstraintMixTimeline *>(base);
	return (spine_path_constraint_mix_timeline) derived;
}

spine_path_constraint_position_timeline spine_constraint_timeline_cast_to_path_constraint_position_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PathConstraintPositionTimeline *derived = static_cast<PathConstraintPositionTimeline *>(base);
	return (spine_path_constraint_position_timeline) derived;
}

spine_path_constraint_spacing_timeline spine_constraint_timeline_cast_to_path_constraint_spacing_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PathConstraintSpacingTimeline *derived = static_cast<PathConstraintSpacingTimeline *>(base);
	return (spine_path_constraint_spacing_timeline) derived;
}

spine_physics_constraint_damping_timeline spine_constraint_timeline_cast_to_physics_constraint_damping_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintDampingTimeline *derived = static_cast<PhysicsConstraintDampingTimeline *>(base);
	return (spine_physics_constraint_damping_timeline) derived;
}

spine_physics_constraint_gravity_timeline spine_constraint_timeline_cast_to_physics_constraint_gravity_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintGravityTimeline *derived = static_cast<PhysicsConstraintGravityTimeline *>(base);
	return (spine_physics_constraint_gravity_timeline) derived;
}

spine_physics_constraint_inertia_timeline spine_constraint_timeline_cast_to_physics_constraint_inertia_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintInertiaTimeline *derived = static_cast<PhysicsConstraintInertiaTimeline *>(base);
	return (spine_physics_constraint_inertia_timeline) derived;
}

spine_physics_constraint_mass_timeline spine_constraint_timeline_cast_to_physics_constraint_mass_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintMassTimeline *derived = static_cast<PhysicsConstraintMassTimeline *>(base);
	return (spine_physics_constraint_mass_timeline) derived;
}

spine_physics_constraint_mix_timeline spine_constraint_timeline_cast_to_physics_constraint_mix_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintMixTimeline *derived = static_cast<PhysicsConstraintMixTimeline *>(base);
	return (spine_physics_constraint_mix_timeline) derived;
}

spine_physics_constraint_reset_timeline spine_constraint_timeline_cast_to_physics_constraint_reset_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintResetTimeline *derived = static_cast<PhysicsConstraintResetTimeline *>(base);
	return (spine_physics_constraint_reset_timeline) derived;
}

spine_physics_constraint_strength_timeline spine_constraint_timeline_cast_to_physics_constraint_strength_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintStrengthTimeline *derived = static_cast<PhysicsConstraintStrengthTimeline *>(base);
	return (spine_physics_constraint_strength_timeline) derived;
}

spine_physics_constraint_timeline spine_constraint_timeline_cast_to_physics_constraint_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintTimeline *derived = static_cast<PhysicsConstraintTimeline *>(base);
	return (spine_physics_constraint_timeline) derived;
}

spine_physics_constraint_wind_timeline spine_constraint_timeline_cast_to_physics_constraint_wind_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	PhysicsConstraintWindTimeline *derived = static_cast<PhysicsConstraintWindTimeline *>(base);
	return (spine_physics_constraint_wind_timeline) derived;
}

spine_slider_mix_timeline spine_constraint_timeline_cast_to_slider_mix_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	SliderMixTimeline *derived = static_cast<SliderMixTimeline *>(base);
	return (spine_slider_mix_timeline) derived;
}

spine_slider_timeline spine_constraint_timeline_cast_to_slider_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	SliderTimeline *derived = static_cast<SliderTimeline *>(base);
	return (spine_slider_timeline) derived;
}

spine_transform_constraint_timeline spine_constraint_timeline_cast_to_transform_constraint_timeline(spine_constraint_timeline obj) {
	ConstraintTimeline *base = (ConstraintTimeline *) obj;
	TransformConstraintTimeline *derived = static_cast<TransformConstraintTimeline *>(base);
	return (spine_transform_constraint_timeline) derived;
}

spine_deform_timeline spine_slot_curve_timeline_cast_to_deform_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *base = (SlotCurveTimeline *) obj;
	DeformTimeline *derived = static_cast<DeformTimeline *>(base);
	return (spine_deform_timeline) derived;
}

spine_rgb2_timeline spine_slot_curve_timeline_cast_to_rgb2_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *base = (SlotCurveTimeline *) obj;
	RGB2Timeline *derived = static_cast<RGB2Timeline *>(base);
	return (spine_rgb2_timeline) derived;
}

spine_rgba2_timeline spine_slot_curve_timeline_cast_to_rgba2_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *base = (SlotCurveTimeline *) obj;
	RGBA2Timeline *derived = static_cast<RGBA2Timeline *>(base);
	return (spine_rgba2_timeline) derived;
}

spine_rgba_timeline spine_slot_curve_timeline_cast_to_rgba_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *base = (SlotCurveTimeline *) obj;
	RGBATimeline *derived = static_cast<RGBATimeline *>(base);
	return (spine_rgba_timeline) derived;
}

spine_rgb_timeline spine_slot_curve_timeline_cast_to_rgb_timeline(spine_slot_curve_timeline obj) {
	SlotCurveTimeline *base = (SlotCurveTimeline *) obj;
	RGBTimeline *derived = static_cast<RGBTimeline *>(base);
	return (spine_rgb_timeline) derived;
}

spine_from_rotate spine_from_property_cast_to_from_rotate(spine_from_property obj) {
	FromProperty *base = (FromProperty *) obj;
	FromRotate *derived = static_cast<FromRotate *>(base);
	return (spine_from_rotate) derived;
}

spine_from_scale_x spine_from_property_cast_to_from_scale_x(spine_from_property obj) {
	FromProperty *base = (FromProperty *) obj;
	FromScaleX *derived = static_cast<FromScaleX *>(base);
	return (spine_from_scale_x) derived;
}

spine_from_scale_y spine_from_property_cast_to_from_scale_y(spine_from_property obj) {
	FromProperty *base = (FromProperty *) obj;
	FromScaleY *derived = static_cast<FromScaleY *>(base);
	return (spine_from_scale_y) derived;
}

spine_from_shear_y spine_from_property_cast_to_from_shear_y(spine_from_property obj) {
	FromProperty *base = (FromProperty *) obj;
	FromShearY *derived = static_cast<FromShearY *>(base);
	return (spine_from_shear_y) derived;
}

spine_from_x spine_from_property_cast_to_from_x(spine_from_property obj) {
	FromProperty *base = (FromProperty *) obj;
	FromX *derived = static_cast<FromX *>(base);
	return (spine_from_x) derived;
}

spine_from_y spine_from_property_cast_to_from_y(spine_from_property obj) {
	FromProperty *base = (FromProperty *) obj;
	FromY *derived = static_cast<FromY *>(base);
	return (spine_from_y) derived;
}

spine_ik_constraint spine_ik_constraint_base_cast_to_ik_constraint(spine_ik_constraint_base obj) {
	IkConstraintBase *base = (IkConstraintBase *) obj;
	IkConstraint *derived = static_cast<IkConstraint *>(base);
	return (spine_ik_constraint) derived;
}

spine_ik_constraint spine_constraint_cast_to_ik_constraint(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	IkConstraint *derived = static_cast<IkConstraint *>(base);
	return (spine_ik_constraint) derived;
}

spine_ik_constraint_base spine_constraint_cast_to_ik_constraint_base(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	IkConstraintBase *derived = static_cast<IkConstraintBase *>(base);
	return (spine_ik_constraint_base) derived;
}

spine_path_constraint spine_constraint_cast_to_path_constraint(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	PathConstraint *derived = static_cast<PathConstraint *>(base);
	return (spine_path_constraint) derived;
}

spine_path_constraint_base spine_constraint_cast_to_path_constraint_base(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	PathConstraintBase *derived = static_cast<PathConstraintBase *>(base);
	return (spine_path_constraint_base) derived;
}

spine_physics_constraint spine_constraint_cast_to_physics_constraint(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	PhysicsConstraint *derived = static_cast<PhysicsConstraint *>(base);
	return (spine_physics_constraint) derived;
}

spine_physics_constraint_base spine_constraint_cast_to_physics_constraint_base(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	PhysicsConstraintBase *derived = static_cast<PhysicsConstraintBase *>(base);
	return (spine_physics_constraint_base) derived;
}

spine_slider spine_constraint_cast_to_slider(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	Slider *derived = static_cast<Slider *>(base);
	return (spine_slider) derived;
}

spine_slider_base spine_constraint_cast_to_slider_base(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	SliderBase *derived = static_cast<SliderBase *>(base);
	return (spine_slider_base) derived;
}

spine_transform_constraint spine_constraint_cast_to_transform_constraint(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	TransformConstraint *derived = static_cast<TransformConstraint *>(base);
	return (spine_transform_constraint) derived;
}

spine_transform_constraint_base spine_constraint_cast_to_transform_constraint_base(spine_constraint obj) {
	Constraint *base = (Constraint *) obj;
	TransformConstraintBase *derived = static_cast<TransformConstraintBase *>(base);
	return (spine_transform_constraint_base) derived;
}

spine_ik_constraint_data spine_constraint_data_cast_to_ik_constraint_data(spine_constraint_data obj) {
	ConstraintData *base = (ConstraintData *) obj;
	IkConstraintData *derived = static_cast<IkConstraintData *>(base);
	return (spine_ik_constraint_data) derived;
}

spine_path_constraint_data spine_constraint_data_cast_to_path_constraint_data(spine_constraint_data obj) {
	ConstraintData *base = (ConstraintData *) obj;
	PathConstraintData *derived = static_cast<PathConstraintData *>(base);
	return (spine_path_constraint_data) derived;
}

spine_physics_constraint_data spine_constraint_data_cast_to_physics_constraint_data(spine_constraint_data obj) {
	ConstraintData *base = (ConstraintData *) obj;
	PhysicsConstraintData *derived = static_cast<PhysicsConstraintData *>(base);
	return (spine_physics_constraint_data) derived;
}

spine_slider_data spine_constraint_data_cast_to_slider_data(spine_constraint_data obj) {
	ConstraintData *base = (ConstraintData *) obj;
	SliderData *derived = static_cast<SliderData *>(base);
	return (spine_slider_data) derived;
}

spine_transform_constraint_data spine_constraint_data_cast_to_transform_constraint_data(spine_constraint_data obj) {
	ConstraintData *base = (ConstraintData *) obj;
	TransformConstraintData *derived = static_cast<TransformConstraintData *>(base);
	return (spine_transform_constraint_data) derived;
}

spine_path_constraint spine_path_constraint_base_cast_to_path_constraint(spine_path_constraint_base obj) {
	PathConstraintBase *base = (PathConstraintBase *) obj;
	PathConstraint *derived = static_cast<PathConstraint *>(base);
	return (spine_path_constraint) derived;
}

spine_path_constraint_position_timeline spine_constraint_timeline1_cast_to_path_constraint_position_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *base = (ConstraintTimeline1 *) obj;
	PathConstraintPositionTimeline *derived = static_cast<PathConstraintPositionTimeline *>(base);
	return (spine_path_constraint_position_timeline) derived;
}

spine_path_constraint_spacing_timeline spine_constraint_timeline1_cast_to_path_constraint_spacing_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *base = (ConstraintTimeline1 *) obj;
	PathConstraintSpacingTimeline *derived = static_cast<PathConstraintSpacingTimeline *>(base);
	return (spine_path_constraint_spacing_timeline) derived;
}

spine_slider_mix_timeline spine_constraint_timeline1_cast_to_slider_mix_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *base = (ConstraintTimeline1 *) obj;
	SliderMixTimeline *derived = static_cast<SliderMixTimeline *>(base);
	return (spine_slider_mix_timeline) derived;
}

spine_slider_timeline spine_constraint_timeline1_cast_to_slider_timeline(spine_constraint_timeline1 obj) {
	ConstraintTimeline1 *base = (ConstraintTimeline1 *) obj;
	SliderTimeline *derived = static_cast<SliderTimeline *>(base);
	return (spine_slider_timeline) derived;
}

spine_physics_constraint spine_physics_constraint_base_cast_to_physics_constraint(spine_physics_constraint_base obj) {
	PhysicsConstraintBase *base = (PhysicsConstraintBase *) obj;
	PhysicsConstraint *derived = static_cast<PhysicsConstraint *>(base);
	return (spine_physics_constraint) derived;
}

spine_physics_constraint_damping_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_damping_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintDampingTimeline *derived = static_cast<PhysicsConstraintDampingTimeline *>(base);
	return (spine_physics_constraint_damping_timeline) derived;
}

spine_physics_constraint_gravity_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_gravity_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintGravityTimeline *derived = static_cast<PhysicsConstraintGravityTimeline *>(base);
	return (spine_physics_constraint_gravity_timeline) derived;
}

spine_physics_constraint_inertia_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_inertia_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintInertiaTimeline *derived = static_cast<PhysicsConstraintInertiaTimeline *>(base);
	return (spine_physics_constraint_inertia_timeline) derived;
}

spine_physics_constraint_mass_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_mass_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintMassTimeline *derived = static_cast<PhysicsConstraintMassTimeline *>(base);
	return (spine_physics_constraint_mass_timeline) derived;
}

spine_physics_constraint_mix_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_mix_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintMixTimeline *derived = static_cast<PhysicsConstraintMixTimeline *>(base);
	return (spine_physics_constraint_mix_timeline) derived;
}

spine_physics_constraint_strength_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_strength_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintStrengthTimeline *derived = static_cast<PhysicsConstraintStrengthTimeline *>(base);
	return (spine_physics_constraint_strength_timeline) derived;
}

spine_physics_constraint_wind_timeline spine_physics_constraint_timeline_cast_to_physics_constraint_wind_timeline(
	spine_physics_constraint_timeline obj) {
	PhysicsConstraintTimeline *base = (PhysicsConstraintTimeline *) obj;
	PhysicsConstraintWindTimeline *derived = static_cast<PhysicsConstraintWindTimeline *>(base);
	return (spine_physics_constraint_wind_timeline) derived;
}

spine_rotate_timeline spine_bone_timeline1_cast_to_rotate_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	RotateTimeline *derived = static_cast<RotateTimeline *>(base);
	return (spine_rotate_timeline) derived;
}

spine_scale_x_timeline spine_bone_timeline1_cast_to_scale_x_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	ScaleXTimeline *derived = static_cast<ScaleXTimeline *>(base);
	return (spine_scale_x_timeline) derived;
}

spine_scale_y_timeline spine_bone_timeline1_cast_to_scale_y_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	ScaleYTimeline *derived = static_cast<ScaleYTimeline *>(base);
	return (spine_scale_y_timeline) derived;
}

spine_shear_x_timeline spine_bone_timeline1_cast_to_shear_x_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	ShearXTimeline *derived = static_cast<ShearXTimeline *>(base);
	return (spine_shear_x_timeline) derived;
}

spine_shear_y_timeline spine_bone_timeline1_cast_to_shear_y_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	ShearYTimeline *derived = static_cast<ShearYTimeline *>(base);
	return (spine_shear_y_timeline) derived;
}

spine_translate_x_timeline spine_bone_timeline1_cast_to_translate_x_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	TranslateXTimeline *derived = static_cast<TranslateXTimeline *>(base);
	return (spine_translate_x_timeline) derived;
}

spine_translate_y_timeline spine_bone_timeline1_cast_to_translate_y_timeline(spine_bone_timeline1 obj) {
	BoneTimeline1 *base = (BoneTimeline1 *) obj;
	TranslateYTimeline *derived = static_cast<TranslateYTimeline *>(base);
	return (spine_translate_y_timeline) derived;
}

spine_scale_timeline spine_bone_timeline2_cast_to_scale_timeline(spine_bone_timeline2 obj) {
	BoneTimeline2 *base = (BoneTimeline2 *) obj;
	ScaleTimeline *derived = static_cast<ScaleTimeline *>(base);
	return (spine_scale_timeline) derived;
}

spine_shear_timeline spine_bone_timeline2_cast_to_shear_timeline(spine_bone_timeline2 obj) {
	BoneTimeline2 *base = (BoneTimeline2 *) obj;
	ShearTimeline *derived = static_cast<ShearTimeline *>(base);
	return (spine_shear_timeline) derived;
}

spine_translate_timeline spine_bone_timeline2_cast_to_translate_timeline(spine_bone_timeline2 obj) {
	BoneTimeline2 *base = (BoneTimeline2 *) obj;
	TranslateTimeline *derived = static_cast<TranslateTimeline *>(base);
	return (spine_translate_timeline) derived;
}

spine_slider spine_slider_base_cast_to_slider(spine_slider_base obj) {
	SliderBase *base = (SliderBase *) obj;
	Slider *derived = static_cast<Slider *>(base);
	return (spine_slider) derived;
}

spine_to_rotate spine_to_property_cast_to_to_rotate(spine_to_property obj) {
	ToProperty *base = (ToProperty *) obj;
	ToRotate *derived = static_cast<ToRotate *>(base);
	return (spine_to_rotate) derived;
}

spine_to_scale_x spine_to_property_cast_to_to_scale_x(spine_to_property obj) {
	ToProperty *base = (ToProperty *) obj;
	ToScaleX *derived = static_cast<ToScaleX *>(base);
	return (spine_to_scale_x) derived;
}

spine_to_scale_y spine_to_property_cast_to_to_scale_y(spine_to_property obj) {
	ToProperty *base = (ToProperty *) obj;
	ToScaleY *derived = static_cast<ToScaleY *>(base);
	return (spine_to_scale_y) derived;
}

spine_to_shear_y spine_to_property_cast_to_to_shear_y(spine_to_property obj) {
	ToProperty *base = (ToProperty *) obj;
	ToShearY *derived = static_cast<ToShearY *>(base);
	return (spine_to_shear_y) derived;
}

spine_to_x spine_to_property_cast_to_to_x(spine_to_property obj) {
	ToProperty *base = (ToProperty *) obj;
	ToX *derived = static_cast<ToX *>(base);
	return (spine_to_x) derived;
}

spine_to_y spine_to_property_cast_to_to_y(spine_to_property obj) {
	ToProperty *base = (ToProperty *) obj;
	ToY *derived = static_cast<ToY *>(base);
	return (spine_to_y) derived;
}

spine_transform_constraint spine_transform_constraint_base_cast_to_transform_constraint(spine_transform_constraint_base obj) {
	TransformConstraintBase *base = (TransformConstraintBase *) obj;
	TransformConstraint *derived = static_cast<TransformConstraint *>(base);
	return (spine_transform_constraint) derived;
}
