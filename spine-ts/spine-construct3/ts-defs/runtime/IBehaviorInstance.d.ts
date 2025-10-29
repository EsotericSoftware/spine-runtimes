
declare class BehaviorInstanceEvent<InstType, BehInstType> implements ConstructEvent {
	instance: InstType;
    behaviorInstance: BehInstType;
}

interface BehaviorInstanceEventMap<InstType, BehInstType> {
	
}

/** Represents an instance of a behavior associated with a specific object instance.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/ibehaviorinstance | IBehaviorInstance documentation } */
declare class IBehaviorInstance<InstType>
{
    // Note IBehaviorInstance does not derive from ConstructEventTargetDispatcher - it implements it
	// separately to make use of <this> in its type definition (similar to IInstance, but using both
    // <this> and <InstType> for the corresponding instance type).
	addEventListener<K extends keyof BehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: BehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof BehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: BehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	dispatchEvent(evt: ConstructEvent): void;

	readonly runtime: IRuntime;
    readonly behavior: IBehavior_;
	readonly behaviorType: IBehaviorType;
    readonly instance: InstType;
}
