using CSharpFunctionalExtensions;
using SpaceKat.Shared.States;

namespace SpaceKat.Shared.Helpers;

public static class DownloadMetaKeyPresetsHelper
{
    private static readonly Uri DownloadUrl =
        new(
            "https://gitee.com/justkts/space-kat-motion-mapper-meta-key-presets/releases/download/latest/space-kat-motion-mapper-meta-key-presets.zip");

    private static readonly string LocalFileName = Path.Combine(GlobalPaths.DownloadTempDir, "presets.zip");
    private static readonly HttpClient Httpclient = new();

    public static async Task<Result<string, Exception>> DownloadMetaKeyPresetsAsync(Uri url, string localFileName)
    {
        if (!Path.Exists(GlobalPaths.DownloadTempDir))
        {
            Directory.CreateDirectory(GlobalPaths.DownloadTempDir);
        }

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(20));

        try
        {
            var response = await Httpclient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode) return new Exception($"下载失败，状态码：{response.StatusCode}");

            await using var stream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
            await using var fileStream = File.Create(localFileName);
            await using (stream)
            {
                await stream.CopyToAsync(fileStream, CancellationToken.None);
            }

            return localFileName;
        }
        catch (Exception e)
        {
            return new Exception($"下载失败，{e.Message}");
        }
    }

    public static Result<string, Exception> UnZipMetaKeyPresets(
        string localFileName)
    {
        var tempUnZipDir = Path.Combine(GlobalPaths.DownloadTempDir, "tempUnzip");
        Directory.CreateDirectory(tempUnZipDir);
        try
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(localFileName, tempUnZipDir);
            File.Delete(localFileName);
            return tempUnZipDir;
        }
        catch (Exception e)
        {
            File.Delete(localFileName);
            return e;
        }
    }

    public static Result<bool, Exception> CopyToConfigDir(string tempUnzipDir)
    {
        var filePaths = Directory.GetFiles(tempUnzipDir, "*.json");

        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, Path.Combine(GlobalPaths.MetaKeysConfigPath, fileName), true);
        }

        Directory.Delete(tempUnzipDir, true);

        return true;
    }

    public static async Task<Result<bool, Exception>> DownloadAndCopyMetaKeyPresetsAsync(Uri? url = null,
        string? localFilename = null)
    {
        url ??= DownloadUrl;
        localFilename ??= LocalFileName;
        var downloadResult = await DownloadMetaKeyPresetsAsync(url, localFilename);
        if (downloadResult.IsFailure) return downloadResult.Error;
        var unzipResult = UnZipMetaKeyPresets(downloadResult.Value);
        if (unzipResult.IsFailure) return unzipResult.Error;
        return CopyToConfigDir(unzipResult.Value);
    }
}
