// Note types like TextAlignHorizontalMode are taken from the runtime type
// definitions as they match the same types used by the editor

declare namespace SDK.Gfx {
	class IWebGLText {
		Release(): void;

		SetFontName(fontName: string): void;
		SetFontSize(fontSize: number): void;
		SetLineHeight(h: number): void;
		SetBold(b: boolean): void;
		SetItalic(i: boolean): void;
		SetColor(color: SDK.Color): void;
		SetColorRgb(r: number, g: number, b: number): void;
		SetHorizontalAlignment(h: TextAlignHorizontalMode): void;
		SetVerticalAlignment(v: TextAlignVerticalMode): void;
		SetWordWrapMode(mode: TextWordWrapMode): void;

		SetText(text: string): void;
		SetSize(width: number, height: number, zoomScale: number): void;

		GetTexture(): SDK.Gfx.IWebGLTexture | null;
		GetTexRect(): SDK.Rect;
		SetTextureUpdateCallback(callback: () => void): void;
		ReleaseTexture(): void;

		GetTextWidth(): number;
		GetTextHeight(): number;
	}
}