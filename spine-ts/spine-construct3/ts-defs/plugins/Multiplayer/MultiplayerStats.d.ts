
/** Represents statistics for the Multiplayer object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/multiplayer | IMultiplayerObjectType documentation } */
declare class MultiplayerStats
{
	readonly outboundCount: number;
	readonly outboundBandwidth: number;
	readonly outboundDecompressedBandwidth: number;

	readonly inboundCount: number;
	readonly inboundBandwidth: number;
	readonly inboundDecompressedBandwidth: number;
}
