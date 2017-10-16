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

#include <spine/Bone.h>

#include <spine/BoneData.h>
#include <spine/Skeleton.h>

#include <spine/MathUtil.h>
#include <spine/TransformMode.h>

namespace Spine
{
    Bone::Bone(BoneData& data, Skeleton& skeleton, Bone* parent) : Updatable(),
    _data(data),
    _skeleton(skeleton),
    _parent(parent),
    _x(0),
    _y(0),
    _rotation(0),
    _scaleX(0),
    _scaleY(0),
    _shearX(0),
    _shearY(0),
    _ax(0),
    _ay(0),
    _arotation(0),
    _ascaleX(0),
    _ascaleY(0),
    _ashearX(0),
    _ashearY(0),
    _appliedValid(false),
    _a(0),
    _b(0),
    _worldX(0),
    _c(0),
    _d(0),
    _worldY(0),
    _sorted(false)
    {
        setToSetupPose();
    }
    
    void Bone::update()
    {
        updateWorldTransform(_x, _y, _rotation, _scaleX, _scaleY, _shearX, _shearY);
    }
    
    void Bone::updateWorldTransform()
    {
        updateWorldTransform(_x, _y, _rotation, _scaleX, _scaleY, _shearX, _shearY);
    }
    
    void Bone::updateWorldTransform(float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY)
    {
        _ax = x;
        _ay = y;
        _arotation = rotation;
        _ascaleX = scaleX;
        _ascaleY = scaleY;
        _ashearX = shearX;
        _ashearY = shearY;
        _appliedValid = true;
        Skeleton& skeleton = _skeleton;
        
        Bone* parent = _parent;
        if (!parent)
        {
            // Root bone.
            float rotationY = rotation + 90 + shearY;
            float la = MathUtil::cosDeg(rotation + shearX) * scaleX;
            float lb = MathUtil::cosDeg(rotationY) * scaleY;
            float lc = MathUtil::sinDeg(rotation + shearX) * scaleX;
            float ld = MathUtil::sinDeg(rotationY) * scaleY;
            if (_skeleton.isFlipX())
            {
                x = -x;
                la = -la;
                lb = -lb;
            }
            
            _a = la;
            _b = lb;
            _c = lc;
            _d = ld;
            _worldX = x + _skeleton.getX();
            _worldY = y + _skeleton.getY();
            
            return;
        }
        
        float pa = parent->_a;
        float pb = parent->_b;
        float pc = parent->_c;
        float pd = parent->_d;
        
        _worldX = pa * x + pb * y + parent->_worldX;
        _worldY = pc * x + pd * y + parent->_worldY;
        
        switch (_data.getTransformMode())
        {
            case TransformMode_Normal:
            {
                float rotationY = rotation + 90 + shearY;
                float la = MathUtil::cosDeg(rotation + shearX) * scaleX;
                float lb = MathUtil::cosDeg(rotationY) * scaleY;
                float lc = MathUtil::sinDeg(rotation + shearX) * scaleX;
                float ld = MathUtil::sinDeg(rotationY) * scaleY;
                _a = pa * la + pb * lc;
                _b = pa * lb + pb * ld;
                _c = pc * la + pd * lc;
                _d = pc * lb + pd * ld;
                
                return;
            }
            case TransformMode_OnlyTranslation:
            {
                float rotationY = rotation + 90 + shearY;
                _a = MathUtil::cosDeg(rotation + shearX) * scaleX;
                _b = MathUtil::cosDeg(rotationY) * scaleY;
                _c = MathUtil::sinDeg(rotation + shearX) * scaleX;
                _d = MathUtil::sinDeg(rotationY) * scaleY;
                
                break;
            }
            case TransformMode_NoRotationOrReflection:
            {
                float s = pa * pa + pc * pc, prx;
                if (s > 0.0001f)
                {
                    s = fabs(pa * pd - pb * pc) / s;
                    pb = pc * s;
                    pd = pa * s;
                    prx = MathUtil::atan2(pc, pa) * RadDeg;
                }
                else
                {
                    pa = 0;
                    pc = 0;
                    prx = 90 - MathUtil::atan2(pd, pb) * RadDeg;
                }
                float rx = rotation + shearX - prx;
                float ry = rotation + shearY - prx + 90;
                float la = MathUtil::cosDeg(rx) * scaleX;
                float lb = MathUtil::cosDeg(ry) * scaleY;
                float lc = MathUtil::sinDeg(rx) * scaleX;
                float ld = MathUtil::sinDeg(ry) * scaleY;
                
                _a = pa * la - pb * lc;
                _b = pa * lb - pb * ld;
                _c = pc * la + pd * lc;
                _d = pc * lb + pd * ld;
                
                break;
            }
            case TransformMode_NoScale:
            case TransformMode_NoScaleOrReflection:
            {
                float cos = MathUtil::cosDeg(rotation);
                float sin = MathUtil::sinDeg(rotation);
                float za = pa * cos + pb * sin;
                float zc = pc * cos + pd * sin;
                float s = sqrt(za * za + zc * zc);
                if (s > 0.00001f)
                {
                    s = 1 / s;
                }
                
                za *= s;
                zc *= s;
                s = sqrt(za * za + zc * zc);
                float r = SPINE_PI / 2 + MathUtil::atan2(zc, za);
                float zb = MathUtil::cos(r) * s;
                float zd = MathUtil::sin(r) * s;
                float la = MathUtil::cosDeg(shearX) * scaleX;
                float lb = MathUtil::cosDeg(90 + shearY) * scaleY;
                float lc = MathUtil::sinDeg(shearX) * scaleX;
                float ld = MathUtil::sinDeg(90 + shearY) * scaleY;
                
                if (_data.getTransformMode() != TransformMode_NoScaleOrReflection ? pa * pd - pb * pc < 0 : _skeleton.isFlipX() != _skeleton.isFlipY())
                {
                    zb = -zb;
                    zd = -zd;
                }
                
                _a = za * la + zb * lc;
                _b = za * lb + zb * ld;
                _c = zc * la + zd * lc;
                _d = zc * lb + zd * ld;
                
                return;
            }
        }
        
        if (_skeleton.isFlipX())
        {
            _a = -_a;
            _b = -_b;
        }
    }
    
    void Bone::setToSetupPose()
    {
        BoneData& data = _data;
        _x = data.getX();
        _y = data.getY();
        _rotation = data.getRotation();
        _scaleX = data.getScaleX();
        _scaleY = data.getScaleY();
        _shearX = data.getShearX();
        _shearY = data.getShearY();
    }
    
    void Bone::worldToLocal(float worldX, float worldY, float& outLocalX, float& outLocalY)
    {
        float a = _a;
        float b = _b;
        float c = _c;
        float d = _d;
        
        float invDet = 1 / (a * d - b * c);
        float x = worldX - _worldX;
        float y = worldY - _worldY;
        
        outLocalX = (x * d * invDet - y * b * invDet);
        outLocalY = (y * a * invDet - x * c * invDet);
    }
    
    void Bone::localToWorld(float localX, float localY, float& outWorldX, float& outWorldY)
    {
        outWorldX = localX * _a + localY * _b + _worldX;
        outWorldY = localX * _c + localY * _d + _worldY;
    }
    
    float Bone::worldToLocalRotation(float worldRotation)
    {
        float sin = MathUtil::sinDeg(worldRotation);
        float cos = MathUtil::cosDeg(worldRotation);
        
        return MathUtil::atan2(_a * sin - _c * cos, _d * cos - _b * sin) * RadDeg;
    }
    
    float Bone::localToWorldRotation(float localRotation)
    {
        float sin = MathUtil::sinDeg(localRotation);
        float cos = MathUtil::cosDeg(localRotation);
        
        return MathUtil::atan2(cos * _c + sin * _d, cos * _a + sin * _b) * RadDeg;
    }
    
    void Bone::rotateWorld(float degrees)
    {
        float a = _a;
        float b = _b;
        float c = _c;
        float d = _d;
        
        float cos = MathUtil::cosDeg(degrees);
        float sin = MathUtil::sinDeg(degrees);
        
        _a = cos * a - sin * c;
        _b = cos * b - sin * d;
        _c = sin * a + cos * c;
        _d = sin * b + cos * d;
        
        _appliedValid = false;
    }
    
    float Bone::getWorldToLocalRotationX()
    {
        Bone* parent = _parent;
        if (!parent)
        {
            return _arotation;
        }
        
        float pa = parent->_a;
        float pb = parent->_b;
        float pc = parent->_c;
        float pd = parent->_d;
        float a = _a;
        float c = _c;
        
        return MathUtil::atan2(pa * c - pc * a, pd * a - pb * c) * RadDeg;
    }
    
    float Bone::getWorldToLocalRotationY()
    {
        Bone* parent = _parent;
        if (!parent)
        {
            return _arotation;
        }
        
        float pa = parent->_a;
        float pb = parent->_b;
        float pc = parent->_c;
        float pd = parent->_d;
        float b = _b;
        float d = _d;
        
        return MathUtil::atan2(pa * d - pc * b, pd * b - pb * d) * RadDeg;
    }
    
    BoneData& Bone::getData()
    {
        return _data;
    }
    
    Skeleton& Bone::getSkeleton()
    {
        return _skeleton;
    }
    
    Bone* Bone::getParent()
    {
        return _parent;
    }
    
    SimpleArray<Bone*>& Bone::getChildren()
    {
        return _children;
    }
    
    float Bone::getX()
    {
        return _x;
    }
    
    void Bone::setX(float inValue)
    {
        _x = inValue;
    }
    
    float Bone::getY()
    {
        return _y;
    }
    
    void Bone::setY(float inValue)
    {
        _y = inValue;
    }
    
    float Bone::getRotation()
    {
        return _rotation;
    }
    
    void Bone::setRotation(float inValue)
    {
        _rotation = inValue;
    }
    
    float Bone::getScaleX()
    {
        return _scaleX;
    }
    
    void Bone::setScaleX(float inValue)
    {
        _scaleX = inValue;
    }
    
    float Bone::getScaleY()
    {
        return _scaleY;
    }
    
    void Bone::setScaleY(float inValue)
    {
        _scaleY = inValue;
    }
    
    float Bone::getShearX()
    {
        return _shearX;
    }
    
    void Bone::setShearX(float inValue)
    {
        _shearX = inValue;
    }
    
    float Bone::getShearY()
    {
        return _shearY;
    }
    
    void Bone::setShearY(float inValue)
    {
        _shearY = inValue;
    }
    
    float Bone::getAppliedRotation()
    {
        return _arotation;
    }
    
    void Bone::setAppliedRotation(float inValue)
    {
        _arotation = inValue;
    }
    
    float Bone::getAX()
    {
        return _ax;
    }
    
    void Bone::setAX(float inValue)
    {
        _ax = inValue;
    }
    
    float Bone::getAY()
    {
        return _ay;
    }
    
    void Bone::setAY(float inValue)
    {
        _ay = inValue;
    }
    
    float Bone::getAScaleX()
    {
        return _ascaleX;
    }
    
    void Bone::setAScaleX(float inValue)
    {
        _ascaleX = inValue;
    }
    
    float Bone::getAScaleY()
    {
        return _ascaleY;
    }
    
    void Bone::setAScaleY(float inValue)
    {
        _ascaleY = inValue;
    }
    
    float Bone::getAShearX()
    {
        return _ashearX;
    }
    
    void Bone::setAShearX(float inValue)
    {
        _ashearX = inValue;
    }
    
    float Bone::getAShearY()
    {
        return _ashearY;
    }
    
    void Bone::setAShearY(float inValue)
    {
        _ashearY = inValue;
    }
    
    float Bone::getA()
    {
        return _a;
    }
    
    float Bone::getB()
    {
        return _b;
    }
    
    float Bone::getC()
    {
        return _c;
    }
    
    float Bone::getD()
    {
        return _d;
    }
    
    float Bone::getWorldX()
    {
        return _worldX;
    }
    
    float Bone::getWorldY()
    {
        return _worldY;
    }
    
    float Bone::getWorldRotationX()
    {
        return MathUtil::atan2(_c, _a) * RadDeg;
    }
    
    float Bone::getWorldRotationY()
    {
        return MathUtil::atan2(_d, _b) * RadDeg;
    }
    
    float Bone::getWorldScaleX()
    {
        return sqrt(_a * _a + _c * _c);
    }
    
    float Bone::getWorldScaleY()
    {
        return sqrt(_b * _b + _d * _d);
    }
    
    void Bone::updateAppliedTransform()
    {
        _appliedValid = true;
        Bone* parent = _parent;
        if (!parent)
        {
            _ax = _worldX;
            _ay = _worldY;
            _arotation = MathUtil::atan2(_c, _a) * RadDeg;
            _ascaleX = sqrt(_a * _a + _c * _c);
            _ascaleY = sqrt(_b * _b + _d * _d);
            _ashearX = 0;
            _ashearY = MathUtil::atan2(_a * _b + _c * _d, _a * _d - _b * _c) * RadDeg;
            
            return;
        }
        
        float pa = parent->_a;
        float pb = parent->_b;
        float pc = parent->_c;
        float pd = parent->_d;
        
        float pid = 1 / (pa * pd - pb * pc);
        float dx = _worldX - parent->_worldX;
        float dy = _worldY - parent->_worldY;
        
        _ax = (dx * pd * pid - dy * pb * pid);
        _ay = (dy * pa * pid - dx * pc * pid);
        
        float ia = pid * pd;
        float id = pid * pa;
        float ib = pid * pb;
        float ic = pid * pc;
        
        float ra = ia * _a - ib * _c;
        float rb = ia * _b - ib * _d;
        float rc = id * _c - ic * _a;
        float rd = id * _d - ic * _b;
        
        _ashearX = 0;
        _ascaleX = sqrt(ra * ra + rc * rc);
        
        if (_ascaleX > 0.0001f)
        {
            float det = ra * rd - rb * rc;
            _ascaleY = det / _ascaleX;
            _ashearY = MathUtil::atan2(ra * rb + rc * rd, det) * RadDeg;
            _arotation = MathUtil::atan2(rc, ra) * RadDeg;
        }
        else
        {
            _ascaleX = 0;
            _ascaleY = sqrt(rb * rb + rd * rd);
            _ashearY = 0;
            _arotation = 90 - MathUtil::atan2(rd, rb) * RadDeg;
        }
    }
}
