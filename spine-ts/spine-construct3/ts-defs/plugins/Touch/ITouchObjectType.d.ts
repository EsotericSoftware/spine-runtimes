
type TouchRequestPermissionType = "orientation" | "motion";
type TouchRequestPermissionResult = "granted" | "denied";

/** Represents the Touch object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/touch | ITouchObjectType documentation } */
declare class ITouchObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	requestPermission(type: TouchRequestPermissionType): Promise<TouchRequestPermissionResult>;
}
