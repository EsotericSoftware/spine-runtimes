
type ShadowLightCastFromMode = "all" | "same-tag" | "different-tag";

/** Represents the Shadow Light object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/shadow-light | IShadowLightInstance documentation } */
declare class IShadowLightInstance extends IWorldInstance
{
	lightX: number;
	lightY: number;
	lightHeight: number;
	shadowColor: Vec3Arr;
	tag: string;
	castFrom: ShadowLightCastFromMode;
}
