using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Localization;

using SpamBlocker.Configs;
using SpamBlocker.Models;

namespace SpamBlocker.Services
{
    public class DiscordService : IDisposable
    {
        private readonly SpamBlockerConfig _config;
        private readonly HttpClient _httpClient;
        private readonly IStringLocalizer _localizer;

        public DiscordService(SpamBlockerConfig config, IStringLocalizer localizer)
        {
            _config = config;
            _localizer = localizer;
            _httpClient = new HttpClient();
        }

        public async Task SendViolationToDiscord(DiscordViolationData violationData)
        {
            try
            {
                if (!_config.DiscordLogging.Enabled || string.IsNullOrEmpty(_config.DiscordLogging.WebhookUrl))
                {
                    return;
                }

                if (!ShouldLogViolationType(violationData.ViolationType))
                {
                    return;
                }

                string mentionMessage = "";
                if (!string.IsNullOrEmpty(_config.DiscordLogging.MentionRoleId))
                {
                    mentionMessage = $"<@&{_config.DiscordLogging.MentionRoleId}>";
                }

                var fields = new List<object>
                {
                    new
                    {
                        name = $"🎯 {GetLocalizedText("discord_player", "Player")}",
                        value = $"**{GetLocalizedText("discord_name", "Name")}** [{violationData.PlayerName}](https://steamcommunity.com/profiles/{violationData.SteamId})\n**{GetLocalizedText("discord_steamid", "SteamID")}** {violationData.SteamId}",
                        inline = false
                    },
                    new
                    {
                        name = $"⚠️ {GetLocalizedText("discord_violation_type", "Violation Type")}",
                        value = $"{GetViolationTypeEmoji(violationData.ViolationType)} {GetLocalizedViolationType(violationData.ViolationType)}",
                        inline = true
                    },
                    new
                    {
                        name = $"📝 {GetLocalizedText("discord_reason", "Reason")}",
                        value = violationData.Reason,
                        inline = true
                    },
                    new
                    {
                        name = $"🔍 {GetLocalizedText("discord_detected_content", "Detected Content")}",
                        value = $"`{violationData.DetectedContent}`",
                        inline = false
                    }
                };

                var embed = new
                {
                    title = $"🚨 {GetLocalizedText("discord_violation_title", "SpamBlocker Violation")}",
                    description = GetLocalizedText("discord_violation_description", "Violation detected on **{0}**").Replace("{0}", violationData.ServerName),
                    color = ConvertHexToColor(_config.DiscordLogging.EmbedColor),
                    fields,
                    footer = new
                    {
                        text = GetLocalizedText("discord_footer_text", "SpamBlocker System")
                    },
                    timestamp = violationData.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var payload = new
                {
                    content = mentionMessage,
                    embeds = new[] { embed }
                };

                await SendToDiscord(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpamBlocker] Error sending violation to Discord: {ex.Message}");
            }
        }

        private string GetLocalizedText(string key, string fallback)
        {
            try
            {
                var localizedValue = _localizer[key];
                return string.IsNullOrEmpty(localizedValue) ? fallback : localizedValue;
            }
            catch
            {
                return fallback;
            }
        }

        private bool ShouldLogViolationType(string violationType)
        {
            return violationType.ToLowerInvariant() switch
            {
                "blacklistedword" or "chat" => _config.DiscordLogging.LogWordViolations,
                "blockedurl" => _config.DiscordLogging.LogUrlViolations,
                "blockedip" => _config.DiscordLogging.LogIpViolations,
                "blacklistedname" or "name" or "teamchat" => _config.DiscordLogging.LogNameViolations,
                _ => true
            };
        }

        private string GetLocalizedViolationType(string violationType)
        {
            return violationType.ToLowerInvariant() switch
            {
                "blacklistedword" => GetLocalizedText("discord_violation_blacklisted_word", "Blacklisted Word"),
                "chat" => GetLocalizedText("discord_violation_chat", "Chat Violation"),
                "teamchat" => GetLocalizedText("discord_violation_team_chat", "Team Chat Violation"),
                "blockedurl" => GetLocalizedText("discord_violation_blocked_url", "Blocked URL"),
                "blockedip" => GetLocalizedText("discord_violation_blocked_ip", "Blocked IP"),
                "blacklistedname" => GetLocalizedText("discord_violation_blacklisted_name", "Blacklisted Name"),
                "name" => GetLocalizedText("discord_violation_name", "Name Violation"),
                _ => violationType
            };
        }

        private string GetViolationTypeEmoji(string violationType)
        {
            return violationType.ToLowerInvariant() switch
            {
                "blacklistedword" or "chat" or "teamchat" => "💬",
                "blockedurl" => "🔗",
                "blockedip" => "🌐",
                "blacklistedname" or "name" => "👤",
                _ => "⚠️"
            };
        }

        private async Task SendToDiscord(object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_config.DiscordLogging.WebhookUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SpamBlocker] Discord webhook failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpamBlocker] Error sending to Discord webhook: {ex.Message}");
            }
        }

        private static int ConvertHexToColor(string hex)
        {
            try
            {
                if (hex.StartsWith("#"))
                {
                    hex = hex[1..];
                }
                return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                return 16711680;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}