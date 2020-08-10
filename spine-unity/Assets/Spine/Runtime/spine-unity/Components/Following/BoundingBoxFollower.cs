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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {

	#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
	#else
	[ExecuteInEditMode]
	#endif
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

			slot = null;
			currentAttachment = null;
			currentAttachmentName = null;
			currentCollider = null;
			colliderTable.Clear();
			nameTable.Clear();

			var skeleton = skeletonRenderer.skeleton;
			slot = skeleton.FindSlot(slotName);
			int slotIndex = skeleton.FindSlotIndex(slotName);

			if (slot == null) {
				if (BoundingBoxFollower.DebugMessages)
					Debug.LogWarning(string.Format("Slot '{0}' not found for BoundingBoxFollower on '{1}'. (Previous colliders were disposed.)", slotName, this.gameObject.name));
				return;
			}

			int requiredCollidersCount = 0;
			var colliders = GetComponents<PolygonCollider2D>();
			if (this.gameObject.activeInHierarchy) {
				foreach (var skin in skeleton.Data.Skins)
					AddCollidersForSkin(skin, slotIndex, colliders, ref requiredCollidersCount);

				if (skeleton.skin != null)
					AddCollidersForSkin(skeleton.skin, slotIndex, colliders, ref requiredCollidersCount);
			}
			DisposeExcessCollidersAfter(requiredCollidersCount);

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

		void AddCollidersForSkin (Skin skin, int slotIndex, PolygonCollider2D[] previousColliders, ref int collidersCount) {
			if (skin == null) return;
			var skinEntries = new List<Skin.SkinEntry>();
			skin.GetAttachments(slotIndex, skinEntries);

			foreach (var entry in skinEntries) {
				var attachment = skin.GetAttachment(slotIndex, entry.Name);
				var boundingBoxAttachment = attachment as BoundingBoxAttachment;

				if (BoundingBoxFollower.DebugMessages && attachment != null && boundingBoxAttachment == null)
					Debug.Log("BoundingBoxFollower tried to follow a slot that contains non-boundingbox attachments: " + slotName);

				if (boundingBoxAttachment != null) {
					if (!colliderTable.ContainsKey(boundingBoxAttachment)) {
						var bbCollider = collidersCount < previousColliders.Length ?
							previousColliders[collidersCount] : gameObject.AddComponent<PolygonCollider2D>();
						++collidersCount;
						SkeletonUtility.SetColliderPointsLocal(bbCollider, slot, boundingBoxAttachment);
						bbCollider.isTrigger = isTrigger;
						bbCollider.enabled = false;
						bbCollider.hideFlags = HideFlags.NotEditable;
						bbCollider.isTrigger = IsTrigger;
						colliderTable.Add(boundingBoxAttachment, bbCollider);
						nameTable.Add(boundingBoxAttachment, entry.Name);
					}
				}
			}
		}

		void OnDisable () {
			if (clearStateOnDisable)
				ClearState();

			if (skeletonRenderer != null)
				skeletonRenderer.OnRebuild -= HandleRebuild;
		}

		public void ClearState () {
			if (colliderTable != null)
				foreach (var col in colliderTable.Values)
					col.enabled = false;

			currentAttachment = null;
			currentAttachmentName = null;
			currentCollider = null;
		}

		void DisposeExcessCollidersAfter (int requiredCount) {
			var colliders = GetComponents<PolygonCollider2D>();
			if (colliders.Length == 0) return;

			for (int i = requiredCount; i < colliders.Length; ++i) {
				var collider = colliders[i];
				if (collider != null) {
#if UNITY_EDITOR
					if (Application.isEditor && !Application.isPlaying)
						DestroyImmediate(collider);
					else
#endif
						Destroy(collider);
				}
			}
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
