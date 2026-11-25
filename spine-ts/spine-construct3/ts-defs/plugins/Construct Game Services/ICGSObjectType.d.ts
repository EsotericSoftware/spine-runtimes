
/** Represents the Construct Game Services object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/construct-game-services | ICGSObjectType documentation } */

type CGSSignInProvider = "Apple" | "BattleNet" | "Discord" | "Facebook" | "Github" | "Google" | "ItchIO" | "Microsoft" | "Patreon" | "Reddit" | "Steam" | "Yandex";

interface CGSObjectTypeEventMap<InstanceType = IInstance> extends ObjectClassEventMap<InstanceType> {
	"signinpopupblocked": ConstructEvent
}

/*********
MARK: COMMON
*********/

interface CGSLanguageResult {
	iso: string;
	englishName: string;
}

interface CGSPictureResult {
	width: number;
	height: number;
	url: string;
}

interface CGSRatingDimensionsResult {
	id: string;
	title: string;
	description: string;
	maxRating: number;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
}

interface CGSRatingsResult {
	dimensionID: string;
	totalRatings: number;
	formattedTotalRatings: string;
	averageRating: number;
	averageRatingAsPercentage: number;
	formattedAverageRating: string;
	maxRating: number;
	lastRating: string;
	formattedLastRating: string;
	title: string;
	description: string;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
}

interface CGSMyRatingsResult {					
	dimensionID: string;
	value: number;
	maxRating: number;
	date: string;
	formattedDate: string;
}

interface CGSRatingStatusResult {
	isRateable: boolean;
	ratings: CGSRatingsResult[];
	myRatings: CGSMyRatingsResult[];
}

/*********
MARK: AUTHENTICATION
*********/
interface CGSSignInOptions {
	allowPersisting?: boolean;
	expiryMins?: number;
	popupWindowWidth?: number;
	popupWindowHeight?: number;
}
interface CGSLinkOptions{
	popupWindowWidth?: number;
	popupWindowHeight?: number;
}
interface CGSSignInWithUsernamePasswordOpts{
	allowPersisting?: boolean;
	expiryMins?: number;
}
interface CGSSignInWithEmailOpts{
	allowPersisting?: boolean;
	expiryMins?: number;
}

interface CGSPlayerResult{
	id: string;
	created: string;
	playerName: string;
	avatars?: CGSPictureResult[];
	lastActive: string;
}

interface CGSSessionResult{
	playerID: string;
	playerName: string;
	avatars?: CGSPictureResult[];
	expiry: string;
	key: string
}

interface CGSRegisterPlayerResult {
	player: CGSPlayerResult;
	session: CGSSessionResult;
}

interface CGSRegisterPlayerOptions{
	username?: string;
	password?: string;
	emailAddress?: string;
	allowPersisting: boolean;
	expiryMins: number;
}

interface CGSGetLoginProviderResponse{
	username: string;
	avatarURL: string;
	provider: CGSSignInProvider;
	signIns: number;
	firstSignIn: string;
	lastSignIn: string;
}

interface CGSPatreonMembershipResponse{	
	id: string;
	lifetimeSupportCents: number;
	currentlyEntitledAmountCents: number;
	campaign?: CGSPatreonCampaign;
	currentlyEntitledTiers: CGSPatreonEntitledTier[];
}

interface CGSPatreonCampaign{
	id: string;
	vanity: string;
	creationName: string;
	url: string;
}

interface CGSPatreonEntitledTier{
	id: string;
	description: string;
	title: string;
}

/*********
MARK: LEADERBOARDS
*********/

type CGSLeaderboardScoreRange = "Daily" | "Weekly" | "Monthly" | "Yearly";
type CGSLeaderboardTeamOrdering = "BestRanked" | "WorstRanked" | "NameAZ" | "NameZA" | "MostPlayers" | "LeastPlayers" | "Newest" | "Oldest";

interface CGSGetLeaderboardScoresOptions
{
	resultsPerPage?: number;
	page?: number;
	country?: string;
	range?: CGSLeaderboardScoreRange;
	rangeOffset?: number;
	culture?: string;
}

interface CGSLeaderboardNewestScoreOptions{
	country?: string;
	resultsPerPage?: number;
	page?: number;
}

interface CGSLeaderboardPlayerScoreOptions{
	resultsPerPage?: number;
	page?: number;
}

interface CGSAdjustLeaderboardScoreResult
{
	personalBest: boolean;
	score: CGSLeaderboardScoreResult;
}

interface CGSLeaderboardScoreResult {
	id: string;
	score: number;
	formattedScore: string;
	rank: number;
	formattedRank: string;
	country: string;
	date: string;
	formattedDate: string;
	updates: number;
	countryRank: number;
	formattedCountryRank: string;	
	playerID: string;
	playerName: string;
	teamID?: string;
	teamName?: string;
	scoreHistory?: CGSLeaderboardScoreHistoryResult;
	tier?: CGSLeaderboardTierResult;
	optionalValue1?: number;
	optionalValue2?: number;
	optionalValue3?: number;
}

interface CGSLeaderboardScoreHistoryResult{
	date: string;
	score: number;
	formattedScore: string;
	rank: number;
	formattedRank: string;
	countryRank?: number;
	formattedCountryRank?: string;
}

interface CGSLeaderboardScoresHistoryResult{
	player: CGSPlayerResult;
	scoreHistory: CGSLeaderboardScoreHistoryResult[];
	scoreID: string;
	countryID: string;
}

interface CGSLeaderboardTierResult{
	id: string;
	name: string;
	originalLanguage: CGSLanguageResult;
	responseLanguage: CGSLanguageResult;
}

interface CGSLeaderboardScoreResults {
	totalPageCount: number;
	scores: CGSLeaderboardScoreResult[];
}

interface CGSLeaderboardScoreResultsUnpaginated{
	scores: CGSLeaderboardScoreResult[];
}

interface CGSGetLeaderboardTeamsOptions{
	orderBy: CGSLeaderboardTeamOrdering;
	resultsPerPage: number;
	page: number;
}

interface CGSLeaderboardTeamsResult{	
	totalPageCount: number;
	teams: CGSLeaderboardTeamResult[];
}

interface CGSLeaderboardTeamResult{
	teamID: string;
	name: string;
	players: number;
	formattedPlayers: string;
	dateCreated: string;
	scores: number;
	formattedScores: string;
	totalScoreValues: number;
	averageScore: number;
	formattedAverageScore: string;
	bestScore: number;
	formattedBestScore: string;
	rank: number;
	ordinal: string;
	formattedRank: string;
}

/*********
MARK: CLOUD SAVE
*********/

type CGSCloudSaveBucketOrdering = "AZ" | "ZA" | "MostBlobs" | "LeastBlobs" | "MostData" | "LeastData";
type CGSCloudSaveBucketAccessMode = "Private" | "PublicRead" | "PublicReadWrite";

interface CGSCreateCloudSaveOptions {
	key: string;
	bucketID?: string;
	name?: string;
	data: string | Blob
}

interface CGSGetCloudSaveOptions {
	key: string;
	bucketID: string;
	type?: "text" | "blob";
}

interface CGSGetCloudSaveBucketsOptions{
	orderBy: CGSCloudSaveBucketOrdering;	
	resultsPerPage: number;
	page: number;
}

interface CGSGetCloudSaveBucketsResult{
	totalPageCount: number;
	buckets: CGSCloudSaveBucketResult[];
}

interface CGSCloudSaveBucketResult{
	id: string;
	accessMode: CGSCloudSaveBucketAccessMode;
	allowRatings: boolean;
	created: string;
	maxBlobs?: number;
	maxBlobSizeBytes: number;
	maxBlobsPerPlayer?: number;
	name: string;
	totalBlobs: number;
	totalBytes: number;
}

/*********
MARK: ACHIEVEMENTS
*********/

interface CGSAchievementResult{
	id: string;
	created: string;
	formattedDateCreated: string;
	name: string;
	description: string;
	progressive: boolean;
	clientProgressAllowed: boolean;
	progressionRequired: number;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
	isSecret: boolean;
	maxUnlocks: number;
	formattedMaxUnlocked: string;
	totalAwarded: number;
	formattedTotalAwarded: string;
	totalUniquePlayersAwarded: number;
	formattedTotalUniquePlayersAwarded: string;
	percentagePlayerBaseOwned: string;
	firstAwarded: string;
	formattedFirstAwarded: string;
	lastAwarded: string;
	formattedLastAwarded: string;
	incrementOnly: boolean;
	xpBonuses: CGSXPBonus[];
	achievedLogos: CGSPictureResult[];
	unachievedLogos: CGSPictureResult[];
}
interface CGSXPBonus{
	from: number,
	bonus: number
}
interface CGSPlayerAchievement{
	count: number;
	firstAwarded: string;
	formattedCount: string;
	lastAwarded: string;
	formattedFirstAwarded: string;
	progress: number;
	formattedLastAwarded: string;
	formattedProgress: string;
	achievement: CGSAchievementResult;
}
interface CGSGetAllAchievementsResult{
	achievements: CGSAchievementResult[];
}
interface CGSGetPlayerAchievementsResult{
	playerAchievements:  CGSPlayerAchievement[];
}

type CGSAchievementOrderBy = "MostRecentlyAwarded" | "LeastRecentlyAwarded" | "MostAwarded" | "LeastAwarded";

interface CGSPlayersAwardedAchievementOptions{
	orderBy?: CGSAchievementOrderBy; 
	resultsPerPage?: number;
	page?: number;
}
interface CGSGetPlayersAwardedAchievementResult{
	totalPageCount: number;
	achievement: CGSAchievementResult;
	awardedPlayers: CGSPlayerAchievement[];
}

/*********
MARK: BROADCASTS
*********/

interface CGSBroadcastChannelResult{
	id: string;
	name: string;
	description: string;
	created: string;
	formattedCreated: string;
	broadcasts: number;
	formattedBroadcasts: string;
	lastBroadcast: string;
	formattedLastBroadcast: string;
	allowRatings: boolean;
	anyUnreadMessages: boolean;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
	dimensionlessMaxRatingValue: number;
	ratingDimensions: CGSRatingDimensionsResult[];
}

interface CGSGetBroadcastMessagesOptions{
	resultsPerPage: number;
	page: number;
}

interface CGSGetBroadcastChannels {
	totalPageCount: number;
	channels: CGSBroadcastChannelResult[];
}

interface CGSBroadcastMessageResult{
	id: string;
	channelID: string;
	date: string;
	formattedDate: string;
	title: string;
	text: string;
	textLength: number;
	formattedTextLength: string;
	excerpt: string;
	reads: number;
	formattedReads: string;
	uniqueReads: number;
	formattedUniqueReads: string;
	updates: number;
	formattedUpdates: string;
	lastUpdate: string;
	formattedLastUpdate: string;
	isUnread: boolean;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
	ratingStatus: CGSRatingStatusResult
}

interface CGSBroadcastChannelMessagesResult{
	totalPageCount: number;
	messages: CGSBroadcastMessageResult[];
}

/*********
MARK: XP
*********/
interface CGSXPResult{
	xp: number;
	xpFormatted: string;
	rankID?: number;
	nextRankID?: number;
	leaderboardScoreID: string;
}
interface CGSXPRankResult{
	id: string;
	atXP: number;
	formattedAtXP:string;
	title: string;
	description: string;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
	logos?: CGSPictureResult[];
}
interface CGSXPBonusResult{
	id: string;
	startDate: string;
	formattedStartDate: string;
	endDate: string;
	formattedEndDate: string;
	title: string;
	description: string;
	modifier: number;
	responseLanguage: CGSLanguageResult;
	originalLanguage: CGSLanguageResult;
}

declare class ICGSObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType, CGSObjectTypeEventMap<InstType>>
{
	addEventListener<K extends keyof CGSObjectTypeEventMap<InstType>>(type: K, listener: (ev: CGSObjectTypeEventMap<InstType>[K]) => any): void;
	removeEventListener<K extends keyof CGSObjectTypeEventMap<InstType>>(type: K, listener: (ev: CGSObjectTypeEventMap<InstType>[K]) => any): void;

	// Common
	gameID: string;
	targetLanguage: string;

	// Achievements
    getAllAchievements(): Promise<CGSGetAllAchievementsResult>;
	getAchievement(achievementID: string): Promise<CGSAchievementResult>;
	getPlayerAchievements(): Promise<CGSGetPlayerAchievementsResult>;
	getAwardedPlayers(achievementID: string, opts?: CGSPlayersAwardedAchievementOptions): Promise<CGSGetPlayersAwardedAchievementResult>;
	awardAchievement(achievementID: string, value: number): Promise<void>;

	// Broadcasts
	getBroadcastChannels(): Promise<CGSBroadcastChannelResult>;
	markBroadcastChannelAsRead(channelID: string): Promise<boolean>;
	getBroadcastChannelMessages(channelID: string, opts?: CGSGetBroadcastMessagesOptions): Promise<CGSBroadcastChannelMessagesResult>;
	getBroadcastMessage(messageID: string): Promise<CGSBroadcastMessageResult>;
	rateBroadcastMessage(messageID: string, value: number): Promise<boolean>;

	// XP
    getXP() : Promise<CGSXPResult>;
    getXPRanks(): Promise<CGSXPRankResult[]>;
    getActiveXPBonuses(): Promise<CGSXPBonusResult[]>;
    getUpcomingXPBonuses(endDate : Date): Promise<CGSXPBonusResult[]>;

	// Authentication
	readonly isSignedIn: boolean;
	readonly canSignInPersistent: boolean;
	readonly playerID: string;
	readonly playerName: string;
	readonly sessionKey: string;
	readonly totalConnectedLoginProviders: string;

	requestForgottenPassword(emailAddress: string): Promise<void>;
	registerPlayer(playerName: string, opts?: CGSRegisterPlayerOptions): Promise<CGSRegisterPlayerResult>;
	deletePlayer(): Promise<void>;
	deleteAvatar(): Promise<void>;
	signInWithUsernamePassword(username: string, password: string, opts: CGSSignInWithUsernamePasswordOpts): Promise<void>;
	signInWithEmail(emailAddress: string, opts: CGSSignInWithEmailOpts): Promise<void>;
	signInWithProvider(provider: CGSSignInProvider, gameID: string, opts?: CGSSignInOptions): Promise<void>;
	linkProvider(provider: CGSSignInProvider, gameID: string, opts?: CGSLinkOptions): Promise<void>;
	forceLinkProvider(): Promise<void>;

	disconnectProvider(provider: CGSSignInProvider): Promise<void>;
	getLoginProviders(): Promise<CGSGetLoginProviderResponse[]>;
	retryOpenSignInPopup(): void;
	signInPersistent(gameID: string): Promise<void>;
	signOut(): Promise<void>;
	setPlayerName(name: string): Promise<void>;
	setPlayerEmailAddress(emailAddress: string): Promise<void>;
	setPlayerUsernameAndPassword(username: string, password: string): Promise<void>;
	setPlayerUsername(newUsername: string): Promise<void>;
	setPlayerPassword(newPassword: string): Promise<void>;
	setPlayerAvatarByBinary(avatarData: Blob): Promise<void>;
	setPlayerAvatarByURL(avatarURL: string): Promise<void>;
	setPlayerAvatarByBase64(avatar: string): Promise<void>;
	getPatreonMemberships(): Promise<CGSPatreonMembershipResponse[]>;

	// Leaderboards
	submitScore(score: number, optionalValue1?: number, optionalValue2?: number, optionalValue3?: number, leaderboardID?: string): Promise<void>;
	adjustScore(scoreID: string, leaderboardID: string, scoreAdjustment?: number, optValue1Adjustment?: number, optValue2Adjustment?: number, optValue3Adjustment?: number): Promise<CGSAdjustLeaderboardScoreResult>;
	getLeaderboardScores(leaderboardID: string, opts?: CGSGetLeaderboardScoresOptions): Promise<CGSLeaderboardScoreResults>;
	getLeaderboardTeams(leaderboardID : string, opts?: CGSGetLeaderboardTeamsOptions): Promise<CGSLeaderboardTeamsResult>;
	getLeaderboardScoreHistory(leaderboardID: string, playerID?: string, scoreID?: string): Promise<CGSLeaderboardScoresHistoryResult>;
	getLeaderboardNewestScores(leaderboardID : string, opts?: CGSLeaderboardNewestScoreOptions): Promise<CGSLeaderboardScoreResults>;
	getLeaderboardPlayerScores(leaderboardID : string, playerID : string, opts?: CGSLeaderboardPlayerScoreOptions): Promise<CGSLeaderboardScoreResults>;
	getLeaderboardNeighbourScores(leaderboardID : string, playerID? : string, scoreID? : string, range?: number): Promise<CGSLeaderboardScoreResultsUnpaginated>;

	// Cloud save
	rateCloudSave(cloudSaveID: string, value: number): Promise<boolean>;
	setCloudSavePictureByBinary(blobID: string, pictureData: Blob): Promise<void>;
	setCloudSavePictureByURL(blobID: string, pictureURL: string): Promise<void>;
	setCloudSavePictureByBase64(blobID: string, picture: string): Promise<void>;
	deleteCloudSavePicture(blobID: string): Promise<void>;
	deleteCloudSave(blobID: string): Promise<void>;
	createCloudSave(opts: CGSCreateCloudSaveOptions): Promise<void>;
	getCloudSave(opts: CGSGetCloudSaveOptions): Promise<string | Blob>;
	getCloudSaveBuckets(opts: CGSGetCloudSaveBucketsOptions): Promise<CGSGetCloudSaveBucketsResult>;
}
