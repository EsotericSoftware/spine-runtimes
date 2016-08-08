declare module spine {
    class Animation {
        name: string;
        timelines: Array<Timeline>;
        duration: number;
        constructor(name: string, timelines: Array<Timeline>, duration: number);
        apply(skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>): void;
        mix(skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>, alpha: number): void;
        static binarySearch(values: Array<number>, target: number, step?: number): number;
        static linearSearch(values: Array<number>, target: number, step: number): number;
    }
    interface Timeline {
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    abstract class CurveTimeline implements Timeline {
        static LINEAR: number;
        static STEPPED: number;
        static BEZIER: number;
        static BEZIER_SIZE: number;
        private curves;
        constructor(frameCount: number);
        getFrameCount(): number;
        setLinear(frameIndex: number): void;
        setStepped(frameIndex: number): void;
        getCurveType(frameIndex: number): number;
        /** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
         * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
         * the difference between the keyframe's values. */
        setCurve(frameIndex: number, cx1: number, cy1: number, cx2: number, cy2: number): void;
        getCurvePercent(frameIndex: number, percent: number): number;
        abstract apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class RotateTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_ROTATION: number;
        static ROTATION: number;
        boneIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time and angle of the specified keyframe. */
        setFrame(frameIndex: number, time: number, degrees: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class TranslateTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_X: number;
        static PREV_Y: number;
        static X: number;
        static Y: number;
        boneIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time and value of the specified keyframe. */
        setFrame(frameIndex: number, time: number, x: number, y: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class ScaleTimeline extends TranslateTimeline {
        constructor(frameCount: number);
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class ShearTimeline extends TranslateTimeline {
        constructor(frameCount: number);
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class ColorTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_R: number;
        static PREV_G: number;
        static PREV_B: number;
        static PREV_A: number;
        static R: number;
        static G: number;
        static B: number;
        static A: number;
        slotIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time and value of the specified keyframe. */
        setFrame(frameIndex: number, time: number, r: number, g: number, b: number, a: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class AttachmentTimeline implements Timeline {
        slotIndex: number;
        frames: Array<number>;
        attachmentNames: Array<string>;
        constructor(frameCount: number);
        getFrameCount(): number;
        /** Sets the time and value of the specified keyframe. */
        setFrame(frameIndex: number, time: number, attachmentName: string): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
    }
    class EventTimeline implements Timeline {
        frames: Array<number>;
        events: Array<Event>;
        constructor(frameCount: number);
        getFrameCount(): number;
        /** Sets the time of the specified keyframe. */
        setFrame(frameIndex: number, event: Event): void;
        /** Fires events for frames > lastTime and <= time. */
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class DrawOrderTimeline implements Timeline {
        frames: Array<number>;
        drawOrders: Array<Array<number>>;
        constructor(frameCount: number);
        getFrameCount(): number;
        /** Sets the time of the specified keyframe.
         * @param drawOrder May be null to use bind pose draw order. */
        setFrame(frameIndex: number, time: number, drawOrder: Array<number>): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class DeformTimeline extends CurveTimeline {
        frames: Array<number>;
        frameVertices: Array<Array<number>>;
        slotIndex: number;
        attachment: VertexAttachment;
        constructor(frameCount: number);
        /** Sets the time of the specified keyframe. */
        setFrame(frameIndex: number, time: number, vertices: Array<number>): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class IkConstraintTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_MIX: number;
        static PREV_BEND_DIRECTION: number;
        static MIX: number;
        static BEND_DIRECTION: number;
        ikConstraintIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time, mix and bend direction of the specified keyframe. */
        setFrame(frameIndex: number, time: number, mix: number, bendDirection: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class TransformConstraintTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_ROTATE: number;
        static PREV_TRANSLATE: number;
        static PREV_SCALE: number;
        static PREV_SHEAR: number;
        static ROTATE: number;
        static TRANSLATE: number;
        static SCALE: number;
        static SHEAR: number;
        transformConstraintIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time and mixes of the specified keyframe. */
        setFrame(frameIndex: number, time: number, rotateMix: number, translateMix: number, scaleMix: number, shearMix: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class PathConstraintPositionTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_VALUE: number;
        static VALUE: number;
        pathConstraintIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time and value of the specified keyframe. */
        setFrame(frameIndex: number, time: number, value: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class PathConstraintSpacingTimeline extends PathConstraintPositionTimeline {
        constructor(frameCount: number);
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
    class PathConstraintMixTimeline extends CurveTimeline {
        static ENTRIES: number;
        static PREV_TIME: number;
        static PREV_ROTATE: number;
        static PREV_TRANSLATE: number;
        static ROTATE: number;
        static TRANSLATE: number;
        pathConstraintIndex: number;
        frames: Array<number>;
        constructor(frameCount: number);
        /** Sets the time and mixes of the specified keyframe. */
        setFrame(frameIndex: number, time: number, rotateMix: number, translateMix: number): void;
        apply(skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number): void;
    }
}
declare module spine {
    abstract class Attachment {
        name: string;
        constructor(name: string);
    }
    abstract class VertexAttachment extends Attachment {
        bones: Array<number>;
        vertices: Array<number>;
        worldVerticesLength: number;
        constructor(name: string);
        computeWorldVertices(slot: Slot, worldVertices: Array<number>): void;
        /** Transforms local vertices to world coordinates.
         * @param start The index of the first local vertex value to transform. Each vertex has 2 values, x and y.
         * @param count The number of world vertex values to output. Must be <= {@link #getWorldVerticesLength()} - start.
         * @param worldVertices The output world vertices. Must have a length >= offset + count.
         * @param offset The worldVertices index to begin writing values. */
        computeWorldVerticesWith(slot: Slot, start: number, count: number, worldVertices: Array<number>, offset: number): void;
        /** Returns true if a deform originally applied to the specified attachment should be applied to this attachment. */
        applyDeform(sourceAttachment: VertexAttachment): boolean;
    }
}
declare module spine {
    interface AttachmentLoader {
        /** @return May be null to not load an attachment. */
        newRegionAttachment(skin: Skin, name: string, path: string): RegionAttachment;
        /** @return May be null to not load an attachment. */
        newMeshAttachment(skin: Skin, name: string, path: string): MeshAttachment;
        /** @return May be null to not load an attachment. */
        newBoundingBoxAttachment(skin: Skin, name: string): BoundingBoxAttachment;
        /** @return May be null to not load an attachment */
        newPathAttachment(skin: Skin, name: string): PathAttachment;
    }
}
declare module spine {
    enum AttachmentType {
        Region = 0,
        BoundingBox = 1,
        Mesh = 2,
        LinkedMesh = 3,
        Path = 4,
    }
}
declare module spine {
    class BoundingBoxAttachment extends VertexAttachment {
        constructor(name: string);
    }
}
declare module spine {
    class MeshAttachment extends VertexAttachment {
        region: TextureRegion;
        path: string;
        regionUVs: Array<number>;
        worldVertices: Array<number>;
        triangles: Array<number>;
        color: Color;
        hullLength: number;
        private _parentMesh;
        inheritDeform: boolean;
        tempColor: Color;
        constructor(name: string);
        updateUVs(): void;
        /** @return The updated world vertices. */
        updateWorldVertices(slot: Slot, premultipliedAlpha: boolean): number[];
        applyDeform(sourceAttachment: VertexAttachment): boolean;
        getParentMesh(): MeshAttachment;
        /** @param parentMesh May be null. */
        setParentMesh(parentMesh: MeshAttachment): void;
    }
}
declare module spine {
    class PathAttachment extends VertexAttachment {
        lengths: Array<number>;
        closed: boolean;
        constantSpeed: boolean;
        constructor(name: string);
    }
}
declare module spine {
    class RegionAttachment extends Attachment {
        static OX1: number;
        static OY1: number;
        static OX2: number;
        static OY2: number;
        static OX3: number;
        static OY3: number;
        static OX4: number;
        static OY4: number;
        static X1: number;
        static Y1: number;
        static X2: number;
        static Y2: number;
        static X3: number;
        static Y3: number;
        static X4: number;
        static Y4: number;
        static U1: number;
        static V1: number;
        static U2: number;
        static V2: number;
        static U3: number;
        static V3: number;
        static U4: number;
        static V4: number;
        static C1R: number;
        static C1G: number;
        static C1B: number;
        static C1A: number;
        static C2R: number;
        static C2G: number;
        static C2B: number;
        static C2A: number;
        static C3R: number;
        static C3G: number;
        static C3B: number;
        static C3A: number;
        static C4R: number;
        static C4G: number;
        static C4B: number;
        static C4A: number;
        x: number;
        y: number;
        scaleX: number;
        scaleY: number;
        rotation: number;
        width: number;
        height: number;
        color: Color;
        path: string;
        rendererObject: any;
        region: TextureRegion;
        offset: number[];
        vertices: number[];
        tempColor: Color;
        constructor(name: string);
        setRegion(region: TextureRegion): void;
        updateOffset(): void;
        updateWorldVertices(slot: Slot, premultipliedAlpha: boolean): number[];
    }
}
declare module spine {
    class TextureRegion {
        renderObject: any;
        u: number;
        v: number;
        u2: number;
        v2: number;
        width: number;
        height: number;
        rotate: boolean;
        offsetX: number;
        offsetY: number;
        originalWidth: number;
        originalHeight: number;
    }
}
declare module spine {
    enum BlendMode {
        Normal = 0,
        Additive = 1,
        Multiply = 2,
        Screen = 3,
    }
}
declare module spine {
    class Bone implements Updatable {
        data: BoneData;
        skeleton: Skeleton;
        parent: Bone;
        children: Bone[];
        x: number;
        y: number;
        rotation: number;
        scaleX: number;
        scaleY: number;
        shearX: number;
        shearY: number;
        appliedRotation: number;
        a: number;
        b: number;
        worldX: number;
        c: number;
        d: number;
        worldY: number;
        worldSignX: number;
        worldSignY: number;
        sorted: boolean;
        /** @param parent May be null. */
        constructor(data: BoneData, skeleton: Skeleton, parent: Bone);
        /** Same as {@link #updateWorldTransform()}. This method exists for Bone to implement {@link Updatable}. */
        update(): void;
        /** Computes the world transform using the parent bone and this bone's local transform. */
        updateWorldTransform(): void;
        /** Computes the world transform using the parent bone and the specified local transform. */
        updateWorldTransformWith(x: number, y: number, rotation: number, scaleX: number, scaleY: number, shearX: number, shearY: number): void;
        setToSetupPose(): void;
        getWorldRotationX(): number;
        getWorldRotationY(): number;
        getWorldScaleX(): number;
        getWorldScaleY(): number;
        worldToLocalRotationX(): number;
        worldToLocalRotationY(): number;
        rotateWorld(degrees: number): void;
        /** Computes the local transform from the world transform. This can be useful to perform processing on the local transform
         * after the world transform has been modified directly (eg, by a constraint).
         * <p>
         * Some redundant information is lost by the world transform, such as -1,-1 scale versus 180 rotation. The computed local
         * transform values may differ from the original values but are functionally the same. */
        updateLocalTransform(): void;
        worldToLocal(world: Vector2): Vector2;
        localToWorld(local: Vector2): Vector2;
    }
}
declare module spine {
    class BoneData {
        index: number;
        name: string;
        parent: BoneData;
        length: number;
        x: number;
        y: number;
        rotation: number;
        scaleX: number;
        scaleY: number;
        shearX: number;
        shearY: number;
        inheritRotation: boolean;
        inheritScale: boolean;
        BoneData(index: number, name: string, parent: BoneData): void;
    }
}
declare module spine {
    class Event {
        data: EventData;
        intValue: number;
        floatValue: number;
        stringValue: string;
        time: number;
        constructor(time: number, data: EventData);
    }
}
declare module spine {
    class EventData {
        name: string;
        intValue: number;
        floatValue: number;
        stringValue: string;
    }
}
declare module spine {
    class IkConstraint implements Updatable {
        data: IkConstraintData;
        bones: Array<Bone>;
        target: Bone;
        mix: number;
        bendDirection: number;
        level: number;
        constructor(data: IkConstraintData, skeleton: Skeleton);
        apply(): void;
        update(): void;
        /** Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified in the world
         * coordinate system. */
        applyShort(bone: Bone, targetX: number, targetY: number, alpha: number): void;
        /** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
         * target is specified in the world coordinate system.
         * @param child A direct descendant of the parent bone. */
        applyWith(parent: Bone, child: Bone, targetX: number, targetY: number, bendDir: number, alpha: number): void;
    }
}
declare module spine {
    class IkConstraintData {
        name: string;
        bones: BoneData[];
        target: BoneData;
        bendDirection: number;
        mix: number;
    }
}
declare module spine {
    class PathConstraint implements Updatable {
        static NONE: number;
        static BEFORE: number;
        static AFTER: number;
        data: PathConstraintData;
        bones: Array<Bone>;
        target: Slot;
        position: number;
        spacing: number;
        rotateMix: number;
        translateMix: number;
        spaces: number[];
        positions: number[];
        world: number[];
        curves: number[];
        lengths: number[];
        segments: number[];
        constructor(data: PathConstraintData, skeleton: Skeleton);
        apply(): void;
        update(): void;
        computeWorldPositions(path: PathAttachment, spacesCount: number, tangents: boolean, percentPosition: boolean, percentSpacing: boolean): number[];
        addBeforePosition(p: number, temp: Array<number>, i: number, out: Array<number>, o: number): void;
        addAfterPosition(p: number, temp: Array<number>, i: number, out: Array<number>, o: number): void;
        addCurvePosition(p: number, x1: number, y1: number, cx1: number, cy1: number, cx2: number, cy2: number, x2: number, y2: number, out: Array<number>, o: number, tangents: boolean): void;
    }
}
declare module spine {
    class PathConstraintData {
        name: string;
        bones: BoneData[];
        target: SlotData;
        positionMode: PositionMode;
        spacingMode: SpacingMode;
        rotateMode: RotateMode;
        offsetRotation: number;
        position: number;
        spacing: number;
        rotateMix: number;
        translateMix: number;
    }
    enum PositionMode {
        Fixed = 0,
        Percent = 1,
    }
    enum SpacingMode {
        Length = 0,
        Fixed = 1,
        Percent = 2,
    }
    enum RotateMode {
        Tangent = 0,
        Chain = 1,
        ChainScale = 2,
    }
}
declare module spine {
    class Skeleton {
        data: SkeletonData;
        bones: Array<Bone>;
        slots: Array<Slot>;
        drawOrder: Array<Slot>;
        ikConstraints: Array<IkConstraint>;
        ikConstraintsSorted: Array<IkConstraint>;
        transformConstraints: Array<TransformConstraint>;
        pathConstraints: Array<PathConstraint>;
        _updateCache: Updatable[];
        skin: Skin;
        color: Color;
        time: number;
        flipX: boolean;
        flipY: boolean;
        x: number;
        y: number;
        constructor(data: SkeletonData);
        updateCache(): void;
        sortPathConstraintAttachment(skin: Skin, slotIndex: number, slotBone: Bone): void;
        sortPathConstraintAttachmentWith(attachment: Attachment, slotBone: Bone): void;
        sortBone(bone: Bone): void;
        sortReset(bones: Array<Bone>): void;
        /** Updates the world transform for each bone and applies constraints. */
        updateWorldTransform(): void;
        /** Sets the bones, constraints, and slots to their setup pose values. */
        setToSetupPose(): void;
        /** Sets the bones and constraints to their setup pose values. */
        setBonesToSetupPose(): void;
        setSlotsToSetupPose(): void;
        /** @return May return null. */
        getRootBone(): Bone;
        /** @return May be null. */
        findBone(boneName: string): Bone;
        /** @return -1 if the bone was not found. */
        findBoneIndex(boneName: string): number;
        /** @return May be null. */
        findSlot(slotName: string): Slot;
        /** @return -1 if the bone was not found. */
        findSlotIndex(slotName: string): number;
        /** Sets a skin by name.
         * @see #setSkin(Skin) */
        setSkinByName(skinName: string): void;
        /** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}.
         * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
         * old skin, each slot's setup mode attachment is attached from the new skin.
         * @param newSkin May be null. */
        setSkin(newSkin: Skin): void;
        /** @return May be null. */
        getAttachmentByName(slotName: string, attachmentName: string): Attachment;
        /** @return May be null. */
        getAttachment(slotIndex: number, attachmentName: string): Attachment;
        /** @param attachmentName May be null. */
        setAttachment(slotName: string, attachmentName: string): void;
        /** @return May be null. */
        findIkConstraint(constraintName: string): IkConstraint;
        /** @return May be null. */
        findTransformConstraint(constraintName: string): TransformConstraint;
        /** @return May be null. */
        findPathConstraint(constraintName: string): PathConstraint;
        /** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
         * @param offset The distance from the skeleton origin to the bottom left corner of the AABB.
         * @param size The width and height of the AABB. */
        getBounds(offset: Vector2, size: Vector2): void;
        update(delta: number): void;
    }
}
declare module spine {
    class SkeletonBounds {
        minX: number;
        minY: number;
        maxX: number;
        maxY: number;
        boundingBoxes: BoundingBoxAttachment[];
        polygons: number[][];
        update(skeleton: Skeleton, updateAabb: boolean): void;
        aabbCompute(): void;
        /** Returns true if the axis aligned bounding box contains the point. */
        aabbContainsPoint(x: number, y: number): boolean;
        /** Returns true if the axis aligned bounding box intersects the line segment. */
        aabbIntersectsSegment(x1: number, y1: number, x2: number, y2: number): boolean;
        /** Returns true if the axis aligned bounding box intersects the axis aligned bounding box of the specified bounds. */
        aabbIntersectsSkeleton(bounds: SkeletonBounds): boolean;
        /** Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
         * efficient to only call this method if {@link #aabbContainsPoint(float, float)} returns true. */
        containsPoint(x: number, y: number): BoundingBoxAttachment;
        /** Returns true if the polygon contains the point. */
        containsPointPolygon(polygon: Array<number>, x: number, y: number): boolean;
        /** Returns the first bounding box attachment that contains any part of the line segment, or null. When doing many checks, it
         * is usually more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} returns
         * true. */
        intersectsSegment(x1: number, y1: number, x2: number, y2: number): BoundingBoxAttachment;
        /** Returns true if the polygon contains any part of the line segment. */
        intersectsSegmentPolygon(polygon: Array<number>, x1: number, y1: number, x2: number, y2: number): boolean;
        /** Returns the polygon for the specified bounding box, or null. */
        getPolygon(boundingBox: BoundingBoxAttachment): number[];
    }
}
declare module spine {
    class SkeletonData {
        name: string;
        bones: BoneData[];
        slots: SlotData[];
        skins: Skin[];
        defaultSkin: Skin;
        events: EventData[];
        animations: Animation[];
        ikConstraints: IkConstraintData[];
        transformConstraints: TransformConstraintData[];
        pathConstraints: PathConstraintData[];
        width: number;
        height: number;
        version: string;
        hash: string;
        imagesPath: string;
        findBone(boneName: string): BoneData;
        findBoneIndex(boneName: string): number;
        findSlot(slotName: string): SlotData;
        findSlotIndex(slotName: string): number;
        findSkin(skinName: string): Skin;
        findEvent(eventDataName: string): EventData;
        findAnimation(animationName: string): Animation;
        findIkConstraint(constraintName: string): IkConstraintData;
        findTransformConstraint(constraintName: string): TransformConstraintData;
        findPathConstraint(constraintName: string): PathConstraintData;
        findPathConstraintIndex(pathConstraintName: string): number;
    }
}
declare module spine {
    class Skin {
        name: string;
        attachments: Map<Attachment>[];
        constructor(name: string);
        addAttachment(slotIndex: number, name: string, attachment: Attachment): void;
        /** @return May be null. */
        getAttachment(slotIndex: number, name: string): Attachment;
        /** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
        attachAll(skeleton: Skeleton, oldSkin: Skin): void;
    }
}
declare module spine {
    class Slot {
        data: SlotData;
        bone: Bone;
        color: Color;
        private attachment;
        private attachmentTime;
        attachmentVertices: number[];
        constructor(data: SlotData, bone: Bone);
        /** @return May be null. */
        getAttachment(): Attachment;
        /** Sets the attachment and if it changed, resets {@link #getAttachmentTime()} and clears {@link #getAttachmentVertices()}.
         * @param attachment May be null. */
        setAttachment(attachment: Attachment): void;
        setAttachmentTime(time: number): void;
        /** Returns the time since the attachment was set. */
        getAttachmentTime(): number;
        setToSetupPose(): void;
    }
}
declare module spine {
    class SlotData {
        index: number;
        name: string;
        boneData: BoneData;
        color: Color;
        attachmentName: string;
        blendMode: BlendMode;
        constructor(index: number, name: string, boneData: BoneData);
    }
}
declare module spine {
    class TransformConstraint implements Updatable {
        data: TransformConstraintData;
        bones: Array<Bone>;
        target: Bone;
        rotateMix: number;
        translateMix: number;
        scaleMix: number;
        shearMix: number;
        temp: Vector2;
        constructor(data: TransformConstraintData, skeleton: Skeleton);
        apply(): void;
        update(): void;
    }
}
declare module spine {
    class TransformConstraintData {
        name: string;
        bones: BoneData[];
        target: BoneData;
        rotateMix: number;
        translateMix: number;
        scaleMix: number;
        shearMix: number;
        offsetRotation: number;
        offsetX: number;
        offsetY: number;
        offsetScaleX: number;
        offsetScaleY: number;
        offsetShearY: number;
        constructor(name: string);
    }
}
declare module spine {
    interface Updatable {
        update(): void;
    }
}
declare module spine {
    interface Map<T> {
        [key: string]: T;
    }
    interface Disposable {
        dispose(): void;
    }
    class Color {
        r: number;
        g: number;
        b: number;
        a: number;
        constructor(r?: number, g?: number, b?: number, a?: number);
        set(r: number, g: number, b: number, a: number): void;
        setFromColor(c: Color): void;
        add(r: number, g: number, b: number, a: number): void;
        clamp(): this;
    }
    class MathUtils {
        static PI: number;
        static PI2: number;
        static radiansToDegrees: number;
        static radDeg: number;
        static degreesToRadians: number;
        static degRad: number;
        static clamp(value: number, min: number, max: number): number;
        static cosDeg(degrees: number): number;
        static sinDeg(degrees: number): number;
        static signum(value: number): number;
    }
    class Utils {
        static arrayCopy<T>(source: Array<T>, sourceStart: number, dest: Array<T>, destStart: number, numElements: number): void;
        static setArraySize(array: Array<number>, size: number): Array<number>;
    }
    class Vector2 {
        x: number;
        y: number;
        constructor(x?: number, y?: number);
        set(x: number, y: number): Vector2;
    }
}
declare module spine.webgl {
    class AssetManager implements Disposable {
        private _assets;
        private _errors;
        private _toLoad;
        private _loaded;
        loadText(path: string, success: (path: string, text: string) => void, error: (path: string, error: string) => void): void;
        loadTexture(path: string, success: (path: string, image: HTMLImageElement) => void, error: (path: string, error: string) => void): void;
        get(path: string): string | Texture;
        remove(path: string): void;
        removeAll(): void;
        isLoadingComplete(): boolean;
        toLoad(): number;
        loaded(): number;
        dispose(): void;
    }
}
declare module spine.webgl {
    let M00: number;
    let M01: number;
    let M02: number;
    let M03: number;
    let M10: number;
    let M11: number;
    let M12: number;
    let M13: number;
    let M20: number;
    let M21: number;
    let M22: number;
    let M23: number;
    let M30: number;
    let M31: number;
    let M32: number;
    let M33: number;
    class Matrix4 {
        temp: Float32Array;
        values: Float32Array;
        constructor();
        set(values: Float32Array | Array<number>): Matrix4;
        transpose(): Matrix4;
        identity(): Matrix4;
        invert(): Matrix4;
        determinant(): number;
        translate(x: number, y: number, z: number): Matrix4;
        copy(): Matrix4;
        projection(near: number, far: number, fovy: number, aspectRatio: number): Matrix4;
        ortho2d(x: number, y: number, width: number, height: number): Matrix4;
        ortho(left: number, right: number, bottom: number, top: number, near: number, far: number): Matrix4;
        multiply(matrix: Matrix4): Matrix4;
        multiplyLeft(matrix: Matrix4): Matrix4;
    }
}
declare module spine.webgl {
    class Mesh implements Disposable {
        private _attributes;
        private _vertices;
        private _verticesBuffer;
        private _verticesLength;
        private _dirtyVertices;
        private _indices;
        private _indicesBuffer;
        private _indicesLength;
        private _dirtyIndices;
        private _elementsPerVertex;
        attributes(): VertexAttribute[];
        maxVertices(): number;
        numVertices(): number;
        setVerticesLength(length: number): void;
        vertices(): Float32Array;
        maxIndices(): number;
        numIndices(): number;
        setIndicesLength(length: number): void;
        indices(): Uint16Array;
        constructor(_attributes: VertexAttribute[], maxVertices: number, maxIndices: number);
        setVertices(vertices: Array<number>): void;
        setIndices(indices: Array<number>): void;
        draw(shader: Shader, primitiveType: number): void;
        drawWithOffset(shader: Shader, primitiveType: number, offset: number, count: number): void;
        bind(shader: Shader): void;
        unbind(shader: Shader): void;
        private update();
        dispose(): void;
    }
    class VertexAttribute {
        name: string;
        type: VertexAttributeType;
        numElements: number;
        constructor(name: string, type: VertexAttributeType, numElements: number);
    }
    class Position2Attribute extends VertexAttribute {
        constructor();
    }
    class Position3Attribute extends VertexAttribute {
        constructor();
    }
    class TexCoordAttribute extends VertexAttribute {
        constructor(unit?: number);
    }
    class ColorAttribute extends VertexAttribute {
        constructor();
    }
    enum VertexAttributeType {
        Float = 0,
    }
}
declare module spine.webgl {
    class PolygonBatcher {
        private _drawCalls;
        private _drawing;
        private _mesh;
        private _shader;
        private _lastTexture;
        private _verticesLength;
        private _indicesLength;
        private _srcBlend;
        private _dstBlend;
        constructor(maxVertices?: number);
        begin(shader: Shader): void;
        setBlendMode(srcBlend: number, dstBlend: number): void;
        draw(texture: Texture, vertices: Array<number>, indices: Array<number>): void;
        private flush();
        end(): void;
        drawCalls(): number;
    }
}
declare module spine.webgl {
    class Shader implements Disposable {
        private _vertexShader;
        private _fragmentShader;
        static MVP_MATRIX: string;
        static POSITION: string;
        static COLOR: string;
        static TEXCOORDS: string;
        static SAMPLER: string;
        private _vs;
        private _fs;
        private _program;
        private _tmp2x2;
        private _tmp3x3;
        private _tmp4x4;
        program(): WebGLProgram;
        vertexShader(): string;
        fragmentShader(): string;
        constructor(_vertexShader: string, _fragmentShader: string);
        private compile();
        private compileShader(type, source);
        private compileProgram(vs, fs);
        bind(): void;
        unbind(): void;
        setUniformi(uniform: string, value: number): void;
        setUniformf(uniform: string, value: number): void;
        setUniform2f(uniform: string, value: number, value2: number): void;
        setUniform3f(uniform: string, value: number, value2: number, value3: number): void;
        setUniform4f(uniform: string, value: number, value2: number, value3: number, value4: number): void;
        setUniform2x2f(uniform: string, value: Array<number> | Float32Array): void;
        setUniform3x3f(uniform: string, value: Array<number> | Float32Array): void;
        setUniform4x4f(uniform: string, value: Array<number> | Float32Array): void;
        getUniformLocation(uniform: string): WebGLUniformLocation;
        getAttributeLocation(attribute: string): number;
        dispose(): void;
        static newColoredTextured(): Shader;
        static newColored(): Shader;
    }
}
declare module spine.webgl {
    class Texture implements Disposable {
        private _texture;
        private _image;
        private _boundUnit;
        constructor(image: HTMLImageElement, useMipMaps?: boolean);
        getImage(): HTMLImageElement;
        setFilters(minFilter: TextureFilter, magFilter: TextureFilter): void;
        setWraps(uWrap: TextureWrap, vWrap: TextureWrap): void;
        update(useMipMaps: boolean): void;
        bind(unit?: number): void;
        unbind(): void;
        dispose(): void;
        static filterFromString(text: string): TextureFilter;
        static wrapFromString(text: string): TextureWrap;
    }
    enum TextureFilter {
        Nearest,
        Linear,
        MipMap,
        MipMapNearestNearest,
        MipMapLinearNearest,
        MipMapNearestLinear,
        MipMapLinearLinear,
    }
    enum TextureWrap {
        MirroredRepeat,
        ClampToEdge,
        Repeat,
    }
}
declare module spine.webgl {
    class TextureAtlas implements Disposable {
        pages: TextureAtlasPage[];
        regions: TextureAtlasRegion[];
        constructor(atlasText: string, textureLoader: (path: string) => Texture);
        private load(atlasText, textureLoader);
        findRegion(name: string): TextureAtlasRegion;
        dispose(): void;
    }
    class TextureAtlasPage {
        name: string;
        minFilter: TextureFilter;
        magFilter: TextureFilter;
        uWrap: TextureWrap;
        vWrap: TextureWrap;
        texture: Texture;
        width: number;
        height: number;
    }
    class TextureAtlasRegion {
        page: TextureAtlasPage;
        name: string;
        x: number;
        y: number;
        width: number;
        height: number;
        u: number;
        v: number;
        u2: number;
        v2: number;
        offsetX: number;
        offsetY: number;
        originalWidth: number;
        originalHeight: number;
        index: number;
        rotate: boolean;
        texture: Texture;
    }
}
declare module spine.webgl {
    class Vector3 {
        x: number;
        y: number;
        z: number;
        set(x: number, y: number, z: number): Vector3;
        add(v: Vector3): Vector3;
        sub(v: Vector3): Vector3;
        scale(s: number): Vector3;
        normalize(): Vector3;
        cross(v: Vector3): Vector3;
        multiply(matrix: Matrix4): Vector3;
        project(matrix: Matrix4): Vector3;
        dot(v: Vector3): number;
        length(): number;
        distance(v: Vector3): number;
    }
}
declare module spine.webgl {
    var gl: WebGLRenderingContext;
    function init(gl: WebGLRenderingContext): void;
    function getSourceGLBlendMode(blendMode: BlendMode, premultipliedAlpha?: boolean): number;
    function getDestGLBlendMode(blendMode: BlendMode): number;
}
