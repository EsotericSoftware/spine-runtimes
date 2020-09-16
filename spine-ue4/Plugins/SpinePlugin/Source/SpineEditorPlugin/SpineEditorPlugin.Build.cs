using System;
using System.IO;

namespace UnrealBuildTool.Rules
{
	public class SpineEditorPlugin : ModuleRules
	{
		public SpineEditorPlugin(ReadOnlyTargetRules Target) : base(Target)
		{
            PrivatePCHHeaderFile = "Private/SpineEditorPluginPrivatePCH.h";
            PCHUsage = PCHUsageMode.UseSharedPCHs;
#if UE_4_24_OR_LATER
            DefaultBuildSettings = BuildSettingsVersion.V1;
#endif

            PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "../SpinePlugin/Public/spine-cpp/include"));

			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Private"));
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
