/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

//#define RUN_ADDITIONAL_FORUM_RELATED_TEST

using Spine;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Spine {

	public class AnimationStateTests {

		static readonly float FLOAT_ROUNDING_ERROR = 0.000001f; // 32 bits
		static bool IsEqual (float a, float b) {
			return Math.Abs(a - b) <= FLOAT_ROUNDING_ERROR;
		}

		class NullAttachmentLoader : AttachmentLoader {
			public RegionAttachment NewRegionAttachment (Skin skin, string name, string path, Sequence sequence) {
				return null;
			}

			public MeshAttachment NewMeshAttachment (Skin skin, string name, string path, Sequence sequence) {
				return null;
			}

			public BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, string name) {
				return null;
			}

			public ClippingAttachment NewClippingAttachment (Skin skin, string name) {
				return null;
			}

			public PathAttachment NewPathAttachment (Skin skin, string name) {
				return null;
			}

			public PointAttachment NewPointAttachment (Skin skin, string name) {
				return null;
			}
		}

		class LoggingAnimationStateListener {

			AnimationStateTests tests;

			public LoggingAnimationStateListener (AnimationStateTests tests) {
				this.tests = tests;
			}

			public void RegisterAtAnimationState (AnimationState state) {
				state.Start += Start;
				state.Interrupt += Interrupt;
				state.End += End;
				state.Dispose += Dispose;
				state.Complete += Complete;
				state.Event += Event;
			}

			public void UnregisterFromAnimationState (AnimationState state) {
				state.Start -= Start;
				state.Interrupt -= Interrupt;
				state.End -= End;
				state.Dispose -= Dispose;
				state.Complete -= Complete;
				state.Event -= Event;
			}

			public void Start (TrackEntry entry) {
				Add(tests.Actual("start", entry));
			}

			public void Interrupt (TrackEntry entry) {
				Add(tests.Actual("interrupt", entry));
			}

			public void End (TrackEntry entry) {
				Add(tests.Actual("end", entry));
			}

			public void Dispose (TrackEntry entry) {
				Add(tests.Actual("dispose", entry));
			}

			public void Complete (TrackEntry entry) {
				Add(tests.Actual("complete", entry));
			}

			public void Event (TrackEntry entry, Event ev) {
				Add(tests.Actual("event " + ev.String, entry));
			}

			private void Add (Result result) {
				while (tests.expected.Count > tests.actual.Count) {
					Result note = tests.expected[tests.actual.Count];
					if (!note.note) break;
					tests.actual.Add(note);
					Log(note.name);
				}

				string message = result.ToString();
				if (tests.actual.Count >= tests.expected.Count) {
					message += "FAIL: <none>";
					tests.fail = true;
				} else if (!tests.expected[tests.actual.Count].Equals(result)) {
					message += "FAIL: " + tests.expected[tests.actual.Count];
					tests.fail = true;
				} else
					message += "PASS";
				Log(message);
				tests.actual.Add(result);
			}
		};

		readonly SkeletonJson json = new SkeletonJson();

		LoggingAnimationStateListener stateListener = null;

		readonly SkeletonData skeletonData;
		readonly List<Result> actual = new List<Result>();
		readonly List<Result> expected = new List<Result>();

		AnimationStateData stateData;
		AnimationState state;
		float time = 0;
		bool fail;
		int test;

		AnimationStateTests (string testJsonFilePath) {
			skeletonData = json.ReadSkeletonData(testJsonFilePath);

			TrackEntry entry;

			Setup("0.1 time step", // 1
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 1, 1.1f), //
				Expect(0, "dispose", 1, 1.1f) //
			);
			state.SetAnimation(0, "events0", false).TrackEnd = 1;
			Run(0.1f, 1000, null);

			Setup("1/60 time step, dispose queued", // 2
				Expect(0, "start", 0, 0), //
				Expect(0, "interrupt", 0, 0), //
				Expect(0, "end", 0, 0), //
				Expect(0, "dispose", 0, 0), //
				Expect(1, "dispose", 0, 0), //
				Expect(0, "dispose", 0, 0), //
				Expect(1, "dispose", 0, 0), //

				Note("First 2 set/addAnimation calls are done."),

				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.483f, 0.483f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 1, 1.017f), //
				Expect(0, "dispose", 1, 1.017f) //
			);
			state.SetAnimation(0, "events0", false);
			state.AddAnimation(0, "events1", false, 0);
			state.AddAnimation(0, "events0", false, 0);
			state.AddAnimation(0, "events1", false, 0);
			state.SetAnimation(0, "events0", false).TrackEnd = 1;
			Run(1 / 60f, 1000, null);

			Setup("30 time step", // 3
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 30, 30), //
				Expect(0, "event 30", 30, 30), //
				Expect(0, "complete", 30, 30), //
				Expect(0, "end", 30, 60), //
				Expect(0, "dispose", 30, 60) //
			);
			state.SetAnimation(0, "events0", false).TrackEnd = 1;
			Run(30, 1000, null);

			Setup("1 time step", // 4
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 1, 1), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 1, 2), //
				Expect(0, "dispose", 1, 2) //
			);
			state.SetAnimation(0, "events0", false).TrackEnd = 1;
			Run(1, 1.01f, null);

			Setup("interrupt", // 5
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "interrupt", 1.1f, 1.1f), //

				Expect(1, "start", 0.1f, 1.1f), //
				Expect(1, "event 0", 0.1f, 1.1f), //

				Expect(0, "end", 1.1f, 1.2f), //
				Expect(0, "dispose", 1.1f, 1.2f), //

				Expect(1, "event 14", 0.5f, 1.5f), //
				Expect(1, "event 30", 1, 2), //
				Expect(1, "complete", 1, 2), //
				Expect(1, "interrupt", 1.1f, 2.1f), //

				Expect(0, "start", 0.1f, 2.1f), //
				Expect(0, "event 0", 0.1f, 2.1f), //

				Expect(1, "end", 1.1f, 2.2f), //
				Expect(1, "dispose", 1.1f, 2.2f), //

				Expect(0, "event 14", 0.5f, 2.5f), //
				Expect(0, "event 30", 1, 3), //
				Expect(0, "complete", 1, 3), //
				Expect(0, "end", 1, 3.1f), //
				Expect(0, "dispose", 1, 3.1f) //
			);
			state.SetAnimation(0, "events0", false);
			state.AddAnimation(0, "events1", false, 0);
			state.AddAnimation(0, "events0", false, 0).TrackEnd = 1;
			Run(0.1f, 4f, null);

			Setup("interrupt with delay", // 6
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "interrupt", 0.6f, 0.6f), //

				Expect(1, "start", 0.1f, 0.6f), //
				Expect(1, "event 0", 0.1f, 0.6f), //

				Expect(0, "end", 0.6f, 0.7f), //
				Expect(0, "dispose", 0.6f, 0.7f), //

				Expect(1, "event 14", 0.5f, 1.0f), //
				Expect(1, "event 30", 1, 1.5f), //
				Expect(1, "complete", 1, 1.5f), //
				Expect(1, "end", 1, 1.6f), //
				Expect(1, "dispose", 1, 1.6f) //
			);
			state.SetAnimation(0, "events0", false);
			state.AddAnimation(0, "events1", false, 0.5f).TrackEnd = 1;
			Run(0.1f, 1000, null);

			Setup("interrupt with delay and mix time", // 7
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "interrupt", 1, 1), //

				Expect(1, "start", 0.1f, 1), //

				Expect(0, "complete", 1, 1), //

				Expect(1, "event 0", 0.1f, 1), //
				Expect(1, "event 14", 0.5f, 1.4f), //

				Expect(0, "end", 1.6f, 1.7f), //
				Expect(0, "dispose", 1.6f, 1.7f), //

				Expect(1, "event 30", 1, 1.9f), //
				Expect(1, "complete", 1, 1.9f), //
				Expect(1, "end", 1, 2), //
				Expect(1, "dispose", 1, 2) //
			);
			stateData.SetMix("events0", "events1", 0.7f);
			state.SetAnimation(0, "events0", true);
			state.AddAnimation(0, "events1", false, 0.9f).TrackEnd = 1;
			Run(0.1f, 1000, null);

			Setup("animation 0 events do not fire during mix", // 8
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "interrupt", 0.5f, 0.5f), //

				Expect(1, "start", 0.1f, 0.5f), //
				Expect(1, "event 0", 0.1f, 0.5f), //
				Expect(1, "event 14", 0.5f, 0.9f), //

				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 1.1f, 1.2f), //
				Expect(0, "dispose", 1.1f, 1.2f), //

				Expect(1, "event 30", 1, 1.4f), //
				Expect(1, "complete", 1, 1.4f), //
				Expect(1, "end", 1, 1.5f), //
				Expect(1, "dispose", 1, 1.5f) //
			);
			stateData.DefaultMix = 0.7f;
			state.SetAnimation(0, "events0", false);
			state.AddAnimation(0, "events1", false, 0.4f).TrackEnd = 1;
			Run(0.1f, 1000, null);

			Setup("event threshold, some animation 0 events fire during mix", // 9
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "interrupt", 0.5f, 0.5f), //

				Expect(1, "start", 0.1f, 0.5f), //

				Expect(0, "event 14", 0.5f, 0.5f), //

				Expect(1, "event 0", 0.1f, 0.5f), //
				Expect(1, "event 14", 0.5f, 0.9f), //

				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 1.1f, 1.2f), //
				Expect(0, "dispose", 1.1f, 1.2f), //

				Expect(1, "event 30", 1, 1.4f), //
				Expect(1, "complete", 1, 1.4f), //
				Expect(1, "end", 1, 1.5f), //
				Expect(1, "dispose", 1, 1.5f) //
			);
			stateData.SetMix("events0", "events1", 0.7f);
			state.SetAnimation(0, "events0", false).EventThreshold = 0.5f;
			state.AddAnimation(0, "events1", false, 0.4f).TrackEnd = 1;
			Run(0.1f, 1000, null);

			Setup("event threshold, all animation 0 events fire during mix", // 10
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "interrupt", 0.9f, 0.9f), //

				Expect(1, "start", 0.1f, 0.9f), //
				Expect(1, "event 0", 0.1f, 0.9f), //

				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "event 0", 1, 1), //

				Expect(1, "event 14", 0.5f, 1.3f), //

				Expect(0, "end", 1.5f, 1.6f), //
				Expect(0, "dispose", 1.5f, 1.6f), //

				Expect(1, "event 30", 1, 1.8f), //
				Expect(1, "complete", 1, 1.8f), //
				Expect(1, "end", 1, 1.9f), //
				Expect(1, "dispose", 1, 1.9f) //
			);
			state.SetAnimation(0, "events0", true).EventThreshold = 1;
			entry = state.AddAnimation(0, "events1", false, 0.8f);
			entry.MixDuration = 0.7f;
			entry.TrackEnd = 1;
			Run(0.1f, 1000, null);

			Setup("looping", // 11
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "event 0", 1, 1), //
				Expect(0, "event 14", 1.5f, 1.5f), //
				Expect(0, "event 30", 2, 2), //
				Expect(0, "complete", 2, 2), //
				Expect(0, "event 0", 2, 2), //
				Expect(0, "event 14", 2.5f, 2.5f), //
				Expect(0, "event 30", 3, 3), //
				Expect(0, "complete", 3, 3), //
				Expect(0, "event 0", 3, 3), //
				Expect(0, "event 14", 3.5f, 3.5f), //
				Expect(0, "event 30", 4, 4), //
				Expect(0, "complete", 4, 4), //
				Expect(0, "event 0", 4, 4), //
				Expect(0, "end", 4.1f, 4.1f), //
				Expect(0, "dispose", 4.1f, 4.1f) //
			);
			state.SetAnimation(0, "events0", true);
			Run(0.1f, 4, null);

			Setup("not looping, track end past animation 0 duration", // 12
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "interrupt", 2.1f, 2.1f), //

				Expect(1, "start", 0.1f, 2.1f), //
				Expect(1, "event 0", 0.1f, 2.1f), //

				Expect(0, "end", 2.1f, 2.2f), //
				Expect(0, "dispose", 2.1f, 2.2f), //

				Expect(1, "event 14", 0.5f, 2.5f), //
				Expect(1, "event 30", 1, 3), //
				Expect(1, "complete", 1, 3), //
				Expect(1, "end", 1, 3.1f), //
				Expect(1, "dispose", 1, 3.1f) //
			);
			state.SetAnimation(0, "events0", false);
			state.AddAnimation(0, "events1", false, 2).TrackEnd = 1;
			Run(0.1f, 4f, null);


			Setup("interrupt animation after first loop complete", // 13
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "event 0", 1, 1), //
				Expect(0, "event 14", 1.5f, 1.5f), //
				Expect(0, "event 30", 2, 2), //
				Expect(0, "complete", 2, 2), //
				Expect(0, "event 0", 2, 2), //
				Expect(0, "interrupt", 2.1f, 2.1f), //

				Expect(1, "start", 0.1f, 2.1f), //
				Expect(1, "event 0", 0.1f, 2.1f), //

				Expect(0, "end", 2.1f, 2.2f), //
				Expect(0, "dispose", 2.1f, 2.2f), //

				Expect(1, "event 14", 0.5f, 2.5f), //
				Expect(1, "event 30", 1, 3), //
				Expect(1, "complete", 1, 3), //
				Expect(1, "end", 1, 3.1f), //
				Expect(1, "dispose", 1, 3.1f) //
			);
			state.SetAnimation(0, "events0", true);
			Run(0.1f, 6, new TestListener(
				(time) => {
						if (IsEqual(time, 1.4f)) state.AddAnimation(0, "events1", false, 0).TrackEnd = 1;
				}));

			Setup ("add animation on empty track", // 14
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 1, 1.1f), //
				Expect(0, "dispose", 1, 1.1f) //
			);
			state.AddAnimation(0, "events0", false, 0).TrackEnd = 1;
			Run(0.1f, 1.9f, null);

			Setup("end time beyond non-looping animation duration", // 15
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "end", 9f, 9.1f), //
				Expect(0, "dispose", 9f, 9.1f) //
			);
			state.SetAnimation(0, "events0", false).TrackEnd = 9;
			Run(0.1f, 10, null);

			Setup("looping with animation start", // 16
				Expect(0, "start", 0, 0), //
				Expect(0, "event 30", 0.4f, 0.4f), //
				Expect(0, "complete", 0.4f, 0.4f), //
				Expect(0, "event 30", 0.8f, 0.8f), //
				Expect(0, "complete", 0.8f, 0.8f), //
				Expect(0, "event 30", 1.2f, 1.2f), //
				Expect(0, "complete", 1.2f, 1.2f), //
				Expect(0, "end", 1.4f, 1.4f), //
				Expect(0, "dispose", 1.4f, 1.4f) //
			);
			entry = state.SetAnimation(0, "events0", true);
			entry.AnimationLast = 0.6f;
			entry.AnimationStart = 0.6f;
			Run(0.1f, 1.4f, null);

			Setup("looping with animation start and end", // 17
				Expect(0, "start", 0, 0), //
				Expect(0, "event 14", 0.3f, 0.3f), //
				Expect(0, "complete", 0.6f, 0.6f), //
				Expect(0, "event 14", 0.9f, 0.9f), //
				Expect(0, "complete", 1.2f, 1.2f), //
				Expect(0, "event 14", 1.5f, 1.5f), //
				Expect(0, "end", 1.8f, 1.8f), //
				Expect(0, "dispose", 1.8f, 1.8f) //
			);
			entry = state.SetAnimation(0, "events0", true);
			entry.AnimationStart = 0.2f;
			entry.AnimationLast = 0.2f;
			entry.AnimationEnd = 0.8f;
			Run(0.1f, 1.8f, null);

			Setup("non-looping with animation start and end", // 18
				Expect(0, "start", 0, 0), //
				Expect(0, "event 14", 0.3f, 0.3f), //
				Expect(0, "complete", 0.6f, 0.6f), //
				Expect(0, "end", 1, 1.1f), //
				Expect(0, "dispose", 1, 1.1f) //
			);
			entry = state.SetAnimation(0, "events0", false);
			entry.AnimationStart = 0.2f;
			entry.AnimationLast = 0.2f;
			entry.AnimationEnd = 0.8f;
			entry.TrackEnd = 1;
			Run(0.1f, 1.8f, null);

			Setup("mix out looping with animation start and end", // 19
				Expect(0, "start", 0, 0), //
				Expect(0, "event 14", 0.3f, 0.3f), //
				Expect(0, "complete", 0.6f, 0.6f), //
				Expect(0, "interrupt", 0.8f, 0.8f), //

				Expect(1, "start", 0.1f, 0.8f), //
				Expect(1, "event 0", 0.1f, 0.8f), //

				Expect(0, "event 14", 0.9f, 0.9f), //
				Expect(0, "complete", 1.2f, 1.2f), //

				Expect(1, "event 14", 0.5f, 1.2f), //

				Expect(0, "end", 1.4f, 1.5f), //
				Expect(0, "dispose", 1.4f, 1.5f), //

				Expect(1, "event 30", 1, 1.7f), //
				Expect(1, "complete", 1, 1.7f), //
				Expect(1, "end", 1, 1.8f), //
				Expect(1, "dispose", 1, 1.8f) //
			);
			entry = state.SetAnimation(0, "events0", true);
			entry.AnimationStart = (0.2f);
			entry.AnimationLast = (0.2f);
			entry.AnimationEnd = (0.8f);
			entry.EventThreshold = 1;
			entry = state.AddAnimation(0, "events1", false, 0.7f);
			entry.MixDuration = (0.7f);
			entry.TrackEnd = 1;
			Run(0.1f, 20, null);

			Setup("setAnimation with track entry mix", // 20
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "event 0", 1, 1), //
				Expect(0, "interrupt", 1, 1), //

				Expect(1, "start", 0, 1), //

				Expect(1, "event 0", 0.1f, 1.1f), //
				Expect(1, "event 14", 0.5f, 1.5f), //

				Expect(0, "end", 1.7f, 1.8f), //
				Expect(0, "dispose", 1.7f, 1.8f), //

				Expect(1, "event 30", 1, 2), //
				Expect(1, "complete", 1, 2), //
				Expect(1, "end", 1, 2.1f), //
				Expect(1, "dispose", 1, 2.1f) //
			);
			state.SetAnimation(0, "events0", true);
			Run(0.1f, 1000, new TestListener(
				(time) => {
					if (IsEqual(time, 1f)) {
						TrackEntry ent = state.SetAnimation(0, "events1", false);
						ent.MixDuration = (0.7f);
						ent.TrackEnd = 1;
					}
				}));

			Setup("setAnimation twice", // 21
				Expect(0, "start", 0, 0), //
				Expect(0, "interrupt", 0, 0), //
				Expect(0, "end", 0, 0), //
				Expect(0, "dispose", 0, 0), //

				Expect(1, "start", 0, 0), //
				Expect(1, "event 0", 0, 0), //
				Expect(1, "event 14", 0.5f, 0.5f), //

				Note("First 2 setAnimation calls are done."),

				Expect(1, "interrupt", 0.8f, 0.8f), //

				Expect(0, "start", 0, 0.8f), //
				Expect(0, "interrupt", 0, 0.8f), //
				Expect(0, "end", 0, 0.8f), //
				Expect(0, "dispose", 0, 0.8f), //

				Expect(2, "start", 0, 0.8f), //
				Expect(2, "event 0", 0.1f, 0.9f), //

				Expect(1, "end", 0.9f, 1), //
				Expect(1, "dispose", 0.9f, 1), //

				Expect(2, "event 14", 0.5f, 1.3f), //
				Expect(2, "event 30", 1, 1.8f), //
				Expect(2, "complete", 1, 1.8f), //
				Expect(2, "end", 1, 1.9f), //
				Expect(2, "dispose", 1, 1.9f) //
			);
			state.SetAnimation(0, "events0", false); // First should be ignored.
			state.SetAnimation(0, "events1", false);
			Run(0.1f, 1000, new TestListener(
				(time) => {
					if (IsEqual(time, 0.8f)) {
						state.SetAnimation(0, "events0", false); // First should be ignored.
						state.SetAnimation(0, "events2", false).TrackEnd = 1;
					}
				}));

			Setup("setAnimation twice with multiple mixing", // 22
				Expect(0, "start", 0, 0), //
				Expect(0, "interrupt", 0, 0), //
				Expect(0, "end", 0, 0), //
				Expect(0, "dispose", 0, 0), //

				Expect(1, "start", 0, 0), //
				Expect(1, "event 0", 0, 0), //

				Note("First 2 setAnimation calls are done."),

				Expect(1, "interrupt", 0.2f, 0.2f), //

				Expect(0, "start", 0, 0.2f), //
				Expect(0, "interrupt", 0, 0.2f), //
				Expect(0, "end", 0, 0.2f), //
				Expect(0, "dispose", 0, 0.2f), //

				Expect(2, "start", 0, 0.2f), //
				Expect(2, "event 0", 0.1f, 0.3f), //

				Note("Second 2 setAnimation calls are done."),

				Expect(2, "interrupt", 0.2f, 0.4f), //

				Expect(1, "start", 0, 0.4f), //
				Expect(1, "interrupt", 0, 0.4f), //
				Expect(1, "end", 0, 0.4f), //
				Expect(1, "dispose", 0, 0.4f), //

				Expect(0, "start", 0, 0.4f), //
				Expect(0, "event 0", 0.1f, 0.5f), //

				Expect(1, "end", 0.8f, 0.9f), //
				Expect(1, "dispose", 0.8f, 0.9f), //

				Expect(0, "event 14", 0.5f, 0.9f), //

				Expect(2, "end", 0.8f, 1.1f), //
				Expect(2, "dispose", 0.8f, 1.1f), //

				Expect(0, "event 30", 1, 1.4f), //
				Expect(0, "complete", 1, 1.4f), //
				Expect(0, "end", 1, 1.5f), //
				Expect(0, "dispose", 1, 1.5f) //
			);
			stateData.DefaultMix = 0.6f;
			state.SetAnimation(0, "events0", false); // First should be ignored.
			state.SetAnimation(0, "events1", false);
			Run(0.1f, 1000, new TestListener(
				(time) => {
					if (IsEqual(time, 0.2f)) {
						state.SetAnimation(0, "events0", false); // First should be ignored.
						state.SetAnimation(0, "events2", false);
					}
					if (IsEqual(time, 0.4f)) {
						state.SetAnimation(0, "events1", false); // First should be ignored.
						state.SetAnimation(0, "events0", false).TrackEnd = 1;
					}
				}));

			Setup("addAnimation with delay on empty track", // 23
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 5), //
				Expect(0, "event 14", 0.5f, 5.5f), //
				Expect(0, "event 30", 1, 6), //
				Expect(0, "complete", 1, 6), //
				Expect(0, "end", 1, 6.1f), //
				Expect(0, "dispose", 1, 6.1f) //
			);
			state.AddAnimation(0, "events0", false, 5).TrackEnd = 1;
			Run(0.1f, 10, null);

			Setup("setAnimation during AnimationStateListener"); // 24
			state.Start += (trackEntry) => {
					if (trackEntry.Animation.Name.Equals("events0")) state.SetAnimation(1, "events1", false);
				};
			state.Interrupt += (trackEntry) => {
					state.AddAnimation(3, "events1", false, 0);
				};
			state.End += (trackEntry) => {
					if (trackEntry.Animation.Name.Equals("events0")) state.SetAnimation(0, "events1", false);
				};
			state.Dispose += (trackEntry) => {
					if (trackEntry.Animation.Name.Equals("events0")) state.SetAnimation(1, "events1", false);
				};
			state.Complete += (trackEntry) => {
					if (trackEntry.Animation.Name.Equals("events0")) state.SetAnimation(1, "events1", false);
				};
			state.Event += (trackEntry, ev) => {
					if (trackEntry.TrackIndex != 2) state.SetAnimation(2, "events1", false);
				};
			state.AddAnimation(0, "events0", false, 0);
			state.AddAnimation(0, "events1", false, 0);
			state.SetAnimation(1, "events1", false).TrackEnd = 1;
			Run(0.1f, 10, null);

			Setup("clearTrack", // 25
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "end", 0.7f, 0.7f), //
				Expect(0, "dispose", 0.7f, 0.7f) //
			);
			state.AddAnimation(0, "events0", false, 0).TrackEnd = 1;
			Run(0.1f, 10, new TestListener(
				(time) => {
					if (IsEqual(time, 0.7f)) state.ClearTrack(0);
				}));

			Setup("setEmptyAnimation", // 26
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "interrupt", 0.7f, 0.7f), //

				Expect(-1, "start", 0, 0.7f), //
				Expect(-1, "complete", 0.1f, 0.8f), //

				Expect(0, "end", 0.8f, 0.9f), //
				Expect(0, "dispose", 0.8f, 0.9f), //

				Expect(-1, "end", 0.2f, 1), //
				Expect(-1, "dispose", 0.2f, 1) //
			);
			state.AddAnimation(0, "events0", false, 0).TrackEnd = 1;
			Run(0.1f, 10, new TestListener(
				(time) => {
					if (IsEqual(time, 0.7f)) state.SetEmptyAnimation(0, 0);
				}));

			Setup("TrackEntry listener"); // 27
			int counter = 0;
			entry = state.AddAnimation(0, "events0", false, 0);
			entry.Start += (trackEntry) => {
					Interlocked.Add(ref counter, 1 << 1);
				};
			entry.Interrupt += (trackEntry) => {
					Interlocked.Add(ref counter, 1 << 5);
				};
			entry.End += (trackEntry) => {
					Interlocked.Add(ref counter, 1 << 9);
				};
			entry.Dispose += (trackEntry) => {
					Interlocked.Add(ref counter, 1 << 13);
				};
			entry.Complete += (trackEntry) => {
					Interlocked.Add(ref counter, 1 << 17);
				};
			entry.Event += (trackEntry, ev) => {
					Interlocked.Add(ref counter, 1 << 21);
				};
			state.AddAnimation(0, "events0", false, 0);
			state.AddAnimation(0, "events1", false, 0);
			state.SetAnimation(1, "events1", false).TrackEnd = 1;
			Run(0.1f, 10, null);
			if (counter != 15082016) {
				string message = "TEST 27 FAILED! " + counter;
				Log(message);
				FailTestRun(message);
			}

#if RUN_ADDITIONAL_FORUM_RELATED_TEST
			Setup("0.1 time step, start and add", // 2
				Expect(0, "start", 0, 0), //
				Expect(0, "event 0", 0, 0), //
				Expect(0, "event 14", 0.5f, 0.5f), //
				Expect(0, "event 30", 1, 1), //
				Expect(0, "complete", 1, 1), //
				Expect(0, "interrupt", 1.1f, 1.1f), //
				Expect(1, "start", 0.1f, 1.1f), //
				Expect(1, "event 0", 0.1f, 1.1f), //
				Expect(0, "end", 1.3f, 1.4f), //
				Expect(0, "dispose", 1.3f, 1.4f), //
				Expect(1, "event 14", 0.5f, 1.5f), //
				Expect(1, "event 30", 1, 2), //
				Expect(1, "complete", 1, 2), //
				Expect(1, "end", 1, 2.1f), //
				Expect(1, "dispose", 1, 2.1f) //
			);
			state.SetAnimation(0, "events0", false);
			var entry1 = state.AddAnimation(0, "events1", false, 0);
			entry1.MixDuration = 0.25f;
			entry1.TrackEnd = 1.0f;
			Run(0.1f, 1000, null);
#endif // RUN_ADDITIONAL_FORUM_RELATED_TEST

			Log("AnimationState tests passed.");
		}

		void Setup (string description, params Result[] expectedArray) {
			test++;
			expected.AddRange(expectedArray);
			stateData = new AnimationStateData(skeletonData);
			state = new AnimationState(stateData);

			stateListener = new LoggingAnimationStateListener(this);
			time = 0;
			fail = false;
			Log(test + ": " + description);
			if (expectedArray.Length > 0) {
				stateListener.RegisterAtAnimationState(state);
				Log(string.Format("{0,-3}{1,-12}{2,-7}{3,-7}{4,-7}", "#", "EVENT", "TRACK", "TOTAL", "RESULT"));
			}
		}

		void Run (float incr, float endTime, TestListener listener) {
			Skeleton skeleton = new Skeleton(skeletonData);
			state.Apply(skeleton);
			while (time < endTime) {
				time += incr;
				state.Update(incr);

				// Reduce float discrepancies for tests.
				foreach (TrackEntry entry in state.Tracks) {
					if (entry == null) continue;
					entry.TrackTime = Round(entry.TrackTime, 6);
					entry.Delay = Round(entry.Delay, 3);
					if (entry.MixingFrom != null) entry.MixingFrom.TrackTime = Round(entry.MixingFrom.TrackTime, 6);
				}

				state.Apply(skeleton);

				// Apply multiple times to ensure no side effects.
				if (expected.Count > 0) stateListener.UnregisterFromAnimationState(state);
				state.Apply(skeleton);
				state.Apply(skeleton);
				if (expected.Count > 0) stateListener.RegisterAtAnimationState(state);

				if (listener != null) listener.Frame(time);
			}
			state.ClearTracks();

			// Expecting more than actual is a failure.
			for (int i = actual.Count, n = expected.Count; i < n; i++) {
				Log(string.Format("{0,-29}", "<none>") + "FAIL: " + expected[i]);
				fail = true;
			}

			actual.Clear();
			expected.Clear();
			Log("");
			if (fail) {
				string message = "TEST " + test + " FAILED!";
				Log(message);
				FailTestRun(message);
			}
		}

		Result Expect (int animationIndex, string name, float trackTime, float totalTime) {
			Result result = new Result();
			result.name = name;
			result.animationIndex = animationIndex;
			result.trackTime = trackTime;
			result.totalTime = totalTime;
			return result;
		}

		Result Actual (string name, TrackEntry entry) {
			Result result = new Result();
			result.name = name;
			result.animationIndex = skeletonData.Animations.IndexOf(entry.Animation);
			result.trackTime = (float)Math.Round(entry.TrackTime * 1000) / 1000f;
			result.totalTime = (float)Math.Round(time * 1000) / 1000f;
			return result;
		}

		Result Note (string message) {
			Result result = new Result();
			result.name = message;
			result.note = true;
			return result;
		}

		static void Log (string message) {
			if (logImplementation != null) {
				logImplementation(message);
				return;
			}
			Console.WriteLine(message);
		}

		static void FailTestRun (string message) {
			failImplementation(message);
		}

		class Result {
			public string name;
			public int animationIndex;
			public float trackTime, totalTime;
			public bool note;

			public override int GetHashCode () {
				int result = 31 + animationIndex;
				result = 31 * result + name.GetHashCode();
				result = 31 * result + totalTime.GetHashCode();
				result = 31 * result + trackTime.GetHashCode();
				return result;
			}

			public override bool Equals (object obj) {
				Result other = (Result)obj;
				if (animationIndex != other.animationIndex) return false;
				if (!name.Equals(other.name)) return false;
				if (!IsEqual(totalTime, other.totalTime)) return false;
				if (!IsEqual(trackTime, other.trackTime)) return false;
				return true;
			}

			public override string ToString () {
				return string.Format("{0,-3}{1,-12}{2,-7}{3,-7}", "" + animationIndex, name, RoundTime(trackTime), RoundTime(totalTime));
			}
		}

		static float Round (float value, int decimals) {
			float shift = (float)Math.Pow(10, decimals);
			return (float)Math.Round((double)value * shift) / shift;
		}

		static string RoundTime (float value) {
			float roundedValue = Round(value, 3);
			string text = roundedValue.ToString();
			return text.EndsWith(".0") ? text.Substring(0, text.Length - 2) : text;
		}

		class TestListener {

			public TestListener(FrameDelegate frame) {
				this.frame = frame;
			}

			public delegate void FrameDelegate (float time);
			public event FrameDelegate frame;

			public void Frame(float time) {
				frame(time);
			}
		}

		#region Test API
		static public void Main (string testJsonFilePath) {
			new AnimationStateTests(testJsonFilePath);
		}

		public delegate void LogDelegate (string message);
		public static event LogDelegate logImplementation;

		public delegate void FailDelegate (string message);
		public static event FailDelegate failImplementation;
		#endregion
	}
}
