namespace Aurelio.Public.Classes.Enum;

public class Setting
{
    public enum Theme
    {
        System,
        Light,
        Dark
    }

    public enum OpenFileWay
    {
        FileSelectWindow,
        ManualInput
    }

    public enum WindowTitleBarStyle
    {
        System,
        Aurelio,
        Unset
    }
    
    public enum CustomBackGroundWay
    {
        Default,
        Image,
        AcrylicBlur,
        Transparent,
        Mica
    }

    public enum AccountType
    {
        Offline,
        Microsoft,
        ThirdParty
    }

    public enum NoticeWay
    {
        Bubble,
        Card
    }
}