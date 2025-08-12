
type BehaviorInfoCategory = "attributes" | "general" | "movements" | "other";

declare namespace SDK {
	class IBehaviorInfo {
		SetName(v: string): void;
		SetDescription(v: string): void;
		SetVersion(v: string): void;
		SetCategory(v: BehaviorInfoCategory): void;
		SetAuthor(v: string): void;
		SetHelpUrl(v: string): void;
		SetIcon(url: string, type: string): void;

		SetIsOnlyOneAllowed(o: boolean): void;
		SetIsDeprecated(d: boolean): void;
		SetCanBeBundled(b: boolean): void;

		SetProperties(arr: SDK.PluginProperty[]): void;

		AddCordovaPluginReference(o: PluginInfoCordovaPluginReference): void;
		AddFileDependency(o: PluginInfoFileDependency): void;
		AddRemoteScriptDependency(url: string, type?: PluginInfoScriptType): void;

		SetRuntimeModuleMainScript(path: string): void;
		AddC3RuntimeScript(path: string): void;
		SetC3RuntimeScripts(arr: string[]): void;
		SetTypeScriptDefinitionFiles(arr: string[]): void;
		SetScriptInterfaceNames(o: { instance?: string, behaviorType?: string, behavior?: string }): void;
	}
}