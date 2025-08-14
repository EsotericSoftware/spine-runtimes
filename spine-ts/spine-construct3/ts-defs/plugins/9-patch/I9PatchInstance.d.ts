
type NinePatchEdgeType = "tile" | "stretch";
type NinePatchFillType = "tile" | "stretch" | "transparent";
type NinePatchSeamsType = "exact" | "overlap";

/** Represents the 9-patch object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/9-patch | I9PatchInstance documentation } */
declare class I9PatchInstance extends IWorldInstance
{
	leftMargin: number;
	rightMargin: number;
	topMargin: number;
	bottomMargin: number;

	edges: NinePatchEdgeType;
	fill: NinePatchFillType;
	imageScaleX: number;
	imageScaleY: number;
	seams: NinePatchSeamsType;

	replaceImage(blob: Blob): Promise<void>;
}
