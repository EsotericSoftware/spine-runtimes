// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#include "RuntimeMeshComponentPluginPrivatePCH.h"
#include "RuntimeMeshLibrary.h"
#include "MessageLog.h"
#include "UObjectToken.h"
#include "StaticMeshResources.h"
#include "TessellationUtilities.h"
#include "RuntimeMeshBuilder.h"
#include "RuntimeMeshComponent.h"

#define LOCTEXT_NAMESPACE "RuntimeMeshLibrary"


URuntimeMeshLibrary::URuntimeMeshLibrary(const FObjectInitializer& ObjectInitializer)
	: Super(ObjectInitializer)
{

}

void URuntimeMeshLibrary::ConvertQuadToTriangles(TArray<int32>& Triangles, int32 Vert0, int32 Vert1, int32 Vert2, int32 Vert3)
{
	Triangles.Add(Vert0);
	Triangles.Add(Vert1);
	Triangles.Add(Vert3);

	Triangles.Add(Vert1);
	Triangles.Add(Vert2);
	Triangles.Add(Vert3);
}

void URuntimeMeshLibrary::CreateGridMeshTriangles(int32 NumX, int32 NumY, bool bWinding, TArray<int32>& Triangles)
{
	Triangles.Reset();

	if (NumX >= 2 && NumY >= 2)
	{
		// Build Quads
		for (int XIdx = 0; XIdx < NumX - 1; XIdx++)
		{
			for (int YIdx = 0; YIdx < NumY - 1; YIdx++)
			{
				const int32 I0 = (XIdx + 0)*NumY + (YIdx + 0);
				const int32 I1 = (XIdx + 1)*NumY + (YIdx + 0);
				const int32 I2 = (XIdx + 1)*NumY + (YIdx + 1);
				const int32 I3 = (XIdx + 0)*NumY + (YIdx + 1);

				if (bWinding)
				{
					ConvertQuadToTriangles(Triangles, I0, I1, I2, I3);
				}
				else
				{
					ConvertQuadToTriangles(Triangles, I0, I3, I2, I1);
				}
			}
		}
	}
}

void URuntimeMeshLibrary::CreateBoxMesh(FVector BoxRadius, TArray<FVector>& Vertices, TArray<int32>& Triangles, TArray<FVector>& Normals, TArray<FVector2D>& UVs, TArray<FRuntimeMeshTangent>& Tangents)
{
	// Generate verts
	FVector BoxVerts[8];
	BoxVerts[0] = FVector(-BoxRadius.X, BoxRadius.Y, BoxRadius.Z);
	BoxVerts[1] = FVector(BoxRadius.X, BoxRadius.Y, BoxRadius.Z);
	BoxVerts[2] = FVector(BoxRadius.X, -BoxRadius.Y, BoxRadius.Z);
	BoxVerts[3] = FVector(-BoxRadius.X, -BoxRadius.Y, BoxRadius.Z);

	BoxVerts[4] = FVector(-BoxRadius.X, BoxRadius.Y, -BoxRadius.Z);
	BoxVerts[5] = FVector(BoxRadius.X, BoxRadius.Y, -BoxRadius.Z);
	BoxVerts[6] = FVector(BoxRadius.X, -BoxRadius.Y, -BoxRadius.Z);
	BoxVerts[7] = FVector(-BoxRadius.X, -BoxRadius.Y, -BoxRadius.Z);

	// Generate triangles (from quads)
	Triangles.Reset();

	const int32 NumVerts = 24; // 6 faces x 4 verts per face

	Vertices.Reset();
	Vertices.AddUninitialized(NumVerts);

	Normals.Reset();
	Normals.AddUninitialized(NumVerts);

	Tangents.Reset();
	Tangents.AddUninitialized(NumVerts);

	Vertices[0] = BoxVerts[0];
	Vertices[1] = BoxVerts[1];
	Vertices[2] = BoxVerts[2];
	Vertices[3] = BoxVerts[3];
	ConvertQuadToTriangles(Triangles, 0, 1, 2, 3);
	Normals[0] = Normals[1] = Normals[2] = Normals[3] = FVector(0, 0, 1);
	Tangents[0] = Tangents[1] = Tangents[2] = Tangents[3] = FRuntimeMeshTangent(0.f, -1.f, 0.f);

	Vertices[4] = BoxVerts[4];
	Vertices[5] = BoxVerts[0];
	Vertices[6] = BoxVerts[3];
	Vertices[7] = BoxVerts[7];
	ConvertQuadToTriangles(Triangles, 4, 5, 6, 7);
	Normals[4] = Normals[5] = Normals[6] = Normals[7] = FVector(-1, 0, 0);
	Tangents[4] = Tangents[5] = Tangents[6] = Tangents[7] = FRuntimeMeshTangent(0.f, -1.f, 0.f);

	Vertices[8] = BoxVerts[5];
	Vertices[9] = BoxVerts[1];
	Vertices[10] = BoxVerts[0];
	Vertices[11] = BoxVerts[4];
	ConvertQuadToTriangles(Triangles, 8, 9, 10, 11);
	Normals[8] = Normals[9] = Normals[10] = Normals[11] = FVector(0, 1, 0);
	Tangents[8] = Tangents[9] = Tangents[10] = Tangents[11] = FRuntimeMeshTangent(-1.f, 0.f, 0.f);

	Vertices[12] = BoxVerts[6];
	Vertices[13] = BoxVerts[2];
	Vertices[14] = BoxVerts[1];
	Vertices[15] = BoxVerts[5];
	ConvertQuadToTriangles(Triangles, 12, 13, 14, 15);
	Normals[12] = Normals[13] = Normals[14] = Normals[15] = FVector(1, 0, 0);
	Tangents[12] = Tangents[13] = Tangents[14] = Tangents[15] = FRuntimeMeshTangent(0.f, 1.f, 0.f);

	Vertices[16] = BoxVerts[7];
	Vertices[17] = BoxVerts[3];
	Vertices[18] = BoxVerts[2];
	Vertices[19] = BoxVerts[6];
	ConvertQuadToTriangles(Triangles, 16, 17, 18, 19);
	Normals[16] = Normals[17] = Normals[18] = Normals[19] = FVector(0, -1, 0);
	Tangents[16] = Tangents[17] = Tangents[18] = Tangents[19] = FRuntimeMeshTangent(1.f, 0.f, 0.f);

	Vertices[20] = BoxVerts[7];
	Vertices[21] = BoxVerts[6];
	Vertices[22] = BoxVerts[5];
	Vertices[23] = BoxVerts[4];
	ConvertQuadToTriangles(Triangles, 20, 21, 22, 23);
	Normals[20] = Normals[21] = Normals[22] = Normals[23] = FVector(0, 0, -1);
	Tangents[20] = Tangents[21] = Tangents[22] = Tangents[23] = FRuntimeMeshTangent(0.f, 1.f, 0.f);

	// UVs
	UVs.Reset();
	UVs.AddUninitialized(NumVerts);

	UVs[0] = UVs[4] = UVs[8] = UVs[12] = UVs[16] = UVs[20] = FVector2D(0.f, 0.f);
	UVs[1] = UVs[5] = UVs[9] = UVs[13] = UVs[17] = UVs[21] = FVector2D(0.f, 1.f);
	UVs[2] = UVs[6] = UVs[10] = UVs[14] = UVs[18] = UVs[22] = FVector2D(1.f, 1.f);
	UVs[3] = UVs[7] = UVs[11] = UVs[15] = UVs[19] = UVs[23] = FVector2D(1.f, 0.f);
}





void FindVertOverlaps(int32 TestVertIndex, const IRuntimeMeshVerticesBuilder* Vertices, TArray<int32>& VertOverlaps)
{
	// Check if Verts is empty or test is outside range
	if (TestVertIndex < Vertices->Length())
	{
		const FVector TestVert = Vertices->GetPosition(TestVertIndex);

		for (int32 VertIdx = 0; VertIdx < Vertices->Length(); VertIdx++)
		{
			// First see if we overlap, and smoothing groups are the same
			if (TestVert.Equals(Vertices->GetPosition(VertIdx)))
			{
				// If it, so we are at least considered an 'overlap' for normal gen
				VertOverlaps.Add(VertIdx);
			}
		}
	}
}

void URuntimeMeshLibrary::CalculateTangentsForMesh(IRuntimeMeshVerticesBuilder* Vertices, const FRuntimeMeshIndicesBuilder* Triangles)
{
	if (Vertices->Length() == 0) return;

	// Number of triangles
	const int32 NumTris = Triangles->TriangleLength();
	// Number of verts
	const int32 NumVerts = Vertices->Length();

	// Map of vertex to triangles in Triangles array
	TMultiMap<int32, int32> VertToTriMap;
	// Map of vertex to triangles to consider for normal calculation
	TMultiMap<int32, int32> VertToTriSmoothMap;

	// Normal/tangents for each face
	TArray<FVector> FaceTangentX, FaceTangentY, FaceTangentZ;
	FaceTangentX.AddUninitialized(NumTris);
	FaceTangentY.AddUninitialized(NumTris);
	FaceTangentZ.AddUninitialized(NumTris);

	// Iterate over triangles
	for (int TriIdx = 0; TriIdx < NumTris; TriIdx++)
	{
		int32 CornerIndex[3];
		FVector P[3];

		for (int32 CornerIdx = 0; CornerIdx < 3; CornerIdx++)
		{
			// Find vert index (clamped within range)
			int32 VertIndex = FMath::Min(Triangles->GetIndex((TriIdx * 3) + CornerIdx), NumVerts - 1);

			CornerIndex[CornerIdx] = VertIndex;
			P[CornerIdx] = Vertices->GetPosition(VertIndex);

			// Find/add this vert to index buffer
			TArray<int32> VertOverlaps;
			FindVertOverlaps(VertIndex, Vertices, VertOverlaps);

			// Remember which triangles map to this vert
			VertToTriMap.AddUnique(VertIndex, TriIdx);
			VertToTriSmoothMap.AddUnique(VertIndex, TriIdx);

			// Also update map of triangles that 'overlap' this vert (ie don't match UV, but do match smoothing) and should be considered when calculating normal
			for (int32 OverlapIdx = 0; OverlapIdx < VertOverlaps.Num(); OverlapIdx++)
			{
				// For each vert we overlap..
				int32 OverlapVertIdx = VertOverlaps[OverlapIdx];

				// Add this triangle to that vert
				VertToTriSmoothMap.AddUnique(OverlapVertIdx, TriIdx);

				// And add all of its triangles to us
				TArray<int32> OverlapTris;
				VertToTriMap.MultiFind(OverlapVertIdx, OverlapTris);
				for (int32 OverlapTriIdx = 0; OverlapTriIdx < OverlapTris.Num(); OverlapTriIdx++)
				{
					VertToTriSmoothMap.AddUnique(VertIndex, OverlapTris[OverlapTriIdx]);
				}
			}
		}

		// Calculate triangle edge vectors and normal
		const FVector Edge21 = P[1] - P[2];
		const FVector Edge20 = P[0] - P[2];
		const FVector TriNormal = (Edge21 ^ Edge20).GetSafeNormal();

		// If we have UVs, use those to calc 
		if (Vertices->HasUVComponent(0))
		{
			const FVector2D T1 = Vertices->GetUV(CornerIndex[0]);
			const FVector2D T2 = Vertices->GetUV(CornerIndex[1]);
			const FVector2D T3 = Vertices->GetUV(CornerIndex[2]);

			FMatrix	ParameterToLocal(
				FPlane(P[1].X - P[0].X, P[1].Y - P[0].Y, P[1].Z - P[0].Z, 0),
				FPlane(P[2].X - P[0].X, P[2].Y - P[0].Y, P[2].Z - P[0].Z, 0),
				FPlane(P[0].X, P[0].Y, P[0].Z, 0),
				FPlane(0, 0, 0, 1)
			);

			FMatrix ParameterToTexture(
				FPlane(T2.X - T1.X, T2.Y - T1.Y, 0, 0),
				FPlane(T3.X - T1.X, T3.Y - T1.Y, 0, 0),
				FPlane(T1.X, T1.Y, 1, 0),
				FPlane(0, 0, 0, 1)
			);

			// Use InverseSlow to catch singular matrices.  Inverse can miss this sometimes.
			const FMatrix TextureToLocal = ParameterToTexture.Inverse() * ParameterToLocal;

			FaceTangentX[TriIdx] = TextureToLocal.TransformVector(FVector(1, 0, 0)).GetSafeNormal();
			FaceTangentY[TriIdx] = TextureToLocal.TransformVector(FVector(0, 1, 0)).GetSafeNormal();
		}
		else
		{
			FaceTangentX[TriIdx] = Edge20.GetSafeNormal();
			FaceTangentY[TriIdx] = (FaceTangentX[TriIdx] ^ TriNormal).GetSafeNormal();
		}

		FaceTangentZ[TriIdx] = TriNormal;
	}


	// Arrays to accumulate tangents into
	TArray<FVector> VertexTangentXSum, VertexTangentYSum, VertexTangentZSum;
	VertexTangentXSum.AddZeroed(NumVerts);
	VertexTangentYSum.AddZeroed(NumVerts);
	VertexTangentZSum.AddZeroed(NumVerts);

	// For each vertex..
	for (int VertxIdx = 0; VertxIdx < Vertices->Length(); VertxIdx++)
	{
		// Find relevant triangles for normal
		TArray<int32> SmoothTris;
		VertToTriSmoothMap.MultiFind(VertxIdx, SmoothTris);

		for (int i = 0; i < SmoothTris.Num(); i++)
		{
			int32 TriIdx = SmoothTris[i];
			VertexTangentZSum[VertxIdx] += FaceTangentZ[TriIdx];
		}

		// Find relevant triangles for tangents
		TArray<int32> TangentTris;
		VertToTriMap.MultiFind(VertxIdx, TangentTris);

		for (int i = 0; i < TangentTris.Num(); i++)
		{
			int32 TriIdx = TangentTris[i];
			VertexTangentXSum[VertxIdx] += FaceTangentX[TriIdx];
			VertexTangentYSum[VertxIdx] += FaceTangentY[TriIdx];
		}
	}

	// Finally, normalize tangents and build output arrays
	
	for (int VertxIdx = 0; VertxIdx < NumVerts; VertxIdx++)
	{
		FVector& TangentX = VertexTangentXSum[VertxIdx];
		FVector& TangentY = VertexTangentYSum[VertxIdx];
		FVector& TangentZ = VertexTangentZSum[VertxIdx];

		TangentX.Normalize();
		TangentZ.Normalize();

		// Use Gram-Schmidt orthogonalization to make sure X is orth with Z
		TangentX -= TangentZ * (TangentZ | TangentX);
		TangentX.Normalize();

		Vertices->SetTangents(VertxIdx, TangentX, TangentY, TangentZ);
	}
}

void URuntimeMeshLibrary::CalculateTangentsForMesh(const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector2D>& UVs, TArray<FVector>& Normals, TArray<FRuntimeMeshTangent>& Tangents)
{
	FRuntimeMeshComponentVerticesBuilder VerticesBuilder(const_cast<TArray<FVector>*>(&Vertices), &Normals, &Tangents, nullptr, const_cast<TArray<FVector2D>*>(&UVs));
	FRuntimeMeshIndicesBuilder IndicesBuilder(const_cast<TArray<int32>*>(&Triangles));

	CalculateTangentsForMesh(&VerticesBuilder, &IndicesBuilder);
}





void URuntimeMeshLibrary::GenerateTessellationIndexBuffer(const IRuntimeMeshVerticesBuilder* Vertices, const FRuntimeMeshIndicesBuilder* Indices, FRuntimeMeshIndicesBuilder* OutTessellationIndices)
{
	TessellationUtilities::CalculateTessellationIndices(Vertices, Indices, OutTessellationIndices);
}

void URuntimeMeshLibrary::GenerateTessellationIndexBuffer(const TArray<FVector>& Vertices, const TArray<int32>& Triangles, const TArray<FVector2D>& UVs, TArray<FVector>& Normals, TArray<FRuntimeMeshTangent>& Tangents, TArray<int32>& OutTessTriangles)
{
	FRuntimeMeshComponentVerticesBuilder VerticesBuilder(const_cast<TArray<FVector>*>(&Vertices), &Normals, &Tangents, nullptr, const_cast<TArray<FVector2D>*>(&UVs));
	FRuntimeMeshIndicesBuilder IndicesBuilder(const_cast<TArray<int32>*>(&Triangles));
	FRuntimeMeshIndicesBuilder OutIndicesBuilder(&OutTessTriangles);

	GenerateTessellationIndexBuffer(&VerticesBuilder, &IndicesBuilder, &OutIndicesBuilder);
}





static int32 GetNewIndexForOldVertIndex(int32 MeshVertIndex, TMap<int32, int32>& MeshToSectionVertMap, const FPositionVertexBuffer* PosBuffer, const FStaticMeshVertexBuffer* VertBuffer, const FColorVertexBuffer* ColorBuffer, IRuntimeMeshVerticesBuilder* Vertices)
{
	int32* NewIndexPtr = MeshToSectionVertMap.Find(MeshVertIndex);
	if (NewIndexPtr != nullptr)
	{
		return *NewIndexPtr;
	}
	else
	{
		// Copy position
		int32 SectionVertIndex = Vertices->MoveNextOrAdd();

		Vertices->SetPosition(PosBuffer->VertexPosition(MeshVertIndex));
		Vertices->SetNormal(VertBuffer->VertexTangentZ(MeshVertIndex));
		Vertices->SetTangent(VertBuffer->VertexTangentX(MeshVertIndex));
		if (ColorBuffer && ColorBuffer->GetNumVertices())
		{
			Vertices->SetColor(ColorBuffer->VertexColor(MeshVertIndex));
		}

		// copy all uv channels
		for (uint32 Index = 0; Index < VertBuffer->GetNumTexCoords(); Index++)
		{
			Vertices->SetUV(Index, VertBuffer->GetVertexUV(MeshVertIndex, Index));
		}

		MeshToSectionVertMap.Add(MeshVertIndex, SectionVertIndex);

		return SectionVertIndex;
	}
}

void URuntimeMeshLibrary::GetSectionFromStaticMesh(UStaticMesh* InMesh, int32 LODIndex, int32 SectionIndex,
	IRuntimeMeshVerticesBuilder* Vertices, FRuntimeMeshIndicesBuilder* Triangles, FRuntimeMeshIndicesBuilder* AdjacencyTriangles)
{
	if (InMesh)
	{
#if !WITH_EDITOR && (ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13)
		if (!InMesh->bAllowCPUAccess)
		{
			FMessageLog("PIE").Warning()
				->AddToken(FTextToken::Create(LOCTEXT("GetSectionFromStaticMeshStart", "Calling GetSectionFromStaticMesh on")))
				->AddToken(FUObjectToken::Create(InMesh))
				->AddToken(FTextToken::Create(LOCTEXT("GetSectionFromStaticMeshEnd", "but 'Allow CPU Access' is not enabled. This is required for converting StaticMesh to RuntimeMeshComponent in cooked builds.")));
		}
		else 
#endif
#if WITH_EDITOR || (ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13)
		if (InMesh->RenderData != nullptr && InMesh->RenderData->LODResources.IsValidIndex(LODIndex))
		{
			const FStaticMeshLODResources& LOD = InMesh->RenderData->LODResources[LODIndex];
			if (LOD.Sections.IsValidIndex(SectionIndex))
			{
				// Empty output buffers
				Vertices->Reset();
				Triangles->Reset();

				// Map from vert buffer for whole mesh to vert buffer for section of interest
				TMap<int32, int32> MeshToSectionVertMap;

				const FStaticMeshSection& Section = LOD.Sections[SectionIndex];
				const uint32 OnePastLastIndex = Section.FirstIndex + Section.NumTriangles * 3;
				FIndexArrayView Indices = LOD.IndexBuffer.GetArrayView();


				// Iterate over section index buffer, copying verts as needed
				for (uint32 i = Section.FirstIndex; i < OnePastLastIndex; i++)
				{
					uint32 MeshVertIndex = Indices[i];

					// See if we have this vert already in our section vert buffer, and copy vert in if not 
					int32 SectionVertIndex = GetNewIndexForOldVertIndex(MeshVertIndex, MeshToSectionVertMap, &LOD.PositionVertexBuffer, &LOD.VertexBuffer, &LOD.ColorVertexBuffer, Vertices);

					// Add to index buffer
					Triangles->AddIndex(SectionVertIndex);
				}

				if (AdjacencyTriangles != nullptr)
				{
					AdjacencyTriangles->Reset();

					// Adjacency indices use 12 per triangle instead of 3. So start position and length both need to be multiplied by 4
					const uint32 SectionAdjacencyFirstIndex = Section.FirstIndex * 4;
					const uint32 SectionAdjacencyOnePastLastIndex = SectionAdjacencyFirstIndex + Section.NumTriangles * (3 * 4);
					FIndexArrayView AdjacencyIndices = LOD.AdjacencyIndexBuffer.GetArrayView();

					// Iterate over section adjacency index buffer, copying any new verts as needed
					for (uint32 i = SectionAdjacencyFirstIndex; i < SectionAdjacencyOnePastLastIndex; i++)
					{
						uint32 MeshVertIndex = AdjacencyIndices[i];

						// See if we have this vert already in our section vert buffer, and copy vert in if not 
						int32 SectionVertIndex = GetNewIndexForOldVertIndex(MeshVertIndex, MeshToSectionVertMap, &LOD.PositionVertexBuffer, &LOD.VertexBuffer, &LOD.ColorVertexBuffer, Vertices);

						// Add to index buffer
						AdjacencyTriangles->AddIndex(SectionVertIndex);
					}
				}
			}
		}
#endif
	}
}

void URuntimeMeshLibrary::GetSectionFromStaticMesh(UStaticMesh* InMesh, int32 LODIndex, int32 SectionIndex, TArray<FVector>& Vertices,
	TArray<int32>& Triangles, TArray<FVector>& Normals, TArray<FVector2D>& UVs, TArray<FRuntimeMeshTangent>& Tangents)
{
	FRuntimeMeshComponentVerticesBuilder VerticesBuilder(&Vertices, &Normals, &Tangents, nullptr, &UVs);
	FRuntimeMeshIndicesBuilder IndicesBuilder(&Triangles);

	GetSectionFromStaticMesh(InMesh, LODIndex, SectionIndex, &VerticesBuilder, &IndicesBuilder, nullptr);
}

void URuntimeMeshLibrary::CopyRuntimeMeshFromStaticMeshComponent(UStaticMeshComponent* StaticMeshComp, int32 LODIndex,
	URuntimeMeshComponent* RuntimeMeshComp, bool bShouldCreateCollision)
{
#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 14
	UStaticMesh* StaticMesh = StaticMeshComp->GetStaticMesh();
#else
	UStaticMesh* StaticMesh = StaticMeshComp->StaticMesh;
#endif

	if (StaticMeshComp != nullptr && StaticMesh != nullptr && RuntimeMeshComp != nullptr)
	{
		//// MESH DATA

		// Make sure LOD index is valid
		if (StaticMesh->RenderData == nullptr || !StaticMesh->RenderData->LODResources.IsValidIndex(LODIndex))
		{
			return;
		}

		// Get specified LOD
		const FStaticMeshLODResources& LOD = StaticMesh->RenderData->LODResources[LODIndex];

#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION >= 13
		int32 NumSections = StaticMesh->GetNumSections(LODIndex);
#else
		int32 NumSections = LOD.Sections.Num();
#endif
		for (int32 SectionIndex = 0; SectionIndex < NumSections; SectionIndex++)
		{
			// Buffers for copying geom data
			if (LOD.Sections.IsValidIndex(SectionIndex))
			{
				int32 NumUVChannels = LOD.GetNumTexCoords();

				FRuntimeMeshIndicesBuilder AdjacencyTriangles;

				if (NumUVChannels <= 1)
				{
					FRuntimeMeshPackedVerticesBuilder<FRuntimeMeshVertexSimple> Vertices;
					FRuntimeMeshIndicesBuilder Triangles;

					// Get geom data from static mesh
					GetSectionFromStaticMesh(StaticMesh, LODIndex, SectionIndex, &Vertices, &Triangles, &AdjacencyTriangles);

					// Create section using data
					RuntimeMeshComp->CreateMeshSection(SectionIndex, *Vertices.GetVertices(), *Triangles.GetIndices(),
						bShouldCreateCollision, EUpdateFrequency::Infrequent, ESectionUpdateFlags::MoveArrays);
				}
				else
				{
					FRuntimeMeshPackedVerticesBuilder<FRuntimeMeshVertexDualUV> Vertices;
					FRuntimeMeshIndicesBuilder Triangles;

					// Get geom data from static mesh
					GetSectionFromStaticMesh(StaticMesh, LODIndex, SectionIndex, &Vertices, &Triangles, &AdjacencyTriangles);

					// Create section using data
					RuntimeMeshComp->CreateMeshSection(SectionIndex, *Vertices.GetVertices(), *Triangles.GetIndices(),
						bShouldCreateCollision, EUpdateFrequency::Infrequent, ESectionUpdateFlags::MoveArrays);

				}

			}
		}

		//// SIMPLE COLLISION

		// Clear any existing collision hulls
		RuntimeMeshComp->ClearCollisionConvexMeshes();

		if (StaticMesh->BodySetup != nullptr)
		{
			// Iterate over all convex hulls on static mesh..
			const int32 NumConvex = StaticMesh->BodySetup->AggGeom.ConvexElems.Num();
			for (int ConvexIndex = 0; ConvexIndex < NumConvex; ConvexIndex++)
			{
				// Copy convex verts to ProcMesh
				FKConvexElem& MeshConvex = StaticMesh->BodySetup->AggGeom.ConvexElems[ConvexIndex];
				RuntimeMeshComp->AddCollisionConvexMesh(MeshConvex.VertexData);
			}
		}

		//// MATERIALS

		for (int32 MatIndex = 0; MatIndex < StaticMeshComp->GetNumMaterials(); MatIndex++)
		{
			RuntimeMeshComp->SetMaterial(MatIndex, StaticMeshComp->GetMaterial(MatIndex));
		}
	}
}





#undef LOCTEXT_NAMESPACE