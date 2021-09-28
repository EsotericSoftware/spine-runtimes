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

#ifndef GODOT_SPINECOLLISIONSHAPEPROXY_H
#define GODOT_SPINECOLLISIONSHAPEPROXY_H

#include "scene/2d/collision_polygon_2d.h"

class SpineSprite;
class SpineAnimationState;
class SpineSkeleton;

class SpineCollisionShapeProxy : public CollisionPolygon2D{
    GDCLASS(SpineCollisionShapeProxy, CollisionPolygon2D)
protected:
    static void _bind_methods();

    NodePath spine_sprite_path;

    String slot;

    bool sync_transform;
protected:
    void _notification(int p_what);
    void _get_property_list(List<PropertyInfo> *p_list) const;
    bool _get(const StringName &p_property, Variant &r_value) const;
    bool _set(const StringName &p_property, const Variant &p_value);


    SpineSprite *get_spine_sprite() const;

    void _update_polygon_from_spine_sprite(SpineSprite *sprite);
    void _clear_polygon();
    void _sync_transform(SpineSprite *sprite);

    void _get_slot_list(Vector<String> &res) const;
public:
    SpineCollisionShapeProxy();
    ~SpineCollisionShapeProxy();

    NodePath get_spine_sprite_path();
    void set_spine_sprite_path(NodePath v);

    String get_slot() const;
    void set_slot(const String &v);

    bool get_sync_transform();
    void set_sync_transform(bool v);
};


#endif //GODOT_SPINECOLLISIONSHAPEPROXY_H
