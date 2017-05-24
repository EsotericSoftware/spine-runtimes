// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

namespace UnrealBuildTool.Rules
{
	public class RuntimeMeshComponentEditor : ModuleRules
	{
        public RuntimeMeshComponentEditor(TargetInfo Target)
		{
			PrivateIncludePaths.Add("RuntimeMeshComponentEditor/Private");
            PublicIncludePaths.Add("RuntimeMeshComponentEditor/Public");

			PublicDependencyModuleNames.AddRange(
				new string[]
				{
					"Core",
					"CoreUObject",
                    "Slate",
                    "SlateCore",
                    "Engine",
                    "UnrealEd",
                    "PropertyEditor",
                    "RenderCore",
                    "ShaderCore",
                    "RHI",
                    "RuntimeMeshComponent",
                    "RawMesh",
                    "AssetTools",
                    "AssetRegistry"
                }
				);
		}
	}
}