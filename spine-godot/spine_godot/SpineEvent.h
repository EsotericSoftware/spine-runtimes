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

#ifndef GODOT_SPINEEVENT_H
#define GODOT_SPINEEVENT_H

#include "core/variant_parser.h"

#include <spine/spine.h>

#include "SpineEventData.h"

class SpineEvent : public Reference{
	GDCLASS(SpineEvent, Reference);

protected:
	static void _bind_methods();
private:
	spine::Event *event;
public:
	SpineEvent();
	~SpineEvent();

	inline void set_spine_object(spine::Event *e){
		event = e;
	}
	inline spine::Event *get_spine_object() const{
		return event;
	}

	enum EventType{
		EVENTTYPE_START = spine::EventType_Start,
		EVENTTYPE_INTERRUPT = spine::EventType_Interrupt,
		EVENTTYPE_END = spine::EventType_End,
		EVENTTYPE_COMPLETE = spine::EventType_Complete,
		EVENTTYPE_DISPOSE = spine::EventType_Dispose,
		EVENTTYPE_EVENT = spine::EventType_Event
	};


	Ref<SpineEventData> get_data();

	String get_event_name();

	float get_time();

	int get_int_value();
	void set_int_value(int inValue);

	float get_float_value();
	void set_float_value(float inValue);

	String get_string_value();
	void set_string_value(const String &inValue);

	float get_volume();
	void set_volume(float inValue);

	float get_balance();
	void set_balance(float inValue);
};

#endif //GODOT_SPINEEVENT_H
