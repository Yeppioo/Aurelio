using System;
using System.IO;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using MinecraftSkinRender;
using MinecraftSkinRender.OpenGL;
using MinecraftSkinRender.OpenGL.Silk;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;
using Window = Avalonia.Controls.Window;

namespace Aurelio.Views.Main.SettingPages;

public partial class AccountPage : PageMixModelBase, IAurelioPage
{
    public AccountPage()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }
    
    private void BindingEvent()
    {

        
        
        using var window =  Silk.NET.Windowing.Window.Create(WindowOptions.Default with
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
            byte[] imageBytes = Convert.FromBase64String(Data.SettingEntry.UsingMinecraftAccount.Skin);

            using var stream = new MemoryStream(imageBytes);
            var img = SKBitmap.Decode(stream);
            skin.SetSkinTex(img);
            skin.SkinType = SkinType.NewSlim;
            skin.EnableTop = true;
            skin.RenderType = SkinRenderType.Normal;
            skin.Animation = true;
            skin.EnableCape = true;
            skin.FpsUpdate += (a, b) =>
            {
                Console.WriteLine("Fps: " + b);
            };
            skin.BackColor = new(1, 1, 1, 1);
            skin.Width = window.FramebufferSize.X;
            skin.Height = window.FramebufferSize.Y;
            skin.OpenGlInit();
        };
        
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
 

        Loaded += (_, _) =>
        {
            var border = RBorder;
            window.Run();

        };
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}

