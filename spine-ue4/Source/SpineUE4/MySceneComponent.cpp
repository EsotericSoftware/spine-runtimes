// Fill out your copyright notice in the Description page of Project Settings.

#include "SpineUE4.h"
#include "MySceneComponent.h"
#include "spine/spine.h"


// Sets default values for this component's properties
UMySceneComponent::UMySceneComponent(const FObjectInitializer& ObjectInitializer) : USpineSkeletonRendererComponent(ObjectInitializer)
{
	// Set this component to be initialized when the game starts, and to be ticked every frame. You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;
}


// Called when the game starts
void UMySceneComponent::BeginPlay()
{
	Super::BeginPlay();

	// ...
	
}


// Called every frame
void UMySceneComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// ...
}

