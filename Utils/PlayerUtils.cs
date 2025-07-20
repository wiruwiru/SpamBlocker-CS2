using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace SpamBlocker.Utils
{
    public static class PlayerUtils
    {
        public static bool IsValidPlayer(CCSPlayerController? player)
        {
            return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
        }

        public static bool IsValidAuthorizedPlayer(CCSPlayerController? player)
        {
            return IsValidPlayer(player) && player!.AuthorizedSteamID != null;
        }

        public static bool HasValidName(CCSPlayerController? player)
        {
            return IsValidPlayer(player) && !string.IsNullOrEmpty(player!.PlayerName);
        }

        public static List<CCSPlayerController> GetValidPlayers()
        {
            return Utilities.GetPlayers().Where(IsValidPlayer).ToList();
        }

        public static string GetPlayerInfo(CCSPlayerController player)
        {
            if (!IsValidPlayer(player))
                return "Invalid Player";

            return $"{player.PlayerName} (Slot: {player.Slot}, SteamID: {player.AuthorizedSteamID?.SteamId64})";
        }

        public static string GetSafePlayerName(CCSPlayerController? player, string fallback = "Unknown")
        {
            return HasValidName(player) ? player!.PlayerName : fallback;
        }

        public static string GetSafeSteamId(CCSPlayerController? player, string fallback = "Unknown")
        {
            return IsValidAuthorizedPlayer(player) ? player!.AuthorizedSteamID!.SteamId64.ToString() : fallback;
        }
    }
}