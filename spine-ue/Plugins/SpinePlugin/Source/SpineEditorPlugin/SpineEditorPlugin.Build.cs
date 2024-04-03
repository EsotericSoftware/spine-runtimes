using System.IO;

namespace UnrealBuildTool.Rules
{
	public class SpineEditorPlugin : ModuleRules
	{
		public SpineEditorPlugin(ReadOnlyTargetRules target) : base(target)
		{
			PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

            PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "../SpinePlugin/Public/spine-cpp/include"));

			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Private"));
			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "../SpinePlugin/Public/spine-cpp/include"));

            PublicDependencyModuleNames.AddRange(new [] {
                "Core",
                "CoreUObject",
                "Engine",
                "UnrealEd",
                "SpinePlugin"
            });

            PublicIncludePathModuleNames.AddRange(new [] {
               "AssetTools",
               "AssetRegistry"
            });

            DynamicallyLoadedModuleNames.AddRange(new [] {
               "AssetTools",
               "AssetRegistry"
            });
		}
	}
}
