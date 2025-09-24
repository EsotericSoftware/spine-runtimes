
/** Represents the File System object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/file-system | IFileSystemObjectType documentation } */

interface FSAcceptType {
	description: string,
	accept: {
		[key: string]: string[]
	}
}

type FSStartInLocation = "default" | "desktop" | "documents" | "downloads" | "music" | "pictures" | "videos";
type FSFolderPickerMode = "read" | "readwrite";
type FSWriteFileMode = "overwrite" | "append";
type FSReadFileMode = "text" | "binary";

interface FSSaveFilePickerOpts {
	pickerTag: string;
	acceptTypes?: FSAcceptType[];
	showAcceptAll?: boolean;
	suggestedName?: string;
	id?: string;
	startIn?: FSStartInLocation;
}

interface FSOpenFilePickerOpts {
	pickerTag: string;
	acceptTypes?: FSAcceptType[];
	showAcceptAll?: boolean;
	multiple?: boolean;
	id?: string;
	startIn?: FSStartInLocation
}

interface FSFolderPickerOpts {
	pickerTag: string;
	mode?: FSFolderPickerMode;
	id?: string;
	startIn?: FSStartInLocation
}

interface FSWriteFileOpts {
	pickerTag: string;
	folderPath?: string;
	fileTag?: string;
	data: string | ArrayBuffer;
	mode?: FSWriteFileMode
}

interface FSReadFileOpts {
	pickerTag: string;
	folderPath?: string;
	fileTag?: string;
	mode: FSReadFileMode;
}

interface FSListContentResult {
	files: string[],
	folders: string[]
}

interface FSDropEvent extends ConstructEvent {
	files: File[]
}

interface FSObjectTypeEventMap<InstanceType = IInstance> extends ObjectClassEventMap<InstanceType> {
	"drop": FSDropEvent
}

declare class IFileSystemObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	addEventListener<K extends keyof FSObjectTypeEventMap<InstType>>(type: K, listener: (ev: FSObjectTypeEventMap<InstType>[K]) => any): void;
	removeEventListener<K extends keyof FSObjectTypeEventMap<InstType>>(type: K, listener: (ev: FSObjectTypeEventMap<InstType>[K]) => any): void;

	readonly isSupported: boolean;
	readonly desktopFeaturesSupported: boolean;

	hasPickerTag(pickerTag: string): boolean;

	showSaveFilePicker(opts: FSSaveFilePickerOpts): Promise<string[]>;
	showOpenFilePicker(opts: FSOpenFilePickerOpts): Promise<string[]>;
	showFolderPicker(opts: FSFolderPickerOpts): Promise<string[]>;

	writeFile(opts: FSWriteFileOpts): Promise<void>;
	readFile(opts: FSReadFileOpts): Promise<string | ArrayBuffer>;

	createFolder(pickerTag: string, folderPath: string, fileTag?: string): Promise<void>;
	copyFile(pickerTag: string, srcFolderPath: string, destFolderPath: string, fileTag?: string): Promise<void>;
	moveFile(pickerTag: string, srcFolderPath: string, destFolderPath: string, fileTag?: string): Promise<void>;
	delete(pickerTag: string, folderPath?: string, isRecursive?: boolean, fileTag?: string): Promise<void>;
	listContent(pickerTag: string, folderPath?: string, isRecursive?: boolean, fileTag?: string): Promise<FSListContentResult>;

	shellOpen(pickerTag: string, filePath?: string, fileTag?: string): Promise<void>;
	runFile(pickerTag: string, filePath?: string, args?: string, fileTag?: string): Promise<void>;
}
