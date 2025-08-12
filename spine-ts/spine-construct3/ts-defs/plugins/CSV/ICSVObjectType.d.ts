
type CsvDataType = "auto" | "string" | "number";

/** Represents the CSV object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/csv | ICSVObjectType documentation } */
declare class ICSVObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	parseCsv(str: string, delimiter?: string, dataType?: CsvDataType): any[];

	generateCsv(arr: any[], delimiter?: string): string;
}
