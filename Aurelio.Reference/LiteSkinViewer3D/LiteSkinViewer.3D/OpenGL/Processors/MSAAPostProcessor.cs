namespace LiteSkinViewer3D.OpenGL.Processors;

internal sealed class MSAAPostProcessor : IDisposable
{
    private readonly OpenGLApi gl;
    private readonly int samples;
    private int _colorBuffer;
    private int _depthStencilBuffer;

    private int height;

    private int width;

    public MSAAPostProcessor(OpenGLApi gl, int samples = 4)
    {
        this.gl = gl;
        this.samples = samples;
    }

    public int Framebuffer { get; private set; }

    public void Dispose()
    {
        if (Framebuffer != 0) gl.DeleteFramebuffer(Framebuffer);
        if (_colorBuffer != 0) gl.DeleteRenderbuffer(_colorBuffer);
        if (_depthStencilBuffer != 0) gl.DeleteRenderbuffer(_depthStencilBuffer);

        Framebuffer = _colorBuffer = _depthStencilBuffer = 0;
    }

    public void Initialize(int width, int height)
    {
        this.width = width;
        this.height = height;

        Framebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, Framebuffer);

        _colorBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, _colorBuffer);
        gl.RenderbufferStorageMultisample(gl.GL_RENDERBUFFER, samples, gl.GL_RGBA8, width, height);
        gl.FramebufferRenderbuffer(gl.GL_FRAMEBUFFER, gl.GL_COLOR_ATTACHMENT0, gl.GL_RENDERBUFFER, _colorBuffer);

        _depthStencilBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, _depthStencilBuffer);
        gl.RenderbufferStorageMultisample(gl.GL_RENDERBUFFER, samples, gl.GL_DEPTH24_STENCIL8, width, height);
        gl.FramebufferRenderbuffer(gl.GL_FRAMEBUFFER, gl.GL_DEPTH_STENCIL_ATTACHMENT, gl.GL_RENDERBUFFER,
            _depthStencilBuffer);

        if (gl.CheckFramebufferStatus(gl.GL_FRAMEBUFFER) != gl.GL_FRAMEBUFFER_COMPLETE)
            throw new Exception("MSAA Framebuffer 不完整（GL_FRAMEBUFFER_COMPLETE 检查失败）");

        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, 0);
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
    }

    public void Resize(int newWidth, int newHeight)
    {
        if (newWidth == width && newHeight == height) return;

        Dispose();
        Initialize(newWidth, newHeight);
    }

    public void ResolveTo(int targetFramebuffer, int width, int height)
    {
        gl.BindFramebuffer(gl.GL_READ_FRAMEBUFFER, Framebuffer);
        gl.BindFramebuffer(gl.GL_DRAW_FRAMEBUFFER, targetFramebuffer);

        gl.BlitFramebuffer(
            0, 0, width, height,
            0, 0, width, height,
            gl.GL_COLOR_BUFFER_BIT,
            gl.GL_NEAREST);

        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
    }
}