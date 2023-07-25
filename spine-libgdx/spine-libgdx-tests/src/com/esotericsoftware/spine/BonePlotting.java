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

package com.esotericsoftware.spine;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.Animation.MixBlend;
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.Sequence;

/** Demonstrates loading skeleton data without an atlas and plotting bone transform for each animation. */
public class BonePlotting {
	static public void main (String[] args) throws Exception {
		// Create a skeleton loader that doesn't use an atlas and doesn't create any attachments.
		SkeletonJson json = new SkeletonJson(new AttachmentLoader() {
			public RegionAttachment newRegionAttachment (Skin skin, String name, String path, @Null Sequence sequence) {
				return null;
			}

			public MeshAttachment newMeshAttachment (Skin skin, String name, String path, @Null Sequence sequence) {
				return null;
			}

			public BoundingBoxAttachment newBoundingBoxAttachment (Skin skin, String name) {
				return null;
			}

			public ClippingAttachment newClippingAttachment (Skin skin, String name) {
				return null;
			}

			public PathAttachment newPathAttachment (Skin skin, String name) {
				return null;
			}

			public PointAttachment newPointAttachment (Skin skin, String name) {
				return null;
			}
		});

		SkeletonData skeletonData = json.readSkeletonData(new FileHandle("assets/spineboy/spineboy-ess.json"));
		Skeleton skeleton = new Skeleton(skeletonData);
		Bone bone = skeleton.findBone("gun-tip");

		// Pose the skeleton at regular intervals throughout each animation.
		float fps = 1 / 15f;
		for (Animation animation : skeletonData.getAnimations()) {
			float time = 0;
			while (time < animation.getDuration()) {
				animation.apply(skeleton, time, time, false, null, 1, MixBlend.first, MixDirection.in);
				skeleton.updateWorldTransform();

				System.out.println(animation.getName() + "," //
					+ bone.getWorldX() + "," + bone.getWorldY() + "," + bone.getWorldRotationX());

				time += fps;
			}
		}
	}
}
