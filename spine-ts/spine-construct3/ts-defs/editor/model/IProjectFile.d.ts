declare namespace SDK {
	class IProjectFile {
		GetName(): string;
		GetPath(): string;
		GetProject(): SDK.IProjectFile;
		GetBlob(): Blob;
	}
}