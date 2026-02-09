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
		GetXY(): Vec2Arr;

		SetZ(z: number): void;
		GetZ(): number;
		GetTotalZ(): number;
		SetXYZ(x: number, y: number, z: number): void;
		GetXYZ(): Vec3Arr;

		/**
		 * @deprecated Use SetZ() instead
		 */
		SetZElevation(z: number): void;

		/**
		 * @deprecated Use GetZ() instead
		 */
		GetZElevation(): number;

		/**
		 * @deprecated Use GetTotalZ() instead
		 */
		GetTotalZElevation(): number;

		SetWidth(w: number): void;
		GetWidth(): number;
		SetHeight(h: number): void;
		GetHeight(): number;
		SetDepth(d: number): void;
		GetDepth(): number;
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

		SetSampling(sampling: SamplingModeOrAutoType): void;
		GetSampling(): SamplingModeOrAutoType;

		GetActiveSampling(): SamplingModeType;
	}
}