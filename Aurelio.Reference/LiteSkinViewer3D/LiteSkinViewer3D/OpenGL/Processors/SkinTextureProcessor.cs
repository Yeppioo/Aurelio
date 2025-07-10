using LiteSkinViewer3D.Shared.Enums;
using SkiaSharp;

namespace LiteSkinViewer3D.OpenGL.Processors;


public sealed class SkinTextureProcessor : IDisposable {
    private readonly OpenGLApi gl;
    private readonly bool isGLES;

    public int SkinTexture { get; private set; }
    public int CapeTexture { get; private set; }

    public SkinTextureProcessor(OpenGLApi gl, bool isGLES) {
        this.gl = gl;
        this.isGLES = isGLES;
    }

    /// <summary>
    /// 初始化纹理 ID
    /// </summary>
    public void Initialize() {
        SkinTexture = gl.GenTexture();
        CapeTexture = gl.GenTexture();
    }

    /// <summary>
    /// 加载皮肤和披风贴图
    /// </summary>
    /// <param name="skin">皮肤图</param>
    /// <param name="cape">披风图（可选）</param>
    /// <param name="type">皮肤类型</param>
    public void Load(SKBitmap skin, SKBitmap? cape, SkinType type) {
        ArgumentNullException.ThrowIfNull(skin);

        if (type == SkinType.Unknown)
            throw new ArgumentException("Unknown SkinType", nameof(type));

        BindTexture(skin, SkinTexture);

        if (cape != null)
            BindTexture(cape, CapeTexture);
    }

    /// <summary>
    /// 将位图绑定为 OpenGL 纹理
    /// </summary>
    private unsafe void BindTexture(SKBitmap bitmap, int tex) {
        gl.ActiveTexture(gl.GL_TEXTURE0);
        gl.BindTexture(gl.GL_TEXTURE_2D, tex);

        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_NEAREST);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_NEAREST);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_CLAMP_TO_BORDER);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_CLAMP_TO_BORDER);

        var format = gl.GL_RGBA;

        if (bitmap.ColorType == SKColorType.Bgra8888) {
            if (isGLES) {
                using var converted = bitmap.Copy(SKColorType.Rgba8888);
                gl.TexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA8,
                    converted.Width, converted.Height, 0,
                    gl.GL_RGBA, gl.GL_UNSIGNED_BYTE, converted.GetPixels());

                gl.BindTexture(gl.GL_TEXTURE_2D, 0);
                return;
            }

            format = gl.GL_BGRA;
        }

        gl.TexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA8,
            bitmap.Width, bitmap.Height, 0,
            format, gl.GL_UNSIGNED_BYTE, bitmap.GetPixels());

        gl.BindTexture(gl.GL_TEXTURE_2D, 0);
    }

    /// <summary>
    /// 清理纹理资源
    /// </summary>
    public void Dispose() {
        if (SkinTexture != 0) {
            gl.DeleteTexture(SkinTexture);
            SkinTexture = 0;
        }

        if (CapeTexture != 0) {
            gl.DeleteTexture(CapeTexture);
            CapeTexture = 0;
        }
    }
}