using System.Numerics;

namespace MinecraftSkinRender.OpenGL;

public partial class SkinRenderOpenGL(OpenGLApi gl) : SkinRender
{
    private bool _init = false;

    private int _width, _height;

    /// <summary>
    /// 是否为opengles
    /// </summary>
    public bool IsGLES { get; init; }

    /// <summary>
    /// opengl初始化
    /// </summary>
    public unsafe void OpenGlInit()
    {
        if (_init)
        {
            return;
        }

        _init = true;

        Info = $"Renderer: {gl.GetString(gl.GL_RENDERER)}\n" +
        $"OpenGL Version: {gl.GetString(gl.GL_VERSION)}\n" +
        $"GLSL Version: {gl.GetString(gl.GL_SHADING_LANGUAGE_VERSION)}";

        gl.ClearColor(0, 0, 0, 1);
        gl.CullFace(gl.GL_BACK);

        InitShader();
        InitModel();
        InitFXAA();
        InitTexture();

        CheckError();
    }

    private void InitFrameBuffer()
    {
        InitMSAAFrameBuffer();
        InitFXAAFrameBuffer();
    }

    private void DeleteFrameBuffer()
    {
        DeleteMSAAFrameBuffer();
        DeleteFXAAFrameBuffer();
    }

    private unsafe void DrawCape()
    {
        if (HaveCape && _enableCape)
        {
            gl.BindTexture(gl.GL_TEXTURE_2D, _textureCape);
            var modelLoc = gl.GetUniformLocation(_pgModel, "self");
            var mat = GetMatrix4(ModelPartType.Cape);
            gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&mat);
            gl.BindVertexArray(_normalVAO.Cape.VertexArrayObject);
            gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
                gl.GL_UNSIGNED_SHORT, 0);

            gl.BindTexture(gl.GL_TEXTURE_2D, 0);
        }
    }

    private unsafe void DrawSkin()
    {
        gl.BindTexture(gl.GL_TEXTURE_2D, _textureSkin);

        var modelLoc = gl.GetUniformLocation(_pgModel, "self");
        var modelMat = Matrix4x4.Identity;
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_normalVAO.Body.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.Head);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_normalVAO.Head.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.LeftArm);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_normalVAO.LeftArm.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.RightArm);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_normalVAO.RightArm.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.LeftLeg);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_normalVAO.LeftLeg.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.RightLeg);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_normalVAO.RightLeg.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        gl.BindTexture(gl.GL_TEXTURE_2D, 0);
    }

    private unsafe void DrawSkinTop()
    {
        gl.BindTexture(gl.GL_TEXTURE_2D, _textureSkin);

        var modelLoc = gl.GetUniformLocation(_pgModel, "self");
        var modelMat = GetMatrix4(ModelPartType.Body);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_topVAO.Body.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.Head);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_topVAO.Head.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.LeftArm);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_topVAO.LeftArm.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.RightArm);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_topVAO.RightArm.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.LeftLeg);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_topVAO.LeftLeg.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        modelMat = GetMatrix4(ModelPartType.RightLeg);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&modelMat);
        gl.BindVertexArray(_topVAO.RightLeg.VertexArrayObject);
        gl.DrawElements(gl.GL_TRIANGLES, _steveModelDrawOrderCount,
           gl.GL_UNSIGNED_SHORT, 0);

        gl.BindTexture(gl.GL_TEXTURE_2D, 0);
    }

    /// <summary>
    /// 开始渲染
    /// </summary>
    /// <param name="fb">目标fb</param>
    public unsafe void OpenGlRender(int fb)
    {
        if (_switchSkin)
        {
            LoadSkin();
        }
        if (_switchModel)
        {
            LoadModel();
        }

        if (!HaveSkin)
        {
            return;
        }

        if (Width == 0 || Height == 0)
        {
            return;
        }

        if (Width != _width || Height != _height)
        {
            _width = Width;
            _height = Height;
            DeleteFrameBuffer();
            InitFrameBuffer();
        }

        if (_width == 0 || _height == 0)
        {
            return;
        }

        if (_renderType == SkinRenderType.MSAA)
        {
            gl.BindFramebuffer(gl.GL_FRAMEBUFFER, _msaaFrameBuffer);
        }
        else if (_renderType == SkinRenderType.FXAA)
        {
            gl.BindFramebuffer(gl.GL_FRAMEBUFFER, _fxaaFrameBuffer);
        }
        else
        {
            gl.BindFramebuffer(gl.GL_FRAMEBUFFER, fb);
        }

        gl.Viewport(0, 0, _width, _height);

        if (_renderType == SkinRenderType.FXAA)
        {
            gl.ClearColor(1, 1, 1, 1);
        }
        else
        {
            gl.ClearColor(_backColor.X, _backColor.Y, _backColor.Z, _backColor.W);
        }
        gl.ClearDepth(1.0f);
        gl.Clear(gl.GL_COLOR_BUFFER_BIT | gl.GL_DEPTH_BUFFER_BIT);

        CheckError();

        gl.Enable(gl.GL_CULL_FACE);
        gl.Enable(gl.GL_DEPTH_TEST);
        gl.ActiveTexture(gl.GL_TEXTURE0);
        gl.UseProgram(_pgModel);

        //if (IsGLES)
        //{
        //    gl.ClearDepth(1);
        //    gl.DepthMask(true);
        //    gl.Disable(gl.GL_CULL_FACE);
        //    gl.Disable(EnableCap.ScissorTest);
        //    gl.DepthFunc(DepthFunction.Less);
        //}

        CheckError();

        var viewLoc = gl.GetUniformLocation(_pgModel, "view");
        var projectionLoc = gl.GetUniformLocation(_pgModel, "projection");
        var modelLoc = gl.GetUniformLocation(_pgModel, "model");

        var matr = GetMatrix4(ModelPartType.Proj);
        gl.UniformMatrix4fv(projectionLoc, 1, false, (float*)&matr);

        matr = GetMatrix4(ModelPartType.View);
        gl.UniformMatrix4fv(viewLoc, 1, false, (float*)&matr);

        matr = GetMatrix4(ModelPartType.Model);
        gl.UniformMatrix4fv(modelLoc, 1, false, (float*)&matr);

        CheckError();

        gl.DepthMask(true);
        gl.Disable(gl.GL_BLEND);

        DrawSkin();
        DrawCape();

        if (_enableTop)
        {
            gl.DepthMask(false);
            gl.Enable(gl.GL_BLEND);
            gl.Enable(gl.GL_SAMPLE_ALPHA_TO_COVERAGE);
            gl.BlendFunc(gl.GL_SRC_ALPHA, gl.GL_ONE_MINUS_SRC_ALPHA);

            DrawSkinTop();

            gl.DepthMask(true);
            gl.Disable(gl.GL_BLEND);
        }

        if (_renderType == SkinRenderType.MSAA)
        {
            gl.BindFramebuffer(gl.GL_DRAW_FRAMEBUFFER, fb);
            gl.BindFramebuffer(gl.GL_READ_FRAMEBUFFER, _msaaFrameBuffer);
            gl.BlitFramebuffer(0, 0, _width, _height, 0, 0, _width,
                _height, gl.GL_COLOR_BUFFER_BIT, gl.GL_NEAREST);
            gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
        }
        else if (_renderType == SkinRenderType.FXAA)
        {
            gl.Enable(gl.GL_BLEND);
            gl.Disable(gl.GL_DEPTH_TEST);
            gl.BindFramebuffer(gl.GL_FRAMEBUFFER, fb);
            gl.Viewport(0, 0, _width, _height);
            gl.Clear(gl.GL_COLOR_BUFFER_BIT);
            gl.UseProgram(_pgFXAA);
            gl.Uniform2f(_fxaaStep, 1.0f / _width, 1.0f / _height);
            gl.ActiveTexture(gl.GL_TEXTURE0);
            gl.BindTexture(gl.GL_TEXTURE_2D, _fxaaTexture);
            gl.BindVertexArray(_fxaaVAO);
            gl.DrawArrays(gl.GL_TRIANGLE_STRIP, 0, 4);
            gl.BindVertexArray(0);
            gl.Enable(gl.GL_DEPTH_TEST);
            gl.BindTexture(gl.GL_TEXTURE_2D, 0);
            gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
        }

        CheckError();
    }

    private void CheckError()
    {
        int err;
        while ((err = gl.GetError()) != 0)
            Console.WriteLine(err);
    }

    /// <summary>
    /// opengl清理
    /// </summary>
    public unsafe void OpenGlDeinit()
    {
        _skina.Close();

        // Unbind everything
        gl.BindBuffer(gl.GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        gl.BindVertexArray(0);
        gl.UseProgram(0);

        // Delete all resources.
        DeleteModel();
        DeleteFrameBuffer();
        DeleteTexture();
        DeleteFXAA();

        gl.DeleteProgram(_pgModel);
        gl.DeleteProgram(_pgFXAA);
    }
}