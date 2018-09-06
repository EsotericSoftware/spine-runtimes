/******************************************************************************
* Spine Runtimes Software License v2.5
*
* Copyright (c) 2013-2016, Esoteric Software
* All rights reserved.
*
* You are granted a perpetual, non-exclusive, non-sublicensable, and
* non-transferable license to use, install, execute, and perform the Spine
* Runtimes software and derivative works solely for personal or internal
* use. Without the written permission of Esoteric Software (see Section 2 of
* the Spine Software License Agreement), you may not (a) modify, translate,
* adapt, or develop new applications using the Spine Runtimes or otherwise
* create derivative works or improvements of the Spine Runtimes or (b) remove,
* delete, alter, or obscure any trademarks or any copyright, trademark, patent,
* or other intellectual property or proprietary rights notices on or in the
* Software, including any copy thereof. Redistributions in binary or source
* form must include this license and terms.
*
* THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
* MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
* EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
* SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
* PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
* USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
* IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
* POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

#include <spine/EventData.h>

#include <assert.h>

using namespace Spine;

EventData::EventData(const String &name) :
		_name(name),
		_intValue(0),
		_floatValue(0),
		_stringValue(),
		_audioPath(),
		_volume(1),
		_balance(0) {
	assert(_name.length() > 0);
}

/// The name of the event, which is unique within the skeleton.
const String &EventData::getName() const {
	return _name;
}

int EventData::getIntValue() {
	return _intValue;
}

void EventData::setIntValue(int inValue) {
	_intValue = inValue;
}

float EventData::getFloatValue() {
	return _floatValue;
}

void EventData::setFloatValue(float inValue) {
	_floatValue = inValue;
}

const String &EventData::getStringValue() {
	return _stringValue;
}

void EventData::setStringValue(const String &inValue) {
	_stringValue = inValue;
}

const String &EventData::getAudioPath() {
	return _audioPath;
}

void EventData::setAudioPath(const String &inValue) {
	_audioPath = inValue;
}


float EventData::getVolume() {
	return _volume;
}

void EventData::setVolume(float inValue) {
	_volume = inValue;
}

float EventData::getBalance() {
	return _balance;
}

void EventData::setBalance(float inValue) {
	_balance = inValue;
}
