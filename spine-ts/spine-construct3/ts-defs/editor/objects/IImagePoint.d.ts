declare namespace SDK {
	class IImagePoint {
		GetAnimationFrame(): SDK.IAnimationFrame;
		GetName(): string;
		SetName(name: string): void;
		SetX(x: number): void;
		GetX(): number;
		SetY(y: number): void;
		GetY(): number;
	}
}