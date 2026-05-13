using System;
using Jab;
using Serilog;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.NavVMs;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using PlatformAbstractions;
using MetaKeyPresetsEditor.Composition;
using MetaKeyPresetsEditor.Views;

using ILogger = Serilog.ILogger;

namespace SpaceKatMotionMapper.Composition;

[ServiceProvider]
#if WINDOWS
[Import<IWindowsPlatformModule>]
#endif
#if LINUX
[Import<ILinuxPlatformModule>]
#endif
[Import<IMetaKeyPresetsModule>]
[Singleton(typeof(ILogger), Instance = nameof(Logger))]
[Singleton(typeof(LocalSettingsOptions), Factory = nameof(CreateLocalSettingsOptions))]
[Singleton(typeof(Func<TransparentInfoWindow>), Factory = nameof(CreateTransparentWindowFactory))]
[Singleton(typeof(Func<TransparentInfoViewModel>), Factory = nameof(CreateTransparentVMFactory))]
[Singleton(typeof(Func<PresetsEditorMainView>), Factory = nameof(CreatePresetsEditorMainViewFactory))]

// Views
[Singleton(typeof(MainWindow))]
[Singleton(typeof(MainView))]
[Singleton(typeof(SettingsView))]
[Singleton(typeof(KatMotionTimeConfigView))]
[Singleton(typeof(DeadZoneConfigView))]
[Transient(typeof(KatMotionGroupConfigWindow))]
[Transient(typeof(TransparentInfoWindow))]
[Transient(typeof(FavPresetsEditorView))]
[Transient(typeof(FirstDownloadPresetsView))]

// ViewModels
[Singleton(typeof(NavViewModel))]
[Singleton(typeof(MainViewModel))]
[Singleton(typeof(SettingsViewModel))]
[Singleton(typeof(TransparentInfoViewModel))]
[Singleton(typeof(ListeningInfoViewModel))]
[Singleton(typeof(ConnectAndEnableViewModel))]
[Singleton(typeof(AutoDisableViewModel))]
[Singleton(typeof(RunningProgramSelectorViewModel))]
[Singleton(typeof(TimeAndDeadZoneSettingViewModel))]
[Singleton(typeof(MotionTimeConfigViewModel))]
[Singleton(typeof(DeadZoneConfigViewModel))]
[Singleton(typeof(CommonConfigViewModel))]
[Transient(typeof(KatMotionConfigViewModel))]
[Singleton(typeof(OtherConfigsViewModel))]
[Transient(typeof(FavPresetsEditorViewModel))]
[Transient(typeof(FirstDownloadPresetsViewModel))]

// Core services
[Singleton<IDeviceDataWrapper, SpaceDeviceDataWrapper>]
[Singleton<IStorageProviderService, StorageProviderService>]
[Singleton<ILocalSettingsService, LocalSettingsService>]
[Singleton<IFileService, FileService>]
[Singleton<IKatMotionFileService, KatMotionFileService>]
[Singleton<IPopUpNotificationService, PopUpNotificationService>]
[Singleton<IKatMotionActivateService, KatMotionActivateService>]
[Singleton<IActivationStatusService, ActivationStatusService>]
[Singleton<IKatMotionConfigVMManageService, KatMotionConfigVMManageService>]
[Singleton<IOfficialMapperHotKeyService, OfficialMapperHotKeyService>]

[Singleton<IKatMotionRecognizeService, KatMotionRecognizeService>]
[Singleton<ITransparentInfoActionDisplayService, TransparentInfoActionDisplayService>]
[Singleton<IKatMotionTimeConfigService, KatMotionTimeConfigService>]
[Singleton<IKatDeadZoneConfigService, KatDeadZoneConfigService>]
[Singleton<ITimeAndDeadZoneVMService, TimeAndDeadZoneVMService>]
[Singleton<IAutoDisableService, AutoDisableService>]
[Singleton<ITransparentInfoService, TransparentInfoService>]
[Singleton<IModeChangeService, ModeChangeService>]
[Singleton<IConflictKatMotionService, ConflictKatMotionService>]
[Singleton<IMetaKeyPresetService, MetaKeyPresetService>]

[Singleton<IGlobalStates, GlobalStates>]
[Singleton<IViewRegister, ViewRegister>]

public partial class SpaceKatServiceProvider
{
    public ILogger Logger { get; set; } = null!;

    private LocalSettingsOptions CreateLocalSettingsOptions()
        => new LocalSettingsOptions
        {
            ApplicationDataFolder = "SpaceKatMotionMapper",
            LocalSettingsFile = "LocalSettings.json"
        };

    private Func<TransparentInfoWindow> CreateTransparentWindowFactory()
        => () => GetService<TransparentInfoWindow>();

    private Func<TransparentInfoViewModel> CreateTransparentVMFactory()
        => () => GetService<TransparentInfoViewModel>();

    private Func<PresetsEditorMainView> CreatePresetsEditorMainViewFactory()
        => () => GetService<PresetsEditorMainView>();
}
