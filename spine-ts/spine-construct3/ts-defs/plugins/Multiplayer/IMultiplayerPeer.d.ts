
/** Represents a peer in the Multiplayer object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/multiplayer | IMultiplayerObjectType documentation } */
declare class IMultiplayerPeer
{
	readonly id: string;
	readonly alias: string;
	readonly isHost: boolean;
	readonly isMe: boolean;

	readonly latency: number;
	readonly pdv: number;

	send(message: MultiplayerMessageType, transmissionMode?: MultiplayerTransmissionMode): void;
}
