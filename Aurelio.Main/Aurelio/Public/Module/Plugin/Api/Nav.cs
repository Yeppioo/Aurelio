using Aurelio.Public.Classes.Interfaces;
using Avalonia.Layout;
using Avalonia.Media;
using Ursa.Controls;

namespace Aurelio.Public.Module.Plugin.Api;

public static class Nav
{
    public static void AddTab(this SelectionList list, string name, StreamGeometry icon
        , IAurelioPage page, int index = -1)
    { 
        var item = new SelectionListItem
        {
            Tag = page,
            Content = new DockPanel
            {
                Children =
                {
                    new PathIcon
                    {
                        Data = icon,
                        Height = 16,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 15
                    },
                    new TextBlock
                    {
                        Text = name
                    }
                }
            },
        };
        if (index == -1)
        {
            list.Items.Add(item);
        }
        else
        {
            list.Items.Insert(index, item);
        }
    }
}