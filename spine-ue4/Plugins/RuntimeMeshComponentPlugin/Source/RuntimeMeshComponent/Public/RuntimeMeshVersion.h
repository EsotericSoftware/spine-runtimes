// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#pragma once

#include "RuntimeMeshComponentPlugin.h"


// Custom version for runtime mesh serialization
namespace FRuntimeMeshVersion
{
	enum Type
	{
		Initial = 0,
		TemplatedVertexFix = 1,
		SerializationOptional = 2,
		DualVertexBuffer = 3,

		SerializationV2 = 4,

		// -----<new versions can be added above this line>-------------------------------------------------
		VersionPlusOne,
		LatestVersion = VersionPlusOne - 1
	};

	// The GUID for this custom version
	const static FGuid GUID = FGuid(0xEE48714B, 0x8A2C4652, 0x98BE40E6, 0xCB7EF0E6);
};
