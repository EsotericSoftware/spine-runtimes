module spine {
    export class IkConstraintData {
        name: string;
        bones = new Array<BoneData>();
        target: BoneData;
        bendDirection = 1;
        mix = 1;
    }
}