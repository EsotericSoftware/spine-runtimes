declare namespace SDK {
	class IAnimationFrame {
		GetObjectType(): SDK.IObjectType;
		GetWidth(): number;
		GetHeight(): number;
		GetCachedWebGLTexture(): SDK.Gfx.IWebGLTexture | null;
		GetTexRect(): SDK.Rect;

		LoadWebGLTexture(): Promise<SDK.Gfx.IWebGLTexture>;
		GetBlob(): Blob;
		ReplaceBlobAndDecode(blob: Blob): Promise<void>;

		SetDuration(d: number): void;
		GetDuration(): number;
		SetOriginX(x: number): void;
		GetOriginX(): number;
		SetOriginY(y: number): void;
		GetOriginY(): number;

		AddImagePoint(name: string, x: number, y: number): SDK.IImagePoint;
		GetImagePoints(): SDK.IImagePoint[];
		GetCollisionPoly(): SDK.ICollisionPoly;

		Delete(): void;
	}
}