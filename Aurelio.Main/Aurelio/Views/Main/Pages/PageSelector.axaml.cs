using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.Viewers;
using Aurelio.Views.Main.Pages.Viewers.Terminal;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.Pages
{
    public partial class PageSelector : PageMixModelBase, IAurelioTabPage, INotifyPropertyChanged
    {
        private double _containerWidth = 800;

        public PageSelector()
        {
            InitializeComponent();
            DataContext = this;
            RootElement = Root;
            InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
            PageInfo = new PageInfoEntry
            {
                Icon = StreamGeometry.Parse(
                    "F1 M 10 2.421875 C 4.822311 2.421875 0.625 6.619263 0.625 11.796875 C 0.625 16.974487 4.822311 21.171875 10 21.171875 C 15.177689 21.171875 19.375 16.974487 19.375 11.796875 C 19.375 6.619263 15.177689 2.421875 10 2.421875 Z M 13.99353 8.931885 L 13.99353 8.933144 L 10.71228 16.589394 C 10.575562 16.905518 10.265503 17.109375 9.921875 17.109375 C 9.865761 17.109375 9.807739 17.104492 9.750977 17.093506 C 9.351196 17.011719 9.0625 16.656494 9.0625 16.250038 L 9.0625 12.734375 L 5.546265 12.734375 C 5.139771 12.734375 4.786377 12.445068 4.704628 12.047119 C 4.622803 11.650391 4.834595 11.245117 5.20813 11.085205 L 12.86499 7.803955 C 13.19519 7.667236 13.566284 7.739258 13.811646 7.987061 C 14.060669 8.234825 14.13269 8.605919 13.99353 8.931885 Z "),
                Title = MainLang.PageNav
            };
            ShortInfo = $"{MainLang.PageNav}";
            // 监听窗口大小变化
            Root.PropertyChanged += (sender, e) =>
            {
                if (e.Property == BoundsProperty)
                {
                    var newWidth = Root?.Bounds.Width ?? 800;
                    if (Math.Abs(_containerWidth - newWidth) > 1)
                    {
                        _containerWidth = newWidth;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContainerWidth)));
                    }
                }
            };
        }

        public ObservableCollection<NavPageEntry> Viewers { get; set; } =
        [
            new(CodeViewer.StaticPageInfo, CodeViewer.Create),
            new(TerminalViewer.StaticPageInfo, TerminalViewer.Create),
            new(ImageViewer.StaticPageInfo, ImageViewer.Create),
            new(LogViewer.StaticPageInfo, LogViewer.Create),
            new(JsonViewer.StaticPageInfo, JsonViewer.Create),
            new(ZipViewer.StaticPageInfo, ZipViewer.Create)
        ];

        public Control BottomElement { get; set; }
        public Control RootElement { get; set; }
        public PageLoadingAnimator InAnimator { get; set; }
        public TabEntry HostTab { get; set; }
        public PageInfoEntry PageInfo { get; }

        public double ContainerWidth => _containerWidth;
        private string _shortInfo = string.Empty;

        public string ShortInfo
        {
            get => _shortInfo;
            set => SetField(ref _shortInfo, value);
        }
        public new event PropertyChangedEventHandler? PropertyChanged;

        public void OnClose()
        {
        }

        private async void ViewerCardBorder_OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (sender is not Border { Tag: NavPageEntry entry }) return;
            IReadOnlyList<string> fs = [];
            if (entry.StaticPageInfo.NeedPath)
            {
                fs = await this.PickFileAsync(new FilePickerOpenOptions()
                {
                    AllowMultiple = true,
                    Title = MainLang.OpenFile,
                });
                if (fs.Count == 0 && entry.StaticPageInfo.MustPath) return;
            }

            var root = this.GetVisualRoot();

            if (fs.Count == 0)
            {
                if (root is TabWindow window)
                {
                    var page = entry.Create((this, null)) as IAurelioTabPage;
                    if (!entry.StaticPageInfo.AutoCreate)
                        window.TogglePage(null, page!);
                }
                else
                {
                    var page = entry.Create((this, null)) as IAurelioTabPage;
                    if (!entry.StaticPageInfo.AutoCreate)
                        App.UiRoot.TogglePage(null, page!);
                }

                HostTab.Close();
                return;
            }

            if (root is TabWindow window1)
            {
                foreach (var f in fs)
                {
                    var page = entry.Create((this, f)) as IAurelioTabPage;
                    if (!entry.StaticPageInfo.AutoCreate)
                        window1.TogglePage(null, page!);
                }
            }
            else
            {
                foreach (var f in fs)
                {
                    var page = entry.Create((this, f)) as IAurelioTabPage;
                    if (!entry.StaticPageInfo.AutoCreate)
                        App.UiRoot.TogglePage(null, page!);
                }
            }

            HostTab.Close();
        }
    }
}