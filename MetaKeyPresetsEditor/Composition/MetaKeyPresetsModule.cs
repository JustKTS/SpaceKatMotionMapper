using Jab;
using MetaKeyPresetsEditor.Services;
using MetaKeyPresetsEditor.ViewModels;
using MetaKeyPresetsEditor.Views;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;

namespace MetaKeyPresetsEditor.Composition;

[ServiceProviderModule]
[Singleton(typeof(ProgramSpecificConfigViewModel))]
[Singleton(typeof(ProgramSpecMainViewModel))]
[Transient(typeof(PresetsEditorMainWindow))]
[Singleton(typeof(PresetsEditorMainView))]
[Transient(typeof(ExistSpecConfigSelectorViewModel))]
[Transient(typeof(ExistPresetSelectorView))]
[Singleton(typeof(IMetaKeyPresetFileService), typeof(MetaKeyPresetFileService))]
[Singleton(typeof(IUiInteractService), typeof(UiInteractService))]
[Singleton(typeof(IPopUpNotificationSpecService), typeof(PopUpNotificationSpecService))]
[Singleton(typeof(CurrentRunningProcessSelectorViewModel))]
public interface IMetaKeyPresetsModule { }
