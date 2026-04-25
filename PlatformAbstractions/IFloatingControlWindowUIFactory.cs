namespace PlatformAbstractions;

/// <summary>
/// 浮动控制窗口 UI 工厂接口
/// 用于解耦服务层与 UI 层的依赖关系
/// </summary>
public interface IFloatingControlWindowUIFactory
{
    /// <summary>
    /// 创建浮动控制窗口实例
    /// </summary>
    /// <returns>浮动控制窗口对象</returns>
    object CreateFloatingControlWindow();

    /// <summary>
    /// 设置窗口的防平铺属性（仅适用于Linux/X11）
    /// </summary>
    /// <param name="window">窗口对象</param>
    void SetAntiTilingProperties(object window);
}
