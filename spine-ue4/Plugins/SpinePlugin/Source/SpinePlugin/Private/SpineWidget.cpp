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
#include "SpineWidget.h"
#include "SSpineWidget.h"
#include "Engine.h"

#define LOCTEXT_NAMESPACE "Spine"

USpineWidget::USpineWidget(const FObjectInitializer& ObjectInitializer): Super(ObjectInitializer) {
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

void USpineWidget::SynchronizeProperties() {
	Super::SynchronizeProperties();

	if (slateWidget.IsValid()) {
		CheckState();
		if (skeleton) {
			InternalTick(0);			
			slateWidget->SetData(this);
		} else {
			slateWidget->SetData(nullptr);		
		}
	}
}

void USpineWidget::ReleaseSlateResources(bool bReleaseChildren) {
	Super::ReleaseSlateResources(bReleaseChildren);
	slateWidget.Reset();
}

TSharedRef<SWidget> USpineWidget::RebuildWidget() {
	this->slateWidget = SNew(SSpineWidget);
	return this->slateWidget.ToSharedRef();
}

#if WITH_EDITOR
const FText USpineWidget::GetPaletteCategory() {
	return LOCTEXT("Spine", "Spine");
}
#endif

void USpineWidget::InternalTick(float DeltaTime) {
	CheckState();

	if (skeleton) {
		skeleton->updateWorldTransform();
	}
}

void USpineWidget::CheckState() {
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();

		if (Atlas && SkeletonData) {
			spine::SkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			skeleton = new (__FILE__, __LINE__) spine::Skeleton(data);
		}

		lastAtlas = Atlas;
		lastData = SkeletonData;
	}
}

void USpineWidget::DisposeState() {
	if (skeleton) {
		delete skeleton;
		skeleton = nullptr;
	}
}

void USpineWidget::FinishDestroy() {
	DisposeState();
	Super::FinishDestroy();
}