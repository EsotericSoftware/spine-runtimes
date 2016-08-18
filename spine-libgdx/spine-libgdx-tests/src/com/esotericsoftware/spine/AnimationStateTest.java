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

import com.badlogic.gdx.Files.FileType;
import com.badlogic.gdx.backends.lwjgl.LwjglFileHandle;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.AnimationState.AnimationStateListener;
import com.esotericsoftware.spine.AnimationState.TrackEntry;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

public class AnimationStateTest {
	final SkeletonJson json = new SkeletonJson(new AttachmentLoader() {
		public RegionAttachment newRegionAttachment (Skin skin, String name, String path) {
			return null;
		}

		public MeshAttachment newMeshAttachment (Skin skin, String name, String path) {
			return null;
		}

		public BoundingBoxAttachment newBoundingBoxAttachment (Skin skin, String name) {
			return null;
		}

		public PathAttachment newPathAttachment (Skin skin, String name) {
			return null;
		}
	});

	final AnimationStateListener stateListener = new AnimationStateListener() {
		public void start (TrackEntry entry) {
			add(actual("start", entry));
		}

		public void event (TrackEntry entry, Event event) {
			add(actual("event " + event.getString(), entry));
		}

		public void interrupt (TrackEntry entry) {
			add(actual("interrupt", entry));
		}

		public void complete (TrackEntry entry, int loopCount) {
			add(actual("complete " + loopCount, entry));
		}

		public void end (TrackEntry entry) {
			add(actual("end", entry));
		}

		private void add (Result result) {
			String error = "PASS";
			if (actual.size >= expected.size) {
				error = "FAIL: <none>";
				fail = true;
			} else if (!expected.get(actual.size).equals(result)) {
				error = "FAIL: " + expected.get(actual.size);
				fail = true;
			}
			buffer.append(result.toString());
			buffer.append(error);
			buffer.append('\n');
			actual.add(result);
		}
	};

	final SkeletonData skeletonData;
	final AnimationStateData stateData;
	final Array<Result> actual = new Array();
	final Array<Result> expected = new Array();
	final StringBuilder buffer = new StringBuilder(512);

	AnimationState state;
	float time = 0;
	boolean fail;
	int test;

	AnimationStateTest () {
		skeletonData = json.readSkeletonData(new LwjglFileHandle("test/test.json", FileType.Internal));
		stateData = new AnimationStateData(skeletonData);

		setup( // 1
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete 1", 1, 1), //
			expect(0, "end", 1, 1.1f) //
		);
		state.setAnimation(0, "events1", false);
		run(0.1f, 1000);

		setup( // 2
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.467f, 0.467f), //
			expect(0, "event 30", 1.017f, 1.017f), //
			expect(0, "complete 1", 1.017f, 1.017f), //
			expect(0, "end", 1.017f, 1.033f) //
		);
		state.setAnimation(0, "events1", false);
		run(1 / 60f, 1000);

		setup( // 3
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 30, 30), //
			expect(0, "event 30", 30, 30), //
			expect(0, "complete 1", 30, 30), //
			expect(0, "end", 30, 60) //
		);
		state.setAnimation(0, "events1", false);
		run(30, 1000);

		setup( // 4
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 1, 1), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete 1", 1, 1), //
			expect(0, "end", 1, 2) //
		);
		state.setAnimation(0, "events1", false);
		run(1, 1.01f);

		setup( // 5
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete 1", 1, 1), //
			expect(0, "event 0", 1, 1), //
			expect(0, "event 14", 1.5f, 1.5f), //
			expect(0, "event 30", 2, 2), //
			expect(0, "complete 2", 2, 2), //
			expect(0, "event 0", 2, 2) //
		);
		state.setAnimation(0, "events1", true);
		run(0.1f, 2.3f);

		setup( // 6
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete 1", 1, 1), //

			expect(1, "start", 0.1f, 1.1f), //

			expect(0, "interrupt", 1.1f, 1.1f), //
			expect(0, "end", 1.1f, 1.1f), //

			expect(1, "event 0", 0.1f, 1.1f), //
			expect(1, "event 14", 0.5f, 1.5f), //
			expect(1, "event 30", 1, 2), //
			expect(1, "complete 1", 1, 2), //

			expect(0, "start", 0.1f, 2.1f), //

			expect(1, "interrupt", 1.1f, 2.1f), //
			expect(1, "end", 1.1f, 2.1f), //

			expect(0, "event 0", 0.1f, 2.1f), //
			expect(0, "event 14", 0.5f, 2.5f), //
			expect(0, "event 30", 1, 3), //
			expect(0, "complete 1", 1, 3), //
			expect(0, "end", 1, 3.1f) //
		);
		state.setAnimation(0, "events1", false);
		state.addAnimation(0, "events2", false, 0);
		state.addAnimation(0, "events1", false, 0);
		run(0.1f, 4f);

		setup( // 7
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //

			expect(1, "start", 0.1f, 0.6f), //

			expect(0, "interrupt", 0.6f, 0.6f), //
			expect(0, "end", 0.6f, 0.6f), //

			expect(1, "event 0", 0.1f, 0.6f), //
			expect(1, "event 14", 0.5f, 1.0f), //
			expect(1, "event 30", 1, 1.5f), //
			expect(1, "complete 1", 1, 1.5f), //
			expect(1, "end", 1, 1.6f) //
		);
		state.setAnimation(0, "events1", false);
		state.addAnimation(0, "events2", false, 0.5f);
		run(0.1f, 1000);

		setup( // 8
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //

			expect(1, "start", 0.1f, 1), //

			expect(0, "interrupt", 1, 1), //
			expect(0, "complete 1", 1, 1), //

			expect(1, "event 0", 0.1f, 1), //
			expect(1, "event 14", 0.5f, 1.4f), //

			expect(0, "end", 1.6f, 1.6f), //

			expect(1, "event 30", 1, 1.9f), //
			expect(1, "complete 1", 1, 1.9f), //
			expect(1, "end", 1, 2) //
		);
		stateData.setMix("events1", "events2", 0.7f);
		state.setAnimation(0, "events1", true);
		state.addAnimation(0, "events2", false, 0.9f);
		run(0.1f, 1000);

		setup( // 9, first animation's events fire during mix
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //

			expect(1, "start", 0.1f, 0.5f), //

			expect(0, "interrupt", 0.5f, 0.5f), //

			expect(1, "event 0", 0.1f, 0.5f), //
			expect(1, "event 14", 0.5f, 0.9f), //

			expect(0, "complete 1", 1, 1), //
			expect(0, "end", 1.1f, 1.1f), //

			expect(1, "event 30", 1, 1.4f), //
			expect(1, "complete 1", 1, 1.4f), //
			expect(1, "end", 1, 1.5f) //
		);
		state.setAnimation(0, "events1", false);
		state.addAnimation(0, "events2", false, 0.4f);
		run(0.1f, 1000);

		setup( // 10, only some of first animation's events fire during mix due to event threshold
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //

			expect(1, "start", 0.1f, 0.5f), //

			expect(0, "interrupt", 0.5f, 0.5f), //
			expect(0, "event 14", 0.5f, 0.5f), //

			expect(1, "event 0", 0.1f, 0.5f), //
			expect(1, "event 14", 0.5f, 0.9f), //

			expect(0, "complete 1", 1, 1), //
			expect(0, "end", 1.1f, 1.1f), //

			expect(1, "event 30", 1, 1.4f), //
			expect(1, "complete 1", 1, 1.4f), //
			expect(1, "end", 1, 1.5f) //
		);
		state.setEventThreshold(0.5f);
		state.setAnimation(0, "events1", false);
		state.addAnimation(0, "events2", false, 0.4f);
		run(0.1f, 1000);

		setup( // 11, first animation's events do not fire during mix due to event threshold
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //

			expect(1, "start", 0.1f, 0.5f), //

			expect(0, "interrupt", 0.5f, 0.5f), //
			expect(0, "event 14", 0.5f, 0.5f), //

			expect(1, "event 0", 0.1f, 0.5f), //
			expect(1, "event 14", 0.5f, 0.9f), //

			expect(0, "event 30", 1, 1), //
			expect(0, "complete 1", 1, 1), //
			expect(0, "end", 1.1f, 1.1f), //

			expect(1, "event 30", 1, 1.4f), //
			expect(1, "complete 1", 1, 1.4f), //
			expect(1, "end", 1, 1.5f) //
		);
		state.setEventThreshold(1);
		state.setAnimation(0, "events1", false);
		state.addAnimation(0, "events2", false, 0.4f);
		run(0.1f, 1000);

		System.out.println("AnimationState tests passed.");
	}

	void setup (Result... expectedArray) {
		test++;
		expected.addAll(expectedArray);
		state = new AnimationState(stateData);
		state.addListener(stateListener);
		time = 0;
		fail = false;
		buffer.setLength(0);
		buffer.append(String.format("%-12s%-8s%-8s%-8s%s\n", "#" + test, "anim", "track", "total", "result"));
	}

	void run (float incr, float endTime) {
		Skeleton skeleton = new Skeleton(skeletonData);
		state.apply(skeleton);
		while (time < endTime) {
			time += incr;
			skeleton.update(incr);
			state.update(incr);
			state.apply(skeleton);
		}
		actual.clear();
		expected.clear();
		if (fail) {
			System.out.println(buffer);
			System.out.println("TEST FAILED!");
			System.exit(0);
		}
		System.out.println(buffer);
	}

	Result expect (int animationIndex, String name, float trackTime, float totalTime) {
		Result result = new Result();
		result.name = name;
		result.animationIndex = animationIndex;
		result.trackTime = trackTime;
		result.totalTime = totalTime;
		return result;
	}

	Result actual (String name, TrackEntry entry) {
		Result result = new Result();
		result.name = name;
		result.animationIndex = skeletonData.getAnimations().indexOf(entry.animation, true);
		result.trackTime = Math.round(entry.time * 1000) / 1000f;
		result.totalTime = Math.round(time * 1000) / 1000f;
		return result;
	}

	class Result {
		String name;
		int animationIndex;
		float trackTime, totalTime;

		public int hashCode () {
			int result = 31 + animationIndex;
			result = 31 * result + name.hashCode();
			result = 31 * result + Float.floatToIntBits(totalTime);
			result = 31 * result + Float.floatToIntBits(trackTime);
			return result;
		}

		public boolean equals (Object obj) {
			Result other = (Result)obj;
			if (animationIndex != other.animationIndex) return false;
			if (!name.equals(other.name)) return false;
			if (totalTime != other.totalTime) return false;
			if (trackTime != other.trackTime) return false;
			return true;
		}

		public String toString () {
			return String.format("%-12s%-8s%-8s%-8s", name, "" + animationIndex, "" + trackTime, "" + totalTime);
		}

	}

	static public void main (String[] args) throws Exception {
		new AnimationStateTest();
	}
}
