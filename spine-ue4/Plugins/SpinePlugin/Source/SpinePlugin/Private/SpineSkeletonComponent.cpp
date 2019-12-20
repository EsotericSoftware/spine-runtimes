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

#include "SpinePluginPrivatePCH.h"

#include "spine/spine.h"

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

USpineSkeletonComponent::USpineSkeletonComponent () {
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

bool USpineSkeletonComponent::SetSkins(UPARAM(ref) TArray<FString>& SkinNames) {
	CheckState();
	if (skeleton) {
		spine::Skin* newSkin = new spine::Skin("__spine-ue3_custom_skin");
		for (auto& skinName : SkinNames) {
			spine::Skin* skin = skeleton->getData()->findSkin(TCHAR_TO_UTF8(*skinName));
			if (!skin) {
				delete newSkin;
				return false;
			}
			newSkin->addSkin(skin);
		}
		skeleton->setSkin(newSkin);
		if (customSkin != nullptr) {
			delete customSkin;
		}
		customSkin = newSkin;
		return true;
	}
	else return false;
}

bool USpineSkeletonComponent::SetSkin (const FString skinName) {
	CheckState();
	if (skeleton) {
		Skin* skin = skeleton->getData()->findSkin(TCHAR_TO_UTF8(*skinName));
		if (!skin) return false;
		skeleton->setSkin(skin);
		return true;
	}
	else return false;
}

void USpineSkeletonComponent::GetSkins (TArray<FString> &Skins) {
	CheckState();
	if (skeleton) {
		for (size_t i = 0, n = skeleton->getData()->getSkins().size(); i < n; i++) {
			Skins.Add(skeleton->getData()->getSkins()[i]->getName().buffer());
		}
	}
}

bool USpineSkeletonComponent::HasSkin (const FString skinName) {
	CheckState();
	if (skeleton) {
		return skeleton->getData()->findAnimation(TCHAR_TO_UTF8(*skinName)) != nullptr;
	}
	return false;
}

bool USpineSkeletonComponent::SetAttachment (const FString slotName, const FString attachmentName) {
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

void USpineSkeletonComponent::UpdateWorldTransform () {
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

void USpineSkeletonComponent::SetScaleX (float scaleX) {
	CheckState();
	if (skeleton) skeleton->setScaleX(scaleX);
}

float USpineSkeletonComponent::GetScaleX () {
	CheckState();
	if (skeleton) return skeleton->getScaleX();
	return 1;
}

void USpineSkeletonComponent::SetScaleY (float scaleY) {
	CheckState();
	if (skeleton) skeleton->setScaleY(scaleY);
}

float USpineSkeletonComponent::GetScaleY () {
	CheckState();
	if (skeleton) return skeleton->getScaleY();
	return 1;
}

void USpineSkeletonComponent::GetBones (TArray<FString> &Bones) {
	CheckState();
	if (skeleton) {
		for (size_t i = 0, n = skeleton->getBones().size(); i < n; i++) {
			Bones.Add(skeleton->getBones()[i]->getData().getName().buffer());
		}
	}
}

bool USpineSkeletonComponent::HasBone (const FString BoneName) {
	CheckState();
	if (skeleton) {
		return skeleton->getData()->findBone(TCHAR_TO_UTF8(*BoneName)) != nullptr;
	}
	return false;
}

void USpineSkeletonComponent::GetSlots (TArray<FString> &Slots) {
	CheckState();
	if (skeleton) {
		for (size_t i = 0, n = skeleton->getSlots().size(); i < n; i++) {
			Slots.Add(skeleton->getSlots()[i]->getData().getName().buffer());
		}
	}
}

bool USpineSkeletonComponent::HasSlot (const FString SlotName) {
	CheckState();
	if (skeleton) {
		return skeleton->getData()->findSlot(TCHAR_TO_UTF8(*SlotName)) != nullptr;
	}
	return false;
}

void USpineSkeletonComponent::GetAnimations(TArray<FString> &Animations) {
	CheckState();
	if (skeleton) {
		for (size_t i = 0, n = skeleton->getData()->getAnimations().size(); i < n; i++) {
			Animations.Add(skeleton->getData()->getAnimations()[i]->getName().buffer());
		}
	}
}

bool USpineSkeletonComponent::HasAnimation(FString AnimationName) {
	CheckState();
	if (skeleton) {
		return skeleton->getData()->findAnimation(TCHAR_TO_UTF8(*AnimationName)) != nullptr;
	}
	return false;
}

float USpineSkeletonComponent::GetAnimationDuration(FString AnimationName) {
	CheckState();
	if (skeleton) {
		Animation *animation = skeleton->getData()->findAnimation(TCHAR_TO_UTF8(*AnimationName));
		if (animation == nullptr) return 0;
		else return animation->getDuration();
	}
	return 0;
}

void USpineSkeletonComponent::BeginPlay() {
	Super::BeginPlay();
}

void USpineSkeletonComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	InternalTick(DeltaTime);
}

void USpineSkeletonComponent::InternalTick(float DeltaTime, bool CallDelegates, bool Preview) {
	CheckState();

	if (skeleton) {
		if (CallDelegates) BeforeUpdateWorldTransform.Broadcast(this);
		skeleton->updateWorldTransform();
		if (CallDelegates) AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonComponent::CheckState () {
	bool needsUpdate = lastAtlas != Atlas || lastData != SkeletonData;

	if (!needsUpdate) {
		// Are we doing a re-import? Then check if the underlying spine-cpp data
		// has changed.
		if (lastAtlas && lastAtlas == Atlas && lastData && lastData == SkeletonData) {
			spine::Atlas* atlas = Atlas->GetAtlas();
			if (lastSpineAtlas != atlas) {
				needsUpdate = true;
			}
			if (skeleton && skeleton->getData() != SkeletonData->GetSkeletonData(atlas)) {
				needsUpdate = true;
			}
		}
	}

	if (needsUpdate) {
		DisposeState();
		
		if (Atlas && SkeletonData) {
			spine::SkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas());
			skeleton = new (__FILE__, __LINE__) Skeleton(data);
		}
		
		lastAtlas = Atlas;
		lastSpineAtlas = Atlas ? Atlas->GetAtlas() : nullptr;
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
