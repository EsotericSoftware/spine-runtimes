
/** A class used for rendering text in the renderer.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/graphics-interfaces/irenderertext | IRendererText documentation } */
declare class IRendererText
{
	release(): void;

    fontFace: string;
    sizePt: number;
    lineHeight: number;
    isBold: boolean;
    isItalic: boolean;

    setColor(color: Vec3Arr): void;
    setColorRgb(r: number, g: number, b: number): void;
    setCssColor(cssColor: string): void;

    horizontalAlign: TextAlignHorizontalMode;
    verticalAlign: TextAlignVerticalMode;
    wordWrapMode: TextWordWrapMode;
    textDirection: TextDirectionMode;

    text: string;
    setSize(cssWidth: number, cssHeight: number, zoomScale: number): void;

    getTexture(): ITexture | null;
    getTexRect(): DOMRect;
    setTextureUpdateCallback(cb: () => void): void;
    releaseTexture(): void;

    readonly textWidth: number;
    readonly textHeight: number;
}
