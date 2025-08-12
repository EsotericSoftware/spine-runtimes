
interface ConstructLayerDrawEvent extends ConstructDrawEvent {
    layer: IAnyProjectLayer;
}

interface LayerEventMap {
	"beforedraw": ConstructLayerDrawEvent,
    "afterdraw": ConstructLayerDrawEvent
}

/** Represents a layer on a layout in the project.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/layout-interfaces/ilayout/ilayer | ILayer documentation } */
declare class ILayer extends ConstructEventTarget<LayerEventMap>
{
    readonly runtime: IRuntime;
	readonly name: string;
    readonly layout: IAnyProjectLayout;
    readonly index: number;
    
    isVisible: boolean;
    readonly isSelfAndParentsVisible: boolean;
    isInteractive: boolean;
    isHTMLElementsLayer: boolean;
    readonly isSelfAndParentsInteractive: boolean;
    isTransparent: boolean;
    isForceOwnTexture: boolean;

    opacity: number;
    scale: number;
    scaleRate: number;
    angle: number;
    parallaxX: number;
    parallaxY: number;
    zElevation: number;
    blendMode: BlendModeParameter;
    backgroundColor: Vec3Arr;
    renderingMode: "2d" | "3d";

    scrollX: number;
    scrollY: number;
    scrollTo(x: number, y: number): void;
    getScrollPosition(): Vec2Arr;
    restoreScrollPosition(): void;
    getViewport(): DOMRect;

    /** Translate a position in CSS pixels to layer co-ordinates on this layer. */
    cssPxToLayer(clientX: number, clientY: number, z?: number): Vec2Arr;

    /** Translate a position in layer co-ordinates on this layer to CSS pixels. */
    layerToCssPx(layerX: number, layerY: number, z?: number): Vec2Arr;

    /** Translate a position on the draw surface to layer co-ordinates on this layer. */
    drawSurfaceToLayer(clientX: number, clientY: number, z?: number): Vec2Arr;

    /** Translate a position in layer co-ordinates on this layer to the draw surface. */
    layerToDrawSurface(layerX: number, layerY: number, z?: number): Vec2Arr;

    readonly renderScale: number;

    readonly parentLayer: IAnyProjectLayer | null;
    parentLayers(): Generator<IAnyProjectLayer>;
    subLayers(): Generator<IAnyProjectLayer>;
    allSubLayers(): Generator<IAnyProjectLayer>;
}
