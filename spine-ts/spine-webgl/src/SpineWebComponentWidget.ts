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
    Input,
    LoadingScreenWidget,
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

type OffScreenUpdateBehaviourType = "pause" | "update" | "pose";
function isOffScreenUpdateBehaviourType(value: string | null): value is OffScreenUpdateBehaviourType {
    return (
        value === "pause" ||
        value === "update" ||
        value === "pose"
    );
}

type ModeType = "inside" | "origin";
function isModeType(value: string | null): value is ModeType {
    return (
        value === "inside" ||
        value === "origin"
    );
}

type FitType = "fill" | "width" | "height" | "contain" | "cover" | "none" | "scaleDown";
function isFitType(value: string | null): value is FitType {
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

type AttributeTypes = "string" | "number" | "boolean" | "string-number" | "fitType" | "modeType" | "offScreenUpdateBehaviourType";

// The properties that map to widget attributes
interface WidgetAttributes {
    atlasPath?: string
    skeletonPath?: string
    scale: number
    animation?: string
    skin?: string
    fit: FitType
    mode: ModeType
    xAxis: number
    yAxis: number
    offsetX: number
    offsetY: number
    width: number
    height: number
    draggable: boolean
    debug: boolean
    identifier: string
    manualStart: boolean
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

class SpineWebComponentWidget extends HTMLElement implements WidgetAttributes, WidgetOverridableMethods, WidgetInternalProperties, Partial<WidgetPublicProperties> {

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
     * The scale when loading the skeleton data. Default: 1
     * Connected to `scale` attribute.
     */
    public scale = 1;

    /**
     * Optional: The name of the animation to be played
     * Connected to `animation` attribute.
     */
    public get animation() : string | undefined {
        return this._animation;
    }
    public set animation(value: string | undefined) {
        this._animation = value;
        this.initWidget();
    }
    private _animation?: string

    /**
     * Optional: The name of the skin to be set
     * Connected to `skin` attribute.
     */
    public get skin() : string | undefined {
        return this._skin;
    }
    public set skin(value: string | undefined) {
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
     * Specify the way the skeleton is centered within the div:
     * - `inside`: the skeleton bounds center is centered with the div container (Default)
     * - `origin`: the skeleton origin is centered with the div container regardless of the bounds.
     * Origin does not allow to specify any {@link fit} type and guarantee the skeleton to not be autoscaled.
     * Connected to `mode` attribute.
     */
    public mode: ModeType = "inside";

    /**
     * The x offset of the skeleton world origin x axis in div width units
     * Connected to `x-axis` attribute.
     */
    public xAxis = 0;

    /**
     * The y offset of the skeleton world origin x axis in div width units
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
     * Specify a fixed width for the widget. If at least one of `width` and `height` is > 0,
     * the widget will have an actual size and the div reference is the widget itself, not the div parent.
     * Connected to `width` attribute.
     */
    public get width() : number {
        return this._width;
    }
    public set width(value: number) {
        this._width = value;
        this.render();
    }
    private _width = -1

    /**
     * Specify a fixed height for the widget. If at least one of `width` and `height` is > 0,
     * the widget will have an actual size and the div reference is the widget itself, not the div parent.
     * Connected to `height` attribute.
     */
    public get height() : number {
        return this._height;
    }
    public set height(value: number) {
        this._height = value;
        this.render();
    }
    private _height = -1

    /**
     * If true, the widget is draggable
     * Connected to `draggable` attribute.
     */
    public draggable = false;

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
     * An array of indexes indicating the atlas pages indexes to be loaded.
     * If undefined, all pages are loaded. If empty (default), no page is loaded;
     * in this case the user can add later the indexes of the pages they want to load
     * and call the loadTexturesInPagesAttribute, to lazily load them.
     * Connected to `pages` attribute.
     */
    public pages?: Array<number>;

    /**
     * If `true`, the skeleton is clipped to the container div bounds.
     * Be careful on using this feature because it breaks batching!
     * Connected to `clip` attribute.
     */
    public clip = false;

    /**
     * The widget update/apply behaviour when the skeleton div container is offscreen:
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
	public beforeUpdateWorldTransforms: BeforeAfterUpdateSpineWidgetFunction = () => {};

    /**
     * This callback is invoked after the world transforms are computed allows to execute additional logic.
     */
	public afterUpdateWorldTransforms: BeforeAfterUpdateSpineWidgetFunction= () => {};

    /**
     * A callback invoked each time div hosting the widget enters the screen viewport.
     * By default, the callback call the {@link start} method the first time the widget
     * enters the screen viewport.
     */
    public onScreenFunction: (widget: SpineWebComponentWidget) => void = async (widget) => {
        if (widget.loading && !widget.onScreenAtLeastOnce) {
            widget.onScreenAtLeastOnce = true;

            if (widget.manualStart) {
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
     * A rectangle representing the bounds used to fit the skeleton within the element container.
     * The rectangle coordinates and size are expressed in the Spine world space, not the screen space.
     * It is automatically calculated using the `skin` and `animation` provided by the user during loading.
     * If no skin is provided, it is used the default skin.
     * If no animation is provided, it is used the setup pose.
     * Once loaded, the bounds are not automatically recalculated, but {@link recalculateBounds} need to be invoked.
     * Use `setBounds` to set you desired bounds. Bounding Box might be useful to determine the bounds to be used.
     */
    public bounds?: Rectangle;

    /**
     * A Promise that resolve to the widget itself once assets loading is terminated.
     * Useful to safely access {@link skeleton} and {@link state} after a new widget has been just created.
     */
    public loadingPromise?: Promise<this>;

    /**
     * If true, the widget is in the assets loading process.
     */
    public loading = true;

    /**
     * A reference to the {@link LoadingScreenWidget} of this widget.
     * This is instantiated only if it is really necessary.
     * For example, if {@link loadingSpinner} is `false`, this property value is null
     */
    public loadingScreen: LoadingScreenWidget | null = null;

    /**
     * If true, the widget is in the assets loading process.
     */
    public started = false;

    /**
     * True, when the div hosting the widget enters the screen viewport. It uses an IntersectionObserver internally.
     */
    public onScreen = false;

    /**
     * True, when the div hosting the widget enters the screen viewport at least once.
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
     * Optional: Pass a `SkeletonData`, if you want to avoid creating a new one
     */
    public skeletonData?: SkeletonData;

    // Reference to the webcomponent shadow root
    private root: ShadowRoot;

    // Reference to the overlay webcomponent
    private overlay: SpineWebComponentOverlay;

    static attributesDescription: Record<string, { propertyName: keyof WidgetAttributes, type: AttributeTypes, defaultValue?: any }> = {
        atlas: { propertyName: "atlasPath", type: "string" },
        skeleton: { propertyName: "skeletonPath", type: "string" },
        scale: { propertyName: "scale", type: "number" },
        animation: { propertyName: "animation", type: "string" },
        skin: { propertyName: "skin", type: "string" },
        width: { propertyName: "width", type: "number", defaultValue: -1 },
        height: { propertyName: "height", type: "number", defaultValue: -1 },
        draggable: { propertyName: "draggable", type: "boolean" },
        "x-axis": { propertyName: "xAxis", type: "number" },
        "y-axis": { propertyName: "yAxis", type: "number" },
        "offset-x": { propertyName: "offsetX", type: "number" },
        "offset-y": { propertyName: "offsetY", type: "number" },
        identifier: { propertyName: "identifier", type: "string" },
        debug: { propertyName: "debug", type: "boolean" },
        "manual-start": { propertyName: "manualStart", type: "boolean" },
        spinner: { propertyName: "loadingSpinner", type: "boolean" },
        clip: { propertyName: "clip", type: "boolean" },
        pages: { propertyName: "pages", type: "string-number" },
        fit: { propertyName: "fit", type: "fitType", defaultValue: "contain" },
        mode: { propertyName: "mode", type: "modeType", defaultValue: "inside" },
        offscreen: { propertyName: "offScreenUpdateBehaviour", type: "offScreenUpdateBehaviourType", defaultValue: "pause" },
    }

    static get observedAttributes(): string[] {
        return [
            "atlas",        // atlasPath
            "skeleton",     // skeletonPath
            "scale",        // scale
            "animation",    // animation
            "skin",         // skin
            "fit",          // fit
            "width",        // width
            "height",       // height
            "draggable",    // draggable
            "mode",         // mode
            "x-axis",       // xAxis
            "y-axis",       // yAxis
            "offset-x",     // offsetX
            "offset-y",     // offsetY
            "identifier",   // identifier
            "debug",        // debug
            "manual-start", // manualStart
            "spinner",      // loadingSpinner
            "pages",        // pages
            "offscreen",    // offScreenUpdateBehaviour
            "clip",         // clip
        ];
    }

    constructor() {
        super();
        this.root = this.attachShadow({ mode: "closed" });
        this.overlay = this.initializeOverlay();

        this.debugDragDiv = document.createElement("div");
        this.debugDragDiv.style.position = "absolute";
        this.debugDragDiv.style.backgroundColor = "rgba(0, 1, 1, 0.3)";
        this.debugDragDiv.style.setProperty("pointer-events", "none");
    }

    connectedCallback() {
        this.overlay.addWidget(this);
        if (!this.manualStart) {
            this.start();
        }
        this.render();
    }

    disconnectedCallback(): void {
        this.loadingPromise?.then(() => {
            const index = this.overlay.skeletonList.indexOf(this);
            if (index !== -1) {
                this.overlay.skeletonList.splice(index, 1);
            }
        });
    }

    private static castBoolean(value: string | null, defaultValue = "") {
        return value === "true" || value === "" ? true : false;
    }

    private static castString(value: string | null, defaultValue = "") {
        return value === null ? defaultValue : value;
    }

    private static castNumber(value: string | null, defaultValue = 0) {
        if (value === null) return defaultValue;

        const parsed = parseFloat(value);
        if (Number.isNaN(parsed)) return defaultValue;
        return parsed;
    }

    private static castArrayNumber(value: string | null, defaultValue = undefined) {
        if (value === null) return defaultValue;
        return value.split(",").reduce((acc, pageIndex) => {
            const index = parseInt(pageIndex);
            if (!isNaN(index)) acc.push(index);
            return acc;
        }, [] as Array<number>);
    }

    private static castValue(type: AttributeTypes, value: string | null, defaultValue?: any) {
        switch (type) {
            case "string":
                return SpineWebComponentWidget.castString(value, defaultValue);
            case "number":
                return SpineWebComponentWidget.castNumber(value, defaultValue);
            case "boolean":
                return SpineWebComponentWidget.castBoolean(value, defaultValue);
            case "string-number":
                return SpineWebComponentWidget.castArrayNumber(value, defaultValue);
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

    attributeChangedCallback(name: string, oldValue: string | null, newValue: string | null): void {
        const { type, propertyName, defaultValue } = SpineWebComponentWidget.attributesDescription[name];
        const val = SpineWebComponentWidget.castValue(type, newValue, defaultValue ?? this[propertyName]);
        (this as any)[propertyName] = val;
        return;
    }

    /**
     * Recalculates and sets the bounds of the current animation on track 0.
     * Useful when animations or skins are set programmatically.
     * @returns void
     */
	public recalculateBounds(): void {
        const { skeleton, state } = this;
		if (!skeleton || !state) return;
		const track = state.getCurrent(0);
		const animation = track?.animation as (Animation | undefined);
		const bounds = this.calculateAnimationViewport(animation);
		this.setBounds(bounds);
	}

    /**
     * Set the given bounds on the current skeleton.
     * Useful when you want you skeleton to have a fixed size, or you want to
     * focus a certain detail of the skeleton. If the skeleton overflow the element container
     * consider setting {@link clip} to `true`.
     * @param bounds
     * @returns
     */
	public setBounds(bounds: Rectangle): void {
        const { skeleton } = this;
		if (!skeleton) return;
		bounds.x /= skeleton.scaleX;
		bounds.y /= skeleton.scaleY;
		bounds.width /= skeleton.scaleX;
		bounds.height /= skeleton.scaleY;
        this.bounds = bounds;
	}

    /**
     * Starts the widget. Starting the widget means to load the assets currently set into
     * {@link atlasPath} and {@link skeletonPath}.
     */
    public start() {
        if (this.started) {
            console.warn("If you want to start again the widget, first reset it");
        }
        this.started = true;

        this.loadingPromise = this.loadSkeleton();
        this.loadingPromise.then(() => {
            this.loading = false;
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
    public async loadTexturesInPagesAttribute(atlas: TextureAtlas): Promise<Array<any>> {
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
    public getHTMLElementReference(): HTMLElement {
        return this.width <= 0 || this.width <= 0
            ? this.parentElement!
            : this;
    }

    // add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
    private async loadSkeleton() {
        this.loading = true;
        // if (this.identifier !== "TODELETE") return Promise.reject();
		const { atlasPath, skeletonPath, scale = 1, animation, skeletonData: skeletonDataInput, skin } = this;
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

		const skeletonFile = this.overlay.assetManager.require(skeletonPath);
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

		this.initWidget();

		return this;
	}

    private initWidget() {
        const { skeleton, state, animation, skin } = this;

        if (skin) skeleton?.setSkinByName(skin);
		if (animation) state?.setAnimation(0, animation, true);

		this.recalculateBounds();
    }

    private render(): void {
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
                // background-color: red;
            }
        </style>
        `;
    }

    // Create a new overlay webcomponent, if no one exists yet.
    // TODO: allow the possibility to instantiate multiple overlay (eg: background, foreground),
    // to give them an identifier, and to specify which overlay is assigned to a widget
    private initializeOverlay(): SpineWebComponentOverlay {
        let overlay = document.querySelector("spine-overlay") as SpineWebComponentOverlay;
        if (!overlay) {
            overlay = document.createElement("spine-overlay") as SpineWebComponentOverlay;
            document.body.appendChild(overlay);
        }
        return overlay;
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

            if (!isNaN(offset.x) && !isNaN(offset.y) && !isNaN(size.x) && !isNaN(size.y)) {
                minX = Math.min(offset.x, minX);
                maxX = Math.max(offset.x + size.x, maxX);
                minY = Math.min(offset.y, minY);
                maxY = Math.max(offset.y + size.y, maxY);
            } else
                console.error("Animation bounds are invalid: " + animation.name);
        }

        return {
            x: minX,
            y: minY,
            width: maxX - minX,
            height: maxY - minY,
        }
    }
}

class SpineWebComponentOverlay extends HTMLElement {
    private root: ShadowRoot;

    private div: HTMLDivElement;
    private canvas:HTMLCanvasElement;
    private fps: HTMLSpanElement;

    public skeletonList = new Array<SpineWebComponentWidget>();

    private intersectionObserver? : IntersectionObserver;
    private resizeObserver:ResizeObserver;
    private input: Input;

	// how many pixels to add to the edges to prevent "edge cuttin" on fast scrolling
    // be aware that the canvas is already big as the display size
    // making it bigger might reduce performance significantly
	private overflowTop = .2;
	private overflowBottom = .0;
	private overflowLeft = .0;
	private overflowRight = .0;
	private overflowLeftSize: number
	private overflowTopSize: number;

    private currentCanvasBaseWidth = 0;
    private currentCanvasBaseHeight = 0;

    public renderer: SceneRenderer;
    public assetManager: AssetManager;
    private disposed = false;
    readonly time = new TimeKeeper();

    constructor() {
        super();
        this.root = this.attachShadow({ mode: "closed" });

        this.div = document.createElement("div");
		this.div.style.position = "absolute";
		this.div.style.top = "0";
		this.div.style.left = "0";
		this.div.style.setProperty("pointer-events", "none");
		this.div.style.overflow = "hidden"
		// this.div.style.backgroundColor = "rgba(0, 255, 0, 0.3)";

        this.root.appendChild(this.div);

		this.canvas = document.createElement("canvas");
		this.div.appendChild(this.canvas);
		this.canvas.style.position = "absolute";
		this.canvas.style.top = "0";
		this.canvas.style.left = "0";

		this.canvas.style.setProperty("pointer-events", "none");
		this.canvas.style.transform =`translate(0px,0px)`;
		// this.canvas.style.setProperty("will-change", "transform"); // performance seems to be even worse with this uncommented

        this.fps = document.createElement("span");
        this.fps.style.position = "fixed";
		this.fps.style.top = "0";
		this.fps.style.left = "0";
        this.root.appendChild(this.fps);

        const context = new ManagedWebGLRenderingContext(this.canvas, { alpha: true });
		this.renderer = new SceneRenderer(this.canvas, context);
		this.assetManager = new AssetManager(context);
		this.input = new Input(this.canvas);
        this.setupRenderingElements();

        this.updateCanvasSize();
        this.zoomHandler();

        // translateCanvas starts a requestAnimationFrame loop
        this.translateCanvas();

		this.overflowLeftSize = this.overflowLeft * document.documentElement.clientWidth;
		this.overflowTopSize = this.overflowTop * document.documentElement.clientHeight;

        // resize and zoom
		// TODO: should I use the resize event?
		this.resizeObserver = new ResizeObserver(() => {
            this.updateCanvasSize();
			this.zoomHandler();
        });
        this.resizeObserver.observe(document.body);

        const screen = window.screen;
        screen.orientation.onchange = () => {
            this.updateCanvasSize();
            // after an orientation change the scrolling changes, but the scroll event does not fire
            this.scrollHandler();
        }

		window.addEventListener("scroll", this.scrollHandler);
		this.scrollHandler();

        this.input = new Input(document.body, false);
		this.setupDragUtility();
    }

    addWidget(widget: SpineWebComponentWidget) {
        this.skeletonList.push(widget);
        this.intersectionObserver!.observe(widget.getHTMLElementReference());
    }

    private setupRenderingElements() {
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

                    if (onScreen || (!onScreen && offScreenUpdateBehaviour === "pose") ) {
                        state.apply(skeleton);
                        beforeUpdateWorldTransforms(skeleton, state);
                        skeleton.updateWorldTransform(Physics.update);
                        afterUpdateWorldTransforms(skeleton, state);
                    }
                }
            });
            this.fps.innerText = this.time.framesPerSecond.toFixed(2) + " fps";
        };

        const clear = (r: number, g: number, b: number, a: number) => {
            this.renderer.context.gl.clearColor(r, g, b, a);
            this.renderer.context.gl.clear(this.renderer.context.gl.COLOR_BUFFER_BIT);
        }

        const clipToBoundStart = (divBounds: Rectangle) => {
            // break current batch and start a new one
            this.renderer.end();

            // set the new viewport to the div bound
            const viewportWidth = divBounds.width * window.devicePixelRatio;
            const viewporthHeight = divBounds.height * window.devicePixelRatio;
            this.renderer.context.gl.viewport(
                divBounds.x * window.devicePixelRatio,
                this.canvas.height - (divBounds.y + divBounds.height) * window.devicePixelRatio,
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

        const clipToBoundEnd = () =>  {
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

            const devicePixelRatio = window.devicePixelRatio;
            const tempVector = new Vector3();
            this.skeletonList.forEach((widget) => {
                const { skeleton, bounds, mode, debug, offsetX, offsetY, xAxis, yAxis, dragX, dragY, fit, loadingSpinner, onScreen, loading, clip, draggable } = widget;

                if ((!onScreen && dragX === 0 && dragY === 0)) return;
                const divBounds = widget.getHTMLElementReference().getBoundingClientRect();
                divBounds.x += this.overflowLeftSize;
                divBounds.y += this.overflowTopSize;

                // TODO: drag does not work while clip is on

                let divOriginX = 0;
                let divOriginY = 0;
                if (clip) {
                    // in clip mode, the world origin is the div center (divBounds center)
                    clipToBoundStart(divBounds);
                } else {
                    // get the desired point into the the div (center by default) in world coordinate
                    const divX = divBounds.x + divBounds.width * (xAxis + .5);
                    const divY = divBounds.y + divBounds.height * (-yAxis + .5);
                    this.screenToWorld(tempVector, divX, divY);
                    divOriginX = tempVector.x;
                    divOriginY = tempVector.y;
                }

                const divWidthWorld = divBounds.width * devicePixelRatio;
                const divHeightWorld = divBounds.height * devicePixelRatio;

                if (loading) {
                    if (loadingSpinner) {
                        if (!widget.loadingScreen) widget.loadingScreen = new LoadingScreenWidget(renderer);
                        widget.loadingScreen!.draw(true, divOriginX, divOriginY, divWidthWorld, divHeightWorld);
                    }
                    if (clip) clipToBoundEnd();
                    return;
                }

                if (skeleton) {
                    if (mode === "inside") {
                        let { x: ax, y: ay, width: aw, height: ah } = bounds!;

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
                            if (ah * scaleWidth > divHeightWorld){
                                ratioW = scaleHeight;
                                ratioH = scaleHeight;
                            } else {
                                ratioW = scaleWidth;
                                ratioH = scaleWidth;
                            }
                        } else if (fit === "cover") {
                            if (ah * scaleWidth < divHeightWorld){
                                ratioW = scaleHeight;
                                ratioH = scaleHeight;
                            } else {
                                ratioW = scaleWidth;
                                ratioH = scaleWidth;
                            }
                        } else if (fit === "scaleDown") {
                            if (aw > divWidthWorld || ah > divHeightWorld) {
                                if (ah * scaleWidth > divHeightWorld){
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

                    renderer.drawSkeleton(skeleton, true, -1, -1, (vertices, size, vertexSize) => {
                        for (let i = 0; i < size; i+=vertexSize) {
                            vertices[i] = vertices[i] + worldOffsetX;
                            vertices[i+1] = vertices[i+1] + worldOffsetY;
                        }
                    });

                    // store the draggable surface to make darg logic easier
                    if (draggable) {
                        let { x: ax, y: ay, width: aw, height: ah } = bounds!;
                        this.worldToScreen(tempVector, ax * skeleton.scaleX + worldOffsetX, ay * skeleton.scaleY + worldOffsetY);
                        widget.dragBoundsRectangle.x = tempVector.x + window.scrollX;
                        widget.dragBoundsRectangle.y = tempVector.y - ah * skeleton.scaleY / window.devicePixelRatio + window.scrollY;
                        widget.dragBoundsRectangle.width = aw * skeleton.scaleX / window.devicePixelRatio;
                        widget.dragBoundsRectangle.height = ah * skeleton.scaleY / window.devicePixelRatio;

                        if (clip) {
                            widget.dragBoundsRectangle.x += divBounds.x;
                            widget.dragBoundsRectangle.y += divBounds.y;
                        }
                    }

                    // drawing debug stuff
                    if (debug) {
                    // if (true) {
                        let { x: ax, y: ay, width: aw, height: ah } = bounds!;

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

                        if (!widget.debugDragDiv.isConnected) document.body.appendChild(widget.debugDragDiv);
                        widget.debugDragDiv.style.left = `${widget.dragBoundsRectangle.x - this.overflowLeftSize}px`;
                        widget.debugDragDiv.style.top = `${widget.dragBoundsRectangle.y - this.overflowTopSize}px`;
                        widget.debugDragDiv.style.width = `${widget.dragBoundsRectangle.width}px`;
                        widget.debugDragDiv.style.height = `${widget.dragBoundsRectangle.height}px`;
                    }

                    if (clip) clipToBoundEnd();

                }
            });

            renderer.end();
        }

        const loop = () => {
			if (this.disposed) return;
			requestAnimationFrame(loop);
			this.time.update();
			updateWidgets();
			renderWidgets();
		}

		requestAnimationFrame(loop);

		const red = new Color(1, 0, 0, 1);
		const green = new Color(0, 1, 0, 1);
		const blue = new Color(0, 0, 1, 1);
		const transparentWhite = new Color(1, 1, 1, .3);
	}

    connectedCallback(): void {
        this.intersectionObserver = new IntersectionObserver((widgets) => {
            widgets.forEach(({ isIntersecting, target }) => {

                const widget = this.skeletonList.find(w => w.getHTMLElementReference() == target);
                if (!widget) return;
                widget.onScreen = isIntersecting;
                if (isIntersecting) {
                    widget.onScreenFunction(widget);
                }
            })
        }, { rootMargin: "30px 20px 30px 20px" });
    }

    disconnectedCallback(): void {
    }

    // TODO: drag is bugged when zoom on browser (just zoom and activare debug to see the drag surface has some offset)
    private setupDragUtility() {
		// TODO: we should use document - body might have some margin that offset the click events - Meanwhile I take event pageX/Y
		const point: Point = { x: 0, y: 0 };

        const getInput = (ev?: MouseEvent | TouchEvent): Point => {
            const originalEvent = ev instanceof MouseEvent ? ev : ev!.changedTouches[0];
            point.x = originalEvent.pageX + this.overflowLeftSize;
            point.y = originalEvent.pageY + this.overflowTopSize;
            return point;
        }

		let prevX = 0;
		let prevY = 0;
		this.input.addListener({
			down: (x, y, ev) => {
				const input = getInput(ev);
				this.skeletonList.forEach(widget => {
					if (!widget.draggable || (!widget.onScreen && widget.dragX === 0 && widget.dragY === 0)) return;
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
                    widget.dragX += dragX * window.devicePixelRatio;
                    widget.dragY -= dragY * window.devicePixelRatio;
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
		})
	}

    /*
	* Resize/scroll utilities
	*/

	private updateCanvasSize() {
		// resize canvas, if necessary
		this.resizeCanvas();

		// temporarely remove the div to get the page size without considering the div
		// this is necessary otherwise if the bigger element in the page is remove and the div
		// was the second bigger element, now it would be the div to determine the page size
		this.div.remove();
		const { width, height } = this.getPageSize();
		this.root.appendChild(this.div);

		this.div.style.width = width + "px";
		this.div.style.height = height + "px";
	}

    private resizeCanvas() {
        const { width, height } = this.getScreenSize();

		if (this.currentCanvasBaseWidth !== width || this.currentCanvasBaseHeight !== height) {
            this.currentCanvasBaseWidth = width;
            this.currentCanvasBaseHeight = height;
            this.overflowLeftSize = this.overflowLeft * width;
		    this.overflowTopSize = this.overflowTop * height;

            const totalWidth = width * (1 + (this.overflowLeft + this.overflowRight));
            const totalHeight = height * (1 + (this.overflowTop + this.overflowBottom));

            this.canvas.style.width = totalWidth + "px";
		    this.canvas.style.height = totalHeight + "px";
            this.renderer.resize3(totalWidth, totalHeight);
        }
	}

    // right now, we scroll the canvas each frame, that makes scrolling on mobile waaay more smoother
    // this is way scroll handler do nothing
    private scrollHandler = () => {
		// this.translateCanvas();
	}

    private translateCanvas() {
		const scrollPositionX = window.scrollX - this.overflowLeftSize;
		const scrollPositionY = window.scrollY - this.overflowTopSize;
		this.canvas.style.transform =`translate(${scrollPositionX}px,${scrollPositionY}px)`;
        requestAnimationFrame(() => this.translateCanvas());
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

            // TODO: improve this horrible thing
            this.renderer.resize3(
                +this.canvas.style.width.substring(0, this.canvas.style.width.length - 2),
                +this.canvas.style.height.substring(0, this.canvas.style.height.length - 2),
            );
		})
	}

    // we need the bounding client rect otherwise decimals won't be returned
    // this means that during zoom it might occurs that the div would be resized
    // rounded 1px more making a scrollbar appear
    private getPageSize() {
		return document.body.getBoundingClientRect();
	}

    // screen size remain the same when it is rotated
    // we need to swap them based and the orientation angle
    private getScreenSize() {
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
    private screenToWorld(vec: Vector3, x: number, y: number) {
        vec.set(x, y, 0);
        // pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
        this.renderer.camera.screenToWorld(vec, this.canvas.clientWidth, this.canvas.clientHeight);
    }
    private worldToScreen(vec: Vector3, x: number, y: number) {
        vec.set(x, -y, 0);
        // pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
        // this.renderer.camera.worldToScreen(vec, this.canvas.clientWidth, this.canvas.clientHeight);
        this.renderer.camera.worldToScreen(vec, this.renderer.camera.viewportWidth / window.devicePixelRatio, this.renderer.camera.viewportHeight / window.devicePixelRatio);
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

export function getSpineWidget(identifier: string): SpineWebComponentWidget {
    return document.querySelector(`spine-widget[identifier=${identifier}]`) as SpineWebComponentWidget;
}

export function createSpineWidget(parameters: WidgetAttributes): SpineWebComponentWidget {
    const widget = document.createElement("spine-widget") as SpineWebComponentWidget;

    Object.entries(SpineWebComponentWidget.attributesDescription).forEach(entry => {
        const [key, { propertyName }] = entry;
        const value = parameters[propertyName];
        if (value) widget.setAttribute(key, value as any);
    });

    if (!widget.manualStart) {
        widget.start();
    }

    return widget;
}