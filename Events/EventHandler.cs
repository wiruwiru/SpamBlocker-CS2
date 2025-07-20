using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;

using SpamBlocker.Configs;
using SpamBlocker.Managers;
using SpamBlocker.Utils;

namespace SpamBlocker.Events
{
    public class EventHandler
    {
        private readonly FilterManager _filterManager;
        private readonly SpamBlockerConfig _config;

        public EventHandler(FilterManager filterManager, SpamBlockerConfig config)
        {
            _filterManager = filterManager;
            _config = config;
        }

        public void RegisterEvents(BasePlugin plugin)
        {
            plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
            plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
            plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        }

        private void OnMapStart(string mapName)
        {
            LoggingUtils.LogDebug($"Map started: {mapName}", _config);
        }

        private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;

            if (!PlayerUtils.IsValidPlayer(player)) return HookResult.Continue;

            Server.NextFrame(() =>
            {
                var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(2.0f, () => CheckPlayerName(player!));
            });

            return HookResult.Continue;
        }

        private void OnClientDisconnect(int playerSlot)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

            if (!PlayerUtils.IsValidPlayer(player)) return;

            _filterManager.OnPlayerDisconnect(player!);
        }

        private void CheckPlayerName(CCSPlayerController player)
        {
            if (!PlayerUtils.HasValidName(player))
                return;

            var filterResult = _filterManager.CheckPlayerName(player.PlayerName, player);
            if (filterResult.IsBlocked)
            {
                _filterManager.HandleNameViolation(player, filterResult);

                LoggingUtils.LogDebug($"Player name violation detected: {player.PlayerName} - {filterResult.Reason}", _config);
                LoggingUtils.LogViolation(player, "Name", filterResult, _config);
            }
        }
    }
}