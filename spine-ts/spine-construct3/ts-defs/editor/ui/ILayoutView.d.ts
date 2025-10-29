declare namespace SDK.UI {
	class ILayoutView {
		GetProject(): SDK.IProject;
		GetZoomFactor(): number;

		LayoutToClientDeviceX(x: number): number;
		LayoutToClientDeviceY(y: number): number;

		SetDeviceTransform(iRenderer: SDK.Gfx.IWebGLRenderer): void;
		SetDefaultTransform(iRenderer: SDK.Gfx.IWebGLRenderer): void;
		
		Refresh(): void;

		GetLayout(): SDK.ILayout;
		GetActiveLayer(): SDK.ILayer;
	}
}