
/** Represents the Binary Data object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/binary-data | IBinaryDataInstance documentation } */
declare class IBinaryDataInstance extends IWorldInstance
{
	/** Set the content of the Binary Data by copying the provided ArrayBuffer
	 * or typed array.	 */
	setArrayBufferCopy(viewOrBuffer: ArrayBuffer | TypedArray): void;

	/** Set the content of the Binary Data by assuming ownership of the
	 * provided ArrayBuffer. This does not require copying the data, but
	 * nothing else must use the provided ArrayBuffer beyond this call.	 */
	setArrayBufferTransfer(arrayBuffer: ArrayBuffer): void;

	/** Get the content of the Binary Data object by copying its internal
	 * ArrayBuffer. The returned data is safe to modify.  */
	getArrayBufferCopy(): ArrayBuffer;

	/** Get the content of the Binary Data object by returning a reference
	 * to its internal ArrayBuffer. This is only safe to read - it must not
	 * be modified in any way. If modification is necessary, use getArrayBufferCopy().	 */
	getArrayBufferReadOnly(): ArrayBuffer;
}
