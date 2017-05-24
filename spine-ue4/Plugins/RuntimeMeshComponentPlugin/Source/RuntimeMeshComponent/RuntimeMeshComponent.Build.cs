// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

using UnrealBuildTool;

public class RuntimeMeshComponent : ModuleRules
{
	public RuntimeMeshComponent(TargetInfo Target)
	{
        PrivateIncludePaths.Add("RuntimeMeshComponent/Private");
        PublicIncludePaths.Add("RuntimeMeshComponent/Public");

        PublicDependencyModuleNames.AddRange(
                new string[]
                {
                        "Core",
                        "CoreUObject",
                        "Engine",
                        "RenderCore",
                        "ShaderCore",
                        "RHI"
                }
            );
    }
}
