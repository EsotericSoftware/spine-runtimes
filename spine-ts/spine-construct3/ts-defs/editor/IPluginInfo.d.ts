
type PluginInfoCategory = "3d" | "data-and-storage" | "html-elements" | "general" | "input" | "media" | "monetisation" | "platform-specific" | "web" | "other";
type PluginInfoPluginType = "object" | "world";
type PluginInfoScriptType = "" | "module";

interface PluginInfoCordovaPluginReference {
	id: string,
	version?: string,
	platform?: "all" | "ios" | "android",
	plugin?: SDK.IPluginBase,
	variables?: Array<[string, SDK.PluginProperty]>
}

interface PluginInfoCordovaResourceFile {
	src: string,
	target?: string,
	platform?: "all" | "ios" | "android"
}

interface PluginInfoFileDependency {
	filename: string,
	fileType?: string,
	scriptType?: PluginInfoScriptType,
	type: "copy-to-output" | "external-dom-script" | "external-runtime-script" | "external-css" | "wrapper-extension",
	platform?: "all" | "windows-x86" | "windows-x64" | "windows-arm64" | "xbox-uwp-x64" | "macos-universal" | "linux-x64" | "linux-arm" | "linux-arm64"
}

declare namespace SDK {
	class IPluginInfo {
		SetName(v: string): void;
		SetDescription(v: string): void;
		SetVersion(v: string): void;
		SetCategory(v: PluginInfoCategory): void;
		SetAuthor(v: string): void;
		SetHelpUrl(v: string): void;
		SetPluginType(v: PluginInfoPluginType): void;
		SetIcon(url: string, type: string): void;

		SetIsResizable(v: boolean): void;
		SetIsRotatable(v: boolean): void;
		SetSupportsZElevation(v: boolean): void;
		SetHasImage(v: boolean): void;
		SetDefaultImageURL(v: string): void;
		SetHasAnimations(v: boolean): void;
		SetIsTiled(v: boolean): void;
		SetIsFont(v: boolean): void;
		SetHasTilemap(v: boolean): void;
		SetIsDeprecated(v: boolean): void;
		SetIsSingleGlobal(v: boolean): void;
		SetSupportsEffects(v: boolean): void;
		SetMustPreDraw(v: boolean): void;
		SetIs3D(v: boolean): void;
		SetSupportsColor(v: boolean): void;
		SetCanBeBundled(v: boolean): void;
		SetSupportsColor(v: boolean): void;

		AddCommonPositionACEs(): void;
		AddCommonSizeACEs(): void;
		AddCommonAngleACEs(): void;
		AddCommonAppearanceACEs(): void;
		AddCommonZOrderACEs(): void;
		AddCommonSceneGraphACEs(): void;

		SetProperties(arr: SDK.PluginProperty[]): void;

		AddCordovaPluginReference(o: PluginInfoCordovaPluginReference): void;
		AddCordovaResourceFile(o: PluginInfoCordovaResourceFile): void;
		AddFileDependency(o: PluginInfoFileDependency): void;
		AddRemoteScriptDependency(url: string, type?: PluginInfoScriptType): void;
		SetGooglePlayServicesEnabled(e: boolean): void;
		SetWrapperExportProperties(componentId: string, propertyIds: string[]): void;

		SetRuntimeModuleMainScript(path: string): void;
		AddC3RuntimeScript(path: string): void;
		SetC3RuntimeScripts(arr: string[]): void;
		SetDOMSideScripts(arr: string[]): void;
		SetTypeScriptDefinitionFiles(arr: string[]): void;
		SetScriptInterfaceNames(o: { instance?: string, objectType?: string, plugin?: string }): void;
	}
}