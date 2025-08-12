
interface FileChooserChangeEvent<InstType = IFileChooserInstance> extends InstanceEvent<InstType> {
	files: File[];
}

interface FileChooserInstanceEventMap<InstType = IFileChooserInstance> extends WorldInstanceEventMap<InstType> {
	"change": FileChooserChangeEvent<InstType>;
}

/** Represents the File Chooser object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/file-chooser | IFileChooserInstance documentation } */
declare class IFileChooserInstance extends IDOMInstance
{
	addEventListener<K extends keyof FileChooserInstanceEventMap<this>>(type: K, listener: (ev: FileChooserInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof FileChooserInstanceEventMap<this>>(type: K, listener: (ev: FileChooserInstanceEventMap<this>[K]) => any): void;

	click(): void;
	clear(): void;

	getFiles(): File[];
}
