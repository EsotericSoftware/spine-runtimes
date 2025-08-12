declare namespace SDK {
	class IZipFile {
		PathExists(path: string): boolean;
		GetFileList(): string[];
		GetFirstEntryWithExtension(ext: string): SDK.IZipFileEntry | null;
		GetEntry(path: string): SDK.IZipFileEntry | null;

		ReadText(entry: SDK.IZipFileEntry): Promise<string>;
		ReadJson(entry: SDK.IZipFileEntry): Promise<JSONValue>;
		ReadBlob(entry: SDK.IZipFileEntry): Promise<Blob>;
	}
}