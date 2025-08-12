
type DictionaryValueType = string | number;

/** Represents the Dictionary object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/dictionary | IDictionaryInstance documentation } */
declare class IDictionaryInstance extends IWorldInstance
{
	getDataMap(): Map<string, DictionaryValueType>;
}
