// Fill out your copyright notice in the Description page of Project Settings.

using UnrealBuildTool;
using System.Collections.Generic;

public class SpineUETarget : TargetRules
{
	public SpineUETarget(TargetInfo Target) : base(Target)
	{
		DefaultBuildSettings = BuildSettingsVersion.Latest;
		Type = TargetType.Game;
		ExtraModuleNames.AddRange(new string[] { "SpineUE" });
	}
}
