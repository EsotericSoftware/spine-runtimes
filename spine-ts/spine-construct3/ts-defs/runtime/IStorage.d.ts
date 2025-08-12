
/** Provides access to storage for the project.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/interfaces/istorage | IStorage documentation } */
declare class IStorage
{
	getItem(key: string): Promise<unknown>;
    setItem(key: string, value: unknown): Promise<void>;
    removeItem(key: string): Promise<void>;
    clear(): Promise<void>;
    keys(): Promise<string[]>;
}
