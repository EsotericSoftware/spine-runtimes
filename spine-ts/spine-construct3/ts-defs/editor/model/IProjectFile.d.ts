declare namespace SDK {
	class IProjectFile {
		GetName(): string;
		GetProject(): SDK.IProjectFile;
		GetBlob(): Blob;
	}
}