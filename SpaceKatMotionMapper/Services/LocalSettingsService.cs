using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class LocalSettingsService : ILocalSettingsService
{
    private const string DefaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;

    private readonly string _localApplicationData = Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData
    );

    private readonly string _applicationDataFolder;
    private readonly string _localSettingsFile;

    private Dictionary<string, object> _settings;

    private bool _isInitialized;
    
    public LocalSettingsService(IFileService fileService, LocalSettingsOptions options)
    {
        _fileService = fileService;

        _applicationDataFolder = Path.Combine(
            _localApplicationData,
            options.ApplicationDataFolder ?? nameof(SpaceKatMotionMapper)
        );
        if (!Directory.Exists(_applicationDataFolder))
        {
            Directory.CreateDirectory(_applicationDataFolder);
        }

        _localSettingsFile = options.LocalSettingsFile ?? DefaultLocalSettingsFile;

        _settings = [];
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings =
                await Task.Run(
                    () =>
                        _fileService.Read<Dictionary<string, object>>(
                            _applicationDataFolder,
                            _localSettingsFile
                        )
                ).ConfigureAwait(false) ?? [];

            _isInitialized = true;
        }
    }

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        await InitializeAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(key) || !_settings.TryGetValue(key, out var obj)) return default;
        if (obj is not JsonElement value) return await JsonConvertHelper.ToObjectAsync<T>((string)obj).ConfigureAwait(false);
        var ret = value.ToString();
        if (string.IsNullOrEmpty(ret)) return default;
        return await JsonConvertHelper.ToObjectAsync<T>(ret).ConfigureAwait(false);
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        await InitializeAsync().ConfigureAwait(false);
        try
        {
            _settings[key] = JsonSerializer.Serialize(value, JsonSgOption.GetTypeInfo<T>());
            _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }
}