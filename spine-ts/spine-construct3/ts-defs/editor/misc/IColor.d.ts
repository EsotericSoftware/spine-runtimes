declare namespace SDK {
	class Color {
		constructor(r?: number, g?: number, b?: number, a?: number);

		setRgb(r: number, g: number, b: number): void;
		setRgba(r: number, g: number, b: number, a: number): void;

		copy(c: SDK.Color): void;
		copyRgb(c: SDK.Color): void;
		clone(): SDK.Color;

		setR(r: number): void;
		getR(): number;
		setG(g: number): void;
		getG(): number;
		setB(b: number): void;
		getB(): number;
		setA(a: number): void;
		getA(): number;

		equals(c: SDK.Color): boolean;
		equalsIgnoringAlpha(c: SDK.Color): boolean;
		equalsRgb(r: number, b: number, g: number): boolean;
		equalsRgba(r: number, b: number, g: number, a: number): boolean;

		premultiply(): void;
		unpremultiply(): void;
	}
}