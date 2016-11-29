
#pragma once

#include "UnrealEd.h"
#include "SpineAtlasAsset.h"
#include "SpineSkeletonImportFactory.generated.h"

UCLASS()
class USpineSkeletonAssetFactory : public UFactory, public FReimportHandler
{
	GENERATED_UCLASS_BODY()

	virtual FText GetToolTip() const override;
	virtual bool FactoryCanImport(const FString& Filename) override;
    virtual UObject* FactoryCreateFile (UClass * InClass, UObject * InParent, FName InName, EObjectFlags Flags, const FString & Filename, const TCHAR* Parms, FFeedbackContext * Warn, bool& bOutOperationCanceled) override;
    
    virtual bool CanReimport(UObject* Obj, TArray<FString>& OutFilenames) override;
    virtual void SetReimportPaths(UObject* Obj, const TArray<FString>& NewReimportPaths) override;
    virtual EReimportResult::Type Reimport(UObject* Obj) override;
};