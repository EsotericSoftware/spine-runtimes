
type EditorContainerSelectMode = "normal" | "all" | "wrap";

declare namespace SDK {
	class IContainer {
		IsActive(): boolean;

		GetMembers(): SDK.IObjectType[];
		RemoveObjectType(iObjectType: SDK.IObjectType): void;

		SetSelectMode(mode: EditorContainerSelectMode): void;
		GetSelectMode(): EditorContainerSelectMode;
	}
}