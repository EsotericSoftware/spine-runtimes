// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "Engine.h"
#include "Components/MeshComponent.h"
#include "RuntimeMeshProfiling.h"
#include "RuntimeMeshVersion.h"
#include "RuntimeMeshCore.h"
#include "RuntimeMeshRendering.h"
#include "RuntimeMeshUpdateCommands.h"


/** Interface class for the RT proxy of a single mesh section */
class FRuntimeMeshSectionProxyInterface : public FRuntimeMeshVisibilityInterface
{
public:

	FRuntimeMeshSectionProxyInterface() {}
	virtual ~FRuntimeMeshSectionProxyInterface() {}

	virtual bool ShouldRender() = 0;
	virtual bool WantsToRenderInStaticPath() const = 0;

	virtual bool ShouldUseAdjacencyIndexBuffer() const = 0;

	virtual FMaterialRelevance GetMaterialRelevance() const = 0;

	virtual void CreateMeshBatch(FMeshBatch& MeshBatch, FMaterialRenderProxy* WireframeMaterial, bool bIsSelected) = 0;


	virtual void FinishCreate_RenderThread(FRuntimeMeshSectionCreateDataInterface* UpdateData) = 0;
	virtual void FinishUpdate_RenderThread(FRuntimeMeshRenderThreadCommandInterface* UpdateData) = 0;
	virtual void FinishPositionUpdate_RenderThread(FRuntimeMeshRenderThreadCommandInterface* UpdateData) = 0;
	virtual void FinishPropertyUpdate_RenderThread(FRuntimeMeshRenderThreadCommandInterface* UpdateData) = 0;

};

/** Templated class for the RT proxy of a single mesh section */
template <typename VertexType, bool NeedsPositionOnlyBuffer>
class FRuntimeMeshSectionProxy : public FRuntimeMeshSectionProxyInterface
{
protected:
	/** Whether this section is currently visible */
	bool bIsVisible;

	/** Should this section cast a shadow */
	bool bCastsShadow;

	/** Whether this section should be using an adjacency index buffer */
	bool bShouldUseAdjacency;

	/** Whether this section is using a tessellation adjacency index buffer */
	bool bIsUsingAdjacency;

	/** Update frequency of this section */
	const EUpdateFrequency UpdateFrequency;

	/** Material applied to this section */
	UMaterialInterface* Material;

	FMaterialRelevance MaterialRelevance;

	FRuntimeMeshVertexBuffer<FVector>* PositionVertexBuffer;

	/** Vertex buffer for this section */
	FRuntimeMeshVertexBuffer<VertexType> VertexBuffer;

	/** Index buffer for this section */
	FRuntimeMeshIndexBuffer IndexBuffer;

	/** Vertex factory for this section */
	FRuntimeMeshVertexFactory VertexFactory;

public:
	FRuntimeMeshSectionProxy(FSceneInterface* InScene, EUpdateFrequency InUpdateFrequency, bool bInIsVisible, bool bInCastsShadow, UMaterialInterface* InMaterial, FMaterialRelevance InMaterialRelevance) :
		bIsVisible(bInIsVisible), bCastsShadow(bInCastsShadow), UpdateFrequency(InUpdateFrequency), Material(InMaterial), MaterialRelevance(InMaterialRelevance),
		PositionVertexBuffer(nullptr), VertexBuffer(InUpdateFrequency), IndexBuffer(InUpdateFrequency), VertexFactory(this) 
	{ 
		bShouldUseAdjacency = RequiresAdjacencyInformation(InMaterial, VertexFactory.GetType(), InScene->GetFeatureLevel());
	}

	virtual ~FRuntimeMeshSectionProxy() override
	{
		VertexBuffer.ReleaseResource();
		IndexBuffer.ReleaseResource();
		VertexFactory.ReleaseResource();

		if (PositionVertexBuffer)
		{
			PositionVertexBuffer->ReleaseResource();
			delete PositionVertexBuffer;
		}
	}


	virtual bool ShouldRender() override { return bIsVisible && VertexBuffer.Num() > 0 && IndexBuffer.Num() > 0; }

	virtual bool WantsToRenderInStaticPath() const override { return UpdateFrequency == EUpdateFrequency::Infrequent; }
	
	virtual bool ShouldUseAdjacencyIndexBuffer() const override { return bShouldUseAdjacency; }

	virtual FMaterialRelevance GetMaterialRelevance() const { return MaterialRelevance; }
	
	virtual void CreateMeshBatch(FMeshBatch& MeshBatch, FMaterialRenderProxy* WireframeMaterial, bool bIsSelected) override
	{
		MeshBatch.VertexFactory = &VertexFactory;
		MeshBatch.bWireframe = WireframeMaterial != nullptr;
		MeshBatch.MaterialRenderProxy = MeshBatch.bWireframe ? WireframeMaterial : Material->GetRenderProxy(bIsSelected);
		
		if (bIsUsingAdjacency && WireframeMaterial == nullptr)
		{
			MeshBatch.Type = PT_12_ControlPointPatchList;
		}
		else
		{
			MeshBatch.Type = PT_TriangleList;
		}

		MeshBatch.DepthPriorityGroup = SDPG_World;
		MeshBatch.CastShadow = bCastsShadow;

		FMeshBatchElement& BatchElement = MeshBatch.Elements[0];
		BatchElement.IndexBuffer = &IndexBuffer;
		BatchElement.FirstIndex = 0;
		BatchElement.NumPrimitives = bIsUsingAdjacency? IndexBuffer.Num() / 12 : IndexBuffer.Num() / 3;
		BatchElement.MinVertexIndex = 0;
		BatchElement.MaxVertexIndex = VertexBuffer.Num() - 1;
	}


	virtual void FinishCreate_RenderThread(FRuntimeMeshSectionCreateDataInterface* UpdateData) override
	{
 		check(IsInRenderingThread());

		auto* SectionUpdateData = UpdateData->As<FRuntimeMeshSectionCreateData<VertexType>>();
		check(SectionUpdateData);
		
		if (NeedsPositionOnlyBuffer)
		{
			// Initialize the position buffer
			PositionVertexBuffer = new FRuntimeMeshVertexBuffer<FVector>(UpdateFrequency);

			// Get and adjust the vertex structure
			auto VertexStructure = VertexType::GetVertexStructure(VertexBuffer);
			VertexStructure.PositionComponent = FVertexStreamComponent(PositionVertexBuffer, 0, sizeof(FVector), VET_Float3);
			VertexFactory.Init(VertexStructure);
		}
		else
		{
			// Get and submit the vertex structure
			auto VertexStructure = VertexType::GetVertexStructure(VertexBuffer);
			VertexFactory.Init(VertexStructure);
		}
		
		// Initialize the vertex factory
		VertexFactory.InitResource();

		auto& Vertices = SectionUpdateData->VertexBuffer;
		VertexBuffer.SetNum(Vertices.Num());
		VertexBuffer.SetData(Vertices);

		if (NeedsPositionOnlyBuffer)
		{
			auto& PositionVertices = SectionUpdateData->PositionVertexBuffer;
			PositionVertexBuffer->SetNum(PositionVertices.Num());
			PositionVertexBuffer->SetData(PositionVertices);
		}
		
		auto& Indices = SectionUpdateData->IndexBuffer;
		IndexBuffer.SetNum(Indices.Num());
		IndexBuffer.SetData(Indices);
		bIsUsingAdjacency = SectionUpdateData->bIsAdjacencyIndexBuffer;
	}
	
	virtual void FinishUpdate_RenderThread(FRuntimeMeshRenderThreadCommandInterface* UpdateData) override
	{
		check(IsInRenderingThread());

		auto* SectionUpdateData = UpdateData->As<FRuntimeMeshSectionUpdateData<VertexType>>();
		check(SectionUpdateData);

		if (SectionUpdateData->bIncludeVertexBuffer)
		{
			auto& VertexBufferData = SectionUpdateData->VertexBuffer;
			VertexBuffer.SetNum(VertexBufferData.Num());
			VertexBuffer.SetData(VertexBufferData);
		}

		if (NeedsPositionOnlyBuffer && SectionUpdateData->bIncludePositionBuffer)
		{
			auto& PositionVertices = SectionUpdateData->PositionVertexBuffer;
			PositionVertexBuffer->SetNum(PositionVertices.Num());
			PositionVertexBuffer->SetData(PositionVertices);
		}

		if (SectionUpdateData->bIncludeIndices)
		{
			auto& IndexBufferData = SectionUpdateData->IndexBuffer;
			IndexBuffer.SetNum(IndexBufferData.Num());
			IndexBuffer.SetData(IndexBufferData);
			bIsUsingAdjacency = SectionUpdateData->bIsAdjacencyIndexBuffer;
		}
	}

	virtual void FinishPositionUpdate_RenderThread(FRuntimeMeshRenderThreadCommandInterface* UpdateData) override 
	{
		check(IsInRenderingThread());

		// Get the Position Only update data
		auto* SectionUpdateData = UpdateData->As<FRuntimeMeshSectionPositionOnlyUpdateData<VertexType>>();
		check(SectionUpdateData);
		
		// Copy the new data to the gpu
		PositionVertexBuffer->SetData(SectionUpdateData->PositionVertexBuffer);
	}

	virtual void FinishPropertyUpdate_RenderThread(FRuntimeMeshRenderThreadCommandInterface* UpdateData) override
	{
		auto* SectionUpdateData = UpdateData->As<FRuntimeMeshSectionPropertyUpdateData>();
		check(SectionUpdateData);

		// Copy visibility/shadow
		bIsVisible = SectionUpdateData->bIsVisible;
		bCastsShadow = SectionUpdateData->bCastsShadow;
	}

};
