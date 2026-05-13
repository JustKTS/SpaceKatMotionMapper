using System;
using Jab;
using Serilog;
using MetaKeyPresetsEditor.Composition;
using MetaKeyPresetsEditor.Views;

using ILogger = Serilog.ILogger;

namespace MetaKeyPresetsEditor;

[ServiceProvider]
[Import<IMetaKeyPresetsModule>]
#if WINDOWS
[Import<IMetaKeyPlatformModule_Windows>]
#elif LINUX
[Import<IMetaKeyPlatformModule_Linux>]
#endif
[Singleton(typeof(ILogger), Instance = nameof(Logger))]
[Singleton(typeof(Func<PresetsEditorMainView>), Factory = nameof(CreatePresetsEditorMainViewFactory))]
public partial class MetaKeyPresetsServiceProvider
{
    public ILogger Logger { get; set; } = null!;

    private Func<PresetsEditorMainView> CreatePresetsEditorMainViewFactory()
        => () => GetService<PresetsEditorMainView>();
}
