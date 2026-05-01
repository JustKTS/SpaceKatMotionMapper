using System;

namespace PlatformAbstractions;

/// <summary>
/// 单例实例服务接口，确保同一时间只有一个应用实例在运行。
/// Linux 平台使用文件锁实现，Windows 平台使用命名 Mutex 实现。
/// </summary>
public interface ISingletonInstanceService : IDisposable
{
    /// <summary>
    /// 尝试获取单例所有权。
    /// </summary>
    /// <returns>true 表示当前是唯一实例；false 表示已有其他实例在运行。</returns>
    bool TryAcquire();
}
