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

	int worldVerticesLength = 1000;
	float* worldVertices = (float*)malloc((2 + 2 + 5) * worldVerticesLength);
	int idx = 0;
	int meshSection = 0;

	ClearAllMeshSections();

	for (int i = 0; i < skeleton->slotsCount; ++i) {
		spSlot* slot = skeleton->drawOrder[i];
		spAttachment* attachment = slot->attachment;
		if (!attachment) continue;						
		
		if (attachment->type == SP_ATTACHMENT_REGION) {
			spRegionAttachment* regionAttachment = (spRegionAttachment*)attachment;			
			spRegionAttachment_computeWorldVertices(regionAttachment, slot->bone, worldVertices);

			uint8 r = static_cast<uint8>(skeleton->r * slot->r * 255);
			uint8 g = static_cast<uint8>(skeleton->g * slot->g * 255);
			uint8 b = static_cast<uint8>(skeleton->b * slot->b * 255);
			uint8 a = static_cast<uint8>(skeleton->a * slot->a * 255);
			
			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[0], 0, worldVertices[1]));
			uvs.Add(FVector2D(regionAttachment->uvs[0], regionAttachment->uvs[1]));

			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[2], 0, worldVertices[3]));
			uvs.Add(FVector2D(regionAttachment->uvs[2], regionAttachment->uvs[3]));

			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[4], 0, worldVertices[5]));
			uvs.Add(FVector2D(regionAttachment->uvs[4], regionAttachment->uvs[5]));

			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(worldVertices[6], 0, worldVertices[7]));
			uvs.Add(FVector2D(regionAttachment->uvs[6], regionAttachment->uvs[7]));

			indices.Add(idx + 0);
			indices.Add(idx + 1);
			indices.Add(idx + 2);
			indices.Add(idx + 0);
			indices.Add(idx + 2);
			indices.Add(idx + 3);
			idx += 4;			
		}
		/*else if (attachment->type == ATTACHMENT_MESH) {
			MeshAttachment* mesh = (MeshAttachment*)attachment;
			if (mesh->super.worldVerticesLength > SPINE_MESH_VERTEX_COUNT_MAX) continue;
			texture = (Texture*)((AtlasRegion*)mesh->rendererObject)->page->rendererObject;
			MeshAttachment_computeWorldVertices(mesh, slot, worldVertices);

			Uint8 r = static_cast<Uint8>(skeleton->r * slot->r * 255);
			Uint8 g = static_cast<Uint8>(skeleton->g * slot->g * 255);
			Uint8 b = static_cast<Uint8>(skeleton->b * slot->b * 255);
			Uint8 a = static_cast<Uint8>(skeleton->a * slot->a * 255);
			vertex.color.r = r;
			vertex.color.g = g;
			vertex.color.b = b;
			vertex.color.a = a;

			Vector2u size = texture->getSize();
			for (int i = 0; i < mesh->trianglesCount; ++i) {
				int index = mesh->triangles[i] << 1;
				vertex.position.x = worldVertices[index];
				vertex.position.y = worldVertices[index + 1];
				vertex.texCoords.x = mesh->uvs[index] * size.x;
				vertex.texCoords.y = mesh->uvs[index + 1] * size.y;
				vertexArray->append(vertex);
			}
		}*/
	}

	CreateMeshSection(0, vertices, indices, TArray<FVector>(), uvs, colors, TArray<FProcMeshTangent>(), false);
	free(worldVertices);
}