using System;
using System.Threading.Tasks;

namespace SpaceKat.Shared.Services.Contract;

public interface IAutoDisableService
{
    bool IsInitialized { get; }
    bool IsPlatformSupported { get; }
    bool IsEnable { get; set; }
    event EventHandler<bool>? IsCurrentFpInList;
    Task InitializeAsync();
    Task<bool> WaitForInitializedAsync();
    void AddProgramPath(string programPath, string processName = "");
    void RemoveProgramPath(string programPath);
    bool IsPathContained(string path);
    string[] GetAllProgramPaths();
}
