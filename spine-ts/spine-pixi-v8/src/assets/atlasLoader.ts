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
    checkExtension,
    DOMAdapter,
    extensions,
    ExtensionType,
    LoaderParserPriority,
    path,
    TextureSource
} from 'pixi.js';
import { SpineTexture } from '../SpineTexture';
import { TextureAtlas } from '@esotericsoftware/spine-core';

import type { AssetExtension, Loader, ResolvedAsset, Texture } from 'pixi.js';

type RawAtlas = string;

const spineTextureAtlasLoader: AssetExtension<RawAtlas | TextureAtlas, ISpineAtlasMetadata> = {
    extension: ExtensionType.Asset,

    loader: {
        extension: {
            type: ExtensionType.LoadParser,
            priority: LoaderParserPriority.Normal,
            name: 'spineTextureAtlasLoader',
        },

        test(url: string): boolean
        {
            return checkExtension(url, '.atlas');
        },

        async load(url: string): Promise<RawAtlas>
        {
            const response = await DOMAdapter.get().fetch(url);

            const txt = await response.text();

            return txt;
        },

        testParse(asset: unknown, options: ResolvedAsset): Promise<boolean>
        {
            const isExtensionRight = checkExtension(options.src as string, '.atlas');
            const isString = typeof asset === 'string';

            return Promise.resolve(isExtensionRight && isString);
        },

        unload(atlas: TextureAtlas)
        {
            atlas.dispose();
        },

        async parse(asset: RawAtlas, options: ResolvedAsset, loader: Loader): Promise<TextureAtlas>
        {
            const metadata: ISpineAtlasMetadata = options.data || {};
            let basePath = path.dirname(options.src as string);

            if (basePath && basePath.lastIndexOf('/') !== basePath.length - 1)
            {
                basePath += '/';
            }

            // Retval is going to be a texture atlas. However we need to wait for it's callback to resolve this promise.
            const retval = new TextureAtlas(asset);

            // If the user gave me only one texture, that one is assumed to be the "first" texture in the atlas
            if (metadata.images instanceof TextureSource || typeof metadata.images === 'string')
            {
                const pixiTexture = metadata.images;

                metadata.images = {} as Record<string, TextureSource | string>;
                metadata.images[retval.pages[0].name] = pixiTexture;
            }

            // we will wait for all promises for the textures at the same time at the end.
            const textureLoadingPromises:Promise<any>[] = [];

            // fill the pages
            for (const page of retval.pages)
            {
                const pageName = page.name;
                const providedPage = metadata?.images ? metadata.images[pageName] : undefined;

                if (providedPage instanceof TextureSource)
                {
                    page.setTexture(SpineTexture.from(providedPage));
                }
                else
                {
                    // eslint-disable-next-line max-len
                    const url: string = providedPage ?? path.normalize([...basePath.split(path.sep), pageName].join(path.sep));

                    const assetsToLoadIn = {
                        src: url,
                        data: {
                            ...metadata.imageMetadata,
                            alphaMode: page.pma ? 'premultiplied-alpha' : 'premultiply-alpha-on-upload'
                        }
                    };

                    const pixiPromise = loader.load<Texture>(assetsToLoadIn).then((texture) =>
                    {
                        page.setTexture(SpineTexture.from(texture.source));
                    });

                    textureLoadingPromises.push(pixiPromise);
                }
            }

            await Promise.all(textureLoadingPromises);

            return retval;
        },
    },
} as AssetExtension<RawAtlas | TextureAtlas, ISpineAtlasMetadata>;

extensions.add(spineTextureAtlasLoader);

export interface ISpineAtlasMetadata
{
    // If you are downloading an .atlas file, this metadata will go to the Texture loader
    imageMetadata?: any;
    // If you already have atlas pages loaded as pixi textures
    // and want to use that to create the atlas, you can pass them here
    images?: TextureSource | string | Record<string, TextureSource | string>;
}
