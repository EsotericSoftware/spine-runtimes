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

#include <spine/Event.h>

#include <spine/EventData.h>

using namespace spine;

Event::Event(float time, const EventData &data) : _data(data), _time(time), _intValue(0), _floatValue(0), _stringValue(), _volume(0), _balance(0) {
}

const EventData &Event::getData() {
	return _data;
}

float Event::getTime() {
	return _time;
}

int Event::getInt() {
	return _intValue;
}

void Event::setInt(int inValue) {
	_intValue = inValue;
}

float Event::getFloat() {
	return _floatValue;
}

void Event::setFloat(float inValue) {
	_floatValue = inValue;
}

const String &Event::getString() {
	return _stringValue;
}

void Event::setString(const String &inValue) {
	_stringValue = inValue;
}


float Event::getVolume() {
	return _volume;
}

void Event::setVolume(float inValue) {
	_volume = inValue;
}

float Event::getBalance() {
	return _balance;
}

void Event::setBalance(float inValue) {
	_balance = inValue;
}
