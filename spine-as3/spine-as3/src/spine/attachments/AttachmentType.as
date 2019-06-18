/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.attachments {
	public class AttachmentType {
		public static const region : AttachmentType = new AttachmentType(0, "region");
		public static const regionsequence : AttachmentType = new AttachmentType(1, "regionsequence");
		public static const boundingbox : AttachmentType = new AttachmentType(2, "boundingbox");
		public static const mesh : AttachmentType = new AttachmentType(3, "mesh");
		public static const linkedmesh : AttachmentType = new AttachmentType(3, "linkedmesh");
		public static const path : AttachmentType = new AttachmentType(4, "path");
		public static const point : AttachmentType = new AttachmentType(5, "point");
		public static const clipping : AttachmentType = new AttachmentType(6, "clipping");
		public var ordinal : int;
		public var name : String;
		
		public static const values : Array = [ region, boundingbox, mesh, linkedmesh, path, point, clipping ];

		public function AttachmentType(ordinal : int, name : String) {
			this.ordinal = ordinal;
			this.name = name;
		}
	}
}
