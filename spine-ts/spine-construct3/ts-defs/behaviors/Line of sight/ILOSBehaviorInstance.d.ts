
/** Represents the Line-of-sight behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/line-of-sight | ILOSBehaviorInstance documentation } */
declare class ILOSBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	ray: ILOSBehaviorRay;

	range: number;
	coneOfView: number;
	hasLOStoPosition(x: number, y: number): boolean;
	hasLOSBetweenPositions(fromX: number, fromY: number, fromAngle: number, toX: number, toY: number): boolean;
	castRay(fromX: number, fromY: number, toX: number, toY: number, useCollisionCells?: boolean): ILOSBehaviorRay;

	addObstacle<InstType2 extends IInstance>(objectClass: IObjectClass<InstType2>): void;
	clearObstacles(): void;
}
