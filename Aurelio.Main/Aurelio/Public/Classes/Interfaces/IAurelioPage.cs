using System.ComponentModel;
using Aurelio.Public.Module.Ui.Helper;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioPage : INotifyPropertyChanged
{
    public Control RootElement { get; set; }

    public PageLoadingAnimator InAnimator { get; set; }
    // public PageLoadingAnimator OutAnimator { get; set; }
}