using System.Buffers;
using Windows.Win32.Foundation;

namespace Win32Helpers;

internal static class Helpers
{
    // ReSharper disable once InconsistentNaming
    internal static unsafe string CallWin32ToGetPWSTR(int bufferLength, Func<PWSTR, int, int> getter)
    {
        var buffer = ArrayPool<char>.Shared.Rent(bufferLength);
        try
        {
            fixed (char* ptr = buffer)
            {
                getter(ptr, bufferLength);
                return new string(ptr);
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}