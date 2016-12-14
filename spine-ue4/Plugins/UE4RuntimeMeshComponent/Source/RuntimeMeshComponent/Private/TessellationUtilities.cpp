// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#include "RuntimeMeshComponentPluginPrivatePCH.h"
#include "TessellationUtilities.h"

const uint32 EdgesPerTriangle = 3;
const uint32 IndicesPerTriangle = 3;
const uint32 VerticesPerTriangle = 3;
const uint32 DuplicateIndexCount = 3;

const uint32 PnAenDomCorner_IndicesPerPatch = 12;


void TessellationUtilities::AddIfLeastUV(PositionDictionary& PosDict, const Vertex& Vert, uint32 Index)
{
	auto* Pos = PosDict.Find(Vert.Position);
	if (Pos == nullptr)
	{
		PosDict.Add(Vert.Position, Corner(Index, Vert.TexCoord));
	}
	else if (Vert.TexCoord < Pos->TexCoord)
	{
		PosDict[Vert.Position] = Corner(Index, Vert.TexCoord);
	}
}


void TessellationUtilities::CalculateTessellationIndices(const IRuntimeMeshVerticesBuilder* Vertices, const FRuntimeMeshIndicesBuilder* Indices, FRuntimeMeshIndicesBuilder* TessellationIndices)
{
	EdgeDictionary EdgeDict;
	EdgeDict.Reserve(Indices->Length());
	PositionDictionary PosDict;
	PosDict.Reserve(Indices->Length());

	TessellationIndices->Reset(PnAenDomCorner_IndicesPerPatch * Indices->Length() / IndicesPerTriangle);
	
	ExpandIB(Vertices, Indices, EdgeDict, PosDict, TessellationIndices);

	ReplacePlaceholderIndices(Vertices, Indices, EdgeDict, PosDict, TessellationIndices);
}


void TessellationUtilities::ExpandIB(const IRuntimeMeshVerticesBuilder* Vertices, const FRuntimeMeshIndicesBuilder* Indices,
	EdgeDictionary& OutEdgeDict, PositionDictionary& OutPosDict, FRuntimeMeshIndicesBuilder* OutIndices)
{
	const uint32 TriangleCount = Indices->Length() / IndicesPerTriangle;

	for (uint32 U = 0; U < TriangleCount; U++)
	{
		const uint32 StartInIndex = U * IndicesPerTriangle;
		const uint32 StartOutIndex = U * PnAenDomCorner_IndicesPerPatch;

		Indices->Seek(StartInIndex);
		const uint32 Index0 = Indices->ReadOne();
		const uint32 Index1 = Indices->ReadOne();
		const uint32 Index2 = Indices->ReadOne();

		Vertices->Seek(Index0);
		const Vertex Vertex0(Vertices->GetPosition(), Vertices->GetUV(0));
		Vertices->Seek(Index1);
		const Vertex Vertex1(Vertices->GetPosition(), Vertices->GetUV(0));
		Vertices->Seek(Index2);
		const Vertex Vertex2(Vertices->GetPosition(), Vertices->GetUV(0));

		Triangle Tri(Index0, Index1, Index2, Vertex0, Vertex1, Vertex2);

		OutIndices->Seek(StartOutIndex);
		OutIndices->AddTriangle(Tri.GetIndex(0), Tri.GetIndex(1), Tri.GetIndex(2));

		OutIndices->AddIndex(Tri.GetIndex(0));
		OutIndices->AddIndex(Tri.GetIndex(1));

		OutIndices->AddIndex(Tri.GetIndex(1));
		OutIndices->AddIndex(Tri.GetIndex(2));

		OutIndices->AddIndex(Tri.GetIndex(2));
		OutIndices->AddIndex(Tri.GetIndex(0));

		OutIndices->AddTriangle(Tri.GetIndex(0), Tri.GetIndex(1), Tri.GetIndex(2));

		
		Edge Rev0 = Tri.GetEdge(0).GetReverse();
		Edge Rev1 = Tri.GetEdge(1).GetReverse();
		Edge Rev2 = Tri.GetEdge(2).GetReverse();

		OutEdgeDict.Add(Rev0, Rev0);
		OutEdgeDict.Add(Rev1, Rev1);
		OutEdgeDict.Add(Rev2, Rev2);

		AddIfLeastUV(OutPosDict, Vertex0, Index0);
		AddIfLeastUV(OutPosDict, Vertex1, Index1);
		AddIfLeastUV(OutPosDict, Vertex2, Index2);
	}
}



void TessellationUtilities::ReplacePlaceholderIndices(const IRuntimeMeshVerticesBuilder* Vertices, const FRuntimeMeshIndicesBuilder* Indices,
	EdgeDictionary& EdgeDict, PositionDictionary& PosDict, FRuntimeMeshIndicesBuilder* OutIndices)
{
	const uint32 TriangleCount = Indices->Length() / PnAenDomCorner_IndicesPerPatch;

	for (uint32 U = 0; U < TriangleCount; U++)
	{
		const uint32 StartOutIndex = U * PnAenDomCorner_IndicesPerPatch;

		OutIndices->Seek(StartOutIndex);
		const uint32 Index0 = OutIndices->ReadOne();
		const uint32 Index1 = OutIndices->ReadOne();
		const uint32 Index2 = OutIndices->ReadOne();

		Vertices->Seek(Index0);
		const Vertex Vertex0(Vertices->GetPosition(), Vertices->GetUV(0));
		Vertices->Seek(Index1);
		const Vertex Vertex1(Vertices->GetPosition(), Vertices->GetUV(0));
		Vertices->Seek(Index2);
		const Vertex Vertex2(Vertices->GetPosition(), Vertices->GetUV(0));

		Triangle Tri(Index0, Index1, Index2, Vertex0, Vertex1, Vertex2);

		Edge* Ed = EdgeDict.Find(Tri.GetEdge(0));
		if (Ed != nullptr)
		{
			OutIndices->Seek(StartOutIndex + 3);
			OutIndices->AddIndex(Ed->GetIndex(0));
			OutIndices->AddIndex(Ed->GetIndex(1));
		}

		Ed = EdgeDict.Find(Tri.GetEdge(1));
		if (Ed != nullptr)
		{
			OutIndices->Seek(StartOutIndex + 5);
			OutIndices->AddIndex(Ed->GetIndex(0));
			OutIndices->AddIndex(Ed->GetIndex(1));
		}

		Ed = EdgeDict.Find(Tri.GetEdge(2));
		if (Ed != nullptr)
		{
			OutIndices->Seek(StartOutIndex + 7);
			OutIndices->AddIndex(Ed->GetIndex(0));
			OutIndices->AddIndex(Ed->GetIndex(1));
		}
		
		// Deal with dominant positions.
		for (uint32 V = 0; V < VerticesPerTriangle; V++)
		{
			Corner* Corn = PosDict.Find(Tri.GetEdge(V).GetVertex(0).Position);
			if (Corn != nullptr)
			{
				OutIndices->Seek(StartOutIndex + 9 + V);
				OutIndices->AddIndex(Corn->Index);
			}
		}
	}
}