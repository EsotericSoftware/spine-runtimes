extends RefCounted

static func create_data(directory: String) -> SpineSkeletonDataResource:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(directory))
	var image := Image.create(1, 1, false, Image.FORMAT_RGBA8)
	image.fill(Color.WHITE)
	assert(image.save_png(directory.path_join("pixel.png")) == OK)
	var atlas_file := FileAccess.open(directory.path_join("pixel.atlas"), FileAccess.WRITE)
	atlas_file.store_string("pixel.png\nsize: 1, 1\nfilter: Nearest, Nearest\npixel\nbounds: 0, 0, 1, 1\n")
	atlas_file.close()
	var file := FileAccess.open(directory.path_join("skeleton.spine-json"), FileAccess.WRITE)
	file.store_string(JSON.stringify({"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}],
		"slots": [{"name": "slot", "bone": "root", "attachment": "pixel"}],
		"skins": [{"name": "default", "attachments": {"slot": {"pixel": {"type": "region", "width": 2, "height": 2}}}}],
		"animations": {
			"walk": {"bones": {"root": {"rotate": [{"value": 0}, {"time": 2, "value": 90}]}}},
			"run": {"bones": {"root": {"rotate": [{"value": -90}, {"time": 2, "value": 0}]}}}
		}}))
	file.close()
	var atlas := SpineAtlasResource.new()
	assert(atlas.load_from_atlas_file(directory.path_join("pixel.atlas")) == OK)
	var skeleton_file := SpineSkeletonFileResource.new()
	assert(skeleton_file.load_from_file(directory.path_join("skeleton.spine-json")) == OK)
	var data := SpineSkeletonDataResource.new()
	data.atlas_res = atlas
	data.skeleton_file_res = skeleton_file
	return data

static func create_sprite(scene: Node, data: SpineSkeletonDataResource, three_d: bool) -> Node:
	var sprite: Node = SpineSprite3D.new() if three_d else SpineSprite.new()
	sprite.name = "Sprite3D" if three_d else "Sprite2D"
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.skeleton_data_res = data
	scene.add_child(sprite)
	sprite.owner = scene
	var track := SpineAnimationTrack.new()
	track.name = "Track"
	sprite.add_child(track)
	track.owner = scene
	return sprite

static func create_master(scene: Node, sprites: Array) -> AnimationPlayer:
	var timeline_root := Node.new()
	timeline_root.name = "NestedTimeline"
	scene.add_child(timeline_root)
	timeline_root.owner = scene
	var player := AnimationPlayer.new()
	player.name = "Director"
	player.root_node = NodePath("../..")
	player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
	timeline_root.add_child(player)
	player.owner = scene
	var animation := Animation.new()
	animation.length = 4
	for sprite in sprites:
		var child_player: AnimationPlayer = sprite.get_node("Track").get_child(0)
		var index := animation.add_track(Animation.TYPE_ANIMATION)
		animation.track_set_path(index, scene.get_path_to(child_player))
		animation.animation_track_insert_key(index, 0, "walk_looped")
		animation.animation_track_insert_key(index, 1, "run")
		animation.animation_track_insert_key(index, 3, "-- Empty --")
	var library := AnimationLibrary.new()
	library.add_animation("cutscene", animation)
	player.add_animation_library("", library)
	return player

static func cleanup(directory: String):
	for name in ["pixel.png", "pixel.atlas", "skeleton.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(directory.path_join(name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(directory))
