extends SpineSprite

func _ready():
	
	# Test SpineAnimation
	var walkAnim: SpineAnimation = get_skeleton().get_data().find_animation("walk")
	assert(walkAnim.get_name() == "walk")
	var duration = walkAnim.get_duration()
	walkAnim.set_duration(duration + 1)
	assert(walkAnim.get_duration() == duration + 1)
	assert(walkAnim.get_timelines().size() == 39)
	var timeline: SpineTimeline = walkAnim.get_timelines()[0]
	var propertyIds = timeline.getPropertyIds()
	assert(walkAnim.has_timeline(propertyIds))
	assert(!walkAnim.has_timeline([0]))
