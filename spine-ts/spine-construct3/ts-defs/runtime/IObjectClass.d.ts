
interface ObjectClassEventMap<InstanceType> {
	"instancecreate": InstanceEvent<InstanceType>;
	"hierarchyready": InstanceEvent<InstanceType>;
	"instancedestroy": InstanceDestroyEvent<InstanceType>;
}

/** A base class for object types or families in the project.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/iobjectclass | IObjectClass documentation } */
declare class IObjectClass<InstanceType extends IInstance, EventMapType = ObjectClassEventMap<InstanceType>> extends ConstructEventTarget<EventMapType>
{
	readonly name: string;

	readonly runtime: IRuntime;
	readonly plugin: IPlugin_;

	/** Get all instances belonging to this object type or family. */
	getAllInstances<InstT extends InstanceType = InstanceType>(): InstT[];

	/** Iterate all instances belonging to this object type or family. */
	instances<InstT extends InstanceType = InstanceType>(): Generator<InstT>;

	/** Get the first instance of this object type or family (or null if none exist). */
	getFirstInstance<InstT extends InstanceType = InstanceType>(): InstT | null;

	/** Get all the currently picked instances, when called from an event sheet. */
	getPickedInstances<InstT extends InstanceType = InstanceType>(): InstT[];

	/** Return the instance with the same IID, with wraparound, if one exists. */
	getPairedInstance<InstT extends InstanceType = InstanceType>(otherInst: IInstance): InstT | null;

	/** Iterate all currently picked instances, when called from an event sheet. */
	pickedInstances<InstT extends InstanceType = InstanceType>(): Generator<InstT>;

	/** Get the first picked instance of this object type or family when called
	 * from an event sheet, or null if none is picked. */
	getFirstPickedInstance<InstT extends InstanceType = InstanceType>(): InstT | null;
}
