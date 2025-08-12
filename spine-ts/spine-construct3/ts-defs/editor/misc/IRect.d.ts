declare namespace SDK {
	class Rect {
		constructor(left?: number, top?: number, right?: number, bottom?: number);

		set(left: number, top: number, right: number, bottom: number): void;
		copy(r: SDK.Rect): void;
		clone(): SDK.Rect;

		setLeft(left: number): void;
		getLeft(): number;
		setTop(top: number): void;
		getTop(): number;
		setRight(right: number): void;
		getRight(): number;
		setBottom(bottom: number): void;
		getBottom(): number;

		width(): number;
		height(): number;
		midX(): number;
		midY(): number;

		offset(x: number, y: number): void;
		inflate(x: number, y: number): void;
		deflate(x: number, y: number): void;
		multiply(x: number, y: number): void;
		divide(x: number, y: number): void;
		clamp(left: number, top: number, right: number, bottom: number): void;
		normalize(): void;
		intersectsRect(r: SDK.Rect): boolean;
		containsPoint(x: number, y: number): boolean;
	}
}