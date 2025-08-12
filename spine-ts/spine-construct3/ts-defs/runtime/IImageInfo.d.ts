
/** Represents an image in the project.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/iimageinfo | IImageInfo documentation } */
declare class IImageInfo
{
	readonly width: number;
    readonly height: number;
    getSize(): Vec2Arr;

    // For addon SDK
    getTexture(renderer: IRenderer): ITexture | null;
    getTexRect(): DOMRect;
}
