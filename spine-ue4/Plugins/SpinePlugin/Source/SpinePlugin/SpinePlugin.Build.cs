// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

namespace UnrealBuildTool.Rules
{
	public class SpinePlugin : ModuleRules
	{
		public SpinePlugin(ReadOnlyTargetRules Target) : base(Target)
		{
			PublicIncludePaths.AddRange(new string[] { "SpinePlugin/Public", "SpinePlugin/Public/spine-cpp/include" });
            PrivateIncludePaths.AddRange(new string[] { "SpinePlugin/Private", "SpinePlugin/Public/spine-cpp/include" });
            PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "RHI", "RenderCore", "ShaderCore", "ProceduralMeshComponent", "UMG", "Slate", "SlateCore" });
			Definitions.Add("SPINE_UE4");
			// In Unreal 4.20+, comment the above line, uncomment the below line
			// PublicDefinitions.Add("SPINE_UE4");
		}
	}
}
