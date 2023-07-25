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

import com.badlogic.gdx.utils.Array;

import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.attachments.Attachment;

/** Unit tests to ensure {@link AttachmentTimeline} is working as expected. */
public class AttachmentTimelineTests {
	private final SkeletonData skeletonData;
	private final Skeleton skeleton;
	private Slot slot;
	private AnimationState state;

	public AttachmentTimelineTests () {
		skeletonData = new SkeletonData();

		BoneData boneData = new BoneData(0, "bone", null);
		skeletonData.getBones().add(boneData);

		skeletonData.getSlots().add(new SlotData(0, "slot", boneData));

		Attachment attachment1 = new Attachment("attachment1") {
			public Attachment copy () {
				return null;
			}
		};
		Attachment attachment2 = new Attachment("attachment2") {
			public Attachment copy () {
				return null;
			}
		};

		Skin skin = new Skin("skin");
		skin.setAttachment(0, "attachment1", attachment1);
		skin.setAttachment(0, "attachment2", attachment2);
		skeletonData.setDefaultSkin(skin);

		skeleton = new Skeleton(skeletonData);
		slot = skeleton.findSlot("slot");

		AttachmentTimeline timeline = new AttachmentTimeline(2, 0);
		timeline.setFrame(0, 0, "attachment1");
		timeline.setFrame(1, 0.5f, "attachment2");

		Animation animation = new Animation("animation", Array.with((Timeline)timeline), 1);
		animation.setDuration(1);

		state = new AnimationState(new AnimationStateData(skeletonData));
		state.setAnimation(0, animation, true);

		test(0, attachment1);
		test(0, attachment1);
		test(0.25f, attachment1);
		test(0f, attachment1);
		test(0.25f, attachment2);
		test(0.25f, attachment2);

		System.out.println("AttachmentTimeline tests passed.");
	}

	private void test (float delta, Attachment attachment) {
		state.update(delta);
		state.apply(skeleton);
		if (slot.getAttachment() != attachment)
			throw new FailException("Wrong attachment: " + slot.getAttachment() + " != " + attachment);

	}

	static class FailException extends RuntimeException {
		public FailException (String message) {
			super(message);
		}
	}

	static public void main (String[] args) throws Exception {
		new AttachmentTimelineTests();
	}
}
