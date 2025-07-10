namespace Aurelio.Public.Classes.Enum;

public class Setting
{
    public enum AccountType
    {
        Offline,
        Microsoft,
        ThirdParty
    }

    public enum CustomBackGroundWay
    {
        Default,
        Image,
        AcrylicBlur,
        Transparent,
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