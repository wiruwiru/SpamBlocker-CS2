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

        public static void LogViolation(CCSPlayerController player, string type, FilterResult filterResult, SpamBlockerConfig config)
        {
            if (config.ChatProtection.LogViolations)
            {
                LogViolationToConsole(player, type, filterResult);
            }

            if (config.DiscordLogging.Enabled && _discordService != null)
            {
                var serverName = GetSafeServerName(config);
                var violationData = DiscordViolationData.FromPlayer(player, type, filterResult, serverName);

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

        private static void LogViolationToConsole(CCSPlayerController player, string type, FilterResult filterResult)
        {
            try
            {
                string logMessage = $"[SpamBlocker] Violation - Player: {player.PlayerName} " +
                                  $"(SteamID: {player.AuthorizedSteamID?.SteamId64}) | " +
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