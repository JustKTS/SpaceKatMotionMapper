using Avalonia.Input;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Tests.Helpers;

public class VirtualKeyHelpersTests
{
    [Test]
    public async Task ToKeyCodeWrapper_ShouldMapTabKey()
    {
        var wrappedKey = Key.Tab.ToKeyCodeWrapper();

        await Assert.That(wrappedKey).IsEqualTo(KeyCodeWrapper.TAB);
    }
}


