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

	/* Updates this skeleton renderer using the provided skeleton animation component. */
	void UpdateRenderer(USpineSkeletonComponent* Skeleton);

	// Material Instance parents
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	UMaterialInterface* NormalBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	UMaterialInterface* AdditiveBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	UMaterialInterface* MultiplyBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	UMaterialInterface* ScreenBlendMaterial;

	// Need to hold on to the dynamic instances, or the GC will kill us while updating them
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	TArray<UMaterialInstanceDynamic*> atlasNormalBlendMaterials;
	TMap<spine::AtlasPage*, UMaterialInstanceDynamic*> pageToNormalBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	TArray<UMaterialInstanceDynamic*> atlasAdditiveBlendMaterials;
	TMap<spine::AtlasPage*, UMaterialInstanceDynamic*> pageToAdditiveBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	TArray<UMaterialInstanceDynamic*> atlasMultiplyBlendMaterials;
	TMap<spine::AtlasPage*, UMaterialInstanceDynamic*> pageToMultiplyBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	TArray<UMaterialInstanceDynamic*> atlasScreenBlendMaterials;
	TMap<spine::AtlasPage*, UMaterialInstanceDynamic*> pageToScreenBlendMaterial;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	float DepthOffset = 0.1f;
	
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	FName TextureParameterName;

	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	FLinearColor Color = FLinearColor(1, 1, 1, 1);

	/** Whether to generate collision geometry for the skeleton, or not. */
	UPROPERTY(Category = Spine, EditAnywhere, BlueprintReadWrite)
	bool bCreateCollision;

	virtual void FinishDestroy() override;
	
protected:
	void UpdateMesh (spine::Skeleton* Skeleton);

	void Flush (int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector> &Normals, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector> &Colors2, UMaterialInstanceDynamic* Material);
	
	spine::Vector<float> worldVertices;
	spine::SkeletonClipping clipper;
};
