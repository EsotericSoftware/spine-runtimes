
/** Represents the Solid behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/solid | ISolidBehaviorInstance documentation } */
declare class ISolidBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	/**
	 * @deprecated Use setAllTags() or getAllTags() instead, which use more suitable data types than a space-separated string.
	 */
	tags: string;

	setAllTags(tags: Iterable<string>): void;
	getAllTags(): Set<string>;

	isEnabled: boolean;
}
