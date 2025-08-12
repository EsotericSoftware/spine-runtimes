
type MultiplayerRoomListType = "all" | "unlocked" | "available";
type MultiplayerRoomState = "available" | "locked" | "full";

interface MultiplayerSignallingServerState {
	myId: string;
	serverVersion: string;
	serverName: string;
	serverOperator: string;
	serverMOTD: string;
}

interface MultiplayerSignallingLoginResult {
	myAlias: string;
}

interface MultiplayerSignallingJoinResult {
	isHost: boolean;
	hostId: string;
	hostAlias: string;
}

interface MultiplayerSignallingAutoJoinResult {
	isHost: boolean;
	hostId: string;
	hostAlias: string;
	room: string;
}

interface MultiplayerSignallingGameInstanceEntry {
	name: string;
	peerCount: number;
}

interface MultiplayerSignallingRoomEntry {
	name: string;
	peerCount: number;
	maxPeerCount: number;
	state: MultiplayerRoomState;
}

interface MultiplayerErrorEvent extends ConstructEvent {
	message: string;
}

interface MultiplayerSignallingEventMap {
	"connected": MultiplayerSignallingServerState;
	"login": MultiplayerSignallingLoginResult;
    "join": MultiplayerSignallingAutoJoinResult;
    "leave": ConstructEvent;
	"disconnected": ConstructEvent;
	"kicked": ConstructEvent;
	"error": MultiplayerErrorEvent;
}

/** Represents the signalling state for the Multiplayer object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/multiplayer | IMultiplayerObjectType documentation } */
declare class MultiplayerSignallingState extends ConstructEventTarget<MultiplayerSignallingEventMap>
{
	addICEServer(url: string, username?: string, credential?: string): void;
	connect(url?: string): Promise<MultiplayerSignallingServerState>;
	readonly isConnected: boolean;
	disconnect(): void;

	login(alias: string): Promise<MultiplayerSignallingLoginResult>;
	readonly isLoggedIn: boolean;

	joinRoom(game: string, instance: string, room: string, maxClients?: number): Promise<MultiplayerSignallingJoinResult>;
	autoJoinRoom(game: string, instance: string, room: string, maxClients?: number, isLocking?: boolean): Promise<MultiplayerSignallingAutoJoinResult>;
	leaveRoom(): Promise<void>;
	
	requestGameInstanceList(game: string): Promise<MultiplayerSignallingGameInstanceEntry[]>;
	requestRoomList(game: string, instance: string, type: MultiplayerRoomListType): Promise<MultiplayerSignallingRoomEntry[]>;
}
