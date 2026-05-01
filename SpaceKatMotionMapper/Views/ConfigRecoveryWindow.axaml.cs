using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public class ConfigRecoveryWindow : UrsaWindow
{
    public bool ShouldReset { get; private set; }

    public ConfigRecoveryWindow(string errorMessage)
    {
        Title = "设备配置错误";
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

        var exitBtn = new Button
        {
            Content = "退出程序",
            Margin = new Thickness(0, 0, 10, 0)
        };
        exitBtn.Click += (_, _) =>
        {
            ShouldReset = false;
            Close();
        };

        var resetBtn = new Button
        {
            Content = "替换为默认配置"
        };
        resetBtn.Click += (_, _) =>
        {
            ShouldReset = true;
            Close();
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { exitBtn, resetBtn }
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
