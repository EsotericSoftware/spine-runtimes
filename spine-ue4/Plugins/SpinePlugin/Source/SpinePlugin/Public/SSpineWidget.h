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
	FSlateBrush* brush;
protected:
	virtual int32 OnPaint(const FPaintArgs& Args, const FGeometry& AllottedGeometry, const FSlateRect& MyCullingRect, FSlateWindowElementList& OutDrawElements, int32 LayerId, const FWidgetStyle& InWidgetStyle, bool bParentEnabled) const override;

	void UpdateMesh(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, spine::Skeleton* Skeleton);

	void Flush(int32 LayerId, FSlateWindowElementList& OutDrawElements, const FGeometry& AllottedGeometry, int &Idx, TArray<FVector> &Vertices, TArray<int32> &Indices, TArray<FVector2D> &Uvs, TArray<FColor> &Colors, TArray<FVector> &Colors2, UMaterialInstanceDynamic* Material);

	USpineWidget* widget;	
	FRenderData renderData;
	FVector boundsMin;
	FVector boundsSize;
};
