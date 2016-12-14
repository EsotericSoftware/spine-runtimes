// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "Engine.h"
#include "Components/MeshComponent.h"
#include "RuntimeMeshProfiling.h"
#include "RuntimeMeshVersion.h"




/* Base class for all render thread command information */
class FRuntimeMeshRenderThreadCommandInterface
{
public:

	FRuntimeMeshRenderThreadCommandInterface() { }
	virtual ~FRuntimeMeshRenderThreadCommandInterface() { }
	
	virtual void SetTargetSection(int32 InTargetSection) { TargetSection = InTargetSection; }
	virtual int32 GetTargetSection() { return TargetSection; }
	
	/* Cast the update data to the specific type of update data */
	template <typename Type>
	Type* As()
	{
		return static_cast<Type*>(this);
	}

private:
	/* Section index that this creation data applies to */
	int32 TargetSection;
};

/* Base class for section creation data. Allows the non templated SceneProxy to get the section proxy;*/
class FRuntimeMeshSectionCreateDataInterface : public FRuntimeMeshRenderThreadCommandInterface
{
public:
	/* The new proxy to be used for section creation */
	class FRuntimeMeshSectionProxyInterface* NewProxy;


	FRuntimeMeshSectionCreateDataInterface() { }
	virtual ~FRuntimeMeshSectionCreateDataInterface() override { }

};

/** Templated class for update data sent to the RT for updating a single mesh section */
template<typename VertexType>
class FRuntimeMeshSectionCreateData : public FRuntimeMeshSectionCreateDataInterface
{
public:
	/* Updated position vertex buffer for the section */
	TArray<FVector> PositionVertexBuffer;

	/* Updated vertex buffer for the section */
	TArray<VertexType> VertexBuffer;

	/* Whether the supplied index buffer contains adjacency info */
	bool bIsAdjacencyIndexBuffer;

	/* Updated index buffer for the section */
	TArray<int32> IndexBuffer;


	FRuntimeMeshSectionCreateData() {}
	virtual ~FRuntimeMeshSectionCreateData() override { }

};

/** Templated class for update data sent to the RT for updating a single mesh section */
template<typename VertexType>
class FRuntimeMeshSectionUpdateData : public FRuntimeMeshRenderThreadCommandInterface
{
public:
	/* Updated position vertex buffer for the section */
	TArray<FVector> PositionVertexBuffer;

	/* Updated vertex buffer for the section */
	TArray<VertexType> VertexBuffer;

	/* Updated index buffer for the section */
	TArray<int32> IndexBuffer;

	/* Should we apply the position buffer */
	bool bIncludePositionBuffer;

	/* Should we apply the vertex buffer */
	bool bIncludeVertexBuffer;

	/* Should we apply the indices as an update */
	bool bIncludeIndices;

	/* Whether the supplied index buffer contains adjacency info */
	bool bIsAdjacencyIndexBuffer;

	FRuntimeMeshSectionUpdateData() {}
	virtual ~FRuntimeMeshSectionUpdateData() override { }
};

/** Templated class for update data sent to the RT for updating a single mesh section */
template<typename VertexType>
class FRuntimeMeshSectionPositionOnlyUpdateData : public FRuntimeMeshRenderThreadCommandInterface
{
public:
	/* Updated position vertex buffer for the section */
	TArray<FVector> PositionVertexBuffer;

	FRuntimeMeshSectionPositionOnlyUpdateData() {}
	virtual ~FRuntimeMeshSectionPositionOnlyUpdateData() override { }
};

/** Property update for a single section */
class FRuntimeMeshSectionPropertyUpdateData : public FRuntimeMeshRenderThreadCommandInterface
{
public:
	/* Is this section visible */
	bool bIsVisible;

	/* Is this section casting shadows */
	bool bCastsShadow;

	FRuntimeMeshSectionPropertyUpdateData() {}
	virtual ~FRuntimeMeshSectionPropertyUpdateData() override { }
};

enum class ERuntimeMeshSectionBatchUpdateType
{
	None = 0x0,
	Create = 0x1,
	Destroy = 0x2,
	PositionsUpdate = 0x4,
	VerticesUpdate = 0x8,
	IndicesUpdate = 0x10,
	PropertyUpdate = 0x20,
};

ENUM_CLASS_FLAGS(ERuntimeMeshSectionBatchUpdateType)


/* Struct carrying all update data for a batch update sent to the render thread */
struct FRuntimeMeshBatchUpdateData
{
	TArray<FRuntimeMeshSectionCreateDataInterface*> CreateSections;
	TArray<int32> DestroySections;
	TArray<FRuntimeMeshRenderThreadCommandInterface*> UpdateSections;
	TArray<FRuntimeMeshSectionPropertyUpdateData*> PropertyUpdateSections;
};



struct FRuntimeMeshBatchUpdateState
{
	void StartBatch() { bIsPending = true; }

	void ResetBatch() 
	{
		bIsPending = false;
		bRequiresSceneProxyReCreate = false;
		bRequiresBoundsUpdate = false;
		bRequiresCollisionUpdate = false;

		SectionUpdates.Empty();
	}

	


	bool IsBatchPending() { return bIsPending; }

	void MarkSectionCreated(int32 SectionIndex, bool bPromoteToProxyRecreate)
	{
		// Flag recreate instead of individual section
		if (bPromoteToProxyRecreate)
		{
			bRequiresSceneProxyReCreate = true;
			return;
		}

		EnsureUpdateLength(SectionIndex);

		// Clear destroyed flag and set created
		SectionUpdates[SectionIndex] &= ~ERuntimeMeshSectionBatchUpdateType::Destroy;
		SectionUpdates[SectionIndex] |= ERuntimeMeshSectionBatchUpdateType::Create;
	}

	void MarkUpdateForSection(int32 SectionIndex, ERuntimeMeshSectionBatchUpdateType UpdateType)
	{
		EnsureUpdateLength(SectionIndex);
		
		// Add update type
		SectionUpdates[SectionIndex] |= UpdateType;
	}

	void MarkSectionDestroyed(int32 SectionIndex, bool bPromoteToProxyRecreate)
	{
		// Flag recreate instead of individual section
		if (bPromoteToProxyRecreate)
		{
			bRequiresSceneProxyReCreate = true;
			return;
		}

		EnsureUpdateLength(SectionIndex);

		// Clear destroyed flag and set created
		SectionUpdates[SectionIndex] &= ~ERuntimeMeshSectionBatchUpdateType::Create;
		SectionUpdates[SectionIndex] |= ERuntimeMeshSectionBatchUpdateType::Destroy;
	}

	void MarkRenderStateDirty() { bRequiresSceneProxyReCreate = true; }

	void MarkCollisionDirty() { bRequiresCollisionUpdate = true; }

	void MarkBoundsDirty() { bRequiresBoundsUpdate = true; }



	bool HasAnyFlagSet(int32 SectionIndex) { return SectionUpdates[SectionIndex] != ERuntimeMeshSectionBatchUpdateType::None; }

	bool HasFlagSet(int32 SectionIndex, ERuntimeMeshSectionBatchUpdateType UpdateType)
	{
		return (SectionUpdates[SectionIndex] & UpdateType) == UpdateType;
	}

	bool RequiresSceneProxyRecreate() { return bRequiresSceneProxyReCreate; }

	bool RequiresBoundsUpdate() { return bRequiresBoundsUpdate; }

	bool RequiresCollisionUpdate() { return bRequiresCollisionUpdate; }

	int32 GetMaxSection() { return SectionUpdates.Num() - 1; }

private:

	void EnsureUpdateLength(int32 SectionIndex)
	{
		if (SectionIndex < SectionUpdates.Num())
		{
			return;
		}

		SectionUpdates.AddZeroed((SectionIndex + 1) - SectionUpdates.Num());
	}


	bool bIsPending;
	bool bRequiresSceneProxyReCreate;
	bool bRequiresBoundsUpdate;
	bool bRequiresCollisionUpdate;
	TArray<ERuntimeMeshSectionBatchUpdateType> SectionUpdates;
	



};
