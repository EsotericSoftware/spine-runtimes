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

	UMaterialInterface* GetDefaultMaterial() const { return DefaultMaterial; }

	UMaterialInterface* GetAlternateMaterial() const { return nullptr; }
	
	UMaterialInterface* GetMaterial(int32 MaterialIndex) const;

	int32 GetNumMaterials() const;

	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	float depthOffset = 0.1f;
protected:
	void UpdateMesh (spSkeleton* skeleton);

	UPROPERTY()
	TArray<UMaterialInstanceDynamic*> atlasMaterials;
};
