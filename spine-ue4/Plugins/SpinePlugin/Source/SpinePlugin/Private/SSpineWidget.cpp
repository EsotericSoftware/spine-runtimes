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

#include "SSpineWidget.h"
#include "Framework/Application/SlateApplication.h"
#include "Materials/MaterialInterface.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "Modules/ModuleManager.h"
#include "Runtime/SlateRHIRenderer/Public/Interfaces/ISlateRHIRendererModule.h"
#include "Rendering/DrawElements.h"
#include "Slate/SlateVectorArtData.h"
#include "Slate/SlateVectorArtInstanceData.h"
#include "Slate/SMeshWidget.h"
#include "SlateMaterialBrush.h"
#include <spine/spine.h>
#include "SpineWidget.h"

using namespace spine;

void SSpineWidget::Construct(const FArguments& args) {
}

void SSpineWidget::SetData(USpineWidget* Widget) {
	this->widget = Widget;
}

static void setVertex(FSlateVertex* vertex, float x, float y, float u, float v, const FColor& color, const FVector2D& offset) {
	vertex->Position.X = offset.X + x;
	vertex->Position.Y = offset.Y + y;
	vertex->TexCoords[0] = u;
	vertex->TexCoords[1] = v;
	vertex->TexCoords[2] = u;
	vertex->TexCoords[3] = v;
	vertex->MaterialTexCoords.X = u;
	vertex->MaterialTexCoords.Y = v;
	vertex->Color = color;
	vertex->PixelSize[0] = 1;
	vertex->PixelSize[1] = 1;
}

int32 SSpineWidget::OnPaint(const FPaintArgs& Args, const FGeometry& AllottedGeometry, const FSlateRect& MyClippingRect, FSlateWindowElementList& OutDrawElements,
							   int32 LayerId, const FWidgetStyle& InWidgetStyle, bool bParentEnabled) const {

	SSpineWidget* self = (SSpineWidget*)this;

	if (widget && widget->skeleton && widget->Atlas) {		
		widget->skeleton->getColor().set(widget->Color.R, widget->Color.G, widget->Color.B, widget->Color.A);

		if (widget->atlasNormalBlendMaterials.Num() != widget->Atlas->atlasPages.Num()) {
			widget->atlasNormalBlendMaterials.SetNum(0);
			widget->pageToNormalBlendMaterial.Empty();
			widget->atlasAdditiveBlendMaterials.SetNum(0);
			widget->pageToAdditiveBlendMaterial.Empty();
			widget->atlasMultiplyBlendMaterials.SetNum(0);
			widget->pageToMultiplyBlendMaterial.Empty();
			widget->atlasScreenBlendMaterials.SetNum(0);
			widget->pageToScreenBlendMaterial.Empty();

			for (int i = 0; i < widget->Atlas->atlasPages.Num(); i++) {
				AtlasPage* currPage = widget->Atlas->GetAtlas(false)->getPages()[i];

				UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(widget->NormalBlendMaterial, widget);
				material->SetTextureParameterValue(widget->TextureParameterName, widget->Atlas->atlasPages[i]);
				widget->atlasNormalBlendMaterials.Add(material);
				widget->pageToNormalBlendMaterial.Add(currPage, material);

				material = UMaterialInstanceDynamic::Create(widget->AdditiveBlendMaterial, widget);
				material->SetTextureParameterValue(widget->TextureParameterName, widget->Atlas->atlasPages[i]);
				widget->atlasAdditiveBlendMaterials.Add(material);
				widget->pageToAdditiveBlendMaterial.Add(currPage, material);

				material = UMaterialInstanceDynamic::Create(widget->MultiplyBlendMaterial, widget);
				material->SetTextureParameterValue(widget->TextureParameterName, widget->Atlas->atlasPages[i]);
				widget->atlasMultiplyBlendMaterials.Add(material);
				widget->pageToMultiplyBlendMaterial.Add(currPage, material);

				material = UMaterialInstanceDynamic::Create(widget->ScreenBlendMaterial, widget);
				material->SetTextureParameterValue(widget->TextureParameterName, widget->Atlas->atlasPages[i]);
				widget->atlasScreenBlendMaterials.Add(material);
				widget->pageToScreenBlendMaterial.Add(currPage, material);
			}
		} else {
			widget->pageToNormalBlendMaterial.Empty();
			widget->pageToAdditiveBlendMaterial.Empty();
			widget->pageToMultiplyBlendMaterial.Empty();
			widget->pageToScreenBlendMaterial.Empty();

			for (int i = 0; i < widget->Atlas->atlasPages.Num(); i++) {
				AtlasPage* currPage = widget->Atlas->GetAtlas(false)->getPages()[i];

				UTexture2D* texture = widget->Atlas->atlasPages[i];
				UTexture* oldTexture = nullptr;

				UMaterialInstanceDynamic* current = widget->atlasNormalBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(widget->TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(widget->NormalBlendMaterial, widget);
					material->SetTextureParameterValue(widget->TextureParameterName, texture);
					widget->atlasNormalBlendMaterials[i] = material;
				}
				widget->pageToNormalBlendMaterial.Add(currPage, widget->atlasNormalBlendMaterials[i]);

				current = widget->atlasAdditiveBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(widget->TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(widget->AdditiveBlendMaterial, widget);
					material->SetTextureParameterValue(widget->TextureParameterName, texture);
					widget->atlasAdditiveBlendMaterials[i] = material;
				}
				widget->pageToAdditiveBlendMaterial.Add(currPage, widget->atlasAdditiveBlendMaterials[i]);

				current = widget->atlasMultiplyBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(widget->TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(widget->MultiplyBlendMaterial, widget);
					material->SetTextureParameterValue(widget->TextureParameterName, texture);
					widget->atlasMultiplyBlendMaterials[i] = material;
				}
				widget->pageToMultiplyBlendMaterial.Add(currPage, widget->atlasMultiplyBlendMaterials[i]);

				current = widget->atlasScreenBlendMaterials[i];
				if (!current || !current->GetTextureParameterValue(widget->TextureParameterName, oldTexture) || oldTexture != texture) {
					UMaterialInstanceDynamic* material = UMaterialInstanceDynamic::Create(widget->ScreenBlendMaterial, widget);
					material->SetTextureParameterValue(widget->TextureParameterName, texture);
					widget->atlasScreenBlendMaterials[i] = material;
				}
				widget->pageToScreenBlendMaterial.Add(currPage, widget->atlasScreenBlendMaterials[i]);
			}
		}
		// self->UpdateMesh(LayerId, OutDrawElements, AllottedGeometry, widget->skeleton);
	}
	//return LayerId;
	
	self->renderData.IndexData.SetNumUninitialized(6);
	uint32* indexData = (uint32*)renderData.IndexData.GetData();
	indexData[0] = 0;
	indexData[1] = 1;
	indexData[2] = 2;
	indexData[3] = 2;
	indexData[4] = 3;
	indexData[5] = 0;

	self->renderData.VertexData.SetNumUninitialized(4);
	FSlateVertex* vertexData = (FSlateVertex*)renderData.VertexData.GetData();
	FVector2D offset = AllottedGeometry.AbsolutePosition;
	FColor white = FColor(0xffffffff);

	float width = AllottedGeometry.GetAbsoluteSize().X;
	float height = AllottedGeometry.GetAbsoluteSize().Y;

	setVertex(&vertexData[0], 0, 0, 0, 0, white, offset);
	setVertex(&vertexData[1], width, 0, 1, 0, white, offset);
	setVertex(&vertexData[2], width, height, 1, 1, white, offset);
	setVertex(&vertexData[3], 0, height, 0, 1, white, offset);

	if (brush && renderData.VertexData.Num() > 0 && renderData.IndexData.Num() > 0) {
		FSlateShaderResourceProxy* shaderResource = FSlateDataPayload::ResourceManager->GetShaderResource(widget->Brush);
		FSlateResourceHandle resourceHandle = FSlateApplication::Get().GetRenderer()->GetResourceHandle(widget->Brush);
		if (shaderResource)
			FSlateDrawElement::MakeCustomVerts(OutDrawElements, LayerId, resourceHandle, renderData.VertexData,
											   renderData.IndexData, nullptr, 0, 0);
	}

	return LayerId;
}

void SSpineWidget::Flush(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector>& Colors2, UMaterialInstanceDynamic* Material) {
	if (Vertices.Num() == 0) return;
	SSpineWidget* self = (SSpineWidget*)this;
	
	self->renderData.IndexData.SetNumUninitialized(Indices.Num());
	uint32* indexData = (uint32*)renderData.IndexData.GetData();
	memcpy(indexData, Indices.GetData(), sizeof(uint32) * Indices.Num());

	self->renderData.VertexData.SetNumUninitialized(Vertices.Num());
	FSlateVertex* vertexData = (FSlateVertex*)renderData.VertexData.GetData();
	FVector2D offset = AllottedGeometry.AbsolutePosition;
	FColor white = FColor(0xffffffff);

	float width = AllottedGeometry.GetAbsoluteSize().X;
	float height = AllottedGeometry.GetAbsoluteSize().Y;

	for (size_t i = 0; i < Vertices.Num(); i++) {
		setVertex(&vertexData[i], Vertices[i].X, Vertices[i].Y, Uvs[i].X, Uvs[i].Y, Colors[i], offset);
	}

	FSlateBrush brush;
	brush.SetResourceObject(Material);
	brush = widget->Brush;

	FSlateShaderResourceProxy* shaderResource = FSlateDataPayload::ResourceManager->GetShaderResource(brush);	
	if (shaderResource) {
		FSlateResourceHandle resourceHandle = FSlateApplication::Get().GetRenderer()->GetResourceHandle(brush);
		FSlateDrawElement::MakeCustomVerts(OutDrawElements, LayerId, resourceHandle, renderData.VertexData, renderData.IndexData, nullptr, 0, 0);
	}

	Vertices.SetNum(0);
	Indices.SetNum(0);
	Uvs.SetNum(0);
	Colors.SetNum(0);
	Colors2.SetNum(0);
	Idx++;
}

void SSpineWidget::UpdateMesh(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, Skeleton* Skeleton) {
	TArray<FVector> vertices;
	TArray<int32> indices;
	TArray<FVector2D> uvs;
	TArray<FColor> colors;
	TArray<FVector> darkColors;

	int idx = 0;
	int meshSection = 0;
	UMaterialInstanceDynamic* lastMaterial = nullptr;

	SkeletonClipping &clipper = widget->clipper;
	Vector<float> &worldVertices = widget->worldVertices;

	float depthOffset = 0;
	unsigned short quadIndices[] = { 0, 1, 2, 0, 2, 3 };

	for (int i = 0; i < Skeleton->getSlots().size(); ++i) {
		Vector<float> &attachmentVertices = worldVertices;
		unsigned short* attachmentIndices = nullptr;
		int numVertices;
		int numIndices;
		AtlasRegion* attachmentAtlasRegion = nullptr;
		Color attachmentColor;
		attachmentColor.set(1, 1, 1, 1);
		float* attachmentUvs = nullptr;

		Slot* slot = Skeleton->getDrawOrder()[i];
		Attachment* attachment = slot->getAttachment();
		if (!attachment) continue;
		if (!attachment->getRTTI().isExactly(RegionAttachment::rtti) && !attachment->getRTTI().isExactly(MeshAttachment::rtti) && !attachment->getRTTI().isExactly(ClippingAttachment::rtti)) continue;

		if (attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
			RegionAttachment* regionAttachment = (RegionAttachment*)attachment;
			attachmentColor.set(regionAttachment->getColor());
			attachmentAtlasRegion = (AtlasRegion*)regionAttachment->getRendererObject();
			regionAttachment->computeWorldVertices(slot->getBone(), attachmentVertices, 0, 2);
			attachmentIndices = quadIndices;
			attachmentUvs = regionAttachment->getUVs().buffer();
			numVertices = 4;
			numIndices = 6;
		}
		else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
			MeshAttachment* mesh = (MeshAttachment*)attachment;
			attachmentColor.set(mesh->getColor());
			attachmentAtlasRegion = (AtlasRegion*)mesh->getRendererObject();
			mesh->computeWorldVertices(*slot, 0, mesh->getWorldVerticesLength(), attachmentVertices, 0, 2);
			attachmentIndices = mesh->getTriangles().buffer();
			attachmentUvs = mesh->getUVs().buffer();
			numVertices = mesh->getWorldVerticesLength() >> 1;
			numIndices = mesh->getTriangles().size();
		}
		else /* clipping */ {
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
			if (!widget->pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = widget->pageToNormalBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Additive:
			if (!widget->pageToAdditiveBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = widget->pageToAdditiveBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Multiply:
			if (!widget->pageToMultiplyBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = widget->pageToMultiplyBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Screen:
			if (!widget->pageToScreenBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = widget->pageToScreenBlendMaterial[attachmentAtlasRegion->page];
			break;
		default:
			if (!widget->pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) continue;
			material = widget->pageToNormalBlendMaterial[attachmentAtlasRegion->page];
		}

		if (clipper.isClipping()) {
			clipper.clipTriangles(attachmentVertices.buffer(), attachmentIndices, numIndices, attachmentUvs, 2);
			attachmentVertices = clipper.getClippedVertices();
			numVertices = clipper.getClippedVertices().size() >> 1;
			attachmentIndices = clipper.getClippedTriangles().buffer();
			numIndices = clipper.getClippedTriangles().size();
			attachmentUvs = clipper.getClippedUVs().buffer();
			if (clipper.getClippedTriangles().size() == 0) continue;
		}

		if (lastMaterial != material) {
			Flush(LayerId, OutDrawElements, AllottedGeometry, meshSection, vertices, indices, uvs, colors, darkColors, lastMaterial);
			lastMaterial = material;
			idx = 0;
		}

		uint8 r = static_cast<uint8>(Skeleton->getColor().r * slot->getColor().r * attachmentColor.r * 255);
		uint8 g = static_cast<uint8>(Skeleton->getColor().g * slot->getColor().g * attachmentColor.g * 255);
		uint8 b = static_cast<uint8>(Skeleton->getColor().b * slot->getColor().b * attachmentColor.b * 255);
		uint8 a = static_cast<uint8>(Skeleton->getColor().a * slot->getColor().a * attachmentColor.a * 255);

		float dr = slot->hasDarkColor() ? slot->getDarkColor().r : 0.0f;
		float dg = slot->hasDarkColor() ? slot->getDarkColor().g : 0.0f;
		float db = slot->hasDarkColor() ? slot->getDarkColor().b : 0.0f;

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
		depthOffset += widget->DepthOffset;

		clipper.clipEnd(*slot);
	}

	Flush(LayerId, OutDrawElements, AllottedGeometry, meshSection, vertices, indices, uvs, colors, darkColors, lastMaterial);
	clipper.clipEnd();
}