using Cocos2D;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
using Spine;
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


    //    typedef enum {
    //    ATTACHMENT_REGION, ATTACHMENT_REGION_SEQUENCE, ATTACHMENT_BOUNDING_BOX
    //}

    public class CCSkeleton : CCNodeRGBA//, CCBlendProtocol
    {

        public float FLT_MAX = 3.402823466e+38F;       /* max value */
        public float FLT_MIN = 1.175494351e-38F;     /* min positive value */

        public static int ATTACHMENT_REGION = 0;
        public static int ATTACHMENT_REGION_SEQUENCE = 1;
        public static int ATTACHMENT_BOUNDING_BOX = 2;

        public Skeleton skeleton;
        public Bone rootBone;

        float timeScale;
        bool debugSlots = true;
        bool debugBones;
        bool premultipliedAlpha;

        ~CCSkeleton()
        {
            //Eliminamos el skleñetp
            //if (ownsSkeletonData!=null) //SkeletonData_dispose(skeleton->data);
            //if (atlas) spAtlas_dispose(atlas);
            //spSkeleton_dispose(skeleton);

        }

        static CCSkeleton createWithData(SkeletonData skeletonData, bool ownsSkeletonData = false)
        {
            CCSkeleton node = new CCSkeleton(skeletonData, ownsSkeletonData);
            //node. //Autorelease();
            node.initialize();

            return node;
        }

        static CCSkeleton createWithFile(string skeletonDataFile, Atlas atlas, float scale = 0)
        {
            CCSkeleton node = new CCSkeleton(skeletonDataFile, atlas, scale);
            node.initialize();
            return node;
        }

        static CCSkeleton createWithFile(string skeletonDataFile, string atlasFile, float scale = 0)
        {
            CCSkeleton node = new CCSkeleton(skeletonDataFile, atlasFile, scale);
            node.initialize();
            return node;
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
            initialize();
            var json = new SkeletonJson(atlas);
            json.Scale = scale == 0 ? (1 / CCDirector.SharedDirector.ContentScaleFactor) : scale;
            SkeletonData skeletonData = json.ReadSkeletonData(skeletonDataFile);
            setSkeletonData(skeletonData, true);

            //Console.WriteLine("Error reading skeleton data.");
            //Console.WriteLine(string.Format(skeletonData,json. ? json->error : "Error reading skeleton data.");
            //CCAssert(skeletonData, );
            //spSkeletonJson_dispose(json);

        }

        public CCSkeleton(string skeletonDataFile, string atlasFile, float scale = 0)
        {

            initialize();
            //GraphicsDevice elemento = CCDrawManager.GraphicsDevice;

            //DrawManager.graphicsDevice
            //new XnaTextureLoader(elemento));


            //var graphicsDeviceManager = GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
            //if (graphicsDeviceManager != null)
            //{
            //    graphicsDeviceManager.CreateDevice();
            //}
            //this.Initialize();

            //CCDirector.SharedDirector.
            //CCApplication.SharedApplication.GraphicsDevice
            var tmp = CCApplication.SharedApplication.GraphicsDevice;

            var nuevo = new GraphicsDevice(tmp.Adapter, tmp.GraphicsProfile, tmp.PresentationParameters);

            atlas = new Atlas(atlasFile, new XnaTextureLoader(nuevo));

            SkeletonJson json = new SkeletonJson(atlas);
            json.Scale = scale == 0 ? (1 / CCDirector.SharedDirector.ContentScaleFactor) : scale;
            SkeletonData skeletonData = json.ReadSkeletonData(skeletonDataFile);
            setSkeletonData(skeletonData, true);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            skeleton.Update(dt);

        }

        public override void Draw()
        {
            base.Draw();

            CCBlendFunc blendFunc = new CCBlendFunc();

            // CC_NODE_DRAW_SETUP();

            CCBlendFunc blen = new CCBlendFunc(blendFunc.Source, blendFunc.Destination);

            //ccGLBlendFunc(blendFunc.Source, blendFunc.Destination);
            CCColor3B color = Color;
            skeleton.R = color.R; // / (float)255;
            skeleton.G = color.G; /// (float)255;
            skeleton.B = color.B; // / (float)255;
            skeleton.A = Opacity;// / (float)255;

            bool additive = false;

            CCTextureAtlas textureAtlas = null;
            //ccV3F_C4B_T2F_Quad quad;

            CCV3F_C4B_T2F_Quad quad = new CCV3F_C4B_T2F_Quad();

            quad.TopLeft.Vertices.Z = 0;
            quad.TopRight.Vertices.Z = 0;
            quad.BottomLeft.Vertices.Z = 0;
            quad.BottomRight.Vertices.Z = 0;

            //skeleton.Slots.Count

            for (int i = 0, n = skeleton.Slots.Count; i < n; i++)
            {
                Slot slot = skeleton.DrawOrder[i];

                //AQUI

                if (slot.Attachment==null || slot.Attachment.GetType().ToString() != "Spine.RegionAttachment") continue;  //=> ??????????????

                RegionAttachment attachment = (RegionAttachment)slot.Attachment;
                CCTextureAtlas regionTextureAtlas = getTextureAtlas(attachment);


                if (slot.Data.AdditiveBlending != additive)
                {
                    if (textureAtlas != null)
                    {
                        textureAtlas.DrawQuads();
                        textureAtlas.RemoveAllQuads();
                    }

                    additive = !additive;

                    CCDrawManager.BlendFunc(blendFunc);

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

                

                spRegionAttachment_updateQuad(attachment, slot,ref quad, premultipliedAlpha);

                textureAtlas.UpdateQuad(ref quad, quadCount);

                //textureAtlas.RemoveAllQuads();
            }

            if (textureAtlas != null)
            {
                textureAtlas.DrawQuads();
                textureAtlas.RemoveAllQuads();
            }

            if (debugSlots)
            {
                // Slots.
                //CCDrawManager.dr
                //CCDrawingPrimitives.

                CCColor4B clr = new CCColor4B(0, 0, 255, 255);
                //ccDrawColor4B(0, 0, 255, 255);
                //glLineWidth(1); -->
                CCPoint[] points = new CCPoint[4];

                CCV3F_C4B_T2F_Quad quada = new CCV3F_C4B_T2F_Quad();
                //ccV3F_C4B_T2F_Quad quad;

                for (int i = 0, n =1; i < n; i++)
                {
                    Slot slot = skeleton.DrawOrder[i];
                    if (slot.Attachment!=null || slot.Attachment.Name != "ATTACHMENT_REGION") continue;
                    
                    RegionAttachment attachment = (RegionAttachment)slot.Attachment;
                    
                    spRegionAttachment_updateQuad(attachment, slot,ref quad);
                    //attachment.
                    points[0] = new CCPoint(quada.BottomLeft.Vertices.X, quada.BottomLeft.Vertices.Y);
                    points[1] = new CCPoint(quada.BottomRight.Vertices.X, quada.BottomRight.Vertices.Y);
                    points[2] = new CCPoint(quada.TopRight.Vertices.X, quada.TopRight.Vertices.Y);
                    points[3] = new CCPoint(quada.TopLeft.Vertices.X, quada.TopLeft.Vertices.Y);
                    
                    CCDrawingPrimitives.DrawPoly(points, 4, true, clr);
                    //ccDrawPoly(points, 4, true);
                }
            }

            if (debugBones)
            {
                // Bone lengths.

                //glLineWidth(2);
                // ccDrawColor4B(255, 0, 0, 255);

                CCColor4B clr = new CCColor4B(255, 0, 0, 255);
                
                for (int i = 0, n = skeleton.Bones.Count; i < n; i++)
                {
                    Bone bone = skeleton.Bones[i];
                    float x = bone.Data.Length * bone.M00 + bone.WorldX;
                    float y = bone.Data.Length * bone.M10 + bone.WorldY;
                    CCDrawingPrimitives.Begin();
                    CCDrawingPrimitives.DrawLine(new CCPoint(bone.WorldX, bone.WorldY), new CCPoint(x, y), clr);
                    CCDrawingPrimitives.End();
                    //ccDrawLine(ccp(bone.worldX, bone->worldY), ccp(x, y));
                }
                
                // Bone origins.
                //ccPointSize(4);
                clr = new CCColor4B(0, 0, 255, 255);
                //ccDrawColor4B(0, 0, 255, 255); // Root bone is blue.
                for (int i = 0, n = skeleton.Bones.Count; i < n; i++)
                {
                    Bone bone = skeleton.Bones[i];
                    CCDrawingPrimitives.Begin();
                    CCDrawingPrimitives.DrawPoint(new CCPoint(bone.WorldX, bone.WorldY));
                    CCDrawingPrimitives.End();
                    //ccDrawPoint(ccp(bone->worldX, bone->worldY));
                   // if (i == 0) ccDrawColor4B(0, 255, 0, 255); //==================???
               
                }
            }

        }
        
    
        CCTextureAtlas getTextureAtlas (RegionAttachment regionAttachment) {

            var imageRendered =(Texture2D) ((AtlasRegion)regionAttachment.RendererObject).page.rendererObject;

            var Image2d = new CCTexture2D();
            Image2d.InitWithTexture(imageRendered);
      
            var textAtlas = new CCTextureAtlas();
            textAtlas.InitWithTexture(Image2d, 10);
            
            return textAtlas;

        }

        public CCRect boundingBox()
        {
            float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	
    float[] vertices = new float[8];

	for (int i = 0; i < skeleton.Slots.Count; ++i) {
		Slot slot = skeleton.Slots[i];

        if (slot.Attachment!=null || slot.Attachment.Name != "ATTACHMENT_REGION") continue;
		
        RegionAttachment attachment = (RegionAttachment)slot.Attachment;

        attachment.ComputeWorldVertices(slot.Skeleton.X, slot.Skeleton.Y, slot.Bone, vertices);
		
        minX = Math.Min(minX, vertices[VERTEX_X1] * ScaleX);
		minY = Math.Min(minY, vertices[VERTEX_Y1] * ScaleY);
		maxX = Math.Max(maxX, vertices[VERTEX_X1] * ScaleX);
		maxY = Math.Max(maxY, vertices[VERTEX_Y1] * ScaleY);
		minX = Math.Min(minX, vertices[VERTEX_X4] * ScaleX);
		minY = Math.Min(minY, vertices[VERTEX_Y4] * ScaleY);
		maxX = Math.Max(maxX, vertices[VERTEX_X4] * ScaleX);
		maxY = Math.Max(maxY, vertices[VERTEX_Y4] * ScaleY);
		minX = Math.Min(minX, vertices[VERTEX_X2] * ScaleX);
		minY = Math.Min(minY, vertices[VERTEX_Y2] * ScaleY);
		maxX = Math.Max(maxX, vertices[VERTEX_X2] * ScaleX);
		maxY = Math.Max(maxY, vertices[VERTEX_Y2] * ScaleY);
		minX = Math.Min(minX, vertices[VERTEX_X3] * ScaleX);
		minY = Math.Min(minY, vertices[VERTEX_Y3] * ScaleY);
		maxX = Math.Max(maxX, vertices[VERTEX_X3] * ScaleX);
		maxY = Math.Max(maxY, vertices[VERTEX_Y3] * ScaleY);
	}
	CCPoint position = Position;

    return new CCRect(position.X + minX, position.Y + minY, maxX - minX, maxY - minY);
	//return CCRectMake(position.x + minX, position.y + minY, maxX - minX, maxY - minY);

        }

        // --- Convenience methods for common Skeleton_* functions.
        public void updateWorldTransform()
        {
            skeleton.UpdateWorldTransform();
            //spSkeleton_updateWorldTransform(skeleton);
        }

        public void setToSetupPose()
        {
            skeleton.SetToSetupPose();
            //spSkeleton_setToSetupPose(skeleton);
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

        public void setSkeletonData(SkeletonData skeletonData, bool ownsSkeletonData)
        {
            skeleton = new Skeleton(skeletonData);
            rootBone = skeleton.Bones[0];
            this.ownsSkeletonData = ownsSkeletonData;
        }
        //virtual cocos2d::CCTextureAtlas* getTextureAtlas (RegionAttachment regionAttachment);

        bool ownsSkeletonData;
        Atlas atlas;

        public void initialize()
        {
            atlas = null;
            debugSlots = true;
            debugBones = true;
            timeScale = 1;


            //blendFunc.src = GL_ONE;
            //blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
            setOpacityModifyRGB(true);


            //SetShaderProgram(CCShaderCache::sharedShaderCache()->programForKey(kCCShader_PositionTextureColor));
            ScheduleUpdate();
        }

        #region SpinesCocos2d
      
        public static int VERTEX_X1 = 0;
        public static int VERTEX_Y1 = 1;
        public static int VERTEX_X2 = 2;
        public static int VERTEX_Y2 = 3;
        public static int VERTEX_X3 = 4;
        public static int VERTEX_Y3 = 5;
        public static int VERTEX_X4 = 6;
        public static int VERTEX_Y4 = 7;

        void spRegionAttachment_updateQuad(RegionAttachment self, Slot slot,ref CCV3F_C4B_T2F_Quad quad, bool premultipliedAlpha = false)
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
            quad.BottomLeft.Colors.R =(byte) r;
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

            quad.BottomLeft.Vertices.X = vertices[VERTEX_X1];
            quad.BottomLeft.Vertices.Y = vertices[VERTEX_Y1];
            quad.TopLeft.Vertices.X = vertices[VERTEX_X2];
            quad.TopLeft.Vertices.Y = vertices[VERTEX_Y2];
            quad.TopRight.Vertices.X = vertices[VERTEX_X3];
            quad.TopRight.Vertices.Y = vertices[VERTEX_Y3];
            quad.BottomRight.Vertices.X = vertices[VERTEX_X4];
            quad.BottomRight.Vertices.Y = vertices[VERTEX_Y4];

            quad.BottomLeft.TexCoords.U = self.UVs[VERTEX_X1];
            quad.BottomLeft.TexCoords.V = self.UVs[VERTEX_Y1];
            quad.TopLeft.TexCoords.U = self.UVs[VERTEX_X2];
            quad.TopLeft.TexCoords.V = self.UVs[VERTEX_Y2];
            quad.TopRight.TexCoords.U = self.UVs[VERTEX_X3];
            quad.TopRight.TexCoords.V = self.UVs[VERTEX_Y3];
            quad.BottomRight.TexCoords.U = self.UVs[VERTEX_X4];
            quad.BottomRight.TexCoords.V = self.UVs[VERTEX_Y4];

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
