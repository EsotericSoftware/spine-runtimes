/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using Cocos2D;
using Spine;
using SpineCocosXna.Spine.Data.Cocos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpineCocosXna.Spine
{

    public enum spAttachmentType
    {
        ATTACHMENT_REGION = 1, ATTACHMENT_REGION_SEQUENCE = 2, ATTACHMENT_BOUNDING_BOX = 3
    }

    public class CCSkeleton : CCNodeRGBA
    {

        public float FLT_MAX = 3.402823466e+38F;     /* max value */
        public float FLT_MIN = 1.175494351e-38F;     /* min positive value */

        public static int ATTACHMENT_REGION = 0;
        public static int ATTACHMENT_REGION_SEQUENCE = 1;
        public static int ATTACHMENT_BOUNDING_BOX = 2;

        public CCBlendFunc blendFunc = new CCBlendFunc();

        public Skeleton skeleton;
        public Bone rootBone;

        float timeScale;
        bool debugSlots = true;
        bool debugBones;
        bool premultipliedAlpha;

        bool ownsSkeletonData;
        Atlas atlas;

        static CCSkeleton createWithData(SkeletonData skeletonData, bool ownsSkeletonData = false)
        {
            CCSkeleton node = new CCSkeleton(skeletonData, ownsSkeletonData);
            return node;
        }

        static CCSkeleton createWithFile(string skeletonDataFile, Atlas atlas, float scale = 0)
        {
            CCSkeleton node = new CCSkeleton(skeletonDataFile, atlas, scale);
            return node;
        }

        static CCSkeleton createWithFile(string skeletonDataFile, string atlasFile, float scale = 0)
        {
            CCSkeleton node = new CCSkeleton(skeletonDataFile, atlasFile, scale);
            return node;
        }

        public void initialize()
        {
            atlas = null;
            debugSlots = false;
            debugBones = false;
            timeScale = 1;

            blendFunc = new CCBlendFunc(CCOGLES.GL_ONE, CCOGLES.GL_ONE_MINUS_SRC_ALPHA);
            CCDrawManager.BlendFunc(blendFunc);

            setOpacityModifyRGB(true);
             
            //SetShaderProgram(CCShaderCache::sharedShaderCache()->programForKey(kCCShader_PositionTextureColor));

            ScheduleUpdate();
        }

        public void setSkeletonData(SkeletonData skeletonData, bool ownsSkeletonData)
        {
            skeleton = new Skeleton(skeletonData);
            rootBone = skeleton.Bones[0];
            this.ownsSkeletonData = ownsSkeletonData;
        }

        public CCSkeleton()
        {
            initialize();
        }

        public CCSkeleton(SkeletonData skeletonData, bool ownsSkeletonData = false)
        {
            initialize();
            setSkeletonData(skeletonData, ownsSkeletonData);
        }

        public CCSkeleton(string skeletonDataFile, Atlas atlas, float scale = 0)
        {
            var json = new SkeletonJson(atlas);
            json.Scale = scale == 0 ? (1 / CCDirector.SharedDirector.ContentScaleFactor) : scale;
            SkeletonData skeletonData = json.ReadSkeletonData(skeletonDataFile);
            setSkeletonData(skeletonData, true);
        }

        public CCSkeleton(string skeletonDataFile, string atlasFile, float scale = 0)
        {

            initialize();

            atlas = new Atlas(atlasFile, new MonoTextureLoader(CCDrawManager.GraphicsDevice));

            SkeletonJson json = new SkeletonJson(atlas);

            json.Scale = scale == 0 ? (1 / CCDirector.SharedDirector.ContentScaleFactor) : scale;

            SkeletonData skeletonData = json.ReadSkeletonData(skeletonDataFile);
            setSkeletonData(skeletonData, true);
        }

        ~CCSkeleton()
        {
            
            //Eliminamos el skleñetp
            //if (ownsSkeletonData!=null) 

            //SkeletonData_dispose(skeleton->data);
            //if (atlas) spAtlas_dispose(atlas);
            //spSkeleton_dispose(skeleton);
            
        }
      
        public override void Update(float dt)
        {
            base.Update(dt);
            skeleton.Update(dt*timeScale);
        }

        public override void Draw()
        {
            base.Draw();

            // CC_NODE_DRAW_SETUP();

            CCDrawManager.BlendFunc(blendFunc);
            CCColor3B color = Color;
            skeleton.R = color.R / 255f ; 
            skeleton.G = color.G / 255f; 
            skeleton.B = color.B / 255f; 
            skeleton.A = Opacity / 255f;

            bool additive = false;

            CCTextureAtlas textureAtlas = null;

            CCV3F_C4B_T2F_Quad quad = new CCV3F_C4B_T2F_Quad();

            quad.TopLeft.Vertices.Z = 0;
            quad.TopRight.Vertices.Z = 0;
            quad.BottomLeft.Vertices.Z = 0;
            quad.BottomRight.Vertices.Z = 0;

            for (int i = 0; i < skeleton.Slots.Count; i++)
            {
                Slot slot = skeleton.DrawOrder[i];

                if (slot.Attachment == null || slot.Attachment.GetType().ToString() != "Spine.RegionAttachment") continue;  //=> ??????????????


                RegionAttachment attachment = (RegionAttachment)slot.Attachment;
                CCTextureAtlas regionTextureAtlas = GetTextureAtlas(attachment);
                regionTextureAtlas.DrawQuads(); //retain();
                
                //self.rendererObject = textureAtlas;
                //self.width = texture.PixelsWide;
                //self.height = texture.PixelsHigh;

                if (slot.Data.AdditiveBlending != additive)
                {
                    if (textureAtlas != null)
                    {
                        textureAtlas.DrawQuads();
                        textureAtlas.RemoveAllQuads();
                    }

                    additive = !additive;

                    CCDrawManager.BlendFunc(new CCBlendFunc(blendFunc.Source, (additive) ? CCOGLES.GL_ONE : blendFunc.Destination) );

                }
                else if (textureAtlas != null && regionTextureAtlas != textureAtlas)
                {
                    textureAtlas.DrawQuads();
                    textureAtlas.RemoveAllQuads();
                }

                textureAtlas = regionTextureAtlas;

                int quadCount = textureAtlas.TotalQuads;
                if (textureAtlas.Capacity == quadCount)
                {
                    textureAtlas.DrawQuads();
                    textureAtlas.RemoveAllQuads();

                    if (!textureAtlas.ResizeCapacity(textureAtlas.Capacity * 2)) return;
                }
                
                spRegionAttachment_updateQuad(attachment, slot, ref quad, premultipliedAlpha);

                textureAtlas.UpdateQuad(ref quad, quadCount);

            }
          
            if (textureAtlas != null)
            {
                textureAtlas.DrawQuads();
                textureAtlas.RemoveAllQuads();
            }

           
            if (debugSlots)
            {

                CCPoint[] points = new CCPoint[4];

                CCV3F_C4B_T2F_Quad quada = new CCV3F_C4B_T2F_Quad();

                for (int i = 0, n = skeleton.Slots.Count; i < n; i++)
                {

                    Slot slot = skeleton.DrawOrder[i];

                    if (slot.Attachment == null || slot.Attachment.Name != "ATTACHMENT_REGION") continue;

                    RegionAttachment attachment = (RegionAttachment)slot.Attachment;
                    spRegionAttachment_updateQuad(attachment, slot, ref quad);

                    points[0] = new CCPoint(quada.BottomLeft.Vertices.X, quada.BottomLeft.Vertices.Y);
                    points[1] = new CCPoint(quada.BottomRight.Vertices.X, quada.BottomRight.Vertices.Y);
                    points[2] = new CCPoint(quada.TopRight.Vertices.X, quada.TopRight.Vertices.Y);
                    points[3] = new CCPoint(quada.TopLeft.Vertices.X, quada.TopLeft.Vertices.Y);

                    CCDrawingPrimitives.DrawPoly(points, 4, true, new CCColor4B(0, 0, 255, 255));
                }
            }

            if (debugBones)
            {
                // Bone lengths.
                for (int i = 0; i < skeleton.Bones.Count; i++)
                {
                    Bone bone = skeleton.Bones[i];
                    float x = bone.Data.Length * bone.M00 + bone.WorldX;
                    float y = bone.Data.Length * bone.M10 + bone.WorldY;

                    CCDrawingPrimitives.Begin();
                    CCDrawingPrimitives.DrawLine(new CCPoint(bone.WorldX, bone.WorldY), new CCPoint(x, y), new CCColor4B(255, 0, 0, 255));
                    CCDrawingPrimitives.End();
                }

                // Bone origins.
                for (int i = 0; i < skeleton.Bones.Count; i++)
                {
                    Bone bone = skeleton.Bones[i];
                    CCDrawingPrimitives.Begin();
                    CCDrawingPrimitives.DrawPoint(new CCPoint(bone.WorldX, bone.WorldY), 4, new CCColor4B(0, 0, 255, 255));
                    CCDrawingPrimitives.End();

                }
            }

        }


        public CCTextureAtlas GetTextureAtlas(RegionAttachment attachment)
        {
            AtlasRegion regionAtlas = (AtlasRegion)attachment.RendererObject;
            var texture2D = (Microsoft.Xna.Framework.Graphics.Texture2D)regionAtlas.page.rendererObject;
            //Console.WriteLine(texture2D.IsDisposed);
           
            return spine_cocos2dx.CreateAtlasFromTexture2D(texture2D);
        }

        public CCRect boundingBox()
        {
            float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;


            float[] vertices = new float[8];


            for (int i = 0; i < skeleton.Slots.Count; ++i)
            {

                Slot slot = skeleton.Slots[i];

                if (slot.Attachment != null || slot.Attachment.Name != "ATTACHMENT_REGION") continue;

                RegionAttachment attachment = (RegionAttachment)slot.Attachment;

                attachment.ComputeWorldVertices(slot.Skeleton.X, slot.Skeleton.Y, slot.Bone, vertices);

                minX = Math.Min(minX, vertices[RegionAttachment.X1] * ScaleX);
                minY = Math.Min(minY, vertices[RegionAttachment.Y1] * ScaleY);
                maxX = Math.Max(maxX, vertices[RegionAttachment.X1] * ScaleX);
                maxY = Math.Max(maxY, vertices[RegionAttachment.Y1] * ScaleY);
                minX = Math.Min(minX, vertices[RegionAttachment.X4] * ScaleX);
                minY = Math.Min(minY, vertices[RegionAttachment.Y4] * ScaleY);
                maxX = Math.Max(maxX, vertices[RegionAttachment.X4] * ScaleX);
                maxY = Math.Max(maxY, vertices[RegionAttachment.Y4] * ScaleY);
                minX = Math.Min(minX, vertices[RegionAttachment.X2] * ScaleX);
                minY = Math.Min(minY, vertices[RegionAttachment.Y2] * ScaleY);
                maxX = Math.Max(maxX, vertices[RegionAttachment.X2] * ScaleX);
                maxY = Math.Max(maxY, vertices[RegionAttachment.Y2] * ScaleY);
                minX = Math.Min(minX, vertices[RegionAttachment.X3] * ScaleX);
                minY = Math.Min(minY, vertices[RegionAttachment.Y3] * ScaleY);
                maxX = Math.Max(maxX, vertices[RegionAttachment.X3] * ScaleX);
                maxY = Math.Max(maxY, vertices[RegionAttachment.Y3] * ScaleY);
            }
            CCPoint position = Position;

            return new CCRect(position.X + minX, position.Y + minY, maxX - minX, maxY - minY);

        }

        // --- Convenience methods for common Skeleton_* functions.
        public void updateWorldTransform()
        {
            skeleton.UpdateWorldTransform();
        }

        public void setToSetupPose()
        {
            skeleton.SetToSetupPose();
        }
        public void setBonesToSetupPose()
        {
            skeleton.SetBonesToSetupPose();
        }
        public void setSlotsToSetupPose()
        {
            skeleton.SetSlotsToSetupPose();
        }

        /* Returns 0 if the bone was not found. */
        public Bone findBone(string boneName)
        {
            return skeleton.FindBone(boneName);
        }
        /* Returns 0 if the slot was not found. */
        public Slot findSlot(string slotName)
        {
            return skeleton.FindSlot(slotName);
        }

        /* Sets the skin used to look up attachments not found in the SkeletonData defaultSkin. Attachments from the new skin are
         * attached if the corresponding attachment from the old skin was attached. Returns false if the skin was not found.
         * @param skin May be 0.*/
        public bool setSkin(string skinName)
        {
            skeleton.SetSkin(skinName);
            return true;
        }

        /* Returns 0 if the slot or attachment was not found. */
        public Attachment getAttachment(string slotName, string attachmentName)
        {
            return skeleton.GetAttachment(slotName, attachmentName);
        }
        /* Returns false if the slot or attachment was not found. */
        public bool setAttachment(string slotName, string attachmentName)
        {
            skeleton.SetAttachment(slotName, attachmentName);
            return true;
        }

        // --- CCBlendProtocol
        //CC_PROPERTY(cocos2d::ccBlendFunc, blendFunc, BlendFunc);
        public void setOpacityModifyRGB(bool value)
        {
            premultipliedAlpha = value;
        }

        public bool isOpacityModifyRGB()
        {
            return premultipliedAlpha;
        }
      
        //virtual cocos2d::CCTextureAtlas* getTextureAtlas (RegionAttachment regionAttachment);
        #region SpinesCocos2d

        void spRegionAttachment_updateQuad(RegionAttachment self, Slot slot, ref CCV3F_C4B_T2F_Quad quad, bool premultipliedAlpha = false)
        {

            float[] vertices = new float[8];

            self.ComputeWorldVertices(slot.Skeleton.X, slot.Skeleton.Y, slot.Bone, vertices);

            float r = slot.Skeleton.R * slot.R * 255;
            float g = slot.Skeleton.G * slot.G * 255;
            float b = slot.Skeleton.B * slot.B * 255;

            float normalizedAlpha = slot.Skeleton.A * slot.A;
            if (premultipliedAlpha)
            {
                r *= normalizedAlpha;
                g *= normalizedAlpha;
                b *= normalizedAlpha;
            }

            float a = normalizedAlpha * 255;
            quad.BottomLeft.Colors.R = (byte)r;
            quad.BottomLeft.Colors.G = (byte)g;
            quad.BottomLeft.Colors.B = (byte)b;
            quad.BottomLeft.Colors.A = (byte)a;
            quad.TopLeft.Colors.R = (byte)r;
            quad.TopLeft.Colors.G = (byte)g;
            quad.TopLeft.Colors.B = (byte)b;
            quad.TopLeft.Colors.A = (byte)a;
            quad.TopRight.Colors.R = (byte)r;
            quad.TopRight.Colors.G = (byte)g;
            quad.TopRight.Colors.B = (byte)b;
            quad.TopRight.Colors.A = (byte)a;
            quad.BottomRight.Colors.R = (byte)r;
            quad.BottomRight.Colors.G = (byte)g;
            quad.BottomRight.Colors.B = (byte)b;
            quad.BottomRight.Colors.A = (byte)a;

            quad.BottomLeft.Vertices.X = vertices[RegionAttachment.X1];
            quad.BottomLeft.Vertices.Y = vertices[RegionAttachment.Y1];
            quad.TopLeft.Vertices.X = vertices[RegionAttachment.X2];
            quad.TopLeft.Vertices.Y = vertices[RegionAttachment.Y2];
            quad.TopRight.Vertices.X = vertices[RegionAttachment.X3];
            quad.TopRight.Vertices.Y = vertices[RegionAttachment.Y3];
            quad.BottomRight.Vertices.X = vertices[RegionAttachment.X4];
            quad.BottomRight.Vertices.Y = vertices[RegionAttachment.Y4];

            quad.BottomLeft.TexCoords.U = self.UVs[RegionAttachment.X1];
            quad.BottomLeft.TexCoords.V = self.UVs[RegionAttachment.Y1];
            quad.TopLeft.TexCoords.U = self.UVs[RegionAttachment.X2];
            quad.TopLeft.TexCoords.V = self.UVs[RegionAttachment.Y2];
            quad.TopRight.TexCoords.U = self.UVs[RegionAttachment.X3];
            quad.TopRight.TexCoords.V = self.UVs[RegionAttachment.Y3];
            quad.BottomRight.TexCoords.U = self.UVs[RegionAttachment.X4];
            quad.BottomRight.TexCoords.V = self.UVs[RegionAttachment.Y4];

        }

        void _spAtlasPage_createTexture(AtlasPage self, string path)
        {

            CCTexture2D texture = CCTextureCache.SharedTextureCache.AddImage(path);
            CCTextureAtlas textureAtlas = new CCTextureAtlas();
            textureAtlas.InitWithTexture(texture, 128);

            textureAtlas.DrawQuads(); //retain();

            self.rendererObject = textureAtlas;
            self.width = texture.PixelsWide;
            self.height = texture.PixelsHigh;
        }

        void _spAtlasPage_disposeTexture(AtlasPage self)
        {
            ((CCTextureAtlas)self.rendererObject).RemoveAllQuads();//Release(); // =============> ?
        }

        #endregion

     

    }
}
