using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenNetMeter.Utilities
{
    internal static class Updater
    {
        private const string LatestReleaseUrl = "https://api.github.com/repos/Ashfaaq18/OpenNetMeter/releases/latest";

        public static async Task<(bool, string?, string?)> CheckForUpdate()
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OpenNetMeter");
                var json = await client.GetStringAsync(LatestReleaseUrl);
                using JsonDocument doc = JsonDocument.Parse(json);
                string? tag = doc.RootElement.GetProperty("tag_name").GetString();
                string? assetUrl = null;
                foreach (var asset in doc.RootElement.GetProperty("assets").EnumerateArray())
                {
                    string? url = asset.GetProperty("browser_download_url").GetString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        assetUrl = url;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(tag))
                    return (false, null, null);
                Version latest = new Version(tag.TrimStart('v', 'V'));
                Version current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);
                return (latest > current, assetUrl, tag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return (false, null, null);
            }
        }

        public static async Task<bool> DownloadUpdate(string url, string destPath)
        {
            try
            {
                using HttpClient client = new HttpClient();
                var data = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(destPath, data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update download failed: {ex.Message}");
                return false;
            }
        }
    }
}
