#include "SpinePluginPrivatePCH.h"

USpineBoneDriverComponent::USpineBoneDriverComponent () {
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineBoneDriverComponent::BeginPlay () {
	Super::BeginPlay();
}

void USpineBoneDriverComponent::BeforeUpdateWorldTransform(USpineSkeletonComponent* skeleton) {	
	AActor* owner = GetOwner();
	if (owner) {		
		skeleton->SetBoneWorldPosition(BoneName, owner->GetActorLocation() );
	}
}

void USpineBoneDriverComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (Target) {
		USpineSkeletonComponent* skeleton = static_cast<USpineSkeletonComponent*>(Target->GetComponentByClass(USpineSkeletonComponent::StaticClass()));
		if (skeleton != lastBoundComponent) {
			// if (lastBoundComponent) lastBoundComponent->BeforeUpdateWorldTransform.RemoveAll(this);
			skeleton->BeforeUpdateWorldTransform.AddDynamic(this, &USpineBoneDriverComponent::BeforeUpdateWorldTransform);
			lastBoundComponent = skeleton;
		}		
	}
}

