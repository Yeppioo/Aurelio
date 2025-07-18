using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Enum.Minecraft;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Minecraft;

public class MinecraftInstanceSettingEntry : ReactiveObject
{
    [Reactive][JsonProperty] public DateTime LastPlayed { get; set; } = DateTime.MinValue;
    [Reactive][JsonProperty] public MinecraftInstanceIconType IconType { get; set; } = MinecraftInstanceIconType.Auto;
    [Reactive][JsonProperty] public double MemoryLimit { get; set; } = -1;
    [Reactive][JsonProperty] public int EnableIndependentMinecraft { get; set; }
    [Reactive][JsonProperty] public string AutoJoinServerAddress { get; set; }

    [Reactive]
    [JsonProperty]
    public RecordJavaRuntime JavaRuntime { get; set; } = new()
    {
        JavaVersion = "global"
    };

    [Reactive][JsonProperty] public string IconData { get; set; }
    [JsonProperty] public ObservableCollection<string> Tags { get; } = [];

    // 收藏夹状态 - 独立的布尔属性，不再使用特殊标签
    [Reactive][JsonProperty] public bool IsFavourite { get; set; }

    // 添加标签方法，防止重复添加
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag)) Tags.Add(tag);
    }

    // 清除重复标签方法
    public void RemoveDuplicateTags()
    {
        var uniqueTags = Tags.Distinct().ToList();
        if (uniqueTags.Count != Tags.Count)
        {
            Tags.Clear();
            foreach (var tag in uniqueTags) Tags.Add(tag);
        }
    }

    // 验证标签名称是否有效
    public bool IsValidTagName(string tagName)
    {
        return !string.IsNullOrWhiteSpace(tagName);
    }
}