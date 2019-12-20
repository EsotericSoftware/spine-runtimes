/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {

	/** The interface which can be implemented to customize creating and populating attachments.
	 *
	 * See [Loading skeleton data](http://esotericsoftware.com/spine-loading-skeleton-data#AttachmentLoader) in the Spine
	 * Runtimes Guide. */
	export interface AttachmentLoader {
		/** @return May be null to not load an attachment. */
		newRegionAttachment (skin: Skin, name: string, path: string): RegionAttachment;

		/** @return May be null to not load an attachment. */
		newMeshAttachment (skin: Skin, name: string, path: string) : MeshAttachment;

		/** @return May be null to not load an attachment. */
		newBoundingBoxAttachment (skin: Skin, name: string) : BoundingBoxAttachment;

		/** @return May be null to not load an attachment */
		newPathAttachment(skin: Skin, name: string): PathAttachment;

		/** @return May be null to not load an attachment */
		newPointAttachment(skin: Skin, name: string): PointAttachment;

		/** @return May be null to not load an attachment */
		newClippingAttachment(skin: Skin, name: string): ClippingAttachment;
	}
}
