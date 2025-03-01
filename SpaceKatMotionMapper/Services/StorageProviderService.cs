using Avalonia.Platform.Storage;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class StorageProviderService(ITopLevelHelper topLevelHelper) : IStorageProviderService
{
    public IStorageProvider GetStorageProvider() => topLevelHelper.GetTopLevel().StorageProvider;
}