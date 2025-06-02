using Aurelio.ViewModels;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace Aurelio.Public.Module.App.Init.Services;

public class BindKeys
{
    public static void Main()
    {
        var w = Aurelio.App.UiRoot;
        var c = new MoreButtonMenuCommands();
        w.KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+T"),
            Command = new RelayCommand(c.NewTab)
        });
        w.KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+W"),
            Command = new RelayCommand(c.CloseCurrentTab)
        });
    }
}