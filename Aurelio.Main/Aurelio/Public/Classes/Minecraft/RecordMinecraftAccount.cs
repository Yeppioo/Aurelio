using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aurelio.Public.Module.Value;
using Avalonia.Media.Imaging;
using LiteSkinViewer2D;
using LiteSkinViewer2D.Extensions;
using Newtonsoft.Json;
using SkiaSharp;

namespace Aurelio.Public.Classes.Minecraft;

public sealed record RecordMinecraftAccount : INotifyPropertyChanged
{
    [JsonIgnore] private Bitmap? _body;
    [JsonIgnore] private Bitmap? _head;
    public Enum.Setting.AccountType AccountType { get; set; }
    public string Name { get; set; } = "Unnamed";
    public string UUID { get; set; }
    [JsonIgnore] public string FormatLastUsedTime => Calculator.FormatUsedTime(LastUsedTime);
    public DateTime LastUsedTime { get; set; } = DateTime.MinValue;
    public DateTime AddTime { get; set; } = DateTime.MinValue;
    public string? Data { get; set; }

    public string Skin { get; set; } =
        @"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";

    [JsonIgnore]
    public Bitmap Head
    {
        get { return _head ??= HandleHeadSkin(); }
    }

    [JsonIgnore]
    public Bitmap Body
    {
        get { return _body ??= HandleBodySkin(); }
    }


    public bool Equals(RecordMinecraftAccount? other)
    {
        return other != null && Skin == other.Skin && Name == other.Name && Data == other.Data;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private Bitmap HandleBodySkin()
    {
        var imageBytes = Convert.FromBase64String(Skin);
        return FullBodyCapturer.Default.Capture(SKBitmap.Decode(imageBytes)).ToBitmap();
    }

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
        var imageBytes = Convert.FromBase64String(Skin);
        return HeadCapturer.Default.Capture(SKBitmap.Decode(imageBytes)).ToBitmap(48);
    }
}