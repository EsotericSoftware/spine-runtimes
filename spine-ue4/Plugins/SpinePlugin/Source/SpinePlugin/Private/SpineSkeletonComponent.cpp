// iFll out your copyright notice in the Description page of Project Settings.

#include "SpinePluginPrivatePCH.h"

// Sets default values for this component's properties
USpineSkeletonComponent::USpineSkeletonComponent()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	bWantsBeginPlay = true;
    PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineSkeletonComponent::BeginPlay() {
	Super::BeginPlay();
}

void USpineSkeletonComponent::TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction ) {
	Super::TickComponent( DeltaTime, TickType, ThisTickFunction );
	
	if (lastAtlas != atlas || lastData != skeletonData) {
		DisposeState();

		if (atlas && skeletonData) {
			spSkeletonData* data = skeletonData->GetSkeletonData(atlas->GetAtlas(true), true);
			skeleton = spSkeleton_create(data);
			stateData = spAnimationStateData_create(data);
			state = spAnimationState_create(stateData);
			spAnimationState_setAnimationByName(state, 0, "walk", true);
		}

		lastAtlas = atlas;
		lastData = skeletonData;
	}

	if (state) {
		spAnimationState_update(state, DeltaTime);
		spAnimationState_apply(state, skeleton);
		spSkeleton_updateWorldTransform(skeleton);
	}
}

void USpineSkeletonComponent::DisposeState() {
	if (stateData) {
		spAnimationStateData_dispose(stateData);
		stateData = nullptr;
	}

	if (state) {
		spAnimationState_dispose(state);
		state = nullptr;
	}

	if (skeleton) {
		spSkeleton_dispose(skeleton);
		skeleton = nullptr;
	}
}

void USpineSkeletonComponent::BeginDestroy() {
	DisposeState();
	Super::BeginDestroy();
}