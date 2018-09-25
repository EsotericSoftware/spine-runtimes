// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

using System;
using System.IO;

namespace UnrealBuildTool.Rules
{
	public class SpinePlugin : ModuleRules
	{
		public SpinePlugin(ReadOnlyTargetRules Target) : base(Target)
		{
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public/spine-cpp/include"));

			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Public/spine-cpp/include"));

            PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "RHI", "RenderCore", "ShaderCore", "ProceduralMeshComponent", "UMG", "Slate", "SlateCore" });
			PublicDefinitions.Add("SPINE_UE4");

			// For UE 4.19 and below comment the line above and uncomment the line
			// below.
			// Definitions.Add("SPINE_UE4");
		}
	}
}
