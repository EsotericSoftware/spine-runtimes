
/** Represents the Shadow Caster behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/shadow-caster | IShadowCasterBehaviorInstance documentation } */
declare class IShadowCasterBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	height: number;
	tag: string;
	isEnabled: boolean;
}
