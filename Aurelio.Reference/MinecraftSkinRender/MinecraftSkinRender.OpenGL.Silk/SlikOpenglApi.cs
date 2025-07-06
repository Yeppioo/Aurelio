using Silk.NET.Core.Native;
using Silk.NET.OpenGL;

namespace MinecraftSkinRender.OpenGL.Silk;

public class SlikOpenglApi(GL gl) : OpenGLApi
{
    public override void ActiveTexture(int bit)
    {
        gl.ActiveTexture((GLEnum)bit);
    }

    public override void AttachShader(int a, int b)
    {
        gl.AttachShader((uint)a, (uint)b);
    }

    public override void BindBuffer(int bit, int index)
    {
        gl.BindBuffer((GLEnum)bit, (uint)index);
    }

    public override void BindFramebuffer(int type, int data)
    {
        gl.BindFramebuffer((GLEnum)type, (uint)data);
    }

    public override void BindRenderbuffer(int target, int renderbuffer)
    {
        gl.BindRenderbuffer((GLEnum)target, (uint)renderbuffer);
    }

    public override void BindTexture(int bit, int index)
    {
        gl.BindTexture((GLEnum)bit, (uint)index);
    }

    public override void BindVertexArray(int vertexArray)
    {
        gl.BindVertexArray((uint)vertexArray);
    }

    public override void BlendFunc(int a, int b)
    {
        gl.BlendFunc((BlendingFactor)a, (BlendingFactor)b);
    }

    public override void BlitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
    {
        gl.BlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, (uint)mask, (GLEnum)filter);
    }

    public override unsafe void BufferData(int type, int v1, nint v2, int type1)
    {
        gl.BufferData((GLEnum)type, (nuint)v1, (void*)v2, (GLEnum)type1);
    }

    public override int CheckFramebufferStatus(int target)
    {
        return (int)gl.CheckFramebufferStatus((GLEnum)target);
    }

    public override void Clear(int bit)
    {
        gl.Clear((uint)bit);
    }

    public override void ClearColor(float r, float g, float b, float a)
    {
        gl.ClearColor(r, g, b, a);
    }

    public override void ClearDepth(float v)
    {
        gl.ClearDepth(v);
    }

    public override void CompileShader(int index)
    {
        gl.CompileShader((uint)index);
    }

    public override int CreateProgram()
    {
        return (int)gl.CreateProgram();
    }

    public override int CreateShader(int type)
    {
        return (int)gl.CreateShader((GLEnum)type);
    }

    public override void CullFace(int mode)
    {
        gl.CullFace((GLEnum)mode);
    }

    public override void DeleteBuffer(int buffers)
    {
        gl.DeleteBuffer((uint)buffers);
    }

    public override void DeleteFramebuffer(int fb)
    {
        gl.DeleteFramebuffer((uint)fb);
    }

    public override void DeleteProgram(int index)
    {
        gl.DeleteProgram((uint)index);
    }

    public override void DeleteRenderbuffer(int renderbuffers)
    {
        gl.DeleteRenderbuffer((uint)renderbuffers);
    }

    public override void DeleteShader(int index)
    {
        gl.DeleteShader((uint)index);
    }

    public override void DeleteTexture(int data)
    {
        gl.DeleteTexture((uint)data);
    }

    public override void DeleteVertexArray(int arrays)
    {
        gl.DeleteVertexArray((uint)arrays);
    }

    public override void DepthMask(bool flag)
    {
        gl.DepthMask(flag);
    }

    public override void DetachShader(int index, int data)
    {
        gl.DetachShader((uint)index, (uint)data);
    }

    public override void Disable(int bit)
    {
        gl.Disable((GLEnum)bit);
    }

    public override void DisableVertexAttribArray(int index)
    {
        gl.DisableVertexAttribArray((uint)index);
    }

    public override unsafe void DrawElements(int type, int count, int type1, nint arry)
    {
        gl.DrawElements((GLEnum)type, (uint)count, (GLEnum)type1, (void*)arry);
    }

    public override void Enable(int bit)
    {
        gl.Enable((GLEnum)bit);
    }

    public override void EnableVertexAttribArray(int index)
    {
        gl.EnableVertexAttribArray((uint)index);
    }

    public override void FramebufferRenderbuffer(int target, int attachment, int renderbuffertarget, int renderbuffer)
    {
        gl.FramebufferRenderbuffer((GLEnum)target, (GLEnum)attachment, (GLEnum)renderbuffertarget, (uint)renderbuffer);
    }

    public override void FramebufferTexture2D(int target, int attachment, int textarget, int texture, int level)
    {
        gl.FramebufferTexture2D((GLEnum)target, (GLEnum)attachment, (GLEnum)textarget, (uint)texture, level);
    }

    public override int GenBuffer()
    {
        return (int)gl.GenBuffer();
    }

    public override int GenFramebuffer()
    {
        return (int)gl.GenFramebuffer();
    }

    public override int GenRenderbuffer()
    {
        return (int)gl.GenRenderbuffer();
    }

    public override int GenTexture()
    {
        return (int)gl.GenTexture();
    }

    public override int GenVertexArray()
    {
        return (int)gl.GenVertexArray();
    }

    public override int GetAttribLocation(int index, string attr)
    {
        return gl.GetAttribLocation((uint)index, attr);
    }

    public override int GetError()
    {
        return (int)gl.GetError();
    }

    public override void GetIntegerv(int bit, out int data)
    {
        gl.GetInteger((GLEnum)bit, out data);
    }

    public override void GetProgramInfoLog(int index, out string log)
    {
        gl.GetProgramInfoLog((uint)index, out log);
    }

    public override void GetProgramiv(int index, int type, out int length)
    {
        gl.GetProgram((uint)index, (GLEnum)type, out length);
    }

    public override void GetShaderInfoLog(int index, out string log)
    {
        gl.GetShaderInfoLog((uint)index, out log);
    }

    public override void GetShaderiv(int index, int type, out int data)
    {
        gl.GetShader((uint)index, (GLEnum)type, out data);
    }

    public override unsafe string GetString(int name)
    {
        return SilkMarshal.PtrToString((nint)gl.GetString((GLEnum)name))!;
    }

    public override int GetUniformLocation(int index, string uni)
    {
        return gl.GetUniformLocation((uint)index, uni);
    }

    public override void LinkProgram(int index)
    {
        gl.LinkProgram((uint)index);
    }

    public override void RenderbufferStorage(int target, int internalformat, int width, int height)
    {
        gl.RenderbufferStorage((GLEnum)target, (GLEnum)internalformat, (uint)width, (uint)height);
    }

    public override void RenderbufferStorageMultisample(int target, int samples, int internalformat, int width, int height)
    {
        gl.RenderbufferStorageMultisample((GLEnum)target, (uint)samples, (GLEnum)internalformat, (uint)width, (uint)height);
    }

    public override void ShaderSource(int a, string source)
    {
        gl.ShaderSource((uint)a, source);
    }

    public override unsafe void TexImage2D(int type, int a, int type1, int w, int h, int size, int type2, int type3, nint data)
    {
        gl.TexImage2D((GLEnum)type, a, type1, (uint)w, (uint)h, size, (GLEnum)type2, (GLEnum)type3, (void*)data);
    }

    public override void TexParameteri(int a, int b, int c)
    {
        gl.TexParameter((GLEnum)a, (GLEnum)b, c);
    }

    public override void Uniform1i(int index, int data)
    {
        gl.Uniform1(index, data);
    }

    public override void Uniform2f(int v, float width, float height)
    {
        gl.Uniform2(v, width, height);
    }

    public override unsafe void UniformMatrix4fv(int index, int length, bool b, float* data)
    {
        gl.UniformMatrix4(index, (uint)length, b, data);
    }

    public override void UseProgram(int index)
    {
        gl.UseProgram((uint)index);
    }

    public override void VertexAttribPointer(int index, int length, int type, bool b, int size, nint arr)
    {
        gl.VertexAttribPointer((uint)index, length, (GLEnum)type, b, (uint)size, arr);
    }

    public override void Viewport(int x, int y, int w, int h)
    {
        gl.Viewport(x, y, (uint)w, (uint)h);
    }

    public override void DrawArrays(int type, int v1, int v2)
    {
        gl.DrawArrays((GLEnum)type, v1, (uint)v2);
    }

    public override void Uniform1f(int loc, float v)
    {
        gl.Uniform1(loc, v);
    }

    // public override void TexStorage2DMultisample(int target, int samples, int internalformat, int width, int height, bool fixedsamplelocations)
    // {
    //     gl.TexStorage2DMultisample((GLEnum)target, (uint)samples, (GLEnum)internalformat, (uint)width, (uint)height, fixedsamplelocations);
    // }
}
