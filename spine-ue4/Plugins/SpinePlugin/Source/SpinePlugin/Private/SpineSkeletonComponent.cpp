/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

USpineSkeletonComponent::USpineSkeletonComponent () {
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

bool USpineSkeletonComponent::SetSkin(const FString& skinName) {
	CheckState();
	if (skeleton) return spSkeleton_setSkinByName(skeleton, TCHAR_TO_UTF8(*skinName)) != 0;
	else return false;
}

bool USpineSkeletonComponent::SetAttachment (const FString& slotName, const FString& attachmentName) {
	CheckState();
	if (skeleton) return spSkeleton_setAttachment(skeleton, TCHAR_TO_UTF8(*slotName), TCHAR_TO_UTF8(*attachmentName)) != 0;
	return false;
}

FTransform USpineSkeletonComponent::GetBoneWorldTransform (const FString& BoneName) {
	CheckState();
	if (skeleton) {
		spBone* bone = spSkeleton_findBone(skeleton, TCHAR_TO_UTF8(*BoneName));		
		if (!bone) return FTransform();
		if (!bone->appliedValid) this->InternalTick(0, false);		

		// Need to fetch the renderer component to get world transform of actor plus
		// offset by renderer component and its parent component(s). If no renderer
		// component is found, this components owner's transform is used as a fallback
		FTransform baseTransform;
		AActor* owner = GetOwner();
		if (owner) {
			USpineSkeletonRendererComponent* rendererComponent = static_cast<USpineSkeletonRendererComponent*>(owner->GetComponentByClass(USpineSkeletonRendererComponent::StaticClass()));
			if (rendererComponent) baseTransform = rendererComponent->GetComponentTransform();
			else baseTransform = owner->GetActorTransform();
		}

		FVector position(bone->worldX, 0, bone->worldY);
		FMatrix localTransform;
		localTransform.SetIdentity();
		localTransform.SetAxis(2, FVector(bone->a, 0, bone->c));
		localTransform.SetAxis(0, FVector(bone->b, 0, bone->d));
		localTransform.SetOrigin(FVector(bone->worldX, 0, bone->worldY));				
		localTransform = localTransform * baseTransform.ToMatrixWithScale();

		FTransform result;
		result.SetFromMatrix(localTransform);		
		return result;
	}
	return FTransform();
}

void USpineSkeletonComponent::SetBoneWorldPosition (const FString& BoneName, const FVector& position) {
	CheckState();
	if (skeleton) {
		spBone* bone = spSkeleton_findBone(skeleton, TCHAR_TO_UTF8(*BoneName));
		if (!bone) return;
		if (!bone->appliedValid) this->InternalTick(0, false);

		// Need to fetch the renderer component to get world transform of actor plus
		// offset by renderer component and its parent component(s). If no renderer
		// component is found, this components owner's transform is used as a fallback
		FTransform baseTransform;
		AActor* owner = GetOwner();
		if (owner) {
			USpineSkeletonRendererComponent* rendererComponent = static_cast<USpineSkeletonRendererComponent*>(owner->GetComponentByClass(USpineSkeletonRendererComponent::StaticClass()));
			if (rendererComponent) baseTransform = rendererComponent->GetComponentTransform();
			else baseTransform = owner->GetActorTransform();
		}

		baseTransform = baseTransform.Inverse();
		FVector localPosition = baseTransform.TransformPosition(position);
		float localX = 0, localY = 0;
		if (bone->parent) {
			spBone_worldToLocal(bone->parent, localPosition.X, localPosition.Z, &localX, &localY);
		} else {
			spBone_worldToLocal(bone, localPosition.X, localPosition.Z, &localX, &localY);
		}
		bone->x = localX;
		bone->y = localY;
	}
}

void USpineSkeletonComponent::UpdateWorldTransform() {
	CheckState();
	if (skeleton) {
		spSkeleton_updateWorldTransform(skeleton);
	}
}

void USpineSkeletonComponent::SetToSetupPose () {
	CheckState();
	if (skeleton) spSkeleton_setToSetupPose(skeleton);
}

void USpineSkeletonComponent::SetBonesToSetupPose () {
	CheckState();
	if (skeleton) spSkeleton_setBonesToSetupPose(skeleton);
}

void USpineSkeletonComponent::SetSlotsToSetupPose () {
	CheckState();
	if (skeleton) spSkeleton_setSlotsToSetupPose(skeleton);
}

void USpineSkeletonComponent::SetFlipX (bool flipX) {
	CheckState();
	if (skeleton) skeleton->flipX = flipX ? 1 : 0;
}

bool USpineSkeletonComponent::GetFlipX() {
	CheckState();
	if (skeleton) return skeleton->flipX != 0;
	return false;
}

void USpineSkeletonComponent::SetFlipY(bool flipY) {
	CheckState();
	if (skeleton) skeleton->flipY = flipY ? 1 : 0;
}

bool USpineSkeletonComponent::GetFlipY() {
	CheckState();
	if (skeleton) return skeleton->flipY != 0;
	return false;
}

void USpineSkeletonComponent::BeginPlay() {
	Super::BeginPlay();
}

void USpineSkeletonComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	InternalTick(DeltaTime);
}

void USpineSkeletonComponent::InternalTick(float DeltaTime, bool CallDelegates) {
	CheckState();

	if (skeleton) {
		if (CallDelegates) BeforeUpdateWorldTransform.Broadcast(this);
		spSkeleton_updateWorldTransform(skeleton);
		if (CallDelegates) AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonComponent::CheckState () {
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();
		
		if (Atlas && SkeletonData) {
			spSkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			skeleton = spSkeleton_create(data);
		}
		
		lastAtlas = Atlas;
		lastData = SkeletonData;
	}
}

void USpineSkeletonComponent::DisposeState () {
	if (skeleton) {
		spSkeleton_dispose(skeleton);
		skeleton = nullptr;
	}
}

void USpineSkeletonComponent::FinishDestroy () {
	DisposeState();
	Super::FinishDestroy();
}

#undef LOCTEXT_NAMESPACE
