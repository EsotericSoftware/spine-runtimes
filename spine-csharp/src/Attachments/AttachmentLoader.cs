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

namespace Spine {
	/// <summary>
	/// The interface which can be implemented to customize creating and populating attachments.
	/// </summary>
	/// <remarks>
	/// See <a href='https://esotericsoftware.com/spine-loading-skeleton-data#AttachmentLoader'>Loading skeleton data</a> in the Spine
	/// Runtimes Guide.
	/// </remarks>
	public interface AttachmentLoader {
		/// <summary>
		/// Creates a new region attachment.
		/// </summary>
		/// <param name="skin">The skin that will contain the attachment.</param>
		/// <param name="name">The name of the attachment.</param>
		/// <param name="path">The path to the attachment.</param>
		/// <param name="sequence">The sequence for the attachment.</param>
		/// <returns>May be null to not load the attachment.</returns>
		RegionAttachment NewRegionAttachment (Skin skin, string name, string path, Sequence sequence);

		/// <summary>
		/// Creates a new mesh attachment.
		/// </summary>
		/// <param name="skin">The skin that will contain the attachment.</param>
		/// <param name="name">The name of the attachment.</param>
		/// <param name="path">The path to the attachment.</param>
		/// <param name="sequence">The sequence for the attachment.</param>
		/// <returns>May be null to not load the attachment. In that case null should also be returned for child meshes.</returns>
		MeshAttachment NewMeshAttachment (Skin skin, string name, string path, Sequence sequence);

		/// <summary>
		/// Creates a new bounding box attachment.
		/// </summary>
		/// <param name="skin">The skin that will contain the attachment.</param>
		/// <param name="name">The name of the attachment.</param>
		/// <returns>May be null to not load the attachment.</returns>
		BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, string name);

		/// <summary>
		/// Creates a new path attachment.
		/// </summary>
		/// <param name="skin">The skin that will contain the attachment.</param>
		/// <param name="name">The name of the attachment.</param>
		/// <returns>May be null to not load the attachment.</returns>
		PathAttachment NewPathAttachment (Skin skin, string name);

		/// <summary>
		/// Creates a new point attachment.
		/// </summary>
		/// <param name="skin">The skin that will contain the attachment.</param>
		/// <param name="name">The name of the attachment.</param>
		/// <returns>May be null to not load the attachment.</returns>
		PointAttachment NewPointAttachment (Skin skin, string name);

		/// <summary>
		/// Creates a new clipping attachment.
		/// </summary>
		/// <param name="skin">The skin that will contain the attachment.</param>
		/// <param name="name">The name of the attachment.</param>
		/// <returns>May be null to not load the attachment.</returns>
		ClippingAttachment NewClippingAttachment (Skin skin, string name);
	}
}
