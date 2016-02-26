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
using System.Collections;
using System.Collections.Generic;
using Spine;

[ExecuteInEditMode]
public class BoundingBoxFollower3D : MonoBehaviour
{

    public SkeletonRenderer skeletonRenderer;

    [SpineSlot(dataField: "skeletonRenderer", containsBoundingBoxes: true)]
    public string slotName;

    public float Thickness = 0.125f;
    public bool IsTrigger = true;
    public Collider[] IgnoreCollisions;

    private Slot slot;
    private BoundingBoxAttachment currentAttachment;
    private Collider currentCollider;
    private string currentAttachmentName;
    private bool valid = false;
    private bool hasReset;

    [SpineAttachment(false, false, false, "slotName", "skeletonRenderer")]
    public string attachmentName;

    public Dictionary<BoundingBoxAttachment, Collider> colliderTable = new Dictionary<BoundingBoxAttachment, Collider>();
    public Dictionary<BoundingBoxAttachment, string> attachmentNameTable = new Dictionary<BoundingBoxAttachment, string>();

    public string CurrentAttachmentName
    {
        get
        {
            return currentAttachmentName;
        }
    }

    public BoundingBoxAttachment CurrentAttachment
    {
        get
        {
            return currentAttachment;
        }
    }

    public Collider CurrentCollider
    {
        get
        {
            return currentCollider;
        }
    }

    public Slot Slot
    {
        get
        {
            return slot;
        }
    }


    void OnEnable()
    {
        ClearColliders();

        if (skeletonRenderer == null)
            skeletonRenderer = GetComponentInParent<SkeletonRenderer>();

        if (skeletonRenderer != null)
        {
            skeletonRenderer.OnRebuild -= HandleReset;
            skeletonRenderer.OnRebuild += HandleReset;
        }
    }

    void OnDisable()
    {
        skeletonRenderer.OnRebuild -= HandleReset;
    }

    void Start()
    {
        if (!hasReset && skeletonRenderer != null)
            HandleReset(skeletonRenderer);
    }

    public void HandleReset(SkeletonRenderer renderer)
    {
        if (slotName == null || slotName == "")
            return;

        if (string.IsNullOrEmpty(attachmentName))
        {
            return;
        }

        hasReset = true;

        ClearColliders();
        colliderTable.Clear();

        if (skeletonRenderer.skeleton == null)
        {
            skeletonRenderer.OnRebuild -= HandleReset;
            skeletonRenderer.Initialize(false);
            skeletonRenderer.OnRebuild += HandleReset;
        }

        var skeleton = skeletonRenderer.skeleton;
        slot = skeleton.FindSlot(slotName);
        int slotIndex = skeleton.FindSlotIndex(slotName);


        var attachment = skeleton.GetAttachment(slotIndex, attachmentName);
        AddBoundingBoxAttachment(attachment as BoundingBoxAttachment);

        if (colliderTable.Count == 0)
            valid = false;
        else
            valid = true;

        if (!valid)
            Debug.LogWarning("Bounding Box Follower not valid! Slot [" + slotName + "] does not contain any Bounding Box Attachments!");
    }

    private void AddBoundingBoxAttachment(BoundingBoxAttachment attachment)
    {
        if (attachment == null) return;

        var collider = SkeletonUtility.AddBoundingBoxAsComponent_3D(attachment, gameObject, IsTrigger, Thickness);
        collider.enabled = false;
        collider.hideFlags = HideFlags.HideInInspector;
        colliderTable.Add(attachment, collider);
        attachmentNameTable.Add(attachment, name);

        for (int j = 0; j < IgnoreCollisions.Length; j++)
        {
            if (IgnoreCollisions[j] != null)
            {
                Physics.IgnoreCollision(collider, IgnoreCollisions[j]);
            }
        }
    }

    void ClearColliders()
    {
        var colliders = GetComponents<Collider>();
        if (Application.isPlaying)
        {
            foreach (var c in colliders)
            {
                Destroy(c);
            }
        }
        else
        {
            foreach (var c in colliders)
            {
                DestroyImmediate(c);
            }
        }

        colliderTable.Clear();
        attachmentNameTable.Clear();
    }

    void LateUpdate()
    {
        if (skeletonRenderer == null)
            return;

        if (!skeletonRenderer.valid)
            return;

        if (slot != null)
        {
            if (slot.Attachment != currentAttachment)
            {
                var boxAttachment = (BoundingBoxAttachment)slot.Attachment;
                SetCurrent(boxAttachment);
            }
        }
    }

    void SetCurrent(BoundingBoxAttachment attachment)
    {
        if (currentCollider)
            currentCollider.enabled = false;

        if (attachment != null)
        {
            currentCollider = colliderTable[attachment];
            currentCollider.enabled = true;
        }
        else
        {
            currentCollider = null;
        }

        currentAttachment = attachment;

        currentAttachmentName = currentAttachment == null ? null : attachmentNameTable[attachment];
    }
}
