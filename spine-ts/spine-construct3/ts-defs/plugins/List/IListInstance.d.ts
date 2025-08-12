
interface ListInstanceEventMap<InstType = IListInstance> extends WorldInstanceEventMap<InstType> {
	"click": InstanceEvent<InstType>;
	"dblclick": InstanceEvent<InstType>;
	"selectionchange": InstanceEvent<InstType>;
}

/** Represents the List object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/list | IListInstance documentation } */
declare class IListInstance extends IDOMInstance
{
	addEventListener<K extends keyof ListInstanceEventMap<this>>(type: K, listener: (ev: ListInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof ListInstanceEventMap<this>>(type: K, listener: (ev: ListInstanceEventMap<this>[K]) => any): void;

	tooltip: string;
	readonly itemCount: number;

	addItem(text: string): void;
	insertItem(index: number, text: string): void;
	setItemText(index: number, text: string): void;
	getItemText(index: number): string;
	removeItem(index: number): void;
	clear(): void;

	selectedIndex: number;
	readonly selectedCount: number;
	getSelectedIndexAt(index: number): number;
	getSelectedTextAt(index: number): string;
}
