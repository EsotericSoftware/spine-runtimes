// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "Engine.h"
#include "Components/MeshComponent.h"
#include "RuntimeMeshProfiling.h"
#include "RuntimeMeshVersion.h"
#include "Runtime/Launch/Resources/Version.h"
#include "RuntimeMeshCore.generated.h"

class FRuntimeMeshVertexFactory;


template<typename T>
struct FRuntimeMeshVertexTraits
{
private:
	template<typename C, C> struct ChT;

	struct FallbackPosition { FVector Position; };
	struct DerivedPosition : T, FallbackPosition { };
	template<typename C> static char(&PositionCheck(ChT<FVector FallbackPosition::*, &C::Position>*))[1];
	template<typename C> static char(&PositionCheck(...))[2];

	struct FallbackNormal { FPackedRGBA16N Normal; };
	struct DerivedNormal : T, FallbackNormal { };
	template<typename C> static char(&NormalCheck(ChT<FPackedRGBA16N FallbackNormal::*, &C::Normal>*))[1];
	template<typename C> static char(&NormalCheck(...))[2];

	struct FallbackTangent { FPackedRGBA16N Tangent; };
	struct DerivedTangent : T, FallbackTangent { };
	template<typename C> static char(&TangentCheck(ChT<FPackedRGBA16N FallbackTangent::*, &C::Tangent>*))[1];
	template<typename C> static char(&TangentCheck(...))[2];

	struct FallbackColor { FColor Color; };
	struct DerivedColor : T, FallbackColor { };
	template<typename C> static char(&ColorCheck(ChT<FColor FallbackColor::*, &C::Color>*))[1];
	template<typename C> static char(&ColorCheck(...))[2];

	struct FallbackUV0 { FVector2D UV0; };
	struct DerivedUV0 : T, FallbackUV0 { };
	template<typename C> static char(&UV0Check(ChT<FVector2D FallbackUV0::*, &C::UV0>*))[1];
	template<typename C> static char(&UV0Check(...))[2];

	struct FallbackUV1 { FVector2D UV1; };
	struct DerivedUV1 : T, FallbackUV1 { };
	template<typename C> static char(&UV1Check(ChT<FVector2D FallbackUV1::*, &C::UV1>*))[1];
	template<typename C> static char(&UV1Check(...))[2];

	struct FallbackUV2 { FVector2D UV2; };
	struct DerivedUV2 : T, FallbackUV2 { };
	template<typename C> static char(&UV2Check(ChT<FVector2D FallbackUV2::*, &C::UV2>*))[1];
	template<typename C> static char(&UV2Check(...))[2];

	struct FallbackUV3 { FVector2D UV3; };
	struct DerivedUV3 : T, FallbackUV3 { };
	template<typename C> static char(&UV3Check(ChT<FVector2D FallbackUV3::*, &C::UV3>*))[1];
	template<typename C> static char(&UV3Check(...))[2];

	struct FallbackUV4 { FVector2D UV4; };
	struct DerivedUV4 : T, FallbackUV4 { };
	template<typename C> static char(&UV4Check(ChT<FVector2D FallbackUV4::*, &C::UV4>*))[1];
	template<typename C> static char(&UV4Check(...))[2];

	struct FallbackUV5 { FVector2D UV5; };
	struct DerivedUV5 : T, FallbackUV5 { };
	template<typename C> static char(&UV5Check(ChT<FVector2D FallbackUV5::*, &C::UV5>*))[1];
	template<typename C> static char(&UV5Check(...))[2];

	struct FallbackUV6 { FVector2D UV6; };
	struct DerivedUV6 : T, FallbackUV6 { };
	template<typename C> static char(&UV6Check(ChT<FVector2D FallbackUV6::*, &C::UV6>*))[1];
	template<typename C> static char(&UV6Check(...))[2];

	struct FallbackUV7 { FVector2D UV7; };
	struct DerivedUV7 : T, FallbackUV7 { };
	template<typename C> static char(&UV7Check(ChT<FVector2D FallbackUV7::*, &C::UV7>*))[1];
	template<typename C> static char(&UV7Check(...))[2];

	template<typename A, typename B>
	struct IsSameType
	{
		static const bool Value = false;
	};

	template<typename A>
	struct IsSameType<A, A>
	{
		static const bool Value = true;
	};

	template<bool HasNormal, bool HasTangent, typename Type>
	struct TangentBasisHighPrecisionDetector
	{
		static const bool Value = false;
	};

	template<typename Type>
	struct TangentBasisHighPrecisionDetector<true, false, Type>
	{
		static const bool Value = IsSameType<decltype(DeclVal<T>().Normal), FPackedRGBA16N>::Value;
	};

	template<bool HasNormal, typename Type>
	struct TangentBasisHighPrecisionDetector<HasNormal, true, Type>
	{
		static const bool Value = IsSameType<decltype(DeclVal<T>().Tangent), FPackedRGBA16N>::Value;
	};

	template<bool HasUV0, typename Type>
	struct UVChannelHighPrecisionDetector
	{
		static const bool Value = false;
	};

	template<typename Type>
	struct UVChannelHighPrecisionDetector<true, Type>
	{
		static const bool Value = IsSameType<decltype(DeclVal<T>().UV0), FVector2D>::Value;
	};
	

public:
	static const bool HasPosition = sizeof(PositionCheck<DerivedPosition>(0)) == 2;
	static const bool HasNormal = sizeof(NormalCheck<DerivedNormal>(0)) == 2;
	static const bool HasTangent = sizeof(TangentCheck<DerivedTangent>(0)) == 2;
	static const bool HasColor = sizeof(ColorCheck<DerivedColor>(0)) == 2;
	static const bool HasUV0 = sizeof(UV0Check<DerivedUV0>(0)) == 2;
	static const bool HasUV1 = sizeof(UV1Check<DerivedUV1>(0)) == 2;
	static const bool HasUV2 = sizeof(UV2Check<DerivedUV2>(0)) == 2;
	static const bool HasUV3 = sizeof(UV3Check<DerivedUV3>(0)) == 2;
	static const bool HasUV4 = sizeof(UV4Check<DerivedUV4>(0)) == 2;
	static const bool HasUV5 = sizeof(UV5Check<DerivedUV5>(0)) == 2;
	static const bool HasUV6 = sizeof(UV6Check<DerivedUV6>(0)) == 2;
	static const bool HasUV7 = sizeof(UV7Check<DerivedUV7>(0)) == 2;
	static const int32 NumUVChannels = 
		(HasUV0 ? 1 : 0) +
		(HasUV1 ? 1 : 0) +
		(HasUV2 ? 1 : 0) +
		(HasUV3 ? 1 : 0) +
		(HasUV4 ? 1 : 0) +
		(HasUV5 ? 1 : 0) +
		(HasUV6 ? 1 : 0) +
		(HasUV7 ? 1 : 0);


	
	static const bool HasHighPrecisionNormals = TangentBasisHighPrecisionDetector<HasNormal, HasTangent, T>::Value;
	static const bool HasHighPrecisionUVs = UVChannelHighPrecisionDetector<HasUV0, T>::Value;
};



#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 12
/** Structure definition of a vertex */
using RuntimeMeshVertexStructure = FLocalVertexFactory::FDataType;
#else
/** Structure definition of a vertex */
using RuntimeMeshVertexStructure = FLocalVertexFactory::DataType;
#endif

#define RUNTIMEMESH_VERTEXCOMPONENT(VertexBuffer, VertexType, Member, MemberType) \
	STRUCTMEMBER_VERTEXSTREAMCOMPONENT(&VertexBuffer, VertexType, Member, MemberType)

/* Update frequency for a section. Used to optimize for update or render speed*/
UENUM(BlueprintType)
enum class EUpdateFrequency : uint8
{
	/* Tries to skip recreating the scene proxy if possible. */
	Average UMETA(DisplayName = "Average"),
	/* Tries to skip recreating the scene proxy if possible and optimizes the buffers for frequent updates. */
	Frequent UMETA(DisplayName = "Frequent"),
	/* If the component is static it will try to use the static rendering path (this will force a recreate of the scene proxy) */
	Infrequent UMETA(DisplayName = "Infrequent")
};

/* Control flags for update actions */
enum class ESectionUpdateFlags
{
	None = 0x0,

	/** 
	*	This will use move-assignment when copying the supplied vertices/triangles into the section.
	*	This is faster as it doesn't require copying the data.
	*
	*	CAUTION: This means that your copy of the arrays will be cleared!
	*/
	MoveArrays = 0x1,

	/**
	*	Should the normals and tangents be calculated automatically?
	*	To do this manually see RuntimeMeshLibrary::CalculateTangentsForMesh()
	*/
	CalculateNormalTangent = 0x2,

	/**
	*	Should the tessellation indices be calculated to support tessellation?
	*	To do this manually see RuntimeMeshLibrary::GenerateTessellationIndexBuffer()
	*/
	CalculateTessellationIndices = 0x4,
	
};
ENUM_CLASS_FLAGS(ESectionUpdateFlags)

/**
*	Struct used to specify a tangent vector for a vertex
*	The Y tangent is computed from the cross product of the vertex normal (Tangent Z) and the TangentX member.
*/
USTRUCT(BlueprintType)
struct FRuntimeMeshTangent
{
	GENERATED_USTRUCT_BODY()

	/** Direction of X tangent for this vertex */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Tangent)
	FVector TangentX;

	/** Bool that indicates whether we should flip the Y tangent when we compute it using cross product */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Tangent)
	bool bFlipTangentY;

	FRuntimeMeshTangent()
		: TangentX(1.f, 0.f, 0.f)
		, bFlipTangentY(false)
	{}

	FRuntimeMeshTangent(float X, float Y, float Z, bool bInFlipTangentY = false)
		: TangentX(X, Y, Z)
		, bFlipTangentY(bInFlipTangentY)
	{}

	FRuntimeMeshTangent(FVector InTangentX, bool bInFlipTangentY = false)
		: TangentX(InTangentX)
		, bFlipTangentY(bInFlipTangentY)
	{}

	void AdjustNormal(FPackedNormal& Normal) const
	{
		Normal.Vector.W = bFlipTangentY ? 0 : 255;
	}

	void AdjustNormal(FPackedRGBA16N& Normal) const
	{
		Normal.W = bFlipTangentY ? 0 : 65535;
	}
};

/*
*	Configuration flag for the collision cooking to prioritize cooking speed or collision performance.
*/
UENUM(BlueprintType)
enum class ERuntimeMeshCollisionCookingMode : uint8
{
	/*
	*	Favors runtime collision performance of cooking speed. 
	*	This means that cooking a new mesh will be slower, but collision will be faster.
	*/
	CollisionPerformance UMETA(DisplayName = "Collision Performance"),

	/*
	*	Favors cooking speed over collision performance.
	*	This means that cooking a new mesh will be faster, but collision will be slower.
	*/
	CookingPerformance UMETA(DisplayName = "Cooking Performance"),
};

/* The different buffers within the Runtime Mesh Component */
enum class ERuntimeMeshBuffer
{
	None = 0x0,
	Vertices = 0x1,
	Triangles = 0x2,
	Positions = 0x4
};
ENUM_CLASS_FLAGS(ERuntimeMeshBuffer)


USTRUCT()
struct FRuntimeMeshCollisionSection
{
	GENERATED_BODY()

	UPROPERTY()
	TArray<FVector> VertexBuffer;

	UPROPERTY()
	TArray<int32> IndexBuffer;

	void Reset()
	{
		VertexBuffer.Empty();
		IndexBuffer.Empty();
	}

	friend FArchive& operator <<(FArchive& Ar, FRuntimeMeshCollisionSection& Section)
	{
		Ar << Section.VertexBuffer;
		Ar << Section.IndexBuffer;
		return Ar;
	}
}; 

USTRUCT()
struct FRuntimeConvexCollisionSection
{
	GENERATED_BODY()

	UPROPERTY()
	TArray<FVector> VertexBuffer;

	UPROPERTY()
	FBox BoundingBox;

	void Reset()
	{
		VertexBuffer.Empty();
		BoundingBox.Init();
	}

	friend FArchive& operator <<(FArchive& Ar, FRuntimeConvexCollisionSection& Section)
	{
		Ar << Section.VertexBuffer;
		Ar << Section.BoundingBox;
		return Ar;
	}
};






struct RUNTIMEMESHCOMPONENT_API FRuntimeMeshVertexTypeInfo
{
	const FString TypeName;
	const FGuid TypeGuid;

	FRuntimeMeshVertexTypeInfo(FString Name, FGuid Guid) : TypeName(Name), TypeGuid(Guid) { }

	virtual bool Equals(const FRuntimeMeshVertexTypeInfo* Other) const
	{
		return TypeGuid == Other->TypeGuid;
	}

	template<typename Type>
	void EnsureEquals() const
	{
		if (!Equals(&Type::TypeInfo))
		{
			ThrowMismatchException(Type::TypeInfo.TypeName);
		}
	}

	virtual class FRuntimeMeshSectionInterface* CreateSection(bool bInNeedsPositionOnlyBuffer) const = 0;
protected:

	void ThrowMismatchException(const FString& OtherName) const
	{
		UE_LOG(RuntimeMeshLog, Fatal, TEXT("Vertex Type Mismatch: %s  and  %s"), *TypeName, *OtherName);
	}
};


/*
*  Internal container used to track known vertex types, for serialization and other purposes.
*/
class RUNTIMEMESHCOMPONENT_API FRuntimeMeshVertexTypeRegistrationContainer
{
	struct VertexRegistration
	{
		const FRuntimeMeshVertexTypeInfo* const TypeInfo;
		uint32 ReferenceCount;
		
		VertexRegistration(const FRuntimeMeshVertexTypeInfo* const InTypeInfo)
			: TypeInfo(InTypeInfo), ReferenceCount(1) { }
	};
	TMap<FGuid, VertexRegistration> Registrations;

public:

	static FRuntimeMeshVertexTypeRegistrationContainer& GetInstance();

	void Register(const FRuntimeMeshVertexTypeInfo* InType);

	void UnRegister(const FRuntimeMeshVertexTypeInfo* InType);

	const FRuntimeMeshVertexTypeInfo* GetVertexType(FGuid Key) const;

};


template<typename VertexType>
class FRuntimeMeshVertexTypeRegistration : FNoncopyable
{
public:
	FRuntimeMeshVertexTypeRegistration()
	{ 
		FRuntimeMeshVertexTypeRegistrationContainer::GetInstance().Register(&VertexType::TypeInfo);
	}

	~FRuntimeMeshVertexTypeRegistration()
	{
		FRuntimeMeshVertexTypeRegistrationContainer::GetInstance().UnRegister(&VertexType::TypeInfo);
	}
};




#define DECLARE_RUNTIMEMESH_CUSTOMVERTEX_TYPEINFO(TypeName, Guid) \
	struct FRuntimeMeshVertexTypeInfo_##TypeName : public FRuntimeMeshVertexTypeInfo \
	{ \
		FRuntimeMeshVertexTypeInfo_##TypeName() : FRuntimeMeshVertexTypeInfo(TEXT(#TypeName), Guid) { } \
	}; \
	static const FRuntimeMeshVertexTypeInfo_##TypeName TypeInfo;

#define DEFINE_RUNTIMEMESH_CUSTOMVERTEX_TYPEINFO(TypeName) \
	const  TypeName::FRuntimeMeshVertexTypeInfo_##TypeName TypeName::TypeInfo = TypeName::FRuntimeMeshVertexTypeInfo_##TypeName(); \
    FRuntimeMeshVertexTypeRegistration<##TypeName> FRuntimeMeshVertexTypeInfoRegistration_##TypeName(&##TypeName::TypeInfo);


