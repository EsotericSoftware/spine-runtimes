using System;
using System.IO;

namespace UnrealBuildTool.Rules
{
	public class SpinePlugin : ModuleRules
	{
		public SpinePlugin(ReadOnlyTargetRules Target) : base(Target)
		{
            PrivatePCHHeaderFile = "Private/SpinePluginPrivatePCH.h";
			PCHUsage = PCHUsageMode.UseSharedPCHs;
#if UE_4_24_OR_LATER
            DefaultBuildSettings = BuildSettingsVersion.V1;
#endif

			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
			PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public/spine-cpp/include"));

			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Private"));
			PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Public/spine-cpp/include"));

            PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "ProceduralMeshComponent", "UMG", "Slate", "SlateCore" });
			PublicDefinitions.Add("SPINE_UE4");
		}
	}
}
