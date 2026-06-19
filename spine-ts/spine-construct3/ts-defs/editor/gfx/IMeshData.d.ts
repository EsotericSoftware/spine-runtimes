
declare namespace SDK.Gfx {
	
	/** Represents mesh data created by the renderer. */
	class IMeshData
	{
		readonly vertexCount: number;
		readonly indexCount: number;

		positions: Float32Array;
		texCoords: Float32Array;
		colors: Float32Array;
		indices: Uint16Array | Uint32Array;
		
		readonly debugLabel: string;

		MarkDataChanged(bufferType: string, start: number, end: number): void;
		MarkAllVertexDataChanged(start?: number, end?: number): void;
		MarkIndexDataChanged(start?: number, end?: number): void;
		FillColor(r: number, g: number, b: number, a: number): void;

		Release(): void;
	}
}