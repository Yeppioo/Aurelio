using System.Collections.Generic;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public class FontFamilyChineseNameTable
{
    // 常见字体中英文对照表
    public static readonly Dictionary<string, string> FontNameMap = new()
    {
        { "Microsoft YaHei", "微软雅黑" },
        { "SimSun", "宋体" },
        { "SimHei", "黑体" },
        { "NSimSun", "新宋体" },
        { "FangSong", "仿宋" },
        { "KaiTi", "楷体" },
        { "DengXian", "等线" },
        { "DengXian Light", "等线 Light" },
        { "DengXian Bold", "等线 Bold" },
        { "DengXian Demibold", "等线 DemiBold" },
        { "PingFang SC", "苹方-简" },
        { "PingFang TC", "苹方-繁" },
        { "Source Han Sans SC", "思源黑体-简" },
        { "Source Han Sans TC", "思源黑体-繁" },
        { "Source Han Serif SC", "思源宋体-简" },
        { "Source Han Serif TC", "思源宋体-繁" },
        { "STHeiti", "华文黑体" },
        { "STKaiti", "华文楷体" },
        { "STFangsong", "华文仿宋" },
        { "STSong", "华文宋体" },
        { "STZhongsong", "华文中宋" },
        { "STCaiyun", "华文彩云" },
        { "STHupo", "华文琥珀" },
        { "STLiti", "华文隶书" },
        { "STXingkai", "华文行楷" },
        { "STXinwei", "华文新魏" },
        { "STXihei", "华文细黑" },
        { "STYuan", "华文圆体" }
    };
}