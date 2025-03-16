using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
    
    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        var options1 = options.Value;

        _applicationDataFolder = Path.Combine(
            _localApplicationData,
            options1.ApplicationDataFolder ?? nameof(SpaceKatMotionMapper)
        );
        if (!Directory.Exists(_applicationDataFolder))
        {
            Directory.CreateDirectory(_applicationDataFolder);
        }

        _localSettingsFile = options1.LocalSettingsFile ?? DefaultLocalSettingsFile;

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
                ) ?? [];

            _isInitialized = true;
        }
    }

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        await InitializeAsync();

        if (string.IsNullOrEmpty(key) || !_settings.TryGetValue(key, out var obj)) return default;
        if (obj is not JsonElement value) return await JsonConvertHelper.ToObjectAsync<T>((string)obj);
        var ret = value.ToString();
        if (string.IsNullOrEmpty(ret)) return default;
        return await JsonConvertHelper.ToObjectAsync<T>(ret);
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        await InitializeAsync();
        try
        {
            _settings[key] = JsonSerializer.Serialize(value, JsonSgOption.Default);
            _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }
}