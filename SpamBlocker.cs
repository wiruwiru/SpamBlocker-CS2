using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

using SpamBlocker.Configs;
using SpamBlocker.Managers;
using SpamBlocker.Services;
using SpamBlocker.Utils;
using EventHandler = SpamBlocker.Events.EventHandler;

namespace SpamBlocker;

[MinimumApiVersion(318)]
public class SpamBlocker : BasePlugin, IPluginConfig<SpamBlockerConfig>
{
    public override string ModuleName => "SpamBlocker";
    public override string ModuleVersion => "1.0.2";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Advanced spam and content filtering plugin with Discord integration";

    public SpamBlockerConfig Config { get; set; } = new();
    private FilterManager? _filterManager;
    private ChatManager? _chatManager;
    private EventHandler? _eventHandler;
    private DiscordService? _discordService;

    public void OnConfigParsed(SpamBlockerConfig config)
    {
        Config = config;
        LoggingUtils.LogInfo("Configuration parsed and loaded");

        if (_discordService != null)
        {
            _discordService.Dispose();
            _discordService = null;
        }

        InitializeDiscordService();
    }

    public override void Load(bool hotReload)
    {
        LoggingUtils.LogInfo("SpamBlocker loading...");

        InitializeServices();
        RegisterAllEvents();

        if (hotReload)
        {
            LoggingUtils.LogInfo("Hot reload detected - checking all existing players");
            Server.NextFrame(CheckAllExistingPlayers);
        }

        LoggingUtils.LogInfo("SpamBlocker loaded successfully!");
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        if (hotReload)
        {
            Server.NextFrame(CheckAllExistingPlayers);
        }
    }

    private void InitializeServices()
    {
        LoggingUtils.LogDebug("Initializing services...", Config);

        InitializeDiscordService();

        _filterManager = new FilterManager(Config, this);
        _chatManager = new ChatManager(_filterManager, Config, this);
        _eventHandler = new EventHandler(_filterManager, Config);

        LoggingUtils.LogDebug("All services initialized successfully", Config);
    }

    private void InitializeDiscordService()
    {
        if (Config.DiscordLogging.Enabled && !string.IsNullOrEmpty(Config.DiscordLogging.WebhookUrl))
        {
            _discordService = new DiscordService(Config, Localizer);
            LoggingUtils.SetDiscordService(_discordService);
            LoggingUtils.LogInfo("Discord logging service initialized");
        }
        else
        {
            LoggingUtils.SetDiscordService(null);
            LoggingUtils.LogDebug("Discord logging disabled", Config);
        }
    }

    private void RegisterAllEvents()
    {
        LoggingUtils.LogDebug("Registering all event handlers...", Config);

        if (_chatManager != null)
        {
            _chatManager.RegisterEvents(this);
            LoggingUtils.LogDebug("Chat events registered", Config);
        }

        if (_eventHandler != null)
        {
            _eventHandler.RegisterEvents(this);
            LoggingUtils.LogDebug("General events registered", Config);
        }

        LoggingUtils.LogDebug("All events registered successfully", Config);
    }

    private void CheckAllExistingPlayers()
    {
        if (_filterManager == null)
        {
            LoggingUtils.LogError("FilterManager is null - cannot check existing players");
            return;
        }

        LoggingUtils.LogDebug("Checking all existing players for violations...", Config);

        var players = Utilities.GetPlayers().Where(p => p != null && p.IsValid && !p.IsBot && !p.IsHLTV).ToList();
        foreach (var player in players)
        {
            if (!string.IsNullOrEmpty(player.PlayerName))
            {
                var filterResult = _filterManager.CheckContent(player.PlayerName, player);
                if (filterResult.IsBlocked)
                {
                    _filterManager.HandleNameViolation(player, filterResult);
                    LoggingUtils.LogViolation(player, "Name", filterResult, Config);
                }
            }
        }

        LoggingUtils.LogDebug("Finished checking all existing players", Config);
    }

    public override void Unload(bool hotReload)
    {
        LoggingUtils.LogInfo("Unloading SpamBlocker...");

        try
        {
            _filterManager?.ClearCache();

            _discordService?.Dispose();
            LoggingUtils.SetDiscordService(null);

            _filterManager = null;
            _chatManager = null;
            _eventHandler = null;
            _discordService = null;

            LoggingUtils.LogInfo("SpamBlocker unloaded successfully");
        }
        catch (Exception ex)
        {
            LoggingUtils.LogError($"Error during SpamBlocker unload: {ex.Message}", ex);
        }
    }
}