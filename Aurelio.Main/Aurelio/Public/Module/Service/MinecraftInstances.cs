using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Views.Main.Pages;
using Avalonia.Threading;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;

namespace Aurelio.Public.Module.Service;

public class MinecraftInstances
{
    public static async void Load(string[] path)
    {
        List<MinecraftEntry> minecrafts = [];
        foreach (var p in path)
        {
            var parser = new MinecraftParser(p);
            minecrafts.AddRange(parser.GetMinecrafts());
        }

        minecrafts = minecrafts.OrderBy(x => x.Id).ToList();
        Data.MinecraftInstances.Add(new MinecraftCategoryEntry
        {
            Name = MainLang.SearchResult,
            Tag = "filtered",
            Visible = false
        });
        Data.MinecraftInstances.Add(new MinecraftCategoryEntry
        {
            Name = MainLang.AllInstance, Tag = "all", Expanded = true,
            Minecrafts = new ObservableCollection<RecordMinecraftEntry>
                (minecrafts.Select(x => new RecordMinecraftEntry(x)).ToList())
        });
        PageInstance.HomeTabPage.Search(false);
    }
}