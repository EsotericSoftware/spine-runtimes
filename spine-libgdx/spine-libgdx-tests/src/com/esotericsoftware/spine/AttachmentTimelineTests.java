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

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.attachments.Attachment;

/** Unit tests for {@link AttachmentTimeline}. */
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

		Attachment attachment1 = new Attachment("attachment1") {};
		Attachment attachment2 = new Attachment("attachment2") {};

		Skin skin = new Skin("skin");
		skin.addAttachment(0, "attachment1", attachment1);
		skin.addAttachment(0, "attachment2", attachment2);
		skeletonData.setDefaultSkin(skin);

		skeleton = new Skeleton(skeletonData);
		slot = skeleton.findSlot("slot");

		AttachmentTimeline timeline = new AttachmentTimeline(2);
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
