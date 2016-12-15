// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "ProceduralMeshComponent.h"
#include "SpineSkeletonAnimationComponent.h"
#include "SpineSkeletonRendererComponent.generated.h"


UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonRendererComponent: public UProceduralMeshComponent {
	GENERATED_BODY()

public: 
	USpineSkeletonRendererComponent (const FObjectInitializer& ObjectInitializer);
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadOnly)
	UMaterialInterface* NormalBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadOnly)
	UMaterialInterface* AdditiveBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadOnly)
	UMaterialInterface* MultiplyBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadOnly)
	UMaterialInterface* ScreenBlendMaterial;

	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	float DepthOffset = 0.1f;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	FName TextureParameterName;
	
protected:
	void UpdateMesh (spSkeleton* Skeleton);

	void Flush (int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, UMaterialInstanceDynamic* Material);
	
	// Need to hold on to the dynamic instances, or the GC will kill us while updating them
	UPROPERTY()
	TArray<UMaterialInstanceDynamic*> atlasNormalBlendMaterials;
	TMap<spAtlasPage*, UMaterialInstanceDynamic*> pageToNormalBlendMaterial;
	
	UPROPERTY()
	TArray<UMaterialInstanceDynamic*> atlasAdditiveBlendMaterials;
	TMap<spAtlasPage*, UMaterialInstanceDynamic*> pageToAdditiveBlendMaterial;
	
	UPROPERTY()
	TArray<UMaterialInstanceDynamic*> atlasMultiplyBlendMaterials;
	TMap<spAtlasPage*, UMaterialInstanceDynamic*> pageToMultiplyBlendMaterial;
	
	UPROPERTY()
	TArray<UMaterialInstanceDynamic*> atlasScreenBlendMaterials;
	TMap<spAtlasPage*, UMaterialInstanceDynamic*> pageToScreenBlendMaterial;
};
