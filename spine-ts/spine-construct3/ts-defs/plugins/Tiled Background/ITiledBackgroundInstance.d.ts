
/** Represents the Tiled Background object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/tiled-background | ITiledBackgroundInstance documentation } */
declare class ITiledBackgroundInstance extends IWorldInstance
{
	imageOffsetX: number;
	imageOffsetY: number;
	setImageOffset(x: number, y: number): void;
	getImageOffset(): Vec2Arr;

	imageScaleX: number;
	imageScaleY: number;
	setImageScale(x: number, y: number): void;
	getImageScale(): Vec2Arr;

	imageAngle: number;
	imageAngleDegrees: number;

	readonly imageWidth: number;
	readonly imageHeight: number;
	getImageSize(): Vec2Arr;

	enableTileRandomization: number;
	tileXRandom: number;
	tileYRandom: number;
	setTileRandom(x: number, y: number): void;
	getTileRandom(): Vec2Arr;
	tileAngleRandom: number;
	tileBlendMarginX: number;
	tileBlendMarginY: number;
	setTileBlendMargin(x: number, y: number): void;
	getTileBlendMargin(): Vec2Arr;

	replaceImage(blob: Blob): Promise<void>;
}
