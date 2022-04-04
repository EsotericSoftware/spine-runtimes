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

#include "SpineSpriteAnimateDialog.h"

#ifdef TOOLS_ENABLED

#include "SpineSprite.h"

void SpineSpriteAnimateDialog::_bind_methods() {
	ClassDB::bind_method(D_METHOD("_on_animate_button_pressed"), &SpineSpriteAnimateDialog::_on_animate_button_pressed);
	ClassDB::bind_method(D_METHOD("_on_scene_tree_selected"), &SpineSpriteAnimateDialog::_on_scene_tree_selected);
	ClassDB::bind_method(D_METHOD("_on_scene_tree_hide"), &SpineSpriteAnimateDialog::_on_scene_tree_hide);
	ClassDB::bind_method(D_METHOD("_on_animate_dialog_action"), &SpineSpriteAnimateDialog::_on_animate_dialog_action);
}

SpineSpriteAnimateDialog::SpineSpriteAnimateDialog() {
	animate_dialog = memnew(ConfirmationDialog);
	add_child(animate_dialog);
	animate_dialog->get_ok()->set_text("Generate");
	animate_dialog_override_button = animate_dialog->add_button("Override", false, "override");
	animate_dialog_override_button->set_visible(false);
	animate_dialog->set_title("Animations Generator");
	animate_dialog->set_resizable(true);
	animate_dialog->set_custom_minimum_size(Vector2(550, 400));
	animate_dialog->set_hide_on_ok(false);
	animate_dialog->connect("custom_action", this, "_on_animate_dialog_action");
	Vector<Variant> al;
	al.push_back("confirmed");
	animate_dialog->connect("confirmed", this, "_on_animate_dialog_action", al);

	auto vb = memnew(VBoxContainer);
	animate_dialog->add_child(vb);

	auto scroll = memnew(ScrollContainer);
	scroll->set_h_size_flags(SIZE_EXPAND_FILL);
	scroll->set_v_size_flags(SIZE_EXPAND_FILL);
	//    vb->add_margin_child("Animations", scroll);
	vb->add_child(scroll);

	animate_dialog_tree = memnew(Tree);
	animate_dialog_tree->set_h_size_flags(SIZE_EXPAND_FILL);
	animate_dialog_tree->set_v_size_flags(SIZE_EXPAND_FILL);
	scroll->add_child(animate_dialog_tree);

	animate_dialog_tree->set_columns(3);
	animate_dialog_tree->set_column_titles_visible(true);
	animate_dialog_tree->set_hide_folding(true);
	animate_dialog_tree->set_hide_root(true);

	animate_dialog_tree->set_column_title(0, TTR("Animation"));
	animate_dialog_tree->set_column_title(1, TTR("Loop"));
	animate_dialog_tree->set_column_title(2, TTR("Track ID"));

	animate_dialog_tree->create_item();
	add_row("test1");
	add_row("test12");
	add_row("test13");

	auto l = memnew(Label);
	l->set_text("W.I.P");
	vb->add_child(l);

	scene_tree_dialog = memnew(SceneTreeDialog);
	scene_tree_dialog->set_title("Choose a AnimationPlayer to override, or choose none to create a new one.");
	Vector<StringName> valid_types;
	valid_types.push_back("AnimationPlayer");
	scene_tree_dialog->get_scene_tree()->set_valid_types(valid_types);
	scene_tree_dialog->get_scene_tree()->set_show_enabled_subscene(true);
	scene_tree_dialog->get_ok()->hide();
	add_child(scene_tree_dialog);
	scene_tree_dialog->connect("selected", this, "_on_scene_tree_selected");
	scene_tree_dialog->connect("popup_hide", this, "_on_scene_tree_hide");

	error_dialog = memnew(AcceptDialog);
	add_child(error_dialog);
}

SpineSpriteAnimateDialog::~SpineSpriteAnimateDialog() {
}

void SpineSpriteAnimateDialog::set_animate_button(ToolButton *b) {
	animate_button = b;
	animate_button->connect("pressed", this, "_on_animate_button_pressed");
}

void SpineSpriteAnimateDialog::add_row(const String &animation, bool loop, int64_t track_id) {
	auto item = animate_dialog_tree->create_item();
	item->set_text(0, animation);

	item->set_cell_mode(1, TreeItem::CELL_MODE_CHECK);
	item->set_checked(1, loop);
	item->set_editable(1, true);

	item->set_cell_mode(2, TreeItem::CELL_MODE_RANGE);
	item->set_range(2, track_id);
	item->set_editable(2, true);
}

void SpineSpriteAnimateDialog::clear_tree() {
	animate_dialog_tree->clear();
	animate_dialog_tree->create_item();
}

void SpineSpriteAnimateDialog::error(const String &text, const String &title) {
	error_dialog->set_text(text);
	error_dialog->set_title(title);
	error_dialog->popup_centered();
}

#define ERROR_MSG(x) \
	do {             \
		error(x);    \
		err = true;  \
		return;      \
	} while (false)
void SpineSpriteAnimateDialog::load_data_from_sprite(SpineSprite *sprite, bool &err) {
	if (sprite == nullptr) {
		ERROR_MSG("The sprite is null.");
	}
	if (!sprite->get_animation_state().is_valid() || !sprite->get_skeleton().is_valid()) {
		ERROR_MSG("The sprite is not loaded.");
	}
	clear_tree();

	Vector<String> animations;
	sprite->get_skeleton()->get_data()->get_animation_names(animations);

	for (size_t i = 0; i < animations.size(); ++i) {
		add_row(animations[i]);
	}

	err = false;
}

#define MIN_TRACK_LENGTH 0.15

void SpineSpriteAnimateDialog::gen_new_animation_player(SpineSprite *sprite, bool &err) {
	if (sprite == nullptr) {
		ERROR_MSG("The sprite player is null.");
	}
	if (!sprite->get_animation_state().is_valid() || !sprite->get_skeleton().is_valid()) {
		ERROR_MSG("The sprite is not loaded.");
	}
	auto p = sprite->get_parent();
	if (p == nullptr) {
		p = sprite;
	}

	auto anim_player = memnew(AnimationPlayer);
	anim_player->set_name("AnimationPlayer");
	p->add_child(anim_player);
	anim_player->set_owner(sprite->get_owner());
	anim_player->set_root(anim_player->get_path_to(p));

	gen_animations(sprite, anim_player, get_data_from_tree(), MIN_TRACK_LENGTH, err);
}

Dictionary SpineSpriteAnimateDialog::get_data_from_tree() {
	Dictionary res;
	if (animate_dialog_tree->get_root() == nullptr) return res;

	auto item = animate_dialog_tree->get_root()->get_children();
	while (item) {
		Dictionary row;
		row["loop"] = item->is_checked(1);
		row["track_id"] = item->get_range(2);

		res[item->get_text(0)] = row;
		item = item->get_next();
	}
	return res;
}

void SpineSpriteAnimateDialog::gen_animations(SpineSprite *sprite, AnimationPlayer *anim_player, const Dictionary &config, float min_duration, bool &err) {
	if (sprite == nullptr || anim_player == nullptr) {
		ERROR_MSG("The sprite or animation player is null.");
	}
	if (!sprite->get_animation_state().is_valid() || !sprite->get_skeleton().is_valid()) {
		ERROR_MSG("The sprite is not loaded.");
	}
	if (anim_player->get_node_or_null(anim_player->get_root()) == nullptr) {
		ERROR_MSG("The root node of animation player is null.");
	}

	auto path_to_sprite = anim_player->get_node(anim_player->get_root())->get_path_to(sprite);

	Array animations = sprite->get_skeleton()->get_data()->get_animations();
	for (size_t i = 0; i < animations.size(); ++i) {
		auto spine_anim = (Ref<SpineAnimation>) animations[i];

		Dictionary ca;
		if (config.has(spine_anim->get_anim_name())) {
			ca = config[spine_anim->get_anim_name()];
		}

		if (!ca.has("loop")) ca["loop"] = true;
		if (!ca.has("track_id")) ca["track_id"] = 0;

		Array key_frame_value;
		key_frame_value.push_back(gen_current_animation_data(spine_anim->get_anim_name(), ca["track_id"], ca["loop"], false, false, 0.3, 0));

		auto anim = Ref<Animation>(memnew(Animation));
		auto track_index = anim->add_track(Animation::TYPE_VALUE);
		anim->set_length(min_duration > spine_anim->get_duration() ? min_duration : spine_anim->get_duration());
		anim->track_set_path(track_index, NodePath(vformat("%s:current_animations", path_to_sprite)));
		anim->track_insert_key(track_index, 0.0, key_frame_value);
		anim->value_track_set_update_mode(track_index, Animation::UPDATE_DISCRETE);

		if (anim_player->has_animation(spine_anim->get_anim_name()))
			anim_player->remove_animation(spine_anim->get_anim_name());
		anim_player->add_animation(spine_anim->get_anim_name(), anim);
	}

	err = false;
}

Dictionary SpineSpriteAnimateDialog::gen_current_animation_data(const String &animation, int64_t track_id, bool loop, bool clear, bool empty, bool empty_duration, float delay) {
	Dictionary res;
	res["animation"] = animation;
	res["track_id"] = track_id;
	res["loop"] = loop;
	res["clear"] = clear;
	res["empty"] = empty;
	res["empty_animation_duration"] = empty_duration;
	res["delay"] = delay;
	return res;
}


void SpineSpriteAnimateDialog::load_data_from_anim_player(AnimationPlayer *anim_player, bool &err) {
	if (anim_player == nullptr) {
		ERROR_MSG("The animation player is null.");
	}
	auto root = anim_player->get_node_or_null(anim_player->get_root());
	if (root == nullptr) return;

	auto sprite = get_node_or_null(spine_sprite_path);
	if (sprite == nullptr) return;

	auto item = animate_dialog_tree->get_root()->get_children();
	while (item) {
		String animation = item->get_text(0);

		auto anim = anim_player->get_animation(animation);
		if (anim.is_valid() && anim->get_track_count() > 0) {
			if (anim->track_get_type(0) == Animation::TYPE_VALUE) {
				auto track_path = anim->track_get_path(0);
				if (root->get_node_or_null(track_path) == sprite) {
					if (anim->track_get_key_count(0) > 0) {
						Array key_frame_value = anim->track_get_key_value(0, 0);
						if (!key_frame_value.empty()) {
							Dictionary _ca = key_frame_value.front();
							if (_ca.has("loop")) item->set_checked(1, _ca["loop"]);
							if (_ca.has("track_id")) item->set_range(2, _ca["track_id"]);
						}
					}
				}
			}
		}

		item = item->get_next();
	}

	err = false;
}

//----- Signals -----
void SpineSpriteAnimateDialog::_on_scene_tree_selected(NodePath path) {
	//    print_line(vformat("anime: %s", path));
	auto node = get_node_or_null(path);
	if (node == nullptr) {
		error("The node you chose is null.");
		return;
	}
	if (!node->is_class("AnimationPlayer")) {
		error("The node you chose is not AnimationPlayer.");
		return;
	}
	anim_player_path = path;
}

void SpineSpriteAnimateDialog::_on_animate_button_pressed() {
	anim_player_path = String("");
	auto node = (Node *) the_plugin->get_editor_interface()->get_selection()->get_selected_nodes().back();
	spine_sprite_path = node->get_path();

	//    print_line(vformat("sp: %s", spine_sprite_path));

	animate_dialog_override_button->set_visible(false);
	scene_tree_dialog->popup_centered_ratio();
}

void SpineSpriteAnimateDialog::_on_scene_tree_hide() {
	animate_dialog->popup_centered();

	bool err = false;
	load_data_from_sprite((SpineSprite *) get_node_or_null(spine_sprite_path), err);

	if (err) animate_dialog->hide();

	err = false;
	auto node = get_node_or_null(anim_player_path);
	if (node != nullptr) {
		load_data_from_anim_player((AnimationPlayer *) node, err);
		animate_dialog_override_button->set_visible(!err);
	} else {
		animate_dialog_override_button->set_visible(false);
	}
}

void SpineSpriteAnimateDialog::_on_animate_dialog_action(const String &act) {
	bool err = false;
	if (act == "confirmed") {
		gen_new_animation_player((SpineSprite *) get_node_or_null(spine_sprite_path), err);
	} else if (act == "override") {
		gen_animations((SpineSprite *) get_node_or_null(spine_sprite_path), (AnimationPlayer *) get_node_or_null(anim_player_path), get_data_from_tree(), MIN_TRACK_LENGTH, err);
	}
	if (!err) {
		animate_dialog->hide();
	}
}


#endif