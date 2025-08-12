
type KeyboardKeyOrCode = number | string;

/** Represents the Keyboard object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/keyboard | IKeyboardObjectType documentation } */
declare class IKeyboardObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	isKeyDown(keyOrCode: KeyboardKeyOrCode): boolean;
}
