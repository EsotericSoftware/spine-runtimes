declare namespace SDK.UI {
	class ILayoutView {
		GetProject(): SDK.IProject;
		GetZoomFactor(): number;
		GetViewMode(): "2d" | "3d";

		LayoutToClientDevice(x: number, y: number): Vec2Arr;

		/**
		 * @deprecated Use LayoutToClientDevice() instead
		 */
		LayoutToClientDeviceX(x: number): number;
		/**
		 * @deprecated Use LayoutToClientDevice() instead
		 */
		LayoutToClientDeviceY(y: number): number;

		SetDeviceTransform(iRenderer: SDK.Gfx.IWebGLRenderer): void;
		SetDefaultTransform(iRenderer: SDK.Gfx.IWebGLRenderer): void;
		
		Refresh(): void;

		GetLayout(): SDK.ILayout;
		GetActiveLayer(): SDK.ILayer;
	}
}