/** Represents an object type in the project.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/iobjecttype | IObjectType documentation } */
declare class IObjectType<InstanceType extends IInstance, EventMapType = ObjectClassEventMap<InstanceType>> extends IObjectClass<InstanceType, EventMapType>
{
	/** Set the instance class for this object type. Used for subclassing.
	 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/guides/subclassing-instances | Subclassing instances} */
	setInstanceClass(Class: Function): void;

	 /** Create a new instance of this object type. */
	createInstance<InstT extends InstanceType = InstanceType>(layerNameOrIndex: LayerParameter, x: number, y: number, createHierarchy?: boolean, template?: string): InstT;

	/** Get an array with a list of families this object type belongs to. */
	getAllFamilies(): IFamily<InstanceType>[];

	/** Iterates all families this object type belongs to. */
	families(): Generator<IFamily<InstanceType>>;

	/** Check if this object type belongs to a specified family. */
	isInFamily(family: IFamily): boolean;
}