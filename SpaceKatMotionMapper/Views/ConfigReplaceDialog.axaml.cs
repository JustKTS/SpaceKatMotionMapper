using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public class ConfigReplaceDialog : UrsaWindow
{
    public bool ShouldReplace { get; private set; }

    public ConfigReplaceDialog(string errorMessage)
    {
        Title = "配置文件错误";
        Width = 480;
        Height = 220;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var titleBlock = new TextBlock
        {
            Text = "设备配置文件格式错误",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var messageBlock = new TextBlock
        {
            Text = errorMessage,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.85,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var questionBlock = new TextBlock
        {
            Text = "是否使用内置默认配置替换损坏的文件?",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 15)
        };

        var cancelBtn = new Button
        {
            Content = "保持当前设置",
            Margin = new Thickness(0, 0, 10, 0)
        };
        cancelBtn.Click += (_, _) =>
        {
            ShouldReplace = false;
            Close();
        };

        var replaceBtn = new Button
        {
            Content = "使用内置替换"
        };
        replaceBtn.Click += (_, _) =>
        {
            ShouldReplace = true;
            Close();
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { cancelBtn, replaceBtn }
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 6
        };
        panel.Children.Add(titleBlock);
        panel.Children.Add(messageBlock);
        panel.Children.Add(questionBlock);
        panel.Children.Add(buttonPanel);

        Content = new Border
        {
            Child = panel,
            Padding = new Thickness(4)
        };
    }
}
