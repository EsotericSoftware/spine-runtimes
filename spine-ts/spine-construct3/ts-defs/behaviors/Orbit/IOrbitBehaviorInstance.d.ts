
/** Represents the Orbit behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/orbit | IOrbitBehaviorInstance documentation } */
declare class IOrbitBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	setTargetPosition(x: number, y: number): void;
	getTargetPosition(): Vec2Arr;
	pin(inst: IWorldInstance): void;
	speed: number;
	acceleration: number;
	rotation: number;
	offsetAngle: number;
	primaryRadius: number;
	secondaryRadius: number;
	isMatchRotation: boolean;
	totalRotation: number;
	totalAbsoluteRotation: number;
	getDistanceToTarget(): number;
	isEnabled: boolean;
}
