using LiteSkinViewer3D.Shared;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Models;
using System.Runtime.InteropServices;

namespace LiteSkinViewer3D.OpenGL.Processors;

internal sealed class SteveModelProcessor : IDisposable {
    private readonly OpenGLApi gl;
    private readonly int shaderProgram;

    public SteveModelBindings BaseVAO { get; } = new();
    public SteveModelBindings OverlayVAO { get; } = new();

    public int DrawIndexCount { get; private set; }

    public SteveModelProcessor(OpenGLApi gl, int shaderProgram) {
        this.gl = gl;
        this.shaderProgram = shaderProgram;
    }

    public void Initialize(SkinType type) {
        InitVAO(BaseVAO);
        InitVAO(OverlayVAO);
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
        var baseModel = SteveModelFactory.CreateBaseModel(type);
        var baseTexture = SteveTextureBuilder.GetSteveTexture(type);
        var overlayModel = SteveModelFactory.CreateOverlayModel(type);
        var overlayTexture = SteveTextureBuilder.GetSteveTextureTop(type);

        DrawIndexCount = baseModel.Head.Indices.Length;

        PutVAO(BaseVAO.Head, baseModel.Head, baseTexture.Head);
        PutVAO(BaseVAO.Body, baseModel.Body, baseTexture.Body);
        PutVAO(BaseVAO.LeftArm, baseModel.LeftArm, baseTexture.LeftArm);
        PutVAO(BaseVAO.RightArm, baseModel.RightArm, baseTexture.RightArm);
        PutVAO(BaseVAO.LeftLeg, baseModel.LeftLeg, baseTexture.LeftLeg);
        PutVAO(BaseVAO.RightLeg, baseModel.RightLeg, baseTexture.RightLeg);
        if (baseModel.Cape != null && baseTexture.Cape != null)
            PutVAO(BaseVAO.Cape, baseModel.Cape, baseTexture.Cape);

        PutVAO(OverlayVAO.Head, overlayModel.Head, overlayTexture.Head);
        if (type != SkinType.Legacy) {
            PutVAO(OverlayVAO.Body, overlayModel.Body, overlayTexture.Body);
            PutVAO(OverlayVAO.LeftArm, overlayModel.LeftArm, overlayTexture.LeftArm);
            PutVAO(OverlayVAO.RightArm, overlayModel.RightArm, overlayTexture.RightArm);
            PutVAO(OverlayVAO.LeftLeg, overlayModel.LeftLeg, overlayTexture.LeftLeg);
            PutVAO(OverlayVAO.RightLeg, overlayModel.RightLeg, overlayTexture.RightLeg);
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
            int vi = i * 3, ui = i * 2;
            vertices[i] = new VertexDataGL {
                Position = new(model.Vertices[vi], model.Vertices[vi + 1], model.Vertices[vi + 2]),
                UV = new(uv[ui], uv[ui + 1]),
                Normal = new(Cube.Vertices[vi], Cube.Vertices[vi + 1], Cube.Vertices[vi + 2])
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
        DeleteVAO(BaseVAO);
        DeleteVAO(OverlayVAO);
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