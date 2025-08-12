
type ArrayValueType = string | number;

/** Represents the Array object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/array | IArrayInstance documentation } */
declare class IArrayInstance extends IWorldInstance
{
	readonly width: number;
	readonly height: number;
	readonly depth: number;

	setSize(w: number, h?: number, d?: number): void;

	getAt(x: number, y?: number, z?: number): ArrayValueType;
	setAt(val: ArrayValueType, x: number, y?: number, z?: number): void;
}
