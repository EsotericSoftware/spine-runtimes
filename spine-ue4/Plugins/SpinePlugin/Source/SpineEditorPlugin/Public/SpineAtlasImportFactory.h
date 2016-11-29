
#pragma once

#include "UnrealEd.h"
#include "SpineAtlasAsset.h"
#include "SpineAtlasImportFactory.generated.h"

UCLASS()
class USpineAtlasAssetFactory : public UFactory, public FReimportHandler
{
	GENERATED_UCLASS_BODY()

	virtual FText GetToolTip() const override;
	virtual bool FactoryCanImport(const FString& Filename) override;
    virtual UObject* FactoryCreateFile (UClass * InClass, UObject * InParent, FName InName, EObjectFlags Flags, const FString & Filename, const TCHAR* Parms, FFeedbackContext * Warn, bool& bOutOperationCanceled) override;
    
    virtual bool CanReimport(UObject* Obj, TArray<FString>& OutFilenames) override;
    virtual void SetReimportPaths(UObject* Obj, const TArray<FString>& NewReimportPaths) override;
    virtual EReimportResult::Type Reimport(UObject* Obj) override;

    void LoadAtlas(USpineAtlasAsset* asset, const FString& currentSourcePath, const FString& longPackagePath);
};