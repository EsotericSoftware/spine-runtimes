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
#include "SpineAttachment.h"

class SpineSkeletonDataResource;
class SpineSprite;

class SpineSkin : public SpineSkeletonDataResourceOwnedObject<spine::Skin> {
	GDCLASS(SpineSkin, SpineObjectWrapper)

protected:
	static void _bind_methods();

private:
	bool owns_skin;

public:
	SpineSkin();
	~SpineSkin() override;

	Ref<SpineSkin> init(const String &name, SpineSprite *sprite);

	void set_attachment(int slot_index, const String &name, Ref<SpineAttachment> attachment);

	Ref<SpineAttachment> get_attachment(int slot_index, const String &name);

	void remove_attachment(int slot_index, const String &name);

	Array find_names_for_slot(int slot_index);

	Array find_attachments_for_slot(int slot_index);

	String get_name();

	void add_skin(Ref<SpineSkin> other);

	void copy_skin(Ref<SpineSkin> other);

	Array get_attachments();

	Array get_bones();

	Array get_constraints();
};

class SpineSkinEntry : public REFCOUNTED {
	GDCLASS(SpineSkinEntry, REFCOUNTED);

	friend class SpineSkin;

protected:
	static void _bind_methods();

	void init(int _slot_index, const String &_name, Ref<SpineAttachment> _attachment) {
		this->slot_index = _slot_index;
		this->name = _name;
		this->attachment = _attachment;
	}

private:
	int slot_index;
	String name;
	Ref<SpineAttachment> attachment;

public:
	SpineSkinEntry();

	int get_slot_index();

	const String &get_name();

	Ref<SpineAttachment> get_attachment();
};
