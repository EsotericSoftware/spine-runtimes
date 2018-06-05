// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

namespace UnrealBuildTool.Rules
{
	public class SpineEditorPlugin : ModuleRules
	{
		public SpineEditorPlugin(ReadOnlyTargetRules Target) : base(Target)
		{
			PublicIncludePaths.AddRange(new string[] { "SpineEditorPlugin/Public", "SpinePlugin/Public/spine-cpp/include" });
            
            PrivateIncludePaths.AddRange(new string[] { "SpineEditorPlugin/Private", "SpinePlugin/Public/spine-cpp/include" });

            PublicDependencyModuleNames.AddRange(new string[] {
                "Core",
                "CoreUObject",
                "Engine",
                "UnrealEd",
                "SpinePlugin"
            });
            
            PublicIncludePathModuleNames.AddRange(new string[] {
               "AssetTools",
               "AssetRegistry"
            });
            
            DynamicallyLoadedModuleNames.AddRange(new string[] {
               "AssetTools",
               "AssetRegistry"
            });
		}
	}
}
