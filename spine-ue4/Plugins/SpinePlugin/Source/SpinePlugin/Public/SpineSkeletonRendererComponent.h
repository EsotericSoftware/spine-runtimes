// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "ProceduralMeshComponent.h"
#include "SpineSkeletonComponent.h"
#include "SpineSkeletonRendererComponent.generated.h"


UCLASS( ClassGroup=(Spine), meta=(BlueprintSpawnableComponent) )
class SPINEPLUGIN_API USpineSkeletonRendererComponent : public UProceduralMeshComponent
{
	GENERATED_BODY()

public:	
	USpineSkeletonRendererComponent (const FObjectInitializer& ObjectInitializer);
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadOnly)
	UMaterialInterface* DefaultMaterial;	

	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	float depthOffset = 0.1f;
protected:
	void UpdateMesh (spSkeleton* skeleton);

	void Flush(int &idx, TArray<FVector> &vertices, TArray<int32> &indices, TArray<FVector2D> &uvs, TArray<FColor> &colors, UMaterialInstanceDynamic* material);
	
	TArray<UMaterialInstanceDynamic*> atlasMaterials;
	TMap<spAtlasPage*, UMaterialInstanceDynamic*> pageToMaterial;
};
