using System;
using System.IO;
using System.Net;
using System.Reflection;
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

        public static async Task CheckForUpdates()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                CleanupOldFiles();

                var currentVersion = BasicCustomHudPlugin.Instance.Version;
                var latestReleaseUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", UserAgent);
                    
                    string json = await client.DownloadStringTaskAsync(latestReleaseUrl);


                    var tagMatch = Regex.Match(json, "\"tag_name\":\\s*\"(.*?)\"");
                    if (!tagMatch.Success) return;

                    string tag = tagMatch.Groups[1].Value;
                    string versionStr = tag.TrimStart('v');
                    
                    if (!Version.TryParse(versionStr, out Version newVersion)) return;

                    if (newVersion > currentVersion)
                    {
                        var urlMatch = Regex.Match(json, "\"browser_download_url\":\\s*\"(.*?BasicCustomHUDRemake\\.dll)\"");
                        if (!urlMatch.Success)
                        {
                            urlMatch = Regex.Match(json, "\"browser_download_url\":\\s*\"(.*?\\.dll)\"");
                            if (!urlMatch.Success) return;
                        }

                        string downloadUrl = urlMatch.Groups[1].Value;
                        PerformUpdate(downloadUrl, newVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[AutoUpdater] Update check failed: {ex.Message}");
            }
        }

        private static void PerformUpdate(string url, Version newVersion)
        {
            try
            {
                string currentDllPath = Assembly.GetExecutingAssembly().Location;
                string directory = Path.GetDirectoryName(currentDllPath);
                string tempFilePath = Path.Combine(directory, $"BasicCustomHUDRemake_v{newVersion}.dll.temp");
                string oldFilePath = currentDllPath + ".old";

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", UserAgent);
                    client.DownloadFile(url, tempFilePath);
                }

                if (File.Exists(tempFilePath))
                {

                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                    
                    try 
                    {
                        File.Move(currentDllPath, oldFilePath);
                        File.Move(tempFilePath, currentDllPath);
                        Logger.Info($"[AutoUpdater] Updated to v{newVersion}. Restart server to apply.");
                    }
                    catch (IOException)
                    {
                        if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                    }
                }
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
                string currentDllPath = Assembly.GetExecutingAssembly().Location;
                string directory = Path.GetDirectoryName(currentDllPath);
                
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
