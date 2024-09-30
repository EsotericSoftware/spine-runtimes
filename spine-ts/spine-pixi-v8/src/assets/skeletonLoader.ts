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
	type AssetExtension,
	checkExtension,
	DOMAdapter,
	extensions,
	ExtensionType,
	LoaderParserPriority,
	ResolvedAsset
} from 'pixi.js';

type SkeletonJsonAsset = any;
type SkeletonBinaryAsset = Uint8Array;

function isJson (resource: any): resource is SkeletonJsonAsset {
	return Object.prototype.hasOwnProperty.call(resource, 'bones');
}

function isBuffer (resource: any): resource is SkeletonBinaryAsset {
	return resource instanceof Uint8Array;
}

const spineLoaderExtension: AssetExtension<SkeletonJsonAsset | SkeletonBinaryAsset> = {
	extension: ExtensionType.Asset,

	loader: {
		extension: {
			type: ExtensionType.LoadParser,
			priority: LoaderParserPriority.Normal,
			name: 'spineSkeletonLoader',
		},

		test (url) {
			return checkExtension(url, '.skel');
		},

		async load (url: string): Promise<SkeletonBinaryAsset> {
			const response = await DOMAdapter.get().fetch(url);

			const buffer = new Uint8Array(await response.arrayBuffer());

			return buffer;
		},
		testParse (asset: unknown, options: ResolvedAsset): Promise<boolean> {
			const isJsonSpineModel = checkExtension(options.src!, '.json') && isJson(asset);
			const isBinarySpineModel = checkExtension(options.src!, '.skel') && isBuffer(asset);

			return Promise.resolve(isJsonSpineModel || isBinarySpineModel);
		},
	},
} as AssetExtension<SkeletonJsonAsset | SkeletonBinaryAsset>;

extensions.add(spineLoaderExtension);
