declare namespace SDK {
	class IWorldInstance extends IObjectInstance {
		GetBoundingBox(): SDK.Rect;
		GetQuad(): SDK.Quad;
		GetColor(): SDK.Color;

		SetOpacity(o: number): void;
		GetOpacity(): number;

		SetX(x: number): void;
		GetX(): number;
		SetY(y: number): void;
		GetY(): number;
		SetXY(x: number, y: number): void;

		SetZElevation(z: number): void;
		GetZElevation(): number;
		GetTotalZElevation(): number;

		SetWidth(w: number): void;
		GetWidth(): number;
		SetHeight(h: number): void;
		GetHeight(): number;
		SetSize(w: number, h: number): void;

		SetOriginX(x: number): void;
		GetOriginX(): number;
		SetOriginY(y: number): void;
		GetOriginY(): number;
		SetOrigin(x: number, y: number): void;

		SetAngle(a: number): void;
		GetAngle(): number;

		GetLayer(): SDK.ILayer;
		GetLayout(): SDK.ILayout;

		ApplyBlendMode(iRenderer: SDK.Gfx.IWebGLRenderer): void;
	}
}