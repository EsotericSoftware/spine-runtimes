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

void SSpineWidget::Construct(const FArguments& args) {
}

void SSpineWidget::SetBrush(FSlateBrush* Brush) {
	brush = Brush;
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
	setVertex(&vertexData[0], 0, 0, 0, 0, white, offset);
	setVertex(&vertexData[1], 200, 0, 1, 0, white, offset);
	setVertex(&vertexData[2], 200, 200, 1, 1, white, offset);
	setVertex(&vertexData[3], 0, 200, 0, 1, white, offset);

	if (brush && renderData.VertexData.Num() > 0 && renderData.IndexData.Num() > 0) {
		FSlateShaderResourceProxy* shaderResource = FSlateDataPayload::ResourceManager->GetShaderResource(*brush);
		FSlateResourceHandle resourceHandle = FSlateApplication::Get().GetRenderer()->GetResourceHandle(*brush);
		if (shaderResource)
			FSlateDrawElement::MakeCustomVerts(OutDrawElements, LayerId, resourceHandle, renderData.VertexData,
											   renderData.IndexData, nullptr, 0, 0);
	}

	return LayerId;
}