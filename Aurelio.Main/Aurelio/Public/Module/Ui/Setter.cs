﻿using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Value;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Ursa.Controls;
using TitleBar = Aurelio.Public.Controls.TitleBar;

namespace Aurelio.Public.Module.Ui;

public class Setter
{
    public static void SetAccentColor(Color color)
    {
        try
        {
            Application.Current.Resources["SystemAccentColor"] = color;
            Application.Current.Resources["ButtonDefaultPrimaryForeground"] = color;
            Application.Current.Resources["TextBoxFocusBorderBrush"] = color;
            Application.Current.Resources["ComboBoxSelectorPressedBorderBrush"] = color;
            Application.Current.Resources["ComboBoxSelectorFocusBorderBrush"] = color;
            Application.Current.Resources["TextBoxSelectionBackground"] = color;
            Application.Current.Resources["ProgressBarPrimaryForeground"] = color;
            Application.Current.Resources["ProgressBarIndicatorBrush"] = color;
            Application.Current.Resources["SliderThumbBorderBrush"] = color;
            Application.Current.Resources["SliderTrackForeground"] = color;
            Application.Current.Resources["HyperlinkButtonOverForeground"] = color;
            Application.Current.Resources["SliderThumbPressedBorderBrush"] = color;
            Application.Current.Resources["SliderThumbPointeroverBorderBrush"] = color;
            Application.Current.Resources["SystemAccentColorLight1"] = Calculator.ColorVariant(color, 0.15f);
            Application.Current.Resources["SystemAccentColorLight2"] = Calculator.ColorVariant(color, 0.30f);
            Application.Current.Resources["SystemAccentColorLight3"] = Calculator.ColorVariant(color, 0.45f);
            Application.Current.Resources["SystemAccentColorDark1"] = Calculator.ColorVariant(color, -0.15f);
            Application.Current.Resources["SystemAccentColorDark2"] = Calculator.ColorVariant(color, -0.30f);
            Application.Current.Resources["SystemAccentColorDark3"] = Calculator.ColorVariant(color, -0.45f);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public static void SetBackGround(Setting.BackGround bg, IAurelioWindow w)
    {
        // try
        // {
        //     if (bg == Setting.BackGround.Default)
        //     {
        //         Application.Current.Resources["BackGroundOpacity"] = 1.0;
        //     }
        //     else if (bg == Setting.BackGround.Transparent)
        //     {
        //         Application.Current.Resources["BackGroundOpacity"] = 0.4;
        //     }
        //     else if (bg == Setting.BackGround.AcrylicBlur)
        //     {
        //         Application.Current.Resources["BackGroundOpacity"] = 0.5;
        //     }
        //     else if (bg == Setting.BackGround.Mica)
        //     {
        //         Application.Current.Resources["BackGroundOpacity"] = 0.5;
        //     }
        //     else if (bg == Setting.BackGround.Image)
        //     {
        //         Application.Current.Resources["BackGroundOpacity"] = 0.5;
        //     }
        //     else if (bg == Setting.BackGround.ColorBlock)
        //     {
        //         Application.Current.Resources["BackGroundOpacity"] = 0.5;
        //     }
        // }
        // catch (Exception e)
        // {
        //     Logger.Error(e);
        // }

        if (bg == Setting.BackGround.Default)
        {
            Application.Current.TryGetResource("TextColor",
                Application.Current.ActualThemeVariant, out var c);
            Application.Current.Resources["TabTextColor"] = c;
        }
        else
        {
            Application.Current.Resources["TabTextColor"] = Color.Parse("#ffffff");
        }
        
        try
        {
            var window = w.Window;
            if (bg == Setting.BackGround.Default)
            {
                window.TransparencyLevelHint = [];
                Application.Current.TryGetResource("WindowBackgroundColor",
                    Application.Current.ActualThemeVariant, out var c);
                (w.RootElement as Border)!.Background = new SolidColorBrush((Color)c!);
            }
            else if (bg == Setting.BackGround.Transparent)
            {
                window.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
                (w.RootElement as Border)!.Background = Brushes.Transparent;
            }
            else if (bg == Setting.BackGround.AcrylicBlur)
            {
                window.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                (w.RootElement as Border)!.Background = Brushes.Transparent;
            }
            else if (bg == Setting.BackGround.Mica)
            {
                window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
                (w.RootElement as Border)!.Background = Brushes.Transparent;
            }
            else if (bg == Setting.BackGround.Image)
            {
                window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
                (w.RootElement as Border)!.Background =
                    new ImageBrush(Value.Converter.Base64ToBitmap(Data.SettingEntry.BackGroundImgData))
                    {
                        Stretch = Stretch.UniformToFill
                    };
            }
            else if (bg == Setting.BackGround.ColorBlock)
            {
                window.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
                (w.RootElement as Border)!.Background = new SolidColorBrush(Data.SettingEntry.BackGroundColor);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public static void UpdateWindowStyle(UrsaWindow? window, Action? action = null)
    {
        if (window == null) return;
        if (Data.DesktopType == DesktopType.Linux ||
            Data.DesktopType == DesktopType.FreeBSD ||
            (Data.DesktopType == DesktopType.Windows &&
             Environment.OSVersion.Version.Major < 10))
        {
            window.IsManagedResizerVisible = true;
            window.SystemDecorations = SystemDecorations.None;
        }

        window.FindControl<TitleBar>("TitleBar").IsVisible = true;
        window.FindControl<Border>("Root").CornerRadius = new CornerRadius(8);
        // window.WindowState = WindowState.Maximized;
        // window.WindowState = WindowState.Normal;
        window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        window.ExtendClientAreaToDecorationsHint = true;
        action?.Invoke();
    }

    public static void ToggleTheme(Setting.Theme theme)
    {
        if (theme == Setting.Theme.Light)
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        else if (theme == Setting.Theme.Dark)
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
        else if (theme == Setting.Theme.System) Application.Current.RequestedThemeVariant = ThemeVariant.Default;
    }
}