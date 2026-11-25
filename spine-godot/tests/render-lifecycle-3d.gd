extends "batching-3d.gd"

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Render lifecycle tests require a GPU renderer.")
		quit(1)
		return
	fixture_dir = "user://spine-render-lifecycle-%d" % OS.get_process_id()
	_create_fixture()
	_create_viewport()
	await _test_hidden_allocations()
	await _test_insertion_ownership()
	_finish()

func _assert_clear(label: String, image: Image):
	var maximum := 0.0
	for y in image.get_height():
		for x in image.get_width():
			maximum = maxf(maximum, image.get_pixel(x, y).a)
	_check(maximum < 0.02, "%s: hidden geometry wrote alpha %s" % [label, maximum])

func _test_hidden_allocations():
	_check(sprite.get_render_statistics().batch_pool == 0, "first-allocation fixture starts without pooled instances")
	sprite.visible = false
	sprite.pixel_size = 1.0 # Property refresh allocates the first batch while hidden.
	_check(sprite.get_render_statistics().batch_pool == 1, "hidden property refresh actually allocated a batch")
	_assert_clear("first hidden allocation", await _render_image(false))
	sprite.visible = true
	_assert_visible("first batch reappears without pose advancement", (await _render_image(false)).get_pixel(BLACK_X, SAMPLE_Y))

	sprite.visible = false
	_set_clipping(false)
	var spare := sprite.get_skeleton().find_slot("spare")
	var attachment := sprite.get_skeleton().get_attachment_by_slot_name("spare", "region")
	spare.get_pose().set_attachment(attachment)
	spare.get_applied_pose().set_attachment(attachment)
	var helper := SpineSlotNode3D.new()
	helper.slot_name = "spare"
	helper.normal_material = _solid_material(Color.BLUE)
	sprite.add_child(helper)
	sprite.pixel_size = 1.0
	_check(sprite.get_render_statistics().batch_pool == 2, "hidden material boundary grows the instance pool")
	_assert_clear("hidden pool growth", await _render_image(false))
	sprite.visible = true
	_assert_visible("new pool entry reappears without pose advancement", (await _render_image(false)).get_pixel(WHITE_X, SAMPLE_Y))
	helper.free()
	_reset_fixture_state()

	# Also cover inherited visibility before the first allocation.
	sprite.visible = false
	var parent := Node3D.new()
	parent.visible = false
	viewport.add_child(parent)
	var target := SpineSprite3D.new()
	_use_no_write_rendering(target)
	target.update_mode = SpineConstant.UpdateMode_Manual
	target.skeleton_data_res = skeleton_data
	parent.add_child(target)
	target.pixel_size = 1
	_check(target.get_render_statistics().batch_pool == 1, "inherited-hidden fixture allocated a batch")
	_assert_clear("first allocation under hidden parent", await _render_image(false))
	parent.visible = true
	_assert_visible("parent visibility restores the batch", (await _render_image(false)).get_pixel(BLACK_X, SAMPLE_Y))
	parent.free()
	sprite.visible = true

func _test_insertion_ownership():
	_reset_fixture_state()
	var red := Color(1, 0, 0, 0.5)
	var green := Color(0, 1, 0, 0.5)
	var blue := Color(0, 0, 1, 0.5)
	var yellow := Color(1, 1, 0, 0.5)
	sprite.normal_material = _solid_material(red)
	sprite.get_skeleton().set_attachment("spare", "region")
	var insertion := SpineSlotNode3D.new()
	insertion.slot_name = "slot"
	sprite.add_child(insertion)
	var last_slot := SpineSlotNode3D.new()
	last_slot.slot_name = "spare"
	last_slot.normal_material = _solid_material(blue)
	sprite.add_child(last_slot)
	var inserted := _quad(_solid_material(green))
	inserted.sorting_offset = 4.25
	inserted.sorting_use_aabb_center = true
	insertion.add_child(inserted)
	var expected := await _reference([red, green, blue])
	for disabled in [false, true]:
		sprite.update_mode = SpineConstant.UpdateMode_Process if disabled else SpineConstant.UpdateMode_Manual
		sprite.process_mode = Node.PROCESS_MODE_DISABLED if disabled else Node.PROCESS_MODE_INHERIT
		_assert_close("initial insertion composite", expected, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))
		var offset := inserted.sorting_offset
		var unrelated := Node.new()
		root.add_child(unrelated)
		_check(inserted.sorting_offset == offset and not inserted.sorting_use_aabb_center,
			"unrelated tree additions preserve owned sorting immediately (disabled=%s)" % disabled)
		_assert_close("unrelated additions preserve frozen GPU composition", expected, (await _render_image(false)).get_pixel(GRAY_X, SAMPLE_Y))
		unrelated.free()
		_check(inserted.sorting_offset == offset and not inserted.sorting_use_aabb_center, "unrelated removals preserve owned sorting")
		_assert_close("unrelated removals preserve frozen GPU composition", expected, (await _render_image(false)).get_pixel(GRAY_X, SAMPLE_Y))
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.process_mode = Node.PROCESS_MODE_INHERIT

	var bone := sprite.get_skeleton().find_bone("root")
	bone.set_active(false)
	sprite.pixel_size = 1
	_assert_clear("inactive-bone inserted geometry stays hidden", await _render_image(false))
	var unrelated := Node.new()
	root.add_child(unrelated)
	_assert_clear("unrelated mutation does not reveal inactive-bone geometry", await _render_image(false))
	unrelated.free()
	bone.set_active(true)
	sprite.pixel_size = 1

	var retained := _quad(_solid_material(yellow))
	retained.sorting_offset = 6.75
	retained.sorting_use_aabb_center = true
	insertion.add_child(retained)
	sprite.update_skeleton(0)
	var retained_offset := retained.sorting_offset
	inserted.reparent(last_slot)
	_check(inserted.sorting_offset == 4.25 and inserted.sorting_use_aabb_center, "departing geometry restores its original sorting before transfer")
	_check(retained.sorting_offset == retained_offset and not retained.sorting_use_aabb_center, "unchanged descendants retain ownership during a sibling transfer")
	# Actual helper hierarchy mutations retain the documented explicit refresh
	# requirement for manual sprites; unrelated mutations above do not.
	expected = await _reference([red, yellow, blue, green])
	_assert_close("transferred child is acquired by its new helper", expected, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))
	last_slot.remove_child(inserted)
	_check(inserted.sorting_offset == 4.25 and inserted.sorting_use_aabb_center, "new owner retained the true original values")
	inserted.free()
	insertion.remove_child(retained)
	_check(retained.sorting_offset == 6.75 and retained.sorting_use_aabb_center, "retained sibling restores its own original values on detach")
	retained.free()
	last_slot.free()
	insertion.free()
