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

// Workaround for https://github.com/EsotericSoftware/spine-runtimes/issues/1458
// See issue comments for more information.
struct SpineSlateMaterialBrush : public FSlateBrush {
	SpineSlateMaterialBrush(class UMaterialInterface &InMaterial, const FVector2D &InImageSize)
		: FSlateBrush(ESlateBrushDrawType::Image, FName(TEXT("None")), FMargin(0), ESlateBrushTileType::NoTile, ESlateBrushImageType::FullColor, InImageSize, FLinearColor::White, &InMaterial) {
		ResourceName = FName(*InMaterial.GetFullName());
	}
};

void SSpineWidget::Construct(const FArguments& args) {
}

void SSpineWidget::SetData(USpineWidget* Widget) {
	this->widget = Widget;
	if (widget && widget->skeleton && widget->Atlas) {
		Skeleton *skeleton = widget->skeleton;
		skeleton->setToSetupPose();
		skeleton->updateWorldTransform();
		Vector<float> scratchBuffer;
		skeleton->getBounds(this->boundsMin.X, this->boundsMin.Y, this->boundsSize.X, this->boundsSize.Y, scratchBuffer);
	}
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
	UMaterialInstanceDynamic* MatNow = nullptr;

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
				AtlasPage* currPage = widget->Atlas->GetAtlas()->getPages()[i];

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
				AtlasPage* currPage = widget->Atlas->GetAtlas()->getPages()[i];

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

		self->UpdateMesh(LayerId, OutDrawElements, AllottedGeometry, widget->skeleton);
	}

	return LayerId;
}

void SSpineWidget::Flush(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector>& Colors2, UMaterialInstanceDynamic* Material) {
	if (Vertices.Num() == 0) return;
	SSpineWidget* self = (SSpineWidget*)this;

	const FVector2D widgetSize = AllottedGeometry.GetDrawSize();
	const FVector2D sizeScale = widgetSize / FVector2D(boundsSize.X, boundsSize.Y);
	const float setupScale = sizeScale.GetMin();

	for (int i = 0; i < Vertices.Num(); i++) {
		Vertices[i] = (Vertices[i] + FVector(-boundsMin.X - boundsSize.X / 2, boundsMin.Y + boundsSize.Y / 2, 0)) * setupScale * widget->Scale + FVector(widgetSize.X / 2, widgetSize.Y / 2, 0);
	}

	self->renderData.IndexData.SetNumUninitialized(Indices.Num());
	SlateIndex* indexData = (SlateIndex*)renderData.IndexData.GetData();
	for (int i = 0; i < Indices.Num(); i++) {
		indexData[i] = (SlateIndex)Indices[i];
	}
	
	self->renderData.VertexData.SetNumUninitialized(Vertices.Num());
	FSlateVertex* vertexData = (FSlateVertex*)renderData.VertexData.GetData();
	FVector2D offset = AllottedGeometry.AbsolutePosition;
	FColor white = FColor(0xffffffff);

	for (size_t i = 0; i < (size_t)Vertices.Num(); i++) {
		setVertex(&vertexData[i], Vertices[i].X, Vertices[i].Y, Uvs[i].X, Uvs[i].Y, Colors[i], offset);
	}

	brush = &widget->Brush;
	if (Material) {
		renderData.Brush = MakeShareable(new SpineSlateMaterialBrush(*Material, FVector2D(64, 64)));
		renderData.RenderingResourceHandle = FSlateApplication::Get().GetRenderer()->GetResourceHandle(*renderData.Brush);
	}

	if (renderData.RenderingResourceHandle.IsValid()) {
		FSlateDrawElement::MakeCustomVerts(OutDrawElements, LayerId, renderData.RenderingResourceHandle, renderData.VertexData, renderData.IndexData, nullptr, 0, 0);
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

	for (int i = 0; i < (int)Skeleton->getSlots().size(); ++i) {
		Vector<float> *attachmentVertices = &worldVertices;
		unsigned short* attachmentIndices = nullptr;
		int numVertices;
		int numIndices;
		AtlasRegion* attachmentAtlasRegion = nullptr;
		Color attachmentColor;
		attachmentColor.set(1, 1, 1, 1);
		float* attachmentUvs = nullptr;

		Slot* slot = Skeleton->getDrawOrder()[i];
		if (!slot->getBone().isActive()) {
			clipper.clipEnd(*slot);
			continue;
		}

		Attachment* attachment = slot->getAttachment();
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
			attachmentColor.set(regionAttachment->getColor());
			attachmentAtlasRegion = (AtlasRegion*)regionAttachment->getRendererObject();
			regionAttachment->computeWorldVertices(slot->getBone(), *attachmentVertices, 0, 2);
			attachmentIndices = quadIndices;
			attachmentUvs = regionAttachment->getUVs().buffer();
			numVertices = 4;
			numIndices = 6;
		}
		else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
			MeshAttachment* mesh = (MeshAttachment*)attachment;
			attachmentColor.set(mesh->getColor());
			attachmentAtlasRegion = (AtlasRegion*)mesh->getRendererObject();
			mesh->computeWorldVertices(*slot, 0, mesh->getWorldVerticesLength(), *attachmentVertices, 0, 2);
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
			if (!widget->pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = widget->pageToNormalBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Additive:
			if (!widget->pageToAdditiveBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = widget->pageToAdditiveBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Multiply:
			if (!widget->pageToMultiplyBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = widget->pageToMultiplyBlendMaterial[attachmentAtlasRegion->page];
			break;
		case BlendMode_Screen:
			if (!widget->pageToScreenBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = widget->pageToScreenBlendMaterial[attachmentAtlasRegion->page];
			break;
		default:
			if (!widget->pageToNormalBlendMaterial.Contains(attachmentAtlasRegion->page)) {
				clipper.clipEnd(*slot);
				continue;
			}
			material = widget->pageToNormalBlendMaterial[attachmentAtlasRegion->page];
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

		float* verticesPtr = attachmentVertices->buffer();
		for (int j = 0; j < numVertices << 1; j += 2) {
			colors.Add(FColor(r, g, b, a));
			darkColors.Add(FVector(dr, dg, db));
			vertices.Add(FVector(verticesPtr[j], -verticesPtr[j + 1], depthOffset));
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
