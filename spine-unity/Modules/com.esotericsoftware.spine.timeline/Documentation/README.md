The latest version of this documentation can also be found at the [spine-unity Runtime Documentation](http://esotericsoftware.com/spine-unity#Timeline-Extension-UPM-Package) webpage.

## Timeline Extension UPM Package

![](timeline.png)

Timeline support is provided as a separate UPM (Unity Package Manager) package. See section [Optional Extension UPM Packages](#Optional-Extension-UPM-Packages) on how to download and install this package and section [Updating an Extension UPM Package](#Updating-an-Extension-UPM-Package) on how to update it.

### Spine-Unity Timeline Playables

![](add-menu.png)

Spine Timeline currently provides three types of Timeline Playables:  
- `SkeletonAnimation Track` *(animates `SkeletonAnimation`)*,
- `SkeletonGraphic Track` *(animates `SkeletonGraphic`)*,
- `Skeleton Flip Track` *(flips `SkeletonAnimation` or `SkeletonGraphic`)*.

> Note: Timeline tracks have been renamed in recent versions to improve readability.  
`SkeletonAnimation Track` was called `Spine AnimationState Track`,  
`SkeletonGraphic Track` was called `Spine AnimationState Graphic Track`,  
`Skeleton Flip Track` was called `Spine Skeleton Flip Track`.

**Limitations:** currently only [SkeletonAnimation](#SkeletonAnimation-Component) and [SkeletonGraphic](#SkeletonGraphic-Component) are supported. There is currently no Timeline support for [SkeletonMecanim](#SkeletonMecanim-Component).

#### SkeletonAnimation Track and SkeletonGraphic Track

![](animationstate-track.png)  

These track types can be used to set animations at the AnimationState of the target `SkeletonAnimation` or `SkeletonGraphic`. Track type `SkeletonAnimation Track` is used for `SkeletonAnimation`, `SkeletonGraphic Track` for `SkeletonGraphic`.

![](animationstate-track-inspector.png)  

**Parameters**
- *Track Index.* The target AnimationState track index to set animations at. Do not forget to set this value accordingly when using multiple timeline tracks.
> **Important Note:** Currently it is required to order the timeline tracks with base track at the top and overlay tracks below, otherwise the Editor Preview will display incorrect results.

- *Unscaled Time.* Whenever starting a new animation clip of this track, `SkeletonAnimation.UnscaledTime` (or `SkeletonGraphic.UnscaledTime` respectively) will be set to this value. This allows you to play back Timeline clips either in normal game time or unscaled game time. Note that `PlayableDirector.UpdateMethod` is ignored and replaced by this property, which allows more fine-granular control per Timeline track.

##### Spine Animation State Clip

You can add a `Spine Animation State Clip` to a `SkeletonAnimation Track` (or `SkeletonGraphic Track`) by dragging an `AnimationReferenceAsset` onto a Timeline track. See the [SkeletonData - Preview](#Preview) section on how to generate `AnimationReferenceAssets` for a `SkeletonDataAsset`.

![](animationstate-clip-inspector.png)  

**Parameters**  
*Clip Timing*
- *Clip In.* An initial local start time offset applied when playing this animation. Can also be adjusted by dragging the left edge of the clip.
- *Blend In Duration.* Blend transition duration used when `Use Blend Duration` is enabled and `Default Mix Duration` is disabled. Can be adjusted by  moving the clip into the previous clip, resulting in a cross-fade triangle at the transition.
- *Speed Multiplier.* Playback speed multiplier. When set to 2.0 it will play the animation twice as fast, when set to 0.5 half as fast.

*Spine Animation State Clip*
- *Don't Pause with Director.* If set to true, the animation will continue playing when the Director is paused.
- *Don't End with Clip.* Normally when empty space follows the clip on the timeline, the empty animation is set on the track. Set this parameter to true to continue playing the clip's animation instead.
- *Clip End Mix Out Duration.* When `Don't End with Clip` is false, and the clip is followed by blank space or stopped, the empty animation is set with this `MixDuration`. When set to a value less than 0, the clip is paused instead.

*Mixing Setting*
- *Default Mix Duration.* When disabled, the value under `Mix Duration` below is used for transitions from the previous animation to this animation. When enabled, it will use the setup `Mix Duration` value at the SkeletonData asset for the animation pair.
- *Use Blend Duration.* When enabled, the value under `Mix Duration` will be synced with the timeline clip transition duration 'Ease In Duration'. Enable this value to adjust transition durations by moving the clip into the previous clip, resulting in a cross-fade triangle at the transition.
- *Mix Duration.* When *Default Mix Duration* is disabled, this mix duration is used for transitions from the previous animation to this animation.
- *Event Threshold.* See [TrackEntry.EventThreshold](http://esotericsoftware.com/spine-api-reference#TrackEntry-eventThreshold).
- *Attachment Threshold.* See [TrackEntry.AttachmentThreshold](http://esotericsoftware.com/spine-api-reference#TrackEntry-attachmentThreshold).
- *Draw Order Threshold.* See [TrackEntry.DrawOrderThreshold](http://esotericsoftware.com/spine-api-reference#TrackEntry-drawOrderThreshold).
- *Alpha.* See [TrackEntry.Alpha](http://esotericsoftware.com/spine-api-reference#TrackEntry-alpha).

*Ignored Parameters*
- *Ease Out Duration, Blend Curves*. These parameters are ignored and have no effect.

> Note: *Default Mix Duration* was previously called *Custom Duration* and inverted. This has been changed to improve readability.

**Usage**
1. Add `SkeletonAnimationPlayableHandle` component to your SkeletonAnimation GameObject, or the `SkeletonGraphicPlayableHandle` in case of `SkeletonGraphic`.
2. With an existing Unity Playable Director, and in the Unity Timeline window, right-click on an empty space on the left and choose `Spine` - `SkeletonAnimation Track`.
3. Drag the SkeletonAnimation or SkeletonGraphic GameObject onto the empty reference property of the new Spine AnimationState Track.
4. To add an animation at a track, drag the respective [`AnimationReferenceAsset`](#Preview) into the clips view (the right part of the Timeline panel) as you would with normal animation clips.

See spine-unity Runtime Documentation, [section Preview](#Preview) on how to create an `AnimationReferenceAsset` for each of your animations.

> **Note:** You can use the Duplicate function (`CTRL`/`CMD` + `D`) to duplicate selected clips in the clips view.

**Track Behavior**
- `AnimationState.SetAnimation()` will be called at the beginning of every clip based on the `AnimationReferenceAsset`.
- Clip durations matter since Timeline version 4.0.
	> Note: Behaviour of version 3.8 was that clip durations don't matter. Animations were not cleared when reaching empty space after a clip on the timeline.
- Empty animation: If a clip has no `AnimationReferenceAsset` assigned, it will call `SetEmptyAnimation` instead.
- Error handling: If the animation with the provided `AnimationReferenceAsset` is not found, it will do nothing (the previous animation will continue playing normally).
- Animations playing before the timeline starts playing will not be interrupted until the first clip starts playing.
- What happens at the end of clips and at the end of the timeline depends on clip settings. When `Don't End with Clip` is true, nothing happens at the clip's end. This means the effect of the last clip's `SetAnimation` call will persist until you issue other calls at the AnimationState. If `Don't End with Clip` is false, it will be mix out to the empty animation with duration `Clip End Mix Out Duration`, or paused when `Clip End Mix Out Duration` is less than 0.
- Edit mode preview mixing may look different from Play Mode mixing. Please check in actual Play Mode to see the actual results. Please see the remark in the  [Spine AnimationState Track](#Spine-AnimationState-Track) section on correct track ordering when previewing multiple overlaid tracks.

#### Skeleton Flip Track

![](skeleton-flip-track.png)  

This track type can be used to flip the skeleton of the target `SkeletonAnimation` or `SkeletonGraphic`.

##### Spine Skeleton Flip Clip

![](skeleton-flip-clip-inspector.png)  

**Parameters**
- *Flip X.* Flips the skeleton along the X axis during the extents of the clip.
- *Flip Y.* Flips the skeleton along the Y axis during the extents of the clip.

**Usage**
1. Add `SkeletonAnimationPlayableHandle` component to your SkeletonAnimation GameObject, or the `SkeletonGraphicPlayableHandle` in case of `SkeletonGraphic`.
2. With an existing Unity Playable Director, and in the Unity Timeline window, right-click on an empty space on the left and choose `Spine` - `Skeleton Flip Track`.
3. Drag the SkeletonAnimation or SkeletonGraphic GameObject onto the empty reference property of the new Spine Skeleton Flip Track.
4. Right-click on the row in an empty space in the Timeline dopesheet and choose `Add Spine Skeleton Flip Clip`.
5. Adjust the start and end times of the new clip, name it appropriately at the top of the Inspector, and choose the desired FlipX and FlipY values.

**Track Behavior**
- The specified skeleton flip values will be applied for every frame within the duration of each track.
- At the end of the timeline, the track will revert the skeleton flip to the flip values it captures when it starts playing that timeline. 

#### Known Issues
- The Console potentially logs an incorrect and harmless error `DrivenPropertyManager has failed to register property "m_Script" of object "Spine GameObject (spineboy-pro)" with driver "" because the property doesn't exist.`. This is a known issue on Unity's end. See more here: https://forum.unity.com/threads/default-playables-text-switcher-track-error.502903/
