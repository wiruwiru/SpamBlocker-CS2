using CounterStrikeSharp.API.Core;
using SpamBlocker.Managers;

namespace SpamBlocker.Models
{
    public class DiscordViolationData
    {
        public string PlayerName { get; set; } = "";
        public string SteamId { get; set; } = "";
        public string ServerName { get; set; } = "";
        public string ViolationType { get; set; } = "";
        public string Reason { get; set; } = "";
        public string DetectedContent { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static DiscordViolationData FromPlayer(CCSPlayerController player, string violationType, FilterResult filterResult, string serverName)
        {
            return new DiscordViolationData
            {
                PlayerName = CleanPlayerName(player?.PlayerName ?? "Unknown"),
                SteamId = player?.AuthorizedSteamID?.SteamId64.ToString() ?? "Unknown",
                ServerName = serverName,
                ViolationType = violationType,
                Reason = filterResult.Reason,
                DetectedContent = filterResult.DetectedContent,
                Timestamp = DateTime.UtcNow
            };
        }

        public static DiscordViolationData FromCachedPlayerInfo(object playerInfo, string violationType, FilterResult filterResult, string serverName)
        {
            try
            {
                var nameProperty = playerInfo.GetType().GetProperty("Name");
                var steamIdProperty = playerInfo.GetType().GetProperty("SteamId");

                string playerName = nameProperty?.GetValue(playerInfo)?.ToString() ?? "Unknown";
                string steamId = steamIdProperty?.GetValue(playerInfo)?.ToString() ?? "Unknown";

                return new DiscordViolationData
                {
                    PlayerName = CleanPlayerName(playerName),
                    SteamId = steamId,
                    ServerName = serverName,
                    ViolationType = violationType,
                    Reason = filterResult.Reason,
                    DetectedContent = filterResult.DetectedContent,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch
            {
                return new DiscordViolationData
                {
                    PlayerName = "Unknown",
                    SteamId = "Unknown",
                    ServerName = serverName,
                    ViolationType = violationType,
                    Reason = filterResult.Reason,
                    DetectedContent = filterResult.DetectedContent,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        private static string CleanPlayerName(string playerName)
        {
            return playerName
                .Replace("[Ready]", "")
                .Replace("[No Ready]", "")
                .Replace("[READY]", "")
                .Replace("[NOT READY]", "")
                .Trim();
        }
    }
}