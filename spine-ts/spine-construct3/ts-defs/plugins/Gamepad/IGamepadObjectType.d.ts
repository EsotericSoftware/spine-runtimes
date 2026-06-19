
interface GamepadInputButtonState {
    pressed: boolean;
    value: number;
}

interface GamepadInputGamepadState {
    index: number;
    id: string;
    buttons: GamepadInputButtonState[];
    axes: number[];
}

interface GamepadInputUpdateEvent extends ConstructEvent {
	gamepads: GamepadInputGamepadState[];
}

interface GamepadConnectedEvent extends ConstructEvent {
	index: number;
    id: string;
}

interface GamepadDisconnectedEvent extends ConstructEvent {
	index: number;
}

interface GamepadObjectTypeEventMap<InstanceType = IInstance> extends ObjectClassEventMap<InstanceType> {
    "gamepadconnected": GamepadConnectedEvent;
    "gamepaddisconnected": GamepadDisconnectedEvent;
	"gamepadinputupdate": GamepadInputUpdateEvent;
}

/** Represents the Gamepad object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/gamepad | IGamepadObjectType documentation } */
declare class IGamepadObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	addEventListener<K extends keyof GamepadObjectTypeEventMap<InstType>>(type: K, listener: (ev: GamepadObjectTypeEventMap<InstType>[K]) => any): void;
	removeEventListener<K extends keyof GamepadObjectTypeEventMap<InstType>>(type: K, listener: (ev: GamepadObjectTypeEventMap<InstType>[K]) => any): void;

    readonly deadZone: number;
}
