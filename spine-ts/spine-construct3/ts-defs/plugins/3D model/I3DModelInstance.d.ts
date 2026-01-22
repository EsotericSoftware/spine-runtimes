
type Model3DRenderModeType = "hierarchy" | "isolate";
type Model3DTransformTypes = "offset" | "rotation" | "scale";

/** Represents the 3D Model object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/3d-model | I3DModelInstance documentation } */
declare class I3DModelInstance extends IWorldInstance
{
	loadModel(model: string, mesh?: string, animation?: string, playing?: boolean, progress?: number): Promise<null | undefined>;

	onLoad(): void;
	onError(): void;

	modelName: string;
	meshName: string;
	animationName: string;
	animationProgress: number;
	isPlaying: boolean;
	meshRenderMode: Model3DRenderModeType;

	offsetX: number;
	offsetY: number;
	offsetZ: number;
	rotationX: number;
	rotationY: number;
	rotationZ: number;
	scaleX: number;
	scaleY: number;
	scaleZ: number;

	setTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	addTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	subTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	mulTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;
	divTransform(x: number, y: number, z: number, type: Model3DTransformTypes): void;

	animationDuration(animation: string): number;
	getAllMeshes(): Array<string>;
	getAllAnimations(): Array<string>;

	play(animationName?: string, progress?: number): void;
	stop(): void;
	pause(): void;
	resume(): void;
}