
type Model3DRenderModeType = "hierarchy" | "isolate";
type Model3DTransformTypes = "offset" | "rotation" | "scale";
type Model3DOriginsX = "left" | "middle" | "right";
type Model3DOriginsY = "top" | "middle" | "bottom";
type Model3DOriginsZ = "back" | "middle" | "front";

type Model3DQuaternion = {
	x: number;
	y: number;
	z: number;
	w: number;
};

type Model3dAnimationCallback = (animation: string) => void;

/** Represents the 3D Model object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/3d-model | I3DModelInstance documentation } */
declare class I3DModelInstance extends IWorldInstance
{
	loadModel(model: string, mesh?: string, animation?: string, playing?: boolean, progress?: number): Promise<null | undefined>;

	onLoad(): void;
	onError(): void;

	modelName: string;
	// meshName: string; // deprecated
	meshNames: Array<string>;
	animationName: string;
	animationProgress: number;
	isPlaying: boolean;
	isLooping: boolean;
	meshRenderMode: Model3DRenderModeType;
	backfaceCulling: boolean;

	onAnimationFinished: Model3dAnimationCallback;
	onAnimationLooped: Model3dAnimationCallback;

	offsetX: number;
	offsetY: number;
	offsetZ: number;
	rotationX: number;
	rotationY: number;
	rotationZ: number;
	scaleX: number;
	scaleY: number;
	scaleZ: number;
	originX: Model3DOriginsX;
	originY: Model3DOriginsY;
	originZ: Model3DOriginsZ;

	setTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	addTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	subTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	mulTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	divTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;

	setQuaternion(x: number, y: number, z: number, w: number): void;
	getQuaternion(): Model3DQuaternion;

	animationDuration(animation: string): number;
	getAllMeshes(): Array<string>;
	getAllAnimations(): Array<string>;

	setMeshEnabled(mesh: string, enable: boolean): void;
	setAllMeshesEnabled(enable: boolean): void;

	isMeshEnabled(mesh: string): boolean;
	areAllMeshesEnabled(): boolean;

	meshExists(mesh: string): boolean;

	play(animationName?: string, progress?: number): void;
	stop(): void;
	pause(): void;
	resume(): void;
}