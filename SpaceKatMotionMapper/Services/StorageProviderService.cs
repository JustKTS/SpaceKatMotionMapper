using Avalonia.Platform.Storage;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Helpers;

namespace SpaceKatMotionMapper.Services;

public class StorageProviderService : IStorageProviderService
{
    public IStorageProvider GetStorageProvider() => TopLevelHelper.GetTopLevel().StorageProvider;
}