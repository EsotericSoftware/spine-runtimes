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

#ifndef Spine_Bone_h
#define Spine_Bone_h

#include <spine/Updatable.h>

namespace Spine
{
    /// Stores a bone's current pose.
    ///
    /// A bone has a local transform which is used to compute its world transform. A bone also has an applied transform, which is a
    /// local transform that can be applied to compute the world transform. The local transform and applied transform may differ if a
    /// constraint or application code modifies the world transform after it was computed from the local transform.
    ///
    class Bone : public Updatable
    {
        friend class RotateTimeline;
        
    public:
    private:
        static public bool yDown;
        
        internal BoneData _data;
        internal Skeleton _skeleton;
        internal Bone _parent;
        internal ExposedList<Bone> _children = new ExposedList<Bone>();
        internal float _x, _y, _rotation, _scaleX, _scaleY, _shearX, _shearY;
        internal float _ax, _ay, _arotation, _ascaleX, _ascaleY, _ashearX, _ashearY;
        internal bool _appliedValid;
        
        internal float _a, _b, _worldX;
        internal float _c, _d, _worldY;
        
        //        internal float worldSignX, worldSignY;
        //        public float WorldSignX { get { return worldSignX; } }
        //        public float WorldSignY { get { return worldSignY; } }
        
        internal bool _sorted;
        
        public BoneData Data { get { return data; } }
        public Skeleton Skeleton { get { return skeleton; } }
        public Bone Parent { get { return parent; } }
        public ExposedList<Bone> Children { get { return children; } }
        /// The local X translation.
        public float X { get { return x; } set { x = value; } }
        /// The local Y translation.
        public float Y { get { return y; } set { y = value; } }
        /// The local rotation.
        public float Rotation { get { return rotation; } set { rotation = value; } }
        
        /// The local scaleX.
        public float ScaleX { get { return scaleX; } set { scaleX = value; } }
        
        /// The local scaleY.
        public float ScaleY { get { return scaleY; } set { scaleY = value; } }
        
        /// The local shearX.
        public float ShearX { get { return shearX; } set { shearX = value; } }
        
        /// The local shearY.
        public float ShearY { get { return shearY; } set { shearY = value; } }
        
        /// The rotation, as calculated by any constraints.
        public float AppliedRotation { get { return arotation; } set { arotation = value; } }
        
        /// The applied local x translation.
        public float AX { get { return ax; } set { ax = value; } }
        
        /// The applied local y translation.
        public float AY { get { return ay; } set { ay = value; } }
        
        /// The applied local scaleX.
        public float AScaleX { get { return ascaleX; } set { ascaleX = value; } }
        
        /// The applied local scaleY.
        public float AScaleY { get { return ascaleY; } set { ascaleY = value; } }
        
        /// The applied local shearX.
        public float AShearX { get { return ashearX; } set { ashearX = value; } }
        
        /// The applied local shearY.
        public float AShearY { get { return ashearY; } set { ashearY = value; } }
        
        public float A { get { return a; } }
        public float B { get { return b; } }
        public float C { get { return c; } }
        public float D { get { return d; } }
        
        public float WorldX { get { return worldX; } }
        public float WorldY { get { return worldY; } }
        public float WorldRotationX { get { return MathUtils.Atan2(c, a) * MathUtils.RadDeg; } }
        public float WorldRotationY { get { return MathUtils.Atan2(d, b) * MathUtils.RadDeg; } }
        
        /// Returns the magnitide (always positive) of the world scale X.
        public float WorldScaleX { get { return (float)Math.Sqrt(a * a + c * c); } }
        /// Returns the magnitide (always positive) of the world scale Y.
        public float WorldScaleY { get { return (float)Math.Sqrt(b * b + d * d); } }
        
        /// @param parent May be null.
        public Bone (BoneData data, Skeleton skeleton, Bone parent) {
            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
            if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
            this.data = data;
            this.skeleton = skeleton;
            this.parent = parent;
            SetToSetupPose();
        }
        
        /// Same as <see cref="UpdateWorldTransform"/>. This method exists for Bone to implement <see cref="Spine.IUpdatable"/>.
        public void Update () {
            UpdateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
        }
        
        /// Computes the world transform using the parent bone and this bone's local transform.
        public void UpdateWorldTransform () {
            UpdateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
        }
        
        /// Computes the world transform using the parent bone and the specified local transform.
        public void UpdateWorldTransform (float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY) {
            ax = x;
            ay = y;
            arotation = rotation;
            ascaleX = scaleX;
            ascaleY = scaleY;
            ashearX = shearX;
            ashearY = shearY;
            appliedValid = true;
            Skeleton skeleton = this.skeleton;
            
            Bone parent = this.parent;
            if (parent == null) { // Root bone.
                float rotationY = rotation + 90 + shearY;
                float la = MathUtils.CosDeg(rotation + shearX) * scaleX;
                float lb = MathUtils.CosDeg(rotationY) * scaleY;
                float lc = MathUtils.SinDeg(rotation + shearX) * scaleX;
                float ld = MathUtils.SinDeg(rotationY) * scaleY;
                if (skeleton.flipX) {
                    x = -x;
                    la = -la;
                    lb = -lb;
                }
                if (skeleton.flipY != yDown) {
                    y = -y;
                    lc = -lc;
                    ld = -ld;
                }
                a = la;
                b = lb;
                c = lc;
                d = ld;
                worldX = x + skeleton.x;
                worldY = y + skeleton.y;
                //                worldSignX = Math.Sign(scaleX);
                //                worldSignY = Math.Sign(scaleY);
                return;
            }
            
            float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
            worldX = pa * x + pb * y + parent.worldX;
            worldY = pc * x + pd * y + parent.worldY;
            //            worldSignX = parent.worldSignX * Math.Sign(scaleX);
            //            worldSignY = parent.worldSignY * Math.Sign(scaleY);
            
            switch (data.transformMode) {
                case TransformMode.Normal: {
                    float rotationY = rotation + 90 + shearY;
                    float la = MathUtils.CosDeg(rotation + shearX) * scaleX;
                    float lb = MathUtils.CosDeg(rotationY) * scaleY;
                    float lc = MathUtils.SinDeg(rotation + shearX) * scaleX;
                    float ld = MathUtils.SinDeg(rotationY) * scaleY;
                    a = pa * la + pb * lc;
                    b = pa * lb + pb * ld;
                    c = pc * la + pd * lc;
                    d = pc * lb + pd * ld;
                    return;
                }
                case TransformMode.OnlyTranslation: {
                    float rotationY = rotation + 90 + shearY;
                    a = MathUtils.CosDeg(rotation + shearX) * scaleX;
                    b = MathUtils.CosDeg(rotationY) * scaleY;
                    c = MathUtils.SinDeg(rotation + shearX) * scaleX;
                    d = MathUtils.SinDeg(rotationY) * scaleY;
                    break;
                }
                case TransformMode.NoRotationOrReflection: {
                    float s = pa * pa + pc * pc, prx;
                    if (s > 0.0001f) {
                        s = Math.Abs(pa * pd - pb * pc) / s;
                        pb = pc * s;
                        pd = pa * s;
                        prx = MathUtils.Atan2(pc, pa) * MathUtils.RadDeg;
                    } else {
                        pa = 0;
                        pc = 0;
                        prx = 90 - MathUtils.Atan2(pd, pb) * MathUtils.RadDeg;
                    }
                    float rx = rotation + shearX - prx;
                    float ry = rotation + shearY - prx + 90;
                    float la = MathUtils.CosDeg(rx) * scaleX;
                    float lb = MathUtils.CosDeg(ry) * scaleY;
                    float lc = MathUtils.SinDeg(rx) * scaleX;
                    float ld = MathUtils.SinDeg(ry) * scaleY;
                    a = pa * la - pb * lc;
                    b = pa * lb - pb * ld;
                    c = pc * la + pd * lc;
                    d = pc * lb + pd * ld;
                    break;
                }
                case TransformMode.NoScale:
                case TransformMode.NoScaleOrReflection: {
                    float cos = MathUtils.CosDeg(rotation), sin = MathUtils.SinDeg(rotation);
                    float za = pa * cos + pb * sin;
                    float zc = pc * cos + pd * sin;
                    float s = (float)Math.Sqrt(za * za + zc * zc);
                    if (s > 0.00001f) s = 1 / s;
                    za *= s;
                    zc *= s;
                    s = (float)Math.Sqrt(za * za + zc * zc);
                    float r = MathUtils.PI / 2 + MathUtils.Atan2(zc, za);
                    float zb = MathUtils.Cos(r) * s;
                    float zd = MathUtils.Sin(r) * s;
                    float la = MathUtils.CosDeg(shearX) * scaleX;
                    float lb = MathUtils.CosDeg(90 + shearY) * scaleY;
                    float lc = MathUtils.SinDeg(shearX) * scaleX;
                    float ld = MathUtils.SinDeg(90 + shearY) * scaleY;
                    if (data.transformMode != TransformMode.NoScaleOrReflection? pa * pd - pb* pc< 0 : skeleton.flipX != skeleton.flipY) {
                        zb = -zb;
                        zd = -zd;
                    }
                    a = za * la + zb * lc;
                    b = za * lb + zb * ld;
                    c = zc * la + zd * lc;
                    d = zc * lb + zd * ld;
                    return;
                }
            }
            
            if (skeleton.flipX) {
                a = -a;
                b = -b;
            }
            if (skeleton.flipY != Bone.yDown) {
                c = -c;
                d = -d;
            }
        }
        
        public void SetToSetupPose () {
            BoneData data = this.data;
            x = data.x;
            y = data.y;
            rotation = data.rotation;
            scaleX = data.scaleX;
            scaleY = data.scaleY;
            shearX = data.shearX;
            shearY = data.shearY;
        }
        
        /// 
        /// Computes the individual applied transform values from the world transform. This can be useful to perform processing using
        /// the applied transform after the world transform has been modified directly (eg, by a constraint)..
        ///
        /// Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation.
        /// 
        internal void UpdateAppliedTransform () {
            appliedValid = true;
            Bone parent = this.parent;
            if (parent == null) {
                ax = worldX;
                ay = worldY;
                arotation = MathUtils.Atan2(c, a) * MathUtils.RadDeg;
                ascaleX = (float)Math.Sqrt(a * a + c * c);
                ascaleY = (float)Math.Sqrt(b * b + d * d);
                ashearX = 0;
                ashearY = MathUtils.Atan2(a * b + c * d, a * d - b * c) * MathUtils.RadDeg;
                return;
            }
            float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
            float pid = 1 / (pa * pd - pb * pc);
            float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
            ax = (dx * pd * pid - dy * pb * pid);
            ay = (dy * pa * pid - dx * pc * pid);
            float ia = pid * pd;
            float id = pid * pa;
            float ib = pid * pb;
            float ic = pid * pc;
            float ra = ia * a - ib * c;
            float rb = ia * b - ib * d;
            float rc = id * c - ic * a;
            float rd = id * d - ic * b;
            ashearX = 0;
            ascaleX = (float)Math.Sqrt(ra * ra + rc * rc);
            if (ascaleX > 0.0001f) {
                float det = ra * rd - rb * rc;
                ascaleY = det / ascaleX;
                ashearY = MathUtils.Atan2(ra * rb + rc * rd, det) * MathUtils.RadDeg;
                arotation = MathUtils.Atan2(rc, ra) * MathUtils.RadDeg;
            } else {
                ascaleX = 0;
                ascaleY = (float)Math.Sqrt(rb * rb + rd * rd);
                ashearY = 0;
                arotation = 90 - MathUtils.Atan2(rd, rb) * MathUtils.RadDeg;
            }
        }
        
        public void WorldToLocal (float worldX, float worldY, out float localX, out float localY) {
            float a = this.a, b = this.b, c = this.c, d = this.d;
            float invDet = 1 / (a * d - b * c);
            float x = worldX - this.worldX, y = worldY - this.worldY;
            localX = (x * d * invDet - y * b * invDet);
            localY = (y * a * invDet - x * c * invDet);
        }
        
        public void LocalToWorld (float localX, float localY, out float worldX, out float worldY) {
            worldX = localX * a + localY * b + this.worldX;
            worldY = localX * c + localY * d + this.worldY;
        }
        
        public float WorldToLocalRotationX {
            get {
                Bone parent = this.parent;
                if (parent == null) return arotation;
                float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, a = this.a, c = this.c;
                return MathUtils.Atan2(pa * c - pc * a, pd * a - pb * c) * MathUtils.RadDeg;
            }
        }
        
        public float WorldToLocalRotationY {
            get {
                Bone parent = this.parent;
                if (parent == null) return arotation;
                float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, b = this.b, d = this.d;
                return MathUtils.Atan2(pa * d - pc * b, pd * b - pb * d) * MathUtils.RadDeg;
            }
        }
        
        public float WorldToLocalRotation (float worldRotation) {
            float sin = MathUtils.SinDeg(worldRotation), cos = MathUtils.CosDeg(worldRotation);
            return MathUtils.Atan2(a * sin - c * cos, d * cos - b * sin) * MathUtils.RadDeg;
        }
        
        public float LocalToWorldRotation (float localRotation) {
            float sin = MathUtils.SinDeg(localRotation), cos = MathUtils.CosDeg(localRotation);
            return MathUtils.Atan2(cos * c + sin * d, cos * a + sin * b) * MathUtils.RadDeg;
        }
        
        /// 
        /// Rotates the world transform the specified amount and sets isAppliedValid to false.
        /// 
        /// @param degrees Degrees.
        public void RotateWorld (float degrees)
        {
            float a = this.a, b = this.b, c = this.c, d = this.d;
            float cos = MathUtils.CosDeg(degrees), sin = MathUtils.SinDeg(degrees);
            this.a = cos * a - sin * c;
            this.b = cos * b - sin * d;
            this.c = sin * a + cos * c;
            this.d = sin * b + cos * d;
            appliedValid = false;
        }
        
        friend std::ostream& operator <<(std::ostream& os, const Bone& ref);
    };
}

#endif /* Spine_Bone_h */
