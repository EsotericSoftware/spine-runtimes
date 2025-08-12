
interface ButtonInstanceEventMap<InstType = IButtonInstance> extends WorldInstanceEventMap<InstType> {
	"click": InstanceEvent<InstType>;
}

/** Represents the Button object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/button | IButtonInstance documentation } */
declare class IButtonInstance extends IDOMInstance
{
	addEventListener<K extends keyof ButtonInstanceEventMap<this>>(type: K, listener: (ev: ButtonInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof ButtonInstanceEventMap<this>>(type: K, listener: (ev: ButtonInstanceEventMap<this>[K]) => any): void;

	text: string;
	tooltip: string;
	isEnabled: boolean;
	isChecked: boolean;
}
