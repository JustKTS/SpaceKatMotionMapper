using LanguageExt;
using LanguageExt.Common;
using SpaceKat.Shared.States;

namespace SpaceKat.Shared.Helpers;

public static class DownloadMetaKeyPresetsHelper
{
    private static readonly Uri DownloadUrl =
        new(
            "https://gitee.com/justkts/space-kat-motion-mapper-meta-key-presets/releases/download/latest/space-kat-motion-mapper-meta-key-presets.zip");

    private static readonly string LocalFileName = Path.Combine(GlobalPaths.DownloadTempDir, "presets.zip");

    public static async Task<Either<Exception, string>> DownloadMetaKeyPresetsAsync(Uri url, string localFileName)
    {
        if (!Path.Exists(GlobalPaths.DownloadTempDir))
        {
            Directory.CreateDirectory(GlobalPaths.DownloadTempDir);
        }

        //发起请求并异步等待结果
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(20));
        var httpclient = new HttpClient();

        try
        {
            var response = await httpclient.GetAsync(url, cts.Token);

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

    public static async Task<Either<Exception, string>> UnZipMetaKeyPresetsAsync(
        string localFileName)
    {
        var tempUnZipDir = Path.Combine(GlobalPaths.DownloadTempDir, "tempUnzip");
        Directory.CreateDirectory(tempUnZipDir);
        var ret = await Task.Run<Either<Exception, bool>>(() =>
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(localFileName, tempUnZipDir);
                return true;
            }
            catch (Exception e)
            {
                return e;
            }
        });
        File.Delete(localFileName);
        return ret.Match<Either<Exception, string>>(_ => tempUnZipDir, ex => ex);
    }

    public static Either<Exception, bool> CopyToConfigDir(string tempUnzipDir)
    {
        var filePaths = Directory.GetFiles(tempUnzipDir, "*.json");

        filePaths.Iter(filePath =>
        {
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, Path.Combine(GlobalPaths.MetaKeysConfigPath, fileName), true);
        });

        Directory.Delete(tempUnzipDir, true);

        return true;
    }

    public static async Task<Result<bool>> DownloadAndCopyMetaKeyPresetsAsync(Uri? url = null,
        string? localFilename = null)
    {
        url ??= DownloadUrl;
        localFilename ??= LocalFileName;
        var result = await
            DownloadMetaKeyPresetsAsync(url, localFilename)
                .BindAsync(UnZipMetaKeyPresetsAsync)
                .BindAsync(CopyToConfigDir);

        return result.Match(r => r, ex => new Result<bool>(ex));
    }
}