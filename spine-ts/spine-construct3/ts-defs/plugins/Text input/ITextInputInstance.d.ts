
interface TextInputInstanceEventMap<InstType = ITextInputInstance> extends WorldInstanceEventMap<InstType> {
	"click": InstanceEvent<InstType>;
	"dblclick": InstanceEvent<InstType>;
	"change": InstanceEvent<InstType>;
}

/** Represents the Text Input object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/text-input | ITextInputInstance documentation } */
declare class ITextInputInstance extends IDOMInstance
{
	addEventListener<K extends keyof TextInputInstanceEventMap<this>>(type: K, listener: (ev: TextInputInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof TextInputInstanceEventMap<this>>(type: K, listener: (ev: TextInputInstanceEventMap<this>[K]) => any): void;

	text: string;
	placeholder: string;
	tooltip: string;
	isEnabled: boolean;
	isReadOnly: boolean;
	maxLength: number;

	scrollToBottom(): void;
}
