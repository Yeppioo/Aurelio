using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        // 加载插件程序集
        var assembly = Assembly.LoadFrom(@"Aurelio.Plugin.Minecraft\bin\Debug\net8.0\Aurelio.Plugin.Minecraft.dll");
        
        Console.WriteLine("插件程序集中的所有嵌入资源:");
        var resourceNames = assembly.GetManifestResourceNames();
        
        foreach (var resourceName in resourceNames)
        {
            Console.WriteLine($"- {resourceName}");
        }
        
        Console.WriteLine($"\n总共找到 {resourceNames.Length} 个嵌入资源");
        
        // 测试特定资源是否存在
        string targetResource = "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png";
        bool resourceExists = Array.Exists(resourceNames, name => name == targetResource);
        
        Console.WriteLine($"\n目标资源 '{targetResource}' 是否存在: {resourceExists}");
        
        if (!resourceExists)
        {
            Console.WriteLine("\n可能的匹配资源:");
            foreach (var resourceName in resourceNames)
            {
                if (resourceName.Contains("grass_block_side") || resourceName.Contains("McIcons"))
                {
                    Console.WriteLine($"- {resourceName}");
                }
            }
        }
    }
}
