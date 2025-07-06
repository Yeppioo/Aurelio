using System.Runtime.InteropServices;

namespace MinecraftSkinRender.OpenGL;

public partial class SkinRenderOpenGL
{
    private readonly ModelVAO _normalVAO = new();
    private readonly ModelVAO _topVAO = new();

    private int _steveModelDrawOrderCount;

    private void InitModel()
    {
        InitVAO(_normalVAO);
        InitVAO(_topVAO);
    }

    private void DeleteModel()
    {
        DeleteVAO(_normalVAO);
        DeleteVAO(_topVAO);
    }

    private void InitVAOItem(VAOItem item)
    {
        item.VertexBufferObject = gl.GenBuffer();
        item.IndexBufferObject = gl.GenBuffer();
    }

    private void InitVAO(ModelVAO vao)
    {
        vao.Head.VertexArrayObject = gl.GenVertexArray();
        vao.Body.VertexArrayObject = gl.GenVertexArray();
        vao.LeftArm.VertexArrayObject = gl.GenVertexArray();
        vao.RightArm.VertexArrayObject = gl.GenVertexArray();
        vao.LeftLeg.VertexArrayObject = gl.GenVertexArray();
        vao.RightLeg.VertexArrayObject = gl.GenVertexArray();
        vao.Cape.VertexArrayObject = gl.GenVertexArray();

        InitVAOItem(vao.Head);
        InitVAOItem(vao.Body);
        InitVAOItem(vao.LeftArm);
        InitVAOItem(vao.RightArm);
        InitVAOItem(vao.LeftLeg);
        InitVAOItem(vao.RightLeg);
        InitVAOItem(vao.Cape);
    }

    private unsafe void PutVAO(VAOItem vao, CubeModelItemObj model, float[] uv)
    {
        gl.UseProgram(_pgModel);
        gl.BindVertexArray(vao.VertexArrayObject);

        int a_Position = gl.GetAttribLocation(_pgModel, "a_position");
        int a_texCoord = gl.GetAttribLocation(_pgModel, "a_texCoord");
        int a_normal = gl.GetAttribLocation(_pgModel, "a_normal");

        gl.DisableVertexAttribArray(a_Position);
        gl.DisableVertexAttribArray(a_texCoord);
        gl.DisableVertexAttribArray(a_normal);

        int size = model.Model.Length / 3;

        var points = new VertexOpenGL[size];

        for (var primitive = 0; primitive < size; primitive++)
        {
            var srci = primitive * 3;
            var srci1 = primitive * 2;
            points[primitive] = new VertexOpenGL
            {
                Position = new(model.Model[srci], model.Model[srci + 1], model.Model[srci + 2]),
                UV = new(uv[srci1], uv[srci1 + 1]),
                Normal = new(CubeModel.Vertices[srci], CubeModel.Vertices[srci + 1], CubeModel.Vertices[srci + 2])
            };
        }

        gl.BindBuffer(gl.GL_ARRAY_BUFFER, vao.VertexBufferObject);
        var vertexSize = Marshal.SizeOf<VertexOpenGL>();
        fixed (void* pdata = points)
        {
            gl.BufferData(gl.GL_ARRAY_BUFFER, points.Length * vertexSize,
                    new IntPtr(pdata), gl.GL_STATIC_DRAW);
        }

        gl.BindBuffer(gl.GL_ELEMENT_ARRAY_BUFFER, vao.IndexBufferObject);
        fixed (void* pdata = model.Point)
        {
            gl.BufferData(gl.GL_ELEMENT_ARRAY_BUFFER,
                model.Point.Length * sizeof(ushort), new IntPtr(pdata), gl.GL_STATIC_DRAW);
        }

        gl.VertexAttribPointer(a_Position, 3, gl.GL_FLOAT, false, 8 * sizeof(float), 0);
        gl.VertexAttribPointer(a_texCoord, 2, gl.GL_FLOAT, false, 8 * sizeof(float), 3 * sizeof(float));
        gl.VertexAttribPointer(a_normal, 3, gl.GL_FLOAT, false, 8 * sizeof(float), 5 * sizeof(float));

        gl.EnableVertexAttribArray(a_Position);
        gl.EnableVertexAttribArray(a_texCoord);
        gl.EnableVertexAttribArray(a_normal);

        gl.BindVertexArray(0);
    }

    private unsafe void LoadModel()
    {
        var normal = Steve3DModel.GetSteve(_skinType);
        var top = Steve3DModel.GetSteveTop(_skinType);
        var tex = Steve3DTexture.GetSteveTexture(_skinType);
        var textop = Steve3DTexture.GetSteveTextureTop(_skinType);

        _steveModelDrawOrderCount = normal.Head.Point.Length;

        PutVAO(_normalVAO.Head, normal.Head, tex.Head);
        PutVAO(_normalVAO.Body, normal.Body, tex.Body);
        PutVAO(_normalVAO.LeftArm, normal.LeftArm, tex.LeftArm);
        PutVAO(_normalVAO.RightArm, normal.RightArm, tex.RightArm);
        PutVAO(_normalVAO.LeftLeg, normal.LeftLeg, tex.LeftLeg);
        PutVAO(_normalVAO.RightLeg, normal.RightLeg, tex.RightLeg);
        PutVAO(_normalVAO.Cape, normal.Cape, tex.Cape);

        PutVAO(_topVAO.Head, top.Head, textop.Head);
        if (_skinType != SkinType.Old)
        {
            PutVAO(_topVAO.Head, top.Head, textop.Head);
            PutVAO(_topVAO.Body, top.Body, textop.Body);
            PutVAO(_topVAO.LeftArm, top.LeftArm, textop.LeftArm);
            PutVAO(_topVAO.RightArm, top.RightArm, textop.RightArm);
            PutVAO(_topVAO.LeftLeg, top.LeftLeg, textop.LeftLeg);
            PutVAO(_topVAO.RightLeg, top.RightLeg, textop.RightLeg);
        }
        _switchModel = false;
    }

    private void DeleteVAOItem(VAOItem item)
    {
        gl.DeleteBuffer(item.VertexBufferObject);
        gl.DeleteBuffer(item.IndexBufferObject);
    }

    private void DeleteVAO(ModelVAO vao)
    {
        gl.DeleteVertexArray(vao.Head.VertexArrayObject);
        gl.DeleteVertexArray(vao.Body.VertexArrayObject);
        gl.DeleteVertexArray(vao.LeftArm.VertexArrayObject);
        gl.DeleteVertexArray(vao.RightArm.VertexArrayObject);
        gl.DeleteVertexArray(vao.LeftLeg.VertexArrayObject);
        gl.DeleteVertexArray(vao.RightLeg.VertexArrayObject);

        DeleteVAOItem(vao.Head);
        DeleteVAOItem(vao.Body);
        DeleteVAOItem(vao.LeftArm);
        DeleteVAOItem(vao.RightArm);
        DeleteVAOItem(vao.LeftLeg);
        DeleteVAOItem(vao.RightLeg);
    }
}
