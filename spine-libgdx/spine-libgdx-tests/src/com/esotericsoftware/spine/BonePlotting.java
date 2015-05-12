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

package com.esotericsoftware.spine;

import com.badlogic.gdx.files.FileHandle;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.SkinnedMeshAttachment;

public class BonePlotting {
	static public void main (String[] args) throws Exception {
		// This example shows how to load skeleton data and plot a bone transform for each animation.
		SkeletonJson json = new SkeletonJson(new AttachmentLoader() {
			public SkinnedMeshAttachment newSkinnedMeshAttachment (Skin skin, String name, String path) {
				return null;
			}

			public RegionAttachment newRegionAttachment (Skin skin, String name, String path) {
				return null;
			}

			public MeshAttachment newMeshAttachment (Skin skin, String name, String path) {
				return null;
			}

			public BoundingBoxAttachment newBoundingBoxAttachment (Skin skin, String name) {
				return null;
			}
		});
		SkeletonData skeletonData = json.readSkeletonData(new FileHandle("assets/spineboy/spineboy.json"));
		Skeleton skeleton = new Skeleton(skeletonData);
		Bone bone = skeleton.findBone("gunTip");
		float fps = 1 / 15f;
		for (Animation animation : skeletonData.getAnimations()) {
			float time = 0;
			while (time < animation.getDuration()) {
				animation.apply(skeleton, time, time, false, null);
				skeleton.updateWorldTransform();
				System.out.println(animation.getName() + "," + bone.getWorldX() + "," + bone.getWorldY() + ","
					+ bone.getWorldRotation() + "," + bone.getWorldScaleX() + "," + bone.getWorldScaleY());
				time += fps;
			}
		}
	}
}
