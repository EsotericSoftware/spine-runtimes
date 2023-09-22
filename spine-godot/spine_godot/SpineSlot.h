/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#pragma once

#include "SpineCommon.h"
#include "SpineSlotData.h"
#include "SpineAttachment.h"
#include "SpineBone.h"

class SpineSkeleton;
class SpineSprite;

class SpineSlot : public SpineSpriteOwnedObject<spine::Slot> {
	GDCLASS(SpineSlot, SpineObjectWrapper)

private:
	Ref<SpineBone> _bone;
	Ref<SpineSlotData> _data;

protected:
	static void _bind_methods();

public:
	void set_to_setup_pose();

	Ref<SpineSlotData> get_data();

	Ref<SpineBone> get_bone();

	Color get_color();

	void set_color(Color v);

	Color get_dark_color();

	void set_dark_color(Color v);

	bool has_dark_color();

	Ref<SpineAttachment> get_attachment();

	void set_attachment(Ref<SpineAttachment> v);

	int get_attachment_state();

	void set_attachment_state(int v);

	Array get_deform();

	void set_deform(Array v);

	int get_sequence_index();

	void set_sequence_index(int v);
};
