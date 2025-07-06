using SkiaSharp;

namespace MinecraftSkinRender.OpenGL;

public partial class SkinRenderOpenGL
{
    private int _textureSkin;
    private int _textureCape;

    private void InitTexture()
    {
        _textureSkin = gl.GenTexture();
        _textureCape = gl.GenTexture();
    }

    private unsafe void LoadTex(SKBitmap image, int tex)
    {
        gl.ActiveTexture(gl.GL_TEXTURE0);

        gl.BindTexture(gl.GL_TEXTURE_2D, tex);

        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_NEAREST);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_NEAREST);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_CLAMP_TO_BORDER);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_CLAMP_TO_BORDER);

        CheckError();

        var format = gl.GL_RGBA;
        if (image.ColorType == SKColorType.Bgra8888)
        {
            if (IsGLES)
            {
                using var image1 = image.Copy(SKColorType.Rgba8888);
                gl.TexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA8, image.Width,
               image.Height, 0, gl.GL_RGBA, gl.GL_UNSIGNED_BYTE, image1.GetPixels());
                gl.BindTexture(gl.GL_TEXTURE_2D, 0);

                CheckError();

                return;
            }
            format = gl.GL_BGRA;
        }

        gl.TexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA8, image.Width,
              image.Height, 0, format, gl.GL_UNSIGNED_BYTE, image.GetPixels());
        gl.BindTexture(gl.GL_TEXTURE_2D, 0);
    }

    private void LoadSkin()
    {
        CheckError();

        if (_skinTex == null)
        {
            OnErrorChange(ErrorType.SkinNotFound);
            return;
        }

        if (_skinType == SkinType.Unkonw)
        {
            OnErrorChange(ErrorType.UnknowSkinType);
            return;
        }

        LoadTex(_skinTex, _textureSkin);

        CheckError();

        if (_cape != null)
        {
            LoadTex(_cape, _textureCape);
        }

        CheckError();

        _switchSkin = false;
        _switchModel = true;
    }

    private void DeleteTexture()
    {
        if (_textureSkin != 0)
        {
            gl.DeleteTexture(_textureSkin);
            _textureSkin = 0;
        }
        if (_textureCape != 0)
        {
            gl.DeleteTexture(_textureCape);
            _textureCape = 0;
        }
    }
}
