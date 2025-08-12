
interface MoveToBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"arrived": BehaviorInstanceEvent<InstType, BehInstType>;
	"hitsolid": BehaviorInstanceEvent<InstType, BehInstType>;
}

/** Represents the Move To behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/move | IMoveToBehaviorInstance documentation } */
declare class IMoveToBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof MoveToBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: MoveToBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof MoveToBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: MoveToBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	moveToPosition(x: number, y: number, isDirect?: boolean): void;
	getTargetX(): number;
	getTargetY(): number;
	getTargetPosition(): Vec2Arr;
	getWaypointCount(): number;
	getWaypointX(index: number): number;
	getWaypointY(index: number): number;
	getWaypoint(index: number): number;
	stop(): void;
	readonly isMoving: boolean;
	speed: number;
	maxSpeed: number;
	acceleration: number;
	deceleration: number;
	angleOfMotion: number;
	rotateSpeed: number;
	isStopOnSolids: boolean;
	isEnabled: boolean;
}
