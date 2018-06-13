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

#include "spine/spine.h"

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

USpineSkeletonComponent::USpineSkeletonComponent () {
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

bool USpineSkeletonComponent::SetSkin(const FString& skinName) {
	CheckState();
	if (skeleton) {
		Skin* skin = skeleton->getData()->findSkin(TCHAR_TO_UTF8(*skinName));
		if (!skin) return false;
		skeleton->setSkin(skin);
		return true;
	}
	else return false;
}

bool USpineSkeletonComponent::SetAttachment (const FString& slotName, const FString& attachmentName) {
	CheckState();
	if (skeleton) {
		if (!skeleton->getAttachment(TCHAR_TO_UTF8(*slotName), TCHAR_TO_UTF8(*attachmentName))) return false;
		skeleton->setAttachment(TCHAR_TO_UTF8(*slotName), TCHAR_TO_UTF8(*attachmentName));
		return true;
	}
	return false;
}

FTransform USpineSkeletonComponent::GetBoneWorldTransform (const FString& BoneName) {
	CheckState();
	if (skeleton) {
		Bone* bone = skeleton->findBone(TCHAR_TO_UTF8(*BoneName));		
		if (!bone) return FTransform();
		if (!bone->isAppliedValid()) this->InternalTick(0, false);		

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

		FVector position(bone->getWorldX(), 0, bone->getWorldY());
		FMatrix localTransform;
		localTransform.SetIdentity();
		localTransform.SetAxis(2, FVector(bone->getA(), 0, bone->getC()));
		localTransform.SetAxis(0, FVector(bone->getB(), 0, bone->getD()));
		localTransform.SetOrigin(FVector(bone->getWorldX(), 0, bone->getWorldY()));				
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
		Bone* bone = skeleton->findBone(TCHAR_TO_UTF8(*BoneName));
		if (!bone) return;
		if (!bone->isAppliedValid()) this->InternalTick(0, false);

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
		if (bone->getParent()) {
			bone->getParent()->worldToLocal(localPosition.X, localPosition.Z, localX, localY);
		} else {
			bone->worldToLocal(localPosition.X, localPosition.Z, localX, localY);
		}
		bone->setX(localX);
		bone->setY(localY);
	}
}

void USpineSkeletonComponent::UpdateWorldTransform() {
	CheckState();
	if (skeleton) {
		skeleton->updateWorldTransform();
	}
}

void USpineSkeletonComponent::SetToSetupPose () {
	CheckState();
	if (skeleton) skeleton->setToSetupPose();
}

void USpineSkeletonComponent::SetBonesToSetupPose () {
	CheckState();
	if (skeleton) skeleton->setBonesToSetupPose();
}

void USpineSkeletonComponent::SetSlotsToSetupPose () {
	CheckState();
	if (skeleton) skeleton->setSlotsToSetupPose();
}

void USpineSkeletonComponent::SetFlipX (bool flipX) {
	CheckState();
	if (skeleton) skeleton->setFlipX(flipX);
}

bool USpineSkeletonComponent::GetFlipX() {
	CheckState();
	if (skeleton) return skeleton->getFlipX();
	return false;
}

void USpineSkeletonComponent::SetFlipY(bool flipY) {
	CheckState();
	if (skeleton) skeleton->setFlipY(flipY);
}

bool USpineSkeletonComponent::GetFlipY() {
	CheckState();
	if (skeleton) return skeleton->getFlipY();
	return false;
}

void USpineSkeletonComponent::GetBones(TArray<FString> &Bones) {
	CheckState();
	if (skeleton) {
		for (size_t i = 0, n = skeleton->getBones().size(); i < n; i++) {
			Bones.Add(skeleton->getBones()[i]->getData().getName().buffer());
		}
	}
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
		skeleton->updateWorldTransform();
		if (CallDelegates) AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonComponent::CheckState () {
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();
		
		if (Atlas && SkeletonData) {
			spine::SkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			skeleton = new (__FILE__, __LINE__) Skeleton(data);
		}
		
		lastAtlas = Atlas;
		lastData = SkeletonData;
	}
}

void USpineSkeletonComponent::DisposeState () {
	if (skeleton) {
		delete skeleton;
		skeleton = nullptr;
	}
}

void USpineSkeletonComponent::FinishDestroy () {
	DisposeState();
	Super::FinishDestroy();
}

#undef LOCTEXT_NAMESPACE
