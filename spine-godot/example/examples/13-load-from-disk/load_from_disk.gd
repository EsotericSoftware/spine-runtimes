extends Node2D

func _ready():
	# Load the skeleton file
	var skeleton_file_res = SpineSkeletonFileResource.new();
	skeleton_file_res.load_from_file("/Users/badlogic/workspaces/spine-runtimes/examples/coin/export/coin-pro.skel");
	
	# Load the atlas file
	var atlas_res = SpineAtlasResource.new();
	atlas_res.load_from_atlas_file("/Users/badlogic/workspaces/spine-runtimes/examples/coin/export/coin.atlas");
	
	# Create a skeleton data resource, you can share this across multiple sprites
	var skeleton_data_res = SpineSkeletonDataResource.new();
	skeleton_data_res.skeleton_file_res = skeleton_file_res;
	skeleton_data_res.atlas_res = atlas_res
	
	# Create a sprite from the skeleton data and add it as a child
	var sprite = SpineSprite.new();
	sprite.skeleton_data_res = skeleton_data_res;
	sprite.position.x = 200;
	sprite.position.y = 200;
	sprite.get_animation_state().set_animation("animation", true, 0);
	self.add_child(sprite)
	pass
