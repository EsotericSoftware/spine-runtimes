
/** Renderer class for drawing at runtime.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/graphics-interfaces/irenderer-interface | IRenderer documentation } */

type TextureWrapMode = "clamp-to-edge" | "repeat" | "mirror-repeat";
type TextureSamplingMode = "nearest" | "bilinear" | "trilinear";
interface TextureCreateOptions {
	wrapX?: TextureWrapMode,
    wrapY?: TextureWrapMode,
    sampling?: TextureSamplingMode,
    mipMap?: boolean
}
interface TextureUpdateOptions {
    premultiplyAlpha?: boolean;
}
type TextureStaticDataType = HTMLImageElement | HTMLCanvasElement | OffscreenCanvas | ImageBitmap;
type TextureUpdateDataType = HTMLImageElement | HTMLVideoElement | HTMLCanvasElement | ImageBitmap | OffscreenCanvas | ImageData;

type RendererLineCapMode = "butt" | "square";
type RendererCullFaceMode = "none" | "back" | "front";
type RendererFrontFaceWinding = "cw" | "ccw";

declare class IRenderer
{
	setAlphaBlendMode(): void;
    setBlendMode(blendMode: BlendModeParameter): void;

    setColorFillMode(): void;
    setTextureFillMode(): void;
    setSmoothLineFillMode(): void;

    setColor(color: Vec4Arr): void;
    setColorRgba(r: number, g: number, b: number, a: number): void;
    setOpacity(o: number): void;
    resetColor(): void;

    setCurrentZ(z: number): void;
    getCurrentZ(): void;

    setCullFaceMode(m: RendererCullFaceMode): void;
    getCullFaceMode(): RendererCullFaceMode;
    setFrontFaceWinding(m: RendererFrontFaceWinding): void;
    getFrontFaceWinding(): RendererFrontFaceWinding;

    rect(r: DOMRect): void;
    rect2(l: number, t: number, r: number, b: number): void;

    quad(quad: DOMQuad): void;
    quad2(tlx: number, tly: number, trx: number, try_: number, brx: number, bry: number, blx: number, bly: number): void;
    quad3(quad: DOMQuad, rect: DOMRect): void;
    quad4(quad: DOMQuad, texQuad: DOMQuad): void;
    quad5(quad: DOMQuad, texQuad: DOMQuad, colorArr: Float32Array): void;
    quad3D(tlx: number, tly: number, tlz: number, trx: number, try_: number, trz: number, brx: number, bry: number, brz: number, blx: number, bly: number, blz: number, rect: DOMRect): void;
    quad3D2(tlx: number, tly: number, tlz: number, trx: number, try_: number, trz: number, brx: number, bry: number, brz: number, blx: number, bly: number, blz: number, texQuad: DOMQuad): void;
    quad3D3(tlx: number, tly: number, tlz: number, trx: number, try_: number, trz: number, brx: number, bry: number, brz: number, blx: number, bly: number, blz: number, texQuad: DOMQuad, colorArr: Float32Array): void;

    drawMesh(posArr: Float32Array, uvArr: Float32Array, indexArr: Uint16Array, colorArr?: Float32Array): void;

    convexPoly(pointsArray: number[]): void;
    line(x1: number, y1: number, x2: number, y2: number): void;
    texturedLine(x1: number, y1: number, x2: number, y2: number, u: number, v: number): void;
    lineRect(l: number, t: number, r: number, b: number): void;
    lineRect2(rect: DOMRect): void;
    lineQuad(quad: DOMQuad): void;

    pushLineWidth(w: number): void;
    popLineWidth(): void;

    pushLineCap(lineCap: RendererLineCapMode): void;
    popLineCap(): void;

    setTexture(texture: ITexture): void;
    createStaticTexture(data: TextureStaticDataType, opts?: TextureCreateOptions): Promise<ITexture>;
    createDynamicTexture(width: number, height: number, opts?: TextureCreateOptions): ITexture;
    updateTexture(data: TextureUpdateDataType, texture: ITexture, opts?: TextureUpdateOptions): void;
    deleteTexture(texture: ITexture): void;
    loadTextureForImageInfo(imageInfo: IImageInfo, opts?: TextureCreateOptions): Promise<ITexture>;
    releaseTextureForImageInfo(imageInfo: IImageInfo): void;
    getTextureForImageInfo(imageInfo: IImageInfo): ITexture | null;

    createRendererText(): IRendererText;

    setDeviceTransform(): void;
    setLayerTransform(layer: ILayer): void;
}
