using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using LanguageExt;
using SpaceKat.Shared.Defines;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.ViewModels;

public partial class OtherConfigsViewModel(
    IKatMotionFileService katMotionFileService,
    IPopUpNotificationService popUpNotificationService,
    IStorageProviderService storageProviderService,
    IActivationStatusService activationStatusService,
    IKatMotionConfigVMManageService katMotionConfigVmManageService,
    IKatMotionActivateService katMotionActivateService,
    RunningProgramSelectorViewModel runningProgramSelectorVM,
    TimeAndDeadZoneVMService? timeAndDeadZoneVmService = null)
    : ViewModelBase
{
    public ObservableCollection<KatMotionConfigViewModel> KatMotionConfigGroups { get; } = [];

    [RelayCommand]
    private void Add()
    {
        var vm = new KatMotionConfigViewModel(
            katMotionActivateService,
            katMotionFileService,
            popUpNotificationService,
            storageProviderService,
            runningProgramSelectorVM,
            timeAndDeadZoneVmService
        ){Parent = this};

        katMotionConfigVmManageService.RegisterConfig(vm);
        KatMotionConfigGroups.Add(vm);
    }

    [RelayCommand]
    private void Remove(int index)
    {
        if (index < 0 || index >= KatMotionConfigGroups.Count) return;
        katMotionConfigVmManageService.RemoveConfig(KatMotionConfigGroups[index].Id);
        activationStatusService.DeleteActivationStatus(KatMotionConfigGroups[index].Id);
        katMotionFileService.DeleteConfigGroupFromSysConf(KatMotionConfigGroups[index].Id);
        KatMotionConfigGroups.RemoveAt(index);

        if (KatMotionConfigGroups.Count != 0) return;

        var vm = new KatMotionConfigViewModel(
            katMotionActivateService,
            katMotionFileService,
            popUpNotificationService,
            storageProviderService,
            runningProgramSelectorVM,
            timeAndDeadZoneVmService
        ){Parent = this};
        katMotionConfigVmManageService.RegisterConfig(vm);
        KatMotionConfigGroups.Add(vm);
    }

    [RelayCommand]
    private async Task LoadGroupFromFiles()
    {
        var storageProvider = storageProviderService.GetStorageProvider();
        if (storageProvider == null) return; // 在测试环境中可能为 null

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypeDefines.Json],
            Title = "选择配置文件（可多选）"
        });

        foreach (var file in files)
        {
            var cgRet = katMotionFileService.LoadConfigGroup(file.Path.LocalPath);
            _ = cgRet.Match(cg =>
            {
                // ReSharper disable once IdentifierTypo
                var cgvm = new KatMotionConfigViewModel(
                    katMotionActivateService,
                    katMotionFileService,
                    popUpNotificationService,
                    storageProviderService,
                    runningProgramSelectorVM,
                    timeAndDeadZoneVmService
                ){Parent = this};
                cgvm.LoadFromConfigGroup(cg);
                cgvm.IsDefault = false;
                katMotionConfigVmManageService.RegisterConfig(cgvm);
                KatMotionConfigGroups.Add(cgvm);
                return true;
            }, ex =>
            {
                popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            });
        }
    }

    private void ClearConfigGroups()
    {
        KatMotionConfigGroups.Iter(e => katMotionConfigVmManageService.RemoveConfig(e.Id));
        KatMotionConfigGroups.Clear();
    }

    [RelayCommand]
    private void ReloadConfigGroupsFromSysConf()
    {
        ClearConfigGroups();
        var rets = katMotionFileService.LoadConfigGroupsFromSysConf();

        _ = rets.Match(cgs =>
        {
            foreach (var cg in cgs)
            {
                var cgVm = new KatMotionConfigViewModel(
                    katMotionActivateService,
                    katMotionFileService,
                    popUpNotificationService,
                    storageProviderService,
                    runningProgramSelectorVM,
                    timeAndDeadZoneVmService
                ){Parent = this};
                cgVm.LoadFromConfigGroup(cg);
                cgVm.IsDefault = false;
                katMotionConfigVmManageService.RegisterConfig(cgVm);
                KatMotionConfigGroups.Add(cgVm);
                if (!activationStatusService.IsConfigGroupActivated(cgVm.Id)) continue;
                cgVm.ActivateActionsCommand.Execute(null);
            }

            return true;
        }, ex =>
        {
            popUpNotificationService.Pop(NotificationType.Error, ex.Message);
            return false;
        });
        if (KatMotionConfigGroups.Count == 0)
        {
            Add();
        }
    }

    [RelayCommand]
    private Task<Either<Exception, bool>> SaveGroupsToConfigDir()
    {
        List<KatMotionConfigGroup> groups = [];
        // ReSharper disable once IdentifierTypo
        return Task.FromResult(
            KatMotionConfigGroups.Select(cgvm => cgvm.ToKatMotionConfigGroups()).Select(ret =>
            ret.Match(cg =>
            {
                groups.Add(cg);
                return true;
            }, ex =>
            {
                popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            })).Any(flag => !flag) // TODO: 处理部分失败的情况
            ? new Exception("保存部分文件失败！")
            : katMotionFileService.SaveConfigGroupsToSysConf(groups));
    }

    [RelayCommand]
    private async Task SaveGroupToDirectory()
    {
        var folders = await storageProviderService.GetStorageProvider().OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
                Title = "选择保存目录"
            });
        if (folders.Count == 0) return;

        // ReSharper disable once IdentifierTypo
        foreach (var cgvm in KatMotionConfigGroups)
        {
            var cgRet = cgvm.ToKatMotionConfigGroups();
            var ret = cgRet.Match(cg =>
            {
                var filename = cg.Guid + ".json";
                var dirPath = folders[0].Path.LocalPath;
                var path = Path.Join(dirPath, filename);
                return katMotionFileService.SaveConfigGroup(cg, path);
            }, ex => ex);
            ret.IfLeft(ex => { popUpNotificationService.Pop(NotificationType.Error, ex.Message); });
        }
    }
}