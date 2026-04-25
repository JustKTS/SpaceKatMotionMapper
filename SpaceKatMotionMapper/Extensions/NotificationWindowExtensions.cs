using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Extensions;

/// <summary>
/// 通知窗口扩展方法，用于获取和管理通知窗口
/// </summary>
public static class NotificationWindowExtensions
{
    /// <summary>
    /// 从WindowNotificationManager获取通知窗口
    /// </summary>
    /// <param name="manager">通知管理器</param>
    /// <returns>通知窗口列表</returns>
    public static IEnumerable<Window> GetNotificationWindows(this WindowNotificationManager? manager)
    {
        if (manager == null)
            return [];

        var result = new List<Window>();

        // 方法1: 尝试通过反射获取内部的通知窗口列表
        try
        {
            var notifications = GetNotificationsViaReflection(manager);
            result.AddRange(notifications.OfType<Window>());
        }
        catch
        {
            // 反射失败，继续尝试其他方法
        }

        // 方法2: 通过视觉树查找通知窗口
        try
        {
            var topLevel = GetTopLevel(manager);
            if (topLevel != null)
            {
                result.AddRange(FindNotificationWindowsInVisualTree(topLevel));
            }
        }
        catch
        {
            // 视觉树查找失败
        }

        // 方法3: 查找所有打开的窗口，筛选出可能是通知的窗口
        try
        {
            // Avalonia 11 中需要通过不同方式获取窗口
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var allWindows = desktop.Windows;
                result.AddRange(allWindows.Where(IsNotificationWindow));
            }
        }
        catch
        {
            // 无法获取窗口列表
        }

        return result.Distinct(); // 去重
    }

    /// <summary>
    /// 异步获取通知窗口（带重试机制）
    /// </summary>
    /// <param name="manager">通知管理器</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <param name="delayMs">重试间隔（毫秒）</param>
    /// <returns>通知窗口列表</returns>
    public static async Task<List<Window>> GetNotificationWindowsAsync(
        this WindowNotificationManager manager,
        int maxRetries = 5,
        int delayMs = 100)
    {
        var notifications = new List<Window>();

        for (var i = 0; i < maxRetries; i++)
        {
            var currentNotifications = manager.GetNotificationWindows().ToList();

            if (currentNotifications.Count > 0)
            {
                notifications.AddRange(currentNotifications);
                break;
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
        }

        return notifications;
    }

    /// <summary>
    /// 通过反射获取通知管理器内部的通知列表
    /// </summary>
    private static IEnumerable<object> GetNotificationsViaReflection(WindowNotificationManager manager)
    {
        // 尝试获取 _notifications 字段
        var notificationsField = typeof(WindowNotificationManager)
            .GetField("_notifications", BindingFlags.NonPublic | BindingFlags.Instance);

        if (notificationsField != null)
        {
            var value = notificationsField.GetValue(manager);
            if (value is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
        }

        // 尝试获取 _items 字段（不同的Ursa版本可能使用不同的字段名）
        var itemsField = typeof(WindowNotificationManager)
            .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        if (itemsField == null) yield break;
        {
            var value = itemsField.GetValue(manager);
            if (value is not System.Collections.IEnumerable enumerable) yield break;
            foreach (var item in enumerable)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// 获取WindowNotificationManager关联的顶级窗口
    /// </summary>
    private static TopLevel? GetTopLevel(WindowNotificationManager manager)
    {
        // 尝试通过反射获取 _owner 字段
        var ownerField = typeof(WindowNotificationManager)
            .GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);

        if (ownerField != null && ownerField.GetValue(manager) is TopLevel owner)
        {
            return owner;
        }

        // 如果无法获取，返回当前应用的主窗口
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }

    /// <summary>
    /// 在视觉树中查找通知窗口
    /// </summary>
    private static IEnumerable<Window> FindNotificationWindowsInVisualTree(TopLevel topLevel)
    {
        // 查找OverlayLayer中的通知窗口
        var overlayLayers = topLevel.GetVisualDescendants()
            .OfType<Control>()
            .Where(c => c.Classes.Contains("overlay-layer"));

        foreach (var overlay in overlayLayers)
        {
            var notifications = overlay.GetVisualDescendants()
                .OfType<Window>()
                .Where(w => IsNotificationWindow(w));

            foreach (var notification in notifications)
            {
                yield return notification;
            }
        }

        // 查找具有通知相关类的窗口
        var notificationWindows = topLevel.GetVisualDescendants()
            .OfType<Window>()
            .Where(w => IsNotificationWindow(w));

        foreach (var window in notificationWindows)
        {
            yield return window;
        }
    }

    /// <summary>
    /// 判断一个窗口是否为通知窗口
    /// </summary>
    private static bool IsNotificationWindow(Window? window)
    {
        if (window == null)
            return false;

        // 检查窗口类型
        var windowTypeName = window.GetType().Name.ToLowerInvariant();
        if (windowTypeName.Contains("notification") ||
            windowTypeName.Contains("toast") ||
            windowTypeName.Contains("popup"))
        {
            return true;
        }

        // 检查窗口属性
        if (!window.ShowInTaskbar &&
            window.Topmost &&
            window.WindowDecorations == WindowDecorations.None)
        {
            return true;
        }

        // 检查窗口标题
        if (!string.IsNullOrEmpty(window.Title))
        {
            var title = window.Title.ToLowerInvariant();
            if (title.Contains("notification") ||
                title.Contains("提示") ||
                title.Contains("通知") ||
                title.Contains("alert"))
            {
                return true;
            }
        }

        // 检查窗口大小（通知窗口通常较小）
        if (window.Width < 500 && window.Height < 200)
        {
            return true;
        }

        // 检查窗口类名
        if (window.Classes.Any(c =>
            c.Contains("notification") ||
            c.Contains("toast") ||
            c.Contains("popup")))
        {
            return true;
        }

        return false;
    }
}