declare namespace SDK {
	class IInstanceBase {

		constructor(sdkType: SDK.ITypeBase, iInstance: SDK.IObjectInstance);
		
		_sdkType: SDK.ITypeBase;
		_inst: SDK.IObjectInstance;

		Release(): void;
		OnCreate(): void;
		OnAfterCreate(): void;
		OnPropertyChanged(id: string, value: EditorPropertyValueType): void;
		OnTimelinePropertyChanged(id: string, value: number | string, detail: { resultMode: "relative" | "absolute" }): void;
		OnExitTimelineEditMode(): void;

		LoadC2Property(name: string, valueString: string): boolean;
		GetObjectType(): SDK.IObjectType;
		GetProject(): SDK.IProject;
		GetInstance(): SDK.IObjectInstance;
	}
}
