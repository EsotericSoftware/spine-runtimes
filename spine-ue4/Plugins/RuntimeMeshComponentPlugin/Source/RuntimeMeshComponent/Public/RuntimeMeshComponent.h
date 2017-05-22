// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "Components/MeshComponent.h"
#include "RuntimeMeshCore.h"
#include "RuntimeMeshSection.h"
#include "RuntimeMeshGenericVertex.h"
#include "RuntimeMeshBuilder.h"
#include "PhysicsEngine/ConvexElem.h"
#include "RuntimeMeshComponent.generated.h"

// This set of macros is only meant for argument validation as it will return out of whatever scope.
#if WITH_EDITOR
#define RMC_CHECKINGAME_LOGINEDITOR(Condition, Message, RetVal) \
	{ if (!(Condition)) \
	{ \
		Log(TEXT(Message), true); \
		return RetVal; \
	} }
#else
#define RMC_CHECKINGAME_LOGINEDITOR(Condition, Message, RetVal) \
	check(Condition && Message);
#endif


#define RMC_VALIDATE_CREATIONPARAMETERS(SectionIndex, Vertices, Triangles, RetVal) \
		RMC_CHECKINGAME_LOGINEDITOR((SectionIndex >= 0), "SectionIndex cannot be negative.", RetVal); \
		RMC_CHECKINGAME_LOGINEDITOR((Vertices.Num() > 0), "Vertices length must not be 0.", RetVal); \
		RMC_CHECKINGAME_LOGINEDITOR((Triangles.Num() > 0), "Triangles length must not be 0", RetVal);

#define RMC_VALIDATE_CREATIONPARAMETERS_DUALBUFFER(SectionIndex, Vertices, Triangles, Positions, RetVal) \
		RMC_VALIDATE_CREATIONPARAMETERS(SectionIndex, Vertices, Triangles, RetVal) \
		RMC_CHECKINGAME_LOGINEDITOR((Positions.Num() == Vertices.Num()), "Positions must be the same length as Vertices", RetVal);

#define RMC_VALIDATE_BOUNDINGBOX(BoundingBox, RetVal) \
		RMC_CHECKINGAME_LOGINEDITOR(BoundingBox.IsValid, "BoundingBox must be valid.", RetVal);

#define RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, RetVal) \
		RMC_CHECKINGAME_LOGINEDITOR((SectionIndex >= 0), "SectionIndex cannot be negative.", RetVal); \
		RMC_CHECKINGAME_LOGINEDITOR((SectionIndex < MeshSections.Num() && MeshSections[SectionIndex].IsValid()), "Invalid SectionIndex.", RetVal);

#define RMC_VALIDATE_UPDATEPARAMETERS_INTERNALSECTION(SectionIndex, RetVal) \
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, RetVal) \
		RMC_CHECKINGAME_LOGINEDITOR((MeshSections[SectionIndex]->bIsLegacySectionType), "Section is not of legacy type.", RetVal);
		
#define RMC_VALIDATE_UPDATEPARAMETERS_DUALBUFFER(SectionIndex, RetVal) \
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, RetVal) \
		RMC_CHECKINGAME_LOGINEDITOR((MeshSections[SectionIndex]->IsDualBufferSection()), "Section is not dual buffer.", RetVal);



/* 
*	This tick function is used to drive the collision cooker. It is enabled for one frame when we need to update collision. 
*	This keeps from cooking on each individual create/update section as the original PMC did
*/
USTRUCT()
struct RUNTIMEMESHCOMPONENT_API FRuntimeMeshComponentPrePhysicsTickFunction : public FTickFunction
{
	GENERATED_USTRUCT_BODY()

	/* Target RMC to tick */
	class URuntimeMeshComponent* Target;

	virtual void ExecuteTick(float DeltaTime, ELevelTick TickType, ENamedThreads::Type CurrentThread,
		const FGraphEventRef& MyCompletionGraphEvent) override;

	virtual FString DiagnosticMessage() override;
};

template<>
#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION <= 15
struct TStructOpsTypeTraits<FRuntimeMeshComponentPrePhysicsTickFunction> : public TStructOpsTypeTraitsBase
#else
struct TStructOpsTypeTraits<FRuntimeMeshComponentPrePhysicsTickFunction> : public TStructOpsTypeTraitsBase2<FRuntimeMeshComponentPrePhysicsTickFunction>
#endif
{
	enum
	{
		WithCopy = false
	};
};

/**
*	Component that allows you to specify custom triangle mesh geometry for rendering and collision.
*/
UCLASS(HideCategories = (Object, LOD), Meta = (BlueprintSpawnableComponent))
class RUNTIMEMESHCOMPONENT_API URuntimeMeshComponent : public UMeshComponent, public IInterface_CollisionDataProvider
{
	GENERATED_BODY()

private:
	
	/* Creates an mesh section of a specified type at the specified index. */
	template<typename SectionType>
	TSharedPtr<SectionType> CreateOrResetSection(int32 SectionIndex, bool bWantsSeparatePositionBuffer, bool bInIsLegacySectionType = false)
	{
		// Ensure sections array is long enough
		if (SectionIndex >= MeshSections.Num())
		{
			MeshSections.SetNum(SectionIndex + 1, false);
		}

		// Create new section
		TSharedPtr<SectionType> NewSection = MakeShareable(new SectionType(bWantsSeparatePositionBuffer));
		NewSection->bIsLegacySectionType = bInIsLegacySectionType;

		// Store section at index
		MeshSections[SectionIndex] = NewSection;

		return NewSection;
	}
		
	/* Creates a mesh section of an internal type meant for the generic vertex and the old PMC style API */
	TSharedPtr<FRuntimeMeshSectionInterface> CreateOrResetSectionLegacyType(int32 SectionIndex, int32 NumUVChannels);

	/* Gets the material for a section or the default material if one's not provided. */
	UMaterialInterface* GetSectionMaterial(int32 Index)
	{
		auto Material = GetMaterial(Index);
		return Material ? Material : UMaterial::GetDefaultMaterial(MD_Surface);
	}


	/* Finishes creating a section, including entering it for batch updating, or updating the RT directly */
	void CreateSectionInternal(int32 SectionIndex, ESectionUpdateFlags UpdateFlags);

	/* Finishes updating a section, including entering it for batch updating, or updating the RT directly */
	void UpdateSectionInternal(int32 SectionIndex, bool bHadVertexPositionsUpdate, bool bHadVertexUpdates, bool bHadIndexUpdates, bool bNeedsBoundsUpdate, ESectionUpdateFlags UpdateFlags);

	/* Finishes updating a sections positions (Only used if section is dual vertex buffer), including entering it for batch updating, or updating the RT directly */
	void UpdateSectionVertexPositionsInternal(int32 SectionIndex, bool bNeedsBoundsUpdate);

	/* Finishes updating a sections properties, like visible/casts shadow, a*/
	void UpdateSectionPropertiesInternal(int32 SectionIndex, bool bUpdateRequiresProxyRecreateIfStatic);
	
	/* Internal log helper for the templates to be able to use the internal logger */
	static void Log(FString Text, bool bIsError = false)
	{
		if (bIsError)
		{
			UE_LOG(RuntimeMeshLog, Error, TEXT("%s"), *Text);
		}
		else
		{
			UE_LOG(RuntimeMeshLog, Warning, TEXT("%s"), *Text);
		}
	}

public:
	URuntimeMeshComponent(const FObjectInitializer& ObjectInitializer);

	/**
	*	Create/replace a section.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	Vertices			Vertex buffer all vertex data for this section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void CreateMeshSection(int32 SectionIndex, TArray<VertexType>& Vertices, TArray<int32>& Triangles, bool bCreateCollision = false, 
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		// It is only safe to call these functions from the game thread.
		check(IsInGameThread());

		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_CreateMeshSection_VertexType);

		// Validate all creation parameters
		RMC_VALIDATE_CREATIONPARAMETERS(SectionIndex, Vertices, Triangles, /*VoidReturn*/);

		// Create the section
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = CreateOrResetSection<FRuntimeMeshSection<VertexType>>(SectionIndex, false);
		
		// Set the vertex and index buffers
		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		Section->UpdateVertexBuffer(Vertices, nullptr, bShouldUseMove);
		Section->UpdateIndexBuffer(Triangles, bShouldUseMove);

		// Track collision status and update collision information if necessary
		Section->CollisionEnabled = bCreateCollision;
		Section->UpdateFrequency = UpdateFrequency;

		// Finalize section.
		CreateSectionInternal(SectionIndex, UpdateFlags);
	}

	/**
	*	Create/replace a section.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	Vertices			Vertex buffer all vertex data for this section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void CreateMeshSection(int32 SectionIndex, TArray<VertexType>& Vertices, TArray<int32>& Triangles, const FBox& BoundingBox, bool bCreateCollision = false,
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		// It is only safe to call these functions from the game thread.
		check(IsInGameThread());

		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_CreateMeshSection_VertexType_WithBoundingBox);

		// Validate all creation parameters
		RMC_VALIDATE_CREATIONPARAMETERS(SectionIndex, Vertices, Triangles, /*VoidReturn*/);
		RMC_VALIDATE_BOUNDINGBOX(BoundingBox, /*VoidReturn*/);

		// Create the section
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = CreateOrResetSection<FRuntimeMeshSection<VertexType>>(SectionIndex, false);

		// Set the vertex and index buffers
		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		Section->UpdateVertexBuffer(Vertices, &BoundingBox, bShouldUseMove);
		Section->UpdateIndexBuffer(Triangles, bShouldUseMove);

		// Track collision status and update collision information if necessary
		Section->CollisionEnabled = bCreateCollision;
		Section->UpdateFrequency = UpdateFrequency;

		// Finalize section.
		CreateSectionInternal(SectionIndex, UpdateFlags);
	}

	/**
	*	Create/replace a section using 2 vertex buffers. One contains positions only, the other contains all other data. This allows for very efficient updates of the positions of a mesh.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	VertexData			Vertex buffer containing everything except position for each vertex.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void CreateMeshSectionDualBuffer(int32 SectionIndex, TArray<FVector>& VertexPositions, TArray<VertexType>& VertexData, TArray<int32>& Triangles, bool bCreateCollision = false,
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_CreateMeshSectionDualBuffer_VertexType);

		// Validate all creation parameters
		RMC_VALIDATE_CREATIONPARAMETERS_DUALBUFFER(SectionIndex, VertexData, Triangles, VertexPositions, /*VoidReturn*/);

		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = CreateOrResetSection<FRuntimeMeshSection<VertexType>>(SectionIndex, true);

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		Section->UpdateVertexPositionBuffer(VertexPositions, nullptr, bShouldUseMove);
		Section->UpdateVertexBuffer(VertexData, nullptr, bShouldUseMove);
		Section->UpdateIndexBuffer(Triangles, bShouldUseMove);

		// Track collision status and update collision information if necessary
		Section->CollisionEnabled = bCreateCollision;
		Section->UpdateFrequency = UpdateFrequency;

		// Finalize section.
		CreateSectionInternal(SectionIndex, UpdateFlags);
	}

	/**
	*	Create/replace a section using 2 vertex buffers. One contains positions only, the other contains all other data. This allows for very efficient updates of the positions of a mesh.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	VertexData			Vertex buffer containing everything except position for each vertex.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void CreateMeshSectionDualBuffer(int32 SectionIndex, TArray<FVector>& VertexPositions, TArray<VertexType>& VertexData, TArray<int32>& Triangles, const FBox& BoundingBox,
		bool bCreateCollision = false, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_CreateMeshSectionDualBuffer_VertexType_WithBoundingBox);

		// Validate all creation parameters
		RMC_VALIDATE_CREATIONPARAMETERS_DUALBUFFER(SectionIndex, VertexData, Triangles, VertexPositions, /*VoidReturn*/);
		RMC_VALIDATE_BOUNDINGBOX(BoundingBox, /*VoidReturn*/);

		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = CreateOrResetSection<FRuntimeMeshSection<VertexType>>(SectionIndex, true);

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		Section->UpdateVertexPositionBuffer(VertexPositions, &BoundingBox, bShouldUseMove);
		Section->UpdateVertexBuffer(VertexData, nullptr, bShouldUseMove);
		Section->UpdateIndexBuffer(Triangles, bShouldUseMove);

		// Track collision status and update collision information if necessary
		Section->CollisionEnabled = bCreateCollision;
		Section->UpdateFrequency = UpdateFrequency;

		// Finalize section.
		CreateSectionInternal(SectionIndex, UpdateFlags);
	}
	

	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<VertexType>& Vertices, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_VertexType);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);
		
		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);
		
		// Check dual buffer section status
		if (Section->IsDualBufferSection() && Vertices.Num() != Section->VertexBuffer.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (Vertices.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexBuffer(Vertices, nullptr, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}
		
		// Finalize section update if we have anything to apply
		if (bUpdatedVertices)
		{
			UpdateSectionInternal(SectionIndex, false, bUpdatedVertices, false, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<VertexType>& Vertices, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_VertexType_WithBoundingBox);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);
		RMC_VALIDATE_BOUNDINGBOX(BoundingBox, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() && Vertices.Num() != Section->VertexBuffer.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (Vertices.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexBuffer(Vertices, &BoundingBox, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertices)
		{
			UpdateSectionInternal(SectionIndex, false, bUpdatedVertices, false, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<VertexType>& Vertices, TArray<int32>& Triangles, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_VertexType_WithTriangles);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() && Vertices.Num() != Section->VertexBuffer.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (Vertices.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexBuffer(Vertices, nullptr, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Update triangles if supplied
		bool bUpdatedIndices = false;
		if (Triangles.Num() > 0)
		{
			Section->UpdateIndexBuffer(Triangles, bShouldUseMove);
			bUpdatedIndices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Triangles empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertices || bUpdatedIndices)
		{
			UpdateSectionInternal(SectionIndex, false, bUpdatedVertices, bUpdatedIndices, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<VertexType>& Vertices, TArray<int32>& Triangles, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_VertexType_WithTrianglesAndBoundinBox);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);
		RMC_VALIDATE_BOUNDINGBOX(BoundingBox, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() && Vertices.Num() != Section->VertexBuffer.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (Vertices.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexBuffer(Vertices, &BoundingBox, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Update indices if supplied
		bool bUpdatedIndices = false;
		if (Triangles.Num() > 0)
		{
			Section->UpdateIndexBuffer(Triangles, bShouldUseMove);
			bUpdatedIndices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Triangles empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertices || bUpdatedIndices)
		{
			UpdateSectionInternal(SectionIndex, false, bUpdatedVertices, bUpdatedIndices, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	
	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.	
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	VertexData			Vertex buffer containing everything except position for each vertex.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<FVector>& VertexPositions, TArray<VertexType>& VertexData, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_Dual_VertexType);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS_DUALBUFFER(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() && 
			VertexData.Num() != Section->VertexBuffer.Num() &&
			VertexPositions.Num() != VertexData.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertex positions if supplied
		bool bUpdatedVertexPositions = false;
		if (VertexPositions.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexPositionBuffer(VertexPositions, nullptr, bShouldUseMove);
			bUpdatedVertexPositions = true;
		}
		else
		{
			Log(TEXT("UpdatemeshSection() - Vertex positions empty. They will not be updated."));
		}

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (VertexData.Num() > 0)
		{
			Section->UpdateVertexBuffer(VertexData, nullptr, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertexPositions || bUpdatedVertices)
		{
			UpdateSectionInternal(SectionIndex, bUpdatedVertexPositions, bUpdatedVertices, false, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	VertexData			Vertex buffer containing everything except position for each vertex.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<FVector>& VertexPositions, TArray<VertexType>& VertexData, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_Dual_VertexType_WithBoundingBox);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS_DUALBUFFER(SectionIndex, /*VoidReturn*/);
		RMC_VALIDATE_BOUNDINGBOX(BoundingBox, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() &&
			VertexData.Num() != Section->VertexBuffer.Num() &&
			VertexPositions.Num() != VertexData.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertex positions if supplied
		bool bUpdatedVertexPositions = false;
		if (VertexPositions.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexPositionBuffer(VertexPositions, &BoundingBox, bShouldUseMove);
			bUpdatedVertexPositions = true;
		}
		else
		{
			Log(TEXT("UpdatemeshSection() - Vertex positions empty. They will not be updated."));
		}

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (VertexData.Num() > 0)
		{
			Section->UpdateVertexBuffer(VertexData, nullptr, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertexPositions || bUpdatedVertices)
		{
			UpdateSectionInternal(SectionIndex, bUpdatedVertexPositions, bUpdatedVertices, false, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	VertexData			Vertex buffer containing everything except position for each vertex.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<FVector>& VertexPositions, TArray<VertexType>& VertexData, TArray<int32>& Triangles, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_Dual_VertexType_WithTriangles);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS_DUALBUFFER(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() &&
			VertexData.Num() != Section->VertexBuffer.Num() &&
			VertexPositions.Num() != VertexData.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertex positions if supplied
		bool bUpdatedVertexPositions = false;
		if (VertexPositions.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexPositionBuffer(VertexPositions, nullptr, bShouldUseMove);
			bUpdatedVertexPositions = true;
		}
		else
		{
			Log(TEXT("UpdatemeshSection() - Vertex positions empty. They will not be updated."));
		}

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (VertexData.Num() > 0)
		{
			Section->UpdateVertexBuffer(VertexData, nullptr, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Update triangles if supplied
		bool bUpdatedIndices = false;
		if (Triangles.Num() > 0)
		{
			Section->UpdateIndexBuffer(Triangles, bShouldUseMove);
			bUpdatedIndices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Triangles empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertexPositions || bUpdatedVertices || bUpdatedIndices)
		{
			UpdateSectionInternal(SectionIndex, bUpdatedVertexPositions, bUpdatedVertices, bUpdatedIndices, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	VertexData			Vertex buffer containing everything except position for each vertex.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	void UpdateMeshSection(int32 SectionIndex, TArray<FVector>& VertexPositions, TArray<VertexType>& VertexData, TArray<int32>& Triangles, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		SCOPE_CYCLE_COUNTER(STAT_RuntimeMesh_UpdateMeshSection_Dual_VertexType_WithTrianglesAndBoundinBox);

		// Validate all update parameters
		RMC_VALIDATE_UPDATEPARAMETERS_DUALBUFFER(SectionIndex, /*VoidReturn*/);
		RMC_VALIDATE_BOUNDINGBOX(BoundingBox, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		// Check dual buffer section status
		if (Section->IsDualBufferSection() &&
			VertexData.Num() != Section->VertexBuffer.Num() &&
			VertexPositions.Num() != VertexData.Num())
		{
			Log(TEXT("UpdateMeshSection() - Vertices cannot change length unless the positions are updated as well."), true);
			return;
		}

		bool bShouldUseMove = (UpdateFlags & ESectionUpdateFlags::MoveArrays) != ESectionUpdateFlags::None;
		bool bNeedsBoundsUpdate = false;

		// Update vertex positions if supplied
		bool bUpdatedVertexPositions = false;
		if (VertexPositions.Num() > 0)
		{
			bNeedsBoundsUpdate = Section->UpdateVertexPositionBuffer(VertexPositions, &BoundingBox, bShouldUseMove);
			bUpdatedVertexPositions = true;
		}
		else
		{
			Log(TEXT("UpdatemeshSection() - Vertex positions empty. They will not be updated."));
		}

		// Update vertices if supplied
		bool bUpdatedVertices = false;
		if (VertexData.Num() > 0)
		{
			Section->UpdateVertexBuffer(VertexData, nullptr, bShouldUseMove);
			bUpdatedVertices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Vertices empty. They will not be updated."));
		}

		// Update indices if supplied
		bool bUpdatedIndices = false;
		if (Triangles.Num() > 0)
		{
			Section->UpdateIndexBuffer(Triangles, bShouldUseMove);
			bUpdatedIndices = true;
		}
		else
		{
			Log(TEXT("UpdateMeshSection() - Triangles empty. They will not be updated."));
		}

		// Finalize section update if we have anything to apply
		if (bUpdatedVertexPositions || bUpdatedVertices || bUpdatedIndices)
		{
			UpdateSectionInternal(SectionIndex, bUpdatedVertexPositions, bUpdatedVertices, bUpdatedIndices, bNeedsBoundsUpdate, UpdateFlags);
		}
	}

	
	/**
	*	Updates a sections position buffer only. This cannot be used on a non-dual buffer section. You cannot change the length of the vertex position buffer with this function.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	void UpdateMeshSectionPositionsImmediate(int32 SectionIndex, TArray<FVector>& VertexPositions, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	/**
	*	Updates a sections position buffer only. This cannot be used on a non-dual buffer section. You cannot change the length of the vertex position buffer with this function.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	void UpdateMeshSectionPositionsImmediate(int32 SectionIndex, TArray<FVector>& VertexPositions, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);
	

	/**
	*	Starts an in place update of vertex positions. 
	*	@param	SectionIndex		Index of the section to update.
	*/
	TArray<FVector>* BeginMeshSectionPositionUpdate(int32 SectionIndex);

	/**
	*	Finishes an in place update of vertex positions.
	*	This will push the update to the GPU and calculate the new Bounding Box
	*	@param	SectionIndex		Index of the section to update.
	*/
	void EndMeshSectionPositionUpdate(int32 SectionIndex);

	/**
	*	Finishes an in place update of vertex positions.
	*	This will push the update to the GPU
	*	@param	SectionIndex		Index of the section to update.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*/
	void EndMeshSectionPositionUpdate(int32 SectionIndex, const FBox& BoundingBox);


	template<typename VertexType>
	void BeginMeshSectionUpdate(int32 SectionIndex, TArray<VertexType>*& Vertices)
	{
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		Vertices = &Section->VertexBuffer;
	}

	template<typename VertexType>
	void BeginMeshSectionUpdate(int32 SectionIndex, TArray<VertexType>*& Vertices, TArray<int32>*& Triangles)
	{
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		Vertices = &Section->VertexBuffer;
		Triangles = &Section->IndexBuffer;
	}

	template<typename VertexType>
	void BeginMeshSectionUpdate(int32 SectionIndex, TArray<FVector>*& Positions, TArray<VertexType>*& Vertices)
	{
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		Positions = &Section->PositionVertexBuffer;
		Vertices = &Section->VertexBuffer;
	}

	template<typename VertexType>
	void BeginMeshSectionUpdate(int32 SectionIndex, TArray<FVector>*& Positions, TArray<VertexType>*& Vertices, TArray<int32>*& Triangles)
	{
		RMC_VALIDATE_UPDATEPARAMETERS(SectionIndex, /*VoidReturn*/);

		// Validate section type
		MeshSections[SectionIndex]->GetVertexType()->EnsureEquals<VertexType>();

		// Cast section to correct type
		TSharedPtr<FRuntimeMeshSection<VertexType>> Section = StaticCastSharedPtr<FRuntimeMeshSection<VertexType>>(MeshSections[SectionIndex]);

		Positions = &Section->PositionVertexBuffer;
		Vertices = &Section->VertexBuffer;
		Triangles = &Section->IndexBuffer;
	}


	void BeginMeshSectionUpdate(int32 SectionIndex, IRuntimeMeshVerticesBuilder*& Vertices, FRuntimeMeshIndicesBuilder*& Indices);

	void EndMeshSectionUpdate(int32 SectionIndex, ERuntimeMeshBuffer UpdatedBuffers, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	void EndMeshSectionUpdate(int32 SectionIndex, ERuntimeMeshBuffer UpdatedBuffers, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);
	

	/*
	*	Gets a readonly pointer to the sections mesh data.
	*	To be able to edit the section data use BegineMeshSectionUpdate()
	*/
	void GetSectionMesh(int32 SectionIndex, const IRuntimeMeshVerticesBuilder*& Vertices, const FRuntimeMeshIndicesBuilder*& Indices);





	/**
	*	Create/replace a section.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*/
	void CreateMeshSection(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector>& Normals,
		const TArray<FVector2D>& UV0, const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, bool bCreateCollision = false,
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	/**
	*	Create/replace a section.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*/
	void CreateMeshSection(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector>& Normals,
		const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents,
		bool bCreateCollision = false, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);


	/**
	*	Updates a section. This is faster than CreateMeshSection. 
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*/
	void UpdateMeshSection(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<FVector>& Normals, const TArray<FVector2D>& UV0, 
		const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	/**
	*	Updates a section. This is faster than CreateMeshSection.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*/
	void UpdateMeshSection(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<FVector>& Normals, const TArray<FVector2D>& UV0, 
		const TArray<FVector2D>& UV1, const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	/**
	*	Updates a section. This is faster than CreateMeshSection.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*/
	void UpdateMeshSection(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector>& Normals, 
		const TArray<FVector2D>& UV0, const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	/**
	*	Updates a section. This is faster than CreateMeshSection.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*/
	void UpdateMeshSection(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector>& Normals,
		const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);

	

	/**
	*	Create/replace a section.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	bCalculateNormalTangent	Indicates whether normal/tangent information should be calculated automatically. This can add significant cost.
	*	@param	bGenerateTessellationTriangles	Indicates whether tessellation supporting triangles should be calculated. This can add significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh", meta = (DisplayName = "Create Mesh Section", AutoCreateRefTerm = "Normals,Tangents,UV0,UV1,Colors"))
	void CreateMeshSection_Blueprint(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector>& Normals, 
		const TArray<FRuntimeMeshTangent>& Tangents, const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FLinearColor>& Colors, 
		bool bCreateCollision, bool bCalculateNormalTangent, bool bGenerateTessellationTriangles, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average);

	/**
	*	Updates a section. This is faster than CreateMeshSection. If you change the vertices count, you must update the other components.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV0					Optional array of texture co-ordinates for each vertex (UV Channel 0). If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors		Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	bCalculateNormalTangent	Indicates whether normal/tangent information should be calculated automatically. This can add significant cost.
	*	@param	bGenerateTessellationTriangles	Indicates whether tessellation supporting triangles should be calculated. This can add significant cost.
	*/
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh", meta = (DisplayName = "Update Mesh Section", AutoCreateRefTerm = "Triangles,Normals,Tangents,UV0,UV1,Colors"))
	void UpdateMeshSection_Blueprint(int32 SectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector>& Normals, 
		const TArray<FRuntimeMeshTangent>& Tangents, const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FLinearColor>& Colors, bool bCalculateNormalTangent, bool bGenerateTessellationTriangles);
	


	/**
	*	Create/replace a section.
	*	@param	SectionIndex		Index of the section to create or replace.
	*	@param	Vertices			Vertex buffer all vertex data for this section.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	bCreateCollision	Indicates whether collision should be created for this section. This adds significant cost.
	*	@param	UpdateFrequency		Indicates how frequently the section will be updated. Allows the RMC to optimize itself to a particular use.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	void CreateMeshSection(int32 SectionIndex, IRuntimeMeshVerticesBuilder& Vertices, FRuntimeMeshIndicesBuilder& Indices, bool bCreateCollision = false,
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);


	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	void UpdateMeshSection(int32 SectionIndex, IRuntimeMeshVerticesBuilder& Vertices, FRuntimeMeshIndicesBuilder& Indices, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None);




	/** Clear a section of the procedural mesh. */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void ClearMeshSection(int32 SectionIndex);

	/** Clear all mesh sections and reset to empty state */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void ClearAllMeshSections();


	/** Sets the tessellation triangles needed to correctly support tessellation on a section. */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void SetSectionTessellationTriangles(int32 SectionIndex, const TArray<int32>& TessellationTriangles, bool bShouldMoveArray = false);






	/** Gets the bounding box of a specific section */
	bool GetSectionBoundingBox(int32 SectionIndex, FBox& OutBoundingBox);


	/** Control visibility of a particular section */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void SetMeshSectionVisible(int32 SectionIndex, bool bNewVisibility);

	/** Returns whether a particular section is currently visible */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	bool IsMeshSectionVisible(int32 SectionIndex) const;


	/** Control whether a particular section casts a shadow */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void SetMeshSectionCastsShadow(int32 SectionIndex, bool bNewCastsShadow);

	/** Returns whether a particular section is currently casting shadows */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	bool IsMeshSectionCastingShadows(int32 SectionIndex) const;


	/** Control whether a particular section has collision */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void SetMeshSectionCollisionEnabled(int32 SectionIndex, bool bNewCollisionEnabled);

	/** Returns whether a particular section has collision */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	bool IsMeshSectionCollisionEnabled(int32 SectionIndex);


	/** Returns number of sections currently created for this component */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	int32 GetNumSections() const;

	/** Returns whether a particular section currently exists */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	bool DoesSectionExist(int32 SectionIndex) const;

	/** Returns first available section index */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	int32 FirstAvailableMeshSectionIndex() const;
	
	/** Returns the last in use section index */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	int32 GetLastSectionIndex() const;


	/** Sets the geometry for a collision only section */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void SetMeshCollisionSection(int32 CollisionSectionIndex, const TArray<FVector>& Vertices, const TArray<int32>& Triangles);

	/** Clears the geometry for a collision only section */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void ClearMeshCollisionSection(int32 CollisionSectionIndex);

	/** Clears the geometry for ALL collision only sections */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void ClearAllMeshCollisionSections();



	/** Add simple collision convex to this component */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void AddCollisionConvexMesh(TArray<FVector> ConvexVerts);

	/** Add simple collision convex to this component */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void ClearCollisionConvexMeshes();

	/** Function to replace _all_ simple collision in one go */
	void SetCollisionConvexMeshes(const TArray< TArray<FVector> >& ConvexMeshes);


	/** Begins a batch of updates, delays updates until you call EndBatchUpdates() */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void BeginBatchUpdates()
	{
		BatchState.StartBatch();
	}

	/** Ends a batch of updates started with BeginBatchUpdates() */
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void EndBatchUpdates();

	/** Runs any pending collision cook (Not required to call this. This is only if you need to make sure all changes are cooked before doing something)*/
	UFUNCTION(BlueprintCallable, Category = "Components|RuntimeMesh")
	void CookCollisionNow();



	/**
	*	Controls whether the complex (Per poly) geometry should be treated as 'simple' collision.
	*	Should be set to false if this component is going to be given simple collision and simulated.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "RuntimeMesh")
	bool bUseComplexAsSimpleCollision;

	/**
	*	Controls whether the mesh data should be serialized with the component.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "RuntimeMesh")
	bool bShouldSerializeMeshData;
	
	/* 
	*	The current mode of the collision cooker 
	*	WARNING: This feature will only work in engine version 4.14 or above!
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "RuntimeMesh")
	ERuntimeMeshCollisionCookingMode CollisionMode;
	
	/** Collision data */
	UPROPERTY(Instanced)
	class UBodySetup* BodySetup;

	/* Serialize the entire RMC to the supplied archive. */
	void SerializeRMC(FArchive& Ar);

	/* Serialize the designated section into the supplied archive. */
	void SerializeRMCSection(FArchive& Ar, int32 SectionIndex);

private:


	//~ Begin Interface_CollisionDataProvider Interface
	virtual bool GetPhysicsTriMeshData(struct FTriMeshCollisionData* CollisionData, bool InUseAllTriData) override;
	virtual bool ContainsPhysicsTriMeshData(bool InUseAllTriData) const override;
	virtual bool WantsNegXTriMesh() override { return false; }
	//~ End Interface_CollisionDataProvider Interface


	//~ Begin USceneComponent Interface.
	virtual FBoxSphereBounds CalcBounds(const FTransform& LocalToWorld) const override;
	virtual bool IsSupportedForNetworking() const override
	{
		return true;
	}
	//~ Begin USceneComponent Interface.

	//~ Begin UPrimitiveComponent Interface.
	virtual FPrimitiveSceneProxy* CreateSceneProxy() override;
	virtual class UBodySetup* GetBodySetup() override;
	//~ End UPrimitiveComponent Interface.

	//~ Begin UMeshComponent Interface.
	virtual int32 GetNumMaterials() const override;
	//~ End UMeshComponent Interface.



	/** Update LocalBounds member from the local box of each section */
	void UpdateLocalBounds(bool bMarkRenderTransform = true);
	/** Ensure ProcMeshBodySetup is allocated and configured */
	void EnsureBodySetupCreated();
	/** Mark collision data as dirty, and re-create on instance if necessary */
	void UpdateCollision();

	/* Marks the collision for an end of frame update */
	void MarkCollisionDirty();

	/* Cooks the new collision mesh updating the body */
	void BakeCollision();

	void UpdateNavigation();


	/* Serializes this component */
	virtual void Serialize(FArchive& Ar) override;
	void SerializeInternal(FArchive& Ar, bool bForceSaveAll = false);
	void SerializeLegacy(FArchive& Ar);

	/* Does post load fixups */
	virtual void PostLoad() override;

	/* Registers the pre-physics tick function used to cook new meshes when necessary */
	virtual void RegisterComponentTickFunctions(bool bRegister) override;


	/* Current state of a batch update. */
	FRuntimeMeshBatchUpdateState BatchState;

	/* Is the collision in need of a recook? */
	bool bCollisionDirty;

	/** Array of sections of mesh */	
	TArray<RuntimeMeshSectionPtr> MeshSections;

	/* Array of collision only mesh sections*/
	UPROPERTY(Transient)
	TArray<FRuntimeMeshCollisionSection> MeshCollisionSections;

	/** Convex shapes used for simple collision */
	UPROPERTY(Transient)
	TArray<FRuntimeConvexCollisionSection> ConvexCollisionSections;

	/** Local space bounds of mesh */
	UPROPERTY(Transient)
	FBoxSphereBounds LocalBounds;

	/* Tick function used to cook collision when needed*/
	UPROPERTY(Transient)
	FRuntimeMeshComponentPrePhysicsTickFunction PrePhysicsTick;


	friend class FRuntimeMeshSceneProxy;
	friend struct FRuntimeMeshComponentPrePhysicsTickFunction;
};
