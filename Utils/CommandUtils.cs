using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using SpamBlocker.Configs;

namespace SpamBlocker.Utils
{
    public static class CommandUtils
    {
        public static void ExecuteCommand(string commandTemplate, CCSPlayerController player, SpamBlockerConfig config)
        {
            if (string.IsNullOrEmpty(commandTemplate) || player == null || !player.IsValid)
                return;

            try
            {
                string command = SubstitutePlaceholders(commandTemplate, player);

                LoggingUtils.LogDebug($"Executing command: {command}", config);
                Server.ExecuteCommand(command);
            }
            catch (Exception ex)
            {
                LoggingUtils.LogError($"Error executing command '{commandTemplate}': {ex.Message}");
            }
        }

        public static string SubstitutePlaceholders(string template, CCSPlayerController player)
        {
            if (string.IsNullOrEmpty(template) || player == null || !player.IsValid)
                return template;

            return template
                .Replace("{userid}", player.UserId?.ToString() ?? "")
                .Replace("{steamid}", player.AuthorizedSteamID?.SteamId64.ToString() ?? "")
                .Replace("{name}", player.PlayerName ?? "")
                .Replace("{slot}", player.Slot.ToString());
        }

        public static bool IsCommand(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            return message.StartsWith("!") || message.StartsWith("@") ||
                   message.StartsWith("/") || message.StartsWith(".");
        }

        public static bool IsAdminCommand(string message)
        {
            return !string.IsNullOrEmpty(message) && message.StartsWith("@");
        }
    }
}