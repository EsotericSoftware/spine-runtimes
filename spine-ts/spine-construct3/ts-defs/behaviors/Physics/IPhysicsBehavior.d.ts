
type PhysicsSteppingMode = "fixed" | "variable";

/** Represents global settings with the Physics behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/physics | IPhysicsBehaviorInstance documentation } */
declare class IPhysicsBehavior extends IBehavior_
{
	worldGravity: number;
	steppingMode: PhysicsSteppingMode;
	velocityIterations: number;
	positionIterations: number;
	setCollisionsEnabled<InstType1 extends IInstance, InstType2 extends IInstance>(objectClassA: IObjectClass<InstType1>, objectClassB: IObjectClass<InstType2>, state: boolean): void;
}
