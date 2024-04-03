using UnrealBuildTool;

public class SpineUEEditorTarget : TargetRules
{
	public SpineUEEditorTarget(TargetInfo target) : base(target)
	{
		DefaultBuildSettings = BuildSettingsVersion.V2;
		Type = TargetType.Editor;
		ExtraModuleNames.AddRange(new string[] { "SpineUE" });
	}
}
