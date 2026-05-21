/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#pragma once

#include "SpineCommon.h"
#include "SpineSprite.h"
#include "SpineSlotRangeProxy.h"
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/node.hpp>
#else
#include "scene/main/node.h"
#endif

// Convenience node mirroring spine-unity's SkeletonRenderSeparator. Given a
// source SpineSprite and a list of separator slot names, it partitions the
// skeleton's slots into N+1 contiguous ranges and creates one SpineSlotRangeProxy
// child per range. Insert other nodes between the proxy children to interleave
// them with the skeleton's draw order.
//
// For full manual control, place SpineSlotRangeProxy nodes directly instead of
// using this class. Note: the proxies are (re)built when this node enters the
// tree or its properties change; if the source swaps its skeleton afterwards,
// re-assign a property here to rebuild.
class SpineSpriteRenderSeparator : public Node {
	GDCLASS(SpineSpriteRenderSeparator, Node)

protected:
	NodePath source_sprite_path;
	PackedStringArray separator_slot_names;

	Vector<SpineSlotRangeProxy *> managed_proxies;
	bool proxies_dirty;

	static void _bind_methods();
	void _notification(int what);

	SpineSprite *resolve_source_sprite() const;
	void clear_proxies();
	bool rebuild_proxies();

public:
	SpineSpriteRenderSeparator();

	void set_source_sprite(const NodePath &path);
	NodePath get_source_sprite() const;

	void set_separator_slot_names(const PackedStringArray &names);
	PackedStringArray get_separator_slot_names() const;
};
