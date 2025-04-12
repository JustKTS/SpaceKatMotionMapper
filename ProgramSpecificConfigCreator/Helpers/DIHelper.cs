using System;
using System.ComponentModel.DataAnnotations;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ProgramSpecificConfigCreator.Services;
using ProgramSpecificConfigCreator.ViewModels;
using ProgramSpecificConfigCreator.Views;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;

namespace ProgramSpecificConfigCreator.Helpers;

public static class DIHelper
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ProgramSpecificConfigViewModel>();
        services.AddSingleton<ProgramSpecMainViewModel>();

        services.AddSingleton<ProgramSpecificConfigMainWindow>();

        services.AddTransient<ExistSpecConfigSelectorViewModel>();
        services.AddTransient<ExistSpecConfigSelectorView>();

        services.AddSingleton<IProgramSpecMetaKeyFileService, ProgramSpecMetaKeyFileService>();

        services.AddTransient<IStorageProvider>(sp =>
        {
            var mainWindow = sp.GetRequiredService<ProgramSpecificConfigMainWindow>();
            var toplevel = TopLevel.GetTopLevel(mainWindow);
            return toplevel!.StorageProvider;
        });

        services.AddSingleton<IUiInteractService, UiInteractService>();
        services.AddSingleton<IPopUpNotificationService, PopUpNotificationService>();
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