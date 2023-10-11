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

#include "SpineEditorPlugin.h"
#include "AssetTypeActions_Base.h"
#include "SpineAtlasAsset.h"
#include "SpineSkeletonDataAsset.h"

class FSpineAtlasAssetTypeActions : public FAssetTypeActions_Base {
public:
	UClass *GetSupportedClass() const override { return USpineAtlasAsset::StaticClass(); };
	FText GetName() const override { return INVTEXT("Spine atlas asset"); };
	FColor GetTypeColor() const override { return FColor::Red; };
	uint32 GetCategories() override { return EAssetTypeCategories::Misc; };
};

class FSpineSkeletonDataAssetTypeActions : public FAssetTypeActions_Base {
public:
	UClass *GetSupportedClass() const override { return USpineSkeletonDataAsset::StaticClass(); };
	FText GetName() const override { return INVTEXT("Spine data asset"); };
	FColor GetTypeColor() const override { return FColor::Red; };
	uint32 GetCategories() override { return EAssetTypeCategories::Misc; };
};

class FSpineEditorPlugin : public ISpineEditorPlugin {
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
	TSharedPtr<FSpineAtlasAssetTypeActions> SpineAtlasAssetTypeActions;
	TSharedPtr<FSpineSkeletonDataAssetTypeActions> SpineSkeletonDataAssetTypeActions;
};

IMPLEMENT_MODULE(FSpineEditorPlugin, SpineEditorPlugin)

void FSpineEditorPlugin::StartupModule() {
	SpineAtlasAssetTypeActions = MakeShared<FSpineAtlasAssetTypeActions>();
	FAssetToolsModule::GetModule().Get().RegisterAssetTypeActions(SpineAtlasAssetTypeActions.ToSharedRef());
	SpineSkeletonDataAssetTypeActions = MakeShared<FSpineSkeletonDataAssetTypeActions>();
	FAssetToolsModule::GetModule().Get().RegisterAssetTypeActions(SpineSkeletonDataAssetTypeActions.ToSharedRef());
}

void FSpineEditorPlugin::ShutdownModule() {
	if (!FModuleManager::Get().IsModuleLoaded("AssetTools")) return;
	FAssetToolsModule::GetModule().Get().UnregisterAssetTypeActions(SpineAtlasAssetTypeActions.ToSharedRef());
	FAssetToolsModule::GetModule().Get().UnregisterAssetTypeActions(SpineSkeletonDataAssetTypeActions.ToSharedRef());
}
