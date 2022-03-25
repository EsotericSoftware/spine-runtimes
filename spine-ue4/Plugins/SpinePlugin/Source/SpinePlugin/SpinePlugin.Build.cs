using System;
using System.IO;

namespace UnrealBuildTool.Rules
{
	public class SpinePlugin : ModuleRules
	{
		public SpinePlugin(ReadOnlyTargetRules target) : base(target)
		{
			PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public/spine-cpp/include"));

			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Private"));
			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Public/spine-cpp/include"));

            PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "ProceduralMeshComponent", "UMG", "Slate", "SlateCore" });
            PublicDefinitions.Add("SPINE_UE4");
		}
	}
}
