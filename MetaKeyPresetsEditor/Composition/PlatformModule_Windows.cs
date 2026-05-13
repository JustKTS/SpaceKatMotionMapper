#if WINDOWS
using Jab;
using PlatformAbstractions;
using Win32Helpers.Windows;

namespace MetaKeyPresetsEditor.Composition;

[ServiceProviderModule]
[Singleton<IPlatformWindowService, WindowsPlatformWindowService>]
[Singleton<IFileExplorerService, WindowsFileExplorerService>]
public interface IMetaKeyPlatformModule_Windows { }
#endif
