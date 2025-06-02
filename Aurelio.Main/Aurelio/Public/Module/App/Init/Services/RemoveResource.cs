using System.Collections.Generic;
using System.Linq;
using Avalonia;

namespace Aurelio.Public.Module.App.Init.Services;

public class RemoveResource
{
    public static void Main()
    {
        List<string> list =
        [
            "Colors.axaml",
        ];

        foreach (var item in list)
        {
            // var toRemove = Application.Current.Resources.MergedDictionaries
            //     .FirstOrDefault(d => d.Source?.ToString().Contains(item) == true);
            // if (toRemove != null)
            //     Application.Current.Resources.MergedDictionaries.Remove(toRemove);
        }

     
    }
}