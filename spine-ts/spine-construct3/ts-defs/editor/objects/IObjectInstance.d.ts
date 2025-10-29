declare namespace SDK {
	class IObjectInstance {
		GetProject(): SDK.IProject;
		GetObjectType(): SDK.IObjectType;

		GetUID(): number;

		SetPropertyValue(id: string, value: EditorPropertyValueType): void;
		GetPropertyValue(id: string): EditorPropertyValueType;
		
		GetTimelinePropertyValue(id: string): EditorPropertyValueType;

		GetExternalSdkInstance(): SDK.IInstanceBase | null;
	}
}