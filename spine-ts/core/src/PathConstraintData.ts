module spine {
    export class PathConstraintData {
        name: string;
        bones = new Array<BoneData>();
        target: SlotData;
        positionMode: PositionMode;
        spacingMode: SpacingMode;
        rotateMode: RotateMode;
        offsetRotation: number;
        position: number; spacing: number; rotateMix: number; translateMix: number;
    }

    export enum PositionMode {
        Fixed, Percent
    }

    export enum SpacingMode {
        Length, Fixed, Percent
    }

    export enum RotateMode {
        Tangent, Chain, ChainScale
    }
}