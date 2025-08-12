declare namespace SDK {
	class IEventSheet {
		GetName(): string;
		GetProject(): SDK.IProject;
		GetRoot(): SDK.IEventParentRow;
	}
}