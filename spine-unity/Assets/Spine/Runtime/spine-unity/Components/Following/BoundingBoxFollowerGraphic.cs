/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[HelpURL("http://esotericsoftware.com/spine-unity#BoundingBoxFollowerGraphic")]
	public class BoundingBoxFollowerGraphic : MonoBehaviour {
		internal static bool DebugMessages = true;

		#region Inspector
		public SkeletonGraphic skeletonGraphic;
		[SpineSlot(dataField: "skeletonGraphic", containsBoundingBoxes: true)]
		public string slotName;
		public bool isTrigger, usedByEffector, usedByComposite;
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
			if (skeletonGraphic != null) {
				skeletonGraphic.OnRebuild -= HandleRebuild;
				skeletonGraphic.OnRebuild += HandleRebuild;
			}

			Initialize();
		}

		void HandleRebuild (SkeletonGraphic sr) {
			//if (BoundingBoxFollowerGraphic.DebugMessages) Debug.Log("Skeleton was rebuilt. Repopulating BoundingBoxFollowerGraphic.");
			Initialize();
		}

		/// <summary>
		/// Initialize and instantiate the BoundingBoxFollowerGraphic colliders. This is method checks if the BoundingBoxFollowerGraphic has already been initialized for the skeleton instance and slotName and prevents overwriting unless it detects a new setup.</summary>
		public void Initialize (bool overwrite = false) {
			if (skeletonGraphic == null)
				return;

			skeletonGraphic.Initialize(false);

			if (string.IsNullOrEmpty(slotName))
				return;

			// Don't reinitialize if the setup did not change.
			if (!overwrite &&
				colliderTable.Count > 0 && slot != null &&   // Slot is set and colliders already populated.
				skeletonGraphic.Skeleton == slot.Skeleton && // Skeleton object did not change.
				slotName == slot.Data.Name                   // Slot object did not change.
			)
				return;

			slot = null;
			currentAttachment = null;
			currentAttachmentName = null;
			currentCollider = null;
			colliderTable.Clear();
			nameTable.Clear();

			Skeleton skeleton = skeletonGraphic.Skeleton;
			if (skeleton == null)
				return;
			slot = skeleton.FindSlot(slotName);
			if (slot == null) {
				if (BoundingBoxFollowerGraphic.DebugMessages)
					Debug.LogWarning(string.Format("Slot '{0}' not found for BoundingBoxFollowerGraphic on '{1}'. (Previous colliders were disposed.)", slotName, this.gameObject.name));
				return;
			}
			int slotIndex = slot.Data.Index;

			int requiredCollidersCount = 0;
			PolygonCollider2D[] colliders = GetComponents<PolygonCollider2D>();
			if (this.gameObject.activeInHierarchy) {
				Canvas canvas = skeletonGraphic.canvas;
				if (canvas == null) canvas = skeletonGraphic.GetComponentInParent<Canvas>();
				float scale = canvas != null ? canvas.referencePixelsPerUnit : 100.0f;

				foreach (Skin skin in skeleton.Data.Skins)
					AddCollidersForSkin(skin, slotIndex, colliders, scale, ref requiredCollidersCount);

				if (skeleton.Skin != null)
					AddCollidersForSkin(skeleton.Skin, slotIndex, colliders, scale, ref requiredCollidersCount);
			}
			DisposeExcessCollidersAfter(requiredCollidersCount);

			if (BoundingBoxFollowerGraphic.DebugMessages) {
				bool valid = colliderTable.Count != 0;
				if (!valid) {
					if (this.gameObject.activeInHierarchy)
						Debug.LogWarning("Bounding Box Follower not valid! Slot [" + slotName + "] does not contain any Bounding Box Attachments!");
					else
						Debug.LogWarning("Bounding Box Follower tried to rebuild as a prefab.");
				}
			}
		}

		void AddCollidersForSkin (Skin skin, int slotIndex, PolygonCollider2D[] previousColliders, float scale, ref int collidersCount) {
			if (skin == null) return;
			List<Skin.SkinEntry> skinEntries = new List<Skin.SkinEntry>();
			skin.GetAttachments(slotIndex, skinEntries);

			foreach (Skin.SkinEntry entry in skinEntries) {
				Attachment attachment = skin.GetAttachment(slotIndex, entry.Name);
				BoundingBoxAttachment boundingBoxAttachment = attachment as BoundingBoxAttachment;

				if (BoundingBoxFollowerGraphic.DebugMessages && attachment != null && boundingBoxAttachment == null)
					Debug.Log("BoundingBoxFollowerGraphic tried to follow a slot that contains non-boundingbox attachments: " + slotName);

				if (boundingBoxAttachment != null) {
					if (!colliderTable.ContainsKey(boundingBoxAttachment)) {
						PolygonCollider2D bbCollider = collidersCount < previousColliders.Length ?
							previousColliders[collidersCount] : gameObject.AddComponent<PolygonCollider2D>();
						++collidersCount;
						SkeletonUtility.SetColliderPointsLocal(bbCollider, slot, boundingBoxAttachment, scale);
						bbCollider.isTrigger = isTrigger;
						bbCollider.usedByEffector = usedByEffector;
						bbCollider.usedByComposite = usedByComposite;
						bbCollider.enabled = false;
						bbCollider.hideFlags = HideFlags.NotEditable;
						colliderTable.Add(boundingBoxAttachment, bbCollider);
						nameTable.Add(boundingBoxAttachment, entry.Name);
					}
				}
			}
		}

		void OnDisable () {
			if (clearStateOnDisable)
				ClearState();

			if (skeletonGraphic != null)
				skeletonGraphic.OnRebuild -= HandleRebuild;
		}

		public void ClearState () {
			if (colliderTable != null)
				foreach (PolygonCollider2D col in colliderTable.Values)
					col.enabled = false;

			currentAttachment = null;
			currentAttachmentName = null;
			currentCollider = null;
		}

		void DisposeExcessCollidersAfter (int requiredCount) {
			PolygonCollider2D[] colliders = GetComponents<PolygonCollider2D>();
			if (colliders.Length == 0) return;

			for (int i = requiredCount; i < colliders.Length; ++i) {
				PolygonCollider2D collider = colliders[i];
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
			BoundingBoxAttachment bbAttachment = attachment as BoundingBoxAttachment;

			if (BoundingBoxFollowerGraphic.DebugMessages && attachment != null && bbAttachment == null)
				Debug.LogWarning("BoundingBoxFollowerGraphic tried to match a non-boundingbox attachment. It will treat it as null.");

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
					if (BoundingBoxFollowerGraphic.DebugMessages) Debug.LogFormat("Collider for BoundingBoxAttachment named '{0}' was not initialized. It is possibly from a new skin. currentAttachmentName will be null. You may need to call BoundingBoxFollowerGraphic.Initialize(overwrite: true);", bbAttachment.Name);
				}
			}
		}
	}

}
