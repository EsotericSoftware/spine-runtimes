// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "RuntimeMeshCore.h"
#include "RuntimeMeshGenericVertex.h"

//////////////////////////////////////////////////////////////////////////
//	This is a work in progress, it's functional, but could use some improvement
//////////////////////////////////////////////////////////////////////////

enum class ERuntimeMeshVerticesBuilderType : uint8
{
	Component,
	Packed
};


class IRuntimeMeshVerticesBuilder
{
public:
	IRuntimeMeshVerticesBuilder() { }
	IRuntimeMeshVerticesBuilder(const IRuntimeMeshVerticesBuilder& Other) = delete;
	IRuntimeMeshVerticesBuilder& operator=(const IRuntimeMeshVerticesBuilder& Other) = delete;
	virtual ~IRuntimeMeshVerticesBuilder() { }

	virtual ERuntimeMeshVerticesBuilderType GetBuilderType() const = 0;
	virtual const FRuntimeMeshVertexTypeInfo* GetVertexType() const = 0;

	virtual bool HasPositionComponent() const = 0;
	virtual bool HasNormalComponent() const = 0;
	virtual bool HasTangentComponent() const = 0;
	virtual bool HasColorComponent() const = 0;
	virtual bool HasUVComponent(int32 Index) const = 0;
	virtual bool HasHighPrecisionNormals() const = 0;
	virtual bool HasHighPrecisionUVs() const = 0;

	virtual void SetPosition(const FVector& InPosition) = 0;
	virtual void SetNormal(const FVector4& InNormal) = 0;
	virtual void SetTangent(const FVector& InTangent) = 0;
	virtual void SetColor(const FColor& InColor) = 0;
	virtual void SetUV(int32 Index, const FVector2D& InUV) = 0;

	virtual void SetPosition(int32 VertexIndex, const FVector& InPosition) = 0;
	virtual void SetNormal(int32 VertexIndex, const FVector4& InNormal) = 0;
	virtual void SetTangent(int32 VertexIndex, const FVector& InTangent) = 0;
	virtual void SetColor(int32 VertexIndex, const FColor& InColor) = 0;
	virtual void SetUV(int32 VertexIndex, int32 Index, const FVector2D& InUV) = 0;
	
	void SetTangents(const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ)
	{
		SetNormal(FVector4(InTangentZ, GetBasisDeterminantSign(InTangentX, InTangentY, InTangentZ)));
		SetTangent(InTangentX);
	}
	void SetTangents(int32 VertexIndex, const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ)
	{
		Seek(VertexIndex);
		SetNormal(FVector4(InTangentZ, GetBasisDeterminantSign(InTangentX, InTangentY, InTangentZ)));
		SetTangent(InTangentX);
	}

	virtual FVector GetPosition() const = 0;
	virtual FVector4 GetNormal() const = 0;
	virtual FVector GetTangent() const = 0;
	virtual FColor GetColor() const = 0;
	virtual FVector2D GetUV(int32 Index) const = 0;

	virtual FVector GetPosition(int32 VertexIndex) const = 0;
	virtual FVector4 GetNormal(int32 VertexIndex) const = 0;
	virtual FVector GetTangent(int32 VertexIndex) const = 0;
	virtual FColor GetColor(int32 VertexIndex) const = 0;
	virtual FVector2D GetUV(int32 VertexIndex, int32 Index) const = 0;

	virtual int32 Length() const = 0;
	virtual void Seek(int32 Position) const = 0;
	void SeekEnd() const
	{
		Seek(Length() - 1);
	}
	virtual int32 MoveNext() const = 0;
	virtual int32 MoveNextOrAdd() = 0;

	virtual void Reset() = 0;

	virtual IRuntimeMeshVerticesBuilder* Clone(bool bIncludeData = true) const = 0;

	virtual bool WantsSeparatePositionBuffer() const
	{
		return false;
	}
};


template<typename VertexType>
class FRuntimeMeshPackedVerticesBuilder : public IRuntimeMeshVerticesBuilder
{
private:
	TArray<VertexType>* Vertices;
	TArray<FVector>* Positions;
	int32 CurrentPosition;
	bool bOwnsVertexArray;
public:

	FRuntimeMeshPackedVerticesBuilder(bool bWantsSeparatePositions = false)
		: Vertices(new TArray<VertexType>())
		, Positions(bWantsSeparatePositions? new TArray<FVector>() : nullptr)
		, CurrentPosition(-1)
		, bOwnsVertexArray(true)
	{ }
	FRuntimeMeshPackedVerticesBuilder(TArray<VertexType>* InVertices, TArray<FVector>* InPositions = nullptr)
		: Vertices(InVertices)
		, Positions(InPositions)
		, CurrentPosition(-1)
		, bOwnsVertexArray(false)
	{ }
	FRuntimeMeshPackedVerticesBuilder(const FRuntimeMeshPackedVerticesBuilder& Other) = delete;
	FRuntimeMeshPackedVerticesBuilder& operator=(const FRuntimeMeshPackedVerticesBuilder& Other) = delete;
	virtual ~FRuntimeMeshPackedVerticesBuilder() override
	{
		if (bOwnsVertexArray)
		{
			delete Vertices;

			if (Positions)
			{
				delete Positions;
			}
		}
	}


	virtual ERuntimeMeshVerticesBuilderType GetBuilderType() const
	{
		return ERuntimeMeshVerticesBuilderType::Packed;
	}
	virtual const FRuntimeMeshVertexTypeInfo* GetVertexType() const
	{
		return &VertexType::TypeInfo;
	}


	virtual bool HasPositionComponent() const override { return Positions != nullptr || FRuntimeMeshVertexTraits<VertexType>::HasPosition; }
	virtual bool HasNormalComponent() const override { return FRuntimeMeshVertexTraits<VertexType>::HasNormal; }
	virtual bool HasTangentComponent() const override { return FRuntimeMeshVertexTraits<VertexType>::HasTangent; }
	virtual bool HasColorComponent() const override { return FRuntimeMeshVertexTraits<VertexType>::HasColor; }
	virtual bool HasUVComponent(int32 Index) const override
	{
		switch (Index)
		{
		case 0:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV0;
		case 1:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV1;
		case 2:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV2;
		case 3:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV3;
		case 4:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV4;
		case 5:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV5;
		case 6:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV6;
		case 7:
			return FRuntimeMeshVertexTraits<VertexType>::HasUV7;
		}
		return false;
	}
	virtual bool HasHighPrecisionNormals() const override { return FRuntimeMeshVertexTraits<VertexType>::HasHighPrecisionNormals; }
	virtual bool HasHighPrecisionUVs() const override { return FRuntimeMeshVertexTraits<VertexType>::HasHighPrecisionUVs; }

	virtual void SetPosition(const FVector& InPosition) override 
	{ 
		if (Positions)
		{
			(*Positions)[CurrentPosition] = InPosition;
		}
		else
		{
			SetPositionInternal<VertexType>((*Vertices)[CurrentPosition], InPosition);
		}		
	}
	virtual void SetNormal(const FVector4& InNormal) override { SetNormalInternal<VertexType>((*Vertices)[CurrentPosition], InNormal); }
	virtual void SetTangent(const FVector& InTangent) override { SetTangentInternal<VertexType>((*Vertices)[CurrentPosition], InTangent); }
	virtual void SetColor(const FColor& InColor) override { SetColorInternal<VertexType>((*Vertices)[CurrentPosition], InColor); }
	virtual void SetUV(int32 Index, const FVector2D& InUV)
	{
		switch (Index)
		{
		case 0:
			SetUV0Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 1:
			SetUV1Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 2:
			SetUV2Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 3:
			SetUV3Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 4:
			SetUV4Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 5:
			SetUV5Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 6:
			SetUV6Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 7:
			SetUV7Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		}
	}


	virtual void SetPosition(int32 VertexIndex, const FVector& InPosition) override
	{
		Seek(VertexIndex);
		if (Positions)
		{
			(*Positions)[CurrentPosition] = InPosition;
		}
		else
		{
			SetPositionInternal<VertexType>((*Vertices)[CurrentPosition], InPosition);
		}
	}
	virtual void SetNormal(int32 VertexIndex, const FVector4& InNormal) override { Seek(VertexIndex); SetNormalInternal<VertexType>((*Vertices)[CurrentPosition], InNormal); }
	virtual void SetTangent(int32 VertexIndex, const FVector& InTangent) override { Seek(VertexIndex); SetTangentInternal<VertexType>((*Vertices)[CurrentPosition], InTangent); }
	virtual void SetColor(int32 VertexIndex, const FColor& InColor) override { Seek(VertexIndex); SetColorInternal<VertexType>((*Vertices)[CurrentPosition], InColor); }
	virtual void SetUV(int32 VertexIndex, int32 Index, const FVector2D& InUV)
	{
		Seek(VertexIndex);
		switch (Index)
		{
		case 0:
			SetUV0Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 1:
			SetUV1Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 2:
			SetUV2Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 3:
			SetUV3Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 4:
			SetUV4Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 5:
			SetUV5Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 6:
			SetUV6Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		case 7:
			SetUV7Internal<VertexType>((*Vertices)[CurrentPosition], InUV);
			return;
		}
	}


	virtual FVector GetPosition() const override 
	{ 
		if (Positions)
		{
			return (*Positions)[CurrentPosition];
		}
		else
		{
			return GetPositionInternal<VertexType>((*Vertices)[CurrentPosition]);
		}
	}
	virtual FVector4 GetNormal() const override { return GetNormalInternal<VertexType>((*Vertices)[CurrentPosition]); }
	virtual FVector GetTangent() const override { return GetTangentInternal<VertexType>((*Vertices)[CurrentPosition]); }
	virtual FColor GetColor() const override { return GetColorInternal<VertexType>((*Vertices)[CurrentPosition]); }
	virtual FVector2D GetUV(int32 Index) const override
	{
		switch (Index)
		{
		case 0:
			return GetUV0Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 1:
			return GetUV1Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 2:
			return GetUV2Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 3:
			return GetUV3Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 4:
			return GetUV4Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 5:
			return GetUV5Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 6:
			return GetUV6Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 7:
			return GetUV7Internal<VertexType>((*Vertices)[CurrentPosition]);
		}
		return FVector2D::ZeroVector;
	}


	virtual FVector GetPosition(int32 VertexIndex) const override
	{
		Seek(VertexIndex);
		if (Positions)
		{
			return (*Positions)[CurrentPosition];
		}
		else
		{
			return GetPositionInternal<VertexType>((*Vertices)[CurrentPosition]);
		}
	}
	virtual FVector4 GetNormal(int32 VertexIndex) const override { Seek(VertexIndex); return GetNormalInternal<VertexType>((*Vertices)[CurrentPosition]); }
	virtual FVector GetTangent(int32 VertexIndex) const override { Seek(VertexIndex); return GetTangentInternal<VertexType>((*Vertices)[CurrentPosition]); }
	virtual FColor GetColor(int32 VertexIndex) const override { Seek(VertexIndex); return GetColorInternal<VertexType>((*Vertices)[CurrentPosition]); }
	virtual FVector2D GetUV(int32 VertexIndex, int32 Index) const override
	{
		Seek(VertexIndex);
		switch (Index)
		{
		case 0:
			return GetUV0Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 1:
			return GetUV1Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 2:
			return GetUV2Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 3:
			return GetUV3Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 4:
			return GetUV4Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 5:
			return GetUV5Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 6:
			return GetUV6Internal<VertexType>((*Vertices)[CurrentPosition]);
		case 7:
			return GetUV7Internal<VertexType>((*Vertices)[CurrentPosition]);
		}
		return FVector2D::ZeroVector;
	}

	virtual int32 Length() const override { return Vertices->Num(); }
	virtual void Seek(int32 Position) const override 
	{ 
		const_cast<FRuntimeMeshPackedVerticesBuilder<VertexType>*>(this)->CurrentPosition = Position;
	}
	virtual int32 MoveNext() const override
	{
		return ++const_cast<FRuntimeMeshPackedVerticesBuilder<VertexType>*>(this)->CurrentPosition;
	}
	virtual int32 MoveNextOrAdd() override
	{
		CurrentPosition++;
		if (CurrentPosition >= Vertices->Num())
		{
			Vertices->SetNumZeroed(CurrentPosition + 1, false);
			if (Positions)
			{
				Positions->SetNumZeroed(CurrentPosition + 1, false);
			}
		}
		return CurrentPosition;
	}

	virtual void Reset() override
	{
		Vertices->Reset();
		if (Positions)
		{
			Positions->Reset();
		}
		CurrentPosition = -1;
	}

	virtual IRuntimeMeshVerticesBuilder* Clone(bool bIncludeData = true) const override
	{
		FRuntimeMeshPackedVerticesBuilder<VertexType>* NewBuilder = new FRuntimeMeshPackedVerticesBuilder(Positions != nullptr);

		if (bIncludeData)
		{
			*NewBuilder->Vertices = *Vertices;
			*NewBuilder->Positions = *Positions;
			NewBuilder->Seek(0);
		}

		return NewBuilder;
	}

	TArray<VertexType>* GetVertices()
	{
		return Vertices;
	}

	TArray<FVector>* GetPositions()
	{
		return Positions;
	}


	virtual bool WantsSeparatePositionBuffer() const
	{
		return Positions != nullptr;
	}

private:
	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasPosition>::Type SetPositionInternal(Type& Vertex, const FVector& Position)
	{
		Vertex.Position = Position;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasPosition>::Type SetPositionInternal(Type& Vertex, const FVector& Position)
	{

	}	
	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasPosition, FVector>::Type GetPositionInternal(const Type& Vertex)
	{
		return Vertex.Position;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasPosition, FVector>::Type GetPositionInternal(const Type& Vertex)
	{
		return FVector::ZeroVector;
	}


	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasNormal>::Type SetNormalInternal(Type& Vertex, const FVector4& Normal)
	{
		Vertex.Normal = Normal;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasNormal>::Type SetNormalInternal(Type& Vertex, const FVector4& Normal)
	{

	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasNormal, FVector4>::Type GetNormalInternal(const Type& Vertex)
	{
		return Vertex.Normal;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasNormal, FVector4>::Type GetNormalInternal(const Type& Vertex)
	{
		return FVector::ZeroVector;
	}


	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasTangent>::Type SetTangentInternal(Type& Vertex, const FVector4& Tangent)
	{
		Vertex.Tangent = Tangent;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasTangent>::Type SetTangentInternal(Type& Vertex, const FVector4& Tangent)
	{

	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasTangent, FVector4>::Type GetTangentInternal(const Type& Vertex)
	{
		return Vertex.Tangent;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasTangent, FVector4>::Type GetTangentInternal(const Type& Vertex)
	{
		return FVector::ZeroVector;
	}


	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasColor>::Type SetColorInternal(Type& Vertex, const FColor& Color)
	{
		Vertex.Color = Color;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasColor>::Type SetColorInternal(Type& Vertex, const FColor& Color)
	{

	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasColor, FColor>::Type GetColorInternal(const Type& Vertex)
	{
		return Vertex.Color;
	}
	template<typename Type>
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasColor, FColor>::Type GetColorInternal(const Type& Vertex)
	{
		return FColor::Transparent;
	}



#define CreateUVChannelGetSetPair(Index)																											\
	template<typename Type>																															\
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasUV##Index>::Type SetUV##Index##Internal(Type& Vertex, const FVector2D& UV##Index)			\
	{																																				\
		Vertex.UV##Index = UV##Index;																												\
	}																																				\
	template<typename Type>																															\
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasUV##Index>::Type SetUV##Index##Internal(Type& Vertex, const FVector2D& UV##Index)			\
	{																																				\
	}																																				\
	template<typename Type>																															\
	FORCEINLINE static typename TEnableIf<FRuntimeMeshVertexTraits<Type>::HasUV##Index, FVector2D>::Type GetUV##Index##Internal(const Type& Vertex)						\
	{																																				\
		return Vertex.UV##Index;																													\
	}																																				\
	template<typename Type>																															\
	FORCEINLINE static typename TEnableIf<!FRuntimeMeshVertexTraits<Type>::HasUV##Index, FVector2D>::Type GetUV##Index##Internal(const Type& Vertex)					\
	{																																				\
		return FVector2D::ZeroVector;																												\
	}																																				


CreateUVChannelGetSetPair(0);
CreateUVChannelGetSetPair(1);
CreateUVChannelGetSetPair(2);
CreateUVChannelGetSetPair(3);
CreateUVChannelGetSetPair(4);
CreateUVChannelGetSetPair(5);
CreateUVChannelGetSetPair(6);
CreateUVChannelGetSetPair(7);


#undef CreateUVChannelGetSetPair
};

class RUNTIMEMESHCOMPONENT_API FRuntimeMeshComponentVerticesBuilder : public IRuntimeMeshVerticesBuilder
{
private:
	TArray<FVector>* Positions;
	TArray<FVector>* Normals;
	TArray<FRuntimeMeshTangent>* Tangents;
	TArray<FColor>* Colors;
	TArray<FVector2D>* UV0s;
	TArray<FVector2D>* UV1s;
	int32 CurrentPosition;
	bool bOwnsBuffers;
public:

	FRuntimeMeshComponentVerticesBuilder(bool bInWantsNormal, bool bInWantsTangent, bool bInWantsColor, bool bInWantsUV0, bool bInWantsUV1)
		: Positions(new TArray<FVector>())
		, Normals(bInWantsNormal ? new TArray<FVector>() : nullptr)
		, Tangents(bInWantsTangent ? new TArray<FRuntimeMeshTangent>() : nullptr)
		, Colors(bInWantsColor ? new TArray<FColor>() : nullptr)
		, UV0s(bInWantsUV0 ? new TArray<FVector2D>() : nullptr)
		, UV1s(bInWantsUV1 ? new TArray<FVector2D>() : nullptr)
		, CurrentPosition(-1)
		, bOwnsBuffers(true)		
	{ }
	FRuntimeMeshComponentVerticesBuilder(TArray<FVector>* InPositions, TArray<FVector>* InNormals, TArray<FRuntimeMeshTangent>* InTangents,
		TArray<FColor>* InColors, TArray<FVector2D>* InUV0s, TArray<FVector2D>* InUV1s = nullptr)
		: Positions(InPositions)
		, Normals(InNormals)
		, Tangents(InTangents)
		, Colors(InColors)
		, UV0s(InUV0s)
		, UV1s(InUV1s)
		, CurrentPosition(-1)
		, bOwnsBuffers(false)
	{ }
	FRuntimeMeshComponentVerticesBuilder(const FRuntimeMeshComponentVerticesBuilder& Other) = delete;
	FRuntimeMeshComponentVerticesBuilder& operator=(const FRuntimeMeshComponentVerticesBuilder& Other) = delete;
	virtual ~FRuntimeMeshComponentVerticesBuilder() override
	{
		if (bOwnsBuffers)
		{			
			if (Positions) delete Positions;
			if (Normals) delete Normals;
			if (Tangents) delete Tangents;
			if (Colors) delete Colors;
			if (UV0s) delete UV0s;
			if (UV1s) delete UV1s;
		}
	}


	virtual ERuntimeMeshVerticesBuilderType GetBuilderType() const { return ERuntimeMeshVerticesBuilderType::Packed; }
	virtual const FRuntimeMeshVertexTypeInfo* GetVertexType() const;


	virtual bool HasPositionComponent() const override { return Positions != nullptr; }
	virtual bool HasNormalComponent() const override { return Normals != nullptr; }
	virtual bool HasTangentComponent() const override { return Tangents != nullptr; }
	virtual bool HasColorComponent() const override { return Colors != nullptr; }
	virtual bool HasUVComponent(int32 Index) const override
	{
		switch (Index)
		{
		case 0:
			return UV0s != nullptr;
		case 1:
			return UV1s != nullptr;
		default:
			return false;
		}
	}
	virtual bool HasHighPrecisionNormals() const override { return false; }
	virtual bool HasHighPrecisionUVs() const override { return true; }

	virtual void SetPosition(const FVector& InPosition) override 
	{
		if (CurrentPosition >= Positions->Num())
		{
			Positions->SetNumZeroed(CurrentPosition + 1, false);
		}
		(*Positions)[CurrentPosition] = InPosition;
	}
	virtual void SetNormal(const FVector4& InNormal) override
	{
		if (Normals)
		{
			if (CurrentPosition >= Normals->Num())
			{
				Normals->SetNumZeroed(CurrentPosition + 1, false);
			}
			(*Normals)[CurrentPosition] = InNormal;
			(*Tangents)[CurrentPosition].bFlipTangentY = InNormal.W < 0.0f;
		}
	}
	virtual void SetTangent(const FVector& InTangent) override
	{
		if (Tangents)
		{
			if (CurrentPosition >= Tangents->Num())
			{
				Tangents->SetNumZeroed(CurrentPosition + 1, false);
			}
			(*Tangents)[CurrentPosition].TangentX = InTangent;
		}
	}
	virtual void SetColor(const FColor& InColor) override
	{
		if (Colors)
		{
			if (CurrentPosition >= Colors->Num())
			{
				Colors->SetNumZeroed(CurrentPosition + 1, false);
			}
			(*Colors)[CurrentPosition] = InColor;
		}
	}
	virtual void SetUV(int32 Index, const FVector2D& InUV)
	{
		switch (Index)
		{
		case 0:
		{
			if (UV0s)
			{
				if (CurrentPosition >= UV0s->Num())
				{
					UV0s->SetNumZeroed(CurrentPosition + 1, false);
				}
				(*UV0s)[CurrentPosition] = InUV;
			}
		}
		case 1:
		{
			if (UV1s)
			{
				if (CurrentPosition >= UV1s->Num())
				{
					UV1s->SetNumZeroed(CurrentPosition + 1, false);
				}
				(*UV1s)[CurrentPosition] = InUV;
			}
		}
		default:
			return;
		}
	}

	virtual void SetPosition(int32 VertexIndex, const FVector& InPosition) override
	{
		Seek(VertexIndex);
		if (CurrentPosition >= Positions->Num())
		{
			Positions->SetNumZeroed(CurrentPosition + 1, false);
		}
		(*Positions)[CurrentPosition] = InPosition;
	}
	virtual void SetNormal(int32 VertexIndex, const FVector4& InNormal) override
	{
		Seek(VertexIndex);
		if (Normals)
		{
			if (CurrentPosition >= Normals->Num())
			{
				Normals->SetNumZeroed(CurrentPosition + 1, false);
			}
			(*Normals)[CurrentPosition] = InNormal;
			(*Tangents)[CurrentPosition].bFlipTangentY = InNormal.W < 0.0f;
		}
	}
	virtual void SetTangent(int32 VertexIndex, const FVector& InTangent) override
	{
		Seek(VertexIndex);
		if (Tangents)
		{
			if (CurrentPosition >= Tangents->Num())
			{
				Tangents->SetNumZeroed(CurrentPosition + 1, false);
			}
			(*Tangents)[CurrentPosition].TangentX = InTangent;
		}
	}
	virtual void SetColor(int32 VertexIndex, const FColor& InColor) override
	{
		Seek(VertexIndex);
		if (Colors)
		{
			if (CurrentPosition >= Colors->Num())
			{
				Colors->SetNumZeroed(CurrentPosition + 1, false);
			}
			(*Colors)[CurrentPosition] = InColor;
		}
	}
	virtual void SetUV(int32 VertexIndex, int32 Index, const FVector2D& InUV)
	{
		Seek(VertexIndex);
		switch (Index)
		{
		case 0:
		{
			if (UV0s)
			{
				if (CurrentPosition >= UV0s->Num())
				{
					UV0s->SetNumZeroed(CurrentPosition + 1, false);
				}
				(*UV0s)[CurrentPosition] = InUV;
			}
		}
		case 1:
		{
			if (UV1s)
			{
				if (CurrentPosition >= UV1s->Num())
				{
					UV1s->SetNumZeroed(CurrentPosition + 1, false);
				}
				(*UV1s)[CurrentPosition] = InUV;
			}
		}
		default:
			return;
		}
	}

	virtual FVector GetPosition() const override
	{
		check(Positions && Positions->Num() > CurrentPosition);
		return (*Positions)[CurrentPosition];
	}
	virtual FVector4 GetNormal() const override
	{
		check(Normals && Normals->Num() > CurrentPosition);
		float W = (Tangents && Tangents->Num() > CurrentPosition) ? ((*Tangents)[CurrentPosition].bFlipTangentY ? -1.0f : 1.0f) : 1.0f;
		return FVector4((*Normals)[CurrentPosition], W);
	}
	virtual FVector GetTangent() const override
	{
		check(Tangents && Tangents->Num() > CurrentPosition);
		return (*Tangents)[CurrentPosition].TangentX;
	}
	virtual FColor GetColor() const override
	{
		check(Colors && Colors->Num() > CurrentPosition);
		return (*Colors)[CurrentPosition];
	}
	virtual FVector2D GetUV(int32 Index) const override
	{
		switch (Index)
		{
		case 0:
			check(UV0s && UV0s->Num() > CurrentPosition);
			return (*UV0s)[CurrentPosition];
		case 1:
			check(UV1s && UV1s->Num() > CurrentPosition);
			return (*UV1s)[CurrentPosition];
		}
		return FVector2D::ZeroVector;
	}

	virtual FVector GetPosition(int32 VertexIndex) const override
	{
		Seek(VertexIndex);
		check(Positions && Positions->Num() > CurrentPosition);
		return (*Positions)[CurrentPosition];
	}
	virtual FVector4 GetNormal(int32 VertexIndex) const override
	{
		Seek(VertexIndex);
		check(Normals && Normals->Num() > CurrentPosition);
		float W = (Tangents && Tangents->Num() > CurrentPosition) ? ((*Tangents)[CurrentPosition].bFlipTangentY ? -1.0f : 1.0f) : 1.0f;
		return FVector4((*Normals)[CurrentPosition], W);
	}
	virtual FVector GetTangent(int32 VertexIndex) const override
	{
		Seek(VertexIndex);
		check(Tangents && Tangents->Num() > CurrentPosition);
		return (*Tangents)[CurrentPosition].TangentX;
	}
	virtual FColor GetColor(int32 VertexIndex) const override
	{
		Seek(VertexIndex);
		check(Colors && Colors->Num() > CurrentPosition);
		return (*Colors)[CurrentPosition];
	}
	virtual FVector2D GetUV(int32 VertexIndex, int32 Index) const override
	{
		Seek(VertexIndex);
		switch (Index)
		{
		case 0:
			check(UV0s && UV0s->Num() > CurrentPosition);
			return (*UV0s)[CurrentPosition];
		case 1:
			check(UV1s && UV1s->Num() > CurrentPosition);
			return (*UV1s)[CurrentPosition];
		}
		return FVector2D::ZeroVector;
	}


	virtual int32 Length() const override { return Positions->Num(); }
	virtual void Seek(int32 Position) const override
	{
		const_cast<FRuntimeMeshComponentVerticesBuilder*>(this)->CurrentPosition = Position;
	}
	virtual int32 MoveNext() const override
	{
		return ++const_cast<FRuntimeMeshComponentVerticesBuilder*>(this)->CurrentPosition;
	}
	virtual int32 MoveNextOrAdd() override
	{
		return ++CurrentPosition;
	}

	
	TArray<FVector>* GetPositions() const { return Positions; }
	TArray<FVector>* GetNormals() const { return Normals; }
	TArray<FRuntimeMeshTangent>* GetTangents() const { return Tangents; }
	TArray<FColor>* GetColors() const { return Colors; }
	TArray<FVector2D>* GetUV0s() const { return UV0s; }
	TArray<FVector2D>* GetUV1s() const { return UV1s; }

	virtual void Reset() override
	{
		Positions->Reset();
		Normals->Reset();
		Tangents->Reset();
		Colors->Reset();
		UV0s->Reset();
		UV1s->Reset();
		CurrentPosition = -1;
	}

	virtual IRuntimeMeshVerticesBuilder* Clone(bool bIncludeData = true) const override
	{
		FRuntimeMeshComponentVerticesBuilder* NewBuilder = new FRuntimeMeshComponentVerticesBuilder(Normals != nullptr, Tangents != nullptr, Colors != nullptr, UV0s != nullptr, UV1s != nullptr);

		if (bIncludeData)
		{
			*NewBuilder->Positions = *Positions;
			*NewBuilder->Normals = *Normals;
			*NewBuilder->Tangents = *Tangents;
			*NewBuilder->Colors = *Colors;
			*NewBuilder->UV0s = *UV0s;
			*NewBuilder->UV1s = *UV1s;
			NewBuilder->Seek(0);
		}

		return NewBuilder;
	}
};



class FRuntimeMeshIndicesBuilder
{
	bool bOwnsIndexArray;
	TArray<int32>* Indices;
	int32 CurrentPosition;
public:

	FRuntimeMeshIndicesBuilder()
		: bOwnsIndexArray(true), Indices(new TArray<int32>()), CurrentPosition(0)
	{ }
	FRuntimeMeshIndicesBuilder(TArray<int32>* InVertices)
		: bOwnsIndexArray(false), Indices(InVertices), CurrentPosition(0)
	{ }
	FRuntimeMeshIndicesBuilder(const FRuntimeMeshIndicesBuilder& Other) = delete;
	FRuntimeMeshIndicesBuilder& operator=(const FRuntimeMeshIndicesBuilder& Other) = delete;
	virtual ~FRuntimeMeshIndicesBuilder()
	{
		if (bOwnsIndexArray)
		{
			delete Indices;
		}
	}

	virtual ERuntimeMeshVerticesBuilderType GetBuilderType() const { return ERuntimeMeshVerticesBuilderType::Component; }


	void AddTriangle(int32 Index0, int32 Index1, int32 Index2)
	{
		if ((CurrentPosition + 3) >= Indices->Num())
		{
			Indices->SetNum(CurrentPosition + 3);
		}

		(*Indices)[CurrentPosition++] = Index0;
		(*Indices)[CurrentPosition++] = Index1;
		(*Indices)[CurrentPosition++] = Index2;
	}

	void AddIndex(int32 Index)
	{
		if ((CurrentPosition + 1) >= Indices->Num())
		{
			Indices->SetNum(CurrentPosition + 1);
		}
		(*Indices)[CurrentPosition++] = Index;
	}
	int32 ReadOne() const
	{
		return (*Indices)[const_cast<FRuntimeMeshIndicesBuilder*>(this)->CurrentPosition++];
	}

	int32 GetIndex(int32 Position) const
	{
		const_cast<FRuntimeMeshIndicesBuilder*>(this)->CurrentPosition = Position;
		return (*Indices)[CurrentPosition];
	}

	int32 TriangleLength() const { return Length() / 3; }
	int32 Length() const { return Indices->Num(); }

	bool HasRemaining() const { return CurrentPosition < Indices->Num(); }
	
	void Seek(int32 Position) const
	{
		const_cast<FRuntimeMeshIndicesBuilder*>(this)->CurrentPosition = Position;
	}
	void SeekEnd() const
	{
		Seek(Length());
	}
	void Reset(int32 NewSize = 0)
	{
		Indices->Reset(NewSize);
		CurrentPosition = 0;
	}
	void SetNum(int32 NewSize)
	{
		Indices->SetNum(NewSize);
	}
	
	FRuntimeMeshIndicesBuilder* Clone(bool bIncludeData = true) const
	{
		FRuntimeMeshIndicesBuilder* NewBuilder = new FRuntimeMeshIndicesBuilder();

		if (bIncludeData)
		{
			*NewBuilder->Indices = *Indices;
		}
		
		return NewBuilder;
	}

	TArray<int32>* GetIndices()
	{
		return Indices;
	}
};
