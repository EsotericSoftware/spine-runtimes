
type TurretBehaviorTargetMode = "first" | "nearest";

declare class TurretBehaviorEvent<InstType = IInstance, BehInstType = ITurretBehaviorInstance<InstType>> extends BehaviorInstanceEvent<InstType, BehInstType>
{
	targetInst: IWorldInstance;
}

interface TurretBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"targetacquired": TurretBehaviorEvent<InstType, BehInstType>;
	"shoot": TurretBehaviorEvent<InstType, BehInstType>;
}

/** Represents the Turret behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/turret | ITurretBehaviorInstance documentation } */
declare class ITurretBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof TurretBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: TurretBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof TurretBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: TurretBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	currentTarget: IWorldInstance | null;
	range: number;
	rateOfFire: number;
	isRotateEnabled: boolean;
	rotateSpeed: number;
	targetMode: TurretBehaviorTargetMode;
	isPredictiveAimEnabled: boolean;
	projectileSpeed: number;
	isEnabled: boolean;
}
