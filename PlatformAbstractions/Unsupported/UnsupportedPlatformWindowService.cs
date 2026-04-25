using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformAbstractions.Unsupported;

public class UnsupportedPlatformWindowService : IPlatformWindowService
{
    public IReadOnlyList<ForeProgramInfo> FindAllForegroundPrograms()
    {
        return [];
    }

#pragma warning disable CS1998
    public async IAsyncEnumerable<ForeProgramInfo> FindAllForegroundProgramsAsync(
#pragma warning restore CS1998
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield break;
    }
}
