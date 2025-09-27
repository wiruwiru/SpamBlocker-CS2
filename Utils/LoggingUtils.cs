using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

using SpamBlocker.Configs;
using SpamBlocker.Managers;
using SpamBlocker.Services;
using SpamBlocker.Models;

namespace SpamBlocker.Utils
{
    public static class LoggingUtils
    {
        private static DiscordService? _discordService;
        public static void SetDiscordService(DiscordService? discordService)
        {
            _discordService = discordService;
        }

        public static void LogViolation(CCSPlayerController player, string type, FilterResult filterResult, SpamBlockerConfig config, object? playerInfo = null)
        {
            if (config.ChatProtection.LogViolations)
            {
                LogViolationToConsole(player, type, filterResult, playerInfo);
            }

            if (config.DiscordLogging.Enabled && _discordService != null)
            {
                var serverName = GetSafeServerName(config);

                DiscordViolationData violationData;
                if (playerInfo != null)
                {
                    violationData = DiscordViolationData.FromCachedPlayerInfo(playerInfo, type, filterResult, serverName);
                }
                else
                {
                    violationData = DiscordViolationData.FromPlayer(player, type, filterResult, serverName);
                }

                Task.Run(async () =>
                {
                    try
                    {
                        await _discordService.SendViolationToDiscord(violationData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SpamBlocker] Error sending violation to Discord: {ex.Message}");
                    }
                });
            }
        }

        private static string GetSafeServerName(SpamBlockerConfig config)
        {
            try
            {
                var hostname = ConVar.Find("hostname")?.StringValue;
                return !string.IsNullOrEmpty(hostname) ? hostname : config.DiscordLogging.ServerName;
            }
            catch
            {
                return config.DiscordLogging.ServerName;
            }
        }

        private static void LogViolationToConsole(CCSPlayerController player, string type, FilterResult filterResult, object? playerInfo = null)
        {
            try
            {
                string playerName, steamId;
                if (playerInfo != null)
                {
                    var nameProperty = playerInfo.GetType().GetProperty("Name");
                    var steamIdProperty = playerInfo.GetType().GetProperty("SteamId");

                    playerName = nameProperty?.GetValue(playerInfo)?.ToString() ?? "Unknown";
                    steamId = steamIdProperty?.GetValue(playerInfo)?.ToString() ?? "Unknown";
                }
                else
                {
                    playerName = player?.PlayerName ?? "Unknown";
                    steamId = player?.AuthorizedSteamID?.SteamId64.ToString() ?? "Unknown";
                }

                string logMessage = $"[SpamBlocker] Violation - Player: {playerName} " +
                                  $"(SteamID: {steamId}) | " +
                                  $"Type: {type} | " +
                                  $"Reason: {filterResult.Reason} | " +
                                  $"Content: '{filterResult.DetectedContent}' | " +
                                  $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                Server.PrintToConsole(logMessage);
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"[SpamBlocker] Error logging violation: {ex.Message}");
            }
        }

        public static void LogDebug(string message, SpamBlockerConfig config)
        {
            if (config.Settings.DebugMode)
            {
                Server.PrintToConsole($"[SpamBlocker] DEBUG: {message}");
            }
        }

        public static void LogInfo(string message)
        {
            Server.PrintToConsole($"[SpamBlocker] INFO: {message}");
        }

        public static void LogError(string message, Exception? exception = null)
        {
            string errorMessage = $"[SpamBlocker] ERROR: {message}";
            if (exception != null)
            {
                errorMessage += $" - {exception.Message}";
            }

            Server.PrintToConsole(errorMessage);
        }
    }
}