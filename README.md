# SpamBlocker CS2
SpamBlocker is an advanced filtering and protection system that provides comprehensive control over chat content and player names with Discord integration for real-time violation logging.

## 🚀 Installation

### Basic Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download [SpamBlocker.zip](releases/latest) from releases
3. Extract and upload to your game server: `csgo/addons/counterstrikesharp/plugins/SpamBlocker/`
4. Start server and configure the generated config file at `csgo/addons/counterstrikesharp/configs/plugins/SpamBlocker/`

---

## 📋 Main Configuration Parameters

| Parameter         | Description                                                                                        | Required |
|-------------------|----------------------------------------------------------------------------------------------------|----------|
| `WordFilter`      | Configuration for blacklisted words filtering with case sensitivity and whole word options.       | **YES**  |
| `UrlFilter`       | URL filtering with whitelist/blacklist modes and protocol restrictions.                          | **YES**  |
| `IpFilter`        | IP and port filtering with whitelist for specific IP:Port combinations.                          | **YES**  |
| `NameProtection`  | Player name filtering and automatic renaming for violations with individual blacklist support.   | **YES**  |
| `ChatProtection`  | Chat message filtering with configurable actions (block, kick, ban).                            | **YES**  |
| `DiscordLogging`  | **NEW**: Discord webhook integration for real-time violation logging.                           | **NO**   |
| `Settings`        | General plugin settings including admin bypass and debug mode.                                   | **YES**  |

### Discord Logging Configuration (NEW)
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable Discord logging. (**Default**: `false`)                                           | **YES**  |
| `webhook_url`     | Discord webhook URL for sending violation logs. (**Default**: `""`)                              | **YES**  |
| `embed_color`     | Hex color for Discord embeds. (**Default**: `"#ff0000"`)                                        | **YES**  |
| `mention_role_id` | Discord role ID to mention on violations. (**Default**: `""`)                                   | **NO**   |
| `server_name`     | Server name to display in Discord embeds. (**Default**: `"SpamBlocker Server"`)                 | **YES**  |
| `include_steam_profile` | Include Steam profile links in Discord logs. (**Default**: `true`)                        | **YES**  |
| `log_word_violations` | Log blacklisted word violations to Discord. (**Default**: `true`)                           | **YES**  |
| `log_url_violations` | Log blocked URL violations to Discord. (**Default**: `true`)                                 | **YES**  |
| `log_ip_violations` | Log blocked IP violations to Discord. (**Default**: `true`)                                   | **YES**  |
| `log_name_violations` | Log name violations to Discord. (**Default**: `true`)                                       | **YES**  |

### Word Filter Configuration
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable word filtering. (**Default**: `true`)                                             | **YES**  |
| `blacklisted_words` | List of words to filter from chat and names. (**Default**: `["badword1", "badword2", "spam", "cheat"]`) | **YES**  |
| `case_sensitive`  | Whether word matching is case sensitive. (**Default**: `false`)                                  | **YES**  |
| `whole_word_only` | Only match complete words, not partial matches. (**Default**: `true`)                           | **YES**  |

### URL Filter Configuration
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable URL filtering. (**Default**: `true`)                                              | **YES**  |
| `filter_mode`     | Filtering mode: `"whitelist"` or `"blacklist"`. (**Default**: `"blacklist"`)                    | **YES**  |
| `whitelist`       | List of allowed domains when using whitelist mode. (**Default**: `["steamcommunity.com", "github.com", "google.com"]`) | **YES**  |
| `blacklist`       | List of blocked domains when using blacklist mode. (**Default**: `["kick.com", "twitch.tv"]`) | **YES**  |
| `allowed_protocols` | List of allowed URL protocols. (**Default**: `["http", "https", "steam"]`)                     | **YES**  |

### IP Filter Configuration
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable IP filtering. (**Default**: `true`)                                               | **YES**  |
| `whitelist_ip_ports` | List of allowed IP:Port combinations. Only these specific combinations will be permitted. (**Default**: `["0.0.0.0:27060", "121.0.0.1:27015"]`) | **YES**  |

### Name Protection Configuration
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable name protection. (**Default**: `true`)                                            | **YES**  |
| `replacement_name` | Name to use when renaming players with violations. (**Default**: `"Player"`)                     | **YES**  |
| `add_random_suffix` | Add random number suffix to replacement names. (**Default**: `true`)                           | **YES**  |
| `notify_player`   | Notify player when their name is changed. (**Default**: `true`)                                 | **YES**  |
| `notify_admins`   | Notify admins when a player's name is changed. (**Default**: `true`)                           | **YES**  |
| `name_blacklist`  | Individual blacklist configuration specifically for player names. See detailed configuration below. | **YES**  |

#### Name Blacklist Configuration
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable individual name blacklist. (**Default**: `true`)                                  | **YES**  |
| `blacklisted_names` | List of names specifically prohibited for players. (**Default**: `["admin", "moderator", "owner", "server", "bot", "console", "system", "official", "staff"]`) | **YES**  |
| `case_sensitive`  | Whether name matching is case sensitive. (**Default**: `false`)                                  | **YES**  |
| `whole_name_only` | Only match complete names exactly. (**Default**: `false`)                                        | **YES**  |
| `block_partial_matches` | Block names containing blacklisted names as substrings. (**Default**: `true`)               | **YES**  |
| `allow_numbers_suffix` | Allow numbers at the end of blacklisted names (e.g., "admin123"). (**Default**: `false`)     | **YES**  |
| `min_name_length` | Minimum allowed name length in characters. (**Default**: `2`)                                   | **YES**  |
| `max_name_length` | Maximum allowed name length in characters. (**Default**: `32`)                                  | **YES**  |

### Chat Protection Configuration
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `enabled`         | Enable/disable chat protection. (**Default**: `true`)                                            | **YES**  |
| `action`          | Action to take on violations: `"block"`, `"kick"`, `"ban"`, or `"custom"`. (**Default**: `"block"`) | **YES**  |
| `kick_command`    | Command executed when action is "kick". (**Default**: `"css_kick #{userid} \"Spam/Inappropriate content\""`) | **YES**  |
| `ban_command`     | Command executed when action is "ban". (**Default**: `"css_ban #{userid} 60 \"Spam/Inappropriate content\""`) | **YES**  |
| `custom_command`  | Custom command executed when action is "custom". (**Default**: `""`)                           | **YES**  |
| `notify_player`   | Notify player when their message is blocked. (**Default**: `true`)                             | **YES**  |
| `notify_admins`   | Notify admins about chat violations. (**Default**: `true`)                                     | **YES**  |
| `log_violations`  | Log violations to server console. (**Default**: `true`)                                        | **YES**  |

### General Settings
| Parameter              | Description                                                                                    | Required |
|------------------------|------------------------------------------------------------------------------------------------|----------|
| `AdminBypass`          | Allow admins to bypass all filters. (**Default**: `false`)                                   | **YES**  |
| `AdminBypassPermission` | Permission flag required for admin bypass. (**Default**: `"@css/generic"`)                  | **YES**  |
| `DebugMode`            | Enable detailed logging for troubleshooting. (**Default**: `false`)                         | **YES**  |

---

## 🔧 Features

### Name Protection with Individual Blacklist
- **Dual filtering system**: Specific name blacklist + general content filters
- **Flexible matching**: Exact, partial, or with numeric suffixes
- **Length validation**: Configurable minimum and maximum name lengths
- **Priority system**: Name-specific rules checked before general filters
- **Staff protection**: Prevent impersonation of admin/moderator names

#### Name Filtering Behaviors
With `blacklisted_names: ["admin"]` and different configurations:

| Configuration | "admin" | "admin123" | "myadmin" | "Admin" |
|---------------|---------|------------|-----------|---------|
| `whole_name_only: true` | ❌ Blocked | ✅ Allowed | ✅ Allowed | ✅ Allowed* |
| `block_partial_matches: true` | ❌ Blocked | ❌ Blocked | ❌ Blocked | ❌ Blocked* |
| `allow_numbers_suffix: true` | ❌ Blocked | ✅ Allowed | ❌ Blocked | ❌ Blocked* |
| `case_sensitive: true` | ❌ Blocked | ❌ Blocked | ❌ Blocked | ✅ Allowed |

*Depends on `case_sensitive` setting

### IP:Port Filtering
- **Whitelist-only approach**: Only specifically allowed IP:Port combinations are permitted
- **Automatic blocking**: All IP addresses without ports are blocked
- **Port validation**: Ensures ports are within valid range (1-65535)
- **Exact matching**: Requires exact IP:Port combination match for access

### Command Placeholders
The following placeholders can be used in kick, ban, and custom commands:

| Placeholder | Description |
|-------------|-------------|
| `{userid}`  | Player's user ID |
| `{steamid}` | Player's SteamID64 |
| `{name}`    | Player's current name |
| `{slot}`    | Player's slot number |

### Filter Types
| Filter Type | Description | Applies To | Discord Logging |
|-------------|-------------|------------|-----------------|
| **Word Filter** | Blocks messages containing blacklisted words | Chat messages, player names | ✅ Configurable |
| **Name Blacklist** | Specific filtering for player names with advanced matching options | Player names only | ✅ Configurable |
| **URL Filter** | Controls allowed/blocked domains and protocols | Chat messages, player names | ✅ Configurable |
| **IP Filter** | Restricts IP:Port combinations to whitelist only | Chat messages, player names | ✅ Configurable |

---

## 📊 Support

For issues, questions, or feature requests, please visit our [GitHub Issues](https://github.com/yourrepo/SpamBlocker-CS2/issues) page.