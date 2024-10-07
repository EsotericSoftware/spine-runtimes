import {
    colorBit,
    colorBitGl,
    compileHighShaderGlProgram,
    compileHighShaderGpuProgram,
    generateTextureBatchBit,
    generateTextureBatchBitGl,
    getBatchSamplersUniformGroup,
    roundPixelsBit,
    roundPixelsBitGl,
    Shader
} from 'pixi.js';
import { darkTintBit, darkTintBitGl } from './darkTintBit';

export class DarkTintShader extends Shader
{
    constructor(maxTextures: number)
    {
        const glProgram = compileHighShaderGlProgram({
            name: 'dark-tint-batch',
            bits: [
                colorBitGl,
                darkTintBitGl,
                generateTextureBatchBitGl(maxTextures),
                roundPixelsBitGl,
            ]
        });

        const gpuProgram = compileHighShaderGpuProgram({
            name: 'dark-tint-batch',
            bits: [
                colorBit,
                darkTintBit,
                generateTextureBatchBit(maxTextures),
                roundPixelsBit,
            ]
        });

        super({
            glProgram,
            gpuProgram,
            resources: {
                batchSamplers: getBatchSamplersUniformGroup(maxTextures),
            }
        });
    }
}
