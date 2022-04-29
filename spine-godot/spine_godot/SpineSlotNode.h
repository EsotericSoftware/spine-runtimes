#ifndef GODOT_SPINESLOTNODE_H
#define GODOT_SPINESLOTNODE_H

#include "SpineCommon.h"
#include "SpineSprite.h"
#include "scene/2d/node_2d.h"

class SpineSlotNode: public Node2D {
    GDCLASS(SpineSlotNode, Node2D)

protected:
    String slot_name;
    int slot_index;
    Ref<Material> normal_material;
    Ref<Material> additive_material;
    Ref<Material> multiply_material;
    Ref<Material> screen_material;
    SpineSprite *sprite;

    static void _bind_methods();
    void _notification(int what);
    void _get_property_list(List<PropertyInfo> *list) const;
    bool _get(const StringName &property, Variant &value) const;
    bool _set(const StringName &property, const Variant &value);
    void on_world_transforms_changed(const Variant &_sprite);
    void update_transform(SpineSprite *sprite);
public:
    SpineSlotNode();

    void set_slot_name(const String &_slot_name);

    String get_slot_name();

    int get_slot_index() { return slot_index; }

    Ref<Material> get_normal_material();

    void set_normal_material(Ref<Material> material);

    Ref<Material> get_additive_material();

    void set_additive_material(Ref<Material> material);

    Ref<Material> get_multiply_material();

    void set_multiply_material(Ref<Material> material);

    Ref<Material> get_screen_material();

    void set_screen_material(Ref<Material> material);
};

#endif
