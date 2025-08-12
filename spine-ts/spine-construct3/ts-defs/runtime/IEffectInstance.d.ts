
type EffectParameter = number | Vec3Arr;

/** Represents the parameters for a single effect on a IWorldInstance, ILayer or ILayout.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/ieffectinstance | IEffectInstance documentation } */
declare class IEffectInstance
{
	readonly index: number;
	readonly name: string;
	isActive: boolean;

	setParameter(index: number, value: EffectParameter): void;
	getParameter(index: number): EffectParameter;
}
