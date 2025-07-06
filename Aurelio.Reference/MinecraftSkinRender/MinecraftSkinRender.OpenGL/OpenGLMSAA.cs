namespace MinecraftSkinRender.OpenGL;

public partial class SkinRenderOpenGL
{
    private int _msaaRenderBuffer;
    private int _msaaRenderDepth;
    private int _msaaFrameBuffer;

    private void InitMSAAFrameBuffer()
    {
        _msaaFrameBuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, _msaaFrameBuffer);

        _msaaRenderBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, _msaaRenderBuffer);
        gl.RenderbufferStorageMultisample(gl.GL_RENDERBUFFER, 4, gl.GL_RGBA8, _width, _height);
        gl.FramebufferRenderbuffer(gl.GL_FRAMEBUFFER, gl.GL_COLOR_ATTACHMENT0, gl.GL_RENDERBUFFER, _msaaRenderBuffer);

        _msaaRenderDepth = gl.GenRenderbuffer();
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, _msaaRenderDepth);
        gl.RenderbufferStorageMultisample(gl.GL_RENDERBUFFER, 4, gl.GL_DEPTH24_STENCIL8, _width, _height);
        gl.FramebufferRenderbuffer(gl.GL_FRAMEBUFFER, gl.GL_DEPTH_STENCIL_ATTACHMENT, gl.GL_RENDERBUFFER, _msaaRenderDepth);

        if (gl.CheckFramebufferStatus(gl.GL_FRAMEBUFFER) != gl.GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("glCheckFramebufferStatus status != GL_FRAMEBUFFER_COMPLETE");
        }
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, 0);
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
    }

    private void DeleteMSAAFrameBuffer()
    {
        if (_msaaFrameBuffer != 0)
        {
            gl.DeleteFramebuffer(_msaaFrameBuffer);
            _msaaFrameBuffer = 0;
        }
        if (_msaaRenderBuffer != 0)
        {
            gl.DeleteRenderbuffer(_msaaRenderBuffer);
            _msaaRenderBuffer = 0;
        }
        if (_msaaRenderDepth != 0)
        {
            gl.DeleteRenderbuffer(_msaaRenderDepth);
            _msaaRenderDepth = 0;
        }
    }
}
