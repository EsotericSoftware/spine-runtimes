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

#ifndef GODOT_SPINESPRITEANIMATEDIALOG_H
#define GODOT_SPINESPRITEANIMATEDIALOG_H

#ifdef TOOLS_ENABLED
#include "editor/editor_node.h"

class SpineSprite;

class SpineSpriteAnimateDialog : public Control {
    GDCLASS(SpineSpriteAnimateDialog, Control);

protected:
    static void _bind_methods();

    AcceptDialog *error_dialog;

    ToolButton *animate_button;
    EditorPlugin *the_plugin;

    ConfirmationDialog *animate_dialog;
    Button *animate_dialog_override_button;
    Tree *animate_dialog_tree;
    SceneTreeDialog *scene_tree_dialog;

    NodePath spine_sprite_path;
    NodePath anim_player_path;

    void add_row(const String &animation, bool loop=true, int64_t track_id=0);
    void clear_tree();

    void error(const String &text, const String &title="Error");

    void load_data_from_sprite(SpineSprite *sprite, bool &err);
    void load_data_from_anim_player(AnimationPlayer *anim_player, bool &err);

    Dictionary get_data_from_tree();

    void gen_new_animation_player(SpineSprite *sprite, bool &err);
    void gen_animations(SpineSprite *sprite, AnimationPlayer *anim_player, const Dictionary &config, float min_duration, bool &err);
    Dictionary gen_current_animation_data(const String &animation, int64_t track_id, bool loop, bool clear, bool empty, bool empty_duration, float delay);
public:
    SpineSpriteAnimateDialog();
    ~SpineSpriteAnimateDialog();

    void set_animate_button(ToolButton *b);
    inline ToolButton *get_animate_button() {return animate_button;}

    inline void set_plugin(EditorPlugin *p) {the_plugin = p;}

    void _on_animate_button_pressed();
    void _on_scene_tree_selected(NodePath path);
    void _on_scene_tree_hide();
    void _on_animate_dialog_action(const String &act);
};
#endif

#endif //GODOT_SPINESPRITEANIMATEDIALOG_H
