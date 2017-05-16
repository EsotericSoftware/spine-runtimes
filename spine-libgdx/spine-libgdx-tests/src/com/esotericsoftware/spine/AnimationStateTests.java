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

import java.util.concurrent.atomic.AtomicInteger;

import com.badlogic.gdx.Files.FileType;
import com.badlogic.gdx.backends.lwjgl.LwjglFileHandle;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Pool;
import com.esotericsoftware.spine.AnimationState.AnimationStateListener;
import com.esotericsoftware.spine.AnimationState.TrackEntry;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

public class AnimationStateTests {
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

	final AnimationStateListener stateListener = new AnimationStateListener() {
		public void start (TrackEntry entry) {
			add(actual("start", entry));
		}

		public void interrupt (TrackEntry entry) {
			add(actual("interrupt", entry));
		}

		public void end (TrackEntry entry) {
			add(actual("end", entry));
		}

		public void dispose (TrackEntry entry) {
			add(actual("dispose", entry));
		}

		public void complete (TrackEntry entry) {
			add(actual("complete", entry));
		}

		public void event (TrackEntry entry, Event event) {
			add(actual("event " + event.getString(), entry));
		}

		private void add (Result result) {
			while (expected.size > actual.size) {
				Result note = expected.get(actual.size);
				if (!note.note) break;
				actual.add(note);
				log(note.name);
			}

			String message = result.toString();
			if (actual.size >= expected.size) {
				message += "FAIL: <none>";
				fail = true;
			} else if (!expected.get(actual.size).equals(result)) {
				message += "FAIL: " + expected.get(actual.size);
				fail = true;
			} else
				message += "PASS";
			log(message);
			actual.add(result);
		}
	};

	final SkeletonData skeletonData;
	final Array<Result> actual = new Array();
	final Array<Result> expected = new Array();

	AnimationStateData stateData;
	AnimationState state;
	int entryCount;
	float time = 0;
	boolean fail;
	int test;

	AnimationStateTests () {
		skeletonData = json.readSkeletonData(new LwjglFileHandle("test/test.json", FileType.Internal));

		TrackEntry entry;

		setup("0.1 time step", // 1
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "end", 1, 1.1f), //
			expect(0, "dispose", 1, 1.1f) //
		);
		state.setAnimation(0, "events0", false).setTrackEnd(1);
		run(0.1f, 1000, null);

		setup("1/60 time step, dispose queued", // 2
			expect(0, "start", 0, 0), //
			expect(0, "interrupt", 0, 0), //
			expect(0, "end", 0, 0), //
			expect(0, "dispose", 0, 0), //
			expect(1, "dispose", 0, 0), //
			expect(0, "dispose", 0, 0), //
			expect(1, "dispose", 0, 0), //

			note("First 2 set/addAnimation calls are done."),

			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.483f, 0.483f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "end", 1, 1.017f), //
			expect(0, "dispose", 1, 1.017f) //
		);
		state.setAnimation(0, "events0", false);
		state.addAnimation(0, "events1", false, 0);
		state.addAnimation(0, "events0", false, 0);
		state.addAnimation(0, "events1", false, 0);
		state.setAnimation(0, "events0", false).setTrackEnd(1);
		run(1 / 60f, 1000, null);

		setup("30 time step", // 3
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 30, 30), //
			expect(0, "event 30", 30, 30), //
			expect(0, "complete", 30, 30), //
			expect(0, "end", 30, 60), //
			expect(0, "dispose", 30, 60) //
		);
		state.setAnimation(0, "events0", false).setTrackEnd(1);
		run(30, 1000, null);

		setup("1 time step", // 4
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 1, 1), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "end", 1, 2), //
			expect(0, "dispose", 1, 2) //
		);
		state.setAnimation(0, "events0", false).setTrackEnd(1);
		run(1, 1.01f, null);

		setup("interrupt", // 5
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "interrupt", 1.1f, 1.1f), //

			expect(1, "start", 0.1f, 1.1f), //
			expect(1, "event 0", 0.1f, 1.1f), //

			expect(0, "end", 1.1f, 1.2f), //
			expect(0, "dispose", 1.1f, 1.2f), //

			expect(1, "event 14", 0.5f, 1.5f), //
			expect(1, "event 30", 1, 2), //
			expect(1, "complete", 1, 2), //
			expect(1, "interrupt", 1.1f, 2.1f), //

			expect(0, "start", 0.1f, 2.1f), //
			expect(0, "event 0", 0.1f, 2.1f), //

			expect(1, "end", 1.1f, 2.2f), //
			expect(1, "dispose", 1.1f, 2.2f), //

			expect(0, "event 14", 0.5f, 2.5f), //
			expect(0, "event 30", 1, 3), //
			expect(0, "complete", 1, 3), //
			expect(0, "end", 1, 3.1f), //
			expect(0, "dispose", 1, 3.1f) //
		);
		state.setAnimation(0, "events0", false);
		state.addAnimation(0, "events1", false, 0);
		state.addAnimation(0, "events0", false, 0).setTrackEnd(1);
		run(0.1f, 4f, null);

		setup("interrupt with delay", // 6
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "interrupt", 0.6f, 0.6f), //

			expect(1, "start", 0.1f, 0.6f), //
			expect(1, "event 0", 0.1f, 0.6f), //

			expect(0, "end", 0.6f, 0.7f), //
			expect(0, "dispose", 0.6f, 0.7f), //

			expect(1, "event 14", 0.5f, 1.0f), //
			expect(1, "event 30", 1, 1.5f), //
			expect(1, "complete", 1, 1.5f), //
			expect(1, "end", 1, 1.6f), //
			expect(1, "dispose", 1, 1.6f) //
		);
		state.setAnimation(0, "events0", false);
		state.addAnimation(0, "events1", false, 0.5f).setTrackEnd(1);
		run(0.1f, 1000, null);

		setup("interrupt with delay and mix time", // 7
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "interrupt", 1, 1), //

			expect(1, "start", 0.1f, 1), //

			expect(0, "complete", 1, 1), //

			expect(1, "event 0", 0.1f, 1), //
			expect(1, "event 14", 0.5f, 1.4f), //

			expect(0, "end", 1.6f, 1.7f), //
			expect(0, "dispose", 1.6f, 1.7f), //

			expect(1, "event 30", 1, 1.9f), //
			expect(1, "complete", 1, 1.9f), //
			expect(1, "end", 1, 2), //
			expect(1, "dispose", 1, 2) //
		);
		stateData.setMix("events0", "events1", 0.7f);
		state.setAnimation(0, "events0", true);
		state.addAnimation(0, "events1", false, 0.9f).setTrackEnd(1);
		run(0.1f, 1000, null);

		setup("animation 0 events do not fire during mix", // 8
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "interrupt", 0.5f, 0.5f), //

			expect(1, "start", 0.1f, 0.5f), //
			expect(1, "event 0", 0.1f, 0.5f), //
			expect(1, "event 14", 0.5f, 0.9f), //

			expect(0, "complete", 1, 1), //
			expect(0, "end", 1.1f, 1.2f), //
			expect(0, "dispose", 1.1f, 1.2f), //

			expect(1, "event 30", 1, 1.4f), //
			expect(1, "complete", 1, 1.4f), //
			expect(1, "end", 1, 1.5f), //
			expect(1, "dispose", 1, 1.5f) //
		);
		stateData.setDefaultMix(0.7f);
		state.setAnimation(0, "events0", false);
		state.addAnimation(0, "events1", false, 0.4f).setTrackEnd(1);
		run(0.1f, 1000, null);

		setup("event threshold, some animation 0 events fire during mix", // 9
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "interrupt", 0.5f, 0.5f), //

			expect(1, "start", 0.1f, 0.5f), //

			expect(0, "event 14", 0.5f, 0.5f), //

			expect(1, "event 0", 0.1f, 0.5f), //
			expect(1, "event 14", 0.5f, 0.9f), //

			expect(0, "complete", 1, 1), //
			expect(0, "end", 1.1f, 1.2f), //
			expect(0, "dispose", 1.1f, 1.2f), //

			expect(1, "event 30", 1, 1.4f), //
			expect(1, "complete", 1, 1.4f), //
			expect(1, "end", 1, 1.5f), //
			expect(1, "dispose", 1, 1.5f) //
		);
		stateData.setMix("events0", "events1", 0.7f);
		state.setAnimation(0, "events0", false).setEventThreshold(0.5f);
		state.addAnimation(0, "events1", false, 0.4f).setTrackEnd(1);
		run(0.1f, 1000, null);

		setup("event threshold, all animation 0 events fire during mix", // 10
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "interrupt", 0.9f, 0.9f), //

			expect(1, "start", 0.1f, 0.9f), //
			expect(1, "event 0", 0.1f, 0.9f), //

			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "event 0", 1, 1), //

			expect(1, "event 14", 0.5f, 1.3f), //

			expect(0, "end", 1.5f, 1.6f), //
			expect(0, "dispose", 1.5f, 1.6f), //

			expect(1, "event 30", 1, 1.8f), //
			expect(1, "complete", 1, 1.8f), //
			expect(1, "end", 1, 1.9f), //
			expect(1, "dispose", 1, 1.9f) //
		);
		state.setAnimation(0, "events0", true).setEventThreshold(1);
		entry = state.addAnimation(0, "events1", false, 0.8f);
		entry.setMixDuration(0.7f);
		entry.setTrackEnd(1);
		run(0.1f, 1000, null);

		setup("looping", // 11
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "event 0", 1, 1), //
			expect(0, "event 14", 1.5f, 1.5f), //
			expect(0, "event 30", 2, 2), //
			expect(0, "complete", 2, 2), //
			expect(0, "event 0", 2, 2), //
			expect(0, "event 14", 2.5f, 2.5f), //
			expect(0, "event 30", 3, 3), //
			expect(0, "complete", 3, 3), //
			expect(0, "event 0", 3, 3), //
			expect(0, "event 14", 3.5f, 3.5f), //
			expect(0, "event 30", 4, 4), //
			expect(0, "complete", 4, 4), //
			expect(0, "event 0", 4, 4), //
			expect(0, "end", 4.1f, 4.1f), //
			expect(0, "dispose", 4.1f, 4.1f) //
		);
		state.setAnimation(0, "events0", true);
		run(0.1f, 4, null);

		setup("not looping, track end past animation 0 duration", // 12
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "interrupt", 2.1f, 2.1f), //

			expect(1, "start", 0.1f, 2.1f), //
			expect(1, "event 0", 0.1f, 2.1f), //

			expect(0, "end", 2.1f, 2.2f), //
			expect(0, "dispose", 2.1f, 2.2f), //

			expect(1, "event 14", 0.5f, 2.5f), //
			expect(1, "event 30", 1, 3), //
			expect(1, "complete", 1, 3), //
			expect(1, "end", 1, 3.1f), //
			expect(1, "dispose", 1, 3.1f) //
		);
		state.setAnimation(0, "events0", false);
		state.addAnimation(0, "events1", false, 2).setTrackEnd(1);
		run(0.1f, 4f, null);

		setup("interrupt animation after first loop complete", // 13
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "event 0", 1, 1), //
			expect(0, "event 14", 1.5f, 1.5f), //
			expect(0, "event 30", 2, 2), //
			expect(0, "complete", 2, 2), //
			expect(0, "event 0", 2, 2), //
			expect(0, "interrupt", 2.1f, 2.1f), //

			expect(1, "start", 0.1f, 2.1f), //
			expect(1, "event 0", 0.1f, 2.1f), //

			expect(0, "end", 2.1f, 2.2f), //
			expect(0, "dispose", 2.1f, 2.2f), //

			expect(1, "event 14", 0.5f, 2.5f), //
			expect(1, "event 30", 1, 3), //
			expect(1, "complete", 1, 3), //
			expect(1, "end", 1, 3.1f), //
			expect(1, "dispose", 1, 3.1f) //
		);
		state.setAnimation(0, "events0", true);
		run(0.1f, 6, new TestListener() {
			public void frame (float time) {
				if (MathUtils.isEqual(time, 1.4f)) state.addAnimation(0, "events1", false, 0).setTrackEnd(1);
			}
		});

		setup("add animation on empty track", // 14
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "end", 1, 1.1f), //
			expect(0, "dispose", 1, 1.1f) //
		);
		state.addAnimation(0, "events0", false, 0).setTrackEnd(1);
		run(0.1f, 1.9f, null);

		setup("end time beyond non-looping animation duration", // 15
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "end", 9f, 9.1f), //
			expect(0, "dispose", 9f, 9.1f) //
		);
		state.setAnimation(0, "events0", false).setTrackEnd(9);
		run(0.1f, 10, null);

		setup("looping with animation start", // 16
			expect(0, "start", 0, 0), //
			expect(0, "event 30", 0.4f, 0.4f), //
			expect(0, "complete", 0.4f, 0.4f), //
			expect(0, "event 30", 0.8f, 0.8f), //
			expect(0, "complete", 0.8f, 0.8f), //
			expect(0, "event 30", 1.2f, 1.2f), //
			expect(0, "complete", 1.2f, 1.2f), //
			expect(0, "end", 1.4f, 1.4f), //
			expect(0, "dispose", 1.4f, 1.4f) //
		);
		entry = state.setAnimation(0, "events0", true);
		entry.setAnimationLast(0.6f);
		entry.setAnimationStart(0.6f);
		run(0.1f, 1.4f, null);

		setup("looping with animation start and end", // 17
			expect(0, "start", 0, 0), //
			expect(0, "event 14", 0.3f, 0.3f), //
			expect(0, "complete", 0.6f, 0.6f), //
			expect(0, "event 14", 0.9f, 0.9f), //
			expect(0, "complete", 1.2f, 1.2f), //
			expect(0, "event 14", 1.5f, 1.5f), //
			expect(0, "end", 1.8f, 1.8f), //
			expect(0, "dispose", 1.8f, 1.8f) //
		);
		entry = state.setAnimation(0, "events0", true);
		entry.setAnimationStart(0.2f);
		entry.setAnimationLast(0.2f);
		entry.setAnimationEnd(0.8f);
		run(0.1f, 1.8f, null);

		setup("non-looping with animation start and end", // 18
			expect(0, "start", 0, 0), //
			expect(0, "event 14", 0.3f, 0.3f), //
			expect(0, "complete", 0.6f, 0.6f), //
			expect(0, "end", 1, 1.1f), //
			expect(0, "dispose", 1, 1.1f) //
		);
		entry = state.setAnimation(0, "events0", false);
		entry.setAnimationStart(0.2f);
		entry.setAnimationLast(0.2f);
		entry.setAnimationEnd(0.8f);
		entry.setTrackEnd(1);
		run(0.1f, 1.8f, null);

		setup("mix out looping with animation start and end", // 19
			expect(0, "start", 0, 0), //
			expect(0, "event 14", 0.3f, 0.3f), //
			expect(0, "complete", 0.6f, 0.6f), //
			expect(0, "interrupt", 0.8f, 0.8f), //

			expect(1, "start", 0.1f, 0.8f), //
			expect(1, "event 0", 0.1f, 0.8f), //

			expect(0, "event 14", 0.9f, 0.9f), //
			expect(0, "complete", 1.2f, 1.2f), //

			expect(1, "event 14", 0.5f, 1.2f), //

			expect(0, "end", 1.4f, 1.5f), //
			expect(0, "dispose", 1.4f, 1.5f), //

			expect(1, "event 30", 1, 1.7f), //
			expect(1, "complete", 1, 1.7f), //
			expect(1, "end", 1, 1.8f), //
			expect(1, "dispose", 1, 1.8f) //
		);
		entry = state.setAnimation(0, "events0", true);
		entry.setAnimationStart(0.2f);
		entry.setAnimationLast(0.2f);
		entry.setAnimationEnd(0.8f);
		entry.setEventThreshold(1);
		entry = state.addAnimation(0, "events1", false, 0.7f);
		entry.setMixDuration(0.7f);
		entry.setTrackEnd(1);
		run(0.1f, 20, null);

		setup("setAnimation with track entry mix", // 20
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "event 30", 1, 1), //
			expect(0, "complete", 1, 1), //
			expect(0, "event 0", 1, 1), //
			expect(0, "interrupt", 1, 1), //

			expect(1, "start", 0, 1), //

			expect(1, "event 0", 0.1f, 1.1f), //
			expect(1, "event 14", 0.5f, 1.5f), //

			expect(0, "end", 1.7f, 1.8f), //
			expect(0, "dispose", 1.7f, 1.8f), //

			expect(1, "event 30", 1, 2), //
			expect(1, "complete", 1, 2), //
			expect(1, "end", 1, 2.1f), //
			expect(1, "dispose", 1, 2.1f) //
		);
		state.setAnimation(0, "events0", true);
		run(0.1f, 1000, new TestListener() {
			public void frame (float time) {
				if (MathUtils.isEqual(time, 1f)) {
					TrackEntry entry = state.setAnimation(0, "events1", false);
					entry.setMixDuration(0.7f);
					entry.setTrackEnd(1);
				}
			}
		});

		setup("setAnimation twice", // 21
			expect(0, "start", 0, 0), //
			expect(0, "interrupt", 0, 0), //
			expect(0, "end", 0, 0), //
			expect(0, "dispose", 0, 0), //

			expect(1, "start", 0, 0), //
			expect(1, "event 0", 0, 0), //
			expect(1, "event 14", 0.5f, 0.5f), //

			note("First 2 setAnimation calls are done."),

			expect(1, "interrupt", 0.8f, 0.8f), //

			expect(0, "start", 0, 0.8f), //
			expect(0, "interrupt", 0, 0.8f), //
			expect(0, "end", 0, 0.8f), //
			expect(0, "dispose", 0, 0.8f), //

			expect(2, "start", 0, 0.8f), //
			expect(2, "event 0", 0.1f, 0.9f), //

			expect(1, "end", 0.9f, 1), //
			expect(1, "dispose", 0.9f, 1), //

			expect(2, "event 14", 0.5f, 1.3f), //
			expect(2, "event 30", 1, 1.8f), //
			expect(2, "complete", 1, 1.8f), //
			expect(2, "end", 1, 1.9f), //
			expect(2, "dispose", 1, 1.9f) //
		);
		state.setAnimation(0, "events0", false); // First should be ignored.
		state.setAnimation(0, "events1", false);
		run(0.1f, 1000, new TestListener() {
			public void frame (float time) {
				if (MathUtils.isEqual(time, 0.8f)) {
					state.setAnimation(0, "events0", false); // First should be ignored.
					state.setAnimation(0, "events2", false).setTrackEnd(1);
				}
			}
		});

		setup("setAnimation twice with multiple mixing", // 22
			expect(0, "start", 0, 0), //
			expect(0, "interrupt", 0, 0), //
			expect(0, "end", 0, 0), //
			expect(0, "dispose", 0, 0), //

			expect(1, "start", 0, 0), //
			expect(1, "event 0", 0, 0), //

			note("First 2 setAnimation calls are done."),

			expect(1, "interrupt", 0.2f, 0.2f), //

			expect(0, "start", 0, 0.2f), //
			expect(0, "interrupt", 0, 0.2f), //
			expect(0, "end", 0, 0.2f), //
			expect(0, "dispose", 0, 0.2f), //

			expect(2, "start", 0, 0.2f), //
			expect(2, "event 0", 0.1f, 0.3f), //

			note("Second 2 setAnimation calls are done."),

			expect(2, "interrupt", 0.2f, 0.4f), //

			expect(1, "start", 0, 0.4f), //
			expect(1, "interrupt", 0, 0.4f), //
			expect(1, "end", 0, 0.4f), //
			expect(1, "dispose", 0, 0.4f), //

			expect(0, "start", 0, 0.4f), //
			expect(0, "event 0", 0.1f, 0.5f), //
			expect(0, "event 14", 0.5f, 0.9f), //

			expect(2, "end", 0.8f, 1.1f), //
			expect(2, "dispose", 0.8f, 1.1f), //

			expect(1, "end", 0.8f, 1.1f), //
			expect(1, "dispose", 0.8f, 1.1f), //

			expect(0, "event 30", 1, 1.4f), //
			expect(0, "complete", 1, 1.4f), //
			expect(0, "end", 1, 1.5f), //
			expect(0, "dispose", 1, 1.5f) //
		);
		stateData.setDefaultMix(0.6f);
		state.setAnimation(0, "events0", false); // First should be ignored.
		state.setAnimation(0, "events1", false);
		run(0.1f, 1000, new TestListener() {
			public void frame (float time) {
				if (MathUtils.isEqual(time, 0.2f)) {
					state.setAnimation(0, "events0", false); // First should be ignored.
					state.setAnimation(0, "events2", false);
				}
				if (MathUtils.isEqual(time, 0.4f)) {
					state.setAnimation(0, "events1", false); // First should be ignored.
					state.setAnimation(0, "events0", false).setTrackEnd(1);
				}
			}
		});

		setup("addAnimation with delay on empty track", // 23
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 5), //
			expect(0, "event 14", 0.5f, 5.5f), //
			expect(0, "event 30", 1, 6), //
			expect(0, "complete", 1, 6), //
			expect(0, "end", 1, 6.1f), //
			expect(0, "dispose", 1, 6.1f) //
		);
		state.addAnimation(0, "events0", false, 5).setTrackEnd(1);
		run(0.1f, 10, null);

		setup("setAnimation during AnimationStateListener"); // 24
		state.addListener(new AnimationStateListener() {
			public void start (TrackEntry entry) {
				if (entry.getAnimation().getName().equals("events0")) state.setAnimation(1, "events1", false);
			}

			public void interrupt (TrackEntry entry) {
				state.addAnimation(3, "events1", false, 0);
			}

			public void end (TrackEntry entry) {
				if (entry.getAnimation().getName().equals("events0")) state.setAnimation(0, "events1", false);
			}

			public void dispose (TrackEntry entry) {
				if (entry.getAnimation().getName().equals("events0")) state.setAnimation(1, "events1", false);
			}

			public void complete (TrackEntry entry) {
				if (entry.getAnimation().getName().equals("events0")) state.setAnimation(1, "events1", false);
			}

			public void event (TrackEntry entry, Event event) {
				if (entry.getTrackIndex() != 2) state.setAnimation(2, "events1", false);
			}
		});
		state.addAnimation(0, "events0", false, 0);
		state.addAnimation(0, "events1", false, 0);
		state.setAnimation(1, "events1", false).setTrackEnd(1);
		run(0.1f, 10, null);

		setup("clearTrack", // 25
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "end", 0.7f, 0.7f), //
			expect(0, "dispose", 0.7f, 0.7f) //
		);
		state.addAnimation(0, "events0", false, 0).setTrackEnd(1);
		run(0.1f, 10, new TestListener() {
			public void frame (float time) {
				if (MathUtils.isEqual(time, 0.7f)) state.clearTrack(0);
			}
		});

		setup("setEmptyAnimation", // 26
			expect(0, "start", 0, 0), //
			expect(0, "event 0", 0, 0), //
			expect(0, "event 14", 0.5f, 0.5f), //
			expect(0, "interrupt", 0.7f, 0.7f), //

			expect(-1, "start", 0, 0.7f), //
			expect(-1, "complete", 0.1f, 0.8f), //

			expect(0, "end", 0.8f, 0.9f), //
			expect(0, "dispose", 0.8f, 0.9f), //

			expect(-1, "end", 0.2f, 1), //
			expect(-1, "dispose", 0.2f, 1) //
		);
		state.addAnimation(0, "events0", false, 0).setTrackEnd(1);
		run(0.1f, 10, new TestListener() {
			public void frame (float time) {
				if (MathUtils.isEqual(time, 0.7f)) state.setEmptyAnimation(0, 0);
			}
		});

		setup("TrackEntry listener"); // 27
		final AtomicInteger counter = new AtomicInteger();
		state.addAnimation(0, "events0", false, 0).setListener(new AnimationStateListener() {
			public void start (TrackEntry entry) {
				counter.addAndGet(1 << 1);
			}

			public void interrupt (TrackEntry entry) {
				counter.addAndGet(1 << 5);
			}

			public void end (TrackEntry entry) {
				counter.addAndGet(1 << 9);
			}

			public void dispose (TrackEntry entry) {
				counter.addAndGet(1 << 13);
			}

			public void complete (TrackEntry entry) {
				counter.addAndGet(1 << 17);
			}

			public void event (TrackEntry entry, Event event) {
				counter.addAndGet(1 << 21);
			}
		});
		state.addAnimation(0, "events0", false, 0);
		state.addAnimation(0, "events1", false, 0);
		state.setAnimation(1, "events1", false).setTrackEnd(1);
		run(0.1f, 10, null);
		if (counter.get() != 15082016) {
			log("TEST 28 FAILED! " + counter);
			System.exit(0);
		}

		System.out.println("AnimationState tests passed.");
	}

	void setup (String description, Result... expectedArray) {
		test++;
		expected.addAll(expectedArray);
		stateData = new AnimationStateData(skeletonData);
		state = new AnimationState(stateData);
		state.trackEntryPool = new Pool<TrackEntry>() {
			public TrackEntry obtain () {
				TrackEntry entry = super.obtain();
				entryCount++;
				// System.out.println("+1: " + entryCount + " " + entry.hashCode());
				return entry;
			}

			protected TrackEntry newObject () {
				return new TrackEntry();
			}

			public void free (TrackEntry entry) {
				entryCount--;
				// System.out.println("-1: " + entryCount + " " + entry.hashCode());
				super.free(entry);
			}
		};
		time = 0;
		fail = false;
		log(test + ": " + description);
		if (expectedArray.length > 0) {
			state.addListener(stateListener);
			log(String.format("%-3s%-12s%-7s%-7s%-7s", "#", "EVENT", "TRACK", "TOTAL", "RESULT"));
		}
	}

	void run (float incr, float endTime, TestListener listener) {
		Skeleton skeleton = new Skeleton(skeletonData);
		state.apply(skeleton);
		while (time < endTime) {
			time += incr;
			skeleton.update(incr);
			state.update(incr);

			// Reduce float discrepancies for tests.
			for (TrackEntry entry : state.getTracks()) {
				if (entry == null) continue;
				entry.trackTime = round(entry.trackTime, 6);
				entry.delay = round(entry.delay, 3);
				if (entry.mixingFrom != null) entry.mixingFrom.trackTime = round(entry.mixingFrom.trackTime, 6);
			}

			state.apply(skeleton);

			// Apply multiple times to ensure no side effects.
			if (expected.size > 0) state.removeListener(stateListener);
			state.apply(skeleton);
			state.apply(skeleton);
			if (expected.size > 0) state.addListener(stateListener);

			if (listener != null) listener.frame(time);
		}
		state.clearTracks();

		// Expecting more than actual is a failure.
		for (int i = actual.size, n = expected.size; i < n; i++) {
			log(String.format("%-29s", "<none>") + "FAIL: " + expected.get(i));
			fail = true;
		}

		// Check all allocated entries were freed.
		if (!fail) {
			if (entryCount != 0) {
				log("FAIL: Pool balance: " + entryCount);
				fail = true;
			}
		}

		actual.clear();
		expected.clear();
		log("");
		if (fail) {
			log("TEST " + test + " FAILED!");
			System.exit(0);
		}
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
		result.trackTime = Math.round(entry.trackTime * 1000) / 1000f;
		result.totalTime = Math.round(time * 1000) / 1000f;
		return result;
	}

	Result note (String message) {
		Result result = new Result();
		result.name = message;
		result.note = true;
		return result;
	}

	void log (String message) {
		System.out.println(message);
	}

	class Result {
		String name;
		int animationIndex;
		float trackTime, totalTime;
		boolean note;

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
			if (!MathUtils.isEqual(totalTime, other.totalTime)) return false;
			if (!MathUtils.isEqual(trackTime, other.trackTime)) return false;
			return true;
		}

		public String toString () {
			return String.format("%-3s%-12s%-7s%-7s", "" + animationIndex, name, roundTime(trackTime), roundTime(totalTime));
		}
	}

	static float round (float value, int decimals) {
		float shift = (float)Math.pow(10, decimals);
		return Math.round(value * shift) / shift;
	}

	static String roundTime (float value) {
		String text = Float.toString(round(value, 3));
		return text.endsWith(".0") ? text.substring(0, text.length() - 2) : text;
	}

	static interface TestListener {
		void frame (float time);
	}

	static public void main (String[] args) throws Exception {
		new AnimationStateTests();
	}
}
