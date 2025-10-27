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

export * from "@esotericsoftware/spine-core";

import {
	AnimationState,
	AnimationStateData,
	AtlasAttachmentLoader,
	BlendMode,
	Physics,
	Skeleton,
	SkeletonBinary,
	type SkeletonData,
	SkeletonJson,
	SkeletonRendererCore,
	Texture,
	TextureAtlas,
} from "@esotericsoftware/spine-core";

import type { Canvas, CanvasKit, Image, Paint, Shader } from "canvaskit-wasm";

Skeleton.yDown = true;

type CanvasKitImage = {
	shaders: Shader[];
	paintPerBlendMode: Map<BlendMode, Paint>;
	image: Image;
};

// CanvasKit blend modes for premultiplied alpha
function toCkBlendMode (ck: CanvasKit, blendMode: BlendMode) {
	switch (blendMode) {
		case BlendMode.Normal:
			return ck.BlendMode.SrcOver;
		case BlendMode.Additive:
			return ck.BlendMode.Plus;
		case BlendMode.Multiply:
			return ck.BlendMode.SrcOver;
		case BlendMode.Screen:
			return ck.BlendMode.Screen;
		default:
			return ck.BlendMode.SrcOver;
	}
}

function bufferToUtf8String (buffer: ArrayBuffer | Buffer) {
	if (typeof Buffer !== "undefined") {
		return buffer.toString("utf-8");
	} else if (typeof TextDecoder !== "undefined") {
		return new TextDecoder("utf-8").decode(buffer);
	} else {
		throw new Error("Unsupported environment");
	}
}

class CanvasKitTexture extends Texture {
	getImage (): CanvasKitImage {
		return this._image;
	}

	setFilters (): void { }

	setWraps (): void { }

	dispose (): void {
		const data: CanvasKitImage = this._image;
		for (const paint of data.paintPerBlendMode.values()) {
			paint.delete();
		}
		for (const shader of data.shaders) {
			shader.delete();
		}
		data.image.delete();
		this._image = null;
	}

	static async fromFile (
		ck: CanvasKit,
		path: string,
		readFile: (path: string) => Promise<ArrayBuffer | Buffer>
	): Promise<CanvasKitTexture> {
		const imgData = await readFile(path);
		if (!imgData) throw new Error(`Could not load image ${path}`);
		const image = ck.MakeImageFromEncoded(imgData);
		if (!image) throw new Error(`Could not load image ${path}`);
		const paintPerBlendMode = new Map<BlendMode, Paint>();
		const shaders: Shader[] = [];
		for (const blendMode of [
			BlendMode.Normal,
			BlendMode.Additive,
			BlendMode.Multiply,
			BlendMode.Screen,
		]) {
			const paint = new ck.Paint();
			const shader = image.makeShaderOptions(
				ck.TileMode.Clamp,
				ck.TileMode.Clamp,
				ck.FilterMode.Linear,
				ck.MipmapMode.Linear
			);
			paint.setShader(shader);
			paint.setBlendMode(toCkBlendMode(ck, blendMode));
			paintPerBlendMode.set(blendMode, paint);
			shaders.push(shader);
		}
		return new CanvasKitTexture({ shaders, paintPerBlendMode, image });
	}
}

/**
 * Loads a {@link TextureAtlas} and its atlas page images from the given file path using the `readFile(path: string): Promise<Buffer>` function.
 * Throws an `Error` if the file or one of the atlas page images could not be loaded.
 */
export async function loadTextureAtlas (
	ck: CanvasKit,
	atlasFile: string,
	readFile: (path: string) => Promise<ArrayBuffer | Buffer>
): Promise<TextureAtlas> {
	const atlas = new TextureAtlas(bufferToUtf8String(await readFile(atlasFile)));
	const slashIndex = atlasFile.lastIndexOf("/");
	const parentDir =
		slashIndex >= 0 ? atlasFile.substring(0, slashIndex + 1) : "";
	for (const page of atlas.pages) {
		const texture = await CanvasKitTexture.fromFile(
			ck,
			parentDir + page.name,
			readFile
		);
		page.setTexture(texture);
	}
	return atlas;
}

/**
 * Loads a {@link SkeletonData} from the given file path (`.json` or `.skel`) using the `readFile(path: string): Promise<Buffer>` function.
 * Attachments will be looked up in the provided atlas.
 */
export async function loadSkeletonData (
	skeletonFile: string,
	atlas: TextureAtlas,
	readFile: (path: string) => Promise<ArrayBuffer | Buffer>,
	scale = 1
): Promise<SkeletonData> {
	const attachmentLoader = new AtlasAttachmentLoader(atlas);
	const loader = skeletonFile.endsWith(".json")
		? new SkeletonJson(attachmentLoader)
		: new SkeletonBinary(attachmentLoader);
	loader.scale = scale;
	const data = await readFile(skeletonFile);
	if (loader instanceof SkeletonJson) {
		return loader.readSkeletonData(bufferToUtf8String(data))
	}
	return loader.readSkeletonData(data);
}

/**
 * Manages a {@link Skeleton} and its associated {@link AnimationState}. A drawable is constructed from a {@link SkeletonData}, which can
 * be shared by any number of drawables.
 */
export class SkeletonDrawable {
	public readonly skeleton: Skeleton;
	public readonly animationState: AnimationState;

	/**
	 * Constructs a new drawble from the skeleton data.
	 */
	constructor (skeletonData: SkeletonData) {
		this.skeleton = new Skeleton(skeletonData);
		this.animationState = new AnimationState(
			new AnimationStateData(skeletonData)
		);
	}

	/**
	 * Updates the animation state and skeleton time by the delta time. Applies the
	 * animations to the skeleton and calculates the final pose of the skeleton.
	 *
	 * @param deltaTime the time since the last update in seconds
	 * @param physicsUpdate optional {@link Physics} update mode.
	 */
	update (deltaTime: number, physicsUpdate: Physics = Physics.update) {
		this.animationState.update(deltaTime);
		this.skeleton.update(deltaTime);
		this.animationState.apply(this.skeleton);
		this.skeleton.updateWorldTransform(physicsUpdate);
	}
}

/**
 * Renders a {@link Skeleton} or {@link SkeletonDrawable} to a CanvasKit {@link Canvas}.
 */
export class SkeletonRenderer {
	private skeletonRenderer = new SkeletonRendererCore();

	/**
	 * Creates a new skeleton renderer.
	 * @param ck the {@link CanvasKit} instance returned by `CanvasKitInit()`.
	 */
	constructor (private ck: CanvasKit) { }

	render (canvas: Canvas, skeleton: Skeleton | SkeletonDrawable) {
		if (skeleton instanceof SkeletonDrawable) skeleton = skeleton.skeleton;
		let command = this.skeletonRenderer.render(skeleton);
		while (command) {
			const { positions, uvs, colors, indices } = command;
			const ckImage = command.texture.getImage();
			const image = ckImage.image;
			const width = image.width();
			const height = image.height();

			for (let i = 0; i < uvs.length; i += 2) {
				uvs[i] = uvs[i] * width;
				uvs[i + 1] = uvs[i + 1] * height;
			}

			const vertices = this.ck.MakeVertices(
				this.ck.VertexMode.Triangles,
				positions,
				uvs,
				colors,
				// biome-ignore lint/suspicious/noExplicitAny: canvaskit wants indices as an array of number
				indices as any as number[],
				false
			);
			const ckPaint = ckImage.paintPerBlendMode.get(command.blendMode);
			if (ckPaint) canvas.drawVertices(vertices, this.ck.BlendMode.Modulate, ckPaint);
			vertices.delete();
			command = command.next;
		}
	}

}
