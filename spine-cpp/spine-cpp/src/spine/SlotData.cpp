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

#include <spine/SlotData.h>

#include <assert.h>

namespace Spine
{
    SlotData::SlotData(int index, std::string name, BoneData& boneData) :
    _index(index),
    _name(name),
    _boneData(boneData),
    _r(1),
    _g(1),
    _b(1),
    _a(1),
    _r2(0),
    _g2(0),
    _b2(0),
    _a2(1),
    _hasSecondColor(false),
    _attachmentName(),
    _blendMode(BlendMode_Normal) {
        assert(_index >= 0);
        assert(_name.length() > 0);
    }
    
    const int SlotData::getIndex() {
        return _index;
    }
    
    const std::string& SlotData::getName() {
        return _name;
    }
    
    BoneData& SlotData::getBoneData() {
        return _boneData;
    }
    
    float SlotData::getR() {
        return _r;
    }
    
    void SlotData::setR(float inValue) {
        _r = inValue;
    }
    
    float SlotData::getG() {
        return _g;
    }
    
    void SlotData::setG(float inValue) {
        _g = inValue;
    }
    
    float SlotData::getB() {
        return _b;
    }
    
    void SlotData::setB(float inValue) {
        _b = inValue;
    }
    
    float SlotData::getA() {
        return _a;
    }
    
    void SlotData::setA(float inValue) {
        _a = inValue;
    }
    
    float SlotData::getR2() {
        return _r2;
    }
    
    void SlotData::setR2(float inValue) {
        _r2 = inValue;
    }
    
    float SlotData::getG2() {
        return _g2;
    }
    
    void SlotData::setG2(float inValue) {
        _g2 = inValue;
    }
    
    float SlotData::getB2() {
        return _b2;
    }
    
    void SlotData::setB2(float inValue) {
        _b2 = inValue;
    }
    
    bool SlotData::hasSecondColor() {
        return _hasSecondColor;
    }
    
    void SlotData::setHasSecondColor(bool inValue) {
        _hasSecondColor = inValue;
    }
    
    std::string SlotData::getAttachmentName() {
        return _attachmentName;
    }
    
    void SlotData::setAttachmentName(std::string inValue) {
        _attachmentName = inValue;
    }
    
    BlendMode SlotData::getBlendMode() {
        return _blendMode;
    }
    
    void SlotData::setBlendMode(BlendMode inValue) {
        _blendMode = inValue;
    }
}
