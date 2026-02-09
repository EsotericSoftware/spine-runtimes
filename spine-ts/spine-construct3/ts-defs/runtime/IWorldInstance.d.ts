
interface WorldInstanceEventMap<InstType = IWorldInstance> extends InstanceEventMap<InstType> {
	"hierarchyready": InstanceEvent<InstType>;
}

// Hierarchy options returned by getHierarchyOpts()
interface SceneGraphHierarchyOpts {
	transformX: boolean;
	transformY: boolean;
	transformZ: boolean;
	transformWidth: boolean;
	transformHeight: boolean;
	transformAngle: boolean;
	transformOpacity: boolean;
	transformVisibility: boolean;
	destroyWithParent: boolean;
}

// Options for addChild(), based on SceneGraphHierarchyOpts but making every property optional
type SceneGraphAddChildOpts = Partial<SceneGraphHierarchyOpts>;

// Options for setMeshPoint()
interface SetMeshPointOpts {
	mode?: "absolute" | "relative";
	x: number;
	y: number;
	z?: number;
	u?: number;
	v?: number;
}

// Options returned by getMeshPoint()
interface GetMeshPointOpts {
	x: number;
	y: number;
	z: number;
	u: number;
	v: number;
}

/** Represents an instance of an object that appears in a layout.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/iworldinstance | IWorldInstance documentation } */
declare class IWorldInstance extends IInstance
{
	addEventListener<K extends keyof WorldInstanceEventMap<this>>(type: K, listener: (ev: WorldInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof WorldInstanceEventMap<this>>(type: K, listener: (ev: WorldInstanceEventMap<this>[K]) => any): void;

	readonly layout: IAnyProjectLayout;
	readonly layer: IAnyProjectLayer;

	x: number;
	y: number;
	setPosition(x: number, y: number): void;
	getPosition(): Vec2Arr;
	offsetPosition(dx: number, dy: number): void;

	z: number;
	totalZ: number;
	setPosition3d(x: number, y: number, z: number): void;
	getPosition3d(): Vec3Arr;
	offsetPosition3d(dx: number, dy: number, dz: number): void;

	// zElevation and totalZElevation properties preserved for backwards compatibility only
	/**
	 * @deprecated Use 'z' property instead
	 */
	zElevation: number;

	/**
	 * @deprecated Use 'totalZ' property instead
	 */
	readonly totalZElevation: number;

	originX: number;
	originY: number;
	setOrigin(x: number, y: number): void;
	getOrigin(): Vec2Arr;

	width: number;
	height: number;
	depth: number;
	setSize(w: number, h: number): void;
	getSize(): Vec2Arr;
	setSize3d(w: number, h: number, d: number): void;
	getSize3d(): Vec3Arr;
	
	angle: number;
	angleDegrees: number;

	getBoundingBox(ignoreMesh?: boolean): DOMRect;
	getBoundingQuad(ignoreMesh?: boolean): DOMQuad;
	isOnScreen(): boolean;

	isVisible: boolean;
	opacity: number;
	colorRgb: Vec3Arr;
	blendMode: BlendModeParameter;
	sampling: SamplingModeOrAutoType;
    activeSampling: SamplingModeType;
	effects: IEffectInstance[];

	moveToTop(): void;
	moveToBottom(): void;
	moveToLayer(layer: ILayer): void;
	moveAdjacentToInstance(otherInst: IWorldInstance, isAfter: boolean): void;
	readonly zIndex: number;

	isCollisionEnabled: boolean;
	containsPoint(x: number, y: number): boolean;
	testOverlap(inst: IWorldInstance): boolean;
	testOverlapSolid(): IWorldInstance | null;

	getParent(): IWorldInstance | null;
	getTopParent(): IWorldInstance | null;
	parents(): Generator<IWorldInstance>;
	getChildCount(): number;
	getChildAt(index: number): IWorldInstance | null;
	children(): Generator<IWorldInstance>;
	allChildren(): Generator<IWorldInstance>;
	addChild(child: IWorldInstance, opts?: SceneGraphAddChildOpts): void;
	removeChild(child: IWorldInstance): void;
	removeFromParent(): void;
	getHierarchyOpts(): SceneGraphHierarchyOpts;

	createMesh(hsize: number, vsize: number): void;
	releaseMesh(): void;
	setMeshPoint(col: number, row: number, opts: SetMeshPointOpts): void;
	getMeshPoint(col: number, row: number): GetMeshPointOpts;
	getMeshSize(): Vec2Arr;
}
