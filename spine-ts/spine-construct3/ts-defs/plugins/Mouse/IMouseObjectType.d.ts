
/** Represents the Mouse object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/mouse | IMouseObjectType documentation } */
declare class IMouseObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	getMouseX(layerNameOrNumber?: LayerParameter): number;
	getMouseY(layerNameOrNumber?: LayerParameter): number;
	getMousePosition(layerNameOrNumber?: LayerParameter): Vec2Arr;
	isMouseButtonDown(button: number): boolean;

	setCursorStyle(cursorStyle: string): void;
	setCursorObjectClass<InstType extends IInstance>(objectClass: IObjectClass<InstType>): void;
}
