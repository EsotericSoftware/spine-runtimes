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

#include <spine/IkConstraint.h>

#include <spine/IkConstraintData.h>
#include <spine/Skeleton.h>
#include <spine/Bone.h>

#include <spine/BoneData.h>
#include <spine/MathUtil.h>

namespace Spine {
    RTTI_IMPL(IkConstraint, Constraint);
    
    void IkConstraint::apply(Bone& bone, float targetX, float targetY, float alpha) {
        if (!bone._appliedValid) {
            bone.updateAppliedTransform();
        }
        
        Bone* parent = bone.getParent();
        Bone& p = *parent;
        
        float id = 1 / (p._a * p._d - p._b * p._c);
        float x = targetX - p._worldX, y = targetY - p._worldY;
        float tx = (x * p._d - y * p._b) * id - bone._ax, ty = (y * p._a - x * p._c) * id - bone._ay;
        float rotationIK = atan2(ty, tx) * RadDeg - bone._ashearX - bone._arotation;
        
        if (bone._ascaleX < 0) {
            rotationIK += 180;
        }
        
        if (rotationIK > 180) {
            rotationIK -= 360;
        }
        else if (rotationIK < -180) {
            rotationIK += 360;
        }
        
        bone.updateWorldTransform(bone._ax, bone._ay, bone._arotation + rotationIK * alpha, bone._ascaleX, bone._ascaleY, bone._ashearX, bone._ashearY);
    }
    
    void IkConstraint::apply(Bone& parent, Bone& child, float targetX, float targetY, int bendDir, float alpha) {
        if (areFloatsPracticallyEqual(alpha, 0)) {
            child.updateWorldTransform();
            
            return;
        }
        
        if (!parent._appliedValid) {
            parent.updateAppliedTransform();
        }
        
        if (!child._appliedValid) {
            child.updateAppliedTransform();
        }
        
        float px = parent._ax;
        float py = parent._ay;
        float psx = parent._ascaleX;
        float psy = parent._ascaleY;
        float csx = child._ascaleX;
        
        int os1, os2, s2;
        if (psx < 0) {
            psx = -psx;
            os1 = 180;
            s2 = -1;
        }
        else {
            os1 = 0;
            s2 = 1;
        }
        
        if (psy < 0) {
            psy = -psy;
            s2 = -s2;
        }
        
        if (csx < 0) {
            csx = -csx;
            os2 = 180;
        }
        else {
            os2 = 0;
        }
        
        float cx = child._ax;
        float cy;
        float cwx;
        float cwy;
        float a = parent._a;
        float b = parent._b;
        float c = parent._c;
        float d = parent._d;
        
        bool u = fabs(psx - psy) <= 0.0001f;
        if (!u) {
            cy = 0;
            cwx = a * cx + parent._worldX;
            cwy = c * cx + parent._worldY;
        }
        else {
            cy = child._ay;
            cwx = a * cx + b * cy + parent._worldX;
            cwy = c * cx + d * cy + parent._worldY;
        }
        
        Bone* parentparent = parent._parent;
        Bone& pp = *parentparent;
        
        a = pp._a;
        b = pp._b;
        c = pp._c;
        d = pp._d;
        
        float id = 1 / (a * d - b * c), x = targetX - pp._worldX, y = targetY - pp._worldY;
        float tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
        x = cwx - pp._worldX;
        y = cwy - pp._worldY;
        float dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
        float l1 = sqrt(dx * dx + dy * dy), l2 = child._data.getLength() * csx, a1, a2;
        if (u) {
            l2 *= psx;
            float cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
            if (cos < -1) {
                cos = -1;
            }
            else if (cos > 1) {
                cos = 1;
            }
            
            a2 = acos(cos) * bendDir;
            a = l1 + l2 * cos;
            b = l2 * sin(a2);
            a1 = atan2(ty * a - tx * b, tx * a + ty * b);
        }
        else {
            a = psx * l2;
            b = psy * l2;
            float aa = a * a, bb = b * b, dd = tx * tx + ty * ty, ta = atan2(ty, tx);
            c = bb * l1 * l1 + aa * dd - aa * bb;
            float c1 = -2 * bb * l1, c2 = bb - aa;
            d = c1 * c1 - 4 * c2 * c;
            if (d >= 0) {
                float q = sqrt(d);
                if (c1 < 0) q = -q;
                q = -(c1 + q) / 2;
                float r0 = q / c2, r1 = c / q;
                float r = fabs(r0) < fabs(r1) ? r0 : r1;
                if (r * r <= dd) {
                    y = sqrt(dd - r * r) * bendDir;
                    a1 = ta - atan2(y, r);
                    a2 = atan2(y / psy, (r - l1) / psx);
                    
                    float os = atan2(cy, cx) * s2;
                    float rotation = parent._arotation;
                    a1 = (a1 - os) * RadDeg + os1 - rotation;
                    if (a1 > 180) {
                        a1 -= 360;
                    }
                    else if (a1 < -180) {
                        a1 += 360;
                    }
                    
                    parent.updateWorldTransform(px, py, rotation + a1 * alpha, parent._scaleX, parent._ascaleY, 0, 0);
                    rotation = child._arotation;
                    a2 = ((a2 + os) * RadDeg - child._ashearX) * s2 + os2 - rotation;
                    
                    if (a2 > 180) {
                        a2 -= 360;
                    }
                    else if (a2 < -180) {
                        a2 += 360;
                    }
                    
                    child.updateWorldTransform(cx, cy, rotation + a2 * alpha, child._ascaleX, child._ascaleY, child._ashearX, child._ashearY);
                    
                    return;
                }
            }
            
            float minAngle = SPINE_PI, minX = l1 - a, minDist = minX * minX, minY = 0;
            float maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
            c = -a * l1 / (aa - bb);
            if (c >= -1 && c <= 1) {
                c = acos(c);
                x = a * cos(c) + l1;
                y = b * (float)sin(c);
                d = x * x + y * y;
                
                if (d < minDist) {
                    minAngle = c;
                    minDist = d;
                    minX = x;
                    minY = y;
                }
                
                if (d > maxDist) {
                    maxAngle = c;
                    maxDist = d;
                    maxX = x;
                    maxY = y;
                }
            }
            
            if (dd <= (minDist + maxDist) / 2) {
                a1 = ta - atan2(minY * bendDir, minX);
                a2 = minAngle * bendDir;
            }
            else {
                a1 = ta - atan2(maxY * bendDir, maxX);
                a2 = maxAngle * bendDir;
            }
        }
    }
    
    IkConstraint::IkConstraint(IkConstraintData& data, Skeleton& skeleton) : Constraint(),
    _data(data),
    _mix(data.getMix()),
    _bendDirection(data.getBendDirection()),
    _target(skeleton.findBone(data.getTarget()->getName())) {
        _bones.ensureCapacity(_data.getBones().size());
        for (BoneData** i = _data.getBones().begin(); i != _data.getBones().end(); ++i) {
            BoneData* boneData = (*i);

            _bones.add(skeleton.findBone(boneData->getName()));
        }
    }
    
    /// Applies the constraint to the constrained bones.
    void IkConstraint::apply() {
        update();
    }
    
    void IkConstraint::update() {
        switch (_bones.size()) {
            case 1: {
                Bone* bone0 = _bones[0];
                apply(*bone0, _target->getWorldX(), _target->getWorldY(), _mix);
            }
                break;
            case 2: {
                Bone* bone0 = _bones[0];
                Bone* bone1 = _bones[1];
                apply(*bone0, *bone1, _target->getWorldX(), _target->getWorldY(), _bendDirection, _mix);
            }
                break;
        }
    }
    
    int IkConstraint::getOrder() {
        return _data.getOrder();
    }
    
    IkConstraintData& IkConstraint::getData() {
        return _data;
    }
    
    Vector<Bone*>& IkConstraint::getBones() {
        return _bones;
    }
    
    Bone* IkConstraint::getTarget() {
        return _target;
    }
    
    void IkConstraint::setTarget(Bone* inValue) {
        _target = inValue;
    }
    
    int IkConstraint::getBendDirection() {
        return _bendDirection;
    }
    
    void IkConstraint::setBendDirection(int inValue) {
        _bendDirection = inValue;
    }
    
    float IkConstraint::getMix() {
        return _mix;
    }
    
    void IkConstraint::setMix(float inValue) {
        _mix = inValue;
    }
}
