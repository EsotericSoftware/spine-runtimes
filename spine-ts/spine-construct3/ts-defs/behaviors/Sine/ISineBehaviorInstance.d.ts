
type SineBehaviorMovementType = "horizontal" | "vertical" | "size" | "width" | "height" | "angle" | "opacity" | "value-only" | "forwards-backwards" | "z-elevation";
type SineBehaviorWaveType = "sine" | "triangle" | "sawtooth" | "reverse-sawtooth" | "square";

/** Represents the Sine behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/sine | ISineBehaviorInstance documentation } */
declare class ISineBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	period: number;
	magnitude: number;
	phase: number;
	movement: SineBehaviorMovementType;
	wave: SineBehaviorWaveType;
	readonly value: number;
	updateInitialState(): void;
	isEnabled: boolean;
}
