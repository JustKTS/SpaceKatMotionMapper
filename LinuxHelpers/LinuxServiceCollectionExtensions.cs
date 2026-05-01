using LinuxHelpers.Services.FileExplorer;
using LinuxHelpers.Services.FloatingWindow;
using LinuxHelpers.Services.ForegroundProgram;
using LinuxHelpers.Services.Input;
using LinuxHelpers.Services.Minimize;
using LinuxHelpers.Services.Notification;
using LinuxHelpers.Services.SingletonInstance;
using LinuxHelpers.Services.Window;
using Microsoft.Extensions.DependencyInjection;
using PlatformAbstractions;
using PlatformAbstractions.Unsupported;
using SpaceKat.Shared.Services.Contract;

namespace LinuxHelpers;

/// <summary>
/// Linux 平台服务的 DI 注册扩展方法
/// </summary>
public static class LinuxServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformWindowService, LinuxPlatformWindowService>();
        services.AddSingleton<IPlatformForegroundProgramService, LinuxPlatformForegroundProgramService>();
        services.AddSingleton<IPlatformHotKeyService, UnsupportedPlatformHotKeyService>();
        services.AddSingleton<IKeyActionExecutor, KeyActionExecutorLinux>();
        services.AddSingleton<ILinuxNotificationManager, LinuxNotificationManager>();
        services.AddSingleton<IFloatingControlWindowService, LinuxFloatingControlWindowService>();
        services.AddSingleton<IPlatformMinimizeService, LinuxPlatformMinimizeService>();
        services.AddSingleton<IFileExplorerService, LinuxFileExplorerService>();
        services.AddSingleton<ISingletonInstanceService, LinuxSingletonInstanceService>();
        return services;
    }
}
