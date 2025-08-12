
/** Represents the JSON object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/json | IJSONInstance documentation } */
declare class IJSONInstance extends IWorldInstance
{
	getJsonDataCopy(): any;
	setJsonDataCopy(o: any): void;
	setJsonString(str: string): void;

	toCompactString(): string;
	toBeautifiedString(): string;
}
