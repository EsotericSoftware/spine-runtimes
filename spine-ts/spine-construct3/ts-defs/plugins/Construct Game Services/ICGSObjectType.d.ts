
/** Represents the Construct Game Services object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/construct-game-services | ICGSObjectType documentation } */

type CGSSignInProvider = "Apple" | "BattleNet" | "BattleNetChina" | "Discord" | "Facebook" | "Github" | "Google" | "ItchIO" | "Microsoft" | "Reddit" | "Steam" | "X" | "Yandex";

interface CGSSignInOptions {
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

declare class ICGSObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType, MultiplayerObjectEventMap<InstType>>
{
	readonly isSignedIn: boolean;
	readonly canSignInPersistent: boolean;
	readonly playerId: string;
	readonly playerName: string;
	readonly gameId: string;
	readonly sessionKey: string;
	
	signInWithProvider(provider: CGSSignInProvider, gameId: string, opts?: CGSSignInOptions): Promise<void>;
	signInPersistent(gameId: string): Promise<void>;
	signOut(): Promise<void>;

	submitScore(score: number, leaderboardId?: string): Promise<void>;
	getLeaderboardScores(leaderboardId: string, opts?: CGSGetLeaderboardScoresOptions): Promise<CGSLeaderboardScoreResults>;
}
