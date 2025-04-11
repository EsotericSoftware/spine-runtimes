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
import { SPINE_GAME_OBJECT_TYPE } from "./keys.js";
import { ComputedSizeMixin, DepthMixin, FlipMixin, ScrollFactorMixin, TransformMixin, VisibleMixin, AlphaMixin, OriginMixin, } from "./mixins.js";
import { AnimationState, AnimationStateData, MathUtils, Physics, Skeleton, SkeletonClipping, Skin, } from "@esotericsoftware/spine-core";
class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
    constructor(scene, type) {
        super(scene, type);
    }
}
/** A bounds provider that calculates the bounding box from the setup pose. */
export class SetupPoseBoundsProvider {
    clipping;
    /**
     * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
     */
    constructor(clipping = false) {
        this.clipping = clipping;
    }
    calculateBounds(gameObject) {
        if (!gameObject.skeleton)
            return { x: 0, y: 0, width: 0, height: 0 };
        // Make a copy of animation state and skeleton as this might be called while
        // the skeleton in the GameObject has already been heavily modified. We can not
        // reconstruct that state.
        const skeleton = new Skeleton(gameObject.skeleton.data);
        skeleton.setToSetupPose();
        skeleton.updateWorldTransform(Physics.update);
        const bounds = skeleton.getBoundsRect(this.clipping ? new SkeletonClipping() : undefined);
        return bounds.width == Number.NEGATIVE_INFINITY
            ? { x: 0, y: 0, width: 0, height: 0 }
            : bounds;
    }
}
/** A bounds provider that calculates the bounding box by taking the maximumg bounding box for a combination of skins and specific animation. */
export class SkinsAndAnimationBoundsProvider {
    animation;
    skins;
    timeStep;
    clipping;
    /**
     * @param animation The animation to use for calculating the bounds. If null, the setup pose is used.
     * @param skins The skins to use for calculating the bounds. If empty, the default skin is used.
     * @param timeStep The time step to use for calculating the bounds. A smaller time step means more precision, but slower calculation.
     * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
     */
    constructor(animation, skins = [], timeStep = 0.05, clipping = false) {
        this.animation = animation;
        this.skins = skins;
        this.timeStep = timeStep;
        this.clipping = clipping;
    }
    calculateBounds(gameObject) {
        if (!gameObject.skeleton || !gameObject.animationState)
            return { x: 0, y: 0, width: 0, height: 0 };
        // Make a copy of animation state and skeleton as this might be called while
        // the skeleton in the GameObject has already been heavily modified. We can not
        // reconstruct that state.
        const animationState = new AnimationState(gameObject.animationState.data);
        const skeleton = new Skeleton(gameObject.skeleton.data);
        const clipper = this.clipping ? new SkeletonClipping() : undefined;
        const data = skeleton.data;
        if (this.skins.length > 0) {
            let customSkin = new Skin("custom-skin");
            for (const skinName of this.skins) {
                const skin = data.findSkin(skinName);
                if (skin == null)
                    continue;
                customSkin.addSkin(skin);
            }
            skeleton.setSkin(customSkin);
        }
        skeleton.setToSetupPose();
        const animation = this.animation != null ? data.findAnimation(this.animation) : null;
        if (animation == null) {
            skeleton.updateWorldTransform(Physics.update);
            const bounds = skeleton.getBoundsRect(clipper);
            return bounds.width == Number.NEGATIVE_INFINITY
                ? { x: 0, y: 0, width: 0, height: 0 }
                : bounds;
        }
        else {
            let minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
            animationState.clearTracks();
            animationState.setAnimationWith(0, animation, false);
            const steps = Math.max(animation.duration / this.timeStep, 1.0);
            for (let i = 0; i < steps; i++) {
                const delta = i > 0 ? this.timeStep : 0;
                animationState.update(delta);
                animationState.apply(skeleton);
                skeleton.update(delta);
                skeleton.updateWorldTransform(Physics.update);
                const bounds = skeleton.getBoundsRect(clipper);
                minX = Math.min(minX, bounds.x);
                minY = Math.min(minY, bounds.y);
                maxX = Math.max(maxX, bounds.x + bounds.width);
                maxY = Math.max(maxY, bounds.y + bounds.height);
            }
            const bounds = {
                x: minX,
                y: minY,
                width: maxX - minX,
                height: maxY - minY,
            };
            return bounds.width == Number.NEGATIVE_INFINITY
                ? { x: 0, y: 0, width: 0, height: 0 }
                : bounds;
        }
    }
}
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
export class SpineGameObject extends DepthMixin(OriginMixin(ComputedSizeMixin(FlipMixin(ScrollFactorMixin(TransformMixin(VisibleMixin(AlphaMixin(BaseSpineGameObject)))))))) {
    plugin;
    boundsProvider;
    blendMode = -1;
    skeleton;
    animationStateData;
    animationState;
    beforeUpdateWorldTransforms = () => { };
    afterUpdateWorldTransforms = () => { };
    premultipliedAlpha = false;
    constructor(scene, plugin, x, y, dataKey, atlasKey, boundsProvider = new SetupPoseBoundsProvider()) {
        super(scene, window.SPINE_GAME_OBJECT_TYPE ? window.SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE);
        this.plugin = plugin;
        this.boundsProvider = boundsProvider;
        this.setPosition(x, y);
        this.premultipliedAlpha = this.plugin.isAtlasPremultiplied(atlasKey);
        this.skeleton = this.plugin.createSkeleton(dataKey, atlasKey);
        this.animationStateData = new AnimationStateData(this.skeleton.data);
        this.animationState = new AnimationState(this.animationStateData);
        this.skeleton.updateWorldTransform(Physics.update);
        this.updateSize();
    }
    updateSize() {
        if (!this.skeleton)
            return;
        let bounds = this.boundsProvider.calculateBounds(this);
        // For some reason the TS compiler and the ComputedSize mixin don't work well together and we have
        // to cast to any.
        let self = this;
        self.width = bounds.width;
        self.height = bounds.height;
        this.displayOriginX = -bounds.x;
        this.displayOriginY = -bounds.y;
    }
    /** Converts a point from the skeleton coordinate system to the Phaser world coordinate system. */
    skeletonToPhaserWorldCoordinates(point) {
        let transform = this.getWorldTransformMatrix();
        let a = transform.a, b = transform.b, c = transform.c, d = transform.d, tx = transform.tx, ty = transform.ty;
        let x = point.x;
        let y = point.y;
        point.x = x * a + y * c + tx;
        point.y = x * b + y * d + ty;
    }
    /** Converts a point from the Phaser world coordinate system to the skeleton coordinate system. */
    phaserWorldCoordinatesToSkeleton(point) {
        let transform = this.getWorldTransformMatrix();
        transform = transform.invert();
        let a = transform.a, b = transform.b, c = transform.c, d = transform.d, tx = transform.tx, ty = transform.ty;
        let x = point.x;
        let y = point.y;
        point.x = x * a + y * c + tx;
        point.y = x * b + y * d + ty;
    }
    /** Converts a point from the Phaser world coordinate system to the bone's local coordinate system. */
    phaserWorldCoordinatesToBone(point, bone) {
        this.phaserWorldCoordinatesToSkeleton(point);
        if (bone.parent) {
            bone.parent.worldToLocal(point);
        }
        else {
            bone.worldToLocal(point);
        }
    }
    /**
     * Updates the {@link AnimationState}, applies it to the {@link Skeleton}, then updates the world transforms of all bones.
     * @param delta The time delta in milliseconds
     */
    updatePose(delta) {
        this.animationState.update(delta / 1000);
        this.animationState.apply(this.skeleton);
        this.beforeUpdateWorldTransforms(this);
        this.skeleton.update(delta / 1000);
        this.skeleton.updateWorldTransform(Physics.update);
        this.afterUpdateWorldTransforms(this);
    }
    preUpdate(time, delta) {
        if (!this.skeleton || !this.animationState)
            return;
        this.updatePose(delta);
    }
    preDestroy() {
        // FIXME tear down any event emitters
    }
    willRender(camera) {
        var GameObjectRenderMask = 0xf;
        var result = !this.skeleton || !(GameObjectRenderMask !== this.renderFlags || (this.cameraFilter !== 0 && this.cameraFilter & camera.id));
        if (!this.visible)
            result = false;
        if (!result && this.parentContainer && this.plugin.webGLRenderer) {
            var sceneRenderer = this.plugin.webGLRenderer;
            if (this.plugin.gl && this.plugin.phaserRenderer instanceof Phaser.Renderer.WebGL.WebGLRenderer && sceneRenderer.batcher.isDrawing) {
                sceneRenderer.end();
                this.plugin.phaserRenderer.pipelines.rebind();
            }
        }
        return result;
    }
    renderWebGL(renderer, src, camera, parentMatrix) {
        if (!this.skeleton || !this.animationState || !this.plugin.webGLRenderer)
            return;
        let sceneRenderer = this.plugin.webGLRenderer;
        if (renderer.newType) {
            renderer.pipelines.clear();
            sceneRenderer.begin();
        }
        camera.addToRenderList(src);
        let transform = Phaser.GameObjects.GetCalcMatrix(src, camera, parentMatrix).calc;
        let a = transform.a, b = transform.b, c = transform.c, d = transform.d, tx = transform.tx, ty = transform.ty;
        sceneRenderer.drawSkeleton(this.skeleton, this.premultipliedAlpha, -1, -1, (vertices, numVertices, stride) => {
            for (let i = 0; i < numVertices; i += stride) {
                let vx = vertices[i];
                let vy = vertices[i + 1];
                vertices[i] = vx * a + vy * c + tx;
                vertices[i + 1] = vx * b + vy * d + ty;
            }
        });
        if (!renderer.nextTypeMatch) {
            sceneRenderer.end();
            renderer.pipelines.rebind();
        }
    }
    renderCanvas(renderer, src, camera, parentMatrix) {
        if (!this.skeleton || !this.animationState || !this.plugin.canvasRenderer)
            return;
        let context = renderer.currentContext;
        let skeletonRenderer = this.plugin.canvasRenderer;
        skeletonRenderer.ctx = context;
        camera.addToRenderList(src);
        let transform = Phaser.GameObjects.GetCalcMatrix(src, camera, parentMatrix).calc;
        let skeleton = this.skeleton;
        skeleton.x = transform.tx;
        skeleton.y = transform.ty;
        skeleton.scaleX = transform.scaleX;
        skeleton.scaleY = transform.scaleY;
        let root = skeleton.getRootBone();
        root.rotation = -MathUtils.radiansToDegrees * transform.rotationNormalized;
        this.skeleton.updateWorldTransform(Physics.update);
        context.save();
        skeletonRenderer.draw(skeleton);
        context.restore();
    }
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiU3BpbmVHYW1lT2JqZWN0LmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiLi4vc3JjL1NwaW5lR2FtZU9iamVjdC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OytFQTJCK0U7QUFFL0UsT0FBTyxFQUFFLHNCQUFzQixFQUFFLE1BQU0sV0FBVyxDQUFDO0FBRW5ELE9BQU8sRUFDTixpQkFBaUIsRUFDakIsVUFBVSxFQUNWLFNBQVMsRUFDVCxpQkFBaUIsRUFDakIsY0FBYyxFQUNkLFlBQVksRUFDWixVQUFVLEVBQ1YsV0FBVyxHQUNYLE1BQU0sYUFBYSxDQUFDO0FBQ3JCLE9BQU8sRUFDTixjQUFjLEVBQ2Qsa0JBQWtCLEVBRWxCLFNBQVMsRUFDVCxPQUFPLEVBQ1AsUUFBUSxFQUNSLGdCQUFnQixFQUNoQixJQUFJLEdBRUosTUFBTSw4QkFBOEIsQ0FBQztBQUV0QyxNQUFNLG1CQUFvQixTQUFRLE1BQU0sQ0FBQyxXQUFXLENBQUMsVUFBVTtJQUM5RCxZQUFhLEtBQW1CLEVBQUUsSUFBWTtRQUM3QyxLQUFLLENBQUMsS0FBSyxFQUFFLElBQUksQ0FBQyxDQUFDO0lBQ3BCLENBQUM7Q0FDRDtBQWFELDhFQUE4RTtBQUM5RSxNQUFNLE9BQU8sdUJBQXVCO0lBSzFCO0lBSlQ7O09BRUc7SUFDSCxZQUNTLFdBQVcsS0FBSztRQUFoQixhQUFRLEdBQVIsUUFBUSxDQUFRO0lBQ3JCLENBQUM7SUFFTCxlQUFlLENBQUUsVUFBMkI7UUFDM0MsSUFBSSxDQUFDLFVBQVUsQ0FBQyxRQUFRO1lBQUUsT0FBTyxFQUFFLENBQUMsRUFBRSxDQUFDLEVBQUUsQ0FBQyxFQUFFLENBQUMsRUFBRSxLQUFLLEVBQUUsQ0FBQyxFQUFFLE1BQU0sRUFBRSxDQUFDLEVBQUUsQ0FBQztRQUNyRSw0RUFBNEU7UUFDNUUsK0VBQStFO1FBQy9FLDBCQUEwQjtRQUMxQixNQUFNLFFBQVEsR0FBRyxJQUFJLFFBQVEsQ0FBQyxVQUFV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Y0FBYztZQUN4RSxPQUFPO1FBRVIsSUFBSSxPQUFPLEdBQUcsUUFBUSxDQUFDLGNBQWMsQ0FBQztRQUN0QyxJQUFJLGdCQUFnQixHQUFHLElBQUksQ0FBQyxNQUFNLENBQUMsY0FBYyxDQUFDO1FBQ2pELGdCQUF3QixDQUFDLEdBQUcsR0FBRyxPQUFPLENBQUM7UUFFeEMsTUFBTSxDQUFDLGVBQWUsQ0FBQyxHQUFHLENBQUMsQ0FBQztRQUM1QixJQUFJLFNBQVMsR0FBRyxNQUFNLENBQUMsV0FBVyxDQUFDLGFBQWEsQ0FDL0MsR0FBRyxFQUNILE1BQU0sRUFDTixZQUFZLENBQ1osQ0FBQyxJQUFJLENBQUM7UUFDUCxJQUFJLFFBQVEsR0FBRyxJQUFJLENBQUMsUUFBUSxDQUFDO1FBQzdCLFFBQVEsQ0FBQyxDQUFDLEdBQUcsU0FBUyxDQUFDLEVBQUUsQ0FBQztRQUMxQixRQUFRLENBQUMsQ0FBQyxHQUFHLFNBQVMsQ0FBQyxFQUFFLENBQUM7UUFDMUIsUUFBUSxDQUFDLE1BQU0sR0FBRyxTQUFTLENBQUMsTUFBTSxDQUFDO1FBQ25DLFFBQVEsQ0FBQyxNQUFNLEdBQUcsU0FBUyxDQUFDLE1BQU0sQ0FBQztRQUNuQyxJQUFJLElBQUksR0FBRyxRQUFRLENBQUMsV0FBVyxFQUFHLENBQUM7UUFDbkMsSUFBSSxDQUFDLFFBQVEsR0FBRyxDQUFDLFNBQVMsQ0FBQyxnQkFBZ0IsR0FBRyxTQUFTLENBQUMsa0JBQWtCLENBQUM7UUFDM0UsSUFBSSxDQUFDLFFBQVEsQ0FBQyxvQkFBb0IsQ0FBQyxPQUFPLENBQUMsTUFBTSxDQUFDLENBQUM7UUFFbkQsT0FBTyxDQUFDLElBQUksRUFBRSxDQUFDO1FBQ2YsZ0JBQWdCLENBQUMsSUFBSSxDQUFDLFFBQVEsQ0FBQyxDQUFDO1FBQ2hDLE9BQU8sQ0FBQyxPQUFPLEVBQUUsQ0FBQztJQUNuQixDQUFDO0NBQ0QiLCJzb3VyY2VzQ29udGVudCI6WyIvKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqXG4gKiBTcGluZSBSdW50aW1lcyBMaWNlbnNlIEFncmVlbWVudFxuICogTGFzdCB1cGRhdGVkIEp1bHkgMjgsIDIwMjMuIFJlcGxhY2VzIGFsbCBwcmlvciB2ZXJzaW9ucy5cbiAqXG4gKiBDb3B5cmlnaHQgKGMpIDIwMTMtMjAyMywgRXNvdGVyaWMgU29mdHdhcmUgTExDXG4gKlxuICogSW50ZWdyYXRpb24gb2YgdGhlIFNwaW5lIFJ1bnRpbWVzIGludG8gc29mdHdhcmUgb3Igb3RoZXJ3aXNlIGNyZWF0aW5nXG4gKiBkZXJpdmF0aXZlIHdvcmtzIG9mIHRoZSBTcGluZSBSdW50aW1lcyBpcyBwZXJtaXR0ZWQgdW5kZXIgdGhlIHRlcm1zIGFuZFxuICogY29uZGl0aW9ucyBvZiBTZWN0aW9uIDIgb2YgdGhlIFNwaW5lIEVkaXRvciBMaWNlbnNlIEFncmVlbWVudDpcbiAqIGh0dHA6Ly9lc290ZXJpY3NvZnR3YXJlLmNvbS9zcGluZS1lZGl0b3ItbGljZW5zZVxuICpcbiAqIE90aGVyd2lzZSwgaXQgaXMgcGVybWl0dGVkIHRvIGludGVncmF0ZSB0aGUgU3BpbmUgUnVudGltZXMgaW50byBzb2Z0d2FyZSBvclxuICogb3RoZXJ3aXNlIGNyZWF0ZSBkZXJpdmF0aXZlIHdvcmtzIG9mIHRoZSBTcGluZSBSdW50aW1lcyAoY29sbGVjdGl2ZWx5LFxuICogXCJQcm9kdWN0c1wiKSwgcHJvdmlkZWQgdGhhdCBlYWNoIHVzZXIgb2YgdGhlIFByb2R1Y3RzIG11c3Qgb2J0YWluIHRoZWlyIG93blxuICogU3BpbmUgRWRpdG9yIGxpY2Vuc2UgYW5kIHJlZGlzdHJpYnV0aW9uIG9mIHRoZSBQcm9kdWN0cyBpbiBhbnkgZm9ybSBtdXN0XG4gKiBpbmNsdWRlIHRoaXMgbGljZW5zZSBhbmQgY29weXJpZ2h0IG5vdGljZS5cbiAqXG4gKiBUSEUgU1BJTkUgUlVOVElNRVMgQVJFIFBST1ZJREVEIEJZIEVTT1RFUklDIFNPRlRXQVJFIExMQyBcIkFTIElTXCIgQU5EIEFOWVxuICogRVhQUkVTUyBPUiBJTVBMSUVEIFdBUlJBTlRJRVMsIElOQ0xVRElORywgQlVUIE5PVCBMSU1JVEVEIFRPLCBUSEUgSU1QTElFRFxuICogV0FSUkFOVElFUyBPRiBNRVJDSEFOVEFCSUxJVFkgQU5EIEZJVE5FU1MgRk9SIEEgUEFSVElDVUxBUiBQVVJQT1NFIEFSRVxuICogRElTQ0xBSU1FRC4gSU4gTk8gRVZFTlQgU0hBTEwgRVNPVEVSSUMgU09GVFdBUkUgTExDIEJFIExJQUJMRSBGT1IgQU5ZXG4gKiBESVJFQ1QsIElORElSRUNULCBJTkNJREVOVEFMLCBTUEVDSUFMLCBFWEVNUExBUlksIE9SIENPTlNFUVVFTlRJQUwgREFNQUdFU1xuICogKElOQ0xVRElORywgQlVUIE5PVCBMSU1JVEVEIFRPLCBQUk9DVVJFTUVOVCBPRiBTVUJTVElUVVRFIEdPT0RTIE9SIFNFUlZJQ0VTLFxuICogQlVTSU5FU1MgSU5URVJSVVBUSU9OLCBPUiBMT1NTIE9GIFVTRSwgREFUQSwgT1IgUFJPRklUUykgSE9XRVZFUiBDQVVTRUQgQU5EXG4gKiBPTiBBTlkgVEhFT1JZIE9GIExJQUJJTElUWSwgV0hFVEhFUiBJTiBDT05UUkFDVCwgU1RSSUNUIExJQUJJTElUWSwgT1IgVE9SVFxuICogKElOQ0xVRElORyBORUdMSUdFTkNFIE9SIE9USEVSV0lTRSkgQVJJU0lORyBJTiBBTlkgV0FZIE9VVCBPRiBUSEUgVVNFIE9GIFRIRVxuICogU1BJTkUgUlVOVElNRVMsIEVWRU4gSUYgQURWSVNFRCBPRiBUSEUgUE9TU0lCSUxJVFkgT0YgU1VDSCBEQU1BR0UuXG4gKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKiovXG5cbmltcG9ydCB7IFNQSU5FX0dBTUVfT0JKRUNUX1RZUEUgfSBmcm9tIFwiLi9rZXlzLmpzXCI7XG5pbXBvcnQgeyBTcGluZVBsdWdpbiB9IGZyb20gXCIuL1NwaW5lUGx1Z2luLmpzXCI7XG5pbXBvcnQge1xuXHRDb21wdXRlZFNpemVNaXhpbixcblx0RGVwdGhNaXhpbixcblx0RmxpcE1peGluLFxuXHRTY3JvbGxGYWN0b3JNaXhpbixcblx0VHJhbnNmb3JtTWl4aW4sXG5cdFZpc2libGVNaXhpbixcblx0QWxwaGFNaXhpbixcblx0T3JpZ2luTWl4aW4sXG59IGZyb20gXCIuL21peGlucy5qc1wiO1xuaW1wb3J0IHtcblx0QW5pbWF0aW9uU3RhdGUsXG5cdEFuaW1hdGlvblN0YXRlRGF0YSxcblx0Qm9uZSxcblx0TWF0aFV0aWxzLFxuXHRQaHlzaWNzLFxuXHRTa2VsZXRvbixcblx0U2tlbGV0b25DbGlwcGluZyxcblx0U2tpbixcblx0VmVjdG9yMixcbn0gZnJvbSBcIkBlc290ZXJpY3NvZnR3YXJlL3NwaW5lLWNvcmVcIjtcblxuY2xhc3MgQmFzZVNwaW5lR2FtZU9iamVjdCBleHRlbmRzIFBoYXNlci5HYW1lT2JqZWN0cy5HYW1lT2JqZWN0IHtcblx0Y29uc3RydWN0b3IgKHNjZW5lOiBQaGFzZXIuU2NlbmUsIHR5cGU6IHN0cmluZykge1xuXHRcdHN1cGVyKHNjZW5lLCB0eXBlKTtcblx0fVxufVxuXG4vKiogQSBib3VuZHMgcHJvdmlkZXIgY2FsY3VsYXRlcyB0aGUgYm91bmRpbmcgYm94IGZvciBhIHNrZWxldG9uLCB3aGljaCBpcyB0aGVuIGFzc2lnbmVkIGFzIHRoZSBzaXplIG9mIHRoZSBTcGluZUdhbWVPYmplY3QuICovXG5leHBvcnQgaW50ZXJmYWNlIFNwaW5lR2FtZU9iamVjdEJvdW5kc1Byb3ZpZGVyIHtcblx0Ly8gUmV0dXJucyB0aGUgYm91bmRpbmcgYm94IGZvciB0aGUgc2tlbGV0b24sIGluIHNrZWxldG9uIHNwYWNlLlxuXHRjYWxjdWxhdGVCb3VuZHMgKGdhbWVPYmplY3Q6IFNwaW5lR2FtZU9iamVjdCk6IHtcblx0XHR4OiBudW1iZXI7XG5cdFx0eTogbnVtYmVyO1xuXHRcdHdpZHRoOiBudW1iZXI7XG5cdFx0aGVpZ2h0OiBudW1iZXI7XG5cdH07XG59XG5cbi8qKiBBIGJvdW5kcyBwcm92aWRlciB0aGF0IGNhbGN1bGF0ZXMgdGhlIGJvdW5kaW5nIGJveCBmcm9tIHRoZSBzZXR1cCBwb3NlLiAqL1xuZXhwb3J0IGNsYXNzIFNldHVwUG9zZUJvdW5kc1Byb3ZpZGVyIGltcGxlbWVudHMgU3BpbmVHYW1lT2JqZWN0Qm91bmRzUHJvdmlkZXIge1xuXHQvKipcblx0ICogQHBhcmFtIGNsaXBwaW5nIElmIHRydWUsIGNsaXBwaW5nIGF0dGFjaG1lbnRzIGFyZSB1c2VkIHRvIGNvbXB1dGUgdGhlIGJvdW5kcy4gRmFsc2UsIGJ5IGRlZmF1bHQuXG5cdCAqL1xuXHRjb25zdHJ1Y3RvciAoXG5cdFx0cHJpdmF0ZSBjbGlwcGluZyA9IGZhbHNlLFxuXHQpIHsgfVxuXG5cdGNhbGN1bGF0ZUJvdW5kcyAoZ2FtZU9iamVjdDogU3BpbmVHYW1lT2JqZWN0KSB7XG5cdFx0aWYgKCFnYW1lT2JqZWN0LnNrZWxldG9uKSByZXR1cm4geyB4OiAwLCB5OiAwLCB3aWR0aDogMCwgaGVpZ2h0OiAwIH07XG5cdFx0Ly8gTWFrZSBhIGNvcHkgb2YgYW5pbWF0aW9uIHN0YXRlIGFuZCBza2VsZXRvbiBhcyB0aGlzIG1pZ2h0IGJlIGNhbGxlZCB3aGlsZVxuXHRcdC8vIHRoZSBza2VsZXRvbiBpbiB0aGUgR2FtZU9iamVjdCBoYXMgYWxyZWFkeSBiZWVuIGhlYXZpbHkgbW9kaWZpZWQuIFdlIGNhbiBub3Rcblx0XHQvLyByZWNvbnN0cnVjdCB0aGF0IHN0YXRlLlxuXHRcdGNvbnN0IHNrZWxldG9uID0gbmV3IFNrZWxldG9uKGdhbWVPYmplY3Quc2tlbGV0b24uZGF0YSk7XG5cdFx0c2tlbGV0b24uc2V0VG9TZXR1cFBvc2UoKTtcblx0XHRza2VsZXRvbi51cGRhdGVXb3JsZFRyYW5zZm9ybShQaHlzaWNzLnVwZGF0ZSk7XG5cdFx0Y29uc3QgYm91bmRzID0gc2tlbGV0b24uZ2V0Qm91bmRzUmVjdCh0aGlzLmNsaXBwaW5nID8gbmV3IFNrZWxldG9uQ2xpcHBpbmcoKSA6IHVuZGVmaW5lZCk7XG5cdFx0cmV0dXJuIGJvdW5kcy53aWR0aCA9PSBOdW1iZXIuTkVHQVRJVkVfSU5GSU5JVFlcblx0XHRcdD8geyB4OiAwLCB5OiAwLCB3aWR0aDogMCwgaGVpZ2h0OiAwIH1cblx0XHRcdDogYm91bmRzO1xuXHR9XG59XG5cbi8qKiBBIGJvdW5kcyBwcm92aWRlciB0aGF0IGNhbGN1bGF0ZXMgdGhlIGJvdW5kaW5nIGJveCBieSB0YWtpbmcgdGhlIG1heGltdW1nIGJvdW5kaW5nIGJveCBmb3IgYSBjb21iaW5hdGlvbiBvZiBza2lucyBhbmQgc3BlY2lmaWMgYW5pbWF0aW9uLiAqL1xuZXhwb3J0IGNsYXNzIFNraW5zQW5kQW5pbWF0aW9uQm91bmRzUHJvdmlkZXJcblx0aW1wbGVtZW50cyBTcGluZUdhbWVPYmplY3RCb3VuZHNQcm92aWRlciB7XG5cdC8qKlxuXHQgKiBAcGFyYW0gYW5pbWF0aW9uIFRoZSBhbmltYXRpb24gdG8gdXNlIGZvciBjYWxjdWxhdGluZyB0aGUgYm91bmRzLiBJZiBudWxsLCB0aGUgc2V0dXAgcG9zZSBpcyB1c2VkLlxuXHQgKiBAcGFyYW0gc2tpbnMgVGhlIHNraW5zIHRvIHVzZSBmb3IgY2FsY3VsYXRpbmcgdGhlIGJvdW5kcy4gSWYgZW1wdHksIHRoZSBkZWZhdWx0IHNraW4gaXMgdXNlZC5cblx0ICogQHBhcmFtIHRpbWVTdGVwIFRoZSB0aW1lIHN0ZXAgdG8gdXNlIGZvciBjYWxjdWxhdGluZyB0aGUgYm91bmRzLiBBIHNtYWxsZXIgdGltZSBzdGVwIG1lYW5zIG1vcmUgcHJlY2lzaW9uLCBidXQgc2xvd2VyIGNhbGN1bGF0aW9uLlxuXHQgKiBAcGFyYW0gY2xpcHBpbmcgSWYgdHJ1ZSwgY2xpcHBpbmcgYXR0YWNobWVudHMgYXJlIHVzZWQgdG8gY29tcHV0ZSB0aGUgYm91bmRzLiBGYWxzZSwgYnkgZGVmYXVsdC5cblx0ICovXG5cdGNvbnN0cnVjdG9yIChcblx0XHRwcml2YXRlIGFuaW1hdGlvbjogc3RyaW5nIHwgbnVsbCxcblx0XHRwcml2YXRlIHNraW5zOiBzdHJpbmdbXSA9IFtdLFxuXHRcdHByaXZhdGUgdGltZVN0ZXA6IG51bWJlciA9IDAuMDUsXG5cdFx0cHJpdmF0ZSBjbGlwcGluZyA9IGZhbHNlLFxuXHQpIHsgfVxuXG5cdGNhbGN1bGF0ZUJvdW5kcyAoZ2FtZU9iamVjdDogU3BpbmVHYW1lT2JqZWN0KToge1xuXHRcdHg6IG51bWJlcjtcblx0XHR5OiBudW1iZXI7XG5cdFx0d2lkdGg6IG51bWJlcjtcblx0XHRoZWlnaHQ6IG51bWJlcjtcblx0fSB7XG5cdFx0aWYgKCFnYW1lT2JqZWN0LnNrZWxldG9uIHx8ICFnYW1lT2JqZWN0LmFuaW1hdGlvblN0YXRlKVxuXHRcdFx0cmV0dXJuIHsgeDogMCwgeTogMCwgd2lkdGg6IDAsIGhlaWdodDogMCB9O1xuXHRcdC8vIE1ha2UgYSBjb3B5IG9mIGFuaW1hdGlvbiBzdGF0ZSBhbmQgc2tlbGV0b24gYXMgdGhpcyBtaWdodCBiZSBjYWxsZWQgd2hpbGVcblx0XHQvLyB0aGUgc2tlbGV0b24gaW4gdGhlIEdhbWVPYmplY3QgaGFzIGFscmVhZHkgYmVlbiBoZWF2aWx5IG1vZGlmaWVkLiBXZSBjYW4gbm90XG5cdFx0Ly8gcmVjb25zdHJ1Y3QgdGhhdCBzdGF0ZS5cblx0XHRjb25zdCBhbmltYXRpb25TdGF0ZSA9IG5ldyBBbmltYXRpb25TdGF0ZShnYW1lT2JqZWN0LmFuaW1hdGlvblN0YXRlLmRhdGEpO1xuXHRcdGNvbnN0IHNrZWxldG9uID0gbmV3IFNrZWxldG9uKGdhbWVPYmplY3Quc2tlbGV0b24uZGF0YSk7XG5cdFx0Y29uc3QgY2xpcHBlciA9IHRoaXMuY2xpcHBpbmcgPyBuZXcgU2tlbGV0b25DbGlwcGluZygpIDogdW5kZWZpbmVkO1xuXHRcdGNvbnN0IGRhdGEgPSBza2VsZXRvbi5kYXRhO1xuXHRcdGlmICh0aGlzLnNraW5zLmxlbmd0aCA+IDApIHtcblx0XHRcdGxldCBjdXN0b21Ta2luID0gbmV3IFNraW4oXCJjdXN0b20tc2tpblwiKTtcblx0XHRcdGZvciAoY29uc3Qgc2tpbk5hbWUgb2YgdGhpcy5za2lucykge1xuXHRcdFx0XHRjb25zdCBza2luID0gZGF0YS5maW5kU2tpbihza2luTmFtZSk7XG5cdFx0XHRcdGlmIChza2luID09IG51bGwpIGNvbnRpbnVlO1xuXHRcdFx0XHRjdXN0b21Ta2luLmFkZFNraW4oc2tpbik7XG5cdFx0XHR9XG5cdFx0XHRza2VsZXRvbi5zZXRTa2luKGN1c3RvbVNraW4pO1xuXHRcdH1cblx0XHRza2VsZXRvbi5zZXRUb1NldHVwUG9zZSgpO1xuXG5cdFx0Y29uc3QgYW5pbWF0aW9uID1cblx0XHRcdHRoaXMuYW5pbWF0aW9uICE9IG51bGwgPyBkYXRhLmZpbmRBbmltYXRpb24odGhpcy5hbmltYXRpb24hKSA6IG51bGw7XG5cdFx0aWYgKGFuaW1hdGlvbiA9PSBudWxsKSB7XG5cdFx0XHRza2VsZXRvbi51cGRhdGVXb3JsZFRyYW5zZm9ybShQaHlzaWNzLnVwZGF0ZSk7XG5cdFx0XHRjb25zdCBib3VuZHMgPSBza2VsZXRvbi5nZXRCb3VuZHNSZWN0KGNsaXBwZXIpO1xuXHRcdFx0cmV0dXJuIGJvdW5kcy53aWR0aCA9PSBOdW1iZXIuTkVHQVRJVkVfSU5GSU5JVFlcblx0XHRcdFx0PyB7IHg6IDAsIHk6IDAsIHdpZHRoOiAwLCBoZWlnaHQ6IDAgfVxuXHRcdFx0XHQ6IGJvdW5kcztcblx0XHR9IGVsc2Uge1xuXHRcdFx0bGV0IG1pblggPSBOdW1iZXIuUE9TSVRJVkVfSU5GSU5JVFksXG5cdFx0XHRcdG1pblkgPSBOdW1iZXIuUE9TSVRJVkVfSU5GSU5JVFksXG5cdFx0XHRcdG1heFggPSBOdW1iZXIuTkVHQVRJVkVfSU5GSU5JVFksXG5cdFx0XHRcdG1heFkgPSBOdW1iZXIuTkVHQVRJVkVfSU5GSU5JVFk7XG5cdFx0XHRhbmltYXRpb25TdGF0ZS5jbGVhclRyYWNrcygpO1xuXHRcdFx0YW5pbWF0aW9uU3RhdGUuc2V0QW5pbWF0aW9uV2l0aCgwLCBhbmltYXRpb24sIGZhbHNlKTtcblx0XHRcdGNvbnN0IHN0ZXBzID0gTWF0aC5tYXgoYW5pbWF0aW9uLmR1cmF0aW9uIC8gdGhpcy50aW1lU3RlcCwgMS4wKTtcblx0XHRcdGZvciAobGV0IGkgPSAwOyBpIDwgc3RlcHM7IGkrKykge1xuXHRcdFx0XHRjb25zdCBkZWx0YSA9IGkgPiAwID8gdGhpcy50aW1lU3RlcCA6IDA7XG5cdFx0XHRcdGFuaW1hdGlvblN0YXRlLnVwZGF0ZShkZWx0YSk7XG5cdFx0XHRcdGFuaW1hdGlvblN0YXRlLmFwcGx5KHNrZWxldG9uKTtcblx0XHRcdFx0c2tlbGV0b24udXBkYXRlKGRlbHRhKTtcblx0XHRcdFx0c2tlbGV0b24udXBkYXRlV29ybGRUcmFuc2Zvcm0oUGh5c2ljcy51cGRhdGUpO1xuXG5cdFx0XHRcdGNvbnN0IGJvdW5kcyA9IHNrZWxldG9uLmdldEJvdW5kc1JlY3QoY2xpcHBlcik7XG5cdFx0XHRcdG1pblggPSBNYXRoLm1pbihtaW5YLCBib3VuZHMueCk7XG5cdFx0XHRcdG1pblkgPSBNYXRoLm1pbihtaW5ZLCBib3VuZHMueSk7XG5cdFx0XHRcdG1heFggPSBNYXRoLm1heChtYXhYLCBib3VuZHMueCArIGJvdW5kcy53aWR0aCk7XG5cdFx0XHRcdG1heFkgPSBNYXRoLm1heChtYXhZLCBib3VuZHMueSArIGJvdW5kcy5oZWlnaHQpO1xuXHRcdFx0fVxuXHRcdFx0Y29uc3QgYm91bmRzID0ge1xuXHRcdFx0XHR4OiBtaW5YLFxuXHRcdFx0XHR5OiBtaW5ZLFxuXHRcdFx0XHR3aWR0aDogbWF4WCAtIG1pblgsXG5cdFx0XHRcdGhlaWdodDogbWF4WSAtIG1pblksXG5cdFx0XHR9O1xuXHRcdFx0cmV0dXJuIGJvdW5kcy53aWR0aCA9PSBOdW1iZXIuTkVHQVRJVkVfSU5GSU5JVFlcblx0XHRcdFx0PyB7IHg6IDAsIHk6IDAsIHdpZHRoOiAwLCBoZWlnaHQ6IDAgfVxuXHRcdFx0XHQ6IGJvdW5kcztcblx0XHR9XG5cdH1cbn1cblxuLyoqXG4gKiBBIFNwaW5lR2FtZU9iamVjdCBpcyBhIFBoYXNlciB7QGxpbmsgR2FtZU9iamVjdH0gdGhhdCBjYW4gYmUgYWRkZWQgdG8gYSBQaGFzZXIgU2NlbmUgYW5kIHJlbmRlciBhIFNwaW5lIHNrZWxldG9uLlxuICpcbiAqIFRoZSBTcGluZSBHYW1lT2JqZWN0IGlzIGEgdGhpbiB3cmFwcGVyIGFyb3VuZCBhIFNwaW5lIHtAbGluayBTa2VsZXRvbn0sIHtAbGluayBBbmltYXRpb25TdGF0ZX0gYW5kIHtAbGluayBBbmltYXRpb25TdGF0ZURhdGF9LiBJdCBpcyByZXNwb25zaWJsZSBmb3I6XG4gKiAtIHVwZGF0aW5nIHRoZSBhbmltYXRpb24gc3RhdGVcbiAqIC0gYXBwbHlpbmcgdGhlIGFuaW1hdGlvbiBzdGF0ZSB0byB0aGUgc2tlbGV0b24ncyBib25lcywgc2xvdHMsIGF0dGFjaG1lbnRzLCBhbmQgZHJhdyBvcmRlci5cbiAqIC0gdXBkYXRpbmcgdGhlIHNrZWxldG9uJ3MgYm9uZSB3b3JsZCB0cmFuc2Zvcm1zXG4gKiAtIHJlbmRlcmluZyB0aGUgc2tlbGV0b25cbiAqXG4gKiBTZWUgdGhlIHtAbGluayBTcGluZVBsdWdpbn0gY2xhc3MgZm9yIG1vcmUgaW5mb3JtYXRpb24gb24gaG93IHRvIGNyZWF0ZSBhIGBTcGluZUdhbWVPYmplY3RgLlxuICpcbiAqIFRoZSBza2VsZXRvbiwgYW5pbWF0aW9uIHN0YXRlLCBhbmQgYW5pbWF0aW9uIHN0YXRlIGRhdGEgY2FuIGJlIGFjY2Vzc2VkIHZpYSB0aGUgcmVwc2VjdGl2ZSBmaWVsZHMuIFRoZXkgY2FuIGJlIG1hbnVhbGx5IHVwZGF0ZWQgdmlhIHtAbGluayB1cGRhdGVQb3NlfS5cbiAqXG4gKiBUbyBtb2RpZnkgdGhlIGJvbmUgaGllcmFyY2h5IGJlZm9yZSB0aGUgd29ybGQgdHJhbnNmb3JtcyBhcmUgY29tcHV0ZWQsIGEgY2FsbGJhY2sgY2FuIGJlIHNldCB2aWEgdGhlIHtAbGluayBiZWZvcmVVcGRhdGVXb3JsZFRyYW5zZm9ybXN9IGZpZWxkLlxuICpcbiAqIFRvIG1vZGlmeSB0aGUgYm9uZSBoaWVyYXJjaHkgYWZ0ZXIgdGhlIHdvcmxkIHRyYW5zZm9ybXMgYXJlIGNvbXB1dGVkLCBhIGNhbGxiYWNrIGNhbiBiZSBzZXQgdmlhIHRoZSB7QGxpbmsgYWZ0ZXJVcGRhdGVXb3JsZFRyYW5zZm9ybXN9IGZpZWxkLlxuICpcbiAqIFRoZSBjbGFzcyBhbHNvIGZlYXR1cmVzIG1ldGhvZHMgdG8gY29udmVydCBiZXR3ZWVuIHRoZSBza2VsZXRvbiBjb29yZGluYXRlIHN5c3RlbSBhbmQgdGhlIFBoYXNlciBjb29yZGluYXRlIHN5c3RlbS5cbiAqXG4gKiBTZWUge0BsaW5rIHNrZWxldG9uVG9QaGFzZXJXb3JsZENvb3JkaW5hdGVzfSwge0BsaW5rIHBoYXNlcldvcmxkQ29vcmRpbmF0ZXNUb1NrZWxldG9ufSwgYW5kIHtAbGluayBwaGFzZXJXb3JsZENvb3JkaW5hdGVzVG9Cb25lTG9jYWwufVxuICovXG5leHBvcnQgY2xhc3MgU3BpbmVHYW1lT2JqZWN0IGV4dGVuZHMgRGVwdGhNaXhpbihcblx0T3JpZ2luTWl4aW4oXG5cdFx0Q29tcHV0ZWRTaXplTWl4aW4oXG5cdFx0XHRGbGlwTWl4aW4oXG5cdFx0XHRcdFNjcm9sbEZhY3Rvck1peGluKFxuXHRcdFx0XHRcdFRyYW5zZm9ybU1peGluKFZpc2libGVNaXhpbihBbHBoYU1peGluKEJhc2VTcGluZUdhbWVPYmplY3QpKSlcblx0XHRcdFx0KVxuXHRcdFx0KVxuXHRcdClcblx0KVxuKSB7XG5cdGJsZW5kTW9kZSA9IC0xO1xuXHRza2VsZXRvbjogU2tlbGV0b247XG5cdGFuaW1hdGlvblN0YXRlRGF0YTogQW5pbWF0aW9uU3RhdGVEYXRhO1xuXHRhbmltYXRpb25TdGF0ZTogQW5pbWF0aW9uU3RhdGU7XG5cdGJlZm9yZVVwZGF0ZVdvcmxkVHJhbnNmb3JtczogKG9iamVjdDogU3BpbmVHYW1lT2JqZWN0KSA9PiB2b2lkID0gKCkgPT4geyB9O1xuXHRhZnRlclVwZGF0ZVdvcmxkVHJhbnNmb3JtczogKG9iamVjdDogU3BpbmVHYW1lT2JqZWN0KSA9PiB2b2lkID0gKCkgPT4geyB9O1xuXHRwcml2YXRlIHByZW11bHRpcGxpZWRBbHBoYSA9IGZhbHNlO1xuXG5cdGNvbnN0cnVjdG9yIChcblx0XHRzY2VuZTogUGhhc2VyLlNjZW5lLFxuXHRcdHByaXZhdGUgcGx1Z2luOiBTcGluZVBsdWdpbixcblx0XHR4OiBudW1iZXIsXG5cdFx0eTogbnVtYmVyLFxuXHRcdGRhdGFLZXk6IHN0cmluZyxcblx0XHRhdGxhc0tleTogc3RyaW5nLFxuXHRcdHB1YmxpYyBib3VuZHNQcm92aWRlcjogU3BpbmVHYW1lT2JqZWN0Qm91bmRzUHJvdmlkZXIgPSBuZXcgU2V0dXBQb3NlQm91bmRzUHJvdmlkZXIoKVxuXHQpIHtcblx0XHRzdXBlcihzY2VuZSwgKHdpbmRvdyBhcyBhbnkpLlNQSU5FX0dBTUVfT0JKRUNUX1RZUEUgPyAod2luZG93IGFzIGFueSkuU1BJTkVfR0FNRV9PQkpFQ1RfVFlQRSA6IFNQSU5FX0dBTUVfT0JKRUNUX1RZUEUpO1xuXHRcdHRoaXMuc2V0UG9zaXRpb24oeCwgeSk7XG5cblx0XHR0aGlzLnByZW11bHRpcGxpZWRBbHBoYSA9IHRoaXMucGx1Z2luLmlzQXRsYXNQcmVtdWx0aXBsaWVkKGF0bGFzS2V5KTtcblx0XHR0aGlzLnNrZWxldG9uID0gdGhpcy5wbHVnaW4uY3JlYXRlU2tlbGV0b24oZGF0YUtleSwgYXRsYXNLZXkpO1xuXHRcdHRoaXMuYW5pbWF0aW9uU3RhdGVEYXRhID0gbmV3IEFuaW1hdGlvblN0YXRlRGF0YSh0aGlzLnNrZWxldG9uLmRhdGEpO1xuXHRcdHRoaXMuYW5pbWF0aW9uU3RhdGUgPSBuZXcgQW5pbWF0aW9uU3RhdGUodGhpcy5hbmltYXRpb25TdGF0ZURhdGEpO1xuXHRcdHRoaXMuc2tlbGV0b24udXBkYXRlV29ybGRUcmFuc2Zvcm0oUGh5c2ljcy51cGRhdGUpO1xuXHRcdHRoaXMudXBkYXRlU2l6ZSgpO1xuXHR9XG5cblx0dXBkYXRlU2l6ZSAoKSB7XG5cdFx0aWYgKCF0aGlzLnNrZWxldG9uKSByZXR1cm47XG5cdFx0bGV0IGJvdW5kcyA9IHRoaXMuYm91bmRzUHJvdmlkZXIuY2FsY3VsYXRlQm91bmRzKHRoaXMpO1xuXHRcdC8vIEZvciBzb21lIHJlYXNvbiB0aGUgVFMgY29tcGlsZXIgYW5kIHRoZSBDb21wdXRlZFNpemUgbWl4aW4gZG9uJ3Qgd29yayB3ZWxsIHRvZ2V0aGVyIGFuZCB3ZSBoYXZlXG5cdFx0Ly8gdG8gY2FzdCB0byBhbnkuXG5cdFx0bGV0IHNlbGYgPSB0aGlzIGFzIGFueTtcblx0XHRzZWxmLndpZHRoID0gYm91bmRzLndpZHRoO1xuXHRcdHNlbGYuaGVpZ2h0ID0gYm91bmRzLmhlaWdodDtcblx0XHR0aGlzLmRpc3BsYXlPcmlnaW5YID0gLWJvdW5kcy54O1xuXHRcdHRoaXMuZGlzcGxheU9yaWdpblkgPSAtYm91bmRzLnk7XG5cdH1cblxuXHQvKiogQ29udmVydHMgYSBwb2ludCBmcm9tIHRoZSBza2VsZXRvbiBjb29yZGluYXRlIHN5c3RlbSB0byB0aGUgUGhhc2VyIHdvcmxkIGNvb3JkaW5hdGUgc3lzdGVtLiAqL1xuXHRza2VsZXRvblRvUGhhc2VyV29ybGRDb29yZGluYXRlcyAocG9pbnQ6IHsgeDogbnVtYmVyOyB5OiBudW1iZXIgfSkge1xuXHRcdGxldCB0cmFuc2Zvcm0gPSB0aGlzLmdldFdvcmxkVHJhbnNmb3JtTWF0cml4KCk7XG5cdFx0bGV0IGEgPSB0cmFuc2Zvcm0uYSxcblx0XHRcdGIgPSB0cmFuc2Zvcm0uYixcblx0XHRcdGMgPSB0cmFuc2Zvcm0uYyxcblx0XHRcdGQgPSB0cmFuc2Zvcm0uZCxcblx0XHRcdHR4ID0gdHJhbnNmb3JtLnR4LFxuXHRcdFx0dHkgPSB0cmFuc2Zvcm0udHk7XG5cdFx0bGV0IHggPSBwb2ludC54O1xuXHRcdGxldCB5ID0gcG9pbnQueTtcblx0XHRwb2ludC54ID0geCAqIGEgKyB5ICogYyArIHR4O1xuXHRcdHBvaW50LnkgPSB4ICogYiArIHkgKiBkICsgdHk7XG5cdH1cblxuXHQvKiogQ29udmVydHMgYSBwb2ludCBmcm9tIHRoZSBQaGFzZXIgd29ybGQgY29vcmRpbmF0ZSBzeXN0ZW0gdG8gdGhlIHNrZWxldG9uIGNvb3JkaW5hdGUgc3lzdGVtLiAqL1xuXHRwaGFzZXJXb3JsZENvb3JkaW5hdGVzVG9Ta2VsZXRvbiAocG9pbnQ6IHsgeDogbnVtYmVyOyB5OiBudW1iZXIgfSkge1xuXHRcdGxldCB0cmFuc2Zvcm0gPSB0aGlzLmdldFdvcmxkVHJhbnNmb3JtTWF0cml4KCk7XG5cdFx0dHJhbnNmb3JtID0gdHJhbnNmb3JtLmludmVydCgpO1xuXHRcdGxldCBhID0gdHJhbnNmb3JtLmEsXG5cdFx0XHRiID0gdHJhbnNmb3JtLmIsXG5cdFx0XHRjID0gdHJhbnNmb3JtLmMsXG5cdFx0XHRkID0gdHJhbnNmb3JtLmQsXG5cdFx0XHR0eCA9IHRyYW5zZm9ybS50eCxcblx0XHRcdHR5ID0gdHJhbnNmb3JtLnR5O1xuXHRcdGxldCB4ID0gcG9pbnQueDtcblx0XHRsZXQgeSA9IHBvaW50Lnk7XG5cdFx0cG9pbnQueCA9IHggKiBhICsgeSAqIGMgKyB0eDtcblx0XHRwb2ludC55ID0geCAqIGIgKyB5ICogZCArIHR5O1xuXHR9XG5cblx0LyoqIENvbnZlcnRzIGEgcG9pbnQgZnJvbSB0aGUgUGhhc2VyIHdvcmxkIGNvb3JkaW5hdGUgc3lzdGVtIHRvIHRoZSBib25lJ3MgbG9jYWwgY29vcmRpbmF0ZSBzeXN0ZW0uICovXG5cdHBoYXNlcldvcmxkQ29vcmRpbmF0ZXNUb0JvbmUgKHBvaW50OiB7IHg6IG51bWJlcjsgeTogbnVtYmVyIH0sIGJvbmU6IEJvbmUpIHtcblx0XHR0aGlzLnBoYXNlcldvcmxkQ29vcmRpbmF0ZXNUb1NrZWxldG9uKHBvaW50KTtcblx0XHRpZiAoYm9uZS5wYXJlbnQpIHtcblx0XHRcdGJvbmUucGFyZW50LndvcmxkVG9Mb2NhbChwb2ludCBhcyBWZWN0b3IyKTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0Ym9uZS53b3JsZFRvTG9jYWwocG9pbnQgYXMgVmVjdG9yMik7XG5cdFx0fVxuXHR9XG5cblx0LyoqXG5cdCAqIFVwZGF0ZXMgdGhlIHtAbGluayBBbmltYXRpb25TdGF0ZX0sIGFwcGxpZXMgaXQgdG8gdGhlIHtAbGluayBTa2VsZXRvbn0sIHRoZW4gdXBkYXRlcyB0aGUgd29ybGQgdHJhbnNmb3JtcyBvZiBhbGwgYm9uZXMuXG5cdCAqIEBwYXJhbSBkZWx0YSBUaGUgdGltZSBkZWx0YSBpbiBtaWxsaXNlY29uZHNcblx0ICovXG5cdHVwZGF0ZVBvc2UgKGRlbHRhOiBudW1iZXIpIHtcblx0XHR0aGlzLmFuaW1hdGlvblN0YXRlLnVwZGF0ZShkZWx0YSAvIDEwMDApO1xuXHRcdHRoaXMuYW5pbWF0aW9uU3RhdGUuYXBwbHkodGhpcy5za2VsZXRvbik7XG5cdFx0dGhpcy5iZWZvcmVVcGRhdGVXb3JsZFRyYW5zZm9ybXModGhpcyk7XG5cdFx0dGhpcy5za2VsZXRvbi51cGRhdGUoZGVsdGEgLyAxMDAwKTtcblx0XHR0aGlzLnNrZWxldG9uLnVwZGF0ZVdvcmxkVHJhbnNmb3JtKFBoeXNpY3MudXBkYXRlKTtcblx0XHR0aGlzLmFmdGVyVXBkYXRlV29ybGRUcmFuc2Zvcm1zKHRoaXMpO1xuXHR9XG5cblx0cHJlVXBkYXRlICh0aW1lOiBudW1iZXIsIGRlbHRhOiBudW1iZXIpIHtcblx0XHRpZiAoIXRoaXMuc2tlbGV0b24gfHwgIXRoaXMuYW5pbWF0aW9uU3RhdGUpIHJldHVybjtcblx0XHR0aGlzLnVwZGF0ZVBvc2UoZGVsdGEpO1xuXHR9XG5cblx0cHJlRGVzdHJveSAoKSB7XG5cdFx0Ly8gRklYTUUgdGVhciBkb3duIGFueSBldmVudCBlbWl0dGVyc1xuXHR9XG5cblx0d2lsbFJlbmRlciAoY2FtZXJhOiBQaGFzZXIuQ2FtZXJhcy5TY2VuZTJELkNhbWVyYSkge1xuXHRcdHZhciBHYW1lT2JqZWN0UmVuZGVyTWFzayA9IDB4Zjtcblx0XHR2YXIgcmVzdWx0ID0gIXRoaXMuc2tlbGV0b24gfHwgIShHYW1lT2JqZWN0UmVuZGVyTWFzayAhPT0gdGhpcy5yZW5kZXJGbGFncyB8fCAodGhpcy5jYW1lcmFGaWx0ZXIgIT09IDAgJiYgdGhpcy5jYW1lcmFGaWx0ZXIgJiBjYW1lcmEuaWQpKTtcblx0XHRpZiAoIXRoaXMudmlzaWJsZSkgcmVzdWx0ID0gZmFsc2U7XG5cblx0XHRpZiAoIXJlc3VsdCAmJiB0aGlzLnBhcmVudENvbnRhaW5lciAmJiB0aGlzLnBsdWdpbi53ZWJHTFJlbmRlcmVyKSB7XG5cdFx0XHR2YXIgc2NlbmVSZW5kZXJlciA9IHRoaXMucGx1Z2luLndlYkdMUmVuZGVyZXI7XG5cblx0XHRcdGlmICh0aGlzLnBsdWdpbi5nbCAmJiB0aGlzLnBsdWdpbi5waGFzZXJSZW5kZXJlciBpbnN0YW5jZW9mIFBoYXNlci5SZW5kZXJlci5XZWJHTC5XZWJHTFJlbmRlcmVyICYmIHNjZW5lUmVuZGVyZXIuYmF0Y2hlci5pc0RyYXdpbmcpIHtcblx0XHRcdFx0c2NlbmVSZW5kZXJlci5lbmQoKTtcblx0XHRcdFx0dGhpcy5wbHVnaW4ucGhhc2VyUmVuZGVyZXIucGlwZWxpbmVzLnJlYmluZCgpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHJldHVybiByZXN1bHQ7XG5cdH1cblxuXHRyZW5kZXJXZWJHTCAoXG5cdFx0cmVuZGVyZXI6IFBoYXNlci5SZW5kZXJlci5XZWJHTC5XZWJHTFJlbmRlcmVyLFxuXHRcdHNyYzogU3BpbmVHYW1lT2JqZWN0LFxuXHRcdGNhbWVyYTogUGhhc2VyLkNhbWVyYXMuU2NlbmUyRC5DYW1lcmEsXG5cdFx0cGFyZW50TWF0cml4OiBQaGFzZXIuR2FtZU9iamVjdHMuQ29tcG9uZW50cy5UcmFuc2Zvcm1NYXRyaXhcblx0KSB7XG5cdFx0aWYgKCF0aGlzLnNrZWxldG9uIHx8ICF0aGlzLmFuaW1hdGlvblN0YXRlIHx8ICF0aGlzLnBsdWdpbi53ZWJHTFJlbmRlcmVyKVxuXHRcdFx0cmV0dXJuO1xuXG5cdFx0bGV0IHNjZW5lUmVuZGVyZXIgPSB0aGlzLnBsdWdpbi53ZWJHTFJlbmRlcmVyO1xuXHRcdGlmIChyZW5kZXJlci5uZXdUeXBlKSB7XG5cdFx0XHRyZW5kZXJlci5waXBlbGluZXMuY2xlYXIoKTtcblx0XHRcdHNjZW5lUmVuZGVyZXIuYmVnaW4oKTtcblx0XHR9XG5cblx0XHRjYW1lcmEuYWRkVG9SZW5kZXJMaXN0KHNyYyk7XG5cdFx0bGV0IHRyYW5zZm9ybSA9IFBoYXNlci5HYW1lT2JqZWN0cy5HZXRDYWxjTWF0cml4KFxuXHRcdFx0c3JjLFxuXHRcdFx0Y2FtZXJhLFxuXHRcdFx0cGFyZW50TWF0cml4XG5cdFx0KS5jYWxjO1xuXHRcdGxldCBhID0gdHJhbnNmb3JtLmEsXG5cdFx0XHRiID0gdHJhbnNmb3JtLmIsXG5cdFx0XHRjID0gdHJhbnNmb3JtLmMsXG5cdFx0XHRkID0gdHJhbnNmb3JtLmQsXG5cdFx0XHR0eCA9IHRyYW5zZm9ybS50eCxcblx0XHRcdHR5ID0gdHJhbnNmb3JtLnR5O1xuXHRcdHNjZW5lUmVuZGVyZXIuZHJhd1NrZWxldG9uKFxuXHRcdFx0dGhpcy5za2VsZXRvbixcblx0XHRcdHRoaXMucHJlbXVsdGlwbGllZEFscGhhLFxuXHRcdFx0LTEsXG5cdFx0XHQtMSxcblx0XHRcdCh2ZXJ0aWNlcywgbnVtVmVydGljZXMsIHN0cmlkZSkgPT4ge1xuXHRcdFx0XHRmb3IgKGxldCBpID0gMDsgaSA8IG51bVZlcnRpY2VzOyBpICs9IHN0cmlkZSkge1xuXHRcdFx0XHRcdGxldCB2eCA9IHZlcnRpY2VzW2ldO1xuXHRcdFx0XHRcdGxldCB2eSA9IHZlcnRpY2VzW2kgKyAxXTtcblx0XHRcdFx0XHR2ZXJ0aWNlc1tpXSA9IHZ4ICogYSArIHZ5ICogYyArIHR4O1xuXHRcdFx0XHRcdHZlcnRpY2VzW2kgKyAxXSA9IHZ4ICogYiArIHZ5ICogZCArIHR5O1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0KTtcblxuXHRcdGlmICghcmVuZGVyZXIubmV4dFR5cGVNYXRjaCkge1xuXHRcdFx0c2NlbmVSZW5kZXJlci5lbmQoKTtcblx0XHRcdHJlbmRlcmVyLnBpcGVsaW5lcy5yZWJpbmQoKTtcblx0XHR9XG5cdH1cblxuXHRyZW5kZXJDYW52YXMgKFxuXHRcdHJlbmRlcmVyOiBQaGFzZXIuUmVuZGVyZXIuQ2FudmFzLkNhbnZhc1JlbmRlcmVyLFxuXHRcdHNyYzogU3BpbmVHYW1lT2JqZWN0LFxuXHRcdGNhbWVyYTogUGhhc2VyLkNhbWVyYXMuU2NlbmUyRC5DYW1lcmEsXG5cdFx0cGFyZW50TWF0cml4OiBQaGFzZXIuR2FtZU9iamVjdHMuQ29tcG9uZW50cy5UcmFuc2Zvcm1NYXRyaXhcblx0KSB7XG5cdFx0aWYgKCF0aGlzLnNrZWxldG9uIHx8ICF0aGlzLmFuaW1hdGlvblN0YXRlIHx8ICF0aGlzLnBsdWdpbi5jYW52YXNSZW5kZXJlcilcblx0XHRcdHJldHVybjtcblxuXHRcdGxldCBjb250ZXh0ID0gcmVuZGVyZXIuY3VycmVudENvbnRleHQ7XG5cdFx0bGV0IHNrZWxldG9uUmVuZGVyZXIgPSB0aGlzLnBsdWdpbi5jYW52YXNSZW5kZXJlcjtcblx0XHQoc2tlbGV0b25SZW5kZXJlciBhcyBhbnkpLmN0eCA9IGNvbnRleHQ7XG5cblx0XHRjYW1lcmEuYWRkVG9SZW5kZXJMaXN0KHNyYyk7XG5cdFx0bGV0IHRyYW5zZm9ybSA9IFBoYXNlci5HYW1lT2JqZWN0cy5HZXRDYWxjTWF0cml4KFxuXHRcdFx0c3JjLFxuXHRcdFx0Y2FtZXJhLFxuXHRcdFx0cGFyZW50TWF0cml4XG5cdFx0KS5jYWxjO1xuXHRcdGxldCBza2VsZXRvbiA9IHRoaXMuc2tlbGV0b247XG5cdFx0c2tlbGV0b24ueCA9IHRyYW5zZm9ybS50eDtcblx0XHRza2VsZXRvbi55ID0gdHJhbnNmb3JtLnR5O1xuXHRcdHNrZWxldG9uLnNjYWxlWCA9IHRyYW5zZm9ybS5zY2FsZVg7XG5cdFx0c2tlbGV0b24uc2NhbGVZID0gdHJhbnNmb3JtLnNjYWxlWTtcblx0XHRsZXQgcm9vdCA9IHNrZWxldG9uLmdldFJvb3RCb25lKCkhO1xuXHRcdHJvb3Qucm90YXRpb24gPSAtTWF0aFV0aWxzLnJhZGlhbnNUb0RlZ3JlZXMgKiB0cmFuc2Zvcm0ucm90YXRpb25Ob3JtYWxpemVkO1xuXHRcdHRoaXMuc2tlbGV0b24udXBkYXRlV29ybGRUcmFuc2Zvcm0oUGh5c2ljcy51cGRhdGUpO1xuXG5cdFx0Y29udGV4dC5zYXZlKCk7XG5cdFx0c2tlbGV0b25SZW5kZXJlci5kcmF3KHNrZWxldG9uKTtcblx0XHRjb250ZXh0LnJlc3RvcmUoKTtcblx0fVxufVxuIl19