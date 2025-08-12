
/** Represents the Rotate behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/rotate | IRotateBehaviorInstance documentation } */
declare class IRotateBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	speed: number;
	acceleration: number;
	isEnabled: boolean;
}
