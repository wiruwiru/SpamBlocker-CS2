using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace SpamBlocker.Configs
{
    public class SpamBlockerConfig : BasePluginConfig
    {
        [JsonPropertyName("WordFilter")]
        public WordFilterConfig WordFilter { get; set; } = new();

        [JsonPropertyName("UrlFilter")]
        public UrlFilterConfig UrlFilter { get; set; } = new();

        [JsonPropertyName("IpFilter")]
        public IpFilterConfig IpFilter { get; set; } = new();

        [JsonPropertyName("NameProtection")]
        public NameProtectionConfig NameProtection { get; set; } = new();

        [JsonPropertyName("ChatProtection")]
        public ChatProtectionConfig ChatProtection { get; set; } = new();

        [JsonPropertyName("DiscordLogging")]
        public DiscordLoggingConfig DiscordLogging { get; set; } = new();

        [JsonPropertyName("Settings")]
        public SpamBlockerSettings Settings { get; set; } = new();
    }

    public class WordFilterConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("blacklisted_words")]
        public List<string> BlacklistedWords { get; set; } = new List<string>
        {
            "badword1",
            "badword2",
            "spam",
            "cheat"
        };

        [JsonPropertyName("case_sensitive")]
        public bool CaseSensitive { get; set; } = false;

        [JsonPropertyName("whole_word_only")]
        public bool WholeWordOnly { get; set; } = true;
    }

    public class UrlFilterConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("filter_mode")]
        public string FilterMode { get; set; } = "blacklist"; // "whitelist" or "blacklist"

        [JsonPropertyName("whitelist")]
        public List<string> Whitelist { get; set; } = new List<string>
        {
            "steamcommunity.com",
            "github.com",
            "google.com"
        };

        [JsonPropertyName("blacklist")]
        public List<string> Blacklist { get; set; } = new List<string>
        {
            "kick.com",
            "twitch.tv"
        };

        [JsonPropertyName("allowed_protocols")]
        public List<string> AllowedProtocols { get; set; } = new List<string> { "http", "https", "steam" };
    }

    public class IpFilterConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("whitelist_ip_ports")]
        public List<string> WhitelistIpPorts { get; set; } = new List<string>
        {
            "0.0.0.0:27060",
            "127.0.0.1:27015"
        };

        [JsonPropertyName("whitelist_ips")]
        public List<string> WhitelistIps { get; set; } = new List<string>
        {
            "0.0.0.0"
        };
    }

    public class NameProtectionConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("replacement_name")]
        public string ReplacementName { get; set; } = "Player";

        [JsonPropertyName("add_userid_suffix")]
        public bool AddUserIdSuffix { get; set; } = true;

        [JsonPropertyName("name_blacklist")]
        public NameBlacklistConfig NameBlacklist { get; set; } = new();

        [JsonPropertyName("notify_player")]
        public bool NotifyPlayer { get; set; } = true;

        [JsonPropertyName("notify_admins")]
        public bool NotifyAdmins { get; set; } = true;
    }

    public class ChatProtectionConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("action")]
        public string Action { get; set; } = "block"; // "block", "kick", "ban"

        [JsonPropertyName("kick_command")]
        public string KickCommand { get; set; } = "css_kick #{userid} \"Spam/Inappropriate content\"";

        [JsonPropertyName("ban_command")]
        public string BanCommand { get; set; } = "css_ban #{userid} 60 \"Spam/Inappropriate content\"";

        [JsonPropertyName("custom_command")]
        public string CustomCommand { get; set; } = "";

        [JsonPropertyName("notify_player")]
        public bool NotifyPlayer { get; set; } = true;

        [JsonPropertyName("notify_admins")]
        public bool NotifyAdmins { get; set; } = true;

        [JsonPropertyName("log_violations")]
        public bool LogViolations { get; set; } = true;
    }

    public class DiscordLoggingConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("webhook_url")]
        public string WebhookUrl { get; set; } = "";

        [JsonPropertyName("embed_color")]
        public string EmbedColor { get; set; } = "#ff0000";

        [JsonPropertyName("mention_role_id")]
        public string MentionRoleId { get; set; } = "";

        [JsonPropertyName("server_name")]
        public string ServerName { get; set; } = "SpamBlocker";

        [JsonPropertyName("include_steam_profile")]
        public bool IncludeSteamProfile { get; set; } = true;

        [JsonPropertyName("log_word_violations")]
        public bool LogWordViolations { get; set; } = true;

        [JsonPropertyName("log_url_violations")]
        public bool LogUrlViolations { get; set; } = true;

        [JsonPropertyName("log_ip_violations")]
        public bool LogIpViolations { get; set; } = true;

        [JsonPropertyName("log_name_violations")]
        public bool LogNameViolations { get; set; } = true;
    }

    public class SpamBlockerSettings
    {
        [JsonPropertyName("AdminBypass")]
        public bool AdminBypass { get; set; } = false;

        [JsonPropertyName("AdminBypassPermission")]
        public string AdminBypassPermission { get; set; } = "@css/root";

        [JsonPropertyName("DebugMode")]
        public bool DebugMode { get; set; } = false;
    }

    public class NameBlacklistConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("blacklisted_names")]
        public List<string> BlacklistedNames { get; set; } = new List<string>
        {
            "admin",
            "moderator",
            "owner"
        };

        [JsonPropertyName("case_sensitive")]
        public bool CaseSensitive { get; set; } = false;

        [JsonPropertyName("whole_name_only")]
        public bool WholeNameOnly { get; set; } = false;

        [JsonPropertyName("block_partial_matches")]
        public bool BlockPartialMatches { get; set; } = false;

        [JsonPropertyName("allow_numbers_suffix")]
        public bool AllowNumbersSuffix { get; set; } = false;

        [JsonPropertyName("min_name_length")]
        public int MinNameLength { get; set; } = 2;

        [JsonPropertyName("max_name_length")]
        public int MaxNameLength { get; set; } = 32;
    }

    public enum FilterAction
    {
        Block,
        Kick,
        Ban,
        Custom
    }

    public enum FilterMode
    {
        Whitelist,
        Blacklist
    }
}