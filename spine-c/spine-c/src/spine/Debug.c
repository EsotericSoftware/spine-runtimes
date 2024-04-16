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

#include <spine/Animation.h>
#include <spine/Debug.h>

#include <stdio.h>

static const char *_spTimelineTypeNames[] = {
		"Attachment",
		"Alpha",
		"PathConstraintPosition",
		"PathConstraintSpace",
		"Rotate",
		"ScaleX",
		"ScaleY",
		"ShearX",
		"ShearY",
		"TranslateX",
		"TranslateY",
		"Scale",
		"Shear",
		"Translate",
		"Deform",
		"IkConstraint",
		"PathConstraintMix",
		"Rgb2",
		"Rgba2",
		"Rgba",
		"Rgb",
		"TransformConstraint",
		"DrawOrder",
		"Event"};

void spDebug_printSkeletonData(spSkeletonData *skeletonData) {
	int i, n;
	spDebug_printBoneDatas(skeletonData->bones, skeletonData->bonesCount);

	for (i = 0, n = skeletonData->animationsCount; i < n; i++) {
		spDebug_printAnimation(skeletonData->animations[i]);
	}
}

void _spDebug_printTimelineBase(spTimeline *timeline) {
	printf("   Timeline %s:\n", _spTimelineTypeNames[timeline->type]);
	printf("      frame count: %i\n", timeline->frameCount);
	printf("      frame entries: %i\n", timeline->frameEntries);
	printf("      frames: ");
	spDebug_printFloats(timeline->frames->items, timeline->frames->size);
	printf("\n");
}

void _spDebug_printCurveTimeline(spCurveTimeline *timeline) {
	_spDebug_printTimelineBase(&timeline->super);
	printf("      curves: ");
	spDebug_printFloats(timeline->curves->items, timeline->curves->size);
	printf("\n");
}

void spDebug_printTimeline(spTimeline *timeline) {
	switch (timeline->type) {
		case SP_TIMELINE_ATTACHMENT: {
			spAttachmentTimeline *t = (spAttachmentTimeline *) timeline;
			_spDebug_printTimelineBase(&t->super);
			break;
		}
		case SP_TIMELINE_ALPHA: {
			spAlphaTimeline *t = (spAlphaTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_PATHCONSTRAINTPOSITION: {
			spPathConstraintPositionTimeline *t = (spPathConstraintPositionTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_PATHCONSTRAINTSPACING: {
			spPathConstraintMixTimeline *t = (spPathConstraintMixTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_ROTATE: {
			spRotateTimeline *t = (spRotateTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_SCALEX: {
			spScaleXTimeline *t = (spScaleXTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_SCALEY: {
			spScaleYTimeline *t = (spScaleYTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_SHEARX: {
			spShearXTimeline *t = (spShearXTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_SHEARY: {
			spShearYTimeline *t = (spShearYTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_TRANSLATEX: {
			spTranslateXTimeline *t = (spTranslateXTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_TRANSLATEY: {
			spTranslateYTimeline *t = (spTranslateYTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_SCALE: {
			spScaleTimeline *t = (spScaleTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_SHEAR: {
			spShearTimeline *t = (spShearTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_TRANSLATE: {
			spTranslateTimeline *t = (spTranslateTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_DEFORM: {
			spDeformTimeline *t = (spDeformTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_IKCONSTRAINT: {
			spIkConstraintTimeline *t = (spIkConstraintTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_PATHCONSTRAINTMIX: {
			spPathConstraintMixTimeline *t = (spPathConstraintMixTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_RGB2: {
			spRGB2Timeline *t = (spRGB2Timeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_RGBA2: {
			spRGBA2Timeline *t = (spRGBA2Timeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_RGBA: {
			spRGBATimeline *t = (spRGBATimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_RGB: {
			spRGBTimeline *t = (spRGBTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_TRANSFORMCONSTRAINT: {
			spTransformConstraintTimeline *t = (spTransformConstraintTimeline *) timeline;
			_spDebug_printCurveTimeline(&t->super);
			break;
		}
		case SP_TIMELINE_DRAWORDER: {
			spDrawOrderTimeline *t = (spDrawOrderTimeline *) timeline;
			_spDebug_printTimelineBase(&t->super);
			break;
		}
		case SP_TIMELINE_EVENT: {
			spEventTimeline *t = (spEventTimeline *) timeline;
			_spDebug_printTimelineBase(&t->super);
			break;
		}
		case SP_TIMELINE_SEQUENCE: {
			spSequenceTimeline *t = (spSequenceTimeline *) timeline;
			_spDebug_printTimelineBase(&t->super);
		}
		case SP_TIMELINE_INHERIT: {
			spInheritTimeline *t = (spInheritTimeline *) timeline;
			_spDebug_printTimelineBase(&t->super);
		}
		default: {
			_spDebug_printTimelineBase(timeline);
		}
	}
}

void spDebug_printAnimation(spAnimation *animation) {
	int i, n;
	printf("Animation %s: %i timelines\n", animation->name, animation->timelines->size);

	for (i = 0, n = animation->timelines->size; i < n; i++) {
		spDebug_printTimeline(animation->timelines->items[i]);
	}
}

void spDebug_printBoneDatas(spBoneData **boneDatas, int numBoneDatas) {
	int i;
	for (i = 0; i < numBoneDatas; i++) {
		spDebug_printBoneData(boneDatas[i]);
	}
}

void spDebug_printBoneData(spBoneData *boneData) {
	printf("Bone data %s: %f, %f, %f, %f, %f, %f %f\n", boneData->name, boneData->rotation, boneData->scaleX,
		   boneData->scaleY, boneData->x, boneData->y, boneData->shearX, boneData->shearY);
}

void spDebug_printSkeleton(spSkeleton *skeleton) {
	spDebug_printBones(skeleton->bones, skeleton->bonesCount);
}

void spDebug_printBones(spBone **bones, int numBones) {
	int i;
	for (i = 0; i < numBones; i++) {
		spDebug_printBone(bones[i]);
	}
}

void spDebug_printBone(spBone *bone) {
	printf("Bone %s: %f, %f, %f, %f, %f, %f\n", bone->data->name, bone->a, bone->b, bone->c, bone->d, bone->worldX,
		   bone->worldY);
}

void spDebug_printFloats(float *values, int numFloats) {
	int i;
	printf("(%i) [", numFloats);
	for (i = 0; i < numFloats; i++) {
		printf("%f, ", values[i]);
	}
	printf("]");
}
