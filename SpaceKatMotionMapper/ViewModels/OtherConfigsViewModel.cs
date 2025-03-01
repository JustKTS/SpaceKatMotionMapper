using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using LanguageExt.Common;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.ViewModels;

public partial class OtherConfigsViewModel : ViewModelBase
{
    public ObservableCollection<KatActionConfigViewModel> KatActionConfigGroups { get; }
    private readonly KatActionFileService _katActionFileService;
    private readonly IStorageProviderService _storageProviderService;
    private readonly PopUpNotificationService _popUpNotificationService;
    private readonly ActivationStatusService _activationStatusService;
    private readonly KatActionConfigVMManageService _katActionConfigVmManageService;

    public OtherConfigsViewModel()
    {
        KatActionConfigGroups = [new KatActionConfigViewModel(this)];
        _katActionFileService = App.GetRequiredService<KatActionFileService>();
        _popUpNotificationService = App.GetRequiredService<PopUpNotificationService>();
        _storageProviderService = App.GetRequiredService<IStorageProviderService>();
        _activationStatusService = App.GetRequiredService<ActivationStatusService>();
        _katActionConfigVmManageService = App.GetRequiredService<KatActionConfigVMManageService>();
    }

    [RelayCommand]
    private void Add()
    {
        var vm = new KatActionConfigViewModel(this);
        _katActionConfigVmManageService.RegisterConfig(vm);
        KatActionConfigGroups.Add(vm);
    }

    [RelayCommand]
    private void Remove(int index)
    {
        if (index < 0 || index >= KatActionConfigGroups.Count) return;
        _katActionConfigVmManageService.RemoveConfig(KatActionConfigGroups[index].Id);
        _activationStatusService.DeleteActivationStatus(KatActionConfigGroups[index].Id);
        KatActionConfigGroups.RemoveAt(index);

        if (KatActionConfigGroups.Count != 0) return;

        var vm = new KatActionConfigViewModel(this);
        _katActionConfigVmManageService.RegisterConfig(vm);
        KatActionConfigGroups.Add(vm);
    }

    [RelayCommand]
    private async Task LoadGroupFromFiles()
    {
        var files = await _storageProviderService.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypeDefines.Json],
            Title = "选择配置文件（可多选）"
        });

        foreach (var file in files)
        {
            var cgRet = _katActionFileService.LoadConfigGroup(file.Path.LocalPath);
            _ = cgRet.Match(cg =>
            {
                // ReSharper disable once IdentifierTypo
                var cgvm = new KatActionConfigViewModel(this);
                cgvm.LoadFromConfigGroup(cg);
                cgvm.IsDefault = false;
                KatActionConfigGroups.Add(cgvm);
                return true;
            }, ex =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            });
        }
    }

    [RelayCommand]
    private void ReloadConfigGroupsFromSysConf()
    {
        KatActionConfigGroups.Clear();
        var rets = _katActionFileService.LoadConfigGroupsFromSysConf();
        _ = rets.Match(cgs =>
        {
            foreach (var cg in cgs)
            {
                var cgVm = new KatActionConfigViewModel(this);
                cgVm.LoadFromConfigGroup(cg);
                cgVm.IsDefault = false;
                _katActionConfigVmManageService.RegisterConfig(cgVm);
                KatActionConfigGroups.Add(cgVm);
                if (!_activationStatusService.IsConfigGroupActivated(cgVm.Id)) continue;
                cgVm.ActivateActionsCommand.Execute(null);
            }

            return true;
        }, ex =>
        {
            _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
            return false;
        });
        if (KatActionConfigGroups.Count == 0)
        {
            Add();
        }
    }

    [RelayCommand]
    private Task<Result<bool>> SaveGroupsToConfigDir()
    {
        List<KatActionConfigGroup> groups = [];
        // ReSharper disable once IdentifierTypo
        return Task.FromResult(KatActionConfigGroups.Select(cgvm => cgvm.ToKatActionConfigGroups()).Select(ret =>
            ret.Match(cg =>
            {
                groups.Add(cg);
                return true;
            }, ex =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            })).Any(flag => !flag)
            ? new Result<bool>(false)
            : _katActionFileService.SaveConfigGroupsToSysConf(groups));
    }

    [RelayCommand]
    private async Task SaveGroupToDirectory()
    {
        var folders = await _storageProviderService.GetStorageProvider().OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
                Title = "选择保存目录"
            });
        if (folders.Count == 0) return;

        // ReSharper disable once IdentifierTypo
        foreach (var cgvm in KatActionConfigGroups)
        {
            var cgRet = cgvm.ToKatActionConfigGroups();
            var ret = cgRet.Match(cg =>
            {
                var filename = cg.Name + "_" + cg.Guid + ".json";
                var dirPath = folders[0].Path.LocalPath;
                var path = Path.Join(dirPath, filename);
                return _katActionFileService.SaveConfigGroup(cg, path);
            }, ex => new Result<bool>(ex));
            ret.IfFail(ex => { _popUpNotificationService.Pop(NotificationType.Error, ex.Message); });
        }
    }
}