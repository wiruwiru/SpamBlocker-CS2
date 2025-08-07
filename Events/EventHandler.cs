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
            plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
            plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
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

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            LoggingUtils.LogDebug("Round start event triggered - scheduling name reapplication", _config);

            var timer1 = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, () =>
            {
                LoggingUtils.LogDebug("First name reapplication (1s delay)", _config);
                _filterManager.ReapplyRenamedNames();
            });

            var timer2 = new CounterStrikeSharp.API.Modules.Timers.Timer(3.0f, () =>
            {
                LoggingUtils.LogDebug("Second name reapplication (3s delay)", _config);
                _filterManager.ReapplyRenamedNames();
            });

            var timer3 = new CounterStrikeSharp.API.Modules.Timers.Timer(5.0f, () =>
            {
                LoggingUtils.LogDebug("Third name reapplication (5s delay)", _config);
                _filterManager.ReapplyRenamedNames();
            });

            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            LoggingUtils.LogDebug("Round end event triggered - scheduling name reapplication", _config);

            var timer1 = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, () =>
            {
                LoggingUtils.LogDebug("First name reapplication (1s delay)", _config);
                _filterManager.ReapplyRenamedNames();
            });

            var timer2 = new CounterStrikeSharp.API.Modules.Timers.Timer(3.0f, () =>
            {
                LoggingUtils.LogDebug("Second name reapplication (3s delay)", _config);
                _filterManager.ReapplyRenamedNames();
            });

            return HookResult.Continue;
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;

            if (!PlayerUtils.IsValidPlayer(player)) return HookResult.Continue;
            var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(2.0f, () =>
            {
                LoggingUtils.LogDebug($"Player spawn name reapplication for {PlayerUtils.GetSafePlayerName(player)}", _config);
                _filterManager.ReapplyRenamedNames();
            });

            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;

            if (!PlayerUtils.IsValidPlayer(player)) return HookResult.Continue;
            var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(2.0f, () =>
            {
                LoggingUtils.LogDebug($"Player spawn name reapplication for {PlayerUtils.GetSafePlayerName(player)}", _config);
                _filterManager.ReapplyRenamedNames();
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