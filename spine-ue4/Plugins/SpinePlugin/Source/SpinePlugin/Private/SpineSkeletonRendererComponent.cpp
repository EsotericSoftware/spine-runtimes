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

#include "SpinePluginPrivatePCH.h"
#include "Engine.h"
#include "spine/spine.h"
#include <stdlib.h>

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

USpineSkeletonRendererComponent::USpineSkeletonRendererComponent (const FObjectInitializer& ObjectInitializer) 
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

void USpineSkeletonRendererComponent::BeginPlay () {
	Super::BeginPlay();
}

void USpineSkeletonRendererComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	
	AActor* owner = GetOwner();
	if (owner) {
		UClass* skeletonClass = USpineSkeletonComponent::StaticClass();
		USpineSkeletonComponent* skeleton = Cast<USpineSkeletonComponent>(owner->GetComponentByClass(skeletonClass));
		
		UpdateRenderer(skeleton);
	}
}

void USpineSkeletonRendererComponent::UpdateRenderer(USpineSkeletonComponent* skeleton)
{
	if (skeleton && !skeleton->IsBeingDestroyed() && skeleton->GetSkeleton() && skeleton->Atlas) {
		skeleton->GetSkeleton()->getColor().set(Color.R, Color.G, Color.B, Color.A);

		if (atlasNormalBlendMaterials.Num() != skeleton->Atlas->atlasPages.Num()) {
			atlasNormalBlendMaterials.SetNum(0);
			pageToNormalBlendMaterial.Empty();
			atlasAdditiveBlendMaterials.SetNum(0);
			pageToAdditiveBlendMaterial.Empty();
			atlasMultiplyBlendMaterials.SetNum(0);
			pageToMultiplyBlendMaterial.Empty();
			atlasScreenBlendMaterials.SetNum(0);
			pageToScreenBlendMaterial.Empty();

			for (int i = 0; i < skeleton->Atlas->atlasPages.Num(); i++) {
				AtlasPage* currPage = skeleton->Atlas->GetAtlas()->getPages()[i];

				UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(NormalBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
				atlasNormalBlendMaterials.Add(material);
				pageToNormalBlendMaterial.Add(currPage, material);

				material = UMaterialInstanceDynamic::Create(AdditiveBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
				atlasAdditiveBlendMaterials.Add(material);
				pageToAdditiveBlendMaterial.Add(currPage, material);

				material = UMaterialInstanceDynamic::Create(MultiplyBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
				atlasMultiplyBlendMaterials.Add(material);
				pageToMultiplyBlendMaterial.Add(currPage, material);

				material = UMaterialInstanceDynamic::Create(ScreenBlendMaterial, this);
				material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
				atlasScreenBlendMaterials.Add(material);
				pageToScreenBlendMaterial.Add(currPage, material);
			}
		}
		else {
			pageToNormalBlendMaterial.Empty();
			pageToAdditiveBlendMaterial.Empty();
			pageToMultiplyBlendMaterial.Empty();
			pageToScreenBlendMaterial.Empty();

			for (int i = 0; i < skeleton->Atlas->atlasPages.Num(); i++) {
				AtlasPage* currPage = skeleton->Atlas->GetAtlas()->getPages()[i];

				UTexture2D* texture = skeleton->Atlas->atlasPages[i];
				UTexture* oldTexture = nullptr;

				UMaterialInstanceDynamic* current = atlasNormalBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(NormalBlendMaterial, this);
					material->SetTextureParameterValue(TextureParameterName, texture);
					atlasNormalBlendMaterials[i] = material;
				}
				pageToNormalBlendMaterial.Add(currPage, atlasNormalBlendMaterials[i]);

				current = atlasAdditiveBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(AdditiveBlendMaterial, this);
					material->SetTextureParameterValue(TextureParameterName, texture);
					atlasAdditiveBlendMaterials[i] = material;
				}
				pageToAdditiveBlendMaterial.Add(currPage, atlasAdditiveBlendMaterials[i]);

				current = atlasMultiplyBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(MultiplyBlendMaterial, this);
					material->SetTextureParameterValue(TextureParameterName, texture);
					atlasMultiplyBlendMaterials[i] = material;
				}
				pageToMultiplyBlendMaterial.Add(currPage, atlasMultiplyBlendMaterials[i]);

				current = atlasScreenBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(ScreenBlendMaterial, this);
					material->SetTextureParameterValue(TextureParameterName, texture);
					atlasScreenBlendMaterials[i] = material;
				}
				pageToScreenBlendMaterial.Add(currPage, atlasScreenBlendMaterials[i]);
			}
		}
		UpdateMesh(skeleton->GetSkeleton());
	}
	else {
		ClearAllMeshSections();
	}
}

void USpineSkeletonRendererComponent::Flush (int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector> &Normals, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector>& Colors2, UMaterialInstanceDynamic* Material) {
	if (Vertices.Num() == 0) return;
	SetMaterial(Idx, Material);

	CreateMeshSection(Idx, Vertices, Indices, Normals, Uvs, Colors, TArray<FProcMeshTangent>(), bCreateCollision);

	Vertices.SetNum(0);
	Indices.SetNum(0);
	Normals.SetNum(0);
	Uvs.SetNum(0);
	Colors.SetNum(0);
	Colors2.SetNum(0);
	Idx++;
}

void USpineSkeletonRendererComponent::UpdateMesh(Skeleton* Skeleton) {
	TArray<FVector> vertices;
	TArray<int32> indices;
	TArray<FVector> normals;
	TArray<FVector2D> uvs;
	TArray<FColor> colors;
	TArray<FVector> darkColors;
	
	int idx = 0;
	int meshSection = 0;
	UMaterialInstanceDynamic* lastMaterial = nullptr;

	ClearAllMeshSections();

	// Early out if skeleton is invisible
	if (Skeleton->getColor().a == 0) return;

	float depthOffset = 0;
	unsigned short quadIndices[] = { 0, 1, 2, 0, 2, 3 };

	for (size_t i = 0; i < Skeleton->getSlots().size(); ++i) {
		Vector<float> *attachmentVertices = &worldVertices;
		unsigned short* attachmentIndices = nullptr;
		int numVertices;
		int numIndices;
		AtlasRegion* attachmentAtlasRegion = nullptr;
		spine::Color attachmentColor;
		attachmentColor.set(1, 1, 1, 1);
		float* attachmentUvs = nullptr;

		Slot* slot = Skeleton->getDrawOrder()[i];
		Attachment* attachment = slot->getAttachment();

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
			RegionAttachment* regionAttachment = (RegionAttachment*)attachment;

			// Early out if region is invisible
			if (regionAttachment->getColor().a == 0) {
				clipper.clipEnd(*slot);
				continue;
			}

			attachmentColor.set(regionAttachment->getColor());
			attachmentAtlasRegion = (AtlasRegion*)regionAttachment->getRendererObject();
			regionAttachment->computeWorldVertices(slot->getBone(), *attachmentVertices, 0, 2);
			attachmentIndices = quadIndices;
			attachmentUvs = regionAttachment->getUVs().buffer();
			numVertices = 4;
			numIndices = 6;
		} else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
			MeshAttachment* mesh = (MeshAttachment*)attachment;

			// Early out if region is invisible
			if (mesh->getColor().a == 0) {
				clipper.clipEnd(*slot);
				continue;
			}

			attachmentColor.set(mesh->getColor());
			attachmentAtlasRegion = (AtlasRegion*)mesh->getRendererObject();			
			mesh->computeWorldVertices(*slot, 0, mesh->getWorldVerticesLength(), *attachmentVertices, 0, 2);
			attachmentIndices = mesh->getTriangles().buffer();
			attachmentUvs = mesh->getUVs().buffer();
			numVertices = mesh->getWorldVerticesLength() >> 1;
			numIndices = mesh->getTriangles().size();
		} else /* clipping */ {
			ClippingAttachment* clip = (ClippingAttachment*)attachment;
			clipper.clipStart(*slot, clip);
			continue;
		}

		// if the user switches the atlas data while not having switched
		// to the correct skeleton data yet, we won't find any regions.
		// ignore regions for which we can't find a material
		UMaterialInstanceDynamic* material = nullptr;
		switch (slot->getData().getBlendMode()) {
		case BlendMode_Normal:
			if (!pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = pageToNormalBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Additive:
			if (!pageToAdditiveBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = pageToAdditiveBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Multiply:
			if (!pageToMultiplyBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = pageToMultiplyBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Screen:
			if (!pageToScreenBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = pageToScreenBlendMaterial[attachmentAtlasRegion->page];
			break;
		default:
			if (!pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = pageToNormalBlendMaterial[attachmentAtlasRegion->page];
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

		if (lastMaterial != material) {
			Flush(meshSection, vertices, indices, normals, uvs, colors, darkColors, lastMaterial);
			lastMaterial = material;
			idx = 0;
		}

		SetMaterial(meshSection, material);

		uint8 r = static_cast<uint8>(Skeleton->getColor().r * slot->getColor().r * attachmentColor.r * 255);
		uint8 g = static_cast<uint8>(Skeleton->getColor().g * slot->getColor().g * attachmentColor.g * 255);
		uint8 b = static_cast<uint8>(Skeleton->getColor().b * slot->getColor().b * attachmentColor.b * 255);
		uint8 a = static_cast<uint8>(Skeleton->getColor().a * slot->getColor().a * attachmentColor.a * 255);

		float dr = slot->hasDarkColor() ? slot->getDarkColor().r : 0.0f;
		float dg = slot->hasDarkColor() ? slot->getDarkColor().g : 0.0f;
		float db = slot->hasDarkColor() ? slot->getDarkColor().b : 0.0f;		

		float* verticesPtr = attachmentVertices->buffer();
		for (int j = 0; j < numVertices << 1; j += 2) {
			colors.Add(FColor(r, g, b, a));
			darkColors.Add(FVector(dr, dg, db));
			vertices.Add(FVector(verticesPtr[j], depthOffset, verticesPtr[j + 1]));
			normals.Add(FVector(0, -1, 0));
			uvs.Add(FVector2D(attachmentUvs[j], attachmentUvs[j + 1]));
		}

		for (int j = 0; j < numIndices; j++) {
			indices.Add(idx + attachmentIndices[j]);
		}

		idx += numVertices;
		depthOffset += this->DepthOffset;

		clipper.clipEnd(*slot);			
	}
	
	Flush(meshSection, vertices, indices, normals, uvs, colors, darkColors, lastMaterial);
	clipper.clipEnd();
}

#undef LOCTEXT_NAMESPACE
