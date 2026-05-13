#if LINUX
using Jab;
using PlatformAbstractions;
using LinuxHelpers.Services.FileExplorer;
using LinuxHelpers.Services.Window;

namespace MetaKeyPresetsEditor.Composition;

[ServiceProviderModule]
[Singleton<IFileExplorerService, LinuxFileExplorerService>]
[Singleton<IPlatformWindowService, LinuxPlatformWindowService>]
public interface IMetaKeyPlatformModule_Linux { }
#endif
