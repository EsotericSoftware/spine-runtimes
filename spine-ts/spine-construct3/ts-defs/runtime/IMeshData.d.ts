
/** Represents mesh data created by the renderer. */
declare class IMeshData
{
	readonly vertexCount: number;
	readonly indexCount: number;

	positions: Float32Array;
	texCoords: Float32Array;
	colors: Float32Array;
	indices: Uint16Array | Uint32Array;
	
	readonly debugLabel: string;

	markDataChanged(bufferType: string, start: number, end: number): void;
	markAllVertexDataChanged(start?: number, end?: number): void;
	markIndexDataChanged(start?: number, end?: number): void;
	fillColor(r: number, g: number, b: number, a: number): void;

	release(): void;
}
