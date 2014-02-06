using System;
using Spine;


public class MeshAttachment : Attachment
{
    public MeshAttachment(string name) : base(name)
    {
    }

    private float[] vertices;
    private int[] triangles;
    private float[] worldVertices;

    // Nonessential.
    private int[] edges;
    private float width, height;
    private int hullLength;
    internal float u = 0, v = 0, u2 = 1, v2 = 1;

    public float Width
    {
        get { return width; }
        set { width = value; }
    }

    public float Height
    {
        get { return height; }
        set { height = value; }
    }

    public float[] Vertices
    {
        get { return vertices; }
    }

    public int[] Triangles
    {
        get { return triangles; }
    }

    public float[] WorldVertices
    {
        get { return worldVertices; }
    }

    public Object RendererObject { get; set; }


    public int HullLength
    {
        get { return hullLength; }
        set { hullLength = value; }
    }

    public int[] Edges
    {
        get { return edges; }
        set { edges = value; }
    }

    public void ComputeWorldVertices(float x, float y, Bone bone, float[] _vertices)
    {
        x += bone.worldX;
        y += bone.worldY;
        float m00 = bone.m00;
        float m01 = bone.m01;
        float m10 = bone.m10;
        float m11 = bone.m11;
        float[] worldVertices = this.worldVertices;
        float[] vertices = _vertices != null && _vertices.Length > 0 ? _vertices : this.vertices;
        for (int v = 0, w = 0, n = worldVertices.Length; w < n; v += 2, w += 4)
        {
            float vx = vertices[v];
            float vy = vertices[v + 1];
            worldVertices[w] = vx*m00 + vy*m01 + x;
            worldVertices[w + 1] = vx*m10 + vy*m11 + y;
        }
    }

    public void SetUVs(float u, float v, float u2, float v2)
    {
        this.u = u;
        this.v = v;
        this.u2 = u2;
        this.v2 = v2;
    }

    public void SetMesh(float[] vertices, int[] triangles, float[] uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;

        int worldVerticesLength = vertices.Length/2*4;
        if (worldVertices == null || worldVertices.Length != worldVerticesLength)
            worldVertices = new float[worldVerticesLength];

        float w = u2 - u;
        float h = v2 - v;

        for (int i = 0, ii = 2, n = vertices.Length; i < n; i += 2, ii += 4)
        {
            worldVertices[ii] = u + uvs[i]*w;
            worldVertices[ii + 1] = v + uvs[i + 1]*h;
        }
    }
}

