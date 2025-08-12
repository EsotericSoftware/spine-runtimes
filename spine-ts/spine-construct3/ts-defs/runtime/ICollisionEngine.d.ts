
/** Provides access to collision features.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/interfaces/icollisionengine | IAssetManager documentation } */
declare class ICollisionEngine
{
    readonly runtime: IRuntime;

    testOverlap(wi1: IWorldInstance, wi2: IWorldInstance): boolean;
    testOverlapAny(wi: IWorldInstance, iterable: Iterable<IWorldInstance>): IWorldInstance | null;
    testOverlapSolid(wi: IWorldInstance): IWorldInstance | null;

	setCollisionCellSize(width: number, height: number): void;
    getCollisionCellSize(): Vec2Arr;

    getCollisionCandidates(objectClasses: IObjectClass<IWorldInstance> | IObjectClass<IWorldInstance>[], rect: DOMRect): IWorldInstance[];
}
