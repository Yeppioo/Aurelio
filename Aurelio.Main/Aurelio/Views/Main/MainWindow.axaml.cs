using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using Material.Icons;
using Ursa.Controls;

namespace Aurelio.Views.Main;

public partial class MainWindow : UrsaWindow
{
    public MainViewModel ViewModel { get; set; } = new();
    public ObservableCollection<TabEntry> Tabs => ViewModel.Tabs;
    public TabEntry? SelectedTab => ViewModel.SelectedTab;

    public MainWindow()
    {
        InitializeComponent(attachDevTools: false);
        DataContext = ViewModel;
        NewTabButton.DataContext = ViewModel;
        BindEvents();
#if RELEASE
        InitTitleBar();
#endif
    }

    [AvaloniaHotReload]
    private void InitTitleBar()
    {
        var c = new MoreButtonMenu();
        var menu = (MenuFlyout)c.MainControl!.Flyout;
        MoreButton.Flyout = menu;
        MoreButton.DataContext = new MoreButtonMenuCommands();
        SettingButton.Click += (_, _) =>
        {
            var tab = Tabs.FirstOrDefault(x => x.Tag == "setting");
            if (tab is null)
            {
                var newTab = new TabEntry(ViewModel.SettingTabPage)
                {
                    Tag = "setting"
                };
                Tabs.Add(newTab);
                ViewModel.SelectedTab = newTab;
            }
            else
            {
                if (SelectedTab == tab)
                {
                    _ = ViewModel.SettingTabPage.Animate();
                    return;
                }

                ViewModel.SelectedTab = tab;
            }
        };
    }

    private void BindEvents()
    {
        NavScrollViewer.ScrollChanged += (_, _) => { ViewModel.IsTabMaskVisible = NavScrollViewer.Offset.X > 0; };
        Loaded += (_, _) =>
        {
            RenderOptions.SetTextRenderingMode(this, TextRenderingMode.SubpixelAntialias); // 字体渲染模式
            RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.MediumQuality); // 图片渲染模式
            RenderOptions.SetEdgeMode(this, EdgeMode.Antialias); // 形状渲染模式
        };
        TitleBarContainer.SizeChanged += (_, _) =>
        {
            NavRoot.Margin = new Thickness(80, 0, TitleBarContainer.Bounds.Width + 85, 0);
        };
    }

    private void TabItem_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) return;
        var c = (TabEntry)((Border)sender).Tag;
        c.Close();
    }

    private void NavScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            scrollViewer.Offset = new Vector(
                scrollViewer.Offset.X + e.Delta.Y * 20, // 调整乘数以控制滚动速度
                scrollViewer.Offset.Y
            );
            e.Handled = true;
        }
    }

    private void NavScrollViewer_PointerEntered(object? sender, PointerEventArgs e)
    {
        NavScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
    }

    private void NavScrollViewer_PointerExited(object? sender, PointerEventArgs e)
    {
        NavScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
    }

    public void CreateTab(TabEntry tab) => ViewModel.CreateTab(tab);
}