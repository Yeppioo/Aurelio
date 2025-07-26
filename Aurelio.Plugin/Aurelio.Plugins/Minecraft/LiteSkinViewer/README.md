# LiteSkinViewer 🧊
一个轻量级的 Minecraft 皮肤查看器，支持 3D 渲染预览。

## 🧩 技术基础

LiteSkinViewer 构建于 [Coloryr/MinecraftSkinRender](https://github.com/Coloryr/MinecraftSkinRender) 项目的 OpenGL 实现部分基础之上。  
本项目在原始实现的框架上进行了基础封装和部分重写

## 🌟 特性
- 支持皮肤的 3D 模型展示
- 支持皮肤的 2D 全身截取
- 支持皮肤的 2D 头像的多种截取方式（正面脸部，侧面简化样式）

## 🛠️ 构建与运行
确保你的开发环境支持以下内容：
- .NET SDK 8.0 或以上版本
- Avalonia UI 框架
- OpenGL 支持的图形设备

克隆仓库：

```bash
git clone https://github.com/YangSpring429/LiteSkinViewer.git
```

## 📄 版权声明

LiteSkinViewer 使用并改进了 [Coloryr/MinecraftSkinRender](https://github.com/Coloryr/MinecraftSkinRender) 项目中的源代码
该项目遵循 **Apache License 2.0**，因此本项目同样遵守该许可协议的所有要求，包括：

- 明确注明原始项目及其作者：MinecraftSkinRender by Coloryr  
- 提供完整的 LICENSE 文件与 NOTICE 文件（建议添加至仓库根目录）
- 不使用原始作者的商标、Logo 或名称进行暗示性背书

LiteSkinViewer 在尊重原始项目的基础上进行了封装、重构与扩展，所有基于其源代码的部分均遵循 Apache 2.0 协议进行使用和分发
