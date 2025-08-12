
type SimulateControlTypeCar = "left" | "right" | "up" | "down";

/** Represents the Car behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/car | ICarBehaviorInstance documentation } */
declare class ICarBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	stop(): void;
	simulateControl(ctrl: SimulateControlTypeCar): void;
	speed: number;
	maxSpeed: number;
	acceleration: number;
	deceleration: number;
	readonly vectorX: number;
	readonly vectorY: number;
	getVector(): Vec2Arr;
	readonly angleOfMotion: number;
	steerSpeed: number;
	driftRecover: number;
	friction: number;
	turnWhileStopped: boolean;
	isDefaultControls: boolean;
	isIgnoringInput: boolean;
	isEnabled: boolean;
}
