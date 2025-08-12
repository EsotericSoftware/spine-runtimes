
/** Represents a shared pathfinding obstacle map for the Pathfinding behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/pathfinding | IPathfindingBehaviorInstance documentation } */
declare class IPathfindingMap
{
	readonly cellSize: number;
	readonly cellBorder: number;
	readonly widthInCells: number;
	readonly heightInCells: number;
	isCellObstacle(x: number, y: number): boolean;
	moveCost: number;
	isDiagonalsEnabled: boolean;
	regenerateMap(): Promise<void>;
	regenerateRegion(startX: number, startY: number, endX: number, endY: number): Promise<void>;
	regenerateObjectRegion<InstanceType extends IInstance>(objectClass: IObjectClass<InstanceType>): Promise<void>;
	startPathGroup(baseCost?: number, cellSpread?: number, maxWorkers?: number): void;
	endPathGroup(): void;
}
