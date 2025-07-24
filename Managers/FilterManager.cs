using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

using SpamBlocker.Configs;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers
{
    public class FilterManager
    {
        private readonly SpamBlockerConfig _config;
        private readonly BasePlugin _plugin;
        private readonly HashSet<int> _renamedPlayers;

        public FilterManager(SpamBlockerConfig config, BasePlugin plugin)
        {
            _config = config;
            _plugin = plugin;
            _renamedPlayers = new HashSet<int>();
            LoggingUtils.LogDebug("FilterManager initialized successfully", _config);
        }

        public bool ShouldBypassFilter(CCSPlayerController player)
        {
            if (!_config.Settings.AdminBypass)
            {
                LoggingUtils.LogDebug($"AdminBypass disabled - no bypass for {PlayerUtils.GetSafePlayerName(player)}", _config);
                return false;
            }

            bool hasPermission = AdminManager.PlayerHasPermissions(player, _config.Settings.AdminBypassPermission);
            LoggingUtils.LogDebug($"Admin bypass check for {PlayerUtils.GetSafePlayerName(player)}: {hasPermission}", _config);
            return hasPermission;
        }

        public FilterResult CheckContent(string content, CCSPlayerController player)
        {
            LoggingUtils.LogDebug($"CheckContent called for player {PlayerUtils.GetSafePlayerName(player)} with content: '{content}'", _config);

            if (string.IsNullOrWhiteSpace(content))
            {
                LoggingUtils.LogDebug("Content is null or whitespace - allowing", _config);
                return new FilterResult { IsBlocked = false };
            }

            if (ShouldBypassFilter(player))
            {
                LoggingUtils.LogDebug($"Player {PlayerUtils.GetSafePlayerName(player)} bypassed filter", _config);
                return new FilterResult { IsBlocked = false };
            }

            var result = CheckWordFilter(content);
            if (result.IsBlocked) return result;

            result = CheckIpFilter(content);
            if (result.IsBlocked) return result;

            result = CheckUrlFilter(content);
            if (result.IsBlocked) return result;

            LoggingUtils.LogDebug($"Final filter result for '{content}': ALLOWED", _config);
            return new FilterResult { IsBlocked = false };
        }

        public FilterResult CheckPlayerName(string playerName, CCSPlayerController player)
        {
            LoggingUtils.LogDebug($"CheckPlayerName called for player {PlayerUtils.GetSafePlayerName(player)} with name: '{playerName}'", _config);

            if (string.IsNullOrWhiteSpace(playerName))
            {
                LoggingUtils.LogDebug("Player name is null or whitespace - allowing", _config);
                return new FilterResult { IsBlocked = false };
            }

            if (ShouldBypassFilter(player))
            {
                LoggingUtils.LogDebug($"Player {PlayerUtils.GetSafePlayerName(player)} bypassed name filter", _config);
                return new FilterResult { IsBlocked = false };
            }

            if (_config.NameProtection.NameBlacklist.Enabled)
            {
                var nameResult = CheckNameBlacklist(playerName);
                if (nameResult.IsBlocked)
                    return nameResult;
            }

            return CheckContent(playerName, player);
        }

        private FilterResult CheckWordFilter(string content)
        {
            if (!_config.WordFilter.Enabled)
            {
                LoggingUtils.LogDebug("Word filter disabled", _config);
                return new FilterResult { IsBlocked = false };
            }

            LoggingUtils.LogDebug("Checking blacklisted words...", _config);
            foreach (var word in _config.WordFilter.BlacklistedWords)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;

                LoggingUtils.LogDebug($"Checking word: '{word}'", _config);

                bool isMatch = _config.WordFilter.WholeWordOnly
                    ? RegexUtils.IsWholeWordMatch(content, word, _config.WordFilter.CaseSensitive)
                    : RegexUtils.ContainsWord(content, word, _config.WordFilter.CaseSensitive);

                if (isMatch)
                {
                    LoggingUtils.LogDebug($"Blacklisted word '{word}' detected", _config);
                    return new FilterResult
                    {
                        IsBlocked = true,
                        Reason = "Blacklisted word detected",
                        DetectedContent = word,
                        ViolationType = ViolationType.BlacklistedWord
                    };
                }
            }

            LoggingUtils.LogDebug("No blacklisted words found", _config);
            return new FilterResult { IsBlocked = false };
        }

        private FilterResult CheckIpFilter(string content)
        {
            if (!_config.IpFilter.Enabled)
            {
                LoggingUtils.LogDebug("IP filter disabled", _config);
                return new FilterResult { IsBlocked = false };
            }

            LoggingUtils.LogDebug("Checking IPs and ports...", _config);

            var ipPortMatches = RegexUtils.IpPortRegex.Matches(content);
            LoggingUtils.LogDebug($"Found {ipPortMatches.Count} IP:Port matches", _config);
            foreach (Match match in ipPortMatches)
            {
                var ipPort = match.Value;
                LoggingUtils.LogDebug($"Processing IP:Port: '{ipPort}'", _config);
                var parts = ipPort.Split(':');
                if (parts.Length == 2 && ValidationUtils.IsValidIpAddress(parts[0]))
                {
                    bool isInWhitelist = IsIpPortInWhitelist(ipPort);
                    LoggingUtils.LogDebug($"IP:Port '{ipPort}' in whitelist: {isInWhitelist}", _config);

                    if (!isInWhitelist)
                    {
                        LoggingUtils.LogDebug($"IP:Port '{ipPort}' blocked - not in whitelist", _config);
                        return new FilterResult
                        {
                            IsBlocked = true,
                            Reason = "IP:Port not in whitelist",
                            DetectedContent = ipPort,
                            ViolationType = ViolationType.BlockedIp
                        };
                    }
                }
            }

            var ipOnlyMatches = RegexUtils.IpRegex.Matches(content);
            LoggingUtils.LogDebug($"Found {ipOnlyMatches.Count} IP-only matches", _config);

            foreach (Match match in ipOnlyMatches)
            {
                var ip = match.Value;
                bool isPartOfIpPort = ipPortMatches.Cast<Match>().Any(m => m.Value.StartsWith(ip + ":"));
                if (isPartOfIpPort)
                {
                    LoggingUtils.LogDebug($"IP '{ip}' is part of IP:Port, skipping", _config);
                    continue;
                }

                if (ValidationUtils.IsValidIpAddress(ip))
                {
                    LoggingUtils.LogDebug($"Processing valid IP without port: '{ip}'", _config);

                    bool isInIpWhitelist = IsIpInWhitelist(ip);
                    LoggingUtils.LogDebug($"IP '{ip}' in IP whitelist: {isInIpWhitelist}", _config);
                    if (!isInIpWhitelist)
                    {
                        LoggingUtils.LogDebug($"IP '{ip}' not in whitelist - blocking", _config);
                        return new FilterResult
                        {
                            IsBlocked = true,
                            Reason = "IP address not in whitelist",
                            DetectedContent = ip,
                            ViolationType = ViolationType.BlockedIp
                        };
                    }
                    else
                    {
                        LoggingUtils.LogDebug($"IP '{ip}' found in whitelist - allowing", _config);
                    }
                }
            }

            LoggingUtils.LogDebug("All IPs and ports passed filter checks", _config);
            return new FilterResult { IsBlocked = false };
        }

        private FilterResult CheckUrlFilter(string content)
        {
            if (!_config.UrlFilter.Enabled)
            {
                LoggingUtils.LogDebug("URL filter disabled", _config);
                return new FilterResult { IsBlocked = false };
            }

            LoggingUtils.LogDebug("Checking URLs...", _config);

            var matches = RegexUtils.UrlRegex.Matches(content);
            LoggingUtils.LogDebug($"Found {matches.Count} URL matches", _config);
            foreach (Match match in matches)
            {
                var url = match.Value;
                LoggingUtils.LogDebug($"Processing URL: '{url}'", _config);

                if (ValidationUtils.IsNotRealUrl(url))
                {
                    LoggingUtils.LogDebug($"'{url}' is not a real URL - skipping", _config);
                    continue;
                }

                var domain = ValidationUtils.ExtractDomain(url);
                if (string.IsNullOrEmpty(domain))
                {
                    LoggingUtils.LogDebug($"Could not extract domain from URL: {url}", _config);
                    continue;
                }

                LoggingUtils.LogDebug($"Extracted domain: '{domain}'", _config);

                if (ValidationUtils.IsValidIpAddress(domain))
                {
                    LoggingUtils.LogDebug($"Domain '{domain}' is actually an IP address - skipping URL filter", _config);
                    continue;
                }

                if (!ValidationUtils.IsValidDomainFormat(domain))
                {
                    LoggingUtils.LogDebug($"Domain '{domain}' has invalid format - skipping", _config);
                    continue;
                }

                if (!ValidationUtils.IsAllowedProtocol(url, _config.UrlFilter.AllowedProtocols))
                {
                    LoggingUtils.LogDebug($"Protocol not allowed for URL: {url}", _config);
                    return new FilterResult
                    {
                        IsBlocked = true,
                        Reason = "Protocol not allowed",
                        DetectedContent = url,
                        ViolationType = ViolationType.BlockedUrl
                    };
                }

                var isWhitelistMode = string.Equals(_config.UrlFilter.FilterMode, "whitelist", StringComparison.OrdinalIgnoreCase);
                LoggingUtils.LogDebug($"URL filter mode: {_config.UrlFilter.FilterMode}", _config);
                if (isWhitelistMode)
                {
                    bool inWhitelist = ValidationUtils.IsInDomainList(domain, _config.UrlFilter.Whitelist);
                    LoggingUtils.LogDebug($"Domain '{domain}' in whitelist: {inWhitelist}", _config);
                    if (!inWhitelist)
                    {
                        LoggingUtils.LogDebug($"Domain '{domain}' not in whitelist - blocking URL: {url}", _config);
                        return new FilterResult
                        {
                            IsBlocked = true,
                            Reason = "Domain not in whitelist",
                            DetectedContent = url,
                            ViolationType = ViolationType.BlockedUrl
                        };
                    }
                }
                else
                {
                    bool inBlacklist = ValidationUtils.IsInDomainList(domain, _config.UrlFilter.Blacklist);
                    LoggingUtils.LogDebug($"Domain '{domain}' in blacklist: {inBlacklist}", _config);

                    if (inBlacklist)
                    {
                        LoggingUtils.LogDebug($"Domain '{domain}' in blacklist - blocking URL: {url}", _config);
                        return new FilterResult
                        {
                            IsBlocked = true,
                            Reason = "Domain in blacklist",
                            DetectedContent = url,
                            ViolationType = ViolationType.BlockedUrl
                        };
                    }
                }
            }

            LoggingUtils.LogDebug("All URLs passed filter checks", _config);
            return new FilterResult { IsBlocked = false };
        }

        private FilterResult CheckNameBlacklist(string playerName)
        {
            LoggingUtils.LogDebug($"Checking name blacklist for: '{playerName}'", _config);

            var nameConfig = _config.NameProtection.NameBlacklist;
            if (playerName.Length < nameConfig.MinNameLength)
            {
                LoggingUtils.LogDebug($"Name too short: {playerName.Length}", _config);
                return new FilterResult
                {
                    IsBlocked = true,
                    Reason = "Name too short",
                    DetectedContent = playerName,
                    ViolationType = ViolationType.BlacklistedName
                };
            }

            if (playerName.Length > nameConfig.MaxNameLength)
            {
                LoggingUtils.LogDebug($"Name too long: {playerName.Length}", _config);
                return new FilterResult
                {
                    IsBlocked = true,
                    Reason = "Name too long",
                    DetectedContent = playerName,
                    ViolationType = ViolationType.BlacklistedName
                };
            }

            var comparison = nameConfig.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach (var blacklistedName in nameConfig.BlacklistedNames)
            {
                if (string.IsNullOrWhiteSpace(blacklistedName))
                    continue;

                LoggingUtils.LogDebug($"Checking against blacklisted name: '{blacklistedName}'", _config);

                bool isMatch = false;
                string cleanPlayerName = playerName.Trim();
                if (nameConfig.WholeNameOnly)
                {
                    isMatch = string.Equals(cleanPlayerName, blacklistedName, comparison);
                }
                else if (nameConfig.BlockPartialMatches)
                {
                    isMatch = cleanPlayerName.Contains(blacklistedName, comparison);

                    if (isMatch && nameConfig.AllowNumbersSuffix)
                    {
                        var nameWithoutNumbers = Regex.Replace(cleanPlayerName, @"\d+$", "");
                        isMatch = !string.Equals(nameWithoutNumbers, blacklistedName, comparison);
                    }
                }
                else
                {
                    isMatch = string.Equals(cleanPlayerName, blacklistedName, comparison);
                }

                if (isMatch)
                {
                    LoggingUtils.LogDebug($"Blacklisted name '{blacklistedName}' detected", _config);
                    return new FilterResult
                    {
                        IsBlocked = true,
                        Reason = "Blacklisted name detected",
                        DetectedContent = blacklistedName,
                        ViolationType = ViolationType.BlacklistedName
                    };
                }
            }

            LoggingUtils.LogDebug("No blacklisted names matched", _config);
            return new FilterResult { IsBlocked = false };
        }

        private bool IsIpPortInWhitelist(string ipPort)
        {
            LoggingUtils.LogDebug($"Checking if IP:Port '{ipPort}' is in whitelist", _config);
            return _config.IpFilter.WhitelistIpPorts.Any(whitelistEntry => string.Equals(whitelistEntry.Trim(), ipPort, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsIpInWhitelist(string ip)
        {
            LoggingUtils.LogDebug($"Checking if IP '{ip}' is in IP whitelist", _config);
            return _config.IpFilter.WhitelistIps.Any(whitelistIp => string.Equals(whitelistIp.Trim(), ip, StringComparison.OrdinalIgnoreCase));
        }

        public void HandleNameViolation(CCSPlayerController player, FilterResult result)
        {
            if (!_config.NameProtection.Enabled || !PlayerUtils.IsValidPlayer(player))
            {
                LoggingUtils.LogDebug("Name protection disabled or invalid player", _config);
                return;
            }

            LoggingUtils.LogDebug($"Handling name violation for {PlayerUtils.GetSafePlayerName(player)}: {result.Reason}", _config);

            string newName = GenerateReplacementName(player);
            bool renamed = RenamePlayerSafely(player, newName);
            if (renamed)
            {
                _renamedPlayers.Add(player.Slot);
                LoggingUtils.LogDebug($"Player {player.Slot} successfully renamed to '{newName}' and marked as renamed", _config);

                if (_config.NameProtection.NotifyPlayer)
                {
                    var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, () =>
                    {
                        if (player.IsValid)
                        {
                            NotificationUtils.NotifyPlayer(player, _plugin.Localizer["name_changed"], _plugin);
                        }
                    });
                }

                if (_config.NameProtection.NotifyAdmins)
                {
                    NotificationUtils.NotifyAdminsViolation(
                        newName,
                        _plugin.Localizer["admin_name_change"],
                        result.Reason,
                        result.DetectedContent,
                        _plugin
                    );
                }
            }
            else
            {
                LoggingUtils.LogError($"Failed to rename player {PlayerUtils.GetSafePlayerName(player)} to '{newName}'");
            }
        }

        private string GenerateReplacementName(CCSPlayerController player)
        {
            string newName = _config.NameProtection.ReplacementName;
            if (_config.NameProtection.AddUserIdSuffix && player.UserId.HasValue)
            {
                newName += $"_{player.UserId.Value}";
                LoggingUtils.LogDebug($"Generated replacement name with userid suffix: '{newName}' for player {player.UserId.Value}", _config);
            }
            else if (_config.NameProtection.AddUserIdSuffix)
            {
                var random = new Random();
                newName += $"_{random.Next(1000, 9999)}";
                LoggingUtils.LogDebug($"Generated replacement name with random suffix (userid not available): '{newName}'", _config);
            }

            return newName;
        }

        private bool RenamePlayerSafely(CCSPlayerController player, string newName)
        {
            if (player == null || !player.IsValid || string.IsNullOrEmpty(newName))
            {
                LoggingUtils.LogDebug("Invalid parameters for renaming", _config);
                return false;
            }

            try
            {
                string oldName = player.PlayerName;

                Server.NextFrame(() =>
                {
                    try
                    {
                        if (player.IsValid)
                        {
                            player.PlayerName = newName;
                            Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");

                            var @event = new EventNextlevelChanged(true);
                            @event.FireEvent(false);

                            LoggingUtils.LogDebug($"Player renamed with base controller and event: '{oldName}' -> '{newName}'", _config);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingUtils.LogError($"Error renaming player: {ex.Message}");
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                LoggingUtils.LogError($"Error renaming player {PlayerUtils.GetSafePlayerName(player)} to '{newName}': {ex.Message}");
                return false;
            }
        }

        public void OnPlayerDisconnect(CCSPlayerController player)
        {
            if (PlayerUtils.IsValidPlayer(player))
            {
                _renamedPlayers.Remove(player.Slot);
                LoggingUtils.LogDebug($"Cleaned up data for disconnected player slot {player.Slot}", _config);
            }
        }

        public void ReapplyRenamedNames()
        {
            LoggingUtils.LogDebug("Checking for players with replaced names to reapply", _config);

            foreach (int slot in _renamedPlayers.ToList())
            {
                var player = Utilities.GetPlayerFromSlot(slot);
                if (PlayerUtils.IsValidPlayer(player))
                {
                    string expectedName = GenerateReplacementName(player!);
                    if (player!.PlayerName != expectedName)
                    {
                        LoggingUtils.LogDebug($"Reapplying name '{expectedName}' to player {player.UserId} (current: '{player.PlayerName}')", _config);
                        RenamePlayerSafely(player, expectedName);
                    }
                    else
                    {
                        LoggingUtils.LogDebug($"Player {player.UserId} already has correct name '{expectedName}'", _config);
                    }
                }
                else
                {
                    _renamedPlayers.Remove(slot);
                    LoggingUtils.LogDebug($"Removed invalid player slot {slot} from renamed players list", _config);
                }
            }
        }

        public void ClearCache()
        {
            _renamedPlayers.Clear();
            LoggingUtils.LogDebug("FilterManager cache cleared", _config);
        }
    }

    public class FilterResult
    {
        public bool IsBlocked { get; set; } = false;
        public string Reason { get; set; } = "";
        public string DetectedContent { get; set; } = "";
        public ViolationType ViolationType { get; set; } = ViolationType.None;
    }

    public enum ViolationType
    {
        None,
        BlacklistedWord,
        BlacklistedName,
        BlockedUrl,
        BlockedIp
    }
}