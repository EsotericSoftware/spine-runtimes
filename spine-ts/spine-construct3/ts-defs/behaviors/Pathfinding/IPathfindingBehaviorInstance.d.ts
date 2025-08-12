
type PathfindingDirectMovementMode = "none" | "to-destination" | "anywhere-along-path";

interface PathfindingBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"arrived": BehaviorInstanceEvent<InstType, BehInstType>;
}

/** Represents the Pathfinding behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/pathfinding | IPathfindingBehaviorInstance documentation } */
declare class IPathfindingBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof PathfindingBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: PathfindingBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof PathfindingBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: PathfindingBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	readonly map: IPathfindingMap;

	findPath(x: number, y: number): Promise<boolean>;
	calculatePath(fromX: number, fromY: number, toX: number, toY: number): Promise<boolean>;
	startMoving(): void;
	stop(): void;
	maxSpeed: number;
	speed: number;
	acceleration: number;
	deceleration: number;
	rotateSpeed: number;
	readonly isCalculatingPath: boolean;
	readonly isMoving: boolean;
	readonly currentNode: number;

	getNodeCount(): number;
	getNodeXAt(index: number): number;
	getNodeYAt(index: number): number;
	getNodeAt(index: number): Vec2Arr;
	nodes(): Generator<Vec2Arr>;

	directMovementMode: PathfindingDirectMovementMode;
	isEnabled: boolean;
}
