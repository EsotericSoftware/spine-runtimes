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

#pragma once

#include "SlateCore.h"
#include "Slate/SMeshWidget.h"  
#include <spine/spine.h>
#include "SpineAtlasAsset.h"

class USpineWidget;

class SSpineWidget: public SMeshWidget {

public:
	SLATE_BEGIN_ARGS(SSpineWidget): _MeshData(nullptr) { }
	SLATE_ARGUMENT(USlateVectorArtData*, MeshData)
	SLATE_END_ARGS()

	void Construct(const FArguments& Args);

	void SetData(USpineWidget* Widget);

protected:
	virtual int32 OnPaint(const FPaintArgs& Args, const FGeometry& AllottedGeometry, const FSlateRect& MyCullingRect, FSlateWindowElementList& OutDrawElements, int32 LayerId, const FWidgetStyle& InWidgetStyle, bool bParentEnabled) const override;

	void UpdateMesh(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, spine::Skeleton* Skeleton);

	void Flush(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector> &Colors2, UMaterialInstanceDynamic* Material);

	USpineWidget* widget;
	FSlateBrush* brush;
	FRenderData renderData;
};
