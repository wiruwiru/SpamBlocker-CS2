using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;

using SpamBlocker.Configs;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers
{
    public class ChatManager
    {
        private readonly FilterManager _filterManager;
        private readonly SpamBlockerConfig _config;
        private readonly BasePlugin _plugin;

        public ChatManager(FilterManager filterManager, SpamBlockerConfig config, BasePlugin plugin)
        {
            _filterManager = filterManager;
            _config = config;
            _plugin = plugin;
        }

        public void RegisterEvents(BasePlugin plugin)
        {
            LoggingUtils.LogDebug("Registering chat command listeners...", _config);

            try
            {
                plugin.AddCommandListener("say", OnPlayerChat);
                plugin.AddCommandListener("say_team", OnPlayerChatTeam);

                LoggingUtils.LogDebug("Chat command listeners registered successfully", _config);
                LoggingUtils.LogInfo("Chat filtering is now active");
            }
            catch (Exception ex)
            {
                LoggingUtils.LogError($"Failed to register chat listeners: {ex.Message}", ex);
            }
        }

        private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
        {
            LoggingUtils.LogDebug($"OnPlayerChat triggered for player: {PlayerUtils.GetSafePlayerName(player)}", _config);

            if (!PlayerUtils.IsValidAuthorizedPlayer(player))
            {
                LoggingUtils.LogDebug("Player not valid or authorized - ignoring", _config);
                return HookResult.Continue;
            }

            if (info.GetArg(1).Length == 0)
            {
                LoggingUtils.LogDebug("Empty message - ignoring", _config);
                return HookResult.Continue;
            }

            string message = info.GetArg(1);
            LoggingUtils.LogDebug($"Processing public chat message: '{message}' from {player!.PlayerName}", _config);

            if (CommandUtils.IsCommand(message))
            {
                LoggingUtils.LogDebug("Message is a command - ignoring", _config);
                return HookResult.Continue;
            }

            if (!_config.ChatProtection.Enabled)
            {
                LoggingUtils.LogDebug("Chat protection disabled - allowing message", _config);
                return HookResult.Continue;
            }

            var filterResult = _filterManager.CheckContent(message, player!);
            if (filterResult.IsBlocked)
            {
                LoggingUtils.LogDebug($"Message blocked: {filterResult.Reason}", _config);
                bool isInCooldown = _filterManager.IsPlayerInViolationCooldown(player!, "Chat");
                if (!isInCooldown)
                {
                    HandleChatViolation(player!, message, filterResult, false);
                }
                else
                {
                    LoggingUtils.LogDebug($"Player {player!.PlayerName} is in cooldown for Chat - blocking message but skipping sanctions", _config);
                }
                return HookResult.Handled;
            }
            else
            {
                LoggingUtils.LogDebug("Message passed filter - allowing", _config);
            }

            return HookResult.Continue;
        }

        private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
        {
            LoggingUtils.LogDebug($"OnPlayerChatTeam triggered for player: {PlayerUtils.GetSafePlayerName(player)}", _config);

            if (!PlayerUtils.IsValidAuthorizedPlayer(player))
            {
                LoggingUtils.LogDebug("Player not valid or authorized - ignoring", _config);
                return HookResult.Continue;
            }

            if (info.GetArg(1).Length == 0)
            {
                LoggingUtils.LogDebug("Empty team message - ignoring", _config);
                return HookResult.Continue;
            }

            string message = info.GetArg(1);
            LoggingUtils.LogDebug($"Processing team chat message: '{message}' from {player!.PlayerName}", _config);

            if (CommandUtils.IsAdminCommand(message) && AdminManager.PlayerHasPermissions(player!, "@css/chat"))
            {
                LoggingUtils.LogDebug("Message is admin command and player has permissions - allowing", _config);
                return HookResult.Continue;
            }

            if (CommandUtils.IsCommand(message))
            {
                LoggingUtils.LogDebug("Message is a command - ignoring", _config);
                return HookResult.Continue;
            }

            if (!_config.ChatProtection.Enabled)
            {
                LoggingUtils.LogDebug("Chat protection disabled - allowing team message", _config);
                return HookResult.Continue;
            }

            var filterResult = _filterManager.CheckContent(message, player!);
            if (filterResult.IsBlocked)
            {
                LoggingUtils.LogDebug($"Team message blocked: {filterResult.Reason}", _config);
                bool isInCooldown = _filterManager.IsPlayerInViolationCooldown(player!, "TeamChat");
                if (!isInCooldown)
                {
                    HandleChatViolation(player!, message, filterResult, true);
                }
                else
                {
                    LoggingUtils.LogDebug($"Player {player!.PlayerName} is in cooldown for TeamChat - blocking message but skipping sanctions", _config);
                }

                return HookResult.Handled;
            }
            else
            {
                LoggingUtils.LogDebug("Team message passed filter - allowing", _config);
            }

            return HookResult.Continue;
        }

        private void HandleChatViolation(CCSPlayerController player, string message, FilterResult filterResult, bool isTeamChat)
        {
            var chatType = isTeamChat ? "team chat" : "public chat";
            string violationType = isTeamChat ? "TeamChat" : "Chat";
            LoggingUtils.LogDebug($"{chatType} violation by {player.PlayerName}: '{message}' - {filterResult.Reason}", _config);

            if (_config.ChatProtection.NotifyPlayer)
            {
                NotificationUtils.NotifyPlayer(player, _plugin.Localizer["chat_message_blocked"], _plugin);
            }

            if (_config.ChatProtection.NotifyAdmins)
            {
                var chatTypeKey = isTeamChat ? "team_chat" : "public_chat";
                NotificationUtils.NotifyAdminsViolation(player.PlayerName, _plugin.Localizer["admin_blocked_message", chatTypeKey], filterResult.Reason, filterResult.DetectedContent, _plugin);
            }

            var playerInfo = new
            {
                Name = player.PlayerName,
                SteamId = player.AuthorizedSteamID?.SteamId64.ToString() ?? "Unknown",
                player.Slot,
                player.UserId
            };

            LoggingUtils.LogViolation(player, violationType, filterResult, _config, playerInfo);
            ExecuteChatAction(player, filterResult);
        }

        private void ExecuteChatAction(CCSPlayerController player, FilterResult filterResult)
        {
            var action = _config.ChatProtection.Action.ToLowerInvariant();

            switch (action)
            {
                case "kick":
                    var kickTimer = new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, () =>
                    {
                        if (player.IsValid)
                        {
                            CommandUtils.ExecuteCommand(_config.ChatProtection.KickCommand, player, _config);
                        }
                    });
                    break;

                case "ban":
                    var banTimer = new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, () =>
                    {
                        if (player.IsValid)
                        {
                            CommandUtils.ExecuteCommand(_config.ChatProtection.BanCommand, player, _config);
                        }
                    });
                    break;

                case "custom":
                    if (!string.IsNullOrEmpty(_config.ChatProtection.CustomCommand))
                    {
                        var customTimer = new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, () =>
                        {
                            if (player.IsValid)
                            {
                                CommandUtils.ExecuteCommand(_config.ChatProtection.CustomCommand, player, _config);
                            }
                        });
                    }
                    break;

                case "block":
                default: break;
            }
        }
    }
}