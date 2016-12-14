// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "Engine.h"
#include "RuntimeMeshCore.h"


#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 12
/** Structure definition of a vertex */
using RuntimeMeshVertexStructure = FLocalVertexFactory::FDataType;
#else
/** Structure definition of a vertex */
using RuntimeMeshVertexStructure = FLocalVertexFactory::DataType;
#endif

#define RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, VertexType, Member, MemberType) \
	STRUCTMEMBER_VERTEXSTREAMCOMPONENT(&VertexBuffer, VertexType, Member, MemberType)


/* 
 *	Defines an interface for the proxy sections so that 
 *	the vertex factory can query for section visibility 
 */
class FRuntimeMeshVisibilityInterface
{
public:
	virtual bool ShouldRender() = 0;
};


/** Vertex Buffer for one section. Templated to support different vertex types */
template<typename VertexType>
class FRuntimeMeshVertexBuffer : public FVertexBuffer
{
public:

	FRuntimeMeshVertexBuffer(EUpdateFrequency SectionUpdateFrequency) : VertexCount(0)
	{
		UsageFlags = SectionUpdateFrequency == EUpdateFrequency::Frequent ? BUF_Dynamic : BUF_Static;
	}

	virtual void InitRHI() override
	{
		// Create the vertex buffer
		FRHIResourceCreateInfo CreateInfo;
		VertexBufferRHI = RHICreateVertexBuffer(sizeof(VertexType) * VertexCount, UsageFlags, CreateInfo);
	}

	/* Get the size of the vertex buffer */
	int32 Num() { return VertexCount; }
	
	/* Set the size of the vertex buffer */
	void SetNum(int32 NewVertexCount)
	{
		check(NewVertexCount != 0);

		// Make sure we're not already the right size
		if (NewVertexCount != VertexCount)
		{
			VertexCount = NewVertexCount;
			
			// Rebuild resource
			ReleaseResource();
			InitResource();
		}
	}

	/* Set the data for the vertex buffer */
	void SetData(const TArray<VertexType>& Data)
	{
		check(Data.Num() == VertexCount);

		// Lock the vertex buffer
 		void* Buffer = RHILockVertexBuffer(VertexBufferRHI, 0, Data.Num() * sizeof(VertexType), RLM_WriteOnly);
 		 
 		// Write the vertices to the vertex buffer
 		FMemory::Memcpy(Buffer, Data.GetData(), Data.Num() * sizeof(VertexType));

		// Unlock the vertex buffer
 		RHIUnlockVertexBuffer(VertexBufferRHI);
	}

private:

	/* The number of vertices this buffer is currently allocated to hold */
	int32 VertexCount;
	/* The buffer configuration to use */
	EBufferUsageFlags UsageFlags;
};

/** Index Buffer */
class FRuntimeMeshIndexBuffer : public FIndexBuffer
{
public:

	FRuntimeMeshIndexBuffer(EUpdateFrequency SectionUpdateFrequency) : IndexCount(0)
	{
		UsageFlags = SectionUpdateFrequency == EUpdateFrequency::Frequent ? BUF_Dynamic : BUF_Static;
	}

	virtual void InitRHI() override
	{
		// Create the index buffer
		FRHIResourceCreateInfo CreateInfo;
		IndexBufferRHI = RHICreateIndexBuffer(sizeof(int32), IndexCount * sizeof(int32), BUF_Dynamic, CreateInfo);
	}

	/* Get the size of the index buffer */
	int32 Num() { return IndexCount; }

	/* Set the size of the index buffer */
	void SetNum(int32 NewIndexCount)
	{
		check(NewIndexCount != 0);

		// Make sure we're not already the right size
		if (NewIndexCount != IndexCount)
		{
			IndexCount = NewIndexCount;

			// Rebuild resource
			ReleaseResource();
			InitResource();
		}
	}

	/* Set the data for the index buffer */
	void SetData(const TArray<int32>& Data)
	{
		check(Data.Num() == IndexCount);

		// Lock the index buffer
		void* Buffer = RHILockIndexBuffer(IndexBufferRHI, 0, IndexCount * sizeof(int32), RLM_WriteOnly);

		// Write the indices to the vertex buffer	
		FMemory::Memcpy(Buffer, Data.GetData(), Data.Num() * sizeof(int32));

		// Unlock the index buffer
		RHIUnlockIndexBuffer(IndexBufferRHI);
	}

private:

	/* The number of indices this buffer is currently allocated to hold */
	int32 IndexCount;
	/* The buffer configuration to use */
	EBufferUsageFlags UsageFlags;
};

/** Vertex Factory */
class FRuntimeMeshVertexFactory : public FLocalVertexFactory
{
public:

	FRuntimeMeshVertexFactory(FRuntimeMeshVisibilityInterface* InSectionParent) : SectionParent(InSectionParent) { }
		
	/** Init function that can be called on any thread, and will do the right thing (enqueue command if called on main thread) */
	void Init(const RuntimeMeshVertexStructure VertexStructure)
	{
		if (IsInRenderingThread())
		{
			SetData(VertexStructure);
		}
		else
		{
			// Send the command to the render thread
			ENQUEUE_UNIQUE_RENDER_COMMAND_TWOPARAMETER(
				InitRuntimeMeshVertexFactory,
				FRuntimeMeshVertexFactory*, VertexFactory, this,
				const RuntimeMeshVertexStructure, VertexStructure, VertexStructure,
				{
					VertexFactory->Init(VertexStructure);
				});
		}
	}

	/* Gets the section visibility for static sections */
	virtual uint64 GetStaticBatchElementVisibility(const class FSceneView& View, const struct FMeshBatch* Batch) const override
	{
		return SectionParent->ShouldRender();
	}

private:
	/* Interface to the parent section for checking visibility.*/
	FRuntimeMeshVisibilityInterface* SectionParent;
};
