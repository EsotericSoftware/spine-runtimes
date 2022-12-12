import { SPINE_GAME_OBJECT_TYPE } from "./keys";
import { SpinePlugin } from "./SpinePlugin";
import { ComputedSizeMixin, DepthMixin, FlipMixin, ScrollFactorMixin, TransformMixin, VisibleMixin } from "./mixins";
import { AnimationState, AnimationStateData, Skeleton } from "@esotericsoftware/spine-core";

class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
	constructor (scene: Phaser.Scene, type: string) {
		super(scene, type);
	}
}

interface SpineContainer {

}

export class SpineGameObject extends ComputedSizeMixin(DepthMixin(FlipMixin(ScrollFactorMixin(TransformMixin(VisibleMixin(BaseSpineGameObject)))))) {
	blendMode = -1;
	skeleton: Skeleton | null = null;
	animationState: AnimationState | null = null;

	constructor (scene: Phaser.Scene, private plugin: SpinePlugin, x: number, y: number, dataKey: string, atlasKey: string) {
		super(scene, SPINE_GAME_OBJECT_TYPE);
		this.setPosition(x, y);
		this.setSkeleton(dataKey, atlasKey);
	}

	setSkeleton (dataKey: string, atlasKey: string) {
		if (dataKey && atlasKey) {
			this.skeleton = this.plugin.createSkeleton(dataKey, atlasKey);
			this.animationState = new AnimationState(new AnimationStateData(this.skeleton.data));
		} else {
			this.skeleton = null;
		}
	}

	preUpdate (time: number, delta: number) {
	}

	renderWebGL (renderer: Phaser.Renderer.WebGL.WebGLRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix, container: SpineContainer) {

	}

	renderCanvas (renderer: Phaser.Renderer.Canvas.CanvasRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix) {

	}
}