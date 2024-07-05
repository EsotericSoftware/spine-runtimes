export * from "@esotericsoftware/spine-core";

import { BlendMode, ClippingAttachment, Color, MeshAttachment, NumberArrayLike, RegionAttachment, Skeleton, SkeletonClipping, Texture, TextureAtlas, TextureFilter, TextureWrap, Utils } from "@esotericsoftware/spine-core";
import { Canvas, CanvasKit, Image, Paint, Shader, BlendMode as CanvasKitBlendMode } from "canvaskit-wasm";

Skeleton.yDown = true;

type CanvasKitImage = { shaders: Shader[], paintPerBlendMode: Map<BlendMode, Paint>, image: Image };

// CanvasKit blend modes for premultiplied alpha
function toCkBlendMode(ck: CanvasKit, blendMode: BlendMode) {
    switch(blendMode) {
        case BlendMode.Normal: return ck.BlendMode.SrcOver;
        case BlendMode.Additive: return ck.BlendMode.Plus;
        case BlendMode.Multiply: return ck.BlendMode.Modulate;
        case BlendMode.Screen: return ck.BlendMode.Screen;
        default: return ck.BlendMode.SrcOver;
    }
}

export class CanvasKitTexture extends Texture {
    getImage(): CanvasKitImage {
        return this._image;
    }

    setFilters(minFilter: TextureFilter, magFilter: TextureFilter): void {
    }

    setWraps(uWrap: TextureWrap, vWrap: TextureWrap): void {
    }

    dispose(): void {
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

    static async fromFile(ck: CanvasKit, path: string, readFile: (path: string) => Promise<Buffer>): Promise<CanvasKitTexture> {
        const imgData = await readFile(path);
        if (!imgData) throw new Error(`Could not load image ${path}`);
        const image = ck.MakeImageFromEncoded(imgData);
        if (!image) throw new Error(`Could not load image ${path}`);
        const paintPerBlendMode = new Map<BlendMode, Paint>();
        const shaders: Shader[] = [];
        for (const blendMode of [BlendMode.Normal, BlendMode.Additive, BlendMode.Multiply, BlendMode.Screen]) {
            const paint = new ck.Paint();
            const shader = image.makeShaderOptions(ck.TileMode.Clamp, ck.TileMode.Clamp, ck.FilterMode.Linear, ck.MipmapMode.Linear);
            paint.setShader(shader);
            paint.setBlendMode(toCkBlendMode(ck, blendMode));
            paintPerBlendMode.set(blendMode, paint);
            shaders.push(shader);
        }
        return new CanvasKitTexture({ shaders, paintPerBlendMode, image });
    }
}

function bufferToUtf8String(buffer: any) {
    if (typeof Buffer !== 'undefined') {
        return buffer.toString('utf-8');
      } else if (typeof TextDecoder !== 'undefined') {
        return new TextDecoder('utf-8').decode(buffer);
      } else {
        throw new Error('Unsupported environment');
      }
}

export async function loadTextureAtlas(ck: CanvasKit, atlasFile: string, readFile: (path: string) => Promise<Buffer>): Promise<TextureAtlas> {
    const atlas = new TextureAtlas(bufferToUtf8String(await readFile(atlasFile)));
    const slashIndex = atlasFile.lastIndexOf("/");
    const parentDir = slashIndex >= 0 ? atlasFile.substring(0, slashIndex + 1) : "";
    for (const page of atlas.pages) {
        const texture = await CanvasKitTexture.fromFile(ck, parentDir + "/" + page.name, readFile);
        page.setTexture(texture);
    }
    return atlas;
}

export class SkeletonRenderer {
    private clipper = new SkeletonClipping();
    private tempColor = new Color();
    private tempColor2 = new Color();
    private static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
    private scratchPositions = Utils.newFloatArray(100);
    private scratchColors = Utils.newFloatArray(100);
    constructor(private ck: CanvasKit) {}

    render(canvas: Canvas, skeleton: Skeleton) {
        let clipper = this.clipper;
		let drawOrder = skeleton.drawOrder;
		let skeletonColor = skeleton.color;

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			let slot = drawOrder[i];
			if (!slot.bone.active) {
				clipper.clipEndWithSlot(slot);
				continue;
            }

			let attachment = slot.getAttachment();
            let positions = this.scratchPositions;
            let colors = this.scratchColors;
            let uvs: NumberArrayLike;
			let texture: CanvasKitTexture;
            let triangles: Array<number>;
            let attachmentColor: Color;
            let numVertices = 0;
			if (attachment instanceof RegionAttachment) {
				let region = attachment as RegionAttachment;
                positions = positions.length < 8 ? Utils.newFloatArray(8) : positions;
                numVertices = 4;
				region.computeWorldVertices(slot, positions, 0, 2);
				triangles = SkeletonRenderer.QUAD_TRIANGLES;
				uvs = region.uvs as Float32Array;
				texture = region.region?.texture as CanvasKitTexture;
				attachmentColor = region.color;
			} else if (attachment instanceof MeshAttachment) {
				let mesh = attachment as MeshAttachment;
                positions = positions.length < mesh.worldVerticesLength ? Utils.newFloatArray(mesh.worldVerticesLength) : positions;
                numVertices = mesh.worldVerticesLength >> 1;
				mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, positions, 0, 2);
				triangles = mesh.triangles;
				texture = mesh.region?.texture as CanvasKitTexture;
				uvs = mesh.uvs as Float32Array;
				attachmentColor = mesh.color;
			} else if (attachment instanceof ClippingAttachment) {
				let clip = attachment as ClippingAttachment;
				clipper.clipStart(slot, clip);
				continue;
			} else {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			if (texture) {
				if (clipper.isClipping()) {
					clipper.clipTrianglesUnpacked(positions, triangles, triangles.length, uvs);
                    positions = clipper.clippedVertices;
                    uvs = clipper.clippedUVs;
                    triangles = clipper.clippedTriangles;
				}

                let slotColor = slot.color;
				let finalColor = this.tempColor;
				finalColor.r = skeletonColor.r * slotColor.r * attachmentColor.r;
				finalColor.g = skeletonColor.g * slotColor.g * attachmentColor.g;
				finalColor.b = skeletonColor.b * slotColor.b * attachmentColor.b;
				finalColor.a = skeletonColor.a * slotColor.a * attachmentColor.a;

                if (colors.length / 4 < numVertices) colors = Utils.newFloatArray(numVertices * 4);
                for (let i = 0, n = numVertices * 4; i < n; i += 4) {
                    colors[i] = finalColor.r;
                    colors[i + 1] = finalColor.g;
                    colors[i + 2] = finalColor.b;
                    colors[i + 3] = finalColor.a;
                }

                const scaledUvs = new Array<number>(uvs.length);
                const width = texture.getImage().image.width();
                const height = texture.getImage().image.height();
                for (let i = 0; i < uvs.length; i+=2) {
                    scaledUvs[i] = uvs[i] * width;
                    scaledUvs[i + 1] = uvs[i + 1] * height;
                }

                const blendMode = slot.data.blendMode;
                const vertices = this.ck.MakeVertices(this.ck.VertexMode.Triangles, positions, scaledUvs, colors, triangles, false);
                canvas.drawVertices(vertices, this.ck.BlendMode.Modulate, texture.getImage().paintPerBlendMode.get(blendMode)!);
			}

			clipper.clipEndWithSlot(slot);
		}
		clipper.clipEnd();
    }
}