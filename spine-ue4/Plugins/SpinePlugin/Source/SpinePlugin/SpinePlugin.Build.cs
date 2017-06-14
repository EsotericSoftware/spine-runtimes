// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

namespace UnrealBuildTool.Rules
{
	public class SpinePlugin : ModuleRules
	{
		public SpinePlugin(ReadOnlyTargetRules Target) : base(Target)
		{
			PublicIncludePaths.AddRange(new string[] { "SpinePlugin/Public", "SpinePlugin/Public/spine-c/include" });
            PrivateIncludePaths.AddRange(new string[] { "SpinePlugin/Private", "SpinePlugin/Public/spine-c/include" });
            PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "RHI", "RenderCore", "ShaderCore", "ProceduralMeshComponent", "RuntimeMeshComponent" });
            OptimizeCode = CodeOptimization.Never;
		}
	}
}
