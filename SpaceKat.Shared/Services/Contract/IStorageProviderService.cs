using Avalonia.Platform.Storage;

namespace SpaceKat.Shared.Services.Contract;

public interface IStorageProviderService
{
    public IStorageProvider GetStorageProvider();
}