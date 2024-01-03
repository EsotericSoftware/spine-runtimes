/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpineSkeletonRendererComponent.h"

#include "SpineAtlasAsset.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "spine/spine.h"
#include "UObject/ConstructorHelpers.h"
#if ENGINE_MAJOR_VERSION >= 5
#include "PhysicsEngine/BodySetup.h"
#endif

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

USpineSkeletonRendererComponent::USpineSkeletonRendererComponent(const FObjectInitializer &ObjectInitializer)
	: UProceduralMeshComponent(ObjectInitializer) {
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;

	static ConstructorHelpers::FObjectFinder<UMaterialInterface> NormalMaterialRef(TEXT("/SpinePlugin/SpineUnlitNormalMaterial"));
	NormalBlendMaterial = NormalMaterialRef.Object;

	static ConstructorHelpers::FObjectFinder<UMaterialInterface> AdditiveMaterialRef(TEXT("/SpinePlugin/SpineUnlitAdditiveMaterial"));
	AdditiveBlendMaterial = AdditiveMaterialRef.Object;

	static ConstructorHelpers::FObjectFinder<UMaterialInterface> MultiplyMaterialRef(TEXT("/SpinePlugin/SpineUnlitMultiplyMaterial"));
	MultiplyBlendMaterial = MultiplyMaterialRef.Object;

	static ConstructorHelpers::FObjectFinder<UMaterialInterface> ScreenMaterialRef(TEXT("/SpinePlugin/SpineUnlitScreenMaterial"));
	ScreenBlendMaterial = ScreenMaterialRef.Object;

	TextureParameterName = FName(TEXT("SpriteTexture"));

	worldVertices.ensureCapacity(1024 * 2);
}

void USpineSkeletonRendererComponent::FinishDestroy() {
	Super::FinishDestroy();
}

void USpineSkeletonRendererComponent::BeginPlay() {
	Super::BeginPlay();
}

void USpineSkeletonRendererComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction *ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	AActor *owner = GetOwner();
	if (owner) {
		UClass *skeletonClass = USpineSkeletonComponent::StaticClass();
		USpineSkeletonComponent *skeletonComponent = Cast<USpineSkeletonComponent>(owner->GetComponentByClass(skeletonClass));

		UpdateRenderer(skeletonComponent);
	}
}

void USpineSkeletonRendererComponent::UpdateRenderer(USpineSkeletonComponent *component) {
	if (component && !component->IsBeingDestroyed() && component->GetSkeleton() && component->Atlas) {
		component->GetSkeleton()->getColor().set(Color.R, Color.G, Color.B, Color.A);

		if (atlasNormalBlendMaterials.Num() != component->Atlas->atlasPages.Num()) {
			atlasNormalBlendMaterials.SetNum(0);
			atlasAdditiveBlendMaterials.SetNum(0);
			atlasMultiplyBlendMaterials.SetNum(0);
			atlasScreenBlendMaterials.SetNum(0);

			for (int i = 0; i < component->Atlas->atlasPages.Num(); i++) {
				AtlasPage *currPage = component->Atlas->GetAtlas()->getPages()[i];

				UMaterialInstanceDynamic *material = UMaterialInstanceDynamic::Create(NormalBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, component->Atlas->atlasPages[i]);
				atlasNormalBlendMaterials.Add(material);

				material = UMaterialInstanceDynamic::Create(AdditiveBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, component->Atlas->atlasPages[i]);
				atlasAdditiveBlendMaterials.Add(material);

				material = UMaterialInstanceDynamic::Create(MultiplyBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, component->Atlas->atlasPages[i]);
				atlasMultiplyBlendMaterials.Add(material);

				material = UMaterialInstanceDynamic::Create(ScreenBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, component->Atlas->atlasPages[i]);
				atlasScreenBlendMaterials.Add(material);
			}
		} else {
			for (int i = 0; i < component->Atlas->atlasPages.Num(); i++) {
				UTexture2D *texture = component->Atlas->atlasPages[i];
				UpdateMaterial(texture, atlasNormalBlendMaterials[i], NormalBlendMaterial);
				UpdateMaterial(texture, atlasAdditiveBlendMaterials[i], AdditiveBlendMaterial);
				UpdateMaterial(texture, atlasMultiplyBlendMaterials[i], MultiplyBlendMaterial);
				UpdateMaterial(texture, atlasScreenBlendMaterials[i], ScreenBlendMaterial);
			}
		}
		UpdateMesh(component, component->GetSkeleton());
	} else {
		ClearAllMeshSections();
	}
}

void USpineSkeletonRendererComponent::UpdateMaterial(UTexture2D *Texture, UMaterialInstanceDynamic *&CurrentInstance, UMaterialInterface *ParentMaterial) {

	UTexture *oldTexture = nullptr;
	if (!CurrentInstance || !CurrentInstance->GetTextureParameterValue(TextureParameterName, oldTexture) ||
		oldTexture != Texture || CurrentInstance->Parent != ParentMaterial) {

		UMaterialInstanceDynamic *material = UMaterialInstanceDynamic::Create(ParentMaterial, this);
		material->SetTextureParameterValue(TextureParameterName, Texture);
		CurrentInstance = material;
	}
}

void USpineSkeletonRendererComponent::Flush(int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector> &Normals, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, UMaterialInstanceDynamic *Material) {
	if (Vertices.Num() == 0) return;
	SetMaterial(Idx, Material);

	bool bShouldCreateCollision = false;
	if (bCreateCollision) {
		UWorld *world = GetWorld();
		if (world && world->IsGameWorld()) {
			bShouldCreateCollision = true;
		}
	}

	GetBodySetup()->bGenerateMirroredCollision = GetComponentScale().X < 0 || GetComponentScale().Y < 0 || GetComponentScale().Z < 0;
	CreateMeshSection(Idx, Vertices, Indices, Normals, Uvs, Colors, TArray<FProcMeshTangent>(), bShouldCreateCollision);

	Vertices.SetNum(0);
	Indices.SetNum(0);
	Normals.SetNum(0);
	Uvs.SetNum(0);
	Colors.SetNum(0);
	Idx++;
}

void USpineSkeletonRendererComponent::UpdateMesh(USpineSkeletonComponent *component, Skeleton *Skeleton) {
	vertices.Empty();
	indices.Empty();
	normals.Empty();
	uvs.Empty();
	colors.Empty();

	int idx = 0;
	int meshSection = 0;
	UMaterialInstanceDynamic *lastMaterial = nullptr;

	ClearAllMeshSections();

	// Early out if skeleton is invisible
	if (Skeleton->getColor().a == 0) return;

	float depthOffset = 0;
	unsigned short quadIndices[] = {0, 1, 2, 0, 2, 3};

	for (size_t i = 0; i < Skeleton->getSlots().size(); ++i) {
		Vector<float> *attachmentVertices = &worldVertices;
		unsigned short *attachmentIndices = nullptr;
		int numVertices;
		int numIndices;
		AtlasRegion *attachmentAtlasRegion = nullptr;
		spine::Color attachmentColor;
		attachmentColor.set(1, 1, 1, 1);
		float *attachmentUvs = nullptr;

		Slot *slot = Skeleton->getDrawOrder()[i];
		Attachment *attachment = slot->getAttachment();

		if (slot->getColor().a == 0 || !slot->getBone().isActive()) {
			clipper.clipEnd(*slot);
			continue;
		}

		if (!attachment) {
			clipper.clipEnd(*slot);
			continue;
		}
		if (!attachment->getRTTI().isExactly(RegionAttachment::rtti) && !attachment->getRTTI().isExactly(MeshAttachment::rtti) && !attachment->getRTTI().isExactly(ClippingAttachment::rtti)) {
			clipper.clipEnd(*slot);
			continue;
		}

		if (attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
			RegionAttachment *regionAttachment = (RegionAttachment *) attachment;

			// Early out if region is invisible
			if (regionAttachment->getColor().a == 0) {
				clipper.clipEnd(*slot);
				continue;
			}

			attachmentColor.set(regionAttachment->getColor());
			attachmentVertices->setSize(8, 0);
			regionAttachment->computeWorldVertices(*slot, *attachmentVertices, 0, 2);
			attachmentAtlasRegion = (AtlasRegion *) regionAttachment->getRegion();
			attachmentIndices = quadIndices;
			attachmentUvs = regionAttachment->getUVs().buffer();
			numVertices = 4;
			numIndices = 6;
		} else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
			MeshAttachment *mesh = (MeshAttachment *) attachment;

			// Early out if region is invisible
			if (mesh->getColor().a == 0) {
				clipper.clipEnd(*slot);
				continue;
			}

			attachmentColor.set(mesh->getColor());
			attachmentVertices->setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(*slot, 0, mesh->getWorldVerticesLength(), attachmentVertices->buffer(), 0, 2);
			attachmentAtlasRegion = (AtlasRegion *) mesh->getRegion();
			attachmentIndices = mesh->getTriangles().buffer();
			attachmentUvs = mesh->getUVs().buffer();
			numVertices = mesh->getWorldVerticesLength() >> 1;
			numIndices = mesh->getTriangles().size();
		} else /* clipping */ {
			ClippingAttachment *clip = (ClippingAttachment *) attachment;
			clipper.clipStart(*slot, clip);
			continue;
		}

		if (clipper.isClipping()) {
			clipper.clipTriangles(attachmentVertices->buffer(), attachmentIndices, numIndices, attachmentUvs, 2);
			attachmentVertices = &clipper.getClippedVertices();
			numVertices = clipper.getClippedVertices().size() >> 1;
			attachmentIndices = clipper.getClippedTriangles().buffer();
			numIndices = clipper.getClippedTriangles().size();
			attachmentUvs = clipper.getClippedUVs().buffer();
			if (clipper.getClippedTriangles().size() == 0) {
				clipper.clipEnd(*slot);
				continue;
			}
		}

		// if the user switches the atlas data while not having switched
		// to the correct skeleton data yet, we won't find any regions.
		// ignore regions for which we can't find a material
		UMaterialInstanceDynamic *material = nullptr;
		int foundPageIndex = -1;
		for (int pageIndex = 0; pageIndex < component->Atlas->atlasPages.Num(); pageIndex++) {
			AtlasPage *page = component->Atlas->GetAtlas()->getPages()[pageIndex];
			if (attachmentAtlasRegion->page == page) {
				foundPageIndex = pageIndex;
				break;
			}
		}
		if (foundPageIndex == -1) {
			clipper.clipEnd(*slot);
			continue;
		}
		switch (slot->getData().getBlendMode()) {
			case BlendMode_Additive:
				if (foundPageIndex >= atlasAdditiveBlendMaterials.Num()) {
					clipper.clipEnd(*slot);
					continue;
				}
				material = atlasAdditiveBlendMaterials[foundPageIndex];
				break;
			case BlendMode_Multiply:
				if (foundPageIndex >= atlasMultiplyBlendMaterials.Num()) {
					clipper.clipEnd(*slot);
					continue;
				}
				material = atlasMultiplyBlendMaterials[foundPageIndex];
				break;
			case BlendMode_Screen:
				if (foundPageIndex >= atlasScreenBlendMaterials.Num()) {
					clipper.clipEnd(*slot);
					continue;
				}
				material = atlasScreenBlendMaterials[foundPageIndex];
				break;
			case BlendMode_Normal:
			default:
				if (foundPageIndex >= atlasNormalBlendMaterials.Num()) {
					clipper.clipEnd(*slot);
					continue;
				}
				material = atlasNormalBlendMaterials[foundPageIndex];
				break;
		}

		if (lastMaterial != material) {
			Flush(meshSection, vertices, indices, normals, uvs, colors, lastMaterial);
			lastMaterial = material;
			idx = 0;
		}

		SetMaterial(meshSection, material);

		uint8 r = static_cast<uint8>(Skeleton->getColor().r * slot->getColor().r * attachmentColor.r * 255);
		uint8 g = static_cast<uint8>(Skeleton->getColor().g * slot->getColor().g * attachmentColor.g * 255);
		uint8 b = static_cast<uint8>(Skeleton->getColor().b * slot->getColor().b * attachmentColor.b * 255);
		uint8 a = static_cast<uint8>(Skeleton->getColor().a * slot->getColor().a * attachmentColor.a * 255);

		float *verticesPtr = attachmentVertices->buffer();
		for (int j = 0; j < numVertices << 1; j += 2) {
			colors.Add(FColor(r, g, b, a));
			vertices.Add(FVector(verticesPtr[j], depthOffset, verticesPtr[j + 1]));
			uvs.Add(FVector2D(attachmentUvs[j], attachmentUvs[j + 1]));
		}

		for (int j = 0; j < numIndices; j++) {
			indices.Add(idx + attachmentIndices[j]);
		}

		int numTriangles = indices.Num() / 3;
		for (int j = 0; j < numTriangles; j++) {
			const int triangleIndex = j * 3;
			if (FVector::CrossProduct(
						vertices[indices[triangleIndex + 2]] - vertices[indices[triangleIndex]],
						vertices[indices[triangleIndex + 1]] - vertices[indices[triangleIndex]])
						.Y < 0.f) {
				const int32 targetVertex = indices[triangleIndex];
				indices[triangleIndex] = indices[triangleIndex + 2];
				indices[triangleIndex + 2] = targetVertex;
			}
		}

		FVector normal = FVector(0, 1, 0);
		for (int j = 0; j < numVertices; j++) {
			normals.Add(normal);
		}

		idx += numVertices;
		depthOffset += this->DepthOffset;

		clipper.clipEnd(*slot);
	}

	Flush(meshSection, vertices, indices, normals, uvs, colors, lastMaterial);
	clipper.clipEnd();
}

#undef LOCTEXT_NAMESPACE
