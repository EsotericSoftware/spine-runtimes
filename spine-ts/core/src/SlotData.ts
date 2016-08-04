module spine {
    export class SlotData {
        index: number;
        name: string;
        boneData: BoneData;
        color = new Color(1, 1, 1, 1);
        attachmentName: string;
        blendMode: BlendMode;
    }
}