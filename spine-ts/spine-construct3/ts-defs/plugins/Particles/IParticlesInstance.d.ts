
/** Represents the Particles object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/particles | IParticlesInstance documentation } */
declare class IParticlesInstance extends IWorldInstance
{
	isSpraying: boolean;
	rate: number;
	sprayCone: number;
	initSpeed: number;
	initSize: number;
	initOpacity: number;
	initXRandom: number;
	initYRandom: number;
	initSpeedRandom: number;
	initSizeRandom: number;
	initGrowRate: number;
	initGrowRandom: number;
	acceleration: number;
	gravity: number;
	lifeAngleRandom: number;
	lifeSpeedRandom: number;
	lifeOpacityRandom: number;
	timeout: number;
	fastForward(time: number): void;
	setParticleObjectClass<InstType extends IInstance>(objectClass?: IObjectType<InstType>): void;
}
