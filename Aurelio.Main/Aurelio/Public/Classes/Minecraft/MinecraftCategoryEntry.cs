using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aurelio.ViewModels;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Public.Classes.Minecraft;

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
}