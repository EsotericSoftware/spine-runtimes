import { SPINE_GAME_OBJECT_TYPE } from "./keys";
import { SpinePlugin } from "./SpinePlugin";
import { ComputedSizeMixin, DepthMixin, FlipMixin, ScrollFactorMixin, TransformMixin, VisibleMixin } from "./mixins";

class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
    constructor(scene: Phaser.Scene, type: string) {
        super(scene, type);
    }
}

interface SpineContainer {

}

export class SpineGameObject extends ComputedSizeMixin(DepthMixin(FlipMixin(ScrollFactorMixin(TransformMixin(VisibleMixin(BaseSpineGameObject)))))) {
    blendMode = -1;

    constructor(scene: Phaser.Scene, plugin: SpinePlugin, x: number, y: number, key: string) {
        super(scene, SPINE_GAME_OBJECT_TYPE);
        this.setPosition(x, y);
    }

    preUpdate(time: number, delta: number) {
    }

    renderWebGL(renderer: Phaser.Renderer.WebGL.WebGLRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix, container: SpineContainer) {

    }
}