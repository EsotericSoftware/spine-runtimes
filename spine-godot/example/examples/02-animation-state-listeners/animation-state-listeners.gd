extends Node2D

onready var footstep_audio: AudioStreamPlayer = $FootstepAudio

func _animation_started(sprite: SpineSprite, animation_state: SpineAnimationState, track_entry: SpineTrackEntry):
	print("Animation started: " + track_entry.get_animation().get_name())

func _animation_interrupted(sprite: SpineSprite, animation_state: SpineAnimationState, track_entry: SpineTrackEntry):
	print("Animation interrupted: " + track_entry.get_animation().get_name())

func _animation_ended(sprite: SpineSprite, animation_state: SpineAnimationState, track_entry: SpineTrackEntry):
	print("Animation ended: " + track_entry.get_animation().get_name())

func _animation_completed(sprite: SpineSprite, animation_state: SpineAnimationState, track_entry: SpineTrackEntry):
	print("Animation completed: " + track_entry.get_animation().get_name())

func _animation_disposed(sprite: SpineSprite, animation_state: SpineAnimationState, track_entry: SpineTrackEntry):
	print("Animation disposed: " + track_entry.get_animation().get_name())
	
func _animation_event(sprite: SpineSprite, animation_state: SpineAnimationState, track_entry: SpineTrackEntry, event: SpineEvent):
	print("Animation event: " + track_entry.get_animation().get_name() + ", " + event.get_data().get_event_name())
	if (event.get_data().get_event_name() == "footstep"):		
		footstep_audio.play()

func _ready():
	var spineboy = $Spineboy
	spineboy.connect("animation_started", self, "_animation_started")
	spineboy.connect("animation_interrupted", self, "_animation_interrupted")
	spineboy.connect("animation_ended", self, "_animation_ended")
	spineboy.connect("animation_completed", self, "_animation_completed")
	spineboy.connect("animation_disposed", self, "_animation_disposed")
	spineboy.connect("animation_event", self, "_animation_event")
	
	var animation_state = spineboy.get_animation_state()
	animation_state.set_animation("jump", false, 0)
	animation_state.add_animation("walk", 0, true, 0)
	animation_state.add_animation("run", 2, true, 0)
		
	pass
