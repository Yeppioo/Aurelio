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
        AddAccount.Click += (_, _) => { _ = Public.Module.Op.Account.AddByUi(this); };
        Open3DView.Click += (_, _) => { Data.SettingEntry.UsingMinecraftAccount.Render3D();};
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}