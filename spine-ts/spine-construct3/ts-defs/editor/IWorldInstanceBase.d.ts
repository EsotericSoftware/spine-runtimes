
declare namespace SDK {
	class IWorldInstanceBase extends SDK.IInstanceBase {

		constructor(sdkType: SDK.ITypeBase, iInstance: SDK.IWorldInstance);

		_inst: SDK.IWorldInstance;

		Draw(iRenderer: SDK.Gfx.IWebGLRenderer, iDrawParams: SDK.Gfx.IDrawParams): void;
		OnPlacedInLayout(iLayoutView?: SDK.UI.ILayoutView): void;

		GetTexture(animationFrame: SDK.IAnimationFrame): SDK.Gfx.IWebGLTexture | null;
		GetTexRect(): SDK.Rect;
		HadTextureError(): boolean;

		IsOriginalSizeKnown(): boolean;
		GetOriginalSize(): Vec3Arr;

		/**
		 * @deprecated Use GetOriginalSize() instead
		 */
		GetOriginalWidth(): number;
		/**
		 * @deprecated Use GetOriginalSize() instead
		 */
		GetOriginalHeight(): number;

		HasDoubleTapHandler(): boolean;
		OnDoubleTap(): void;
	}
}