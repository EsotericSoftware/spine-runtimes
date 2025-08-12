// Note types like TextureCreateOptions are taken from the runtime type
// definitions as they match the same types used by the editor

declare namespace SDK.Gfx {
	class IWebGLRenderer {
		SetAlphaBlend(): void;
		
		SetColorFillMode(): void;
		SetTextureFillMode(): void;
		SetSmoothLineFillMode(): void;

		SetColor(color: SDK.Color): void;
		SetColorRgba(r: number, g: number, b: number, a: number): void;
		SetOpacity(opacity: number): void;
		ResetColor(): void;

		SetCurrentZ(z: number): void;
		GetCurrentZ(): number;

		Rect(r: SDK.Rect): void;
		Rect2(left: number, top: number, right: number, bottom: number): void;


		Quad(q: SDK.Quad): void;
		Quad2(tlx: number, tly: number, trx: number, try_: number, brx: number, bry: number, blx: number, bly: number): void;
		Quad3(q: SDK.Quad, r: SDK.Rect): void;
		Quad4(q: SDK.Quad, uv: SDK.Quad): void;
		Quad3D(tlx: number, tly: number, tlz: number, trx: number, try_: number, trz: number, brx: number, bry: number, brz: number, blx: number, bly: number, blz: number, r: SDK.Rect): void;
		Quad3D2(tlx: number, tly: number, tlz: number, trx: number, try_: number, trz: number, brx: number, bry: number, brz: number, blx: number, bly: number, blz: number, uv: SDK.Quad): void;

		DrawMesh(posArr: Float32Array, uvArr: Float32Array, indexArr: Uint16Array): void;

		ConvexPoly(pts: number[]): void;
		Line(x1: number, y1: number, x2: number, y2: number): void;
		TexturedLine(x1: number, y1: number, x2: number, y2: number, u: number, v: number): void;

		LineRect(left: number, top: number, right: number, bottom: number): void;
		LineRect2(r: SDK.Rect): void;
		LineQuad(q: SDK.Quad): void;

		PushLineWidth(w: number): void;
		PopLineWidth(): void;
		PushLineCap(type: RendererLineCapMode): void;
		PopLineCap(): void;

		SetTexture(tex: SDK.Gfx.IWebGLTexture): void;
		CreateDynamicTexture(width: number, height: number, opts?: TextureCreateOptions): SDK.Gfx.IWebGLTexture;
		UpdateTexture(data: TextureUpdateDataType, tex: SDK.Gfx.IWebGLTexture, opts?: TextureUpdateOptions): void;
		DeleteTexture(tex: SDK.Gfx.IWebGLTexture): void;

		CreateRendererText(): SDK.Gfx.IWebGLText;
	}
}