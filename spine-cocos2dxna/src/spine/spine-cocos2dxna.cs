using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
using Spine;

namespace SpineCocosXna.Spine
{

     //VERTEX_X1 = 0, VERTEX_Y1, VERTEX_X2, VERTEX_Y2, VERTEX_X3, VERTEX_Y3, VERTEX_X4, VERTEX_Y4
    public class spine_cocos2dxna
    {

        public static int VERTEX_X1 = 0;
        public static int VERTEX_Y1 = 1;
        public static int VERTEX_X2 = 2;
        public static int VERTEX_Y2 = 3;
        public static int VERTEX_X3 = 4;
        public static int VERTEX_Y3 = 5;
        public static int VERTEX_X4 = 6;
        public static int VERTEX_Y4 = 7;

        void spRegionAttachment_updateQuad(RegionAttachment self, Slot slot, CCV3F_C4B_T2F_Quad quad, bool premultipliedAlpha = false)
        {

            float[] vertices = new float[8];

            self.ComputeWorldVertices(slot.Skeleton.X, slot.Skeleton.Y, slot.Bone, vertices);

            byte r = Convert.ToByte(slot.Skeleton.R * slot.R * 255);
            byte g =Convert.ToByte( slot.Skeleton.G * slot.G * 255);
            byte b = Convert.ToByte(slot.Skeleton.B * slot.B * 255);

            byte normalizedAlpha = Convert.ToByte( slot.Skeleton.A * slot.A);
            if (premultipliedAlpha)
            {
                r *= normalizedAlpha;
                g *= normalizedAlpha;
                b *= normalizedAlpha;
            }

            byte a = Convert.ToByte(normalizedAlpha * 255);
            quad.BottomLeft.Colors.R = r;
            quad.BottomLeft.Colors.G = g;
            quad.BottomLeft.Colors.B = b;
            quad.BottomLeft.Colors.A = a;
            quad.TopLeft.Colors.R = r;
            quad.TopLeft.Colors.G = g;
            quad.TopLeft.Colors.B = b;
            quad.TopLeft.Colors.A = a;
            quad.TopRight.Colors.R = r;
            quad.TopRight.Colors.G = g;
            quad.TopRight.Colors.B = b;
            quad.TopRight.Colors.A = a;
            quad.BottomRight.Colors.R = r;
            quad.BottomRight.Colors.G = g;
            quad.BottomRight.Colors.B = b;
            quad.BottomRight.Colors.A = a;

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

        ////CCFileUtils.GetFileData(path); --> Lectura de fichero


    }
}
