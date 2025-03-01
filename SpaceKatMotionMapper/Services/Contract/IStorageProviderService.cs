using Avalonia.Platform.Storage;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IStorageProviderService
{
    public IStorageProvider GetStorageProvider();
}