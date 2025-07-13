using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Aurelio.Public.Module.Value;
using Aurelio.Public.Module.IO;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using MinecraftLaunch.Skin;
using Newtonsoft.Json;
using SkiaSharp;

namespace Aurelio.Public.Classes.Minecraft;

public sealed record RecordMinecraftAccount : INotifyPropertyChanged
{
    [JsonIgnore] private Bitmap? _head;
    public Enum.Setting.AccountType AccountType { get; set; }
    public string Name { get; set; } = "Unnamed";
    public string UUID { get; set; }
    [JsonIgnore] public string FormatLastUsedTime => Calculator.FormatUsedTime(LastUsedTime);
    public DateTime LastUsedTime { get; set; } = DateTime.MinValue;
    public DateTime AddTime { get; set; } = DateTime.MinValue;
    public string? Data { get; set; }

    public string Skin { get; set; } =
        "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAFDUlEQVR42u2a20sUURzH97G0LKMotPuWbVpslj1olJXdjCgyisowsSjzgrB0gSKyC5UF1ZNQWEEQSBQ9dHsIe+zJ/+nXfM/sb/rN4ZwZ96LOrnPgyxzP/M7Z+X7OZc96JpEISfWrFhK0YcU8knlozeJKunE4HahEqSc2nF6zSEkCgGCyb+82enyqybtCZQWAzdfVVFgBJJNJn1BWFgC49/VpwGVlD0CaxQiA5HSYEwBM5sMAdKTqygcAG9+8coHKY/XXAZhUNgDYuBSPjJL/GkzVVhAEU5tqK5XZ7cnFtHWtq/TahdSw2l0HUisr1UKIWJQBAMehDuqiDdzndsP2EZECAG1ZXaWMwOCODdXqysLf++uXUGv9MhUHIByDOijjdiSAoH3ErANQD73C7TXXuGOsFj1d4YH4OTJAEy8y9Hd0mCaeZ5z8dfp88zw1bVyiYhCLOg1ZeAqC0ybaDttHRGME1DhDeVWV26u17lRAPr2+mj7dvULfHw2q65fhQRrLXKDfIxkau3ZMCTGIRR3URR5toU38HbaPiMwUcKfBAkoun09PzrbQ2KWD1JJaqswjdeweoR93rirzyCMBCmIQizqoizZkm2H7iOgAcHrMHbbV9KijkUYv7qOn55sdc4fo250e+vUg4329/Xk6QB/6DtOws+dHDGJRB3XRBve+XARt+4hIrAF4UAzbnrY0ve07QW8uHfB+0LzqanMM7qVb+3f69LJrD90/1axiEIs6qIs21BTIToewfcSsA+Bfb2x67OoR1aPPzu2i60fSNHRwCw221Suz0O3jO+jh6V1KyCMGse9721XdN5ePutdsewxS30cwuMjtC860T5JUKpXyKbSByUn7psi5l+juDlZYGh9324GcPKbkycaN3jUSAGxb46IAYPNZzW0AzgiQ5tVnzLUpUDCAbakMQXXrOtX1UMtHn+Q9/X5L4wgl7t37r85OSrx+TYl379SCia9KXjxRpiTjIZTBFOvrV1f8ty2eY/T7XJ81FQAwmA8ASH1ob68r5PnBsxA88/xAMh6SpqW4HRnLBrkOA9Xv5wPAZjAUgOkB+SHxgBgR0qSMh0zmZRsmwDJm1gFg2PMDIC8/nAHIMls8x8GgzOsG5WiaaREgYzDvpTwjLDy8NM15LpexDEA3LepjU8Z64my+8PtDCmUyRr+fFwA2J0eAFYA0AxgSgMmYBMZTwFQnO9RNAEaHOj2DXF5UADmvAToA2ftyxZYA5BqgmZZApDkdAK4mAKo8GzPlr8G8AehzMAyA/i1girUA0HtYB2CaIkUBEHQ/cBHSvwF0AKZFS5M0ZwMQtEaEAmhtbSUoDADH9ff3++QZ4o0I957e+zYAMt6wHkhzpjkuAcgpwNcpA7AZDLsvpwiuOkBvxygA6Bsvb0HlaeKIF2EbADZpGiGzBsA0gnwQHGOhW2snRpbpPexbAB2Z1oicAMQpTnFKnOIUpzjFKU5xilOc4hSnOBWaCj5cLfbhZ8kB0M7/5x4AOQKm4wWIUpsCkQeQ6/sF/N9e37mClPxXuul9glwPP6MCwHbfevqrIGwnSaUEIOh+4EmR6Xi87AEU+/x/JgAU8n7BtJ//zxQA01FXLgDCjr4iDaCQKaADkMdaOgD5fsB0vt9/JgBsU0Q/3JRvf+inv3kdf88UgHzfL9CHuAlAQS9ATHcq9P0C0/m+DYBpikQGQL7H66YeDnoJqiwB+BZCscDpL8BMB4B/kH9wed1ivl/3AAAAAElFTkSuQmCC";

    [JsonIgnore]
    public Bitmap Head
    {
        get { return _head ??= HandleHeadSkin(); }
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
    
    private Bitmap HandleHeadSkin()
    {
        SkinResolver SkinResolver = new(Convert.FromBase64String(Skin));
        var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
        return Converter.Base64ToBitmap(Converter.BytesToBase64(bytes), 48);
    }
}