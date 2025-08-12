
type SpriteAnimationFromMode = "current-frame" | "beginning";

interface SpriteFrameChangeEvent<InstType = ISpriteInstance> extends InstanceEvent<InstType> {
	animationName: string;
	animationFrame: number;
}

interface SpriteAnimationEndEvent<InstType = ISpriteInstance> extends InstanceEvent<InstType> {
	animationName: string;
}

interface SpriteInstanceEventMap<InstType = ISpriteInstance> extends WorldInstanceEventMap<InstType> {
	"framechange": SpriteFrameChangeEvent<InstType>;
	"animationend": SpriteAnimationEndEvent<InstType>;
}

/** Represents the Sprite object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/sprite | ISpriteInstance documentation } */
declare class ISpriteInstance extends IWorldInstance
{
	addEventListener<K extends keyof SpriteInstanceEventMap<this>>(type: K, listener: (ev: SpriteInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof SpriteInstanceEventMap<this>>(type: K, listener: (ev: SpriteInstanceEventMap<this>[K]) => any): void;

	getImagePointCount(): number;
	getImagePointX(nameOrIndex: ImagePointParameter): number;
	getImagePointY(nameOrIndex: ImagePointParameter): number;
	getImagePointZ(nameOrIndex: ImagePointParameter): number;
	getImagePoint(nameOrIndex: ImagePointParameter): Vec3Arr;

	getPolyPointCount(): number;
	getPolyPointX(index: number): number;
	getPolyPointY(index: number): number;
	getPolyPoint(index: number): Vec2Arr;

	stopAnimation(): void;
	startAnimation(from?: SpriteAnimationFromMode): void;
	setAnimation(name: string, from?: SpriteAnimationFromMode): void;
	getAnimation(name: string): IAnimation | null;
	readonly animation: IAnimation;
	readonly animationName: string;
	animationFrame: number;
	animationFrameTag: string;
	animationSpeed: number;
	animationRepeatToFrame: number;

	readonly imageWidth: number;
	readonly imageHeight: number;
	getImageSize(): Vec2Arr;

	replaceCurrentAnimationFrame(blob: Blob): Promise<void>;
	setSolidCollisionFilter(isInclusive: boolean, tags: string): void;
}
