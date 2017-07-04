// Fill out your copyright notice in the Description page of Project Settings.

using UnrealBuildTool;

public class SpineUE4 : ModuleRules
{
	public SpineUE4(ReadOnlyTargetRules Target) : base(Target)
	{
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "SpinePlugin" });
		PublicIncludePaths.AddRange(new string[] { "SpinePlugin/Public", "SpinePlugin/Classes" });

		PrivateDependencyModuleNames.AddRange(new string[] {  });

		// Uncomment if you are using Slate UI
		// PrivateDependencyModuleNames.AddRange(new string[] { "Slate", "SlateCore" });
		
		// Uncomment if you are using online features
		// PrivateDependencyModuleNames.Add("OnlineSubsystem");

		// To include OnlineSubsystemSteam, add it to the plugins section in your uproject file with the Enabled attribute set to true
	}
}
