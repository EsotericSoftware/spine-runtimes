
/** Represents a 3D axis-aligned bounding box.  */
declare class IAABB3D
{
	constructor(left?: number, top?: number, back?: number, right?: number, bottom?: number, front?: number);

    clone(): IAABB3D;
    copy(other: IAABB3D): void;
    set(left: number, top: number, back: number, right: number, bottom: number, front: number): void;

    left: number;
    top: number;
    back: number
    right: number;
    bottom: number;
    front: number;
}
