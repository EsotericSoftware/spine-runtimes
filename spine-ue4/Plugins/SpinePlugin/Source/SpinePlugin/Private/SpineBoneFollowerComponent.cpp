#include "SpinePluginPrivatePCH.h"

USpineBoneFollowerComponent::USpineBoneFollowerComponent () {	
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineBoneFollowerComponent::BeginPlay () {
	Super::BeginPlay();
}

void USpineBoneFollowerComponent::TickComponent ( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction ) {
	Super::TickComponent( DeltaTime, TickType, ThisTickFunction );

	AActor* owner = GetOwner();
	if (Target && owner) {
		USpineSkeletonComponent* skeleton = static_cast<USpineSkeletonComponent*>(Target->GetComponentByClass(USpineSkeletonComponent::StaticClass()));
		if (skeleton) {
			FTransform transform = skeleton->GetBoneWorldTransform(BoneName);
			if (UsePosition) owner->SetActorLocation(transform.GetLocation());
			if (UseRotation) owner->SetActorRotation(transform.GetRotation());
			if (UseScale) owner->SetActorScale3D(transform.GetScale3D());
		}
	}
}

