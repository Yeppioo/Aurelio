using LiteSkinViewer3D.OpenGL.Processors;
using LiteSkinViewer3D.Shared;
using LiteSkinViewer3D.Shared.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiteSkinViewer3D.OpenGL;

public class OpenGLSkinViewerBase : SkinViewerBase {
    private readonly OpenGLApi _gl;
    private readonly bool _isGLES;

    private FXAAPostProcessor? _fxaaProcessor;
    private MSAAPostProcessor? _msaaProcessor;
    private SteveModelProcessor? _modelProcessor;
    private SkinTextureProcessor? _textureProcessor;

    private bool _init = false;
    private int _width, _height, _drawIndexCount, _programId;

    public bool IsGLES => _isGLES;

    public OpenGLSkinViewerBase(OpenGLApi gl, bool isGLES = false) {
        _gl = gl;
        _isGLES = isGLES;
    }

    public unsafe void OpenGlInit() {
        if (_init)
            return;

        var info = $"Renderer: {_gl.GetString(_gl.GL_RENDERER)}\n" +
            $"OpenGL Version: {_gl.GetString(_gl.GL_VERSION)}\n" +
            $"GLSL Version: {_gl.GetString(_gl.GL_SHADING_LANGUAGE_VERSION)}";

        Debug.WriteLine(info);

        _init = true;

        _gl.ClearColor(0, 0, 0, 1);
        _gl.CullFace(_gl.GL_BACK);

        InitShader();

        _modelProcessor = new SteveModelProcessor(_gl, _programId);
        _modelProcessor.Initialize(_skinType);
        _drawIndexCount = _modelProcessor.DrawIndexCount;

        _textureProcessor = new SkinTextureProcessor(_gl, IsGLES);
        _textureProcessor.Initialize();

        _fxaaProcessor = new FXAAPostProcessor(_gl, IsGLES);
        _msaaProcessor = new MSAAPostProcessor(_gl);
    }

    public unsafe void OpenGlRender(int fb) {
        if (_switchSkin) {
            _textureProcessor.Load(_skinTex!, _cape, _skinType);
            _switchSkin = false;
            _switchModel = true;
        }

        if (_switchModel) {
            _modelProcessor.Load(_skinType);
            _switchModel = false;
        }

        if (!HaveSkin || Width == 0 || Height == 0)
            return;

        if (Width != _width || Height != _height) {
            _width = Width;
            _height = Height;
            _fxaaProcessor.Resize(_width, _height);
            _msaaProcessor.Resize(_width, _height);
        }

        int renderTarget = _renderMode switch {
            SkinRenderMode.MSAA => _msaaProcessor.Framebuffer,
            SkinRenderMode.FXAA => _fxaaProcessor.Framebuffer,
            _ => fb
        };

        _gl.BindFramebuffer(_gl.GL_FRAMEBUFFER, renderTarget);
        _gl.Viewport(0, 0, _width, _height);
        _gl.ClearColor(_backColor.X, _backColor.Y, _backColor.Z, _backColor.W);

        _gl.ClearDepth(1.0f);
        _gl.Clear(_gl.GL_COLOR_BUFFER_BIT | _gl.GL_DEPTH_BUFFER_BIT);

        _gl.Enable(_gl.GL_CULL_FACE);
        _gl.Enable(_gl.GL_DEPTH_TEST);
        _gl.ActiveTexture(_gl.GL_TEXTURE0);
        _gl.UseProgram(_programId);

        SetupMatrices();

        _gl.DepthMask(true);
        _gl.Disable(_gl.GL_BLEND);

        RenderSkin();
        RenderCape();

        if (_enableTop) {
            _gl.DepthMask(false);
            _gl.Enable(_gl.GL_BLEND);
            _gl.Enable(_gl.GL_SAMPLE_ALPHA_TO_COVERAGE);
            _gl.BlendFunc(_gl.GL_SRC_ALPHA, _gl.GL_ONE_MINUS_SRC_ALPHA);

            RenderSkinTop();

            _gl.DepthMask(true);
            _gl.Disable(_gl.GL_BLEND);
        }

        if (_renderMode == SkinRenderMode.MSAA)
            _msaaProcessor?.ResolveTo(fb, _width, _height);
        else if (_renderMode == SkinRenderMode.FXAA)
            _fxaaProcessor?.Render(fb);

        CheckError();
    }

    public unsafe void OpenGlDeinit() {
        // 关闭贴图资源或纹理流
        _skinAnimationController.Close();

        // 解绑所有 GL 状态，避免悬挂引用
        _gl.BindBuffer(_gl.GL_ARRAY_BUFFER, 0);
        _gl.BindBuffer(_gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        _gl.BindVertexArray(0);
        _gl.UseProgram(0);

        if (_modelProcessor != null) {
            _modelProcessor.Dispose();
            _modelProcessor = null;
        }

        if (_textureProcessor != null) {
            _textureProcessor.Dispose();
            _textureProcessor = null;
        }

        if (_fxaaProcessor != null) {
            _fxaaProcessor.Dispose();
            _fxaaProcessor = null;
        }

        if (_msaaProcessor != null) {
            _msaaProcessor.Dispose();
            _msaaProcessor = null;
        }

        _gl.DeleteProgram(_programId);
        _init = false;
    }

    private unsafe void RenderSkin() {
        _gl.BindTexture(_gl.GL_TEXTURE_2D, _textureProcessor.SkinTexture);

        int modelLoc = _gl.GetUniformLocation(_programId, "self");
        var modelParts = new (ModelComponent partType, MeshBinding vao)[] {
            (ModelComponent.Body, _modelProcessor.BaseVAO.Body),
            (ModelComponent.Head, _modelProcessor.BaseVAO.Head),
            (ModelComponent.ArmLeft, _modelProcessor.BaseVAO.LeftArm),
            (ModelComponent.ArmRight, _modelProcessor.BaseVAO.RightArm),
            (ModelComponent.LegLeft, _modelProcessor.BaseVAO.LeftLeg),
            (ModelComponent.LegRight, _modelProcessor.BaseVAO.RightLeg),
        };

        foreach (var (partType, vao) in modelParts) {
            var modelMatrix = GetMatrix4(partType);
            _gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMatrix);
            _gl.BindVertexArray(vao.VertexArrayObject);
            _gl.DrawElements(_gl.GL_TRIANGLES, _drawIndexCount, _gl.GL_UNSIGNED_SHORT, 0);
        }

        _gl.BindTexture(_gl.GL_TEXTURE_2D, 0);
    }

    private unsafe void RenderSkinTop() {
        _gl.BindTexture(_gl.GL_TEXTURE_2D, _textureProcessor.SkinTexture);

        int modelLoc = _gl.GetUniformLocation(_programId, "self");
        var parts = new (ModelComponent partType, MeshBinding vao)[] {
            (ModelComponent.Body, _modelProcessor.OverlayVAO.Body),
            (ModelComponent.Head, _modelProcessor.OverlayVAO.Head),
            (ModelComponent.ArmLeft, _modelProcessor.OverlayVAO.LeftArm),
            (ModelComponent.ArmRight, _modelProcessor.OverlayVAO.RightArm),
            (ModelComponent.LegLeft, _modelProcessor.OverlayVAO.LeftLeg),
            (ModelComponent.LegRight, _modelProcessor.OverlayVAO.RightLeg),
        };

        foreach (var (type, vao) in parts) {
            var mat = GetMatrix4(type);
            _gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&mat);
            _gl.BindVertexArray(vao.VertexArrayObject);
            _gl.DrawElements(_gl.GL_TRIANGLES, _drawIndexCount, _gl.GL_UNSIGNED_SHORT, 0);
        }

        _gl.BindTexture(_gl.GL_TEXTURE_2D, 0);
    }

    private unsafe void RenderCape() {
        if (!HaveCape || !_enableCape)
            return;

        _gl.BindTexture(_gl.GL_TEXTURE_2D, _textureProcessor.CapeTexture);

        int modelLoc = _gl.GetUniformLocation(_programId, "self");
        var mat = GetMatrix4(ModelComponent.Cape);

        _gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&mat);
        _gl.BindVertexArray(_modelProcessor.BaseVAO.Cape.VertexArrayObject);
        _gl.DrawElements(_gl.GL_TRIANGLES, _drawIndexCount, _gl.GL_UNSIGNED_SHORT, 0);

        _gl.BindTexture(_gl.GL_TEXTURE_2D, 0);
    }

    private void InitShader() {
        string vertexSource = GetHeader() + GLSLSource.VertexShaderSource;
        string fragmentSource = GetHeader() + GLSLSource.FragmentShaderSource;

        int vertexShader = CompileShader(_gl.GL_VERTEX_SHADER, vertexSource, "Vertex");
        int fragmentShader = CompileShader(_gl.GL_FRAGMENT_SHADER, fragmentSource, "Fragment");

        _programId = _gl.CreateProgram();
        _gl.AttachShader(_programId, vertexShader);
        _gl.AttachShader(_programId, fragmentShader);
        _gl.LinkProgram(_programId);

        _gl.GetProgramiv(_programId, _gl.GL_LINK_STATUS, out int linkStatus);
        if (linkStatus == 0) {
            _gl.GetProgramInfoLog(_programId, out var log);
            throw new Exception($"GL_PROGRAM_LINK_ERROR:\n{log}");
        }

        _gl.DetachShader(_programId, vertexShader);
        _gl.DetachShader(_programId, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private static string GetHeader() {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? "#version 150\n#define Macos\n"
            : string.Empty;
    }

    private unsafe void SetupMatrices() {
        var view = GetMatrix4(ModelComponent.ViewMatrix);
        var model = GetMatrix4(ModelComponent.ModelMatrix);
        var projection = GetMatrix4(ModelComponent.ProjectionMatrix);

        int projLoc = _gl.GetUniformLocation(_programId, "projection");
        int viewLoc = _gl.GetUniformLocation(_programId, "view");
        int modelLoc = _gl.GetUniformLocation(_programId, "model");

        _gl.UniformMatrix4fv(projLoc, 1, false, (float*)&projection);
        _gl.UniformMatrix4fv(viewLoc, 1, false, (float*)&view);
        _gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&model);
    }

    private int CompileShader(int type, string source, string label) {
        int shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShaderiv(shader, _gl.GL_COMPILE_STATUS, out int compiled);

        if (compiled == 0) {
            _gl.GetShaderInfoLog(shader, out var info);
            throw new Exception($"GL_{label}_SHADER_COMPILE_ERROR:\n{info}");
        }

        return shader;
    }

    private void CheckError() {
        int err;
        while ((err = _gl.GetError()) != 0)
            Console.WriteLine(err);
    }
}