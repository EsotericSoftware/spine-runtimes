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
	Bone,
	Disposable,
	LoadingScreen,
	MeshAttachment,
	MixBlend,
	MixDirection,
	NumberArrayLike,
	Physics,
	RegionAttachment,
	Skeleton,
	SkeletonBinary,
	SkeletonData,
	SkeletonJson,
	Skin,
	Slot,
	TextureAtlas,
	Utils,
	Vector2,
} from "@esotericsoftware/spine-webgl";
import { SpineWebComponentOverlay } from "./SpineWebComponentOverlay.js";
import { AttributeTypes, castValue, isBase64, Rectangle } from "./wcUtils.js";

type UpdateSpineWidgetFunction = (delta: number, skeleton: Skeleton, state: AnimationState) => void;

export type OffScreenUpdateBehaviourType = "pause" | "update" | "pose";
export type FitType = "fill" | "width" | "height" | "contain" | "cover" | "none" | "scaleDown" | "origin";
export type AnimationsInfo = Record<string, {
	cycle?: boolean,
	repeatDelay?: number;
	animations: Array<AnimationsType>
}>;
export type AnimationsType = { animationName: string | "#EMPTY#", loop?: boolean, delay?: number, mixDuration?: number };
export type PointerEventType = "down" | "up" | "enter" | "leave" | "move" | "drag";
export type PointerEventTypesInput = Exclude<PointerEventType, "enter" | "leave">;

// The properties that map to widget attributes
interface WidgetAttributes {
	atlasPath?: string
	skeletonPath?: string
	rawData?: Record<string, string>
	jsonSkeletonKey?: string
	scale: number
	animation?: string
	animations?: AnimationsInfo
	defaultMix?: number
	skin?: string[]
	fit: FitType
	xAxis: number
	yAxis: number
	offsetX: number
	offsetY: number
	padLeft: number
	padRight: number
	padTop: number
	padBottom: number
	animationsBound?: string[]
	boundsX: number
	boundsY: number
	boundsWidth: number
	boundsHeight: number
	autoCalculateBounds: boolean
	width: number
	height: number
	drag: boolean
	interactive: boolean
	debug: boolean
	identifier: string
	manualStart: boolean
	startWhenVisible: boolean
	pages?: Array<number>
	clip: boolean
	offScreenUpdateBehaviour: OffScreenUpdateBehaviourType
	spinner: boolean
}

// The methods user can override to have custom behaviour
interface WidgetOverridableMethods {
	update?: UpdateSpineWidgetFunction;
	beforeUpdateWorldTransforms: UpdateSpineWidgetFunction;
	afterUpdateWorldTransforms: UpdateSpineWidgetFunction;
	onScreenFunction: (widget: SpineWebComponentSkeleton) => void
}

// Properties that does not map to any widget attribute, but that might be useful
interface WidgetPublicProperties {
	skeleton: Skeleton
	state: AnimationState
	bounds: Rectangle
	onScreen: boolean
	onScreenAtLeastOnce: boolean
	whenReady: Promise<SpineWebComponentSkeleton>
	loading: boolean
	started: boolean
	textureAtlas: TextureAtlas
	disposed: boolean
}

// Usage of this properties is discouraged because they can be made private in the future
interface WidgetInternalProperties {
	pma: boolean
	dprScale: number
	dragging: boolean
	dragX: number
	dragY: number
}

export class SpineWebComponentSkeleton extends HTMLElement implements Disposable, WidgetAttributes, WidgetOverridableMethods, WidgetInternalProperties, Partial<WidgetPublicProperties> {

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
	 * Holds the assets in base64 format.
	 * Connected to `raw-data` attribute.
	 */
	public rawData?: Record<string, string>;

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
	 * Optional: The name of the animation to be played. When set, the widget is reinitialized.
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
	 * An {@link AnimationsInfo} that describes a sequence of animations on different tracks.
	 * Connected to `animations` attribute, but since attributes are string, there's a different form to pass it.
	 * It is a string composed of groups surrounded by square brackets. Each group has 5 parameters, the firsts 2 mandatory. They corresponds to: track, animation name, loop, delay, mix time.
	 * For the first group on a track {@link AnimationState.setAnimation} is used, while {@link AnimationState.addAnimation} is used for the others.
	 * If you use the special token #EMPTY# as animation name {@link AnimationState.setEmptyAnimation} and {@link AnimationState.addEmptyAnimation} iare used respectively.
	 * Use the special group [loop, trackNumber], to allow the animation of the track on the given trackNumber to restart from the beginning once finished.
	 */
	public get animations (): AnimationsInfo | undefined {
		return this._animations;
	}
	public set animations (value: AnimationsInfo | undefined) {
		if (value === undefined) value = undefined;
		this._animations = value;
		this.initWidget();
	}
	public _animations?: AnimationsInfo

	/**
	 * Optional: The default mix set to the {@link AnimationStateData.defaultMix}.
	 * Connected to `default-mix` attribute.
	 */
	public get defaultMix (): number {
		return this._defaultMix;
	}
	public set defaultMix (value: number | undefined) {
		if (value === undefined) value = 0;
		this._defaultMix = value;
	}
	public _defaultMix = 0;

	/**
	 * Optional: The name of the skin to be set
	 * Connected to `skin` attribute.
	 */
	public get skin (): string[] | undefined {
		return this._skin;
	}
	public set skin (value: string[] | undefined) {
		this._skin = value;
		this.initWidget();
	}
	private _skin?: string[]

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
	 * - `origin`: the skeleton origin is centered with the element container regardless of the bounds.
	 * Connected to `fit` attribute.
	 */
	public fit: FitType = "contain";

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
	 * Invoke {@link calculateBounds} to recalculate them, or set {@link autoCalculateBounds} to true.
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
	 * Optional: an array of animation names that are used to calculate the bounds of the skeleton.
	 * Connected to `animations-bound` attribute.
	 */
	public animationsBound?: string[];

	/**
	 * Whether or not the bounds are recalculated when an animation or a skin is changed. `false` by default.
	 * Connected to `auto-calculate-bounds` attribute.
	 */
	public autoCalculateBounds = false;

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
	 * Connected to `drag` attribute.
	 */
	public drag = false;

	/**
	 * The x of the root relative to the canvas/webgl context center in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public worldX = Infinity;

	/**
	 * The y of the root relative to the canvas/webgl context center in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public worldY = Infinity;

	/**
	 * The x coordinate of the pointer relative to the pointer relative to the skeleton root in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public pointerWorldX = 1;

	/**
	 * The x coordinate of the pointer relative to the pointer relative to the skeleton root in spine world coordinates.
	 * This is an experimental property and might be removed in the future.
	 */
	public pointerWorldY = 1;

	/**
	 * If true, the widget is interactive
	 * Connected to `interactive` attribute.
	 * This is an experimental property and might be removed in the future.
	 */
	public interactive = false;

	/**
	 * If the widget is interactive, this method is invoked with a {@link PointerEventType} when the pointer
	 * performs actions within the widget bounds (for example, it enter or leaves the bounds).
	 * By default, the function does nothing.
	 * This is an experimental property and might be removed in the future.
	 */
	public pointerEventCallback = (event: PointerEventType, originalEvent?: UIEvent) => { }

	// TODO: probably it makes sense to associate a single callback to a groups of slots to avoid the same callback to be called for each slot of the group
	/**
	 * This methods allows to associate to a Slot a callback. For these slots, if the widget is interactive,
	 * when the pointer performs actions within the slot's attachment the associated callback is invoked with
	 * a {@link PointerEventType} (for example, it enter or leaves the slot's attachment bounds).
	 * This is an experimental property and might be removed in the future.
	 */
	public addPointerSlotEventCallback (slot: number | string | Slot, slotFunction: (slot: Slot, event: PointerEventType) => void) {
		this.pointerSlotEventCallbacks.set(this.getSlotFromRef(slot), { slotFunction, inside: false });
	}

	/**
	 * Remove callbacks added through {@link addPointerSlotEventCallback}.
	 * @param slot: the slot reference to which remove the associated callback
	 */
	public removePointerSlotEventCallbacks (slot: number | string | Slot) {
		this.pointerSlotEventCallbacks.delete(this.getSlotFromRef(slot));
	}

	private getSlotFromRef (slotRef: number | string | Slot): Slot {
		let slot: Slot | null;

		if (typeof slotRef === 'number') slot = this.skeleton!.slots[slotRef];
		else if (typeof slotRef === 'string') slot = this.skeleton!.findSlot(slotRef);
		else slot = slotRef;

		if (!slot) throw new Error(`No slot found with the given slot reference: ${slotRef}`);

		return slot;
	}

	/**
	 * If true, some convenience elements are drawn to show the skeleton world origin (green),
	 * the root (red), and the bounds rectangle (blue)
	 * Connected to `debug` attribute.
	 */
	public debug = false;

	/**
	 * An identifier to obtain this widget using the {@link getSkeleton} function.
	 * This is useful when you need to interact with the widget using js.
	 * Connected to `identifier` attribute.
	 */
	public identifier = "";

	/**
	 * If false, assets loading are loaded immediately and the skeleton shown as soon as the assets are loaded
	 * If true, it is necessary to invoke the start method to start the widget and the loading process
	 * Connected to `manual-start` attribute.
	 */
	public manualStart = false;

	/**
	 * If true, automatically sets manualStart to true to pervent widget to start immediately.
	 * Then, in combination with the default {@link onScreenFunction}, the widget {@link start}
	 * the first time it enters the viewport.
	 * This is useful when you want to load the assets only when the widget is revealed.
	 * By default, is false.
	 * Connected to `start-when-visible` attribute.
	 */
	public set startWhenVisible (value: boolean) {
		this.manualStart = true;
		this._startWhenVisible = value;
	}
	public get startWhenVisible (): boolean {
		return this._startWhenVisible;
	}
	public _startWhenVisible = false;

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
	 * If true, a Spine loading spinner is shown during asset loading. Default to false.
	 * Connected to `spinner` attribute.
	 */
	public spinner = false;

	/**
	 * Replace the default state and skeleton update logic for this widget.
	 * @param delta - The milliseconds elapsed since the last update.
	 * @param skeleton - The widget's skeleton
	 * @param state - The widget's state
	 */
	public update?: UpdateSpineWidgetFunction;

	/**
	 * This callback is invoked before the world transforms are computed allows to execute additional logic.
	 */
	public beforeUpdateWorldTransforms: UpdateSpineWidgetFunction = () => { };

	/**
	 * This callback is invoked after the world transforms are computed allows to execute additional logic.
	 */
	public afterUpdateWorldTransforms: UpdateSpineWidgetFunction = () => { };

	/**
	 * A callback invoked each time the element container enters the screen viewport.
	 * By default, the callback call the {@link start} method the first time the widget
	 * enters the screen viewport and {@link startWhenVisible} is `true`.
	 */
	public onScreenFunction: (widget: SpineWebComponentSkeleton) => void = async (widget) => {
		if (widget.loading && !widget.onScreenAtLeastOnce && widget.manualStart && widget.startWhenVisible)
			widget.start()
	}

	/**
	 * The skeleton hosted by this widget. It's ready once assets are loaded.
	 * Safely acces this property by using {@link whenReady}.
	 */
	public skeleton?: Skeleton;

	/**
	 * The animation state hosted by this widget. It's ready once assets are loaded.
	 * Safely acces this property by using {@link whenReady}.
	 */
	public state?: AnimationState;

	/**
	 * The textureAtlas used by this widget to reference attachments. It's ready once assets are loaded.
	 * Safely acces this property by using {@link whenReady}.
	 */
	public textureAtlas?: TextureAtlas;

	/**
	 * A Promise that resolve to the widget itself once assets loading is terminated.
	 * Useful to safely access {@link skeleton} and {@link state} after a new widget has been just created.
	 */
	public get whenReady (): Promise<this> {
		return this._whenReady;
	};
	private _whenReady: Promise<this>;

	/**
	 * If true, the widget is in the assets loading process.
	 */
	public loading = true;

	/**
	 * The {@link LoadingScreenWidget} of this widget.
	 * This is instantiated only if it is really necessary.
	 * For example, if {@link spinner} is `false`, this property value is null
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
	 * @internal
	 * Holds the dpr (devicePixelRatio) currently used to calculate the scale for this skeleton
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dprScale = 1;

	/**
	 * @internal
	 * The accumulated offset on the x axis due to dragging
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragX = 0;

	/**
	 * @internal
	 * The accumulated offset on the y axis due to dragging
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragY = 0;

	/**
	 * @internal
	 * If true, the widget is currently being dragged
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public dragging = false;

	/**
	 * @internal
	 * If true, the widget has texture with premultiplied alpha
	 * Do not rely on this properties. It might be made private in the future.
	 */
	public pma = false;

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

	// this promise in necessary only for manual start. Before calling manual start is necessary that the overlay has been assigned to the widget.
	// overlay assignment is asynchronous due to webcomponent promotion and dom load termination.
	// When manual start is false, loadSkeleton is invoked after the overlay is assigned. loadSkeleton needs the assetManager that is owned by the overlay.
	// the overlay owns the assetManager because the overly owns the gl context.
	// if it wasn't for the gl context with which textures are created, we could:
	// - have a unique asset manager independent from the overlay (we literally reload the same assets in two different overlays)
	// - remove overlayAssignedPromise and the needs to wait for its resolving
	// - remove appendTo that is just to avoid the user to use the overlayAssignedPromise when the widget is created using js
	private overlayAssignedPromise: Promise<void>;

	static attributesDescription: Record<string, { propertyName: keyof WidgetAttributes, type: AttributeTypes, defaultValue?: any }> = {
		atlas: { propertyName: "atlasPath", type: "string" },
		skeleton: { propertyName: "skeletonPath", type: "string" },
		"raw-data": { propertyName: "rawData", type: "object" },
		"json-skeleton-key": { propertyName: "jsonSkeletonKey", type: "string" },
		scale: { propertyName: "scale", type: "number" },
		animation: { propertyName: "animation", type: "string", defaultValue: undefined },
		animations: { propertyName: "animations", type: "animationsInfo", defaultValue: undefined },
		"animation-bounds": { propertyName: "animationsBound", type: "array-string", defaultValue: undefined },
		"default-mix": { propertyName: "defaultMix", type: "number", defaultValue: 0 },
		skin: { propertyName: "skin", type: "array-string" },
		width: { propertyName: "width", type: "number", defaultValue: -1 },
		height: { propertyName: "height", type: "number", defaultValue: -1 },
		drag: { propertyName: "drag", type: "boolean" },
		interactive: { propertyName: "interactive", type: "boolean" },
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
		"auto-calculate-bounds": { propertyName: "autoCalculateBounds", type: "boolean" },
		identifier: { propertyName: "identifier", type: "string" },
		debug: { propertyName: "debug", type: "boolean" },
		"manual-start": { propertyName: "manualStart", type: "boolean" },
		"start-when-visible": { propertyName: "startWhenVisible", type: "boolean" },
		"spinner": { propertyName: "spinner", type: "boolean" },
		clip: { propertyName: "clip", type: "boolean" },
		pages: { propertyName: "pages", type: "array-number" },
		fit: { propertyName: "fit", type: "fitType", defaultValue: "contain" },
		offscreen: { propertyName: "offScreenUpdateBehaviour", type: "offScreenUpdateBehaviourType", defaultValue: "pause" },
	}

	static get observedAttributes (): string[] {
		return Object.keys(SpineWebComponentSkeleton.attributesDescription);
	}

	constructor () {
		super();
		this.root = this.attachShadow({ mode: "closed" });

		// these two are terrible code smells
		this._whenReady = new Promise<this>((resolve) => {
			this.resolveLoadingPromise = resolve;
		});
		this.overlayAssignedPromise = new Promise<void>((resolve) => {
			this.resolveOverlayAssignedPromise = resolve;
		});
	}

	connectedCallback (): void {
		if (this.disposed) {
			throw new Error("You cannot attach a disposed widget");
		};

		if (this.overlay) {
			this.initAfterConnect();
		} else {
			if (document.readyState === "loading") window.addEventListener("DOMContentLoaded", this.DOMContentLoadedCallback);
			else this.DOMContentLoadedCallback();
		}

		this.render();
	}

	private initAfterConnect () {
		this.overlay.addWidget(this);
		if (!this.manualStart && !this.started) {
			this.start();
		}
	}

	private DOMContentLoadedCallback = () => {
		customElements.whenDefined("spine-overlay").then(async () => {
			this.overlay = SpineWebComponentOverlay.getOrCreateOverlay(this.getAttribute("overlay-id"));
			this.resolveOverlayAssignedPromise();
			this.initAfterConnect();
		});
	}

	disconnectedCallback (): void {
		window.removeEventListener("DOMContentLoaded", this.DOMContentLoadedCallback);
		const index = this.overlay?.widgets.indexOf(this);
		if (index > 0) {
			this.overlay!.widgets.splice(index, 1);
		}
	}

	/**
	 * Remove the widget from the overlay and the DOM.
	 */
	dispose () {
		this.disposed = true;
		this.disposeGLResources();
		this.loadingScreen?.dispose();
		this.overlay.removeWidget(this);
		this.remove();
		this.skeletonData = undefined;
		this.skeleton = undefined;
		this.state = undefined;
	}

	attributeChangedCallback (name: string, oldValue: string | null, newValue: string | null): void {
		const { type, propertyName, defaultValue } = SpineWebComponentSkeleton.attributesDescription[name];
		const val = castValue(type, newValue, defaultValue);
		(this as any)[propertyName] = val;
		return;
	}

	/**
	 * Starts the widget. Starting the widget means to load the assets currently set into
	 * {@link atlasPath} and {@link skeletonPath}. If start is invoked when the widget is already started,
	 * the skeleton and the state are reset. Bounds are recalculated only if {@link autoCalculateBounds} is true.
	 */
	public start () {
		if (this.started) {
			this.skeleton = undefined;
			this.state = undefined;
			this._whenReady = new Promise<this>((resolve) => {
				this.resolveLoadingPromise = resolve;
			});
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
	public async loadTexturesInPagesAttribute (): Promise<Array<any>> {
		const atlas = this.overlay.assetManager.require(this.atlasPath!) as TextureAtlas;
		const pagesIndexToLoad = this.pages ?? atlas.pages.map((_, i) => i); // if no pages provided, loads all
		const atlasPath = this.atlasPath?.includes("/") ? this.atlasPath.substring(0, this.atlasPath.lastIndexOf("/") + 1) : "";
		const promisePageList: Array<Promise<any>> = [];
		const texturePaths = [];

		for (const index of pagesIndexToLoad) {
			const page = atlas.pages[index];
			const texturePath = `${atlasPath}${page.name}`;
			texturePaths.push(texturePath);

			const promiseTextureLoad = this.lastTexturePaths.includes(texturePath)
				? Promise.resolve(texturePath)
				: this.overlay.assetManager.loadTextureAsync(texturePath).then(texture => {
					this.lastTexturePaths.push(texturePath);
					page.setTexture(texture);
					return texturePath;
				});

			promisePageList.push(promiseTextureLoad);
		}

		// dispose textures no longer used
		for (const lastTexturePath of this.lastTexturePaths) {
			if (!texturePaths.includes(lastTexturePath)) this.overlay.assetManager.disposeAsset(lastTexturePath);
		}

		return Promise.all(promisePageList)
	}

	/**
	 * @returns The `HTMLElement` where the widget is hosted.
	 */
	public getHostElement (): HTMLElement {
		return (this.width <= 0 || this.width <= 0) && !this.getAttribute("style") && !this.getAttribute("class")
			? this.parentElement!
			: this;
	}

	/**
	 * Append the widget to the given `HTMLElement`.
	 * @param atlas the `HTMLElement` to append this widget to.
	 */
	public async appendTo (element: HTMLElement): Promise<void> {
		element.appendChild(this);
		await this.overlayAssignedPromise;
	}

	/**
	 * Calculates and sets the bounds of the current animation on track 0.
	 * Useful when animations or skins are set programmatically.
	 * @returns void
	 */
	public calculateBounds (forcedRecalculate = false): void {
		const { skeleton, state } = this;
		if (!skeleton || !state) return;

		let bounds: Rectangle;

		if (this.animationsBound && forcedRecalculate) {
			let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;

			for (const animationName of this.animationsBound) {
				const animation = this.skeleton?.data.animations.find(({ name }) => animationName === name)
				const { x, y, width, height } = this.calculateAnimationViewport(animation);

				minX = Math.min(minX, x);
				minY = Math.min(minY, y);
				maxX = Math.max(maxX, x + width);
				maxY = Math.max(maxY, y + height);
			}

			bounds = {
				x: minX,
				y: minY,
				width: maxX - minX,
				height: maxY - minY
			};
		} else {
			bounds = this.calculateAnimationViewport(state.getCurrent(0)?.animation as (Animation | undefined));
		}

		bounds.x /= skeleton.scaleX;
		bounds.y /= skeleton.scaleY;
		bounds.width /= skeleton.scaleX;
		bounds.height /= skeleton.scaleY;
		this.bounds = bounds;
	}

	private lastSkelPath = "";
	private lastAtlasPath = "";
	private lastTexturePaths: string[] = [];
	// add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
	private async loadSkeleton () {
		this.loading = true;

		const { atlasPath, skeletonPath, scale, skeletonData: skeletonDataInput, rawData } = this;
		if (!atlasPath || !skeletonPath) {
			throw new Error(`Missing atlas path or skeleton path. Assets cannot be loaded: atlas: ${atlasPath}, skeleton: ${skeletonPath}`);
		}
		const isBinary = skeletonPath.endsWith(".skel");

		if (rawData) {
			for (let [key, value] of Object.entries(rawData)) {
				this.overlay.assetManager.setRawDataURI(key, isBase64(value) ? `data:application/octet-stream;base64,${value}` : value);
			}
		}

		// this ensure there is an overlay assigned because the overlay owns the asset manager used to load assets below
		await this.overlayAssignedPromise;

		if (this.lastSkelPath && this.lastSkelPath !== skeletonPath) {
			this.overlay.assetManager.disposeAsset(this.lastSkelPath);
			this.lastSkelPath = "";
		}

		if (this.lastAtlasPath && this.lastAtlasPath !== atlasPath) {
			this.overlay.assetManager.disposeAsset(this.lastAtlasPath);
			this.lastAtlasPath = "";
		}

		// skeleton and atlas txt are loaded immeaditely
		// textures are loaeded depending on the 'pages' param:
		// - [0,2]: only pages at index 0 and 2 are loaded
		// - []: no page is loaded
		// - undefined: all pages are loaded (default)
		await Promise.all([
			this.lastSkelPath
				? Promise.resolve()
				: (isBinary ? this.overlay.assetManager.loadBinaryAsync(skeletonPath) : this.overlay.assetManager.loadJsonAsync(skeletonPath))
					.then(() => this.lastSkelPath = skeletonPath),
			this.lastAtlasPath
				? Promise.resolve()
				: this.overlay.assetManager.loadTextureAtlasButNoTexturesAsync(atlasPath).then(() => {
					this.lastAtlasPath = atlasPath;
					return this.loadTexturesInPagesAttribute();
				}),
		]);

		const atlas = this.overlay.assetManager.require(atlasPath) as TextureAtlas;
		this.pma = atlas.pages[0]?.pma

		const atlasLoader = new AtlasAttachmentLoader(atlas);

		const skeletonLoader = isBinary ? new SkeletonBinary(atlasLoader) : new SkeletonJson(atlasLoader);
		skeletonLoader.scale = scale;

		// biome-ignore lint/suspicious/noExplicitAny: it is any untile we have a json schema
		const skeletonFileAsset = this.overlay.assetManager.require(skeletonPath) as Record<string, any>;
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
		this.dprScale = this.overlay.getDevicePixelRatio();
		// skeleton.scaleX = this.dprScale;
		// skeleton.scaleY = this.dprScale;

		this.loading = false;

		// the bounds are calculated the first time, if no custom bound is provided
		this.initWidget(this.bounds.width <= 0 || this.bounds.height <= 0);

		return this;
	}

	private initWidget (forceRecalculate = false) {
		if (this.loading) return;

		const { skeleton, state, animation, animations: animationsInfo, skin, defaultMix } = this;

		if (skin) {
			if (skin.length === 1) {
				skeleton?.setSkin(skin[0]);
			} else {
				const customSkin = new Skin("custom");
				for (const s of skin) customSkin.addSkin(skeleton?.data.findSkin(s) as Skin);
				skeleton?.setSkin(customSkin);
			}

			skeleton?.setupPoseSlots();
		}

		if (state) {
			state.data.defaultMix = defaultMix;

			if (animationsInfo) {
				for (const [trackIndexString, { cycle, animations, repeatDelay }] of Object.entries(animationsInfo)) {
					const cycleFn = () => {
						const trackIndex = Number(trackIndexString);
						for (const [index, { animationName, delay, loop, mixDuration }] of animations.entries()) {
							let track;
							if (index === 0) {
								if (animationName === "#EMPTY#") {
									track = state.setEmptyAnimation(trackIndex, mixDuration);
								} else {
									track = state.setAnimation(trackIndex, animationName, loop);
								}
							} else {
								if (animationName === "#EMPTY#") {
									track = state.addEmptyAnimation(trackIndex, mixDuration, delay);
								} else {
									track = state.addAnimation(trackIndex, animationName, loop, delay);
								}
							}

							if (mixDuration) track.mixDuration = mixDuration;

							if (cycle && index === animations.length - 1) {
								track.listener = {
									complete: () => {
										if (repeatDelay)
											setTimeout(() => cycleFn(), 1000 * repeatDelay);
										else
											cycleFn();
										delete track.listener?.complete;
									}
								};
							};
						}
					}

					cycleFn();
				}
			} else if (animation) {
				state.setAnimation(0, animation, true);
			} else {
				state.setEmptyAnimation(0);
			}
		}

		if (forceRecalculate || this.autoCalculateBounds) this.calculateBounds(forceRecalculate);
	}

	private render (): void {
		let noSize = (!this.getAttribute("style") && !this.getAttribute("class"));
		this.root.innerHTML = `
        <style>
            :host {
                position: relative;
                display: inline-block;
				${noSize ? "width: 0; height: 0;" : ""}
            }
        </style>
        `;
	}

	/*
	* Interaction utilities
	*/

	/**
	 * @internal
	 */
	public pointerInsideBounds = false;

	private verticesTemp = Utils.newFloatArray(2 * 1024);

	/**
	 * @internal
	 */
	public pointerSlotEventCallbacks: Map<Slot, {
		slotFunction: (slot: Slot, event: PointerEventType, originalEvent?: UIEvent) => void,
		inside: boolean,
	}> = new Map();

	/**
	 * @internal
	 */
	public pointerEventUpdate (type: PointerEventTypesInput, originalEvent?: UIEvent) {
		if (!this.interactive) return;

		this.checkBoundsInteraction(type, originalEvent);
		this.checkSlotInteraction(type, originalEvent);
	}

	private checkBoundsInteraction (type: PointerEventTypesInput, originalEvent?: UIEvent) {
		if (this.isPointerInsideBounds()) {

			if (!this.pointerInsideBounds) {
				this.pointerEventCallback("enter", originalEvent);
			}
			this.pointerInsideBounds = true;

			this.pointerEventCallback(type, originalEvent);

		} else {

			if (this.pointerInsideBounds) {
				this.pointerEventCallback("leave", originalEvent);
			}
			this.pointerInsideBounds = false;

		}
	}

	/**
	 * @internal
	 */
	public isPointerInsideBounds (): boolean {
		if (this.isOffScreenAndWasMoved() || !this.skeleton) return false;

		const x = this.pointerWorldX / this.skeleton.scaleX;
		const y = this.pointerWorldY / this.skeleton.scaleY;

		return (
			x >= this.bounds.x &&
			x <= this.bounds.x + this.bounds.width &&
			y >= this.bounds.y &&
			y <= this.bounds.y + this.bounds.height
		);
	}

	private checkSlotInteraction (type: PointerEventTypesInput, originalEvent?: UIEvent) {
		for (let [slot, interactionState] of this.pointerSlotEventCallbacks) {
			if (!slot.bone.active) continue;
			let attachment = slot.applied.attachment;

			if (!(attachment instanceof RegionAttachment || attachment instanceof MeshAttachment)) continue;

			const { slotFunction, inside } = interactionState

			let vertices = this.verticesTemp;
			let hullLength = 8;

			// we could probably cache the vertices from rendering if interaction with this slot is enabled
			if (attachment instanceof RegionAttachment) {
				let regionAttachment = <RegionAttachment>attachment;
				regionAttachment.computeWorldVertices(slot, vertices, 0, 2);
			} else if (attachment instanceof MeshAttachment) {
				let mesh = <MeshAttachment>attachment;
				mesh.computeWorldVertices(this.skeleton!, slot, 0, mesh.worldVerticesLength, vertices, 0, 2);
				hullLength = mesh.hullLength;
			}

			// here we have only "move" and "drag" events
			if (this.isPointInPolygon(vertices, hullLength, [this.pointerWorldX, this.pointerWorldY])) {

				if (!inside) {
					interactionState.inside = true;
					slotFunction(slot, "enter", originalEvent);
				}

				if (type === "down" || type === "up") {
					if (interactionState.inside) {
						slotFunction(slot, type, originalEvent);
					}
					continue;
				}

				slotFunction(slot, type, originalEvent);

			} else {

				if (inside) {
					interactionState.inside = false;
					slotFunction(slot, "leave", originalEvent);
				}

			}
		}
	}

	private isPointInPolygon (vertices: NumberArrayLike, hullLength: number, point: number[]) {
		const [px, py] = point;

		if (hullLength < 6) {
			throw new Error("A polygon must have at least 3 vertices (6 numbers in the array). ");
		}

		let isInside = false;

		for (let i = 0, j = hullLength - 2; i < hullLength; i += 2) {
			const xi = vertices[i], yi = vertices[i + 1];
			const xj = vertices[j], yj = vertices[j + 1];

			const intersects = ((yi > py) !== (yj > py)) &&
				(px < ((xj - xi) * (py - yi)) / (yj - yi) + xi);

			if (intersects) isInside = !isInside;

			j = i;
		}

		return isInside;
	}

	/*
	* Other utilities
	*/

	public boneFollowerList: Array<{ slot: Slot, bone: Bone, element: HTMLElement, followVisibility: boolean, followRotation: boolean, followOpacity: boolean, followScale: boolean, hideAttachment: boolean }> = [];
	public followSlot (slotName: string | Slot, element: HTMLElement, options: { followVisibility?: boolean, followRotation?: boolean, followOpacity?: boolean, followScale?: boolean, hideAttachment?: boolean } = {}) {
		const {
			followVisibility = false,
			followRotation = true,
			followOpacity = true,
			followScale = true,
			hideAttachment = false,
		} = options;

		const slot = typeof slotName === 'string' ? this.skeleton?.findSlot(slotName) : slotName;
		if (!slot) return;

		if (hideAttachment) {
			slot.applied.setAttachment(null);
		}

		element.style.position = 'absolute';
		element.style.top = '0px';
		element.style.left = '0px';
		element.style.display = 'none';

		this.boneFollowerList.push({ slot, bone: slot.bone, element, followVisibility, followRotation, followOpacity, followScale, hideAttachment });
		this.overlay.addSlotFollowerElement(element);
	}
	public unfollowSlot (element: HTMLElement): HTMLElement | undefined {
		const index = this.boneFollowerList.findIndex(e => e.element === element);
		if (index > -1) {
			return this.boneFollowerList.splice(index, 1)[0].element;
		}
	}

	public isOffScreenAndWasMoved (): boolean {
		return !this.onScreen && this.dragX === 0 && this.dragY === 0;
	}

	private calculateAnimationViewport (animation?: Animation): Rectangle {
		const renderer = this.overlay.renderer;
		const { skeleton } = this;
		if (!skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		skeleton.setupPose();

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
			animation.apply(skeleton, time, time, false, [], 1, MixBlend.setup, MixDirection.in, false);
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

		skeleton.setupPose();

		return {
			x: minX,
			y: minY,
			width: maxX - minX,
			height: maxY - minY,
		}
	}

	private disposeGLResources () {
		const { assetManager } = this.overlay;
		if (this.lastAtlasPath) assetManager.disposeAsset(this.lastAtlasPath);
		if (this.lastSkelPath) assetManager.disposeAsset(this.lastSkelPath);
	}

}

customElements.define("spine-skeleton", SpineWebComponentSkeleton);

/**
 * Return the first {@link SpineWebComponentSkeleton} with the given {@link SpineWebComponentSkeleton.identifier}
 * @param identifier The {@link SpineWebComponentSkeleton.identifier} to search on the DOM
 * @returns A skeleton web component instance with the given identifier
 */
export function getSkeleton (identifier: string): SpineWebComponentSkeleton {
	return document.querySelector(`spine-skeleton[identifier=${identifier}]`) as SpineWebComponentSkeleton;
}

/**
 * Create a {@link SpineWebComponentSkeleton} with the given {@link WidgetAttributes}.
 * @param parameters The options to pass to the {@link SpineWebComponentSkeleton}
 * @returns The skeleton web component instance created
 */
export function createSkeleton (parameters: WidgetAttributes): SpineWebComponentSkeleton {
	const widget = document.createElement("spine-skeleton") as SpineWebComponentSkeleton;

	Object.entries(SpineWebComponentSkeleton.attributesDescription).forEach(entry => {
		const [key, { propertyName }] = entry;
		const value = parameters[propertyName];
		if (value) widget.setAttribute(key, value as any);
	});

	return widget;
}
