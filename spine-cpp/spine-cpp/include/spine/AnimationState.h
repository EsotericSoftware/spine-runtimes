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

#ifndef Spine_AnimationState_h
#define Spine_AnimationState_h

#include <spine/Vector.h>

namespace Spine
{
    class Animation;
    
    /// State for the playback of an animation
    class TrackEntry
    {
    public:
        TrackEntry();
        
        /// The index of the track where this entry is either current or queued.
        int getTrackIndex();
        
        /// The animation to apply for this track entry.
        Animation* getAnimation();
        
        ///
        /// If true, the animation will repeat. If false, it will not, instead its last frame is applied if played beyond its duration.
        bool getLoop();
        void setLoop(bool inValue);
        
        ///
        /// Seconds to postpone playing the animation. When a track entry is the current track entry, delay postpones incrementing
        /// the track time. When a track entry is queued, delay is the time from the start of the previous animation to when the
        /// track entry will become the current track entry.
        float getDelay();
        void setDelay(float inValue);
        
        ///
        /// Current time in seconds this track entry has been the current track entry. The track time determines
        /// TrackEntry.AnimationTime. The track time can be set to start the animation at a time other than 0, without affecting looping.
        float getTrackTime();
        void setTrackTime(float inValue);
        
        ///
        /// The track time in seconds when this animation will be removed from the track. Defaults to the animation duration for
        /// non-looping animations and to int.MaxValue for looping animations. If the track end time is reached and no
        /// other animations are queued for playback, and mixing from any previous animations is complete, properties keyed by the animation,
        /// are set to the setup pose and the track is cleared.
        ///
        /// It may be desired to use AnimationState.addEmptyAnimation(int, float, float) to mix the properties back to the
        /// setup pose over time, rather than have it happen instantly.
        ///
        float getTrackEnd();
        void setTrackEnd(float inValue);
        
        ///
        /// Seconds when this animation starts, both initially and after looping. Defaults to 0.
        ///
        /// When changing the animation start time, it often makes sense to set TrackEntry.AnimationLast to the same value to
        /// prevent timeline keys before the start time from triggering.
        ///
        float getAnimationStart();
        void setAnimationStart(float inValue);
        
        ///
        /// Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
        /// loop back to TrackEntry.AnimationStart at this time. Defaults to the animation duration.
        float getAnimationEnd();
        void setAnimationEnd(float inValue);
        
        ///
        /// The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
        /// animation is applied, event timelines will fire all events between the animation last time (exclusive) and animation time
        /// (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation is applied.
        float getAnimationLast();
        void setAnimationLast(float inValue);
        
        ///
        /// Uses TrackEntry.TrackTime to compute the animation time between TrackEntry.AnimationStart. and
        /// TrackEntry.AnimationEnd. When the track time is 0, the animation time is equal to the animation start time.
        ///
        float getAnimationTime();
        
        ///
        /// Multiplier for the delta time when the animation state is updated, causing time for this animation to play slower or
        /// faster. Defaults to 1.
        ///
        float getTimeScale();
        void setTimeScale(float inValue);
        
        ///
        /// Values less than 1 mix this animation with the last skeleton pose. Defaults to 1, which overwrites the last skeleton pose with
        /// this animation.
        ///
        /// Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. It doesn't make sense
        /// to use alpha on track 0 if the skeleton pose is from the last frame render.
        ///
        float getAlpha();
        void setAlpha(float inValue);
        
        ///
        /// When the mix percentage (mix time / mix duration) is less than the event threshold, event timelines for the animation
        /// being mixed out will be applied. Defaults to 0, so event timelines are not applied for an animation being mixed out.
        float getEventThreshold();
        void setEventThreshold(float inValue);
        
        ///
        /// When the mix percentage (mix time / mix duration) is less than the attachment threshold, attachment timelines for the
        /// animation being mixed out will be applied. Defaults to 0, so attachment timelines are not applied for an animation being
        /// mixed out.
        float getAttachmentThreshold();
        void setAttachmentThreshold(float inValue);
        
        ///
        /// When the mix percentage (mix time / mix duration) is less than the draw order threshold, draw order timelines for the
        /// animation being mixed out will be applied. Defaults to 0, so draw order timelines are not applied for an animation being
        /// mixed out.
        ///
        float getDrawOrderThreshold();
        void setDrawOrderThreshold(float inValue);
        
        ///
        /// The animation queued to start after this animation, or null.
        TrackEntry* getNext();
        
        ///
        /// Returns true if at least one loop has been completed.
        bool isComplete();
        
        ///
        /// Seconds from 0 to the mix duration when mixing from the previous animation to this animation. May be slightly more than
        /// TrackEntry.MixDuration when the mix is complete.
        float getMixTime();
        void setMixTime(float inValue);
        
        ///
        /// Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
        /// AnimationStateData based on the animation before this animation (if any).
        ///
        /// The mix duration can be set manually rather than use the value from AnimationStateData.GetMix.
        /// In that case, the mixDuration must be set before AnimationState.update(float) is next called.
        ///
        /// When using AnimationState::addAnimation(int, Animation, bool, float) with a delay
        /// less than or equal to 0, note the Delay is set using the mix duration from the AnimationStateData
        ///
        ///
        ///
        float getMixDuration();
        void setMixDuration(float inValue);
        
        ///
        /// The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
        /// mixing is currently occuring. When mixing from multiple animations, MixingFrom makes up a linked list.
        TrackEntry* getMixingFrom();
        
        ///
        /// Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
        /// long way around when using alpha and starting animations on other tracks.
        ///
        /// Mixing involves finding a rotation between two others, which has two possible solutions: the short way or the long way around.
        /// The two rotations likely change over time, so which direction is the short or long way also changes.
        /// If the short way was always chosen, bones would flip to the other side when that direction became the long way.
        /// TrackEntry chooses the short way the first time it is applied and remembers that direction.
        void resetRotationDirections();
        
    private:
        Animation* _animation;
        
        TrackEntry* _next;
        TrackEntry* _mixingFrom;
        int _trackIndex;
//
        bool _loop;
        float _eventThreshold, _attachmentThreshold, _drawOrderThreshold;
        float _animationStart, _animationEnd, _animationLast, _nextAnimationLast;
        float _delay, _trackTime, _trackLast, _nextTrackLast, _trackEnd, _timeScale = 1.0f;
        float _alpha, _mixTime, _mixDuration, _interruptAlpha, _totalAlpha;
        Vector<int> _timelineData;
        Vector<TrackEntry> _timelineDipMix;
        Vector<float> _timelinesRotation;
//        event AnimationState.TrackEntryDelegate Start, Interrupt, End, Dispose, Complete;
//        event AnimationState.TrackEntryEventDelegate Event;
        
        /// Sets the timeline data.
        /// @param to May be null.
//        TrackEntry setTimelineData(TrackEntry* to, Vector<TrackEntry> mixingToArray, HashSet<int> propertyIDs)
//        {
//            if (to != null) mixingToArray.Add(to);
//            var lastEntry = mixingFrom != null ? mixingFrom.setTimelineData(this, mixingToArray, propertyIDs) : this;
//            if (to != null) mixingToArray.Pop();
//
//            var mixingTo = mixingToArray.Items;
//            int mixingToLast = mixingToArray.Count - 1;
//            var timelines = animation.timelines.Items;
//            int timelinesCount = animation.timelines.Count;
//            var timelineDataItems = timelineData.Resize(timelinesCount).Items; // timelineData.setSize(timelinesCount);
//            timelineDipMix.clear();
//            var timelineDipMixItems = timelineDipMix.Resize(timelinesCount).Items; //timelineDipMix.setSize(timelinesCount);
//
//            // outer:
//            for (int i = 0; i < timelinesCount; i++) {
//                int id = timelines[i].PropertyId;
//                if (!propertyIDs.Add(id)) {
//                    timelineDataItems[i] = AnimationState.Subsequent;
//                } else if (to == null || !to.hasTimeline(id)) {
//                    timelineDataItems[i] = AnimationState.First;
//                } else {
//                    for (int ii = mixingToLast; ii >= 0; ii--) {
//                        var entry = mixingTo[ii];
//                        if (!entry.hasTimeline(id)) {
//                            if (entry.mixDuration > 0) {
//                                timelineDataItems[i] = AnimationState.DipMix;
//                                timelineDipMixItems[i] = entry;
//                                goto continue_outer; // continue outer;
//                            }
//                            break;
//                        }
//                    }
//                    timelineDataItems[i] = AnimationState.Dip;
//                }
//            continue_outer: {}
//            }
//            return lastEntry;
//        }
        
        bool hasTimeline(int inId)
        {
//            var timelines = animation.timelines.Items;
//            for (int i = 0, n = animation.timelines.Count; i < n; ++i)
//            {
//                if (timelines[i].PropertyId == inId)
//                {
//                    return true;
//                }
//            }
            return false;
        }
        
//        void onStart() { if (Start != null) Start(this); }
//        void onInterrupt() { if (Interrupt != null) Interrupt(this); }
//        void onEnd() { if (End != null) End(this); }
//        void onDispose() { if (Dispose != null) Dispose(this); }
//        void onComplete() { if (Complete != null) Complete(this); }
//        void onEvent(Event& e) { if (Event != null) Event(this, e); }
//
//        void reset()
//        {
//            next = null;
//            mixingFrom = null;
//            animation = null;
//            timelineData.clear();
//            timelineDipMix.clear();
//            timelinesRotation.clear();
//
//            Start = null;
//            Interrupt = null;
//            End = null;
//            Dispose = null;
//            Complete = null;
//            Event = null;
//        }
    };
    
    class EventQueue
    {
//        private readonly List<EventQueueEntry> eventQueueEntries = new List<EventQueueEntry>();
//        internal bool drainDisabled;
//
//        private readonly AnimationState state;
//        private readonly Pool<TrackEntry> trackEntryPool;
//        internal event Action animationsChanged;
//
//        internal EventQueue(AnimationState state, Action HandleanimationsChanged, Pool<TrackEntry> trackEntryPool) {
//            this.state = state;
//            this.animationsChanged += HandleanimationsChanged;
//            this.trackEntryPool = trackEntryPool;
//        }
//
//        struct EventQueueEntry {
//            public EventType type;
//            public TrackEntry entry;
//            public Event e;
//
//            public EventQueueEntry(EventType eventType, TrackEntry trackEntry, Event e = null) {
//                this.type = eventType;
//                this.entry = trackEntry;
//                this.e = e;
//            }
//        }
//
//        enum EventType {
//            Start, Interrupt, End, Dispose, Complete, Event
//        }
//
//        internal void Start(TrackEntry entry) {
//            eventQueueEntries.Add(new EventQueueEntry(EventType.Start, entry));
//            if (animationsChanged != null) animationsChanged();
//        }
//
//        internal void Interrupt(TrackEntry entry) {
//            eventQueueEntries.Add(new EventQueueEntry(EventType.Interrupt, entry));
//        }
//
//        internal void End(TrackEntry entry) {
//            eventQueueEntries.Add(new EventQueueEntry(EventType.End, entry));
//            if (animationsChanged != null) animationsChanged();
//        }
//
//        internal void Dispose(TrackEntry entry) {
//            eventQueueEntries.Add(new EventQueueEntry(EventType.Dispose, entry));
//        }
//
//        internal void Complete(TrackEntry entry) {
//            eventQueueEntries.Add(new EventQueueEntry(EventType.Complete, entry));
//        }
//
//        internal void Event(TrackEntry entry, Event e) {
//            eventQueueEntries.Add(new EventQueueEntry(EventType.Event, entry, e));
//        }
//
//        /// Raises all events in the queue and drains the queue.
//        internal void drain() {
//            if (drainDisabled) return;
//            drainDisabled = true;
//
//            var entries = this.eventQueueEntries;
//            AnimationState state = this.state;
//
//            // Don't cache entries.Count so callbacks can queue their own events (eg, call setAnimation in AnimationState_Complete).
//            for (int i = 0; i < entries.Count; i++) {
//                var queueEntry = entries[i];
//                TrackEntry trackEntry = queueEntry.entry;
//
//                switch (queueEntry.type) {
//                    case EventType.Start:
//                        trackEntry.onStart();
//                        state.onStart(trackEntry);
//                        break;
//                    case EventType.Interrupt:
//                        trackEntry.onInterrupt();
//                        state.onInterrupt(trackEntry);
//                        break;
//                    case EventType.End:
//                        trackEntry.onEnd();
//                        state.onEnd(trackEntry);
//                    case EventType.Dispose:
//                        trackEntry.onDispose();
//                        state.onDispose(trackEntry);
//                        trackEntryPool.Free(trackEntry); // Pooling
//                        break;
//                    case EventType.Complete:
//                        trackEntry.onComplete();
//                        state.onComplete(trackEntry);
//                        break;
//                    case EventType.Event:
//                        trackEntry.onEvent(queueEntry.e);
//                        state.onEvent(trackEntry, queueEntry.e);
//                        break;
//                }
//            }
//            eventQueueEntries.clear();
//
//            drainDisabled = false;
//        }
//
//        internal void clear() {
//            eventQueueEntries.clear();
//        }
    };
    
    class AnimationState
    {
//        static readonly Animation EmptyAnimation = new Animation("<empty>", new Vector<Timeline>(), 0);
//        internal const int Subsequent = 0, First = 1, Dip = 2, DipMix = 3;
//
//        private AnimationStateData data;
//
//        Pool<TrackEntry> trackEntryPool = new Pool<TrackEntry>();
//        private readonly Vector<TrackEntry> tracks = new Vector<TrackEntry>();
//        private readonly Vector<Event> events = new Vector<Event>();
//        private readonly EventQueue queue; // Initialized by constructor.
//
//        private readonly HashSet<int> propertyIDs = new HashSet<int>();
//        private readonly Vector<TrackEntry> mixingTo = new Vector<TrackEntry>();
//        private bool animationsChanged;
//
//        private float timeScale = 1;
//
//        public AnimationStateData Data { get { return data; } }
//        /// A list of tracks that have animations, which may contain nulls.
//        public Vector<TrackEntry> Tracks { get { return tracks; } }
//        public float TimeScale { get { return timeScale; } set { timeScale = value; } }
//
//        public delegate void TrackEntryDelegate(TrackEntry trackEntry);
//        public event TrackEntryDelegate Start, Interrupt, End, Dispose, Complete;
//
//        public delegate void TrackEntryEventDelegate(TrackEntry trackEntry, Event e);
//        public event TrackEntryEventDelegate Event;
//
//        public AnimationState(AnimationStateData data) {
//            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
//            this.data = data;
//            this.queue = new EventQueue(
//                                        this,
//                                        delegate { this.animationsChanged = true; },
//                                        trackEntryPool
//                                        );
//        }
//
//        ///
//        /// Increments the track entry times, setting queued animations as current if needed
//        /// @param delta delta time
//        public void update(float delta) {
//            delta *= timeScale;
//            var tracksItems = tracks.Items;
//            for (int i = 0, n = tracks.Count; i < n; i++) {
//                TrackEntry current = tracksItems[i];
//                if (current == null) continue;
//
//                current.animationLast = current.nextAnimationLast;
//                current.trackLast = current.nextTrackLast;
//
//                float currentDelta = delta * current.timeScale;
//
//                if (current.delay > 0) {
//                    current.delay -= currentDelta;
//                    if (current.delay > 0) continue;
//                    currentDelta = -current.delay;
//                    current.delay = 0;
//                }
//
//                TrackEntry next = current.next;
//                if (next != null) {
//                    // When the next entry's delay is passed, change to the next entry, preserving leftover time.
//                    float nextTime = current.trackLast - next.delay;
//                    if (nextTime >= 0) {
//                        next.delay = 0;
//                        next.trackTime = nextTime + (delta * next.timeScale);
//                        current.trackTime += currentDelta;
//                        setCurrent(i, next, true);
//                        while (next.mixingFrom != null) {
//                            next.mixTime += currentDelta;
//                            next = next.mixingFrom;
//                        }
//                        continue;
//                    }
//                } else if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
//                    // clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
//                    tracksItems[i] = null;
//
//                    queue.End(current);
//                    disposeNext(current);
//                    continue;
//                }
//                if (current.mixingFrom != null && updateMixingFrom(current, delta)) {
//                    // End mixing from entries once all have completed.
//                    var from = current.mixingFrom;
//                    current.mixingFrom = null;
//                    while (from != null) {
//                        queue.End(from);
//                        from = from.mixingFrom;
//                    }
//                }
//
//                current.trackTime += currentDelta;
//            }
//
//            queue.drain();
//        }
//
//        /// Returns true when all mixing from entries are complete.
//        private bool updateMixingFrom(TrackEntry to, float delta) {
//            TrackEntry from = to.mixingFrom;
//            if (from == null) return true;
//
//            bool finished = updateMixingFrom(from, delta);
//
//            // Require mixTime > 0 to ensure the mixing from entry was applied at least once.
//            if (to.mixTime > 0 && (to.mixTime >= to.mixDuration || to.timeScale == 0)) {
//                // Require totalAlpha == 0 to ensure mixing is complete, unless mixDuration == 0 (the transition is a single frame).
//                if (from.totalAlpha == 0 || to.mixDuration == 0) {
//                    to.mixingFrom = from.mixingFrom;
//                    to.interruptAlpha = from.interruptAlpha;
//                    queue.End(from);
//                }
//                return finished;
//            }
//
//            from.animationLast = from.nextAnimationLast;
//            from.trackLast = from.nextTrackLast;
//            from.trackTime += delta * from.timeScale;
//            to.mixTime += delta * to.timeScale;
//            return false;
//        }
//
//        ///
//        /// Poses the skeleton using the track entry animations. There are no side effects other than invoking listeners, so the
//        /// animation state can be applied to multiple skeletons to pose them identically.
//        public bool apply(Skeleton skeleton) {
//            if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
//            if (animationsChanged) animationsChanged();
//
//            var events = this.events;
//
//            bool applied = false;
//            var tracksItems = tracks.Items;
//            for (int i = 0, m = tracks.Count; i < m; i++) {
//                TrackEntry current = tracksItems[i];
//                if (current == null || current.delay > 0) continue;
//                applied = true;
//                MixPose currentPose = i == 0 ? MixPose.Current : MixPose.CurrentLayered;
//
//                // apply mixing from entries first.
//                float mix = current.alpha;
//                if (current.mixingFrom != null)
//                    mix *= applyMixingFrom(current, skeleton, currentPose);
//                else if (current.trackTime >= current.trackEnd && current.next == null) //
//                    mix = 0; // Set to setup pose the last time the entry will be applied.
//
//                // apply current entry.
//                float animationLast = current.animationLast, animationTime = current.AnimationTime;
//                int timelineCount = current.animation.timelines.Count;
//                var timelines = current.animation.timelines;
//                var timelinesItems = timelines.Items;
//                if (mix == 1) {
//                    for (int ii = 0; ii < timelineCount; ii++)
//                        timelinesItems[ii].apply(skeleton, animationLast, animationTime, events, 1, MixPose.Setup, MixDirection.In);
//                } else {
//                    var timelineData = current.timelineData.Items;
//
//                    bool firstFrame = current.timelinesRotation.Count == 0;
//                    if (firstFrame) current.timelinesRotation.EnsureCapacity(timelines.Count << 1);
//                    var timelinesRotation = current.timelinesRotation.Items;
//
//                    for (int ii = 0; ii < timelineCount; ii++) {
//                        Timeline timeline = timelinesItems[ii];
//                        MixPose pose = timelineData[ii] >= AnimationState.First ? MixPose.Setup : currentPose;
//                        var rotateTimeline = timeline as RotateTimeline;
//                        if (rotateTimeline != null)
//                            applyRotateTimeline(rotateTimeline, skeleton, animationTime, mix, pose, timelinesRotation, ii << 1, firstFrame);
//                        else
//                            timeline.apply(skeleton, animationLast, animationTime, events, mix, pose, MixDirection.In);
//                    }
//                }
//                queueEvents(current, animationTime);
//                events.clear(false);
//                current.nextAnimationLast = animationTime;
//                current.nextTrackLast = current.trackTime;
//            }
//
//            queue.drain();
//            return applied;
//        }
//
//        private float applyMixingFrom(TrackEntry to, Skeleton skeleton, MixPose currentPose) {
//            TrackEntry from = to.mixingFrom;
//            if (from.mixingFrom != null) applyMixingFrom(from, skeleton, currentPose);
//
//            float mix;
//            if (to.mixDuration == 0) { // Single frame mix to undo mixingFrom changes.
//                mix = 1;
//                currentPose = MixPose.Setup;
//            } else {
//                mix = to.mixTime / to.mixDuration;
//                if (mix > 1) mix = 1;
//            }
//
//            var eventBuffer = mix < from.eventThreshold ? this.events : null;
//            bool attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
//            float animationLast = from.animationLast, animationTime = from.AnimationTime;
//            var timelines = from.animation.timelines;
//            int timelineCount = timelines.Count;
//            var timelinesItems = timelines.Items;
//            var timelineData = from.timelineData.Items;
//            var timelineDipMix = from.timelineDipMix.Items;
//
//            bool firstFrame = from.timelinesRotation.Count == 0;
//            if (firstFrame) from.timelinesRotation.Resize(timelines.Count << 1); // from.timelinesRotation.setSize
//            var timelinesRotation = from.timelinesRotation.Items;
//
//            MixPose pose;
//            float alphaDip = from.alpha * to.interruptAlpha, alphaMix = alphaDip * (1 - mix), alpha;
//            from.totalAlpha = 0;
//            for (int i = 0; i < timelineCount; i++) {
//                Timeline timeline = timelinesItems[i];
//                switch (timelineData[i]) {
//                    case Subsequent:
//                        if (!attachments && timeline is AttachmentTimeline) continue;
//                        if (!drawOrder && timeline is DrawOrderTimeline) continue;
//                        pose = currentPose;
//                        alpha = alphaMix;
//                        break;
//                    case First:
//                        pose = MixPose.Setup;
//                        alpha = alphaMix;
//                        break;
//                    case Dip:
//                        pose = MixPose.Setup;
//                        alpha = alphaDip;
//                        break;
//                    default:
//                        pose = MixPose.Setup;
//                        TrackEntry dipMix = timelineDipMix[i];
//                        alpha = alphaDip * Math.Max(0, 1 - dipMix.mixTime / dipMix.mixDuration);
//                        break;
//                }
//                from.totalAlpha += alpha;
//                var rotateTimeline = timeline as RotateTimeline;
//                if (rotateTimeline != null) {
//                    applyRotateTimeline(rotateTimeline, skeleton, animationTime, alpha, pose, timelinesRotation, i << 1, firstFrame);
//                } else {
//                    timeline.apply(skeleton, animationLast, animationTime, eventBuffer, alpha, pose, MixDirection.Out);
//                }
//            }
//
//            if (to.mixDuration > 0) queueEvents(from, animationTime);
//            this.events.clear(false);
//            from.nextAnimationLast = animationTime;
//            from.nextTrackLast = from.trackTime;
//
//            return mix;
//        }
//
//        static private void applyRotateTimeline(RotateTimeline rotateTimeline, Skeleton skeleton, float time, float alpha, MixPose pose,
//                                                 float[] timelinesRotation, int i, bool firstFrame) {
//
//            if (firstFrame) timelinesRotation[i] = 0;
//
//            if (alpha == 1) {
//                rotateTimeline.apply(skeleton, 0, time, null, 1, pose, MixDirection.In);
//                return;
//            }
//
//            Bone bone = skeleton.bones.Items[rotateTimeline.boneIndex];
//            float[] frames = rotateTimeline.frames;
//            if (time < frames[0]) {
//                if (pose == MixPose.Setup) bone.rotation = bone.data.rotation;
//                return;
//            }
//
//            float r2;
//            if (time >= frames[frames.Length - RotateTimeline.ENTRIES]) // Time is after last frame.
//                r2 = bone.data.rotation + frames[frames.Length + RotateTimeline.PREV_ROTATION];
//            else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation.BinarySearch(frames, time, RotateTimeline.ENTRIES);
//                float prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
//                float frameTime = frames[frame];
//                float percent = rotateTimeline.GetCurvePercent((frame >> 1) - 1,
//                                                               1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));
//
//                r2 = frames[frame + RotateTimeline.ROTATION] - prevRotation;
//                r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
//                r2 = prevRotation + r2 * percent + bone.data.rotation;
//                r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
//            }
//
//            // Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
//            float r1 = pose == MixPose.Setup ? bone.data.rotation : bone.rotation;
//            float total, diff = r2 - r1;
//            if (diff == 0) {
//                total = timelinesRotation[i];
//            } else {
//                diff -= (16384 - (int)(16384.499999999996 - diff / 360)) * 360;
//                float lastTotal, lastDiff;
//                if (firstFrame) {
//                    lastTotal = 0;
//                    lastDiff = diff;
//                } else {
//                    lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
//                    lastDiff = timelinesRotation[i + 1]; // Difference between bones.
//                }
//                bool current = diff > 0, dir = lastTotal >= 0;
//                // Detect cross at 0 (not 180).
//                if (Math.Sign(lastDiff) != Math.Sign(diff) && Math.Abs(lastDiff) <= 90) {
//                    // A cross after a 360 rotation is a loop.
//                    if (Math.Abs(lastTotal) > 180) lastTotal += 360 * Math.Sign(lastTotal);
//                    dir = current;
//                }
//                total = diff + lastTotal - lastTotal % 360; // Store loops as part of lastTotal.
//                if (dir != current) total += 360 * Math.Sign(lastTotal);
//                timelinesRotation[i] = total;
//            }
//            timelinesRotation[i + 1] = diff;
//            r1 += total * alpha;
//            bone.rotation = r1 - (16384 - (int)(16384.499999999996 - r1 / 360)) * 360;
//        }
//
//        private void queueEvents(TrackEntry entry, float animationTime) {
//            float animationStart = entry.animationStart, animationEnd = entry.animationEnd;
//            float duration = animationEnd - animationStart;
//            float trackLastWrapped = entry.trackLast % duration;
//
//            // Queue events before complete.
//            var events = this.events;
//            var eventsItems = events.Items;
//            int i = 0, n = events.Count;
//            for (; i < n; i++) {
//                var e = eventsItems[i];
//                if (e.time < trackLastWrapped) break;
//                if (e.time > animationEnd) continue; // Discard events outside animation start/end.
//                queue.Event(entry, e);
//            }
//
//            // Queue complete if completed a loop iteration or the animation.
//            if (entry.loop ? (trackLastWrapped > entry.trackTime % duration)
//                : (animationTime >= animationEnd && entry.animationLast < animationEnd)) {
//                queue.Complete(entry);
//            }
//
//            // Queue events after complete.
//            for (; i < n; i++) {
//                Event e = eventsItems[i];
//                if (e.time < animationStart) continue; // Discard events outside animation start/end.
//                queue.Event(entry, eventsItems[i]);
//            }
//        }
//
//        ///
//        /// Removes all animations from all tracks, leaving skeletons in their previous pose.
//        /// It may be desired to use AnimationState.setEmptyAnimations(float) to mix the skeletons back to the setup pose,
//        /// rather than leaving them in their previous pose.
//        public void clearTracks() {
//            bool olddrainDisabled = queue.drainDisabled;
//            queue.drainDisabled = true;
//            for (int i = 0, n = tracks.Count; i < n; i++) {
//                clearTrack(i);
//            }
//            tracks.clear();
//            queue.drainDisabled = olddrainDisabled;
//            queue.drain();
//        }
//
//        ///
//        /// Removes all animations from the tracks, leaving skeletons in their previous pose.
//        /// It may be desired to use AnimationState.setEmptyAnimations(float) to mix the skeletons back to the setup pose,
//        /// rather than leaving them in their previous pose.
//        public void clearTrack(int trackIndex) {
//            if (trackIndex >= tracks.Count) return;
//            TrackEntry current = tracks.Items[trackIndex];
//            if (current == null) return;
//
//            queue.End(current);
//
//            disposeNext(current);
//
//            TrackEntry entry = current;
//            while (true) {
//                TrackEntry from = entry.mixingFrom;
//                if (from == null) break;
//                queue.End(from);
//                entry.mixingFrom = null;
//                entry = from;
//            }
//
//            tracks.Items[current.trackIndex] = null;
//
//            queue.drain();
//        }
//
//        /// Sets the active TrackEntry for a given track number.
//        private void setCurrent(int index, TrackEntry current, bool interrupt) {
//            TrackEntry from = expandToIndex(index);
//            tracks.Items[index] = current;
//
//            if (from != null) {
//                if (interrupt) queue.Interrupt(from);
//                current.mixingFrom = from;
//                current.mixTime = 0;
//
//                // Store interrupted mix percentage.
//                if (from.mixingFrom != null && from.mixDuration > 0)
//                    current.interruptAlpha *= Math.Min(1, from.mixTime / from.mixDuration);
//
//                from.timelinesRotation.clear(); // Reset rotation for mixing out, in case entry was mixed in.
//            }
//
//            queue.Start(current); // triggers animationsChanged
//        }
//
//
//        /// Sets an animation by name. setAnimation(int, Animation, bool)
//        public TrackEntry setAnimation(int trackIndex, string animationName, bool loop) {
//            Animation animation = data.skeletonData.FindAnimation(animationName);
//            if (animation == null) throw new ArgumentException("Animation not found: " + animationName, "animationName");
//            return setAnimation(trackIndex, animation, loop);
//        }
//
//        /// Sets the current animation for a track, discarding any queued animations.
//        /// @param loop If true, the animation will repeat.
//        /// If false, it will not, instead its last frame is applied if played beyond its duration.
//        /// In either case TrackEntry.TrackEnd determines when the track is cleared.
//        /// @return
//        /// A track entry to allow further customization of animation playback. References to the track entry must not be kept
//        /// after AnimationState.Dispose.
//        public TrackEntry setAnimation(int trackIndex, Animation animation, bool loop) {
//            if (animation == null) throw new ArgumentNullException("animation", "animation cannot be null.");
//            bool interrupt = true;
//            TrackEntry current = expandToIndex(trackIndex);
//            if (current != null) {
//                if (current.nextTrackLast == -1) {
//                    // Don't mix from an entry that was never applied.
//                    tracks.Items[trackIndex] = current.mixingFrom;
//                    queue.Interrupt(current);
//                    queue.End(current);
//                    disposeNext(current);
//                    current = current.mixingFrom;
//                    interrupt = false;
//                } else {
//                    disposeNext(current);
//                }
//            }
//            TrackEntry entry = newTrackEntry(trackIndex, animation, loop, current);
//            setCurrent(trackIndex, entry, interrupt);
//            queue.drain();
//            return entry;
//        }
//
//        /// Queues an animation by name.
//        /// addAnimation(int, Animation, bool, float)
//        public TrackEntry addAnimation(int trackIndex, string animationName, bool loop, float delay) {
//            Animation animation = data.skeletonData.FindAnimation(animationName);
//            if (animation == null) throw new ArgumentException("Animation not found: " + animationName, "animationName");
//            return addAnimation(trackIndex, animation, loop, delay);
//        }
//
//        /// Adds an animation to be played delay seconds after the current or last queued animation
//        /// for a track. If the track is empty, it is equivalent to calling setAnimation.
//        /// @param delay
//        /// Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
//        /// duration of the previous track minus any mix duration plus the negative delay.
//        ///
//        /// @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
//        /// after AnimationState.Dispose
//        public TrackEntry addAnimation(int trackIndex, Animation animation, bool loop, float delay) {
//            if (animation == null) throw new ArgumentNullException("animation", "animation cannot be null.");
//
//            TrackEntry last = expandToIndex(trackIndex);
//            if (last != null) {
//                while (last.next != null)
//                    last = last.next;
//            }
//
//            TrackEntry entry = newTrackEntry(trackIndex, animation, loop, last);
//
//            if (last == null) {
//                setCurrent(trackIndex, entry, true);
//                queue.drain();
//            } else {
//                last.next = entry;
//                if (delay <= 0) {
//                    float duration = last.animationEnd - last.animationStart;
//                    if (duration != 0)
//                        delay += duration * (1 + (int)(last.trackTime / duration)) - data.GetMix(last.animation, animation);
//                    else
//                        delay = 0;
//                }
//            }
//
//            entry.delay = delay;
//            return entry;
//        }
//
//        ///
//        /// Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration.
//        public TrackEntry setEmptyAnimation(int trackIndex, float mixDuration) {
//            TrackEntry entry = setAnimation(trackIndex, AnimationState.EmptyAnimation, false);
//            entry.mixDuration = mixDuration;
//            entry.trackEnd = mixDuration;
//            return entry;
//        }
//
//        ///
//        /// Adds an empty animation to be played after the current or last queued animation for a track, and mixes to it over the
//        /// specified mix duration.
//        /// @return
//        /// A track entry to allow further customization of animation playback. References to the track entry must not be kept after AnimationState.Dispose.
//        ///
//        /// @param trackIndex Track number.
//        /// @param mixDuration Mix duration.
//        /// @param delay Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
//        /// duration of the previous track minus any mix duration plus the negative delay.
//        public TrackEntry addEmptyAnimation(int trackIndex, float mixDuration, float delay) {
//            if (delay <= 0) delay -= mixDuration;
//            TrackEntry entry = addAnimation(trackIndex, AnimationState.EmptyAnimation, false, delay);
//            entry.mixDuration = mixDuration;
//            entry.trackEnd = mixDuration;
//            return entry;
//        }
//
//        ///
//        /// Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.
//        public void setEmptyAnimations(float mixDuration) {
//            bool olddrainDisabled = queue.drainDisabled;
//            queue.drainDisabled = true;
//            for (int i = 0, n = tracks.Count; i < n; i++) {
//                TrackEntry current = tracks.Items[i];
//                if (current != null) setEmptyAnimation(i, mixDuration);
//            }
//            queue.drainDisabled = olddrainDisabled;
//            queue.drain();
//        }
//
//        private TrackEntry expandToIndex(int index) {
//            if (index < tracks.Count) return tracks.Items[index];
//            while (index >= tracks.Count)
//                tracks.Add(null);
//            return null;
//        }
//
//        /// Object-pooling version of new TrackEntry. Obtain an unused TrackEntry from the pool and clear/initialize its values.
//        /// @param last May be null.
//        private TrackEntry newTrackEntry(int trackIndex, Animation animation, bool loop, TrackEntry last) {
//            TrackEntry entry = trackEntryPool.Obtain(); // Pooling
//            entry.trackIndex = trackIndex;
//            entry.animation = animation;
//            entry.loop = loop;
//
//            entry.eventThreshold = 0;
//            entry.attachmentThreshold = 0;
//            entry.drawOrderThreshold = 0;
//
//            entry.animationStart = 0;
//            entry.animationEnd = animation.Duration;
//            entry.animationLast = -1;
//            entry.nextAnimationLast = -1;
//
//            entry.delay = 0;
//            entry.trackTime = 0;
//            entry.trackLast = -1;
//            entry.nextTrackLast = -1; // nextTrackLast == -1 signifies a TrackEntry that wasn't applied yet.
//            entry.trackEnd = float.MaxValue; // loop ? float.MaxValue : animation.Duration;
//            entry.timeScale = 1;
//
//            entry.alpha = 1;
//            entry.interruptAlpha = 1;
//            entry.mixTime = 0;
//            entry.mixDuration = (last == null) ? 0 : data.GetMix(last.animation, animation);
//            return entry;
//        }
//
//        /// Dispose all track entries queued after the given TrackEntry.
//        private void disposeNext(TrackEntry entry) {
//            TrackEntry next = entry.next;
//            while (next != null) {
//                queue.Dispose(next);
//                next = next.next;
//            }
//            entry.next = null;
//        }
//
//        private void animationsChanged() {
//            animationsChanged = false;
//
//            var propertyIDs = this.propertyIDs;
//            propertyIDs.clear();
//            var mixingTo = this.mixingTo;
//
//            var tracksItems = tracks.Items;
//            for (int i = 0, n = tracks.Count; i < n; i++) {
//                var entry = tracksItems[i];
//                if (entry != null) entry.setTimelineData(null, mixingTo, propertyIDs);
//            }
//        }
//
//        /// @return The track entry for the animation currently playing on the track, or null if no animation is currently playing.
//        public TrackEntry getCurrent(int trackIndex) {
//            return (trackIndex >= tracks.Count) ? null : tracks.Items[trackIndex];
//        }
//
//        internal void onStart(TrackEntry entry) { if (Start != null) Start(entry); }
//        internal void onInterrupt(TrackEntry entry) { if (Interrupt != null) Interrupt(entry); }
//        internal void onEnd(TrackEntry entry) { if (End != null) End(entry); }
//        internal void onDispose(TrackEntry entry) { if (Dispose != null) Dispose(entry); }
//        internal void onComplete(TrackEntry entry) { if (Complete != null) Complete(entry); }
//        internal void onEvent(TrackEntry entry, Event e) { if (Event != null) Event(entry, e); }
    };
}

#endif /* Spine_AnimationState_h */
