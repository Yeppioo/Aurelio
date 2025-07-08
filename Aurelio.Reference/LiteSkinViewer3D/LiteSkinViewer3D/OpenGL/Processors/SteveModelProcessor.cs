using LiteSkinViewer3D.Shared;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Models;
using System.Runtime.InteropServices;

namespace LiteSkinViewer3D.OpenGL.Processors;

internal sealed class SteveModelProcessor : IDisposable {
    private readonly OpenGLApi gl;
    private readonly int shaderProgram;

    public SteveModelBindings TopVAO { get; } = new();
    public SteveModelBindings NormalVAO { get; } = new();
    public int DrawIndexCount { get; private set; }

    public SteveModelProcessor(OpenGLApi gl, int shaderProgram) {
        this.gl = gl;
        this.shaderProgram = shaderProgram;
    }

    public void Initialize(SkinType type) {
        InitVAO(NormalVAO);
        InitVAO(TopVAO);
        Load(type);
    }

    private void InitVAOItem(MeshBinding item) {
        item.VertexBufferObject = gl.GenBuffer();
        item.IndexBufferObject = gl.GenBuffer();
        item.VertexArrayObject = gl.GenVertexArray();
    }

    private void InitVAO(SteveModelBindings smb) {
        InitVAOItem(smb.Head);
        InitVAOItem(smb.Body);
        InitVAOItem(smb.LeftArm);
        InitVAOItem(smb.RightArm);
        InitVAOItem(smb.LeftLeg);
        InitVAOItem(smb.RightLeg);
        InitVAOItem(smb.Cape);
    }

    public unsafe void Load(SkinType type) {
        var model = SteveModelFactory.CreateBaseModel(type);
        var top = SteveModelFactory.CreateOverlayModel(type);
        var tex = SteveTextureBuilder.GetSteveTexture(type);
        var textop = SteveTextureBuilder.GetSteveTextureTop(type);

        DrawIndexCount = model.Head.Indices.Length;

        PutVAO(NormalVAO.Head, model.Head, tex.Head);
        PutVAO(NormalVAO.Body, model.Body, tex.Body);
        PutVAO(NormalVAO.LeftArm, model.LeftArm, tex.LeftArm);
        PutVAO(NormalVAO.RightArm, model.RightArm, tex.RightArm);
        PutVAO(NormalVAO.LeftLeg, model.LeftLeg, tex.LeftLeg);
        PutVAO(NormalVAO.RightLeg, model.RightLeg, tex.RightLeg);
        PutVAO(NormalVAO.Cape, model.Cape!, tex.Cape!);

        PutVAO(TopVAO.Head, top.Head, textop.Head);
        if (type != SkinType.Legacy) {
            PutVAO(TopVAO.Body, top.Body, textop.Body);
            PutVAO(TopVAO.LeftArm, top.LeftArm, textop.LeftArm);
            PutVAO(TopVAO.RightArm, top.RightArm, textop.RightArm);
            PutVAO(TopVAO.LeftLeg, top.LeftLeg, textop.LeftLeg);
            PutVAO(TopVAO.RightLeg, top.RightLeg, textop.RightLeg);
        }
    }

    private unsafe void PutVAO(MeshBinding mb, CubeItemModel model, float[] uv) {
        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(mb.VertexArrayObject);

        int posLoc = gl.GetAttribLocation(shaderProgram, "a_position");
        int texLoc = gl.GetAttribLocation(shaderProgram, "a_texCoord");
        int normLoc = gl.GetAttribLocation(shaderProgram, "a_normal");

        int vertexCount = model.Vertices.Length / 3;
        var vertices = new VertexDataGL[vertexCount];

        for (int i = 0; i < vertexCount; i++) {
            int mi = i * 3, ui = i * 2;
            vertices[i] = new VertexDataGL {
                UV = new(uv[ui], uv[ui + 1]),
                Position = new(model.Vertices[mi], model.Vertices[mi + 1], model.Vertices[mi + 2]),
                Normal = new(Cube.Vertices[mi], Cube.Vertices[mi + 1], Cube.Vertices[mi + 2])
            };
        }

        gl.BindBuffer(gl.GL_ARRAY_BUFFER, mb.VertexBufferObject);
        fixed (void* ptr = vertices) {
            gl.BufferData(gl.GL_ARRAY_BUFFER, vertexCount * Marshal.SizeOf<VertexDataGL>(), new IntPtr(ptr), gl.GL_STATIC_DRAW);
        }

        gl.BindBuffer(gl.GL_ELEMENT_ARRAY_BUFFER, mb.IndexBufferObject);
        fixed (void* iptr = model.Indices) {
            gl.BufferData(gl.GL_ELEMENT_ARRAY_BUFFER, model.Indices.Length * sizeof(ushort), new IntPtr(iptr), gl.GL_STATIC_DRAW);
        }

        gl.VertexAttribPointer(posLoc, 3, gl.GL_FLOAT, false, 8 * sizeof(float), 0);
        gl.VertexAttribPointer(texLoc, 2, gl.GL_FLOAT, false, 8 * sizeof(float), 3 * sizeof(float));
        gl.VertexAttribPointer(normLoc, 3, gl.GL_FLOAT, false, 8 * sizeof(float), 5 * sizeof(float));

        gl.EnableVertexAttribArray(posLoc);
        gl.EnableVertexAttribArray(texLoc);
        gl.EnableVertexAttribArray(normLoc);

        gl.BindVertexArray(0);
    }

    public void Dispose() {
        DeleteVAO(NormalVAO);
        DeleteVAO(TopVAO);
    }

    private void DeleteVAO(SteveModelBindings smb) {
        DeleteVAOItem(smb.Head);
        DeleteVAOItem(smb.Body);
        DeleteVAOItem(smb.LeftArm);
        DeleteVAOItem(smb.RightArm);
        DeleteVAOItem(smb.LeftLeg);
        DeleteVAOItem(smb.RightLeg);
        DeleteVAOItem(smb.Cape);
    }

    private void DeleteVAOItem(MeshBinding mb) {
        if (mb.VertexBufferObject != 0) gl.DeleteBuffer(mb.VertexBufferObject);
        if (mb.IndexBufferObject != 0) gl.DeleteBuffer(mb.IndexBufferObject);
        if (mb.VertexArrayObject != 0) gl.DeleteVertexArray(mb.VertexArrayObject);
    }
}