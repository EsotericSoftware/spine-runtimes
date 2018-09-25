// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

using System;
using System.IO;

namespace UnrealBuildTool.Rules
{
	public class SpineEditorPlugin : ModuleRules
	{
		public SpineEditorPlugin(ReadOnlyTargetRules Target) : base(Target)
		{
            PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "../SpinePlugin/Public/spine-cpp/include"));

			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "../SpinePlugin/Public/spine-cpp/include"));

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
