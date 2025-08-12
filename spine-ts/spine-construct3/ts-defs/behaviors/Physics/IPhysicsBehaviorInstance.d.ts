
/** Represents the Physics behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/physics | IPhysicsBehaviorInstance documentation } */
declare class IPhysicsBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	readonly behavior: IPhysicsBehavior;

	isEnabled: boolean;
	angularVelocity: number;
	density: number;
	friction: number;
	elasticity: number;
	linearDamping: number;
	angularDamping: number;
	isImmovable: boolean;
	isPreventRotation: boolean;
	isBullet: boolean;
	readonly mass: number;

	/**
	 * @deprecated Use isAwake (which also has a setter) instead of isSleeping
	 */
	readonly isSleeping: boolean;
	isAwake: boolean;

	applyForce(fx: number, fy: number, imgPt?: ImagePointParameter): void;
	applyForceTowardPosition(f: number, px: number, py: number, imgPt?: ImagePointParameter): void;
	applyForceAtAngle(f: number, a: number, imgPt?: ImagePointParameter): void;

	applyImpulse(ix: number, iy: number, imgPt?: ImagePointParameter): void;
	applyImpulseTowardPosition(i: number, px: number, py: number, imgPt?: ImagePointParameter): void;
	applyImpulseAtAngle(i: number, a: number, imgPt?: ImagePointParameter): void;

	applyTorque(m: number): void;
	applyTorqueToAngle(m: number, a: number): void;
	applyTorqueToPosition(m: number, px: number, py: number): void;

	setVelocity(vx: number, vy: number): void;
	getVelocityX(): number;
	getVelocityY(): number;
	getVelocity(): Vec2Arr;

	teleport(x: number, y: number): void;

	getCenterOfMassX(): number;
	getCenterOfMassY(): number;
	getCenterOfMass(): Vec2Arr;

	getContactCount(): number;
	getContactX(): number;
	getContactY(): number;
	getContact(): Vec2Arr;

	createDistanceJoint(imgPt: ImagePointParameter, otherInst: IWorldInstance, otherImgPt: ImagePointParameter, damping: number, freq: number): void;
	createRevoluteJoint(imgPt: ImagePointParameter, otherInst: IWorldInstance): void;
	createLimitedRevoluteJoint(imgPt: ImagePointParameter, otherInst: IWorldInstance, lower: number, upper: number): void;
	createPrismaticJoint(imgPt: ImagePointParameter, otherInst: IWorldInstance, axisAngle: number, enableLimit: boolean, lowerTranslation: number, upperTranslation: number, enableMotor: boolean, motorSpeed: number, maxMotorForce: number): void;
	removeAllJoints(): void;
}
