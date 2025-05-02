using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using LanguageExt.Common;
using SpaceKat.Shared.Defines;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.ViewModels;

public partial class OtherConfigsViewModel : ViewModelBase
{
    public ObservableCollection<KatMotionConfigViewModel> KatMotionConfigGroups { get; } = [];
    private readonly KatMotionFileService _katMotionFileService;
    private readonly IStorageProviderService _storageProviderService;
    private readonly PopUpNotificationService _popUpNotificationService;
    private readonly ActivationStatusService _activationStatusService;
    private readonly KatMotionConfigVMManageService _katMotionConfigVmManageService;

    public OtherConfigsViewModel()
    {
        _katMotionFileService = App.GetRequiredService<KatMotionFileService>();
        _popUpNotificationService = App.GetRequiredService<PopUpNotificationService>();
        _storageProviderService = App.GetRequiredService<IStorageProviderService>();
        _activationStatusService = App.GetRequiredService<ActivationStatusService>();
        _katMotionConfigVmManageService = App.GetRequiredService<KatMotionConfigVMManageService>();
        Add();
    }

    [RelayCommand]
    private void Add()
    {
        var vm = new KatMotionConfigViewModel(this);
        _katMotionConfigVmManageService.RegisterConfig(vm);
        KatMotionConfigGroups.Add(vm);
    }

    [RelayCommand]
    private void Remove(int index)
    {
        if (index < 0 || index >= KatMotionConfigGroups.Count) return;
        _katMotionConfigVmManageService.RemoveConfig(KatMotionConfigGroups[index].Id);
        _activationStatusService.DeleteActivationStatus(KatMotionConfigGroups[index].Id);
        _katMotionFileService.DeleteConfigGroupFromSysConf(KatMotionConfigGroups[index].Id);
        KatMotionConfigGroups.RemoveAt(index);

        if (KatMotionConfigGroups.Count != 0) return;

        var vm = new KatMotionConfigViewModel(this);
        _katMotionConfigVmManageService.RegisterConfig(vm);
        KatMotionConfigGroups.Add(vm);
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
            var cgRet = _katMotionFileService.LoadConfigGroup(file.Path.LocalPath);
            _ = cgRet.Match(cg =>
            {
                // ReSharper disable once IdentifierTypo
                var cgvm = new KatMotionConfigViewModel(this);
                cgvm.LoadFromConfigGroup(cg);
                cgvm.IsDefault = false;
                _katMotionConfigVmManageService.RegisterConfig(cgvm);
                KatMotionConfigGroups.Add(cgvm);
                return true;
            }, ex =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            });
        }
    }

    private void ClearConfigGroups()
    {
        KatMotionConfigGroups.Iter(e => _katMotionConfigVmManageService.RemoveConfig(e.Id));
        KatMotionConfigGroups.Clear();
    }

    [RelayCommand]
    private void ReloadConfigGroupsFromSysConf()
    {
        ClearConfigGroups();
        var rets = _katMotionFileService.LoadConfigGroupsFromSysConf();
        _ = rets.Match(cgs =>
        {
            foreach (var cg in cgs)
            {
                var cgVm = new KatMotionConfigViewModel(this);
                cgVm.LoadFromConfigGroup(cg);
                cgVm.IsDefault = false;
                _katMotionConfigVmManageService.RegisterConfig(cgVm);
                KatMotionConfigGroups.Add(cgVm);
                if (!_activationStatusService.IsConfigGroupActivated(cgVm.Id)) continue;
                cgVm.ActivateActionsCommand.Execute(null);
            }

            return true;
        }, ex =>
        {
            _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
            return false;
        });
        if (KatMotionConfigGroups.Count == 0)
        {
            Add();
        }
    }

    [RelayCommand]
    private Task<Result<bool>> SaveGroupsToConfigDir()
    {
        List<KatMotionConfigGroup> groups = [];
        // ReSharper disable once IdentifierTypo
        return Task.FromResult(KatMotionConfigGroups.Select(cgvm => cgvm.ToKatMotionConfigGroups()).Select(ret =>
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
            : _katMotionFileService.SaveConfigGroupsToSysConf(groups));
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
        foreach (var cgvm in KatMotionConfigGroups)
        {
            var cgRet = cgvm.ToKatMotionConfigGroups();
            var ret = cgRet.Match(cg =>
            {
                var filename = cg.Guid + ".json";
                var dirPath = folders[0].Path.LocalPath;
                var path = Path.Join(dirPath, filename);
                return _katMotionFileService.SaveConfigGroup(cg, path);
            }, ex => new Result<bool>(ex));
            ret.IfFail(ex => { _popUpNotificationService.Pop(NotificationType.Error, ex.Message); });
        }
    }
}