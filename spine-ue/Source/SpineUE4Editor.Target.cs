using UnrealBuildTool;

public class SpineUE4EditorTarget : TargetRules
{
	public SpineUE4EditorTarget(TargetInfo target) : base(target)
	{
		DefaultBuildSettings = BuildSettingsVersion.V2;
		Type = TargetType.Editor;
		ExtraModuleNames.AddRange(new string[] { "SpineUE4" });
	}
}
