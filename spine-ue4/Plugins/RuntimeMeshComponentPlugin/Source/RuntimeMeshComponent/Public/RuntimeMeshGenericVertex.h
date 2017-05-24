// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "RuntimeMeshCore.h"

//////////////////////////////////////////////////////////////////////////
//	
//	This file contains a generic vertex structure capable of efficiently representing a vertex 
//	with any combination of position, normal, tangent, color, and 0-8 uv channels.
//
//	To get around an issue with MSVC and potentially other compilers not performing
//	empty base class optimizations (EBO) to children in multiple inheritance, 
//	this vertex is built via a tree of inheritance using partial specializations.
//
//	At each tier of this tree partial specialization will choose which components 
//	we need to add in, thereby removing entire inherited classes when we don't need them.
//
//  Structure:
//											  FRuntimeMeshVertex
//											/					\
//	FRuntimeMeshPositionNormalTangentComponentCombiner			FRuntimeMeshColorUVComponentCombiner
//					/					\								/					\
//	FRuntimeMeshPositionComponent		 \					FRuntimeMeshColorComponent		 \
//										  \													  \
//						FRuntimeMeshNormalTangentComponents							FRuntimeMeshUVComponents
//
//
//
//
//
//	Example use: (This defines a vertex with all components and 1 UV with default precision for normal/tangent and UV)
//
//	using MyVertex = FRuntimeMeshVertex<true, true, true, true, 1, ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType::Default>;
//
//	MyVertex Vertex;
//	Vertex.Position = FVector(0,0,0);
//	Vertex.Normal = FVector(0,0,0);
//	Vertex.UV0 = FVector2D(0,0);
//
//
//////////////////////////////////////////////////////////////////////////


template<int32 TextureChannels, bool HalfPrecisionUVs, bool HasPositionComponent>
RuntimeMeshVertexStructure CreateVertexStructure(const FVertexBuffer& VertexBuffer);


//////////////////////////////////////////////////////////////////////////
// Texture Component Type Selector
//////////////////////////////////////////////////////////////////////////

enum class ERuntimeMeshVertexUVType
{
	Default = 1,
	HighPrecision = 2,
};

template<ERuntimeMeshVertexUVType UVType>
struct FRuntimeMeshVertexUVsTypeSelector;

template<>
struct FRuntimeMeshVertexUVsTypeSelector<ERuntimeMeshVertexUVType::Default>
{
	typedef FVector2DHalf UVsType;
	static const EVertexElementType VertexElementType1Channel = VET_Half2;
	static const EVertexElementType VertexElementType2Channel = VET_Half4;

};

template<>
struct FRuntimeMeshVertexUVsTypeSelector<ERuntimeMeshVertexUVType::HighPrecision>
{
	typedef FVector2D UVsType;
	static const EVertexElementType VertexElementType1Channel = VET_Float2;
	static const EVertexElementType VertexElementType2Channel = VET_Float4;
};

//////////////////////////////////////////////////////////////////////////
// Texture Component
//////////////////////////////////////////////////////////////////////////

/* Defines the UV coordinates for a vertex (Defaulted to 0 channels) */
template<int32 TextureChannels, typename UVType> struct FRuntimeMeshUVComponents
{
	static_assert(TextureChannels >= 0 && TextureChannels <= 8, "You must have between 0 and 8 (inclusive) UV channels");
};

/* Defines the UV coordinates for a vertex (Specialized to 0 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<0, UVType>
{
	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 1 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<1, UVType>
{
	UVType UV0;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) : UV0(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) : UV0(InUV0) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 2 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<2, UVType>
{
	UVType UV0;
	UVType UV1;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) : UV0(0, 0), UV1(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) : UV0(InUV0), UV1(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) : UV0(InUV0), UV1(InUV1) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 3 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<3, UVType>
{
	UVType UV0;
	UVType UV1;
	UVType UV2;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) :
		UV0(0, 0), UV1(0, 0), UV2(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) :
		UV0(InUV0), UV1(0, 0), UV2(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) :
		UV0(InUV0), UV1(InUV1), UV2(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 4 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<4, UVType>
{
	UVType UV0;
	UVType UV1;
	UVType UV2;
	UVType UV3;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) :
		UV0(0, 0), UV1(0, 0), UV2(0, 0), UV3(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) :
		UV0(InUV0), UV1(0, 0), UV2(0, 0), UV3(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) :
		UV0(InUV0), UV1(InUV1), UV2(0, 0), UV3(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 5 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<5, UVType>
{
	UVType UV0;
	UVType UV1;
	UVType UV2;
	UVType UV3;
	UVType UV4;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) :
		UV0(0, 0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) :
		UV0(InUV0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) :
		UV0(InUV0), UV1(InUV1), UV2(0, 0), UV3(0, 0), UV4(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(0, 0), UV4(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4) : UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 6 channels, Half Precision) */
template<typename UVType> struct FRuntimeMeshUVComponents<6, UVType>
{
	UVType UV0;
	UVType UV1;
	UVType UV2;
	UVType UV3;
	UVType UV4;
	UVType UV5;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) :
		UV0(0, 0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) :
		UV0(InUV0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) :
		UV0(InUV0), UV1(InUV1), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(0, 0), UV4(0, 0), UV5(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(0, 0), UV5(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4, const FVector2D& InUV5) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(InUV5) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 7 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<7, UVType>
{
	UVType UV0;
	UVType UV1;
	UVType UV2;
	UVType UV3;
	UVType UV4;
	UVType UV5;
	UVType UV6;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) :
		UV0(0, 0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) :
		UV0(InUV0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) :
		UV0(InUV0), UV1(InUV1), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(0, 0), UV5(0, 0), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(0, 0), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4, const FVector2D& InUV5) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(InUV5), UV6(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4, const FVector2D& InUV5, const FVector2D& InUV6) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(InUV5), UV6(InUV6) { }
};

/* Defines the UV coordinates for a vertex (Specialized to 8 channels) */
template<typename UVType> struct FRuntimeMeshUVComponents<8, UVType>
{
	UVType UV0;
	UVType UV1;
	UVType UV2;
	UVType UV3;
	UVType UV4;
	UVType UV5;
	UVType UV6;
	UVType UV7;

	FRuntimeMeshUVComponents() { }
	FRuntimeMeshUVComponents(EForceInit) :
		UV0(0, 0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0) :
		UV0(InUV0), UV1(0, 0), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1) :
		UV0(InUV0), UV1(InUV1), UV2(0, 0), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(0, 0), UV4(0, 0), UV5(0, 0), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(0, 0), UV5(0, 0), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(0, 0), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4, const FVector2D& InUV5) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(InUV5), UV6(0, 0), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4, const FVector2D& InUV5, const FVector2D& InUV6) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(InUV5), UV6(InUV6), UV7(0, 0) { }
	FRuntimeMeshUVComponents(const FVector2D& InUV0, const FVector2D& InUV1, const FVector2D& InUV2, const FVector2D& InUV3,
		const FVector2D& InUV4, const FVector2D& InUV5, const FVector2D& InUV6, const FVector2D& InUV7) :
		UV0(InUV0), UV1(InUV1), UV2(InUV2), UV3(InUV3), UV4(InUV4), UV5(InUV5), UV6(InUV6), UV7(InUV7) { }
};


//////////////////////////////////////////////////////////////////////////
// Tangent Basis Component Type Selector
//////////////////////////////////////////////////////////////////////////

enum class ERuntimeMeshVertexTangentBasisType
{
	Default = 1,
	HighPrecision = 2,
};

template<ERuntimeMeshVertexTangentBasisType TangentBasisType>
struct FRuntimeMeshVertexTangentTypeSelector;

template<>
struct FRuntimeMeshVertexTangentTypeSelector<ERuntimeMeshVertexTangentBasisType::Default>
{
	typedef FPackedNormal TangentType;
	static const EVertexElementType VertexElementType = VET_PackedNormal;
};

template<>
struct FRuntimeMeshVertexTangentTypeSelector<ERuntimeMeshVertexTangentBasisType::HighPrecision>
{
	typedef FPackedRGBA16N TangentType;
	static const EVertexElementType VertexElementType = VET_UShort4N;
};


//////////////////////////////////////////////////////////////////////////
// Tangent Basis Components
//////////////////////////////////////////////////////////////////////////

template<bool WantsNormal, bool WantsTangent, typename TangentType>
struct FRuntimeMeshNormalTangentComponents;

struct RuntimeMeshNormalUtil
{
	static void SetNormalW(FPackedNormal& Target, float Determinant)
	{
		Target.Vector.W = Determinant < 0.0f ? 0 : 255;
	}

	static void SetNormalW(FPackedRGBA16N& Target, float Determinant)
	{
		Target.W = Determinant < 0.0f ? 0 : 65535;
	}
};

template<typename TangentType>
struct FRuntimeMeshNormalTangentComponents<true, false, TangentType>
{
	TangentType Normal;

	FRuntimeMeshNormalTangentComponents() { }
	FRuntimeMeshNormalTangentComponents(EForceInit) : Normal(FVector4(0.0f, 0.0f, 1.0f, 1.0f)) { }

	void SetNormalAndTangent(const FVector& InNormal, const FRuntimeMeshTangent& InTangent)
	{
		Normal = InNormal;

		InTangent.AdjustNormal(Normal);
	}

	void SetNormalAndTangent(const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ)
	{
		Normal = InTangentZ;

		// store determinant of basis in w component of normal vector
		RuntimeMeshNormalUtil::SetNormalW(Normal, GetBasisDeterminantSign(InTangentX, InTangentY, InTangentZ));
	}
};

template<typename TangentType>
struct FRuntimeMeshNormalTangentComponents<false, true, TangentType>
{
	TangentType Tangent;

	FRuntimeMeshNormalTangentComponents() { }
	FRuntimeMeshNormalTangentComponents(EForceInit) : Tangent(FVector(1.0f, 0.0f, 0.0f)) { }

	void SetNormalAndTangent(const FVector& InNormal, const FRuntimeMeshTangent& InTangent)
	{
		Tangent = InTangent.TangentX;
	}

	void SetNormalAndTangent(const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ)
	{
		Tangent = InTangentX;
	}
};

template<typename TangentType>
struct FRuntimeMeshNormalTangentComponents<true, true, TangentType>
{
	TangentType Normal;
	TangentType Tangent;

	FRuntimeMeshNormalTangentComponents() { }
	FRuntimeMeshNormalTangentComponents(EForceInit) : Normal(FVector4(0.0f, 0.0f, 1.0f, 1.0f)), Tangent(FVector(1.0f, 0.0f, 0.0f)) { }

	void SetNormalAndTangent(const FVector& InNormal, const FRuntimeMeshTangent& InTangent)
	{
		Normal = InNormal;
		Tangent = InTangent.TangentX;

		InTangent.AdjustNormal(Normal);
	}
	
	void SetNormalAndTangent(const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ)
	{
		Normal = InTangentZ;
		Tangent = InTangentX;

		// store determinant of basis in w component of normal vector
		RuntimeMeshNormalUtil::SetNormalW(Normal, GetBasisDeterminantSign(InTangentX, InTangentY, InTangentZ));
	}
};



//////////////////////////////////////////////////////////////////////////
// Position Component
//////////////////////////////////////////////////////////////////////////

template<bool WantsPosition>
struct FRuntimeMeshPositionComponent;

template<>
struct FRuntimeMeshPositionComponent<true>
{
	FVector Position;

	FRuntimeMeshPositionComponent() { }
	FRuntimeMeshPositionComponent(EForceInit) : Position(0.0f, 0.0f, 0.0f) { }
};



//////////////////////////////////////////////////////////////////////////
// Color Component
//////////////////////////////////////////////////////////////////////////

template<bool WantsColor>
struct FRuntimeMeshColorComponent;

template<>
struct FRuntimeMeshColorComponent<true>
{
	FColor Color;

	FRuntimeMeshColorComponent() { }
	FRuntimeMeshColorComponent(EForceInit) : Color(FColor::White) { }
};




//////////////////////////////////////////////////////////////////////////
// Position Normal Tangent Combiner
//////////////////////////////////////////////////////////////////////////

template<bool WantsPosition, bool WantsNormal, bool WantsTangent, typename TangentBasisType>
struct FRuntimeMeshPositionNormalTangentComponentCombiner :
	public FRuntimeMeshPositionComponent<WantsPosition>,
	public FRuntimeMeshNormalTangentComponents<WantsNormal, WantsTangent, TangentBasisType>
{
	FRuntimeMeshPositionNormalTangentComponentCombiner() { }
	FRuntimeMeshPositionNormalTangentComponentCombiner(EForceInit)
		: FRuntimeMeshPositionComponent<WantsPosition>(EForceInit::ForceInit)
		, FRuntimeMeshNormalTangentComponents<WantsNormal, WantsTangent, TangentBasisType>(EForceInit::ForceInit)
	{ }
};

template<bool WantsPosition, typename TangentBasisType>
struct FRuntimeMeshPositionNormalTangentComponentCombiner<WantsPosition, false, false, TangentBasisType> :
	public FRuntimeMeshPositionComponent<WantsPosition>
{
	FRuntimeMeshPositionNormalTangentComponentCombiner() { }
	FRuntimeMeshPositionNormalTangentComponentCombiner(EForceInit)
		: FRuntimeMeshPositionComponent<WantsPosition>(EForceInit::ForceInit)
	{ }
};

template<bool WantsNormal, bool WantsTangent, typename TangentBasisType>
struct FRuntimeMeshPositionNormalTangentComponentCombiner<false, WantsNormal, WantsTangent, TangentBasisType> :
	public FRuntimeMeshNormalTangentComponents<WantsNormal, WantsTangent, TangentBasisType>
{
	FRuntimeMeshPositionNormalTangentComponentCombiner() { }
	FRuntimeMeshPositionNormalTangentComponentCombiner(EForceInit)
		: FRuntimeMeshNormalTangentComponents<WantsNormal, WantsTangent, TangentBasisType>(EForceInit::ForceInit)
	{ }
};

template<typename TangentBasisType>
struct FRuntimeMeshPositionNormalTangentComponentCombiner<false, false, false, TangentBasisType>;



//////////////////////////////////////////////////////////////////////////
// Color UV Combiner
//////////////////////////////////////////////////////////////////////////

template<bool WantsColor, int32 NumWantedUVChannels, typename UVType>
struct FRuntimeMeshColorUVComponentCombiner :
	public FRuntimeMeshColorComponent<WantsColor>,
	public FRuntimeMeshUVComponents<NumWantedUVChannels, UVType>
{
	FRuntimeMeshColorUVComponentCombiner() { }
	FRuntimeMeshColorUVComponentCombiner(EForceInit)
		: FRuntimeMeshColorComponent<WantsColor>(EForceInit::ForceInit)
		, FRuntimeMeshUVComponents<NumWantedUVChannels, UVType>(EForceInit::ForceInit)
	{ }
};

template<int32 NumWantedUVChannels, typename UVType>
struct FRuntimeMeshColorUVComponentCombiner<false, NumWantedUVChannels, UVType> :
	public FRuntimeMeshUVComponents<NumWantedUVChannels, UVType>
{
	FRuntimeMeshColorUVComponentCombiner() { }
	FRuntimeMeshColorUVComponentCombiner(EForceInit)
		: FRuntimeMeshUVComponents<NumWantedUVChannels, UVType>(EForceInit::ForceInit)
	{ }
};

template<bool WantsColor, typename UVType>
struct FRuntimeMeshColorUVComponentCombiner<WantsColor, 0, UVType> :
	public FRuntimeMeshColorComponent<WantsColor>
{
	FRuntimeMeshColorUVComponentCombiner() { }
	FRuntimeMeshColorUVComponentCombiner(EForceInit)
		: FRuntimeMeshColorComponent<WantsColor>(EForceInit::ForceInit)
	{ }
};

template<typename UVType>
struct FRuntimeMeshColorUVComponentCombiner<false, 0, UVType>;







//////////////////////////////////////////////////////////////////////////
// Template Vertex Type Info Structure
//////////////////////////////////////////////////////////////////////////

template<bool WantsPosition, bool WantsNormal, bool WantsTangent, bool WantsColor, int32 NumWantedUVChannels,
	ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
struct FRuntimeMeshVertexTypeInfo_GenericVertex : public FRuntimeMeshVertexTypeInfo
{
	FRuntimeMeshVertexTypeInfo_GenericVertex(FString VertexName) :
		FRuntimeMeshVertexTypeInfo(
			FString::Printf(TEXT("RuntimeMeshVertex<%d, %d, %d, %d, %d, %d, %d>"), WantsPosition, WantsNormal, WantsTangent, WantsColor, NumWantedUVChannels, (int32)NormalTangentType, (int32)UVType),
			GetVertexGuid(VertexName)) { }

	static FGuid GetVertexGuid(FString VertexName)
	{
		uint32 TypeID = 0;
		TypeID = (TypeID << 1) | (WantsPosition ? 1 : 0);
		TypeID = (TypeID << 1) | (WantsNormal ? 1 : 0);
		TypeID = (TypeID << 1) | (WantsTangent ? 1 : 0);
		TypeID = (TypeID << 3) | (uint32)NormalTangentType;
		TypeID = (TypeID << 1) | (WantsColor ? 1 : 0);
		TypeID = (TypeID << 6) | (NumWantedUVChannels & 0xFF);
		TypeID = (TypeID << 3) | (uint32)UVType;

		FGuid Guid = FGuid(0x00FFEB44, 0x31094597, /*0x93918032*/  GetTypeHash(VertexName), (0x78C3 << 16) | TypeID);
		return Guid;
	}
};

//////////////////////////////////////////////////////////////////////////
// Macros to create a custom vertex type based on the generic vertex and implement some common constructors
//////////////////////////////////////////////////////////////////////////



#define RUNTIMEMESH_VERTEX_DEFAULTINIT_POSITION_true Position = FVector(0.0f, 0.0f, 0.0f);
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_POSITION_false 
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_POSITION(HasPosition) RUNTIMEMESH_VERTEX_DEFAULTINIT_POSITION_##HasPosition

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_NORMAL_true Normal = FVector4(0.0f, 0.0f, 1.0f, 1.0f);
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_NORMAL_false 
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_NORMAL(HasNormal) RUNTIMEMESH_VERTEX_DEFAULTINIT_NORMAL_##HasNormal

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT_true Tangent = FVector(1.0f, 0.0f, 0.0f);
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT_false 
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT(HasTangent) RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT_##HasTangent

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR_true Color = FColor::White;
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR_false 
#define RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR(HasColor) RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR_##HasColor

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_0

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_1 \
	UV0 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_2 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_1 \
	UV1 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_3 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_2 \
	UV2 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_4 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_3 \
	UV3 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_5 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_4 \
	UV4 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_6 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_5 \
	UV5 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_7 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_6 \
	UV6 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_8 \
	RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_7 \
	UV7 = FVector2D(0.0f, 0.0f);

#define RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNELS(NumChannels) RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNEL_##NumChannels




#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_0
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_1 , const FVector2D& InUV0 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_2 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_1 , const FVector2D& InUV1 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_3 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_2 , const FVector2D& InUV2 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_4 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_3 , const FVector2D& InUV3 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_5 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_4 , const FVector2D& InUV4 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_6 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_5 , const FVector2D& InUV5 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_7 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_6 , const FVector2D& InUV6 = FVector2D::ZeroVector
#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_8 RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_7 , const FVector2D& InUV7 = FVector2D::ZeroVector

#define RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNELS(NumChannels) RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNEL_##NumChannels




#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_0

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_1 \
	UV0 = InUV0;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_2 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_1 \
	UV1 = InUV1;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_3 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_2 \
	UV2 = InUV2;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_4 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_3 \
	UV3 = InUV3;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_5 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_4 \
	UV4 = InUV4;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_6 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_5 \
	UV5 = InUV5;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_7 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_6 \
	UV6 = InUV6;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_8 \
	RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_7 \
	UV7 = InUV7;

#define RUNTIMEMESH_VERTEX_INIT_UVCHANNELS(NumChannels) RUNTIMEMESH_VERTEX_INIT_UVCHANNEL_##NumChannels



#define RUNTIMEMESH_VERTEX_PARAMETER_POSITION_true const FVector& InPosition, 
#define RUNTIMEMESH_VERTEX_PARAMETER_POSITION_false
#define RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) RUNTIMEMESH_VERTEX_PARAMETER_POSITION_##NeedsPosition

#define RUNTIMEMESH_VERTEX_INIT_POSITION_true Position = InPosition;
#define RUNTIMEMESH_VERTEX_INIT_POSITION_false
#define RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition) RUNTIMEMESH_VERTEX_INIT_POSITION_##NeedsPosition


// PreProcessor IF with pass through for all the constructor arguments
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, Condition, IfTrue) \
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF_##Condition(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, IfTrue)
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF_false(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, IfTrue)
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF_true(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, IfTrue) IfTrue


// Implementation of Position only Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(const FVector& InPosition)							\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_NORMAL(NeedsNormal)			\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT(NeedsTangent)		\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR(NeedsColor)			\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNELS(UVChannelCount)	\
	}

// Defines the Position Constuctor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)								\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsPosition,				\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)				\
	)

// Implementation of Position/Normal Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) const FVector& InNormal)	\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		Normal = InNormal;											\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT(NeedsTangent)		\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR(NeedsColor)			\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNELS(UVChannelCount)	\
	}

// Defines the Position/Normal Constuctor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)						\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsNormal,				\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)		\
	)

// Implementation of Position/Color Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_COLOR_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) const FColor& InColor)	\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_NORMAL(NeedsNormal)			\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_TANGENT(NeedsTangent)		\
		Color = InColor;											\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_UVCHANNELS(UVChannelCount)	\
	}

// Defines the Position/Color Constructor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_COLOR(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)						\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsColor,				\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_COLOR_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)		\
	)








// Implementation of Position/Normal/Tangent Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) const FVector& InNormal, const FRuntimeMeshTangent& InTangent RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNELS(UVChannelCount))	\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		Normal = InNormal;											\
		Tangent = InTangent.TangentX;								\
		InTangent.AdjustNormal(Normal);								\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR(NeedsColor)			\
		RUNTIMEMESH_VERTEX_INIT_UVCHANNELS(UVChannelCount)			\
	}

// Defines the Position/Normal/Tangent Constructor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)						\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsNormal,						\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsTangent,					\
			RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)	\
		)																																															\
	)


// Implementation of Position/TangentX/TangentY/TangentZ Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNELS(UVChannelCount))	\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		SetNormalAndTangent(InTangentX, InTangentY, InTangentZ);	\
		RUNTIMEMESH_VERTEX_DEFAULTINIT_COLOR(NeedsColor)			\
		RUNTIMEMESH_VERTEX_INIT_UVCHANNELS(UVChannelCount)			\
	}

// Defines the Position/TangentX/TangentY/TangentZ Constructor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)						\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsNormal,									\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsTangent,								\
			RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)	\
		)																																																		\
	)







// Implementation of Position/Normal/Tangent Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT_COLOR_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) const FVector& InNormal, const FRuntimeMeshTangent& InTangent, const FColor& InColor RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNELS(UVChannelCount))	\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		Normal = InNormal;											\
		Tangent = InTangent.TangentX;								\
		InTangent.AdjustNormal(Normal);								\
		Color = InColor;											\
		RUNTIMEMESH_VERTEX_INIT_UVCHANNELS(UVChannelCount)			\
	}

// Defines the Position/Normal/Tangent Constructor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT_COLOR(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)									\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsNormal,									\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsTangent,								\
			RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsColor,								\
				RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT_COLOR_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)		\
			)																																																		\
		)																																																			\
	)


// Implementation of Position/TangentX/TangentY/TangentZ Constructor
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ_COLOR_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType) \
	VertexName(RUNTIMEMESH_VERTEX_PARAMETER_POSITION(NeedsPosition) const FVector& InTangentX, const FVector& InTangentY, const FVector& InTangentZ, const FColor& InColor RUNTIMEMESH_VERTEX_PARAMETER_UVCHANNELS(UVChannelCount))	\
	{																\
		RUNTIMEMESH_VERTEX_INIT_POSITION(NeedsPosition)				\
		SetNormalAndTangent(InTangentX, InTangentY, InTangentZ);	\
		Color = InColor;											\
		RUNTIMEMESH_VERTEX_INIT_UVCHANNELS(UVChannelCount)			\
	}

// Defines the Position/TangentX/TangentY/TangentZ Constructor if it's wanted
#define RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ_COLOR(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)								\
	RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsNormal,											\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsTangent,										\
			RUNTIMEMESH_VERTEX_CONSTRUCTOR_DEFINITION_IF(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, NeedsColor,										\
				RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ_COLOR_IMPLEMENTATION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)	\
			)																																																				\
		)																																																					\
	)


#define RUNTIMEMESH_VERTEX_SERIALIZATION_POSITION_true Ar << V.Position;
#define RUNTIMEMESH_VERTEX_SERIALIZATION_POSITION_false
#define RUNTIMEMESH_VERTEX_SERIALIZATION_POSITION(NeedsPosition) RUNTIMEMESH_VERTEX_SERIALIZATION_POSITION_##NeedsPosition

#define RUNTIMEMESH_VERTEX_SERIALIZATION_NORMAL_true Ar << V.Normal;
#define RUNTIMEMESH_VERTEX_SERIALIZATION_NORMAL_false
#define RUNTIMEMESH_VERTEX_SERIALIZATION_NORMAL(NeedsNormal) RUNTIMEMESH_VERTEX_SERIALIZATION_NORMAL_##NeedsNormal

#define RUNTIMEMESH_VERTEX_SERIALIZATION_TANGENT_true Ar << V.Tangent;
#define RUNTIMEMESH_VERTEX_SERIALIZATION_TANGENT_false
#define RUNTIMEMESH_VERTEX_SERIALIZATION_TANGENT(NeedsTangent) RUNTIMEMESH_VERTEX_SERIALIZATION_TANGENT_##NeedsTangent

#define RUNTIMEMESH_VERTEX_SERIALIZATION_COLOR_true Ar << V.Color;
#define RUNTIMEMESH_VERTEX_SERIALIZATION_COLOR_false
#define RUNTIMEMESH_VERTEX_SERIALIZATION_COLOR(NeedsColor) RUNTIMEMESH_VERTEX_SERIALIZATION_COLOR_##NeedsColor

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_0

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_1 \
	Ar << V.UV0;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_2 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_1 \
	Ar << V.UV1;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_3 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_2 \
	Ar << V.UV2;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_4 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_3 \
	Ar << V.UV3;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_5 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_4 \
	Ar << V.UV4;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_6 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_5 \
	Ar << V.UV5;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_7 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_6 \
	Ar << V.UV6;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_8 \
	RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_7 \
	Ar << V.UV7;

#define RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNELS(NumChannels) RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNEL_##NumChannels

#define RUNTIMEMESH_VERTEX_SERIALIZER(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount)	\
	friend FArchive& operator<<(FArchive& Ar, VertexName & V)		\
	{																\
		RUNTIMEMESH_VERTEX_SERIALIZATION_POSITION(NeedsPosition)	\
		RUNTIMEMESH_VERTEX_SERIALIZATION_NORMAL(NeedsNormal)		\
		RUNTIMEMESH_VERTEX_SERIALIZATION_TANGENT(NeedsTangent)		\
		RUNTIMEMESH_VERTEX_SERIALIZATION_COLOR(NeedsColor)			\
		RUNTIMEMESH_VERTEX_SERIALIZATION_UVCHANNELS(UVChannelCount)	\
		return Ar;													\
	}



#define DECLARE_RUNTIME_MESH_VERTEXINTERNAL(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, APIQUALIFIER)							\
	struct APIQUALIFIER FRuntimeMeshVertexTypeInfo_##VertexName																															\
		: public FRuntimeMeshVertexTypeInfo_GenericVertex<NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType>										\
	{																																																\
		FRuntimeMeshVertexTypeInfo_##VertexName()																																					\
			: FRuntimeMeshVertexTypeInfo_GenericVertex<NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType>(TEXT("")) { }								\
																																																	\
		virtual class FRuntimeMeshSectionInterface* CreateSection(bool bInNeedsPositionOnlyBuffer) const override;																					\
	};																																																\
	struct APIQUALIFIER VertexName : public FRuntimeMeshVertex<NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType>						\
	{																																																\
		static const FRuntimeMeshVertexTypeInfo_##VertexName TypeInfo;																																\
																																																	\
		typedef FRuntimeMeshVertex<NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType> Super;														\
																																																	\
		VertexName() { }																																											\
																																																	\
		VertexName(EForceInit) : Super(EForceInit::ForceInit) { }																																	\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)										\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)								\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_COLOR(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)								\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)						\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)			\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_NORMAL_TANGENT_COLOR(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)					\
																																																	\
		RUNTIMEMESH_VERTEX_CONSTRUCTOR_POSITION_TANGENTX_TANGENTY_TANGENTZ_COLOR(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)		\
																																																	\
		RUNTIMEMESH_VERTEX_SERIALIZER(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount)																				\
	};		

#define DECLARE_RUNTIME_MESH_VERTEX(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType)		\
	DECLARE_RUNTIME_MESH_VERTEXINTERNAL(VertexName, NeedsPosition, NeedsNormal, NeedsTangent, NeedsColor, UVChannelCount, TangentsType, UVChannelType, /**/)

/* Used only for the generic vertex to create the type registration */
#define DEFINE_RUNTIME_MESH_VERTEX(VertexName)																																						\
	const FRuntimeMeshVertexTypeInfo_##VertexName VertexName::TypeInfo;																																\
	FRuntimeMeshVertexTypeRegistration< VertexName > FRuntimeMeshVertexTypeInfoRegistration_##VertexName;																											\
	FRuntimeMeshSectionInterface* FRuntimeMeshVertexTypeInfo_##VertexName::CreateSection(bool bInNeedsPositionOnlyBuffer) const																		\
	{																																																\
		return new FRuntimeMeshSection< VertexName >(bInNeedsPositionOnlyBuffer);																													\
	}

//////////////////////////////////////////////////////////////////////////
// Template Vertex
//////////////////////////////////////////////////////////////////////////

// This version uses both sub combiners since there's at least 1 thing we need from both.
template<bool WantsPosition, bool WantsNormal, bool WantsTangent, bool WantsColor, int32 NumWantedUVChannels,
ERuntimeMeshVertexTangentBasisType NormalTangentType = ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType UVType = ERuntimeMeshVertexUVType::Default>
struct FRuntimeMeshVertex :
	public FRuntimeMeshPositionNormalTangentComponentCombiner<WantsPosition, WantsNormal, WantsTangent, typename FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::TangentType>,
	public FRuntimeMeshColorUVComponentCombiner<WantsColor, NumWantedUVChannels, typename FRuntimeMeshVertexUVsTypeSelector<UVType>::UVsType>
{
    // Make sure something is enabled
    static_assert((WantsPosition || WantsNormal || WantsTangent || WantsColor || NumWantedUVChannels > 0), "Invalid configuration... You must have at least 1 component enabled.");
        
    // Get vertex structure
    static RuntimeMeshVertexStructure GetVertexStructure(const FVertexBuffer& VertexBuffer);
    
    FRuntimeMeshVertex() { }
    FRuntimeMeshVertex(EForceInit)
    : FRuntimeMeshPositionNormalTangentComponentCombiner<WantsPosition, WantsNormal, WantsTangent, typename FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::TangentType>(EForceInit::ForceInit)
    , FRuntimeMeshColorUVComponentCombiner<WantsColor, NumWantedUVChannels, typename FRuntimeMeshVertexUVsTypeSelector<UVType>::UVsType>(EForceInit::ForceInit)
    { }
};

// This version only uses the position/normal/tangent combiner as we don't need anything from the other
template<bool WantsPosition, bool WantsNormal, bool WantsTangent, ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
struct FRuntimeMeshVertex<WantsPosition, WantsNormal, WantsTangent, false, 0, NormalTangentType, UVType> :
	public FRuntimeMeshPositionNormalTangentComponentCombiner<WantsPosition, WantsNormal, WantsTangent, typename FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::TangentType>
{
    // Get vertex structure
    static RuntimeMeshVertexStructure GetVertexStructure(const FVertexBuffer& VertexBuffer);
    
    FRuntimeMeshVertex() { }
    FRuntimeMeshVertex(EForceInit)
    : FRuntimeMeshPositionNormalTangentComponentCombiner<WantsPosition, WantsNormal, WantsTangent, typename FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::TangentType>(EForceInit::ForceInit)
    { }
};

// This version only uses the color/uv combiner as we don't need anything from the other
template<bool WantsColor, int32 NumWantedUVChannels, ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
struct FRuntimeMeshVertex<false, false, false, WantsColor, NumWantedUVChannels, NormalTangentType, UVType> :
	public FRuntimeMeshColorUVComponentCombiner<WantsColor, NumWantedUVChannels, typename FRuntimeMeshVertexUVsTypeSelector<UVType>::UVsType>
{
    // Get vertex structure
    static RuntimeMeshVertexStructure GetVertexStructure(const FVertexBuffer& VertexBuffer);
    
    FRuntimeMeshVertex() { }
    FRuntimeMeshVertex(EForceInit)
    : FRuntimeMeshColorUVComponentCombiner<WantsColor, NumWantedUVChannels, typename FRuntimeMeshVertexUVsTypeSelector<UVType>::UVsType>(EForceInit::ForceInit)
    { }
};



//////////////////////////////////////////////////////////////////////////
// Vertex Structure Generator
//////////////////////////////////////////////////////////////////////////

struct FRuntimeMeshVertexUtilities
{
    //////////////////////////////////////////////////////////////////////////
    // Position Component
    //////////////////////////////////////////////////////////////////////////
    template<typename RuntimeVertexType, bool WantsPosition>
    struct FRuntimeMeshPositionComponentUtilities
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.PositionComponent = RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, Position, VET_Float3);
        }

    };
    
    template<typename RuntimeVertexType>
    struct FRuntimeMeshPositionComponentUtilities<RuntimeVertexType, false>
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
        }
    };
    
    //////////////////////////////////////////////////////////////////////////
    // Normal/Tangent Components
    //////////////////////////////////////////////////////////////////////////
    template<typename RuntimeVertexType, bool WantsNormal, bool WantsTangent, ERuntimeMeshVertexTangentBasisType NormalTangentType>
    struct FRuntimeMeshNormalTangentComponentVertexStructure
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TangentBasisComponents[1] = RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, Normal,
                                                                                    FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::VertexElementType);
            VertexStructure.TangentBasisComponents[0] = RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, Tangent,
                                                                                    FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::VertexElementType);
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexTangentBasisType NormalTangentType>
    struct FRuntimeMeshNormalTangentComponentVertexStructure<RuntimeVertexType, true, false, NormalTangentType>
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TangentBasisComponents[1] = RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, Normal,
                                                                                    FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::VertexElementType);
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexTangentBasisType NormalTangentType>
    struct FRuntimeMeshNormalTangentComponentVertexStructure<RuntimeVertexType, false, true, NormalTangentType>
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TangentBasisComponents[0] = RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, Tangent,
                                                                                    FRuntimeMeshVertexTangentTypeSelector<NormalTangentType>::VertexElementType);
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexTangentBasisType NormalTangentType>
    struct FRuntimeMeshNormalTangentComponentVertexStructure<RuntimeVertexType, false, false, NormalTangentType>
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
        }
    };
    
    //////////////////////////////////////////////////////////////////////////
    // Color Component
    //////////////////////////////////////////////////////////////////////////
    template<typename RuntimeVertexType, bool WantsColor>
    struct FRuntimeMeshColorComponentVertexStructure
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.ColorComponent = RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, Color, VET_Color);
        }
    };
    
    template<typename RuntimeVertexType>
    struct FRuntimeMeshColorComponentVertexStructure<RuntimeVertexType, false>
    {
        static void AddComponent(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
        }
    };
    
    
    //////////////////////////////////////////////////////////////////////////
    // UV Components
    //////////////////////////////////////////////////////////////////////////
    template<typename RuntimeVertexType, int32 NumWantedUVChannels, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 1, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType1Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 2, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 3, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV2, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType1Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 4, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV2, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 5, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV2, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV4, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType1Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 6, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV2, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV4, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 7, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV2, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV4, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV6, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType1Channel));
        }
    };
    
    template<typename RuntimeVertexType, ERuntimeMeshVertexUVType UVType>
    struct FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, 8, UVType>
    {
        static void AddChannels(const FVertexBuffer& VertexBuffer, RuntimeMeshVertexStructure& VertexStructure)
        {
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV0, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV2, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV4, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
            VertexStructure.TextureCoordinates.Add(RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, RuntimeVertexType, UV6, FRuntimeMeshVertexUVsTypeSelector<UVType>::VertexElementType2Channel));
        }
    };
    
    
    
    //////////////////////////////////////////////////////////////////////////
    // Vertex Structure Helper
    //////////////////////////////////////////////////////////////////////////
    template<bool WantsPosition, bool WantsNormal, bool WantsTangent, bool WantsColor, int32 NumWantedUVChannels,
    ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
    static RuntimeMeshVertexStructure CreateVertexStructure(const FVertexBuffer& VertexBuffer)
    {
        typedef FRuntimeMeshVertex<WantsPosition, WantsNormal, WantsTangent, WantsColor, NumWantedUVChannels, NormalTangentType, UVType> RuntimeVertexType;
        
        RuntimeMeshVertexStructure VertexStructure;
        
        // Add Position component if necessary
        FRuntimeMeshPositionComponentUtilities<RuntimeVertexType, WantsPosition>::AddComponent(VertexBuffer, VertexStructure);
        
        // Add normal and tangent components if necessary
        FRuntimeMeshNormalTangentComponentVertexStructure<RuntimeVertexType, WantsNormal, WantsTangent, NormalTangentType>::AddComponent(VertexBuffer, VertexStructure);
        
        // Add color component if necessary
        FRuntimeMeshColorComponentVertexStructure<RuntimeVertexType, WantsColor>::AddComponent(VertexBuffer, VertexStructure);
        
        // Add all texture channels
        FRuntimeMeshTextureChannelsVertexStructure<RuntimeVertexType, NumWantedUVChannels, UVType>::AddChannels(VertexBuffer, VertexStructure);
        
        return VertexStructure;
    }
};












// These need to be declared after FRuntimemeshVertexStructureHelper and RuntimeMeshVertexStructure to fix circular dependencies between the two
template<bool WantsPosition, bool WantsNormal, bool WantsTangent, bool WantsColor, int32 NumWantedUVChannels, ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
RuntimeMeshVertexStructure FRuntimeMeshVertex<WantsPosition, WantsNormal, WantsTangent, WantsColor, NumWantedUVChannels, NormalTangentType, UVType>::GetVertexStructure(const FVertexBuffer& VertexBuffer)
{
    return FRuntimeMeshVertexUtilities::CreateVertexStructure<WantsPosition, WantsNormal, WantsTangent, WantsColor, NumWantedUVChannels, NormalTangentType, UVType>(VertexBuffer);
}

template<bool WantsPosition, bool WantsNormal, bool WantsTangent, ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
RuntimeMeshVertexStructure FRuntimeMeshVertex<WantsPosition, WantsNormal, WantsTangent, false, 0, NormalTangentType, UVType>::GetVertexStructure(const FVertexBuffer& VertexBuffer)
{
    return FRuntimeMeshVertexUtilities::CreateVertexStructure<WantsPosition, WantsNormal, WantsTangent, false, 0, NormalTangentType, UVType>(VertexBuffer);
}

template<bool WantsColor, int32 NumWantedUVChannels, ERuntimeMeshVertexTangentBasisType NormalTangentType, ERuntimeMeshVertexUVType UVType>
RuntimeMeshVertexStructure FRuntimeMeshVertex<false, false, false, WantsColor, NumWantedUVChannels, NormalTangentType, UVType>::GetVertexStructure(const FVertexBuffer& VertexBuffer)
{
    return FRuntimeMeshVertexUtilities::CreateVertexStructure<false, false, false, WantsColor, NumWantedUVChannels, NormalTangentType, UVType>(VertexBuffer);
}



//////////////////////////////////////////////////////////////////////////
// Name Vertex Configurations
//////////////////////////////////////////////////////////////////////////

/** Simple vertex with 1 UV channel */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexSimple, true, true, true, true, 1, ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 2 UV channels */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexDualUV, true, true, true, true, 2, ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 3 UV channels */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexTripleUV, true, true, true, true, 3, ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 1 UV channel and NO position component (Meant to be used with separate position buffer) */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexNoPosition, false, true, true, true, 1, ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 2 UV channels and NO position component (Meant to be used with separate position buffer) */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexNoPositionDualUV, false, true, true, true, 2, ERuntimeMeshVertexTangentBasisType::Default, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 1 UV channel */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexHiPrecisionNormals, true, true, true, true, 1, ERuntimeMeshVertexTangentBasisType::HighPrecision, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 2 UV channels */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexDualUVHiPrecisionNormals, true, true, true, true, 2, ERuntimeMeshVertexTangentBasisType::HighPrecision, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 1 UV channel and NO position component (Meant to be used with separate position buffer) */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexNoPositionHiPrecisionNormals, false, true, true, true, 1, ERuntimeMeshVertexTangentBasisType::HighPrecision, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)

/** Simple vertex with 2 UV channels and NO position component (Meant to be used with separate position buffer) */
DECLARE_RUNTIME_MESH_VERTEXINTERNAL(FRuntimeMeshVertexNoPositionDualUVHiPrecisionNormals, false, true, true, true, 2, ERuntimeMeshVertexTangentBasisType::HighPrecision, ERuntimeMeshVertexUVType::HighPrecision, RUNTIMEMESHCOMPONENT_API)


