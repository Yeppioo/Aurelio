using Avalonia.OpenGL;
using LiteSkinViewer3D.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LiteSkinViewer3D.Avalonia;

public class AvaloniaApi(GlInterface gl) : OpenGLApi() {
    public delegate void Func1(int a, int b);

    public delegate void Func2(int target, int samples, int internalformat, int width, int height);

    public delegate void Func3(bool flag);

    public delegate void Func5(int a);

    public delegate void Func6(int a, int b, int c, int d, int e);

    public delegate void Func8(int a, float b, float c);

    public readonly Func1 GLBlendFunc = Marshal.GetDelegateForFunctionPointer<Func1>(gl.GetProcAddress("glBlendFunc"));

    public readonly Func5 GLCullFace = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glCullFace"));

    public readonly Func3 GLDepthMask = Marshal.GetDelegateForFunctionPointer<Func3>(gl.GetProcAddress("glDepthMask"));

    public readonly Func1 GLDetachShader = Marshal.GetDelegateForFunctionPointer<Func1>(gl.GetProcAddress("glDetachShader"));

    public readonly Func5 GLDisable = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glDisable"));

    public readonly Func5 GLDisableVertexAttribArray = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glDisableVertexAttribArray"));

    public readonly Func6 GLFramebufferTexture2D = Marshal.GetDelegateForFunctionPointer<Func6>(gl.GetProcAddress("glFramebufferTexture2D"));

    public readonly Func2 GLRenderbufferStorageMultisample = Marshal.GetDelegateForFunctionPointer<Func2>(gl.GetProcAddress("glRenderbufferStorageMultisample"));

    public readonly Func1 GLUniform1i = Marshal.GetDelegateForFunctionPointer<Func1>(gl.GetProcAddress("glUniform1i"));

    public readonly Func8 GLUniform2f = Marshal.GetDelegateForFunctionPointer<Func8>(gl.GetProcAddress("glUniform2f"));

    public override void ActiveTexture(int bit) {
        gl.ActiveTexture(bit);
    }

    public override void AttachShader(int a, int b) {
        gl.AttachShader(a, b);
    }

    public override void BindBuffer(int bit, int index) {
        gl.BindBuffer(bit, index);
    }

    public override void BindFramebuffer(int type, int data) {
        gl.BindFramebuffer(type, data);
    }

    public override void BindTexture(int bit, int index) {
        gl.BindTexture(bit, index);
    }

    public override void BlendFunc(int a, int b) {
        GLBlendFunc(a, b);
    }

    public override void Clear(int bit) {
        gl.Clear(bit);
    }

    public override void ClearColor(float r, float g, float b, float a) {
        gl.ClearColor(r, g, b, a);
    }

    public override void CompileShader(int index) {
        gl.CompileShader(index);
    }

    public override int CreateProgram() {
        return gl.CreateProgram();
    }

    public override int CreateShader(int type) {
        return gl.CreateShader(type);
    }

    public override void DeleteFramebuffer(int fb) {
        gl.DeleteFramebuffer(fb);
    }

    public override void DeleteProgram(int index) {
        gl.DeleteProgram(index);
    }

    public override void DeleteShader(int index) {
        gl.DeleteShader(index);
    }

    public override void DeleteTexture(int data) {
        gl.DeleteTexture(data);
    }

    public override void DetachShader(int index, int data) {
        GLDetachShader(index, data);
    }

    public override void Disable(int bit) {
        GLDisable(bit);
    }

    public override void DisableVertexAttribArray(int index) {
        GLDisableVertexAttribArray(index);
    }

    public override void DrawElements(int type, int count, int type1, nint arry) {
        gl.DrawElements(type, count, type1, arry);
    }

    public override void Enable(int bit) {
        gl.Enable(bit);
    }

    public override void EnableVertexAttribArray(int index) {
        gl.EnableVertexAttribArray(index);
    }

    public override int GenFramebuffer() {
        return gl.GenFramebuffer();
    }

    public override int GenTexture() {
        return gl.GenTexture();
    }

    public override int GetAttribLocation(int index, string attr) {
        return gl.GetAttribLocationString(index, attr);
    }

    public override int GetError() {
        return gl.GetError();
    }

    public override void GetIntegerv(int bit, out int data) {
        gl.GetIntegerv(bit, out data);
    }

    public unsafe override void GetProgramInfoLog(int index, out string log) {
        int num = default;
        gl.GetProgramiv(index, GL_INFO_LOG_LENGTH, &num);
        byte[] array = new byte[num];
        int len;
        fixed (byte* ptr = array) {
            void* infoLog = ptr;
            gl.GetProgramInfoLog(index, num, out len, infoLog);
        }
        log = Encoding.UTF8.GetString(array, 0, len);
    }

    public unsafe override void GetShaderInfoLog(int index, out string log) {
        int num = default;
        gl.GetShaderiv(index, GL_INFO_LOG_LENGTH, &num);
        byte[] array = new byte[num];
        int length;
        fixed (byte* ptr = array) {
            void* infoLog = ptr;
            gl.GetShaderInfoLog(index, num, out length, infoLog);
        }
        log = Encoding.UTF8.GetString(array, 0, length);
    }

    public override int GetUniformLocation(int index, string uni) {
        return gl.GetUniformLocationString(index, uni);
    }

    public override void LinkProgram(int index) {
        gl.LinkProgram(index);
    }

    public override void ShaderSource(int a, string source) {
        gl.ShaderSourceString(a, source);
    }

    public override void TexImage2D(int type, int a, int type1, int w, int h, int size, int type2, int type3, nint data) {
        gl.TexImage2D(type, a, type1, w, h, size, type2, type3, data);
    }

    public override void TexParameteri(int a, int b, int c) {
        gl.TexParameteri(a, b, c);
    }

    public override void Uniform1i(int index, int data) {
        GLUniform1i(index, data);
    }

    public override void UseProgram(int index) {
        gl.UseProgram(index);
    }

    public override void VertexAttribPointer(int index, int length, int type, bool b, int size, nint arr) {
        gl.VertexAttribPointer(index, length, type, b ? 1 : 0, size, arr);
    }

    public override void Viewport(int x, int y, int w, int h) {
        gl.Viewport(x, y, w, h);
    }

    public override int GenBuffer() {
        return gl.GenBuffer();
    }

    public override void BufferData(int type, int v1, nint v2, int type1) {
        gl.BufferData(type, v1, v2, type1);
    }

    public override int GenVertexArray() {
        return gl.GenVertexArray();
    }

    public override void BindVertexArray(int vertexArray) {
        gl.BindVertexArray(vertexArray);
    }

    public unsafe override void UniformMatrix4fv(int index, int length, bool b, float* data) {
        gl.UniformMatrix4fv(index, length, b, data);
    }

    public unsafe override void GetShaderiv(int index, int type, out int data) {
        int num = default;
        gl.GetShaderiv(index, type, &num);
        data = num;
    }

    public unsafe override void GetProgramiv(int index, int type, out int length) {
        int num = default;
        gl.GetProgramiv(index, type, &num);
        length = num;
    }

    public override void CullFace(int mode) {
        GLCullFace(mode);
    }

    public override string GetString(int name) {
        return gl.GetString(name)!;
    }

    public override int GenRenderbuffer() {
        return gl.GenRenderbuffer();
    }

    public override void BindRenderbuffer(int target, int renderbuffer) {
        gl.BindRenderbuffer(target, renderbuffer);
    }

    public override void RenderbufferStorageMultisample(int target, int samples, int internalformat, int width, int height) {
        GLRenderbufferStorageMultisample(target, samples, internalformat, width, height);
    }

    public override void FramebufferRenderbuffer(int target, int attachment, int renderbuffertarget, int renderbuffer) {
        gl.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
    }

    public override int CheckFramebufferStatus(int target) {
        return gl.CheckFramebufferStatus(target);
    }

    public override void DeleteRenderbuffer(int renderbuffers) {
        gl.DeleteRenderbuffer(renderbuffers);
    }

    public override void DepthMask(bool flag) {
        GLDepthMask(flag);
    }

    public override void BlitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter) {
        gl.BlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
    }

    public override void DeleteBuffer(int buffers) {
        gl.DeleteBuffer(buffers);
    }

    public override void DeleteVertexArray(int arrays) {
        gl.DeleteVertexArray(arrays);
    }

    public override void FramebufferTexture2D(int target, int attachment, int textarget, int texture, int level) {
        GLFramebufferTexture2D(target, attachment, textarget, texture, level);
    }

    public override void ClearDepth(float v) {
        gl.ClearDepth(v);
    }

    public override void RenderbufferStorage(int target, int internalformat, int width, int height) {
        gl.RenderbufferStorage(target, internalformat, width, height);
    }

    public override void Uniform2f(int v, float width, float height) {
        GLUniform2f(v, width, height);
    }

    public override void DrawArrays(int type, int v1, int v2) {
        gl.DrawArrays(type, v1, v2);
    }

    public override void Uniform1f(int loc, float v) {
        gl.Uniform1f(loc, v);
    }
}