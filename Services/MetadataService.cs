using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GameManagerPro.Services
{
    public class MetadataService
    {
        private readonly HttpClient _httpClient;

        public MetadataService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://www.steamgriddb.com/api/v2/");
        }

        public async Task<string> GetGameImageUrlAsync(string gameName, string folderName = null)
        {
            var apiKey = App.SettingsService.Settings.SteamGridDbApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }

            try
            {
                var imageUrl = await TryGetImageUrl(gameName);
                
                if (imageUrl == null && !string.IsNullOrWhiteSpace(gameName))
                {
                    var spacedGameName = System.Text.RegularExpressions.Regex.Replace(gameName, @"(?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z])|(?<=[A-Za-z])([0-9])", " $1$2$3");
                    if (spacedGameName != gameName)
                        imageUrl = await TryGetImageUrl(spacedGameName.Trim());
                }

                if (imageUrl == null && !string.IsNullOrWhiteSpace(folderName) && folderName != gameName)
                {
                    imageUrl = await TryGetImageUrl(folderName);
                }

                if (imageUrl == null && !string.IsNullOrWhiteSpace(folderName) && folderName != gameName)
                {
                    var spacedFolderName = System.Text.RegularExpressions.Regex.Replace(folderName, @"(?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z])|(?<=[A-Za-z])([0-9])", " $1$2$3");
                    if (spacedFolderName != folderName)
                        imageUrl = await TryGetImageUrl(spacedFolderName.Trim());
                }

                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching metadata: {ex.Message}");
                return null;
            }
        }

        private async Task<string> TryGetImageUrl(string query)
        {
            try
            {
                // 1. Search for the game
                var searchUrl = $"search/autocomplete/{Uri.EscapeDataString(query)}";
                var searchResponse = await _httpClient.GetStringAsync(searchUrl);
                var searchJson = JObject.Parse(searchResponse);
                
                var success = searchJson["success"]?.Value<bool>() ?? false;
                if (!success) return null;

                var data = searchJson["data"] as JArray;
                if (data == null || data.Count == 0) return null;

                // Sort games by relevance to avoid picking wrong games like REPOSE for REPO
                var sortedData = data.OrderByDescending(item =>
                {
                    var name = item["name"]?.Value<string>();
                    if (string.IsNullOrWhiteSpace(name)) return 0;
                    
                    var cleanName = System.Text.RegularExpressions.Regex.Replace(name, "[^a-zA-Z0-9]", "").ToLower();
                    var cleanQuery = System.Text.RegularExpressions.Regex.Replace(query, "[^a-zA-Z0-9]", "").ToLower();
                    
                    if (cleanName == cleanQuery) return 4;
                    
                    if (System.Text.RegularExpressions.Regex.IsMatch(name, "^" + System.Text.RegularExpressions.Regex.Escape(query) + @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        return 3;

                    if (System.Text.RegularExpressions.Regex.IsMatch(name, @"\b" + System.Text.RegularExpressions.Regex.Escape(query) + @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        return 2;

                    return 1;
                }).ToList();

                // Only consider games with score >= 2 (must be at least a whole word match)
                // If query is very short, be strict.
                foreach (var item in sortedData)
                {
                    var name = item["name"]?.Value<string>();
                    var cleanName = System.Text.RegularExpressions.Regex.Replace(name ?? "", "[^a-zA-Z0-9]", "").ToLower();
                    var cleanQuery = System.Text.RegularExpressions.Regex.Replace(query, "[^a-zA-Z0-9]", "").ToLower();

                    bool isExactMatch = cleanName == cleanQuery;
                    bool isWordMatch = System.Text.RegularExpressions.Regex.IsMatch(name ?? "", @"\b" + System.Text.RegularExpressions.Regex.Escape(query) + @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    if (!isExactMatch && !isWordMatch) continue;

                    var gameId = item["id"]?.Value<int>();
                    if (gameId == null) continue;

                    // 2. Get grid image
                    var gridUrl = $"grids/game/{gameId}?dimensions=600x900,342x482";
                    var gridResponse = await _httpClient.GetStringAsync(gridUrl);
                    var gridJson = JObject.Parse(gridResponse);

                    var gridSuccess = gridJson["success"]?.Value<bool>() ?? false;
                    if (!gridSuccess) continue;

                    var gridData = gridJson["data"] as JArray;
                    if (gridData != null && gridData.Count > 0)
                    {
                        return gridData[0]["url"]?.Value<string>();
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
