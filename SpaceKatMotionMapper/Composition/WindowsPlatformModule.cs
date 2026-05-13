#if WINDOWS
using Jab;
using PlatformAbstractions;
using SpaceKat.Shared.Services.Contract;
using Win32Helpers.Services.Input;
using Win32Helpers.Windows;
using Win32Helpers.Services.SingletonInstance;
using WindowsInput;
using SpaceKatMotionMapper.Services.Generic;

namespace SpaceKatMotionMapper.Composition;

[ServiceProviderModule]
[Singleton<IPlatformWindowService, WindowsPlatformWindowService>]
[Singleton<IPlatformHotKeyService, WindowsPlatformHotKeyService>]
[Singleton<IPlatformForegroundProgramService, WindowsPlatformForegroundProgramService>]
[Singleton<IPlatformMinimizeService, GenericPlatformMinimizeService>]
[Singleton<IKeyActionExecutor, KeyActionExecutorWindows>]
[Singleton<IInputSimulator, InputSimulator>]
[Singleton<IFileExplorerService, WindowsFileExplorerService>]
[Singleton<ISingletonInstanceService, WindowsSingletonInstanceService>]
public interface IWindowsPlatformModule { }
#endif
