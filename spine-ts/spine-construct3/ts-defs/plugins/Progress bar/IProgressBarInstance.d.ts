
interface ProgressInstanceEventMap<InstType = IProgressBarInstance> extends WorldInstanceEventMap<InstType> {
	"click": InstanceEvent<InstType>;
}
/** Represents the Progress Bar object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/progress-bar | IProgressBarInstance documentation } */
declare class IProgressBarInstance extends IDOMInstance
{
	addEventListener<K extends keyof ProgressInstanceEventMap<this>>(type: K, listener: (ev: ProgressInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof ProgressInstanceEventMap<this>>(type: K, listener: (ev: ProgressInstanceEventMap<this>[K]) => any): void;

	progress: number;
	maximum: number;
	tooltip: string;
	setIndeterminate(): void;
}
