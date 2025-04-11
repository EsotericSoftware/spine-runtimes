/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
import { SpinePlugin } from "./SpinePlugin.js";
import { AnimationState, AnimationStateData, Bone, Skeleton } from "@esotericsoftware/spine-core";
declare class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
    constructor(scene: Phaser.Scene, type: string);
}
/** A bounds provider calculates the bounding box for a skeleton, which is then assigned as the size of the SpineGameObject. */
export interface SpineGameObjectBoundsProvider {
    calculateBounds(gameObject: SpineGameObject): {
        x: number;
        y: number;
        width: number;
        height: number;
    };
}
/** A bounds provider that calculates the bounding box from the setup pose. */
export declare class SetupPoseBoundsProvider implements SpineGameObjectBoundsProvider {
    private clipping;
    /**
     * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
     */
    constructor(clipping?: boolean);
    calculateBounds(gameObject: SpineGameObject): {
        x: number;
        y: number;
        width: number;
        height: number;
    };
}
/** A bounds provider that calculates the bounding box by taking the maximumg bounding box for a combination of skins and specific animation. */
export declare class SkinsAndAnimationBoundsProvider implements SpineGameObjectBoundsProvider {
    private animation;
    private skins;
    private timeStep;
    private clipping;
    /**
     * @param animation The animation to use for calculating the bounds. If null, the setup pose is used.
     * @param skins The skins to use for calculating the bounds. If empty, the default skin is used.
     * @param timeStep The time step to use for calculating the bounds. A smaller time step means more precision, but slower calculation.
     * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
     */
    constructor(animation: string | null, skins?: string[], timeStep?: number, clipping?: boolean);
    calculateBounds(gameObject: SpineGameObject): {
        x: number;
        y: number;
        width: number;
        height: number;
    };
}
declare const SpineGameObject_base: typeof BaseSpineGameObject & import("./mixins.js").Type<Phaser.GameObjects.Components.Alpha, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.Visible, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.Transform, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.ScrollFactor, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.Flip, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.ComputedSize, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.Origin, any[]> & import("./mixins.js").Type<Phaser.GameObjects.Components.Depth, any[]>;
/**
 * A SpineGameObject is a Phaser {@link GameObject} that can be added to a Phaser Scene and render a Spine skeleton.
 *
 * The Spine GameObject is a thin wrapper around a Spine {@link Skeleton}, {@link AnimationState} and {@link AnimationStateData}. It is responsible for:
 * - updating the animation state
 * - applying the animation state to the skeleton's bones, slots, attachments, and draw order.
 * - updating the skeleton's bone world transforms
 * - rendering the skeleton
 *
 * See the {@link SpinePlugin} class for more information on how to create a `SpineGameObject`.
 *
 * The skeleton, animation state, and animation state data can be accessed via the repsective fields. They can be manually updated via {@link updatePose}.
 *
 * To modify the bone hierarchy before the world transforms are computed, a callback can be set via the {@link beforeUpdateWorldTransforms} field.
 *
 * To modify the bone hierarchy after the world transforms are computed, a callback can be set via the {@link afterUpdateWorldTransforms} field.
 *
 * The class also features methods to convert between the skeleton coordinate system and the Phaser coordinate system.
 *
 * See {@link skeletonToPhaserWorldCoordinates}, {@link phaserWorldCoordinatesToSkeleton}, and {@link phaserWorldCoordinatesToBoneLocal.}
 */
export declare class SpineGameObject extends SpineGameObject_base {
    private plugin;
    boundsProvider: SpineGameObjectBoundsProvider;
    blendMode: number;
    skeleton: Skeleton;
    animationStateData: AnimationStateData;
    animationState: AnimationState;
    beforeUpdateWorldTransforms: (object: SpineGameObject) => void;
    afterUpdateWorldTransforms: (object: SpineGameObject) => void;
    private premultipliedAlpha;
    constructor(scene: Phaser.Scene, plugin: SpinePlugin, x: number, y: number, dataKey: string, atlasKey: string, boundsProvider?: SpineGameObjectBoundsProvider);
    updateSize(): void;
    /** Converts a point from the skeleton coordinate system to the Phaser world coordinate system. */
    skeletonToPhaserWorldCoordinates(point: {
        x: number;
        y: number;
    }): void;
    /** Converts a point from the Phaser world coordinate system to the skeleton coordinate system. */
    phaserWorldCoordinatesToSkeleton(point: {
        x: number;
        y: number;
    }): void;
    /** Converts a point from the Phaser world coordinate system to the bone's local coordinate system. */
    phaserWorldCoordinatesToBone(point: {
        x: number;
        y: number;
    }, bone: Bone): void;
    /**
     * Updates the {@link AnimationState}, applies it to the {@link Skeleton}, then updates the world transforms of all bones.
     * @param delta The time delta in milliseconds
     */
    updatePose(delta: number): void;
    preUpdate(time: number, delta: number): void;
    preDestroy(): void;
    willRender(camera: Phaser.Cameras.Scene2D.Camera): boolean;
    renderWebGL(renderer: Phaser.Renderer.WebGL.WebGLRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix): void;
    renderCanvas(renderer: Phaser.Renderer.Canvas.CanvasRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix): void;
}
export {};
