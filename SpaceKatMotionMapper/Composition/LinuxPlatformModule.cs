#if LINUX
using Jab;
using PlatformAbstractions;
using PlatformAbstractions.Unsupported;
using SpaceKat.Shared.Services.Contract;
using LinuxHelpers.Services.Window;
using LinuxHelpers.Services.ForegroundProgram;
using LinuxHelpers.Services.Input;
using LinuxHelpers.Services.Minimize;
using LinuxHelpers.Services.FileExplorer;
using LinuxHelpers.Services.SingletonInstance;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Composition;

[ServiceProviderModule]
[Singleton<IPlatformWindowService, LinuxPlatformWindowService>]
[Singleton<IPlatformForegroundProgramService, LinuxPlatformForegroundProgramService>]
[Singleton<IPlatformHotKeyService, UnsupportedPlatformHotKeyService>]
[Singleton<IKeyActionExecutor, KeyActionExecutorLinux>]
[Singleton<IPlatformMinimizeService, LinuxPlatformMinimizeService>]
[Singleton<IFileExplorerService, LinuxFileExplorerService>]
[Singleton<ISingletonInstanceService, LinuxSingletonInstanceService>]
[Singleton<IPlatformAutostartService, UnsupportedAutostartService>]
public interface ILinuxPlatformModule { }
#endif
