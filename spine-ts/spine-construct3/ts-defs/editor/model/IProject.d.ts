type EditorProjectFileKind = "general" | "sound" | "music" | "video" | "font" | "icon";

declare namespace SDK {
	class IProject {
		GetName(): string;

		GetSystemType(): SDK.IObjectType;
		GetSingleGlobalObjectType(pluginId: string): SDK.IObjectType | null;
		CreateObjectType(pluginId: string, name: string): Promise<SDK.IObjectType>;
		CreateFamily(name: string, members: SDK.IObjectType[]): SDK.IFamily;
		GetObjectTypeByName(name: string): SDK.IObjectType | null;
		GetFamilyByName(name: string): SDK.IFamily | null;
		GetObjectClassByName(name: string): SDK.IObjectClass | null;
		GetObjectClassBySID(sid: number): SDK.IObjectClass | null;
		GetInstanceByUID(uid: number): SDK.IObjectInstance | null;

		AddOrReplaceProjectFile(blob: Blob, filename: string, kind?: EditorProjectFileKind): void;
		GetProjectFileByName(name: string): SDK.IProjectFile | null;
		GetProjectFileByExportPath(path: string): SDK.IProjectFile | null;

		ShowImportAudioDialog(fileList: Blob[]): void;
		EnsureFontLoaded(fontName: string): Promise<void>;

		UndoPointChangeObjectInstancesProperty(instances: SDK.IObjectInstance | SDK.IObjectInstance[], propertyId: string): void;
	}
}