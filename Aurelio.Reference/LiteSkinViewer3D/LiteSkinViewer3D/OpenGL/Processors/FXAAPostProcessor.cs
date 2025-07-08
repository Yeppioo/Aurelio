using System.Diagnostics;

namespace LiteSkinViewer3D.OpenGL.Processors;

public sealed class FXAAPostProcessor : IDisposable {
    private readonly OpenGLApi gl;
    private readonly bool isGLES;

    private int width, height;

    private int _vao, _vbo;
    private int _texture, _renderBuffer, _frameBuffer;
    private int _shader;
    private int _uniformTexelStep;

    private readonly float[] quadVertices = [
        -1f,  1f, 0f, 1f,
        -1f, -1f, 0f, 0f,
         1f,  1f, 1f, 1f,
         1f, -1f, 1f, 0f
    ];

    public int ColorTexture => _texture;
    public int SourceFramebuffer => _frameBuffer;

    public FXAAPostProcessor(OpenGLApi gl, bool isGLES) {
        this.gl = gl;
        this.isGLES = isGLES;
    }

    public void Initialize(int width, int height) {
        this.width = width;
        this.height = height;

        CreateFramebuffer();
        CompileShader();
        CreateQuad();
        SetUniforms();
    }

    public void Resize(int width, int height) {
        if (this.width == width && this.height == height)
            return;

        DeleteFramebuffer();
        this.width = width;
        this.height = height;
        CreateFramebuffer();
    }

    public void Render(int destinationFramebuffer) {
        gl.Enable(gl.GL_BLEND);
        gl.Disable(gl.GL_DEPTH_TEST);

        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, destinationFramebuffer);
        gl.Viewport(0, 0, width, height);
        gl.Clear(gl.GL_COLOR_BUFFER_BIT);

        gl.UseProgram(_shader);
        gl.Uniform2f(_uniformTexelStep, 1f / width, 1f / height);

        gl.ActiveTexture(gl.GL_TEXTURE0);
        gl.BindTexture(gl.GL_TEXTURE_2D, _texture);

        gl.BindVertexArray(_vao);
        gl.DrawArrays(gl.GL_TRIANGLE_STRIP, 0, 4);

        gl.BindVertexArray(0);
        gl.BindTexture(gl.GL_TEXTURE_2D, _texture);
        gl.UseProgram(0);

        gl.Disable(gl.GL_BLEND);
        gl.Enable(gl.GL_DEPTH_TEST);
    }

    private void CreateFramebuffer() {
        _frameBuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, _frameBuffer);

        _texture = gl.GenTexture();
        gl.BindTexture(gl.GL_TEXTURE_2D, _texture);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_CLAMP_TO_EDGE);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_CLAMP_TO_EDGE);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_LINEAR);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_LINEAR);
        gl.TexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA, width, height, 0, gl.GL_RGBA, gl.GL_UNSIGNED_BYTE, 0);

        gl.FramebufferTexture2D(gl.GL_FRAMEBUFFER, gl.GL_COLOR_ATTACHMENT0, gl.GL_TEXTURE_2D, _texture, 0);

        _renderBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, _renderBuffer);
        gl.RenderbufferStorage(gl.GL_RENDERBUFFER, gl.GL_DEPTH24_STENCIL8, width, height);
        gl.FramebufferRenderbuffer(gl.GL_FRAMEBUFFER, gl.GL_DEPTH_STENCIL_ATTACHMENT, gl.GL_RENDERBUFFER, _renderBuffer);

        if (gl.CheckFramebufferStatus(gl.GL_FRAMEBUFFER) != gl.GL_FRAMEBUFFER_COMPLETE)
            throw new Exception("FXAA Framebuffer incomplete!");

        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
    }

    private void CompileShader() {
        string header = isGLES ? GLSLSource.GlesHeader : GLSLSource.GlHeader;

        int vs = Compile(gl.GL_VERTEX_SHADER, header + GLSLSource.VertexShaderFXAASource, "Vertex");
        int fs = Compile(gl.GL_FRAGMENT_SHADER, header + GLSLSource.FragmentShaderFXAASource, "Fragment");

        _shader = gl.CreateProgram();
        gl.AttachShader(_shader, vs);
        gl.AttachShader(_shader, fs);
        gl.LinkProgram(_shader);
        gl.GetProgramiv(_shader, gl.GL_LINK_STATUS, out int linked);
        if (linked == 0) {
            gl.GetProgramInfoLog(_shader, out var log);
            throw new Exception($"FXAA Shader Link Failed:\n{log}");
        }

        gl.DetachShader(_shader, vs);
        gl.DetachShader(_shader, fs);
        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
    }

    private int Compile(int type, string src, string label) {
        int shader = gl.CreateShader(type);
        gl.ShaderSource(shader, src);
        gl.CompileShader(shader);
        gl.GetShaderiv(shader, gl.GL_COMPILE_STATUS, out int compiled);
        if (compiled == 0) {
            gl.GetShaderInfoLog(shader, out var log);
            throw new Exception($"FXAA {label} Shader Compile Error:\n{log}");
        }
        return shader;
    }

    private unsafe void CreateQuad() {
        _vao = gl.GenVertexArray();
        _vbo = gl.GenBuffer();

        gl.BindVertexArray(_vao);
        gl.BindBuffer(gl.GL_ARRAY_BUFFER, _vbo);
        fixed (float* ptr = quadVertices) {
            gl.BufferData(gl.GL_ARRAY_BUFFER, quadVertices.Length * sizeof(float), new(ptr), gl.GL_STATIC_DRAW);
        }

        int pos = gl.GetAttribLocation(_shader, "a_position");
        int uv = gl.GetAttribLocation(_shader, "a_texCoord");

        gl.EnableVertexAttribArray(pos);
        gl.VertexAttribPointer(pos, 2, gl.GL_FLOAT, false, 4 * sizeof(float), 0);

        gl.EnableVertexAttribArray(uv);
        gl.VertexAttribPointer(uv, 2, gl.GL_FLOAT, false, 4 * sizeof(float), 2 * sizeof(float));

        gl.BindVertexArray(0);
    }

    private void SetUniforms() {
        gl.UseProgram(_shader);

        _uniformTexelStep = gl.GetUniformLocation(_shader, "u_texelStep");
        gl.Uniform1i(gl.GetUniformLocation(_shader, "u_fxaaOn"), 1);
        gl.Uniform1i(gl.GetUniformLocation(_shader, "u_disablePass"), 0);
        gl.Uniform1i(gl.GetUniformLocation(_shader, "u_showEdges"), 0);
        gl.Uniform1f(gl.GetUniformLocation(_shader, "u_lumaThreshold"), 0.3f);
        gl.Uniform1f(gl.GetUniformLocation(_shader, "u_mulReduce"), 1f / 8f);
        gl.Uniform1f(gl.GetUniformLocation(_shader, "u_minReduce"), 1f / 128f);
        gl.Uniform1f(gl.GetUniformLocation(_shader, "u_maxSpan"), 8.0f);

        gl.UseProgram(0);
    }

    private void DeleteFramebuffer() {
        if (_frameBuffer != 0) gl.DeleteFramebuffer(_frameBuffer);
        if (_renderBuffer != 0) gl.DeleteRenderbuffer(_renderBuffer);
        if (_texture != 0) gl.DeleteTexture(_texture);
        _frameBuffer = _renderBuffer = _texture = 0;
    }

    public void Dispose() {
        DeleteFramebuffer();
        if (_vao != 0) gl.DeleteVertexArray(_vao);
        if (_vbo != 0) gl.DeleteBuffer(_vbo);
        if (_shader != 0) gl.DeleteProgram(_shader);
        _vao = _vbo = _shader = 0;
    }
}
