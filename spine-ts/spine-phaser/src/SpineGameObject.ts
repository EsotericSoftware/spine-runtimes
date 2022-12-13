import { SPINE_GAME_OBJECT_TYPE } from "./keys";
import { SpinePlugin } from "./SpinePlugin";
import { ComputedSizeMixin, DepthMixin, FlipMixin, ScrollFactorMixin, TransformMixin, VisibleMixin } from "./mixins";
import { AnimationState, AnimationStateData, MathUtils, Skeleton } from "@esotericsoftware/spine-core";
import { Matrix4, Vector3 } from "@esotericsoftware/spine-webgl";

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
	private premultipliedAlpha = false;

	constructor (scene: Phaser.Scene, private plugin: SpinePlugin, x: number, y: number, dataKey: string, atlasKey: string) {
		super(scene, SPINE_GAME_OBJECT_TYPE);
		this.setPosition(x, y);
		this.setSkeleton(dataKey, atlasKey);
	}

	setSkeleton (dataKey: string, atlasKey: string) {
		if (dataKey && atlasKey) {
			this.premultipliedAlpha = this.plugin.isAtlasPremultiplied(atlasKey);
			this.skeleton = this.plugin.createSkeleton(dataKey, atlasKey);
			this.animationState = new AnimationState(new AnimationStateData(this.skeleton.data));
		} else {
			this.skeleton = null;
			this.animationState = null;
		}
	}

	preUpdate (time: number, delta: number) {
		if (!this.skeleton || !this.animationState) return;

		this.animationState.update(delta / 1000);
		this.animationState.apply(this.skeleton);
		this.skeleton.updateWorldTransform();
	}

	preDestroy () {
		this.skeleton = null;
		this.animationState = null;
		// FIXME tear down any event emitters
	}

	willRender (camera: Phaser.Cameras.Scene2D.Camera) {
		// FIXME
		return true;
	}

	renderWebGL (renderer: Phaser.Renderer.WebGL.WebGLRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix) {
		if (!this.skeleton || !this.animationState || !this.plugin.webGLRenderer) return;

		let sceneRenderer = this.plugin.webGLRenderer;
		if (renderer.newType) {
			renderer.pipelines.clear();
			sceneRenderer.begin();
		}

		camera.addToRenderList(src);
		let transform = Phaser.GameObjects.GetCalcMatrix(src, camera, parentMatrix).calc;
		let x = transform.tx;
		let y = transform.ty;
		let scaleX = transform.scaleX;
		let scaleY = transform.scaleY;
		let rotation = transform.rotationNormalized;
		let cosRotation = Math.cos(rotation);
		let sinRotation = Math.sin(rotation);

		sceneRenderer.drawSkeleton(this.skeleton, this.premultipliedAlpha, -1, -1, (vertices, numVertices, stride) => {
			for (let i = 0; i < numVertices; i += stride) {
				let vx = vertices[i];
				let vy = vertices[i + 1];
				let vxOld = vx * scaleX, vyOld = vy * scaleY;
				vx = vxOld * cosRotation - vyOld * sinRotation;
				vy = vxOld * sinRotation + vyOld * cosRotation;
				vx += x;
				vy += y;
				vertices[i] = vx;
				vertices[i + 1] = vy;
			}
		});

		if (!renderer.nextTypeMatch) {
			sceneRenderer.end();
			renderer.pipelines.rebind();
			console.log("Draw calls: " + sceneRenderer.batcher.getDrawCalls());
		}
	}

	renderCanvas (renderer: Phaser.Renderer.Canvas.CanvasRenderer, src: SpineGameObject, camera: Phaser.Cameras.Scene2D.Camera, parentMatrix: Phaser.GameObjects.Components.TransformMatrix) {
		if (!this.skeleton || !this.animationState || !this.plugin.canvasRenderer) return;

		let context = renderer.currentContext;
		let skeletonRenderer = this.plugin.canvasRenderer;
		(skeletonRenderer as any).ctx = context;

		camera.addToRenderList(src);
		let transform = Phaser.GameObjects.GetCalcMatrix(src, camera, parentMatrix).calc;
		let skeleton = this.skeleton;
		skeleton.x = transform.tx;
		skeleton.y = transform.ty;
		skeleton.scaleX = transform.scaleX;
		skeleton.scaleY = transform.scaleY;
		let root = skeleton.getRootBone()!;
		root.rotation = -MathUtils.radiansToDegrees * transform.rotationNormalized;
		this.skeleton.updateWorldTransform();

		context.save();
		skeletonRenderer.draw(skeleton);
		context.restore();
	}
}