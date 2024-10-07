import { Buffer, BufferUsage, Geometry } from 'pixi.js';

const placeHolderBufferData = new Float32Array(1);
const placeHolderIndexData = new Uint32Array(1);

export class DarkTintBatchGeometry extends Geometry
{
    constructor()
    {
        const vertexSize = 7;

        const attributeBuffer = new Buffer({
            data: placeHolderBufferData,
            label: 'attribute-batch-buffer',
            usage: BufferUsage.VERTEX | BufferUsage.COPY_DST,
            shrinkToFit: false,
        });

        const indexBuffer = new Buffer({
            data: placeHolderIndexData,
            label: 'index-batch-buffer',
            usage: BufferUsage.INDEX | BufferUsage.COPY_DST, // | BufferUsage.STATIC,
            shrinkToFit: false,
        });

        const stride = vertexSize * 4;

        super({
            attributes: {
                aPosition: {
                    buffer: attributeBuffer,
                    format: 'float32x2',
                    stride,
                    offset: 0,
                },
                aUV: {
                    buffer: attributeBuffer,
                    format: 'float32x2',
                    stride,
                    offset: 2 * 4,
                },
                aColor: {
                    buffer: attributeBuffer,
                    format: 'unorm8x4',
                    stride,
                    offset: 4 * 4,
                },
                aDarkColor: {
                    buffer: attributeBuffer,
                    format: 'unorm8x4',
                    stride,
                    offset: 5 * 4,
                },
                aTextureIdAndRound: {
                    buffer: attributeBuffer,
                    format: 'uint16x2',
                    stride,
                    offset: 6 * 4,
                },
            },
            indexBuffer
        });
    }
}

