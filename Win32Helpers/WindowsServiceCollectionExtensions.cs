#if WINDOWS
using Microsoft.Extensions.DependencyInjection;
using PlatformAbstractions;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using Win32Helpers.Services.Input;
using Win32Helpers.Services.SingletonInstance;
using WindowsInput;
using Win32Helpers.Windows;

namespace Win32Helpers;

/// <summary>
/// Windows 平台服务的 DI 注册扩展方法
/// </summary>
public static class WindowsServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformWindowService, WindowsPlatformWindowService>();
        services.AddSingleton<IPlatformHotKeyService, WindowsPlatformHotKeyService>();
        services.AddSingleton<IPlatformForegroundProgramService, WindowsPlatformForegroundProgramService>();
        services.AddSingleton<IPlatformMinimizeService, GenericPlatformMinimizeService>();
        services.AddSingleton<IKeyActionExecutor, KeyActionExecutorWindows>();
        services.AddSingleton<IInputSimulator, InputSimulator>();
        services.AddSingleton<CurrentForeProgramHelper>();
        services.AddSingleton<IFileExplorerService, WindowsFileExplorerService>();
        services.AddSingleton<ISingletonInstanceService, WindowsSingletonInstanceService>();
        return services;
    }
}
#endif
