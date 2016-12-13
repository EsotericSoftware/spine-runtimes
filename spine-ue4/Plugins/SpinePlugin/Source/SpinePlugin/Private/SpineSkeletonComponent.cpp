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

bool USpineSkeletonComponent::setAttachment (const FString& slotName, const FString& attachmentName) {
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
