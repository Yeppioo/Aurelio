using System.Collections.ObjectModel;
using Aurelio.Public.ViewModels;

namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class MinecraftCategoryEntry : ViewModelBase
{
    private bool _expanded;
    private bool _visible = true;
    public string Name { get; set; }
    public string Tag { get; set; }

    public bool Expanded
    {
        get => _expanded;
        set => SetField(ref _expanded, value);
    }

    public bool Visible
    {
        get => _visible;
        set => SetField(ref _visible, value);
    }

    public ObservableCollection<RecordMinecraftEntry> Minecrafts { get; set; } = [];

    public override bool Equals(object? obj)
    {
        return Tag == ((MinecraftCategoryEntry)obj).Tag && Visible == ((MinecraftCategoryEntry)obj).Visible;
    }
}