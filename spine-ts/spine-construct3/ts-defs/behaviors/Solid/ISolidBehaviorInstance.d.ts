
/** Represents the Solid behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/solid | ISolidBehaviorInstance documentation } */
declare class ISolidBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	isEnabled: boolean;
	readonly usesInstanceTags: boolean;

	/**
	 * @deprecated Use instance tags instead
	 */
	tags: string;

	/**
	 * @deprecated Use instance tags instead
	 */
	setAllTags(tags: Iterable<string>): void;

	/**
	 * @deprecated Use instance tags instead
	 */
	getAllTags(): Set<string>;
}
