using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.Module.Value;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Skin;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using MinecraftSkinRender.OpenGL;
using Newtonsoft.Json;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace Aurelio.Public.Classes.Minecraft;

public sealed record RecordMinecraftAccount : INotifyPropertyChanged
{
    [JsonIgnore] private Bitmap? _head;

    [JsonIgnore]
    public Bitmap Head
    {
        get { return _head ??= HandleHeadSkin(); }
    }

    [JsonIgnore] private Bitmap? _body;

    [JsonIgnore]
    public Bitmap Body
    {
        get { return _body ??= HandleBodySkin(); }
    }

    public Enum.Setting.AccountType AccountType { get; set; }

    public string Name { get; set; } = "Unnamed";
    public string UUID { get; set; }
    [JsonIgnore]
    public string FormatLastUsedTime => Calculator.FormatUsedTime(LastUsedTime);
    public DateTime LastUsedTime { get; set; } = DateTime.MinValue;
    public DateTime AddTime { get; set; } = DateTime.MinValue;
    public string? Data { get; set; }

    public string Skin { get; set; } =
        "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAFDUlEQVR42u2a20sUURzH97G0LKMotPuWbVpslj1olJXdjCgyisowsSjzgrB0gSKyC5UF1ZNQWEEQSBQ9dHsIe+zJ/+nXfM/sb/rN4ZwZ96LOrnPgyxzP/M7Z+X7OZc96JpEISfWrFhK0YcU8knlozeJKunE4HahEqSc2nF6zSEkCgGCyb+82enyqybtCZQWAzdfVVFgBJJNJn1BWFgC49/VpwGVlD0CaxQiA5HSYEwBM5sMAdKTqygcAG9+8coHKY/XXAZhUNgDYuBSPjJL/GkzVVhAEU5tqK5XZ7cnFtHWtq/TahdSw2l0HUisr1UKIWJQBAMehDuqiDdzndsP2EZECAG1ZXaWMwOCODdXqysLf++uXUGv9MhUHIByDOijjdiSAoH3ErANQD73C7TXXuGOsFj1d4YH4OTJAEy8y9Hd0mCaeZ5z8dfp88zw1bVyiYhCLOg1ZeAqC0ybaDttHRGME1DhDeVWV26u17lRAPr2+mj7dvULfHw2q65fhQRrLXKDfIxkau3ZMCTGIRR3URR5toU38HbaPiMwUcKfBAkoun09PzrbQ2KWD1JJaqswjdeweoR93rirzyCMBCmIQizqoizZkm2H7iOgAcHrMHbbV9KijkUYv7qOn55sdc4fo250e+vUg4329/Xk6QB/6DtOws+dHDGJRB3XRBve+XARt+4hIrAF4UAzbnrY0ve07QW8uHfB+0LzqanMM7qVb+3f69LJrD90/1axiEIs6qIs21BTIToewfcSsA+Bfb2x67OoR1aPPzu2i60fSNHRwCw221Suz0O3jO+jh6V1KyCMGse9721XdN5ePutdsewxS30cwuMjtC860T5JUKpXyKbSByUn7psi5l+juDlZYGh9324GcPKbkycaN3jUSAGxb46IAYPNZzW0AzgiQ5tVnzLUpUDCAbakMQXXrOtX1UMtHn+Q9/X5L4wgl7t37r85OSrx+TYl379SCia9KXjxRpiTjIZTBFOvrV1f8ty2eY/T7XJ81FQAwmA8ASH1ob68r5PnBsxA88/xAMh6SpqW4HRnLBrkOA9Xv5wPAZjAUgOkB+SHxgBgR0qSMh0zmZRsmwDJm1gFg2PMDIC8/nAHIMls8x8GgzOsG5WiaqREgYzDvpTwjLDy8NM15LpexDEA3LepjU8Z64my+8PtDCmUyRr+fFwA2J0eAFYA0AxgSgMmYBMZTwFQnO9RNAEaHOj2DXF5UADmvAToA2ftyxZYA5BqgmZZApDkdAK4mAKo8GzPlr8G8AehzMAyA/i1girUA0HtYB2CaIkUBEHQ/cBHSvwF0AKZFS5M0ZwMQtEaEAmhtbSUoDADH9ff3++QZ4o0I957e+zYAMt6wHkhzpjkuAcgpwNcpA7AZDLsvpwiuOkBvxygA6Bsvb0HlaeKIF2EbADZpGiGzBsA0gnwQHGOhW2snRpbpPexbAB2Z1oicAMQpTnGKU5ziFKc4xSlOcYpTnOIUpzgVmgo+XC324WfJAdDO/+ceADkCpuMFiFKbApEHkOv7BfzfXt+5gpT8V7rpfYJcDz+jAsB233r6yyBsJ0mlBCDofuBJkel4vOwBFPv8fyYAFPJ+wbSf/88UANNRVy4Awo6+Ig2gkCmgA5DHWjoA+X7AlM//owLANkX0w0359od++pvX8fdMAcj3/QJ9iJsAFPQCxHSnQt8vMJ3v2wCYpkhkAOR7vG7q4aCXoMoSgG8hFAuc/grMdAD4B/kHl9da7Ne9AAAAAElFTkSuQmCC";

    public void UpdateSkin(byte[] data)
    {
        if (data == null) return;
        Skin = Convert.ToBase64String(data);
    }

    private Bitmap HandleHeadSkin()
    {
        // SkinResolver SkinResolver = new(Convert.FromBase64String(Skin));
        // var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
        // return Converter.Base64ToBitmap(Converter.BytesToBase64(bytes));

        using var stream = new MemoryStream(Convert.FromBase64String(Skin));
        using var skin = SKBitmap.Decode(stream);
        var image =Skin2DHeadTypeB.MakeHeadImage(skin);
        using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
        using var imageStream = new MemoryStream(imageData.ToArray());
        return Bitmap.DecodeToWidth(imageStream, 220);
    }

    private Bitmap HandleBodySkin()
    {
        using var stream = new MemoryStream(Convert.FromBase64String(Skin));
        using var skin = SKBitmap.Decode(stream);
        var head = Skin2DTypeB.MakeSkinImage(skin);
        using var imageStream = new MemoryStream();
        head.Encode(imageStream, SKEncodedImageFormat.Png, 100);
        imageStream.Seek(0, SeekOrigin.Begin);
        return Bitmap.DecodeToWidth(imageStream, 220);
    }

    public bool Equals(RecordMinecraftAccount? other)
    {
        return other != null && Skin == other.Skin && Name == other.Name && Data == other.Data;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void Render3D()
    {
        using var window = Silk.NET.Windowing.Window.Create(WindowOptions.Default with
        {
            API = GraphicsAPI.Default with
            {
                // API = ContextAPI.OpenGL,
                // Version = new(3, 0)
            },
            Size = new(400, 400),
            VSync = true
        });

        // Declare some variables
        GL gl = null;
        SkinRenderOpenGL? skin = null;

        // Our loading function
        window.Load += () =>
        {
            gl = window.CreateOpenGL();
            skin = new(new SlikOpenglApi(gl))
            {
                IsGLES = true
            };
            byte[] imageBytes = Convert.FromBase64String(Skin);

            using var stream = new MemoryStream(imageBytes);
            var img = SKBitmap.Decode(stream);
            skin.SetSkinTex(img);
            skin.SkinType = SkinType.NewSlim;
            skin.EnableTop = true;
            skin.RenderType = SkinRenderType.Normal;
            skin.Animation = true;
            skin.EnableCape = true;
            skin.FpsUpdate += (a, b) => { Console.WriteLine("Fps: " + b); };
            skin.BackColor = new(1, 1, 1, 1);
            skin.Width = window.FramebufferSize.X;
            skin.Height = window.FramebufferSize.Y;
            skin.OpenGlInit();
        };

        window.FramebufferResize += s =>
        {
            if (skin == null)
            {
                return;
            }

            skin.Width = s.X;
            skin.Height = s.Y;
        };

        // The render function
        window.Render += delta =>
        {
            if (skin == null)
            {
                return;
            }

            skin.Rot(0, 1f);
            skin.Tick(delta);
            skin.OpenGlRender(0);
            //gl.Clear(ClearBufferMask.ColorBufferBit);
            //gl.ClearColor(0, 0, 1, 0);
            //window.SwapBuffers();
        };

        // The closing function
        window.Closing += () =>
        {
            if (skin == null)
            {
                return;
            }

            skin.OpenGlDeinit();
            // Unload OpenGL
            gl?.Dispose();
        };

        window.Run();
    }
}