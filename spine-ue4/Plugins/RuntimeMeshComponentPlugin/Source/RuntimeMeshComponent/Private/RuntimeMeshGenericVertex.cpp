// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#include "RuntimeMeshComponentPluginPrivatePCH.h"
#include "RuntimeMeshGenericVertex.h"
#include "RuntimeMeshBuilder.h"

// Finish all the built in vertex types.
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexSimple);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexDualUV);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexTripleUV);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexNoPosition);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexNoPositionDualUV);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexHiPrecisionNormals);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexDualUVHiPrecisionNormals);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexNoPositionHiPrecisionNormals);
DEFINE_RUNTIME_MESH_VERTEX(FRuntimeMeshVertexNoPositionDualUVHiPrecisionNormals);




const FRuntimeMeshVertexTypeInfo* FRuntimeMeshComponentVerticesBuilder::GetVertexType() const
{
	if (HasUVComponent(1))
	{
		if (HasUVComponent(2)) return &FRuntimeMeshVertexTripleUV::TypeInfo;
		else return &FRuntimeMeshVertexDualUV::TypeInfo;
	}
	else
	{
		return &FRuntimeMeshVertexSimple::TypeInfo;
	}
}