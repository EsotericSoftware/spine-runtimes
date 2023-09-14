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

package spine.attachments;

class AttachmentType {
	public static var region(default, never):AttachmentType = new AttachmentType(0, "region");
	public static var boundingbox(default, never):AttachmentType = new AttachmentType(1, "boundingbox");
	public static var mesh(default, never):AttachmentType = new AttachmentType(2, "mesh");
	public static var linkedmesh(default, never):AttachmentType = new AttachmentType(3, "linkedmesh");
	public static var path(default, never):AttachmentType = new AttachmentType(4, "path");
	public static var point(default, never):AttachmentType = new AttachmentType(5, "point");
	public static var clipping(default, never):AttachmentType = new AttachmentType(6, "clipping");

	public static var values(default, never):Array<AttachmentType> = [region, boundingbox, mesh, linkedmesh, path, point, clipping];

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):AttachmentType {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
