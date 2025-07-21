using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Avalonia.Interactivity;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace Aurelio.Views.Main.Pages.Viewers;

/// <summary>
/// JSON可视化查看器
///
/// 使用示例：
///
/// 1. 基本构造函数：
///    var viewer = new JsonViewer("JSON查看器");
///
/// 2. 从文件创建：
///    var viewer = new JsonViewer(@"C:\data\sample.json", "示例JSON");
///
/// 3. 从JSON字符串创建：
///    var jsonString = "{\"name\":\"test\",\"value\":123}";
///    var viewer = new JsonViewer(jsonString, "测试JSON", true);
///
/// 4. 使用静态工厂方法：
///    var viewer1 = JsonViewer.FromFile(@"C:\data\sample.json");
///    var viewer2 = JsonViewer.FromJsonString(jsonString, "测试JSON");
/// </summary>
/// <summary>
/// JSON树节点视图模型
/// </summary>
public class JsonNodeViewModel : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isSelected;

    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public ObservableCollection<JsonNodeViewModel> Children { get; set; } = new();
    public bool HasChildren => Children.Count > 0;
    public bool IsNestedJson { get; set; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string DisplayText
    {
        get
        {
            if (IsNestedJson)
                return $"{Name}: 嵌套JSON";

            if (Type == "Array")
                return $"{Name}: Array [{Children.Count} items]";

            if (Type == "Object")
                return $"{Name}: Object [{Children.Count} properties]";

            if (string.IsNullOrEmpty(Name))
                return Value;

            return $"{Name}: {Value}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(
        [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class JsonViewer : PageMixModelBase, IAurelioTabPage
{
    private string _rawJsonText = string.Empty;
    private string _rootClassName = "RootClass";
    private string _namespaceName = "Generated";
    private string _generatedCSharpCode = string.Empty;
    private string _errorMessage = string.Empty;
    private JsonNodeViewModel? _selectedNode;
    private string _nodeDetails = string.Empty;
    private bool _isWordWrapEnabled = true;

    public JsonViewer(string title)
    {
        InitializeComponent();
        PageInfo = new PageInfoEntry
        {
            Title = title,
            Icon = StreamGeometry.Parse(
                "M192 0c-41.8 0-77.4 26.7-90.5 64L64 64C28.7 64 0 92.7 0 128L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64l-37.5 0C269.4 26.7 233.8 0 192 0zm0 64a32 32 0 1 1 0 64 32 32 0 1 1 0-64zM72 272a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm104-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zM72 368a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm88 0c0-8.8 7.2-16 16-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16z")
        };
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        // 直接使用自身作为数据上下文
        DataContext = this;

        // 在UI加载后初始化编辑器
        Loaded += (_, _) => InitializeEditors();
    }

    /// <summary>
    /// 从文件路径创建JSON查看器
    /// </summary>
    /// <param name="filePath">JSON文件路径</param>
    /// <param name="title">标签页标题</param>
    public JsonViewer(string filePath, string title) : this(title)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var jsonContent = File.ReadAllText(filePath);
                RawJsonText = jsonContent;

                // 自动解析JSON
                ParseJsonInternal();
            }
            else
            {
                ErrorMessage = $"文件不存在: {filePath}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"读取文件失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 从JSON字符串创建JSON查看器
    /// </summary>
    /// <param name="jsonContent">JSON字符串内容</param>
    /// <param name="title">标签页标题</param>
    /// <param name="autoParseJson">是否自动解析JSON</param>
    public JsonViewer(string jsonContent, string title, bool autoParseJson) : this(title)
    {
        if (!string.IsNullOrEmpty(jsonContent))
        {
            RawJsonText = jsonContent;

            if (autoParseJson)
            {
                // 自动解析JSON
                ParseJsonInternal();
            }
        }
    }

    /// <summary>
    /// 静态工厂方法：从文件创建JSON查看器
    /// </summary>
    /// <param name="filePath">JSON文件路径</param>
    /// <param name="title">标签页标题，如果为空则使用文件名</param>
    /// <returns>JsonViewer实例</returns>
    public static JsonViewer FromFile(string filePath, string? title = null)
    {
        var displayTitle = title ?? Path.GetFileName(filePath);
        return new JsonViewer(filePath, displayTitle);
    }

    /// <summary>
    /// 静态工厂方法：从JSON字符串创建查看器
    /// </summary>
    /// <param name="jsonContent">JSON字符串内容</param>
    /// <param name="title">标签页标题</param>
    /// <param name="autoParseJson">是否自动解析JSON，默认为true</param>
    /// <returns>JsonViewer实例</returns>
    public static JsonViewer FromJsonString(string jsonContent, string title, bool autoParseJson = true)
    {
        return new JsonViewer(jsonContent, title, autoParseJson);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public ObservableCollection<JsonNodeViewModel> JsonTreeNodes { get; } = new();

    // AvaloniaEdit文档属性
    public TextDocument JsonDocument { get; private set; } = new();
    public TextDocument CSharpDocument { get; private set; } = new();

    public string RawJsonText
    {
        get => _rawJsonText;
        set
        {
            SetField(ref _rawJsonText, value);
            JsonDocument.Text = value;
        }
    }

    public bool IsWordWrapEnabled
    {
        get => _isWordWrapEnabled;
        set => SetField(ref _isWordWrapEnabled, value);
    }

    public string RootClassName
    {
        get => _rootClassName;
        set => SetField(ref _rootClassName, value);
    }

    public string NamespaceName
    {
        get => _namespaceName;
        set => SetField(ref _namespaceName, value);
    }

    public string GeneratedCSharpCode
    {
        get => _generatedCSharpCode;
        set
        {
            SetField(ref _generatedCSharpCode, value);
            CSharpDocument.Text = value;
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetField(ref _errorMessage, value);
    }

    public JsonNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            SetField(ref _selectedNode, value);
            UpdateNodeDetails();
        }
    }

    public string NodeDetails
    {
        get => _nodeDetails;
        set => SetField(ref _nodeDetails, value);
    }

    public void OnClose()
    {
    }

    /// <summary>
    /// 初始化编辑器
    /// </summary>
    private void InitializeEditors()
    {
        try
        {
            JsonDocument.TextChanged += (_, _) =>
            {
                if (_rawJsonText != JsonDocument.Text)
                {
                    SetField(ref _rawJsonText, JsonDocument.Text, nameof(RawJsonText));
                }
            };

            // 设置编辑器语法高亮和主题
            SetupEditor(JsonEditor, ".json");
            SetupEditor(CSharpEditor, ".cs");
        }
        catch
        {
            // 忽略编辑器设置错误
        }
    }

    /// <summary>
    /// 设置编辑器的语法高亮和链接颜色
    /// </summary>
    /// <param name="editor">编辑器实例</param>
    /// <param name="language">语言类型</param>
    private void SetupEditor(AvaloniaEdit.TextEditor? editor, string language)
    {
        if (editor == null) return;

        try
        {
            // 设置语法高亮
            var _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

            //Initial setup of TextMate.
            var _textMateInstallation = editor.InstallTextMate(_registryOptions);

            //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
            //And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
            _textMateInstallation.SetGrammar(
                _registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(language).Id));

            // 设置链接颜色为#54A9FF
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromRgb(84, 169, 255));
        }
        catch
        {
            // 忽略编辑器设置错误
        }
    }


    // Event handlers for toolbar buttons
    public void ParseJson(object? sender, RoutedEventArgs e)
    {
        ParseJsonInternal();
    }

    /// <summary>
    /// 内部JSON解析方法，可以被构造函数调用
    /// </summary>
    private void ParseJsonInternal()
    {
        try
        {
            ErrorMessage = string.Empty;
            JsonTreeNodes.Clear();

            if (string.IsNullOrWhiteSpace(RawJsonText))
            {
                ErrorMessage = "请输入JSON文本";
                return;
            }

            var jsonObject = JToken.Parse(RawJsonText);
            var rootNode = CreateJsonNode("root", jsonObject, "root");
            JsonTreeNodes.Add(rootNode);
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"JSON解析错误: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"解析失败: {ex.Message}";
        }
    }

    public void FormatJson(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(RawJsonText))
                return;

            var jsonObject = JToken.Parse(RawJsonText);
            RawJsonText = jsonObject.ToString(Newtonsoft.Json.Formatting.Indented);
            ErrorMessage = string.Empty;
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"JSON格式化错误: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"格式化失败: {ex.Message}";
        }
    }

    public void ClearAll(object? sender, RoutedEventArgs e)
    {
        RawJsonText = string.Empty;
        JsonTreeNodes.Clear();
        GeneratedCSharpCode = string.Empty;
        ErrorMessage = string.Empty;
        SelectedNode = null;
        NodeDetails = string.Empty;
    }

    public void ExpandAll(object? sender, RoutedEventArgs e)
    {
        foreach (var node in JsonTreeNodes)
        {
            ExpandNodeRecursively(node);
        }
    }

    public void CollapseAll(object? sender, RoutedEventArgs e)
    {
        foreach (var node in JsonTreeNodes)
        {
            CollapseNodeRecursively(node);
        }
    }

    public void GenerateCSharpClass(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (JsonTreeNodes.Count == 0)
            {
                ErrorMessage = "请先解析JSON";
                return;
            }

            var csharpCode = GenerateCSharpFromJson();
            GeneratedCSharpCode = csharpCode;
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"C#代码生成失败: {ex.Message}";
        }
    }

    private void ExpandNodeRecursively(JsonNodeViewModel node)
    {
        node.IsExpanded = true;
        foreach (var child in node.Children)
        {
            ExpandNodeRecursively(child);
        }
    }

    private void CollapseNodeRecursively(JsonNodeViewModel node)
    {
        node.IsExpanded = false;
        foreach (var child in node.Children)
        {
            CollapseNodeRecursively(child);
        }
    }

    private void UpdateNodeDetails()
    {
        if (SelectedNode == null)
        {
            NodeDetails = "未选择节点";
            return;
        }

        var details = new StringBuilder();
        details.AppendLine($"节点名称: {SelectedNode.Name}");
        details.AppendLine($"节点类型: {SelectedNode.Type}");
        details.AppendLine($"路径: {SelectedNode.Path}");

        if (!string.IsNullOrEmpty(SelectedNode.Value))
            details.AppendLine($"值: {SelectedNode.Value}");

        if (SelectedNode.Type == "Array")
            details.AppendLine($"数组元素数量: {SelectedNode.Children.Count}");
        else if (SelectedNode.Type == "Object")
            details.AppendLine($"对象属性数量: {SelectedNode.Children.Count}");

        if (SelectedNode.IsNestedJson)
            details.AppendLine("包含嵌套JSON");

        NodeDetails = details.ToString();
    }

    private JsonNodeViewModel CreateJsonNode(string name, JToken token, string path)
    {
        var node = new JsonNodeViewModel
        {
            Name = name,
            Path = path
        };

        switch (token.Type)
        {
            case JTokenType.Object:
                node.Type = "Object";
                node.Value = $"Object [{((JObject)token).Count} properties]";
                foreach (var property in ((JObject)token).Properties())
                {
                    var childPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                    var childNode = CreateJsonNode(property.Name, property.Value, childPath);
                    node.Children.Add(childNode);
                }

                break;

            case JTokenType.Array:
                node.Type = "Array";
                var array = (JArray)token;
                node.Value = $"Array [{array.Count} items]";
                for (int i = 0; i < array.Count; i++)
                {
                    var childPath = $"{path}[{i}]";
                    var childNode = CreateJsonNode($"[{i}]", array[i], childPath);
                    node.Children.Add(childNode);
                }

                break;

            case JTokenType.String:
                node.Type = "String";
                var stringValue = token.Value<string>() ?? string.Empty;
                node.Value = $"\"{stringValue}\"";

                // 检测嵌套JSON
                if (IsValidJson(stringValue))
                {
                    try
                    {
                        var nestedToken = JToken.Parse(stringValue);
                        node.IsNestedJson = true;
                        var nestedNode = CreateJsonNode("嵌套JSON", nestedToken, $"{path}.嵌套JSON");
                        node.Children.Add(nestedNode);
                    }
                    catch
                    {
                        // 忽略嵌套JSON解析错误
                    }
                }

                break;

            case JTokenType.Integer:
                node.Type = "Integer";
                node.Value = token.Value<long>().ToString();
                break;

            case JTokenType.Float:
                node.Type = "Float";
                node.Value = token.Value<double>().ToString(CultureInfo.InvariantCulture);
                break;

            case JTokenType.Boolean:
                node.Type = "Boolean";
                node.Value = token.Value<bool>().ToString().ToLower();
                break;

            case JTokenType.Null:
                node.Type = "Null";
                node.Value = "null";
                break;

            default:
                node.Type = token.Type.ToString();
                node.Value = token.ToString();
                break;
        }

        return node;
    }

    private bool IsValidJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
            return false;

        text = text.Trim();
        return (text.StartsWith("{") && text.EndsWith("}")) ||
               (text.StartsWith("[") && text.EndsWith("]"));
    }

    private string GenerateCSharpFromJson()
    {
        if (JsonTreeNodes.Count == 0)
            return string.Empty;

        var rootNode = JsonTreeNodes[0];
        var classes = new Dictionary<string, StringBuilder>();
        var classNames = new HashSet<string>();

        // 生成主类
        GenerateClassFromNode(rootNode, RootClassName, classes, classNames);

        // 构建完整的C#代码
        var result = new StringBuilder();
        result.AppendLine("using System;");
        result.AppendLine("using System.Collections.Generic;");
        result.AppendLine("using Newtonsoft.Json;");
        result.AppendLine();
        result.AppendLine($"namespace {NamespaceName}");
        result.AppendLine("{");

        foreach (var kvp in classes)
        {
            result.Append(kvp.Value.ToString());
            result.AppendLine();
        }

        result.AppendLine("}");

        return result.ToString();
    }

    private void GenerateClassFromNode(JsonNodeViewModel node, string className,
        Dictionary<string, StringBuilder> classes, HashSet<string> classNames)
    {
        if (node.Type != "Object")
            return;

        // 确保类名唯一
        var uniqueClassName = GetUniqueClassName(className, classNames);
        classNames.Add(uniqueClassName);

        var classBuilder = new StringBuilder();
        classBuilder.AppendLine($"    public class {uniqueClassName}");
        classBuilder.AppendLine("    {");

        foreach (var child in node.Children)
        {
            var propertyName = ToPascalCase(child.Name);
            var propertyType = GetCSharpType(child);
            var defaultValue = GetDefaultValue(child);
            var isNullable = child.Type == "Null" || ShouldBeNullable(child);

            // 添加注释说明
            if (!string.IsNullOrEmpty(child.Value) && child.Type != "Object" && child.Type != "Array")
            {
                classBuilder.AppendLine($"        /// <summary>");
                classBuilder.AppendLine($"        /// 示例值: {child.Value}");
                classBuilder.AppendLine($"        /// </summary>");
            }

            classBuilder.AppendLine($"        [JsonProperty(\"{child.Name}\")]");

            // 添加默认值
            if (!string.IsNullOrEmpty(defaultValue))
            {
                classBuilder.AppendLine(
                    $"        public {propertyType} {propertyName} {{ get; set; }} = {defaultValue};");
            }
            else
            {
                classBuilder.AppendLine($"        public {propertyType} {propertyName} {{ get; set; }}");
            }

            classBuilder.AppendLine();

            // 递归生成嵌套类
            if (child.Type == "Object")
            {
                var nestedClassName = ToPascalCase(child.Name) + "Class";
                GenerateClassFromNode(child, nestedClassName, classes, classNames);
            }
            else if (child.Type == "Array" && child.Children.Count > 0)
            {
                var firstElement = child.Children[0];
                if (firstElement.Type == "Object")
                {
                    var elementClassName = ToPascalCase(child.Name) + "Item";
                    GenerateClassFromNode(firstElement, elementClassName, classes, classNames);
                }
            }
        }

        classBuilder.AppendLine("    }");
        classes[uniqueClassName] = classBuilder;
    }

    private string GetCSharpType(JsonNodeViewModel node)
    {
        return node.Type switch
        {
            "String" => "string",
            "Integer" => "long",
            "Float" => "double",
            "Boolean" => "bool",
            "Null" => "object?",
            "Object" => ToPascalCase(node.Name) + "Class",
            "Array" => GetArrayType(node),
            _ => "object"
        };
    }

    private string GetDefaultValue(JsonNodeViewModel node)
    {
        return node.Type switch
        {
            "String" => "string.Empty",
            "Integer" => "0",
            "Float" => "0.0",
            "Boolean" => node.Value.ToLower() == "true" ? "true" : "false",
            "Array" => $"new {GetArrayType(node)}()",
            "Object" => $"new {ToPascalCase(node.Name)}Class()",
            _ => ""
        };
    }

    private bool ShouldBeNullable(JsonNodeViewModel node)
    {
        // 如果值为null或者是可选字段，则应该是nullable
        return node.Type == "Null" || node.Value == "null";
    }

    private string GetArrayType(JsonNodeViewModel arrayNode)
    {
        if (arrayNode.Children.Count == 0)
            return "List<object>";

        var firstElement = arrayNode.Children[0];
        var elementType = firstElement.Type switch
        {
            "String" => "string",
            "Integer" => "long",
            "Float" => "double",
            "Boolean" => "bool",
            "Object" => ToPascalCase(arrayNode.Name) + "Item",
            _ => "object"
        };

        return $"List<{elementType}>";
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "Property";

        // 移除特殊字符并转换为PascalCase
        var cleaned = Regex.Replace(input, @"[^a-zA-Z0-9]", " ");
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var result = new StringBuilder();
        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                    result.Append(word.Substring(1).ToLower());
            }
        }

        var pascalCase = result.ToString();

        // 确保以字母开头
        if (string.IsNullOrEmpty(pascalCase) || !char.IsLetter(pascalCase[0]))
            pascalCase = "Property" + pascalCase;

        return pascalCase;
    }

    private string GetUniqueClassName(string baseName, HashSet<string> existingNames)
    {
        var uniqueName = baseName;
        var counter = 1;

        while (existingNames.Contains(uniqueName))
        {
            uniqueName = $"{baseName}{counter}";
            counter++;
        }

        return uniqueName;
    }
}