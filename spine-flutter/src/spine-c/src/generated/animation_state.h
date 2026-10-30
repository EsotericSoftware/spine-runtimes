#ifndef SPINE_SPINE_ANIMATION_STATE_H
#define SPINE_SPINE_ANIMATION_STATE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_animation_state spine_animation_state_create(spine_animation_state_data data);

SPINE_C_API void spine_animation_state_dispose(spine_animation_state self);

/**
 * Increments each track entry's track time, setting queued animations as
 * current if needed.
 */
SPINE_C_API void spine_animation_state_update(spine_animation_state self, float delta);
/**
 * Poses the skeleton using the track entry animations. The animation state is
 * not changed, so can be applied to multiple skeletons to pose them
 * identically.
 *
 * @return True if any animations were applied.
 */
SPINE_C_API bool spine_animation_state_apply(spine_animation_state self, spine_skeleton skeleton);
/**
 * Removes all animations from all tracks, leaving skeletons in their current
 * pose.
 *
 * It may be desired to use AnimationState::setEmptyAnimations(float) to mix the
 * skeletons back to the setup pose, rather than leaving them in their current
 * pose.
 */
SPINE_C_API void spine_animation_state_clear_tracks(spine_animation_state self);
/**
 * Removes all animations from the track, leaving skeletons in their current
 * pose.
 *
 * It may be desired to use AnimationState::setEmptyAnimation(int, float) to mix
 * the skeletons back to the setup pose, rather than leaving them in their
 * current pose.
 */
SPINE_C_API void spine_animation_state_clear_track(spine_animation_state self, size_t trackIndex);
/**
 * Sets an animation by name.
 *
 * See setAnimation(int, Animation, bool).
 */
SPINE_C_API spine_track_entry spine_animation_state_set_animation_1(spine_animation_state self, size_t trackIndex, const char *animationName,
																	bool loop);
/**
 * Sets the current animation for a track, discarding any queued animations.
 *
 * If the formerly current track entry is for the same animation and was never
 * applied to a skeleton, it is replaced (not mixed from).
 *
 * @param loop If true, the animation will repeat. If false, it will not, instead its last frame is applied if played beyond its duration. In either case TrackEntry.TrackEnd determines when the track is cleared.
 *
 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept after AnimationState.Dispose.
 */
SPINE_C_API spine_track_entry spine_animation_state_set_animation_2(spine_animation_state self, size_t trackIndex, spine_animation animation,
																	bool loop);
/**
 * Queues an animation by name.
 *
 * See addAnimation(int, Animation, bool, float).
 */
SPINE_C_API spine_track_entry spine_animation_state_add_animation_1(spine_animation_state self, size_t trackIndex, const char *animationName,
																	bool loop, float delay);
/**
 * Adds an animation to be played delay seconds after the current or last queued
 * animation for a track. If the track has no entries, this is equivalent to
 * calling setAnimation.
 *
 * @param delay Seconds to begin this animation after the start of the previous animation. May be < = 0 to use the animation duration of the previous track minus any mix duration plus the negative delay.
 *
 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept after AnimationState.Dispose
 */
SPINE_C_API spine_track_entry spine_animation_state_add_animation_2(spine_animation_state self, size_t trackIndex, spine_animation animation,
																	bool loop, float delay);
/**
 * Sets an empty animation for a track, discarding any queued animations, and
 * sets the track entry's TrackEntry::getMixDuration(). An empty animation has
 * no timelines and serves as a placeholder for mixing in or out.
 *
 * Mixing out is done by setting an empty animation with a mix duration using
 * either setEmptyAnimation(int, float), setEmptyAnimations(float), or
 * addEmptyAnimation(int, float, float). Mixing to an empty animation causes the
 * previous animation to be applied less and less over the mix duration.
 * Properties keyed in the previous animation transition to the value from lower
 * tracks or to the setup pose value if no lower tracks key the property. A mix
 * duration of 0 still mixes out over one frame.
 *
 * Mixing in is done by first setting an empty animation, then adding an
 * animation using addAnimation(int, Animation, bool, float) with the desired
 * delay (an empty animation has a duration of 0) and on the returned track
 * entry set TrackEntry::setMixDuration(float). Mixing from an empty animation
 * causes the new animation to be applied more and more over the mix duration.
 * Properties keyed in the new animation transition from the value from lower
 * tracks or from the setup pose value if no lower tracks key the property to
 * the value keyed in the new animation.
 *
 * See Empty animations in the Spine Runtimes Guide.
 */
SPINE_C_API spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state self, size_t trackIndex, float mixDuration);
/**
 * Adds an empty animation to be played after the current or last queued
 * animation for a track, and sets the track entry's
 * TrackEntry::getMixDuration(). If the track has no entries, it is equivalent
 * to calling setEmptyAnimation(int, float).
 *
 * See setEmptyAnimation(int, float) and Empty animations in the Spine Runtimes
 * Guide.
 *
 * @param delay If > 0, sets TrackEntry::getDelay(). If < = 0, the delay set is the duration of the previous track entry minus any mix duration plus the specified delay (ie the mix ends at ( delay = 0) or before ( delay < 0) the previous track entry duration). If the previous entry is looping, its next loop completion is used instead of its duration.
 *
 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept after the AnimationStateListener::dispose(TrackEntry) event occurs.
 */
SPINE_C_API spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state self, size_t trackIndex, float mixDuration,
																		float delay);
/**
 * Sets an empty animation for every track, discarding any queued animations,
 * and mixes to it over the specified mix duration.
 *
 * See Empty animations in the Spine Runtimes Guide.
 */
SPINE_C_API void spine_animation_state_set_empty_animations(spine_animation_state self, float mixDuration);
/**
 *
 * @return The track entry for the animation currently playing on the track, or NULL if no animation is currently playing.
 */
SPINE_C_API /*@null*/ spine_track_entry spine_animation_state_get_track(spine_animation_state self, size_t trackIndex);
/**
 * The AnimationStateData to look up mix durations.
 */
SPINE_C_API spine_animation_state_data spine_animation_state_get_data(spine_animation_state self);
/**
 * The list of tracks that have had animations, which may contain null entries
 * for tracks that currently have no animation.
 */
SPINE_C_API spine_array_track_entry spine_animation_state_get_tracks(spine_animation_state self);
/**
 * Multiplier for the delta time when the animation state is updated, causing
 * time for all animations and mixes to play slower or faster. Defaults to 1.
 *
 * See TrackEntry::getTimeScale() for affecting a single animation.
 */
SPINE_C_API float spine_animation_state_get_time_scale(spine_animation_state self);
SPINE_C_API void spine_animation_state_set_time_scale(spine_animation_state self, float inValue);
SPINE_C_API void spine_animation_state_disable_queue(spine_animation_state self);
SPINE_C_API void spine_animation_state_enable_queue(spine_animation_state self);
SPINE_C_API void spine_animation_state_set_manual_track_entry_disposal(spine_animation_state self, bool inValue);
SPINE_C_API bool spine_animation_state_get_manual_track_entry_disposal(spine_animation_state self);
SPINE_C_API void spine_animation_state_dispose_track_entry(spine_animation_state self, /*@null*/ spine_track_entry entry);
SPINE_C_API /*@null*/ void *spine_animation_state_get_renderer_object(spine_animation_state self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ANIMATION_STATE_H */
