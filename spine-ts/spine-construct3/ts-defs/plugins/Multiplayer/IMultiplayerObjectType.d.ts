
type MultiplayerTransmissionMode = "o" | "r" | "u";
type MultiplayerMessageType = string | JSONValue | ArrayBuffer;
type MultiplayerTransferDirection = "send" | "receive";

interface MultiplayerPeerEvent extends ConstructEvent {
	peerId: string;
	peerAlias: string;
}

interface MultiplayerPeerDisconnectEvent extends MultiplayerPeerEvent {
	leaveReason: string;
}

interface MultiplayerMessageEvent extends ConstructEvent {
	fromId: string;
	fromAlias: string;
	message: JSONValue | ArrayBuffer;
	transmissionMode: MultiplayerTransmissionMode;
}

interface MultiplayerBinaryTransferProgressEvent extends ConstructEvent {
	peerId: string;
	peerAlias: string;
	direction: MultiplayerTransferDirection;
	tag: string;
	progress: number;
}

interface MultiplayerBinaryTransferEvent extends ConstructEvent {
	peerId: string;
	peerAlias: string;
	direction: MultiplayerTransferDirection;
	tag: string;
}

interface MultiplayerBinaryTransferStartEvent extends MultiplayerBinaryTransferEvent {
	mimeType: string;
	fileName: string;
	size: number;
}

interface MultiplayerBinaryTransferCompleteEvent extends MultiplayerBinaryTransferStartEvent {
	data: File | Blob | ArrayBuffer | null;
}

interface MultiplayerObjectEventMap<InstanceType> {
	"peerconnect": MultiplayerPeerEvent;
	"peerdisconnect": MultiplayerPeerDisconnectEvent;
	"message": MultiplayerMessageEvent;
	"kicked": ConstructEvent;
	"binarytransferstart": MultiplayerBinaryTransferStartEvent,
	"binarytransferprogress": MultiplayerBinaryTransferProgressEvent,
	"binarytransfercancelled": MultiplayerBinaryTransferEvent,
	"binarytransfercomplete": MultiplayerBinaryTransferCompleteEvent
}

interface TransferPeerBinaryOpts {
	tag?: string;
	signal?: AbortSignal;
}

/** Represents the Multiplayer object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/multiplayer | IMultiplayerObjectType documentation } */
declare class IMultiplayerObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType, MultiplayerObjectEventMap<InstType>>
{
	readonly signalling: MultiplayerSignallingState;
	readonly stats: MultiplayerStats;

	readonly isHost: boolean;
	readonly myAlias: string;
	readonly myId: string;
	readonly hostAlias: string;
	readonly hostId: string;
	readonly currentGame: string;
	readonly currentGameInstance: string;
	readonly currentRoom: string;

	readonly peerCount: number;
	getAllPeers(): IMultiplayerPeer[];
	getPeerById(id: string): IMultiplayerPeer | null;
	sendPeerMessage(peerId: string, message: MultiplayerMessageType, transmissionMode?: MultiplayerTransmissionMode): void;
	transferPeerBinary(peerId: string, data: File | Blob | ArrayBuffer, opts?: TransferPeerBinaryOpts): Promise<void>;
	hostBroadcastMessage(fromId: string | null, message: MultiplayerMessageType, transmissionMode?: MultiplayerTransmissionMode): void;

	disconnectRoom(): void;
	simulateLatency(latency: number, pdv: number, loss: number): void;
}
