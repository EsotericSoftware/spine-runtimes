
/** Represents a behavior in a project (the behavior equivalent of a plugin).
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/ibehavior | IBehavior documentation } */
declare class IBehavior_
{
	readonly runtime: IRuntime;
    readonly id: string;

    /** Get all instances that use this behavior. The instances could be a mix of
     * different object types and plugin types. */
    getAllInstances(): IInstance[];

    static getByConstructor(ctor: Function): IBehavior_ | null;
}

declare var IBehavior: typeof IBehavior_;