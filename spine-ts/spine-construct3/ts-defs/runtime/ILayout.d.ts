
interface LayoutEvent extends ConstructEvent {
	layout: ILayout;
}

interface LayoutEventMap {
	"beforelayoutstart": LayoutEvent;
	"afterlayoutstart": LayoutEvent;
    "beforelayoutend": LayoutEvent;
    "afterlayoutend": LayoutEvent;
}

type LayerPositionWhere = "above" | "below" | "top-sublayer" | "bottom-sublayer";
type LayoutProjection = "perspective" | "orthographic";

/** Represents a layout in the project.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/layout-interfaces/ilayout | ILayout documentation } */
declare class ILayout extends ConstructEventTarget<LayoutEventMap>
{
    readonly runtime: IRuntime;
    readonly name: string;
    readonly index: number;

    width: number;
    height: number;
    setSize(w: number, h: number): void;
    getSize(): Vec2Arr;

    scale: number;
    angle: number;
    scrollX: number;
    scrollY: number;
    scrollTo(x: number, y: number): void;
    getScrollPosition(): Vec2Arr;
    readonly isUnboundedScrolling: boolean;

    getLayer(nameOrIndex: LayerParameter): IAnyProjectLayer | null;
    getAllLayers(): IAnyProjectLayer[];
    allLayers(): Generator<IAnyProjectLayer>;

    addLayer(layerName: string, insertBy: ILayer | null, where: LayerPositionWhere): void;
    moveLayer(layer: ILayer, insertBy: ILayer | null, where: LayerPositionWhere): void;
    removeLayer(layer: ILayer): void;
    removeAllDynamicLayers(): void;

    setVanishingPoint(vpX: number, vpY: number): void;
    getVanishingPoint(): Vec2Arr;

    projection: LayoutProjection;
}
