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

import com.badlogic.gdx.Files.FileType;
import com.badlogic.gdx.backends.lwjgl.LwjglFileHandle;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.AnimationState.AnimationStateListener;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;

public class AnimationStateTest {
	final SkeletonJson json = new SkeletonJson(new AttachmentLoader() {
		public MeshAttachment newMeshAttachment (Skin skin, String name, String path) {
			return null;
		}

		public RegionAttachment newRegionAttachment (Skin skin, String name, String path) {
			return null;
		}

		public BoundingBoxAttachment newBoundingBoxAttachment (Skin skin, String name) {
			return null;
		}

		public PathAttachment newPathAttachment (Skin skin, String name) {
			return null;
		}
	});

	AnimationStateListener stateListener = new AnimationStateListener() {
		public void start (int trackIndex) {
			actual.add(new Result("start", null));
		}

		public void event (int trackIndex, Event event) {
			actual.add(new Result("event", event.getString()));
		}

		public void complete (int trackIndex, int loopCount) {
			actual.add(new Result("complete", null));
		}

		public void end (int trackIndex) {
			actual.add(new Result("end", null));
		}
	};

	final SkeletonData skeletonData;
	final AnimationStateData stateData;
	final Array<Result> actual = new Array();

	public AnimationStateTest () {
		skeletonData = json.readSkeletonData(new LwjglFileHandle("test/test.json", FileType.Internal));
		stateData = new AnimationStateData(skeletonData);

		AnimationState state;

		state = newState();
		state.setAnimation(0, "events", false);
		test(state, 1 / 60f, 1000, //
			new Result("start", null), //
			new Result("event", "0"), //
			new Result("event", "14"), //
			new Result("event", "30"), //
			new Result("complete", null), //
			new Result("end", null) //
		);

		state = newState();
		state.setAnimation(0, "events", false);
		test(state, 30, 1000, //
			new Result("start", null), //
			new Result("event", "0"), //
			new Result("event", "14"), //
			new Result("event", "30"), //
			new Result("complete", null), //
			new Result("end", null) //
		);

		state = newState();
		state.setAnimation(0, "events", false);
		test(state, 1, 1.01f, //
			new Result("start", null), //
			new Result("event", "0"), //
			new Result("event", "14"), //
			new Result("event", "30"), //
			new Result("complete", null), //
			new Result("end", null) //
		);

		state = newState();
		state.setAnimation(0, "events", false);
		state.addAnimation(0, "events", false, 0);
		test(state, 0.1f, 3f, //
			new Result("start", null), //
			new Result("event", "0"), //
			new Result("event", "14"), //
			new Result("event", "30"), //
			new Result("complete", null), //
			new Result("end", null), //
			new Result("start", null), //
			new Result("event", "0"), //
			new Result("event", "14"), //
			new Result("event", "30"), //
			new Result("complete", null), //
			new Result("end", null) //
		);
	}

	private AnimationState newState () {
		AnimationState state = new AnimationState(stateData);
		state.addListener(stateListener);
		return state;
	}

	private void test (AnimationState state, float incr, float endTime, Result... expectedArray) {
		Array expected = new Array(expectedArray);

		Skeleton skeleton = new Skeleton(skeletonData);

		for (int i = 0; i < endTime; i++) {
			skeleton.update(incr);
			state.update(incr);
			state.apply(skeleton);
		}

		if (expected.equals(actual)) {
			actual.clear();
			return;
		}
		int i = 0;
		for (int n = expected.size; i < n; i++) {
			System.out.print(expected.get(i) + " == " + (i < actual.size ? actual.get(i) : ""));
			if (i >= actual.size || !actual.get(i).equals(expected.get(i)))
				System.out.println(" <- FAIL");
			else
				System.out.println();
		}
		for (int n = actual.size; i < n; i++)
			System.out.print(" == " + actual.get(i) + " <- FAIL");
		System.exit(0);
	}

	static public class Result {
		String eventName;
		String payload;

		public Result (String eventName, String payload) {
			this.eventName = eventName;
			this.payload = payload;
		}

		public int hashCode () {
			final int prime = 31;
			int result = 1;
			result = prime * result + ((eventName == null) ? 0 : eventName.hashCode());
			result = prime * result + ((payload == null) ? 0 : payload.hashCode());
			return result;
		}

		public boolean equals (Object obj) {
			if (this == obj) return true;
			if (obj == null) return false;
			if (getClass() != obj.getClass()) return false;
			Result other = (Result)obj;
			if (eventName == null) {
				if (other.eventName != null) return false;
			} else if (!eventName.equals(other.eventName)) return false;
			if (payload == null) {
				if (other.payload != null) return false;
			} else if (!payload.equals(other.payload)) return false;
			return true;
		}

		public String toString () {
			return "[" + eventName + ", " + payload + "]";
		}
	}

	static public void main (String[] args) throws Exception {
		new AnimationStateTest();
	}
}
