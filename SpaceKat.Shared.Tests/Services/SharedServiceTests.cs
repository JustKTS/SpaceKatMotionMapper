using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Tests.Services;

public class SharedServiceTests
{
    [Test]
    public async Task ActivationStatusService_ShouldReadExistingStatusFromLocalSettings()
    {
        var id = Guid.NewGuid();
        var settings = new InMemoryLocalSettingsService
        {
            ActivationStatus = new Dictionary<Guid, bool> { [id] = true }
        };

        var sut = new ActivationStatusService(settings);

        await Assert.That(sut.IsConfigGroupActivated(id)).IsTrue();
    }

    [Test]
    public async Task ActivationStatusService_SetAndDelete_ShouldPersistAndReflectStatus()
    {
        var id = Guid.NewGuid();
        var settings = new InMemoryLocalSettingsService();
        var sut = new ActivationStatusService(settings);

        sut.SetActivationStatus(id, true);
        await Assert.That(sut.IsConfigGroupActivated(id)).IsTrue();
        await Assert.That(settings.LastSavedActivationStatus.ContainsKey(id)).IsTrue();

        sut.DeleteActivationStatus(id);
        await Assert.That(sut.IsConfigGroupActivated(id)).IsFalse();
        await Assert.That(settings.LastSavedActivationStatus.ContainsKey(id)).IsFalse();
    }

    private sealed class InMemoryLocalSettingsService : ILocalSettingsService
    {
        public Dictionary<Guid, bool>? ActivationStatus { get; set; }
        public Dictionary<Guid, bool> LastSavedActivationStatus { get; private set; } = [];

        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (typeof(T) == typeof(Dictionary<Guid, bool>))
            {
                return Task.FromResult((T?)(object?)ActivationStatus);
            }

            return Task.FromResult(default(T));
        }

        public Task SaveSettingAsync<T>(string key, T value)
        {
            if (value is Dictionary<Guid, bool> dict)
            {
                LastSavedActivationStatus = new Dictionary<Guid, bool>(dict);
                ActivationStatus = new Dictionary<Guid, bool>(dict);
            }

            return Task.CompletedTask;
        }
    }
}
