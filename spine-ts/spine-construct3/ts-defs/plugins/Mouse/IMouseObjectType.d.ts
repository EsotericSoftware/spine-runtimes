
interface MouseObjectRequestPointerLockOptions {
	unadjustedMovement?: boolean;
}

interface MouseObjectMovementEvent extends ConstructEvent {
	movementX: number;
	movementY: number;
}

interface MouseObjectTypeEventMap<InstanceType = IInstance> extends ObjectClassEventMap<InstanceType> {
    "movement": MouseObjectMovementEvent;
	"pointerlockchange": ConstructEvent;
	"pointerlockerror": ConstructEvent;
}

/** Represents the Mouse object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/mouse | IMouseObjectType documentation } */
declare class IMouseObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	addEventListener<K extends keyof MouseObjectTypeEventMap<InstType>>(type: K, listener: (ev: MouseObjectTypeEventMap<InstType>[K]) => any): void;
	removeEventListener<K extends keyof MouseObjectTypeEventMap<InstType>>(type: K, listener: (ev: MouseObjectTypeEventMap<InstType>[K]) => any): void;

	getMouseX(layerNameOrNumber?: LayerParameter): number;
	getMouseY(layerNameOrNumber?: LayerParameter): number;
	getMousePosition(layerNameOrNumber?: LayerParameter): Vec2Arr;
	isMouseButtonDown(button: number): boolean;

	setCursorStyle(cursorStyle: string): void;
	setCursorObjectClass<InstType extends IInstance>(objectClass: IObjectClass<InstType>): void;

	readonly hasPointerLock: boolean;
	requestPointerLock(opts?: MouseObjectRequestPointerLockOptions): void;
	releasePointerLock(): void;
}
