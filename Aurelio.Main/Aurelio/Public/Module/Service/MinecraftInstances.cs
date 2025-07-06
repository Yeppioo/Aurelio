using System.Collections.Generic;
using System.Linq;
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
        
    }
}