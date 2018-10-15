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

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {

	[ExecuteInEditMode]
	public class BoundingBoxFollower : MonoBehaviour {
		internal static bool DebugMessages = true;

		#region Inspector
		public SkeletonRenderer skeletonRenderer;
		[SpineSlot(dataField: "skeletonRenderer", containsBoundingBoxes: true)]
		public string slotName;
		public bool isTrigger;
		public bool clearStateOnDisable = true;
		#endregion

		Slot slot;
		BoundingBoxAttachment currentAttachment;
		string currentAttachmentName;
		PolygonCollider2D currentCollider;

		public readonly Dictionary<BoundingBoxAttachment, PolygonCollider2D> colliderTable = new Dictionary<BoundingBoxAttachment, PolygonCollider2D>();
		public readonly Dictionary<BoundingBoxAttachment, string> nameTable = new Dictionary<BoundingBoxAttachment, string>();

		public Slot Slot { get { return slot; } }
		public BoundingBoxAttachment CurrentAttachment { get { return currentAttachment; } }
		public string CurrentAttachmentName { get { return currentAttachmentName; } }
		public PolygonCollider2D CurrentCollider { get { return currentCollider; } }
		public bool IsTrigger { get { return isTrigger; } }

		void Start () {
			Initialize();
		}

		void OnEnable () {
			if (skeletonRenderer != null) {
				skeletonRenderer.OnRebuild -= HandleRebuild;
				skeletonRenderer.OnRebuild += HandleRebuild;
			}

			Initialize();
		}

		void HandleRebuild (SkeletonRenderer sr) {
			//if (BoundingBoxFollower.DebugMessages) Debug.Log("Skeleton was rebuilt. Repopulating BoundingBoxFollower.");
			Initialize();
		}

		/// <summary>
		/// Initialize and instantiate the BoundingBoxFollower colliders. This is method checks if the BoundingBoxFollower has already been initialized for the skeleton instance and slotName and prevents overwriting unless it detects a new setup.</summary>
		public void Initialize (bool overwrite = false) {
			if (skeletonRenderer == null)
				return;

			skeletonRenderer.Initialize(false);
			
			if (string.IsNullOrEmpty(slotName))
				return;

			// Don't reinitialize if the setup did not change.
			if (!overwrite
				&&
				colliderTable.Count > 0 && slot != null			// Slot is set and colliders already populated.
				&&
				skeletonRenderer.skeleton == slot.Skeleton		// Skeleton object did not change.
				&&
				slotName == slot.data.name						// Slot object did not change.
			) 
				return;

			DisposeColliders();

			var skeleton = skeletonRenderer.skeleton;
			slot = skeleton.FindSlot(slotName);
			int slotIndex = skeleton.FindSlotIndex(slotName);

			if (slot == null) {
				if (BoundingBoxFollower.DebugMessages)
					Debug.LogWarning(string.Format("Slot '{0}' not found for BoundingBoxFollower on '{1}'. (Previous colliders were disposed.)", slotName, this.gameObject.name));
				return;
			}

			if (this.gameObject.activeInHierarchy) {
				foreach (var skin in skeleton.Data.Skins)
					AddSkin(skin, slotIndex);

				if (skeleton.skin != null)
					AddSkin(skeleton.skin, slotIndex);
			}

			if (BoundingBoxFollower.DebugMessages) {
				bool valid = colliderTable.Count != 0;
				if (!valid) {
					if (this.gameObject.activeInHierarchy)
						Debug.LogWarning("Bounding Box Follower not valid! Slot [" + slotName + "] does not contain any Bounding Box Attachments!");
					else 
						Debug.LogWarning("Bounding Box Follower tried to rebuild as a prefab.");
				}
			}
		}

		void AddSkin (Skin skin, int slotIndex) {
			if (skin == null) return;
			var attachmentNames = new List<string>();
			skin.FindNamesForSlot(slotIndex, attachmentNames);

			foreach (var skinKey in attachmentNames) {
				var attachment = skin.GetAttachment(slotIndex, skinKey);
				var boundingBoxAttachment = attachment as BoundingBoxAttachment;

				if (BoundingBoxFollower.DebugMessages && attachment != null && boundingBoxAttachment == null)
					Debug.Log("BoundingBoxFollower tried to follow a slot that contains non-boundingbox attachments: " + slotName);

				if (boundingBoxAttachment != null) {
					if (!colliderTable.ContainsKey(boundingBoxAttachment)) {
						var bbCollider = SkeletonUtility.AddBoundingBoxAsComponent(boundingBoxAttachment, slot, gameObject, isTrigger);

						bbCollider.enabled = false;
						bbCollider.hideFlags = HideFlags.NotEditable;
						bbCollider.isTrigger = IsTrigger;
						colliderTable.Add(boundingBoxAttachment, bbCollider);
						nameTable.Add(boundingBoxAttachment, skinKey);
					}
				}
			}
		}

		void OnDisable () {
			if (clearStateOnDisable)
				ClearState();
		}

		public void ClearState () {
			if (colliderTable != null)
				foreach (var col in colliderTable.Values)
					col.enabled = false;

			currentAttachment = null;
			currentAttachmentName = null;
			currentCollider = null;
		}

		void DisposeColliders () {
			var colliders = GetComponents<PolygonCollider2D>();
			if (colliders.Length == 0) return;

			if (Application.isEditor) {
				if (Application.isPlaying) {
					foreach (var c in colliders) {
						if (c != null)
							Destroy(c);
					}
				} else {
					foreach (var c in colliders)
						if (c != null) 
							DestroyImmediate(c);
				}
			} else {
				foreach (PolygonCollider2D c in colliders)
					if (c != null)
						Destroy(c);
			}

			slot = null;
			currentAttachment = null;
			currentAttachmentName = null;
			currentCollider = null;
			colliderTable.Clear();
			nameTable.Clear();
		}

		void LateUpdate () {
			if (slot != null && slot.Attachment != currentAttachment)
				MatchAttachment(slot.Attachment);
		}

		/// <summary>Sets the current collider to match attachment.</summary>
		/// <param name="attachment">If the attachment is not a bounding box, it will be treated as null.</param>
		void MatchAttachment (Attachment attachment) {
			var bbAttachment = attachment as BoundingBoxAttachment;

			if (BoundingBoxFollower.DebugMessages && attachment != null && bbAttachment == null)
				Debug.LogWarning("BoundingBoxFollower tried to match a non-boundingbox attachment. It will treat it as null.");

			if (currentCollider != null)
				currentCollider.enabled = false;

			if (bbAttachment == null) {
				currentCollider = null;
				currentAttachment = null;
				currentAttachmentName = null;
			} else {
				PolygonCollider2D foundCollider;
				colliderTable.TryGetValue(bbAttachment, out foundCollider);
				if (foundCollider != null) {
					currentCollider = foundCollider;
					currentCollider.enabled = true;
					currentAttachment = bbAttachment;
					currentAttachmentName = nameTable[bbAttachment];
				} else {
					currentCollider = null;
					currentAttachment = bbAttachment;
					currentAttachmentName = null;
					if (BoundingBoxFollower.DebugMessages) Debug.LogFormat("Collider for BoundingBoxAttachment named '{0}' was not initialized. It is possibly from a new skin. currentAttachmentName will be null. You may need to call BoundingBoxFollower.Initialize(overwrite: true);", bbAttachment.Name);
				}
			}
		}
	}

}
