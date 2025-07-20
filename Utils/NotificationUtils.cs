using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace SpamBlocker.Utils
{
    public static class NotificationUtils
    {
        public static void NotifyAdmins(string message, BasePlugin plugin, string adminPermission = "@css/admin")
        {
            var admins = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && AdminManager.PlayerHasPermissions(p, adminPermission)).ToList();
            foreach (var admin in admins)
            {
                admin.PrintToChat($"{plugin.Localizer["prefix"]} {message}");
            }

            LoggingUtils.LogInfo(message);
        }

        public static void NotifyPlayer(CCSPlayerController player, string message, BasePlugin plugin)
        {
            if (player == null || !player.IsValid)
                return;

            player.PrintToChat($"{plugin.Localizer["prefix"]} {message}");
        }

        public static void NotifyAdminsViolation(string playerName, string violationType, string reason, string detectedContent, BasePlugin plugin, string adminPermission = "@css/admin")
        {
            string message = $"{violationType} {plugin.Localizer["violation_by"]} {playerName}: {reason} ('{detectedContent}')";
            NotifyAdmins(message, plugin, adminPermission);
        }
    }
}