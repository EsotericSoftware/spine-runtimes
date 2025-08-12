
/** Represents the Bullet behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/bullet | IBulletBehaviorInstance documentation } */
declare class IBulletBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	speed: number;
	acceleration: number;
	gravity: number;
	angleOfMotion: number;
	bounceOffSolids: boolean;
	distanceTravelled: number;
	isEnabled: boolean;
}
