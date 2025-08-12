
type SimulateControlType8Direction = "left" | "right" | "up" | "down";

/** Represents the 8 Direction behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/direction | I8DirectionBehaviorInstance documentation } */
declare class I8DirectionBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	stop(): void;
	reverse(): void;
	simulateControl(ctrl: SimulateControlType8Direction): void;
	speed: number;
	maxSpeed: number;
	acceleration: number;
	deceleration: number;
	vectorX: number;
	vectorY: number;
	setVector(x: number, y: number): void;
	getVector(): Vec2Arr;
	isDefaultControls: boolean;
	isIgnoringInput: boolean;
	isAllowSliding: boolean;
	isEnabled: boolean;
}
