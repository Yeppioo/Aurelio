namespace Aurelio.Plugin.Minecraft.Classes.Enum;

public class Setting
{
    public enum AccountType
    {
        Offline,
        Microsoft,
        ThirdParty
    }
    
    
    public enum WindowVisibility
    {
        AfterLaunchKeepVisible,
        AfterLaunchMakeMinimize,
        AfterLaunchMinimizeAndShowWhenGameExit,
        AfterLaunchHideAndShowWhenGameExit,
        AfterLaunchExit
    }
}