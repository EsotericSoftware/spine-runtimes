/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine.attachments;

import spine.Skin;

/** The interface which can be implemented to customize creating and populating attachments.
 *
 * @see https://esotericsoftware.com/spine-loading-skeleton-data#AttachmentLoader Loading skeleton data in the Spine Runtimes Guide
 */
interface AttachmentLoader {
	/** @return May be null to not load the attachment. */
	function newRegionAttachment(skin:Skin, placeholder:String, name:String, path:String, sequence:Sequence):RegionAttachment;

	/** @return May be null to not load the attachment. In that case null should also be returned for child meshes. */
	function newMeshAttachment(skin:Skin, placeholder:String, name:String, path:String, sequence:Sequence):MeshAttachment;

	/** @return May be null to not load the attachment. */
	function newBoundingBoxAttachment(skin:Skin, placeholder:String, name:String):BoundingBoxAttachment;

	/** @return May be null to not load the attachment. */
	function newPathAttachment(skin:Skin, placeholder:String, name:String):PathAttachment;

	/** @return May be null to not load the attachment. */
	function newPointAttachment(skin:Skin, placeholder:String, name:String):PointAttachment;

	/** @return May be null to not load the attachment. */
	function newClippingAttachment(skin:Skin, placeholder:String, name:String):ClippingAttachment;
}
