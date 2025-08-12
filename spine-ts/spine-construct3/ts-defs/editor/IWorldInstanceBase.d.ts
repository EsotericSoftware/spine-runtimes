
declare namespace SDK {
	class IWorldInstanceBase extends SDK.IInstanceBase {

		constructor(sdkType: SDK.ITypeBase, iInstance: SDK.IWorldInstance);

		_inst: SDK.IWorldInstance;

		Draw(iRenderer: SDK.Gfx.IWebGLRenderer, iDrawParams: SDK.Gfx.IDrawParams): void;
		OnPlacedInLayout(): void;

		GetTexture(animationFrame: SDK.IAnimationFrame): SDK.Gfx.IWebGLTexture | null;
		GetTexRect(): SDK.Rect;
		HadTextureError(): boolean;

		IsOriginalSizeKnown(): boolean;
		GetOriginalWidth(): number;
		GetOriginalHeight(): number;

		HasDoubleTapHandler(): boolean;
		OnDoubleTap(): void;
	}
}