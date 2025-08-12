
type SimulateControlTypePlatform = "left" | "right" | "jump";
type PlatformCeilingCollisionMode = "stop" | "preserve-momentum";
type PlatformWallSide = "left" | "right";

/** Represents the Platform behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/platform | IPlatformBehaviorInstance documentation } */
declare class IPlatformBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	fallThrough(): void;
	resetDoubleJump(d: boolean): void;
	simulateControl(ctrl: SimulateControlTypePlatform): void;
	speed: number;
	maxSpeed: number;
	acceleration: number;
	deceleration: number;
	jumpStrength: number;
	maxFallSpeed: number;
	gravity: number;
	gravityAngle: number;
	isDoubleJumpEnabled: boolean;
	jumpSustain: number;
	ceilingCollisionMode: PlatformCeilingCollisionMode;
	isByWall(side: PlatformWallSide): boolean;
	readonly isOnFloor: boolean;
	readonly isMoving: boolean;
	readonly isJumping: boolean;
	readonly isFalling: boolean;
	vectorX: number;
	vectorY: number;
	setVector(x: number, y: number): void;
	getVector(): Vec2Arr;
	isDefaultControls: boolean;
	isIgnoringInput: boolean;
	isEnabled: boolean;
}
