
/** Represents the SVG Picture object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/svg-picture | ISVGPictureInstance documentation } */
declare class ISVGPictureInstance extends IWorldInstance
{
	svgUrl: string;

	setSvgUrl(url: string): Promise<void>;
}
