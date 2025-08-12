
/** The global runOnStartup() function is called by Construct on startup, while the
 * loading screen is still showing, as the first point of entry in to the engine.
 * Typically a runtime event listener will be added for the next event of interest,
 * such as "beforeprojectstart". */
declare function runOnStartup(callback: (runtime: IRuntime) => void | Promise<void>): void;

// For editor validation of TypeScript in event sheets
declare namespace C3 {
    let TypeScriptInEvents: any;
}

// General runtime type definitions
type LayoutParameter = string | number;
type LayerParameter = string | number;
type ImagePointParameter = string | number;
type BlendModeParameter = "normal" | "additive" | "copy" | "destination-over" | "source-in" | "destination-in" | "source-out" | "destination-out" | "source-atop" | "destination-atop";

type FramerateModeType = "vsync" | "unlimited-tick" | "unlimited-frame";
type SamplingModeType = "nearest" | "bilinear" | "trilinear";

type TextAlignHorizontalMode = "left" | "center" | "right";
type TextAlignVerticalMode = "top" | "center" | "bottom";
type TextWordWrapMode = "word" | "cjk" | "character";
type TextDirectionMode = "ltr" | "rtl";

interface TextFragmentPositionAndSize {
	x: number,
	y: number,
	width: number,
	height: number
}

type SDKPropertyType = number | string | boolean;
type WrapperExtensionParameterType = number | string | boolean;

type TypedArray = Int8Array | Uint8Array | Uint8ClampedArray | Int16Array |Uint16Array | Int32Array | Uint32Array | Float32Array | Float64Array | BigInt64Array | BigUint64Array;
type JSONValue = string | number | boolean | null
    | { [x: string]: JSONValue }
    | Array<JSONValue>;
type JSONObject = { [x: string]: JSONValue };
type JSONArray = Array<JSONValue>;

type CallFunctionParameter = string | number;
type CallFunctionReturnValue = string | number | null;

interface ConstructEvent {
}

declare class ConstructEventTarget<EventMapType> {
	addEventListener<K extends keyof EventMapType>(type: K, listener: (ev: EventMapType[K]) => any): void;
	removeEventListener<K extends keyof EventMapType>(type: K, listener: (ev: EventMapType[K]) => any): void;
}

declare class ConstructEventTargetDispatcher<EventMapType> extends ConstructEventTarget<EventMapType> {
	dispatchEvent(evt: ConstructEvent): void;
}

interface ConstructResizeEvent extends ConstructEvent {
	cssWidth: number;
	cssHeight: number;
	deviceWidth: number;
	deviceHeight: number;
}

interface ConstructPointerEvent extends PointerEvent {
	lastButtons: number;
}

interface ConstructSaveEvent extends ConstructEvent {
	saveData: any;
}

interface ConstructDrawEvent extends ConstructEvent {
	renderer: IRenderer;
}

interface RuntimeEventMap {
	"suspend": ConstructEvent;
	"resume": ConstructEvent;
	"resize": ConstructResizeEvent;
	"pretick": ConstructEvent;
	"tick": ConstructEvent;
	"tick2": ConstructEvent;
    "beforeprojectstart": ConstructEvent;
	"afterprojectstart": ConstructEvent;
	"beforeanylayoutstart": LayoutEvent;
	"afteranylayoutstart": LayoutEvent;
	"beforeanylayoutend": LayoutEvent;
	"afteranylayoutend": LayoutEvent;
	"keydown": KeyboardEvent;
	"keyup": KeyboardEvent;
	"mousedown": MouseEvent;
	"mousemove": MouseEvent;
	"mouseup": MouseEvent;
	"dblclick": MouseEvent;
	"wheel": WheelEvent;
	"pointerdown": ConstructPointerEvent;
	"pointermove": ConstructPointerEvent;
	"pointerup": ConstructPointerEvent;
	"pointercancel": ConstructPointerEvent;
	"deviceorientation": DeviceOrientationEvent;
	"devicemotion": DeviceMotionEvent;
	"save": ConstructSaveEvent;
	"load": ConstructSaveEvent;
	"afterload": ConstructEvent;
	"instancecreate": InstanceEvent;
	"hierarchyready": InstanceEvent<IWorldInstance>;
	"instancedestroy": InstanceDestroyEvent;
	"loadingprogress": ConstructEvent;
}

/** Represents the Construct engine itself, and is the main entry point in to various Construct APIs.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/iruntime | IRuntime documentation } */
declare class IRuntime extends ConstructEventTarget<RuntimeEventMap>
{
	readonly objects: IConstructProjectObjects;
	readonly globalVars: IConstructProjectGlobalVariables;

	readonly assets: IAssetManager;
	readonly collisions: ICollisionEngine;
	readonly storage: IStorage;
	readonly keyboard: IKeyboardObjectType;
	readonly mouse: IMouseObjectType;
	readonly touch: ITouchObjectType;
	readonly timelineController: ITimelineControllerObjectType;
	readonly platformInfo: IPlatformInfo;
	get renderer(): IRenderer;
	readonly sdk: ISDKUtils;

	readonly layout: IAnyProjectLayout;
	getLayout(nameOrIndex: LayoutParameter): IAnyProjectLayout;
	getAllLayouts(): IAnyProjectLayout[];
	goToLayout(nameOrIndex: LayoutParameter): void;

	readonly projectName: string;
	readonly projectVersion: string;
	readonly projectId: string;
	readonly projectUniqueId: string;
	readonly exportDate: Date;
	readonly isInWorker: boolean;
	readonly viewportWidth: number;
	readonly viewportHeight: number;
	getViewportSize(): Vec2Arr;

	readonly sampling: SamplingModeType;
	readonly isPixelRoundingEnabled: boolean;

	get dt(): number;
	get dtRaw(): number;
	get gameTime(): number;
	get wallTime(): number;
	get tickCount(): number;
	timeScale: number;
	get isSuspended(): boolean;

	/**
	 * @deprecated Use framesPerSecond instead of fps
	 */
	get fps(): number;

	get framesPerSecond(): number;
	get ticksPerSecond(): number;
	get cpuUtilisation(): number;
	get gpuUtilisation(): number;
	framerateMode: FramerateModeType;
	minDt: number;
	maxDt: number;

	get loadingProgress(): number;
	get imageLoadingProgress(): number;

	getInstanceByUid(uid: number): IInstance | null;
	sortZOrder(iterable: Iterable<IWorldInstance>, callback: (a: IWorldInstance, b: IWorldInstance) => number): void;

	/** Call a function in an event sheet, by a case-insensitive string of its name.
	 * Each parameter provided for 'params' is passed to the function.
	 * There must be at least as many parameters provided as the function uses,
	 * although any additional parameters will be ignored. If the function has a
	 * return value, it will be returned from this method, else it returns null. */
	callFunction(name: string, ...params: CallFunctionParameter[]): CallFunctionReturnValue;

	signal(tag: string): void;
	waitForSignal(tag: string): Promise<void>;

	/** When called from an event sheet, sets the current function return value,
	 * much like the 'Set return value' action.	 */
	setReturnValue(value: number | string): void;

	/** Return a random number in the range [0, 1). This is similar to Math.random(),
	 * but can produce a deterministic sequence of values if the Advanced Random object
	 * overrides the system random. */
	random(): number;

	/** Runtime wrapper for creating a Web Worker, avoiding some issues with browser bugs and
	 * nested workers.
	 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/guides/creating-workers | Creating workers}
	 * @deprecated All modern browsers now support nested workers so this method is no longer needed. */
	createWorker(url: string, opts?: WorkerOptions): Promise<MessagePort>;

	/** Invoke a browser download of the content at the given URL, using the provided filename. */
	invokeDownload(url: string, filename: string): void;

	/** Runtime wrapper for alert() method which can be used in worker mode. */
	alert(message: string): Promise<void>;

	getHTMLLayer(index: number): HTMLElement;

	/**
	 * @deprecated Use runtime.sdk.addLoadPromise() instead of runtime.addLoadPromise()
	 */
	addLoadPromise(promise: Promise<void>): void;

	saveCanvasImage(format?: string, quality?: number, areaRect?: DOMRect): Promise<Blob>;
}
