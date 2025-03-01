using Avalonia.Platform.Storage;

namespace SpaceKatMotionMapper.Defines;

public static class FilePickerFileTypeDefines
{
    public static readonly FilePickerFileType Json = new FilePickerFileType("json")
    {
        Patterns = ["*.json"],
        MimeTypes = ["application/json"]
    };  
    public static readonly FilePickerFileType Exe = new FilePickerFileType("exe")
    {
        Patterns = ["*.exe"],
        MimeTypes = ["application/vnd.microsoft.portable-executable"]
    };
}