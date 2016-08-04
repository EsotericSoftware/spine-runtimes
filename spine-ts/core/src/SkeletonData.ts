module spine {
    export class SkeletonData {
        name: string;
        bones = new Array<BoneData>(); // Ordered parents first.
        slots = new Array<SlotData>(); // Setup pose draw order.
        skins = new Array<Skin>();
        defaultSkin: Skin;
        events = new Array<EventData>();
        animations = new Array<Animation>();
        ikConstraints = new Array<IkConstraintData>();
        transformConstraints = new Array<TransformConstraintData>();
        pathConstraints = new Array<PathConstraintData>();
        width: number; height: number;
        version: string; hash: string; imagesPath: string;
    }
}