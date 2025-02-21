extends SpineSprite

func test_spine_animation():
	var walkAnim: SpineAnimation = get_skeleton().get_data().find_animation("walk")
	assert(walkAnim.get_name() == "walk")
	var duration = walkAnim.get_duration()
	walkAnim.set_duration(duration + 1)
	assert(walkAnim.get_duration() == duration + 1)
	assert(walkAnim.get_timelines().size() == 39)
	var timeline: SpineTimeline = walkAnim.get_timelines()[0]
	var propertyIds = timeline.get_property_ids()
	assert(walkAnim.has_timeline(propertyIds))
	assert(!walkAnim.has_timeline([0]))
	
func test_spine_timeline():
	var walkAnim: SpineAnimation = get_skeleton().get_data().find_animation("walk")
	var timeline: SpineTimeline = walkAnim.get_timelines()[0]
	assert(timeline.get_duration() == 1)
	assert(timeline.get_property_ids() == [4294967300])
	assert(timeline.get_type() == "RotateTimeline")

func test_spine_object_invalidation():
	var skeleton_data = get_skeleton().get_data()
	var bone_data = skeleton_data.find_bone("gun");
	var old_bone_data_x = bone_data.get_x();
	var bone = get_skeleton().find_bone("gun")
	var old_bone_x = bone.get_x()
	skeleton_data_res = null
	assert(old_bone_x != bone.get_x())
	assert(old_bone_data_x == bone_data.get_x())
	skeleton_data.atlas_res = null;
	assert(old_bone_data_x != bone_data.get_x())

func _ready():
	
	test_spine_animation()
	test_spine_timeline()
	test_spine_object_invalidation()
	print("All tests passed")
