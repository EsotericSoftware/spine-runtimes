declare namespace SDK {
	class IBehaviorInstance {
		GetProject(): SDK.IProject;

		GetPropertyValue(id: string): EditorPropertyValueType;
		SetPropertyValue(id: string, value: EditorPropertyValueType): void;

		GetObjectInstance(): SDK.IObjectInstance;

		GetExternalSdkInstance(): SDK.IBehaviorInstanceBase | null;
	}
}