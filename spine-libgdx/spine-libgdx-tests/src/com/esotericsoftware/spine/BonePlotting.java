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

package com.esotericsoftware.spine;

import com.badlogic.gdx.files.FileHandle;
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.Animation.MixPose;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;

public class BonePlotting {
	static public void main (String[] args) throws Exception {
		// This example shows how to load skeleton data and plot a bone transform for each animation.
		SkeletonJson json = new SkeletonJson(new AttachmentLoader() {
			public RegionAttachment newRegionAttachment (Skin skin, String name, String path) {
				return null;
			}

			public MeshAttachment newMeshAttachment (Skin skin, String name, String path) {
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
		float fps = 1 / 15f;
		for (Animation animation : skeletonData.getAnimations()) {
			float time = 0;
			while (time < animation.getDuration()) {
				animation.apply(skeleton, time, time, false, null, 1, MixPose.current, MixDirection.in);
				skeleton.updateWorldTransform();
				System.out
					.println(animation.getName() + "," + bone.getWorldX() + "," + bone.getWorldY() + "," + bone.getWorldRotationX());
				time += fps;
			}
		}
	}
}
