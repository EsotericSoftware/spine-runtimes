/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {

	[ExecuteInEditMode]
	public class BoundingBoxFollower : MonoBehaviour {
		#region Inspector
		public SkeletonRenderer skeletonRenderer;
		[SpineSlot(dataField: "skeletonRenderer", containsBoundingBoxes: true)]
		public string slotName;
		public bool isTrigger;
		#endregion

		Slot slot;
		BoundingBoxAttachment currentAttachment;
		string currentAttachmentName;
		PolygonCollider2D currentCollider;

		bool valid = false;
		bool hasReset;

		public readonly Dictionary<BoundingBoxAttachment, PolygonCollider2D> colliderTable = new Dictionary<BoundingBoxAttachment, PolygonCollider2D>();
		public readonly Dictionary<BoundingBoxAttachment, string> attachmentNameTable = new Dictionary<BoundingBoxAttachment, string>();

		public Slot Slot { get { return slot; } }
		public BoundingBoxAttachment CurrentAttachment { get { return currentAttachment; } }
		public string CurrentAttachmentName { get { return currentAttachmentName; } }
		public PolygonCollider2D CurrentCollider { get { return currentCollider; } }
		public bool IsTrigger { get { return isTrigger; } }

		void OnEnable () {
			ClearColliders();

			if (skeletonRenderer == null)
				skeletonRenderer = GetComponentInParent<SkeletonRenderer>();

			if (skeletonRenderer != null) {
				skeletonRenderer.OnRebuild -= HandleRebuild;
				skeletonRenderer.OnRebuild += HandleRebuild;

				if (hasReset)
					HandleRebuild(skeletonRenderer);
			}
		}

		void OnDisable () {
			skeletonRenderer.OnRebuild -= HandleRebuild;
		}

		void Start () {
			if (!hasReset && skeletonRenderer != null)
				HandleRebuild(skeletonRenderer);
		}

		public void HandleRebuild (SkeletonRenderer renderer) {
			if (string.IsNullOrEmpty(slotName))
				return;

			hasReset = true;
			ClearColliders();
			colliderTable.Clear();

			if (skeletonRenderer.skeleton == null) {
				skeletonRenderer.OnRebuild -= HandleRebuild;
				skeletonRenderer.Initialize(false);
				skeletonRenderer.OnRebuild += HandleRebuild;
			}

			var skeleton = skeletonRenderer.skeleton;
			slot = skeleton.FindSlot(slotName);
			int slotIndex = skeleton.FindSlotIndex(slotName);

			if (this.gameObject.activeInHierarchy) {
				foreach (var skin in skeleton.Data.Skins) {
					var attachmentNames = new List<string>();
					skin.FindNamesForSlot(slotIndex, attachmentNames);

					foreach (var attachmentName in attachmentNames) {
						var attachment = skin.GetAttachment(slotIndex, attachmentName);
						var boundingBoxAttachment = attachment as BoundingBoxAttachment;

#if UNITY_EDITOR
						if (attachment != null && boundingBoxAttachment == null)
							Debug.Log("BoundingBoxFollower tried to follow a slot that contains non-boundingbox attachments: " + slotName);
#endif

						if (boundingBoxAttachment != null) {
							var bbCollider = SkeletonUtility.AddBoundingBoxAsComponent(boundingBoxAttachment, gameObject, true);
							bbCollider.enabled = false;
							bbCollider.hideFlags = HideFlags.NotEditable;
							bbCollider.isTrigger = IsTrigger;
							colliderTable.Add(boundingBoxAttachment, bbCollider);
							attachmentNameTable.Add(boundingBoxAttachment, attachmentName);
						}
					}
				}
			}

#if UNITY_EDITOR
			valid = colliderTable.Count != 0;
			if (!valid) {
				if (this.gameObject.activeInHierarchy)
					Debug.LogWarning("Bounding Box Follower not valid! Slot [" + slotName + "] does not contain any Bounding Box Attachments!");
				else 
					Debug.LogWarning("Bounding Box Follower tried to rebuild as a prefab.");
			}
#endif
		}

		void ClearColliders () {
			var colliders = GetComponents<PolygonCollider2D>();
			if (colliders.Length == 0) return;

#if UNITY_EDITOR
			if (Application.isPlaying) {
				foreach (var c in colliders) {
					if (c != null)
						Destroy(c);
				}
			} else {
				foreach (var c in colliders)
					DestroyImmediate(c);
			}
#else
			foreach (var c in colliders)
				if (c != null)
					Destroy(c);
#endif

			colliderTable.Clear();
			attachmentNameTable.Clear();
		}

		void LateUpdate () {
			if (!skeletonRenderer.valid)
				return;

			if (slot != null && slot.Attachment != currentAttachment)
				MatchAttachment(slot.Attachment);
		}

		/// <summary>Sets the current collider to match attachment.</summary>
		/// <param name="attachment">If the attachment is not a bounding box, it will be treated as null.</param>
		void MatchAttachment (Attachment attachment) {
			var bbAttachment = attachment as BoundingBoxAttachment;

#if UNITY_EDITOR
			if (attachment != null && bbAttachment == null)
				Debug.LogWarning("BoundingBoxFollower tried to match a non-boundingbox attachment. It will treat it as null.");
#endif

			if (currentCollider != null)
				currentCollider.enabled = false;

			if (bbAttachment == null) {
				currentCollider = null;
			} else {
				currentCollider = colliderTable[bbAttachment];
				currentCollider.enabled = true;
			}

			currentAttachment = bbAttachment;
			currentAttachmentName = currentAttachment == null ? null : attachmentNameTable[bbAttachment];
		}
	}

}
