using Aurelio.Public.Content;
using Avalonia;
using Avalonia.Media;

namespace Aurelio.Plugin.Api.Static;

public static class Resource
{
    public static Icons Icons { get; } = new();
    public static FontFamily DefaultFont { get; } = (FontFamily)Application.Current.Resources["Font"];
    
}