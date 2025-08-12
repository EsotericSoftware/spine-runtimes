
type DrawingCanvasGradientDirection = "horizontal" | "vertical";
type DrawingCanvasLineCap = "butt" | "square";
type DrawingCanvasColor = Vec3Arr | Vec4Arr;
type DrawingCanvasPoly = Array<Array<number>>;

interface DrawingCanvasInstanceEventMap<InstType = IDrawingCanvasInstance> extends WorldInstanceEventMap<InstType> {
	"resolutionchange": InstanceEvent<InstType>;
}

/** Represents the Drawing Canvas object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/drawing-canvas | IDrawingCanvasInstance documentation } */
declare class IDrawingCanvasInstance extends IWorldInstance
{
	addEventListener<K extends keyof DrawingCanvasInstanceEventMap<this>>(type: K, listener: (ev: DrawingCanvasInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof DrawingCanvasInstanceEventMap<this>>(type: K, listener: (ev: DrawingCanvasInstanceEventMap<this>[K]) => any): void;

	readonly pixelScale: number;
	readonly surfaceDeviceWidth: number;
	readonly surfaceDeviceHeight: number;
	getSurfaceDeviceSize(): Vec2Arr;
	setFixedResolutionMode(fixedWidth: number, fixedHeight: number): void;
	setAutoResolutionMode(): void;

	clearCanvas(color: DrawingCanvasColor): void;
	clearRect(left: number, top: number, right: number, bottom: number, color: DrawingCanvasColor): void;
	fillRect(left: number, top: number, right: number, bottom: number, color: DrawingCanvasColor): void;
	fillLinearGradient(left: number, top: number, right: number, bottom: number, color1: DrawingCanvasColor, color2: DrawingCanvasColor, dirStr?: DrawingCanvasGradientDirection): void;
	fillEllipse(x: number, y: number, radiusX: number, radiusY: number, color: DrawingCanvasColor, isSmooth?: boolean): void;
	outlineEllipse(x: number, y: number, radiusX: number, radiusY: number, color: DrawingCanvasColor, thickness: number, isSmooth?: boolean): void;

	outlineRect(left: number, top: number, right: number, bottom: number, color: DrawingCanvasColor, thickness: number): void;
	line(x1: number, y1: number, x2: number, y2: number, color: DrawingCanvasColor, thickness: number, capStr?: DrawingCanvasLineCap): void;
	lineDashed(x1: number, y1: number, x2: number, y2: number, color: DrawingCanvasColor, thickness: number, dashLength: number, capStr?: DrawingCanvasLineCap): void;
	linePoly(polyArr: DrawingCanvasPoly, color: DrawingCanvasColor, thickness: number, capStr?: DrawingCanvasLineCap): void;
	lineDashedPoly(polyArr: DrawingCanvasPoly, color: DrawingCanvasColor, thickness: number, dashLength: number, capStr?: DrawingCanvasLineCap): void;
	fillPoly(polyArr: DrawingCanvasPoly, color: DrawingCanvasColor, isConvex?: boolean): void;
	setDrawBlend(blendMode: BlendModeParameter): void;

	pasteInstances(instances: IWorldInstance[], includeFx?: boolean): Promise<void>;
	getImagePixelData(): Promise<ImageData>;
	loadImagePixelData(imageData: ImageData, premultiplyAlpha?: boolean): void;
	saveImage(format?: string, quality?: number, areaRect?: DOMRect): Promise<Blob>;
}
