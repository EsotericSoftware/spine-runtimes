declare namespace SDK {
	class Quad {
		constructor(tlx?: number, tly?: number, trx?: number, try_?: number, brx?: number, bry?: number, blx?: number, bly?: number);

		set(tlx: number, tly: number, trx: number, try_: number, brx: number, bry: number, blx: number, bly: number): void;
		setRect(left: number, top: number, right: number, bottom: number): void;

		copy(q: SDK.Quad): void;

		setTlx(v: number): void;
		getTlx(): number;
		setTly(v: number): void;
		getTly(): number;
		setTrx(v: number): void;
		getTrx(): number;
		setTry(v: number): void;
		getTry(): number;
		setBrx(v: number): void;
		getBrx(): number;
		setBry(v: number): void;
		getBry(): number;
		setBlx(v: number): void;
		getBlx(): number;
		setBly(v: number): void;
		getBly(): number;

		midX(): number;
		midY(): number;

		offset(x: number, y: number): void;

		setFromRect(r: SDK.Rect): void;
		setFromRotatedRect(r: SDK.Rect, a: number): void;
		getBoundingBox(r: SDK.Rect): void;

		containsPoint(x: number, y: number): boolean;
		intersectsSegment(x1: number, y1: number, x2: number, y2: number): boolean;
		intersectsQuad(q: SDK.Quad): boolean;
	}
}