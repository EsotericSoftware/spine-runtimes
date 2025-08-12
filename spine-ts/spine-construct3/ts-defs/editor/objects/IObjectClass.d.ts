declare namespace SDK {
	class IObjectClass {
		GetProject(): SDK.IProject;
		GetName(): string;
		Delete(): void;
	}
}