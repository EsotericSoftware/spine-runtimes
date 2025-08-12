
/** Represents a plugin in a project.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/iplugin | IPlugin documentation } */
declare class IPlugin_
{
	readonly runtime: IRuntime;

    readonly id: string;
    readonly isSingleGlobal: boolean;
    readonly isWorldType: boolean;
    readonly isHTMLElementType: boolean;
    readonly isRotatable: boolean;
    readonly hasEffects: boolean;
    readonly is3d: boolean;
    readonly supportsHierarchies: boolean;
    readonly supportsMesh: boolean;

    getSingleGlobalObjectType(): IObjectType<IInstance>;
    getSingleGlobalInstance(): IInstance;
    static getByConstructor(ctor: Function): IPlugin_ | null;
}

declare var IPlugin: typeof IPlugin_;
