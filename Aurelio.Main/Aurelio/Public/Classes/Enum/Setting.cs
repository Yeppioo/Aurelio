namespace Aurelio.Public.Classes.Enum;

public class Setting
{
    public enum AccountType
    {
        Offline,
        Microsoft,
        ThirdParty
    }
    
    public enum LaunchPage
    {
        NewTab,
        MinecraftInstance,
        Setting
    }
    
    public enum WindowVisibility
    {
        AfterLaunchKeepVisible,
        AfterLaunchMakeMinimize,
        AfterLaunchMinimizeAndShowWhenGameExit,
        AfterLaunchHideAndShowWhenGameExit,
        AfterLaunchExit
    }

    public enum BackGround
    {
        Default,
        Image,
        AcrylicBlur,
        Transparent,
        ColorBlock,
        Mica
    }

    public enum NoticeWay
    {
        Bubble,
        Card
    }

    public enum OpenFileWay
    {
        FileSelectWindow,
        ManualInput
    }

    public enum Theme
    {
        System,
        Light,
        Dark
    }

    public enum WindowTitleBarStyle
    {
        System,
        Aurelio,
        Unset
    }
}