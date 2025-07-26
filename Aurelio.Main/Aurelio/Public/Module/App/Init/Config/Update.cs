namespace Aurelio.Public.Module.App.Init.Config;

public class Update
{
    public static void Main()
    {
        IO.Local.Setter.ClearFolder(ConfigPath.TempFolderPath);
        AppMethod.SaveSetting();
    }
}