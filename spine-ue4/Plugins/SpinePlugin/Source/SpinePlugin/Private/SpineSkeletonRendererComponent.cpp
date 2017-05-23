/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpinePluginPrivatePCH.h"
#include "Engine.h"
#include "spine/spine.h"
#include <stdlib.h>

#define LOCTEXT_NAMESPACE "Spine"

USpineSkeletonRendererComponent::USpineSkeletonRendererComponent (const FObjectInitializer& ObjectInitializer) 
: URuntimeMeshComponent(ObjectInitializer) {
	bWantsBeginPlay = true;
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

	worldVertices = spFloatArray_create(1024 * 2);
	clipper = spSkeletonClipping_create();
}

void USpineSkeletonRendererComponent::FinishDestroy() {
	if (clipper) spSkeletonClipping_dispose(clipper);
	if (worldVertices) spFloatArray_dispose(worldVertices);
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
		
		if (skeleton && !skeleton->IsBeingDestroyed() && skeleton->GetSkeleton() && skeleton->Atlas) {
			spColor_setFromFloats(&skeleton->GetSkeleton()->color, Color.R, Color.G, Color.B, Color.A);

			if (atlasNormalBlendMaterials.Num() != skeleton->Atlas->atlasPages.Num()) {
				atlasNormalBlendMaterials.SetNum(0);
				pageToNormalBlendMaterial.Empty();
				atlasAdditiveBlendMaterials.SetNum(0);
				pageToAdditiveBlendMaterial.Empty();
				atlasMultiplyBlendMaterials.SetNum(0);
				pageToMultiplyBlendMaterial.Empty();
				atlasScreenBlendMaterials.SetNum(0);
				pageToScreenBlendMaterial.Empty();
				
				spAtlasPage* currPage = skeleton->Atlas->GetAtlas(false)->pages;
				for (int i = 0; i < skeleton->Atlas->atlasPages.Num(); i++) {
					
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(NormalBlendMaterial, owner);
					material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
					atlasNormalBlendMaterials.Add(material);
					pageToNormalBlendMaterial.Add(currPage, material);
					
					material = UMaterialInstanceDynamic::Create(AdditiveBlendMaterial, owner);
					material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
					atlasAdditiveBlendMaterials.Add(material);
					pageToAdditiveBlendMaterial.Add(currPage, material);
					
					material = UMaterialInstanceDynamic::Create(MultiplyBlendMaterial, owner);
					material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
					atlasMultiplyBlendMaterials.Add(material);
					pageToMultiplyBlendMaterial.Add(currPage, material);
					
					material = UMaterialInstanceDynamic::Create(ScreenBlendMaterial, owner);
					material->SetTextureParameterValue(TextureParameterName, skeleton->Atlas->atlasPages[i]);
					atlasScreenBlendMaterials.Add(material);
					pageToScreenBlendMaterial.Add(currPage, material);
					
					currPage = currPage->next;
				}
			} else {
				pageToNormalBlendMaterial.Empty();
				pageToAdditiveBlendMaterial.Empty();
				pageToMultiplyBlendMaterial.Empty();
				pageToScreenBlendMaterial.Empty();
				
				spAtlasPage* currPage = skeleton->Atlas->GetAtlas(false)->pages;
				for (int i = 0; i < skeleton->Atlas->atlasPages.Num(); i++) {
					UTexture2D* texture = skeleton->Atlas->atlasPages[i];
					UTexture* oldTexture = nullptr;
					
					UMaterialInstanceDynamic* current = atlasNormalBlendMaterials[i];
					if(!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
						UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(NormalBlendMaterial, owner);
						material->SetTextureParameterValue(TextureParameterName, texture);
						atlasNormalBlendMaterials[i] = material;
					}
					pageToNormalBlendMaterial.Add(currPage, atlasNormalBlendMaterials[i]);
					
					current = atlasAdditiveBlendMaterials[i];
					if(!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
						UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(AdditiveBlendMaterial, owner);
						material->SetTextureParameterValue(TextureParameterName, texture);
						atlasAdditiveBlendMaterials[i] = material;
					}
					pageToAdditiveBlendMaterial.Add(currPage, atlasAdditiveBlendMaterials[i]);
					
					current = atlasMultiplyBlendMaterials[i];
					if(!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
						UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(MultiplyBlendMaterial, owner);
						material->SetTextureParameterValue(TextureParameterName, texture);
						atlasMultiplyBlendMaterials[i] = material;
					}
					pageToMultiplyBlendMaterial.Add(currPage, atlasMultiplyBlendMaterials[i]);
					
					current = atlasScreenBlendMaterials[i];
					if(!current || !current->GetTextureParameterValue(TextureParameterName, oldTexture) || oldTexture != texture) {
						UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(ScreenBlendMaterial, owner);
						material->SetTextureParameterValue(TextureParameterName, texture);
						atlasScreenBlendMaterials[i] = material;
					}
					pageToScreenBlendMaterial.Add(currPage, atlasScreenBlendMaterials[i]);
					
					currPage = currPage->next;
				}
			}
			UpdateMesh(skeleton->GetSkeleton());
		} else {
			ClearAllMeshSections();
		}
	}
}


void USpineSkeletonRendererComponent::Flush (int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector>& Colors2, UMaterialInstanceDynamic* Material) {
	if (Vertices.Num() == 0) return;
	SetMaterial(Idx, Material);

	TArray<FRuntimeMeshVertexTripleUV> verts;
	for (int32 i = 0; i < Vertices.Num(); i++) {
		verts.Add(FRuntimeMeshVertexTripleUV(Vertices[i], FVector(), FVector(), Colors[i], Uvs[i], FVector2D(Colors2[i].X, Colors2[i].Y), FVector2D(Colors2[i].Z, 0)));
	}

	CreateMeshSection(Idx, verts, Indices);

	// CreateMeshSection(Idx, Vertices, Indices, TArray<FVector>(), Uvs, darkRG, Colors, TArray<FRuntimeMeshTangent>(), false);
	Vertices.SetNum(0);
	Indices.SetNum(0);
	Uvs.SetNum(0);
	Colors.SetNum(0);
	Colors2.SetNum(0);
	Idx++;
}

void USpineSkeletonRendererComponent::UpdateMesh(spSkeleton* Skeleton) {
	TArray<FVector> vertices;
	TArray<int32> indices;
	TArray<FVector2D> uvs;
	TArray<FColor> colors;
	TArray<FVector> darkColors;
	
	int idx = 0;
	int meshSection = 0;
	UMaterialInstanceDynamic* lastMaterial = nullptr;

	ClearAllMeshSections();

	float depthOffset = 0;
	unsigned short quadIndices[] = { 0, 1, 2, 0, 2, 3 };

	for (int i = 0; i < Skeleton->slotsCount; ++i) {
		float* attachmentVertices = worldVertices->items;
		unsigned short* attachmentIndices = nullptr;
		int numVertices;
		int numIndices;
		spAtlasRegion* attachmentAtlasRegion = nullptr;
		spColor attachmentColor;
		spColor_setFromFloats(&attachmentColor, 1, 1, 1, 1);
		float* attachmentUvs = nullptr;

		spSlot* slot = Skeleton->drawOrder[i];
		spAttachment* attachment = slot->attachment;
		if (!attachment) continue;
		if (attachment->type != SP_ATTACHMENT_REGION && attachment->type != SP_ATTACHMENT_MESH && attachment->type != SP_ATTACHMENT_CLIPPING) continue;
		
		if (attachment->type == SP_ATTACHMENT_REGION) {
			spRegionAttachment* regionAttachment = (spRegionAttachment*)attachment;
			spColor_setFromColor(&attachmentColor, &regionAttachment->color);
			attachmentAtlasRegion = (spAtlasRegion*)regionAttachment->rendererObject;
			spRegionAttachment_computeWorldVertices(regionAttachment, slot->bone, attachmentVertices, 0, 2);
			attachmentIndices = quadIndices;
			attachmentUvs = regionAttachment->uvs;
			numVertices = 4;
			numIndices = 6;
		} else if (attachment->type == SP_ATTACHMENT_MESH) {
			spMeshAttachment* mesh = (spMeshAttachment*)attachment;
			spColor_setFromColor(&attachmentColor, &mesh->color);
			attachmentAtlasRegion = (spAtlasRegion*)mesh->rendererObject;			
			if (mesh->super.worldVerticesLength > worldVertices->size) spFloatArray_setSize(worldVertices, mesh->super.worldVerticesLength);
			spVertexAttachment_computeWorldVertices(&mesh->super, slot, 0, mesh->super.worldVerticesLength, attachmentVertices, 0, 2);
			attachmentIndices = mesh->triangles;
			attachmentUvs = mesh->uvs;
			numVertices = mesh->super.worldVerticesLength >> 1;
			numIndices = mesh->trianglesCount;
		} else /* clipping */ {
			spClippingAttachment* clip = (spClippingAttachment*)slot->attachment;
			spSkeletonClipping_clipStart(clipper, slot, clip);
			continue;
		}

		// if the user switches the atlas data while not having switched
		// to the correct skeleton data yet, we won't find any regions.
		// ignore regions for which we can't find a material
		UMaterialInstanceDynamic* material = nullptr;
		switch (slot->data->blendMode) {
		case SP_BLEND_MODE_NORMAL:
			if (!pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = pageToNormalBlendMaterial[attachmentAtlasRegion->page];
			break;
		case SP_BLEND_MODE_ADDITIVE:
			if (!pageToAdditiveBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = pageToAdditiveBlendMaterial[attachmentAtlasRegion->page];
			break;
		case SP_BLEND_MODE_MULTIPLY:
			if (!pageToMultiplyBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = pageToMultiplyBlendMaterial[attachmentAtlasRegion->page];
			break;
		case SP_BLEND_MODE_SCREEN:
			if (!pageToScreenBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = pageToScreenBlendMaterial[attachmentAtlasRegion->page];
			break;
		default:
			if (!pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = pageToNormalBlendMaterial[attachmentAtlasRegion->page];
		}

		if (spSkeletonClipping_isClipping(clipper)) {
			spSkeletonClipping_clipTriangles(clipper, attachmentVertices, numVertices << 1, attachmentIndices, numIndices, attachmentUvs, 2);
			attachmentVertices = clipper->clippedVertices->items;
			numVertices = clipper->clippedVertices->size >> 1;
			attachmentIndices = clipper->clippedTriangles->items;
			numIndices = clipper->clippedTriangles->size;
			attachmentUvs = clipper->clippedUVs->items;
			if (clipper->clippedTriangles->size == 0) continue;
		}

		if (lastMaterial != material) {
			Flush(meshSection, vertices, indices, uvs, colors, darkColors, lastMaterial);
			lastMaterial = material;
			idx = 0;
		}

		SetMaterial(meshSection, material);

		uint8 r = static_cast<uint8>(Skeleton->color.r * slot->color.r * attachmentColor.r * 255);
		uint8 g = static_cast<uint8>(Skeleton->color.g * slot->color.g * attachmentColor.g * 255);
		uint8 b = static_cast<uint8>(Skeleton->color.b * slot->color.b * attachmentColor.b * 255);
		uint8 a = static_cast<uint8>(Skeleton->color.a * slot->color.a * attachmentColor.a * 255);

		float dr = slot->darkColor ? slot->darkColor->r : 0.0f;
		float dg = slot->darkColor ? slot->darkColor->g : 0.0f;
		float db = slot->darkColor ? slot->darkColor->b : 0.0f;		

		for (int j = 0; j < numVertices << 1; j += 2) {
			colors.Add(FColor(r, g, b, a));
			darkColors.Add(FVector(dr, dg, db));
			vertices.Add(FVector(attachmentVertices[j], depthOffset, attachmentVertices[j + 1]));
			uvs.Add(FVector2D(attachmentUvs[j], attachmentUvs[j + 1]));
		}

		for (int j = 0; j < numIndices; j++) {
			indices.Add(idx + attachmentIndices[j]);
		}

		idx += numVertices;
		depthOffset += this->DepthOffset;

		spSkeletonClipping_clipEnd(clipper, slot);			
	}
	
	Flush(meshSection, vertices, indices, uvs, colors, darkColors, lastMaterial);
	spSkeletonClipping_clipEnd2(clipper);
}

#undef LOCTEXT_NAMESPACE
