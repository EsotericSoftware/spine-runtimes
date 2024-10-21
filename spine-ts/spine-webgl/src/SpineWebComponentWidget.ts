/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import {
	Animation,
	AnimationState,
	AnimationStateData,
	AtlasAttachmentLoader,
	AssetManager,
	Color,
	Disposable,
	Input,
	LoadingScreen,
	ManagedWebGLRenderingContext,
	MixBlend,
	MixDirection,
	Physics,
	SceneRenderer,
	SkeletonBinary,
	SkeletonData,
	SkeletonJson,
	Skeleton,
	TextureAtlas,
	TimeKeeper,
	Vector2,
	Vector3,
} from "./index.js";

interface Point {
	x: number,
	y: number,
}

interface Rectangle extends Point {
	width: number,
	height: number,
}

type BeforeAfterUpdateSpineWidgetFunction = (skeleton: Skeleton, state: AnimationState) => void;
type UpdateSpineWidgetFunction = (delta: number, skeleton: Skeleton, state: AnimationState) => void;

export type OffScreenUpdateBehaviourType = "pause" | "update" | "pose";
function isOffScreenUpdateBehaviourType (value: string | null): value is OffScreenUpdateBehaviourType {
	return (
		value === "pause" ||
		value === "update" ||
		value === "pose"
	);
}

export type ModeType = "inside" | "origin";
function isModeType (value: string | null): value is ModeType {
	return (
		value === "inside" ||
		value === "origin"
	);
}

export type FitType = "fill" | "width" | "height" | "contain" | "cover" | "none" | "scaleDown";
function isFitType (value: string | null): value is FitType {
	return (
		value === "fill" ||
		value === "width" ||
		value === "height" ||
		value === "contain" ||
		value === "cover" ||
		value === "none" ||
		value === "scaleDown"
	);
}

export type AttributeTypes = "string" | "number" | "boolean" | "string-number" | "fitType" | "modeType" | "offScreenUpdateBehaviourType";

// The properties that map to widget attributes
interface WidgetAttributes {
	atlasPath?: string
	skeletonPath?: string
	jsonSkeletonKey?: string
	scale: number
	animation?: string
	skin?: string
	fit: FitType
	mode: ModeType
	xAxis: number
	yAxis: number
	offsetX: number
	offsetY: number
	padLeft: number
	padRight: number
	padTop: number
	padBottom: number
	boundsX: number
	boundsY: number
	boundsWidth: number
	boundsHeight: number
	autoRecalculateBounds: boolean
	width: number
	height: number
	isDraggable: boolean
	debug: boolean
	identifier: string
	manualStart: boolean
	onScreenManualStart: boolean
	pages?: Array<number>
	clip: boolean
	offScreenUpdateBehaviour: OffScreenUpdateBehaviourType
	loadingSpinner: boolean
}

// The methods user can override to have custom behaviour
interface WidgetOverridableMethods {
	update?: UpdateSpineWidgetFunction;
	beforeUpdateWorldTransforms: BeforeAfterUpdateSpineWidgetFunction;
	afterUpdateWorldTransforms: BeforeAfterUpdateSpineWidgetFunction;
	onScreenFunction: (widget: SpineWebComponentWidget) => void
}

// Properties that does not map to any widget attribute, but that might be useful
interface WidgetPublicProperties {
	skeleton: Skeleton
	state: AnimationState
	bounds: Rectangle
	onScreen: boolean
	onScreenAtLeastOnce: boolean
	loadingPromise: Promise<SpineWebComponentWidget>
	loading: boolean
	started: boolean
	textureAtlas: TextureAtlas
	disposed: boolean
}

// Usage of this properties is discouraged because they can be made private in the future
interface WidgetInternalProperties {
	currentScaleDpi: number
	dragging: boolean
	dragX: number
	dragY: number
	dragBoundsRectangle: Rectangle
	debugDragDiv: HTMLDivElement
}

export class SpineWebComponentWidget extends HTMLElement implements Disposable, WidgetAttributes, WidgetOverridableMethods, WidgetInternalProperties, Partial<WidgetPublicProperties> {

	// this promise in necessary only for manual start. Before calling manual start is necessary that the overlay has been assigned to the widget.
	// overlay assignment is asynchronous due to webcomponent promotion and dom load termination.
	// When manual start is false, loadSkeleton is invoked after the overlay is assigned. loadSkeleton needs the assetManager that is owned by the overlay.
	// the overlay owns the assetManager because the overly owns the gl context.
	// if it wasn't for the gl context with which textures are created, we could:
	// - have a unique asset manager independent from the overlay (we literally reload the same assets in two different overlays)
	// - remove overlayAssignedPromise and the needs to wait for its resolving
	// - remove appendTo that is just to avoid the user to use the overlayAssignedPromise when the widget is created using js
	public overlayAssignedPromise: Promise<void>;

	public async appendTo (element: HTMLElement): Promise<void> {
		element.appendChild(this);
		await this.overlayAssignedPromise;
	}

	/**
	 * If true, enables a top-left span showing FPS (it has black text)
	 */
	public static SHOW_FPS = false;

	/**
	 * The URL of the skeleton atlas file (.atlas)
	 * Connected to `atlas` attribute.
	 */
	public atlasPath?: string;

	/**
	 * The URL of the skeleton JSON (.json) or binary (.skel) file
	 * Connected to `skeleton` attribute.
	 */
	public skeletonPath?: string;

	/**
	 * The name of the skeleton when the skeleton file is a JSON and contains multiple skeletons.
	 * Connected to `json-skeleton-key` attribute.
	 */
	public jsonSkeletonKey?: string;

	/**
	 * The scale passed to the Skeleton Loader. SkeletonData values will be scaled accordingly.
	 * Default: 1
	 * Connected to `scale` attribute.
	 */
	public scale = 1;

	/**
	 * Optional: The name of the animation to be played
	 * Connected to `animation` attribute.
	 */
	public get animation (): string | undefined {
		return this._animation;
	}
	public set animation (value: string | undefined) {
		if (value === "") value = undefined;
		this._animation = value;
		this.initWidget();
	}
	private _animation?: string

	/**
	 * Optional: The name of the skin to be set
	 * Connected to `skin` attribute.
	 */
	public get skin (): string | undefined {
		return this._skin;
	}
	public set skin (value: string | undefined) {
		this._skin = value;
		this.initWidget();
	}
	private _skin?: string

	/**
	 * Specify the way the skeleton is sized within the element automatically changing its `scaleX` and `scaleY`.
	 * It works only with {@link mode} `inside`. Possible values are:
	 * - `contain`: as large as possible while still containing the skeleton entirely within the element container (Default).
	 * - `fill`: fill the element container by distorting the skeleton's aspect ratio.
	 * - `width`: make sure the full width of the source is shown, regardless of whether this means the skeleton overflows the element container vertically.
	 * - `height`: make sure the full height of the source is shown, regardless of whether this means the skeleton overflows the element container horizontally.
	 * - `cover`: as small as possible while still covering the entire element container.
	 * - `scaleDown`: scale the skeleton down to ensure that the skeleton fits within the element container.
	 * - `none`: display the skeleton without autoscaling it.
	 * Connected to `fit` attribute.
	 */
	public fit: FitType = "contain";

	/**
	 * Specify the way the skeleton is centered within the element container:
	 * - `inside`: the skeleton bounds center is centered with the element container (Default)
	 * - `origin`: the skeleton origin is centered with the element container regardless of the bounds.
	 * Origin does not allow to specify any {@link fit} type and guarantee the skeleton to not be autoscaled.
	 * Connected to `mode` attribute.
	 */
	public mode: ModeType = "inside";

	/**
	 * The x offset of the skeleton world origin x axis as a percentage of the element container width
	 * Connected to `x-axis` attribute.
	 */
	public xAxis = 0;

	/**
	 * The y offset of the skeleton world origin x axis as a percentage of the element container height
	 * Connected to `y-axis` attribute.
	 */
	public yAxis = 0;

	/**
	 * The x offset of the root in pixels wrt to the skeleton world origin
	 * Connected to `offset-x` attribute.
	 */
	public offsetX = 0;

	/**
	 * The y offset of the root in pixels wrt to the skeleton world origin
	 * Connected to `offset-y` attribute.
	 */
	public offsetY = 0;

	/**
	 * A padding that shrink the element container virtually from left as a percentage of the element container width
	 * Connected to `pad-left` attribute.
	 */
	public padLeft = 0;

	/**
	 * A padding that shrink the element container virtually from right as a percentage of the element container width
	 * Connected to `pad-right` attribute.
	 */
	public padRight = 0;

	/**
	 * A padding that shrink the element container virtually from the top as a percentage of the element container height
	 * Connected to `pad-top` attribute.
	 */
	public padTop = 0;

	/**
	 * A padding that shrink the element container virtually from the bottom as a percentage of the element container height
	 * Connected to `pad-bottom` attribute.
	 */
	public padBottom = 0;

	/**
	 * A rectangle representing the bounds used to fit the skeleton within the element container.
	 * The rectangle coordinates and size are expressed in the Spine world space, not the screen space.
	 * It is automatically calculated using the `skin` and `animation` provided by the user during loading.
	 * If no skin is provided, it is used the default skin.
	 * If no animation is provided, it is used the setup pose.
	 * Bounds are not automatically recalculated.when the animation or skin change.
	 * Invoke {@link recalculateBounds} to recalculate them, or set {@link autoRecalculateBounds} to true.
	 * Use `setBounds` to set you desired bounds. Bounding Box might be useful to determine the bounds to be used.
	 * If the skeleton overflow the element container consider setting {@link clip} to `true`.
	 */
	public bounds: Rectangle = { x: 0, y: 0, width: -1, height: -1 };

	/**
	 * The x of the bounds in Spine world coordinates
	 * Connected to `bound-x` attribute.
	 */
	get boundsX (): number {
		return this.bounds.x;
	}
	set boundsX (value: number) {
		this.bounds.x = value;
	}

	/**
	 * The y of the bounds in Spine world coordinates
	 * Connected to `bound-y` attribute.
	 */
	get boundsY (): number {
		return this.bounds.y;
	}
	set boundsY (value: number) {
		this.bounds.y = value;
	}

	/**
	 * The width of the bounds in Spine world coordinates
	 * Connected to `bound-width` attribute.
	 */
	get boundsWidth (): number {
		return this.bounds.width;
	}
	set boundsWidth (value: number) {
		this.bounds.width = value;
		if (value <= 0) this.initWidget(true);
	}

	/**
	 * The height of the bounds in Spine world coordinates
	 * Connected to `bound-height` attribute.
	 */
	get boundsHeight (): number {
		return this.bounds.height;
	}
	set boundsHeight (value: number) {
		this.bounds.height = value;
		if (value <= 0) this.initWidget(true);
	}

	/**
	 * Whether or not the bounds are recalculated when an animation or a skin is changed. `false` by default.
	 * Connected to `auto-recalculate-bounds` attribute.
	 */
	public autoRecalculateBounds = false;

	/**
	 * Specify a fixed width for the widget. If at least one of `width` and `height` is > 0,
	 * the widget will have an actual size and the element container reference is the widget itself, not the element container parent.
	 * Connected to `width` attribute.
	 */
	public get width (): number {
		return this._width;
	}
	public set width (value: number) {
		this._width = value;
		this.render();
	}
	private _width = -1

	/**
	 * Specify a fixed height for the widget. If at least one of `width` and `height` is > 0,
	 * the widget will have an actual size and the element container reference is the widget itself, not the element container parent.
	 * Connected to `height` attribute.
	 */
	public get height (): number {
		return this._height;
	}
	public set height (value: number) {
		this._height = value;
		this.render();
	}
	private _height = -1

	/**
	 * If true, the widget is draggable
	 * Connected to `isdraggable` attribute.
	 */
	public isDraggable = false;

	/**
	 * The x of the root relative to the canvas/webgl context center in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public worldX = 0;

	/**
	 * The y of the root relative to the canvas/webgl context center in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public worldY = 0;

	/**
	 * The x coordinate of the cursor relative to the cursor relative to the skeleton root in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public cursorWorldX = 1;

	/**
	 * The x coordinate of the cursor relative to the cursor relative to the skeleton root in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public cursorWorldY = 1;

	/**
	 * If true, some convenience elements are drawn to show the skeleton world origin (green),
	 * the root (red), and the bounds rectangle (blue)
	 * Connected to `debug` attribute.
	 */
	public debug = false;

	/**
	 * An identifier to obtain a reference to this widget using the getSpineWidget function
	 * Connected to `identifier` attribute.
	 */
	public identifier = "";

	/**
	 * If true, assets loading are loaded immediately and the skeleton shown as soon as the assets are loaded
	 * If false, it is necessary to invoke the start method to start the loading process
	 * Connected to `manual-start` attribute.
	 */
	public manualStart = false;

	/**
	 * If true and manualStart is true, allows the default {@link onScreenFunction} to invoke the {@link start} method.
	 * This is useful when you want to load the assets only when the widget is revealed.
	 * By default, is false implying the start method to be invoked manually.
	 * Connected to `on-screen-manual-start` attribute.
	 */
	public onScreenManualStart = false;

	/**
	 * An array of indexes indicating the atlas pages indexes to be loaded.
	 * If undefined, all pages are loaded. If empty (default), no page is loaded;
	 * in this case the user can add later the indexes of the pages they want to load
	 * and call the loadTexturesInPagesAttribute, to lazily load them.
	 * Connected to `pages` attribute.
	 */
	public pages?: Array<number>;

	/**
	 * If `true`, the skeleton is clipped to the element container bounds.
	 * Be careful on using this feature because it breaks batching!
	 * Connected to `clip` attribute.
	 */
	public clip = false;

	/**
	 * The widget update/apply behaviour when the skeleton element container is offscreen:
	 * - `pause`: the state is not updated, neither applied (Default)
	 * - `update`: the state is updated, but not applied
	 * - `pose`: the state is updated and applied
	 * Connected to `offscreen` attribute.
	 */
	public offScreenUpdateBehaviour: OffScreenUpdateBehaviourType = "pause";

	/**
	 * If true, the a Spine loading spinner is shown during asset loading
	 * Connected to `spinner` attribute.
	 */
	public loadingSpinner = true;

	/**
	 * Replace the default state and skeleton update logic for this widget.
	 * @param delta - The milliseconds elapsed since the last update.
	 * @param skeleton - A reference to the widget's skeleton
	 * @param state - A reference to the widget's state
	 */
	public update?: UpdateSpineWidgetFunction;

	/**
	 * This callback is invoked before the world transforms are computed allows to execute additional logic.
	 */
	public beforeUpdateWorldTransforms: BeforeAfterUpdateSpineWidgetFunction = () => { };

	/**
	 * This callback is invoked after the world transforms are computed allows to execute additional logic.
	 */
	public afterUpdateWorldTransforms: BeforeAfterUpdateSpineWidgetFunction = () => { };

	/**
	 * A callback invoked each time the element container enters the screen viewport.
	 * By default, the callback call the {@link start} method the first time the widget
	 * enters the screen viewport.
	 */
	public onScreenFunction: (widget: SpineWebComponentWidget) => void = async (widget) => {
		if (widget.loading && !widget.onScreenAtLeastOnce) {
			widget.onScreenAtLeastOnce = true;

			if (widget.manualStart && widget.onScreenManualStart) {
				widget.start();
			}
		}
	}

	/**
	 * The skeleton hosted by this widget. It's ready once assets are loaded.
	 * Safely acces this property by using {@link loadingPromise}.
	 */
	public skeleton?: Skeleton;

	/**
	 * The animation state hosted by this widget. It's ready once assets are loaded.
	 * Safely acces this property by using {@link loadingPromise}.
	 */
	public state?: AnimationState;

	/**
	 * The textureAtlas used by this widget to reference attachments. It's ready once assets are loaded.
	 * Safely acces this property by using {@link loadingPromise}.
	 */
	public textureAtlas?: TextureAtlas;

	/**
	 * A Promise that resolve to the widget itself once assets loading is terminated.
	 * Useful to safely access {@link skeleton} and {@link state} after a new widget has been just created.
	 */
	public loadingPromise: Promise<this>;

	/**
	 * If true, the widget is in the assets loading process.
	 */
	public loading = true;

	/**
	 * A reference to the {@link LoadingScreenWidget} of this widget.
	 * This is instantiated only if it is really necessary.
	 * For example, if {@link loadingSpinner} is `false`, this property value is null
	 */
	public loadingScreen: LoadingScreen | null = null;

	/**
	 * If true, the widget is in the assets loading process.
	 */
	public started = false;

	/**
	 * True, when the element container enters the screen viewport. It uses an IntersectionObserver internally.
	 */
	public onScreen = false;

	/**
	 * True, when the element container enters the screen viewport at least once.
	 * It uses an IntersectionObserver internally.
	 */
	public onScreenAtLeastOnce = false;

	/**
	 * Holds the dpi (devicePixelRatio) currently used to calculate the scale for this skeleton
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public currentScaleDpi = 1;

	/**
	 * The accumulated offset on the x axis due to dragging
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragX = 0;

	/**
	 * The accumulated offset on the y axis due to dragging
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragY = 0;

	/**
	 * If true, the widget is currently being dragged
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragging = false;

	/**
	 * The rectangle in the screen space used to determine if a click is within the skeleton bounds,
	 * so if to start the drag action.
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragBoundsRectangle: Rectangle = { x: 0, y: 0, width: 0, height: 0 };

	/**
	 * An HTMLDivElement used to show the drag surface in debug mode
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public debugDragDiv: HTMLDivElement;

	/**
	 * If true, indicate {@link dispose} has been called and the widget cannot be used anymore
	 */
	public disposed = false;

	/**
	 * Optional: Pass a `SkeletonData`, if you want to avoid creating a new one
	 */
	public skeletonData?: SkeletonData;

	// Reference to the webcomponent shadow root
	private root: ShadowRoot;

	// Reference to the overlay webcomponent
	private overlay!: SpineWebComponentOverlay;

	// Invoked when widget is ready
	private resolveLoadingPromise!: (value: this | PromiseLike<this>) => void;

	// Invoked when widget has an overlay assigned
	private resolveOverlayAssignedPromise!: () => void;

	static attributesDescription: Record<string, { propertyName: keyof WidgetAttributes, type: AttributeTypes, defaultValue?: any }> = {
		atlas: { propertyName: "atlasPath", type: "string" },
		skeleton: { propertyName: "skeletonPath", type: "string" },
		"json-skeleton-key": { propertyName: "jsonSkeletonKey", type: "string" },
		scale: { propertyName: "scale", type: "number" },
		animation: { propertyName: "animation", type: "string", defaultValue: undefined },
		skin: { propertyName: "skin", type: "string" },
		width: { propertyName: "width", type: "number", defaultValue: -1 },
		height: { propertyName: "height", type: "number", defaultValue: -1 },
		isdraggable: { propertyName: "isDraggable", type: "boolean" },
		"x-axis": { propertyName: "xAxis", type: "number" },
		"y-axis": { propertyName: "yAxis", type: "number" },
		"offset-x": { propertyName: "offsetX", type: "number" },
		"offset-y": { propertyName: "offsetY", type: "number" },
		"pad-left": { propertyName: "padLeft", type: "number" },
		"pad-right": { propertyName: "padRight", type: "number" },
		"pad-top": { propertyName: "padTop", type: "number" },
		"pad-bottom": { propertyName: "padBottom", type: "number" },
		"bounds-x": { propertyName: "boundsX", type: "number" },
		"bounds-y": { propertyName: "boundsY", type: "number" },
		"bounds-width": { propertyName: "boundsWidth", type: "number", defaultValue: -1 },
		"bounds-height": { propertyName: "boundsHeight", type: "number", defaultValue: -1 },
		"auto-recalculate-bounds": { propertyName: "autoRecalculateBounds", type: "boolean" },
		identifier: { propertyName: "identifier", type: "string" },
		debug: { propertyName: "debug", type: "boolean" },
		"manual-start": { propertyName: "manualStart", type: "boolean" },
		"on-screen-manual-start": { propertyName: "onScreenManualStart", type: "boolean" },
		spinner: { propertyName: "loadingSpinner", type: "boolean" },
		clip: { propertyName: "clip", type: "boolean" },
		pages: { propertyName: "pages", type: "string-number" },
		fit: { propertyName: "fit", type: "fitType", defaultValue: "contain" },
		mode: { propertyName: "mode", type: "modeType", defaultValue: "inside" },
		offscreen: { propertyName: "offScreenUpdateBehaviour", type: "offScreenUpdateBehaviourType", defaultValue: "pause" },
	}

	static get observedAttributes (): string[] {
		return Object.keys(SpineWebComponentWidget.attributesDescription);
	}

	constructor () {
		super();
		this.root = this.attachShadow({ mode: "closed" });

		this.debugDragDiv = document.createElement("div");
		this.debugDragDiv.style.position = "absolute";
		this.debugDragDiv.style.backgroundColor = "rgba(255, 0, 0, .3)";
		this.debugDragDiv.style.setProperty("pointer-events", "none");

		// these two are terrible code smells
		this.loadingPromise = new Promise<this>((resolve) => {
			this.resolveLoadingPromise = resolve;
		});
		this.overlayAssignedPromise = new Promise<void>((resolve) => {
			this.resolveOverlayAssignedPromise = resolve;
		});
	}

	connectedCallback () {
		if (this.disposed) {
			throw new Error("You cannot attach a disposed widget");
		};

		if (this.overlay) {
			this.initAfterConnect();
		} else {
			window.addEventListener("DOMContentLoaded", this.DOMContentLoadedHandler);
			if (document.readyState !== "loading") {
				this.DOMContentLoadedHandler();
			}
		}

		this.render();
	}

	private initAfterConnect () {
		this.overlay.addWidget(this);
		if (!this.manualStart && !this.started) {
			this.start();
		}
	}

	private DOMContentLoadedHandler = () => {
		customElements.whenDefined("spine-overlay").then(async () => {
			this.overlay = SpineWebComponentOverlay.getOrCreateOverlay(this.getAttribute("overlay-id"));
			this.resolveOverlayAssignedPromise();
			this.initAfterConnect();
		});
	}

	disconnectedCallback (): void {
		window.removeEventListener("DOMContentLoaded", this.DOMContentLoadedHandler);
		const index = this.overlay!.skeletonList.indexOf(this);
		if (index !== -1) {
			this.overlay!.skeletonList.splice(index, 1);
		}
		this.debugDragDiv?.remove();
	}

	/**
	 * Remove the widget from the overlay and the DOM.
	 */
	dispose () {
		this.remove();
		this.loadingScreen?.dispose();
		this.skeletonData = undefined;
		this.skeleton = undefined;
		this.state = undefined;
		this.disposed = true;
	}

	attributeChangedCallback (name: string, oldValue: string | null, newValue: string | null): void {
		const { type, propertyName, defaultValue } = SpineWebComponentWidget.attributesDescription[name];
		const val = castValue(type, newValue, defaultValue);
		(this as any)[propertyName] = val;
		return;
	}

	/**
	 * Starts the widget. Starting the widget means to load the assets currently set into
	 * {@link atlasPath} and {@link skeletonPath}. If start is invoked when the widget is already started,
	 * the skeleton, state, skin and animation will be reset.
	 */
	public start () {
		if (this.started) {
			this.skeleton = undefined;
			this.state = undefined;
			this._skin = undefined;
			this._animation = undefined;
			this.bounds.width = -1;
			this.bounds.height = -1;
		}
		this.started = true;

		customElements.whenDefined("spine-overlay").then(() => {
			this.resolveLoadingPromise(this.loadSkeleton());
		});
	}

	/**
	 * Loads the texture pages in the given `atlas` corresponding to the indexes set into {@link pages}.
	 * This method is automatically called during asset loading. When `pages` is undefined (default),
	 * all pages are loaded. This method is useful when you want to load a subset of pages programmatically.
	 * In that case, set `pages` to an empty array at the beginning.
	 * Then set the pages you want to load and invoke this method.
	 * @param atlas the `TextureAtlas` from which to get the `TextureAtlasPage`s
	 * @returns The list of loaded assets
	 */
	public async loadTexturesInPagesAttribute (atlas: TextureAtlas): Promise<Array<any>> {
		const pagesIndexToLoad = this.pages ?? atlas.pages.map((_, i) => i); // if no pages provided, loads all
		const atlasPath = this.atlasPath?.includes("/") ? this.atlasPath.substring(0, this.atlasPath.lastIndexOf("/") + 1) : "";
		const promisePageList: Array<Promise<any>> = [];
		pagesIndexToLoad.forEach((index) => {
			const page = atlas.pages[index];
			const promiseTextureLoad = this.overlay.assetManager.loadTextureAsync(`${atlasPath}${page.name}`).then(texture => page.setTexture(texture));
			promisePageList.push(promiseTextureLoad);
		});

		return Promise.all(promisePageList)
	}

	/**
	 * @returns The `HTMLElement` where the widget is hosted.
	 */
	public getHTMLElementReference (): HTMLElement {
		return (this.width <= 0 || this.width <= 0) && !this.getAttribute("style")
			? this.parentElement!
			: this;
	}

	/**
	 * Recalculates and sets the bounds of the current animation on track 0.
	 * Useful when animations or skins are set programmatically.
	 * @returns void
	 */
	public recalculateBounds (): void {
		const { skeleton, state } = this;
		if (!skeleton || !state) return;
		const track = state.getCurrent(0);
		const animation = track?.animation as (Animation | undefined);
		const bounds = this.calculateAnimationViewport(animation);
		bounds.x /= skeleton.scaleX;
		bounds.y /= skeleton.scaleY;
		bounds.width /= skeleton.scaleX;
		bounds.height /= skeleton.scaleY;
		this.bounds = bounds;
	}

	// add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
	private async loadSkeleton () {
		this.loading = true;

		const { atlasPath, skeletonPath, scale, skeletonData: skeletonDataInput } = this;
		if (!atlasPath || !skeletonPath) {
			throw new Error(`Missing atlas path or skeleton path. Assets cannot be loaded: atlas: ${atlasPath}, skeleton: ${skeletonPath}`);
		}
		const isBinary = skeletonPath.endsWith(".skel");

		// skeleton and atlas txt are loaded immeaditely
		// textures are loaeded depending on the 'pages' param:
		// - [0,2]: only pages at index 0 and 2 are loaded
		// - []: no page is loaded
		// - undefined: all pages are loaded (default)
		await Promise.all([
			isBinary ? this.overlay.assetManager.loadBinaryAsync(skeletonPath) : this.overlay.assetManager.loadJsonAsync(skeletonPath),
			this.overlay.assetManager.loadTextureAtlasButNoTexturesAsync(atlasPath).then(atlas => this.loadTexturesInPagesAttribute(atlas)),
		]);

		const atlas = this.overlay.assetManager.require(atlasPath);
		const atlasLoader = new AtlasAttachmentLoader(atlas);

		const skeletonLoader = isBinary ? new SkeletonBinary(atlasLoader) : new SkeletonJson(atlasLoader);
		skeletonLoader.scale = scale;

		const skeletonFileAsset = this.overlay.assetManager.require(skeletonPath);
		const skeletonFile = this.jsonSkeletonKey ? skeletonFileAsset[this.jsonSkeletonKey] : skeletonFileAsset;
		const skeletonData = (skeletonDataInput || this.skeleton?.data) ?? skeletonLoader.readSkeletonData(skeletonFile);

		const skeleton = new Skeleton(skeletonData);
		const animationStateData = new AnimationStateData(skeletonData);
		const state = new AnimationState(animationStateData);

		this.skeleton = skeleton;
		this.state = state;
		this.textureAtlas = atlas;

		// ideally we would know the dpi and the zoom, however they are combined
		// to simplify we just assume that the user wants to load the skeleton at scale 1
		// at the current browser zoom level
		// this might be problematic for free-scale modes (origin and inside+none)
		this.currentScaleDpi = window.devicePixelRatio;
		// skeleton.scaleX = this.currentScaleDpi;
		// skeleton.scaleY = this.currentScaleDpi;

		// the bounds are calculated the first time, if no custom bound is provided
		this.initWidget(this.bounds.width <= 0 || this.bounds.height <= 0);

		this.loading = false;
		return this;
	}

	private initWidget (forceRecalculate = false) {
		const { skeleton, state, animation, skin } = this;

		if (skin) {
			skeleton?.setSkinByName(skin);
			skeleton?.setSlotsToSetupPose();
		}
		if (animation) {
			state?.setAnimation(0, animation, true);
		} else {
			state?.setEmptyAnimation(0);
		}

		if (forceRecalculate || this.autoRecalculateBounds) this.recalculateBounds();
	}

	private render (): void {
		let width;
		let height;
		if (this.width === -1 || this.height === -1) {
			width = "0";
			height = "0";
		} else {
			width = `${this.width}px`
			height = `${this.height}px`
		}
		this.root.innerHTML = `
        <style>
            :host {
                position: relative;
                display: inline-block;
                width:  ${width};
                height: ${height};
            }

			:host(.debug-background-color) {
				background-color: rgba(255, 0, 0, 0.3);
			}
        </style>
        `;
	}

	/*
	* Other utilities
	*/

	private calculateAnimationViewport (animation?: Animation): Rectangle {
		const renderer = this.overlay.renderer;
		const { skeleton } = this;
		if (!skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		skeleton.setToSetupPose();

		let offset = new Vector2(), size = new Vector2();
		const tempArray = new Array<number>(2);
		if (!animation) {
			skeleton.updateWorldTransform(Physics.update);
			skeleton.getBounds(offset, size, tempArray, renderer.skeletonRenderer.getSkeletonClipping());
			return {
				x: offset.x,
				y: offset.y,
				width: size.x,
				height: size.y,
			}
		}

		let steps = 100, stepTime = animation.duration ? animation.duration / steps : 0, time = 0;
		let minX = 100000000, maxX = -100000000, minY = 100000000, maxY = -100000000;
		for (let i = 0; i < steps; i++, time += stepTime) {
			animation.apply(skeleton, time, time, false, [], 1, MixBlend.setup, MixDirection.mixIn);
			skeleton.updateWorldTransform(Physics.update);
			skeleton.getBounds(offset, size, tempArray, renderer.skeletonRenderer.getSkeletonClipping());

			if (!isNaN(offset.x) && !isNaN(offset.y) && !isNaN(size.x) && !isNaN(size.y) &&
				!isNaN(minX) && !isNaN(minY) && !isNaN(maxX) && !isNaN(maxY)) {
				minX = Math.min(offset.x, minX);
				maxX = Math.max(offset.x + size.x, maxX);
				minY = Math.min(offset.y, minY);
				maxY = Math.max(offset.y + size.y, maxY);
			} else {
				return { x: 0, y: 0, width: -1, height: -1 };
			}
		}

		return {
			x: minX,
			y: minY,
			width: maxX - minX,
			height: maxY - minY,
		}
	}

}

interface OverlayAttributes {
	overlayId?: string
	scrollable: boolean
	scrollableTweakOff: boolean
	overflowTop: number
	overflowBottom: number
	overflowLeft: number
	overflowRight: number
}

class SpineWebComponentOverlay extends HTMLElement implements OverlayAttributes, Disposable {

	private static OVERLAY_ID = "spine-overlay-default-identifier";
	private static OVERLAY_LIST = new Map<string, SpineWebComponentOverlay>();

	/**
	 * @internal
	 */
	static getOrCreateOverlay (overlayId: string | null): SpineWebComponentOverlay {
		let overlay = SpineWebComponentOverlay.OVERLAY_LIST.get(overlayId || SpineWebComponentOverlay.OVERLAY_ID);
		if (!overlay) {
			overlay = document.createElement('spine-overlay') as SpineWebComponentOverlay;
			overlay.setAttribute('overlay-id', SpineWebComponentOverlay.OVERLAY_ID);
			document.body.appendChild(overlay);
		}
		return overlay;
	}

	/**
	 * A list holding the widgets added to this overlay.
	 */
	public skeletonList = new Array<SpineWebComponentWidget>();

	/**
	 * A reference to the {@link SceneRenderer} used by this overlay.
	 */
	public renderer: SceneRenderer;

	/**
	 * A reference to the {@link AssetManager} used by this overlay.
	 */
	public assetManager: AssetManager;

	/**
	 * The identifier of this overlay. This is necessary when multiply overlay are created.
	   * Connected to `overlay-id` attribute.
	 */
	public overlayId?: string;

	/**
	 * If true, the overlay will have the size of the element container in contrast to the default behaviour where the
	 * overlay has always the size of the screen.
	 * This is necessary when the overlay is inserted into a container that scroll in a different way with respect to the page.
	 * Otherwise the following problems might occur:
	 * 1) For scrollable containers, the widget will be slightly slower to scroll than the html behind. The effect is more evident for lower refresh rate display.
	 * 2) For scrollable containers, the widget will overflow the container bounds until the widget html element container is visible
	 * 3) For fixed containers, the widget will scroll in a jerky way
	 *
	 * In order to fix this behaviour, it is necessary to insert a dedicated `spine-overlay` webcomponent as a direct child of the container.
	 * Moreover, it is necessary to perform the following actions:
	 * 1) The scrollable container must have a `transform`css attribute. If it hasn't this attribute the `spine-overlay` will add it for you.
	 * If your scrollable container has already this css attribute, or if you prefer to add it by yourself (example: `transform: translateZ(0);`), set the `scrollable-tweak-off` to the `spine-overlay`.
	 * 2) The `spine-overlay` must have the `scrollable`attribute
	 * 3) The `spine-overlay` must have an `overlay-id` attribute. Choose the value you prefer.
	 * 4) Each `spine-widget` must have an `overlay-id` attribute. The same as the hosting `spine-overlay`.
	   * Connected to `scrollable` attribute.
	 */
	public scrollable = false;

	/**
	 * If `false` (default value), the overlay container style will be affected adding `transform: translateZ(0);` to it.
	 * The `transform` is not affected if it already exists on the container.
	 * This is necessary to make the scrolling works with containers that scroll in a different way with respect to the page, as explained in {@link scrollable}.
	 * Connected to `scrollable-tweak-off` attribute.
	 */
	public scrollableTweakOff = false;

	/**
	 * How many pixels to add to the top of the canvas to prevent "edge cutting" on fast scrolling, in canvas height units.
	 * By default, the canvas is big as the screen resolution. Making it too big might reduce performance.
	 * Connected to `overflow-top` attribute.
	 */
	public overflowTop = .2;

	/**
	 * How many pixels to add to the bottom of the canvas to prevent "edge cutting" on fast scrolling, in canvas height units.
	 * By default, the canvas is big as the screen resolution. Making it too big might reduce performance.
	 * Connected to `overflow-bottom` attribute.
	 */
	public overflowBottom = .0;

	/**
	 * How many pixels to add to the left of the canvas to prevent "edge cutting" on fast scrolling, in canvas width units.
	 * By default, the canvas is big as the screen resolution. Making it too big might reduce performance.
	 * Connected to `overflow-left` attribute.
	 */
	public overflowLeft = .0;

	/**
	 * How many pixels to add to the right of the canvas to prevent "edge cutting" on fast scrolling, in canvas width units.
	 * By default, the canvas is big as the screen resolution. Making it too big might reduce performance.
	 * Connected to `overflow-right` attribute.
	 */
	public overflowRight = .0;

	private root: ShadowRoot;

	private div: HTMLDivElement;
	private canvas: HTMLCanvasElement;
	private fps: HTMLSpanElement;
	private fpsAppended = false;

	private intersectionObserver?: IntersectionObserver;
	private resizeObserver?: ResizeObserver;
	private input?: Input;

	private overflowLeftSize = 0;
	private overflowTopSize = 0;

	private currentCanvasBaseWidth = 0;
	private currentCanvasBaseHeight = 0;

	private disposed = false;
	private loaded = false;
	readonly time = new TimeKeeper();

	constructor () {
		super();
		this.root = this.attachShadow({ mode: "open" });

		this.div = document.createElement("div");
		this.div.style.position = "absolute";
		this.div.style.top = "0";
		this.div.style.left = "0";
		this.div.style.setProperty("pointer-events", "none");
		this.div.style.overflow = "hidden"
		// this.div.style.backgroundColor = "rgba(0, 255, 0, 0.1)";

		this.root.appendChild(this.div);

		this.canvas = document.createElement("canvas");

		this.div.appendChild(this.canvas);
		this.canvas.style.position = "absolute";
		this.canvas.style.top = "0";
		this.canvas.style.left = "0";

		this.canvas.style.setProperty("pointer-events", "none");
		this.canvas.style.transform = `translate(0px,0px)`;
		// this.canvas.style.setProperty("will-change", "transform"); // performance seems to be even worse with this uncommented

		this.fps = document.createElement("span");
		this.fps.style.position = "fixed";
		this.fps.style.top = "0";
		this.fps.style.left = "0";

		const context = new ManagedWebGLRenderingContext(this.canvas, { alpha: true });
		this.renderer = new SceneRenderer(this.canvas, context);

		this.assetManager = new AssetManager(context);
	}

	connectedCallback (): void {
		let overlayId = this.getAttribute('overlay-id');
		if (!overlayId) {
			overlayId = SpineWebComponentOverlay.OVERLAY_ID;
			this.setAttribute('overlay-id', overlayId);
		}
		const existingOverlay = SpineWebComponentOverlay.OVERLAY_LIST.get(overlayId);
		if (existingOverlay && existingOverlay !== this) {
			throw new Error(`"SpineWebComponentOverlay - You cannot have two spine-overlay with the same overlay-id: ${overlayId}"`);
		}
		SpineWebComponentOverlay.OVERLAY_LIST.set(overlayId, this);
		// window.addEventListener("scroll", this.scrollHandler);
		window.addEventListener("load", this.onLoadCallback);
		if (this.loaded) this.onLoadCallback();
		window.screen.orientation.addEventListener('change', this.orientationChangeCallback);

		this.intersectionObserver = new IntersectionObserver((widgets) => {
			widgets.forEach(({ isIntersecting, target, intersectionRatio }) => {
				const widget = this.skeletonList.find(w => w.getHTMLElementReference() == target);
				if (!widget) return;

				// old browsers do not have isIntersecting
				if (isIntersecting === undefined) {
					isIntersecting = intersectionRatio > 0;
				}

				widget.onScreen = isIntersecting;
				if (isIntersecting) {
					widget.onScreenFunction(widget);
				}
			})
		}, { rootMargin: "30px 20px 30px 20px" });

		// resize observer is supported by all major browsers today chrome started to support it in version 64 (early 2018)
		// we cannot use window.resize event since it does not fire when body resizes, but not the window
		// Alternatively, we can store the body size, check the current body size in the loop (like the translateCanvas), and
		// if they differs call the resizeCallback. I already tested it, and it works. ResizeObserver should be more efficient.
		this.resizeObserver = new ResizeObserver(this.resizeCallback);
		if (this.scrollable) {
			const style = getComputedStyle(this.parentElement!);
			if (style.transform === "none" && !this.scrollableTweakOff) {
				this.parentElement!.style.transform = `translateZ(0)`;
			}
			this.resizeObserver.observe(this.parentElement!);
		} else {
			this.resizeObserver.observe(document.body);
		}

		this.skeletonList.forEach((widget) => {
			this.intersectionObserver?.observe(widget.getHTMLElementReference());
		})
		this.input = this.setupDragUtility();

		this.startRenderingLoop();
	}

	private running = false;
	disconnectedCallback (): void {
		const id = this.getAttribute('id');
		if (id) SpineWebComponentOverlay.OVERLAY_LIST.delete(id);
		// window.removeEventListener("scroll", this.scrollHandler);
		window.removeEventListener("load", this.onLoadCallback);
		window.screen.orientation.removeEventListener('change', this.orientationChangeCallback);
		this.intersectionObserver?.disconnect();
		this.resizeObserver?.disconnect();
		this.input?.dispose();
	}


	static attributesDescription: Record<string, { propertyName: keyof OverlayAttributes, type: AttributeTypes, defaultValue?: any }> = {
		"overlay-id": { propertyName: "overlayId", type: "string" },
		"scrollable": { propertyName: "scrollable", type: "boolean" },
		"scrollable-tweak-off": { propertyName: "scrollableTweakOff", type: "boolean" },
		"overflow-top": { propertyName: "overflowTop", type: "number" },
		"overflow-bottom": { propertyName: "overflowBottom", type: "number" },
		"overflow-left": { propertyName: "overflowLeft", type: "number" },
		"overflow-right": { propertyName: "overflowRight", type: "number" },
	}

	static get observedAttributes (): string[] {
		return Object.keys(SpineWebComponentOverlay.attributesDescription);
	}

	attributeChangedCallback (name: string, oldValue: string | null, newValue: string | null): void {
		const { type, propertyName, defaultValue } = SpineWebComponentOverlay.attributesDescription[name];
		const val = castValue(type, newValue, defaultValue);
		(this as any)[propertyName] = val;
		return;
	}

	private resizeCallback = () => {
		this.updateCanvasSize();
		this.zoomHandler();
	}

	private orientationChangeCallback = () => {
		this.updateCanvasSize();
		// after an orientation change the scrolling changes, but the scroll event does not fire
		this.scrollHandler();
	}

	// right now, we scroll the canvas each frame before rendering loop, that makes scrolling on mobile waaay more smoother
	// this is way scroll handler do nothing
	private scrollHandler = () => {
		// this.translateCanvas();
	}

	private onLoadCallback = () => {
		this.updateCanvasSize();
		this.zoomHandler();
		this.scrollHandler();
		if (!this.loaded) {
			this.parentElement!.appendChild(this);
		}
		this.loaded = true;
	}

	/**
	 * Remove the overlay from the DOM, dispose all the contained widgets, and dispose the renderer.
	 */
	dispose (): void {
		this.remove();
		this.skeletonList.forEach(widget => widget.dispose());
		this.skeletonList.length = 0;
		this.renderer.dispose();
		this.disposed = true;
	}

	addWidget (widget: SpineWebComponentWidget) {
		this.skeletonList.push(widget);
		this.intersectionObserver?.observe(widget.getHTMLElementReference());
		if (this.loaded && (this.compareDocumentPosition(widget) & Node.DOCUMENT_POSITION_FOLLOWING)) {
			this.parentElement!.appendChild(this);
		}
	}

	private startRenderingLoop () {
		if (this.running) return;

		const updateWidgets = () => {
			const delta = this.time.delta;
			this.skeletonList.forEach(({ skeleton, state, update, onScreen, offScreenUpdateBehaviour, beforeUpdateWorldTransforms, afterUpdateWorldTransforms }) => {
				if (!skeleton || !state) return;
				if (!onScreen && offScreenUpdateBehaviour === "pause") return;
				if (update) update(delta, skeleton, state)
				else {
					// delta = 0
					state.update(delta);
					skeleton.update(delta);

					if (onScreen || (!onScreen && offScreenUpdateBehaviour === "pose")) {
						state.apply(skeleton);
						beforeUpdateWorldTransforms(skeleton, state);
						skeleton.updateWorldTransform(Physics.update);
						afterUpdateWorldTransforms(skeleton, state);
					}
				}
			});

			// fps top-left span
			if (SpineWebComponentWidget.SHOW_FPS) {
				if (!this.fpsAppended) {
					this.root.appendChild(this.fps);
					this.fpsAppended = true;
				}
				this.fps.innerText = this.time.framesPerSecond.toFixed(2) + " fps";
			} else {
				if (this.fpsAppended) {
					this.root.removeChild(this.fps);
					this.fpsAppended = false;
				}
			}
		};

		const clear = (r: number, g: number, b: number, a: number) => {
			this.renderer.context.gl.clearColor(r, g, b, a);
			this.renderer.context.gl.clear(this.renderer.context.gl.COLOR_BUFFER_BIT);
		}

		const clipToBoundStart = (divBounds: Rectangle) => {
			// break current batch and start a new one
			this.renderer.end();

			// set the new viewport to the div bound
			const viewportWidth = this.screenToWorldLength(divBounds.width);
			const viewporthHeight = this.screenToWorldLength(divBounds.height);
			this.renderer.context.gl.viewport(
				this.screenToWorldLength(divBounds.x),
				this.canvas.height - this.screenToWorldLength(divBounds.y + divBounds.height),
				viewportWidth,
				viewporthHeight
			);
			this.renderer.camera.setViewport(viewportWidth, viewporthHeight);
			this.renderer.camera.update();

			// start the new batch that will be filled with only this skeleton
			this.renderer.begin();

			// Debug viewport
			// if (true) {
			//     this.renderer.circle(true, -viewportWidth / 2, -viewporthHeight / 2, 20, red);
			//     this.renderer.circle(true, viewportWidth / 2, -viewporthHeight / 2, 20, red);
			//     this.renderer.circle(true, -viewportWidth / 2, viewporthHeight / 2, 20, red);
			//     this.renderer.circle(true, viewportWidth / 2, viewporthHeight / 2, 20, red);
			//     this.renderer.circle(true, 0, 0, 10, red);

			//     this.renderer.rect(true, 0, 0, -viewportWidth, -viewporthHeight, transparentWhite);
			//     this.renderer.rect(true, 0, 0, viewportWidth, viewporthHeight, transparentWhite);
			// }
		}

		const clipToBoundEnd = () => {
			// end clip batch
			this.renderer.end();

			this.renderer.context.gl.viewport(0, 0, this.canvas.width, this.canvas.height);
			this.renderer.camera.setViewport(this.canvas.width, this.canvas.height);
			this.renderer.camera.update();

			// start new normal batch
			this.renderer.begin();
		}

		const renderWidgets = () => {
			clear(0, 0, 0, 0);
			let renderer = this.renderer;
			renderer.begin();

			const ref = this.parentElement!.getBoundingClientRect();
			const tempVector = new Vector3();
			this.skeletonList.forEach((widget) => {
				const { skeleton, bounds, mode, debug, offsetX, offsetY, xAxis, yAxis, dragX, dragY, fit, loadingSpinner, onScreen, loading, clip, isDraggable } = widget;

				if ((!onScreen && dragX === 0 && dragY === 0)) return;
				const elementRef = widget.getHTMLElementReference();
				const divBounds = elementRef.getBoundingClientRect();
				// need to use left and top, because x and y are not available on older browser
				divBounds.x = divBounds.left + this.overflowLeftSize;
				divBounds.y = divBounds.top + this.overflowTopSize;

				if (this.scrollable) {
					divBounds.x -= ref.left;
					divBounds.y -= ref.top;
				}

				const { padLeft, padRight, padTop, padBottom } = widget
				const paddingShiftHorizontal = (padLeft - padRight) / 2;
				const paddingShiftVertical = (padTop - padBottom) / 2;
				let divOriginX = 0;
				let divOriginY = 0;
				if (clip) {
					// in clip mode, the world origin is the div center (divBounds center)
					clipToBoundStart(divBounds);
					divOriginX = this.screenToWorldLength(divBounds.width * (xAxis + paddingShiftHorizontal));
					divOriginY = this.screenToWorldLength(divBounds.height * (yAxis - paddingShiftVertical));
				} else {
					// get the desired point into the the div (center by default) in world coordinate
					const divX = divBounds.x + divBounds.width * ((xAxis + .5) + paddingShiftHorizontal);
					const divY = divBounds.y + divBounds.height * ((-yAxis + .5) + paddingShiftVertical) - 1;
					this.screenToWorld(tempVector, divX, divY);
					divOriginX = tempVector.x;
					divOriginY = tempVector.y;
				}

				const paddingShrinkWidth = 1 - (padLeft + padRight);
				const paddingShrinkHeight = 1 - (padTop + padBottom);
				const divWidthWorld = this.screenToWorldLength(divBounds.width * paddingShrinkWidth);
				const divHeightWorld = this.screenToWorldLength(divBounds.height * paddingShrinkHeight);

				if (loading) {
					if (loadingSpinner) {
						if (!widget.loadingScreen) widget.loadingScreen = new LoadingScreen(renderer);
						widget.loadingScreen!.drawInCoordinates(divOriginX, divOriginY);
					}
					if (clip) clipToBoundEnd();
					return;
				}

				if (skeleton) {
					if (mode === "inside") {
						let { x: ax, y: ay, width: aw, height: ah } = bounds;
						if (aw <= 0 || ah <= 0) return;

						// scale ratio
						const scaleWidth = divWidthWorld / aw;
						const scaleHeight = divHeightWorld / ah;

						// default value is used for fit = none
						let ratioW = skeleton.scaleX;
						let ratioH = skeleton.scaleY;

						if (fit === "fill") { // Fill the target box by distorting the source's aspect ratio.
							ratioW = scaleWidth;
							ratioH = scaleHeight;
						} else if (fit === "width") {
							ratioW = scaleWidth;
							ratioH = scaleWidth;
						} else if (fit === "height") {
							ratioW = scaleHeight;
							ratioH = scaleHeight;
						} else if (fit === "contain") {
							// if scaled height is bigger than div height, use height ratio instead
							if (ah * scaleWidth > divHeightWorld) {
								ratioW = scaleHeight;
								ratioH = scaleHeight;
							} else {
								ratioW = scaleWidth;
								ratioH = scaleWidth;
							}
						} else if (fit === "cover") {
							if (ah * scaleWidth < divHeightWorld) {
								ratioW = scaleHeight;
								ratioH = scaleHeight;
							} else {
								ratioW = scaleWidth;
								ratioH = scaleWidth;
							}
						} else if (fit === "scaleDown") {
							if (aw > divWidthWorld || ah > divHeightWorld) {
								if (ah * scaleWidth > divHeightWorld) {
									ratioW = scaleHeight;
									ratioH = scaleHeight;
								} else {
									ratioW = scaleWidth;
									ratioH = scaleWidth;
								}
							}
						}

						// get the center of the bounds
						const boundsX = (ax + aw / 2) * ratioW;
						const boundsY = (ay + ah / 2) * ratioH;

						// get vertices offset: calculate the distance between div center and bounds center
						divOriginX = divOriginX - boundsX;
						divOriginY = divOriginY - boundsY;

						if (fit !== "none") {
							// scale the skeleton
							skeleton.scaleX = ratioW;
							skeleton.scaleY = ratioH;
							skeleton.updateWorldTransform(Physics.update);
						}
					}

					const worldOffsetX = divOriginX + offsetX + dragX;
					const worldOffsetY = divOriginY + offsetY + dragY;

					widget.worldX = worldOffsetX;
					widget.worldY = worldOffsetY;

					renderer.drawSkeleton(skeleton, true, -1, -1, (vertices, size, vertexSize) => {
						for (let i = 0; i < size; i += vertexSize) {
							vertices[i] = vertices[i] + worldOffsetX;
							vertices[i + 1] = vertices[i + 1] + worldOffsetY;
						}
					});

					// store the draggable surface to make drag logic easier
					if (isDraggable) {
						let { x: ax, y: ay, width: aw, height: ah } = bounds;
						this.worldToScreen(tempVector, ax * skeleton.scaleX + worldOffsetX, ay * skeleton.scaleY + worldOffsetY);
						widget.dragBoundsRectangle.x = tempVector.x + window.scrollX;
						widget.dragBoundsRectangle.y = tempVector.y - this.worldToScreenLength(ah * skeleton.scaleY) + window.scrollY;
						widget.dragBoundsRectangle.width = this.worldToScreenLength(aw * skeleton.scaleX);
						widget.dragBoundsRectangle.height = this.worldToScreenLength(ah * skeleton.scaleY);

						if (clip) {
							widget.dragBoundsRectangle.x += divBounds.x;
							widget.dragBoundsRectangle.y += divBounds.y;
						}

						if (debug && !widget.debugDragDiv.isConnected) {
							document.body.appendChild(widget.debugDragDiv);
						}
						widget.debugDragDiv.style.left = `${widget.dragBoundsRectangle.x - this.overflowLeftSize}px`;
						widget.debugDragDiv.style.top = `${widget.dragBoundsRectangle.y - this.overflowTopSize}px`;
						widget.debugDragDiv.style.width = `${widget.dragBoundsRectangle.width}px`;
						widget.debugDragDiv.style.height = `${widget.dragBoundsRectangle.height}px`;

						if (!debug && widget.debugDragDiv.isConnected) widget.debugDragDiv.remove();
					} else {
						if (widget.debugDragDiv.isConnected) widget.debugDragDiv.remove();
					}

					// drawing debug stuff
					if (debug) {
						// if (true) {
						let { x: ax, y: ay, width: aw, height: ah } = bounds;

						// show bounds and its center
						renderer.rect(false,
							ax * skeleton.scaleX + worldOffsetX,
							ay * skeleton.scaleY + worldOffsetY,
							aw * skeleton.scaleX,
							ah * skeleton.scaleY,
							blue);
						const bbCenterX = (ax + aw / 2) * skeleton.scaleX + worldOffsetX;
						const bbCenterY = (ay + ah / 2) * skeleton.scaleY + worldOffsetY;
						renderer.circle(true, bbCenterX, bbCenterY, 10, blue);

						// show skeleton root
						const root = skeleton.getRootBone()!;
						renderer.circle(true, root.x + worldOffsetX, root.y + worldOffsetY, 10, red);

						// show shifted origin
						const originX = worldOffsetX - dragX - offsetX;
						const originY = worldOffsetY - dragY - offsetY;
						renderer.circle(true, originX, originY, 10, green);

						// show line from origin to bounds center
						renderer.line(originX, originY, bbCenterX, bbCenterY, green);
						if (elementRef === widget) widget.classList.add("debug-background-color");
					} else {
						if (elementRef === widget) widget.classList.remove("debug-background-color");
					}

					if (clip) clipToBoundEnd();
				}
			});

			renderer.end();
		}

		const loop = () => {
			if (this.disposed || !this.isConnected) {
				this.running = false;
				return;
			};
			requestAnimationFrame(loop);
			if (!this.loaded) return;
			this.time.update();
			this.translateCanvas();
			updateWidgets();
			renderWidgets();
		}

		requestAnimationFrame(loop);
		this.running = true;

		const red = new Color(1, 0, 0, 1);
		const green = new Color(0, 1, 0, 1);
		const blue = new Color(0, 0, 1, 1);
		const transparentWhite = new Color(1, 1, 1, .3);
	}

	public cursorCanvasX = 1;
	public cursorCanvasY = 1;
	public cursorWorldX = 1;
	public cursorWorldY = 1;

	private setupDragUtility (): Input {
		// TODO: we should use document - body might have some margin that offset the click events - Meanwhile I take event pageX/Y
		const inputManager = new Input(document.body, false)
		const point: Point = { x: 0, y: 0 };
		const tempVector = new Vector3();

		const getInput = (ev?: MouseEvent | TouchEvent): Point => {
			const originalEvent = ev instanceof MouseEvent ? ev : ev!.changedTouches[0];
			point.x = originalEvent.pageX + this.overflowLeftSize;
			point.y = originalEvent.pageY + this.overflowTopSize;
			return point;
		}

		let prevX = 0;
		let prevY = 0;
		inputManager.addListener({
			// moved is used to pass curson position wrt to canvas and widget position and currently is EXPERIMENTAL
			moved: (x, y, ev) => {
				const input = getInput(ev);
				this.cursorCanvasX = input.x - window.scrollX;
				this.cursorCanvasY = input.y - window.scrollY;

				const ref = this.parentElement!.getBoundingClientRect();
				if (this.scrollable) {
					this.cursorCanvasX -= ref.left;
					this.cursorCanvasY -= ref.top;
				}

				tempVector.set(this.cursorCanvasX, this.cursorCanvasY, 0);
				this.renderer.camera.screenToWorld(tempVector, this.canvas.clientWidth, this.canvas.clientHeight);

				if (Number.isNaN(tempVector.x) || Number.isNaN(tempVector.y)) return;
				this.cursorWorldX = tempVector.x;
				this.cursorWorldY = tempVector.y;
				this.skeletonList.forEach(widget => {
					widget.cursorWorldX = this.cursorWorldX - widget.worldX;
					widget.cursorWorldY = this.cursorWorldY - widget.worldY;
				});
			},
			down: (x, y, ev) => {
				const input = getInput(ev);
				this.skeletonList.forEach(widget => {
					if (!widget.isDraggable || (!widget.onScreen && widget.dragX === 0 && widget.dragY === 0)) return;
					if (inside(input, widget.dragBoundsRectangle)) {
						widget.dragging = true;
						ev?.preventDefault();
					}
				});
				prevX = input.x;
				prevY = input.y;
			},
			dragged: (x, y, ev) => {
				const input = getInput(ev);
				let dragX = input.x - prevX;
				let dragY = input.y - prevY;
				this.skeletonList.forEach(widget => {
					if (!widget.dragging || (!widget.onScreen && widget.dragX === 0 && widget.dragY === 0)) return;
					const skeleton = widget.skeleton!;
					widget.dragX += this.screenToWorldLength(dragX);
					widget.dragY -= this.screenToWorldLength(dragY);
					skeleton.physicsTranslate(dragX, dragY);
					ev?.preventDefault();
					ev?.stopPropagation();
				});
				prevX = input.x;
				prevY = input.y;
			},
			up: () => {
				this.skeletonList.forEach(widget => {
					widget.dragging = false;
				});
			}
		});

		return inputManager;
	}

	/*
	* Resize/scroll utilities
	*/

	private updateCanvasSize () {
		// resize canvas, if necessary
		this.resizeCanvas();

		// temporarely remove the div to get the page size without considering the div
		// this is necessary otherwise if the bigger element in the page is remove and the div
		// was the second bigger element, now it would be the div to determine the page size


		if (!this.scrollable) {
			this.div?.remove();
			const { width, height } = this.getPageSize();
			this.div!.style.width = width + "px";
			this.div!.style.height = height + "px";
			this.root.appendChild(this.div!);
		} else {
			this.div?.remove();
			this.div!.style.width = this.parentElement!.scrollWidth + "px";
			this.div!.style.height = this.parentElement!.scrollHeight + "px";
			// this.canvas.style.transform = `translate(${-this.overflowLeftSize}px,${-this.overflowTopSize}px)`;
			this.root.appendChild(this.div!);
		}


	}

	private resizeCanvas () {
		let width, height;
		if (!this.scrollable) {
			const screenSize = this.getScreenSize();
			width = screenSize.width;
			height = screenSize.height;
		} else {
			width = this.parentElement!.clientWidth;
			height = this.parentElement!.clientHeight;
		}

		// this is needed because screen size is wrong when zoom levels occurs
		// zooming out will make the canvas smaller and its known that zoom level
		// on browsers is not reliable
		// ideally, window.innerWidth/innerHeight would be preferrable. However
		// on mobile browsers the dynamic search bar makes the innerHeight smaller
		// at the beginning (changing the canvas size at each scroll is not ideal)
		// width = Math.max(width, window.innerWidth);
		// height = Math.max(height, window.innerHeight);

		if (this.currentCanvasBaseWidth !== width || this.currentCanvasBaseHeight !== height) {
			this.currentCanvasBaseWidth = width;
			this.currentCanvasBaseHeight = height;
			this.overflowLeftSize = this.overflowLeft * width;
			this.overflowTopSize = this.overflowTop * height;

			const totalWidth = width * (1 + (this.overflowLeft + this.overflowRight));
			const totalHeight = height * (1 + (this.overflowTop + this.overflowBottom));

			this.canvas.style.width = totalWidth + "px";
			this.canvas.style.height = totalHeight + "px";
			this.resize(totalWidth, totalHeight);
		}
	}

	private translateCanvas () {
		let scrollPositionX = -this.overflowLeftSize;
		let scrollPositionY = -this.overflowTopSize;

		if (!this.scrollable) {
			scrollPositionX += window.scrollX;
			scrollPositionY += window.scrollY;
		} else {
			scrollPositionX += this.parentElement!.scrollLeft;
			scrollPositionY += this.parentElement!.scrollTop;
		}

		this.canvas.style.transform = `translate(${scrollPositionX}px,${scrollPositionY}px)`;
	}

	private zoomHandler = () => {
		this.skeletonList.forEach((widget) => {
			// inside mode scale automatically to fit the skeleton within its parent
			if (widget.mode !== "origin" && widget.fit !== "none") return;

			const skeleton = widget.skeleton;
			if (!skeleton) return;
			const scale = window.devicePixelRatio;
			skeleton.scaleX = skeleton.scaleX / widget.currentScaleDpi * scale;
			skeleton.scaleY = skeleton.scaleY / widget.currentScaleDpi * scale;
			widget.currentScaleDpi = scale;
		});

		this.resize(parseFloat(this.canvas.style.width), parseFloat(this.canvas.style.height));
	}

	private resize (width: number, height: number) {
		let canvas = this.canvas;
		this.canvas.width = Math.round(this.screenToWorldLength(width));
		this.canvas.height = Math.round(this.screenToWorldLength(height));
		this.renderer.context.gl.viewport(0, 0, canvas.width, canvas.height);
		this.renderer.camera.setViewport(canvas.width, canvas.height);
		this.renderer.camera.update();
	}

	// we need the bounding client rect otherwise decimals won't be returned
	// this means that during zoom it might occurs that the div would be resized
	// rounded 1px more making a scrollbar appear
	private getPageSize () {
		return document.body.getBoundingClientRect();
	}

	// screen size remain the same when it is rotated
	// we need to swap them based and the orientation angle
	private getScreenSize () {
		const { screen } = window;
		const { width, height } = window.screen;
		const angle = screen.orientation.angle;
		const rotated = angle === 90 || angle === 270;
		return rotated
			? { width: height, height: width }
			: { width, height };
	}

	/*
	* Other utilities
	*/
	public screenToWorld (vec: Vector3, x: number, y: number) {
		vec.set(x, y, 0);
		// pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
		this.renderer.camera.screenToWorld(vec, this.canvas.clientWidth, this.canvas.clientHeight);
	}
	public worldToScreen (vec: Vector3, x: number, y: number) {
		vec.set(x, -y, 0);
		// pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
		// this.renderer.camera.worldToScreen(vec, this.canvas.clientWidth, this.canvas.clientHeight);
		this.renderer.camera.worldToScreen(vec, this.worldToScreenLength(this.renderer.camera.viewportWidth), this.worldToScreenLength(this.renderer.camera.viewportHeight));
	}
	public screenToWorldLength (length: number) {
		return length * window.devicePixelRatio;
	}
	public worldToScreenLength (length: number) {
		return length / window.devicePixelRatio;
	}
}

const inside = (point: { x: number; y: number }, rectangle: Rectangle): boolean => {
	return (
		point.x >= rectangle.x &&
		point.x <= rectangle.x + rectangle.width &&
		point.y >= rectangle.y &&
		point.y <= rectangle.y + rectangle.height
	);
}

customElements.define("spine-widget", SpineWebComponentWidget);
customElements.define("spine-overlay", SpineWebComponentOverlay);

export function getSpineWidget (identifier: string): SpineWebComponentWidget {
	return document.querySelector(`spine-widget[identifier=${identifier}]`) as SpineWebComponentWidget;
}

export function createSpineWidget (parameters: WidgetAttributes): SpineWebComponentWidget {
	const widget = document.createElement("spine-widget") as SpineWebComponentWidget;

	Object.entries(SpineWebComponentWidget.attributesDescription).forEach(entry => {
		const [key, { propertyName }] = entry;
		const value = parameters[propertyName];
		if (value) widget.setAttribute(key, value as any);
	});

	return widget;
}

function castBoolean (value: string | null, defaultValue = "") {
	return value === "true" || value === "" ? true : false;
}

function castString (value: string | null, defaultValue = "") {
	return value === null ? defaultValue : value;
}

function castNumber (value: string | null, defaultValue = 0) {
	if (value === null) return defaultValue;

	const parsed = parseFloat(value);
	if (Number.isNaN(parsed)) return defaultValue;
	return parsed;
}

function castArrayNumber (value: string | null, defaultValue = undefined) {
	if (value === null) return defaultValue;
	return value.split(",").reduce((acc, pageIndex) => {
		const index = parseInt(pageIndex);
		if (!isNaN(index)) acc.push(index);
		return acc;
	}, [] as Array<number>);
}

function castValue (type: AttributeTypes, value: string | null, defaultValue?: any) {
	switch (type) {
		case "string":
			return castString(value, defaultValue);
		case "number":
			return castNumber(value, defaultValue);
		case "boolean":
			return castBoolean(value, defaultValue);
		case "string-number":
			return castArrayNumber(value, defaultValue);
		case "fitType":
			return isFitType(value) ? value : defaultValue;
		case "modeType":
			return isModeType(value) ? value : defaultValue;
		case "offScreenUpdateBehaviourType":
			return isOffScreenUpdateBehaviourType(value) ? value : defaultValue;
		default:
			break;
	}
}