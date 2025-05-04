using Aurelio.Public.Module.Value;
using Avalonia;
using Avalonia.Media;
using SukiUI;
using SukiUI.Models;

namespace Aurelio.Public.Module.Services;

public static class Theme
{
    public static void ChangeThemeColor(Color color1 , Color color2)
    {
        var sukiTheme = new SukiColorTheme("Theme", color1, color2);
        SukiTheme.GetInstance().AddColorTheme(sukiTheme);
        SukiTheme.GetInstance().ChangeColorTheme(sukiTheme);
        
        Application.Current.Resources["SystemAccentColor"] = color1;
        Application.Current.Resources["ButtonDefaultPrimaryForeground"] = color1;
        Application.Current.Resources["TextBoxFocusBorderBrush"] = color1;
        Application.Current.Resources["ComboBoxSelectorPressedBorderBrush"] = color1;
        Application.Current.Resources["ComboBoxSelectorFocusBorderBrush"] = color1;
        Application.Current.Resources["TextBoxSelectionBackground"] = color1;
        Application.Current.Resources["ProgressBarPrimaryForeground"] = color1;
        Application.Current.Resources["ProgressBarIndicatorBrush"] = color1;
        Application.Current.Resources["SliderThumbBorderBrush"] = color1;
        Application.Current.Resources["SliderTrackForeground"] = color1;
        Application.Current.Resources["HyperlinkButtonOverForeground"] = color1;
        Application.Current.Resources["SliderThumbPressedBorderBrush"] = color1;
        Application.Current.Resources["SliderThumbPointeroverBorderBrush"] = color1;
        Application.Current.Resources["SystemAccentColorLight1"] = Calculator.ColorVariant(color1, 0.15f);
        Application.Current.Resources["SystemAccentColorLight2"] = Calculator.ColorVariant(color1, 0.30f);
        Application.Current.Resources["SystemAccentColorLight3"] = Calculator.ColorVariant(color1, 0.45f);
        Application.Current.Resources["SystemAccentColorDark1"] = Calculator.ColorVariant(color1, -0.15f);
        Application.Current.Resources["SystemAccentColorDark2"] = Calculator.ColorVariant(color1, -0.30f);
        Application.Current.Resources["SystemAccentColorDark3"] = Calculator.ColorVariant(color1, -0.45f);
    }
}