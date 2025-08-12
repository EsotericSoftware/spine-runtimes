
/** Represents the Tilemap object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/tilemap | ITilemapInstance documentation } */
declare class ITilemapInstance extends IWorldInstance
{
	readonly mapWidth: number;
	readonly mapHeight: number;
	getMapSize(): Vec2Arr;

	readonly mapDisplayWidth: number;
	readonly mapDisplayHeight: number;
	getMapDisplaySize(): Vec2Arr;

	readonly tileWidth: number;
	readonly tileHeight: number;
	getTileSize(): Vec2Arr;

	getTileAt(x: number, y: number): number;
	setTileAt(x: number, y: number, tile: number): void;

	replaceImage(blob: Blob): Promise<void>;

	// TODO: TypeScript doesn't appear to recognize this syntax
	static TILE_FLIPPED_HORIZONTAL: number;
	static TILE_FLIPPED_VERTICAL: number;
	static TILE_FLIPPED_DIAGONAL: number;
	static TILE_FLAGS_MASK: number;
	static TILE_ID_MASK: number;
}
