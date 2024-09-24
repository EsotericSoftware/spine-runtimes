/** ****************************************************************************
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

import {
    collectAllRenderables,
    extensions, ExtensionType,
    InstructionSet,
    type Renderer,
    type RenderPipe,
} from 'pixi.js';
import { BatchableSpineSlot } from './BatchableSpineSlot';
import { Spine } from './Spine';
import { MeshAttachment, RegionAttachment, SkeletonClipping } from '@esotericsoftware/spine-core';

const clipper = new SkeletonClipping();

const spineBlendModeMap = {
    0: 'normal',
    1: 'add',
    2: 'multiply',
    3: 'screen'
};

// eslint-disable-next-line max-len
export class SpinePipe implements RenderPipe<Spine>
{
    /** @ignore */
    static extension = {
        type: [
            ExtensionType.WebGLPipes,
            ExtensionType.WebGPUPipes,
            ExtensionType.CanvasPipes,
        ],
        name: 'spine',
    } as const;

    renderer: Renderer;

    private gpuSpineData:Record<string, any> = {};

    constructor(renderer: Renderer)
    {
        this.renderer = renderer;
    }

    validateRenderable(spine: Spine): boolean
    {
        spine._applyState();

        // if pine attachments have changed, we need to rebuild the batch!
        if (spine.spineAttachmentsDirty)
        {
            return true;
        }
        // if the textures have changed, we need to rebuild the batch, but only if the texture is not already in the batch
        else if (spine.spineTexturesDirty)
        {
            // loop through and see if the textures have changed..
            const drawOrder = spine.skeleton.drawOrder;
            const gpuSpine = this.gpuSpineData[spine.uid];

            for (let i = 0, n = drawOrder.length; i < n; i++)
            {
                const slot = drawOrder[i];
                const attachment = slot.getAttachment();

                if (attachment instanceof RegionAttachment || attachment instanceof MeshAttachment)
                {
                    const cacheData = spine._getCachedData(slot, attachment);
                    const batchableSpineSlot = gpuSpine.slotBatches[cacheData.id];

                    const texture = cacheData.texture;

                    if (texture !== batchableSpineSlot.texture)
                    {
                        if (!batchableSpineSlot._batcher.checkAndUpdateTexture(batchableSpineSlot, texture))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    addRenderable(spine: Spine, instructionSet:InstructionSet)
    {
        const gpuSpine = this.gpuSpineData[spine.uid] ||= { slotBatches: {} };

        const batcher = this.renderer.renderPipes.batch;

        const drawOrder = spine.skeleton.drawOrder;

        const roundPixels = (this.renderer._roundPixels | spine._roundPixels) as 0 | 1;

        spine._applyState();

        for (let i = 0, n = drawOrder.length; i < n; i++)
        {
            const slot = drawOrder[i];
            const attachment = slot.getAttachment();
            const blendMode = spineBlendModeMap[slot.data.blendMode];

            if (attachment instanceof RegionAttachment || attachment instanceof MeshAttachment)
            {
                const cacheData = spine._getCachedData(slot, attachment);
                const batchableSpineSlot = gpuSpine.slotBatches[cacheData.id] ||= new BatchableSpineSlot();

                batchableSpineSlot.setData(
                    spine,
                    cacheData,
                    blendMode,
                    roundPixels
                );

                if (!cacheData.skipRender)
                {
                    batcher.addToBatch(batchableSpineSlot, instructionSet);
                }
            }

            const containerAttachment = spine._slotsObject[slot.data.name];

            if (containerAttachment)
            {
                const container = containerAttachment.container;

                container.includeInBuild = true;
                collectAllRenderables(container, instructionSet, this.renderer);
                container.includeInBuild = false;
            }
        }

        clipper.clipEnd();
    }

    updateRenderable(spine: Spine)
    {
        // we assume that spine will always change its verts size..
        const gpuSpine = this.gpuSpineData[spine.uid];

        spine._applyState();

        const drawOrder = spine.skeleton.drawOrder;

        for (let i = 0, n = drawOrder.length; i < n; i++)
        {
            const slot = drawOrder[i];
            const attachment = slot.getAttachment();

            if (attachment instanceof RegionAttachment || attachment instanceof MeshAttachment)
            {
                const cacheData = spine._getCachedData(slot, attachment);

                if (!cacheData.skipRender)
                {
                    const batchableSpineSlot = gpuSpine.slotBatches[spine._getCachedData(slot, attachment).id];

                    batchableSpineSlot._batcher?.updateElement(batchableSpineSlot);
                }
            }
        }
    }

    destroyRenderable(spine: Spine)
    {
        // TODO remove the renderable from the batcher
        this.gpuSpineData[spine.uid] = null as any;
    }

    destroy()
    {
        this.gpuSpineData = null as any;
        this.renderer = null as any;
    }
}

extensions.add(SpinePipe);
