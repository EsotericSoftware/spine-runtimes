/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

/*****************************************************************************
 * Spine Extensions created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;
using Spine;

public static class SkeletonExtensions {
	
	public static void SetColor (this Slot slot, Color color) {
		slot.A = color.a;
		slot.R = color.r;
		slot.G = color.g;
		slot.B = color.b;
	}

	public static void SetColor (this Slot slot, Color32 color) {
		slot.A = color.a / 255f;
		slot.R = color.r / 255f;
		slot.G = color.g / 255f;
		slot.B = color.b / 255f;
	}

	public static void SetColor (this RegionAttachment attachment, Color color) {
		attachment.A = color.a;
		attachment.R = color.r;
		attachment.G = color.g;
		attachment.B = color.b;
	}

	public static void SetColor (this RegionAttachment attachment, Color32 color) {
		attachment.A = color.a / 255f;
		attachment.R = color.r / 255f;
		attachment.G = color.g / 255f;
		attachment.B = color.b / 255f;
	}

	public static void SetColor (this MeshAttachment attachment, Color color) {
		attachment.A = color.a;
		attachment.R = color.r;
		attachment.G = color.g;
		attachment.B = color.b;
	}

	public static void SetColor (this MeshAttachment attachment, Color32 color) {
		attachment.A = color.a / 255f;
		attachment.R = color.r / 255f;
		attachment.G = color.g / 255f;
		attachment.B = color.b / 255f;
	}

	public static void SetColor (this SkinnedMeshAttachment attachment, Color color) {
		attachment.A = color.a;
		attachment.R = color.r;
		attachment.G = color.g;
		attachment.B = color.b;
	}

	public static void SetColor (this SkinnedMeshAttachment attachment, Color32 color) {
		attachment.A = color.a / 255f;
		attachment.R = color.r / 255f;
		attachment.G = color.g / 255f;
		attachment.B = color.b / 255f;
	}

	public static void SetPosition (this Bone bone, Vector2 position) {
		bone.X = position.x;
		bone.Y = position.y;
	}

	public static void SetPosition (this Bone bone, Vector3 position) {
		bone.X = position.x;
		bone.Y = position.y;
	}

}
