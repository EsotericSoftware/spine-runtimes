
/** Represents the Rotate behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/billboard | IBillboardBehaviorInstance documentation } */
declare class IBillboardBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	offsetX: number;
	offsetY: number;
	offsetZ: number;
	
	isEnabled: boolean;
}
