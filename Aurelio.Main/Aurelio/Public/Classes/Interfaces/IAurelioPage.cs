using Aurelio.Public.Module.Ui.Helper;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioPage
{
    public string ShortInfo { get; set; } 
    public Control BottomElement { get; set; } 
    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}