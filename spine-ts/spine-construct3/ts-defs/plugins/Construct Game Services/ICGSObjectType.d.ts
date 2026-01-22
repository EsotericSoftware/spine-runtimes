
/** Represents the Construct Game Services object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/construct-game-services | ICGSObjectType documentation } */

type CGSSignInProvider = "Apple" | "BattleNet" | "Discord" | "Facebook" | "Github" | "Google" | "ItchIO" | "Microsoft" | "Patreon" | "Reddit" | "Steam" | "X" | "Yandex";

interface CGSSignInOptions {
	allowPersisting?: boolean;
	expiryMins?: number;
	popupWindowWidth?: number;
	popupWindowHeight?: number;
}

type CGSLeaderboardScoreRange = "Daily" | "Weekly" | "Monthly" | "Yearly";

interface CGSGetLeaderboardScoresOptions {
	resultsPerPage?: number;
	page?: number;
	country?: string;
	range?: CGSLeaderboardScoreRange;
	rangeOffset?: number;
	culture?: string;
}

interface CGSLeaderboardScoreResult {
	score: number;
	formattedScore: string;
	rank: number;
	formattedRank: string;
	country: string;
	playerId: string;
	playerName: string;
}

interface CGSLeaderboardScoreResults {
	totalPageCount: number;
	scores: CGSLeaderboardScoreResult[];
}

interface CGSCreateCloudSaveOptions {
	key: string;
	bucketId?: string;
	name?: string;
	data: string | Blob
}

interface CGSGetCloudSaveOptions {
	key: string;
	bucketId: string;
	type?: "text" | "blob";
}

interface CGSObjectTypeEventMap<InstanceType = IInstance> extends ObjectClassEventMap<InstanceType> {
	"signinpopupblocked": ConstructEvent
}

declare class ICGSObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType, CGSObjectTypeEventMap<InstType>>
{
	addEventListener<K extends keyof CGSObjectTypeEventMap<InstType>>(type: K, listener: (ev: CGSObjectTypeEventMap<InstType>[K]) => any): void;
	removeEventListener<K extends keyof CGSObjectTypeEventMap<InstType>>(type: K, listener: (ev: CGSObjectTypeEventMap<InstType>[K]) => any): void;

	readonly isSignedIn: boolean;
	readonly canSignInPersistent: boolean;
	readonly playerId: string;
	readonly playerName: string;
	readonly gameId: string;
	readonly sessionKey: string;
	
	signInWithProvider(provider: CGSSignInProvider, gameId: string, opts?: CGSSignInOptions): Promise<void>;
	retryOpenSignInPopup(): void;
	signInPersistent(gameId: string): Promise<void>;
	signOut(): Promise<void>;
	setPlayerName(name: string): Promise<void>;

	submitScore(score: number, leaderboardId?: string): Promise<void>;
	getLeaderboardScores(leaderboardId: string, opts?: CGSGetLeaderboardScoresOptions): Promise<CGSLeaderboardScoreResults>;

	createCloudSave(opts: CGSCreateCloudSaveOptions): Promise<void>;
	getCloudSave(opts: CGSGetCloudSaveOptions): Promise<string | Blob>;
}
