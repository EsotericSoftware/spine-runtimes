#include "SpinePluginPrivatePCH.h"
#include "Engine.h"
#include "spine/spine.h"
#include <stdlib.h>

USpineSkeletonRendererComponent::USpineSkeletonRendererComponent(const FObjectInitializer& ObjectInitializer)
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.	
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;

	static ConstructorHelpers::FObjectFinder<UMaterialInterface> MaskedMaterialRef(TEXT("/Paper2D/MaskedUnlitSpriteMaterial"));
	DefaultMaterial = MaskedMaterialRef.Object;
}

// Called when the game starts
void USpineSkeletonRendererComponent::BeginPlay()
{
	Super::BeginPlay();
}

// Called every frame
void USpineSkeletonRendererComponent::TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction )
{
	Super::TickComponent( DeltaTime, TickType, ThisTickFunction );

	UClass* skeletonClass = USpineSkeletonComponent::StaticClass();
	AActor* owner = GetOwner();
	if (owner) {
		USpineSkeletonComponent* skeleton = Cast<USpineSkeletonComponent>(owner->GetComponentByClass(skeletonClass));
		if (skeleton && skeleton->skeleton) {
			spSkeleton_updateWorldTransform(skeleton->skeleton);			
			UpdateMesh(skeleton->skeleton);			
		}
	}		
}

void USpineSkeletonRendererComponent::UpdateMesh(spSkeleton* skeleton) {	
	TArray<FVector> vertices;
	TArray<int32> indices;
	TArray<FVector2D> uvs;
	TArray<FColor> colors;		

	TArray<float> worldVertices;
	worldVertices.SetNumUninitialized(2 * 1024);
	int idx = 0;
	int meshSection = 0;

	ClearAllMeshSections();

	float depthOffset = 0;

	for (int i = 0; i < skeleton->slotsCount; ++i) {
		spSlot* slot = skeleton->drawOrder[i];
		spAttachment* attachment = slot->attachment;
		if (!attachment) continue;						
		
		if (attachment->type == SP_ATTACHMENT_REGION) {
			spRegionAttachment* regionAttachment = (spRegionAttachment*)attachment;			
			spRegionAttachment_computeWorldVertices(regionAttachment, slot->bone, worldVertices.GetData());

			uint8 r = static_cast<uint8>(skeleton->r * slot->r * 255);
			uint8 g = static_cast<uint8>(skeleton->g * slot->g * 255);
			uint8 b = static_cast<uint8>(skeleton->b * slot->b * 255);
			uint8 a = static_cast<uint8>(skeleton->a * slot->a * 255);
			
			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[0], depthOffset, worldVertices[1]));
			uvs.Add(FVector2D(regionAttachment->uvs[0], regionAttachment->uvs[1]));

			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[2], depthOffset, worldVertices[3]));
			uvs.Add(FVector2D(regionAttachment->uvs[2], regionAttachment->uvs[3]));

			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[4], depthOffset, worldVertices[5]));
			uvs.Add(FVector2D(regionAttachment->uvs[4], regionAttachment->uvs[5]));

			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[6], depthOffset, worldVertices[7]));
			uvs.Add(FVector2D(regionAttachment->uvs[6], regionAttachment->uvs[7]));

			indices.Add(idx + 0);
			indices.Add(idx + 1);
			indices.Add(idx + 2);
			indices.Add(idx + 0);
			indices.Add(idx + 2);
			indices.Add(idx + 3);
			idx += 4;
			depthOffset -= this->depthOffset;
		} else if (attachment->type == SP_ATTACHMENT_MESH) {
			spMeshAttachment* mesh = (spMeshAttachment*)attachment;
			if (mesh->super.worldVerticesLength> worldVertices.Num()) {
				worldVertices.SetNum(mesh->super.worldVerticesLength);
			}
			spMeshAttachment_computeWorldVertices(mesh, slot, worldVertices.GetData());

			uint8 r = static_cast<uint8>(skeleton->r * slot->r * 255);
			uint8 g = static_cast<uint8>(skeleton->g * slot->g * 255);
			uint8 b = static_cast<uint8>(skeleton->b * slot->b * 255);
			uint8 a = static_cast<uint8>(skeleton->a * slot->a * 255);
			
			for (int i = 0; i < mesh->super.worldVerticesLength; i += 2) {				
				colors.Add(FColor(r, g, b, a));
				vertices.Add(FVector(worldVertices[i], depthOffset, worldVertices[i + 1]));
				uvs.Add(FVector2D(mesh->uvs[i], mesh->uvs[i + 1]));				
			}

			for (int i = 0; i < mesh->trianglesCount; i++) {
				indices.Add(idx + mesh->triangles[i]);
			}
			idx += mesh->super.worldVerticesLength >> 1;
			depthOffset -= this->depthOffset;
		}
	}

	CreateMeshSection(0, vertices, indices, TArray<FVector>(), uvs, colors, TArray<FProcMeshTangent>(), false);	
}

UMaterialInterface* USpineSkeletonRendererComponent::GetMaterial(int32 MaterialIndex) const {
	return MaterialIndex == 0 ? GetDefaultMaterial() : nullptr;		
}

int32 USpineSkeletonRendererComponent::GetNumMaterials() const {
	return 1;
}