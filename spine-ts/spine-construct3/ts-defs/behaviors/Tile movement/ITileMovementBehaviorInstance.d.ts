
type SimulateControlTypeTile = "left" | "right" | "up" | "down";

/** Represents the Tile Movement behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/tile-movement | ITileMovementBehaviorInstance documentation } */
declare class ITileMovementBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	isIgnoringInput: boolean;
	isEnabled: boolean;
	isDefaultControls: boolean;
	simulateControl(dir: SimulateControlTypeTile): void;

	setSpeed(x: number, y: number): void;
	getSpeed(): Vec2Arr;
	setGridPosition(x: number, y: number, immediate?: boolean): void;
	getGridPosition(): Vec2Arr;
	modifyGridDimensions(width: number, height: number, x: number, y: number): void;
	isMoving(): boolean;
	isMovingDirection(dir: SimulateControlTypeTile): boolean;
	canMoveTo(x: number, y: number): boolean;
	canMoveDirection(dir: SimulateControlTypeTile, distance: number): boolean;
	getTargetPosition(): Vec2Arr;
	getGridTargetPosition(): Vec2Arr;
	toGridSpace(x: number, y: number): Vec2Arr;
	fromGridSpace(x: number, y: number): Vec2Arr;
}
