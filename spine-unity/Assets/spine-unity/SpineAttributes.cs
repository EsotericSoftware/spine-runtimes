/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

// Contributed by: Mitch Thompson

using UnityEngine;
using System.Collections;

namespace Spine.Unity {
	public abstract class SpineAttributeBase : PropertyAttribute {
		public string dataField = "";
		public string startsWith = "";
	}

	public class SpineSlot : SpineAttributeBase {
		public bool containsBoundingBoxes = false;

		/// <summary>
		/// Smart popup menu for Spine Slots
		/// </summary>
		/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives).
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
		/// <param name="containsBoundingBoxes">Disables popup results that don't contain bounding box attachments when true.</param>
		public SpineSlot(string startsWith = "", string dataField = "", bool containsBoundingBoxes = false) {
			this.startsWith = startsWith;
			this.dataField = dataField;
			this.containsBoundingBoxes = containsBoundingBoxes;
		}
	}

	public class SpineEvent : SpineAttributeBase {
		/// <summary>
		/// Smart popup menu for Spine Events (Spine.EventData)
		/// </summary>
		/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives).
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
		public SpineEvent(string startsWith = "", string dataField = "") {
			this.startsWith = startsWith;
			this.dataField = dataField;
		}
	}

	public class SpineSkin : SpineAttributeBase {
		/// <summary>
		/// Smart popup menu for Spine Skins
		/// </summary>
		/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
		public SpineSkin(string startsWith = "", string dataField = "") {
			this.startsWith = startsWith;
			this.dataField = dataField;
		}
	}
	public class SpineAnimation : SpineAttributeBase {
		/// <summary>
		/// Smart popup menu for Spine Animations
		/// </summary>
		/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
		public SpineAnimation(string startsWith = "", string dataField = "") {
			this.startsWith = startsWith;
			this.dataField = dataField;
		}
	}

	public class SpineAttachment : SpineAttributeBase {
		public bool returnAttachmentPath = false;
		public bool currentSkinOnly = false;
		public bool placeholdersOnly = false;
		public string slotField = "";

		/// <summary>
		/// Smart popup menu for Spine Attachments
		/// </summary>
		/// <param name="currentSkinOnly">Filters popup results to only include the current Skin.  Only valid when a SkeletonRenderer is the data source.</param>
		/// <param name="returnAttachmentPath">Returns a fully qualified path for an Attachment in the format "Skin/Slot/AttachmentName". This path format is only used by the SpineAttachment helper methods like SpineAttachment.GetAttachment and .GetHierarchy. Do not use full path anywhere else in Spine's system.</param>
		/// <param name="placeholdersOnly">Filters popup results to exclude attachments that are not children of Skin Placeholders</param>
		/// <param name="slotField">If specified, a locally scoped field with the name supplied by in slotField will be used to limit the popup results to children of a named slot</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
		public SpineAttachment (bool currentSkinOnly = true, bool returnAttachmentPath = false, bool placeholdersOnly = false, string slotField = "", string dataField = "") {
			this.currentSkinOnly = currentSkinOnly;
			this.returnAttachmentPath = returnAttachmentPath;
			this.placeholdersOnly = placeholdersOnly;
			this.slotField = slotField;
			this.dataField = dataField;		
		}

		public static SpineAttachment.Hierarchy GetHierarchy (string fullPath) {
			return new SpineAttachment.Hierarchy(fullPath);
		}

		public static Spine.Attachment GetAttachment (string attachmentPath, Spine.SkeletonData skeletonData) {
			var hierarchy = SpineAttachment.GetHierarchy(attachmentPath);
			if (hierarchy.name == "")
				return null;

			return skeletonData.FindSkin(hierarchy.skin).GetAttachment(skeletonData.FindSlotIndex(hierarchy.slot), hierarchy.name);
		}

		public static Spine.Attachment GetAttachment (string attachmentPath, SkeletonDataAsset skeletonDataAsset) {
			return GetAttachment(attachmentPath, skeletonDataAsset.GetSkeletonData(true));
		}

		/// <summary>
		/// A struct that represents 3 strings that help identify and locate an attachment in a skeleton.</summary>
		public struct Hierarchy {
			public string skin;
			public string slot;
			public string name;

			public Hierarchy (string fullPath) {
				string[] chunks = fullPath.Split(new char[]{'/'}, System.StringSplitOptions.RemoveEmptyEntries);
				if (chunks.Length == 0) {
					skin = "";
					slot = "";
					name = "";
					return;
				}
				else if (chunks.Length < 2) {
					throw new System.Exception("Cannot generate Attachment Hierarchy from string! Not enough components! [" + fullPath + "]");
				}
				skin = chunks[0];
				slot = chunks[1];
				name = "";
				for (int i = 2; i < chunks.Length; i++) {
					name += chunks[i];
				}
			}
		}
	}

	public class SpineBone : SpineAttributeBase {
		/// <summary>
		/// Smart popup menu for Spine Bones
		/// </summary>
		/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
		public SpineBone(string startsWith = "", string dataField = "") {
			this.startsWith = startsWith;
			this.dataField = dataField;
		}

		public static Spine.Bone GetBone(string boneName, SkeletonRenderer renderer) {
			return renderer.skeleton == null ? null : renderer.skeleton.FindBone(boneName);
		}

		public static Spine.BoneData GetBoneData(string boneName, SkeletonDataAsset skeletonDataAsset) {
			var data = skeletonDataAsset.GetSkeletonData(true);
			return data.FindBone(boneName);
		}
	}

	public class SpineAtlasRegion : PropertyAttribute {
	
	}

}
