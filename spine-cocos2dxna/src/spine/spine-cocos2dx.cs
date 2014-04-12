using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
using Spine;
namespace SpineCocosXna.Spine.Data.Cocos
{
    class spine_cocos2dx
    {

        public static CCTextureAtlas CreateAtlasFromTexture2D(Microsoft.Xna.Framework.Graphics.Texture2D texture2d)
        {
            CCTexture2D texture = new CCTexture2D();
            texture.InitWithTexture(texture2d);
            
            CCTextureAtlas textureAtlas = CCTextureAtlas.Create(texture, 128);
            return textureAtlas;
        }

        public static void AtlasPageCreateTexture(ref AtlasPage self, string path)
        {
            CCTexture2D texture = CCTextureCache.SharedTextureCache.AddImage(path);
            CCTextureAtlas textureAtlas = CCTextureAtlas.Create(texture, 128);
            //textureAtlas. ->retain();
            self.rendererObject = textureAtlas;
            self.width = texture.PixelsWide;
            self.height = texture.PixelsHigh;
        }

        string spUtilReadFile(string path)
        {
            return CCFileUtils.GetFileData(path);
        }

        void spRegionAttachment_updateQuad(ref RegionAttachment self, ref Slot slot, CCV3F_C4B_T2F_Quad quad, bool premultipliedAlpha)
        {

            float[] vertices = new float[8];

            self.ComputeWorldVertices(slot.Skeleton.X, slot.Skeleton.Y, slot.Bone, vertices);

            float r = (slot.Skeleton.R * slot.R * 255);
            float g = (slot.Skeleton.G * slot.G * 255);
            float b = (slot.Skeleton.B * slot.B * 255);

            float normalizedAlpha = slot.Skeleton.A * slot.A;
            if (premultipliedAlpha)
            {

                r *= normalizedAlpha;
                g *= normalizedAlpha;
                b *= normalizedAlpha;
            }

            float a = normalizedAlpha * 255;

            quad.BottomLeft.Colors.R = Convert.ToByte(r);
            quad.BottomLeft.Colors.G = Convert.ToByte(g);
            quad.BottomLeft.Colors.B = Convert.ToByte(b);
            quad.BottomLeft.Colors.A = Convert.ToByte(a);
            quad.TopLeft.Colors.R = Convert.ToByte(r);
            quad.TopLeft.Colors.G = Convert.ToByte(g);
            quad.TopLeft.Colors.B = Convert.ToByte(b);
            quad.TopLeft.Colors.A = Convert.ToByte(a);
            quad.TopRight.Colors.R = Convert.ToByte(r);
            quad.TopRight.Colors.G = Convert.ToByte(g);
            quad.TopRight.Colors.B = Convert.ToByte(b);
            quad.TopRight.Colors.A = Convert.ToByte(a);
            quad.BottomRight.Colors.R = Convert.ToByte(r);
            quad.BottomRight.Colors.G = Convert.ToByte(g);
            quad.BottomRight.Colors.B = Convert.ToByte(b);
            quad.BottomRight.Colors.A = Convert.ToByte(a);

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
    }
}
