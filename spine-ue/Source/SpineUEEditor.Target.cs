using UnrealBuildTool;

public class SpineUEEditorTarget : TargetRules
{
	public SpineUEEditorTarget(TargetInfo target) : base(target)
	{
		DefaultBuildSettings = BuildSettingsVersion.Latest;
		Type = TargetType.Editor;
		ExtraModuleNames.AddRange(new string[] { "SpineUE" });
	}
}
