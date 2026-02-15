using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LabApi.Features.Console;

namespace BasicCustomHUDRemake
{
    public static class AutoUpdater
    {
        private const string RepoOwner = "KaneTheDeveloper";
        private const string RepoName = "BasicCustomHUDRemake";
        private const string UserAgent = "BasicCustomHUDRemake-AutoUpdater";

        private static readonly HttpClient _httpClient;

        static AutoUpdater()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        }

        /// <summary>
        /// Gets the plugin's DLL path via the LabAPI <see cref="LabApi.Loader.Features.Plugins.Plugin.FilePath"/> property.
        /// </summary>
        private static string GetPluginFilePath()
        {
            return BasicCustomHudPlugin.Instance.FilePath;
        }

        public static async Task CheckForUpdates()
        {
            try
            {
                CleanupOldFiles();

                var currentVersion = BasicCustomHudPlugin.Instance.Version;
                var latestReleaseUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

                string json = await _httpClient.GetStringAsync(latestReleaseUrl);

                var tagMatch = Regex.Match(json, "\"tag_name\":\\s*\"(.*?)\"");
                if (!tagMatch.Success) return;

                string tag = tagMatch.Groups[1].Value;
                string versionStr = tag.TrimStart('v');

                if (!Version.TryParse(versionStr, out Version newVersion)) return;

                if (newVersion > currentVersion)
                {
                    Logger.Info($"[AutoUpdater] New version available: v{newVersion} (current: v{currentVersion})");

                    var urlMatch = Regex.Match(json, "\"browser_download_url\":\\s*\"(.*?BasicCustomHUDRemake\\.dll)\"");
                    if (!urlMatch.Success)
                    {
                        urlMatch = Regex.Match(json, "\"browser_download_url\":\\s*\"(.*?\\.dll)\"");
                        if (!urlMatch.Success)
                        {
                            Logger.Warn("[AutoUpdater] No DLL asset found in the latest release.");
                            return;
                        }
                    }

                    string downloadUrl = urlMatch.Groups[1].Value;
                    await PerformUpdateAsync(downloadUrl, newVersion);
                }
                else
                {
                    Logger.Debug($"[AutoUpdater] Plugin is up to date (v{currentVersion}).");
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"[AutoUpdater] Network error during update check: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[AutoUpdater] Update check failed: {ex.Message}");
            }
        }

        private static async Task PerformUpdateAsync(string url, Version newVersion)
        {
            try
            {
                string currentDllPath = GetPluginFilePath();

                if (string.IsNullOrEmpty(currentDllPath) || !File.Exists(currentDllPath))
                {
                    Logger.Error("[AutoUpdater] Could not resolve plugin file path. Update aborted.");
                    return;
                }

                string directory = Path.GetDirectoryName(currentDllPath);
                string tempFilePath = Path.Combine(directory, $"BasicCustomHUDRemake_v{newVersion}.dll.temp");
                string oldFilePath = currentDllPath + ".old";

                byte[] data = await _httpClient.GetByteArrayAsync(url);
                File.WriteAllBytes(tempFilePath, data);

                if (File.Exists(tempFilePath))
                {
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);

                    try
                    {
                        File.Move(currentDllPath, oldFilePath);
                        File.Move(tempFilePath, currentDllPath);
                        Logger.Info($"[AutoUpdater] Updated to v{newVersion}. Restart server to apply.");
                    }
                    catch (IOException ex)
                    {
                        Logger.Warn($"[AutoUpdater] Could not replace DLL (file may be locked): {ex.Message}");
                        if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"[AutoUpdater] Download failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[AutoUpdater] Update installation failed: {ex.Message}");
            }
        }

        private static void CleanupOldFiles()
        {
            try
            {
                string currentDllPath = GetPluginFilePath();

                if (string.IsNullOrEmpty(currentDllPath))
                    return;

                string directory = Path.GetDirectoryName(currentDllPath);

                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                    return;

                string oldFilePath = currentDllPath + ".old";
                if (File.Exists(oldFilePath))
                {
                    try { File.Delete(oldFilePath); } catch { }
                }

                foreach (var file in Directory.GetFiles(directory, "BasicCustomHUDRemake_v*.dll.temp"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
            catch { }
        }
    }
}
