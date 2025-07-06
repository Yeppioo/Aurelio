using System.ComponentModel;
using Aurelio.Public.Const;
using Aurelio.Public.Module.Ui.Helper;
using Avalonia.Controls;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioPage : INotifyPropertyChanged
{
    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    // public PageLoadingAnimator OutAnimator { get; set; }
    
}