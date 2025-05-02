using System;
using System.ComponentModel.DataAnnotations;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MetaKeyPresetsEditor.Services;
using MetaKeyPresetsEditor.ViewModels;
using MetaKeyPresetsEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;

namespace MetaKeyPresetsEditor.Helpers;

public static class DIHelper
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ProgramSpecificConfigViewModel>();
        services.AddSingleton<ProgramSpecMainViewModel>();

        services.AddTransient<PresetsEditorMainWindow>();
        services.AddSingleton<PresetsEditorMainView>();

        services.AddTransient<ExistSpecConfigSelectorViewModel>();
        services.AddTransient<ExistPresetSelectorView>();

        services.AddSingleton<IMetaKeyPresetFileService, MetaKeyPresetFileService>();

        services.AddTransient<IStorageProvider>(sp =>
        {
            var mainView = sp.GetRequiredService<PresetsEditorMainView>();
            var toplevel = TopLevel.GetTopLevel(mainView);
            return toplevel!.StorageProvider;
        });

        services.AddSingleton<IUiInteractService, UiInteractService>();
        services.AddSingleton<IPopUpNotificationSpecService, PopUpNotificationSpecService>();
    }
    
    
    public static IServiceProvider? ServiceProvider { get; private set; }

    public static IServiceProvider GetServiceProvider()
    {
        return ServiceProvider ?? throw new InvalidOperationException("ServiceProvider is not set.");
    }
    
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}