// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "Engine.h"
#include "Components/MeshComponent.h"
#include "RuntimeMeshProfiling.h"
#include "RuntimeMeshVersion.h"
#include "RuntimeMeshSectionProxy.h"
#include "RuntimeMeshBuilder.h"
#include "RuntimeMeshLibrary.h"

/** Interface class for a single mesh section */
class FRuntimeMeshSectionInterface
{
protected:
	const bool bNeedsPositionOnlyBuffer;

public:
	/** Position only vertex buffer for this section */
	TArray<FVector> PositionVertexBuffer;

	/** Index buffer for this section */
	TArray<int32> IndexBuffer;

	/** Index buffer used for tessellation containing the needed adjacency info */
	TArray<int32> TessellationIndexBuffer;

	/** Local bounding box of section */
	FBox LocalBoundingBox;

	/** Should we build collision data for triangles in this section */
	bool CollisionEnabled;

	/** Should we display this section */
	bool bIsVisible;

	/** Should this section cast a shadow */
	bool bCastsShadow;

	/** If this section is currently using an adjacency index buffer */
	bool bShouldUseAdjacencyIndexBuffer;

	/** Update frequency of this section */
	EUpdateFrequency UpdateFrequency;

	FRuntimeMeshSectionInterface(bool bInNeedsPositionOnlyBuffer) : 
		bNeedsPositionOnlyBuffer(bInNeedsPositionOnlyBuffer),
		LocalBoundingBox(EForceInit::ForceInitToZero),
		CollisionEnabled(false),
		bIsVisible(true),
		bCastsShadow(true),
		bIsLegacySectionType(false)
	{}

	virtual ~FRuntimeMeshSectionInterface() { }

protected:

	/** Is this an internal section type. */
	bool bIsLegacySectionType;

	bool IsDualBufferSection() const { return bNeedsPositionOnlyBuffer; }

	/* Updates the vertex position buffer,   returns whether we have a new bounding box */
	bool UpdateVertexPositionBuffer(TArray<FVector>& Positions, const FBox* BoundingBox, bool bShouldMoveArray)
	{
		// Holds the new bounding box after this update.
		FBox NewBoundingBox(EForceInit::ForceInitToZero);

		if (bShouldMoveArray)
		{
			// Move buffer data
			PositionVertexBuffer = MoveTemp(Positions);

			// Calculate the bounding box if one doesn't exist.
			if (BoundingBox == nullptr)
			{
				for (int32 VertexIdx = 0; VertexIdx < PositionVertexBuffer.Num(); VertexIdx++)
				{
					NewBoundingBox += PositionVertexBuffer[VertexIdx];
				}
			}
			else
			{
				// Copy the supplied bounding box instead of calculating it.
				NewBoundingBox = *BoundingBox;
			}
		}
		else
		{
			if (BoundingBox == nullptr)
			{
				// Copy the buffer and calculate the bounding box at the same time
				int32 NumVertices = Positions.Num();
				PositionVertexBuffer.SetNumUninitialized(NumVertices);
				for (int32 VertexIdx = 0; VertexIdx < NumVertices; VertexIdx++)
				{
					NewBoundingBox += Positions[VertexIdx];
					PositionVertexBuffer[VertexIdx] = Positions[VertexIdx];
				}
			}
			else
			{
				// Copy the buffer
				PositionVertexBuffer = Positions;

				// Copy the supplied bounding box instead of calculating it.
				NewBoundingBox = *BoundingBox;
			}
		}

		// Update the bounding box if necessary and alert our caller if we did
		if (!(LocalBoundingBox == NewBoundingBox))
		{
			LocalBoundingBox = NewBoundingBox;
			return true;
		}

		return false;
	}

	virtual void UpdateVertexBuffer(IRuntimeMeshVerticesBuilder& Vertices, const FBox* BoundingBox, bool bShouldMoveArray) = 0;

	void UpdateIndexBuffer(TArray<int32>& Triangles, bool bShouldMoveArray)
	{
		if (bShouldMoveArray)
		{
			IndexBuffer = MoveTemp(Triangles);
		}
		else
		{
			IndexBuffer = Triangles;
		}
	}

	void UpdateIndexBuffer(FRuntimeMeshIndicesBuilder& Triangles, bool bShouldMoveArray)
	{
		if (bShouldMoveArray)
		{
			IndexBuffer = MoveTemp(*Triangles.GetIndices());
			Triangles.Reset();
		}
		else
		{
			IndexBuffer = *Triangles.GetIndices();
		}
	}

	void UpdateTessellationIndexBuffer(TArray<int32>& Triangles, bool bShouldMoveArray)
	{
		if (bShouldMoveArray)
		{
			TessellationIndexBuffer = MoveTemp(Triangles);
		}
		else
		{
			TessellationIndexBuffer = Triangles;
		}
	}

	virtual FRuntimeMeshSectionCreateDataInterface* GetSectionCreationData(FSceneInterface* InScene, UMaterialInterface* InMaterial) const = 0;

	virtual FRuntimeMeshRenderThreadCommandInterface* GetSectionUpdateData(bool bIncludePositionVertices, bool bIncludeVertices, bool bIncludeIndices) const = 0;

	virtual FRuntimeMeshRenderThreadCommandInterface* GetSectionPositionUpdateData() const = 0;

	virtual void RecalculateBoundingBox() = 0;

#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13
	virtual int32 GetCollisionInformation(TArray<FVector>& Positions, TArray<TArray<FVector2D>>& UVs, bool bIncludeUVs) = 0;
#else
	virtual int32 GetCollisionInformation(TArray<FVector>& Positions) = 0;
#endif

	virtual void GetInternalVertexComponents(int32& NumUVChannels, bool& WantsHalfPrecisionUVs) { }

	// This is only meant for internal use for supporting the old style create/update sections
	virtual bool UpdateVertexBufferInternal(const TArray<FVector>& Positions, const TArray<FVector>& Normals, const TArray<FRuntimeMeshTangent>& Tangents, const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FColor>& Colors) { return false; }
	
	virtual void GetSectionMesh(IRuntimeMeshVerticesBuilder*& Vertices, FRuntimeMeshIndicesBuilder*& Indices) = 0;

	virtual const FRuntimeMeshVertexTypeInfo* GetVertexType() const = 0;

	virtual void GenerateNormalTangent() = 0;

	virtual void GenerateTessellationIndices() = 0;


	virtual void Serialize(FArchive& Ar)
	{
		if (Ar.CustomVer(FRuntimeMeshVersion::GUID) >= FRuntimeMeshVersion::SerializationV2)
		{
			if (bNeedsPositionOnlyBuffer)
			{
				Ar << PositionVertexBuffer;
			}
			Ar << IndexBuffer;
			Ar << TessellationIndexBuffer;
			Ar << LocalBoundingBox;
			Ar << CollisionEnabled;
			Ar << bIsVisible;
			Ar << bCastsShadow;
			Ar << bShouldUseAdjacencyIndexBuffer;

			// Serialize the update frequency as an int32
			int32 UpdateFreq = (int32)UpdateFrequency;
			Ar << UpdateFreq;
			UpdateFrequency = (EUpdateFrequency)UpdateFreq;

			Ar << bIsLegacySectionType;
		}
		else
		{
			if (Ar.CustomVer(FRuntimeMeshVersion::GUID) >= FRuntimeMeshVersion::DualVertexBuffer)
			{
				Ar << PositionVertexBuffer;
			}
			Ar << IndexBuffer;
			Ar << LocalBoundingBox;
			Ar << CollisionEnabled;
			Ar << bIsVisible;
			int32 UpdateFreq = (int32)UpdateFrequency;
			Ar << UpdateFreq;
			UpdateFrequency = (EUpdateFrequency)UpdateFreq;
		}
	}

	

	friend class FRuntimeMeshSceneProxy;
	friend class URuntimeMeshComponent;
};

namespace RuntimeMeshSectionInternal
{
	template<typename Type>
	static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasPosition, int32>::Type
		GetAllVertexPositions(const TArray<Type>& VertexBuffer, const TArray<FVector>& PositionVertexBuffer, TArray<FVector>& Positions)
	{
		int32 VertexCount = VertexBuffer.Num();
		for (int32 VertIdx = 0; VertIdx < VertexCount; VertIdx++)
		{
			Positions.Add(VertexBuffer[VertIdx].Position);
		}
		return VertexCount;
	}

	template<typename Type>
	static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasPosition, int32>::Type
		GetAllVertexPositions(const TArray<Type>& VertexBuffer, const TArray<FVector>& PositionVertexBuffer, TArray<FVector>& Positions)
	{
		Positions.Append(PositionVertexBuffer);
		return PositionVertexBuffer.Num();
	}



	template<typename Type>
	static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasPosition, bool>::Type
		UpdateVertexBufferInternal(TArray<Type>& VertexBuffer, FBox& LocalBoundingBox, TArray<Type>& Vertices, const FBox* BoundingBox, bool bShouldMoveArray)
	{
		// Holds the new bounding box after this update.
		FBox NewBoundingBox(EForceInit::ForceInitToZero);

		if (bShouldMoveArray)
		{
			// Move buffer data
			VertexBuffer = MoveTemp(Vertices);

			// Calculate the bounding box if one doesn't exist.
			if (BoundingBox == nullptr)
			{
				for (int32 VertexIdx = 0; VertexIdx < VertexBuffer.Num(); VertexIdx++)
				{
					NewBoundingBox += VertexBuffer[VertexIdx].Position;
				}
			}
			else
			{
				// Copy the supplied bounding box instead of calculating it.
				NewBoundingBox = *BoundingBox;
			}
		}
		else
		{
			if (BoundingBox == nullptr)
			{
				// Copy the buffer and calculate the bounding box at the same time
				int32 NumVertices = Vertices.Num();
				VertexBuffer.SetNumUninitialized(NumVertices);
				for (int32 VertexIdx = 0; VertexIdx < NumVertices; VertexIdx++)
				{
					NewBoundingBox += Vertices[VertexIdx].Position;
					VertexBuffer[VertexIdx] = Vertices[VertexIdx];
				}
			}
			else
			{
				// Copy the buffer
				VertexBuffer = Vertices;

				// Copy the supplied bounding box instead of calculating it.
				NewBoundingBox = *BoundingBox;
			}
		}

		// Update the bounding box if necessary and alert our caller if we did
		if (!(LocalBoundingBox == NewBoundingBox))
		{
			LocalBoundingBox = NewBoundingBox;
			return true;
		}

		return false;
	}

	template<typename Type>
	static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasPosition, bool>::Type
		UpdateVertexBufferInternal(TArray<Type>& VertexBuffer, FBox& LocalBoundingBox, TArray<Type>& Vertices, const FBox* BoundingBox, bool bShouldMoveArray)
	{
		if (bShouldMoveArray)
		{
			VertexBuffer = MoveTemp(Vertices);
		}
		else
		{
			VertexBuffer = Vertices;
		}
		return false;
	}


	template<typename Type>
	static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasPosition>::Type RecalculateBoundingBox(TArray<Type>& VertexBuffer, FBox& BoundingBox)
	{
		for (int32 Index = 0; Index < VertexBuffer.Num(); Index++)
		{
			BoundingBox += VertexBuffer[Index].Position;
		}
	}

	template<typename Type>
	static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasPosition>::Type RecalculateBoundingBox(TArray<Type>& VertexBuffer, FBox& BoundingBox)
	{
	}

}

/** Templated class for a single mesh section */
template<typename VertexType>
class FRuntimeMeshSection : public FRuntimeMeshSectionInterface
{

public:
	/** Vertex buffer for this section */
	TArray<VertexType> VertexBuffer;

	FRuntimeMeshSection(bool bInNeedsPositionOnlyBuffer) : FRuntimeMeshSectionInterface(bInNeedsPositionOnlyBuffer) { }
	virtual ~FRuntimeMeshSection() override { }


protected:
	bool UpdateVertexBuffer(TArray<VertexType>& Vertices, const FBox* BoundingBox, bool bShouldMoveArray)
	{
		return RuntimeMeshSectionInternal::UpdateVertexBufferInternal<VertexType>(VertexBuffer, LocalBoundingBox, Vertices, BoundingBox, bShouldMoveArray);
	}

	virtual void UpdateVertexBuffer(IRuntimeMeshVerticesBuilder& Vertices, const FBox* BoundingBox, bool bShouldMoveArray) override
	{
		if (Vertices.GetBuilderType() == ERuntimeMeshVerticesBuilderType::Component)
		{
			FRuntimeMeshComponentVerticesBuilder* VerticesBuilder = static_cast<FRuntimeMeshComponentVerticesBuilder*>(&Vertices);

			TArray<FVector>* Positions = VerticesBuilder->GetPositions();
			TArray<FVector>* Normals = VerticesBuilder->GetNormals();
			TArray<FRuntimeMeshTangent>* Tangents = VerticesBuilder->GetTangents();
			TArray<FColor>* Colors = VerticesBuilder->GetColors();
			TArray<FVector2D>* UV0s = VerticesBuilder->GetUV0s();
			TArray<FVector2D>* UV1s = VerticesBuilder->GetUV1s();
					

			UpdateVertexBufferInternal(
				Positions ? *Positions : TArray<FVector>(),
				Normals ? *Normals : TArray<FVector>(),
				Tangents ? *Tangents : TArray<FRuntimeMeshTangent>(),
				UV0s ? *UV0s : TArray<FVector2D>(),
				UV1s ? *UV1s : TArray<FVector2D>(),
				Colors ? *Colors : TArray<FColor>());

			if (BoundingBox)
			{
				LocalBoundingBox = *BoundingBox;
			}
			else
			{
				LocalBoundingBox = FBox(*Positions);
			}

			if (bShouldMoveArray)
			{
				// This is just to keep similar behavior to the packed vertices builder.
				Vertices.Reset();
			}
		}
		else
		{
			// Make sure section type is the same
			Vertices.GetVertexType()->EnsureEquals<VertexType>();

			FRuntimeMeshPackedVerticesBuilder<VertexType>* VerticesBuilder = static_cast<FRuntimeMeshPackedVerticesBuilder<VertexType>*>(&Vertices);

			RuntimeMeshSectionInternal::UpdateVertexBufferInternal<VertexType>(VertexBuffer, LocalBoundingBox, *VerticesBuilder->GetVertices(), BoundingBox, bShouldMoveArray);

			if (BoundingBox == nullptr && VerticesBuilder->WantsSeparatePositionBuffer())
			{
				LocalBoundingBox = FBox(*VerticesBuilder->GetPositions());
			}
		}	
	}

	virtual FRuntimeMeshSectionCreateDataInterface* GetSectionCreationData(FSceneInterface* InScene, UMaterialInterface* InMaterial) const override
	{
		auto UpdateData = new FRuntimeMeshSectionCreateData<VertexType>();

		FMaterialRelevance MaterialRelevance = (InMaterial != nullptr) 
			? InMaterial->GetRelevance(InScene->GetFeatureLevel()) 
			: UMaterial::GetDefaultMaterial(MD_Surface)->GetRelevance(InScene->GetFeatureLevel());

		// Create new section proxy based on whether we need separate position buffer
		if (IsDualBufferSection())
		{
			UpdateData->NewProxy = new FRuntimeMeshSectionProxy<VertexType, true>(InScene, UpdateFrequency, bIsVisible, bCastsShadow, InMaterial, MaterialRelevance);
			UpdateData->PositionVertexBuffer = PositionVertexBuffer;
		}
		else
		{
			UpdateData->NewProxy = new FRuntimeMeshSectionProxy<VertexType, false>(InScene, UpdateFrequency, bIsVisible, bCastsShadow, InMaterial, MaterialRelevance);
		}
		const_cast<FRuntimeMeshSection*>(this)->bShouldUseAdjacencyIndexBuffer = UpdateData->NewProxy->ShouldUseAdjacencyIndexBuffer();

		UpdateData->VertexBuffer = VertexBuffer;

		// Switch between normal/tessellation indices

		if (bShouldUseAdjacencyIndexBuffer && TessellationIndexBuffer.Num() > 0)
		{
			UpdateData->IndexBuffer = TessellationIndexBuffer;
			UpdateData->bIsAdjacencyIndexBuffer = true;
		}
		else
		{
			UpdateData->IndexBuffer = IndexBuffer;
			UpdateData->bIsAdjacencyIndexBuffer = false;
		}

		return UpdateData;
	}

	virtual FRuntimeMeshRenderThreadCommandInterface* GetSectionUpdateData(bool bIncludePositionVertices, bool bIncludeVertices, bool bIncludeIndices) const override
	{
		auto UpdateData = new FRuntimeMeshSectionUpdateData<VertexType>();
		UpdateData->bIncludeVertexBuffer = bIncludeVertices;
		UpdateData->bIncludePositionBuffer = bIncludePositionVertices;
		UpdateData->bIncludeIndices = bIncludeIndices;

		if (bIncludePositionVertices)
		{
			UpdateData->PositionVertexBuffer = PositionVertexBuffer;
		}

		if (bIncludeVertices)
		{
			UpdateData->VertexBuffer = VertexBuffer;
		}

		if (bIncludeIndices)
		{
			if (bShouldUseAdjacencyIndexBuffer && TessellationIndexBuffer.Num() > 0)
			{
				UpdateData->IndexBuffer = TessellationIndexBuffer;
				UpdateData->bIsAdjacencyIndexBuffer = true;
			}
			else
			{
				UpdateData->IndexBuffer = IndexBuffer;
				UpdateData->bIsAdjacencyIndexBuffer = false;
			}
		}

		return UpdateData;
	}

	virtual FRuntimeMeshRenderThreadCommandInterface* GetSectionPositionUpdateData() const override
	{
		auto UpdateData = new FRuntimeMeshSectionPositionOnlyUpdateData<VertexType>();

		UpdateData->PositionVertexBuffer = PositionVertexBuffer;

		return UpdateData;
	}

#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13
	virtual int32 GetCollisionInformation(TArray<FVector>& Positions, TArray<TArray<FVector2D>>& UVs, bool bIncludeUVs) override
#else
	virtual int32 GetCollisionInformation(TArray<FVector>& Positions) override
#endif
	{
		FRuntimeMeshPackedVerticesBuilder<VertexType> VerticesBuilder(&VertexBuffer, bNeedsPositionOnlyBuffer ? &PositionVertexBuffer : nullptr);

		int32 PositionStart = Positions.Num();
		Positions.SetNum(PositionStart + VerticesBuilder.Length());

#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13
		if (bIncludeUVs)
		{
			UVs[0].SetNumZeroed(PositionStart + VerticesBuilder.Length());
		}
#endif

		for (int VertexIdx = 0; VertexIdx < VerticesBuilder.Length(); VertexIdx++)
		{
			Positions[PositionStart + VertexIdx] = VerticesBuilder.GetPosition(VertexIdx);

#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13
			if (bIncludeUVs && VerticesBuilder.HasUVComponent(0))
			{
				UVs[0][PositionStart + VertexIdx] = VerticesBuilder.GetUV(0);
			}
#endif
		}

		return VerticesBuilder.Length();
	}

	virtual void GetSectionMesh(IRuntimeMeshVerticesBuilder*& Vertices, FRuntimeMeshIndicesBuilder*& Indices) override
	{
		Vertices = new FRuntimeMeshPackedVerticesBuilder<VertexType>(&VertexBuffer);
		Indices = new FRuntimeMeshIndicesBuilder(&IndexBuffer);
	}

	virtual const FRuntimeMeshVertexTypeInfo* GetVertexType() const { return &VertexType::TypeInfo; }

	virtual void GenerateNormalTangent()
	{
		if (IsDualBufferSection())
		{
			URuntimeMeshLibrary::CalculateTangentsForMesh<VertexType>(PositionVertexBuffer, VertexBuffer, IndexBuffer);
		}
		else
		{
			URuntimeMeshLibrary::CalculateTangentsForMesh<VertexType>(VertexBuffer, IndexBuffer);
		}
	}

	virtual void GenerateTessellationIndices()
	{
		TArray<int32> TessellationIndices;
		if (IsDualBufferSection())
		{
			URuntimeMeshLibrary::GenerateTessellationIndexBuffer<VertexType>(PositionVertexBuffer, VertexBuffer, IndexBuffer, TessellationIndices);
		}
		else
		{
			URuntimeMeshLibrary::GenerateTessellationIndexBuffer<VertexType>(VertexBuffer, IndexBuffer, TessellationIndices);
		}
		UpdateTessellationIndexBuffer(TessellationIndices, true);
	}

	virtual void RecalculateBoundingBox() override
	{
		LocalBoundingBox.Init();

		if (IsDualBufferSection())
		{
			for (int32 Index = 0; Index < PositionVertexBuffer.Num(); Index++)
			{
				LocalBoundingBox += PositionVertexBuffer[Index];
			}
		}
		else
		{
			RuntimeMeshSectionInternal::RecalculateBoundingBox<VertexType>(VertexBuffer, LocalBoundingBox);
		}
	}

	virtual void GetInternalVertexComponents(int32& NumUVChannels, bool& WantsHalfPrecisionUVs) override
	{
		NumUVChannels = FRuntimeMeshVertexTraits<VertexType>::NumUVChannels;
		WantsHalfPrecisionUVs = !FRuntimeMeshVertexTraits<VertexType>::HasHighPrecisionUVs;
	}

	virtual bool UpdateVertexBufferInternal(const TArray<FVector>& Positions, const TArray<FVector>& Normals, const TArray<FRuntimeMeshTangent>& Tangents, const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FColor>& Colors) override
	{
		// Check existence of data components
		const bool HasPositions = Positions.Num() > 0;

		int32 NewVertexCount = HasPositions ? Positions.Num() : VertexBuffer.Num();
		int32 OldVertexCount = FMath::Min(VertexBuffer.Num(), NewVertexCount);

		// Size the vertex buffer correctly
		if (NewVertexCount != VertexBuffer.Num())
		{
			VertexBuffer.SetNumZeroed(NewVertexCount);
		}

		// Clear the bounding box if we have new positions
		if (HasPositions)
		{
			LocalBoundingBox.Init();
		}

		FRuntimeMeshPackedVerticesBuilder<VertexType> VerticesBuilder(&VertexBuffer);
				
		// Loop through existing range to update data
		for (int32 VertexIdx = 0; VertexIdx < OldVertexCount; VertexIdx++)
		{
			VerticesBuilder.Seek(VertexIdx);

			// Update position and bounding box
			if (HasPositions)
			{
				VerticesBuilder.SetPosition(Positions[VertexIdx]);
				LocalBoundingBox += Positions[VertexIdx];
			}
			
			// see if we have a new normal and/or tangent
			bool HasNormal = Normals.Num() > VertexIdx;
			bool HasTangent = Tangents.Num() > VertexIdx;

			// Update normal and tangent together
			if (HasNormal && HasTangent)
			{
				FVector4 NewNormal(Normals[VertexIdx], Tangents[VertexIdx].bFlipTangentY ? -1.0f : 1.0f);
				VerticesBuilder.SetNormal(NewNormal);
				VerticesBuilder.SetTangent(Tangents[VertexIdx].TangentX);
			}
			// Else update only normal keeping the W component 
			else if (HasNormal)
			{
				float W = VerticesBuilder.GetNormal().W;
				VerticesBuilder.SetNormal(FVector4(Normals[VertexIdx], W));
			}
			// Else update tangent updating the normals W component
			else if (HasTangent)
			{
				FVector4 Normal = VerticesBuilder.GetNormal();
				Normal.W = Tangents[VertexIdx].bFlipTangentY ? -1.0f : 1.0f;
				VerticesBuilder.SetNormal(Normal);
				VerticesBuilder.SetTangent(Tangents[VertexIdx].TangentX);
			}

			// Update color
			if (Colors.Num() > VertexIdx)
			{
				VerticesBuilder.SetColor(Colors[VertexIdx]);
			}

			// Update UV0
			if (UV0.Num() > VertexIdx)
			{
				VerticesBuilder.SetUV(0, UV0[VertexIdx]);
			}

			// Update UV1 if needed
			if (UV1.Num() > VertexIdx && VerticesBuilder.HasUVComponent(1))
			{
				VerticesBuilder.SetUV(1, UV1[VertexIdx]);
			}
		}

		// Loop through additional range to add new data
		for (int32 VertexIdx = OldVertexCount; VertexIdx < NewVertexCount; VertexIdx++)
		{
			VerticesBuilder.Seek(VertexIdx);

			// Set position
			VerticesBuilder.SetPosition(Positions[VertexIdx]);

			// Update bounding box
			LocalBoundingBox += Positions[VertexIdx];

			// see if we have a new normal and/or tangent
			bool HasNormal = Normals.Num() > VertexIdx;
			bool HasTangent = Tangents.Num() > VertexIdx;

			// Set normal and tangent both
			if (HasNormal && HasTangent)
			{
				FVector4 NewNormal(Normals[VertexIdx], Tangents[VertexIdx].bFlipTangentY ? -1.0f : 1.0f);
				VerticesBuilder.SetNormal(NewNormal);
				VerticesBuilder.SetTangent(Tangents[VertexIdx].TangentX);
			}
			// Set normal and default tangent
			else if (HasNormal)
			{
				VerticesBuilder.SetNormal(FVector4(Normals[VertexIdx], 1.0f));
				VerticesBuilder.SetTangent(FVector(1.0f, 0.0f, 0.0f));
			}
			// Default normal and set tangent
			else if (HasTangent)
			{
				VerticesBuilder.SetNormal(FVector4(0.0f, 0.0f, 1.0f, Tangents[VertexIdx].bFlipTangentY ? -1.0f : 1.0f));
				VerticesBuilder.SetTangent(Tangents[VertexIdx].TangentX);
			}
			// Default normal and tangent
			else
			{
				VerticesBuilder.SetNormal(FVector4(0.0f, 0.0f, 1.0f, 1.0f));
				VerticesBuilder.SetTangent(FVector(1.0f, 0.0f, 0.0f));
			}

			// Set color or default 
			VerticesBuilder.SetColor(Colors.Num() > VertexIdx ? Colors[VertexIdx] : FColor::White);

			// Update UV0
			VerticesBuilder.SetUV(0, UV0.Num() > VertexIdx ? UV0[VertexIdx] : FVector2D::ZeroVector);

			// Update UV1 if needed
			if (VerticesBuilder.HasUVComponent(1))
			{
				VerticesBuilder.SetUV(1, UV1.Num() > VertexIdx ? UV1[VertexIdx] : FVector2D::ZeroVector);
			}
		}

		return true;
	}

private:
	void SerializeLegacy(FArchive& Ar)
	{
		int32 VertexBufferLength = VertexBuffer.Num();
		Ar << VertexBufferLength;

		if (Ar.IsLoading())
		{
			VertexBuffer.SetNum(VertexBufferLength);
			FRuntimeMeshPackedVerticesBuilder<VertexType> VerticesBuilder(&VertexBuffer);

			for (int32 Index = 0; Index < VertexBufferLength; Index++)
			{
				VerticesBuilder.Seek(Index);

				FVector TempPosition;
				Ar << TempPosition;
				VerticesBuilder.SetPosition(TempPosition);

				FPackedNormal TempNormal;
				Ar << TempNormal;
				VerticesBuilder.SetNormal(TempNormal);

				Ar << TempNormal;
				VerticesBuilder.SetTangent(TempNormal);

				FColor TempColor;
				Ar << TempColor;
				VerticesBuilder.SetColor(TempColor);

				if (FRuntimeMeshVertexTraits<VertexType>::HasHighPrecisionUVs)
				{
					FVector2D TempUV;
					Ar << TempUV;
					VerticesBuilder.SetUV(0, TempUV);

					if (FRuntimeMeshVertexTraits<VertexType>::NumUVChannels > 1)
					{
						Ar << TempUV;
						VerticesBuilder.SetUV(1, TempUV);
					}
				}
				else
				{
					FVector2DHalf TempUV;
					Ar << TempUV;
					VerticesBuilder.SetUV(0, TempUV);

					if (FRuntimeMeshVertexTraits<VertexType>::NumUVChannels > 1)
					{
						Ar << TempUV;
						VerticesBuilder.SetUV(1, TempUV);
					}
				}
			}
		}
		else
		{
			check(false && "Cannot use legacy save.");
		}
	}

public:
	virtual void Serialize(FArchive& Ar) override
	{

		if (Ar.CustomVer(FRuntimeMeshVersion::GUID) >= FRuntimeMeshVersion::SerializationV2)
		{
			Ar << VertexBuffer;
			FRuntimeMeshSectionInterface::Serialize(Ar);
		}
		else
		{
			FRuntimeMeshSectionInterface::Serialize(Ar);
			SerializeLegacy(Ar);
		}
	}

	friend class URuntimeMeshComponent;
};


/** Smart pointer to a Runtime Mesh Section */
using RuntimeMeshSectionPtr = TSharedPtr<FRuntimeMeshSectionInterface>;
