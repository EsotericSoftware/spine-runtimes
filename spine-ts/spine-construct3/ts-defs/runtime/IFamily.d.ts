/** Represents a family in the project.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/ifamily | IFamily documentation } */
declare class IFamily<InstanceType extends IInstance, EventMapType = ObjectClassEventMap<InstanceType>> extends IObjectClass<InstanceType, EventMapType>
{
	/** Get an array with a list of object types that belong to this family. */
	getAllObjectTypes(): IObjectType<InstanceType>[];

	/** Iterates all object types that belong to this family. */
	objectTypes(): Generator<IObjectType<InstanceType>>;

	/** Check if a given object type belongs to this family. */
	hasObjectType(objectType: IObjectType): boolean;
}