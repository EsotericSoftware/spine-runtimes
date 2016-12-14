// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "RuntimeMeshCore.h"
#include "RuntimeMeshComponent.h"

#define RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(TaskType, DataType, DataPtr, RuntimeMesh, Code)														\
	class FParallelRuntimeMeshComponentTask_##TaskType																							\
	{																																			\
		TWeakObjectPtr<URuntimeMeshComponent> RuntimeMeshComponent;																				\
		DataType* RMCCallData;																															\
	public:																																		\
		FParallelRuntimeMeshComponentTask_##TaskType(TWeakObjectPtr<URuntimeMeshComponent> InRMC, DataType* InData)			\
			: RuntimeMeshComponent(InRMC), RMCCallData(InData)																		\
		{																																		\
		}																																		\
																																				\
		~FParallelRuntimeMeshComponentTask_##TaskType()																							\
		{																																		\
			if (RMCCallData != nullptr)																											\
			{																																	\
				delete RMCCallData;																												\
			}																																	\
		}																																		\
																																				\
		FORCEINLINE TStatId GetStatId() const																									\
		{																																		\
			RETURN_QUICK_DECLARE_CYCLE_STAT(FParallelRuntimeMeshComponentTask_##TaskType, STATGROUP_TaskGraphTasks);							\
		}																																		\
																																				\
		static ENamedThreads::Type GetDesiredThread()																							\
		{																																		\
			return ENamedThreads::GameThread;																									\
		}																																		\
																																				\
		static ESubsequentsMode::Type GetSubsequentsMode()																						\
		{																																		\
			return ESubsequentsMode::FireAndForget;																								\
		}																																		\
																																				\
		void DoTask(ENamedThreads::Type CurrentThread, const FGraphEventRef& MyCompletionGraphEvent)											\
		{																																		\
			DataType* Data = RMCCallData;																										\
			if (URuntimeMeshComponent* Mesh = RuntimeMeshComponent.Get())																		\
			{																																	\
				Code																															\
			}																																	\
		}																																		\
	};																																			\
	TGraphTask<FParallelRuntimeMeshComponentTask_##TaskType>::CreateTask().ConstructAndDispatchWhenReady(RuntimeMesh, DataPtr);




class RUNTIMEMESHCOMPONENT_API FRuntimeMeshAsync
{



public:

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
	static void CreateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<VertexType>& Vertices, TArray<int32>& Triangles, 
		bool bCreateCollision = false, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			bool bCreateCollision;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
		}
		CallData->bCreateCollision = bCreateCollision;
		CallData->UpdateFrequency = UpdateFrequency;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(CreateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->CreateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->bCreateCollision, Data->UpdateFrequency, Data->UpdateFlags);
		});
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
	static void CreateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<VertexType>& Vertices, TArray<int32>& Triangles,
		const FBox& BoundingBox, bool bCreateCollision = false, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			FBox BoundingBox;
			bool bCreateCollision;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
		}
		CallData->BoundingBox = BoundingBox;
		CallData->bCreateCollision = bCreateCollision;
		CallData->UpdateFrequency = UpdateFrequency;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(CreateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->CreateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->BoundingBox, Data->bCreateCollision, Data->UpdateFrequency, Data->UpdateFlags);
		});
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
	static void CreateMeshSectionDualBuffer(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<FVector>& VertexPositions,
		TArray<VertexType>& VertexData, TArray<int32>& Triangles, bool bCreateCollision = false,
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			bool bCreateCollision;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
			CallData->Vertices = MoveTemp(VertexData);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
			CallData->Vertices = VertexData;
			CallData->Triangles = Triangles;
		}
		CallData->bCreateCollision = bCreateCollision;
		CallData->UpdateFrequency = UpdateFrequency;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(CreateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->CreateMeshSection(Data->SectionIndex, Data->VertexPositions, Data->Vertices, Data->Triangles, Data->bCreateCollision, Data->UpdateFrequency, Data->UpdateFlags);
		});
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
	static void CreateMeshSectionDualBuffer(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<FVector>& VertexPositions,
		TArray<VertexType>& VertexData, TArray<int32>& Triangles, const FBox& BoundingBox,
		bool bCreateCollision = false, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			FBox BoundingBox;
			bool bCreateCollision;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
			CallData->Vertices = MoveTemp(VertexData);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
			CallData->Vertices = VertexData;
			CallData->Triangles = Triangles;
		}
		CallData->BoundingBox = BoundingBox;
		CallData->bCreateCollision = bCreateCollision;
		CallData->UpdateFrequency = UpdateFrequency;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(CreateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->CreateMeshSection(Data->SectionIndex, Data->VertexPositions, Data->Vertices, Data->Triangles, Data->BoundingBox, Data->bCreateCollision, Data->UpdateFrequency, Data->UpdateFlags);
		});
	}


	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<VertexType>& Vertices,
		ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<VertexType> Vertices;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
		}
		else
		{
			CallData->Vertices = Vertices;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->UpdateFlags);
		});
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<VertexType>& Vertices,
		const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<VertexType> Vertices;
			FBox BoundingBox;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
		}
		else
		{
			CallData->Vertices = Vertices;
		}
		
		CallData->BoundingBox = BoundingBox;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->BoundingBox, Data->UpdateFlags);
		});
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. If this is a dual buffer section, you cannot change the length of the vertices.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer all vertex data for this section, or in the case of dual buffer section it contains everything but position.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<VertexType>& Vertices,
		TArray<int32>& Triangles, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->UpdateFlags);
		});
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
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<VertexType>& Vertices,
		TArray<int32>& Triangles, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			FBox BoundingBox;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
		}

		CallData->BoundingBox = BoundingBox;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->BoundingBox, Data->UpdateFlags);
		});
	}


	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	Vertices			Vertex buffer containing everything except position for each vertex.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<FVector>& VertexPositions,
		TArray<VertexType>& Vertices, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			TArray<VertexType> Vertices;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
			CallData->Vertices = MoveTemp(Vertices);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
			CallData->Vertices = Vertices;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->VertexPositions, Data->Vertices, Data->UpdateFlags);
		});
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	Vertices			Vertex buffer containing everything except position for each vertex.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<FVector>& VertexPositions,
		TArray<VertexType>& Vertices, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			TArray<VertexType> Vertices;
			FBox BoundingBox;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
			CallData->Vertices = MoveTemp(Vertices);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
			CallData->Vertices = Vertices;
		}

		CallData->BoundingBox = BoundingBox;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->VertexPositions, Data->Vertices, Data->BoundingBox, Data->UpdateFlags);
		});
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	Vertices			Vertex buffer containing everything except position for each vertex.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<FVector>& VertexPositions,
		TArray<VertexType>& Vertices, TArray<int32>& Triangles, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->VertexPositions, Data->Vertices, Data->Triangles, Data->UpdateFlags);
		});
	}

	/**
	*	Updates a section. This is faster than CreateMeshSection. This is only for dual buffer sections. You cannot change the length of positions or vertex data unless you specify both together.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	Vertices			Vertex buffer containing everything except position for each vertex.
	*	@param	Triangles			Index buffer indicating which vertices make up each triangle. Length must be a multiple of 3.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	template<typename VertexType>
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, TArray<FVector>& VertexPositions,
		TArray<VertexType>& Vertices, TArray<int32>& Triangles, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			TArray<VertexType> Vertices;
			TArray<int32> Triangles;
			FBox BoundingBox;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
		}

		CallData->BoundingBox = BoundingBox;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->VertexPositions, Data->Vertices, Data->Triangles, Data->BoundingBox, Data->UpdateFlags);
		});
	}


	/**
	*	Updates a sections position buffer only. This cannot be used on a non-dual buffer section. You cannot change the length of the vertex position buffer with this function.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	static void UpdateMeshSectionPositionsImmediate(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex,
		TArray<FVector>& VertexPositions, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSectionPositionsImmediate, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSectionPositionsImmediate(Data->SectionIndex, Data->VertexPositions, Data->UpdateFlags);
		});
	}

	/**
	*	Updates a sections position buffer only. This cannot be used on a non-dual buffer section. You cannot change the length of the vertex position buffer with this function.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	VertexPositions		Vertex buffer containing only the position information for each vertex.
	*	@param	BoundingBox			The bounds of this section. Faster than the RMC automatically calculating it.
	*	@param	UpdateFlags			Flags pertaining to this particular update.
	*/
	static void UpdateMeshSectionPositionsImmediate(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex,
		TArray<FVector>& VertexPositions, const FBox& BoundingBox, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> VertexPositions;
			FBox BoundingBox;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->VertexPositions = MoveTemp(VertexPositions);
		}
		else
		{
			CallData->VertexPositions = VertexPositions;
		}
		
		CallData->BoundingBox = BoundingBox;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSectionPositionsImmediate, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSectionPositionsImmediate(Data->SectionIndex, Data->VertexPositions, Data->BoundingBox, Data->UpdateFlags);
		});
	}





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
	static void CreateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, const TArray<FVector>& Vertices,
		const TArray<int32>& Triangles, const TArray<FVector>& Normals, const TArray<FVector2D>& UV0, const TArray<FColor>& Colors,
		const TArray<FRuntimeMeshTangent>& Tangents, bool bCreateCollision = false,
		EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> Vertices;
			TArray<int32> Triangles;
			TArray<FVector> Normals;
			TArray<FVector2D> UV0;
			TArray<FColor> Colors;
			TArray<FRuntimeMeshTangent> Tangents;
			bool bCreateCollision = false;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
			CallData->Normals = MoveTemp(Normals);
			CallData->UV0 = MoveTemp(UV0);
			CallData->Colors = MoveTemp(Colors);
			CallData->Tangents = MoveTemp(Tangents);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
			CallData->Normals = Normals;
			CallData->UV0 = UV0;
			CallData->Colors = Colors;
			CallData->Tangents = Tangents;
		}

		CallData->bCreateCollision = bCreateCollision;
		CallData->UpdateFrequency = UpdateFrequency;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(CreateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->CreateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->Normals, Data->UV0, Data->Colors, 
			Data->Tangents, Data->bCreateCollision, Data->UpdateFrequency, Data->UpdateFlags);
		});
	}

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
	static void CreateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, const TArray<FVector>& Vertices,
		const TArray<int32>& Triangles, const TArray<FVector>& Normals,	const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, 
		const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents,
		bool bCreateCollision = false, EUpdateFrequency UpdateFrequency = EUpdateFrequency::Average, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> Vertices;
			TArray<int32> Triangles;
			TArray<FVector> Normals;
			TArray<FVector2D> UV0;
			TArray<FVector2D> UV1;
			TArray<FColor> Colors;
			TArray<FRuntimeMeshTangent> Tangents;
			bool bCreateCollision = false;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
			CallData->Normals = MoveTemp(Normals);
			CallData->UV0 = MoveTemp(UV0);
			CallData->UV1 = MoveTemp(UV1);
			CallData->Colors = MoveTemp(Colors);
			CallData->Tangents = MoveTemp(Tangents);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
			CallData->Normals = Normals;
			CallData->UV0 = UV0;
			CallData->UV1 = UV1;
			CallData->Colors = Colors;
			CallData->Tangents = Tangents;
		}

		CallData->bCreateCollision = bCreateCollision;
		CallData->UpdateFrequency = UpdateFrequency;
		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(CreateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->CreateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->Normals, Data->UV0, Data->UV1, Data->Colors,
			Data->Tangents, Data->bCreateCollision, Data->UpdateFrequency, Data->UpdateFlags);
		});
	}


	/**
	*	Updates a section. This is faster than CreateMeshSection.
	*	@param	SectionIndex		Index of the section to update.
	*	@param	Vertices			Vertex buffer of all vertex positions to use for this mesh section.
	*	@param	Normals				Optional array of normal vectors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	UV1					Optional array of texture co-ordinates for each vertex (UV Channel 1). If supplied, must be same length as Vertices array.
	*	@param	Colors				Optional array of colors for each vertex. If supplied, must be same length as Vertices array.
	*	@param	Tangents			Optional array of tangent vector for each vertex. If supplied, must be same length as Vertices array.
	*/
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, const TArray<FVector>& Vertices,
		const TArray<FVector>& Normals, const TArray<FVector2D>& UV0, const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, 
		ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> Vertices;
			TArray<FVector> Normals;
			TArray<FVector2D> UV0;
			TArray<FVector2D> UV1;
			TArray<FColor> Colors;
			TArray<FRuntimeMeshTangent> Tangents;
			bool bCreateCollision = false;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Normals = MoveTemp(Normals);
			CallData->UV0 = MoveTemp(UV0);
			CallData->Colors = MoveTemp(Colors);
			CallData->Tangents = MoveTemp(Tangents);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Normals = Normals;
			CallData->UV0 = UV0;
			CallData->Colors = Colors;
			CallData->Tangents = Tangents;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->Normals, Data->UV0, Data->Colors,	Data->Tangents, Data->UpdateFlags);
		});
	}

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
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, const TArray<FVector>& Vertices,
		const TArray<FVector>& Normals, const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, const TArray<FColor>& Colors, 
		const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> Vertices;
			TArray<FVector> Normals;
			TArray<FVector2D> UV0;
			TArray<FVector2D> UV1;
			TArray<FColor> Colors;
			TArray<FRuntimeMeshTangent> Tangents;
			bool bCreateCollision = false;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Normals = MoveTemp(Normals);
			CallData->UV0 = MoveTemp(UV0);
			CallData->UV1 = MoveTemp(UV1);
			CallData->Colors = MoveTemp(Colors);
			CallData->Tangents = MoveTemp(Tangents);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Normals = Normals;
			CallData->UV0 = UV0;
			CallData->UV1 = UV1;
			CallData->Colors = Colors;
			CallData->Tangents = Tangents;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->Normals, Data->UV0, Data->UV1, Data->Colors, Data->Tangents, Data->UpdateFlags);
		});
	}

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
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, const TArray<FVector>& Vertices,
		const TArray<int32>& Triangles, const TArray<FVector>& Normals,	const TArray<FVector2D>& UV0, const TArray<FColor>& Colors, 
		const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> Vertices;
			TArray<int32> Triangles;
			TArray<FVector> Normals;
			TArray<FVector2D> UV0;
			TArray<FColor> Colors;
			TArray<FRuntimeMeshTangent> Tangents;
			bool bCreateCollision = false;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
			CallData->Normals = MoveTemp(Normals);
			CallData->UV0 = MoveTemp(UV0);
			CallData->Colors = MoveTemp(Colors);
			CallData->Tangents = MoveTemp(Tangents);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
			CallData->Normals = Normals;
			CallData->UV0 = UV0;
			CallData->Colors = Colors;
			CallData->Tangents = Tangents;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->Normals, Data->UV0, Data->Colors, Data->Tangents, Data->UpdateFlags);
		});
	}

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
	static void UpdateMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex, const TArray<FVector>& Vertices,
		const TArray<int32>& Triangles, const TArray<FVector>& Normals,	const TArray<FVector2D>& UV0, const TArray<FVector2D>& UV1, 
		const TArray<FColor>& Colors, const TArray<FRuntimeMeshTangent>& Tangents, ESectionUpdateFlags UpdateFlags = ESectionUpdateFlags::None)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<FVector> Vertices;
			TArray<int32> Triangles;
			TArray<FVector> Normals;
			TArray<FVector2D> UV0;
			TArray<FVector2D> UV1;
			TArray<FColor> Colors;
			TArray<FRuntimeMeshTangent> Tangents;
			bool bCreateCollision = false;
			EUpdateFrequency UpdateFrequency;
			ESectionUpdateFlags UpdateFlags;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (!!(UpdateFlags & ESectionUpdateFlags::MoveArrays))
		{
			CallData->Vertices = MoveTemp(Vertices);
			CallData->Triangles = MoveTemp(Triangles);
			CallData->Normals = MoveTemp(Normals);
			CallData->UV0 = MoveTemp(UV0);
			CallData->UV1 = MoveTemp(UV1);
			CallData->Colors = MoveTemp(Colors);
			CallData->Tangents = MoveTemp(Tangents);
		}
		else
		{
			CallData->Vertices = Vertices;
			CallData->Triangles = Triangles;
			CallData->Normals = Normals;
			CallData->UV0 = UV0;
			CallData->UV1 = UV1;
			CallData->Colors = Colors;
			CallData->Tangents = Tangents;
		}

		CallData->UpdateFlags = UpdateFlags | ESectionUpdateFlags::MoveArrays; // We can always use move arrays here since we either just copied it, or moved it from the original

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(UpdateMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->UpdateMeshSection(Data->SectionIndex, Data->Vertices, Data->Triangles, Data->Normals, Data->UV0, Data->UV1, Data->Colors,	Data->Tangents, Data->UpdateFlags);
		});
	}



	/** Clear a section of the procedural mesh. */
	static void ClearMeshSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
		};

		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(ClearMeshSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->ClearMeshSection(Data->SectionIndex);
		});
	}

	/** Clear all mesh sections and reset to empty state */
	static void ClearAllMeshSections(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(ClearAllMeshSections, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->ClearAllMeshSections();
		});
	}


	/** Sets the tessellation triangles needed to correctly support tessellation on a section. */
	static void SetSectionTessellationTriangles(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 SectionIndex,
		const TArray<int32>& TessellationTriangles, bool bShouldMoveArray = false)
	{
		struct FRMCAsyncData
		{
			int32 SectionIndex;
			TArray<int32> TessellationTriangles;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->SectionIndex = SectionIndex;

		if (bShouldMoveArray)
		{
			CallData->TessellationTriangles = MoveTemp(TessellationTriangles);
		}
		else
		{
			CallData->TessellationTriangles = TessellationTriangles;
		}


		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(SetSectionTessellationTriangles, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->SetSectionTessellationTriangles(Data->SectionIndex, Data->TessellationTriangles, true);
		});
	}


	/** Sets the geometry for a collision only section */
	static void SetMeshCollisionSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 CollisionSectionIndex,
		const TArray<FVector>& Vertices, const TArray<int32>& Triangles)
	{
		struct FRMCAsyncData
		{
			int32 CollisionSectionIndex;
			TArray<FVector> Vertices;
			TArray<int32> Triangles;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->CollisionSectionIndex = CollisionSectionIndex;
		CallData->Vertices = Vertices;
		CallData->Triangles = Triangles;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(SetMeshCollisionSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->SetMeshCollisionSection(Data->CollisionSectionIndex, Data->Vertices, Data->Triangles);
		});
	}

	/** Clears the geometry for a collision only section */
	static void ClearMeshCollisionSection(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, int32 CollisionSectionIndex)
	{
		struct FRMCAsyncData
		{
			int32 CollisionSectionIndex;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->CollisionSectionIndex = CollisionSectionIndex;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(ClearMeshCollisionSection, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->ClearMeshCollisionSection(Data->CollisionSectionIndex);
		});
	}

	/** Clears the geometry for ALL collision only sections */
	static void ClearAllMeshCollisionSections(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent)
	{
		struct FRMCAsyncData
		{
			int32 CollisionSectionIndex;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(ClearAllMeshCollisionSections, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->ClearAllMeshCollisionSections();
		});
	}


	/** Add simple collision convex to this component */
	static void AddCollisionConvexMesh(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, TArray<FVector> ConvexVerts)
	{
		struct FRMCAsyncData
		{
			TArray<FVector> ConvexVerts;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->ConvexVerts = ConvexVerts;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(AddCollisionConvexMesh, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->AddCollisionConvexMesh(Data->ConvexVerts);
		});
	}

	/** Add simple collision convex to this component */
	static void ClearCollisionConvexMeshes(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent)
	{
		struct FRMCAsyncData
		{
			int32 CollisionSectionIndex;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(ClearCollisionConvexMeshes, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->ClearCollisionConvexMeshes();
		});
	}

	/** Function to replace _all_ simple collision in one go */
	static void SetCollisionConvexMeshes(TWeakObjectPtr<URuntimeMeshComponent> InRuntimeMeshComponent, const TArray< TArray<FVector> >& ConvexMeshes)
	{
		struct FRMCAsyncData
		{
			TArray<TArray<FVector>> ConvexMeshes;
		};
		FRMCAsyncData* CallData = new FRMCAsyncData;
		CallData->ConvexMeshes = ConvexMeshes;

		RUNTIMEMESHCOMPONENTASYNC_ENQUEUETASK(SetCollisionConvexMeshes, FRMCAsyncData, CallData, InRuntimeMeshComponent,
		{
			Mesh->SetCollisionConvexMeshes(Data->ConvexMeshes);
		});
	}
};
