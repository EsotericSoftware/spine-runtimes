// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#include "RuntimeMeshComponentEditorPrivatePCH.h"
#include "RuntimeMeshComponentDetails.h"
#include "RuntimeMeshComponent.h"

#include "DlgPickAssetPath.h"
#include "IAssetTools.h"
#include "AssetToolsModule.h"
#include "AssetRegistryModule.h"
#include "PhysicsEngine/PhysicsSettings.h"
#include "PhysicsEngine/BodySetup.h"

#define LOCTEXT_NAMESPACE "RuntimeMeshComponentDetails"

TSharedRef<IDetailCustomization> FRuntimeMeshComponentDetails::MakeInstance()
{
	return MakeShareable(new FRuntimeMeshComponentDetails);
}

void FRuntimeMeshComponentDetails::CustomizeDetails( IDetailLayoutBuilder& DetailBuilder )
{
	IDetailCategoryBuilder& RuntimeMeshCategory = DetailBuilder.EditCategory("RuntimeMesh");

	const FText ConvertToStaticMeshText = LOCTEXT("ConvertToStaticMesh", "Create StaticMesh");

	// Cache set of selected things
	SelectedObjectsList = DetailBuilder.GetDetailsView().GetSelectedObjects();

	RuntimeMeshCategory.AddCustomRow(ConvertToStaticMeshText, false)
	.NameContent()
	[
		SNullWidget::NullWidget
	]
	.ValueContent()
	.VAlign(VAlign_Center)
	.MaxDesiredWidth(250)
	[
		SNew(SButton)
		.VAlign(VAlign_Center)
		.ToolTipText(LOCTEXT("ConvertToStaticMeshTooltip", "Create a new StaticMesh asset using current geometry from this RuntimeMeshComponent. Does not modify instance."))
		.OnClicked(this, &FRuntimeMeshComponentDetails::ClickedOnConvertToStaticMesh)
		.IsEnabled(this, &FRuntimeMeshComponentDetails::ConvertToStaticMeshEnabled)
		.Content()
		[
			SNew(STextBlock)
			.Text(ConvertToStaticMeshText)
		]
	];
}

URuntimeMeshComponent* FRuntimeMeshComponentDetails::GetFirstSelectedRuntimeMeshComp() const
{
	// Find first selected valid RuntimeMeshComp
	URuntimeMeshComponent* RuntimeMeshComp = nullptr;
	for (const TWeakObjectPtr<UObject>& Object : SelectedObjectsList)
	{
		URuntimeMeshComponent* TestRuntimeComp = Cast<URuntimeMeshComponent>(Object.Get());
		// See if this one is good
		if (TestRuntimeComp != nullptr && !TestRuntimeComp->IsTemplate())
		{
			RuntimeMeshComp = TestRuntimeComp;
			break;
		}
	}

	return RuntimeMeshComp;
}


bool FRuntimeMeshComponentDetails::ConvertToStaticMeshEnabled() const
{
	return GetFirstSelectedRuntimeMeshComp() != nullptr;
}


FReply FRuntimeMeshComponentDetails::ClickedOnConvertToStaticMesh()
{
 	// Find first selected RuntimeMeshComp
 	URuntimeMeshComponent* RuntimeMeshComp = GetFirstSelectedRuntimeMeshComp();
 	if (RuntimeMeshComp != nullptr)
 	{
 		FString NewNameSuggestion = FString(TEXT("RuntimeMeshComp"));
 		FString PackageName = FString(TEXT("/Game/Meshes/")) + NewNameSuggestion;
 		FString Name;
 		FAssetToolsModule& AssetToolsModule = FModuleManager::LoadModuleChecked<FAssetToolsModule>("AssetTools");
 		AssetToolsModule.Get().CreateUniqueAssetName(PackageName, TEXT(""), PackageName, Name);
 
 		TSharedPtr<SDlgPickAssetPath> PickAssetPathWidget =
 			SNew(SDlgPickAssetPath)
 			.Title(LOCTEXT("ConvertToStaticMeshPickName", "Choose New StaticMesh Location"))
 			.DefaultAssetPath(FText::FromString(PackageName));
 
 		if (PickAssetPathWidget->ShowModal() == EAppReturnType::Ok)
 		{
 			// Get the full name of where we want to create the physics asset.
 			FString UserPackageName = PickAssetPathWidget->GetFullAssetPath().ToString();
 			FName MeshName(*FPackageName::GetLongPackageAssetName(UserPackageName));
 
 			// Check if the user inputed a valid asset name, if they did not, give it the generated default name
 			if (MeshName == NAME_None)
 			{
 				// Use the defaults that were already generated.
 				UserPackageName = PackageName;
 				MeshName = *Name;
 			}
 
 			// Raw mesh data we are filling in
 			FRawMesh RawMesh;
 					
			// Unique Materials to apply to new mesh
#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 14
			TArray<FStaticMaterial> Materials;
#else
			TArray<UMaterialInterface*> Materials;
#endif
 
			bool bUseHighPrecisionTangents = false;
			bool bUseFullPrecisionUVs = false;

 			const int32 NumSections = RuntimeMeshComp->GetNumSections();
 			int32 VertexBase = 0;
 			for (int32 SectionIdx = 0; SectionIdx < NumSections; SectionIdx++)
 			{
				const IRuntimeMeshVerticesBuilder* Vertices;
				const FRuntimeMeshIndicesBuilder* Indices;
				RuntimeMeshComp->GetSectionMesh(SectionIdx, Vertices, Indices);

				if (Vertices->HasHighPrecisionNormals())
				{
					bUseHighPrecisionTangents = true;
				}
				if (Vertices->HasHighPrecisionUVs())
				{
					bUseFullPrecisionUVs = true;
				}
 
 				// Copy verts
				Vertices->Seek(-1);
				while (Vertices->MoveNext() < Vertices->Length())
				{
					RawMesh.VertexPositions.Add(Vertices->GetPosition());
				}
 
 				// Copy 'wedge' info
				Indices->Seek(0);
				while (Indices->HasRemaining())
 				{
					int32 Index = Indices->ReadOne();
 
 					RawMesh.WedgeIndices.Add(Index + VertexBase);
 

					Vertices->Seek(Index);
 
					FVector TangentX = Vertices->GetTangent();
					FVector TangentZ = Vertices->GetNormal();
 					FVector TangentY = (TangentX ^ TangentZ).GetSafeNormal() * Vertices->GetNormal().W;
 
 					RawMesh.WedgeTangentX.Add(TangentX);
 					RawMesh.WedgeTangentY.Add(TangentY);
 					RawMesh.WedgeTangentZ.Add(TangentZ);
 
					for (int UVIndex = 0; UVIndex < 8; UVIndex++)
					{
						if (!Vertices->HasUVComponent(UVIndex))
						{
							break;
						}
						RawMesh.WedgeTexCoords[UVIndex].Add(Vertices->GetUV(UVIndex));
					}

 					RawMesh.WedgeColors.Add(Vertices->GetColor());
 				}
 
				// Find a material index for this section.
				UMaterialInterface* Material = RuntimeMeshComp->GetMaterial(SectionIdx);

#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 14
				int32 MaterialIndex = Materials.AddUnique(FStaticMaterial(Material));
#else
				int32 MaterialIndex = Materials.AddUnique(Material);
#endif
				

 				// copy face info
 				int32 NumTris = Indices->Length() / 3;
 				for (int32 TriIdx=0; TriIdx < NumTris; TriIdx++)
 				{
					// Set the face material
					RawMesh.FaceMaterialIndices.Add(MaterialIndex);

 					RawMesh.FaceSmoothingMasks.Add(0); // Assume this is ignored as bRecomputeNormals is false
 				}
 
 				// Update offset for creating one big index/vertex buffer
				VertexBase += Vertices->Length();
 			}
 
 			// If we got some valid data.
 			if (RawMesh.VertexPositions.Num() >= 3 && RawMesh.WedgeIndices.Num() >= 3)
 			{
 				// Then find/create it.
 				UPackage* Package = CreatePackage(NULL, *UserPackageName);
 				check(Package);
 
 				// Create StaticMesh object
 				UStaticMesh* StaticMesh = NewObject<UStaticMesh>(Package, MeshName, RF_Public | RF_Standalone);
 				StaticMesh->InitResources();
 
 				StaticMesh->LightingGuid = FGuid::NewGuid();
 
 				// Add source to new StaticMesh
 				FStaticMeshSourceModel* SrcModel = new (StaticMesh->SourceModels) FStaticMeshSourceModel();
 				SrcModel->BuildSettings.bRecomputeNormals = false;
 				SrcModel->BuildSettings.bRecomputeTangents = false;
 				SrcModel->BuildSettings.bRemoveDegenerates = false;
 				SrcModel->BuildSettings.bUseHighPrecisionTangentBasis = bUseHighPrecisionTangents;
				SrcModel->BuildSettings.bUseFullPrecisionUVs = bUseFullPrecisionUVs;
 				SrcModel->BuildSettings.bGenerateLightmapUVs = true;
 				SrcModel->BuildSettings.SrcLightmapIndex = 0;
 				SrcModel->BuildSettings.DstLightmapIndex = 1;
 				SrcModel->RawMeshBulkData->SaveRawMesh(RawMesh);
 
				// Set the materials used for this static mesh
#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 14
				StaticMesh->StaticMaterials = Materials;
				int32 NumMaterials = StaticMesh->StaticMaterials.Num();
#else
				StaticMesh->Materials = Materials;
				int32 NumMaterials = StaticMesh->Materials.Num();
#endif

				// Set up the SectionInfoMap to enable collision
				for (int32 SectionIdx = 0; SectionIdx < NumMaterials; SectionIdx++)
				{
					FMeshSectionInfo Info = StaticMesh->SectionInfoMap.Get(0, SectionIdx);
					Info.MaterialIndex = SectionIdx;
					Info.bEnableCollision = true;
					StaticMesh->SectionInfoMap.Set(0, SectionIdx, Info);
				}

				// Configure body setup for working collision.
				StaticMesh->CreateBodySetup();
				StaticMesh->BodySetup->CollisionTraceFlag = CTF_UseComplexAsSimple;

 				// Build mesh from source
 				StaticMesh->Build(false);

				// Make package dirty.
				StaticMesh->MarkPackageDirty();

 				StaticMesh->PostEditChange();
 
 				// Notify asset registry of new asset
 				FAssetRegistryModule::AssetCreated(StaticMesh);
 			}
 		}
 	}

	return FReply::Handled();
}


#undef LOCTEXT_NAMESPACE