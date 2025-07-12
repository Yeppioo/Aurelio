using Aurelio.Public.Module.Ui.Helper;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioPage
{
    public Control RootElement { get; set; }

    public PageLoadingAnimator InAnimator { get; set; }
}