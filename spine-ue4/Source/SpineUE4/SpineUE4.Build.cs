using UnrealBuildTool;

public class SpineUE4 : ModuleRules
{
	public SpineUE4(ReadOnlyTargetRules Target) : base(Target)
	{
        PrivatePCHHeaderFile = "SpineUE4.h";
        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "SpinePlugin", "ProceduralMeshComponent" });
		PrivateDependencyModuleNames.AddRange(new string[] {  });		
	}
}
